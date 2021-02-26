#pragma warning disable 1591
namespace Fp.Structures
{
    public record i1 : DirectOffsetPrimitiveWritableExpression<sbyte>
    {
        public i1(Expression source) : base(source)
        {
        }
    }

    public record ci1 : CastPrimitiveExpression<sbyte>
    {
        public ci1(Expression Value) : base(Value)
        {
        }
    }

    public record vi1 : ValuePrimitiveExpression<sbyte>
    {
        public vi1(sbyte Value) : base(Value)
        {
        }

        public static implicit operator vi1(sbyte value) => new(value);
    }

    public record ai1 : DirectOffsetPrimitiveArrayWritableExpression<sbyte>
    {
        public ai1(Expression offset, Expression length) : base(offset, length) {
        }
    }

    public partial record Expression
    {
        public static implicit operator Expression(sbyte value) => new vi1(value);
    }

    public partial class Structure
    {
        public static i1 i1(Expression source) => new(source);
        public static vi1 vi1(sbyte value) => new(value);
        public static ci1 ci1(Expression value) => new(value);
        public static ai1 ai1(Expression offset, Expression length) => new(offset, length);
    }
}
