using System;
using System.Runtime.InteropServices;

namespace Fp
{
    public partial class Processor
    {
        /// <summary>
        /// Helper for signed 8-bit integers.
        /// </summary>
        public THelper<sbyte> S8;

        /// <summary>
        /// Helper for signed 16-bit integers.
        /// </summary>
        public S16Helper S16;

        /// <summary>
        /// Helper for signed 32-bit integers.
        /// </summary>
        public S32Helper S32;

        /// <summary>
        /// Helper for signed 64-bit integers.
        /// </summary>
        public S64Helper S64;

        /// <summary>
        /// Helper for unsigned 8-bit integers.
        /// </summary>
        public THelper<byte> U8;

        /// <summary>
        /// Helper for unsigned 16-bit integers.
        /// </summary>
        public U16Helper U16;

        /// <summary>
        /// Helper for unsigned 32-bit integers.
        /// </summary>
        public U32Helper U32;

        /// <summary>
        /// Helper for unsigned 64-bit integers.
        /// </summary>
        public U64Helper U64;

        /// <summary>
        /// Helper for 32-bit floating point numbers.
        /// </summary>
        public THelper<float> F32;

        /// <summary>
        /// Helper for 64-bit floating point numbers.
        /// </summary>
        public THelper<double> F64;

        private void InitEncodingDecodingHelpers()
        {
            S8 = new THelper<sbyte>(this);
            S16 = new S16Helper(this);
            S32 = new S32Helper(this);
            S64 = new S64Helper(this);
            U8 = new THelper<byte>(this);
            U16 = new U16Helper(this);
            U32 = new U32Helper(this);
            U64 = new U64Helper(this);
            F32 = new THelper<float>(this);
            F64 = new THelper<double>(this);
        }

