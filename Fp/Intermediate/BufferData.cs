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
    public abstract class BufferData : Data
    {
        /// <inheritdoc />
        protected BufferData(string basePath) : base(basePath) {
        }

        /// <summary>
        /// Get span of specified type from buffer
        /// </summary>
        /// <typeparam name="TWant">Target type</typeparam>
        /// <returns>Span</returns>
        /// <exception cref="ObjectDisposedException">If object was disposed</exception>
        public abstract Span<TWant> AsSpan<TWant>() where TWant : unmanaged;
    }

    /// <inheritdoc />
    public class BufferData<T> : BufferData where T : unmanaged
    {
        private bool _disposed = false;

        /// <inheritdoc />
        public override CommonFormat DefaultFormat => CommonFormat.Generic;

        private readonly IMemoryOwner<T>? _memoryOwner;

        /// <summary>
        /// Buffer
        /// </summary>
        public Memory<T> Buffer { get; private set; }

        /// <summary>
        /// Buffer content length
        /// </summary>
        public readonly int Count;

        /// <summary>
        /// Create new instance of <see cref="BufferData{T}"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="count">Length of content</param>
        public BufferData(string basePath, int count) : base(basePath)
        {
            Dry = true;
            _memoryOwner = null;
            Buffer = Memory<T>.Empty;
            Count = count;
        }

        /// <summary>
        /// Create new instance of <see cref="BufferData{T}"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="memoryOwner">Owner of data buffer</param>
        /// <param name="count">Length of content</param>
        public BufferData(string basePath, IMemoryOwner<T> memoryOwner, int? count = default) : base(basePath)
        {
            _memoryOwner = memoryOwner;
            Buffer = _memoryOwner.Memory;
            Dry = Buffer.IsEmpty;
            Count = count ?? Buffer.Length;
        }

        /// <summary>
        /// Create new instance of <see cref="BufferData{T}"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="buffer">Data in container</param>
        public BufferData(string basePath, ReadOnlyMemory<T> buffer) : base(basePath)
        {
            Buffer = buffer;
            Dry = Buffer.IsEmpty;
            _memoryOwner = null;
            Count = buffer.Length;
        }

        /// <inheritdoc />
        public override Span<TWant> AsSpan<TWant>()
        {
            if (Buffer.IsEmpty)
                throw new ObjectDisposedException(nameof(BufferData<T>));
            return MemoryMarshal.Cast<T, TWant>(Buffer.Span);
        }

        /// <inheritdoc />
        public override bool WriteConvertedData(Stream outputStream, CommonFormat format,
            Dictionary<string, string>? formatOptions = null)
        {
            if (Dry) throw new InvalidOperationException("Cannot convert a dry data container");
            if (Buffer.IsEmpty)
                throw new ObjectDisposedException(nameof(BufferData<T>));
            switch (format)
            {
                case CommonFormat.Generic:
                    Processor.WriteBaseSpan(outputStream,
                        MemoryMarshal.Cast<T, byte>(Buffer.Span.Slice(0, Count)));
                    return true;
                default:
                    return false;
            }
        }

        /// <inheritdoc />
        public override object Clone()
        {
            if (Dry)
                return new BufferData<T>(BasePath, Count);
            if (Buffer.IsEmpty)
                throw new ObjectDisposedException(nameof(BufferData<T>));
            return new BufferData<T>(BasePath, IntermediateUtil.CloneBuffer(Buffer));
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
                Buffer = Memory<T>.Empty;
                _memoryOwner?.Dispose();
            }
        }

        /// <inheritdoc />
        public override void Dispose() => Dispose(true);
    }
}
