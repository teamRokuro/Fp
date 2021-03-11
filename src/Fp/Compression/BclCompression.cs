using System;
using System.IO;

// ReSharper disable once CheckNamespace
namespace Fp
{
    public partial class Scripting
    {
        /// <summary>
        /// Decompress deflate data.
        /// </summary>
        /// <param name="buffer">Buffer to read.</param>
        /// <returns>Decompressed data.</returns>
        public static byte[] deDeflate(this ReadOnlyMemory<byte> buffer) => Processor.Instance.DumpArray(
            new System.IO.Compression.DeflateStream(new MStream(buffer),
                System.IO.Compression.CompressionMode.Decompress));

        /// <summary>
        /// Decompress deflate data.
        /// </summary>
        /// <param name="buffer">Buffer to read.</param>
        /// <returns>Decompressed data.</returns>
        public static byte[] deDeflate(this Memory<byte> buffer) => Processor.Instance.DumpArray(
            new System.IO.Compression.DeflateStream(new MStream(buffer),
                System.IO.Compression.CompressionMode.Decompress));

        /// <summary>
        /// Decompress deflate data.
        /// </summary>
        /// <param name="buffer">Buffer to read.</param>
        /// <returns>Decompressed data.</returns>
        public static byte[] deDeflate(this byte[] buffer) => Processor.Instance.DumpArray(
            new System.IO.Compression.DeflateStream(new MStream(buffer),
                System.IO.Compression.CompressionMode.Decompress));

        /// <summary>
        /// Decompress deflate data.
        /// </summary>
        /// <param name="stream">Stream to read.</param>
        /// <returns>Decompressed data.</returns>
        public static byte[] deDeflate(this Stream stream) => Processor.Instance.DumpArray(
            new System.IO.Compression.DeflateStream(stream, System.IO.Compression.CompressionMode.Decompress, true));
        /// <summary>
        /// Decompress gzipped data.
        /// </summary>
        /// <param name="buffer">Buffer to read.</param>
        /// <returns>Decompressed data.</returns>
        public static byte[] deGzip(this ReadOnlyMemory<byte> buffer) => Processor.Instance.DumpArray(
            new System.IO.Compression.GZipStream(new MStream(buffer),
                System.IO.Compression.CompressionMode.Decompress));

        /// <summary>
        /// Decompress gzipped data.
        /// </summary>
        /// <param name="buffer">Buffer to read.</param>
        /// <returns>Decompressed data.</returns>
        public static byte[] deGzip(this Memory<byte> buffer) => Processor.Instance.DumpArray(
            new System.IO.Compression.GZipStream(new MStream(buffer),
                System.IO.Compression.CompressionMode.Decompress));

        /// <summary>
        /// Decompress gzipped data.
        /// </summary>
        /// <param name="buffer">Buffer to read.</param>
        /// <returns>Decompressed data.</returns>
        public static byte[] deGzip(this byte[] buffer) => Processor.Instance.DumpArray(
            new System.IO.Compression.GZipStream(new MStream(buffer),
                System.IO.Compression.CompressionMode.Decompress));

        /// <summary>
        /// Decompress gzipped data.
        /// </summary>
        /// <param name="stream">Stream to read.</param>
        /// <returns>Decompressed data.</returns>
        public static byte[] deGzip(this Stream stream) => Processor.Instance.DumpArray(
            new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress, true));
    }
}
