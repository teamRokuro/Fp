#pragma warning disable 1591
namespace Fp.Structures
{
    public record u2 : EndiannessDependentOffsetPrimitiveWritableExpression<ushort>
    {
        public u2(Expression source, bool little) : base(source, little)
        {
        }
    }

    public record cu2 : CastPrimitiveExpression<ushort>
    {
        public cu2(Expression Value) : base(Value)
        {
        }
    }

    public record vu2 : ValuePrimitiveExpression<ushort>
    {
        public vu2(ushort Value) : base(Value)
        {
        }

        public static implicit operator vu2(ushort value) => new(value);
    }

    public partial record Expression
    {
        public static implicit operator Expression(ushort value) => new vu2(value);
    }

    public partial class Structure
    {
        public static u2 lu2(Expression source) => new(source, true);
        public static u2 bu2(Expression source) => new(source, false);
        public static vu2 vu2(ushort value) => new(value);
        public static cu2 cu2(Expression value) => new(value);
    }
}
