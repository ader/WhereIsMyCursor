# WhereIsMyCursor

English | [繁體中文](README.md)

Windows system tray cursor position indicator - helps you quickly locate your mouse cursor with a dynamic arrow icon.

![Tray Icon](doc/images/tray-icon.png)

## Features

- **Dynamic Arrow Indicator**: System tray icon shows a real-time arrow pointing to cursor position
- **Distance Color Coding**: 10-level gradient colors (red=near → green=far) indicate cursor distance
- **Blink Alert**: Arrow blinks when cursor is nearby, faster blinking = closer distance
- **Auto-Start**: Option to launch automatically on Windows startup
- **Multi-Language**: Traditional Chinese / English
- **Adjustable Update Rate**: Low (2 FPS), Medium (10 FPS), High (30 FPS)
- **Position Calibration**: Adjust X/Y offset values for precise tray icon targeting

## System Requirements

- Windows 10/11
- [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

## Installation

### Download and Run (Recommended)

1. Go to the [Releases](https://github.com/ader/WhereIsMyCursor/releases) page
2. Download the latest `WhereIsMyCursor-vX.X.X.zip`
3. Extract to any location
4. Run `WhereIsMyCursor.exe`

> Make sure [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0) is installed before first run

### Build from Source

```bash
cd src/WhereIsMyCursor
dotnet build -c Release
```

The executable will be at `bin/Release/net8.0-windows/WhereIsMyCursor.exe`

## Usage

1. After launching, the program minimizes to the system tray
2. The tray icon displays an arrow pointing to the cursor
3. Right-click the tray icon to open the menu:
   - **Settings...** - Open settings window
   - **Exit** - Close the program

## Settings

![Settings Window](doc/images/settings.png)

| Setting | Description |
|---------|-------------|
| Enable blinking effect | Toggle near-distance blink alert |
| Start with Windows | Launch program on Windows startup |
| Update frequency | Low (power saving) / Medium (recommended) / High (smooth) |
| Language | Traditional Chinese / English |
| X offset | Horizontal position calibration |
| Y offset | Vertical position calibration |

## Distance Indicator

| Level | Distance Range | Arrow Color | Blink Interval |
|-------|---------------|-------------|----------------|
| 0 | 0 - 400px | Red (#FF0000) | 150ms (fast) |
| 1 | 400 - 800px | Red-Orange (#FF2D00) | 200ms (fast) |
| 2 | 800 - 1200px | Orange-Red (#FF5A00) | 200ms (fast) |
| 3 | 1200 - 1600px | Orange (#FF8700) | 300ms (fast) |
| 4 | 1600 - 2000px | Orange-Yellow (#FFB400) | 300ms (medium) |
| 5 | 2000 - 2400px | Yellow (#FFE100) | 400ms (medium) |
| 6 | 2400 - 2800px | Yellow-Green (#D4F000) | 500ms (slow) |
| 7 | 2800 - 3200px | Light Green (#A0FF00) | 700ms (slow) |
| 8 | 3200 - 3600px | Green (#6CFF00) | 1000ms (very slow) |
| 9 | ≥ 3600px | Bright Green (#48FF00) | No blink |

Blink pattern: Asymmetric opacity toggle (80% visible / 20% dim)

## Technical Specs

- Framework: .NET 8 + WPF
- Tray Icon: System.Drawing dynamic 16x16 icon generation
- Cursor Position: Win32 API (GetCursorPos)
- Localization: .resx resource files

## License

MIT License
