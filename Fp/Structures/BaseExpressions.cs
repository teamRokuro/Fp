using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Fp.Intermediate;

namespace Fp.Structures
{
    public abstract record Expression<TValue> : Expression
    {
    }

    public abstract record WritableExpression<TValue> : WritableExpression
    {
    }

    public abstract record PrimitiveExpression<TPrimitive> : Expression where TPrimitive : unmanaged
    {
        public sealed override T Read<T>(StructureContext context) =>
            Data.CastNumberWithBoxing<TPrimitive, T>(Read2(context));

        public sealed override T ReadUnmanaged<T>(StructureContext context) =>
            Data.CastNumber<TPrimitive, T>(Read2(context));

        public abstract TPrimitive Read2(StructureContext context);
    }

    public abstract record PrimitiveWritableExpression<TPrimitive> : WritableExpression where TPrimitive : unmanaged
    {
        public sealed override T Read<T>(StructureContext context) =>
            Data.CastNumberWithBoxing<TPrimitive, T>(Read2(context));

        public sealed override T ReadUnmanaged<T>(StructureContext context) =>
            Data.CastNumber<TPrimitive, T>(Read2(context));


        public sealed override void Write<T>(StructureContext context, T value) =>
            Write2(context, Data.CastNumber<T, TPrimitive>(value));

        public abstract TPrimitive Read2(StructureContext context);
        public abstract void Write2(StructureContext context, TPrimitive value);
    }

    public abstract record OffsetPrimitiveWritableExpression<TPrimitive>
        (Expression Source) : PrimitiveWritableExpression<TPrimitive>
        where TPrimitive : unmanaged
    {
        public override IEnumerable<Element> Dependencies => new[] {Source};

        public sealed override TPrimitive Read2(StructureContext context)
        {
            context.Seek(Source.ReadUnmanaged<long>(context));
            return Read3(context);
        }

        public sealed override void Write2(StructureContext context, TPrimitive value)
        {
            context.Seek(Source.ReadUnmanaged<long>(context));
            Write3(context, value);
        }

        public abstract TPrimitive Read3(StructureContext context);
        public abstract void Write3(StructureContext context, TPrimitive value);

        protected static unsafe Span<byte> ReadPrimitive<T>(Stream stream) where T : unmanaged =>
            sizeof(T) switch
            {
                1 => stream.ReadBase8(),
                2 => stream.ReadBase16(),
                4 => stream.ReadBase32(),
                8 => stream.ReadBase64(),
                16 => stream.ReadBase128(),
                <= 16 => stream.ReadBaseX(sizeof(T), SerializationInternals.IoBuffer),
                _ => stream.ReadBaseX(sizeof(T), new byte[sizeof(T)])
            };

        protected static unsafe void WritePrimitive<T>(Stream stream, T value) where T : unmanaged
        {
            byte[] lcl = sizeof(T) <= 16 ? SerializationInternals.IoBuffer : new byte[sizeof(T)];
            MemoryMarshal.Write(lcl, ref value);
            stream.Write(lcl, 0, sizeof(int));
        }

        public override Expression GetMetaExpression(IReadOnlyDictionary<Element, Expression> mapping) =>
            this with {Source = Source.GetSelfMetaExpression(mapping)};
    }

    public abstract record EndiannessDependentOffsetPrimitiveWritableExpression<TPrimitive>
        : OffsetPrimitiveWritableExpression<TPrimitive> where TPrimitive : unmanaged
    {
        public bool Little { get; init; }
        public bool Reverse => Little ^ BitConverter.IsLittleEndian;

        protected EndiannessDependentOffsetPrimitiveWritableExpression(Expression source, bool little) : base(source)
        {
            Little = little;
        }

        public sealed override TPrimitive Read3(StructureContext context)
        {
            var res = MemoryMarshal.Read<TPrimitive>(ReadPrimitive<TPrimitive>(context.Stream));
            return Reverse ? ReverseEndianness(res) : res;
        }

        public sealed override void Write3(StructureContext context, TPrimitive value)
        {
            if (Reverse) value = ReverseEndianness(value);
            WritePrimitive(context.Stream, value);
        }

        public virtual TPrimitive ReverseEndianness(TPrimitive value)
            => value switch
            {
                byte b => Data.CastNumber<byte, TPrimitive>(b),
                sbyte b => Data.CastNumber<sbyte, TPrimitive>(b),
                ushort b => Data.CastNumber<ushort, TPrimitive>(BinaryPrimitives.ReverseEndianness(b)),
                short b => Data.CastNumber<short, TPrimitive>(BinaryPrimitives.ReverseEndianness(b)),
                uint b => Data.CastNumber<uint, TPrimitive>(BinaryPrimitives.ReverseEndianness(b)),
                int b => Data.CastNumber<int, TPrimitive>(BinaryPrimitives.ReverseEndianness(b)),
                ulong b => Data.CastNumber<ulong, TPrimitive>(BinaryPrimitives.ReverseEndianness(b)),
                long b => Data.CastNumber<long, TPrimitive>(BinaryPrimitives.ReverseEndianness(b)),
                float b => Data.CastNumber<float, TPrimitive>(b),
                double b => Data.CastNumber<double, TPrimitive>(b),
                _ => throw new NotSupportedException()
            };
    }

