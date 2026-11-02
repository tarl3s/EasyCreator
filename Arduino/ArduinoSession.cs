using System.Diagnostics;
using EasyCreator.CInterp;

namespace EasyCreator.Arduino;

/// <summary>
/// Live simulation for the UI (no WPF types). Usage:
/// <code>
/// var s = new ArduinoSession();
/// if (!s.Start(sketch, lesson.Components)) show(s.Error);
/// // every frame (e.g. 16 ms):
/// s.Tick(16);                       // runs the sketch for 16 ms × SpeedFactor of virtual time
/// draw(s.GetPins(), s.GetComponents(), s.GetRobot(), s.Arena, s.SerialText);
/// s.PressButton("btn1", true);      // live input between ticks
/// s.Dispose();
/// </code>
/// The sketch runs on its own big-stack thread but only while <see cref="Tick"/> is executing, so every getter and
/// input method is safe to call from the UI thread between ticks.
/// </summary>
public sealed class ArduinoSession : IDisposable
{
    ArduinoBoard? _board;
    CInterpreter? _ip;
    Thread? _thread;
    readonly SemaphoreSlim _run = new(0), _paused = new(0);
    double _targetUs;
    volatile bool _stop;
    readonly Stopwatch _sliceClock = new();
    double _sliceBudgetMs = 25;

    /// <summary>Virtual-time speed: 1 = real time, 2 = twice as fast, 0.25 = slow motion.</summary>
    public double SpeedFactor { get; set; } = 1.0;
    public bool IsRunning { get; private set; }
    /// <summary>Compile or runtime error (friendly text in the chosen language), or null.</summary>
    public string? Error { get; private set; }
    /// <summary>True when the program stopped (error, exit() or Stop()).</summary>
    public bool HasFinished { get; private set; }
    public double TimeMs => _board?.TimeMs ?? 0;
    public string SerialText => _board?.SerialOut.ToString() ?? "";
    public IReadOnlyList<string> Warnings => _board?.Warnings ?? (IReadOnlyList<string>)Array.Empty<string>();
    public Arena? Arena => _board?.Robot?.Arena;
    /// <summary>Line of the statement the sketch was executing at the last pause.</summary>
    public int CurrentLine => _ip?.Line ?? 0;
    /// <summary>The underlying board (advanced use; only touch it between ticks).</summary>
    public ArduinoBoard? Board => _board;

    /// <summary>Compiles and prepares the sketch. Returns false (and sets <see cref="Error"/>) if it does not compile.</summary>
    public bool Start(string sketch, IEnumerable<ComponentSpec> components, string? arenaPreset = null)
    {
        Stop();
        _stop = false;
        Error = null;
        HasFinished = false;
        _board = new ArduinoBoard(components, arenaPreset);
        _ip = new CInterpreter { MaxSteps = long.MaxValue, Timeout = TimeSpan.FromDays(365) };
        _board.Attach(_ip);
        var ipc = _ip;
        string? compileError = CInterpreter.RunOnBigStack(() =>
        {
            try
            {
                ipc.Compile(sketch ?? "");
                if (!ipc.HasFunction("setup") || !ipc.HasFunction("loop"))
                    return L.T("Erro de compilação: o sketch precisa ter as duas funções: void setup() { } e void loop() { }");
                return null;
            }
            catch (CError e) { return e.Friendly; }
            catch (Exception e) { return L.F("Erro ao ler o código: {0}", e.Message); }
        }, 64);
        if (compileError != null)
        {
            Error = compileError;
            HasFinished = true;
            return false;
        }
        _targetUs = 0;
        _board.AfterAdvance = Gate;
        var ip = _ip;
        var board = _board;
        _thread = new Thread(() => SketchThread(ip, board), 256 * 1024 * 1024) { IsBackground = true, Name = "Arduino sketch" };
        IsRunning = true;
        _thread.Start();
        _paused.Wait(); // thread signals once it is ready and waiting
        return true;
    }

