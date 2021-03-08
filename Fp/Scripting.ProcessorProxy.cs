using System;
using System.Collections.Generic;
using System.IO;
using static Fp.Processor;

namespace Fp
{
    // ReSharper disable InconsistentNaming
    /// <summary>
    /// Provides scripting-related functions/properties.
    /// </summary>
    public static partial class Scripting
    {
        #region Decoding

        /// <summary>
        /// Create byte array from hex string
        /// </summary>
        /// <param name="hex">Hex string to decode</param>
        /// <param name="validate">Validate characters</param>
        /// <returns>Array with decoded hex string</returns>
        /// <exception cref="ArgumentException">If string has odd length</exception>
        public static byte[] decodeHex(string hex, bool validate = true) => DecodeHex(hex, validate);

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

        #region Conversions

        /// <summary>
        /// Get byte array from string assuming 8-bit characters.
        /// </summary>
        /// <param name="text">String to process.</param>
        /// <returns>Byte array containing lower byte of each code unit in the string.</returns>
        public static byte[] ascii(this string text) => Ascii(text);

        #endregion

        #region Logging

        /// <summary>
        /// Invoke logger with formatted string containing specified log
        /// </summary>
        /// <param name="log">Message</param>
        public static void info(string log) => Current.LogInfo(log);

        /// <summary>
        /// Invoke logger with formatted string containing specified log
        /// </summary>
        /// <param name="log">Message</param>
        public static void warn(string log) => Current.LogWarn(log);

        /// <summary>
        /// Invoke logger with formatted string containing specified log
        /// </summary>
        /// <param name="log">Message</param>
        public static void fail(string log) => Current.LogFail(log);

        #endregion

        #region Magic

        /// <summary>
        /// Checks for identifier.
        /// </summary>
        /// <param name="source">Source to read.</param>v
        /// <param name="offset">Source offset.</param>
        /// <param name="text">Value to check for.</param>
        /// <returns>True if found.</returns>
        public static bool magic(this byte[] source, ReadOnlySpan<byte> text, int offset = 0) =>
            HasMagic(source, text, offset);

        /// <summary>
        /// Checks for identifier.
        /// </summary>
        /// <param name="source">Source to read.</param>v
        /// <param name="offset">Source offset.</param>
        /// <param name="text">Value to check for.</param>
        /// <returns>True if found.</returns>
        public static bool magic(this byte[] source, string text, int offset = 0) =>
            HasMagic(source, text, offset);

        /// <summary>
        /// Checks for identifier.
        /// </summary>
        /// <param name="source">Source to read.</param>v
        /// <param name="offset">Source offset.</param>
        /// <param name="text">Value to check for.</param>
        /// <returns>True if found.</returns>
        public static bool magic(this Memory<byte> source, ReadOnlySpan<byte> text, int offset = 0) =>
            HasMagic(source.Span, text, offset);

        /// <summary>
        /// Checks for identifier.
        /// </summary>
        /// <param name="source">Source to read.</param>v
        /// <param name="offset">Source offset.</param>
        /// <param name="text">Value to check for.</param>
        /// <returns>True if found.</returns>
        public static bool magic(this Memory<byte> source, string text, int offset = 0) =>
            HasMagic(source.Span, text, offset);

        /// <summary>
        /// Checks for identifier.
        /// </summary>
        /// <param name="source">Source to read.</param>v
        /// <param name="offset">Source offset.</param>
        /// <param name="text">Value to check for.</param>
        /// <returns>True if found.</returns>
        public static bool magic(this ReadOnlyMemory<byte> source, ReadOnlySpan<byte> text, int offset = 0) =>
            HasMagic(source.Span, text, offset);

        /// <summary>
        /// Checks for identifier.
        /// </summary>
        /// <param name="source">Source to read.</param>v
        /// <param name="offset">Source offset.</param>
        /// <param name="text">Value to check for.</param>
        /// <returns>True if found.</returns>
        public static bool magic(this ReadOnlyMemory<byte> source, string text, int offset = 0) =>
            HasMagic(source.Span, text, offset);

        /// <summary>
        /// Checks for identifier.
        /// </summary>
        /// <param name="source">Source to read.</param>v
        /// <param name="offset">Source offset.</param>
        /// <param name="text">Value to check for.</param>
        /// <returns>True if found.</returns>
        public static bool magic(this Span<byte> source, ReadOnlySpan<byte> text, int offset = 0) =>
            HasMagic(source, text, offset);

