# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build Commands

```bash
# Build (from repository root)
dotnet build src/WhereIsMyCursor/WhereIsMyCursor.csproj -c Release

# Run
dotnet run --project src/WhereIsMyCursor/WhereIsMyCursor.csproj

# Publish self-contained
dotnet publish src/WhereIsMyCursor/WhereIsMyCursor.csproj -c Release -o publish
```

## Project Overview

WhereIsMyCursor is a Windows system tray application that displays a dynamic arrow icon pointing toward the mouse cursor location. The arrow color indicates distance (red=near, green=far) with optional blinking effects.

**Tech Stack:** .NET 8, WPF + WinForms (hybrid for NotifyIcon), C#, Windows 10/11

## Architecture

### Entry Point & Lifecycle
- `App.xaml.cs` - Application lifecycle, NotifyIcon management, settings coordination
- No main window; app runs entirely from system tray

### Core Services
- `Services/TrayIconService.cs` - Dynamic icon generation using System.Drawing, calculates arrow angle/distance to cursor, manages blink state via DispatcherTimer
- `Services/CursorService.cs` - Gets cursor position via Win32 GetCursorPos
- `Services/LocalizationService.cs` - Language switching (zh-TW, en)
- `Services/StartupService.cs` - Windows auto-start registry management

### Models & Views
- `Models/AppSettings.cs` - JSON-based settings persisted to `%AppData%/WhereIsMyCursor/settings.json`
- `Views/SettingsWindow.xaml` - Configuration dialog

### Win32 Interop
- `Interop/NativeMethods.cs` - P/Invoke declarations for GetCursorPos, DestroyIcon, window styles, hotkeys

## Key Implementation Details

- **Icon Generation:** TrayIconService generates 16x16 icons with anti-aliasing, rotating arrow based on cursor-to-tray angle
- **Distance Visualization:** 10 color levels from red (#FF0000) to green (#48FF00), 400px per level
- **Blink Effect:** Asymmetric opacity cycling (80% visible/20% dim), interval varies by distance
- **Update Frequency:** Low=500ms, Medium=100ms, High=33ms (configurable)
- **Tray Position:** Estimated from screen bounds and taskbar position with X/Y offset calibration

## Localization

Resources are in `Resources/Strings.resx` (zh-TW) and `Resources/Strings.en.resx`. Use `Strings.PropertyName` for localized strings.

## GitHub

使用 `gh` CLI 處理所有 GitHub 相關操作（issues、pull requests、releases 等）。

```powershell
# gh CLI 路徑
& 'C:\Program Files\GitHub CLI\gh.exe' <command>
```

## Release 流程

執行 release 時直接進行以下步驟，不需詢問：
1. Commit 變更
2. Merge 到 main
3. 建立 tag (vX.Y.Z)
4. Push 到 remote
5. `dotnet publish` 並打包 zip
6. `gh release create` 上傳到 GitHub
