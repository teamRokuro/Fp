using System.Buffers.Binary;
using System.Runtime.InteropServices;
using Fp.Structures.Elements.Primitives;

namespace Fp.Structures.Elements.Primitives
{
    public record S32 : MultibyteOffsetPrimitiveWritableExpression<int>
    {
        public S32(Expression source, bool little) : base(source, little)
        {
        }

        public override int Read3(StructureContext context) => Reverse
            ? BinaryPrimitives.ReverseEndianness(
                MemoryMarshal.Read<int>(context.Stream.ReadBase32()))
            : MemoryMarshal.Read<int>(context.Stream.ReadBase32());

        public override void Write3(StructureContext context, int value)
        {
            if (Reverse) value = BinaryPrimitives.ReverseEndianness(value);
            WritePrimitive(context, value);
        }
    }

    public record S32V : ValuePrimitiveExpression<int>
    {
        public S32V(int Value) : base(Value)
        {
        }

        public override int ReadBase(StructureContext context) => Value;
        public static implicit operator S32V(int value) => new(value);
    }
}

namespace Fp.Structures
{
    public partial class Structure
    {
        public static S32 S32L(Expression source) => new(source, true);
        public static S32 S32B(Expression source) => new(source, false);
    }

    public partial record Expression
    {
        public static implicit operator Expression(int value) => new S32V(value);
    }
}
