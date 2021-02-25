using System;
using System.Buffers;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Fp.Structures
{
    /// <summary>
    /// Serialization utility
    /// </summary>
    /// <remarks>
    /// Copied / modified from https://github.com/Lucina/Azura/blob/6ad5f296c9b398276bb027c6eddf13e4a48c0fb5/Azura/SerializationInternals.cs
    /// </remarks>
    public static class SerializationInternals
    {
        [ThreadStatic] private static byte[]? _buffer;
        internal static byte[] IoBuffer => _buffer ??= new byte[sizeof(decimal)];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Span<byte> ReadBase8(this Stream stream)
        {
            int read = stream.Read(IoBuffer, 0, 1);
            if (read == 0)
                throw new EndOfStreamException(
                    "Failed to read required number of bytes! 0x0 read, 0x1 left");
            return IoBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Span<byte> ReadBase16(this Stream stream)
        {
            int tot = 0;
            do
            {
                int read = stream.Read(IoBuffer, tot, sizeof(ushort) - tot);
                if (read == 0)
                    throw new EndOfStreamException(
                        $"Failed to read required number of bytes! 0x{tot:X} read, 0x{sizeof(ushort) - tot:X} left");
                tot += read;
            } while (tot < sizeof(ushort));

            return IoBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Span<byte> ReadBase32(this Stream stream)
        {
            int tot = 0;
            do
            {
                int read = stream.Read(IoBuffer, tot, sizeof(uint) - tot);
                if (read == 0)
                    throw new EndOfStreamException(
                        $"Failed to read required number of bytes! 0x{tot:X} read, 0x{sizeof(uint) - tot:X} left");
                tot += read;
            } while (tot < sizeof(uint));

            return IoBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Span<byte> ReadBase64(this Stream stream)
        {
            int tot = 0;
            do
            {
                int read = stream.Read(IoBuffer, tot, sizeof(ulong) - tot);
                if (read == 0)
                    throw new EndOfStreamException(
                        $"Failed to read required number of bytes! 0x{tot:X} read, 0x{sizeof(ulong) - tot:X} left");
                tot += read;
            } while (tot < sizeof(ulong));

            return IoBuffer;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Span<byte> ReadBase128(this Stream stream)
        {
            int tot = 0;
            do
            {
                int read = stream.Read(IoBuffer, tot, sizeof(decimal) - tot);
                if (read == 0)
                    throw new EndOfStreamException(
                        $"Failed to read required number of bytes! 0x{tot:X} read, 0x{sizeof(decimal) - tot:X} left");
                tot += read;
            } while (tot < sizeof(decimal));

            return IoBuffer;
        }

        /// <summary>
        /// Read array.
        /// </summary>
        /// <param name="stream">Source stream.</param>
        /// <param name="target">Target buffer.</param>
        /// <param name="offset">Offset to read at.</param>
        /// <param name="count">Number of elements.</param>
        /// <exception cref="ApplicationException">If failed to read required number of bytes.</exception>
        public static void ReadArray(this Stream stream, byte[] target, int offset, int count)
        {
            if (count == 0) return;
            int left = count, tot = 0;
            do
            {
                int read = stream.Read(target, offset + tot, Math.Min(4096, left));
                if (read == 0)
                    throw new ApplicationException(
                        $"Failed to read required number of bytes! 0x{tot:X} read, 0x{left:X} left");
                left -= read;
                tot += read;
            } while (left > 0);
        }

        private const int _hardBufferSize = 128 * 1024;

        /// <summary>
        /// Read span.
        /// </summary>
        /// <param name="stream">Source stream.</param>
        /// <param name="target">Target buffer.</param>
        /// <param name="count">Number of elements.</param>
        /// <param name="enableSwap">Enable element endianness swapping.</param>
        /// <param name="little">If true, little-endian target.</param>
        /// <typeparam name="T">Type of elements.</typeparam>
        /// <exception cref="ApplicationException">If failed to read required number of bytes.</exception>
        public static unsafe void ReadSpan<T>(this Stream stream, Span<T> target, int count, bool enableSwap,
            bool little)
            where T : unmanaged
        {
            if (count == 0) return;
            var mainTarget = MemoryMarshal.Cast<T, byte>(target);
            int order = sizeof(T);
            int mainLen = count * order;
            byte[] buf = ArrayPool<byte>.Shared.Rent(_hardBufferSize);
            var span = buf.AsSpan();
            try
            {
                int left = mainLen, tot = 0;
                do
                {
                    int read = stream.Read(buf, 0, Math.Min(_hardBufferSize, left));
                    if (read == 0)
                        throw new ApplicationException(
                            $"Failed to read required number of bytes! 0x{tot:X} read, 0x{left:X} left");
                    span.Slice(0, read).CopyTo(mainTarget.Slice(tot));
                    left -= read;
                    tot += read;
                } while (left > 0);

                if (order != 1 && enableSwap && little ^ BitConverter.IsLittleEndian)
                    fixed (byte* p = &mainTarget.GetPinnableReference())
                    {
                        switch (order)
                        {
                            case 2:
                                short* tmp2 = (short*)p;
                                for (int i = 0; i < mainLen; i += 2)
                                {
                                    *tmp2 = BinaryPrimitives.ReverseEndianness(*tmp2);
                                    tmp2++;
                                }

                                break;
                            case 4:
                                int* tmp4 = (int*)p;
                                for (int i = 0; i < mainLen; i += 4)
                                {
                                    *tmp4 = BinaryPrimitives.ReverseEndianness(*tmp4);
                                    tmp4++;
                                }

                                break;
                            case 8:
                                long* tmp8 = (long*)p;
                                for (int i = 0; i < mainLen; i += 8)
                                {
                                    *tmp8 = BinaryPrimitives.ReverseEndianness(*tmp8);
                                    tmp8++;
                                }

                                break;
                            default:
                                int half = order / 2;
                                for (int i = 0; i < mainLen; i += order)
                                for (int j = 0; j < half; j++)
                                {
                                    int fir = i + j;
                                    int sec = i + order - 1 - j;
                                    byte tmp = p[fir];
                                    p[fir] = p[sec];
                                    p[sec] = tmp;
                                }

                                break;
                        }
                    }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buf);
            }
        }

        /// <summary>
        /// Write span.
        /// </summary>
        /// <param name="stream">Source stream.</param>
        /// <param name="source">Source buffer.</param>
        /// <param name="count">Number of elements.</param>
        /// <param name="enableSwap">Enable element endianness swapping.</param>
        /// <param name="little">If true, little-endian target.</param>
        /// <typeparam name="T">Type of elements.</typeparam>
        public static unsafe void WriteSpan<T>(this Stream stream, ReadOnlySpan<T> source, int count, bool enableSwap,
            bool little) where T : unmanaged
        {
            if (count == 0)
                return;
            var mainTarget = MemoryMarshal.Cast<T, byte>(source);
            byte[] buf = ArrayPool<byte>.Shared.Rent(_hardBufferSize);
            var span = buf.AsSpan();
            int order = sizeof(T);
            int left = count * order;
            int tot = 0;
            try
            {
                if (order == 1 || !enableSwap || !(little ^ BitConverter.IsLittleEndian))
                {
                    while (left > 0)
                    {
                        int noSwapCur = Math.Min(left, _hardBufferSize);
                        mainTarget.Slice(tot, noSwapCur).CopyTo(buf);
                        stream.Write(buf, 0, noSwapCur);
                        left -= noSwapCur;
                        tot += noSwapCur;
                    }

                    return;
                }

                int maxCount = _hardBufferSize / order * order;
                fixed (byte* p = &span.GetPinnableReference())
                {
                    while (left != 0)
                    {
                        int cur = Math.Min(left, maxCount);
                        mainTarget.Slice(tot, cur).CopyTo(buf);
                        switch (order)
                        {
                            case 2:
                                short* tmp2 = (short*)p;
                                for (int i = 0; i < cur; i += 2)
                                {
                                    *tmp2 = BinaryPrimitives.ReverseEndianness(*tmp2);
                                    tmp2++;
                                }

                                break;
                            case 4:
                                int* tmp4 = (int*)p;
                                for (int i = 0; i < cur; i += 4)
                                {
                                    *tmp4 = BinaryPrimitives.ReverseEndianness(*tmp4);
                                    tmp4++;
                                }

                                break;
                            case 8:
                                long* tmp8 = (long*)p;
                                for (int i = 0; i < cur; i += 8)
                                {
                                    *tmp8 = BinaryPrimitives.ReverseEndianness(*tmp8);
                                    tmp8++;
                                }

                                break;
                            default:
                                int half = order / 2;
                                for (int i = 0; i < cur; i += order)
                                for (int j = 0; j < half; j++)
                                {
                                    int fir = i + j;
                                    int sec = i + order - 1 - j;
                                    byte tmp = p[fir];
                                    p[fir] = p[sec];
                                    p[sec] = tmp;
                                }

                                break;
                        }

                        stream.Write(buf, 0, cur);
                        left -= cur;
                        tot += cur;
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buf);
            }
        }
    }
}
