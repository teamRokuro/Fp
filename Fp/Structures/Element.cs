using System.Collections.Generic;
using System.Linq;

namespace Fp.Structures
{
    /// <summary>
    /// Represents an element of a structure.
    /// </summary>
    public abstract record Element
    {
        /// <summary>
        /// Get available dependencies (including self).
        /// </summary>
        /// <returns>Dependencies.</returns>
        public IEnumerable<Element> GetDependencies() =>
            new[] {this}.Concat(Dependencies.SelectMany(d => d.GetDependencies()));

        /// <summary>
        /// Direct child dependencies.
        /// </summary>
        public virtual IEnumerable<Element> Dependencies => Enumerable.Empty<Element>();
    }
}
