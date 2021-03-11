using System;

namespace Fp
{
    /// <summary>
    /// Processor factory
    /// </summary>
    public abstract record ProcessorFactory
    {
        /// <summary>
        /// Processor info
        /// </summary>
        public ProcessorInfo Info { get; init; }

        /// <summary>
        /// Create new instance of <see cref="ProcessorFactory"/>
        /// </summary>
        /// <param name="info">Processor info</param>
        public ProcessorFactory(ProcessorInfo? info) => Info = info ?? new ProcessorInfo();

        /// <summary>
        /// Creates new processor instance from this factory
        /// </summary>
        /// <returns>Instantiated processor</returns>
        public abstract Processor CreateProcessor();
    }

    /// <summary>
    /// Processor descriptor
    /// </summary>
    /// <typeparam name="T">Processor type</typeparam>
    public record GenericNewProcessorFactory<T> : ProcessorFactory where T : Processor, new()
    {
        /// <summary>
        /// Create new instance of <see cref="GenericNewProcessorFactory{T}"/>
        /// </summary>
        /// <param name="info">Processor info</param>
        public GenericNewProcessorFactory(ProcessorInfo? info) : base(info)
        {
        }

        /// <inheritdoc />
        public override Processor CreateProcessor() => new T();
    }

    /// <summary>
    /// Processor descriptor
    /// </summary>
    public record DelegateProcessorFactory : ProcessorFactory
    {
        /// <summary>
        /// Source delegate
        /// </summary>
        public Func<Processor> Delegate { get; init; }

        /// <summary>
        /// Create new instance of <see cref="DelegateProcessorFactory"/>
        /// </summary>
        /// <param name="info">Processor info</param>
        /// <param name="del">Source delegate</param>
        public DelegateProcessorFactory(ProcessorInfo? info, Func<Processor> del) : base(info) => Delegate = del;

        /// <inheritdoc />
        public override Processor CreateProcessor() => Delegate();
    }
}
