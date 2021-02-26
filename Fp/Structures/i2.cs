#pragma warning disable 1591
namespace Fp.Structures
{
    public record i2 : EndiannessDependentOffsetPrimitiveWritableExpression<short>
    {
        public i2(Expression source, bool little) : base(source, little)
        {
        }
    }

    public record ci2 : CastPrimitiveExpression<short>
    {
        public ci2(Expression Value) : base(Value)
        {
        }
    }

    public record vi2 : ValuePrimitiveExpression<short>
    {
        public vi2(short Value) : base(Value)
        {
        }

        public static implicit operator vi2(short value) => new(value);
    }

    public record ai2 : EndiannessDependentOffsetPrimitiveArrayWritableExpression<short>
    {
        public ai2(Expression offset, Expression length, bool little) : base(offset, length, little) {
        }
    }

    public partial record Expression
    {
        public static implicit operator Expression(short value) => new vi2(value);
    }

    public partial class Structure
    {
        public static i2 li2(Expression source) => new(source, true);
        public static i2 bi2(Expression source) => new(source, false);
        public static vi2 vi2(short value) => new(value);
        public static ci2 ci2(Expression value) => new(value);
        public static ai2 ali2(Expression offset, Expression length) => new(offset, length, true);
        public static ai2 abi2(Expression offset, Expression length) => new(offset, length, false);
    }
}
