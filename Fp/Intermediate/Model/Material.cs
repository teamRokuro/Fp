using System;
using System.Collections.Generic;

namespace Fp.Intermediate.Model
{
    /// <summary>
    /// Material information
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly",
        Justification = "<Pending>")]
    public sealed class Material : ICloneable
    {
        /// <summary>
        /// Material texture properties
        /// </summary>
        public Dictionary<string, string> Textures { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Material miscellaneous properties
        /// </summary>
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

        /// <inheritdoc />
        public object Clone()
        {
            var res = new Material();
            foreach (var e in Textures)
                res.Textures.Add(e.Key, e.Value);
            foreach (var e in Properties)
                res.Properties.Add(e.Key, e.Value);
            return res;
        }
    }
}
