using System;
using System.Collections.Generic;
using System.IO;

namespace Fp.Intermediate
{
    /// <summary>
    /// Generic data, not to be converted
    /// </summary>
    public class GenericData : Data
    {
        /// <inheritdoc />
        public override CommonFormat DefaultFormat => CommonFormat.Generic;

        /// <summary>
        /// Data in container
        /// </summary>
        public readonly ArraySegment<byte>? Bytes;

        /// <summary>
        /// Length of data
        /// </summary>
        public readonly int? Length;

        /// <summary>
        /// Create new instance of <see cref="GenericData"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="length">Length of data</param>
        public GenericData(string basePath, int? length) : base(basePath)
        {
            Dry = true;
            Bytes = null;
            Length = length;
        }


        /// <summary>
        /// Create new instance of <see cref="GenericData"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="bytes">Data in container</param>
        public GenericData(string basePath, ArraySegment<byte> bytes) : base(basePath)
        {
            Bytes = bytes;
            Length = bytes.Count;
        }

        /// <inheritdoc />
        public override bool WriteConvertedData(Stream outputStream, CommonFormat format,
            Dictionary<string, string>? formatOptions = null)
        {
            if (Dry) throw new InvalidOperationException("Cannot convert a dry data container");
            switch (format)
            {
                case CommonFormat.Generic:
                    var data = Bytes!.Value;
                    outputStream.Write(data.Array, data.Offset, data.Count);
                    return true;
                default:
                    return false;
            }
        }

        /// <inheritdoc />
        public override Data IsolateClone()
        {
            return Bytes.HasValue
                ? new GenericData(BasePath, IntermediateUtil.CopySegment(Bytes.Value))
                : new GenericData(BasePath, Length);
        }
    }
}
