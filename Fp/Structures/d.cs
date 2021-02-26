#pragma warning disable 1591
namespace Fp.Structures
{
    public record d : DirectOffsetPrimitiveWritableExpression<double>
    {
        public d(Expression source) : base(source)
        {
        }
    }

    public record cd : CastPrimitiveExpression<double>
    {
        public cd(Expression Value) : base(Value)
        {
        }
    }

    public record vd : ValuePrimitiveExpression<double>
    {
        public vd(double Value) : base(Value)
        {
        }

        public static implicit operator vd(double value) => new(value);
    }

    public record ad : DirectOffsetPrimitiveArrayWritableExpression<double>
    {
        public ad(Expression offset, Expression length) : base(offset, length) {
        }
    }

    public partial record Expression
    {
        public static implicit operator Expression(double value) => new vd(value);
    }

    public partial class Structure
    {
        public static d d(Expression source) => new(source);
        public static vd vd(double value) => new(value);
        public static cd cd(Expression value) => new(value);
        public static ad ad(Expression offset, Expression length) => new(offset, length);
    }
}
