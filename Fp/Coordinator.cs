using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        /// Get processor configuration from cli
        /// </summary>
        /// <param name="exeName">Executable name</param>
        /// <param name="args">Command-line arguments</param>
        /// <param name="logger">Logger for errors</param>
        /// <param name="enableParallel">If true, enable async options</param>
        /// <param name="configuration">Generated configuration</param>
        /// <returns>True if parsing succeeded</returns>
        public static bool CliGetConfiguration(string exeName, IReadOnlyList<string> args, Action<string> logger,
            bool enableParallel, out ProcessorConfiguration configuration)
        {
            configuration = ProcessorConfiguration.Default;
            List<(bool, string, string)> inputs = new List<(bool, string, string)>();
            List<string> exArgs = new List<string>();
            string? outputRootDirectory = null;
            int parallel = 0;
            bool preload = false;
            bool debug = false;
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
                    inputs.Add((File.Exists(str),
                        Path.GetDirectoryName(Path.GetFullPath(str)) ?? Path.GetFullPath("/"), str));
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
                    case "m" when enableParallel:
                    case "-multithread" when enableParallel:
                        string? arg = GetArgValue(args, i);
                        if (arg == null)
                        {
                            logger.Invoke($"[--][X]<FAIL>: No argument specified for switch {str}, requires int");
                            return false;
                        }

                        if (!int.TryParse(arg, out int maxParallelRes))
                        {
                            logger.Invoke($"[--][X]<FAIL>: Switch {str} requires int, got {arg}");
                            return false;
                        }

                        if (maxParallelRes < 1)
                        {
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

            if (inputs.Count == 0)
            {
                if (exeName.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                    exeName = "dotnet " + exeName;
                string usageStr = $@"Usage:
    {exeName} <inputs...>";
                if (enableParallel)
                    usageStr += " [-m <value>]";
                usageStr += @" [-o <dir>] [-p] [-- [args...]]

Parameters
    inputs           : Input files/directories.
    args             : Arguments for processor. (Optional)

Options
    -d|--debug       : Enable debug";
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
                new ProcessorConfiguration(inputs, outputRootDirectory, parallel, preload, debug, logger, exArgs);
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
            FileSystemSource fileSystem,
            params Func<Processor>[] processorFactories)
        {
            if (!CliGetConfiguration(exeName, args, logger, false, out ProcessorConfiguration conf)) return;
            switch (conf.Parallel)
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
        /// <param name="exeName">Executable name</param>
        /// <param name="args">Command-line arguments</param>
        /// <param name="fileSystem">Filesystem to read from</param>
        /// <param name="processorFactories">Functions that create new processor instances</param>
        /// <param name="logger">Log output target</param>
        /// <returns>A task that will execute recursively</returns>
        /// <exception cref="ArgumentException">If invalid argument count is provided</exception>
        public static async Task CliRunFilesystemAsync(string exeName, string[] args, Action<string> logger,
            FileSystemSource fileSystem,
            params Func<Processor>[] processorFactories)
        {
            if (!CliGetConfiguration(exeName, args, logger, true, out ProcessorConfiguration conf)) return;
            switch (conf.Parallel)
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
            params Func<Processor>[] processorFactories)
        {
            if (processorFactories.Length == 0)
                throw new ArgumentException(
                    "Cannot start operation with 0 provided processors");
            if (configuration.Parallel < 0)
                throw new ArgumentException(
                    $"Cannot start operation with Parallel value of {configuration.Parallel}");
            int parallelCount = Math.Min(TaskScheduler.Current.MaximumConcurrencyLevel,
                Math.Max(1, configuration.Parallel));
            int baseCount = processorFactories.Length;
            Processor[] processors = new Processor[parallelCount * baseCount];
            bool[] actives = new bool[processors.Length];
            for (int iParallel = 0; iParallel < parallelCount; iParallel++)
            for (int iBase = 0; iBase < baseCount; iBase++)
                processors[iParallel * baseCount + iBase] = processorFactories[iBase].Invoke();

            Queue<(string, string)> dQueue = new Queue<(string, string)>();
            Queue<(string, string)> fQueue = new Queue<(string, string)>();
            foreach ((bool isFile, string dir, string item) in configuration.Inputs)
                (isFile ? fQueue : dQueue).Enqueue((dir, item));
            List<Task> tasks = new List<Task>();
            List<int> taskIdxes = new List<int>();
            List<int> taskIds = new List<int>();
            fileSystem.ParallelAccess = true;
            while (fQueue.Count != 0 || dQueue.Count != 0)
            {
                if (fQueue.Count != 0)
                {
                    (string inputRoot, string file) = fQueue.Dequeue();
                    for (int iBase = 0; iBase < baseCount; iBase++)
                    {
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
                        Processor processor = processors.Where((t, i) =>
                        {
                            if (i % baseCount != iBase || actives[i]) return false;
                            rIdx = i;
                            return true;
                        }).First();
                        actives[rIdx] = true;
                        int workerId = Enumerable.Range(0, parallelCount).Except(taskIds).First();
                        Task task = Task.Run(() =>
                            OperateFile(processor, file, inputRoot, configuration, fileSystem, workerId));
                        tasks.Add(task);
                        taskIdxes.Add(rIdx);
                        taskIds.Add(workerId);
                    }
                }
                else
                {
                    (string inputRoot, string curDir) = dQueue.Dequeue();
                    if (!Directory.Exists(curDir)) continue;
                    foreach (string file in fileSystem.EnumerateFiles(curDir))
                        fQueue.Enqueue((inputRoot, file));
                    foreach (string folder in fileSystem.EnumerateDirectories(curDir))
                        dQueue.Enqueue((inputRoot, folder));
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
        /// <exception cref="ArgumentException">If <paramref name="processorFactories"/> is empty or <paramref name="configuration"/> has a <see cref="ProcessorConfiguration.Parallel"/> value less than 1</exception>
        public static void Operate(ProcessorConfiguration configuration, FileSystemSource fileSystem,
            params Func<Processor>[] processorFactories)
        {
            if (processorFactories.Length == 0)
                throw new ArgumentException(
                    "Cannot start operation with 0 provided processors");
            if (configuration.Parallel != 0 && configuration.Parallel != 1)
                throw new ArgumentException(
                    $"Cannot start synchronous operation with {nameof(configuration.Parallel)} value of {configuration.Parallel}, use {nameof(Coordinator)}.{nameof(OperateAsync)} instead");
            int baseCount = processorFactories.Length;
            Processor[] processors = new Processor[baseCount];
            for (int iBase = 0; iBase < baseCount; iBase++)
                processors[iBase] = processorFactories[iBase].Invoke();
            Queue<(string, string)> dQueue = new Queue<(string, string)>();
            Queue<(string, string)> fQueue = new Queue<(string, string)>();
            foreach ((bool isFile, string dir, string item) in configuration.Inputs)
                (isFile ? fQueue : dQueue).Enqueue((dir, item));
            while (fQueue.Count != 0 || dQueue.Count != 0)
            {
                if (fQueue.Count != 0)
                {
                    (string inputRoot, string file) = fQueue.Dequeue();
                    for (int iBase = 0; iBase < baseCount; iBase++)
                    {
                        OperateFile(processors[iBase], file, inputRoot, configuration, fileSystem, 0);
                        if (processors[iBase].Lock)
                            break;
                    }
                }
                else
                {
                    (string inputRoot, string curDir) = dQueue.Dequeue();
                    if (!Directory.Exists(curDir)) continue;
                    foreach (string file in fileSystem.EnumerateFiles(curDir))
                        fQueue.Enqueue((inputRoot, file));
                    foreach (string folder in fileSystem.EnumerateDirectories(curDir))
                        dQueue.Enqueue((inputRoot, folder));
                }
            }
        }

        private static void OperateFile(Processor processor, string file, string inputRoot,
            ProcessorConfiguration configuration, FileSystemSource fileSystem, int workerId)
        {
            try
            {
                processor.SrcCleanup();
                processor.Prepare(fileSystem, inputRoot, configuration.OutputRootDirectory, file);
                processor.Debug = configuration.Debug;
                processor.Preload = configuration.Preload;
                processor.Logger = configuration.Logger;
                processor.WorkerId = workerId;
                processor.Process(configuration.Args);
            }
            finally
            {
                processor.SrcCleanup();
            }
        }
    }
}
