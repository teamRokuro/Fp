using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Fp
{
    /// <summary>
    /// Buffer containing unstructured data
    /// </summary>
    public abstract class BufferData : Data
    {
        /// <inheritdoc />
        protected BufferData(string basePath) : base(basePath)
        {
        }

        /// <summary>
        /// Get span of specified type from buffer
        /// </summary>
        /// <typeparam name="TWant">Target type</typeparam>
        /// <returns>Span</returns>
        /// <exception cref="ObjectDisposedException">If object was disposed</exception>
        public abstract ReadOnlySpan<TWant> AsSpan<TWant>() where TWant : unmanaged;
    }

    /// <inheritdoc />
    public class BufferData<T> : BufferData where T : unmanaged
    {
        private bool _disposed;

        /// <inheritdoc />
        public override CommonFormat DefaultFormat => CommonFormat.Generic;

        /// <summary>
        /// Memory owner
        /// </summary>
        public IMemoryOwner<T>? MemoryOwner { get; }

        /// <summary>
        /// Buffer
        /// </summary>
        public ReadOnlyMemory<T> Buffer { get; private set; }

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
            MemoryOwner = null;
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
            MemoryOwner = memoryOwner;
            Dry = false;
            Count = count ?? MemoryOwner.Memory.Length;
            Buffer = MemoryOwner.Memory.Slice(0, Count);
        }

        /// <summary>
        /// Create new instance of <see cref="BufferData{T}"/>
        /// </summary>
        /// <param name="basePath">Base path of resource</param>
        /// <param name="buffer">Data in container</param>
        public BufferData(string basePath, ReadOnlyMemory<T> buffer) : base(basePath)
        {
            Buffer = buffer;
            Dry = false;
            MemoryOwner = null;
            Count = buffer.Length;
        }

        /// <inheritdoc />
        public override ReadOnlySpan<TWant> AsSpan<TWant>()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(BufferData<T>));
            return MemoryMarshal.Cast<T, TWant>(Buffer.Span);
        }

        /// <inheritdoc />
        public override bool WriteConvertedData(Stream outputStream, CommonFormat format,
            Dictionary<object, object>? formatOptions = null)
        {
            if (Dry) throw new InvalidOperationException("Cannot convert a dry data container");
            if (_disposed)
                throw new ObjectDisposedException(nameof(BufferData<T>));
            switch (format)
            {
                case CommonFormat.Generic:
                    Processor.WriteBaseSpan(outputStream,
                        MemoryMarshal.Cast<T, byte>(Buffer.Span));
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
            if (_disposed)
                throw new ObjectDisposedException(nameof(BufferData<T>));
            return new BufferData<T>(BasePath, DataUtil.CloneBuffer(Buffer));
        }

        /// <summary>
        /// Dispose object
        /// </summary>
        /// <param name="disposing">False if called from finalizer</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                Buffer = Memory<T>.Empty;
                MemoryOwner?.Dispose();
            }
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc />
        public override unsafe string ToString() =>
            $"Buffer {{ Element Size = {sizeof(T)}, Element Count = {Count}, Buffer Length = {sizeof(T) * Count} }}";
    }

    public partial class Processor
    {
        /// <summary>
        /// Creates byte data object.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="memory">Data.</param>
        /// <returns>Data object.</returns>
        public static BufferData<byte> Buffer(FpPath path, ReadOnlyMemory<byte> memory) =>
            new(path.AsCombined(), memory);

        /// <summary>
        /// Creates byte data object.
        /// </summary>
        /// <param name="name">Path.</param>
        /// <param name="memory">Data.</param>
        /// <returns>Data object.</returns>
        public static BufferData<byte> Buffer(string name, ReadOnlyMemory<byte> memory) =>
            new(name, memory);
    }

    public partial class Scripting
    {
        /// <summary>
        /// Creates byte data object.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="memory">Data.</param>
        /// <returns>Data object.</returns>
        public static Data data(this FpPath path, ReadOnlyMemory<byte> memory) => Processor.Buffer(path, memory);

        /// <summary>
        /// Creates byte data object.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="memory">Data.</param>
        /// <returns>Data object.</returns>
        public static Data data(this string path, ReadOnlyMemory<byte> memory) => Processor.Buffer(path, memory);
    }
}
