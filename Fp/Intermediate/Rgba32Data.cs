using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using Fp.Images.Png;

namespace Fp.Intermediate
{
    /// <summary>
    /// 32-bit RGBA data
    /// </summary>
    public class Rgba32Data<T> : BufferData<T> where T : unmanaged
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
        /// Create new instance of <see cref="Rgba32Data{T}"/>
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
        /// Create new instance of <see cref="Rgba32Data{T}"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="memoryOwner">Owner of PCM data buffer</param>
        /// <param name="contentLength">Length of content</param>
        public Rgba32Data(string basePath, int width, int height, IMemoryOwner<T> memoryOwner, int contentLength) :
            base(basePath, memoryOwner, contentLength)
        {
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Create new instance of <see cref="Rgba32Data{T}"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="buffer">PCM data</param>
        public Rgba32Data(string basePath, int width, int height, Memory<T> buffer) : base(basePath, buffer)
        {
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
                    PngBuilder.WritePngRgba32(outputStream, MemoryMarshal.Cast<T, uint>(Buffer.Span), Width, Height,
                        CompressionLevel.Optimal);
                    return true;
                default:
                    return false;
            }
        }
    }
}