    public abstract record NoRefPrimitiveExpression<TPrimitive> : PrimitiveExpression<TPrimitive>
        where TPrimitive : unmanaged
    {
    }

    public abstract record ValuePrimitiveExpression<TPrimitive>
        (TPrimitive Value) : NoRefPrimitiveExpression<TPrimitive>
        where TPrimitive : unmanaged
    {
        public override TPrimitive Read2(StructureContext context) => Value;
    }

    public abstract record CastPrimitiveExpression<TPrimitive>
        (Expression Value) : NoRefPrimitiveExpression<TPrimitive>
        where TPrimitive : unmanaged
    {
        public override TPrimitive Read2(StructureContext context) => Value.ReadUnmanaged<TPrimitive>(context);
    }

    public record RefExpression<T>(Func<T> ValueFunc) : Expression
    {
        public override T1? Read<T1>(StructureContext context) where T1 : default
        {
            var res = ValueFunc();
            return res switch
            {
                T1 r2 => r2,
                object r3 => (T1)r3,
                _ => default
            };
        }
    }

    public abstract record UnaryExpression(Expression Value) : Expression
    {
        public override IEnumerable<Element> Dependencies => new[] {Value};
        public override T Read<T>(StructureContext context) => Data.CastNumberWithBoxing<object, T>(GetResult(context));
        public override T ReadUnmanaged<T>(StructureContext context) => Data.CastNumber<object, T>(GetResult(context));

        public override Expression GetMetaExpression(IReadOnlyDictionary<Element, Expression> mapping) =>
            this with {Value = Value.GetSelfMetaExpression(mapping)};

        private object GetResult(StructureContext context)
        {
            var value = Value.Read<object>(context) ?? throw new NullReferenceException();
            return Apply(value);
        }

        public object Apply(object value)
        {
            return value switch
            {
                byte r => u1(r),
                ushort r => u2(r),
                uint r => u4(r),
                ulong r => u8(r),
                sbyte r => s1(r),
                short r => s2(r),
                int r => s4(r),
                long r => s8(r),
                float r => f(r),
                double r => d(r),
                _ => throw new NotSupportedException()
            };
        }

        public abstract object u1(byte r);
        public abstract object u2(ushort r);
        public abstract object u4(uint r);
        public abstract object u8(ulong r);
        public abstract object s1(sbyte r);
        public abstract object s2(short r);
        public abstract object s4(int r);
        public abstract object s8(long r);
        public abstract object f(float r);
        public abstract object d(double r);
    }

    public abstract record BinaryExpression(Expression Lhs, Expression Rhs) : Expression
    {
        public override IEnumerable<Element> Dependencies => new[] {Lhs, Rhs};
        public override T Read<T>(StructureContext context) => Data.CastNumberWithBoxing<object, T>(GetResult(context));
        public override T ReadUnmanaged<T>(StructureContext context) => Data.CastNumber<object, T>(GetResult(context));

        public override Expression GetMetaExpression(IReadOnlyDictionary<Element, Expression> mapping) =>
            this with {Lhs = Lhs.GetSelfMetaExpression(mapping), Rhs = Rhs.GetSelfMetaExpression(mapping)};

        private object GetResult(StructureContext context)
        {
            var lhs = Lhs.Read<object>(context) ?? throw new NullReferenceException();
            var rhs = Rhs.Read<object>(context) ?? throw new NullReferenceException();
            return Apply(lhs, rhs);
        }

