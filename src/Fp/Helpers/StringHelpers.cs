using System;
using System.IO;
using System.Text;
using static Fp.Processor;

namespace Fp.Helpers
{
    /// <summary>
    /// Represents converted string
    /// </summary>
    public readonly struct StringData
    {
        /// <summary>
        /// String value
        /// </summary>
        public readonly string String;

        /// <summary>
        /// Byte length
        /// </summary>
        public readonly int ByteLength;

        /// <summary>
        /// Creates new instance of <see cref="StringData"/>
        /// </summary>
        /// <param name="s">String value</param>
        /// <param name="byteLength">Byte length</param>
        public StringData(string s, int byteLength)
        {
            String = s;
            ByteLength = byteLength;
        }

        /// <summary>
        /// Convert string to <see cref="StringData"/> with 0 byte length
        /// </summary>
        /// <param name="str">String value</param>
        /// <returns><see cref="StringData"/></returns>
        public static implicit operator StringData(string str) => new StringData(str, 0);

        /// <summary>
        /// Deconstruct this instance.
        /// </summary>
        /// <param name="s">String value</param>
        /// <param name="byteLength">Byte length</param>
        public void Deconstruct(out string s, out int byteLength)
        {
            s = String;
            byteLength = ByteLength;
        }

        /// <summary>
        /// Encoding type
        /// </summary>
        public enum Encoding
        {
            /// <summary>
            /// UTF-8 encoding
            /// </summary>
            UTF8,

            /// <summary>
            /// UTF-16 encoding
            /// </summary>
            UTF16
        }
    }

    /// <summary>
    /// Base helper type for strings.
    /// </summary>
    public abstract record BaseStringHelper : Helper
    {
        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="maxBytes">Maximum bytes to read.</param>
        public virtual StringData this[byte[] source, int offset, int maxBytes] =>
            this[source.AsSpan(), offset, maxBytes];

        /// <summary>
        /// Write data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        public virtual StringData this[byte[] source, int offset]
        {
            get => this[source.AsSpan(), offset];
            set => this[source.AsSpan(), offset] = value;
        }

        /// <summary>
        /// Write data.
        /// </summary>
        /// <param name="source">Data source.</param>
        public virtual StringData this[byte[] source]
        {
            get => this[source.AsSpan()];
            set => this[source.AsSpan()] = value;
        }

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="maxBytes">Maximum bytes to read.</param>
        public virtual StringData this[Memory<byte> source, int offset, int maxBytes] =>
            this[source.Span, offset, maxBytes];

        /// <summary>
        /// Read/write data.
        /// </summary>
        /// <param name="offset">Offset.</param>
        /// <param name="source">Data source.</param>
        public virtual StringData this[Memory<byte> source, int offset]
        {
            get => this[source.Span, offset];
            set => this[source.Span, offset] = value;
        }

        /// <summary>
        /// Read/write data.
        /// </summary>
        /// <param name="source">Data source.</param>
        public virtual StringData this[Memory<byte> source]
        {
            get => this[source.Span];
            set => this[source.Span] = value;
        }

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="maxBytes">Maximum bytes to read.</param>
        public virtual StringData this[Span<byte> source, int offset, int maxBytes] =>
            this[(ReadOnlySpan<byte>)source, offset, maxBytes];

        /// <summary>
        /// Read/write data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        public virtual StringData this[Span<byte> source, int offset]
        {
            get => this[source.Slice(offset)];
            set => this[source.Slice(offset)] = value;
        }

        /// <summary>
        /// Read/write data.
        /// </summary>
        /// <param name="source">Data source.</param>
        public abstract StringData this[Span<byte> source] { get; set; }

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="maxBytes">Maximum bytes to read.</param>
        public virtual StringData this[ReadOnlyMemory<byte> source, int offset, int maxBytes] =>
            this[source.Span, offset, maxBytes];

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        /// <param name="maxBytes">Maximum bytes to read.</param>
        public abstract StringData this[ReadOnlySpan<byte> source, int offset,
            int maxBytes] { get; }

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="source">Data source.</param>
        /// <param name="offset">Offset.</param>
        public virtual StringData this[ReadOnlySpan<byte> source, int offset] =>
            this[source, offset, int.MaxValue];

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="source">Data source.</param>
        public virtual StringData this[ReadOnlySpan<byte> source] => this[source, 0, int.MaxValue];

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="offset">Offset (no seeking if -1).</param>v
        /// <param name="stream">Data source.</param>
        /// <param name="maxBytes">Maximum bytes to read.</param>
        public abstract StringData this[long offset, Stream stream, int maxBytes] { get; }

