using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Fp
{
    /// <summary>
    /// Execution manager
    /// </summary>
    public static class Coordinator
    {
        /// <summary>
        /// Default output folder name
        /// </summary>
        public const string DefaultOutputFolderName = "fp_output";

        /// <summary>
        /// Default name for current executable (argv[0] or generic name)
        /// </summary>
        public static string DefaultCurrentExecutableName
        {
            get
            {
                if (_defaultCurrentExecutableName != null) return _defaultCurrentExecutableName;
                try
                {
                    _defaultCurrentExecutableName = Environment.GetCommandLineArgs()[0];
                }
                catch
                {
                    _defaultCurrentExecutableName = "<program>";
                }

                return _defaultCurrentExecutableName;
            }
        }

        /// <summary>
        /// Gets logger factory with console logger
        /// </summary>
        /// <returns></returns>
        public static LoggerFactory GetDefaultConsoleLoggerFactory() =>
            new(new[] {new ConsoleLoggerProvider(new ConsoleLogger.Config())});

        private static string? _defaultCurrentExecutableName;

        /// <summary>
        /// Get processor configuration from cli
        /// </summary>
        /// <param name="exeName">Executable name</param>
        /// <param name="args">Command-line arguments</param>
        /// <param name="loggerFactory">Logger for errors</param>
        /// <param name="enableParallel">If true, enable async options</param>
        /// <param name="configuration">Generated configuration</param>
        /// <param name="inputs">Generated input sources</param>
        /// <returns>True if parsing succeeded</returns>
        public static bool CliGetConfiguration(IList<string> exeName, IReadOnlyList<string> args,
            ILoggerFactory? loggerFactory, bool enableParallel, out ProcessorConfiguration? configuration,
            out List<(bool, string, string)> inputs)
        {
            loggerFactory ??= NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(exeName[exeName.Count - 1]);
            configuration = null;
            inputs = new List<(bool, string, string)>();
            List<string> exArgs = new();
            string? outputRootDirectory = null;
            int parallel = 1;
            bool preload = false;
            bool debug = false;
            bool nop = false;
            bool argTime = false;
            for (int i = 0; i < args.Count; i++)
            {
                string str = args[i];
                if (argTime)
                {
                    exArgs.Add(str);
                    continue;
                }

                if (str.Length == 0) continue;
                if (str[0] != '-')
                {
                    string full = Path.GetFullPath(str);
                    inputs.Add((File.Exists(str), Path.GetDirectoryName(full) ?? Path.GetFullPath("/"), full));
                    continue;
                }

                switch (str.Substring(1))
                {
                    case "-":
                        argTime = true;
                        break;
                    case "d":
                    case "-debug":
                        debug = true;
                        break;
                    case "m":
                    case "-multithread":
                        if (!enableParallel)
                        {
                            logger.LogWarning("Multithreading is not currently supported, ignoring switch {Switch}",
                                str);
                            break;
                        }

                        string? arg = GetArgValue(args, i);
                        if (arg == null)
                        {
                            logger.LogError("No argument specified for switch {Switch}, requires int", str);
                            return false;
                        }

                        if (!int.TryParse(arg, out int maxParallelRes))
                        {
                            logger.LogError("Switch {Str} requires int, got {Arg}", str, arg);
                            return false;
                        }

                        if (maxParallelRes < 1)
                        {
                            logger.LogError("Switch {Switch} requires value >= 1, got {MaxParallelRes}", str,
                                maxParallelRes);
                            return false;
                        }

                        parallel = maxParallelRes;
                        i++;
                        break;
                    case "n":
                    case "-nop":
                        nop = true;
                        break;
                    case "o":
                    case "-outdir":
                        outputRootDirectory = GetArgValue(args, i);
                        i++;
                        break;
                    case "p":
                    case "-preload":
                        preload = true;
                        break;
                    default:
                        logger.LogError("Unknown switch {Switch}", str);
                        return false;
                }
            }

            if (inputs.Count == 0)
            {
                var sb = new StringBuilder(exeName[0]);
                foreach (string str in exeName.Skip(1))
                    sb.Append(' ').Append(str);

                var sb2 = new StringBuilder();
                foreach (var x in Processor.Registered.Factories)
                {
                    var i = x.Info;
                    sb2.Append(i.Name).AppendLine()
                        .Append("    Extensions:");
                    if (i.Extensions.Length == 0) sb2.Append(" all");
                    foreach (string? ext in i.Extensions)
                        sb2.Append(", ").Append(ext ?? "<empty>");
                    sb2.AppendLine()
                        .Append("    ").Append(i.ExtendedDescription.Replace("\n", "\n    ")).AppendLine();
                }

                logger.LogInformation(@"Usage:
    {ExeName} <inputs...> [options/flags] [-- [args...]]

{Processors}
Parameters:
    inputs           : Input files/directories.
    args             : Arguments for processor. (Optional)

Options:
    -m|--multithread : Use specified # of workers
    -o|--outdir      : Output directory before working

Flags:
    -d|--debug       : Enable debug
    -n|--nop         : No outputs
    -p|--preload     : Load all streams to memory
", sb.ToString(), sb2.ToString());
                return false;
            }

            if (outputRootDirectory == null)
            {
                string commonInput = inputs[0].Item2;
                outputRootDirectory =
                    Path.Combine(
                        inputs.Any(input => commonInput != input.Item2 || commonInput == input.Item3)
                            ? Path.GetFullPath(".")
                            : commonInput,
                        DefaultOutputFolderName);
            }

            configuration =
                new ProcessorConfiguration(outputRootDirectory, parallel, preload, debug, nop, logger, exArgs);
            return true;
        }

        /// <summary>
        /// Process filesystem tree using command-line argument inputs
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        /// <param name="exeName">Executable name</param>
        /// <param name="fileSystem">Filesystem to read from</param>
        /// <param name="loggerFactory">Log output target</param>
        /// <returns>A task that will execute recursively</returns>
        /// <exception cref="ArgumentException">If invalid argument count is provided</exception>
        public static void CliRunFilesystem<T>(string[] args, IList<string>? exeName = null,
            ILoggerFactory? loggerFactory = null, FileSystemSource? fileSystem = null) where T : Processor, new() =>
            CliRunFilesystem(args, exeName, loggerFactory, fileSystem, Processor.GetFactory<T>());

        /// <summary>
        /// Process filesystem tree using command-line argument inputs
        /// </summary>
        /// <param name="exeName">Executable name</param>
        /// <param name="args">Command-line arguments</param>
        /// <param name="fileSystem">Filesystem to read from</param>
        /// <param name="processorFactories">Functions that create new processor instances</param>
        /// <param name="loggerFactory">Log output target</param>
        /// <returns>A task that will execute recursively</returns>
        /// <exception cref="ArgumentException">If invalid argument count is provided</exception>
        public static void CliRunFilesystem(string[] args, IList<string>? exeName, ILoggerFactory? loggerFactory,
            FileSystemSource? fileSystem, params ProcessorFactory[] processorFactories)
        {
            exeName ??= GuessExe(args);
            loggerFactory ??= GetDefaultConsoleLoggerFactory();
            fileSystem ??= FileSystemSource.Default;
            if (!CliGetConfiguration(exeName, args, loggerFactory, false, out ProcessorConfiguration? conf,
                out var inputs)) return;
            switch (conf!.Parallel)
            {
                case 0:
                    Recurse(inputs, new ExecutionSource(conf, fileSystem), processorFactories);
                    break;
                default:
                    RecurseAsync(inputs, new ExecutionSource(conf, fileSystem), processorFactories).Wait();
                    break;
            }
        }

        /// <summary>
        /// Process filesystem tree using command-line argument inputs
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        /// <param name="exeName">Executable name</param>
        /// <param name="fileSystem">Filesystem to read from</param>
        /// <param name="loggerFactory">Log output target</param>
        /// <returns>A task that will execute recursively</returns>
        /// <exception cref="ArgumentException">If invalid argument count is provided</exception>
        public static async Task CliRunFilesystemAsync<T>(string[] args, IList<string>? exeName = null,
            ILoggerFactory? loggerFactory = null, FileSystemSource? fileSystem = null) where T : Processor, new() =>
            await CliRunFilesystemAsync(args, exeName, loggerFactory, fileSystem, Processor.GetFactory<T>());

        /// <summary>
        /// Process filesystem tree using command-line argument inputs
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        /// <param name="exeName">Executable name</param>
        /// <param name="fileSystem">Filesystem to read from</param>
        /// <param name="processorFactories">Functions that create new processor instances</param>
        /// <param name="loggerFactory">Log output target</param>
        /// <returns>A task that will execute recursively</returns>
        /// <exception cref="ArgumentException">If invalid argument count is provided</exception>
        public static async Task CliRunFilesystemAsync(string[] args, IList<string>? exeName,
            ILoggerFactory? loggerFactory,
            FileSystemSource? fileSystem, params ProcessorFactory[] processorFactories)
        {
            exeName ??= GuessExe(args);
            loggerFactory ??= GetDefaultConsoleLoggerFactory();
            fileSystem ??= FileSystemSource.Default;
            if (!CliGetConfiguration(exeName, args, loggerFactory, true, out ProcessorConfiguration? conf,
                out var inputs)) return;
            switch (conf!.Parallel)
            {
                case 0:
                    // ReSharper disable once MethodHasAsyncOverload
                    Recurse(inputs, new ExecutionSource(conf, fileSystem), processorFactories);
                    break;
                default:
                    await RecurseAsync(inputs, new ExecutionSource(conf, fileSystem), processorFactories);
                    break;
            }
        }

        /// <summary>
        /// Guess executable string (might be multiple components) based on args
        /// </summary>
        /// <param name="args">Arguments to check</param>
        /// <param name="prependDotNetIfDll">If the first element ends in .dll, prepend dotnet as an element</param>
        /// <returns></returns>
        /// <remarks>
        /// Just matches up the tail and sends the rest, fallback on argv[0]
        /// </remarks>
        public static IList<string> GuessExe(IList<string>? args, bool prependDotNetIfDll = true)
        {
            var list = GuessExeCore(args);
            if (list[0].EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                list.Insert(0, "dotnet");
            return list;
        }

        private static List<string> GuessExeCore(IList<string>? args)
        {
            if (args == null) return new List<string> {DefaultCurrentExecutableName};
            string[] oargs = Environment.GetCommandLineArgs();
            int i = 0;
            while (i < args.Count && i < oargs.Length)
            {
                if (args[args.Count - i - 1] != oargs[oargs.Length - i - 1])
                    return new List<string> {DefaultCurrentExecutableName};
                i++;
            }

            i = oargs.Length - i;
            if (i > 0) return new List<string>(new ArraySegment<string>(oargs, 0, i));
            return new List<string> {DefaultCurrentExecutableName};
        }

        private static string? GetArgValue(IReadOnlyList<string> args, int cPos) =>
            cPos + 1 >= args.Count ? null : args[cPos + 1];

        private static (Processor[] processors, int baseCount, int parallelCount) InitializeProcessors(
            ProcessorConfiguration configuration, ProcessorFactory[] processorFactories)
        {
            if (processorFactories.Length == 0)
                throw new ArgumentException("Cannot start operation with 0 provided processors");
            if (configuration.Parallel < 1)
                throw new ArgumentException(
                    $"Illegal {nameof(configuration.Parallel)} value of {configuration.Parallel}");
            int parallelCount = Math.Min(TaskScheduler.Current.MaximumConcurrencyLevel,
                Math.Max(1, configuration.Parallel));
            int baseCount = processorFactories.Length;
            Processor[] processors = new Processor[parallelCount * baseCount];
            for (int iParallel = 0; iParallel < parallelCount; iParallel++)
            for (int iBase = 0; iBase < baseCount; iBase++)
                processors[iParallel * baseCount + iBase] = processorFactories[iBase].CreateProcessor();
            return (processors, baseCount, parallelCount);
        }

        private static (Queue<(string inputRoot, string curDir)> dQueue, Queue<(string inputRoot, string file)> fQueue)
            SeedInputs(
                IEnumerable<(bool isFile, string dir, string item)> inputs)
        {
            Queue<(string, string)> dQueue = new();
            Queue<(string, string)> fQueue = new();
            foreach ((bool isFile, string dir, string item) in inputs)
                (isFile ? fQueue : dQueue).Enqueue((dir, item));
            return (dQueue, fQueue);
        }

        private static void GetMoreInputs(FileSystemSource fileSystem, Queue<(string inputRoot, string curDir)> dQueue,
            Queue<(string inputRoot, string file)> fQueue)
        {
            (string inputRoot, string curDir) = dQueue.Dequeue();
            if (!fileSystem.DirectoryExists(curDir)) return;
            foreach (string file in fileSystem.EnumerateFiles(curDir))
                fQueue.Enqueue((inputRoot, file));
            foreach (string folder in fileSystem.EnumerateDirectories(curDir))
                dQueue.Enqueue((inputRoot, folder));
        }

        private static bool _TryDequeue<T>(this Queue<T> queue, out T? result)
        {
            if (queue.Count != 0)
            {
                result = queue.Dequeue();
                return true;
            }

            result = default;
            return false;
        }

        /// <summary>
        /// Process filesystem tree asynchronously
        /// </summary>
        /// <param name="inputs">Input sources</param>
        /// <param name="src">Execution source</param>
        /// <param name="processorFactories">Functions that create new processor instances</param>
        /// <returns>Task that will execute recursively</returns>
        /// <exception cref="ArgumentException">If <paramref name="processorFactories"/> is empty or passed a <see cref="ProcessorConfiguration.Parallel"/> value less than 1</exception>
        public static async Task RecurseAsync(IReadOnlyList<(bool, string, string)> inputs,
            ExecutionSource src, params ProcessorFactory[] processorFactories)
        {
            var (processors, baseCount, parallelCount) = InitializeProcessors(src.Config, processorFactories);
            var (dQueue, fQueue) = SeedInputs(inputs);
            Dictionary<Task, int> tasks = new();
            src.FileSystem.ParallelAccess = true;
            while (fQueue.Count != 0 || dQueue.Count != 0)
                if (fQueue._TryDequeue(out var deq))
                    for (int iBase = 0; iBase < baseCount; iBase++)
                    {
                        while (tasks.Count >= parallelCount) tasks.Remove(await Task.WhenAny(tasks.Keys));
                        int workerId = Enumerable.Range(0, parallelCount).Except(tasks.Values).First();
                        Processor processor = processors[workerId * parallelCount + iBase];
                        if (!processor.AcceptFile(deq.file)) continue;
                        tasks.Add(Task.Run(() => Run(processor, deq, src, workerId)), workerId);
                    }
                else
                    GetMoreInputs(src.FileSystem, dQueue, fQueue);

            await Task.WhenAll(tasks.Keys);
        }

        /// <summary>
        /// Represents execution config
        /// </summary>
        public record ExecutionSource(ProcessorConfiguration Config, FileSystemSource FileSystem);

        /// <summary>
        /// Process filesystem tree
        /// </summary>
        /// <param name="inputs">Input sources</param>
        /// <param name="src">Execution source</param>
        /// <param name="processorFactories">Functions that create new processor instances</param>
        /// <exception cref="ArgumentException">If <paramref name="processorFactories"/> is empty or passed a <see cref="ProcessorConfiguration.Parallel"/> value less than 1</exception>
        public static void Recurse(IReadOnlyList<(bool, string, string)> inputs, ExecutionSource src,
            params ProcessorFactory[] processorFactories)
        {
            if (src.Config.Parallel != 1)
                throw new ArgumentException(
                    $"Cannot start synchronous operation with {nameof(src.Config.Parallel)} value of {src.Config.Parallel}, use {nameof(Coordinator)}.{nameof(RecurseAsync)} instead");
            var (processors, baseCount, _) = InitializeProcessors(src.Config, processorFactories);
            var (dQueue, fQueue) = SeedInputs(inputs);
            while (fQueue.Count != 0 || dQueue.Count != 0)
                if (fQueue._TryDequeue(out var deq))
                    for (int iBase = 0; iBase < baseCount; iBase++)
                    {
                        var processor = processors[iBase];
                        if (!processor.AcceptFile(deq.file)) continue;
                        var res = Run(processor, deq, src, iBase);
                        if (res.Locked) break;
                    }
                else
                    GetMoreInputs(src.FileSystem, dQueue, fQueue);
        }

        /// <summary>
        /// Operate on a file
        /// </summary>
        /// <param name="processor">Processor to operate with</param>
        /// <param name="source">Source info</param>
        /// <param name="src">Execution source</param>
        /// <param name="workerId">Worker ID</param>
        /// <returns>Processing result</returns>
        public static ProcessResult Run(Processor processor, (string inputRoot, string file) source,
            ExecutionSource src, int workerId)
        {
            try
            {
                processor.Cleanup();
                processor.Prepare(src.FileSystem, source.inputRoot, src.Config.OutputRootDirectory, source.file,
                    src.Config, workerId);
                bool success;
                if (processor.Debug)
                {
                    processor.Process();
                    success = true;
                }
                else
                {
                    try
                    {
                        processor.Process();
                        success = true;
                    }
                    catch (Exception e)
                    {
                        src.Config.Logger.LogError(e, "Exception occurred during processing:\n{Exception}", e);
                        success = false;
                    }
                }

                return new ProcessResult(success, processor.Lock);
            }
            finally
            {
                processor.Cleanup();
            }
        }

        /// <summary>
        /// Operate on a file using segmented operation
        /// </summary>
        /// <param name="processor">Processor to operate with</param>
        /// <param name="input">Source info</param>
        /// <param name="src">Execution source</param>
        /// <param name="workerId">Worker ID</param>
        /// <returns>Processing results</returns>
        public static IEnumerable<Data> RunSegmented(Processor processor, (string inputRoot, string file) input,
            ExecutionSource src, int workerId)
        {
            try
            {
                processor.Cleanup();
                processor.Prepare(src.FileSystem, input.inputRoot, src.Config.OutputRootDirectory, input.file,
                    src.Config, workerId);
                if (processor.Debug)
                {
                    return processor.ProcessSegmented();
                }
                else
                {
                    try
                    {
                        return processor.ProcessSegmented();
                    }
                    catch (Exception e)
                    {
                        src.Config.Logger.LogError(e, "Exception occurred during processing:\n{Exception}", e);
                        return Enumerable.Empty<Data>();
                    }
                }
            }
            finally
            {
                processor.Cleanup();
            }
        }

        /// <summary>
        /// Result of processing
        /// </summary>
        public struct ProcessResult
        {
            /// <summary>
            /// Successful operation
            /// </summary>
            public bool Success;

            /// <summary>
            /// Request no more runs
            /// </summary>
            public bool Locked;

            /// <summary>
            /// Create new instance of <see cref="ProcessResult"/>
            /// </summary>
            /// <param name="success">Successful operation</param>
            /// <param name="locked">Request no more runs</param>
            public ProcessResult(bool success, bool locked)
            {
                Success = success;
                Locked = locked;
            }
        }
    }
}
