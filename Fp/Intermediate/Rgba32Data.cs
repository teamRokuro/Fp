using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Fp.Images.Png;

namespace Fp.Intermediate
{
    /// <summary>
    /// 32-bit RGBA data
    /// </summary>
    public class Rgba32Data : Data
    {
        /// <inheritdoc />
        public override CommonFormat DefaultFormat => CommonFormat.PngDeflate;

        /// <summary>
        /// RGBA data
        /// </summary>
        public uint[] Data;

        /// <summary>
        /// Image width
        /// </summary>
        public int Width;

        /// <summary>
        /// Image height
        /// </summary>
        public int Height;

        /// <summary>
        /// Create new instance of <see cref="Rgba32Data"/>
        /// </summary>
        /// <param name="baseName">Base name of resource</param>
        /// <param name="data">RGBA data</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        public Rgba32Data(string baseName, uint[] data, int width, int height) : base(baseName)
        {
            Data = data;
            Width = width;
            Height = height;
        }

        /// <inheritdoc />
        public override bool WriteConvertedData(Stream outputStream, CommonFormat format,
            Dictionary<string, string>? formatOptions = null)
        {
            switch (format)
            {
                case CommonFormat.PngDeflate:
                    PngBuilder.WritePngRgba(outputStream, Data, Width, Height, CompressionLevel.Optimal);
                    return true;
                case CommonFormat.Generic:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException(nameof(format), format, null);
            }
        }
    }
}
