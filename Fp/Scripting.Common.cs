using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Fp
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Provides scripting-related functions/properties.
    /// </summary>
    public static partial class Scripting
    {
        /// <summary>
        /// Process using processor factory.
        /// </summary>
        /// <param name="factory">Processor factory.</param>
        /// <param name="args">Arguments.</param>
        /// <param name="fileSystemSource">Filesystem.</param>
        /// <param name="name">Program name (for help text).</param>
        public static void process(Func<Processor> factory, IList<string> args,
            FileSystemSource? fileSystemSource = null, string? name = null) =>
            Coordinator.CliRunFilesystem(
                name ?? "<program>",
                args.ToArray(),
                new LoggerFactory(new ILoggerProvider[] {new ConsoleLoggerProvider(new ConsoleLogger.Config())}),
                fileSystemSource ?? FileSystemSource.Default,
                factory);

        /// <summary>
        /// Process using direct function.
        /// </summary>
        /// <param name="func">Function or delegate run per file.</param>
        /// <param name="args">Arguments.</param>
        /// <param name="fileSystemSource">Filesystem.</param>
        /// <param name="name">Program name (for help text).</param>
        public static void process(Action func, IList<string> args, FileSystemSource? fileSystemSource = null,
            string? name = null) =>
            process(() => new CurrentDirectProcessor(func), args, fileSystemSource,
                name ?? func.GetType().Assembly.GetName().Name);

        /// <summary>
        /// Process using segmented function.
        /// </summary>
        /// <param name="func">Function that returns enumerable (segmented processing enumerator).</param>
        /// <param name="args">Arguments.</param>
        /// <param name="fileSystemSource">Filesystem.</param>
        /// <param name="name">Program name (for help text).</param>
        public static void process(Func<IEnumerable<Data>> func, IList<string> args,
            FileSystemSource? fileSystemSource = null, string? name = null) =>
            process(() => new CurrentSegmentedProcessor(func), args, fileSystemSource,
                name ?? func.GetType().Assembly.GetName().Name);
    }
    // ReSharper restore InconsistentNaming
}
