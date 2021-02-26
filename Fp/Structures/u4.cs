#pragma warning disable 1591
namespace Fp.Structures
{
    public record u4 : EndiannessDependentOffsetPrimitiveWritableExpression<uint>
    {
        public u4(Expression source, bool little) : base(source, little)
        {
        }
    }

    public record cu4 : CastPrimitiveExpression<uint>
    {
        public cu4(Expression Value) : base(Value)
        {
        }
    }

    public record vu4 : ValuePrimitiveExpression<uint>
    {
        public vu4(uint Value) : base(Value)
        {
        }

        public static implicit operator vu4(uint value) => new(value);
    }

    public partial record Expression
    {
        public static implicit operator Expression(uint value) => new vu4(value);
    }

    public partial class Structure
    {
        public static u4 lu4(Expression source) => new(source, true);
        public static u4 bu4(Expression source) => new(source, false);
        public static vu4 vu4(uint value) => new(value);
        public static cu4 cu4(Expression value) => new(value);
    }
}
