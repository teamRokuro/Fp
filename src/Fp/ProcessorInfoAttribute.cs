using System;
using System.Diagnostics.CodeAnalysis;

namespace Fp
{
    /// <summary>
    /// Processor information attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class ProcessorInfoAttribute : Attribute
    {
        /// <summary>
        /// Processor information
        /// </summary>
        public ProcessorInfo Info;

        /// <summary>
        /// Processor information attribute
        /// </summary>
        /// <param name="name">Processor name</param>
        /// <param name="description">Processor description</param>
        /// <param name="extendedDescription">Processor extended description</param>
        /// <param name="extensions">Processor extensions</param>
        public ProcessorInfoAttribute(string name, string description, string extendedDescription,
            params string?[] extensions)
        {
            Info = new ProcessorInfo(name, description, extendedDescription, extensions);
        }
    }
}
