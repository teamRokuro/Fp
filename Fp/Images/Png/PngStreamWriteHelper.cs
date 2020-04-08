using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fp.Images.Png {
    internal class PngStreamWriteHelper : Stream {
        private readonly Stream _inner;
        private readonly MemoryStream _written = new MemoryStream();

        public override bool CanRead => _inner.CanRead;

        public override bool CanSeek => _inner.CanSeek;

        public override bool CanWrite => _inner.CanWrite;

        public override long Length => _inner.Length;

        public override long Position {
            get => _inner.Position;
            set => _inner.Position = value;
        }

        public PngStreamWriteHelper(Stream inner) {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public override void Flush() => _inner.Flush();

        public void WriteChunkHeader(byte[] header) {
            _written.SetLength(0);
            Write(header, 0, header.Length);
        }

        public void WriteChunkLength(int length) {
            StreamHelper.WriteBigEndianInt32(_inner, length);
        }

        public override int Read(byte[] buffer, int offset, int count) => _inner.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);

        public override void SetLength(long value) => _inner.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count) {
            _written.Write(buffer, offset, count);
            _inner.Write(buffer, offset, count);
        }

        public void WriteCrc() {
            var result = (int) Crc32.Calculate(_written.GetBuffer().AsSpan(0, (int)_written.Length));
            StreamHelper.WriteBigEndianInt32(_inner, result);
        }
    }
}