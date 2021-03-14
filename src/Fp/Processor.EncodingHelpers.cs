using System;
using System.Runtime.InteropServices;
using Fp.Helpers;
using static Fp.Processor;

namespace Fp
{
    public partial class Processor
    {
        /// <summary>
        /// Helper for signed 8-bit integers.
        /// </summary>
        public S8Helper S8 = null!;

        /// <summary>
        /// Helper for signed 8-bit integer arrays.
        /// </summary>
        public S8ArrayHelper S8A = null!;

        /// <summary>
        /// Helper for unsigned 8-bit integers.
        /// </summary>
        public U8Helper U8 = null!;

        /// <summary>
        /// Helper for unsigned 8-bit integer arrays.
        /// </summary>
        public U8ArrayHelper U8A = null!;


        /// <summary>
        /// Helper for signed 16-bit integers.
        /// </summary>
        public S16Helper S16L = null!;

        /// <summary>
        /// Helper for signed 16-bit integer arrays.
        /// </summary>
        public S16ArrayHelper S16LA = null!;

        /// <summary>
        /// Helper for signed 32-bit integers.
        /// </summary>
        public S32Helper S32L = null!;

        /// <summary>
        /// Helper for signed 32-bit integer arrays.
        /// </summary>
        public S32ArrayHelper S32LA = null!;

        /// <summary>
        /// Helper for signed 64-bit integers.
        /// </summary>
        public S64Helper S64L = null!;

        /// <summary>
        /// Helper for signed 64-bit integer arrays.
        /// </summary>
        public S64ArrayHelper S64LA = null!;

        /// <summary>
        /// Helper for unsigned 16-bit integers.
        /// </summary>
        public U16Helper U16L = null!;

        /// <summary>
        /// Helper for unsigned 16-bit integer arrays.
        /// </summary>
        public U16ArrayHelper U16LA = null!;

        /// <summary>
        /// Helper for unsigned 32-bit integers.
        /// </summary>
        public U32Helper U32L = null!;

        /// <summary>
        /// Helper for unsigned 32-bit integer arrays.
        /// </summary>
        public U32ArrayHelper U32LA = null!;

        /// <summary>
        /// Helper for unsigned 64-bit integers.
        /// </summary>
        public U64Helper U64L = null!;

        /// <summary>
        /// Helper for unsigned 64-bit integer arrays.
        /// </summary>
        public U64ArrayHelper U64LA = null!;

        /// <summary>
        /// Helper for signed 16-bit integers.
        /// </summary>
        public S16Helper S16B = null!;

        /// <summary>
        /// Helper for signed 16-bit integer arrays.
        /// </summary>
        public S16ArrayHelper S16BA = null!;

        /// <summary>
        /// Helper for signed 32-bit integers.
        /// </summary>
        public S32Helper S32B = null!;

        /// <summary>
        /// Helper for signed 32-bit integer arrays.
        /// </summary>
        public S32ArrayHelper S32BA = null!;

        /// <summary>
        /// Helper for signed 64-bit integers.
        /// </summary>
        public S64Helper S64B = null!;

        /// <summary>
        /// Helper for signed 64-bit integer arrays.
        /// </summary>
        public S64ArrayHelper S64BA = null!;

        /// <summary>
        /// Helper for unsigned 16-bit integers.
        /// </summary>
        public U16Helper U16B = null!;

        /// <summary>
        /// Helper for unsigned 16-bit integer arrays.
        /// </summary>
        public U16ArrayHelper U16BA = null!;

        /// <summary>
        /// Helper for unsigned 32-bit integers.
        /// </summary>
        public U32Helper U32B = null!;

        /// <summary>
        /// Helper for unsigned 32-bit integer arrays.
        /// </summary>
        public U32ArrayHelper U32BA = null!;

        /// <summary>
        /// Helper for unsigned 64-bit integers.
        /// </summary>
        public U64Helper U64B = null!;

        /// <summary>
        /// Helper for unsigned 64-bit integer arrays.
        /// </summary>
        public U64ArrayHelper U64BA = null!;

        /// <summary>
        /// Helper for 16-bit floating point numbers.
        /// </summary>
        public F16Helper F16 = null!;

        /// <summary>
        /// Helper for 16-bit floating point number arrays.
        /// </summary>
        public F16ArrayHelper F16A = null!;

        /// <summary>
        /// Helper for 32-bit floating point numbers.
        /// </summary>
        public THelper<float> F32 = null!;

