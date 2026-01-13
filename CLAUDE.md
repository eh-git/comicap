# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

ComiCap is a modern Windows desktop screen capture utility that runs as a system tray application. Built with WPF (Windows Presentation Foundation) targeting .NET 8.0, it provides screen capture functionality through Windows API interop with improved error handling and resource management.

## Build and Run

### Prerequisites

- .NET 8.0 SDK or later ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- Windows 10/11 (Windows API required)
- Visual Studio 2022 or Visual Studio Code (recommended)

### Building the Project

```bash
# Restore NuGet packages
dotnet restore ComiCap/ComiCap.csproj

# Build Debug configuration
dotnet build ComiCap/ComiCap.csproj -c Debug

# Build Release configuration
dotnet build ComiCap/ComiCap.csproj -c Release

# Clean and rebuild
dotnet clean ComiCap/ComiCap.csproj
dotnet build ComiCap/ComiCap.csproj -c Release

# Publish standalone executable
dotnet publish ComiCap/ComiCap.csproj -c Release -o publish
```

Output binaries are placed in:
- Debug: `ComiCap/bin/Debug/net8.0-windows/ComiCap.exe`
- Release: `ComiCap/bin/Release/net8.0-windows/ComiCap.exe`
- Publish: `publish/ComiCap.exe`

### Visual Studio Code

VSCode tasks are configured in `.vscode/tasks.json`:
- `Ctrl+Shift+B` - Run default build task
- Command Palette → "Tasks: Run Task" → Select task

Available tasks:
- **build** - Build the project (default)
- **clean** - Clean build artifacts
- **rebuild** - Clean then build
- **restore** - Restore NuGet packages
- **publish** - Publish Release build

### Running the Application

The application is a Windows executable that runs in the system tray. It does not display a main window on startup. Access functionality through the system tray icon's context menu:
- **表示(O)** - Show/create the capture display window
- **キャプチャ(C)** - Capture screen and display in window
- **終了(X)** - Exit application

## Architecture

### Application Lifecycle

The application uses a non-standard WPF lifecycle to operate as a tray-only application:

- **App.xaml**: Defines `TaskbarIcon` resource using H.NotifyIcon.Wpf (no StartupUri)
- **App.xaml.cs**: Entry point that sets `ShutdownMode.OnExplicitShutdown` to prevent automatic shutdown when windows close. Initializes `TaskbarIcon` on startup and manages menu item event handlers.

### Key Components

**App** (`App.xaml`, `App.xaml.cs`)
- Application entry point with tray icon lifecycle management
- Manages `TaskbarIcon` (H.NotifyIcon.Wpf) with context menu
- Singleton pattern for `MainWindow` management
- Menu handlers:
  - `MenuItem_Open_Click` - Creates/shows MainWindow
  - `MenuItem_Capture_Click` - Captures screen and updates MainWindow image
  - `MenuItem_Exit_Click` - Shuts down application
- Comprehensive error handling with user-friendly error dialogs

**CaptureImpl** (`CaptureImpl.cs`)
- Static utility class for screen capture operations
- Uses modern `LibraryImport` (replacement for `DllImport`) for Windows API P/Invoke
- Native APIs: user32.dll (GetDC, ReleaseDC, GetWindowDC, GetForegroundWindow, GetWindowRect), gdi32.dll (BitBlt)
- Provides two capture methods:
  - `CaptureScreen()`: Captures entire primary screen
  - `CaptureActiveWindow()`: Captures the foreground window
- Returns `System.Drawing.Bitmap` objects
- **Modern Features**:
  - Comprehensive error handling with detailed exception messages
  - Proper resource cleanup in finally blocks
  - Nullable reference types enabled
  - XML documentation comments

**MainWindow** (`MainWindow.xaml`, `MainWindow.xaml.cs`)
- WPF window for displaying captured screenshots
- Created on-demand by App, not on application startup
- Singleton management: App tracks instance and disposes on close
- Contains `CapturedImage` control (Image) that stretches uniformly to fit window
- Centered on screen, size 800x600

### Technology Stack

- **UI Framework**: WPF (Windows Presentation Foundation)
- **System Tray**: H.NotifyIcon.Wpf 2.1.4 (pure WPF implementation, no Windows Forms dependency)
- **Native Interop**: Windows API calls via `LibraryImport` (modern P/Invoke)
- **Target Framework**: .NET 8.0-windows
- **Language**: C# 12.0 with latest features (nullable reference types, file-scoped namespaces, LibraryImport)
- **Graphics**: Mix of WPF (`BitmapImage`) and GDI+ (`System.Drawing.Bitmap`)

### NuGet Packages

- **H.NotifyIcon.Wpf** (2.1.4) - Modern WPF-native system tray icon implementation
- **System.Drawing.Common** (8.0.10) - GDI+ support for Bitmap operations

### Code Patterns

- **Modern C# 12.0 Features**:
  - File-scoped namespaces (`namespace ComiCap;`)
  - Nullable reference types enabled (`#nullable enable`)
  - `LibraryImport` for P/Invoke (more efficient than `DllImport`)
  - Using declarations for automatic resource disposal
  - Target-typed new expressions
- **Architecture Patterns**:
  - Singleton pattern for MainWindow management
  - Event-driven UI updates
  - Separation of concerns (capture logic, UI, tray management)
- **Resource Management**:
  - Proper disposal of unmanaged resources (device contexts, graphics objects)
  - Using statements and try-finally blocks
  - Freeze() for WPF BitmapImage to enable cross-thread usage
- Application primarily in Japanese (comments, UI text, variable names in some cases)

## Project Structure

