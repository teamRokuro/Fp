using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dotnet.Script.Core;
using Dotnet.Script.Core.Commands;
using Dotnet.Script.DependencyModel.Logging;
using Fp;
using Microsoft.CodeAnalysis;

namespace fpx
{
    public static class Program
    {
        private static async Task Main(string[] args) => await ConsoleAsync(args);

        public delegate void LogDelegate(string? log, bool newLine = true);

        public static void ConsoleLog(string? log, bool newLine = true)
        {
            if (newLine) Console.WriteLine(log);
            else Console.Write(log);
        }

        public static async Task ConsoleAsync(IReadOnlyList<string> args)
        {
            if (args.Count < 1) ConsoleLog("Usage: <fpx> <script> [...]");
            else await RunAsync(args[0], args.Skip(1), ConsoleLog);
        }

        public static async Task LoadAsync(string text, string? file = null, LogDelegate? log = null)
            => await ExecuteCoreAsync(text, file, null, log);

        public static async Task LoadAsync(string file, LogDelegate? log = null)
            => await ExecuteCoreAsync(await ReadAllTextAsync(file), file, null, log);

        public static async Task RunAsync(string text, IEnumerable<string> args, string? file = null,
            LogDelegate? log = null)
            => await ExecuteCoreAsync(text, file, args, log);

        public static async Task RunAsync(string file, IEnumerable<string> args, LogDelegate? log = null)
            => await ExecuteCoreAsync(await ReadAllTextAsync(file), file, args, log);

        private static async Task<string> ReadAllTextAsync(string file)
        {
#if NET5_0
            return await File.ReadAllTextAsync(file);
#else
            await Task.Yield();
            return File.ReadAllText(file);
#endif
        }

        private static async Task ExecuteCoreAsync(string text, string? file, IEnumerable<string>? args,
            LogDelegate? log)
        {
            string directory = (!string.IsNullOrEmpty(file) && File.Exists(file)
                ? Path.GetDirectoryName(file)
                : null) ?? Directory.GetCurrentDirectory();
            log ??= (_, _) => { };
            var options = new ExecuteCodeCommandOptions(text, directory,
                args?.ToArray() ?? new[] {Processor.NO_EXECUTE_CLI},
                OptimizationLevel.Debug, false, null);
            LogDebug(log, "Executing script...");
            var logWriter = new LogWriter(log);
            await new ExecuteCodeCommand(new ScriptConsole(logWriter, TextReader.Null, logWriter),
                    CreateLogFactory(log))
                .Execute<int>(options);
            LogDebug(log, "Execution finished.");
        }

        [Conditional("DEBUG")]
        private static void LogDebug(LogDelegate logDelegate, string? log, bool newLine = true) =>
            logDelegate(log, newLine);

        // ReSharper disable UnusedParameter.Local
        private static LogFactory CreateLogFactory(LogDelegate log) => t => (l, m, e) =>
            // ReSharper restore UnusedParameter.Local
        {
            switch (l)
            {
                case LogLevel.Warning:
                case LogLevel.Critical:
                case LogLevel.Error:
                    log(m);
                    break;
                case LogLevel.Trace:
                case LogLevel.Debug:
                case LogLevel.Info:
                    LogDebug(log, "Execution finished.");
                    break;
            }
        };


        public class LogWriter : TextWriter
        {
            private readonly LogDelegate _log;
            public LogWriter(LogDelegate log) => _log = log;
            public override Encoding Encoding => Encoding.Unicode;
            public override void Write(char value) => _log(value.ToString(), false);
            public override void Write(string? value) => _log(value, false);
            public override void WriteLine(string? value) => _log(value);
        }
    }
}
