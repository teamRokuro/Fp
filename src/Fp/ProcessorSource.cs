using System;
using System.Collections.Generic;
using System.Linq;

namespace Fp
{
    /// <summary>
    /// Processor factory
    /// </summary>
    public class ProcessorSource
    {
        /// <summary>
        /// Available factories
        /// </summary>
        public readonly HashSet<ProcessorFactory> Factories = new();

        /// <summary>
        /// Get available processors based on info
        /// </summary>
        /// <param name="predicate">Predicate on on info</param>
        /// <returns>Processors</returns>
        public IEnumerable<Processor> GetProcessors(Predicate<ProcessorInfo> predicate) =>
            Factories.Where(f => predicate(f.Info)).Select(f => f.CreateProcessor());

        /// <summary>
        /// Get available processors based on factory
        /// </summary>
        /// <param name="predicate">Predicate on factory</param>
        /// <typeparam name="T">Factory type</typeparam>
        /// <returns>Processors</returns>
        public IEnumerable<Processor> GetProcessors<T>(Predicate<T> predicate)
            where T : ProcessorFactory =>
            Factories.Where(f => f is T t && predicate(t)).Select(f => f.CreateProcessor());
    }
}