        /// <summary>
        /// Checks for identifier.
        /// </summary>
        /// <param name="source">Source to read.</param>v
        /// <param name="offset">Source offset.</param>
        /// <param name="text">Value to check for.</param>
        /// <returns>True if found.</returns>
        public static bool magic(this Span<byte> source, string text, int offset = 0) =>
            HasMagic(source, text, offset);

        /// <summary>
        /// Checks for identifier.
        /// </summary>
        /// <param name="source">Source to read.</param>v
        /// <param name="offset">Source offset.</param>
        /// <param name="text">Value to check for.</param>
        /// <returns>True if found.</returns>
        public static bool magic(this ReadOnlySpan<byte> source, ReadOnlySpan<byte> text, int offset = 0) =>
            HasMagic(source, text, offset);

        /// <summary>
        /// Checks for identifier.
        /// </summary>
        /// <param name="source">Source to read.</param>v
        /// <param name="offset">Source offset.</param>
        /// <param name="text">Value to check for.</param>
        /// <returns>True if found.</returns>
        public static bool magic(this ReadOnlySpan<byte> source, string text, int offset = 0) =>
            HasMagic(source, text, offset);

        /// <summary>
        /// Checks for identifier.
        /// </summary>
        /// <param name="text">Value to check for.</param>
        /// <returns>True if found.</returns>
        public static bool magic(ReadOnlySpan<byte> text) =>
            Current.HasMagic(text);

        /// <summary>
        /// Checks for identifier.
        /// </summary>
        /// <param name="text">Value to check for.</param>
        /// <returns>True if found.</returns>
        public static bool magic(string text) =>
            Current.HasMagic(text);

        #endregion

        #region Data result

        /// <summary>
        /// Creates byte data object.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="memory">Data.</param>
        /// <returns>Data object.</returns>
        public static Data data(this FpPath path, ReadOnlyMemory<byte> memory) => Buffer(path, memory);

        /// <summary>
        /// Creates byte data object.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="memory">Data.</param>
        /// <returns>Data object.</returns>
        public static Data data(this string path, ReadOnlyMemory<byte> memory) => Buffer(path, memory);

        /// <summary>
        /// Creates PCM audio data object.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="info">PCM information.</param>
        /// <param name="memory">Audio buffer.</param>
        /// <returns>Data object.</returns>
        public static Data audio(this FpPath path, PcmInfo info, ReadOnlyMemory<byte> memory) =>
            Audio(path, info, memory);

        /// <summary>
        /// Creates PCM audio data object.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="info">PCM information.</param>
        /// <param name="memory">Audio buffer.</param>
        /// <returns>Data object.</returns>
        public static Data audio(this string path, PcmInfo info, ReadOnlyMemory<byte> memory) =>
            Audio(path, info, memory);

        /// <summary>
        /// Creates 32bpp RGBA image data object.
        /// </summary>
        /// <param name="path">Base path (without extension).</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="buffer">32bpp RGBA buffer.</param>
        /// <returns>Data object.</returns>
        public static Data image(this FpPath path, int width, int height, ReadOnlyMemory<uint> buffer) =>
            Image(path, width, height, buffer);

        /// <summary>
        /// Creates 32bpp RGBA image data object.
        /// </summary>
        /// <param name="path">Base path (without extension).</param>
        /// <param name="width">Image width.</param>
        /// <param name="height">Image height.</param>
        /// <param name="buffer">32bpp RGBA buffer.</param>
        /// <returns>Data object.</returns>
        public static Data image(this string path, int width, int height, ReadOnlyMemory<uint> buffer) =>
            Image(path, width, height, buffer);

