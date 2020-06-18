using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using Fp.Images.Png;

namespace Fp.Images.Png
{
    /// <summary>
    /// Used to construct PNG images. Call <see cref="Create"/> to make a new builder.
    /// </summary>
    public class PngBuilder
    {
        /// <summary>
        /// IDAT chunk bytes
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static readonly byte[] ChunkIDAT = Encoding.ASCII.GetBytes("IDAT");

        /// <summary>
        /// IEND chunk bytes
        /// </summary>
        // ReSharper disable once InconsistentNaming
        public static readonly byte[] ChunkIEND = Encoding.ASCII.GetBytes("IEND");

        private const byte Deflate32KbWindow = 120;
        private const byte ChecksumBits = 1;

        private readonly byte[] _rawData;
        private readonly bool _hasAlphaChannel;
        private readonly int _width;
        private readonly int _height;
        private readonly int _bytesPerPixel;

        /// <summary>
        /// Create a builder for a PNG with the given width and size.
        /// </summary>
        public static PngBuilder Create(int width, int height, bool hasAlphaChannel)
        {
            int bpp = hasAlphaChannel ? 4 : 3;

            int length = height * width * bpp + height;

            return new PngBuilder(new byte[length], hasAlphaChannel, width, height, bpp);
        }

        private PngBuilder(byte[] rawData, bool hasAlphaChannel, int width, int height, int bytesPerPixel)
        {
            _rawData = rawData;
            _hasAlphaChannel = hasAlphaChannel;
            _width = width;
            _height = height;
            _bytesPerPixel = bytesPerPixel;
        }

        /// <summary>
        /// Sets the RGB pixel value for the given column (x) and row (y).
        /// </summary>
        public PngBuilder SetPixel(byte r, byte g, byte b, int x, int y) => SetPixel(new Pixel(r, g, b), x, y);

        /// <summary>
        /// Set the pixel value for the given column (x) and row (y).
        /// </summary>
        public PngBuilder SetPixel(Pixel pixel, int x, int y)
        {
            int start = y * (_width * _bytesPerPixel + 1) + 1 + x * _bytesPerPixel;

            _rawData[start++] = pixel.R;
            _rawData[start++] = pixel.G;
            _rawData[start++] = pixel.B;

            if (_hasAlphaChannel)
            {
                _rawData[start] = pixel.A;
            }

            return this;
        }

        /// <summary>
        /// Get the bytes of the PNG file for this builder.
        /// </summary>
        public byte[] Save()
        {
            using MemoryStream memoryStream = new MemoryStream();
            Save(memoryStream);
            return memoryStream.ToArray();
        }

        /// <summary>
        /// Write the PNG file bytes to the provided stream.
        /// </summary>
        public void Save(Stream outputStream)
        {
            outputStream.Write(HeaderValidationResult.ExpectedHeader, 0, HeaderValidationResult.ExpectedHeader.Length);

            PngStreamWriteHelper stream = new PngStreamWriteHelper(outputStream);

            stream.WriteChunkLength(13);
            stream.WriteChunkHeader(ImageHeader.HeaderBytes);

            StreamHelper.WriteBigEndianInt32(stream, _width);
            StreamHelper.WriteBigEndianInt32(stream, _height);
            stream.WriteByte(8);

            ColorType colorType = ColorType.ColorUsed;
            if (_hasAlphaChannel)
            {
                colorType |= ColorType.AlphaChannelUsed;
            }

            stream.WriteByte((byte)colorType);
            stream.WriteByte((byte)CompressionMethod.DeflateWithSlidingWindow);
            stream.WriteByte((byte)FilterMethod.AdaptiveFiltering);
            stream.WriteByte((byte)InterlaceMethod.None);

            stream.WriteCrc();

            byte[] imageData = Compress(_rawData);
            stream.WriteChunkLength(imageData.Length);
            stream.WriteChunkHeader(ChunkIDAT);
            stream.Write(imageData, 0, imageData.Length);
            stream.WriteCrc();

            stream.WriteChunkLength(0);
            stream.WriteChunkHeader(ChunkIEND);
            stream.WriteCrc();
        }