        /// <summary>
        /// Helper for 32-bit floating point number arrays.
        /// </summary>
        public TArrayHelper<float> F32A = null!;

        /// <summary>
        /// Helper for 64-bit floating point numbers.
        /// </summary>
        public THelper<double> F64 = null!;

        /// <summary>
        /// Helper for 64-bit floating point number arrays.
        /// </summary>
        public TArrayHelper<double> F64A = null!;

        /// <summary>
        /// Helper for UTF-8 strings.
        /// </summary>
        public Utf8StringHelper Utf8 = null!;

        /// <summary>
        /// Helper for UTF-8 strings.
        /// </summary>
        public Utf16StringHelper Utf16 = null!;

        private void InitEncodingDecodingHelpers()
        {
            S8 = new S8Helper(this);
            S8A = new S8ArrayHelper(this);
            U8 = new U8Helper(this);
            U8A = new U8ArrayHelper(this);
            S16L = new S16Helper(this, true);
            S16LA = new S16ArrayHelper(this, true);
            S32L = new S32Helper(this, true);
            S32LA = new S32ArrayHelper(this, true);
            S64L = new S64Helper(this, true);
            S64LA = new S64ArrayHelper(this, true);
            U16L = new U16Helper(this, true);
            U16LA = new U16ArrayHelper(this, true);
            U32L = new U32Helper(this, true);
            U32LA = new U32ArrayHelper(this, true);
            U64L = new U64Helper(this, true);
            U64LA = new U64ArrayHelper(this, true);
            S16B = new S16Helper(this, false);
            S16BA = new S16ArrayHelper(this, false);
            S32B = new S32Helper(this, false);
            S32BA = new S32ArrayHelper(this, false);
            S64B = new S64Helper(this, false);
            S64BA = new S64ArrayHelper(this, false);
            U16B = new U16Helper(this, false);
            U16BA = new U16ArrayHelper(this, false);
            U32B = new U32Helper(this, false);
            U32BA = new U32ArrayHelper(this, false);
            U64B = new U64Helper(this, false);
            U64BA = new U64ArrayHelper(this, false);
            F16 = new F16Helper(this);
            F16A = new F16ArrayHelper(this);
            F32 = new THelper<float>(this);
            F32A = new TArrayHelper<float>(this);
            F64 = new THelper<double>(this);
            F64A = new TArrayHelper<double>(this);
            Utf8 = new Utf8StringHelper(this);
            Utf16 = new Utf16StringHelper(this);
        }

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <returns>Converted array.</returns>
        public static sbyte[] GetS8Array(ReadOnlySpan<byte> span)
        {
            return MemoryMarshal.Cast<byte, sbyte>(span).ToArray();
        }

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        /// <returns>Converted array.</returns>
        public static void SetS8Array(Span<byte> span, sbyte[] array)
        {
            MemoryMarshal.Cast<sbyte, byte>(array).CopyTo(span);
        }

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <returns>Converted array.</returns>
        public static byte[] GetU8Array(ReadOnlySpan<byte> span)
        {
            return span.ToArray();
        }

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        /// <returns>Converted array.</returns>
        public static void SetU8Array(Span<byte> span, byte[] array)
        {
            array.AsSpan().CopyTo(span);
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
        /// <returns>Converted array.</returns>
        public static void SetS16Array(Span<byte> span, short[] array, bool littleEndian)
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
        /// <returns>Converted array.</returns>
        public static void SetS32Array(Span<byte> span, int[] array, bool littleEndian)
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
        /// <returns>Converted array.</returns>
        public static void SetS64Array(Span<byte> span, long[] array, bool littleEndian)
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
        /// <returns>Converted array.</returns>
        public static void SetU16Array(Span<byte> span, ushort[] array, bool littleEndian)
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
        /// <returns>Converted array.</returns>
        public static void SetU32Array(Span<byte> span, uint[] array, bool littleEndian)
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
        /// <returns>Converted array.</returns>
        public static void SetU64Array(Span<byte> span, ulong[] array, bool littleEndian)
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
        /// <returns>Converted array.</returns>
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
        /// <returns>Converted array.</returns>
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
        /// <returns>Converted array.</returns>
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
        /// <returns>Converted array.</returns>
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
        /// <returns>Converted array.</returns>
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
        /// <returns>Converted array.</returns>
        public void SetU64Array(Span<byte> span, ulong[] array) => SetU64Array(span, array, LittleEndian);

        #endregion

        /// <summary>
        /// Get generic array.
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <typeparam name="T">Element type.</typeparam>
        /// <returns>Read data.</returns>
        public static T[] GetTArray<T>(ReadOnlySpan<byte> span) where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(span).ToArray();
        }

