using System;
using System.Collections.Generic;
using System.Linq;

namespace Fp
{
    // ReSharper disable InconsistentNaming
    public partial class Processor
    {
        /// <summary>
        /// Keyword arg for stopping cli execution
        /// </summary>
        public const string NO_EXECUTE_CLI = "--no-execute-cli";

        /// <summary>
        /// Registered scripting processors
        /// </summary>
        public static readonly ProcessorSource Registered = new();

        /// <summary>
        /// Process using processor factory.
        /// </summary>
        /// <param name="fileSystemSource">Filesystem.</param>
        /// <param name="args">Arguments. If null, only register processors.</param>
        /// <param name="factories">Processor factories.</param>
        public static void Run(FileSystemSource? fileSystemSource, IList<string>? args,
            params ProcessorFactory[] factories)
        {
            Registered.Factories.UnionWith(factories);
            if (args == null || args.Count == 1 && args[0] == NO_EXECUTE_CLI) return;
            Coordinator.CliRunFilesystem(args.ToArray(), default, default, fileSystemSource, factories);
        }

        /// <summary>
        /// Process using direct function.
        /// </summary>
        /// <param name="func">Function or delegate run per file.</param>
        /// <param name="args">Arguments. If null, only register processor.</param>
        /// <param name="info">Processor info.</param>
        public static void Run(Action func, IList<string>? args, ProcessorInfo? info = null) =>
            Run(null, args,
                new DelegateProcessorFactory(info, () => new ScriptingDirectProcessor(func)));

        /// <summary>
        /// Process using direct function.
        /// </summary>
        /// <param name="func">Function or delegate run per file.</param>
        /// <param name="args">Arguments. If null, only register processor.</param>
        /// <param name="name">Processor name</param>
        /// <param name="description">Processor description</param>
        /// <param name="extensions">Processor extensions</param>
        public static void Run(Action func, IList<string>? args, string name, string description,
            params string[] extensions) =>
            Run(func, args, new ProcessorInfo(name, description, description, extensions));

        /// <summary>
        /// Process using segmented function.
        /// </summary>
        /// <param name="func">Function that returns enumerable (segmented processing enumerator).</param>
        /// <param name="args">Arguments. If null, only register processor.</param>
        /// <param name="info">Processor info.</param>
        public static void Run(Func<IEnumerable<Data>> func, IList<string>? args, ProcessorInfo? info = null) =>
            Run(null, args,
                new DelegateProcessorFactory(info, () => new ScriptingSegmentedProcessor(func)));

        /// <summary>
        /// Process using segmented function.
        /// </summary>
        /// <param name="func">Function that returns enumerable (segmented processing enumerator).</param>
        /// <param name="args">Arguments. If null, only register processor.</param>
        /// <param name="name">Processor name</param>
        /// <param name="description">Processor description</param>
        /// <param name="extensions">Processor extensions</param>
        public static void Run(Func<IEnumerable<Data>> func, IList<string>? args, string name, string description,
            params string[] extensions) =>
            Run(func, args, new ProcessorInfo(name, description, description, extensions));

        /// <summary>
        /// Process using segmented function.
        /// </summary>
        /// <param name="args">Arguments. If null, only register processor.</param>
        /// <param name="name">Processor name</param>
        /// <param name="description">Processor description</param>
        /// <param name="extensions">Processor extensions</param>
        /// <typeparam name="T">Processor type</typeparam>
        public static void Run<T>(IList<string>? args, string name, string description,
            params string[] extensions) where T : Processor, new() =>
            Run(null, args,
                new GenericNewProcessorFactory<T>(new ProcessorInfo(name, description, description, extensions)));
    }

    public partial class Scripting
    {
        /// <summary>
        /// Process using direct function.
        /// </summary>
        /// <param name="func">Function or delegate run per file.</param>
        /// <param name="args">Arguments. If null, only register processor.</param>
        /// <param name="info">Processor info.</param>
        public static void fp(Action func, IList<string>? args, ProcessorInfo? info = null) =>
            Processor.Run(func, args, info);

        /// <summary>
        /// Process using direct function.
        /// </summary>
        /// <param name="func">Function or delegate run per file.</param>
        /// <param name="args">Arguments. If null, only register processor.</param>
        /// <param name="name">Processor name</param>
        /// <param name="description">Processor description</param>
        /// <param name="extensions">Processor extensions</param>
        public static void fp(Action func, IList<string>? args, string name, string description,
            params string[] extensions) =>
            Processor.Run(func, args, name, description, extensions);

        /// <summary>
        /// Process using segmented function.
        /// </summary>
        /// <param name="func">Function that returns enumerable (segmented processing enumerator).</param>
        /// <param name="args">Arguments. If null, only register processor.</param>
        /// <param name="info">Processor info.</param>
        public static void fp(Func<IEnumerable<Data>> func, IList<string>? args, ProcessorInfo? info = null) =>
            Processor.Run(func, args, info);

        /// <summary>
        /// Process using segmented function.
        /// </summary>
        /// <param name="func">Function that returns enumerable (segmented processing enumerator).</param>
        /// <param name="args">Arguments. If null, only register processor.</param>
        /// <param name="name">Processor name</param>
        /// <param name="description">Processor description</param>
        /// <param name="extensions">Processor extensions</param>
        public static void fp(Func<IEnumerable<Data>> func, IList<string>? args, string name, string description,
            params string[] extensions) =>
            Processor.Run(func, args, name, description, extensions);
    }
    // ReSharper restore InconsistentNaming
}
