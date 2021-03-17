using System;
using System.Runtime.InteropServices;
using Fp.Helpers;
using static Fp.Processor;

namespace Fp
{
    public partial class Processor
    {

        /// <summary>
        /// Helper for unsigned 8-bit integer arrays.
        /// </summary>
        public U8ArrayHelper buf = null!;

        /// <summary>
        /// Helper for signed 8-bit integers.
        /// </summary>
        public S8Helper i1 = null!;

        /// <summary>
        /// Helper for signed 8-bit integer arrays.
        /// </summary>
        public S8ArrayHelper i1a = null!;

        /// <summary>
        /// Helper for unsigned 8-bit integers.
        /// </summary>
        public U8Helper u1 = null!;

        /// <summary>
        /// Helper for unsigned 8-bit integer arrays.
        /// </summary>
        public U8ArrayHelper u1a = null!;

        /// <summary>
        /// Helper for signed 16-bit integers.
        /// </summary>
        public S16Helper i2l = null!;

        /// <summary>
        /// Helper for signed 16-bit integer arrays.
        /// </summary>
        public S16ArrayHelper i2la = null!;

        /// <summary>
        /// Helper for signed 32-bit integers.
        /// </summary>
        public S32Helper i4l = null!;

        /// <summary>
        /// Helper for signed 32-bit integer arrays.
        /// </summary>
        public S32ArrayHelper i4la = null!;

        /// <summary>
        /// Helper for signed 64-bit integers.
        /// </summary>
        public S64Helper i8l = null!;

        /// <summary>
        /// Helper for signed 64-bit integer arrays.
        /// </summary>
        public S64ArrayHelper i8la = null!;

        /// <summary>
        /// Helper for unsigned 16-bit integers.
        /// </summary>
        public U16Helper u2l = null!;

        /// <summary>
        /// Helper for unsigned 16-bit integer arrays.
        /// </summary>
        public U16ArrayHelper u2la = null!;

        /// <summary>
        /// Helper for unsigned 32-bit integers.
        /// </summary>
        public U32Helper u4l = null!;

        /// <summary>
        /// Helper for unsigned 32-bit integer arrays.
        /// </summary>
        public U32ArrayHelper u4la = null!;

        /// <summary>
        /// Helper for unsigned 64-bit integers.
        /// </summary>
        public U64Helper u8l = null!;

        /// <summary>
        /// Helper for unsigned 64-bit integer arrays.
        /// </summary>
        public U64ArrayHelper u8la = null!;

        /// <summary>
        /// Helper for signed 16-bit integers.
        /// </summary>
        public S16Helper i2b = null!;

        /// <summary>
        /// Helper for signed 16-bit integer arrays.
        /// </summary>
        public S16ArrayHelper i2ba = null!;

        /// <summary>
        /// Helper for signed 32-bit integers.
        /// </summary>
        public S32Helper i4b = null!;

        /// <summary>
        /// Helper for signed 32-bit integer arrays.
        /// </summary>
        public S32ArrayHelper i4ba = null!;

        /// <summary>
        /// Helper for signed 64-bit integers.
        /// </summary>
        public S64Helper i8b = null!;

        /// <summary>
        /// Helper for signed 64-bit integer arrays.
        /// </summary>
        public S64ArrayHelper i8ba = null!;

        /// <summary>
        /// Helper for unsigned 16-bit integers.
        /// </summary>
        public U16Helper u2b = null!;

        /// <summary>
        /// Helper for unsigned 16-bit integer arrays.
        /// </summary>
        public U16ArrayHelper u2ba = null!;

        /// <summary>
        /// Helper for unsigned 32-bit integers.
        /// </summary>
        public U32Helper u4b = null!;

        /// <summary>
        /// Helper for unsigned 32-bit integer arrays.
        /// </summary>
        public U32ArrayHelper u4ba = null!;

        /// <summary>
        /// Helper for unsigned 64-bit integers.
        /// </summary>
        public U64Helper u8b = null!;

        /// <summary>
        /// Helper for unsigned 64-bit integer arrays.
        /// </summary>
        public U64ArrayHelper u8ba = null!;

        /// <summary>
        /// Helper for 16-bit floating point numbers.
        /// </summary>
        public F16Helper f2 = null!;

        /// <summary>
        /// Helper for 16-bit floating point number arrays.
        /// </summary>
        public F16ArrayHelper f2a = null!;

