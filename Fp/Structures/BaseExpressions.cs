using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public sealed override IEnumerable<Element> GetDependencies() =>
            new Element[] {this}.Concat(Source.GetDependencies());

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

        public abstract TPrimitive ReverseEndianness(TPrimitive value);
    }

    public abstract record NoRefPrimitiveExpression<TPrimitive> : PrimitiveExpression<TPrimitive>
        where TPrimitive : unmanaged
    {
        public sealed override IEnumerable<Element> GetDependencies() => Enumerable.Empty<Expression>();
    }

    public abstract record ValuePrimitiveExpression<TPrimitive>
        (TPrimitive Value) : NoRefPrimitiveExpression<TPrimitive>
        where TPrimitive : unmanaged
    {
        public override TPrimitive Read2(StructureContext context) => Value;
    }

    public record RefExpression<T>(Func<T> ValueFunc) : Expression
    {
        public override IEnumerable<Element> GetDependencies() => Enumerable.Empty<Element>();

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
}
