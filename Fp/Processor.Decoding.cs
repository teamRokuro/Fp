using System;
using System.Buffers.Binary;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
#if NET5_0
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
#endif
using System.Text;
using static System.Buffers.ArrayPool<byte>;

namespace Fp
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class Processor
    {
        #region Decoding utilities

        /// <summary>
        /// Read signed 8-bit value from stream
        /// </summary>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public sbyte ReadS8(Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream ms && ms.TryGetBuffer(out ArraySegment<byte> buf))
            {
                return (sbyte)buf.AsSpan((int)ms.Position)[0];
            }

            Read(stream, TempBuffer, 0, 1, false);
            return (sbyte)TempBuffer[0];
        }

        /// <summary>
        /// Read signed 8-bit value from stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public sbyte ReadS8(long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream ms && ms.TryGetBuffer(out ArraySegment<byte> buf))
            {
                return (sbyte)buf.AsSpan((int)offset)[0];
            }

            Read(stream, offset, TempBuffer, 0, 1, false);
            return (sbyte)TempBuffer[0];
        }

        /// <summary>
        /// Read signed 8-bit value from span at the specified offset
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="offset">Offset to read from</param>
        /// <returns>Value</returns>
        public sbyte GetS8(ReadOnlySpan<byte> span, int offset = 0) => (sbyte)span[offset];

        /// <summary>
        /// Read signed 8-bit value from memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="offset">Offset to read from</param>
        public sbyte GetMS8(ReadOnlyMemory<byte> memory, int offset = 0) => GetS8(memory.Span, offset);

        /// <summary>
        /// Write signed 8-bit value to span at the specified offset
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public sbyte SetS8(Span<byte> span, sbyte value, int offset = 0) => (sbyte)(span[offset] = (byte)value);

        /// <summary>
        /// Write signed 8-bit value to memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public sbyte SetMS8(Memory<byte> memory, sbyte value, int offset = 0) => SetS8(memory.Span, value, offset);

        /// <summary>
        /// Read unsigned 8-bit value from stream
        /// </summary>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public byte ReadU8(Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream ms && ms.TryGetBuffer(out ArraySegment<byte> buf))
            {
                return buf.AsSpan((int)ms.Position)[0];
            }

            Read(stream, TempBuffer, 0, 1, false);
            return TempBuffer[0];
        }

        /// <summary>
        /// Read unsigned 8-bit value from stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public byte ReadU8(long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream ms && ms.TryGetBuffer(out ArraySegment<byte> buf))
            {
                return buf.AsSpan((int)offset)[0];
            }

            Read(stream, offset, TempBuffer, 0, 1, false);
            return TempBuffer[0];
        }

        /// <summary>
        /// Read unsigned 8-bit value from span at the specified offset
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="offset">Offset to read from</param>
        /// <returns>Value</returns>
        public byte GetU8(ReadOnlySpan<byte> span, int offset = 0) => span[offset];

        /// <summary>
        /// Read unsigned 8-bit value from memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="offset">Offset to read from</param>
        public byte GetMU8(ReadOnlyMemory<byte> memory, int offset = 0) => GetU8(memory.Span, offset);

        /// <summary>
        /// Write unsigned 8-bit value to span at the specified offset
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public byte SetU8(Span<byte> span, byte value, int offset = 0) => span[offset] = value;

        /// <summary>
        /// Write unsigned 8-bit value to memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public byte SetMU8(Memory<byte> memory, byte value, int offset = 0) => SetU8(memory.Span, value, offset);

        #region Static endian

        /// <summary>
        /// Read signed 16-bit value from span at the specified offset
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to read from</param>
        /// <returns>Value</returns>
        public static short GetS16(ReadOnlySpan<byte> span, bool littleEndian, int offset = 0)
        {
            Span<byte> span2 = stackalloc byte[2];
            span.Slice(offset, 2).CopyTo(span2);
            short value = MemoryMarshal.Cast<byte, short>(span2)[0];
            return littleEndian ^ BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
        }

        /// <summary>
        /// Read signed 16-bit value from memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to read from</param>
        public static short GetMS16(ReadOnlyMemory<byte> memory, bool littleEndian, int offset = 0) =>
            GetS16(memory.Span, littleEndian, offset);

        /// <summary>
        /// Write signed 16-bit value to span at the specified offset
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="value">Value</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public static short SetS16(Span<byte> span, short value, bool littleEndian, int offset = 0)
        {
            if (littleEndian ^ BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
            Span<byte> span2 = stackalloc byte[2];
            MemoryMarshal.Cast<byte, short>(span2)[0] = value;
            span2.CopyTo(span.Slice(offset, 2));
            return value;
        }

        /// <summary>
        /// Write signed 16-bit value to memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to write to</param>
        /// <param name="value">Value</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public static short SetMS16(Memory<byte> memory, short value, bool littleEndian, int offset = 0) =>
            SetS16(memory.Span, value, littleEndian, offset);

        /// <summary>
        /// Read unsigned 16-bit value from span at the specified offset
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to read from</param>
        /// <returns>Value</returns>
        public static ushort GetU16(ReadOnlySpan<byte> span, bool littleEndian, int offset = 0)
        {
            Span<byte> span2 = stackalloc byte[2];
            span.Slice(offset, 2).CopyTo(span2);
            ushort value = MemoryMarshal.Cast<byte, ushort>(span2)[0];
            return littleEndian ^ BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
        }

        /// <summary>
        /// Read unsigned 16-bit value from memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to read from</param>
        public static ushort GetMU16(ReadOnlyMemory<byte> memory, bool littleEndian, int offset = 0) =>
            GetU16(memory.Span, littleEndian, offset);

        /// <summary>
        /// Write unsigned 16-bit value to span at the specified offset
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="value">Value</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public static ushort SetU16(Span<byte> span, ushort value, bool littleEndian, int offset = 0)
        {
            if (littleEndian ^ BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
            Span<byte> span2 = stackalloc byte[2];
            MemoryMarshal.Cast<byte, ushort>(span2)[0] = value;
            span2.CopyTo(span.Slice(offset, 2));
            return value;
        }

        /// <summary>
        /// Write unsigned 16-bit value to memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to write to</param>
        /// <param name="value">Value</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public static ushort SetMU16(Memory<byte> memory, ushort value, bool littleEndian, int offset = 0) =>
            SetU16(memory.Span, value, littleEndian, offset);

        /// <summary>
        /// Read signed 32-bit value from span at the specified offset
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to read from</param>
        /// <returns>Value</returns>
        public static int GetS32(ReadOnlySpan<byte> span, bool littleEndian, int offset = 0)
        {
            Span<byte> span2 = stackalloc byte[4];
            span.Slice(offset, 4).CopyTo(span2);
            int value = MemoryMarshal.Cast<byte, int>(span2)[0];
            return littleEndian ^ BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
        }

        /// <summary>
        /// Read unsigned 32-bit value from memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to read from</param>
        /// <returns>Value</returns>
        public static int GetMS32(ReadOnlyMemory<byte> memory, bool littleEndian, int offset = 0) =>
            GetS32(memory.Span, littleEndian, offset);

        /// <summary>
        /// Write signed 32-bit value to span at the specified offset
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="value">Value</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public static int SetS32(Span<byte> span, int value, bool littleEndian, int offset = 0)
        {
            if (littleEndian ^ BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
            Span<byte> span2 = stackalloc byte[4];
            MemoryMarshal.Cast<byte, int>(span2)[0] = value;
            span2.CopyTo(span.Slice(offset, 4));
            return value;
        }

        /// <summary>
        /// Write signed 32-bit value to memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to write to</param>
        /// <param name="value">Value</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public static int SetMS32(Memory<byte> memory, int value, bool littleEndian, int offset = 0) =>
            SetS32(memory.Span, value, littleEndian, offset);

        /// <summary>
        /// Read unsigned 32-bit value from span at the specified offset
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to read from</param>
        /// <returns>Value</returns>
        public static uint GetU32(ReadOnlySpan<byte> span, bool littleEndian, int offset = 0)
        {
            Span<byte> span2 = stackalloc byte[4];
            span.Slice(offset, 4).CopyTo(span2);
            uint value = MemoryMarshal.Cast<byte, uint>(span2)[0];
            return littleEndian ^ BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
        }

        /// <summary>
        /// Read unsigned 32-bit value from memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to read from</param>
        public static uint GetMU32(ReadOnlyMemory<byte> memory, bool littleEndian, int offset = 0) =>
            GetU32(memory.Span, littleEndian, offset);

        /// <summary>
        /// Write unsigned 32-bit value to span at the specified offset
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="value">Value</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public static uint SetU32(Span<byte> span, uint value, bool littleEndian, int offset = 0)
        {
            if (littleEndian ^ BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
            Span<byte> span2 = stackalloc byte[4];
            MemoryMarshal.Cast<byte, uint>(span2)[0] = value;
            span2.CopyTo(span.Slice(offset, 4));
            return value;
        }

        /// <summary>
        /// Write unsigned 32-bit value to memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to write to</param>
        /// <param name="value">Value</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public static uint SetMU32(Memory<byte> memory, uint value, bool littleEndian, int offset = 0) =>
            SetU32(memory.Span, value, littleEndian, offset);

        /// <summary>
        /// Read signed 64-bit value from span at the specified offset
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to read from</param>
        /// <returns>Value</returns>
        public static long GetS64(ReadOnlySpan<byte> span, bool littleEndian, int offset = 0)
        {
            Span<byte> span2 = stackalloc byte[8];
            span.Slice(offset, 8).CopyTo(span2);
            long value = MemoryMarshal.Cast<byte, long>(span2)[0];
            return littleEndian ^ BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
        }

        /// <summary>
        /// Read signed 64-bit value from memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to read from</param>
        public static long GetMS64(ReadOnlyMemory<byte> memory, bool littleEndian, int offset = 0) =>
            GetS64(memory.Span, littleEndian, offset);

        /// <summary>
        /// Write signed 64-bit value to span at the specified offset
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="value">Value</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to write to</param>
        public static long SetS64(Span<byte> span, long value, bool littleEndian, int offset = 0)
        {
            if (littleEndian ^ BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
            Span<byte> span2 = stackalloc byte[8];
            MemoryMarshal.Cast<byte, long>(span2)[0] = value;
            span2.CopyTo(span.Slice(offset, 8));
            return value;
        }

        /// <summary>
        /// Write signed 64-bit value to memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to write to</param>
        /// <param name="value">Value</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public static long SetMS64(Memory<byte> memory, long value, bool littleEndian, int offset = 0) =>
            SetS64(memory.Span, value, littleEndian, offset);

        /// <summary>
        /// Read unsigned 64-bit value from span at the specified offset
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to read from</param>
        /// <returns>Value</returns>
        public static ulong GetU64(ReadOnlySpan<byte> span, bool littleEndian, int offset = 0)
        {
            Span<byte> span2 = stackalloc byte[8];
            span.Slice(offset, 8).CopyTo(span2);
            ulong value = MemoryMarshal.Cast<byte, ulong>(span2)[0];
            return littleEndian ^ BitConverter.IsLittleEndian ? BinaryPrimitives.ReverseEndianness(value) : value;
        }

        /// <summary>
        /// Read unsigned 64-bit value from memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to read from</param>
        public static ulong GetMU64(ReadOnlyMemory<byte> memory, bool littleEndian, int offset = 0) =>
            GetU64(memory.Span, littleEndian, offset);

        /// <summary>
        /// Write unsigned 64-bit value to span at the specified offset
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="value">Value</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public static ulong SetU64(Span<byte> span, ulong value, bool littleEndian, int offset = 0)
        {
            if (littleEndian ^ BitConverter.IsLittleEndian) value = BinaryPrimitives.ReverseEndianness(value);
            Span<byte> span2 = stackalloc byte[8];
            MemoryMarshal.Cast<byte, ulong>(span2)[0] = value;
            span2.CopyTo(span.Slice(offset, 8));
            return value;
        }

        /// <summary>
        /// Write unsigned 64-bit value to memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to write to</param>
        /// <param name="value">Value</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public static ulong SetMU64(Memory<byte> memory, ulong value, bool littleEndian, int offset = 0) =>
            SetU64(memory.Span, value, littleEndian, offset);

        /// <summary>
        /// Convert endianness of signed 16-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        public static void ConvertS16Array(Span<short> span, bool littleEndian)
        {
            if (littleEndian == BitConverter.IsLittleEndian) return;

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = BinaryPrimitives.ReverseEndianness(span[i]);
            }
        }

        /// <summary>
        /// Convert endianness of signed 16-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        public static void ConvertS16Array(Span<byte> span, bool littleEndian) => ConvertS16Array(MemoryMarshal.Cast<byte, short>(span), littleEndian);

        /// <summary>
        /// Convert endianness of unsigned 16-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        public static void ConvertU16Array(Span<ushort> span, bool littleEndian)
        {
            if (littleEndian == BitConverter.IsLittleEndian) return;

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = BinaryPrimitives.ReverseEndianness(span[i]);
            }
        }

        /// <summary>
        /// Convert endianness of unsigned 16-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        public static void ConvertU16Array(Span<byte> span, bool littleEndian) => ConvertU16Array(MemoryMarshal.Cast<byte, ushort>(span), littleEndian);

        /// <summary>
        /// Convert endianness of signed 32-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        public static void ConvertS32Array(Span<int> span, bool littleEndian)
        {
            if (littleEndian == BitConverter.IsLittleEndian) return;

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = BinaryPrimitives.ReverseEndianness(span[i]);
            }
        }

        /// <summary>
        /// Convert endianness of signed 32-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        public static void ConvertS32Array(Span<byte> span, bool littleEndian) => ConvertS32Array(MemoryMarshal.Cast<byte, int>(span), littleEndian);

        /// <summary>
        /// Convert endianness of unsigned 32-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        public static void ConvertU32Array(Span<uint> span, bool littleEndian)
        {
            if (littleEndian == BitConverter.IsLittleEndian) return;

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = BinaryPrimitives.ReverseEndianness(span[i]);
            }
        }

        /// <summary>
        /// Convert endianness of unsigned 32-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        public static void ConvertU32Array(Span<byte> span, bool littleEndian) => ConvertU32Array(MemoryMarshal.Cast<byte, uint>(span), littleEndian);

        /// <summary>
        /// Convert endianness of signed 64-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        public static void ConvertS64Array(Span<long> span, bool littleEndian)
        {
            if (littleEndian == BitConverter.IsLittleEndian) return;

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = BinaryPrimitives.ReverseEndianness(span[i]);
            }
        }

        /// <summary>
        /// Convert endianness of signed 64-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        public static void ConvertS64Array(Span<byte> span, bool littleEndian) => ConvertS64Array(MemoryMarshal.Cast<byte, long>(span), littleEndian);

        /// <summary>
        /// Convert endianness of unsigned 64-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        public static void ConvertU64Array(Span<ulong> span, bool littleEndian)
        {
            if (littleEndian == BitConverter.IsLittleEndian) return;

            for (int i = 0; i < span.Length; i++)
            {
                span[i] = BinaryPrimitives.ReverseEndianness(span[i]);
            }
        }

        /// <summary>
        /// Convert endianness of unsigned 64-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        public static void ConvertU64Array(Span<byte> span, bool littleEndian) => ConvertU64Array(MemoryMarshal.Cast<byte, ulong>(span), littleEndian);

        #endregion

        #region Instance endian

        /// <summary>
        /// Read signed 16-bit value from stream
        /// </summary>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public short ReadS16(Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream)
            {
                Read(stream, 2, out Span<byte> span2, false);
                return GetS16(span2);
            }

            Span<byte> span = stackalloc byte[2];
            Read(stream, span, false);
            return GetS16(span);
        }

        /// <summary>
        /// Read signed 16-bit value from stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public short ReadS16(long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream)
            {
                Read(stream, offset, 2, out Span<byte> span2, false);
                return GetS16(span2);
            }

            Span<byte> span = stackalloc byte[2];
            Read(stream, offset, span, false);
            return GetS16(span);
        }

        /// <summary>
        /// Read signed 16-bit value from span at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public short GetS16(ReadOnlySpan<byte> span, int offset = 0) => GetS16(span, LittleEndian, offset);

        /// <summary>
        /// Read signed 16-bit value from memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="offset">Offset to read from</param>
        public short GetMS16(ReadOnlyMemory<byte> memory, int offset = 0) => GetS16(memory.Span, LittleEndian, offset);

        /// <summary>
        /// Write signed 16-bit value to span at the specified offset
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public short SetS16(Span<byte> span, short value, int offset = 0) => SetS16(span, value, LittleEndian, offset);

        /// <summary>
        /// Write signed 16-bit value to memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public short SetMS16(Memory<byte> memory, short value, int offset = 0) =>
            SetS16(memory.Span, value, LittleEndian, offset);

        /// <summary>
        /// Read unsigned 16-bit value from stream
        /// </summary>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public ushort ReadU16(Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream)
            {
                Read(stream, 2, out Span<byte> span2, false);
                return GetU16(span2);
            }

            Span<byte> span = stackalloc byte[2];
            Read(stream, span, false);
            return GetU16(span);
        }

        /// <summary>
        /// Read unsigned 16-bit value from stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public ushort ReadU16(long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream)
            {
                Read(stream, offset, 2, out Span<byte> span2, false);
                return GetU16(span2);
            }

            Span<byte> span = stackalloc byte[2];
            Read(stream, offset, span, false);
            return GetU16(span);
        }

        /// <summary>
        /// Read unsigned 16-bit value from span at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public ushort GetU16(ReadOnlySpan<byte> span, int offset = 0) => GetU16(span, LittleEndian, offset);

        /// <summary>
        /// Read unsigned 16-bit value from memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="offset">Offset to read from</param>
        public ushort GetMU16(ReadOnlyMemory<byte> memory, int offset = 0) => GetU16(memory.Span, LittleEndian, offset);

        /// <summary>
        /// Write unsigned 16-bit value to span at the specified offset
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public ushort SetU16(Span<byte> span, ushort value, int offset = 0) =>
            SetU16(span, value, LittleEndian, offset);

        /// <summary>
        /// Write unsigned 16-bit value to memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public ushort SetMU16(Memory<byte> memory, ushort value, int offset = 0) =>
            SetU16(memory.Span, value, LittleEndian, offset);

        /// <summary>
        /// Read signed 32-bit value from stream
        /// </summary>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public int ReadS32(Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream)
            {
                Read(stream, 4, out Span<byte> span2, false);
                return GetS32(span2);
            }

            Span<byte> span = stackalloc byte[4];
            Read(stream, span, false);
            return GetS32(span);
        }

        /// <summary>
        /// Read signed 32-bit value from stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public int ReadS32(long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream)
            {
                Read(stream, offset, 4, out Span<byte> span2, false);
                return GetS32(span2);
            }

            Span<byte> span = stackalloc byte[4];
            Read(stream, offset, span, false);
            return GetS32(span);
        }

        /// <summary>
        /// Read signed 32-bit value from span at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public int GetS32(ReadOnlySpan<byte> span, int offset = 0) => GetS32(span, LittleEndian, offset);

        /// <summary>
        /// Read unsigned 32-bit value from memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="offset">Offset to read from</param>
        /// <returns>Value</returns>
        public int GetMS32(ReadOnlyMemory<byte> memory, int offset = 0) => GetS32(memory.Span, LittleEndian, offset);

        /// <summary>
        /// Write signed 32-bit value to span at the specified offset
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public int SetS32(Span<byte> span, int value, int offset = 0) => SetS32(span, value, LittleEndian, offset);

        /// <summary>
        /// Write signed 32-bit value to memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public int SetMS32(Memory<byte> memory, int value, int offset = 0) =>
            SetS32(memory.Span, value, LittleEndian, offset);

        /// <summary>
        /// Read unsigned 32-bit value from stream
        /// </summary>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public uint ReadU32(Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream)
            {
                Read(stream, 4, out Span<byte> span2, false);
                return GetU32(span2);
            }

            Span<byte> span = stackalloc byte[4];
            Read(stream, span, false);
            return GetU32(span);
        }

        /// <summary>
        /// Read unsigned 32-bit value from stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public uint ReadU32(long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream)
            {
                Read(stream, offset, 4, out Span<byte> span2, false);
                return GetU32(span2);
            }

            Span<byte> span = stackalloc byte[4];
            Read(stream, offset, span, false);
            return GetU32(span);
        }

        /// <summary>
        /// Read unsigned 32-bit value from span at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public uint GetU32(ReadOnlySpan<byte> span, int offset = 0) => GetU32(span, LittleEndian, offset);

        /// <summary>
        /// Read unsigned 32-bit value from memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="offset">Offset to read from</param>
        public uint GetMU32(ReadOnlyMemory<byte> memory, int offset = 0) => GetU32(memory.Span, LittleEndian, offset);

        /// <summary>
        /// Write unsigned 32-bit value to span at the specified offset
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public uint SetU32(Span<byte> span, uint value, int offset = 0) => SetU32(span, value, LittleEndian, offset);

        /// <summary>
        /// Write unsigned 32-bit value to memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public uint SetMU32(Memory<byte> memory, uint value, int offset = 0) =>
            SetU32(memory.Span, value, LittleEndian, offset);

        /// <summary>
        /// Read signed 64-bit value from stream
        /// </summary>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public long ReadS64(Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream)
            {
                Read(stream, 8, out Span<byte> span2, false);
                return GetS64(span2);
            }

            Span<byte> span = stackalloc byte[8];
            Read(stream, span, false);
            return GetS64(span);
        }

        /// <summary>
        /// Read signed 64-bit value from stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public long ReadS64(long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream)
            {
                Read(stream, offset, 8, out Span<byte> span2, false);
                return GetS64(span2);
            }

            Span<byte> span = stackalloc byte[8];
            Read(stream, offset, span, false);
            return GetS64(span);
        }

        /// <summary>
        /// Read signed 64-bit value from span at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public long GetS64(ReadOnlySpan<byte> span, int offset = 0) => GetS64(span, LittleEndian, offset);

        /// <summary>
        /// Read signed 64-bit value from memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="offset">Offset to read from</param>
        public long GetMS64(ReadOnlyMemory<byte> memory, int offset = 0) => GetS64(memory.Span, LittleEndian, offset);

        /// <summary>
        /// Write signed 64-bit value to span at the specified offset
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        public long SetS64(Span<byte> span, long value, int offset = 0) => SetS64(span, value, LittleEndian, offset);

        /// <summary>
        /// Write signed 64-bit value to memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public long SetMS64(Memory<byte> memory, long value, int offset = 0) =>
            SetS64(memory.Span, value, LittleEndian, offset);

        /// <summary>
        /// Read unsigned 64-bit value from stream
        /// </summary>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        /// <returns>Value</returns>
        public ulong ReadU64(Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream)
            {
                Read(stream, 8, out Span<byte> span2, false);
                return GetU64(span2);
            }

            Span<byte> span = stackalloc byte[8];
            Read(stream, span, false);
            return GetU64(span);
        }

        /// <summary>
        /// Read unsigned 64-bit value from stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public ulong ReadU64(long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream)
            {
                Read(stream, offset, 8, out Span<byte> span2, false);
                return GetU64(span2);
            }

            Span<byte> span = stackalloc byte[8];
            Read(stream, offset, span, false);
            return GetU64(span);
        }

        /// <summary>
        /// Read unsigned 64-bit value from span at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public ulong GetU64(ReadOnlySpan<byte> span, int offset = 0) => GetU64(span, LittleEndian, offset);

        /// <summary>
        /// Read unsigned 64-bit value from memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="offset">Offset to read from</param>
        public ulong GetMU64(ReadOnlyMemory<byte> memory, int offset = 0) => GetU64(memory.Span, LittleEndian, offset);

        /// <summary>
        /// Write unsigned 64-bit value to span at the specified offset
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public ulong SetU64(Span<byte> span, ulong value, int offset = 0) => SetU64(span, value, LittleEndian, offset);

        /// <summary>
        /// Write unsigned 64-bit value to memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public ulong SetMU64(Memory<byte> memory, ulong value, int offset = 0) =>
            SetU64(memory.Span, value, LittleEndian, offset);

        /// <summary>
        /// Convert endianness of signed 16-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertS16Array(Span<short> span) => ConvertS16Array(span, LittleEndian);

        /// <summary>
        /// Convert endianness of signed 16-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertS16Array(Span<byte> span) => ConvertS16Array(MemoryMarshal.Cast<byte, short>(span), LittleEndian);

        /// <summary>
        /// Convert endianness of unsigned 16-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertU16Array(Span<ushort> span) => ConvertU16Array(span, LittleEndian);

        /// <summary>
        /// Convert endianness of unsigned 16-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertU16Array(Span<byte> span) => ConvertU16Array(MemoryMarshal.Cast<byte, ushort>(span), LittleEndian);

        /// <summary>
        /// Convert endianness of signed 32-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertS32Array(Span<int> span) => ConvertS32Array(span, LittleEndian);

        /// <summary>
        /// Convert endianness of signed 32-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertS32Array(Span<byte> span) => ConvertS32Array(MemoryMarshal.Cast<byte, int>(span), LittleEndian);

        /// <summary>
        /// Convert endianness of unsigned 32-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertU32Array(Span<uint> span) => ConvertU32Array(span, LittleEndian);

        /// <summary>
        /// Convert endianness of unsigned 32-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertU32Array(Span<byte> span) => ConvertU32Array(MemoryMarshal.Cast<byte, uint>(span), LittleEndian);

        /// <summary>
        /// Convert endianness of signed 64-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertS64Array(Span<long> span) => ConvertS64Array(span, LittleEndian);

        /// <summary>
        /// Convert endianness of signed 64-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertS64Array(Span<byte> span) => ConvertS64Array(MemoryMarshal.Cast<byte, long>(span), LittleEndian);

        /// <summary>
        /// Convert endianness of unsigned 64-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertU64Array(Span<ulong> span) => ConvertU64Array(span, LittleEndian);

        /// <summary>
        /// Convert endianness of unsigned 64-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertU64Array(Span<byte> span) => ConvertU64Array(MemoryMarshal.Cast<byte, ulong>(span), LittleEndian);

        #endregion


        /// <summary>
        /// Read 16-bit float value from stream
        /// </summary>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public float ReadHalf(Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream)
            {
                Read(stream, 2, out Span<byte> span2, false);
                return GetSingle(span2);
            }

            Span<byte> span = stackalloc byte[2];
            Read(stream, span, false);
            return GetSingle(span);
        }

        /// <summary>
        /// Read 16-bit float value from stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public float ReadHalf(long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream)
            {
                Read(stream, offset, 2, out Span<byte> span2, false);
                return GetSingle(span2);
            }

            Span<byte> span = stackalloc byte[2];
            Read(stream, offset, span, false);
            return GetSingle(span);
        }

        /// <summary>
        /// Read 16-bit float value from span at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public float GetHalf(ReadOnlySpan<byte> span, int offset = 0)
        {
            Span<byte> span2 = stackalloc byte[2];
            span.Slice(offset, 2).CopyTo(span2);
            return HalfHelper.HalfToSingle(MemoryMarshal.Read<ushort>(span2));
        }

        /// <summary>
        /// Read 16-bit float value from memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="offset">Offset to read from</param>
        public float GetMHalf(ReadOnlyMemory<byte> memory, int offset = 0) => GetHalf(memory.Span, offset);

        /// <summary>
        /// Write 16-bit float value to span at the specified offset
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public float SetHalf(Span<byte> span, float value, int offset = 0)
        {
            Span<byte> span2 = stackalloc byte[2];
            MemoryMarshal.Cast<byte, ushort>(span2)[0] = HalfHelper.SingleToHalf(value);
            span2.CopyTo(span.Slice(offset, 2));
            return value;
        }

        /// <summary>
        /// Write 16-bit float value to memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public float SetMHalf(Memory<byte> memory, float value, int offset = 0) => SetHalf(memory.Span, value, offset);

        /// <summary>
        /// Read 32-bit float value from stream
        /// </summary>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public float ReadSingle(Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream)
            {
                Read(stream, 4, out Span<byte> span2, false);
                return GetSingle(span2);
            }

            Span<byte> span = stackalloc byte[4];
            Read(stream, span, false);
            return GetSingle(span);
        }

        /// <summary>
        /// Read 32-bit float value from stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public float ReadSingle(long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream)
            {
                Read(stream, offset, 4, out Span<byte> span2, false);
                return GetSingle(span2);
            }

            Span<byte> span = stackalloc byte[4];
            Read(stream, offset, span, false);
            return GetSingle(span);
        }

        /// <summary>
        /// Read 32-bit float value from span at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public float GetSingle(ReadOnlySpan<byte> span, int offset = 0)
        {
            Span<byte> span2 = stackalloc byte[4];
            span.Slice(offset, 4).CopyTo(span2);
            return MemoryMarshal.Read<float>(span2);
        }

        /// <summary>
        /// Read 32-bit float value from memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="offset">Offset to read from</param>
        public float GetMSingle(ReadOnlyMemory<byte> memory, int offset = 0) => GetSingle(memory.Span, offset);

        /// <summary>
        /// Write 32-bit float value to span at the specified offset
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public float SetSingle(Span<byte> span, float value, int offset = 0)
        {
            Span<byte> span2 = stackalloc byte[4];
            MemoryMarshal.Cast<byte, float>(span2)[0] = value;
            span2.CopyTo(span.Slice(offset, 4));
            return value;
        }

        /// <summary>
        /// Write 32-bit float value to memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public float SetMSingle(Memory<byte> memory, float value, int offset = 0) =>
            SetSingle(memory.Span, value, offset);

        /// <summary>
        /// Read 64-bit float value from stream
        /// </summary>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public double ReadDouble(Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Span<byte> span = stackalloc byte[8];
            Read(stream, span, false);
            return GetDouble(span);
        }

        /// <summary>
        /// Read 64-bit float value from stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public double ReadDouble(long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            if (stream is MemoryStream)
            {
                Read(stream, offset, 8, out Span<byte> span2, false);
                return GetDouble(span2);
            }

            Span<byte> span = stackalloc byte[8];
            Read(stream, offset, span, false);
            return GetDouble(span);
        }

        /// <summary>
        /// Read 64-bit float value from span at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public static double GetDouble(ReadOnlySpan<byte> span, int offset = 0)
        {
            Span<byte> span2 = stackalloc byte[8];
            span.Slice(offset, 8).CopyTo(span2);
            return MemoryMarshal.Read<double>(span2);
        }

        /// <summary>
        /// Read 64-bit float value from memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="offset">Offset to read from</param>
        public double GetMDouble(ReadOnlyMemory<byte> memory, int offset = 0) => GetDouble(memory.Span, offset);

        /// <summary>
        /// Write 32-bit float value to span at the specified offset
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public double SetDouble(Span<byte> span, double value, int offset = 0)
        {
            Span<byte> span2 = stackalloc byte[8];
            MemoryMarshal.Cast<byte, double>(span2)[0] = value;
            span2.CopyTo(span.Slice(offset, 8));
            return value;
        }

        /// <summary>
        /// Write 32-bit float value to memory at the specified offset
        /// </summary>
        /// <param name="memory">Memory to write to</param>
        /// <param name="value">Value</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Value</returns>
        public double SetMDouble(Memory<byte> memory, double value, int offset = 0) =>
            SetDouble(memory.Span, value, offset);

        /// <summary>
        /// Read array of signed 8-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS8Array(Span<sbyte> span, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, MemoryMarshal.Cast<sbyte, byte>(span), false);
        }

        /// <summary>
        /// Read array of signed 8-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS8Array(Span<sbyte> span, long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, offset, MemoryMarshal.Cast<sbyte, byte>(span), false);
        }

        /// <summary>
        /// Read array of signed 8-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS8Array(Span<byte> span, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, span, false);
        }

        /// <summary>
        /// Read array of signed 8-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS8Array(Span<byte> span, long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, offset, span, false);
        }

        /// <summary>
        /// Read array of signed 8-bit values from stream
        /// </summary>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public sbyte[] ReadS8Array(int count, Stream? stream = null)
        {
            sbyte[] arr = new sbyte[count];
            Span<byte> span = MemoryMarshal.Cast<sbyte, byte>(arr);
            ReadS8Array(span, stream);
            return arr;
        }

        /// <summary>
        /// Read array of signed 8-bit values from stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public sbyte[] ReadS8Array(long offset, int count, Stream? stream = null)
        {
            sbyte[] arr = new sbyte[count];
            Span<byte> span = MemoryMarshal.Cast<sbyte, byte>(arr);
            ReadS8Array(span, offset, stream);
            return arr;
        }

        //

        /// <summary>
        /// Read array of signed 8-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU8Array(Span<byte> span, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, span, false);
        }

        /// <summary>
        /// Read array of signed 8-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU8Array(Span<byte> span, long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, offset, span, false);
        }

        /// <summary>
        /// Read array of signed 8-bit values from stream
        /// </summary>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public byte[] ReadU8Array(int count, Stream? stream = null)
        {
            byte[] arr = new byte[count];
            ReadU8Array(arr, stream);
            return arr;
        }

        /// <summary>
        /// Read array of signed 8-bit values from stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public byte[] ReadU8Array(long offset, int count, Stream? stream = null)
        {
            byte[] arr = new byte[count];
            ReadU8Array(arr, offset, stream);
            return arr;
        }

        /// <summary>
        /// Read array of signed 16-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS16Array(Span<short> span, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, MemoryMarshal.Cast<short, byte>(span), false);
            ConvertS16Array(span);
        }

        /// <summary>
        /// Read array of signed 16-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS16Array(Span<short> span, long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, offset, MemoryMarshal.Cast<short, byte>(span), false);
            ConvertS16Array(span);
        }

        /// <summary>
        /// Read array of signed 16-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS16Array(Span<byte> span, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, span, false);
            ConvertS16Array(span);
        }

        /// <summary>
        /// Read array of signed 16-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS16Array(Span<byte> span, long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, offset, span, false);
            ConvertS16Array(span);
        }

        /// <summary>
        /// Read array of signed 16-bit values from stream
        /// </summary>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public short[] ReadS16Array(int count, Stream? stream = null)
        {
            short[] arr = new short[count];
            Span<byte> span = MemoryMarshal.Cast<short, byte>(arr);
            ReadS16Array(span, stream);
            return arr;
        }

        /// <summary>
        /// Read array of signed 16-bit values from stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public short[] ReadS16Array(long offset, int count, Stream? stream = null)
        {
            short[] arr = new short[count];
            Span<byte> span = MemoryMarshal.Cast<short, byte>(arr);
            ReadS16Array(span, offset, stream);
            return arr;
        }

        /// <summary>
        /// Read array of unsigned 16-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU16Array(Span<ushort> span, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, MemoryMarshal.Cast<ushort, byte>(span), false);
            ConvertU16Array(span);
        }

        /// <summary>
        /// Read array of unsigned 16-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU16Array(Span<ushort> span, long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, offset, MemoryMarshal.Cast<ushort, byte>(span), false);
            ConvertU16Array(span);
        }

        /// <summary>
        /// Read array of unsigned 16-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU16Array(Span<byte> span, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, span, false);
            ConvertU16Array(span);
        }

        /// <summary>
        /// Read array of unsigned 16-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU16Array(Span<byte> span, long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, offset, span, false);
            ConvertU16Array(span);
        }

        /// <summary>
        /// Read array of unsigned 16-bit values from stream
        /// </summary>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public ushort[] ReadU16Array(int count, Stream? stream = null)
        {
            ushort[] arr = new ushort[count];
            Span<byte> span = MemoryMarshal.Cast<ushort, byte>(arr);
            ReadU16Array(span, stream);
            return arr;
        }

        /// <summary>
        /// Read array of unsigned 16-bit values from stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public ushort[] ReadU16Array(long offset, int count, Stream? stream = null)
        {
            ushort[] arr = new ushort[count];
            Span<byte> span = MemoryMarshal.Cast<ushort, byte>(arr);
            ReadU16Array(span, offset, stream);
            return arr;
        }

        /// <summary>
        /// Read array of signed 32-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS32Array(Span<int> span, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, MemoryMarshal.Cast<int, byte>(span), false);
            ConvertS32Array(span);
        }

        /// <summary>
        /// Read array of signed 32-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS32Array(Span<int> span, long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, offset, MemoryMarshal.Cast<int, byte>(span), false);
            ConvertS32Array(span);
        }

        /// <summary>
        /// Read array of signed 32-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS32Array(Span<byte> span, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, span, false);
            ConvertS32Array(span);
        }

        /// <summary>
        /// Read array of signed 32-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS32Array(Span<byte> span, long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, offset, span, false);
            ConvertS32Array(span);
        }

        /// <summary>
        /// Read array of signed 32-bit values from stream
        /// </summary>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public int[] ReadS32Array(int count, Stream? stream = null)
        {
            int[] arr = new int[count];
            Span<byte> span = MemoryMarshal.Cast<int, byte>(arr);
            ReadS32Array(span, stream);
            return arr;
        }

        /// <summary>
        /// Read array of signed 32-bit values from stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public int[] ReadS32Array(long offset, int count, Stream? stream = null)
        {
            int[] arr = new int[count];
            Span<byte> span = MemoryMarshal.Cast<int, byte>(arr);
            ReadS32Array(span, offset, stream);
            return arr;
        }

        /// <summary>
        /// Read array of unsigned 32-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU32Array(Span<uint> span, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, MemoryMarshal.Cast<uint, byte>(span), false);
            ConvertU32Array(span);
        }

        /// <summary>
        /// Read array of unsigned 32-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU32Array(Span<uint> span, long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, offset, MemoryMarshal.Cast<uint, byte>(span), false);
            ConvertU32Array(span);
        }

        /// <summary>
        /// Read array of unsigned 32-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU32Array(Span<byte> span, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, span, false);
            ConvertU32Array(span);
        }

        /// <summary>
        /// Read array of unsigned 32-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU32Array(Span<byte> span, long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, offset, span, false);
            ConvertU32Array(span);
        }

        /// <summary>
        /// Read array of unsigned 32-bit values from stream
        /// </summary>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public uint[] ReadU32Array(int count, Stream? stream = null)
        {
            uint[] arr = new uint[count];
            Span<byte> span = MemoryMarshal.Cast<uint, byte>(arr);
            ReadU32Array(span, stream);
            return arr;
        }

        /// <summary>
        /// Read array of unsigned 32-bit values from stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public uint[] ReadU32Array(long offset, int count, Stream? stream = null)
        {
            uint[] arr = new uint[count];
            Span<byte> span = MemoryMarshal.Cast<uint, byte>(arr);
            ReadU32Array(span, offset, stream);
            return arr;
        }

        /// <summary>
        /// Read array of signed 64-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS64Array(Span<long> span, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, MemoryMarshal.Cast<long, byte>(span), false);
            ConvertS64Array(span);
        }

        /// <summary>
        /// Read array of signed 64-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS64Array(Span<long> span, long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, offset, MemoryMarshal.Cast<long, byte>(span), false);
            ConvertS64Array(span);
        }

        /// <summary>
        /// Read array of signed 64-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS64Array(Span<byte> span, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, span, false);
            ConvertS64Array(span);
        }

        /// <summary>
        /// Read array of signed 64-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS64Array(Span<byte> span, long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, offset, span, false);
            ConvertS64Array(span);
        }

        /// <summary>
        /// Read array of signed 64-bit values from stream
        /// </summary>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public long[] ReadS64Array(int count, Stream? stream = null)
        {
            long[] arr = new long[count];
            Span<byte> span = MemoryMarshal.Cast<long, byte>(arr);
            ReadS64Array(span, stream);
            return arr;
        }

        /// <summary>
        /// Read array of signed 64-bit values from stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public long[] ReadS64Array(long offset, int count, Stream? stream = null)
        {
            long[] arr = new long[count];
            Span<byte> span = MemoryMarshal.Cast<long, byte>(arr);
            ReadS64Array(span, offset, stream);
            return arr;
        }

        /// <summary>
        /// Read array of unsigned 64-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU64Array(Span<ulong> span, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, MemoryMarshal.Cast<ulong, byte>(span), false);
            ConvertU64Array(span);
        }

        /// <summary>
        /// Read array of unsigned 64-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU64Array(Span<ulong> span, long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, offset, MemoryMarshal.Cast<ulong, byte>(span), false);
            ConvertU64Array(span);
        }

        /// <summary>
        /// Read array of unsigned 64-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU64Array(Span<byte> span, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, span, false);
            ConvertU64Array(span);
        }

        /// <summary>
        /// Read array of unsigned 64-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU64Array(Span<byte> span, long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, offset, span, false);
            ConvertU64Array(span);
        }

        /// <summary>
        /// Read array of unsigned 64-bit values from stream
        /// </summary>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public ulong[] ReadU64Array(int count, Stream? stream = null)
        {
            ulong[] arr = new ulong[count];
            Span<byte> span = MemoryMarshal.Cast<ulong, byte>(arr);
            ReadS16Array(span, stream);
            return arr;
        }

        /// <summary>
        /// Read array of unsigned 64-bit values from stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public ulong[] ReadU64Array(long offset, int count, Stream? stream = null)
        {
            ulong[] arr = new ulong[count];
            Span<byte> span = MemoryMarshal.Cast<ulong, byte>(arr);
            ReadS16Array(span, offset, stream);
            return arr;
        }

        /// <summary>
        /// Convert array of half-precision floating-point values to single-precision
        /// </summary>
        /// <param name="source">Source span</param>
        /// <param name="target">Target span</param>
        public static void ConvertHalfArrayToFloat(Span<byte> source, Span<float> target)
        {
            Span<ushort> span = MemoryMarshal.Cast<byte, ushort>(source);
            for (int i = 0; i < span.Length; i++)
            {
                target[i] = HalfHelper.HalfToSingle(span[i]);
            }
        }

        /// <summary>
        /// Convert array of single-precision floating-point values to half-precision
        /// </summary>
        /// <param name="source">Source span</param>
        /// <param name="target">Target span</param>
        public static void ConvertFloatArrayToHalf(Span<float> source, Span<byte> target)
        {
            Span<ushort> span = MemoryMarshal.Cast<byte, ushort>(target);
            for (int i = 0; i < source.Length; i++)
            {
                span[i] = HalfHelper.SingleToHalf(source[i]);
            }
        }

        /// <summary>
        /// Read array of half-precision floating-point values as single-precision from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadHalfArray(Span<float> span, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            byte[] arr = Shared.Rent(span.Length * 2);
            try
            {
                Span<byte> span2 = arr.AsSpan(0, span.Length * 2);
                Read(stream, span2, false);
                ConvertHalfArrayToFloat(span2, span);
            }
            finally
            {
                Shared.Return(arr);
            }
        }

        /// <summary>
        /// Read array of single-precision floating-point values as single-precision from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadHalfArray(Span<float> span, long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            byte[] arr = Shared.Rent(span.Length * 2);
            try
            {
                Span<byte> span2 = arr.AsSpan(0, span.Length * 2);
                Read(stream, offset, span2, false);
                ConvertHalfArrayToFloat(span2, span);
            }
            finally
            {
                Shared.Return(arr);
            }
        }

        /// <summary>
        /// Read array of single-precision floating-point values as single-precision from stream
        /// </summary>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public float[] ReadHalfArray(int count, Stream? stream = null)
        {
            float[] arr = new float[count];
            ReadHalfArray(arr, stream);
            return arr;
        }

        /// <summary>
        /// Read array of single-precision floating-point values as single-precision from stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public float[] ReadHalfArray(long offset, int count, Stream? stream = null)
        {
            float[] arr = new float[count];
            ReadHalfArray(arr, offset, stream);
            return arr;
        }

        /// <summary>
        /// Read array of single-precision floating-point values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadSingleArray(Span<float> span, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, MemoryMarshal.Cast<float, byte>(span), false);
        }

        /// <summary>
        /// Read array of single-precision floating-point values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadSingleArray(Span<float> span, long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, offset, MemoryMarshal.Cast<float, byte>(span), false);
        }

        /// <summary>
        /// Read array of single-precision floating-point values from stream
        /// </summary>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public float[] ReadSingleArray(int count, Stream? stream = null)
        {
            float[] arr = new float[count];
            ReadSingleArray(arr, stream);
            return arr;
        }

        /// <summary>
        /// Read array of single-precision floating-point values from stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public float[] ReadSingleArray(long offset, int count, Stream? stream = null)
        {
            float[] arr = new float[count];
            ReadSingleArray(arr, offset, stream);
            return arr;
        }

        /// <summary>
        /// Read array of double-precision floating-point values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadDoubleArray(Span<double> span, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, MemoryMarshal.Cast<double, byte>(span), false);
        }

        /// <summary>
        /// Read array of double-precision floating-point values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadDoubleArray(Span<double> span, long offset, Stream? stream = null)
        {
            stream ??= InputStream ?? throw new InvalidOperationException();
            Read(stream, offset, MemoryMarshal.Cast<double, byte>(span), false);
        }

        /// <summary>
        /// Read array of double-precision floating-point values from stream
        /// </summary>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public double[] ReadDoubleArray(int count, Stream? stream = null)
        {
            double[] arr = new double[count];
            ReadDoubleArray(arr, stream);
            return arr;
        }

        /// <summary>
        /// Read array of double-precision floating-point values from stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public double[] ReadDoubleArray(long offset, int count, Stream? stream = null)
        {
            double[] arr = new double[count];
            ReadDoubleArray(arr, offset, stream);
            return arr;
        }

        private static unsafe string DecodeSpan(ReadOnlySpan<byte> span, Encoding encoding)
        {
            if (span.Length == 0)
            {
                return string.Empty;
            }

            fixed (byte* spanFixed = &span.GetPinnableReference())
            {
                return encoding.GetString(spanFixed, span.Length);
            }
        }

        /// <summary>
        /// Read UTF-8 encoded string from stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="read">Number of bytes consumed by string (including null terminator)</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <param name="strict">If true, enforces ending stream position to original position + <paramref name="maxLength"/></param>
        /// <returns>Value</returns>
        public string ReadUtf8String(Stream stream, out int read, int maxLength = int.MaxValue, bool strict = false)
        {
            try
            {
                TempMs.SetLength(0);
                read = 0;
                do
                {
                    int v = stream.ReadByte();
                    read += v == -1 ? 0 : 1;
                    if (v == -1 || v == 0) break;
                    TempMs.WriteByte((byte)v);
                } while (read < maxLength);

                if (strict)
                {
                    Skip(maxLength - read, stream);
                }

                return DecodeSpan(TempMs.GetBuffer().AsSpan(0, (int)TempMs.Length), Encoding.UTF8);
            }
            finally
            {
                if (TempMs.Capacity > StringExcessiveCapacity)
                {
                    TempMs.Capacity = StringDefaultCapacity;
                }
            }
        }

        /// <summary>
        /// Read UTF-8 encoded string from stream at the specified offset
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="offset">Offset to read from</param>
        /// <param name="read">Number of bytes consumed by string (including null terminator)</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <param name="strict">If true, enforces ending stream position to original position + <paramref name="maxLength"/></param>
        /// <returns>Value</returns>
        public string ReadUtf8StringFromOffset(Stream stream, long offset, out int read, int maxLength = int.MaxValue,
            bool strict = false)
        {
            long position = stream.Position;
            try
            {
                stream.Position = offset;
                string str = ReadUtf8String(stream, out read, maxLength, strict);
                return str;
            }
            finally
            {
                stream.Position = position;
            }
        }

        /// <summary>
        /// Read UTF-8 encoded string from current file's input stream
        /// </summary>
        /// <param name="read">Number of bytes consumed by string (including null terminator)</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <param name="strict">If true, enforces ending stream position to original position + <paramref name="maxLength"/></param>
        /// <returns>Value</returns>
        public string ReadUtf8String(out int read, int maxLength = int.MaxValue, bool strict = false)
            => ReadUtf8String(InputStream ?? throw new InvalidOperationException(), out read, maxLength, strict);

        /// <summary>
        /// Read UTF-8 encoded string from current file's input stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="read">Number of bytes consumed by string (including null terminator)</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <param name="strict">If true, enforces ending stream position to original position + <paramref name="maxLength"/></param>
        /// <returns>Value</returns>
        public string ReadUtf8StringFromOffset(long offset, out int read, int maxLength = int.MaxValue,
            bool strict = false) =>
            ReadUtf8StringFromOffset(InputStream ?? throw new InvalidOperationException(), offset, out read, maxLength,
                strict);

        /// <summary>
        /// Read UTF-8 encoded string from span
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="read">Number of bytes consumed by string (including null terminator)</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <returns>Value</returns>
        public static string ReadUtf8String(ReadOnlySpan<byte> span, out int read, int maxLength = int.MaxValue)
        {
            int lim = Math.Min(span.Length, maxLength);
            int end = span.Slice(0, lim).IndexOf((byte)0);
            if (end == -1)
            {
                end = lim;
            }

            read = Math.Min(lim, end + 1);
            return DecodeSpan(span.Slice(0, end), Encoding.UTF8);
        }

        /// <summary>
        /// Read UTF-8 encoded string from memory
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="read">Number of bytes consumed by string (including null terminator)</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <returns>Value</returns>
        // ReSharper disable once MemberCanBeProtected.Global
        public static string ReadMUtf8String(ReadOnlyMemory<byte> memory, out int read, int maxLength = int.MaxValue)
            => ReadUtf8String(memory.Span, out read, maxLength);

        /// <summary>
        /// Read UTF-8 encoded string from memory
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="read">Number of bytes consumed by string (including null terminator)</param>
        /// <param name="offset">Offset to read from</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <returns>Value</returns>
        // ReSharper disable once MemberCanBeProtected.Global
        public static string ReadMUtf8StringFromOffset(ReadOnlyMemory<byte> memory, out int read, int offset = 0,
            int maxLength = int.MaxValue)
            => ReadUtf8String(memory.Span.Slice(offset), out read, maxLength);

        /// <summary>
        /// Read UTF-16 encoded string from stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="maxLength">Maximum string length (in bytes)</param>
        /// <param name="read">Number of bytes consumed by string (including null terminator)</param>
        /// <param name="strict">If true, enforces ending stream position to original position + <paramref name="maxLength"/></param>
        /// <returns>Value</returns>
        public string ReadUtf16String(Stream stream, out int read, int maxLength = int.MaxValue, bool strict = false)
        {
            try
            {
                TempMs.SetLength(0);
                read = 0;
                do
                {
                    int cc = Read(stream, TempBuffer, 0, 2);
                    read += cc;
                    if (cc != 2 || TempBuffer[0] == 0 && TempBuffer[1] == 0) break;
                    TempMs.Write(TempBuffer, 0, 2);
                } while (read < maxLength);

                if (strict)
                {
                    Skip(maxLength - read, stream);
                }

                return ReadUtf16String(TempMs.GetBuffer().AsSpan(0, (int)TempMs.Length), out _);
            }
            finally
            {
                if (TempMs.Capacity > StringExcessiveCapacity)
                {
                    TempMs.Capacity = StringDefaultCapacity;
                }
            }
        }

        /// <summary>
        /// Read UTF-16 encoded string from stream at the specified offset
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="offset">Offset to read from</param>
        /// <param name="read">Number of bytes consumed by string (including null terminator)</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <param name="strict">If true, enforces ending stream position to original position + <paramref name="maxLength"/></param>
        /// <returns>Value</returns>
        public string ReadUtf16StringFromOffset(Stream stream, long offset, out int read, int maxLength = int.MaxValue,
            bool strict = false)
        {
            long position = stream.Position;
            try
            {
                stream.Position = offset;
                string str = ReadUtf16String(stream, out read, maxLength, strict);
                return str;
            }
            finally
            {
                stream.Position = position;
            }
        }

        /// <summary>
        /// Read UTF-16 encoded string from current file's input stream
        /// </summary>
        /// <param name="read">Number of bytes consumed by string (including null terminator)</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <param name="strict">If true, enforces ending stream position to original position + <paramref name="maxLength"/></param>
        /// <returns>Value</returns>
        public string ReadUtf16String(out int read, int maxLength = int.MaxValue, bool strict = false)
            => ReadUtf16String(InputStream ?? throw new InvalidOperationException(), out read, maxLength, strict);

        /// <summary>
        /// Read UTF-16 encoded string from current file's input stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="read">Number of bytes consumed by string (including null terminator)</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <param name="strict">If true, enforces ending stream position to original position + <paramref name="maxLength"/></param>
        /// <returns>Value</returns>
        public string ReadUtf16StringFromOffset(long offset, out int read, int maxLength = int.MaxValue,
            bool strict = false) =>
            ReadUtf16StringFromOffset(InputStream ?? throw new InvalidOperationException(), offset, out read, maxLength,
                strict);


        /// <summary>
        /// Read UTF-16 encoded string from span
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="read">Number of bytes consumed by string (including null terminator)</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <returns>Value</returns>
        public static string ReadUtf16String(ReadOnlySpan<byte> span, out int read, int maxLength = int.MaxValue)
        {
            int lim = Math.Min(span.Length, maxLength);
            int end = -1;
            while (end + 2 < lim && (span[end + 1] != 0 || span[end + 2] != 0))
                end += 2;
            if (end == -1)
            {
                end = lim;
                read = lim;
            }
            else
            {
                end++;
                read = Math.Min(lim, end + 2);
            }

            bool big = span.Length >= 2 && span[0] == 0xFE && span[1] == 0xFF;
            bool bom = big || span.Length >= 2 && span[0] == 0xFF && span[1] == 0xFE;

            if (!bom && span.Length > 1)
            {
                const int numBytes = 16 * sizeof(char);
                const float threshold = 0.75f;
                int countAscii = 0, countTotal = 0, sl = span.Length;
                for (int i = 0; i < numBytes && i + 1 < sl; i += 2)
                {
                    if (span[i] == 0 && span[i + 1] < 0x80)
                    {
                        countAscii++;
                    }

                    countTotal++;
                }

                big = (float)countAscii / countTotal >= threshold;
            }

            return DecodeSpan(span.Slice(0, end), GetUtf16Encoding(big, bom));
        }

        /// <summary>
        /// Read UTF-16 encoded string from memory
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="read">Number of bytes consumed by string (including null terminator)</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <returns>Value</returns>
        // ReSharper disable once MemberCanBeProtected.Global
        public static string ReadMUtf16String(ReadOnlyMemory<byte> memory, out int read, int maxLength = int.MaxValue)
            => ReadUtf16String(memory.Span, out read, maxLength);

        /// <summary>
        /// Read UTF-16 encoded string from memory
        /// </summary>
        /// <param name="memory">Memory to read from</param>
        /// <param name="read">Number of bytes consumed by string (including null terminator)</param>
        /// <param name="offset">Offset to read from</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <returns>Value</returns>
        // ReSharper disable once MemberCanBeProtected.Global
        public static string ReadMUtf16StringFromOffset(ReadOnlyMemory<byte> memory, out int read, int offset = 0,
            int maxLength = int.MaxValue)
            => ReadUtf16String(memory.Span.Slice(offset), out read, maxLength);

        /// <summary>
        /// Apply AND to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">AND value</param>
        public static void ApplyAnd(Span<byte> span, byte value)
        {
#if NET5_0
            if (Avx2.IsSupported)
                ApplyAndAvx2(span, value);
            else if (Sse2.IsSupported)
                ApplyAndSse2(span, value);
            else if (AdvSimd.IsSupported)
                ApplyAndArm(span, value);
            else
                ApplyAndFallback(span, value);
#else
            ApplyAndFallback(span, value);
#endif
        }

        /// <summary>
        /// Apply AND to memory
        /// </summary>
        /// <param name="memory">Memory to modify</param>
        /// <param name="value">AND value</param>
        public static void ApplyMAnd(Memory<byte> memory, byte value) => ApplyAnd(memory.Span, value);

        /// <summary>
        /// Apply OR to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">AND value</param>
        public static void ApplyOr(Span<byte> span, byte value)
        {
#if NET5_0
            if (Avx2.IsSupported)
                ApplyOrAvx2(span, value);
            else if (Sse2.IsSupported)
                ApplyOrSse2(span, value);
            else if (AdvSimd.IsSupported)
                ApplyOrArm(span, value);
            else
                ApplyOrFallback(span, value);
#else
            ApplyOrFallback(span, value);
#endif
        }

        /// <summary>
        /// Apply OR to memory
        /// </summary>
        /// <param name="memory">Memory to modify</param>
        /// <param name="value">AND value</param>
        public static void ApplyMOr(Memory<byte> memory, byte value) => ApplyOr(memory.Span, value);

        /// <summary>
        /// Apply XOR to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">XOR value</param>
        public static void ApplyXor(Span<byte> span, byte value)
        {
#if NET5_0
            if (Avx2.IsSupported)
                ApplyXorAvx2(span, value);
            else if (Sse2.IsSupported)
                ApplyXorSse2(span, value);
            else if (AdvSimd.IsSupported)
                ApplyXorArm(span, value);
            else
                ApplyXorFallback(span, value);
#else
            ApplyXorFallback(span, value);
#endif
        }

        /// <summary>
        /// Apply XOR to memory
        /// </summary>
        /// <param name="memory">Memory to modify</param>
        /// <param name="value">XOR value</param>
        public static void ApplyMXor(Memory<byte> memory, byte value) => ApplyXor(memory.Span, value);

