# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ComiCap is a Windows desktop screen capture utility that runs as a system tray application. Built with WPF (Windows Presentation Foundation) targeting .NET Framework 4.5.2, it provides screen capture functionality through Windows API interop.

## Build and Run

### Building the Project

```bash
# Build Debug configuration
msbuild ComiCap.sln /p:Configuration=Debug

# Build Release configuration
msbuild ComiCap.sln /p:Configuration=Release

# Clean and rebuild
msbuild ComiCap.sln /t:Clean,Build /p:Configuration=Debug
```

Output binaries are placed in:
- Debug: `ComiCap/bin/Debug/ComiCap.exe`
- Release: `ComiCap/bin/Release/ComiCap.exe`

### Running the Application

The application is a Windows executable that runs in the system tray. It does not display a main window on startup. Access functionality through the system tray icon's context menu.

## Architecture

### Application Lifecycle

The application uses a non-standard WPF lifecycle to operate as a tray-only application:

- **App.xaml.cs**: Entry point that sets `ShutdownMode.OnExplicitShutdown` to prevent automatic shutdown when windows close. Instantiates `NotifyIconWrapper` on startup.
- **App.xaml**: Note that `StartupUri` is commented out - no window opens automatically on launch.

### Key Components

**NotifyIconWrapper** (`NotifyIconWrapper.cs`)
- System tray icon component (extends `System.ComponentModel.Component`)
- Manages the context menu with "表示" (Show), "Capture", and "終了" (Exit) options
- Creates and shows `MainWindow` on demand when user selects "表示"
- Handles screen capture and updates the image in MainWindow

**CaptureImpl** (`CaptureImpl.cs`)
- Static utility class for screen capture operations
- Uses Windows API P/Invoke (user32.dll, gdi32.dll) for BitBlt operations
- Provides two capture methods:
  - `CaptureScreen()`: Captures entire primary screen
  - `CaptureActiveWindow()`: Captures the foreground window
- Returns `System.Drawing.Bitmap` objects

**MainWindow** (`MainWindow.xaml`, `MainWindow.xaml.cs`)
- WPF window with a single Image control for displaying captured screenshots
- Created on-demand by NotifyIconWrapper, not on application startup
- Simple layout with 100x100 image placeholder at position (47, 60)

### Technology Stack

- **UI Framework**: WPF (Windows Presentation Foundation)
- **Windows Forms Integration**: Uses `System.Windows.Forms` for NotifyIcon and Screen classes
- **Native Interop**: Windows API calls via P/Invoke for screen capture
- **Target Framework**: .NET Framework 4.5.2
- **Graphics**: Mix of WPF (`BitmapImage`) and GDI+ (`System.Drawing.Bitmap`)

### Code Patterns

- Application is primarily in Japanese (comments, variable names in some cases)
- Uses Windows Forms components within WPF application for system tray functionality
- Converts between GDI+ Bitmap and WPF BitmapImage using MemoryStream
- Manual resource disposal for unmanaged resources (device contexts, graphics objects)

## Project Structure

```
ComiCap/
├── ComiCap.sln                      # Visual Studio solution file
└── ComiCap/
    ├── ComiCap.csproj               # Project file
    ├── App.xaml/App.xaml.cs         # Application entry point
    ├── MainWindow.xaml/.xaml.cs     # Main display window
    ├── NotifyIconWrapper.cs         # System tray component
    ├── CaptureImpl.cs               # Screen capture utilities
    ├── App.config                   # Application configuration
    └── Properties/                  # Assembly info and resources
```

## Detailed Source Code Reference

**IMPORTANT**: Before making any code changes, always refer to `SOURCE_STRUCTURE.md` for detailed information about:
- Complete file-by-file code structure and member details
- Data flow diagrams and application lifecycle
- Technical considerations (WPF/Windows Forms interop, image processing)
- Known limitations and improvement recommendations
- Resource management patterns

This document provides comprehensive analysis of the codebase to help you understand implementation details and avoid common pitfalls.

## Important Notes

- This is a Windows-only application (requires Win32 APIs)
- The application requires appropriate permissions for screen capture
- Currently, the capture is hardcoded to primary screen dimensions
- Image display in MainWindow is fixed at 100x100px regardless of capture size