        internal static byte[] Compress(Span<byte> data)
        {
            const int headerLength = 2;
            const int checksumLength = 4;
            using MemoryStream compressStream = new MemoryStream(data.Length);
            using DeflateStream compressor = new DeflateStream(compressStream, CompressionLevel.Fastest, true);
            Processor.WriteBaseSpan(compressor, data);
            compressor.Close();

            byte[] result = new byte[headerLength + compressStream.Length + checksumLength];

            // Write the ZLib header.
            result[0] = Deflate32KbWindow;
            result[1] = ChecksumBits;

            // Write the compressed data.
            compressStream.GetBuffer().AsSpan(0, (int)compressStream.Length).CopyTo(result.AsSpan(headerLength));

            // Write Checksum of raw data.
            int checksum = Adler32Checksum.Calculate(data);

            long offset = headerLength + compressStream.Length;

            result[offset++] = (byte)(checksum >> 24);
            result[offset++] = (byte)(checksum >> 16);
            result[offset++] = (byte)(checksum >> 8);
            result[offset] = (byte)checksum;

            return result;
        }

        // Compress plain rgba
        internal static byte[] Compress2(ReadOnlySpan<byte> data, int width, int height, CompressionLevel compressionLevel)
        {
            const int headerLength = 2;
            const int checksumLength = 4;
            using MemoryStream compressStream = new MemoryStream(data.Length);
            using DeflateStream compressor = new DeflateStream(compressStream, compressionLevel, true);
            for (int y = 0; y < height; y++)
            {
                compressor.WriteByte(0);
                Processor.WriteBaseSpan(compressor, data.Slice(4 * width * y, 4 * width));
            }

            compressor.Close();

            byte[] result = new byte[headerLength + compressStream.Length + checksumLength];

            // Write the ZLib header.
            result[0] = Deflate32KbWindow;
            result[1] = ChecksumBits;

            // Write the compressed data.
            compressStream.GetBuffer().AsSpan(0, (int)compressStream.Length).CopyTo(result.AsSpan(headerLength));

            // Write Checksum of raw data.
            int checksum = Adler32Checksum.Calculate(data);

            long offset = headerLength + compressStream.Length;

            result[offset++] = (byte)(checksum >> 24);
            result[offset++] = (byte)(checksum >> 16);
            result[offset++] = (byte)(checksum >> 8);
            result[offset] = (byte)checksum;

            return result;
        }

        internal static void WritePngRgba32<T>(Stream outputStream, ReadOnlySpan<T> data, int width, int height,
            CompressionLevel compressionLevel) where T : unmanaged
        {
            outputStream.Write(HeaderValidationResult.ExpectedHeader, 0, HeaderValidationResult.ExpectedHeader.Length);

            PngStreamWriteHelper stream = new PngStreamWriteHelper(outputStream);

            stream.WriteChunkLength(13);
            stream.WriteChunkHeader(ImageHeader.HeaderBytes);

            StreamHelper.WriteBigEndianInt32(stream, width);
            StreamHelper.WriteBigEndianInt32(stream, height);
            stream.WriteByte(8);

            stream.WriteByte((byte)(ColorType.ColorUsed | ColorType.AlphaChannelUsed));
            stream.WriteByte((byte)CompressionMethod.DeflateWithSlidingWindow);
            stream.WriteByte((byte)FilterMethod.AdaptiveFiltering);
            stream.WriteByte((byte)InterlaceMethod.None);
            stream.WriteCrc();

            byte[] imageData = Compress2(MemoryMarshal.Cast<T, byte>(data), width, height, compressionLevel);
            stream.WriteChunkLength(imageData.Length);
            stream.WriteChunkHeader(ChunkIDAT);
            stream.Write(imageData, 0, imageData.Length);
            stream.WriteCrc();

            stream.WriteChunkLength(0);
            stream.WriteChunkHeader(ChunkIEND);
            stream.WriteCrc();
        }
    }
}

namespace Fp
{
    public partial class Processor
    {
        /// <summary>
        /// Write rgba data to stream
        /// </summary>
        /// <param name="data">Raw rgba color data</param>
        /// <param name="width">Width of image</param>
        /// <param name="height">Height of image</param>
        /// <param name="compressionLevel">Deflate compression level</param>
        /// <param name="outputStream">Stream to write to</param>
        public void WritePngRgba32<T>(ReadOnlySpan<T> data, int width, int height,
            CompressionLevel compressionLevel = CompressionLevel.Optimal, Stream? outputStream = null)
            where T : unmanaged
        {
            outputStream ??= OutputStream ?? throw new InvalidOperationException();
            PngBuilder.WritePngRgba32(outputStream, data, width, height, compressionLevel);
        }
    }
}
