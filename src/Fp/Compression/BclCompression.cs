using System;
using System.IO;

// ReSharper disable once CheckNamespace
namespace Fp
{
    public partial class FpUtil
    {
        /// <summary>
        /// Decompress deflate data.
        /// </summary>
        /// <param name="buffer">Buffer to read.</param>
        /// <returns>Decompressed data.</returns>
        public static byte[] DeDeflate(this ReadOnlyMemory<byte> buffer) => Processor.Dump(
            new System.IO.Compression.DeflateStream(new MStream(buffer),
                System.IO.Compression.CompressionMode.Decompress));

        /// <summary>
        /// Decompress deflate data.
        /// </summary>
        /// <param name="buffer">Buffer to read.</param>
        /// <returns>Decompressed data.</returns>
        public static byte[] DeDeflate(this Memory<byte> buffer) => Processor.Dump(
            new System.IO.Compression.DeflateStream(new MStream(buffer),
                System.IO.Compression.CompressionMode.Decompress));

        /// <summary>
        /// Decompress deflate data.
        /// </summary>
        /// <param name="buffer">Buffer to read.</param>
        /// <returns>Decompressed data.</returns>
        public static byte[] DeDeflate(this byte[] buffer) => Processor.Dump(
            new System.IO.Compression.DeflateStream(new MStream(buffer),
                System.IO.Compression.CompressionMode.Decompress));

        /// <summary>
        /// Decompress deflate data.
        /// </summary>
        /// <param name="stream">Stream to read.</param>
        /// <returns>Decompressed data.</returns>
        public static byte[] DeDeflate(this Stream stream) => Processor.Dump(
            new System.IO.Compression.DeflateStream(stream, System.IO.Compression.CompressionMode.Decompress, true));

        /// <summary>
        /// Decompress gzipped data.
        /// </summary>
        /// <param name="buffer">Buffer to read.</param>
        /// <returns>Decompressed data.</returns>
        public static byte[] DeGzip(this ReadOnlyMemory<byte> buffer) => Processor.Dump(
            new System.IO.Compression.GZipStream(new MStream(buffer),
                System.IO.Compression.CompressionMode.Decompress));

        /// <summary>
        /// Decompress gzipped data.
        /// </summary>
        /// <param name="buffer">Buffer to read.</param>
        /// <returns>Decompressed data.</returns>
        public static byte[] DeGzip(this Memory<byte> buffer) => Processor.Dump(
            new System.IO.Compression.GZipStream(new MStream(buffer),
                System.IO.Compression.CompressionMode.Decompress));

        /// <summary>
        /// Decompress gzipped data.
        /// </summary>
        /// <param name="buffer">Buffer to read.</param>
        /// <returns>Decompressed data.</returns>
        public static byte[] DeGzip(this byte[] buffer) => Processor.Dump(
            new System.IO.Compression.GZipStream(new MStream(buffer),
                System.IO.Compression.CompressionMode.Decompress));

        /// <summary>
        /// Decompress gzipped data.
        /// </summary>
        /// <param name="stream">Stream to read.</param>
        /// <returns>Decompressed data.</returns>
        public static byte[] DeGzip(this Stream stream) => Processor.Dump(
            new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress, true));
    }
}
