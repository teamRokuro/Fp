using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Fp {
    /// <summary>
    /// Execution manager
    /// </summary>
    public static class Coordinator {
        /// <summary>
        /// Default output folder name
        /// </summary>
        public const string DefaultOutputFolderName = "fp_output";

        /// <summary>
        /// Get processor configuration from cli
        /// </summary>
        /// <param name="exeName">Executable name</param>
        /// <param name="args">Command-line arguments</param>
        /// <param name="logger">Logger for errors</param>
        /// <param name="enableParallel">If true, enable async options</param>
        /// <param name="configuration">Generated configuration</param>
        /// <returns>True if parsing succeeded</returns>
        public static bool CliGetConfiguration(string exeName, IReadOnlyList<string> args, Action<string> logger,
            bool enableParallel, out ProcessorConfiguration configuration) {
            configuration = ProcessorConfiguration.Default;
            var inputs = new List<Tuple<bool, string, string>>();
            var exArgs = new List<string>();
            string outputRootDirectory = null;
            var parallel = 0;
            var preload = false;
            var argTime = false;
            for (var i = 0; i < args.Count; i++) {
                var str = args[i];
                if (argTime) {
                    exArgs.Add(str);
                    continue;
                }

                if (str.Length == 0) continue;
                if (str[0] != '-') {
                    inputs.Add(new Tuple<bool, string, string>(File.Exists(str),
                        Path.GetDirectoryName(Path.GetFullPath(str)), str));
                    continue;
                }

                switch (str.Substring(1)) {
                    case "-":
                        argTime = true;
                        break;
                    case "m" when enableParallel:
                    case "-multithread" when enableParallel:
                        var arg = GetArgValue(args, i);
                        if (arg == null) {
                            logger.Invoke($"[--][X]<FAIL>: No argument specified for switch {str}, requires int");
                            return false;
                        }

                        if (!int.TryParse(arg, out var maxParallelRes)) {
                            logger.Invoke($"[--][X]<FAIL>: Switch {str} requires int, got {arg}");
                            return false;
                        }

                        if (maxParallelRes < 1) {
                            logger.Invoke($"[--][X]<FAIL>: Switch {str} requires value >= 1, got {maxParallelRes}");
                            return false;
                        }

                        parallel = maxParallelRes;
                        i++;
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
                        logger.Invoke($"[--][X]<FAIL>: Unknown switch {str}");
                        return false;
                }
            }

            if (inputs.Count == 0) {
                if (exeName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                    exeName = "dotnet " + exeName;
                var usageStr = $@"Usage:
    {exeName} <inputs...>";
                if (enableParallel)
                    usageStr += " [-m <value>]";
                usageStr += @" [-o <dir>] [-p] [-- [args...]]

Parameters
    inputs           : Input files/directories.
    args             : Arguments for processor. (Optional)

Options";
                if (enableParallel)
                    usageStr += @"
    -m|--multithread : Use specified # of workers";
                usageStr += @"
    -o|--outdir      : Output directory
    -p|--preload     : Load all streams to memory
                       before working";
                logger.Invoke(usageStr);
                return false;
            }

            if (outputRootDirectory == null) {
                var commonInput = inputs[0].Item2;
                outputRootDirectory =
                    Path.Combine(
                        inputs.Any(input => commonInput != input.Item2 || commonInput == input.Item3)
                            ? Path.GetFullPath(".")
                            : commonInput,
                        DefaultOutputFolderName);
            }

            configuration = new ProcessorConfiguration(inputs, outputRootDirectory, parallel, preload, logger, exArgs);
            return true;
        }

        /// <summary>
        /// Process filesystem tree using command-line argument inputs
        /// </summary>
        /// <param name="exeName">Executable name</param>
        /// <param name="args">Command-line arguments</param>
        /// <param name="fileSystem">Filesystem to read from</param>
        /// <param name="processorFactories">Functions that create new processor instances</param>
        /// <param name="logger">Log output target</param>
        /// <returns>A task that will execute recursively</returns>
        /// <exception cref="ArgumentException">If invalid argument count is provided</exception>
        public static void CliRunFilesystem(string exeName, string[] args, Action<string> logger,
            FileSystemSource fileSystem, params Func<(string path, string name, Processor processor)>[] processorFactories) {
            if (!CliGetConfiguration(exeName, args, logger, false, out var conf)) return;
            switch (conf.Parallel) {
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
        /// <param name="exeName">Executable name</param>
        /// <param name="args">Command-line arguments</param>
        /// <param name="fileSystem">Filesystem to read from</param>
        /// <param name="processorFactories">Functions that create new processor instances</param>
        /// <param name="logger">Log output target</param>
        /// <returns>A task that will execute recursively</returns>
        /// <exception cref="ArgumentException">If invalid argument count is provided</exception>
        public static async Task CliRunFilesystemAsync(string exeName, string[] args, Action<string> logger,
            FileSystemSource fileSystem, params Func<(string path, string name, Processor processor)>[] processorFactories) {
            if (!CliGetConfiguration(exeName, args, logger, true, out var conf)) return;
            switch (conf.Parallel) {
                case 0:
                    Operate(conf, fileSystem, processorFactories);
                    break;
                default:
                    await OperateAsync(conf, fileSystem, processorFactories);
                    break;
            }
        }

        private static string GetArgValue(IReadOnlyList<string> args, int cPos) =>
            cPos + 1 >= args.Count ? null : args[cPos + 1];

        /// <summary>
        /// Process filesystem tree asynchronously
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="fileSystem">Filesystem to read from</param>
        /// <param name="processorFactories">Functions that create new processor instances</param>
        /// <returns>Task that will execute recursively</returns>
        /// <exception cref="ArgumentException">If <paramref name="processorFactories"/> is empty or <paramref name="configuration"/> has a <see cref="ProcessorConfiguration.Parallel"/> value less than 1</exception>
        public static async Task OperateAsync(ProcessorConfiguration configuration,
            FileSystemSource fileSystem, params Func<(string path, string name, Processor processor)>[] processorFactories) {
            if (processorFactories.Length == 0)
                throw new ArgumentException(
                    "Cannot start operation with 0 provided processors");
            if (configuration.Parallel < 0)
                throw new ArgumentException(
                    $"Cannot start operation with Parallel value of {configuration.Parallel}");
            var parallelCount = Math.Min(TaskScheduler.Current.MaximumConcurrencyLevel, configuration.Parallel);
            var baseCount = processorFactories.Length;
            var processors = new Processor[parallelCount, baseCount];
            for (var iParallel = 0; iParallel < parallelCount; iParallel++)
            for (var iBase = 0; iBase < baseCount; iBase++)
                processors[iParallel, iBase] = processorFactories[iBase].Invoke().processor;

            var dQueue = new Queue<Tuple<string, string>>();
            var fQueue = new Queue<Tuple<string, string>>();
            foreach (var (isFile, dir, item) in configuration.Inputs)
                (isFile ? fQueue : dQueue).Enqueue(new Tuple<string, string>(dir, item));
            var tasks = new List<Task>();
            fileSystem.ParallelAccess = true;
            while (fQueue.Count != 0 || dQueue.Count != 0) {
                if (fQueue.Count != 0) {
                    var (inputRoot, file) = fQueue.Dequeue();
                    for (var iBase = 0; iBase < baseCount; iBase++) {
                        var iParallelLcl = tasks.Count;
                        while (tasks.Count >= parallelCount) {
                            var completed = await Task.WhenAny(tasks);
                            iParallelLcl = tasks.IndexOf(completed);
                            tasks.Remove(completed);
                        }

                        var iBaseLcl = iBase;

                        var task = Task.Run(() =>
                            OperateFile(processors[iParallelLcl, iBaseLcl], file, inputRoot, configuration, fileSystem,
                                iParallelLcl));
                        tasks.Insert(iParallelLcl, task);
                    }
                }
                else {
                    var (inputRoot, curDir) = dQueue.Dequeue();
                    if (!Directory.Exists(curDir)) continue;
                    foreach (var file in fileSystem.EnumerateFiles(curDir))
                        fQueue.Enqueue(new Tuple<string, string>(inputRoot, file));
                    foreach (var folder in fileSystem.EnumerateDirectories(curDir))
                        dQueue.Enqueue(new Tuple<string, string>(inputRoot, folder));
                }
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Process filesystem tree
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="fileSystem">Filesystem to read from</param>
        /// <param name="processorFactories">Functions that create new processor instances</param>
        /// <returns>Task that will execute recursively</returns>
        /// <exception cref="ArgumentException">If <paramref name="processorFactories"/> is empty or <paramref name="configuration"/> has a <see cref="ProcessorConfiguration.Parallel"/> value less than 1</exception>
        public static void Operate(ProcessorConfiguration configuration, FileSystemSource fileSystem,
            params Func<(string path, string name, Processor processor)>[] processorFactories) {
            if (processorFactories.Length == 0)
                throw new ArgumentException(
                    "Cannot start operation with 0 provided processors");
            if (configuration.Parallel < 0)
                throw new ArgumentException(
                    $"Cannot start synchronous operation with {nameof(configuration.Parallel)} value of {configuration.Parallel}, use {nameof(Coordinator)}.{nameof(OperateAsync)} instead");
            var baseCount = processorFactories.Length;
            var processors = new Processor[baseCount];
            for (var iBase = 0; iBase < baseCount; iBase++)
                processors[iBase] = processorFactories[iBase].Invoke().processor;
            var dQueue = new Queue<Tuple<string, string>>();
            var fQueue = new Queue<Tuple<string, string>>();
            foreach (var (isFile, dir, item) in configuration.Inputs)
                (isFile ? fQueue : dQueue).Enqueue(new Tuple<string, string>(dir, item));
            while (fQueue.Count != 0 || dQueue.Count != 0) {
                if (fQueue.Count != 0) {
                    var (inputRoot, file) = fQueue.Dequeue();
                    for (var iBase = 0; iBase < baseCount; iBase++) {
                        OperateFile(processors[iBase], file, inputRoot, configuration, fileSystem, 0);
                        if(processors[iBase].Lock)
                            break;
                    }
                }
                else {
                    var (inputRoot, curDir) = dQueue.Dequeue();
                    if (!Directory.Exists(curDir)) continue;
                    foreach (var file in fileSystem.EnumerateFiles(curDir))
                        fQueue.Enqueue(new Tuple<string, string>(inputRoot, file));
                    foreach (var folder in fileSystem.EnumerateDirectories(curDir))
                        dQueue.Enqueue(new Tuple<string, string>(inputRoot, folder));
                }
            }
        }

        private static void OperateFile(Processor processor, string file, string inputRoot,
            ProcessorConfiguration configuration, FileSystemSource fileSystem, int workerId) {
            try {
                processor.InputRootDirectory = inputRoot;
                processor.InputFile = file;
                processor.InputDirectory = Path.GetDirectoryName(file) ?? throw new Exception();
                processor.OutputRootDirectory = configuration.OutputRootDirectory;
                processor.OutputDirectory = Processor.BasicJoin(false, configuration.OutputRootDirectory,
                    processor.InputDirectory.Substring(inputRoot.Length));
                processor.InputStream = null;
                processor.OutputStream = null;
                processor.LittleEndian = true;
                processor.OutputCounter = 0;
                processor.FileSystem = fileSystem;
                processor.Logger = configuration.Logger;
                processor.Preload = configuration.Preload;
                processor.WorkerId = workerId;
                processor.SupportBackSlash = false;
                processor.Process(configuration.Args);
            }
            finally {
                processor.SrcCleanup();
            }
        }
    }
}