    void SketchThread(CInterpreter ip, ArduinoBoard board)
    {
        _paused.Release();
        _run.Wait();
        try
        {
            if (_stop) return;
            ip.Call("setup");
            while (true)
            {
                ip.Call("loop");
                board.Advance(board.LoopOverheadUs);
            }
        }
        catch (CHalt) { }
        catch (CExit) { Error = null; }
        catch (CError e) { Error = e.Friendly; }
        catch (InsufficientExecutionStackException) { Error = L.T("Erro de execução: recursão profunda demais."); }
        catch (Exception e) { Error = L.F("Erro interno do simulador perto da linha {0}: {1}", ip.Line, e.Message); }
        finally
        {
            HasFinished = true;
            IsRunning = false;
            _paused.Release();
        }
    }

    /// <summary>Called by the board after every time slice on the sketch thread: pause when the target time is reached
    /// or the wall-clock budget of this tick is spent.</summary>
    void Gate()
    {
        if (_stop) throw new CHalt("parado");
        if (_board!.TimeUs >= _targetUs || _sliceClock.Elapsed.TotalMilliseconds > _sliceBudgetMs)
        {
            _paused.Release();
            _run.Wait();
            if (_stop) throw new CHalt("parado");
        }
    }

    /// <summary>Advances the simulation by <paramref name="realMs"/> × <see cref="SpeedFactor"/> of virtual time.
    /// Blocks for at most about <paramref name="budgetMs"/> of wall time (heavy sketches then run slower than real time).</summary>
    public void Tick(double realMs, double budgetMs = 25)
    {
        if (!IsRunning || _board == null) return;
        realMs = Math.Clamp(realMs, 0, 250);
        _targetUs = _board.TimeUs + realMs * 1000 * Math.Max(0, SpeedFactor);
        _sliceBudgetMs = Math.Max(1, budgetMs);
        _sliceClock.Restart();
        _run.Release();
        if (!_paused.Wait(5000))
        {
            Error ??= L.T("O simulador parou de responder.");
            _stop = true;
        }
    }

    /// <summary>Stops the sketch thread.</summary>
    public void Stop()
    {
        if (_thread == null) return;
        _stop = true;
        if (IsRunning)
        {
            _run.Release();
            _paused.Wait(2000);
        }
        _thread.Join(1000);
        _thread = null;
        IsRunning = false;
        HasFinished = true;
    }

    public void Dispose() => Stop();

    // ---------------- state ----------------
    public PinSnapshot[] GetPins() => _board?.GetPins() ?? Array.Empty<PinSnapshot>();
    public List<ComponentSnapshot> GetComponents() => _board?.GetComponents() ?? new List<ComponentSnapshot>();
    public ComponentSnapshot? GetComponent(string id) => GetComponents().FirstOrDefault(c => string.Equals(c.Id, id, StringComparison.OrdinalIgnoreCase));
    public RobotSnapshot? GetRobot() => _board?.GetRobot();

    // ---------------- live input ----------------
    public bool PressButton(string id, bool pressed) => _board?.SetButton(id, pressed) ?? false;
    /// <summary>Sets a potentiometer or LDR reading (0–1023).</summary>
    public bool SetAnalog(string id, int value) => _board?.SetAnalog(id, value) ?? false;
    /// <summary>Sets what an ultrasonic sensor measures (cm).</summary>
    public bool SetDistance(string id, double cm) => _board?.SetDistance(id, cm) ?? false;
    /// <summary>Puts a box on the robot's line, <paramref name="alongCm"/> cm from its start.</summary>
    public bool PlaceObstacle(double alongCm) => _board?.PlaceObstacle(alongCm) ?? false;
    public void ClearObstacles() => _board?.ClearObstacles();
    /// <summary>Text typed in the Serial Monitor (read with Serial.read()/available()).</summary>
    public void SendSerial(string text) => _board?.SendSerial(text);
}
