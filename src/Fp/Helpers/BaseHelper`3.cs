using System;
using System.IO;

namespace Fp.Helpers
{
    /// <summary>
    /// Base data helper.
    /// </summary>
    /// <typeparam name="TRead">Element read type.</typeparam>
    /// <typeparam name="TWrite">Element write type.</typeparam>
    /// <typeparam name="T1">1st arg type.</typeparam>
    public abstract record BaseHelper<TRead, TWrite, T1> : Helper
    {
        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="v1">1st arg.</param>
        public virtual TRead this[byte[] source, int offset, T1 v1] => this[source.AsSpan(), offset, v1];

        /// <summary>
        /// Write data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        public virtual TWrite this[byte[] source, int offset]
        {
            set => this[source.AsSpan(), offset] = value;
        }

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="v1">1st arg.</param>
        public virtual TRead this[Memory<byte> source, int offset, T1 v1] => this[source.Span, offset, v1];

        /// <summary>
        /// Write data.
        /// </summary>
        /// <param name="offset">Offset.</param>
        /// <param name="source">Data source.</param>
        public virtual TWrite this[Memory<byte> source, int offset]
        {
            set => this[source.Span, offset] = value;
        }

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="v1">1st arg.</param>
        public virtual TRead this[Span<byte> source, int offset, T1 v1] => this[(ReadOnlySpan<byte>)source, offset, v1];

        /// <summary>
        /// Write data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        public abstract TWrite this[Span<byte> source, int offset] { set; }

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="v1">1st arg.</param>
        public virtual TRead this[ReadOnlyMemory<byte> source, int offset, T1 v1] => this[source.Span, offset, v1];

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="v1">1st arg.</param>
        public abstract TRead this[ReadOnlySpan<byte> source, int offset, T1 v1] { get; }

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="source">Data source.</param>
        public abstract TRead this[ReadOnlySpan<byte> source] { get; }

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="offset">Offset (no seeking if -1).</param>
        /// <param name="v1">1st arg.</param>
        /// <param name="stream">Data source.</param>
        public abstract TRead this[long offset, T1 v1, Stream stream] { get; }

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="offset">Offset (no seeking if -1).</param>
        /// <param name="v1">1st arg.</param>
        public virtual TRead this[long offset, T1 v1] => this[offset, v1, InputStream];

        /// <summary>
        /// Write data.
        /// </summary>
        /// <param name="offset">Offset (no seeking if -1).</param>
        /// <param name="stream">Data source.</param>
        public abstract TWrite this[long offset, Stream stream] { set; }

        /// <summary>
        /// Write data.
        /// </summary>
        /// <param name="offset">Offset (no seeking if -1).</param>
        public virtual TWrite this[long offset]
        {
            set => this[offset, OutputStream] = value;
        }
    }
}
