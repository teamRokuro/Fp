using System;
using System.Diagnostics.CodeAnalysis;
#if NET5_0
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
#endif
using static Fp.Processor;

namespace Fp
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class Processor
    {
        #region Bitwise utilities

        /// <summary>
        /// Apply AND to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">AND value</param>
        public static void ApplyAnd(Span<byte> span, byte value)
        {
#if NET5_0
            if (Avx2.IsSupported)
                ApplyAndAvx2(span, value);
            else if (Sse2.IsSupported)
                ApplyAndSse2(span, value);
            else if (AdvSimd.IsSupported)
                ApplyAndArm(span, value);
            else
                ApplyAndFallback(span, value);
#else
            ApplyAndFallback(span, value);
#endif
        }

        /// <summary>
        /// Apply AND to memory
        /// </summary>
        /// <param name="memory">Memory to modify</param>
        /// <param name="value">AND value</param>
        public static void ApplyMAnd(Memory<byte> memory, byte value) => ApplyAnd(memory.Span, value);

        /// <summary>
        /// Apply OR to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">AND value</param>
        public static void ApplyOr(Span<byte> span, byte value)
        {
#if NET5_0
            if (Avx2.IsSupported)
                ApplyOrAvx2(span, value);
            else if (Sse2.IsSupported)
                ApplyOrSse2(span, value);
            else if (AdvSimd.IsSupported)
                ApplyOrArm(span, value);
            else
                ApplyOrFallback(span, value);
#else
            ApplyOrFallback(span, value);
#endif
        }

        /// <summary>
        /// Apply OR to memory
        /// </summary>
        /// <param name="memory">Memory to modify</param>
        /// <param name="value">AND value</param>
        public static void ApplyMOr(Memory<byte> memory, byte value) => ApplyOr(memory.Span, value);

        /// <summary>
        /// Apply XOR to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">XOR value</param>
        public static void ApplyXor(Span<byte> span, byte value)
        {
#if NET5_0
            if (Avx2.IsSupported)
                ApplyXorAvx2(span, value);
            else if (Sse2.IsSupported)
                ApplyXorSse2(span, value);
            else if (AdvSimd.IsSupported)
                ApplyXorArm(span, value);
            else
                ApplyXorFallback(span, value);
#else
            ApplyXorFallback(span, value);
#endif
        }

        /// <summary>
        /// Apply XOR to memory
        /// </summary>
        /// <param name="memory">Memory to modify</param>
        /// <param name="value">XOR value</param>
        public static void ApplyMXor(Memory<byte> memory, byte value) => ApplyXor(memory.Span, value);

#if NET5_0
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Vector128<byte> FillVector128Sse2(byte value)
        {
            var srcPtr = stackalloc int[128 / 8 / 4];
            int iValue = (value << 8) | value;
            iValue |= iValue << 16;
            srcPtr[0] = iValue;
            srcPtr[1] = iValue;
            srcPtr[2] = iValue;
            srcPtr[3] = iValue;
            return Sse2.LoadVector128((byte*)srcPtr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Vector128<byte> FillVector128AdvSimd(byte value)
        {
            var srcPtr = stackalloc int[128 / 8 / 4];
            int iValue = (value << 8) | value;
            iValue |= iValue << 16;
            srcPtr[0] = iValue;
            srcPtr[1] = iValue;
            srcPtr[2] = iValue;
            srcPtr[3] = iValue;
            return AdvSimd.LoadVector128((byte*)srcPtr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe Vector256<byte> FillVector256Avx(byte value)
        {
            var srcPtr = stackalloc int[256 / 8 / 4];
            int iValue = (value << 8) | value;
            iValue |= iValue << 16;
            srcPtr[0] = iValue;
            srcPtr[1] = iValue;
            srcPtr[2] = iValue;
            srcPtr[3] = iValue;
            srcPtr[4] = iValue;
            srcPtr[5] = iValue;
            srcPtr[6] = iValue;
            srcPtr[7] = iValue;
            return Avx.LoadVector256((byte*)srcPtr);
        }

        /// <summary>
        /// Apply AND to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">AND value</param>
        public static unsafe void ApplyAndArm(Span<byte> span, byte value)
        {
            const int split = 128 / 8;
            fixed (byte* pSource = span)
            {
                int i = 0;
                int l = span.Length;

        #region First part

                int kill1Idx = Math.Min((int)unchecked((ulong)(split - (long)pSource) % split), l);
                while (i < kill1Idx)
                {
                    pSource[i] &= value;
                    i++;
                }

                if (kill1Idx == l) return;

        #endregion

        #region Arm

                var src = FillVector128AdvSimd(value);
                int kill2Idx = l - l % split;
                while (i < kill2Idx)
                {
                    AdvSimd.Store(pSource + i, AdvSimd.And(AdvSimd.LoadVector128(pSource + i), src));
                    i += split;
                }

        #endregion

        #region Last part

                while (i < span.Length)
                {
                    pSource[i] &= value;
                    i++;
                }

        #endregion
            }
        }

        /// <summary>
        /// Apply AND to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">AND value</param>
        public static unsafe void ApplyAndSse2(Span<byte> span, byte value)
        {
            const int split = 128 / 8;
            fixed (byte* pSource = span)
            {
                int i = 0;
                int l = span.Length;

        #region First part

                int kill1Idx = Math.Min((int)unchecked((ulong)(split - (long)pSource) % split), l);
                while (i < kill1Idx)
                {
                    pSource[i] &= value;
                    i++;
                }

                if (kill1Idx == l) return;

        #endregion

        #region Sse2

                var src = FillVector128Sse2(value);
                int kill2Idx = l - l % split;
                while (i < kill2Idx)
                {
                    Sse2.StoreAligned(pSource + i, Sse2.And(Sse2.LoadAlignedVector128(pSource + i), src));
                    i += split;
                }

        #endregion

        #region Last part

                while (i < span.Length)
                {
                    pSource[i] &= value;
                    i++;
                }

        #endregion
            }
        }

        /// <summary>
        /// Apply AND to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">AND value</param>
        public static unsafe void ApplyAndAvx2(Span<byte> span, byte value)
        {
            const int split = 256 / 8;
            fixed (byte* pSource = span)
            {
                int i = 0;
                int l = span.Length;

        #region First part

                int kill1Idx = Math.Min((int)unchecked((ulong)(split - (long)pSource) % split), l);
                while (i < kill1Idx)
                {
                    pSource[i] &= value;
                    i++;
                }

                if (kill1Idx == l) return;

        #endregion

        #region Avx

                var src = FillVector256Avx(value);
                int kill2Idx = l - l % split;
                while (i < kill2Idx)
                {
                    Avx.StoreAligned(pSource + i, Avx2.And(Avx.LoadAlignedVector256(pSource + i), src));
                    i += split;
                }

        #endregion

        #region Last part

                while (i < span.Length)
                {
                    pSource[i] &= value;
                    i++;
                }

        #endregion
            }
        }

        /// <summary>
        /// Apply OR to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">OR value</param>
        public static unsafe void ApplyOrArm(Span<byte> span, byte value)
        {
            const int split = 128 / 8;
            fixed (byte* pSource = span)
            {
                int i = 0;
                int l = span.Length;

        #region First part

                int kill1Idx = Math.Min((int)unchecked((ulong)(split - (long)pSource) % split), l);
                while (i < kill1Idx)
                {
                    pSource[i] &= value;
                    i++;
                }

                if (kill1Idx == l) return;

        #endregion

        #region Arm

                var src = FillVector128AdvSimd(value);
                int kill2Idx = l - l % split;
                while (i < kill2Idx)
                {
                    AdvSimd.Store(pSource + i, AdvSimd.Or(AdvSimd.LoadVector128(pSource + i), src));
                    i += split;
                }

        #endregion

        #region Last part

                while (i < span.Length)
                {
                    pSource[i] &= value;
                    i++;
                }

        #endregion
            }
        }

        /// <summary>
        /// Apply OR to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">OR value</param>
        public static unsafe void ApplyOrSse2(Span<byte> span, byte value)
        {
            const int split = 128 / 8;
            fixed (byte* pSource = span)
            {
                int i = 0;
                int l = span.Length;

        #region First part

                int kill1Idx = Math.Min((int)unchecked((ulong)(split - (long)pSource) % split), l);
                while (i < kill1Idx)
                {
                    pSource[i] |= value;
                    i++;
                }

                if (kill1Idx == l) return;

        #endregion

        #region Sse2

                var src = FillVector128Sse2(value);
                int kill2Idx = l - l % split;
                while (i < kill2Idx)
                {
                    Sse2.StoreAligned(pSource + i, Sse2.Or(Sse2.LoadAlignedVector128(pSource + i), src));
                    i += split;
                }

        #endregion

        #region Last part

                while (i < span.Length)
                {
                    pSource[i] |= value;
                    i++;
                }

        #endregion
            }
        }

        /// <summary>
        /// Apply OR to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">OR value</param>
        public static unsafe void ApplyOrAvx2(Span<byte> span, byte value)
        {
            const int split = 256 / 8;
            fixed (byte* pSource = span)
            {
                int i = 0;
                int l = span.Length;

        #region First part

                int kill1Idx = Math.Min((int)unchecked((ulong)(split - (long)pSource) % split), l);
                while (i < kill1Idx)
                {
                    pSource[i] |= value;
                    i++;
                }

                if (kill1Idx == l) return;

        #endregion

        #region Avx

                var src = FillVector256Avx(value);
                int kill2Idx = l - l % split;
                while (i < kill2Idx)
                {
                    Avx.StoreAligned(pSource + i, Avx2.Or(Avx.LoadAlignedVector256(pSource + i), src));
                    i += split;
                }

        #endregion

        #region Last part

                while (i < span.Length)
                {
                    pSource[i] |= value;
                    i++;
                }

        #endregion
            }
        }

        /// <summary>
        /// Apply XOR to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">XOR value</param>
        public static unsafe void ApplyXorArm(Span<byte> span, byte value)
        {
            const int split = 128 / 8;
            fixed (byte* pSource = span)
            {
                int i = 0;
                int l = span.Length;

        #region First part

                int kill1Idx = Math.Min((int)unchecked((ulong)(split - (long)pSource) % split), l);
                while (i < kill1Idx)
                {
                    pSource[i] &= value;
                    i++;
                }

                if (kill1Idx == l) return;

        #endregion

        #region Arm

                var src = FillVector128AdvSimd(value);
                int kill2Idx = l - l % split;
                while (i < kill2Idx)
                {
                    AdvSimd.Store(pSource + i, AdvSimd.Or(AdvSimd.LoadVector128(pSource + i), src));
                    i += split;
                }

        #endregion

        #region Last part

                while (i < span.Length)
                {
                    pSource[i] &= value;
                    i++;
                }

        #endregion
            }
        }

        /// <summary>
        /// Apply XOR to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">XOR value</param>
        public static unsafe void ApplyXorSse2(Span<byte> span, byte value)
        {
            const int split = 128 / 8;
            fixed (byte* pSource = span)
            {
                int i = 0;
                int l = span.Length;

        #region First part

                int kill1Idx = Math.Min((int)unchecked((ulong)(split - (long)pSource) % split), l);
                while (i < kill1Idx)
                {
                    pSource[i] ^= value;
                    i++;
                }

                if (kill1Idx == l) return;

        #endregion

        #region Sse2

                var src = FillVector128Sse2(value);
                int kill2Idx = l - l % split;
                while (i < kill2Idx)
                {
                    Sse2.StoreAligned(pSource + i, Sse2.Xor(Sse2.LoadAlignedVector128(pSource + i), src));
                    i += split;
                }

        #endregion

        #region Last part

                while (i < span.Length)
                {
                    pSource[i] ^= value;
                    i++;
                }

        #endregion
            }
        }

        /// <summary>
        /// Apply XOR to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">XOR value</param>
        public static unsafe void ApplyXorAvx2(Span<byte> span, byte value)
        {
            const int split = 256 / 8;
            fixed (byte* pSource = span)
            {
                int i = 0;
                int l = span.Length;

        #region First part

                int kill1Idx = Math.Min((int)unchecked((ulong)(split - (long)pSource) % split), l);
                while (i < kill1Idx)
                {
                    pSource[i] ^= value;
                    i++;
                }

                if (kill1Idx == l) return;

        #endregion

        #region Avx

                var src = FillVector256Avx(value);
                int kill2Idx = l - l % split;
                while (i < kill2Idx)
                {
                    Avx.StoreAligned(pSource + i, Avx2.Xor(Avx.LoadAlignedVector256(pSource + i), src));
                    i += split;
                }

        #endregion

        #region Last part

                while (i < span.Length)
                {
                    pSource[i] ^= value;
                    i++;
                }

        #endregion
            }
        }
#endif

        /// <summary>
        /// Apply AND to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">AND value</param>
        public static void ApplyAndFallback(Span<byte> span, byte value)
        {
            for (int i = 0; i < span.Length; i++)
                span[i] &= value;
        }

        /// <summary>
        /// Apply OR to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">OR value</param>
        public static void ApplyOrFallback(Span<byte> span, byte value)
        {
            for (int i = 0; i < span.Length; i++)
                span[i] |= value;
        }

        /// <summary>
        /// Apply XOR to memory
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="value">XOR value</param>
        public static void ApplyXorFallback(Span<byte> span, byte value)
        {
            for (int i = 0; i < span.Length; i++)
                span[i] ^= value;
        }

        /// <summary>
        /// Transform delegate
        /// </summary>
        /// <param name="input">Input value</param>
        /// <param name="index">Index</param>
        public delegate byte TransformDelegate(byte input, int index);

        /// <summary>
        /// Transform memory region
        /// </summary>
        /// <param name="memory">Memory to modify</param>
        /// <param name="func">Transformation delegate</param>
        public static void ApplyTransform(Memory<byte> memory, TransformDelegate func) =>
            ApplyTransform(memory.Span, func);

        /// <summary>
        /// Transform memory region
        /// </summary>
        /// <param name="span">Memory to modify</param>
        /// <param name="func">Transformation delegate</param>
        public static void ApplyTransform(Span<byte> span, TransformDelegate func)
        {
            for (int i = 0; i < span.Length; i++)
                span[i] = func(span[i], i);
        }

        #endregion
    }

    // ReSharper disable InconsistentNaming
    public partial class Scripting
    {
        #region Bitwise

        /// <summary>
        /// Apply AND on target
        /// </summary>
        /// <param name="target">Target memory</param>
        /// <param name="value">Value to apply</param>
        public static void and(byte[] target, byte value) => ApplyAnd(target, value);

        /// <summary>
        /// Apply AND on target
        /// </summary>
        /// <param name="target">Target memory</param>
        /// <param name="value">Value to apply</param>
        public static void and(Memory<byte> target, byte value) => ApplyAnd(target.Span, value);

        /// <summary>
        /// Apply AND on target
        /// </summary>
        /// <param name="target">Target memory</param>
        /// <param name="value">Value to apply</param>
        public static void and(Span<byte> target, byte value) => ApplyAnd(target, value);

        /// <summary>
        /// Apply OR on target
        /// </summary>
        /// <param name="target">Target memory</param>
        /// <param name="value">Value to apply</param>
        public static void or(byte[] target, byte value) => ApplyOr(target, value);

        /// <summary>
        /// Apply OR on target
        /// </summary>
        /// <param name="target">Target memory</param>
        /// <param name="value">Value to apply</param>
        public static void or(Memory<byte> target, byte value) => ApplyOr(target.Span, value);

        /// <summary>
        /// Apply OR on target
        /// </summary>
        /// <param name="target">Target memory</param>
        /// <param name="value">Value to apply</param>
        public static void or(Span<byte> target, byte value) => ApplyOr(target, value);

        /// <summary>
        /// Apply exclusive OR on target
        /// </summary>
        /// <param name="target">Target memory</param>
        /// <param name="value">Value to apply</param>
        public static void xor(byte[] target, byte value) => ApplyXor(target, value);

        /// <summary>
        /// Apply exclusive OR on target
        /// </summary>
        /// <param name="target">Target memory</param>
        /// <param name="value">Value to apply</param>
        public static void xor(Memory<byte> target, byte value) => ApplyXor(target.Span, value);

        /// <summary>
        /// Apply exclusive OR on target
        /// </summary>
        /// <param name="target">Target memory</param>
        /// <param name="value">Value to apply</param>
        public static void xor(Span<byte> target, byte value) => ApplyXor(target, value);

        #endregion
    }
    // ReSharper restore InconsistentNaming
}
