using System;
using System.Diagnostics.CodeAnalysis;

namespace Fp {
    /// <summary>
    /// Processor information attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    public class ProcessorInfoAttribute : Attribute {
        /// <summary>
        /// Processor name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Processor description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Processor extended description
        /// </summary>
        public string ExtendedDescription { get; set; }

        /// <summary>
        /// Processor extensions
        /// </summary>
        public string[] Extensions { get; set; }

        /// <summary>
        /// Processor information attribute
        /// </summary>
        /// <param name="name">Processor name</param>
        /// <param name="description">Processor description</param>
        /// <param name="extendedDescription">Processor extended description</param>
        /// <param name="extensions">Processor extensions</param>
        public ProcessorInfoAttribute(string name, string description, string extendedDescription, params string[] extensions) {
            Name = name;
            Description = description;
            ExtendedDescription = extendedDescription;
            Extensions = extensions;
        }
    }
}