using System.Collections.Generic;
using System.IO;

namespace Fp.Intermediate
{
    /// <summary>
    /// Intermediate-format data container
    /// </summary>
    public abstract class Data
    {
        /// <summary>
        /// Base path of resource
        /// </summary>
        public readonly string BasePath;

        /// <summary>
        /// Default format for container
        /// </summary>
        public abstract CommonFormat DefaultFormat { get; }

        /// <summary>
        /// If true, object does not contain complete data, e.g. for <see cref="WriteConvertedData"/>
        /// </summary>
        public bool Dry { get; protected set; }

        /// <summary>
        /// Create instance of <see cref="Data"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        protected Data(string basePath)
        {
            BasePath = basePath;
        }

        /// <summary>
        /// Get stream of data converted to common file format
        /// </summary>
        /// <param name="outputStream">Target stream</param>
        /// <param name="format">Requested file format</param>
        /// <param name="formatOptions">Format-specific options</param>
        /// <returns>False if requested format is not supported</returns>
        public abstract bool WriteConvertedData(Stream outputStream, CommonFormat format,
            Dictionary<string, string>? formatOptions = null);

        /// <summary>
        /// Get compact data
        /// </summary>
        /// <param name="requireNew">Require new object even if already compact</param>
        /// <returns>Cloned data</returns>
        public abstract Data GetCompact(bool requireNew = false);
    }
}