        /// <summary>
        /// Signed 8-bit helper.
        /// </summary>
        public readonly struct S8Helper
        {
            /// <summary>
            /// Parent instance.
            /// </summary>
            public readonly Processor Parent;

            /// <summary>
            /// Creates new instance of <see cref="S8Helper"/>.
            /// </summary>
            /// <param name="parent">Parent instance.</param>
            public S8Helper(Processor parent)
            {
                Parent = parent;
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public sbyte this[byte[] source, int offset = 0]
            {
                get => Parent.GetS8(source, offset);
                set => Parent.SetS8(source, value, offset);
            }


            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public sbyte this[Memory<byte> source, int offset = 0]
            {
                get => Parent.GetMS8(source, offset);
                set => Parent.SetMS8(source, value, offset);
            }


            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public sbyte this[Span<byte> source, int offset = 0]
            {
                get => Parent.GetS8(source, offset);
                set => Parent.SetS8(source, value, offset);
            }

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public sbyte this[ReadOnlyMemory<byte> source, int offset = 0] => Parent.GetMS8(source, offset);

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public sbyte this[ReadOnlySpan<byte> source, int offset = 0] => Parent.GetS8(source, offset);

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            /// <param name="count">Element count.</param>
            public sbyte[] this[int offset, byte[] source, int count = -1]
            {
                get => Parent.GetS8Array(count == -1
                    ? source.AsSpan(offset)
                    : source.AsSpan(offset, count * sizeof(sbyte)));
                set => Parent.SetS8Array(
                    count == -1 ? source.AsSpan(offset) : source.AsSpan(offset, count * sizeof(sbyte)), value);
            }

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public sbyte[] this[int offset, Memory<byte> source, int count = -1]
            {
                get => Parent.GetS8Array(count == -1
                    ? source.Span.Slice(offset)
                    : source.Span.Slice(offset, count * sizeof(sbyte)));
                set => Parent.SetS8Array(
                    count == -1 ? source.Span.Slice(offset) : source.Span.Slice(offset, count * sizeof(sbyte)), value);
            }

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public sbyte[] this[int offset, Span<byte> source, int count = -1]
            {
                get => Parent.GetS8Array(
                    count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(sbyte)));
                set
                {
                    Parent.SetS8Array(count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(sbyte)),
                        value);
                }
            }

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public sbyte[] this[int offset, ReadOnlyMemory<byte> source, int count = -1] =>
                Parent.GetS8Array(count == -1
                    ? source.Span.Slice(offset)
                    : source.Span.Slice(offset, count * sizeof(sbyte)));

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public sbyte[] this[int offset, ReadOnlySpan<byte> source, int count = -1] =>
                Parent.GetS8Array(count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(sbyte)));
        }

        /// <summary>
        /// Signed 16-bit helper.
        /// </summary>
        public readonly struct S16Helper
        {
            /// <summary>
            /// Parent instance.
            /// </summary>
            public readonly Processor Parent;

            /// <summary>
            /// Creates new instance of <see cref="S16Helper"/>.
            /// </summary>
            /// <param name="parent">Parent instance.</param>
            public S16Helper(Processor parent)
            {
                Parent = parent;
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public short this[byte[] source, int offset = 0]
            {
                get => Parent.GetS16(source, offset);
                set => Parent.SetS16(source, value, offset);
            }


            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public short this[Memory<byte> source, int offset = 0]
            {
                get => Parent.GetMS16(source, offset);
                set => Parent.SetMS16(source, value, offset);
            }


            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public short this[Span<byte> source, int offset = 0]
            {
                get => Parent.GetS16(source, offset);
                set => Parent.SetS16(source, value, offset);
            }

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public short this[ReadOnlyMemory<byte> source, int offset = 0] => Parent.GetMS16(source, offset);

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public short this[ReadOnlySpan<byte> source, int offset = 0] => Parent.GetS16(source, offset);

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            /// <param name="count">Element count.</param>
            public short[] this[int offset, byte[] source, int count = -1]
            {
                get => Parent.GetS16Array(count == -1
                    ? source.AsSpan(offset)
                    : source.AsSpan(offset, count * sizeof(short)));
                set => Parent.SetS16Array(
                    count == -1 ? source.AsSpan(offset) : source.AsSpan(offset, count * sizeof(short)), value);
            }

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public short[] this[int offset, Memory<byte> source, int count = -1]
            {
                get => Parent.GetS16Array(count == -1
                    ? source.Span.Slice(offset)
                    : source.Span.Slice(offset, count * sizeof(short)));
                set => Parent.SetS16Array(
                    count == -1 ? source.Span.Slice(offset) : source.Span.Slice(offset, count * sizeof(short)), value);
            }

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public short[] this[int offset, Span<byte> source, int count = -1]
            {
                get => Parent.GetS16Array(
                    count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(short)));
                set
                {
                    Parent.SetS16Array(count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(short)),
                        value);
                }
            }

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public short[] this[int offset, ReadOnlyMemory<byte> source, int count = -1] =>
                Parent.GetS16Array(count == -1
                    ? source.Span.Slice(offset)
                    : source.Span.Slice(offset, count * sizeof(short)));

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public short[] this[int offset, ReadOnlySpan<byte> source, int count = -1] =>
                Parent.GetS16Array(count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(short)));
        }

        /// <summary>
        /// Signed 32-bit helper.
        /// </summary>
        public readonly struct S32Helper
        {
            /// <summary>
            /// Parent instance.
            /// </summary>
            public readonly Processor Parent;

            /// <summary>
            /// Creates new instance of <see cref="S32Helper"/>.
            /// </summary>
            /// <param name="parent">Parent instance.</param>
            public S32Helper(Processor parent)
            {
                Parent = parent;
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public int this[byte[] source, int offset = 0]
            {
                get => Parent.GetS32(source, offset);
                set => Parent.SetS32(source, value, offset);
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public int this[Memory<byte> source, int offset = 0]
            {
                get => Parent.GetMS32(source, offset);
                set => Parent.SetMS32(source, value, offset);
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public int this[Span<byte> source, int offset = 0]
            {
                get => Parent.GetS32(source, offset);
                set => Parent.SetS32(source, value, offset);
            }

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public int this[ReadOnlyMemory<byte> source, int offset = 0] => Parent.GetMS32(source, offset);

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public int this[ReadOnlySpan<byte> source, int offset = 0] => Parent.GetS32(source, offset);

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            /// <param name="count">Element count.</param>
            public int[] this[int offset, byte[] source, int count = -1]
            {
                get => Parent.GetS32Array(count == -1
                    ? source.AsSpan(offset)
                    : source.AsSpan(offset, count * sizeof(int)));
                set => Parent.SetS32Array(
                    count == -1 ? source.AsSpan(offset) : source.AsSpan(offset, count * sizeof(int)), value);
            }

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public int[] this[int offset, Memory<byte> source, int count = -1]
            {
                get => Parent.GetS32Array(count == -1
                    ? source.Span.Slice(offset)
                    : source.Span.Slice(offset, count * sizeof(int)));
                set => Parent.SetS32Array(
                    count == -1 ? source.Span.Slice(offset) : source.Span.Slice(offset, count * sizeof(int)), value);
            }

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public int[] this[int offset, Span<byte> source, int count = -1]
            {
                get => Parent.GetS32Array(
                    count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(int)));
                set
                {
                    Parent.SetS32Array(count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(int)),
                        value);
                }
            }

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public int[] this[int offset, ReadOnlyMemory<byte> source, int count = -1] =>
                Parent.GetS32Array(count == -1
                    ? source.Span.Slice(offset)
                    : source.Span.Slice(offset, count * sizeof(int)));

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public int[] this[int offset, ReadOnlySpan<byte> source, int count = -1] =>
                Parent.GetS32Array(count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(int)));
        }

        /// <summary>
        /// Signed 64-bit helper.
        /// </summary>
        public readonly struct S64Helper
        {
            /// <summary>
            /// Parent instance.
            /// </summary>
            public readonly Processor Parent;

            /// <summary>
            /// Creates new instance of <see cref="S32Helper"/>.
            /// </summary>
            /// <param name="parent">Parent instance.</param>
            public S64Helper(Processor parent)
            {
                Parent = parent;
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public long this[byte[] source, int offset = 0]
            {
                get => Parent.GetS64(source, offset);
                set => Parent.SetS64(source, value, offset);
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public long this[Memory<byte> source, int offset = 0]
            {
                get => Parent.GetMS64(source, offset);
                set => Parent.SetMS64(source, value, offset);
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public long this[Span<byte> source, int offset = 0]
            {
                get => Parent.GetS64(source, offset);
                set => Parent.SetS64(source, value, offset);
            }

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public long this[ReadOnlyMemory<byte> source, int offset = 0] => Parent.GetMS64(source, offset);

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public long this[ReadOnlySpan<byte> source, int offset = 0] => Parent.GetS64(source, offset);

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            /// <param name="count">Element count.</param>
            public int[] this[int offset, byte[] source, int count = -1]
            {
                get => Parent.GetS32Array(count == -1
                    ? source.AsSpan(offset)
                    : source.AsSpan(offset, count * sizeof(int)));
                set => Parent.SetS32Array(
                    count == -1 ? source.AsSpan(offset) : source.AsSpan(offset, count * sizeof(int)), value);
            }

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public int[] this[int offset, Memory<byte> source, int count = -1]
            {
                get => Parent.GetS32Array(count == -1
                    ? source.Span.Slice(offset)
                    : source.Span.Slice(offset, count * sizeof(int)));
                set => Parent.SetS32Array(
                    count == -1 ? source.Span.Slice(offset) : source.Span.Slice(offset, count * sizeof(int)), value);
            }

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public int[] this[int offset, Span<byte> source, int count = -1]
            {
                get => Parent.GetS32Array(
                    count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(int)));
                set
                {
                    Parent.SetS32Array(count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(int)),
                        value);
                }
            }

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public int[] this[int offset, ReadOnlyMemory<byte> source, int count = -1] =>
                Parent.GetS32Array(count == -1
                    ? source.Span.Slice(offset)
                    : source.Span.Slice(offset, count * sizeof(int)));

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public int[] this[int offset, ReadOnlySpan<byte> source, int count = -1] =>
                Parent.GetS32Array(count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(int)));
        }

        /// <summary>
        /// Unsigned 8-bit helper.
        /// </summary>
        public readonly struct U8Helper
        {
            /// <summary>
            /// Parent instance.
            /// </summary>
            public readonly Processor Parent;

            /// <summary>
            /// Creates new instance of <see cref="U8Helper"/>.
            /// </summary>
            /// <param name="parent">Parent instance.</param>
            public U8Helper(Processor parent)
            {
                Parent = parent;
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public byte this[byte[] source, int offset = 0]
            {
                get => Parent.GetU8(source, offset);
                set => Parent.SetU8(source, value, offset);
            }


            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public byte this[Memory<byte> source, int offset = 0]
            {
                get => Parent.GetMU8(source, offset);
                set => Parent.SetMU8(source, value, offset);
            }


            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public byte this[Span<byte> source, int offset = 0]
            {
                get => Parent.GetU8(source, offset);
                set => Parent.SetU8(source, value, offset);
            }

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public byte this[ReadOnlyMemory<byte> source, int offset = 0] => Parent.GetMU8(source, offset);

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public byte this[ReadOnlySpan<byte> source, int offset = 0] => Parent.GetU8(source, offset);

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            /// <param name="count">Element count.</param>
            public byte[] this[int offset, byte[] source, int count = -1]
            {
                get => Parent.GetU8Array(count == -1
                    ? source.AsSpan(offset)
                    : source.AsSpan(offset, count * sizeof(byte)));
                set => Parent.SetU8Array(
                    count == -1 ? source.AsSpan(offset) : source.AsSpan(offset, count * sizeof(byte)), value);
            }

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public byte[] this[int offset, Memory<byte> source, int count = -1]
            {
                get => Parent.GetU8Array(count == -1
                    ? source.Span.Slice(offset)
                    : source.Span.Slice(offset, count * sizeof(byte)));
                set => Parent.SetU8Array(
                    count == -1 ? source.Span.Slice(offset) : source.Span.Slice(offset, count * sizeof(byte)), value);
            }

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public byte[] this[int offset, Span<byte> source, int count = -1]
            {
                get => Parent.GetU8Array(
                    count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(byte)));
                set
                {
                    Parent.SetU8Array(count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(byte)),
                        value);
                }
            }

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public byte[] this[int offset, ReadOnlyMemory<byte> source, int count = -1] =>
                Parent.GetU8Array(count == -1
                    ? source.Span.Slice(offset)
                    : source.Span.Slice(offset, count * sizeof(byte)));

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public byte[] this[int offset, ReadOnlySpan<byte> source, int count = -1] =>
                Parent.GetU8Array(count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(byte)));
        }

        /// <summary>
        /// Unsigned 16-bit helper.
        /// </summary>
        public readonly struct U16Helper
        {
            /// <summary>
            /// Parent instance.
            /// </summary>
            public readonly Processor Parent;

            /// <summary>
            /// Creates new instance of <see cref="S32Helper"/>.
            /// </summary>
            /// <param name="parent">Parent instance.</param>
            public U16Helper(Processor parent)
            {
                Parent = parent;
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public ushort this[byte[] source, int offset = 0]
            {
                get => Parent.GetU16(source, offset);
                set => Parent.SetU16(source, value, offset);
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public ushort this[Memory<byte> source, int offset = 0]
            {
                get => Parent.GetMU16(source, offset);
                set => Parent.SetMU16(source, value, offset);
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public ushort this[Span<byte> source, int offset = 0]
            {
                get => Parent.GetU16(source, offset);
                set => Parent.SetU16(source, value, offset);
            }

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public ushort this[ReadOnlyMemory<byte> source, int offset = 0] => Parent.GetMU16(source, offset);

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public ushort this[ReadOnlySpan<byte> source, int offset = 0] => Parent.GetU16(source, offset);

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            /// <param name="count">Element count.</param>
            public long[] this[int offset, byte[] source, int count = -1]
            {
                get => Parent.GetS64Array(count == -1
                    ? source.AsSpan(offset)
                    : source.AsSpan(offset, count * sizeof(long)));
                set => Parent.SetS64Array(
                    count == -1 ? source.AsSpan(offset) : source.AsSpan(offset, count * sizeof(long)), value);
            }

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public long[] this[int offset, Memory<byte> source, int count = -1]
            {
                get => Parent.GetS64Array(count == -1
                    ? source.Span.Slice(offset)
                    : source.Span.Slice(offset, count * sizeof(long)));
                set => Parent.SetS64Array(
                    count == -1 ? source.Span.Slice(offset) : source.Span.Slice(offset, count * sizeof(long)), value);
            }

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public long[] this[int offset, Span<byte> source, int count = -1]
            {
                get => Parent.GetS64Array(
                    count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(long)));
                set
                {
                    Parent.SetS64Array(count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(long)),
                        value);
                }
            }

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public long[] this[int offset, ReadOnlyMemory<byte> source, int count = -1] =>
                Parent.GetS64Array(count == -1
                    ? source.Span.Slice(offset)
                    : source.Span.Slice(offset, count * sizeof(long)));

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public long[] this[int offset, ReadOnlySpan<byte> source, int count = -1] =>
                Parent.GetS64Array(count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(long)));
        }

        /// <summary>
        /// Unsigned 32-bit helper.
        /// </summary>
        public readonly struct U32Helper
        {
            /// <summary>
            /// Parent instance.
            /// </summary>
            public readonly Processor Parent;

            /// <summary>
            /// Creates new instance of <see cref="S32Helper"/>.
            /// </summary>
            /// <param name="parent">Parent instance.</param>
            public U32Helper(Processor parent)
            {
                Parent = parent;
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public uint this[byte[] source, int offset = 0]
            {
                get => Parent.GetU32(source, offset);
                set => Parent.SetU32(source, value, offset);
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public uint this[Memory<byte> source, int offset = 0]
            {
                get => Parent.GetMU32(source, offset);
                set => Parent.SetMU32(source, value, offset);
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public uint this[Span<byte> source, int offset = 0]
            {
                get => Parent.GetU32(source, offset);
                set => Parent.SetU32(source, value, offset);
            }

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public uint this[ReadOnlyMemory<byte> source, int offset = 0] => Parent.GetMU32(source, offset);

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public uint this[ReadOnlySpan<byte> source, int offset = 0] => Parent.GetU32(source, offset);

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            /// <param name="count">Element count.</param>
            public ushort[] this[int offset, byte[] source, int count = -1]
            {
                get => Parent.GetU16Array(count == -1
                    ? source.AsSpan(offset)
                    : source.AsSpan(offset, count * sizeof(ushort)));
                set => Parent.SetU16Array(
                    count == -1 ? source.AsSpan(offset) : source.AsSpan(offset, count * sizeof(ushort)), value);
            }

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public ushort[] this[int offset, Memory<byte> source, int count = -1]
            {
                get => Parent.GetU16Array(count == -1
                    ? source.Span.Slice(offset)
                    : source.Span.Slice(offset, count * sizeof(ushort)));
                set => Parent.SetU16Array(
                    count == -1 ? source.Span.Slice(offset) : source.Span.Slice(offset, count * sizeof(ushort)), value);
            }

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public ushort[] this[int offset, Span<byte> source, int count = -1]
            {
                get => Parent.GetU16Array(
                    count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(ushort)));
                set
                {
                    Parent.SetU16Array(count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(ushort)),
                        value);
                }
            }

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public ushort[] this[int offset, ReadOnlyMemory<byte> source, int count = -1] =>
                Parent.GetU16Array(count == -1
                    ? source.Span.Slice(offset)
                    : source.Span.Slice(offset, count * sizeof(ushort)));

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public ushort[] this[int offset, ReadOnlySpan<byte> source, int count = -1] =>
                Parent.GetU16Array(count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(ushort)));
        }

        /// <summary>
        /// Unsigned 64-bit helper.
        /// </summary>
        public readonly struct U64Helper
        {
            /// <summary>
            /// Parent instance.
            /// </summary>
            public readonly Processor Parent;

            /// <summary>
            /// Creates new instance of <see cref="S32Helper"/>.
            /// </summary>
            /// <param name="parent">Parent instance.</param>
            public U64Helper(Processor parent)
            {
                Parent = parent;
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public ulong this[byte[] source, int offset = 0]
            {
                get => Parent.GetU64(source, offset);
                set => Parent.SetU64(source, value, offset);
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public ulong this[Memory<byte> source, int offset = 0]
            {
                get => Parent.GetMU64(source, offset);
                set => Parent.SetMU64(source, value, offset);
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public ulong this[Span<byte> source, int offset = 0]
            {
                get => Parent.GetU64(source, offset);
                set => Parent.SetU64(source, value, offset);
            }

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public ulong this[ReadOnlyMemory<byte> source, int offset = 0] => Parent.GetMU64(source, offset);

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public ulong this[ReadOnlySpan<byte> source, int offset = 0] => Parent.GetU64(source, offset);

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            /// <param name="count">Element count.</param>
            public ulong[] this[int offset, byte[] source, int count = -1]
            {
                get => Parent.GetU64Array(count == -1
                    ? source.AsSpan(offset)
                    : source.AsSpan(offset, count * sizeof(ulong)));
                set => Parent.SetU64Array(
                    count == -1 ? source.AsSpan(offset) : source.AsSpan(offset, count * sizeof(ulong)), value);
            }

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public ulong[] this[int offset, Memory<byte> source, int count = -1]
            {
                get => Parent.GetU64Array(count == -1
                    ? source.Span.Slice(offset)
                    : source.Span.Slice(offset, count * sizeof(ulong)));
                set => Parent.SetU64Array(
                    count == -1 ? source.Span.Slice(offset) : source.Span.Slice(offset, count * sizeof(ulong)), value);
            }

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public ulong[] this[int offset, Span<byte> source, int count = -1]
            {
                get => Parent.GetU64Array(
                    count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(ulong)));
                set
                {
                    Parent.SetU64Array(count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(ulong)),
                        value);
                }
            }

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public ulong[] this[int offset, ReadOnlyMemory<byte> source, int count = -1] =>
                Parent.GetU64Array(count == -1
                    ? source.Span.Slice(offset)
                    : source.Span.Slice(offset, count * sizeof(ulong)));

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public ulong[] this[int offset, ReadOnlySpan<byte> source, int count = -1] =>
                Parent.GetU64Array(count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(ulong)));
        }

        /// <summary>
        /// Generic helper.
        /// </summary>
        public unsafe readonly struct THelper<T> where T : unmanaged
        {
            /// <summary>
            /// Parent instance.
            /// </summary>
            public readonly Processor Parent;

            /// <summary>
            /// Creates new instance of <see cref="THelper{T}"/>.
            /// </summary>
            /// <param name="parent">Parent instance.</param>
            public THelper(Processor parent)
            {
                Parent = parent;
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public T this[byte[] source, int offset = 0]
            {
                get => MemoryMarshal.Cast<byte, T>(source)[offset];
                set => MemoryMarshal.Cast<byte, T>(source)[offset] = value;
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public T this[Memory<byte> source, int offset = 0]
            {
                get => MemoryMarshal.Cast<byte, T>(source.Span)[offset];
                set => MemoryMarshal.Cast<byte, T>(source.Span)[offset] = value;
            }

            /// <summary>
            /// Read/write value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public T this[Span<byte> source, int offset = 0]
            {
                get => MemoryMarshal.Cast<byte, T>(source)[offset];
                set => MemoryMarshal.Cast<byte, T>(source)[offset] = value;
            }

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public T this[ReadOnlyMemory<byte> source, int offset = 0] =>
                MemoryMarshal.Cast<byte, T>(source.Span)[offset];

            /// <summary>
            /// Read value.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            public T this[ReadOnlySpan<byte> source, int offset = 0] => MemoryMarshal.Cast<byte, T>(source)[offset];

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="source">Data source.</param>
            /// <param name="offset">Offset.</param>
            /// <param name="count">Element count.</param>
            public T[] this[int offset, byte[] source, int count = -1]
            {
                get => Parent.GetTArray<T>(count == -1
                    ? source.AsSpan(offset)
                    : source.AsSpan(offset, count * sizeof(T)));
                set => Parent.SetTArray(
                    count == -1 ? source.AsSpan(offset) : source.AsSpan(offset, count * sizeof(T)), value);
            }

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public T[] this[int offset, Memory<byte> source, int count = -1]
            {
                get => Parent.GetTArray<T>(count == -1
                    ? source.Span.Slice(offset)
                    : source.Span.Slice(offset, count * sizeof(T)));
                set => Parent.SetTArray(
                    count == -1 ? source.Span.Slice(offset) : source.Span.Slice(offset, count * sizeof(T)), value);
            }

            /// <summary>
            /// Read/write array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public T[] this[int offset, Span<byte> source, int count = -1]
            {
                get => Parent.GetTArray<T>(
                    count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(T)));
                set
                {
                    Parent.SetTArray(count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(T)),
                        value);
                }
            }

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public T[] this[int offset, ReadOnlyMemory<byte> source, int count = -1] =>
                Parent.GetTArray<T>(count == -1
                    ? source.Span.Slice(offset)
                    : source.Span.Slice(offset, count * sizeof(T)));

            /// <summary>
            /// Read array.
            /// </summary>
            /// <param name="offset">Offset.</param>
            /// <param name="source">Data source.</param>
            /// <param name="count">Element count.</param>
            public T[] this[int offset, ReadOnlySpan<byte> source, int count = -1] =>
                Parent.GetTArray<T>(count == -1 ? source.Slice(offset) : source.Slice(offset, count * sizeof(T)));
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
