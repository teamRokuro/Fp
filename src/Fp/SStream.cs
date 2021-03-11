using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace Fp
{
    /// <summary>
    /// Stream that acts as a limited-range proxy for another stream
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class SStream : Stream
    {
        private readonly Stream _sourceStream;
        private readonly long _offset;
        private long _position;
        private long _length;

        /// <summary>
        /// Create a limited-range proxy from the current position with a specified length
        /// </summary>
        /// <param name="sourceStream">Stream to wrap</param>
        /// <param name="length">Length of proxy</param>
        /// <param name="isolate">If true, enforces stream position for object before reads</param>
        public SStream(Stream sourceStream, long length, bool isolate = true)
        {
            if (isolate && !sourceStream.CanSeek)
                throw new ArgumentException("Cannot set isolate if stream is not seekable");
            Isolate = isolate;
            _position = 0;
            _sourceStream = sourceStream;
            if (sourceStream.CanSeek) _offset = sourceStream.Position;
            _length = length;
        }

        /// <summary>
        /// If true, enforces stream position for object before reads
        /// </summary>
        public readonly bool Isolate;

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
            get => CanSeek ? _position : throw new NotSupportedException();
            set => Seek(value, SeekOrigin.Begin);
        }

        /// <inheritdoc />
        public override void Flush() => _sourceStream.Flush();

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Isolate && _offset + _position != _sourceStream.Position)
                _sourceStream.Seek(_offset + _position, SeekOrigin.Begin);
            int read = _sourceStream.Read(buffer, offset,
                (int)(Math.Min(_length, _position + count) - _position));
            _position += read;
            return read;
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!CanSeek) throw new NotSupportedException();
            if (!Isolate && origin == SeekOrigin.Current)
                return _position = _sourceStream.Seek(offset, SeekOrigin.Begin) - _offset;
            return _position = origin switch
            {
                SeekOrigin.Begin => _sourceStream.Seek(_offset + offset, SeekOrigin.Begin),
                SeekOrigin.Current => _sourceStream.Seek(_offset + _position + offset, SeekOrigin.Begin),
                SeekOrigin.End => _sourceStream.Seek(_offset + _length - _sourceStream.Length + offset, SeekOrigin.End),
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            } - _offset;
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            if (value < 0) throw new ArgumentException("Length cannot be negative");
            if (_sourceStream.CanSeek && _offset + _length > _sourceStream.Length)
                throw new ArgumentException(
                    $"Cannot set length to {value}, base stream length {_sourceStream.Length} self offset {_offset}");
            _length = value;
            _position = Math.Min(_length, _position);
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (Isolate && _offset + _position != _sourceStream.Position)
                _sourceStream.Seek(_offset + _position, SeekOrigin.Begin);
            int write = (int)(Math.Min(_length, _position + count) - _position);
            _sourceStream.Write(buffer, offset, write);
            _position += write;
        }
    }
}
