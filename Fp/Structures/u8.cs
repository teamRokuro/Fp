#pragma warning disable 1591
namespace Fp.Structures
{
    public record u8 : DirectOffsetPrimitiveWritableExpression<ulong>
    {
        public u8(Expression source) : base(source)
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

    public partial record Expression
    {
        public static implicit operator Expression(ulong value) => new vu8(value);
    }

    public partial class Structure
    {
        public static u8 lu8(Expression source) => new(source);
        public static u8 bu8(Expression source) => new(source);
        public static vu8 vu8(ulong value) => new(value);
        public static cu8 cu8(Expression value) => new(value);
    }
}
