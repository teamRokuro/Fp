using System;
using System.IO;
using Avalonia;
using Avalonia.ReactiveUI;
using Fp;

namespace Dereliction
{
    internal static class Program
    {
        public const string SCRIPT_DIRECTORY = "scripts";
        private static string[]? _args;
        private static string? _workingDirectory;
        private static string? _mainScript;

        private static void CategorizeArgs()
        {
            if (_args != null) return;
            _args = Environment.GetCommandLineArgs();
            if (_args.Length == 1) return;
            string main = _args[1];
            if (Directory.Exists(main)) _workingDirectory = main;
            else if (Processor.PathHasExtension(main, ".csx")) _mainScript = main;
        }

        public static string WorkingDirectory
        {
            get
            {
                if (_args == null) CategorizeArgs();
                if (_workingDirectory == null)
                {
                    _workingDirectory = Path.GetFullPath(SCRIPT_DIRECTORY);
                    if (!Directory.Exists(_workingDirectory)) Directory.CreateDirectory(_workingDirectory);
                }

                return _workingDirectory;
            }
        }

        public static string? MainScript
        {
            get
            {
                if (_args == null) CategorizeArgs();
                return _mainScript;
            }
        }

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // Avalonia configuration, don't remove; also used by visual designer.
        private static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