        public object Apply(object left, object right)
        {
            return left switch
            {
                byte l => right switch
                {
                    byte r => u1u1(l, r),
                    ushort r => u1u2(l, r),
                    uint r => u1u4(l, r),
                    ulong r => u1u8(l, r),
                    sbyte r => u1s1(l, r),
                    short r => u1s2(l, r),
                    int r => u1s4(l, r),
                    long r => u1s8(l, r),
                    float r => u1f(l, r),
                    double r => u1d(l, r),
                    _ => throw new NotSupportedException()
                },
                ushort l => right switch
                {
                    byte r => u2u1(l, r),
                    ushort r => u2u2(l, r),
                    uint r => u2u4(l, r),
                    ulong r => u2u8(l, r),
                    sbyte r => u2s1(l, r),
                    short r => u2s2(l, r),
                    int r => u2s4(l, r),
                    long r => u2s8(l, r),
                    float r => u2f(l, r),
                    double r => u2d(l, r),
                    _ => throw new NotSupportedException()
                },
                uint l => right switch
                {
                    byte r => u4u1(l, r),
                    ushort r => u4u2(l, r),
                    uint r => u4u4(l, r),
                    ulong r => u4u8(l, r),
                    sbyte r => u4s1(l, r),
                    short r => u4s2(l, r),
                    int r => u4s4(l, r),
                    long r => u4s8(l, r),
                    float r => u4f(l, r),
                    double r => u4d(l, r),
                    _ => throw new NotSupportedException()
                },
                ulong l => right switch
                {
                    byte r => u8u1(l, r),
                    ushort r => u8u2(l, r),
                    uint r => u8u4(l, r),
                    ulong r => u8u8(l, r),
                    sbyte r => u8s1(l, r),
                    short r => u8s2(l, r),
                    int r => u8s4(l, r),
                    long r => u8s8(l, r),
                    float r => u8f(l, r),
                    double r => u8d(l, r),
                    _ => throw new NotSupportedException()
                },
                sbyte l => right switch
                {
                    byte r => s1u1(l, r),
                    ushort r => s1u2(l, r),
                    uint r => s1u4(l, r),
                    ulong r => s1u8(l, r),
                    sbyte r => s1s1(l, r),
                    short r => s1s2(l, r),
                    int r => s1s4(l, r),
                    long r => s1s8(l, r),
                    float r => s1f(l, r),
                    double r => s1d(l, r),
                    _ => throw new NotSupportedException()
                },
                short l => right switch
                {
                    byte r => s2u1(l, r),
                    ushort r => s2u2(l, r),
                    uint r => s2u4(l, r),
                    ulong r => s2u8(l, r),
                    sbyte r => s2s1(l, r),
                    short r => s2s2(l, r),
                    int r => s2s4(l, r),
                    long r => s2s8(l, r),
                    float r => s2f(l, r),
                    double r => s2d(l, r),
                    _ => throw new NotSupportedException()
                },
                int l => right switch
                {
                    byte r => s4u1(l, r),
                    ushort r => s4u2(l, r),
                    uint r => s4u4(l, r),
                    ulong r => s4u8(l, r),
                    sbyte r => s4s1(l, r),
                    short r => s4s2(l, r),
                    int r => s4s4(l, r),
                    long r => s4s8(l, r),
                    float r => s4f(l, r),
                    double r => s4d(l, r),
                    _ => throw new NotSupportedException()
                },
                long l => right switch
                {
                    byte r => s8u1(l, r),
                    ushort r => s8u2(l, r),
                    uint r => s8u4(l, r),
                    ulong r => s8u8(l, r),
                    sbyte r => s8s1(l, r),
                    short r => s8s2(l, r),
                    int r => s8s4(l, r),
                    long r => s8s8(l, r),
                    float r => s8f(l, r),
                    double r => s8d(l, r),
                    _ => throw new NotSupportedException()
                },
                float l => right switch
                {
                    byte r => fu1(l, r),
                    ushort r => fu2(l, r),
                    uint r => fu4(l, r),
                    ulong r => fu8(l, r),
                    sbyte r => fs1(l, r),
                    short r => fs2(l, r),
                    int r => fs4(l, r),
                    long r => fs8(l, r),
                    float r => ff(l, r),
                    double r => fd(l, r),
                    _ => throw new NotSupportedException()
                },
                double l => right switch
                {
                    byte r => du1(l, r),
                    ushort r => du2(l, r),
                    uint r => du4(l, r),
                    ulong r => du8(l, r),
                    sbyte r => ds1(l, r),
                    short r => ds2(l, r),
                    int r => ds4(l, r),
                    long r => ds8(l, r),
                    float r => df(l, r),
                    double r => dd(l, r),
                    _ => throw new NotSupportedException()
                },

                _ => throw new NotSupportedException()
            };
        }

        #region u1

        public abstract object u1u1(byte l, byte r);
        public abstract object u1u2(byte l, ushort r);
        public abstract object u1u4(byte l, uint r);
        public abstract object u1u8(byte l, ulong r);
        public abstract object u1s1(byte l, sbyte r);
        public abstract object u1s2(byte l, short r);
        public abstract object u1s4(byte l, int r);
        public abstract object u1s8(byte l, long r);
        public abstract object u1f(byte l, float r);
        public abstract object u1d(byte l, double r);

