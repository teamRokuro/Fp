using System;
using System.IO;
using Fp.Helpers;
using static Fp.Processor;

namespace Fp
{
    // ReSharper disable InconsistentNaming
    public static partial class Scripting
    {
        /// <summary>
        /// Current file path.
        /// </summary>
        public static FpPath _file => FpPath.GetFromString(Current.InputFile) ?? throw new InvalidOperationException();

        /// <summary>
        /// Current file path without extension.
        /// </summary>
        public static FpPath _fileNoExt =>
            new(Path.GetFileNameWithoutExtension(Current.InputFile), Current.InputDirectory);

        /// <summary>
        /// Current file name.
        /// </summary>
        public static string _name => Path.GetFileName(Current.InputFile);

        /// <summary>
        /// Current file name without extension.
        /// </summary>
        public static string _nameNoExt => Path.GetFileNameWithoutExtension(Current.InputFile);

        /// <summary>
        /// Current file name.
        /// </summary>
        public static FpPath _namePath => FpPath.GetFromString(_name) ?? throw new InvalidOperationException();

        /// <summary>
        /// Current file name without extension.
        /// </summary>
        public static FpPath _namePathNoExt =>
            FpPath.GetFromString(_nameNoExt) ?? throw new InvalidOperationException();

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
