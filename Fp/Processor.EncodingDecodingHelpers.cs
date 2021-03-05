using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Fp
{
    public partial class Processor
    {
        /// <summary>
        /// Helper for signed 8-bit integers.
        /// </summary>
        public THelper<sbyte> S8 = null!;

        /// <summary>
        /// Helper for signed 8-bit integer arrays.
        /// </summary>
        public TArrayHelper<sbyte> S8A = null!;

        /// <summary>
        /// Helper for signed 16-bit integers.
        /// </summary>
        public S16Helper S16 = null!;

        /// <summary>
        /// Helper for signed 16-bit integer arrays.
        /// </summary>
        public S16ArrayHelper S16A = null!;

        /// <summary>
        /// Helper for signed 32-bit integers.
        /// </summary>
        public S32Helper S32 = null!;

        /// <summary>
        /// Helper for signed 32-bit integer arrays.
        /// </summary>
        public S32ArrayHelper S32A = null!;

        /// <summary>
        /// Helper for signed 64-bit integers.
        /// </summary>
        public S64Helper S64 = null!;

        /// <summary>
        /// Helper for signed 64-bit integer arrays.
        /// </summary>
        public S64ArrayHelper S64A = null!;

        /// <summary>
        /// Helper for unsigned 8-bit integers.
        /// </summary>
        public THelper<byte> U8 = null!;

        /// <summary>
        /// Helper for unsigned 8-bit integer arrays.
        /// </summary>
        public TArrayHelper<byte> U8A = null!;

        /// <summary>
        /// Helper for unsigned 16-bit integers.
        /// </summary>
        public U16Helper U16 = null!;

        /// <summary>
        /// Helper for unsigned 16-bit integer arrays.
        /// </summary>
        public U16ArrayHelper U16A = null!;

        /// <summary>
        /// Helper for unsigned 32-bit integers.
        /// </summary>
        public U32Helper U32 = null!;

        /// <summary>
        /// Helper for unsigned 32-bit integer arrays.
        /// </summary>
        public U32ArrayHelper U32A = null!;

        /// <summary>
        /// Helper for unsigned 64-bit integers.
        /// </summary>
        public U64Helper U64 = null!;

        /// <summary>
        /// Helper for unsigned 64-bit integer arrays.
        /// </summary>
        public U64ArrayHelper U64A = null!;

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

        private void InitEncodingDecodingHelpers()
        {
            S8 = new THelper<sbyte>(this);
            S8A = new TArrayHelper<sbyte>(this);
            S16 = new S16Helper(this);
            S16A = new S16ArrayHelper(this);
            S32 = new S32Helper(this);
            S32A = new S32ArrayHelper(this);
            S64 = new S64Helper(this);
            S64A = new S64ArrayHelper(this);
            U8 = new THelper<byte>(this);
            U8A = new TArrayHelper<byte>(this);
            U16 = new U16Helper(this);
            U16A = new U16ArrayHelper(this);
            U32 = new U32Helper(this);
            U32A = new U32ArrayHelper(this);
            U64 = new U64Helper(this);
            U64A = new U64ArrayHelper(this);
            F32 = new THelper<float>(this);
            F32A = new TArrayHelper<float>(this);
            F64 = new THelper<double>(this);
            F64A = new TArrayHelper<double>(this);
        }

        /// <summary>
        /// Base data helper.
        /// </summary>
        public abstract record Helper
        {
            /// <summary>
            /// Current input stream.
            /// </summary>
            public abstract Stream InputStream { get; }

            /// <summary>
            /// Current output stream.
            /// </summary>
            public abstract Stream OutputStream { get; }
        }

        /// <summary>
        /// Base single-unit data helper.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        public abstract unsafe record BaseHelper<T> : Helper where T : unmanaged
        {
            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public virtual T this[byte[] source, int offset = 0]
            {
                get => this[source.AsSpan(), offset];
                set => this[source.AsSpan(), offset] = value;
            }


            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public virtual T this[Memory<byte> source, int offset = 0]
            {
                get => this[source.Span, offset];
                set => this[source.Span, offset] = value;
            }


            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public abstract T this[Span<byte> source, int offset = 0] { get; set; }

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public virtual T this[ReadOnlyMemory<byte> source, int offset = 0] => this[source.Span, offset];

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public abstract T this[ReadOnlySpan<byte> source, int offset = 0] { get; }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="stream">Data source (<see cref="InputStream"/> / <see cref="OutputStream"/> when null).</param>
            /// <param name="offset">Offset (no seeking if -1).</param>
            public virtual T this[long offset = -1, Stream? stream = null]
            {
                get
                {
                    stream ??= InputStream;
                    Span<byte> span = stackalloc byte[sizeof(T)];
                    if (offset != -1) Read(stream, offset, span, false);
                    else Read(stream, span, false);
                    return this[span];
                }
                set
                {
                    stream ??= OutputStream;
                    Span<byte> span = stackalloc byte[sizeof(T)];
                    this[span] = value;
                    if (offset != -1) Write(stream, offset, span);
                    else Write(stream, span);
                }
            }
        }

        /// <summary>
        /// Base array data helper.
        /// </summary>
        /// <typeparam name="T">Element type.</typeparam>
        public abstract unsafe record BaseArrayHelper<T> : Helper where T : unmanaged
        {
            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            /// <param name="count">Element count.</param>
            public virtual T[] this[byte[] source, int offset, int count] => this[source.AsSpan(offset), count];

            /// <summary>
            /// Write array.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public virtual T[] this[byte[] source, int offset]
            {
                set => this[source.AsSpan(offset)] = value;
            }

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public virtual T[] this[Memory<byte> source, int count] => this[source.Span, count];


            /// <summary>
            /// Write array.
            /// </summary>
            /// <param name="source">Data source.</param>
            public virtual T[] this[Memory<byte> source]
            {
                set => this[source.Span] = value;
            }

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public abstract T[] this[Span<byte> source, int count] { get; }

            /// <summary>
            /// Write array.
            /// </summary>
            /// <param name="source">Data source.</param>
            public abstract T[] this[Span<byte> source] { set; }

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public virtual T[] this[ReadOnlyMemory<byte> source, int count] => this[source.Span, count];

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public abstract T[] this[ReadOnlySpan<byte> source, int count] { get; }

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="offset">Offset (no seeking if -1).</param>
            /// <param name="count">Element count.</param>
            /// <param name="stream">Data source (<see cref="InputStream"/> / <see cref="OutputStream"/> when null).</param>
            public virtual T[] this[long offset, int count, Stream? stream = null]
            {
                get
                {
                    stream ??= InputStream;
                    byte[] arr = new byte[count * sizeof(T)];
                    if (offset != -1) Read(stream, offset, arr, 0, arr.Length, false);
                    else Read(stream, arr, 0, arr.Length, false);
                    return this[arr, 0, count];
                }
            }

            /// <summary>
            /// Write value.
            /// </summary>
            /// <param name="offset">Offset (no seeking if -1).</param>
            /// <param name="stream">Data source (<see cref="InputStream"/> / <see cref="OutputStream"/> when null).</param>
            public virtual T[] this[long offset, Stream? stream = null]
            {
                set
                {
                    stream ??= OutputStream;
                    byte[] arr = new byte[value.Length * sizeof(T)];
                    this[arr, 0] = value;
                    if (offset != -1) Write(stream, offset, arr);
                    else Write(stream, arr);
                }
            }
        }

        /// <summary>
        /// Signed 8-bit helper.
        /// </summary>
        public record S8Helper(Processor Parent) : BaseHelper<sbyte>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public override sbyte this[Span<byte> source, int offset = 0]
            {
                get => Parent.GetS8(source, offset);
                set => Parent.SetS8(source, value, offset);
            }

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public override sbyte this[ReadOnlySpan<byte> source, int offset = 0] => Parent.GetS8(source, offset);
        }

        /// <summary>
        /// Signed 8-bit array helper.
        /// </summary>
        public record S8ArrayHelper(Processor Parent) : BaseArrayHelper<sbyte>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override sbyte[] this[Span<byte> source, int count] =>
                Parent.GetS8Array(
                    count == -1 ? source : source.Slice(0, count * sizeof(sbyte)));

            /// <inheritdoc />
            public override sbyte[] this[Span<byte> source]
            {
                set => Parent.SetS8Array(source, value);
            }

            /// <inheritdoc />
            public override sbyte[] this[ReadOnlySpan<byte> source, int count] =>
                Parent.GetS8Array(count == -1 ? source : source.Slice(0, count * sizeof(sbyte)));
        }

        /// <summary>
        /// Signed 16-bit helper.
        /// </summary>
        public record S16Helper(Processor Parent) : BaseHelper<short>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override short this[Span<byte> source, int offset = 0]
            {
                get => Parent.GetS16(source, offset);
                set => Parent.SetS16(source, value, offset);
            }


            /// <inheritdoc />
            public override short this[ReadOnlySpan<byte> source, int offset = 0] => Parent.GetS16(source, offset);
        }

        /// <summary>
        /// Signed 16-bit array helper.
        /// </summary>
        public record S16ArrayHelper(Processor Parent) : BaseArrayHelper<short>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override short[] this[Span<byte> source, int count] =>
                Parent.GetS16Array(
                    count == -1 ? source : source.Slice(0, count * sizeof(short)));

            /// <inheritdoc />
            public override short[] this[Span<byte> source]
            {
                set => Parent.SetS16Array(source, value);
            }

            /// <inheritdoc />
            public override short[] this[ReadOnlySpan<byte> source, int count] =>
                Parent.GetS16Array(count == -1 ? source : source.Slice(0, count * sizeof(short)));
        }

        /// <summary>
        /// Signed 32-bit helper.
        /// </summary>
        public record S32Helper(Processor Parent) : BaseHelper<int>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override int this[Span<byte> source, int offset = 0]
            {
                get => Parent.GetS32(source, offset);
                set => Parent.SetS32(source, value, offset);
            }

            /// <inheritdoc />
            public override int this[ReadOnlySpan<byte> source, int offset = 0] => Parent.GetS32(source, offset);
        }

        /// <summary>
        /// Signed 32-bit helper.
        /// </summary>
        public record S32ArrayHelper(Processor Parent) : BaseArrayHelper<int>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override int[] this[Span<byte> source, int count] =>
                Parent.GetS32Array(
                    count == -1 ? source : source.Slice(0, count * sizeof(int)));

            /// <inheritdoc />
            public override int[] this[Span<byte> source]
            {
                set => Parent.SetS32Array(source, value);
            }

            /// <inheritdoc />
            public override int[] this[ReadOnlySpan<byte> source, int count] =>
                Parent.GetS32Array(count == -1 ? source : source.Slice(0, count * sizeof(int)));
        }

        /// <summary>
        /// Signed 64-bit helper.
        /// </summary>
        public record S64Helper(Processor Parent) : BaseHelper<long>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override long this[Span<byte> source, int offset = 0]
            {
                get => Parent.GetS64(source, offset);
                set => Parent.SetS64(source, value, offset);
            }

            /// <inheritdoc />
            public override long this[ReadOnlySpan<byte> source, int offset = 0] => Parent.GetS64(source, offset);
        }

        /// <summary>
        /// Signed 64-bit helper.
        /// </summary>
        public record S64ArrayHelper(Processor Parent) : BaseArrayHelper<long>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override long[] this[Span<byte> source, int count] =>
                Parent.GetS64Array(
                    count == -1 ? source : source.Slice(0, count * sizeof(long)));

            /// <inheritdoc />
            public override long[] this[Span<byte> source]
            {
                set => Parent.SetS64Array(source, value);
            }

            /// <inheritdoc />
            public override long[] this[ReadOnlySpan<byte> source, int count] =>
                Parent.GetS64Array(count == -1 ? source : source.Slice(0, count * sizeof(long)));
        }

        /// <summary>
        /// Unsigned 8-bit helper.
        /// </summary>
        public record U8Helper(Processor Parent) : BaseHelper<byte>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override byte this[Span<byte> source, int offset = 0]
            {
                get => Parent.GetU8(source, offset);
                set => Parent.SetU8(source, value, offset);
            }

            /// <inheritdoc />
            public override byte this[ReadOnlySpan<byte> source, int offset = 0] => Parent.GetU8(source, offset);
        }

        /// <summary>
        /// Unsigned 8-bit helper.
        /// </summary>
        public record U8ArrayHelper(Processor Parent) : BaseArrayHelper<byte>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override byte[] this[Span<byte> source, int count] =>
                Parent.GetU8Array(
                    count == -1 ? source : source.Slice(0, count * sizeof(byte)));

            /// <inheritdoc />
            public override byte[] this[Span<byte> source]
            {
                set => Parent.SetU8Array(source, value);
            }

            /// <inheritdoc />
            public override byte[] this[ReadOnlySpan<byte> source, int count] =>
                Parent.GetU8Array(count == -1 ? source : source.Slice(0, count * sizeof(byte)));
        }

        /// <summary>
        /// Unsigned 16-bit helper.
        /// </summary>
        public record U16Helper(Processor Parent) : BaseHelper<ushort>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override ushort this[Span<byte> source, int offset = 0]
            {
                get => Parent.GetU16(source, offset);
                set => Parent.SetU16(source, value, offset);
            }

            /// <inheritdoc />
            public override ushort this[ReadOnlySpan<byte> source, int offset = 0] => Parent.GetU16(source, offset);
        }

        /// <summary>
        /// Unsigned 16-bit helper.
        /// </summary>
        public record U16ArrayHelper(Processor Parent) : BaseArrayHelper<ushort>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override ushort[] this[Span<byte> source, int count] =>
                Parent.GetU16Array(
                    count == -1 ? source : source.Slice(0, count * sizeof(ushort)));

            /// <inheritdoc />
            public override ushort[] this[Span<byte> source]
            {
                set => Parent.SetU16Array(source, value);
            }

            /// <inheritdoc />
            public override ushort[] this[ReadOnlySpan<byte> source, int count] =>
                Parent.GetU16Array(count == -1 ? source : source.Slice(0, count * sizeof(ushort)));
        }

        /// <summary>
        /// Unsigned 32-bit helper.
        /// </summary>
        public record U32Helper(Processor Parent) : BaseHelper<uint>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override uint this[Span<byte> source, int offset = 0]
            {
                get => Parent.GetU32(source, offset);
                set => Parent.SetU32(source, value, offset);
            }

            /// <inheritdoc />
            public override uint this[ReadOnlySpan<byte> source, int offset = 0] => Parent.GetU32(source, offset);
        }

        /// <summary>
        /// Unsigned 32-bit helper.
        /// </summary>
        public record U32ArrayHelper(Processor Parent) : BaseArrayHelper<uint>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override uint[] this[Span<byte> source, int count] =>
                Parent.GetU32Array(
                    count == -1 ? source : source.Slice(0, count * sizeof(ushort)));

            /// <inheritdoc />
            public override uint[] this[Span<byte> source]
            {
                set => Parent.SetU32Array(source, value);
            }

            /// <inheritdoc />
            public override uint[] this[ReadOnlySpan<byte> source, int count] =>
                Parent.GetU32Array(count == -1 ? source : source.Slice(0, count * sizeof(ushort)));
        }

        /// <summary>
        /// Unsigned 64-bit helper.
        /// </summary>
        public record U64Helper(Processor Parent) : BaseHelper<ulong>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override ulong this[Span<byte> source, int offset = 0]
            {
                get => Parent.GetU64(source, offset);
                set => Parent.SetU64(source, value, offset);
            }

            /// <inheritdoc />
            public override ulong this[ReadOnlySpan<byte> source, int offset = 0] => Parent.GetU64(source, offset);
        }

        /// <summary>
        /// Unsigned 64-bit helper.
        /// </summary>
        public record U64ArrayHelper(Processor Parent) : BaseArrayHelper<ulong>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override ulong[] this[Span<byte> source, int count] =>
                Parent.GetU64Array(
                    count == -1 ? source : source.Slice(0, count * sizeof(ulong)));

            /// <inheritdoc />
            public override ulong[] this[Span<byte> source]
            {
                set => Parent.SetU64Array(source, value);
            }

            /// <inheritdoc />
            public override ulong[] this[ReadOnlySpan<byte> source, int count] =>
                Parent.GetU64Array(count == -1 ? source : source.Slice(0, count * sizeof(ulong)));
        }

        /// <summary>
        /// Generic helper.
        /// </summary>
        public record THelper<T>(Processor Parent) : BaseHelper<T> where T : unmanaged
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override T this[Span<byte> source, int offset = 0]
            {
                get => MemoryMarshal.Cast<byte, T>(source)[offset];
                set => MemoryMarshal.Cast<byte, T>(source)[offset] = value;
            }

            /// <inheritdoc />
            public override T this[ReadOnlySpan<byte> source, int offset = 0] =>
                MemoryMarshal.Cast<byte, T>(source)[offset];
        }

        /// <summary>
        /// Generic helper.
        /// </summary>
        public unsafe record TArrayHelper<T>(Processor Parent) : BaseArrayHelper<T> where T : unmanaged
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override T[] this[Span<byte> source, int count] =>
                Parent.GetTArray<T>(
                    count == -1 ? source : source.Slice(0, count * sizeof(T)));

            /// <inheritdoc />
            public override T[] this[Span<byte> source]
            {
                set => Parent.SetTArray(source, value);
            }

            /// <inheritdoc />
            public override T[] this[ReadOnlySpan<byte> source, int count] =>
                Parent.GetTArray<T>(count == -1 ? source : source.Slice(0, count * sizeof(T)));
        }

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <returns>Converted array.</returns>
        public sbyte[] GetS8Array(ReadOnlySpan<byte> span)
        {
            return MemoryMarshal.Cast<byte, sbyte>(span).ToArray();
        }

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        /// <returns>Converted array.</returns>
        public void SetS8Array(Span<byte> span, sbyte[] array)
        {
            MemoryMarshal.Cast<sbyte, byte>(array).CopyTo(span);
        }

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <returns>Converted array.</returns>
        public short[] GetS16Array(ReadOnlySpan<byte> span)
        {
            short[] result = MemoryMarshal.Cast<byte, short>(span).ToArray();
            ConvertS16Array(result);
            return result;
        }

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        /// <returns>Converted array.</returns>
        public void SetS16Array(Span<byte> span, short[] array)
        {
            MemoryMarshal.Cast<short, byte>(array).CopyTo(span);
            ConvertS16Array(span);
        }

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <returns>Converted array.</returns>
        public int[] GetS32Array(ReadOnlySpan<byte> span)
        {
            int[] result = MemoryMarshal.Cast<byte, int>(span).ToArray();
            ConvertS32Array(result);
            return result;
        }

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        /// <returns>Converted array.</returns>
        public void SetS32Array(Span<byte> span, int[] array)
        {
            MemoryMarshal.Cast<int, byte>(array).CopyTo(span);
            ConvertS32Array(span);
        }

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <returns>Converted array.</returns>
        public long[] GetS64Array(ReadOnlySpan<byte> span)
        {
            long[] result = MemoryMarshal.Cast<byte, long>(span).ToArray();
            ConvertS64Array(result);
            return result;
        }

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        /// <returns>Converted array.</returns>
        public void SetS64Array(Span<byte> span, long[] array)
        {
            MemoryMarshal.Cast<long, byte>(array).CopyTo(span);
            ConvertS64Array(span);
        }

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <returns>Converted array.</returns>
        public byte[] GetU8Array(ReadOnlySpan<byte> span)
        {
            return span.ToArray();
        }

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        /// <returns>Converted array.</returns>
        public void SetU8Array(Span<byte> span, byte[] array)
        {
            array.AsSpan().CopyTo(span);
        }

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <returns>Converted array.</returns>
        public ushort[] GetU16Array(ReadOnlySpan<byte> span)
        {
            ushort[] result = MemoryMarshal.Cast<byte, ushort>(span).ToArray();
            ConvertU16Array(result);
            return result;
        }

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        /// <returns>Converted array.</returns>
        public void SetU16Array(Span<byte> span, ushort[] array)
        {
            MemoryMarshal.Cast<ushort, byte>(array).CopyTo(span);
            ConvertS32Array(span);
        }

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <returns>Converted array.</returns>
        public uint[] GetU32Array(ReadOnlySpan<byte> span)
        {
            uint[] result = MemoryMarshal.Cast<byte, uint>(span).ToArray();
            ConvertU32Array(result);
            return result;
        }

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        /// <returns>Converted array.</returns>
        public void SetU32Array(Span<byte> span, uint[] array)
        {
            MemoryMarshal.Cast<uint, byte>(array).CopyTo(span);
            ConvertS32Array(span);
        }

        /// <summary>
        /// Read converted array (with endianness switch).
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <returns>Converted array.</returns>
        public ulong[] GetU64Array(ReadOnlySpan<byte> span)
        {
            ulong[] result = MemoryMarshal.Cast<byte, ulong>(span).ToArray();
            ConvertU64Array(result);
            return result;
        }

        /// <summary>
        /// Write array (with endianness switch).
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        /// <returns>Converted array.</returns>
        public void SetU64Array(Span<byte> span, ulong[] array)
        {
            MemoryMarshal.Cast<ulong, byte>(array).CopyTo(span);
            ConvertS32Array(span);
        }

        /// <summary>
        /// Get generic array.
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <typeparam name="T">Element type.</typeparam>
        /// <returns>Read data.</returns>
        public T[] GetTArray<T>(ReadOnlySpan<byte> span) where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(span).ToArray();
        }

        /// <summary>
        /// Set generic array.
        /// </summary>
        /// <param name="span">Target span.</param>
        /// <param name="array">Source array.</param>
        /// <typeparam name="T">Element type.</typeparam>
        public void SetTArray<T>(Span<byte> span, T[] array) where T : unmanaged
        {
            MemoryMarshal.Cast<T, byte>(array).CopyTo(span);
        }
    }
}
