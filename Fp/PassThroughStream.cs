using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Fp
{
    /// <summary>
    /// Stream that acts as a limited-range proxy for another stream
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class PassThroughStream : Stream
    {
        private readonly Stream _sourceStream;
        private readonly long _offset;
        private long _length;

        /// <summary>
        /// Create a limited-range proxy from the current position with a specified length
        /// </summary>
        /// <param name="sourceStream">Stream to wrap</param>
        /// <param name="length">Length of proxy</param>
        public PassThroughStream(Stream sourceStream, long length)
        {
            _sourceStream = sourceStream;
            _offset = sourceStream.Position;
            _length = length;
        }

        /// <inheritdoc />
        public override bool CanRead => _sourceStream.CanRead;

        /// <inheritdoc />
        public override bool CanSeek => _sourceStream.CanSeek;

        /// <inheritdoc />
        public override bool CanWrite => _sourceStream.CanWrite;

        /// <inheritdoc />
        public override long Length => _length;

        /// <inheritdoc />
        public override long Position
        {
            get => _sourceStream.Position - _offset;
            set => _sourceStream.Position = _offset + value;
        }

        /// <inheritdoc />
        public override void Flush() => _sourceStream.Flush();

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count) => _sourceStream.Read(buffer, offset,
            (int)(Math.Min(_length, _sourceStream.Position - _offset + count) - (_sourceStream.Position - _offset)));

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin) => _sourceStream.Seek(_offset + offset, origin);

        /// <inheritdoc />
        public override void SetLength(long value) => _length = value;

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count) => _sourceStream.Write(buffer, offset,
            (int)(Math.Min(_length, _sourceStream.Position - _offset + count) - (_sourceStream.Position - _offset)));
    }
}
