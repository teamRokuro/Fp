#pragma warning disable 1591
namespace Fp.Structures
{
    public record f : DirectOffsetPrimitiveWritableExpression<float>
    {
        public f(Expression source) : base(source)
        {
        }
    }

    public record cf : CastPrimitiveExpression<float>
    {
        public cf(Expression Value) : base(Value)
        {
        }
    }

    public record vf : ValuePrimitiveExpression<float>
    {
        public vf(float Value) : base(Value)
        {
        }

        public static implicit operator vf(float value) => new(value);
    }

    public partial record Expression
    {
        public static implicit operator Expression(float value) => new vd(value);
    }

    public partial class Structure
    {
        public static f f(Expression source) => new(source);
        public static vf vf(float value) => new(value);
        public static cf cf(Expression value) => new(value);
    }
}
