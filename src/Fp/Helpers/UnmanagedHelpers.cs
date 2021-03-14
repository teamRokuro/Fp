using System;
using System.IO;
using System.Runtime.InteropServices;
using static Fp.Processor;

namespace Fp.Helpers
{
    /// <summary>
    /// Signed 8-bit helper.
    /// </summary>
    public record S8Helper(Processor Parent) : BaseUnmanagedHelper<sbyte>
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <summary>
        /// Read/write value.
        /// </summary>
        /// <param name="source">Data source.</param>
        public override sbyte this[Span<byte> source]
        {
            get => Parent.GetS8(source);
            set => Parent.SetS8(source, value);
        }

        /// <summary>
        /// Read value.
        /// </summary>
        /// <param name="source">Data source.</param>
        public override sbyte this[ReadOnlySpan<byte> source] => Parent.GetS8(source);
    }

    /// <summary>
    /// Signed 8-bit array helper.
    /// </summary>
    public record S8ArrayHelper(Processor Parent) : BaseUnmanagedArrayHelper<sbyte>
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override sbyte[] this[Span<byte> source]
        {
            get => GetS8Array(source);
            set => SetS8Array(source, value);
        }

        /// <inheritdoc />
        public override sbyte[] this[ReadOnlySpan<byte> source] => GetS8Array(source);
    }

    /// <summary>
    /// Signed 16-bit helper.
    /// </summary>
    public record S16Helper(Processor Parent, bool LittleEndian) : BaseUnmanagedHelper<short>
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <summary>
        /// Read/write value.
        /// </summary>
        /// <param name="source">Data source.</param>
        public override short this[Span<byte> source]
        {
            get => GetS16(source, LittleEndian);
            set => SetS16(source, value, LittleEndian);
        }

        /// <summary>
        /// Read value.
        /// </summary>
        /// <param name="source">Data source.</param>
        public override short this[ReadOnlySpan<byte> source] => Parent.GetS16(source);
    }

    /// <summary>
    /// Signed 16-bit array helper.
    /// </summary>
    public record S16ArrayHelper(Processor Parent, bool LittleEndian) : BaseUnmanagedArrayHelper<short>
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override short[] this[Span<byte> source]
        {
            get => GetS16Array(source, LittleEndian);
            set => SetS16Array(source, value, LittleEndian);
        }

        /// <inheritdoc />
        public override short[] this[ReadOnlySpan<byte> source] => GetS16Array(source, LittleEndian);
    }

    /// <summary>
    /// Signed 32-bit helper.
    /// </summary>
    public record S32Helper(Processor Parent, bool LittleEndian) : BaseUnmanagedHelper<int>
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <summary>
        /// Read/write value.
        /// </summary>
        /// <param name="source">Data source.</param>
        public override int this[Span<byte> source]
        {
            get => GetS32(source, LittleEndian);
            set => SetS32(source, value, LittleEndian);
        }

        /// <summary>
        /// Read value.
        /// </summary>
        /// <param name="source">Data source.</param>
        public override int this[ReadOnlySpan<byte> source] => Parent.GetS32(source);
    }

    /// <summary>
    /// Signed 32-bit helper.
    /// </summary>
    public record S32ArrayHelper(Processor Parent, bool LittleEndian) : BaseUnmanagedArrayHelper<int>
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override int[] this[Span<byte> source]
        {
            get => GetS32Array(source, LittleEndian);
            set => SetS32Array(source, value, LittleEndian);
        }

        /// <inheritdoc />
        public override int[] this[ReadOnlySpan<byte> source] => GetS32Array(source, LittleEndian);
    }

    /// <summary>
    /// Signed 64-bit helper.
    /// </summary>
    public record S64Helper(Processor Parent, bool LittleEndian) : BaseUnmanagedHelper<long>
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <summary>
        /// Read/write value.
        /// </summary>
        /// <param name="source">Data source.</param>
        public override long this[Span<byte> source]
        {
            get => GetS64(source, LittleEndian);
            set => SetS64(source, value, LittleEndian);
        }

        /// <summary>
        /// Read value.
        /// </summary>
        /// <param name="source">Data source.</param>
        public override long this[ReadOnlySpan<byte> source] => Parent.GetS64(source);
    }

    /// <summary>
    /// Signed 64-bit helper.
    /// </summary>
    public record S64ArrayHelper(Processor Parent, bool LittleEndian) : BaseUnmanagedArrayHelper<long>
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override long[] this[Span<byte> source]
        {
            get => GetS64Array(source, LittleEndian);
            set => SetS64Array(source, value, LittleEndian);
        }

        /// <inheritdoc />
        public override long[] this[ReadOnlySpan<byte> source] => GetS64Array(source, LittleEndian);
    }

    /// <summary>
    /// Unsigned 8-bit helper.
    /// </summary>
    public record U8Helper(Processor Parent) : BaseUnmanagedHelper<byte>
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <summary>
        /// Read/write value.
        /// </summary>
        /// <param name="source">Data source.</param>
        public override byte this[Span<byte> source]
        {
            get => Parent.GetU8(source);
            set => Parent.SetU8(source, value);
        }

        /// <summary>
        /// Read value.
        /// </summary>
        /// <param name="source">Data source.</param>
        public override byte this[ReadOnlySpan<byte> source] => Parent.GetU8(source);
    }

    /// <summary>
    /// Unsigned 8-bit helper.
    /// </summary>
    public record U8ArrayHelper(Processor Parent) : BaseUnmanagedArrayHelper<byte>
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override byte[] this[Span<byte> source]
        {
            get => GetU8Array(source);
            set => SetU8Array(source, value);
        }

        /// <inheritdoc />
        public override byte[] this[ReadOnlySpan<byte> source] => GetU8Array(source);
    }

    /// <summary>
    /// Unsigned 16-bit helper.
    /// </summary>
    public record U16Helper(Processor Parent, bool LittleEndian) : BaseUnmanagedHelper<ushort>
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <summary>
        /// Read/write value.
        /// </summary>
        /// <param name="source">Data source.</param>
        public override ushort this[Span<byte> source]
        {
            get => GetU16(source, LittleEndian);
            set => SetU16(source, value, LittleEndian);
        }

        /// <summary>
        /// Read value.
        /// </summary>
        /// <param name="source">Data source.</param>
        public override ushort this[ReadOnlySpan<byte> source] => Parent.GetU16(source);
    }

    /// <summary>
    /// Unsigned 16-bit helper.
    /// </summary>
    public record U16ArrayHelper(Processor Parent, bool LittleEndian) : BaseUnmanagedArrayHelper<ushort>
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override ushort[] this[Span<byte> source]
        {
            get => GetU16Array(source, LittleEndian);
            set => SetU16Array(source, value, LittleEndian);
        }

        /// <inheritdoc />
        public override ushort[] this[ReadOnlySpan<byte> source] => GetU16Array(source, LittleEndian);
    }

    /// <summary>
    /// Unsigned 32-bit helper.
    /// </summary>
    public record U32Helper(Processor Parent, bool LittleEndian) : BaseUnmanagedHelper<uint>
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <summary>
        /// Read/write value.
        /// </summary>
        /// <param name="source">Data source.</param>
        public override uint this[Span<byte> source]
        {
            get => GetU32(source, LittleEndian);
            set => SetU32(source, value, LittleEndian);
        }

        /// <summary>
        /// Read value.
        /// </summary>
        /// <param name="source">Data source.</param>
        public override uint this[ReadOnlySpan<byte> source] => Parent.GetU32(source);
    }

    /// <summary>
    /// Unsigned 32-bit helper.
    /// </summary>
    public record U32ArrayHelper(Processor Parent, bool LittleEndian) : BaseUnmanagedArrayHelper<uint>
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override uint[] this[Span<byte> source]
        {
            get => GetU32Array(source, LittleEndian);
            set => SetU32Array(source, value, LittleEndian);
        }

        /// <inheritdoc />
        public override uint[] this[ReadOnlySpan<byte> source] => GetU32Array(source, LittleEndian);
    }

    /// <summary>
    /// Unsigned 64-bit helper.
    /// </summary>
    public record U64Helper(Processor Parent, bool LittleEndian) : BaseUnmanagedHelper<ulong>
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <summary>
        /// Read/write value.
        /// </summary>
        /// <param name="source">Data source.</param>
        public override ulong this[Span<byte> source]
        {
            get => GetU64(source, LittleEndian);
            set => SetU64(source, value, LittleEndian);
        }

        /// <summary>
        /// Read value.
        /// </summary>
        /// <param name="source">Data source.</param>
        public override ulong this[ReadOnlySpan<byte> source] => Parent.GetU64(source);
    }

    /// <summary>
    /// Unsigned 64-bit helper.
    /// </summary>
    public record U64ArrayHelper(Processor Parent, bool LittleEndian) : BaseUnmanagedArrayHelper<ulong>
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override ulong[] this[Span<byte> source]
        {
            get => GetU64Array(source, LittleEndian);
            set => SetU64Array(source, value, LittleEndian);
        }

        /// <inheritdoc />
        public override ulong[] this[ReadOnlySpan<byte> source] => GetU64Array(source, LittleEndian);
    }

    /// <summary>
    /// Generic helper.
    /// </summary>
    public record THelper<T>(Processor Parent) : BaseUnmanagedHelper<T> where T : unmanaged
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override T this[Span<byte> source]
        {
            get => MemoryMarshal.Cast<byte, T>(source)[0];
            set => MemoryMarshal.Cast<byte, T>(source)[0] = value;
        }

        /// <inheritdoc />
        public override T this[ReadOnlySpan<byte> source] =>
            MemoryMarshal.Cast<byte, T>(source)[0];
    }

    /// <summary>
    /// Generic helper.
    /// </summary>
    public record TArrayHelper<T>(Processor Parent) : BaseUnmanagedArrayHelper<T> where T : unmanaged
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override T[] this[Span<byte> source]
        {
            get => GetTArray<T>(source);
            set => SetTArray(source, value);
        }

        /// <inheritdoc />
        public override T[] this[ReadOnlySpan<byte> source] =>
            GetTArray<T>(source);
    }

    /// <summary>
    /// Unsigned 8-bit helper.
    /// </summary>
    public record F16Helper(Processor Parent) : BaseHelper<float>
    {
        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <summary>
        /// Read/write value.
        /// </summary>
        /// <param name="source">Data source.</param>
        public override float this[Span<byte> source]
        {
            get => this[(ReadOnlySpan<byte>)source];
            set => GetBytesHalf(value, source);
        }

        /// <summary>
        /// Read value.
        /// </summary>
        /// <param name="source">Data source.</param>
        public override float this[ReadOnlySpan<byte> source] => GetHalf(source);

        /// <inheritdoc />
        public override float this[long offset, Stream stream]
        {
            get => offset != -1 ? Parent.ReadHalf(offset, stream) : Parent.ReadHalf(stream);
            set => Parent.WriteHalf(value, stream, offset != -1 ? offset : null);
        }
    }

    /// <summary>
    /// Unsigned 8-bit helper.
    /// </summary>
    public record F16ArrayHelper(Processor Parent) : BaseUnmanagedArrayHelper<float>
    {
        /// <inheritdoc />
        public override int ElementSize => 2;

        /// <inheritdoc />
        public override Stream InputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <inheritdoc />
        public override Stream OutputStream => Parent._inputStream ?? throw new InvalidOperationException();

        /// <summary>
        /// Read / write value.
        /// </summary>
        /// <param name="source">Data source.</param>
        public override float[] this[Span<byte> source]
        {
            get => this[(ReadOnlySpan<byte>)source];
            set
            {
                byte[] src = new byte[value.Length * 2];
                ConvertFloatArrayToHalf(value, src);
                src.CopyTo(source);
            }
        }

        /// <inheritdoc />
        public override float[] this[ReadOnlySpan<byte> source]
        {
            get
            {
                float[] result = new float[source.Length / 2];
                // New array for aligning
                ConvertHalfArrayToFloat(source.Slice(0, source.Length / 2 * 2).ToArray(), result);
                return result;
            }
        }
    }
}
