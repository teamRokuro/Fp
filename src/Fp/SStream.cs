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
        /// <summary>
        /// Base stream for this wrapper stream
        /// </summary>
        public Stream BaseStream => _baseStream;

        /// <summary>
        /// Offset in base stream in this wrapper stream (only available if <see cref="CanSeek"/> is true)
        /// </summary>
        public long Offset => CanSeek ? _offset : throw new NotSupportedException();

        /// <summary>
        /// If true, enforces stream position for object before reads
        /// </summary>
        public readonly bool Isolate;

        private readonly Stream _baseStream;
        private readonly long _offset;
        private long _position;
        private long _length;

        /// <summary>
        /// Create a limited-range proxy from the current position with a specified length
        /// </summary>
        /// <param name="baseStream">Stream to wrap</param>
        /// <param name="length">Length of proxy</param>
        /// <param name="isolate">If true, enforces stream position for object before reads</param>
        public SStream(Stream baseStream, long length, bool isolate = true)
        {
            if (isolate && !baseStream.CanSeek)
                throw new ArgumentException("Cannot set isolate if stream is not seekable");
            Isolate = isolate;
            _position = 0;
            _baseStream = baseStream;
            if (baseStream.CanSeek) _offset = baseStream.Position;
            _length = length;
        }

        /// <inheritdoc />
        public override bool CanRead => _baseStream.CanRead;

        /// <inheritdoc />
        public override bool CanSeek => _baseStream.CanSeek;

        /// <inheritdoc />
        public override bool CanWrite => _baseStream.CanWrite;

        /// <inheritdoc />
        public override long Length => _length;

        /// <inheritdoc />
        public override long Position
        {
            get => CanSeek ? _position : throw new NotSupportedException();
            set => Seek(value, SeekOrigin.Begin);
        }

        /// <inheritdoc />
        public override void Flush() => _baseStream.Flush();

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (Isolate && _offset + _position != _baseStream.Position)
                _baseStream.Seek(_offset + _position, SeekOrigin.Begin);
            int read = _baseStream.Read(buffer, offset,
                (int)(Math.Min(_length, _position + count) - _position));
            _position += read;
            return read;
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!CanSeek) throw new NotSupportedException();
            if (!Isolate && origin == SeekOrigin.Current)
                return _position = _baseStream.Seek(offset, SeekOrigin.Begin) - _offset;
            return _position = origin switch
            {
                SeekOrigin.Begin => _baseStream.Seek(_offset + offset, SeekOrigin.Begin),
                SeekOrigin.Current => _baseStream.Seek(_offset + _position + offset, SeekOrigin.Begin),
                SeekOrigin.End => _baseStream.Seek(_offset + _length - _baseStream.Length + offset, SeekOrigin.End),
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            } - _offset;
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            if (value < 0) throw new ArgumentException("Length cannot be negative");
            if (_baseStream.CanSeek && _offset + _length > _baseStream.Length)
                throw new ArgumentException(
                    $"Cannot set length to {value}, base stream length {_baseStream.Length} self offset {_offset}");
            _length = value;
            _position = Math.Min(_length, _position);
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (Isolate && _offset + _position != _baseStream.Position)
                _baseStream.Seek(_offset + _position, SeekOrigin.Begin);
            int write = (int)(Math.Min(_length, _position + count) - _position);
            _baseStream.Write(buffer, offset, write);
            _position += write;
        }
    }
}
