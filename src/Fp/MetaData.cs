using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Fp
{
    /// <summary>
    /// Represents general metadata
    /// </summary>
    public class MetaData : Data
    {
        /// <summary>
        /// Metadata value
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Create instance of <see cref="MetaData"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="value">Metadata value</param>
        public MetaData(string basePath, object value) : base(basePath)
        {
            Value = value;
        }

        /// <inheritdoc />
        public override CommonFormat DefaultFormat => CommonFormat.Generic;

        /// <inheritdoc />
        public override bool WriteConvertedData(Stream outputStream, CommonFormat format,
            Dictionary<object, object>? formatOptions = null)
        {
            switch (format)
            {
                case CommonFormat.Generic:
                {
                    using var tw = new StreamWriter(outputStream, Encoding.UTF8, 4096, true);
                    tw.Write(Value);
                    return true;
                }
                default:
                    return false;
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
        }

        /// <inheritdoc />
        public override object Clone() => new MetaData(BasePath, Value);

        /// <inheritdoc />
        public override string ToString() => Value.ToString();
    }

    public partial class Processor
    {
        /// <summary>
        /// Creates metadata object.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="value">Value.</param>
        /// <returns>Data object.</returns>
        public static MetaData Meta(FpPath path, object value) => new(path.AsCombined(), value);

        /// <summary>
        /// Creates metadata object.
        /// </summary>
        /// <param name="name">Path.</param>
        /// <param name="value">Value.</param>
        /// <returns>Data object.</returns>
        public static MetaData Meta(string name, object value) => new(name, value);
    }

    public partial class Scripting
    {
        /// <summary>
        /// Creates metadata object.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="value">Value.</param>
        /// <returns>Data object.</returns>
        public static Data meta(this FpPath path, object value) => Processor.Meta(path, value);

        /// <summary>
        /// Creates metadata object.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="value">Value.</param>
        /// <returns>Data object.</returns>
        public static Data meta(this string path, object value) => Processor.Meta(path, value);
    }
}
