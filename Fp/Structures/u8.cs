#pragma warning disable 1591
namespace Fp.Structures
{
    public record u8 : EndiannessDependentOffsetPrimitiveWritableExpression<ulong>
    {
        public u8(Expression source, bool little) : base(source, little)
        {
        }
    }

    public record cu8 : CastPrimitiveExpression<ulong>
    {
        public cu8(Expression Value) : base(Value)
        {
        }
    }

    public record vu8 : ValuePrimitiveExpression<ulong>
    {
        public vu8(ulong Value) : base(Value)
        {
        }

        public static implicit operator vu8(ulong value) => new(value);
    }

    public record au8 : EndiannessDependentOffsetPrimitiveArrayWritableExpression<ulong>
    {
        public au8(Expression offset, Expression length, bool little) : base(offset, length, little) {
        }
    }

    public partial record Expression
    {
        public static implicit operator Expression(ulong value) => new vu8(value);
    }

    public partial class Structure
    {
        public static u8 lu8(Expression source) => new(source, true);
        public static u8 bu8(Expression source) => new(source, false);
        public static vu8 vu8(ulong value) => new(value);
        public static cu8 cu8(Expression value) => new(value);
        public static au8 alu8(Expression offset, Expression length) => new(offset, length, true);
        public static au8 abu8(Expression offset, Expression length) => new(offset, length, false);
    }
}
