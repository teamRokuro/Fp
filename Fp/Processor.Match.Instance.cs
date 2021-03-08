using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Fp
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public partial class Processor
    {
        #region Instance stream match

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
        public long MatchLast(long streamOffset, string match, int bufferLength = 4096) => MatchLast(
            InputStream ?? throw new InvalidOperationException(), streamOffset, long.MaxValue, match, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public long MatchLast(string match, int bufferLength = 4096)
            => MatchLast(InputStream ?? throw new InvalidOperationException(), 0, long.MaxValue, match, bufferLength);

        #endregion
    }
}