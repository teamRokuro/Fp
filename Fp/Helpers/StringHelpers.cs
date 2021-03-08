using System;
using System.IO;
using System.Text;
using static Fp.Processor;

namespace Fp.Helpers
{
    /// <summary>
    /// UTF-8 string helper.
    /// </summary>
    public record Utf8StringHelper(Processor Parent) : BaseHelper<(string text, int byteCount), string, int>
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override string this[Span<byte> source, int offset]
        {
            set => Encoding.UTF8.GetBytes(value).CopyTo(source.Slice(offset));
        }

        /// <inheritdoc />
        public override (string text, int byteCount) this[ReadOnlySpan<byte> source, int offset, int maxBytes] =>
            (ReadUtf8String(source, out int byteCount, maxBytes), byteCount);

        /// <inheritdoc />
        public override (string text, int byteCount) this[ReadOnlySpan<byte> source] =>
            (ReadUtf8String(source, out int byteCount), byteCount);

        /// <inheritdoc />
        public override (string text, int byteCount) this[long offset, int maxBytes, Stream stream] =>
            offset != -1
                ? (Instance.ReadUtf8StringFromOffset(stream, offset, out int byteCount1, maxBytes), byteCount1)
                : (Instance.ReadUtf8String(stream, out int byteCount2), byteCount2);

        /// <inheritdoc />
        public override string this[long offset, Stream stream]
        {
            set => Instance.WriteUtf8String(value, false, stream, offset != -1 ? offset : null);
        }
    }

    /// <summary>
    /// UTF-16 string helper.
    /// </summary>
    public record Utf16StringHelper(Processor Parent) : BaseHelper<(string text, int byteCount), string, int>
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override string this[Span<byte> source, int offset]
        {
            set => Encoding.Unicode.GetBytes(value).CopyTo(source.Slice(offset));
        }

        /// <inheritdoc />
        public override (string text, int byteCount) this[ReadOnlySpan<byte> source, int offset, int maxBytes] =>
            (ReadUtf16String(source, out int byteCount, maxBytes), byteCount);

        /// <inheritdoc />
        public override (string text, int byteCount) this[ReadOnlySpan<byte> source] =>
            (ReadUtf16String(source, out int byteCount), byteCount);

        /// <inheritdoc />
        public override (string text, int byteCount) this[long offset, int maxBytes, Stream stream] =>
            offset != -1
                ? (Instance.ReadUtf16StringFromOffset(stream, offset, out int byteCount1, maxBytes), byteCount1)
                : (Instance.ReadUtf16String(stream, out int byteCount2), byteCount2);

        /// <inheritdoc />
        public override string this[long offset, Stream stream]
        {
            set => Instance.WriteUtf16String(value, false, false, false, stream, offset != -1 ? offset : null);
        }
    }
}
