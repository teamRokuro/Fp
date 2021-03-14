using System;
using System.IO;

namespace Fp.Helpers
{
    /// <summary>
    /// Base unmanaged array data helper.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    public abstract unsafe record BaseUnmanagedArrayHelper<T> : Helper where T : unmanaged
    {
        /// <summary>
        /// Element size in memory (contiguous read)
        /// </summary>
        public virtual int ElementSize => sizeof(T);

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="count">Element count.</param>
        public virtual T[] this[byte[] source, int offset, int count] => this[source.AsSpan(), offset, count];

        /// <summary>
        /// Write data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        public virtual T[] this[byte[] source, int offset]
        {
            set => this[source.AsSpan(), offset] = value;
        }

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="v1">1st arg.</param>
        public virtual T[] this[Memory<byte> source, int offset, int v1] => this[source.Span, offset, v1];

        /// <summary>
        /// Write data.
        /// </summary>
        /// <param name="offset">Offset.</param>
        /// <param name="source">Data source.</param>
        public virtual T[] this[Memory<byte> source, int offset]
        {
            set => this[source.Span, offset] = value;
        }

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="v1">1st arg.</param>
        public virtual T[] this[Span<byte> source, int offset, int v1] => this[(ReadOnlySpan<byte>)source, offset, v1];

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="v1">1st arg.</param>
        public virtual T[] this[ReadOnlyMemory<byte> source, int offset, int v1] => this[source.Span, offset, v1];

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="source">Data source.</param>
        public abstract T[] this[ReadOnlySpan<byte> source] { get; }

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="offset">Offset (no seeking if -1).</param>
        /// <param name="v1">1st arg.</param>
        public virtual T[] this[long offset, int v1] => this[offset, v1, InputStream];

        /// <summary>
        /// Write data.
        /// </summary>
        /// <param name="offset">Offset (no seeking if -1).</param>
        public virtual T[] this[long offset]
        {
            set => this[offset, OutputStream] = value;
        }

        /// <summary>
        /// Read / write data.
        /// </summary>
        /// <param name="source">Data source.</param>
        public virtual T[] this[byte[] source]
        {
            get => this[source.AsSpan()];
            set => this[source.AsSpan()] = value;
        }

        /// <summary>
        /// Read / write data.
        /// </summary>
        /// <param name="source">Data source.</param>
        public virtual T[] this[Memory<byte> source]
        {
            get => this[source.Span];
            set => this[source.Span] = value;
        }

        /// <summary>
        /// Write data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        public virtual T[] this[Span<byte> source, int offset]
        {
            set => this[source.Slice(offset)] = value;
        }

        /// <summary>
        /// Read / write data.
        /// </summary>
        /// <param name="source">Data source.</param>
        public abstract T[] this[Span<byte> source] { get; set; }

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="count">Element count.</param>
        public virtual T[] this[ReadOnlySpan<byte> source, int offset, int count] =>
            this[source.Slice(offset, count * ElementSize)];

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="offset">Offset (no seeking if -1).</param>
        /// <param name="count">Element count.</param>
        /// <param name="stream">Data source.</param>
        public virtual T[] this[long offset, int count, Stream stream]
        {
            get
            {
                byte[] arr = new byte[count * ElementSize];
                if (offset != -1) Processor.Read(stream, offset, arr, 0, arr.Length, false);
                else Processor.Read(stream, arr, 0, arr.Length, false);
                return this[arr, 0, count];
            }
        }

        /// <summary>
        /// Write data.
        /// </summary>
        /// <param name="offset">Offset (no seeking if -1).</param>
        /// <param name="stream">Data source.</param>
        public virtual T[] this[long offset, Stream stream]
        {
            set
            {
                byte[] arr = new byte[value.Length * ElementSize];
                this[arr, 0] = value;
                if (offset != -1) Processor.Write(stream, offset, arr);
                else Processor.Write(stream, arr);
            }
        }
    }
}