        /// <summary>
        /// Read data.
        /// </summary>
        /// <param name="offset">Offset (no seeking if -1).</param>
        /// <param name="maxBytes">Maximum bytes to read.</param>
        public virtual StringData this[long offset, int maxBytes] =>
            this[offset, InputStream, maxBytes];

        /// <summary>
        /// Write data.
        /// </summary>
        /// <param name="offset">Offset (no seeking if -1).</param>
        /// <param name="stream">Data source.</param>
        public abstract StringData this[long offset, Stream stream] { get; set; }

        /// <summary>
        /// Write data.
        /// </summary>
        /// <param name="offset">Offset (no seeking if -1).</param>
        public virtual StringData this[long offset]
        {
            get => this[offset, OutputStream];
            set => this[offset, OutputStream] = value;
        }
    }

    /// <summary>
    /// UTF-8 string helper.
    /// </summary>
    public record Utf8StringHelper(Processor Parent) : BaseStringHelper
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override StringData this[Span<byte> source]
        {
            get => this[(ReadOnlySpan<byte>)source];
            set => Encoding.UTF8.GetBytes(value.String).CopyTo(source);
        }

        /// <inheritdoc />
        public override StringData this[ReadOnlySpan<byte> source, int offset,
            int maxBytes] =>
            new(ReadUtf8String(source.Slice(offset), out _, out int numBytes, maxBytes), numBytes);

        /// <inheritdoc />
        public override StringData this[long offset, Stream stream, int maxBytes] =>
            offset != -1
                ? new StringData(Instance.ReadUtf8StringFromOffset(stream, offset, out _, out int numBytes1, maxBytes),
                    numBytes1)
                : new StringData(Instance.ReadUtf8String(stream, out _, out int numBytes2), numBytes2);

        /// <inheritdoc />
        public override StringData this[long offset, Stream stream]
        {
            get =>
                offset != -1
                    ? new StringData(Instance.ReadUtf8StringFromOffset(stream, offset, out _, out int numBytes1),
                        numBytes1)
                    : new StringData(Instance.ReadUtf8String(stream, out _, out int numBytes2), numBytes2);
            set => Instance.WriteUtf8String(value.String, false, stream, offset != -1 ? offset : null);
        }
    }

    /// <summary>
    /// UTF-16 string helper.
    /// </summary>
    public record Utf16StringHelper(Processor Parent) : BaseStringHelper
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override StringData this[Span<byte> source]
        {
            get => this[(ReadOnlySpan<byte>)source];
            set => Encoding.Unicode.GetBytes(value.String).CopyTo(source);
        }

        /// <inheritdoc />
        public override StringData this[ReadOnlySpan<byte> source, int offset,
            int maxBytes] =>
            new(ReadUtf16String(source.Slice(offset), out _, out int numBytes, maxBytes), numBytes);

        /// <inheritdoc />
        public override StringData this[long offset, Stream stream, int maxBytes] =>
            offset != -1
                ? new StringData(Instance.ReadUtf16StringFromOffset(stream, offset, out _, out int numBytes1, maxBytes),
                    numBytes1)
                : new StringData(Instance.ReadUtf16String(stream, out _, out int numBytes2), numBytes2);

        /// <inheritdoc />
        public override StringData this[long offset, Stream stream]
        {
            get =>
                offset != -1
                    ? new StringData(Instance.ReadUtf16StringFromOffset(stream, offset, out _, out int numBytes1),
                        numBytes1)
                    : new StringData(Instance.ReadUtf16String(stream, out _, out int numBytes2), numBytes2);
            set => Instance.WriteUtf16String(value.String, false, false, false, stream, offset != -1 ? offset : null);
        }
    }
}
