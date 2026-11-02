# EasyCreator

**Learn programming, robotics and engineering — from your first line of code to university level — with challenges that get harder as you go.**

[Português](README.pt-BR.md) · Available in English, Português and Español.

[![Download for Windows](https://img.shields.io/badge/Download-Windows%20x64-18B98A?style=for-the-badge)](https://github.com/tarl3s/EasyCreator/releases/latest/download/EasyCreator-win-x64.zip)

Unzip anywhere and run `EasyCreator.exe`. No installation, no .NET needed.

## What's inside

| Area | Tracks | How you practise |
|---|---|---|
| 💻 Programming | Python, JavaScript, C, C# | Write code in the app; it is checked against tests (some hidden). Runs offline, or with Python/Node/gcc/.NET installed on your computer. |
| 🤖 Robotics | Arduino | A simulated Arduino Uno: LEDs, buttons, potentiometers, servos, ultrasonic sensor, motors, LCD and a line-following robot. |
| ⚛️ Physics | Motion to modern physics | Labs: projectiles with drag, inclined plane, springs, pendulum, collisions, orbits, lenses, ideal gas. |
| ⚡ Electrical engineering | Ohm to power systems | A circuit simulator (DC, transients, AC phasors, three-phase) with a virtual multimeter and oscilloscope. |
| 🔥 Metallurgy & welding | Atoms to weld beads | Welding simulator (SMAW, MIG/MAG, TIG), heat treatment and tensile test. **Real welding needs hands-on training with a qualified instructor and proper PPE.** |
| 🚀 Projects | Rockets, submarines, cars, aircraft & helicopters, drones | Design and fly/drive/dive them in simulators, from basics to a full design project. |

Every track has 20 lessons, levels 1 (beginner) to 5 (university). Lessons unlock in order and your progress is saved.

**🧪 Sandbox** — free mode: write programs in any of the four languages, or build your own Arduino circuit (pick the parts and pins) and simulate it.

## Build from source

Windows with the .NET 8 SDK:

```bash
dotnet build
dotnet run
dotnet run -- --test        # every test suite, log in %TEMP%\easycreator_test.log
dotnet run -- --snap --lang en   # renders the main screens to %TEMP%\easycreator_snap_*.png
```

Lessons live in `Content/` (JSON, Portuguese source) with English and Spanish texts in `Content/i18n/`; UI translations are in `Lang/`.

## Safety

The simulators teach the ideas; they do not replace real training. Mains electricity, welding, rocket motors, LiPo batteries and propellers can kill or injure. Follow the safety notes in each lesson and the laws of your country.

## License

MIT — see [LICENSE](LICENSE).
