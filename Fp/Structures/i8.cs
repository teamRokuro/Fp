#pragma warning disable 1591
namespace Fp.Structures
{
    public record i8 : EndiannessDependentOffsetPrimitiveWritableExpression<long>
    {
        public i8(Expression source, bool little) : base(source, little)
        {
        }
    }

    public record ci8 : CastPrimitiveExpression<long>
    {
        public ci8(Expression Value) : base(Value)
        {
        }
    }

    public record vi8 : ValuePrimitiveExpression<long>
    {
        public vi8(long Value) : base(Value)
        {
        }

        public static implicit operator vi8(long value) => new(value);
    }

    public record ai8 : EndiannessDependentOffsetPrimitiveArrayWritableExpression<long>
    {
        public ai8(Expression offset, Expression length, bool little) : base(offset, length, little) {
        }
    }

    public partial record Expression
    {
        public static implicit operator Expression(long value) => new vi8(value);
    }

    public partial class Structure
    {
        public static i8 li8(Expression source) => new(source, true);
        public static i8 bi8(Expression source) => new(source, false);
        public static vi8 vi8(long value) => new(value);
        public static ci8 ci8(Expression value) => new(value);
        public static ai8 ali8(Expression offset, Expression length) => new(offset, length, true);
        public static ai8 abi8(Expression offset, Expression length) => new(offset, length, false);
    }
}
