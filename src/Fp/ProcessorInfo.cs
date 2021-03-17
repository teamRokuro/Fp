namespace Fp
{
    /// <summary>
    /// Processor information
    /// </summary>
    public record ProcessorInfo
    {
        /// <summary>
        /// Processor name
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// Processor description
        /// </summary>
        public string Description { get; init; }

        /// <summary>
        /// Processor extended description
        /// </summary>
        public string ExtendedDescription { get; init; }

        /// <summary>
        /// Processor extensions
        /// </summary>
        /// <remarks>
        /// If empty, processor should not filter by extensions. Null values mean files without a period.
        /// </remarks>
        public string?[] Extensions { get; init; }

        /// <summary>
        /// Create new instance of <see cref="ProcessorInfo"/> with generic values
        /// </summary>
        public ProcessorInfo()
        {
            Name = "unnamed processor";
            Description = "no description provided";
            ExtendedDescription = "no description provided";
            Extensions = new string[0];
        }

        /// <summary>
        /// Create new instance of <see cref="ProcessorInfo"/>
        /// </summary>
        /// <param name="name">Processor name</param>
        /// <param name="description">Processor description</param>
        /// <param name="extendedDescription">Processor extended description</param>
        /// <param name="extensions">Processor extensions</param>
        public ProcessorInfo(string name, string description, string extendedDescription,
            params string?[] extensions)
        {
            Name = name;
            Description = description;
            ExtendedDescription = extendedDescription;
            Extensions = extensions;
        }
    }
}
