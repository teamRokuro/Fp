using System.Buffers.Binary;
using Fp.Structures.Elements.Primitives;

namespace Fp.Structures.Elements.Primitives
{
    public record i4 : EndiannessDependentOffsetPrimitiveWritableExpression<int>
    {
        public i4(Expression source, bool little) : base(source, little)
        {
        }

        public override int ReverseEndianness(int value) => BinaryPrimitives.ReverseEndianness(value);
    }

    public record vi4 : ValuePrimitiveExpression<int>
    {
        public vi4(int Value) : base(Value)
        {
        }

        public static implicit operator vi4(int value) => new(value);
    }
}

namespace Fp.Structures
{
    public partial class Structure
    {
        public static i4 li4(Expression source) => new(source, true);
        public static i4 bi4(Expression source) => new(source, false);
        public static vi4 vi4(int value) => new(value);
    }

    public partial record Expression
    {
        public static implicit operator Expression(int value) => new vi4(value);
    }
}