#if NET5_0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Vector128<byte> FillVector128Sse2(byte value)
        {
            var srcPtr = stackalloc int[128 / 8 / 4];
            int iValue = (value << 8) | value;
            iValue |= iValue << 16;
            srcPtr[0] = iValue;
            srcPtr[1] = iValue;
            srcPtr[2] = iValue;
            srcPtr[3] = iValue;
            return Sse2.LoadVector128((byte*)srcPtr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Vector128<byte> FillVector128AdvSimd(byte value)
        {
            var srcPtr = stackalloc int[128 / 8 / 4];
            int iValue = (value << 8) | value;
            iValue |= iValue << 16;
            srcPtr[0] = iValue;
            srcPtr[1] = iValue;
            srcPtr[2] = iValue;
            srcPtr[3] = iValue;
            return AdvSimd.LoadVector128((byte*)srcPtr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Vector256<byte> FillVector256Avx(byte value)
        {
            var srcPtr = stackalloc int[256 / 8 / 4];
            int iValue = (value << 8) | value;
            iValue |= iValue << 16;
            srcPtr[0] = iValue;
            srcPtr[1] = iValue;
            srcPtr[2] = iValue;
            srcPtr[3] = iValue;
            srcPtr[4] = iValue;
            srcPtr[5] = iValue;
            srcPtr[6] = iValue;
            srcPtr[7] = iValue;
            return Avx.LoadVector256((byte*)srcPtr);
        }

        /// <summary>
        /// Apply AND to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">AND value</param>
        public static unsafe void ApplyAndArm(Span<byte> span, byte value)
        {
            const int split = 128 / 8;
            fixed (byte* pSource = span)
            {
                int i = 0;
                int l = span.Length;

                #region First part

                int kill1Idx = Math.Min((int)unchecked((ulong)(split - (long)pSource) % split), l);
                while (i < kill1Idx)
                {
                    pSource[i] &= value;
                    i++;
                }

                if (kill1Idx == l) return;

                #endregion

                #region Arm

                var src = FillVector128AdvSimd(value);
                int kill2Idx = l - l % split;
                while (i < kill2Idx)
                {
                    AdvSimd.Store(pSource + i, AdvSimd.And(AdvSimd.LoadVector128(pSource + i), src));
                    i += split;
                }

                #endregion

                #region Last part

                while (i < span.Length)
                {
                    pSource[i] &= value;
                    i++;
                }

                #endregion
            }
        }

        /// <summary>
        /// Apply AND to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">AND value</param>
        public static unsafe void ApplyAndSse2(Span<byte> span, byte value)
        {
            const int split = 128 / 8;
            fixed (byte* pSource = span)
            {
                int i = 0;
                int l = span.Length;

                #region First part

                int kill1Idx = Math.Min((int)unchecked((ulong)(split - (long)pSource) % split), l);
                while (i < kill1Idx)
                {
                    pSource[i] &= value;
                    i++;
                }

                if (kill1Idx == l) return;

                #endregion

                #region Sse2

                var src = FillVector128Sse2(value);
                int kill2Idx = l - l % split;
                while (i < kill2Idx)
                {
                    Sse2.StoreAligned(pSource + i, Sse2.And(Sse2.LoadAlignedVector128(pSource + i), src));
                    i += split;
                }

                #endregion

                #region Last part

                while (i < span.Length)
                {
                    pSource[i] &= value;
                    i++;
                }

                #endregion
            }
        }

        /// <summary>
        /// Apply AND to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">AND value</param>
        public static unsafe void ApplyAndAvx2(Span<byte> span, byte value)
        {
            const int split = 256 / 8;
            fixed (byte* pSource = span)
            {
                int i = 0;
                int l = span.Length;

                #region First part

                int kill1Idx = Math.Min((int)unchecked((ulong)(split - (long)pSource) % split), l);
                while (i < kill1Idx)
                {
                    pSource[i] &= value;
                    i++;
                }

                if (kill1Idx == l) return;

                #endregion

                #region Avx

                var src = FillVector256Avx(value);
                int kill2Idx = l - l % split;
                while (i < kill2Idx)
                {
                    Avx.StoreAligned(pSource + i, Avx2.And(Avx.LoadAlignedVector256(pSource + i), src));
                    i += split;
                }

                #endregion

                #region Last part

                while (i < span.Length)
                {
                    pSource[i] &= value;
                    i++;
                }

                #endregion
            }
        }

        /// <summary>
        /// Apply OR to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">OR value</param>
        public static unsafe void ApplyOrArm(Span<byte> span, byte value)
        {
            const int split = 128 / 8;
            fixed (byte* pSource = span)
            {
                int i = 0;
                int l = span.Length;

                #region First part

                int kill1Idx = Math.Min((int)unchecked((ulong)(split - (long)pSource) % split), l);
                while (i < kill1Idx)
                {
                    pSource[i] &= value;
                    i++;
                }

                if (kill1Idx == l) return;

                #endregion

                #region Arm

                var src = FillVector128AdvSimd(value);
                int kill2Idx = l - l % split;
                while (i < kill2Idx)
                {
                    AdvSimd.Store(pSource + i, AdvSimd.Or(AdvSimd.LoadVector128(pSource + i), src));
                    i += split;
                }

                #endregion

                #region Last part

                while (i < span.Length)
                {
                    pSource[i] &= value;
                    i++;
                }

                #endregion
            }
        }

        /// <summary>
        /// Apply OR to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">OR value</param>
        public static unsafe void ApplyOrSse2(Span<byte> span, byte value)
        {
            const int split = 128 / 8;
            fixed (byte* pSource = span)
            {
                int i = 0;
                int l = span.Length;

                #region First part

                int kill1Idx = Math.Min((int)unchecked((ulong)(split - (long)pSource) % split), l);
                while (i < kill1Idx)
                {
                    pSource[i] |= value;
                    i++;
                }

                if (kill1Idx == l) return;

                #endregion

                #region Sse2

                var src = FillVector128Sse2(value);
                int kill2Idx = l - l % split;
                while (i < kill2Idx)
                {
                    Sse2.StoreAligned(pSource + i, Sse2.Or(Sse2.LoadAlignedVector128(pSource + i), src));
                    i += split;
                }

                #endregion

                #region Last part

                while (i < span.Length)
                {
                    pSource[i] |= value;
                    i++;
                }

                #endregion
            }
        }

        /// <summary>
        /// Apply OR to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">OR value</param>
        public static unsafe void ApplyOrAvx2(Span<byte> span, byte value)
        {
            const int split = 256 / 8;
            fixed (byte* pSource = span)
            {
                int i = 0;
                int l = span.Length;

                #region First part

                int kill1Idx = Math.Min((int)unchecked((ulong)(split - (long)pSource) % split), l);
                while (i < kill1Idx)
                {
                    pSource[i] |= value;
                    i++;
                }

                if (kill1Idx == l) return;

                #endregion

                #region Avx

                var src = FillVector256Avx(value);
                int kill2Idx = l - l % split;
                while (i < kill2Idx)
                {
                    Avx.StoreAligned(pSource + i, Avx2.Or(Avx.LoadAlignedVector256(pSource + i), src));
                    i += split;
                }

                #endregion

                #region Last part

                while (i < span.Length)
                {
                    pSource[i] |= value;
                    i++;
                }

                #endregion
            }
        }

        /// <summary>
        /// Apply XOR to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">XOR value</param>
        public static unsafe void ApplyXorArm(Span<byte> span, byte value)
        {
            const int split = 128 / 8;
            fixed (byte* pSource = span)
            {
                int i = 0;
                int l = span.Length;

                #region First part

                int kill1Idx = Math.Min((int)unchecked((ulong)(split - (long)pSource) % split), l);
                while (i < kill1Idx)
                {
                    pSource[i] &= value;
                    i++;
                }

                if (kill1Idx == l) return;

                #endregion

                #region Arm

                var src = FillVector128AdvSimd(value);
                int kill2Idx = l - l % split;
                while (i < kill2Idx)
                {
                    AdvSimd.Store(pSource + i, AdvSimd.Or(AdvSimd.LoadVector128(pSource + i), src));
                    i += split;
                }

                #endregion

                #region Last part

                while (i < span.Length)
                {
                    pSource[i] &= value;
                    i++;
                }

                #endregion
            }
        }

        /// <summary>
        /// Apply XOR to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">XOR value</param>
        public static unsafe void ApplyXorSse2(Span<byte> span, byte value)
        {
            const int split = 128 / 8;
            fixed (byte* pSource = span)
            {
                int i = 0;
                int l = span.Length;

                #region First part

                int kill1Idx = Math.Min((int)unchecked((ulong)(split - (long)pSource) % split), l);
                while (i < kill1Idx)
                {
                    pSource[i] ^= value;
                    i++;
                }

                if (kill1Idx == l) return;

                #endregion

                #region Sse2

                var src = FillVector128Sse2(value);
                int kill2Idx = l - l % split;
                while (i < kill2Idx)
                {
                    Sse2.StoreAligned(pSource + i, Sse2.Xor(Sse2.LoadAlignedVector128(pSource + i), src));
                    i += split;
                }

                #endregion

                #region Last part

                while (i < span.Length)
                {
                    pSource[i] ^= value;
                    i++;
                }

                #endregion
            }
        }

        /// <summary>
        /// Apply XOR to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">XOR value</param>
        public static unsafe void ApplyXorAvx2(Span<byte> span, byte value)
        {
            const int split = 256 / 8;
            fixed (byte* pSource = span)
            {
                int i = 0;
                int l = span.Length;

                #region First part

                int kill1Idx = Math.Min((int)unchecked((ulong)(split - (long)pSource) % split), l);
                while (i < kill1Idx)
                {
                    pSource[i] ^= value;
                    i++;
                }

                if (kill1Idx == l) return;

                #endregion

                #region Avx

                var src = FillVector256Avx(value);
                int kill2Idx = l - l % split;
                while (i < kill2Idx)
                {
                    Avx.StoreAligned(pSource + i, Avx2.Xor(Avx.LoadAlignedVector256(pSource + i), src));
                    i += split;
                }

                #endregion

                #region Last part

                while (i < span.Length)
                {
                    pSource[i] ^= value;
                    i++;
                }

                #endregion
            }
        }
#endif

        /// <summary>
        /// Apply AND to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">AND value</param>
        public static void ApplyAndFallback(Span<byte> span, byte value)
        {
            for (int i = 0; i < span.Length; i++)
                span[i] &= value;
        }

        /// <summary>
        /// Apply OR to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">OR value</param>
        public static void ApplyOrFallback(Span<byte> span, byte value)
        {
            for (int i = 0; i < span.Length; i++)
                span[i] |= value;
        }

        /// <summary>
        /// Apply XOR to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">XOR value</param>
        public static void ApplyXorFallback(Span<byte> span, byte value)
        {
            for (int i = 0; i < span.Length; i++)
                span[i] ^= value;
        }

        /// <summary>
        /// Transform delegate
        /// </summary>
        /// <param name="input">Input value</param>
        /// <param name="index">Index</param>
        public delegate byte TransformDelegate(byte input, int index);

        /// <summary>
        /// Transform memory region
        /// </summary>
        /// <param name="memory">Memory to modify</param>
        /// <param name="func">Transformation delegate</param>
        public static void ApplyTransform(Memory<byte> memory, TransformDelegate func) =>
            ApplyTransform(memory.Span, func);

        /// <summary>
        /// Transform memory region
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="func">Transformation delegate</param>
        public static void ApplyTransform(Span<byte> span, TransformDelegate func)
        {
            for (int i = 0; i < span.Length; i++)
                span[i] = func(span[i], i);
        }

        #endregion
    }
}
