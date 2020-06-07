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
        public readonly ArraySegment<uint>? Raster;

        /// <summary>
        /// Image width
        /// </summary>
        public readonly int Width;

        /// <summary>
        /// Image height
        /// </summary>
        public readonly int Height;

        /// <summary>
        /// Create new instance of <see cref="Rgba32Data"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        public Rgba32Data(string basePath, int width, int height) : base(basePath)
        {
            Dry = true;
            Raster = null;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Create new instance of <see cref="Rgba32Data"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="raster">RGBA data, or null for dry</param>
        public Rgba32Data(string basePath, int width, int height, ArraySegment<uint>? raster = null) : base(basePath)
        {
            Dry = !raster.HasValue;
            Raster = raster;
            Width = width;
            Height = height;
        }

        /// <inheritdoc />
        public override bool WriteConvertedData(Stream outputStream, CommonFormat format,
            Dictionary<string, string>? formatOptions = null)
        {
            if (Dry) throw new InvalidOperationException("Cannot convert a dry data container");
            switch (format)
            {
                case CommonFormat.PngDeflate:
                    PngBuilder.WritePngRgba(outputStream, Raster!.Value, Width, Height, CompressionLevel.Optimal);
                    return true;
                default:
                    return false;
            }
        }

        /// <inheritdoc />
        public override Data GetCompact(bool requireNew = false)
        {
            if (!requireNew && Raster.HasValue && IntermediateUtil.IsCompactSegment(Raster.Value))
                return this;
            return Raster.HasValue
                ? new Rgba32Data(BasePath, Width, Height, IntermediateUtil.CopySegment(Raster.Value))
                : new Rgba32Data(BasePath, Width, Height);
        }
    }
}
