#pragma warning disable 1591
namespace Fp.Structures
{
    public record u1 : DirectOffsetPrimitiveWritableExpression<byte>
    {
        public u1(Expression source) : base(source)
        {
        }
    }

    public record cu1 : CastPrimitiveExpression<byte>
    {
        public cu1(Expression Value) : base(Value)
        {
        }
    }

    public record vu1 : ValuePrimitiveExpression<byte>
    {
        public vu1(byte Value) : base(Value)
        {
        }

        public static implicit operator vu1(byte value) => new(value);
    }

    public record au1 : DirectOffsetPrimitiveArrayWritableExpression<byte>
    {
        public au1(Expression offset, Expression length) : base(offset, length) {
        }
    }

    public record buf : DirectOffsetPrimitiveArrayWritableExpression<byte>
    {
        public buf(Expression offset, Expression length) : base(offset, length) {
        }
    }

    public partial record Expression
    {
        public static implicit operator Expression(byte value) => new vu1(value);
    }

    public partial class Structure
    {
        public static u1 u1(Expression source) => new(source);
        public static vu1 vu1(byte value) => new(value);
        public static cu1 cu1(Expression value) => new(value);
        public static au1 au1(Expression offset, Expression length) => new(offset, length);
        public static buf buf(Expression offset, Expression length) => new(offset, length);
    }
}
