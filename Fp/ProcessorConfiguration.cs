using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Fp
{
    /// <summary>
    /// Configuration for batch-processing fs with processors
    /// </summary>
    public record ProcessorConfiguration
    {
        /// <summary>
        /// Create configuration
        /// </summary>
        /// <param name="inputs">Input sources</param>
        /// <param name="outputRootDirectory">Output source</param>
        /// <param name="parallel">Thread count</param>
        /// <param name="preload">Whether to read all streams to memory</param>
        /// <param name="debug">Whether to enable <see cref="Processor.Debug"/></param>
        /// <param name="nop">Whether to disable outputs</param>
        /// <param name="logger">Log writer</param>
        /// <param name="args">Arguments</param>
        public ProcessorConfiguration(IReadOnlyList<(bool, string, string)> inputs, string outputRootDirectory,
            int parallel, bool preload, bool debug, bool nop, ILogger logger, IReadOnlyList<string> args)
        {
            Inputs = inputs;
            OutputRootDirectory = outputRootDirectory;
            Parallel = parallel;
            Preload = preload;
            Debug = debug;
            Nop = nop;
            Logger = logger;
            Args = args;
        }

        /// <summary>
        /// Input sources
        /// </summary>
        public IReadOnlyList<(bool, string, string)> Inputs { get; init; }

        /// <summary>
        /// Output source
        /// </summary>
        public string OutputRootDirectory { get; init; }

        /// <summary>
        /// Thread count
        /// </summary>
        public int Parallel { get; init; }

        /// <summary>
        /// Whether to read all streams to memory
        /// </summary>
        public bool Preload { get; init; }

        /// <summary>
        /// Whether to enable <see cref="Processor.Debug"/>
        /// </summary>
        public bool Debug { get; init; }

        /// <summary>
        /// Whether to disable outputs
        /// </summary>
        public bool Nop { get; init; }

        /// <summary>
        /// Log writer
        /// </summary>
        public ILogger Logger { get; init; }

        /// <summary>
        /// Arguments
        /// </summary>
        public IReadOnlyList<string> Args { get; init; }
    }
}

namespace System.Runtime.CompilerServices
{
    internal static class IsExternalInit
    {
    }
}
