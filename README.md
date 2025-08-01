# Godot Wrapper

## Enables Steam on Windows to track running time of Godot with opened project

Copilot says:

---

A .NET Windows Application that launches `Godot_console.exe` from the current working directory.

## Features

- Launches `./Godot_console.exe` in the current working directory
- Passes through command line arguments to Godot
- **No console window by default** - runs as Windows Application (WinExe)
- **Verbose mode** - use `--verbose` or `-v` flag to allocate and show console window
- Displays output from Godot in real-time (when verbose)
- Returns Godot's exit code
- Error handling for missing executable
- Builds as a single-file executable (no .NET runtime required on target machine)

## Usage

1. Build the single-file executable:

   ```powershell
   dotnet publish GodotWrapper.csproj -c Release --self-contained true -r win-x64
   ```

   Or use the provided batch file:

   ```powershell
   .\build.bat
   ```

2. Run the wrapper:

   ```powershell
   # Silent mode (default - no console window)
   .\bin\Release\net8.0\win-x64\publish\GodotWrapper.exe

   # Verbose mode (allocates and shows console window)
   .\bin\Release\net8.0\win-x64\publish\GodotWrapper.exe --verbose
   # or
   .\bin\Release\net8.0\win-x64\publish\GodotWrapper.exe -v
   ```

   With Godot arguments:

   ```powershell
   # Silent mode with Godot arguments
   .\bin\Release\net8.0\win-x64\publish\GodotWrapper.exe --headless --script myscript.gd

   # Verbose mode with Godot arguments
   .\bin\Release\net8.0\win-x64\publish\GodotWrapper.exe --verbose --headless --script myscript.gd
   ```

3. For development/testing, you can still use:

   ```powershell
   # Silent mode
   dotnet run

   # Verbose mode
   dotnet run -- --verbose
   ```

## Verbose Mode

By default, the wrapper runs as a Windows Application with no console window. To see output and debug information, use the verbose flag:

- `--verbose` or `-v`: Allocates a console window using Windows API and shows all output
- Without verbose flag: Runs completely silently as a Windows Application

The verbose flag is automatically filtered out and not passed to Godot.

## Requirements

- .NET 8.0 SDK for building
- No runtime requirements on target machine (self-contained)
- `Godot_console.exe` must be present in the same directory where you run the wrapper

## How it works

1. The wrapper checks for the `--verbose` or `-v` flag to determine if console should be allocated
2. If in verbose mode, uses `AllocConsole()` Windows API to create a console window
3. Checks for `Godot_console.exe` in the current working directory
4. If found, launches Godot with the same working directory
5. Filters out verbose flags before passing arguments to Godot
6. In verbose mode: displays output from Godot in real-time with `[Godot]` prefixes
7. In silent mode: runs as a pure Windows Application with no visible UI
8. The wrapper exits with the same exit code as Godot

## Technical Implementation

This wrapper uses the Windows Application (`WinExe`) output type instead of Console Application (`Exe`). When verbose mode is requested, it:

1. Calls `AllocConsole()` to create a new console window
2. Gets the standard output handle using `GetStdHandle()`
3. Creates a `SafeFileHandle` and `FileStream` to redirect console output
4. Sets up a `StreamWriter` with proper encoding for console output

This approach is cleaner than hiding an existing console window and provides better control over when and how the console appears.