        /// <summary>
        /// Helper for 32-bit floating point numbers.
        /// </summary>
        public THelper<float> f4 = null!;

        /// <summary>
        /// Helper for 32-bit floating point number arrays.
        /// </summary>
        public TArrayHelper<float> f4a = null!;

        /// <summary>
        /// Helper for 64-bit floating point numbers.
        /// </summary>
        public THelper<double> f8 = null!;

        /// <summary>
        /// Helper for 64-bit floating point number arrays.
        /// </summary>
        public TArrayHelper<double> f8a = null!;

        /// <summary>
        /// Helper for ASCII strings.
        /// </summary>
        public AsciiStringHelper ascii = null!;

        /// <summary>
        /// Helper for UTF-8 strings.
        /// </summary>
        public Utf8StringHelper utf8 = null!;

        /// <summary>
        /// Helper for UTF-8 strings.
        /// </summary>
        public Utf16StringHelper utf16 = null!;

        private void InitEncodingDecodingHelpers()
        {
            i1 = new S8Helper(this);
            i1a = new S8ArrayHelper(this);
            u1 = new U8Helper(this);
            u1a = new U8ArrayHelper(this);
            buf = u1a;
            i2l = new S16Helper(this, true);
            i2la = new S16ArrayHelper(this, true);
            i4l = new S32Helper(this, true);
            i4la = new S32ArrayHelper(this, true);
            i8l = new S64Helper(this, true);
            i8la = new S64ArrayHelper(this, true);
            u2l = new U16Helper(this, true);
            u2la = new U16ArrayHelper(this, true);
            u4l = new U32Helper(this, true);
            u4la = new U32ArrayHelper(this, true);
            u8l = new U64Helper(this, true);
            u8la = new U64ArrayHelper(this, true);
            i2b = new S16Helper(this, false);
            i2ba = new S16ArrayHelper(this, false);
            i4b = new S32Helper(this, false);
            i4ba = new S32ArrayHelper(this, false);
            i8b = new S64Helper(this, false);
            i8ba = new S64ArrayHelper(this, false);
            u2b = new U16Helper(this, false);
            u2ba = new U16ArrayHelper(this, false);
            u4b = new U32Helper(this, false);
            u4ba = new U32ArrayHelper(this, false);
            u8b = new U64Helper(this, false);
            u8ba = new U64ArrayHelper(this, false);
            f2 = new F16Helper(this);
            f2a = new F16ArrayHelper(this);
            f4 = new THelper<float>(this);
            f4a = new TArrayHelper<float>(this);
            f8 = new THelper<double>(this);
            f8a = new TArrayHelper<double>(this);
            ascii = new AsciiStringHelper(this);
            utf8 = new Utf8StringHelper(this);
            utf16 = new Utf16StringHelper(this);
        }

        /// <summary>
        /// Write array.
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        public static void SetS8Array(Span<byte> span, ReadOnlySpan<sbyte> array)
        {
            MemoryMarshal.Cast<sbyte, byte>(array).CopyTo(span);
        }

        /// <summary>
        /// Write array.
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        public static void SetU8Array(Span<byte> span, ReadOnlySpan<byte> array)
        {
            array.CopyTo(span);
        }

