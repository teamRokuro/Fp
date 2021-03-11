using System;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using static System.Buffers.ArrayPool<byte>;

namespace Fp
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public partial class Processor
    {
        #region Encoding utilities

        /// <summary>
        /// Write signed 8-bit value to span at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset to write to</param>
        public void GetBytes(sbyte value, Span<byte> span, int offset = 0) => span[offset] = (byte)value;

        /// <summary>
        /// Write signed 8-bit value to memory at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="memory">Memory to write to</param>
        /// <param name="offset">Offset to write to</param>
        public void GetBytesM(sbyte value, Memory<byte> memory, int offset = 0) => GetBytes(value, memory.Span, offset);

        /// <summary>
        /// Write unsigned 8-bit value to span at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset to write to</param>
        public void GetBytes(byte value, Span<byte> span, int offset = 0) => span[offset] = value;

        /// <summary>
        /// Write unsigned 8-bit value to memory at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="memory">Memory to write to</param>
        /// <param name="offset">Offset to write to</param>
        public void GetBytesM(byte value, Memory<byte> memory, int offset = 0) => GetBytes(value, memory.Span, offset);

        /// <summary>
        /// Write signed 8-bit value to array at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Resultant array (newly allocated if none provided)</returns>
        public byte[] GetBytes(sbyte value, byte[]? array = null, int offset = 0)
        {
            array ??= new byte[1];
            GetBytes(value, array.AsSpan(offset, 1));
            return array;
        }

        /// <summary>
        /// Write unsigned 8-bit value to array at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Resultant array (newly allocated if none provided)</returns>
        public byte[] GetBytes(byte value, byte[]? array = null, int offset = 0)
        {
            array ??= new byte[1];
            GetBytes(value, array.AsSpan(offset, 1));
            return array;
        }

        /// <summary>
        /// Write signed 8-bit value to stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="stream">Stream to write to, uses current output file if null</param>
        /// <param name="offset">Offset to write to, current position if null</param>
        public void WriteS8(sbyte value, Stream? stream = null, long? offset = null)
        {
            stream ??= OutputStream ?? throw new InvalidOperationException();
            GetBytes(value, TempBuffer);
            long origPos = offset.HasValue ? stream.Position : -1;
            try
            {
                if (offset.HasValue)
                {
                    stream.Position = offset.Value;
                }

                stream.Write(TempBuffer, 0, sizeof(sbyte));
            }
            finally
            {
                if (offset.HasValue)
                {
                    stream.Position = origPos;
                }
            }
        }

        /// <summary>
        /// Write unsigned 8-bit value to stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="stream">Stream to write to, uses current output file if null</param>
        /// <param name="offset">Offset to write to, current position if null</param>
        public void WriteU8(byte value, Stream? stream = null, long? offset = null)
        {
            stream ??= OutputStream ?? throw new InvalidOperationException();
            GetBytes(value, TempBuffer);
            long origPos = offset.HasValue ? stream.Position : -1;
            try
            {
                if (offset.HasValue)
                {
                    stream.Position = offset.Value;
                }

                stream.Write(TempBuffer, 0, sizeof(byte));
            }
            finally
            {
                if (offset.HasValue)
                {
                    stream.Position = origPos;
                }
            }
        }

        /// <summary>
        /// Write signed 16-bit value to span at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset to write to</param>
        public unsafe void GetBytes(short value, Span<byte> span, int offset = 0)
        {
            if (_swap) value = BinaryPrimitives.ReverseEndianness(value);
            new ReadOnlySpan<byte>(&value, 2).CopyTo(span.Slice(offset, 2));
        }

        /// <summary>
        /// Write signed 16-bit value to memory at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="memory">Memory to write to</param>
        /// <param name="offset">Offset to write to</param>
        public void GetBytesM(short value, Memory<byte> memory, int offset = 0) => GetBytes(value, memory.Span, offset);

        /// <summary>
        /// Write unsigned 16-bit value to span at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset to write to</param>
        public unsafe void GetBytes(ushort value, Span<byte> span, int offset = 0)
        {
            if (_swap) value = BinaryPrimitives.ReverseEndianness(value);
            new ReadOnlySpan<byte>(&value, 2).CopyTo(span.Slice(offset, 2));
        }

        /// <summary>
        /// Write unsigned 16-bit value to memory at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="memory">Memory to write to</param>
        /// <param name="offset">Offset to write to</param>
        public void GetBytesM(ushort value, Memory<byte> memory, int offset = 0) =>
            GetBytes(value, memory.Span, offset);

        /// <summary>
        /// Write signed 16-bit value to array at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Resultant array (newly allocated if none provided)</returns>
        public byte[] GetBytes(short value, byte[]? array = null, int offset = 0)
        {
            array ??= new byte[2];
            GetBytes(value, array.AsSpan(offset, 2));
            return array;
        }

        /// <summary>
        /// Write unsigned 16-bit value to array at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Resultant array (newly allocated if none provided)</returns>
        public byte[] GetBytes(ushort value, byte[]? array = null, int offset = 0)
        {
            array ??= new byte[2];
            GetBytes(value, array.AsSpan(offset, 2));
            return array;
        }

        /// <summary>
        /// Write signed 16-bit value to stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="stream">Stream to write to, uses current output file if null</param>
        /// <param name="offset">Offset to write to, current position if null</param>
        public void WriteS16(short value, Stream? stream = null, long? offset = null)
        {
            stream ??= OutputStream ?? throw new InvalidOperationException();
            GetBytes(value, TempBuffer);
            long origPos = offset.HasValue ? stream.Position : -1;
            try
            {
                if (offset.HasValue)
                {
                    stream.Position = offset.Value;
                }

                stream.Write(TempBuffer, 0, sizeof(short));
            }
            finally
            {
                if (offset.HasValue)
                {
                    stream.Position = origPos;
                }
            }
        }

        /// <summary>
        /// Write unsigned 16-bit value to stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="stream">Stream to write to, uses current output file if null</param>
        /// <param name="offset">Offset to write to, current position if null</param>
        public void WriteU16(ushort value, Stream? stream = null, long? offset = null)
        {
            stream ??= OutputStream ?? throw new InvalidOperationException();
            GetBytes(value, TempBuffer);
            long origPos = offset.HasValue ? stream.Position : -1;
            try
            {
                if (offset.HasValue)
                {
                    stream.Position = offset.Value;
                }

                stream.Write(TempBuffer, 0, sizeof(ushort));
            }
            finally
            {
                if (offset.HasValue)
                {
                    stream.Position = origPos;
                }
            }
        }

        /// <summary>
        /// Write signed 32-bit value to span at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset to write to</param>
        public unsafe void GetBytes(int value, Span<byte> span, int offset = 0)
        {
            if (_swap) value = BinaryPrimitives.ReverseEndianness(value);
            new ReadOnlySpan<byte>(&value, 4).CopyTo(span.Slice(offset, 4));
        }

        /// <summary>
        /// Write signed 32-bit value to memory at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="memory">Memory to write to</param>
        /// <param name="offset">Offset to write to</param>
        public void GetBytesM(int value, Memory<byte> memory, int offset = 0) => GetBytes(value, memory.Span, offset);

        /// <summary>
        /// Write unsigned 32-bit value to span at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset to write to</param>
        public unsafe void GetBytes(uint value, Span<byte> span, int offset = 0)
        {
            if (_swap) value = BinaryPrimitives.ReverseEndianness(value);
            new ReadOnlySpan<byte>(&value, 4).CopyTo(span.Slice(offset, 4));
        }

        /// <summary>
        /// Write unsigned 32-bit value to memory at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="memory">Memory to write to</param>
        /// <param name="offset">Offset to write to</param>
        public void GetBytesM(uint value, Memory<byte> memory, int offset = 0) => GetBytes(value, memory.Span, offset);

        /// <summary>
        /// Write signed 32-bit value to array at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Resultant array (newly allocated if none provided)</returns>
        public byte[] GetBytes(int value, byte[]? array = null, int offset = 0)
        {
            array ??= new byte[4];
            GetBytes(value, array.AsSpan(offset, 4));
            return array;
        }

        /// <summary>
        /// Write unsigned 32-bit value to array at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Resultant array (newly allocated if none provided)</returns>
        public byte[] GetBytes(uint value, byte[]? array = null, int offset = 0)
        {
            array ??= new byte[4];
            GetBytes(value, array.AsSpan(offset, 4));
            return array;
        }

        /// <summary>
        /// Write signed 32-bit value to stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="stream">Stream to write to, uses current output file if null</param>
        /// <param name="offset">Offset to write to, current position if null</param>
        public void WriteS32(int value, Stream? stream = null, long? offset = null)
        {
            stream ??= OutputStream ?? throw new InvalidOperationException();
            GetBytes(value, TempBuffer);
            long origPos = offset.HasValue ? stream.Position : -1;
            try
            {
                if (offset.HasValue)
                {
                    stream.Position = offset.Value;
                }

                stream.Write(TempBuffer, 0, sizeof(int));
            }
            finally
            {
                if (offset.HasValue)
                {
                    stream.Position = origPos;
                }
            }
        }

        /// <summary>
        /// Write unsigned 32-bit value to stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="stream">Stream to write to, uses current output file if null</param>
        /// <param name="offset">Offset to write to, current position if null</param>
        public void WriteU32(uint value, Stream? stream = null, long? offset = null)
        {
            stream ??= OutputStream ?? throw new InvalidOperationException();
            GetBytes(value, TempBuffer);
            long origPos = offset.HasValue ? stream.Position : -1;
            try
            {
                if (offset.HasValue)
                {
                    stream.Position = offset.Value;
                }

                stream.Write(TempBuffer, 0, sizeof(uint));
            }
            finally
            {
                if (offset.HasValue)
                {
                    stream.Position = origPos;
                }
            }
        }

        /// <summary>
        /// Write signed 64-bit value to span at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset to write to</param>
        public unsafe void GetBytes(long value, Span<byte> span, int offset = 0)
        {
            if (_swap) value = BinaryPrimitives.ReverseEndianness(value);
            new ReadOnlySpan<byte>(&value, 8).CopyTo(span.Slice(offset, 8));
        }

        /// <summary>
        /// Write signed 64-bit value to memory at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="memory">Memory to write to</param>
        /// <param name="offset">Offset to write to</param>
        public void GetBytesM(long value, Memory<byte> memory, int offset = 0) => GetBytes(value, memory.Span, offset);

        /// <summary>
        /// Write unsigned 64-bit value to span at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset to write to</param>
        public unsafe void GetBytes(ulong value, Span<byte> span, int offset = 0)
        {
            if (_swap) value = BinaryPrimitives.ReverseEndianness(value);
            new ReadOnlySpan<byte>(&value, 8).CopyTo(span.Slice(offset, 8));
        }

        /// <summary>
        /// Write unsigned 64-bit value to memory at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="memory">Memory to write to</param>
        /// <param name="offset">Offset to write to</param>
        public void GetBytesM(ulong value, Memory<byte> memory, int offset = 0) => GetBytes(value, memory.Span, offset);

        /// <summary>
        /// Write signed 64-bit value to array at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Resultant array (newly allocated if none provided)</returns>
        public byte[] GetBytes(long value, byte[]? array = null, int offset = 0)
        {
            array ??= new byte[8];
            GetBytes(value, array.AsSpan(offset, 8));
            return array;
        }

        /// <summary>
        /// Write unsigned 64-bit value to array at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Resultant array (newly allocated if none provided)</returns>
        public byte[] GetBytes(ulong value, byte[]? array = null, int offset = 0)
        {
            array ??= new byte[8];
            GetBytes((long)value, array.AsSpan(offset, 8));
            return array;
        }

        /// <summary>
        /// Write signed 64-bit value to stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="stream">Stream to write to, uses current output file if null</param>
        /// <param name="offset">Offset to write to, current position if null</param>
        public void WriteS64(long value, Stream? stream = null, long? offset = null)
        {
            stream ??= OutputStream ?? throw new InvalidOperationException();
            GetBytes(value, TempBuffer);
            long origPos = offset.HasValue ? stream.Position : -1;
            try
            {
                if (offset.HasValue)
                {
                    stream.Position = offset.Value;
                }

                stream.Write(TempBuffer, 0, sizeof(long));
            }
            finally
            {
                if (offset.HasValue)
                {
                    stream.Position = origPos;
                }
            }
        }

        /// <summary>
        /// Write unsigned 64-bit value to stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="stream">Stream to write to, uses current output file if null</param>
        /// <param name="offset">Offset to write to, current position if null</param>
        public void WriteU64(ulong value, Stream? stream = null, long? offset = null)
        {
            stream ??= OutputStream ?? throw new InvalidOperationException();
            GetBytes(value, TempBuffer);
            long origPos = offset.HasValue ? stream.Position : -1;
            try
            {
                if (offset.HasValue)
                {
                    stream.Position = offset.Value;
                }

                stream.Write(TempBuffer, 0, sizeof(ulong));
            }
            finally
            {
                if (offset.HasValue)
                {
                    stream.Position = origPos;
                }
            }
        }

        /// <summary>
        /// Write 16-bit float value to span at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset to write to</param>
        public static unsafe void GetBytesHalf(ushort value, Span<byte> span, int offset = 0) =>
            new ReadOnlySpan<byte>(&value, 2).CopyTo(span.Slice(offset, 2));

        /// <summary>
        /// Write 16-bit float value to memory at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="memory">Memory to write to</param>
        /// <param name="offset">Offset to write to</param>
        public static void GetBytesHalfM(ushort value, Memory<byte> memory, int offset = 0) =>
            GetBytesHalf(value, memory.Span, offset);

        /// <summary>
        /// Write 32-bit float value as 16-bit to span at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset to write to</param>
        public static unsafe void GetBytesHalf(float value, Span<byte> span, int offset = 0)
        {
            ushort v = HalfHelper.SingleToHalf(value);
            new ReadOnlySpan<byte>(&v, 2).CopyTo(span.Slice(offset, 2));
        }

        /// <summary>
        /// Write 32-bit float value as 16-bit to memory at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="memory">Memory to write to</param>
        /// <param name="offset">Offset to write to</param>
        public static void GetBytesHalfM(float value, Memory<byte> memory, int offset = 0) =>
            GetBytesHalf(value, memory.Span, offset);

        /// <summary>
        /// Write 32-bit float value to array at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Resultant array (newly allocated if none provided)</returns>
        public static byte[] GetBytesHalf(ushort value, byte[]? array = null, int offset = 0)
        {
            array ??= new byte[2];
            GetBytesHalf(value, array.AsSpan(offset, 2));
            return array;
        }

        /// <summary>
        /// Write 32-bit float value to array at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Resultant array (newly allocated if none provided)</returns>
        public static byte[] GetBytesHalf(float value, byte[]? array = null, int offset = 0)
        {
            array ??= new byte[2];
            GetBytesHalf(value, array.AsSpan(offset, 2));
            return array;
        }

        /// <summary>
        /// Write signed 32-bit float value to stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="stream">Stream to write to, uses current output file if null</param>
        /// <param name="offset">Offset to write to, current position if null</param>
        public void WriteHalf(ushort value, Stream? stream = null, long? offset = null)
        {
            stream ??= OutputStream ?? throw new InvalidOperationException();
            GetBytesHalf(value, TempBuffer);
            long origPos = offset.HasValue ? stream.Position : -1;
            try
            {
                if (offset.HasValue)
                {
                    stream.Position = offset.Value;
                }

                stream.Write(TempBuffer, 0, sizeof(ushort));
            }
            finally
            {
                if (offset.HasValue)
                {
                    stream.Position = origPos;
                }
            }
        }

        /// <summary>
        /// Write signed 32-bit float value to stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="stream">Stream to write to, uses current output file if null</param>
        /// <param name="offset">Offset to write to, current position if null</param>
        public void WriteHalf(float value, Stream? stream = null, long? offset = null)
        {
            stream ??= OutputStream ?? throw new InvalidOperationException();
            GetBytesHalf(value, TempBuffer);
            long origPos = offset.HasValue ? stream.Position : -1;
            try
            {
                if (offset.HasValue)
                {
                    stream.Position = offset.Value;
                }

                stream.Write(TempBuffer, 0, sizeof(ushort));
            }
            finally
            {
                if (offset.HasValue)
                {
                    stream.Position = origPos;
                }
            }
        }

        /// <summary>
        /// Write 32-bit float value to span at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset to write to</param>
        public static unsafe void GetBytes(float value, Span<byte> span, int offset = 0) =>
            new ReadOnlySpan<byte>(&value, 4).CopyTo(span.Slice(offset, 4));

        /// <summary>
        /// Write 32-bit float value to memory at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="memory">Memory to write to</param>
        /// <param name="offset">Offset to write to</param>
        public void GetBytesM(float value, Memory<byte> memory, int offset = 0) => GetBytes(value, memory.Span, offset);

        /// <summary>
        /// Write 32-bit float value to array at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Resultant array (newly allocated if none provided)</returns>
        public byte[] GetBytes(float value, byte[]? array = null, int offset = 0)
        {
            array ??= new byte[4];
            GetBytes(value, array.AsSpan(offset, 4));
            return array;
        }

        /// <summary>
        /// Write signed 32-bit float value to stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="stream">Stream to write to, uses current output file if null</param>
        /// <param name="offset">Offset to write to, current position if null</param>
        public void WriteSingle(float value, Stream? stream = null, long? offset = null)
        {
            stream ??= OutputStream ?? throw new InvalidOperationException();
            GetBytes(value, TempBuffer);
            long origPos = offset.HasValue ? stream.Position : -1;
            try
            {
                if (offset.HasValue)
                {
                    stream.Position = offset.Value;
                }

                stream.Write(TempBuffer, 0, sizeof(float));
            }
            finally
            {
                if (offset.HasValue)
                {
                    stream.Position = origPos;
                }
            }
        }

        /// <summary>
        /// Write 64-bit float value to span at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset to write to</param>
        public static unsafe void GetBytes(double value, Span<byte> span, int offset = 0) =>
            new ReadOnlySpan<byte>(&value, 8).CopyTo(span.Slice(offset, 8));

        /// <summary>
        /// Write 64-bit float value to memory at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="memory">Memory to write to</param>
        /// <param name="offset">Offset to write to</param>
        public void GetBytesM(double value, Memory<byte> memory, int offset = 0) =>
            GetBytes(value, memory.Span, offset);

        /// <summary>
        /// Write 64-bit float value to array at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Resultant array (newly allocated if none provided)</returns>
        public byte[] GetBytes(double value, byte[]? array = null, int offset = 0)
        {
            array ??= new byte[8];
            GetBytes(value, array.AsSpan(offset, 8));
            return array;
        }

        /// <summary>
        /// Write signed 64-bit float value to stream
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="stream">Stream to write to, uses current output file if null</param>
        /// <param name="offset">Offset to write to, current position if null</param>
        public void WriteDouble(double value, Stream? stream = null, long? offset = null)
        {
            stream ??= OutputStream ?? throw new InvalidOperationException();
            GetBytes(value, TempBuffer);
            long origPos = offset.HasValue ? stream.Position : -1;
            try
            {
                if (offset.HasValue)
                {
                    stream.Position = offset.Value;
                }

                stream.Write(TempBuffer, 0, sizeof(double));
            }
            finally
            {
                if (offset.HasValue)
                {
                    stream.Position = origPos;
                }
            }
        }

        #endregion
    }
}
