using System;
using System.IO;

namespace Fp.Helpers
{
    /// <summary>
    /// Base unmanaged array data helper.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    public abstract unsafe record BaseUnmanagedArrayHelper<T> : BaseHelper<T[], T[], int> where T : unmanaged
    {
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

        /// <inheritdoc />
        public override T[] this[Span<byte> source, int offset]
        {
            set => this[source.Slice(offset)] = value;
        }

        /// <summary>
        /// Read / write data.
        /// </summary>
        /// <param name="source">Data source.</param>
        public abstract T[] this[Span<byte> source] { get; set; }

        /// <inheritdoc />
        public override T[] this[ReadOnlySpan<byte> source, int offset, int count] =>
            this[source.Slice(offset, count * sizeof(T))];

        /// <inheritdoc />
        public override T[] this[long offset, int count, Stream stream]
        {
            get
            {
                byte[] arr = new byte[count * sizeof(T)];
                if (offset != -1) Processor.Read(stream, offset, arr, 0, arr.Length, false);
                else Processor.Read(stream, arr, 0, arr.Length, false);
                return this[arr, 0, count];
            }
        }

        /// <inheritdoc />
        public override T[] this[long offset, Stream stream]
        {
            set
            {
                byte[] arr = new byte[value.Length * sizeof(T)];
                this[arr, 0] = value;
                if (offset != -1) Processor.Write(stream, offset, arr);
                else Processor.Write(stream, arr);
            }
        }
    }
}
