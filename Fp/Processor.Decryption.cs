using System;
using System.Diagnostics.CodeAnalysis;

namespace Fp
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public partial class Processor
    {
        #region Decryption utilities

        /// <summary>
        /// Block cipher padding mode
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum PaddingMode
        {
            /// <summary>
            /// End of message is padded with null bytes
            /// </summary>
            Zero,

            /// <summary>
            /// ANSI X9.23 padding
            /// </summary>
            AnsiX9_23,

            /// <summary>
            /// ISO 10126 padding
            /// </summary>
            Iso10126,

            /// <summary>
            /// PKCS#7 padding
            /// </summary>
            Pkcs7,

            /// <summary>
            /// PKCS#5 padding
            /// </summary>
            Pkcs5,

            /// <summary>
            /// ISO/IEC 7816-4:2005 padding
            /// </summary>
            Iso_Iec_7816_4
        }

        /// <summary>
        /// Get depadded message length using specified padding mode
        /// </summary>
        /// <param name="span">Message</param>
        /// <param name="paddingMode">Padding mode to use</param>
        /// <returns>Depadded length of message</returns>
        public static int GetDepaddedLength(Span<byte> span, PaddingMode paddingMode) =>
            paddingMode switch
            {
                PaddingMode.Zero => GetDepaddedLengthZero(span),
                PaddingMode.Iso_Iec_7816_4 => GetDepaddedLengthIso_Iec_7816_4(span),
                var p when
                p == PaddingMode.AnsiX9_23 ||
                p == PaddingMode.Iso10126 ||
                p == PaddingMode.Pkcs7 ||
                p == PaddingMode.Pkcs5
                => GetDepaddedLengthLastByteSubtract(span),
                _ => throw new ArgumentOutOfRangeException(nameof(paddingMode), paddingMode, null)
            };

        private static int GetDepaddedLengthZero(Span<byte> span)
        {
            for (int i = span.Length; i > 0; i--)
            {
                if (span[i - 1] != 0)
                {
                    return i;
                }
            }

            return 0;
        }


        private static int GetDepaddedLengthIso_Iec_7816_4(Span<byte> span)
        {
            for (int i = span.Length - 1; i >= 0; i--)
            {
                switch (span[i])
                {
                    case 0x00:
                        break;
                    case 0x80:
                        return i;
                    default:
                        throw new ArgumentException(
                            $"Invalid padding byte for {nameof(PaddingMode.Iso_Iec_7816_4)}, need 0x80 or 0x00 but got 0x{span[i]:X2}");
                }
            }

            throw new ArgumentException(
                $"Message is all null bytes and {nameof(PaddingMode.Iso_Iec_7816_4)} requires 0x80 to mark beginning of padding");
        }

        private static int GetDepaddedLengthLastByteSubtract(ReadOnlySpan<byte> span) =>
            span.Length == 0 ? 0 : span.Length - span[span.Length - 1];

        /// <summary>
        /// Create byte array from hex string
        /// </summary>
        /// <param name="hex">Hex string to decode</param>
        /// <param name="validate">Validate characters</param>
        /// <returns>Array with decoded hex string</returns>
        /// <exception cref="ArgumentException">If string has odd length</exception>
        public static unsafe byte[] DecodeHex(string hex, bool validate = true)
        {
            int len = hex.Length;
            if (len == 0)
            {
                return new byte[0];
            }

            if (len % 2 != 0)
            {
                throw new ArgumentException($"Hex string has length {hex.Length}, must be even");
            }

            len /= 2;
            fixed (char* buf = &hex.AsSpan().GetPinnableReference())
            {
                char* rBuf = buf;
                if (len != 0 && rBuf[0] == '0' && (rBuf[1] == 'x' || rBuf[1] == 'X'))
                {
                    rBuf += 2;
                    len--;
                }

                byte[] res = new byte[len];
                char c;
                if (validate)
                {
                    for (int i = 0; i < len; i++)
                    {
                        c = *rBuf++;
                        if (c > 0x60)
                        {
                            if (c > 0x66)
                            {
                                throw new ArgumentException($"Illegal character {c} at position {rBuf - buf}");
                            }

                            res[i] = (byte)((c + 9) << 4);
                        }
                        else if (c > 0x40)
                        {
                            if (c > 0x46)
                            {
                                throw new ArgumentException($"Illegal character {c} at position {rBuf - buf}");
                            }

                            res[i] = (byte)((c + 9) << 4);
                        }
                        else if (c > 0x2F)
                        {
                            if (c > 0x39)
                            {
                                throw new ArgumentException($"Illegal character {c} at position {rBuf - buf}");
                            }

                            res[i] = (byte)(c << 4);
                        }
                        else
                        {
                            throw new ArgumentException($"Illegal character {c} at position {rBuf - buf}");
                        }

                        c = *rBuf++;
                        if (c > 0x60)
                        {
                            if (c > 0x66)
                            {
                                throw new ArgumentException($"Illegal character {c} at position {rBuf - buf}");
                            }

                            res[i] += (byte)((c + 9) & 0xf);
                        }
                        else if (c > 0x40)
                        {
                            if (c > 0x46)
                            {
                                throw new ArgumentException($"Illegal character {c} at position {rBuf - buf}");
                            }

                            res[i] += (byte)((c + 9) & 0xf);
                        }
                        else if (c > 0x2F)
                        {
                            if (c > 0x39)
                            {
                                throw new ArgumentException($"Illegal character {c} at position {rBuf - buf}");
                            }

                            res[i] += (byte)(c & 0xf);
                        }
                        else
                        {
                            throw new ArgumentException($"Illegal character {c} at position {rBuf - buf}");
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < len; i++)
                    {
                        c = *rBuf++;
                        if (c < 0x3A)
                        {
                            res[i] = (byte)(c << 4);
                        }
                        else
                        {
                            res[i] = (byte)((c + 9) << 4);
                        }

                        c = *rBuf++;
                        if (c < 0x3A)
                        {
                            res[i] += (byte)(c & 0xf);
                        }
                        else
                        {
                            res[i] += (byte)((c + 9) & 0xf);
                        }
                    }
                }

                return res;
            }
        }

        #endregion
    }
}
