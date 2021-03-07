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
        /// Helper for unsigned 8-bit integers.
        /// </summary>
        public THelper<byte> U8 = null!;

        /// <summary>
        /// Helper for unsigned 8-bit integer arrays.
        /// </summary>
        public TArrayHelper<byte> U8A = null!;


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
            U8 = new THelper<byte>(this);
            U8A = new TArrayHelper<byte>(this);
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
            public virtual T this[byte[] source, int offset]
            {
                get => this[source.AsSpan(), offset];
                set => this[source.AsSpan(), offset] = value;
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            public virtual T this[byte[] source]
            {
                get => this[source.AsSpan(), 0];
                set => this[source.AsSpan(), 0] = value;
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public virtual T this[Memory<byte> source, int offset]
            {
                get => this[source.Span, offset];
                set => this[source.Span, offset] = value;
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            public virtual T this[Memory<byte> source]
            {
                get => this[source.Span, 0];
                set => this[source.Span, 0] = value;
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public virtual T this[Span<byte> source, int offset]
            {
                get => this[source.Slice(offset)];
                set => this[source.Slice(offset)] = value;
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            public abstract T this[Span<byte> source] { get; set; }

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public virtual T this[ReadOnlyMemory<byte> source, int offset] => this[source.Span, offset];

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            public virtual T this[ReadOnlyMemory<byte> source] => this[source.Span, 0];

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public virtual T this[ReadOnlySpan<byte> source, int offset] => this[source.Slice(offset)];

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            public abstract T this[ReadOnlySpan<byte> source] { get; }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="stream">Data source.</param>
            /// <param name="offset">Offset (no seeking if -1).</param>
            public virtual T this[long offset, Stream stream]
            {
                get
                {
                    Span<byte> span = stackalloc byte[sizeof(T)];
                    if (offset != -1) Read(stream, offset, span, false);
                    else Read(stream, span, false);
                    return this[span];
                }
                set
                {
                    Span<byte> span = stackalloc byte[sizeof(T)];
                    this[span] = value;
                    if (offset != -1) Write(stream, offset, span);
                    else Write(stream, span);
                }
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="stream">Data source.</param>
            /// <param name="offset">Offset (no seeking if -1).</param>
            public virtual T this[int offset, Stream stream]
            {
                get => this[(long)offset, stream];
                set => this[(long)offset, stream] = value;
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="offset">Offset (no seeking if -1).</param>
            public virtual T this[long offset]
            {
                get => this[offset, InputStream];
                set => this[offset, OutputStream] = value;
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="offset">Offset (no seeking if -1).</param>
            public virtual T this[int offset]
            {
                get => this[offset, InputStream];
                set => this[offset, OutputStream] = value;
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
            public virtual T[] this[byte[] source, int offset, int count] =>
                this[source.AsSpan(offset, count * sizeof(T))];

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
            /// Write array.
            /// </summary>
            /// <param name="source">Data source.</param>
            public virtual T[] this[byte[] source]
            {
                get => this[source.AsSpan()];
                set => this[source.AsSpan()] = value;
            }

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            /// <param name="count">Element count.</param>
            public virtual T[] this[Memory<byte> source, int offset, int count] =>
                this[source.Span.Slice(offset, count * sizeof(T))];

            /// <summary>
            /// Write array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            public virtual T[] this[Memory<byte> source, int offset]
            {
                set => this[source.Span.Slice(offset)] = value;
            }

            /// <summary>
            /// Write array.
            /// </summary>
            /// <param name="source">Data source.</param>
            public virtual T[] this[Memory<byte> source]
            {
                get => this[source.Span];
                set => this[source.Span] = value;
            }

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            /// <param name="count">Element count.</param>
            public virtual T[] this[Span<byte> source, int offset, int count] =>
                this[source.Slice(offset, count * sizeof(T))];

            /// <summary>
            /// Write array.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public virtual T[] this[Span<byte> source, int offset]
            {
                set => this[source.Slice(offset)] = value;
            }

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="source">Data source.</param>
            public abstract T[] this[Span<byte> source] { get; set; }

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            /// <param name="count">Element count.</param>
            public virtual T[] this[ReadOnlyMemory<byte> source, int offset, int count] =>
                this[source.Span, offset, count];

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="source">Data source.</param>
            public virtual T[] this[ReadOnlyMemory<byte> source] =>
                this[source.Span];

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            /// <param name="count">Element count.</param>
            public virtual T[] this[ReadOnlySpan<byte> source, int offset, int count] =>
                this[source.Slice(offset, count * sizeof(T))];

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="source">Data source.</param>
            public abstract T[] this[ReadOnlySpan<byte> source] { get; }

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="offset">Offset (no seeking if -1).</param>
            /// <param name="count">Element count.</param>
            /// <param name="stream">Data source.</param>
            public virtual T[] this[long offset, int count, Stream stream]
            {
                get
                {
                    byte[] arr = new byte[count * sizeof(T)];
                    if (offset != -1) Read(stream, offset, arr, 0, arr.Length, false);
                    else Read(stream, arr, 0, arr.Length, false);
                    return this[arr, 0, count];
                }
            }

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="offset">Offset (no seeking if -1).</param>
            /// <param name="count">Element count.</param>
            /// <param name="stream">Data source.</param>
            public virtual T[] this[int offset, int count, Stream stream] => this[(long)offset, count, stream];

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="offset">Offset (no seeking if -1).</param>
            /// <param name="count">Element count.</param>
            public virtual T[] this[long offset, int count] => this[offset, count, InputStream];

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="offset">Offset (no seeking if -1).</param>
            /// <param name="count">Element count.</param>
            public virtual T[] this[int offset, int count] => this[(long)offset, count];

            /// <summary>
            /// Write value.
            /// </summary>
            /// <param name="offset">Offset (no seeking if -1).</param>
            /// <param name="stream">Data source.</param>
            public virtual T[] this[long offset, Stream stream]
            {
                set
                {
                    byte[] arr = new byte[value.Length * sizeof(T)];
                    this[arr, 0] = value;
                    if (offset != -1) Write(stream, offset, arr);
                    else Write(stream, arr);
                }
            }

            /// <summary>
            /// Write value.
            /// </summary>
            /// <param name="offset">Offset (no seeking if -1).</param>
            /// <param name="stream">Data source.</param>
            public virtual T[] this[int offset, Stream stream]
            {
                set => this[(long)offset, stream] = value;
            }

            /// <summary>
            /// Write value.
            /// </summary>
            /// <param name="offset">Offset (no seeking if -1).</param>
            public virtual T[] this[long offset]
            {
                set => this[offset, OutputStream] = value;
            }

            /// <summary>
            /// Write value.
            /// </summary>
            /// <param name="offset">Offset (no seeking if -1).</param>
            public virtual T[] this[int offset]
            {
                set => this[(long)offset, OutputStream] = value;
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
        public record S8ArrayHelper(Processor Parent) : BaseArrayHelper<sbyte>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

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
        public record S16Helper(Processor Parent, bool LittleEndian) : BaseHelper<short>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

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
        public record S16ArrayHelper(Processor Parent, bool LittleEndian) : BaseArrayHelper<short>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

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
        public record S32Helper(Processor Parent, bool LittleEndian) : BaseHelper<int>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

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
        public record S32ArrayHelper(Processor Parent, bool LittleEndian) : BaseArrayHelper<int>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

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
        public record S64Helper(Processor Parent, bool LittleEndian) : BaseHelper<long>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

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
        public record S64ArrayHelper(Processor Parent, bool LittleEndian) : BaseArrayHelper<long>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

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
        public record U8Helper(Processor Parent, bool LittleEndian) : BaseHelper<byte>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

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
        public record U8ArrayHelper(Processor Parent, bool LittleEndian) : BaseArrayHelper<byte>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

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
        public record U16Helper(Processor Parent, bool LittleEndian) : BaseHelper<ushort>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

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
        public record U16ArrayHelper(Processor Parent, bool LittleEndian) : BaseArrayHelper<ushort>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

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
        public record U32Helper(Processor Parent, bool LittleEndian) : BaseHelper<uint>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

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
        public record U32ArrayHelper(Processor Parent, bool LittleEndian) : BaseArrayHelper<uint>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

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
        public record U64Helper(Processor Parent, bool LittleEndian) : BaseHelper<ulong>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

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
        public record U64ArrayHelper(Processor Parent, bool LittleEndian) : BaseArrayHelper<ulong>
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

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
        public record THelper<T>(Processor Parent) : BaseHelper<T> where T : unmanaged
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

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
        public record TArrayHelper<T>(Processor Parent) : BaseArrayHelper<T> where T : unmanaged
        {
            /// <inheritdoc />
            public override Stream InputStream => Parent.InputStream ?? throw new InvalidOperationException();

            /// <inheritdoc />
            public override Stream OutputStream => Parent.InputStream ?? throw new InvalidOperationException();

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
}