        /// <summary>
        /// Set generic array.
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        /// <typeparam name="T">Element type.</typeparam>
        public static void SetTArray<T>(Span<byte> span, T[] array) where T : unmanaged
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
        public static BaseUnmanagedArrayHelper<byte> buf => Current.U8A;

        /// <summary>
        /// Helper for signed 8-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<sbyte> i1 => Current.S8;

        /// <summary>
        /// Helper for signed 8-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<sbyte> i1a => Current.S8A;

        /// <summary>
        /// Helper for unsigned 8-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<byte> u1 => Current.U8;

        /// <summary>
        /// Helper for unsigned 8-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<byte> u1a => Current.U8A;


        /// <summary>
        /// Helper for signed 16-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<short> i2l => Current.S16L;

        /// <summary>
        /// Helper for signed 16-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<short> i2la => Current.S16LA;

        /// <summary>
        /// Helper for signed 32-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<int> i4l => Current.S32L;

        /// <summary>
        /// Helper for signed 32-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<int> i4la => Current.S32LA;

        /// <summary>
        /// Helper for signed 64-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<long> i8l => Current.S64L;

        /// <summary>
        /// Helper for signed 64-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<long> i8la => Current.S64LA;

        /// <summary>
        /// Helper for unsigned 16-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<ushort> u2l => Current.U16L;

        /// <summary>
        /// Helper for unsigned 16-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<ushort> u2la => Current.U16LA;

        /// <summary>
        /// Helper for unsigned 32-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<uint> u4l => Current.U32L;

        /// <summary>
        /// Helper for unsigned 32-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<uint> u4la => Current.U32LA;

        /// <summary>
        /// Helper for unsigned 64-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<ulong> u8l => Current.U64L;

        /// <summary>
        /// Helper for unsigned 64-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<ulong> u8la => Current.U64LA;

        /// <summary>
        /// Helper for signed 16-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<short> i2b => Current.S16B;

        /// <summary>
        /// Helper for signed 16-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<short> i2ba => Current.S16BA;

        /// <summary>
        /// Helper for signed 32-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<int> i4b => Current.S32B;

        /// <summary>
        /// Helper for signed 32-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<int> i4ba => Current.S32BA;

        /// <summary>
        /// Helper for signed 64-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<long> i8b => Current.S64B;

        /// <summary>
        /// Helper for signed 64-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<long> i8ba => Current.S64BA;

        /// <summary>
        /// Helper for unsigned 16-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<ushort> u2b => Current.U16B;

        /// <summary>
        /// Helper for unsigned 16-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<ushort> u2ba => Current.U16BA;

        /// <summary>
        /// Helper for unsigned 32-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<uint> u4b => Current.U32B;

        /// <summary>
        /// Helper for unsigned 32-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<uint> u4ba => Current.U32BA;

        /// <summary>
        /// Helper for unsigned 64-bit integers.
        /// </summary>
        public static BaseUnmanagedHelper<ulong> u8b => Current.U64B;

        /// <summary>
        /// Helper for unsigned 64-bit integer arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<ulong> u8ba => Current.U64BA;

        /// <summary>
        /// Helper for 16-bit floating point numbers.
        /// </summary>
        public static F16Helper f2 => Current.F16;

        /// <summary>
        /// Helper for 16-bit floating point number arrays.
        /// </summary>
        public static F16ArrayHelper f2a => Current.F16A;

        /// <summary>
        /// Helper for 32-bit floating point numbers.
        /// </summary>
        public static BaseUnmanagedHelper<float> f4 => Current.F32;

        /// <summary>
        /// Helper for 32-bit floating point number arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<float> f4a => Current.F32A;

        /// <summary>
        /// Helper for 64-bit floating point numbers.
        /// </summary>
        public static BaseUnmanagedHelper<double> f8 => Current.F64;

        /// <summary>
        /// Helper for 64-bit floating point number arrays.
        /// </summary>
        public static BaseUnmanagedArrayHelper<double> f8a => Current.F64A;

        /// <summary>
        /// Helper for UTF-8 strings.
        /// </summary>
        public static Utf8StringHelper utf8 => Current.Utf8;

        /// <summary>
        /// Helper for UTF-8 strings.
        /// </summary>
        public static Utf16StringHelper utf16 => Current.Utf16;
    }
    // ReSharper restore InconsistentNaming
}