        #endregion

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
        public static IEnumerable<long> match(Stream stream, long streamOffset, long streamMaxOffset,
            ReadOnlyMemory<byte> match, int matchOffset, int matchLength, int maxCount = int.MaxValue,
            int bufferLength = 4096) => Match(stream, streamOffset, streamMaxOffset, match, matchOffset, matchLength,
            maxCount, bufferLength);

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
        public static List<long> match(Stream stream, long streamOffset, long streamMaxOffset, ReadOnlySpan<byte> match,
            int maxCount = int.MaxValue, int bufferLength = 4096) =>
            Match(stream, streamOffset, streamMaxOffset, match, maxCount, bufferLength);


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
        public static IEnumerable<long> match(Stream stream, long streamOffset, ReadOnlyMemory<byte> match,
            int matchOffset, int matchLength, int bufferLength = 4096)
            => Match(stream, streamOffset, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> match(Stream stream, ReadOnlyMemory<byte> match, int matchOffset,
            int matchLength, int bufferLength = 4096)
            => Match(stream, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> match(Stream stream, long streamOffset, long streamMaxOffset,
            ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => Match(stream, streamOffset, streamMaxOffset, match, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> match(Stream stream, long streamOffset, ReadOnlyMemory<byte> match,
            int bufferLength = 4096)
            => Match(stream, streamOffset, match, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> match(Stream stream, ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => Match(stream, match, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> match(Stream stream, long streamOffset, long streamMaxOffset, string match,
            int bufferLength = 4096) =>
            Match(stream, streamOffset, streamMaxOffset, match, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long>
            match(Stream stream, long streamOffset, string match, int bufferLength = 4096) =>
            Match(stream, streamOffset, streamOffset, match, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> match(Stream stream, string match, int bufferLength = 4096) =>
            Match(stream, 0, long.MaxValue, match, bufferLength);

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
        public static long matchFirst(Stream stream, long streamOffset, long streamMaxOffset,
            ReadOnlyMemory<byte> match, int matchOffset, int matchLength, int bufferLength = 4096)
            => MatchFirst(stream, streamOffset, streamMaxOffset, match, matchOffset, matchLength, bufferLength);

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
        public static long matchFirst(Stream stream, long streamOffset, ReadOnlyMemory<byte> match, int matchOffset,
            int matchLength, int bufferLength = 4096)
            => MatchFirst(stream, streamOffset, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long matchFirst(Stream stream, ReadOnlyMemory<byte> match, int matchOffset, int matchLength,
            int bufferLength = 4096)
            => MatchFirst(stream, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long matchFirst(Stream stream, long streamOffset, long streamMaxOffset,
            ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => MatchFirst(stream, streamOffset, streamMaxOffset, match, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long matchFirst(Stream stream, long streamOffset, ReadOnlyMemory<byte> match,
            int bufferLength = 4096)
            => MatchFirst(stream, streamOffset, match, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long matchFirst(Stream stream, ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => MatchFirst(stream, match, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long matchFirst(Stream stream, long streamOffset, long streamMaxOffset, string match,
            int bufferLength = 4096)
            => MatchFirst(stream, streamOffset, streamMaxOffset, match, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long matchFirst(Stream stream, long streamOffset, string match, int bufferLength = 4096)
            => MatchFirst(stream, streamOffset, match, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long matchFirst(Stream stream, string match, int bufferLength = 4096) =>
            MatchFirst(stream, match, bufferLength);

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
        public static long matchLast(Stream stream, long streamOffset, long streamMaxOffset, ReadOnlyMemory<byte> match,
            int matchOffset, int matchLength, int bufferLength = 4096)
            => MatchLast(stream, streamOffset, streamMaxOffset, match, matchOffset, matchLength, bufferLength);

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
        public static long matchLast(Stream stream, long streamOffset, ReadOnlyMemory<byte> match, int matchOffset,
            int matchLength, int bufferLength = 4096)
            => MatchLast(stream, streamOffset, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long matchLast(Stream stream, ReadOnlyMemory<byte> match, int matchOffset, int matchLength,
            int bufferLength = 4096)
            => MatchLast(stream, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long matchLast(Stream stream, long streamOffset, long streamMaxOffset, ReadOnlyMemory<byte> match,
            int bufferLength = 4096)
            => MatchLast(stream, streamOffset, streamMaxOffset, match, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long matchLast(Stream stream, long streamOffset, ReadOnlyMemory<byte> match,
            int bufferLength = 4096)
            => MatchLast(stream, streamOffset, match, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long matchLast(Stream stream, ReadOnlyMemory<byte> match, int bufferLength = 4096)
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
        public static long matchLast(Stream stream, long streamOffset, long streamMaxOffset, string match,
            int bufferLength = 4096) =>
            MatchLast(stream, streamOffset, streamMaxOffset, match, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long matchLast(Stream stream, long streamOffset, string match, int bufferLength = 4096) =>
            MatchLast(stream, streamOffset, match, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long matchLast(Stream stream, string match, int bufferLength = 4096) =>
            MatchLast(stream, match, bufferLength);

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
        public static IEnumerable<long> match(long streamOffset, long streamMaxOffset, ReadOnlyMemory<byte> match,
            int matchOffset, int matchLength, int bufferLength = 4096)
            => Current.Match(streamOffset, streamMaxOffset, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> match(long streamOffset, ReadOnlyMemory<byte> match, int matchOffset,
            int matchLength,
            int bufferLength = 4096)
            => Current.Match(streamOffset, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> match(ReadOnlyMemory<byte> match, int matchOffset, int matchLength,
            int bufferLength = 4096)
            => Current.Match(match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> match(long streamOffset, long streamMaxOffset, byte[] match,
            int bufferLength = 4096)
            => Current.Match(streamOffset, streamMaxOffset, match, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> match(long streamOffset, ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => Current.Match(streamOffset, match, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> match(ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => Current.Match(match, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> match(long streamOffset, long streamMaxOffset, string match,
            int bufferLength = 4096)
            => Current.Match(streamOffset, streamMaxOffset, match, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> match(long streamOffset, string match, int bufferLength = 4096)
            => Current.Match(streamOffset, match, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> match(string match, int bufferLength = 4096)
            => Current.Match(match, bufferLength);

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
        public static long matchFirst(long streamOffset, long streamMaxOffset, ReadOnlyMemory<byte> match,
            int matchOffset,
            int matchLength, int bufferLength = 4096)
            => Current.MatchFirst(streamOffset, streamMaxOffset, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long matchFirst(long streamOffset, ReadOnlyMemory<byte> match, int matchOffset, int matchLength,
            int bufferLength = 4096)
            => Current.MatchFirst(streamOffset, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long matchFirst(ReadOnlyMemory<byte> match, int matchOffset, int matchLength,
            int bufferLength = 4096)
            => Current.MatchFirst(match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long matchFirst(long streamOffset, long streamMaxOffset, ReadOnlyMemory<byte> match,
            int bufferLength = 4096)
            => Current.MatchFirst(streamOffset, streamMaxOffset, match, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long matchFirst(long streamOffset, ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => Current.MatchFirst(streamOffset, match, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long matchFirst(ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => Current.MatchFirst(match, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long matchFirst(long streamOffset, long streamMaxOffset, string match,
            int bufferLength = 4096)
            => Current.MatchFirst(streamOffset, streamMaxOffset, match, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long matchFirst(long streamOffset, string match, int bufferLength = 4096)
            => Current.MatchFirst(streamOffset, match, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long matchFirst(string match, int bufferLength = 4096)
            => Current.MatchFirst(match, bufferLength);

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
        public static long matchLast(long streamOffset, long streamMaxOffset, ReadOnlyMemory<byte> match,
            int matchOffset,
            int matchLength, int bufferLength = 4096)
            => Current.MatchLast(streamOffset, streamMaxOffset, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long matchLast(long streamOffset, ReadOnlyMemory<byte> match, int matchOffset, int matchLength,
            int bufferLength = 4096)
            => Current.MatchLast(streamOffset, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long matchLast(ReadOnlyMemory<byte> match, int matchOffset, int matchLength,
            int bufferLength = 4096)
            => Current.MatchLast(match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long matchLast(long streamOffset, long streamMaxOffset, ReadOnlyMemory<byte> match,
            int bufferLength = 4096)
            => Current.MatchLast(streamOffset, streamMaxOffset, match, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long matchLast(long streamOffset, ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => Current.MatchLast(streamOffset, match, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long matchLast(ReadOnlyMemory<byte> match, int bufferLength = 4096)
            => Current.MatchLast(match, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long matchLast(long streamOffset, long streamMaxOffset, string match,
            int bufferLength = 4096)
            => Current.MatchLast(streamOffset, streamMaxOffset, match, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long matchLast(long streamOffset, string match, int bufferLength = 4096)
            => Current.MatchLast(streamOffset, match, bufferLength);

        /// <summary>
        /// Find last occurrence of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of last match or -1 if no match found</returns>
        public static long matchLast(string match, int bufferLength = 4096)
            => Current.MatchLast(match, bufferLength);

        #endregion
    }
    // ReSharper restore InconsistentNaming
}
