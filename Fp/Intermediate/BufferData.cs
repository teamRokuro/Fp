using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Fp.Intermediate
{
    /// <summary>
    /// Buffer containing unstructured data
    /// </summary>
    public class BufferData<T> : Data where T : unmanaged
    {
        private bool _disposed = false;

        /// <inheritdoc />
        public override CommonFormat DefaultFormat => CommonFormat.Generic;

        private readonly IMemoryOwner<T>? _memoryOwner;

        /// <summary>
        /// Buffer
        /// </summary>
        public readonly Memory<T> Buffer;

        /// <summary>
        /// Buffer content length
        /// </summary>
        public readonly int ContentLength;

        /// <summary>
        /// Create new instance of <see cref="BufferData{T}"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="contentLength">Length of content</param>
        public BufferData(string basePath, int contentLength) : base(basePath)
        {
            Dry = true;
            _memoryOwner = null;
            Buffer = Memory<T>.Empty;
            ContentLength = contentLength;
        }

        /// <summary>
        /// Create new instance of <see cref="BufferData{T}"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="memoryOwner">Owner of data buffer</param>
        /// <param name="contentLength">Length of content</param>
        public BufferData(string basePath, IMemoryOwner<T> memoryOwner, int contentLength) : base(basePath)
        {
            _memoryOwner = memoryOwner;
            Buffer = _memoryOwner.Memory;
            ContentLength = contentLength;
        }

        /// <summary>
        /// Create new instance of <see cref="BufferData{T}"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="buffer">Data in container</param>
        public BufferData(string basePath, Memory<T> buffer) : base(basePath)
        {
            Buffer = buffer;
            _memoryOwner = null;
            ContentLength = buffer.Length;
        }

        /// <inheritdoc />
        public override bool WriteConvertedData(Stream outputStream, CommonFormat format,
            Dictionary<string, string>? formatOptions = null)
        {
            if (Dry) throw new InvalidOperationException("Cannot convert a dry data container");
            switch (format)
            {
                case CommonFormat.Generic:
                    Processor.WriteBaseSpan(outputStream,
                        MemoryMarshal.Cast<T, byte>(Buffer.Span.Slice(0, ContentLength)));
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Dispose object
        /// </summary>
        /// <param name="disposing">False if called from finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _memoryOwner?.Dispose();
            }
        }

        /// <inheritdoc />
        public override void Dispose() => Dispose(true);
    }
}