        #region Static endianness

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <returns>Converted array.</returns>
        public static short[] GetS16Array(ReadOnlySpan<byte> span, bool littleEndian)
        {
            short[] result = MemoryMarshal.Cast<byte, short>(span).ToArray();
            ConvertS16Array(result, littleEndian);
            return result;
        }

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        public static void SetS16Array(Span<byte> span, ReadOnlySpan<short> array, bool littleEndian)
        {
            MemoryMarshal.Cast<short, byte>(array).CopyTo(span);
            ConvertS16Array(span, littleEndian);
        }

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <returns>Converted array.</returns>
        public static int[] GetS32Array(ReadOnlySpan<byte> span, bool littleEndian)
        {
            int[] result = MemoryMarshal.Cast<byte, int>(span).ToArray();
            ConvertS32Array(result, littleEndian);
            return result;
        }

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        public static void SetS32Array(Span<byte> span, ReadOnlySpan<int> array, bool littleEndian)
        {
            MemoryMarshal.Cast<int, byte>(array).CopyTo(span);
            ConvertS32Array(span, littleEndian);
        }

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <returns>Converted array.</returns>
        public static long[] GetS64Array(ReadOnlySpan<byte> span, bool littleEndian)
        {
            long[] result = MemoryMarshal.Cast<byte, long>(span).ToArray();
            ConvertS64Array(result, littleEndian);
            return result;
        }

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        public static void SetS64Array(Span<byte> span, ReadOnlySpan<long> array, bool littleEndian)
        {
            MemoryMarshal.Cast<long, byte>(array).CopyTo(span);
            ConvertS64Array(span, littleEndian);
        }

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <returns>Converted array.</returns>
        public static ushort[] GetU16Array(ReadOnlySpan<byte> span, bool littleEndian)
        {
            ushort[] result = MemoryMarshal.Cast<byte, ushort>(span).ToArray();
            ConvertU16Array(result, littleEndian);
            return result;
        }

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        public static void SetU16Array(Span<byte> span, ReadOnlySpan<ushort> array, bool littleEndian)
        {
            MemoryMarshal.Cast<ushort, byte>(array).CopyTo(span);
            ConvertS32Array(span, littleEndian);
        }

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <returns>Converted array.</returns>
        public static uint[] GetU32Array(ReadOnlySpan<byte> span, bool littleEndian)
        {
            uint[] result = MemoryMarshal.Cast<byte, uint>(span).ToArray();
            ConvertU32Array(result, littleEndian);
            return result;
        }

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        public static void SetU32Array(Span<byte> span, ReadOnlySpan<uint> array, bool littleEndian)
        {
            MemoryMarshal.Cast<uint, byte>(array).CopyTo(span);
            ConvertS32Array(span, littleEndian);
        }

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        /// <returns>Converted array.</returns>
        public static ulong[] GetU64Array(ReadOnlySpan<byte> span, bool littleEndian)
        {
            ulong[] result = MemoryMarshal.Cast<byte, ulong>(span).ToArray();
            ConvertU64Array(result, littleEndian);
            return result;
        }

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        /// <param name="littleEndian">If true, use little-endian encoding</param>
        public static void SetU64Array(Span<byte> span, ReadOnlySpan<ulong> array, bool littleEndian)
        {
            MemoryMarshal.Cast<ulong, byte>(array).CopyTo(span);
            ConvertS32Array(span, littleEndian);
        }

        #endregion

        #region Instance endianness

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <returns>Converted array.</returns>
        public short[] GetS16Array(ReadOnlySpan<byte> span) => GetS16Array(span, LittleEndian);

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        public void SetS16Array(Span<byte> span, short[] array) => SetS16Array(span, array, LittleEndian);

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <returns>Converted array.</returns>
        public int[] GetS32Array(ReadOnlySpan<byte> span) => GetS32Array(span, LittleEndian);

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        public void SetS32Array(Span<byte> span, int[] array) => SetS32Array(span, array, LittleEndian);

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <returns>Converted array.</returns>
        public long[] GetS64Array(ReadOnlySpan<byte> span) => GetS64Array(span, LittleEndian);

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        public void SetS64Array(Span<byte> span, long[] array) => SetS64Array(span, array, LittleEndian);

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <returns>Converted array.</returns>
        public ushort[] GetU16Array(ReadOnlySpan<byte> span) => GetU16Array(span, LittleEndian);

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        public void SetU16Array(Span<byte> span, ushort[] array) => SetU16Array(span, array, LittleEndian);

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <returns>Converted array.</returns>
        public uint[] GetU32Array(ReadOnlySpan<byte> span) => GetU32Array(span, LittleEndian);

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        public void SetU32Array(Span<byte> span, uint[] array) => SetU32Array(span, array, LittleEndian);

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <returns>Converted array.</returns>
        public ulong[] GetU64Array(ReadOnlySpan<byte> span) => GetU64Array(span, LittleEndian);

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        public void SetU64Array(Span<byte> span, ulong[] array) => SetU64Array(span, array, LittleEndian);

        #endregion

        /// <summary>
        /// Get generic array.
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <typeparam name="T">Element type.</typeparam>
        /// <returns>Read data.</returns>
        public static Span<T> GetTArray<T>(Span<byte> span) where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(span);
        }

