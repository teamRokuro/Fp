using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using static System.Buffers.ArrayPool<byte>;

namespace Fp
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public partial class Processor
    {
        #region Pattern matching utilities

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="maxCount">Maximum matches</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        // ReSharper disable once MemberCanBeProtected.Global
        public static IEnumerable<long> Match(Stream stream, long streamOffset, long streamMaxOffset,
            ReadOnlyMemory<byte> match, int matchOffset, int matchLength, int maxCount = int.MaxValue,
            int bufferLength = 4096)
        {
            if (maxCount < 1)
            {
                throw new ArgumentException($"{nameof(maxCount)} has value {maxCount} but must be at least 1");
            }

            int count = 0;
            long initPos = stream.Position;
            byte[][] buffers = new byte[2][];
            try
            {
                long[] realPositions = new long[2];
                for (int i = 0; i < buffers.Length; i++)
                {
                    buffers[i] = Shared.Rent(Math.Max(matchLength, bufferLength));
                }

                long basePos = streamOffset;
                realPositions[0] = basePos;
                long curPos = basePos;
                int currentReadBufferIndex = 0;
                int currentReadBufferOffset = 0;
                int latest = 0;

                // Loop while offset allows checking for another match
                while (basePos + matchLength < streamMaxOffset)
                {
                    int read;
                    int latestFilled = 0;
                    stream.Position = streamOffset;
                    // Read to fill buffer
                    do
                    {
                        latestFilled += read = stream.Read(buffers[latest], latestFilled,
                            (int)Math.Min(streamMaxOffset - basePos, buffers[latest].Length) - latestFilled);
                    } while (read != 0);

                    // Leave on failure to read (reached end)
                    if (latestFilled == 0)
                    {
                        break;
                    }

                    streamOffset += latestFilled;
                    // Loop while loaded buffers allow read
                    while (curPos + matchLength <= realPositions[latest] + latestFilled)
                    {
                        int tempBufIndex = currentReadBufferIndex;
                        int tempBufOffset = currentReadBufferOffset;
                        bool ok = true;
                        // Check for current offset
                        for (int i = 0; i < matchLength && ok; i++)
                        {
                            if (buffers[tempBufIndex][tempBufOffset] != match.Span[matchOffset + i])
                            {
                                ok = false;
                            }
                            else
                            {
                                // Update current read buffer
                                tempBufOffset++;
                                if (tempBufOffset < buffers[tempBufIndex].Length)
                                {
                                    continue;
                                }

                                tempBufIndex = (tempBufIndex + 1) % 2;
                                tempBufOffset = 0;
                            }
                        }

                        if (ok)
                        {
                            yield return curPos;
                            count++;
                            if (count == maxCount)
                            {
                                yield break;
                            }

                            curPos += matchLength;
                            currentReadBufferOffset += matchLength;
                        }
                        else
                        {
                            curPos++;
                            currentReadBufferOffset++;
                        }

                        // Update current read buffer
                        if (currentReadBufferOffset < buffers[currentReadBufferIndex].Length)
                        {
                            continue;
                        }

                        int over = currentReadBufferOffset - buffers[currentReadBufferIndex].Length;
                        // Safe to increment buffer index by 1 because buffer length is at least the length of match subarray
                        currentReadBufferIndex = (currentReadBufferIndex + 1) % 2;
                        currentReadBufferOffset = over;
                    }

                    basePos += latestFilled;
                    // Check if current buffer was fully populated (prepare for next)
                    if (latestFilled != buffers[latest].Length)
                    {
                        continue;
                    }

                    latest = (latest + 1) % 2;
                    realPositions[latest] = basePos;
                }
            }
            finally
            {
                if (buffers != null)
                {
                    foreach (byte[] t in buffers)
                    {
                        if (t != null)
                        {
                            Shared.Return(t);
                        }
                    }
                }

                stream.Position = initPos;
            }
        }

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="maxCount">Maximum matches</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static List<long> Match(Stream stream, long streamOffset, long streamMaxOffset, ReadOnlySpan<byte> match,
            int maxCount = int.MaxValue, int bufferLength = 4096)
        {
            if (maxCount < 1)
            {
                throw new ArgumentException($"{nameof(maxCount)} has value {maxCount} but must be at least 1");
            }

            int count = 0;
            List<long> res = new List<long>();
            long initPos = stream.Position;
            int matchLength = match.Length;
            byte[][] buffers = new byte[2][];
            try
            {
                long[] realPositions = new long[2];
                for (int i = 0; i < buffers.Length; i++)
                {
                    buffers[i] = Shared.Rent(Math.Max(matchLength, bufferLength));
                }

                long basePos = streamOffset;
                realPositions[0] = basePos;
                long curPos = basePos;
                int currentReadBufferIndex = 0;
                int currentReadBufferOffset = 0;
                int latest = 0;

                // Loop while offset allows checking for another match
                while (basePos + matchLength < streamMaxOffset)
                {
                    int read;
                    int latestFilled = 0;
                    stream.Position = streamOffset;
                    // Read to fill buffer
                    do
                    {
                        latestFilled += read = stream.Read(buffers[latest], latestFilled,
                            (int)Math.Min(streamMaxOffset - basePos, buffers[latest].Length) - latestFilled);
                    } while (read != 0);

                    // Leave on failure to read (reached end)
                    if (latestFilled == 0)
                    {
                        break;
                    }

                    streamOffset += latestFilled;
                    // Loop while loaded buffers allow read
                    while (curPos + matchLength <= realPositions[latest] + latestFilled)
                    {
                        int tempBufIndex = currentReadBufferIndex;
                        int tempBufOffset = currentReadBufferOffset;
                        bool ok = true;
                        // Check for current offset
                        for (int i = 0; i < matchLength && ok; i++)
                        {
                            if (buffers[tempBufIndex][tempBufOffset] != match[i])
                            {
                                ok = false;
                            }
                            else
                            {
                                // Update current read buffer
                                tempBufOffset++;
                                if (tempBufOffset < buffers[tempBufIndex].Length)
                                {
                                    continue;
                                }

                                tempBufIndex = (tempBufIndex + 1) % 2;
                                tempBufOffset = 0;
                            }
                        }

                        if (ok)
                        {
                            res.Add(curPos);
                            count++;
                            if (count == maxCount)
                            {
                                return res;
                            }

                            curPos += matchLength;
                            currentReadBufferOffset += matchLength;
                        }
                        else
                        {
                            curPos++;
                            currentReadBufferOffset++;
                        }

                        // Update current read buffer
                        if (currentReadBufferOffset < buffers[currentReadBufferIndex].Length)
                        {
                            continue;
                        }

                        int over = currentReadBufferOffset - buffers[currentReadBufferIndex].Length;
                        // Safe to increment buffer index by 1 because buffer length is at least the length of match subarray
                        currentReadBufferIndex = (currentReadBufferIndex + 1) % 2;
                        currentReadBufferOffset = over;
                    }

                    basePos += latestFilled;
                    // Check if current buffer was fully populated (prepare for next)
                    if (latestFilled != buffers[latest].Length)
                    {
                        continue;
                    }

                    latest = (latest + 1) % 2;
                    realPositions[latest] = basePos;
                }

                return res;
            }
            finally
            {
                if (buffers != null)
                {
                    foreach (byte[] t in buffers)
                    {
                        if (t != null)
                        {
                            Shared.Return(t);
                        }
                    }
                }

                stream.Position = initPos;
            }
        }

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> Match(Stream stream, long streamOffset, ReadOnlyMemory<byte> match,
            int matchOffset, int matchLength, int bufferLength = 4096)
            => Match(stream, streamOffset, long.MaxValue, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> Match(Stream stream, ReadOnlyMemory<byte> match, int matchOffset,
            int matchLength, int bufferLength = 4096)
            => Match(stream, 0, long.MaxValue, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> Match(Stream stream, long streamOffset, long streamMaxOffset,
            ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => Match(stream, streamOffset, streamMaxOffset, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> Match(Stream stream, long streamOffset, ReadOnlyMemory<byte> match,
            int bufferLength = 4096)
            => Match(stream, streamOffset, long.MaxValue, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> Match(Stream stream, ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => Match(stream, 0, long.MaxValue, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> Match(Stream stream, long streamOffset, long streamMaxOffset, string match,
            int bufferLength = 4096)
        {
            byte[] matchArr = Encoding.ASCII.GetBytes(match);
            return Match(stream, streamOffset, streamMaxOffset, matchArr, 0, matchArr.Length, bufferLength);
        }

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> Match(Stream stream, long streamOffset, string match, int bufferLength = 4096)
        {
            byte[] matchArr = Encoding.ASCII.GetBytes(match);
            return Match(stream, streamOffset, long.MaxValue, matchArr, 0, matchArr.Length, bufferLength);
        }

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> Match(Stream stream, string match, int bufferLength = 4096)
        {
            byte[] matchArr = Encoding.ASCII.GetBytes(match);
            return Match(stream, 0, long.MaxValue, matchArr, 0, matchArr.Length, bufferLength);
        }

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long MatchFirst(Stream stream, long streamOffset, long streamMaxOffset,
            ReadOnlyMemory<byte> match, int matchOffset, int matchLength, int bufferLength = 4096)
        {
            foreach (long v in Match(stream, streamOffset, streamMaxOffset, match, matchOffset, matchLength,
                bufferLength))
            {
                return v;
            }

            return -1;
        }

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long MatchFirst(Stream stream, long streamOffset, ReadOnlyMemory<byte> match, int matchOffset,
            int matchLength, int bufferLength = 4096)
            => MatchFirst(stream, streamOffset, long.MaxValue, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long MatchFirst(Stream stream, ReadOnlyMemory<byte> match, int matchOffset, int matchLength,
            int bufferLength = 4096)
            => MatchFirst(stream, 0, long.MaxValue, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long MatchFirst(Stream stream, long streamOffset, long streamMaxOffset,
            ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => MatchFirst(stream, streamOffset, streamMaxOffset, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long MatchFirst(Stream stream, long streamOffset, ReadOnlyMemory<byte> match,
            int bufferLength = 4096)
            => MatchFirst(stream, streamOffset, long.MaxValue, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long MatchFirst(Stream stream, ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => MatchFirst(stream, 0, long.MaxValue, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long MatchFirst(Stream stream, long streamOffset, long streamMaxOffset, string match,
            int bufferLength = 4096)
        {
            byte[] matchArr = Encoding.ASCII.GetBytes(match);
            return MatchFirst(stream, streamOffset, streamMaxOffset, matchArr, 0, matchArr.Length, bufferLength);
        }

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long MatchFirst(Stream stream, long streamOffset, string match, int bufferLength = 4096)
        {
            byte[] matchArr = Encoding.ASCII.GetBytes(match);
            return MatchFirst(stream, streamOffset, long.MaxValue, matchArr, 0, matchArr.Length, bufferLength);
        }

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long MatchFirst(Stream stream, string match, int bufferLength = 4096)
        {
            byte[] matchArr = Encoding.ASCII.GetBytes(match);
            return MatchFirst(stream, 0, long.MaxValue, matchArr, 0, matchArr.Length, bufferLength);
        }

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long MatchLast(Stream stream, long streamOffset, long streamMaxOffset, ReadOnlyMemory<byte> match,
            int matchOffset, int matchLength, int bufferLength = 4096)
        {
            long u = -1;
            foreach (long v in Match(stream, streamOffset, streamMaxOffset, match, matchOffset, matchLength,
                bufferLength))
            {
                u = v;
            }

            return u;
        }

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long MatchLast(Stream stream, long streamOffset, ReadOnlyMemory<byte> match, int matchOffset,
            int matchLength, int bufferLength = 4096)
            => MatchLast(stream, streamOffset, long.MaxValue, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long MatchLast(Stream stream, ReadOnlyMemory<byte> match, int matchOffset, int matchLength,
            int bufferLength = 4096)
            => MatchLast(stream, 0, long.MaxValue, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long MatchLast(Stream stream, long streamOffset, long streamMaxOffset, ReadOnlyMemory<byte> match,
            int bufferLength = 4096)
            => MatchLast(stream, streamOffset, streamMaxOffset, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long MatchLast(Stream stream, long streamOffset, ReadOnlyMemory<byte> match,
            int bufferLength = 4096)
            => MatchLast(stream, streamOffset, long.MaxValue, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long MatchLast(Stream stream, ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => MatchLast(stream, 0, long.MaxValue, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long MatchLast(Stream stream, long streamOffset, long streamMaxOffset, string match,
            int bufferLength = 4096)
        {
            byte[] matchArr = Encoding.ASCII.GetBytes(match);
            return MatchLast(stream, streamOffset, streamMaxOffset, matchArr, 0, matchArr.Length, bufferLength);
        }

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long MatchLast(Stream stream, long streamOffset, string match, int bufferLength = 4096)
        {
            byte[] matchArr = Encoding.ASCII.GetBytes(match);
            return MatchLast(stream, streamOffset, long.MaxValue, matchArr, 0, matchArr.Length, bufferLength);
        }

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long MatchLast(Stream stream, string match, int bufferLength = 4096)
        {
            byte[] matchArr = Encoding.ASCII.GetBytes(match);
            return MatchLast(stream, 0, long.MaxValue, matchArr, 0, matchArr.Length, bufferLength);
        }

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public IEnumerable<long> Match(long streamOffset, long streamMaxOffset, ReadOnlyMemory<byte> match,
            int matchOffset, int matchLength, int bufferLength = 4096)
            => Match(InputStream ?? throw new InvalidOperationException(), streamOffset, streamMaxOffset, match,
                matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public IEnumerable<long> Match(long streamOffset, ReadOnlyMemory<byte> match, int matchOffset, int matchLength,
            int bufferLength = 4096)
            => Match(InputStream ?? throw new InvalidOperationException(), streamOffset, long.MaxValue, match,
                matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public IEnumerable<long> Match(ReadOnlyMemory<byte> match, int matchOffset, int matchLength,
            int bufferLength = 4096)
            => Match(InputStream ?? throw new InvalidOperationException(), 0, long.MaxValue, match, matchOffset,
                matchLength, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public IEnumerable<long> Match(long streamOffset, long streamMaxOffset, byte[] match,
            int bufferLength = 4096)
            => Match(InputStream ?? throw new InvalidOperationException(), streamOffset, streamMaxOffset, match, 0,
                match.Length, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public IEnumerable<long> Match(long streamOffset, ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => Match(InputStream ?? throw new InvalidOperationException(), streamOffset, long.MaxValue, match, 0,
                match.Length, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public IEnumerable<long> Match(ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => Match(InputStream ?? throw new InvalidOperationException(), 0, long.MaxValue, match, 0, match.Length,
                bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public IEnumerable<long> Match(long streamOffset, long streamMaxOffset, string match,
            int bufferLength = 4096)
            => Match(InputStream ?? throw new InvalidOperationException(), streamOffset, streamMaxOffset, match,
                bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public IEnumerable<long> Match(long streamOffset, string match, int bufferLength = 4096)
            => Match(InputStream ?? throw new InvalidOperationException(), streamOffset, long.MaxValue, match,
                bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public IEnumerable<long> Match(string match, int bufferLength = 4096)
            => Match(InputStream ?? throw new InvalidOperationException(), 0, long.MaxValue, match, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchFirst(long streamOffset, long streamMaxOffset, ReadOnlyMemory<byte> match, int matchOffset,
            int matchLength, int bufferLength = 4096)
        {
            foreach (long v in Match(InputStream ?? throw new InvalidOperationException(), streamOffset,
                streamMaxOffset,
                match, matchOffset, matchLength,
                bufferLength))
            {
                return v;
            }

            return -1;
        }

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchFirst(long streamOffset, ReadOnlyMemory<byte> match, int matchOffset, int matchLength,
            int bufferLength = 4096)
            => MatchFirst(InputStream ?? throw new InvalidOperationException(), streamOffset, long.MaxValue, match,
                matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchFirst(ReadOnlyMemory<byte> match, int matchOffset, int matchLength, int bufferLength = 4096)
            => MatchFirst(InputStream ?? throw new InvalidOperationException(), 0, long.MaxValue, match, matchOffset,
                matchLength, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchFirst(long streamOffset, long streamMaxOffset, ReadOnlyMemory<byte> match,
            int bufferLength = 4096)
            => MatchFirst(InputStream ?? throw new InvalidOperationException(), streamOffset, streamMaxOffset, match, 0,
                match.Length, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchFirst(long streamOffset, ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => MatchFirst(InputStream ?? throw new InvalidOperationException(), streamOffset, long.MaxValue, match, 0,
                match.Length, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchFirst(ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => MatchFirst(InputStream ?? throw new InvalidOperationException(), 0, long.MaxValue, match, 0,
                match.Length, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchFirst(long streamOffset, long streamMaxOffset, string match,
            int bufferLength = 4096)
            => MatchFirst(InputStream ?? throw new InvalidOperationException(), streamOffset, streamMaxOffset, match,
                bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchFirst(long streamOffset, string match, int bufferLength = 4096)
            => MatchFirst(InputStream ?? throw new InvalidOperationException(), streamOffset, long.MaxValue, match,
                bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchFirst(string match, int bufferLength = 4096)
            => MatchFirst(InputStream ?? throw new InvalidOperationException(), 0, long.MaxValue, match, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public long MatchLast(long streamOffset, long streamMaxOffset, ReadOnlyMemory<byte> match, int matchOffset,
            int matchLength, int bufferLength = 4096)
        {
            long u = -1;
            foreach (long v in Match(InputStream ?? throw new InvalidOperationException(), streamOffset,
                streamMaxOffset,
                match, matchOffset, matchLength,
                bufferLength))
            {
                u = v;
            }

            return u;
        }

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public long MatchLast(long streamOffset, ReadOnlyMemory<byte> match, int matchOffset, int matchLength,
            int bufferLength = 4096)
            => MatchLast(InputStream ?? throw new InvalidOperationException(), streamOffset, long.MaxValue, match,
                matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public long MatchLast(ReadOnlyMemory<byte> match, int matchOffset, int matchLength, int bufferLength = 4096)
            => MatchLast(InputStream ?? throw new InvalidOperationException(), 0, long.MaxValue, match, matchOffset,
                matchLength, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public long MatchLast(long streamOffset, long streamMaxOffset, ReadOnlyMemory<byte> match,
            int bufferLength = 4096)
            => MatchLast(InputStream ?? throw new InvalidOperationException(), streamOffset, streamMaxOffset, match, 0,
                match.Length, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public long MatchLast(long streamOffset, ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => MatchLast(InputStream ?? throw new InvalidOperationException(), streamOffset, long.MaxValue, match, 0,
                match.Length, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public long MatchLast(ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => MatchLast(InputStream ?? throw new InvalidOperationException(), 0, long.MaxValue, match, 0, match.Length,
                bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public long MatchLast(long streamOffset, long streamMaxOffset, string match,
            int bufferLength = 4096)
            => MatchLast(InputStream ?? throw new InvalidOperationException(), streamOffset, streamMaxOffset, match,
                bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public long MatchLast(long streamOffset, string match, int bufferLength = 4096)
        {
            byte[] matchArr = Encoding.ASCII.GetBytes(match);
            return MatchLast(InputStream ?? throw new InvalidOperationException(), streamOffset, long.MaxValue,
                matchArr, 0, matchArr.Length, bufferLength);
        }

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public long MatchLast(string match, int bufferLength = 4096)
            => MatchLast(InputStream ?? throw new InvalidOperationException(), 0, long.MaxValue, match, bufferLength);

        private static long WriteBaseStream(Stream stream, long length, Stream outputStream, bool lenient,
            int bufferLength)
        {
            long outLen = 0;
            byte[] buffer = Shared.Rent(bufferLength);
            try
            {
                long left = length;
                int read;
                do
                {
                    read = stream.Read(buffer, 0, (int)Math.Min(left, buffer.Length));
                    outputStream.Write(buffer, 0, read);
                    left -= read;
                    outLen += read;
                } while (left > 0 && read != 0);

                if (left > 0 && read != 0 && !lenient)
                {
                    throw new ProcessorException(
                        $"Failed to read required number of bytes! 0x{read:X} read, 0x{left:X} left, 0x{stream.Position:X} end position");
                }
            }
            finally
            {
                Shared.Return(buffer);
            }

            return outLen;
        }

        #endregion
    }
}