        #endregion

        #region u2

        public abstract object u2u1(ushort l, byte r);
        public abstract object u2u2(ushort l, ushort r);
        public abstract object u2u4(ushort l, uint r);
        public abstract object u2u8(ushort l, ulong r);
        public abstract object u2s1(ushort l, sbyte r);
        public abstract object u2s2(ushort l, short r);
        public abstract object u2s4(ushort l, int r);
        public abstract object u2s8(ushort l, long r);
        public abstract object u2f(ushort l, float r);
        public abstract object u2d(ushort l, double r);

        #endregion

        #region u4

        public abstract object u4u1(uint l, byte r);
        public abstract object u4u2(uint l, ushort r);
        public abstract object u4u4(uint l, uint r);
        public abstract object u4u8(uint l, ulong r);
        public abstract object u4s1(uint l, sbyte r);
        public abstract object u4s2(uint l, short r);
        public abstract object u4s4(uint l, int r);
        public abstract object u4s8(uint l, long r);
        public abstract object u4f(uint l, float r);
        public abstract object u4d(uint l, double r);

        #endregion

        #region u8

        public abstract object u8u1(ulong l, byte r);
        public abstract object u8u2(ulong l, ushort r);
        public abstract object u8u4(ulong l, uint r);
        public abstract object u8u8(ulong l, ulong r);
        public abstract object u8s1(ulong l, sbyte r);
        public abstract object u8s2(ulong l, short r);
        public abstract object u8s4(ulong l, int r);
        public abstract object u8s8(ulong l, long r);
        public abstract object u8f(ulong l, float r);
        public abstract object u8d(ulong l, double r);

        #endregion

        #region s1

        public abstract object s1u1(sbyte l, byte r);
        public abstract object s1u2(sbyte l, ushort r);
        public abstract object s1u4(sbyte l, uint r);
        public abstract object s1u8(sbyte l, ulong r);
        public abstract object s1s1(sbyte l, sbyte r);
        public abstract object s1s2(sbyte l, short r);
        public abstract object s1s4(sbyte l, int r);
        public abstract object s1s8(sbyte l, long r);
        public abstract object s1f(sbyte l, float r);
        public abstract object s1d(sbyte l, double r);

        #endregion

        #region s2

        public abstract object s2u1(short l, byte r);
        public abstract object s2u2(short l, ushort r);
        public abstract object s2u4(short l, uint r);
        public abstract object s2u8(short l, ulong r);
        public abstract object s2s1(short l, sbyte r);
        public abstract object s2s2(short l, short r);
        public abstract object s2s4(short l, int r);
        public abstract object s2s8(short l, long r);
        public abstract object s2f(short l, float r);
        public abstract object s2d(short l, double r);

        #endregion

        #region s4

        public abstract object s4u1(int l, byte r);
        public abstract object s4u2(int l, ushort r);
        public abstract object s4u4(int l, uint r);
        public abstract object s4u8(int l, ulong r);
        public abstract object s4s1(int l, sbyte r);
        public abstract object s4s2(int l, short r);
        public abstract object s4s4(int l, int r);
        public abstract object s4s8(int l, long r);
        public abstract object s4f(int l, float r);
        public abstract object s4d(int l, double r);

        #endregion

        #region s8

        public abstract object s8u1(long l, byte r);
        public abstract object s8u2(long l, ushort r);
        public abstract object s8u4(long l, uint r);
        public abstract object s8u8(long l, ulong r);
        public abstract object s8s1(long l, sbyte r);
        public abstract object s8s2(long l, short r);
        public abstract object s8s4(long l, int r);
        public abstract object s8s8(long l, long r);
        public abstract object s8f(long l, float r);
        public abstract object s8d(long l, double r);

        #endregion

        #region f

        public abstract object fu1(float l, byte r);
        public abstract object fu2(float l, ushort r);
        public abstract object fu4(float l, uint r);
        public abstract object fu8(float l, ulong r);
        public abstract object fs1(float l, sbyte r);
        public abstract object fs2(float l, short r);
        public abstract object fs4(float l, int r);
        public abstract object fs8(float l, long r);
        public abstract object ff(float l, float r);
        public abstract object fd(float l, double r);

        #endregion

        #region d

        public abstract object du1(double l, byte r);
        public abstract object du2(double l, ushort r);
        public abstract object du4(double l, uint r);
        public abstract object du8(double l, ulong r);
        public abstract object ds1(double l, sbyte r);
        public abstract object ds2(double l, short r);
        public abstract object ds4(double l, int r);
        public abstract object ds8(double l, long r);
        public abstract object df(double l, float r);
        public abstract object dd(double l, double r);

        #endregion
    }
}
