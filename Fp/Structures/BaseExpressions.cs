using System;
using System.Collections.Generic;
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
            Data.CastNumberWithBoxing<TPrimitive, T>(ReadBase(context));

        public sealed override T ReadUnmanaged<T>(StructureContext context) =>
            Data.CastNumber<TPrimitive, T>(ReadBase(context));

        public abstract TPrimitive ReadBase(StructureContext context);
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

        protected static void WritePrimitive<T>(StructureContext context, T value) where T : unmanaged
        {
            byte[] lcl = SerializationInternals.IoBuffer;
            MemoryMarshal.Write(lcl, ref value);
            context.Stream.Write(lcl, 0, sizeof(int));
        }
    }

    public abstract record MultibyteOffsetPrimitiveWritableExpression<TPrimitive>
        : OffsetPrimitiveWritableExpression<TPrimitive> where TPrimitive : unmanaged
    {
        public bool Little { get; init; }
        public bool Reverse => Little ^ BitConverter.IsLittleEndian;

        protected MultibyteOffsetPrimitiveWritableExpression(Expression source, bool little) : base(source)
        {
            Little = little;
        }
    }

    public abstract record ValuePrimitiveExpression<TPrimitive>
        (TPrimitive Value) : PrimitiveExpression<TPrimitive>
        where TPrimitive : unmanaged
    {
        public sealed override IEnumerable<Element> GetDependencies() => Enumerable.Empty<Expression>();
    }
}
