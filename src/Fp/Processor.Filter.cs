using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using static Fp.Processor;

namespace Fp
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class Processor
    {
        #region Filter utilities

        /// <summary>
        /// Check if a file exists in the same folder as current file
        /// </summary>
        /// <param name="sibling">Filename to check</param>
        /// <returns>True if file with provided name exists next to current file</returns>
        public bool HasSibling(string sibling) =>
            (FileSystem ?? throw new InvalidOperationException())
            .FileExists(Path.Combine(InputDirectory, sibling));

        /// <summary>
        /// Check if a file exists in the same folder as specified path
        /// </summary>
        /// <param name="path">Main path</param>
        /// <param name="sibling">Filename to check</param>
        /// <returns>True if file with provided name exists next to current file</returns>
        public bool PathHasSibling(string path, string sibling) =>
            (FileSystem ?? throw new InvalidOperationException())
            .FileExists(Path.Combine(Path.GetDirectoryName(path) ?? "/", sibling));

        /// <summary>
        /// Check if current file has any one of the given extensions
        /// </summary>
        /// <param name="extensions">File extensions to check</param>
        /// <returns>True if any extension matches</returns>
        public bool HasExtension(params string[] extensions) =>
            PathHasExtension(InputFile ?? throw new InvalidOperationException(), extensions);

        /// <summary>
        /// Check if a file has any one of the given extensions
        /// </summary>
        /// <param name="extensions">File extensions to check</param>
        /// <param name="file">File to check</param>
        /// <returns>True if any extension matches</returns>
        public static bool PathHasExtension(string file, params string?[] extensions) =>
            extensions.Any(extension => extension == null
                ? !file.Contains('.')
                : file.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase));

        /// <summary>
        /// Check if a span has a specific value at a certain offset
        /// </summary>
        /// <param name="source">Span to read</param>
        /// <param name="span">Value to check against</param>
        /// <param name="offset">Position in span to check</param>
        /// <returns>True if span region matches value</returns>
        public static bool HasMagic(ReadOnlySpan<byte> source, ReadOnlySpan<byte> span, int offset = 0) =>
            source.Length - offset >= span.Length && span.SequenceEqual(source.Slice(offset, span.Length));

        /// <summary>
        /// Check if a span has a specific value at a certain offset
        /// </summary>
        /// <param name="source">Span to read</param>
        /// <param name="array">Value to check against</param>
        /// <param name="offset">Position in span to check</param>
        /// <returns>True if span region matches value</returns>
        public static bool HasMagic(ReadOnlySpan<byte> source, byte[] array, int offset = 0)
            => HasMagic(source, array.AsSpan(), offset);

        /// <summary>
        /// Check if a span has a specific value at a certain offset
        /// </summary>
        /// <param name="source">Span to read</param>
        /// <param name="str">Value to check against</param>
        /// <param name="offset">Position in span to check</param>
        /// <returns>True if span region matches value</returns>
        public static bool HasMagic(ReadOnlySpan<byte> source, string str, int offset = 0)
            => HasMagic(source, Ascii(str), offset);

        /// <summary>
        /// Check if a stream has a specific value at a certain offset
        /// </summary>
        /// <param name="stream">Stream to read</param>
        /// <param name="span">Value to check against</param>
        /// <param name="offset">Position in stream to check</param>
        /// <returns>True if stream region matches value</returns>
        public bool HasMagic(Stream stream, ReadOnlySpan<byte> span, long offset = 0)
        {
            Span<byte> span2 = stackalloc byte[span.Length];
            int read = Read(stream, offset, span2);
            return read == span.Length && span.SequenceEqual(span2);
        }

        /// <summary>
        /// Check if a stream has a specific value at a certain offset
        /// </summary>
        /// <param name="stream">Stream to read</param>
        /// <param name="str">Value to check against</param>
        /// <param name="offset">Position in stream to check</param>
        /// <returns>True if stream region matches value</returns>
        public bool HasMagic(Stream stream, string str, long offset = 0)
            => HasMagic(stream, Ascii(str), offset);

        /// <summary>
        /// Check if current file's input stream has a specific value at a certain offset
        /// </summary>
        /// <param name="span">Value to check against</param>
        /// <param name="offset">Position in stream to check</param>
        /// <returns>True if stream region matches value</returns>
        public bool HasMagic(ReadOnlySpan<byte> span, long offset = 0)
            => HasMagic(_inputStream ?? throw new InvalidOperationException(), span, offset);

        /// <summary>
        /// Check if current file's input stream has a specific value at a certain offset
        /// </summary>
        /// <param name="array">Value to check against</param>
        /// <param name="offset">Position in stream to check</param>
        /// <returns>True if stream region matches value</returns>
        public bool HasMagic(byte[] array, long offset = 0)
            => HasMagic(_inputStream ?? throw new InvalidOperationException(), array.AsSpan(), offset);

        /// <summary>
        /// Check if current file's input stream has a specific value at a certain offset
        /// </summary>
        /// <param name="str">Value to check against</param>
        /// <param name="offset">Position in stream to check</param>
        /// <returns>True if stream region matches value</returns>
        public bool HasMagic(string str, long offset = 0)
            => HasMagic(_inputStream ?? throw new InvalidOperationException(), Ascii(str),
                offset);

        #endregion
    }


    // ReSharper disable InconsistentNaming
    public partial class Scripting
    {
        #region Filter

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
        /// <param name="offset">Source offset.</param>
        /// <returns>True if found.</returns>
        public static bool magic(ReadOnlySpan<byte> text, long offset = 0) =>
            Current.HasMagic(text, offset);

        /// <summary>
        /// Checks for identifier.
        /// </summary>
        /// <param name="text">Value to check for.</param>
        /// <param name="offset">Source offset.</param>
        /// <returns>True if found.</returns>
        public static bool magic(string text, long offset = 0) =>
            Current.HasMagic(text, offset);

        #endregion
    }
    // ReSharper restore InconsistentNaming
}
