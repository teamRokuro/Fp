#pragma warning disable 1591
namespace Fp.Structures
{
    public record i4 : EndiannessDependentOffsetPrimitiveWritableExpression<int>
    {
        public i4(Expression source, bool little) : base(source, little)
        {
        }
    }

    public record ci4 : CastPrimitiveExpression<int>
    {
        public ci4(Expression Value) : base(Value)
        {
        }
    }

    public record vi4 : ValuePrimitiveExpression<int>
    {
        public vi4(int Value) : base(Value)
        {
        }

        public static implicit operator vi4(int value) => new(value);
    }

    public record ai4 : EndiannessDependentOffsetPrimitiveArrayWritableExpression<int>
    {
        public ai4(Expression offset, Expression length, bool little) : base(offset, length, little) {
        }
    }

    public partial record Expression
    {
        public static implicit operator Expression(int value) => new vi4(value);
    }

    public partial class Structure
    {
        public static i4 li4(Expression source) => new(source, true);
        public static i4 bi4(Expression source) => new(source, false);
        public static vi4 vi4(int value) => new(value);
        public static ci4 ci4(Expression value) => new(value);
        public static ai4 ali4(Expression offset, Expression length) => new(offset, length, true);
        public static ai4 abi4(Expression offset, Expression length) => new(offset, length, false);
    }
}
