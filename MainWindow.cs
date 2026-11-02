using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EasyCreator;

/// <summary>Single window; pages are swapped in the content area. Esc / ← Voltar goes back.</summary>
public sealed class MainWindow : Window
{
    readonly Stack<Func<UIElement>> _history = new();
    readonly ContentControl _host = new();

    public MainWindow()
    {
        Title = "EasyCreator";
        Width = 1280; Height = 800; MinWidth = 900; MinHeight = 600;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Ui.Bg0; Foreground = Ui.Text;
        Content = _host;
        KeyDown += (_, e) => { if (e.Key == Key.Escape && Keyboard.FocusedElement is not TextBox) Back(); };
        Go(() => Pages.Home(this));
    }

    public void Go(Func<UIElement> page)
    {
        _history.Push(page);
        Show(page());
    }

    /// <summary>Rebuilds the current page (e.g. after progress changed).</summary>
    public void Refresh() { if (_history.Count > 0) Show(_history.Peek()()); }

    public void Back()
    {
        if (_history.Count <= 1) return;
        _history.Pop();
        Refresh();
    }

    void Show(UIElement page)
    {
        (_host.Content as IDisposable)?.Dispose();
        _host.Content = page;
    }
}
