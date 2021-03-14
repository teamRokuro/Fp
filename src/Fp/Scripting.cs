using System;
using System.Collections.Generic;
using System.Linq;

namespace Fp
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Provides scripting-related functions/properties.
    /// </summary>
    public static partial class Scripting
    {
        /// <summary>
        /// Registered scripting processors
        /// </summary>
        public static readonly ProcessorSource processors = new();

        /// <summary>
        /// Process using processor factory.
        /// </summary>
        /// <param name="fileSystemSource">Filesystem.</param>
        /// <param name="args">Arguments. If null, only register processors.</param>
        /// <param name="factories">Processor factories.</param>
        public static void fpCliFilesystem(FileSystemSource? fileSystemSource, IList<string>? args,
            params ProcessorFactory[] factories)
        {
            processors.Factories.UnionWith(factories);
            if (args == null || args.Count == 1 && args[0] == NO_EXECUTE_CLI) return;
            Coordinator.CliRunFilesystem(args.ToArray(), default, default, fileSystemSource, factories);
        }

        /// <summary>
        /// Keyword arg for stopping cli execution
        /// </summary>
        public const string NO_EXECUTE_CLI = "--no-execute-cli";

        /// <summary>
        /// Process using direct function.
        /// </summary>
        /// <param name="func">Function or delegate run per file.</param>
        /// <param name="args">Arguments. If null, only register processor.</param>
        /// <param name="info">Processor info.</param>
        public static void fp(Action func, IList<string>? args, ProcessorInfo? info = null) =>
            fpCliFilesystem(null, args,
                new DelegateProcessorFactory(info, () => new ScriptingDirectProcessor(func)));

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
            fp(func, args, new ProcessorInfo(name, description, description, extensions));

        /// <summary>
        /// Process using segmented function.
        /// </summary>
        /// <param name="func">Function that returns enumerable (segmented processing enumerator).</param>
        /// <param name="args">Arguments. If null, only register processor.</param>
        /// <param name="info">Processor info.</param>
        public static void fp(Func<IEnumerable<Data>> func, IList<string>? args, ProcessorInfo? info = null) =>
            fpCliFilesystem(null, args,
                new DelegateProcessorFactory(info, () => new ScriptingSegmentedProcessor(func)));

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
            fp(func, args, new ProcessorInfo(name, description, description, extensions));
    }
    // ReSharper restore InconsistentNaming
}
