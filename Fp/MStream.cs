using System;
using System.IO;

namespace Fp
{
    /// <summary>
    /// Stream from <see cref="Memory{T}"/> of bytes
    /// </summary>
    public class MStream : Stream
    {
        private readonly Memory<byte> _memory;
        private readonly int _length;
        private int _position;

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override bool CanSeek => true;

        /// <inheritdoc />
        public override bool CanWrite => true;

        /// <inheritdoc />
        public override long Length => _length;

        /// <inheritdoc />
        public override long Position
        {
            get => _position;
            set => _position = (int)value;
        }

        /// <inheritdoc />
        public override void Flush()
        {
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            count = Math.Min(count, Math.Min(_length - _position, buffer.Length - offset));
            _memory.Slice(_position, count).CopyTo(buffer.AsMemory(offset, count));
            _position += count;
            return count;
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    _position = (int)offset;
                    break;
                case SeekOrigin.Current:
                    _position += (int)offset;
                    break;
                case SeekOrigin.End:
                    _position = _length + (int)offset;
                    break;
            }

            return _position;
        }

        /// <summary>
        /// Not supported
        /// </summary>
        /// <param name="value"></param>
        /// <exception cref="NotSupportedException"></exception>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (Math.Min(count, Math.Min(_length - _position, buffer.Length - offset)) < count)
                throw new IndexOutOfRangeException(
                    $"Array of length {buffer.Length}, offset {offset} and count {count} could not be used to " +
                    $"populate Memory<byte> at position {_position} with length {_length}");
            buffer.AsMemory(offset, count).CopyTo(_memory.Slice(_position, count));
            _position += count;
        }

        /// <summary>
        /// Create new instance
        /// </summary>
        /// <param name="memory">Memory instance</param>
        public MStream(Memory<byte> memory)
        {
            _memory = memory;
            _length = memory.Length;
        }

        internal Memory<byte> GetMemory() => _memory;
    }
}
