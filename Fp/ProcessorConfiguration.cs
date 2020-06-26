using System;
using System.Collections.Generic;

namespace Fp
{
    /// <summary>
    /// Configuration for batch-processing fs with processors
    /// </summary>
    public struct ProcessorConfiguration
    {
        internal static readonly ProcessorConfiguration Default = new ProcessorConfiguration();

        /// <summary>
        /// Create configuration
        /// </summary>
        /// <param name="inputs">Input sources</param>
        /// <param name="outputRootDirectory">Output source</param>
        /// <param name="parallel">Thread count</param>
        /// <param name="preload">Whether to read all streams to memory</param>
        /// <param name="debug">Whether to enable <see cref="Processor.Debug"/></param>
        /// <param name="logger">Log writer</param>
        /// <param name="args">Arguments</param>
        public ProcessorConfiguration(IReadOnlyList<(bool, string, string)> inputs, string outputRootDirectory,
            int parallel, bool preload, bool debug, Action<string> logger, IReadOnlyList<string> args)
        {
            Inputs = inputs ?? throw new ArgumentNullException(nameof(inputs));
            OutputRootDirectory = outputRootDirectory ?? throw new ArgumentException(nameof(outputRootDirectory));
            Parallel = parallel;
            Preload = preload;
            Debug = debug;
            Logger = logger;
            Args = args;
        }

        /// <summary>
        /// Input sources
        /// </summary>
        public IReadOnlyList<(bool, string, string)> Inputs { get; }

        /// <summary>
        /// Output source
        /// </summary>
        public string OutputRootDirectory { get; }

        /// <summary>
        /// Thread count
        /// </summary>
        public int Parallel { get; }

        /// <summary>
        /// Whether to read all streams to memory
        /// </summary>
        public bool Preload { get; }

        /// <summary>
        /// Whether to enable <see cref="Processor.Debug"/>
        /// </summary>
        public bool Debug { get; }

        /// <summary>
        /// Log writer
        /// </summary>
        public Action<string> Logger { get; }

        /// <summary>
        /// Arguments
        /// </summary>
        public IReadOnlyList<string> Args { get; }
    }
}