        /// <summary>
        /// Get generic array.
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <typeparam name="T">Element type.</typeparam>
        /// <returns>Read data.</returns>
        public static ReadOnlySpan<T> GetTArray<T>(ReadOnlySpan<byte> span) where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(span);
        }

        /// <summary>
        /// Set generic array.
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        /// <typeparam name="T">Element type.</typeparam>
        public static void SetTArray<T>(Span<byte> span, ReadOnlySpan<T> array) where T : unmanaged
        {
            MemoryMarshal.Cast<T, byte>(array).CopyTo(span);
        }
    }

    // ReSharper disable InconsistentNaming
    public partial class Scripting
    {
        /// <summary>
        /// Helper for unsigned 8-bit integers.
        /// </summary>
        public static BaseUnmanagedArrayHelper<byte> buf => Current.buf;

        /// <summary>
        /// Helper for signed 8-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<sbyte> i1 => Current.i1;

        /// <summary>
        /// Helper for signed 8-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<sbyte> i1a => Current.i1a;

        /// <summary>
        /// Helper for unsigned 8-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<byte> u1 => Current.u1;

        /// <summary>
        /// Helper for unsigned 8-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<byte> u1a => Current.u1a;


        /// <summary>
        /// Helper for signed 16-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<short> i2l => Current.i2l;

        /// <summary>
        /// Helper for signed 16-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<short> i2la => Current.i2la;

        /// <summary>
        /// Helper for signed 32-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<int> i4l => Current.i4l;

        /// <summary>
        /// Helper for signed 32-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<int> i4la => Current.i4la;

        /// <summary>
        /// Helper for signed 64-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<long> i8l => Current.i8l;

        /// <summary>
        /// Helper for signed 64-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<long> i8la => Current.i8la;

        /// <summary>
        /// Helper for unsigned 16-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<ushort> u2l => Current.u2l;

        /// <summary>
        /// Helper for unsigned 16-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<ushort> u2la => Current.u2la;

        /// <summary>
        /// Helper for unsigned 32-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<uint> u4l => Current.u4l;

        /// <summary>
        /// Helper for unsigned 32-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<uint> u4la => Current.u4la;

        /// <summary>
        /// Helper for unsigned 64-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<ulong> u8l => Current.u8l;

        /// <summary>
        /// Helper for unsigned 64-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<ulong> u8la => Current.u8la;

        /// <summary>
        /// Helper for signed 16-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<short> i2b => Current.i2b;

        /// <summary>
        /// Helper for signed 16-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<short> i2ba => Current.i2ba;

        /// <summary>
        /// Helper for signed 32-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<int> i4b => Current.i4b;

        /// <summary>
        /// Helper for signed 32-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<int> i4ba => Current.i4ba;

        /// <summary>
        /// Helper for signed 64-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<long> i8b => Current.i8b;

        /// <summary>
        /// Helper for signed 64-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<long> i8ba => Current.i8ba;

        /// <summary>
        /// Helper for unsigned 16-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<ushort> u2b => Current.u2b;

        /// <summary>
        /// Helper for unsigned 16-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<ushort> u2ba => Current.u2ba;

        /// <summary>
        /// Helper for unsigned 32-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<uint> u4b => Current.u4b;

        /// <summary>
        /// Helper for unsigned 32-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<uint> u4ba => Current.u4ba;

        /// <summary>
        /// Helper for unsigned 64-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<ulong> u8b => Current.u8b;

        /// <summary>
        /// Helper for unsigned 64-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<ulong> u8ba => Current.u8ba;

        /// <summary>
        /// Helper for 16-bit floating point numbers.
        /// </summary>
        public static F16Helper f2 => Current.f2;

        /// <summary>
        /// Helper for 16-bit floating point number arrays.
        /// </summary>
        public static F16ArrayHelper f2a => Current.f2a;

        /// <summary>
        /// Helper for 32-bit floating point numbers.
        /// </summary>
        public static BaseUnmanagedHelper<float> f4 => Current.f4;

        /// <summary>
        /// Helper for 32-bit floating point number arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<float> f4a => Current.f4a;

        /// <summary>
        /// Helper for 64-bit floating point numbers.
        /// </summary>
        public static BaseUnmanagedHelper<double> f8 => Current.f8;

        /// <summary>
        /// Helper for 64-bit floating point number arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<double> f8a => Current.f8a;

        /// <summary>
        /// Helper for ASCII strings.
        /// </summary>
        public static AsciiStringHelper ascii => Current.ascii;

        /// <summary>
        /// Helper for UTF-8 strings.
        /// </summary>
        public static Utf8StringHelper utf8 => Current.utf8;

        /// <summary>
        /// Helper for UTF-8 strings.
        /// </summary>
        public static Utf16StringHelper utf16 => Current.utf16;
    }
    // ReSharper restore InconsistentNaming
}
