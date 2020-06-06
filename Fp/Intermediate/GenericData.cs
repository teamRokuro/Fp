using System;
using System.Collections.Generic;
using System.IO;

namespace Fp.Intermediate {
    /// <summary>
    /// Generic data, not to be converted
    /// </summary>
    public class GenericData : Data {
        /// <inheritdoc />
        public override CommonFormat DefaultFormat => CommonFormat.Generic;

        /// <summary>
        /// Data in container
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// Offset in array
        /// </summary>
        public int Offset;
        /// <summary>
        /// Length of data
        /// </summary>
        public int Length;

        /// <summary>
        /// Create new instance of <see cref="GenericData"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="data">Data in container</param>
        public GenericData(string basePath, byte[] data) : base(basePath) {
            Data = data;
            Offset = 0;
            Length = data.Length;
        }

        /// <summary>
        /// Create new instance of <see cref="GenericData"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="data">Data in container</param>
        /// <param name="offset">Offset in array</param>
        /// <param name="length">Length of data</param>
        public GenericData(string basePath, byte[] data, int offset, int length) : base(basePath) {
            Data = data;
            Offset = offset;
            Length = length;
        }

        /// <inheritdoc />
        public override bool WriteConvertedData(Stream outputStream, CommonFormat format, Dictionary<string, string>? formatOptions = null) {
            switch (format) {
                case CommonFormat.Generic:
                    outputStream.Write(Data, 0, Data.Length);
                    return true;
                case CommonFormat.PngDeflate:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }
    }
}