using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using static Fp.Processor;

namespace Fp
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public partial class Processor
    {
        #region Memory match

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="sourceMaxOffset">Upper bound (exclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="maxCount">Maximum matches</param>
        /// <returns>Enumerator for matches</returns>
        // ReSharper disable once MemberCanBeProtected.Global
        public static IEnumerable<int> Match(ReadOnlyMemory<byte> source, int sourceOffset, int sourceMaxOffset,
            ReadOnlyMemory<byte> match, int matchOffset, int matchLength, int maxCount = int.MaxValue)
        {
            if (maxCount < 1)
            {
                throw new ArgumentException($"{nameof(maxCount)} has value {maxCount} but must be at least 1");
            }

            int count = 0;
            int basePos = sourceOffset;
            var subMatch = match.Slice(matchOffset, matchLength);
            while (basePos + matchLength <= sourceMaxOffset && count < maxCount)
            {
                if (source.Span.Slice(basePos, matchLength).SequenceEqual(subMatch.Span))
                {
                    yield return basePos;
                    count++;
                    basePos += matchLength;
                }
                else
                    basePos++;
            }
        }

        /// <summary>
        /// Get all occurrences of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="sourceMaxOffset">Upper bound (exclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="maxCount">Maximum matches</param>
        /// <returns>Enumerator for matches</returns>
        public static List<int> Match(ReadOnlySpan<byte> source, int sourceOffset, int sourceMaxOffset,
            ReadOnlySpan<byte> match,
            int maxCount = int.MaxValue)
        {
            if (maxCount < 1)
            {
                throw new ArgumentException($"{nameof(maxCount)} has value {maxCount} but must be at least 1");
            }

            List<int> res = new();
            int count = 0;
            int basePos = sourceOffset;
            while (basePos + match.Length <= sourceMaxOffset && count < maxCount)
            {
                if (source.Slice(basePos, match.Length).SequenceEqual(match))
                {
                    res.Add(basePos);
                    count++;
                    basePos += match.Length;
                }
                else
                    basePos++;
            }

            return res;
        }

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<int> Match(ReadOnlyMemory<byte> source, int sourceOffset, ReadOnlyMemory<byte> match,
            int matchOffset, int matchLength)
            => Match(source, sourceOffset, int.MaxValue, match, matchOffset, matchLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<int> Match(ReadOnlyMemory<byte> source, ReadOnlyMemory<byte> match, int matchOffset,
            int matchLength)
            => Match(source, 0, int.MaxValue, match, matchOffset, matchLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="sourceMaxOffset">Upper bound (exclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<int> Match(ReadOnlyMemory<byte> source, int sourceOffset, int sourceMaxOffset,
            ReadOnlyMemory<byte> match)
            => Match(source, sourceOffset, sourceMaxOffset, match, 0, match.Length);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<int> Match(ReadOnlyMemory<byte> source, int sourceOffset, ReadOnlyMemory<byte> match)
            => Match(source, sourceOffset, int.MaxValue, match, 0, match.Length);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="match">Target to match</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<int> Match(ReadOnlyMemory<byte> source, ReadOnlyMemory<byte> match)
            => Match(source, 0, int.MaxValue, match, 0, match.Length);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="sourceMaxOffset">Upper bound (exclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<int> Match(ReadOnlyMemory<byte> source, int sourceOffset, int sourceMaxOffset,
            string match) =>
            Match(source, sourceOffset, sourceMaxOffset, Ascii(match));

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<int> Match(ReadOnlyMemory<byte> source, int sourceOffset, string match) =>
            Match(source, sourceOffset, int.MaxValue, Ascii(match));

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="match">Target to match</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<int> Match(ReadOnlyMemory<byte> source, string match) =>
            Match(source, 0, int.MaxValue, Ascii(match));

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="sourceMaxOffset">Upper bound (exclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static int MatchFirst(ReadOnlyMemory<byte> source, int sourceOffset, int sourceMaxOffset,
            ReadOnlyMemory<byte> match, int matchOffset, int matchLength)
        {
            foreach (int v in Match(source, sourceOffset, sourceMaxOffset, match, matchOffset, matchLength))
            {
                return v;
            }

            return -1;
        }

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static int MatchFirst(ReadOnlyMemory<byte> source, int sourceOffset, ReadOnlyMemory<byte> match,
            int matchOffset,
            int matchLength)
            => MatchFirst(source, sourceOffset, int.MaxValue, match, matchOffset, matchLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static int MatchFirst(ReadOnlyMemory<byte> source, ReadOnlyMemory<byte> match, int matchOffset,
            int matchLength)
            => MatchFirst(source, 0, int.MaxValue, match, matchOffset, matchLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="sourceMaxOffset">Upper bound (exclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static int MatchFirst(ReadOnlyMemory<byte> source, int sourceOffset, int sourceMaxOffset,
            ReadOnlyMemory<byte> match)
            => MatchFirst(source, sourceOffset, sourceMaxOffset, match, 0, match.Length);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static int MatchFirst(ReadOnlyMemory<byte> source, int sourceOffset, ReadOnlyMemory<byte> match)
            => MatchFirst(source, sourceOffset, int.MaxValue, match, 0, match.Length);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static int MatchFirst(ReadOnlyMemory<byte> source, ReadOnlyMemory<byte> match)
            => MatchFirst(source, 0, int.MaxValue, match, 0, match.Length);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="sourceMaxOffset">Upper bound (exclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static int MatchFirst(ReadOnlyMemory<byte> source, int sourceOffset, int sourceMaxOffset,
            string match) =>
            MatchFirst(source, sourceOffset, sourceMaxOffset, Ascii(match));

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static int MatchFirst(ReadOnlyMemory<byte> source, int sourceOffset, string match) =>
            MatchFirst(source, sourceOffset, int.MaxValue, Ascii(match));

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static int MatchFirst(ReadOnlyMemory<byte> source, string match) =>
            MatchFirst(source, 0, int.MaxValue, Ascii(match));

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="sourceMaxOffset">Upper bound (exclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static int MatchLast(ReadOnlyMemory<byte> source, int sourceOffset, int sourceMaxOffset,
            ReadOnlyMemory<byte> match,
            int matchOffset, int matchLength)
        {
            int u = -1;
            foreach (int v in Match(source, sourceOffset, sourceMaxOffset, match, matchOffset, matchLength))
            {
                u = v;
            }

            return u;
        }

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static int MatchLast(ReadOnlyMemory<byte> source, int sourceOffset, ReadOnlyMemory<byte> match,
            int matchOffset,
            int matchLength)
            => MatchLast(source, sourceOffset, int.MaxValue, match, matchOffset, matchLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static int MatchLast(ReadOnlyMemory<byte> source, ReadOnlyMemory<byte> match, int matchOffset,
            int matchLength)
            => MatchLast(source, 0, int.MaxValue, match, matchOffset, matchLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="sourceMaxOffset">Upper bound (exclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static int MatchLast(ReadOnlyMemory<byte> source, int sourceOffset, int sourceMaxOffset,
            ReadOnlyMemory<byte> match)
            => MatchLast(source, sourceOffset, sourceMaxOffset, match, 0, match.Length);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static int MatchLast(ReadOnlyMemory<byte> source, int sourceOffset, ReadOnlyMemory<byte> match)
            => MatchLast(source, sourceOffset, int.MaxValue, match, 0, match.Length);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static int MatchLast(ReadOnlyMemory<byte> source, ReadOnlyMemory<byte> match)
            => MatchLast(source, 0, int.MaxValue, match, 0, match.Length);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="sourceMaxOffset">Upper bound (exclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static int MatchLast(ReadOnlyMemory<byte> source, int sourceOffset, int sourceMaxOffset, string match) =>
            MatchLast(source, sourceOffset, sourceMaxOffset, Ascii(match));

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static int MatchLast(ReadOnlyMemory<byte> source, int sourceOffset, string match) =>
            MatchLast(source, sourceOffset, int.MaxValue, Ascii(match));

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static int MatchLast(ReadOnlyMemory<byte> source, string match) =>
            MatchLast(source, 0, int.MaxValue, Ascii(match));

        #endregion
    }


    // ReSharper disable InconsistentNaming
    public partial class Scripting
    {
        #region Matching

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="sourceMaxOffset">Upper bound (exclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="maxCount">Maximum matches</param>
        /// <returns>Enumerator for matches</returns>
        // ReSharper disable once MemberCanBeProtected.Global
        public static IEnumerable<int> match(ReadOnlyMemory<byte> source, int sourceOffset, int sourceMaxOffset,
            ReadOnlyMemory<byte> match, int matchOffset, int matchLength, int maxCount = int.MaxValue)
            => Match(source, sourceOffset, sourceMaxOffset, match, matchOffset, matchLength, maxCount);

        /// <summary>
        /// Get all occurrences of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="sourceMaxOffset">Upper bound (exclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="maxCount">Maximum matches</param>
        /// <returns>Enumerator for matches</returns>
        public static List<int> match(ReadOnlySpan<byte> source, int sourceOffset, int sourceMaxOffset,
            ReadOnlySpan<byte> match,
            int maxCount = int.MaxValue)
            => Match(source, sourceOffset, sourceMaxOffset, match, maxCount);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<int> match(ReadOnlyMemory<byte> source, int sourceOffset, ReadOnlyMemory<byte> match,
            int matchOffset, int matchLength)
            => Match(source, sourceOffset, match, matchOffset, matchLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<int> match(ReadOnlyMemory<byte> source, ReadOnlyMemory<byte> match, int matchOffset,
            int matchLength)
            => Match(source, match, matchOffset, matchLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="sourceMaxOffset">Upper bound (exclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<int> match(ReadOnlyMemory<byte> source, int sourceOffset, int sourceMaxOffset,
            ReadOnlyMemory<byte> match)
            => Match(source, sourceOffset, sourceMaxOffset, match);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<int> match(ReadOnlyMemory<byte> source, int sourceOffset, ReadOnlyMemory<byte> match)
            => Match(source, sourceOffset, match);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="match">Target to match</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<int> match(ReadOnlyMemory<byte> source, ReadOnlyMemory<byte> match)
            => Match(source, match);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="sourceMaxOffset">Upper bound (exclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<int> match(ReadOnlyMemory<byte> source, int sourceOffset, int sourceMaxOffset,
            string match) =>
            Match(source, sourceOffset, sourceMaxOffset, match);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<int> match(ReadOnlyMemory<byte> source, int sourceOffset, string match) =>
            Match(source, sourceOffset, int.MaxValue, match);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="match">Target to match</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<int> match(ReadOnlyMemory<byte> source, string match) =>
            Match(source, match);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="sourceMaxOffset">Upper bound (exclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static int matchFirst(ReadOnlyMemory<byte> source, int sourceOffset, int sourceMaxOffset,
            ReadOnlyMemory<byte> match, int matchOffset, int matchLength)
            => MatchFirst(source, sourceOffset, sourceMaxOffset, match, matchOffset, matchLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static int matchFirst(ReadOnlyMemory<byte> source, int sourceOffset, ReadOnlyMemory<byte> match,
            int matchOffset,
            int matchLength)
            => MatchFirst(source, sourceOffset, match, matchOffset, matchLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static int matchFirst(ReadOnlyMemory<byte> source, ReadOnlyMemory<byte> match, int matchOffset,
            int matchLength)
            => MatchFirst(source, match, matchOffset, matchLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="sourceMaxOffset">Upper bound (exclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static int matchFirst(ReadOnlyMemory<byte> source, int sourceOffset, int sourceMaxOffset,
            ReadOnlyMemory<byte> match)
            => MatchFirst(source, sourceOffset, sourceMaxOffset, match);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static int matchFirst(ReadOnlyMemory<byte> source, int sourceOffset, ReadOnlyMemory<byte> match)
            => MatchFirst(source, sourceOffset, match);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static int matchFirst(ReadOnlyMemory<byte> source, ReadOnlyMemory<byte> match)
            => MatchFirst(source, match);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="sourceMaxOffset">Upper bound (exclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static int matchFirst(ReadOnlyMemory<byte> source, int sourceOffset, int sourceMaxOffset,
            string match) =>
            MatchFirst(source, sourceOffset, sourceMaxOffset, match);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static int matchFirst(ReadOnlyMemory<byte> source, int sourceOffset, string match) =>
            MatchFirst(source, sourceOffset, match);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static int matchFirst(ReadOnlyMemory<byte> source, string match) =>
            MatchFirst(source, match);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="sourceMaxOffset">Upper bound (exclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static int matchLast(ReadOnlyMemory<byte> source, int sourceOffset, int sourceMaxOffset,
            ReadOnlyMemory<byte> match,
            int matchOffset, int matchLength)
            => MatchLast(source, sourceOffset, sourceMaxOffset, match, matchOffset, matchLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static int matchLast(ReadOnlyMemory<byte> source, int sourceOffset, ReadOnlyMemory<byte> match,
            int matchOffset,
            int matchLength)
            => MatchLast(source, sourceOffset, match, matchOffset, matchLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static int matchLast(ReadOnlyMemory<byte> source, ReadOnlyMemory<byte> match, int matchOffset,
            int matchLength)
            => MatchLast(source, match, matchOffset, matchLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="sourceMaxOffset">Upper bound (exclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static int matchLast(ReadOnlyMemory<byte> source, int sourceOffset, int sourceMaxOffset,
            ReadOnlyMemory<byte> match)
            => MatchLast(source, sourceOffset, sourceMaxOffset, match);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static int matchLast(ReadOnlyMemory<byte> source, int sourceOffset, ReadOnlyMemory<byte> match)
            => MatchLast(source, sourceOffset, match);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static int matchLast(ReadOnlyMemory<byte> source, ReadOnlyMemory<byte> match)
            => MatchLast(source, match);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="sourceMaxOffset">Upper bound (exclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static int matchLast(ReadOnlyMemory<byte> source, int sourceOffset, int sourceMaxOffset, string match) =>
            MatchLast(source, sourceOffset, sourceMaxOffset, match);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="sourceOffset">Lower bound (inclusive) of positions to search</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static int matchLast(ReadOnlyMemory<byte> source, int sourceOffset, string match) =>
            MatchLast(source, sourceOffset, match);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="source">Data to read from</param>
        /// <param name="match">Target to match</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static int matchLast(ReadOnlyMemory<byte> source, string match) =>
            MatchLast(source, match);

        #endregion
    }
    // ReSharper restore InconsistentNaming
}
