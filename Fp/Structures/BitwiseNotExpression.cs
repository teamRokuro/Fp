using System;

namespace Fp.Structures
{
    /// <summary>
    /// Represents a bitwise NOT expression.
    /// </summary>
    public record BitwiseNotExpression : UnaryExpression
    {
        /// <summary>
        /// Creates a new instance of <see cref="BitwiseNotExpression"/>.
        /// </summary>
        /// <param name="value">Expression.</param>
        public BitwiseNotExpression(Expression value) : base(value)
        {
        }

#pragma warning disable 1591
        public override object u1(byte r) => ~r;

        public override object u2(ushort r) => ~r;

        public override object u4(uint r) => ~r;

        public override object u8(ulong r) => ~r;

        public override object s1(sbyte r) => ~r;

        public override object s2(short r) => ~r;

        public override object s4(int r) => ~r;

        public override object s8(long r) => ~r;

        public override object f(float r) => throw new NotSupportedException();

        public override object d(double r) => throw new NotSupportedException();
#pragma warning disable 1591
    }

#pragma warning disable 1591
    public partial record Expression
    {
        public static Expression operator ~(Expression value) => new BitwiseNotExpression(value);
    }
#pragma warning restore 1591
}
