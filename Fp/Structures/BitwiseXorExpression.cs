using System;

namespace Fp.Structures
{
    /// <summary>
    /// Represents a bitwise COR expression.
    /// </summary>
    public record BitwiseXorExpression : BinaryExpression
    {
        /// <summary>
        /// Creates a new instance of <see cref="BitwiseXorExpression"/>.
        /// </summary>
        /// <param name="lhs">Left expression.</param>
        /// <param name="rhs">Right expression.</param>
        public BitwiseXorExpression(Expression lhs, Expression rhs) : base(lhs, rhs)
        {
        }

#pragma warning disable 1591
        public override object u1u1(byte l, byte r) => l ^ r;

        public override object u1u2(byte l, ushort r) => l ^ r;

        public override object u1u4(byte l, uint r) => l ^ r;

        public override object u1u8(byte l, ulong r) => l ^ r;

        public override object u1s1(byte l, sbyte r) => l ^ r;

        public override object u1s2(byte l, short r) => l ^ r;

        public override object u1s4(byte l, int r) => l ^ r;

        public override object u1s8(byte l, long r) => l ^ r;

        public override object u1f(byte l, float r) => throw new NotSupportedException();

        public override object u1d(byte l, double r) => throw new NotSupportedException();

        public override object u2u1(ushort l, byte r) => l ^ r;

        public override object u2u2(ushort l, ushort r) => l ^ r;

        public override object u2u4(ushort l, uint r) => l ^ r;

        public override object u2u8(ushort l, ulong r) => l ^ r;

        public override object u2s1(ushort l, sbyte r) => l ^ r;

        public override object u2s2(ushort l, short r) => l ^ r;

        public override object u2s4(ushort l, int r) => l ^ r;

        public override object u2s8(ushort l, long r) => l ^ r;

        public override object u2f(ushort l, float r) => throw new NotSupportedException();

        public override object u2d(ushort l, double r) => throw new NotSupportedException();

        public override object u4u1(uint l, byte r) => l ^ r;

        public override object u4u2(uint l, ushort r) => l ^ r;

        public override object u4u4(uint l, uint r) => l ^ r;

        public override object u4u8(uint l, ulong r) => l ^ r;

        public override object u4s1(uint l, sbyte r) => l ^ r;

        public override object u4s2(uint l, short r) => l ^ r;

        public override object u4s4(uint l, int r) => l ^ r;

        public override object u4s8(uint l, long r) => l ^ r;

        public override object u4f(uint l, float r) => throw new NotSupportedException();

        public override object u4d(uint l, double r) => throw new NotSupportedException();

        public override object u8u1(ulong l, byte r) => l ^ r;

        public override object u8u2(ulong l, ushort r) => l ^ r;

        public override object u8u4(ulong l, uint r) => l ^ r;

        public override object u8u8(ulong l, ulong r) => l ^ r;

        public override object u8s1(ulong l, sbyte r) => throw new NotSupportedException();

        public override object u8s2(ulong l, short r) => throw new NotSupportedException();

        public override object u8s4(ulong l, int r) => throw new NotSupportedException();

        public override object u8s8(ulong l, long r) => throw new NotSupportedException();

        public override object u8f(ulong l, float r) => throw new NotSupportedException();

        public override object u8d(ulong l, double r) => throw new NotSupportedException();

        public override object s1u1(sbyte l, byte r) => l ^ r;

        public override object s1u2(sbyte l, ushort r) => l ^ r;

        public override object s1u4(sbyte l, uint r) => l ^ r;

        public override object s1u8(sbyte l, ulong r) => throw new NotSupportedException();

        public override object s1s1(sbyte l, sbyte r) => l ^ r;

        public override object s1s2(sbyte l, short r) => l ^ r;

        public override object s1s4(sbyte l, int r) => l ^ r;

        public override object s1s8(sbyte l, long r) => l ^ r;

        public override object s1f(sbyte l, float r) => throw new NotSupportedException();

        public override object s1d(sbyte l, double r) => throw new NotSupportedException();

        public override object s2u1(short l, byte r) => l ^ r;

        public override object s2u2(short l, ushort r) => l ^ r;

        public override object s2u4(short l, uint r) => l ^ r;

        public override object s2u8(short l, ulong r) => throw new NotSupportedException();

        public override object s2s1(short l, sbyte r) => l ^ r;

        public override object s2s2(short l, short r) => l ^ r;

        public override object s2s4(short l, int r) => l ^ r;

        public override object s2s8(short l, long r) => l ^ r;

        public override object s2f(short l, float r) => throw new NotSupportedException();

        public override object s2d(short l, double r) => throw new NotSupportedException();

        public override object s4u1(int l, byte r) => l ^ r;

        public override object s4u2(int l, ushort r) => l ^ r;

        public override object s4u4(int l, uint r) => l ^ r;

        public override object s4u8(int l, ulong r) => throw new NotSupportedException();

        public override object s4s1(int l, sbyte r) => l ^ r;

        public override object s4s2(int l, short r) => l ^ r;

        public override object s4s4(int l, int r) => l ^ r;

        public override object s4s8(int l, long r) => l ^ r;

        public override object s4f(int l, float r) => throw new NotSupportedException();

        public override object s4d(int l, double r) => throw new NotSupportedException();

        public override object s8u1(long l, byte r) => l ^ r;

        public override object s8u2(long l, ushort r) => l ^ r;

        public override object s8u4(long l, uint r) => l ^ r;

        public override object s8u8(long l, ulong r) => throw new NotSupportedException();

        public override object s8s1(long l, sbyte r) => l ^ r;

        public override object s8s2(long l, short r) => l ^ r;

        public override object s8s4(long l, int r) => l ^ r;

        public override object s8s8(long l, long r) => l ^ r;

        public override object s8f(long l, float r) => throw new NotSupportedException();

        public override object s8d(long l, double r) => throw new NotSupportedException();

        public override object fu1(float l, byte r) => throw new NotSupportedException();

        public override object fu2(float l, ushort r) => throw new NotSupportedException();

        public override object fu4(float l, uint r) => throw new NotSupportedException();

        public override object fu8(float l, ulong r) => throw new NotSupportedException();

        public override object fs1(float l, sbyte r) => throw new NotSupportedException();

        public override object fs2(float l, short r) => throw new NotSupportedException();

        public override object fs4(float l, int r) => throw new NotSupportedException();

        public override object fs8(float l, long r) => throw new NotSupportedException();

        public override object ff(float l, float r) => throw new NotSupportedException();

        public override object fd(float l, double r) => throw new NotSupportedException();

        public override object du1(double l, byte r) => throw new NotSupportedException();

        public override object du2(double l, ushort r) => throw new NotSupportedException();

        public override object du4(double l, uint r) => throw new NotSupportedException();

        public override object du8(double l, ulong r) => throw new NotSupportedException();

        public override object ds1(double l, sbyte r) => throw new NotSupportedException();

        public override object ds2(double l, short r) => throw new NotSupportedException();

        public override object ds4(double l, int r) => throw new NotSupportedException();

        public override object ds8(double l, long r) => throw new NotSupportedException();

        public override object df(double l, float r) => throw new NotSupportedException();

        public override object dd(double l, double r) => throw new NotSupportedException();
#pragma warning restore 1591
    }

#pragma warning disable 1591
    public partial record Expression
    {
        public static Expression operator ^(Expression lhs, Expression rhs) => new BitwiseXorExpression(lhs, rhs);
    }
#pragma warning restore 1591
}
