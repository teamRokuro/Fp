using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Fp.Images.Png;

namespace Fp.Intermediate
{
    /// <summary>
    /// 32-bit RGBA data
    /// </summary>
    public class Rgba32Data : BufferData<uint>
    {
        /// <inheritdoc />
        public override CommonFormat DefaultFormat => CommonFormat.PngDeflate;

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
        public Rgba32Data(string basePath, int width, int height) : base(basePath, width * height)
        {
            Dry = true;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Create new instance of <see cref="Rgba32Data"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="memoryOwner">Owner of PCM data buffer</param>
        /// <param name="count">Length of content</param>
        public Rgba32Data(string basePath, int width, int height, IMemoryOwner<uint> memoryOwner,
            int? count = default) : base(basePath, memoryOwner, count)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Create new instance of <see cref="Rgba32Data"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="buffer">PCM data</param>
        public Rgba32Data(string basePath, int width, int height, Memory<uint> buffer) : base(basePath, buffer)
        {
            Width = width;
            Height = height;
        }

        /// <inheritdoc />
        public override bool WriteConvertedData(Stream outputStream, CommonFormat format,
            Dictionary<string, string>? formatOptions = null)
        {
            if (Dry) throw new InvalidOperationException("Cannot convert a dry data container");
            if (Buffer.IsEmpty)
                throw new ObjectDisposedException(nameof(Rgba32Data));
            switch (format)
            {
                case CommonFormat.PngDeflate:
                    PngBuilder.WritePngRgba32(outputStream, Buffer.Span, Width, Height,
                        CompressionLevel.Optimal);
                    return true;
                default:
                    return false;
            }
        }

        /// <inheritdoc />
        public override object Clone()
        {
            if (Dry)
                return new Rgba32Data(BasePath, Width, Height);
            if (Buffer.IsEmpty)
                throw new ObjectDisposedException(nameof(Rgba32Data));
            return new Rgba32Data(BasePath, Width, Height, IntermediateUtil.CloneBuffer(Buffer));
        }
    }
}