```
ComiCap/
├── ComiCap.sln                 # Visual Studio solution file
├── .vscode/                    # Visual Studio Code configuration
│   ├── tasks.json             # Build tasks (build, clean, rebuild, restore, publish)
│   ├── launch.json            # Debug configurations
│   └── settings.json          # Workspace settings
├── CLAUDE.md                   # This file - guidance for Claude Code
├── README.md                   # User-facing documentation (Japanese)
└── ComiCap/
    ├── ComiCap.csproj         # SDK-style project file (.NET 8.0)
    ├── App.xaml               # Application definition with TaskbarIcon
    ├── App.xaml.cs            # Application logic and tray icon handlers
    ├── MainWindow.xaml        # Main display window
    ├── MainWindow.xaml.cs     # Window code-behind
    ├── CaptureImpl.cs         # Screen capture implementation
    ├── icon.ico               # Application icon
    └── Properties/
        ├── AssemblyInfo.cs    # Minimal file (ThemeInfo only, rest auto-generated)
        ├── Resources.Designer.cs   # Auto-generated resources
        ├── Resources.resx          # Resource definitions
        ├── Settings.Designer.cs    # Auto-generated settings
        └── Settings.settings       # Application settings
```

**Removed in v2.0.0**:
- `App.config` - Not needed in .NET 8
- `NotifyIconWrapper.cs` / `.Designer.cs` / `.resx` - Replaced by H.NotifyIcon.Wpf

## Modernization Details (v2.0.0)

### Framework Migration

- **.NET Framework 4.5.2 → .NET 8.0-windows**
- **Old-style .csproj → SDK-style .csproj** (118 lines → 45 lines)
- **MSBuild 14.0 → .NET SDK 8.0**
- **App.config removed** (not needed in .NET 8)
- **AssemblyInfo.cs simplified** (most attributes auto-generated by SDK)

### Architecture Changes

- **Windows Forms NotifyIcon → H.NotifyIcon.Wpf**
  - Pure WPF implementation (no Windows Forms dependency)
  - Defined in App.xaml as a resource
  - Better integration with WPF theming and styling
- **NotifyIconWrapper.cs removed** - Functionality moved to App.xaml.cs
- **Component-based architecture → Resource-based architecture**
- **MainWindow singleton management** - Prevents multiple window instances

### Code Quality Improvements

- **C# 12.0 Features**:
  - Nullable reference types (`#nullable enable`)
  - File-scoped namespaces
  - `LibraryImport` for P/Invoke (replacement for `DllImport`)
  - Using declarations
  - Target-typed new
- **Error Handling**:
  - Try-catch blocks in all UI event handlers
  - Detailed exception messages
  - User-friendly error dialogs
  - Null checks for MainWindow
- **Resource Management**:
  - Proper finally blocks for cleanup
  - Bitmap disposal on errors
  - Graphics and DeviceContext cleanup
- **Documentation**:
  - XML documentation comments on all public methods
  - Clear exception documentation

### Removed Code

- **Unused members**: `MainWindow.m_bitmap` field (never used)
- **Obsolete files**: `App.config`, `NotifyIconWrapper.cs/.Designer.cs/.resx`
- **Windows Forms dependencies**: Removed except for `System.Windows.Forms.Screen` (still needed for screen bounds)

## Development Notes

### Important Considerations

- **Windows-only application** - Requires Win32 APIs (user32.dll, gdi32.dll)
- **Administrator permissions** - May be required for capturing some protected windows
- **Primary screen only** - Currently captures only the primary monitor
- **Image conversion overhead** - GDI+ Bitmap → PNG MemoryStream → WPF BitmapImage
- **UI thread blocking** - Screen capture runs on UI thread (could be improved with async/await)

### Potential Improvements

1. **Async/await**: Make capture operations asynchronous
2. **Multi-monitor support**: Detect and capture from all monitors
3. **Save to file**: Add menu option to save captures to disk
4. **Clipboard integration**: Copy captures to clipboard
5. **Hotkey support**: Global hotkeys for quick capture
6. **Settings persistence**: Save window position, capture options
7. **Image editing**: Basic annotation tools
8. **Capture history**: Keep recent captures in memory

### Common Tasks

#### Adding a new menu item

1. Edit `App.xaml` - Add `<MenuItem>` to `<ContextMenu>`
2. Edit `App.xaml.cs` - Add event handler method
3. Implement the functionality

#### Changing capture behavior

1. Edit `CaptureImpl.cs` - Modify `CaptureScreen()` or `CaptureActiveWindow()`
2. Update error handling as needed
3. Update XML documentation

#### Updating dependencies

```bash
# List outdated packages
dotnet list ComiCap/ComiCap.csproj package --outdated

# Update a specific package
dotnet add ComiCap/ComiCap.csproj package <PackageName>
```

## Testing

Currently, there are no automated tests. Manual testing procedure:

1. Build and run the application
2. Verify system tray icon appears
3. Right-click icon and test each menu item:
   - **表示** - Window should open and activate
   - **キャプチャ** - Should capture screen and display in window
   - **終了** - Should cleanly exit application
4. Test error scenarios:
   - Capture when window is minimized
   - Capture when no displays available (edge case)
   - Multiple rapid captures

## Important Notes

- Always use `LibraryImport` instead of `DllImport` for new P/Invoke declarations
- Enable nullable reference types in all new files
- Use file-scoped namespaces for all new files
- Properly dispose of all `IDisposable` resources
- Add XML documentation comments to all public APIs
- Follow existing error handling patterns (try-catch with user-friendly messages)
- Test on both Debug and Release builds before committing
