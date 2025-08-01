using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace GodotWrapper
{
  class Program
  {
    [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern int AllocConsole();

    private const int STD_OUTPUT_HANDLE = -11;
    private const int MY_CODE_PAGE = 437;

    static async Task<int> Main(string[] args)
    {
      // Check for verbose flag
      bool showConsole = args.Contains("--verbose") || args.Contains("-v");

      if (showConsole)
      {
        AllocConsole();
        IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
        SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
        FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
        System.Text.Encoding encoding = System.Text.Encoding.GetEncoding(MY_CODE_PAGE);
        StreamWriter standardOutput = new StreamWriter(fileStream, encoding);
        standardOutput.AutoFlush = true;
        Console.SetOut(standardOutput);
      }

      try
      {
        // Remove verbose flags from args before passing to Godot
        var godotArgs = args.Where(arg => arg != "--verbose" && arg != "-v").ToArray();

        // Get the current working directory
        string workingDirectory = Directory.GetCurrentDirectory();
        string godotExecutablePath = Path.Combine(workingDirectory, "Godot_console.exe");

        if (showConsole)
        {
          Console.WriteLine($"Working Directory: {workingDirectory}");
          Console.WriteLine($"Looking for Godot executable at: {godotExecutablePath}");
        }

        // Check if Godot_console.exe exists
        if (!File.Exists(godotExecutablePath))
        {
          if (showConsole)
          {
            Console.WriteLine($"Error: Godot_console.exe not found at {godotExecutablePath}");
          }
          return 1;
        }

        if (showConsole)
        {
          Console.WriteLine("Launching Godot_console.exe...");
        }

        // Configure the process start info
        var processStartInfo = new ProcessStartInfo
        {
          FileName = godotExecutablePath,
          WorkingDirectory = workingDirectory,
          UseShellExecute = false,
          RedirectStandardOutput = showConsole,
          RedirectStandardError = showConsole,
          CreateNoWindow = !showConsole
        };

        // Add any command line arguments passed to this wrapper
        if (godotArgs.Length > 0)
        {
          processStartInfo.Arguments = string.Join(" ", godotArgs);
          if (showConsole)
          {
            Console.WriteLine($"Arguments: {processStartInfo.Arguments}");
          }
        }

        // Start the Godot process
        using var process = new Process { StartInfo = processStartInfo };

        // Event handlers for output (only if console is shown)
        if (showConsole)
        {
          process.OutputDataReceived += (sender, e) =>
          {
            if (!string.IsNullOrEmpty(e.Data))
              Console.WriteLine($"[Godot] {e.Data}");
          };

          process.ErrorDataReceived += (sender, e) =>
          {
            if (!string.IsNullOrEmpty(e.Data))
              Console.WriteLine($"[Godot Error] {e.Data}");
          };
        }

        process.Start();

        // Begin async reading of output (only if console is shown)
        if (showConsole)
        {
          process.BeginOutputReadLine();
          process.BeginErrorReadLine();
        }

        // Wait for the process to exit
        await process.WaitForExitAsync();

        if (showConsole)
        {
          Console.WriteLine($"Godot_console.exe exited with code: {process.ExitCode}");
        }
        return process.ExitCode;
      }
      catch (Exception ex)
      {
        if (showConsole)
        {
          Console.WriteLine($"Error launching Godot_console.exe: {ex.Message}");
        }
        return 1;
      }
    }
  }
}
