using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace Fp
{
    /// <summary>
    /// 32-bit RGBA data
    /// </summary>
    public class Rgba32Data : BufferData<uint>
    {
        private static readonly PngEncoder _pngEncoder =
            new() {CompressionLevel = PngCompressionLevel.BestCompression};

        /// <inheritdoc />
        public override CommonFormat DefaultFormat => CommonFormat.PngDeflate;

        /// <summary>
        /// Provides option keys for <see cref="Rgba32Data"/>
        /// </summary>
        public static class Options
        {
            /// <summary>
            /// Jpeg quality level (int 0-100)
            /// </summary>
            public const string JpegQuality = "JpegQuality";
        }

        /// <summary>
        /// Image width
        /// </summary>
        public readonly int Width;

        /// <summary>
        /// Image height
        /// </summary>
        public readonly int Height;

        private bool _disposed;

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
        public Rgba32Data(string basePath, int width, int height, ReadOnlyMemory<uint> buffer) : base(basePath, buffer)
        {
            Width = width;
            Height = height;
        }

        /// <inheritdoc />
        public override bool WriteConvertedData(Stream outputStream, CommonFormat format,
            Dictionary<object, object>? formatOptions = null)
        {
            if (Dry) throw new InvalidOperationException("Cannot convert a dry data container");
            if (_disposed)
                throw new ObjectDisposedException(nameof(Rgba32Data));
            switch (format)
            {
                case CommonFormat.PngDeflate:
                case CommonFormat.Jpeg:
                    Image<Rgba32> image = new(Width, Height);
                    if (image.TryGetSinglePixelSpan(out Span<Rgba32> span))
                        Buffer.Span.Slice(0, Width * Height).CopyTo(MemoryMarshal.Cast<Rgba32, uint>(span));
                    else
                        for (int y = 0; y < Height; y++)
                            Buffer.Span.Slice(Width * y, Width)
                                .CopyTo(MemoryMarshal.Cast<Rgba32, uint>(image.GetPixelRowSpan(y)));

                    if (format == CommonFormat.PngDeflate)
                        image.SaveAsPng(outputStream, _pngEncoder);
                    else
                    {
                        var jpegEncoder = new JpegEncoder();
                        if (formatOptions != null &&
                            formatOptions.TryGetValue(Options.JpegQuality, out object? jpegQuality))
                            jpegEncoder.Quality = Math.Min(100, Math.Max(0, CastNumber<object, int>(jpegQuality)));
                        image.SaveAsJpeg(outputStream, jpegEncoder);
                    }

                    return true;
                default:
                    return false;
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;
            base.Dispose(disposing);
        }

        /// <inheritdoc />
        public override object Clone()
        {
            if (Dry)
                return new Rgba32Data(BasePath, Width, Height);
            if (_disposed)
                throw new ObjectDisposedException(nameof(Rgba32Data));
            return new Rgba32Data(BasePath, Width, Height, DataUtil.CloneBuffer(Buffer));
        }

        /// <inheritdoc />
        public override string ToString() => $"RGBA32 {{ Path = {BasePath}, Width = {Width}, Height = {Height} }}";
    }

    public partial class Processor
    {
        /// <summary>
        /// Creates 32bpp RGBA image data object.
        /// </summary>
        /// <param name="path">Base path (without extension).</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="buffer">32bpp RGBA buffer.</param>
        /// <returns>Data object.</returns>
        public static Rgba32Data Image(FpPath path, int width, int height, ReadOnlyMemory<uint> buffer) =>
            new(path.AsCombined(), width, height, buffer);

        /// <summary>
        /// Creates 32bpp RGBA image data object.
        /// </summary>
        /// <param name="name">Base path (without extension).</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="buffer">32bpp RGBA buffer.</param>
        /// <returns>Data object.</returns>
        public static Rgba32Data Image(string name, int width, int height, ReadOnlyMemory<uint> buffer) =>
            new(name, width, height, buffer);
    }

    public partial class FpUtil
    {
        /// <summary>
        /// Convert 24bpp RGB data to 32bpp RGBA.
        /// </summary>
        /// <param name="data">Source.</param>
        /// <param name="target">Target.</param>
        /// <param name="a">Alpha value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FromRgb(this ReadOnlySpan<byte> data, Span<byte> target, byte a = 255)
        {
            data.Slice(0, 3).CopyTo(target);
            target[3] = a;
        }

        /// <summary>
        /// Convert 24bpp RGB data to 32bpp RGBA.
        /// </summary>
        /// <param name="data">Source.</param>
        /// <param name="a">Alpha value.</param>
        /// <returns>32-bit value with color.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe uint FromRgb(this ReadOnlySpan<byte> data, byte a = 255)
        {
            uint result;
            byte* target = (byte*)&result;
            target[0] = data[0];
            target[1] = data[1];
            target[2] = data[2];
            target[3] = a;
            return result;
        }

        /// <summary>
        /// Convert 32bpp BGRA data to 32bpp RGBA.
        /// </summary>
        /// <param name="data">Source.</param>
        /// <param name="target">Target.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FromBgra(this ReadOnlySpan<byte> data, Span<byte> target)
        {
            target[2] = data[0];
            target[1] = data[1];
            target[0] = data[2];
            target[3] = data[3];
        }

        /// <summary>
        /// Convert 32bpp BGRA data to 32bpp RGBA.
        /// </summary>
        /// <param name="data">Source.</param>
        /// <returns>32-bit value with color.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe uint FromBgra(this ReadOnlySpan<byte> data)
        {
            uint result;
            byte* target = (byte*)&result;
            target[2] = data[0];
            target[1] = data[1];
            target[0] = data[2];
            target[3] = data[3];
            return result;
        }
    }

    public partial class Scripting
    {
        /// <summary>
        /// Creates 32bpp RGBA image data object.
        /// </summary>
        /// <param name="path">Base path (without extension).</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="buffer">32bpp RGBA buffer.</param>
        /// <returns>Data object.</returns>
        public static Data image(this FpPath path, int width, int height, ReadOnlyMemory<uint> buffer) =>
            Processor.Image(path, width, height, buffer);

        /// <summary>
        /// Creates 32bpp RGBA image data object.
        /// </summary>
        /// <param name="path">Base path (without extension).</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="buffer">32bpp RGBA buffer.</param>
        /// <returns>Data object.</returns>
        public static Data image(this string path, int width, int height, ReadOnlyMemory<uint> buffer) =>
            Processor.Image(path, width, height, buffer);
    }
}
