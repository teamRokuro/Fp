using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// <returns>True if parsing succeeded</returns>
        public static bool CliGetConfiguration(string exeName, IReadOnlyList<string> args,
            ILoggerFactory? loggerFactory, bool enableParallel, out ProcessorConfiguration? configuration)
        {
            loggerFactory ??= NullLoggerFactory.Instance;
            var logger = loggerFactory.CreateLogger(exeName);
            configuration = null;
            List<(bool, string, string)> inputs = new();
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
                if (exeName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                    exeName = "dotnet " + exeName;
                logger.LogInformation(@"Usage:
    {ExeName} <inputs...> [options/flags] [-- [args...]]

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
", exeName);
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
                new ProcessorConfiguration(inputs, outputRootDirectory, parallel, preload, debug, nop, logger, exArgs);
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
        public static void CliRunFilesystem<T>(string[] args, string? exeName = null,
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
        public static void CliRunFilesystem(string[] args, string? exeName, ILoggerFactory? loggerFactory,
            FileSystemSource? fileSystem, params ProcessorFactory[] processorFactories)
        {
            exeName ??= DefaultCurrentExecutableName;
            loggerFactory ??= GetDefaultConsoleLoggerFactory();
            fileSystem ??= FileSystemSource.Default;
            if (!CliGetConfiguration(exeName, args, loggerFactory, false, out ProcessorConfiguration? conf)) return;
            switch (conf!.Parallel)
            {
                case 0:
                    Operate(conf, fileSystem, processorFactories);
                    break;
                default:
                    OperateAsync(conf, fileSystem, processorFactories).Wait();
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
        public static async Task CliRunFilesystemAsync<T>(string[] args, string? exeName = null,
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
        public static async Task CliRunFilesystemAsync(string[] args, string? exeName, ILoggerFactory? loggerFactory,
            FileSystemSource? fileSystem, params ProcessorFactory[] processorFactories)
        {
            exeName ??= DefaultCurrentExecutableName;
            loggerFactory ??= GetDefaultConsoleLoggerFactory();
            fileSystem ??= FileSystemSource.Default;
            if (!CliGetConfiguration(exeName, args, loggerFactory, true, out ProcessorConfiguration? conf)) return;
            switch (conf!.Parallel)
            {
                case 0:
                    Operate(conf, fileSystem, processorFactories);
                    break;
                default:
                    await OperateAsync(conf, fileSystem, processorFactories);
                    break;
            }
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
        /// <param name="configuration"></param>
        /// <param name="fileSystem">Filesystem to read from</param>
        /// <param name="processorFactories">Functions that create new processor instances</param>
        /// <returns>Task that will execute recursively</returns>
        /// <exception cref="ArgumentException">If <paramref name="processorFactories"/> is empty or <paramref name="configuration"/> has a <see cref="ProcessorConfiguration.Parallel"/> value less than 1</exception>
        public static async Task OperateAsync(ProcessorConfiguration configuration,
            FileSystemSource fileSystem,
            params ProcessorFactory[] processorFactories)
        {
            var (processors, baseCount, parallelCount) = InitializeProcessors(configuration, processorFactories);
            var (dQueue, fQueue) = SeedInputs(configuration.Inputs);
            bool[] actives = new bool[processors.Length];
            List<Task> tasks = new();
            List<int> taskIdxes = new();
            List<int> taskIds = new();
            fileSystem.ParallelAccess = true;
            while (fQueue.Count != 0 || dQueue.Count != 0)
            {
                if (fQueue._TryDequeue(out var deq))
                    for (int iBase = 0; iBase < baseCount; iBase++)
                    {
                        // Wait for available task
                        while (tasks.Count >= parallelCount)
                        {
                            Task completed = await Task.WhenAny(tasks);
                            int ofs = tasks.IndexOf(completed);
                            actives[taskIdxes[ofs]] = false;
                            tasks.RemoveAt(ofs);
                            taskIdxes.RemoveAt(ofs);
                            taskIds.RemoveAt(ofs);
                        }

                        int rIdx = -1;
                        Processor processor = processors.Where((_, i) =>
                        {
                            if (i % baseCount != iBase || actives[i]) return false;
                            rIdx = i;
                            return true;
                        }).First();
                        actives[rIdx] = true;
                        int workerId = Enumerable.Range(0, parallelCount).Except(taskIds).First();
                        Task task = Task.Run(() =>
                            OperateFile(processor, deq.file, deq.inputRoot, configuration, fileSystem, workerId));
                        tasks.Add(task);
                        taskIdxes.Add(rIdx);
                        taskIds.Add(workerId);
                    }
                else
                    GetMoreInputs(fileSystem, dQueue, fQueue);
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Process filesystem tree
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="fileSystem">Filesystem to read from</param>
        /// <param name="processorFactories">Functions that create new processor instances</param>
        /// <exception cref="ArgumentException">If <paramref name="processorFactories"/> is empty or <paramref name="configuration"/> has a <see cref="ProcessorConfiguration.Parallel"/> value less than 1</exception>
        public static void Operate(ProcessorConfiguration configuration, FileSystemSource fileSystem,
            params ProcessorFactory[] processorFactories)
        {
            if (configuration.Parallel != 1)
                throw new ArgumentException(
                    $"Cannot start synchronous operation with {nameof(configuration.Parallel)} value of {configuration.Parallel}, use {nameof(Coordinator)}.{nameof(OperateAsync)} instead");
            var (processors, baseCount, _) = InitializeProcessors(configuration, processorFactories);
            var (dQueue, fQueue) = SeedInputs(configuration.Inputs);
            while (fQueue.Count != 0 || dQueue.Count != 0)
            {
                if (fQueue._TryDequeue(out var deq))
                    for (int iBase = 0; iBase < baseCount; iBase++)
                    {
                        var res = OperateFile(processors[iBase], deq.file, deq.inputRoot, configuration, fileSystem,
                            iBase);
                        if (res.Locked) break;
                    }
                else
                    GetMoreInputs(fileSystem, dQueue, fQueue);
            }
        }

        private static ProcessResult OperateFile(Processor processor, string file, string inputRoot,
            ProcessorConfiguration configuration, FileSystemSource fileSystem, int workerId)
        {
            try
            {
                processor.Cleanup();
                processor.Prepare(fileSystem, inputRoot, configuration.OutputRootDirectory, file, configuration,
                    workerId);
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
                        configuration.Logger.LogError(e, "Exception occurred during processing:\n{Exception}", e);
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

        private struct ProcessResult
        {
            public bool Success;
            public bool Locked;

            public ProcessResult(bool success, bool locked)
            {
                Success = success;
                Locked = locked;
            }
        }
    }
}
