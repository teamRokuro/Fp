﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Fp {
    /// <summary>
    /// Base type for file processors
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    [SuppressMessage("ReSharper", "NotAccessedField.Global")]
    [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
    public partial class Processor : IDisposable {
        #region Properties and fields

        /// <summary>
        /// True if current platform allows backslashes as separators (modifies path creation / operation behaviour)
        /// </summary>
        public static readonly bool PlatformSupportBackSlash =
            Path.DirectorySeparatorChar == '\\' || Path.AltDirectorySeparatorChar == '\\';

        /// <summary>
        /// Whether to allow backslash as separator
        /// </summary>
        public bool SupportBackSlash = false;

        internal int WorkerId;

        /// <summary>
        /// Log output target
        /// </summary>
        public Action<string> Logger;

        /// <summary>
        /// Whether to preload newly opened file input streams to <see cref="MemoryStream"/>
        /// </summary>
        public bool Preload;

        /// <summary>
        /// Input stream for current file if opened
        /// </summary>
        public Stream InputStream;

        /// <summary>
        /// Length of input stream for current file if opened
        /// </summary>
        public long InputLength;

        /// <summary>
        /// Root input directory
        /// </summary>
        public string InputRootDirectory;

        /// <summary>
        /// Current input directory
        /// </summary>
        public string InputDirectory;

        /// <summary>
        /// Current input file
        /// </summary>
        public string InputFile;

        /// <summary>
        /// Output stream for current file if opened
        /// </summary>
        public Stream OutputStream;

        /// <summary>
        /// Root output directory
        /// </summary>
        public string OutputRootDirectory;

        /// <summary>
        /// Current output directory
        /// </summary>
        public string OutputDirectory;

        /// <summary>
        /// Current output file index
        /// </summary>
        public int OutputCounter;

        /// <summary>
        /// Filesystem provider for this processor
        /// </summary>
        public FileSystemSource FileSystem;

        /// <summary>
        /// Whether to read input as little-endian
        /// </summary>
        public bool LittleEndian {
            get => _littleEndian;
            set {
                _littleEndian = value;
                _swap = BitConverter.IsLittleEndian ^ _littleEndian;
            }
        }

        /// <summary>
        /// Set by subclasses to indicate if no more processors
        /// should be run on the current input file (not applicable
        /// in multithreaded environment)
        /// </summary>
        public bool Lock;

        private bool _littleEndian;

        private bool _swap;

        private bool _overrideProcess = true;
        private bool _overrideProcessSegmented = true;

        #endregion

        #region Main operation functions

        /// <summary>
        /// Process current file
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        protected virtual void ProcessImpl(IReadOnlyList<string> args) {
            _overrideProcess = false;
        }

        /// <summary>
        /// Process current file in parts
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        /// <returns>Generated outputs</returns>
        protected virtual IEnumerable<(string path, byte[] buffer, int offset, int length)> ProcessSegmentedImpl(
            IReadOnlyList<string> args) {
            _overrideProcessSegmented = false;
            yield break;
        }

        /// <summary>
        /// Process current file
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        /// <returns>Generated outputs</returns>
        public void Process(IReadOnlyList<string> args) {
            ProcessImpl(args);
            if (_overrideProcess) return;
            foreach (var (path, buffer, offset, length) in ProcessSegmentedImpl(args)) {
                using var stream = FileSystem.OpenWrite(GenPath(default, path));
                stream.Write(buffer, offset, length);
            }
        }

        /// <summary>
        /// Process current file in parts
        /// </summary>
        /// <param name="args">Command-line arguments</param>
        /// <returns>Generated outputs</returns>
        public IEnumerable<(string path, byte[] buffer, int offset, int length)> ProcessSegmented(
            IReadOnlyList<string> args) {
            foreach (var entry in ProcessSegmentedImpl(args))
                yield return entry;
            if (_overrideProcessSegmented)
                yield break;
            var prevFs = FileSystem;
            var fs = new FileSystemSource.SegmentedFileSystemSource(prevFs);
            FileSystem = fs;
            try {
                ProcessImpl(args);
                foreach (var entry in fs)
                    yield return entry;
            }
            finally {
                FileSystem = prevFs;
            }
        }

        #endregion

        #region Filter utilities

        /// <summary>
        /// Check if a file exists in the same folder as current file
        /// </summary>
        /// <param name="sibling">Filename to check</param>
        /// <returns>True if file with provided name exists next to current file</returns>
        public bool HasSibling(string sibling) => FileSystem.FileExists(Path.Combine(InputDirectory, sibling));

        /// <summary>
        /// Check if a file has the given extension
        /// </summary>
        /// <param name="extension">File extension to check</param>
        /// <param name="file">File to check, uses current file if null</param>
        /// <returns>True if extension matches</returns>
        public bool HasExtension(string extension, string file = null) => (file ?? InputFile).ToLowerInvariant()
            .EndsWith(extension.ToLowerInvariant(), StringComparison.Ordinal);

        /// <summary>
        /// Check if a stream has a specific value at a certain offset
        /// </summary>
        /// <param name="stream">Stream to read</param>
        /// <param name="span">Value to check against</param>
        /// <param name="offset">Position in stream to check</param>
        /// <returns>True if stream region matches value</returns>
        public bool HasMagic(Stream stream, Span<byte> span, long offset = 0) {
            Span<byte> span2 = stackalloc byte[span.Length];
            var position = stream.Position;
            Read(stream, offset, span2, false);
            stream.Position = position;
            return span.SequenceEqual(span2);
        }

        /// <summary>
        /// Check if a stream has a specific value at a certain offset
        /// </summary>
        /// <param name="stream">Stream to read</param>
        /// <param name="array">Value to check against</param>
        /// <param name="offset">Position in stream to check</param>
        /// <returns>True if stream region matches value</returns>
        public bool HasMagic(Stream stream, byte[] array, long offset = 0)
            => HasMagic(stream, array.AsSpan(), offset);

        /// <summary>
        /// Check if a stream has a specific value at a certain offset
        /// </summary>
        /// <param name="stream">Stream to read</param>
        /// <param name="str">Value to check against</param>
        /// <param name="offset">Position in stream to check</param>
        /// <returns>True if stream region matches value</returns>
        public bool HasMagic(Stream stream, string str, long offset = 0)
            => HasMagic(stream, Encoding.ASCII.GetBytes(str).AsSpan(), offset);

        /// <summary>
        /// Check if current file's input stream has a specific value at a certain offset
        /// </summary>
        /// <param name="span">Value to check against</param>
        /// <param name="offset">Position in stream to check</param>
        /// <returns>True if stream region matches value</returns>
        public bool HasMagic(Span<byte> span, long offset = 0)
            => HasMagic(InputStream, span, offset);

        /// <summary>
        /// Check if current file's input stream has a specific value at a certain offset
        /// </summary>
        /// <param name="array">Value to check against</param>
        /// <param name="offset">Position in stream to check</param>
        /// <returns>True if stream region matches value</returns>
        public bool HasMagic(byte[] array, long offset = 0)
            => HasMagic(InputStream, array.AsSpan(), offset);

        /// <summary>
        /// Check if current file's input stream has a specific value at a certain offset
        /// </summary>
        /// <param name="str">Value to check against</param>
        /// <param name="offset">Position in stream to check</param>
        /// <returns>True if stream region matches value</returns>
        public bool HasMagic(string str, long offset = 0)
            => HasMagic(InputStream, Encoding.ASCII.GetBytes(str).AsSpan(), offset);

        #endregion

        #region Stream read utilities

        /// <summary>
        /// Skip data in stream
        /// </summary>
        /// <param name="bytes">Number of bytes to skip</param>
        /// <param name="stream">Stream to operate on</param>
        /// <returns>New position in stream</returns>
        public static long Skip(long bytes, Stream stream)
            => stream.Seek(bytes, SeekOrigin.Current);

        /// <summary>
        /// Skip data in current file's input stream
        /// </summary>
        /// <param name="bytes">Number of bytes to skip</param>
        /// <returns>New position in stream</returns>
        public long Skip(long bytes)
            => InputStream.Seek(bytes, SeekOrigin.Current);

        private static int ReadBaseArray(Stream stream, byte[] array, int offset, int length, bool lenient) {
            int left = length, read, tot = 0;
            do {
                read = stream.Read(array, offset + tot, left);
                left -= read;
                tot += read;
            } while (left > 0 && read != 0);

            if (left > 0 && read == 0 && !lenient)
                throw new ProcessorException(
                    $"Failed to read required number of bytes! 0x{read:X} read, 0x{left:X} left, 0x{stream.Position:X} end position");
            return tot;
        }

        private static int ReadBaseSpan(Stream stream, Span<byte> span, bool lenient) {
            var buf = ArrayPool<byte>.Shared.Rent(4096);
            var bufSpan = buf.AsSpan();
            var bufLen = buf.Length;
            try {
                int left = span.Length, read, tot = 0;
                do {
                    read = stream.Read(buf, 0, Math.Min(left, bufLen));
                    bufSpan.Slice(0, read).CopyTo(span.Slice(tot));
                    left -= read;
                    tot += read;
                } while (left > 0);

                if (left > 0 && read == 0 && !lenient)
                    throw new ProcessorException(
                        $"Failed to read required number of bytes! 0x{read:X} read, 0x{left:X} left, 0x{stream.Position:X} end position");
                return tot;
            }
            finally {
                ArrayPool<byte>.Shared.Return(buf);
            }
        }

        /// <summary>
        /// Read data from stream, optionally replacing reference to provided span to prevent copy when reading from <see cref="MemoryStream"/>
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="span">Target to copy to</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <param name="forceNew">Force use provided span</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public static int Read(Stream stream, ref Span<byte> span, bool lenient = true, bool forceNew = false) {
            if (forceNew || !(stream is MemoryStream ms) || !ms.TryGetBuffer(out var buf))
                return ReadBaseSpan(stream, span, lenient);
            try {
                return (span = buf.AsSpan((int)ms.Position, span.Length)).Length;
            }
            catch (ArgumentOutOfRangeException exception) {
                throw new ProcessorException("Failed to convert span from memory stream", exception);
            }
        }

        /// <summary>
        /// Read data from stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="length">Number of bytes to try to read</param>
        /// <param name="span">Target to copy to</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <param name="forceNew">Force use newly allocated buffer</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public static int Read(Stream stream, int length, out Span<byte> span, bool lenient = true,
            bool forceNew = false) {
            if (!forceNew && stream is MemoryStream ms && ms.TryGetBuffer(out var buf)) {
                try {
                    return (span = buf.AsSpan((int)stream.Position, length)).Length;
                }
                catch (ArgumentOutOfRangeException exception) {
                    throw new ProcessorException("Failed to convert span from memory stream", exception);
                }
            }

            span = new Span<byte>(new byte[length]);
            return ReadBaseSpan(stream, span, lenient);
        }

        /// <summary>
        /// Read data from stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="span">Target to copy to</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public static int Read(Stream stream, Span<byte> span, bool lenient = true) {
            if (!(stream is MemoryStream ms) || !ms.TryGetBuffer(out var buf))
                return ReadBaseSpan(stream, span, lenient);
            try {
                buf.AsSpan((int)ms.Position, span.Length).CopyTo(span);
                return span.Length;
            }
            catch (ArgumentOutOfRangeException exception) {
                throw new ProcessorException("Failed to convert span from memory stream", exception);
            }
        }

        /// <summary>
        /// Read data from current file's input stream, optionally replacing reference to provided span to prevent copy when reading from <see cref="MemoryStream"/>
        /// </summary>
        /// <param name="span">Target to copy to</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <param name="forceNew">Force use provided span</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public int Read(ref Span<byte> span, bool lenient = true, bool forceNew = false)
            => Read(InputStream, ref span, lenient, forceNew);

        /// <summary>
        /// Read data from current file's input stream
        /// </summary>
        /// <param name="length">Number of bytes to try to read</param>
        /// <param name="span">Target to copy to</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <param name="forceNew">Force use newly allocated buffer</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public int Read(int length, out Span<byte> span, bool lenient = true, bool forceNew = false)
            => Read(InputStream, length, out span, lenient, forceNew);

        /// <summary>
        /// Read data from current file's input stream
        /// </summary>
        /// <param name="span">Target to copy to</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public int Read(Span<byte> span, bool lenient = true)
            => Read(InputStream, span, lenient);

        /// <summary>
        /// Read data from stream at the specified offset
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Target to copy to</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public static int Read(Stream stream, long offset, Span<byte> span, bool lenient = true) {
            var position = stream.Position;
            stream.Position = offset;
            var count = Read(stream, span, lenient);
            stream.Position = position;
            return count;
        }

        /// <summary>
        /// Read data from current file's input stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Target to copy to</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public int Read(long offset, Span<byte> span, bool lenient = true) {
            var position = InputStream.Position;
            InputStream.Position = offset;
            var count = Read(InputStream, span, lenient);
            InputStream.Position = position;
            return count;
        }

        /// <summary>
        /// Read data from stream at the specified offset
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Target to copy to</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <param name="forceNew">Force use provided span</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public static int Read(Stream stream, long offset, ref Span<byte> span, bool lenient = true,
            bool forceNew = false) {
            var position = stream.Position;
            stream.Position = offset;
            var count = Read(stream, ref span, lenient, forceNew);
            stream.Position = position;
            return count;
        }

        /// <summary>
        /// Read data from current file's input stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Target to copy to</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <param name="forceNew">Force use provided span</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public int Read(long offset, ref Span<byte> span, bool lenient = true, bool forceNew = false) {
            var position = InputStream.Position;
            InputStream.Position = offset;
            var count = Read(InputStream, ref span, lenient, forceNew);
            InputStream.Position = position;
            return count;
        }

        /// <summary>
        /// Read data from stream at the specified offset
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="offset">Offset to read from</param>
        /// <param name="length">Number of bytes to try to read</param>
        /// <param name="span">Target to copy to</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <param name="forceNew">Force use newly allocated buffer</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public static int Read(Stream stream, long offset, int length, out Span<byte> span, bool lenient = true,
            bool forceNew = false) {
            var position = stream.Position;
            stream.Position = offset;
            var count = Read(stream, length, out span, lenient, forceNew);
            stream.Position = position;
            return count;
        }

        /// <summary>
        /// Read data from current file's input stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="length">Number of bytes to try to read</param>
        /// <param name="span">Target to copy to</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <param name="forceNew">Force use newly allocated buffer</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public int Read(long offset, int length, out Span<byte> span, bool lenient = true, bool forceNew = false) {
            var position = InputStream.Position;
            InputStream.Position = offset;
            var count = Read(InputStream, length, out span, lenient, forceNew);
            InputStream.Position = position;
            return count;
        }

        /// <summary>
        /// Read data from stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="array">Target to copy to</param>
        /// <param name="arrayOffset">Offset in array to write to</param>
        /// <param name="arrayLength">Length to write</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public static int Read(Stream stream, byte[] array, int arrayOffset, int arrayLength, bool lenient = true) =>
            ReadBaseArray(stream, array, arrayOffset, arrayLength, lenient);

        /// <summary>
        /// Read data from current file's input stream
        /// </summary>
        /// <param name="array">Target to copy to</param>
        /// <param name="arrayOffset">Offset in array to write to</param>
        /// <param name="arrayLength">Length to write</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public int Read(byte[] array, int arrayOffset, int arrayLength, bool lenient = true)
            => Read(InputStream, array, arrayOffset, arrayLength, lenient);

        /// <summary>
        /// Read data from stream at the specified offset
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="offset">Offset to read from</param>
        /// <param name="array">Target to copy to</param>
        /// <param name="arrayOffset">Offset in array to write to</param>
        /// <param name="arrayLength">Length to write</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public static int Read(Stream stream, long offset, byte[] array, int arrayOffset, int arrayLength,
            bool lenient = true) {
            var position = stream.Position;
            stream.Position = offset;
            var count = Read(stream, array, arrayOffset, arrayLength, lenient);
            stream.Position = position;
            return count;
        }

        /// <summary>
        /// Read data from current file's input stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="array">Target to copy to</param>
        /// <param name="arrayOffset">Offset in array to write to</param>
        /// <param name="arrayLength">Length to write</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public int Read(long offset, byte[] array, int arrayOffset, int arrayLength, bool lenient = true) {
            var position = InputStream.Position;
            InputStream.Position = offset;
            var count = Read(InputStream, array, arrayOffset, arrayLength, lenient);
            InputStream.Position = position;
            return count;
        }

        /// <summary>
        /// Read data from stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="array">Target to copy to</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public int Read(Stream stream, byte[] array, bool lenient = true)
            => Read(stream, array, 0, array.Length, lenient);

        /// <summary>
        /// Read data from current file's input stream
        /// </summary>
        /// <param name="array">Target to copy to</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public int Read(byte[] array, bool lenient = true)
            => Read(InputStream, array, 0, array.Length, lenient);

        /// <summary>
        /// Read data from stream at the specified offset
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="offset">Offset to read from</param>
        /// <param name="array">Target to copy to</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public int Read(Stream stream, long offset, byte[] array, bool lenient = true)
            => Read(stream, offset, array, 0, array.Length, lenient);

        /// <summary>
        /// Read data from current file's input stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="array">Target to copy to</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public int Read(long offset, byte[] array, bool lenient = true)
            => Read(offset, array, 0, array.Length, lenient);

        /// <summary>
        /// Get byte array from input starting at current position
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="forceNew">Force use newly allocated buffer</param>
        /// <returns>Array with file contents</returns>
        public byte[] GetArray(Stream stream = null, bool forceNew = false) {
            stream ??= InputStream;
            if (!forceNew && stream is MemoryStream ms)
                return ms.Capacity == ms.Length && ms.TryGetBuffer(out _) ? ms.GetBuffer() : ms.ToArray();
            var arr = new byte[stream.Length - stream.Position];
            Read(stream, arr, false);
            return arr;
        }

        #endregion

        #region Decoding utilities

        /// <summary>
        /// Read signed 16-bit value from stream
        /// </summary>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public short ReadS16(Stream stream = null) {
            stream ??= InputStream;
            if (stream is MemoryStream) {
                Read(stream, 2, out var span2, false);
                return ReadS16(span2);
            }

            Span<byte> span = stackalloc byte[2];
            Read(stream, span, false);
            return ReadS16NoCopy(span);
        }

        /// <summary>
        /// Read signed 16-bit value from stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public short ReadS16(long offset, Stream stream = null) {
            stream ??= InputStream;
            if (stream is MemoryStream) {
                Read(stream, offset, 2, out var span2, false);
                return ReadS16(span2);
            }

            Span<byte> span = stackalloc byte[2];
            Read(stream, offset, span, false);
            return ReadS16NoCopy(span);
        }

        /// <summary>
        /// Read signed 16-bit value from span at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public short ReadS16(Span<byte> span, int offset = 0) {
            if (!_swap)
                return MemoryMarshal.Cast<byte, short>(span.Slice(offset, 2))[0];
            Span<byte> span2 = stackalloc byte[2];
            span.Slice(offset, 2).CopyTo(span2);
            span2.Reverse();
            return MemoryMarshal.Cast<byte, short>(span2)[0];
        }

        /// <summary>
        /// Read signed 16-bit value from span at the specified offset, reversing span in-place if necessary
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public short ReadS16NoCopy(Span<byte> span, int offset = 0) {
            var sub = span.Slice(offset, 2);
            if (_swap)
                sub.Reverse();
            return MemoryMarshal.Cast<byte, short>(sub)[0];
        }

        /// <summary>
        /// Read signed 16-bit value from array at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="array">Array to read from</param>
        /// <returns>Value</returns>
        public short ReadS16(byte[] array, int offset = 0)
            => ReadS16(array.AsSpan(), offset);

        /// <summary>
        /// Read unsigned 16-bit value from stream
        /// </summary>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public ushort ReadU16(Stream stream = null) {
            stream ??= InputStream;
            if (stream is MemoryStream) {
                Read(stream, 2, out var span2, false);
                return ReadU16(span2);
            }

            Span<byte> span = stackalloc byte[2];
            Read(stream, span, false);
            return ReadU16NoCopy(span);
        }

        /// <summary>
        /// Read unsigned 16-bit value from stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public ushort ReadU16(long offset, Stream stream = null) {
            stream ??= InputStream;
            if (stream is MemoryStream) {
                Read(stream, offset, 2, out var span2, false);
                return ReadU16(span2);
            }

            Span<byte> span = stackalloc byte[2];
            Read(stream, offset, span, false);
            return ReadU16NoCopy(span);
        }

        /// <summary>
        /// Read unsigned 16-bit value from span at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public ushort ReadU16(Span<byte> span, int offset = 0) {
            if (!_swap)
                return MemoryMarshal.Cast<byte, ushort>(span.Slice(offset, 2))[0];
            Span<byte> span2 = stackalloc byte[2];
            span.Slice(offset, 2).CopyTo(span2);
            span2.Reverse();
            return MemoryMarshal.Cast<byte, ushort>(span2)[0];
        }

        /// <summary>
        /// Read unsigned 16-bit value from span at the specified offset, reversing span in-place if necessary
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public ushort ReadU16NoCopy(Span<byte> span, int offset = 0) {
            var sub = span.Slice(offset, 2);
            if (_swap)
                sub.Reverse();
            return MemoryMarshal.Cast<byte, ushort>(sub)[0];
        }

        /// <summary>
        /// Read unsigned 16-bit value from array at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="array">Array to read from</param>
        /// <returns>Value</returns>
        public ushort ReadU16(byte[] array, int offset = 0)
            => ReadU16(array.AsSpan(), offset);

        /// <summary>
        /// Read signed 32-bit value from stream
        /// </summary>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public int ReadS32(Stream stream = null) {
            stream ??= InputStream;
            if (stream is MemoryStream) {
                Read(stream, 4, out var span2, false);
                return ReadS32(span2);
            }

            Span<byte> span = stackalloc byte[4];
            Read(stream, span, false);
            return ReadS32NoCopy(span);
        }

        /// <summary>
        /// Read signed 32-bit value from stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public int ReadS32(long offset, Stream stream = null) {
            stream ??= InputStream;
            if (stream is MemoryStream) {
                Read(stream, offset, 4, out var span2, false);
                return ReadS32(span2);
            }

            Span<byte> span = stackalloc byte[4];
            Read(stream, offset, span, false);
            return ReadS32NoCopy(span);
        }

        /// <summary>
        /// Read signed 32-bit value from span at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public int ReadS32(Span<byte> span, int offset = 0) {
            if (!_swap)
                return MemoryMarshal.Cast<byte, int>(span.Slice(offset, 4))[0];
            Span<byte> span2 = stackalloc byte[4];
            span.Slice(offset, 4).CopyTo(span2);
            span2.Reverse();
            return MemoryMarshal.Cast<byte, int>(span2)[0];
        }

        /// <summary>
        /// Read signed 32-bit value from span at the specified offset, reversing span in-place if necessary
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public int ReadS32NoCopy(Span<byte> span, int offset = 0) {
            var sub = span.Slice(offset, 4);
            if (_swap)
                sub.Reverse();
            return MemoryMarshal.Cast<byte, int>(sub)[0];
        }

        /// <summary>
        /// Read signed 32-bit value from array at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="array">Array to read from</param>
        /// <returns>Value</returns>
        public int ReadS32(byte[] array, int offset = 0)
            => ReadS32(array.AsSpan(), offset);

        /// <summary>
        /// Read unsigned 32-bit value from stream
        /// </summary>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public uint ReadU32(Stream stream = null) {
            stream ??= InputStream;
            if (stream is MemoryStream) {
                Read(stream, 4, out var span2, false);
                return ReadU32(span2);
            }

            Span<byte> span = stackalloc byte[4];
            Read(stream, span, false);
            return ReadU32NoCopy(span);
        }

        /// <summary>
        /// Read unsigned 32-bit value from stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public uint ReadU32(long offset, Stream stream = null) {
            stream ??= InputStream;
            if (stream is MemoryStream) {
                Read(stream, offset, 4, out var span2, false);
                return ReadU32(span2);
            }

            Span<byte> span = stackalloc byte[4];
            Read(stream, offset, span, false);
            return ReadU32NoCopy(span);
        }

        /// <summary>
        /// Read unsigned 32-bit value from span at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public uint ReadU32(Span<byte> span, int offset = 0) {
            if (!_swap)
                return MemoryMarshal.Cast<byte, uint>(span.Slice(offset, 4))[0];
            Span<byte> span2 = stackalloc byte[4];
            span.Slice(offset, 4).CopyTo(span2);
            span2.Reverse();
            return MemoryMarshal.Cast<byte, uint>(span2)[0];
        }

        /// <summary>
        /// Read unsigned 32-bit value from span at the specified offset, reversing span in-place if necessary
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public uint ReadU32NoCopy(Span<byte> span, int offset = 0) {
            var sub = span.Slice(offset, 4);
            if (_swap)
                sub.Reverse();
            return MemoryMarshal.Cast<byte, uint>(sub)[0];
        }

        /// <summary>
        /// Read unsigned 32-bit value from array at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="array">Array to read from</param>
        /// <returns>Value</returns>
        public uint ReadU32(byte[] array, int offset = 0)
            => (uint)ReadS32(array.AsSpan(), offset);

        /// <summary>
        /// Read signed 64-bit value from stream
        /// </summary>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public long ReadS64(Stream stream = null) {
            stream ??= InputStream;
            if (stream is MemoryStream) {
                Read(stream, 8, out var span2, false);
                return ReadS64(span2);
            }

            Span<byte> span = stackalloc byte[8];
            Read(stream, span, false);
            return ReadS64NoCopy(span);
        }

        /// <summary>
        /// Read signed 64-bit value from stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public long ReadS64(long offset, Stream stream = null) {
            stream ??= InputStream;
            if (stream is MemoryStream) {
                Read(stream, offset, 8, out var span2, false);
                return ReadS64(span2);
            }

            Span<byte> span = stackalloc byte[8];
            Read(stream, offset, span, false);
            return ReadS64NoCopy(span);
        }

        /// <summary>
        /// Read signed 64-bit value from span at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public long ReadS64(Span<byte> span, int offset = 0) {
            if (!_swap)
                return MemoryMarshal.Cast<byte, long>(span.Slice(offset, 8))[0];
            Span<byte> span2 = stackalloc byte[8];
            span.Slice(offset, 8).CopyTo(span2);
            span2.Reverse();
            return MemoryMarshal.Cast<byte, long>(span2)[0];
        }

        /// <summary>
        /// Read signed 64-bit value from span at the specified offset, reversing span in-place if necessary
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public long ReadS64NoCopy(Span<byte> span, int offset = 0) {
            var sub = span.Slice(offset, 8);
            if (_swap)
                sub.Reverse();
            return MemoryMarshal.Cast<byte, long>(sub)[0];
        }

        /// <summary>
        /// Read signed 64-bit value from array at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="array">Array to read from</param>
        /// <returns>Value</returns>
        public long ReadS64(byte[] array, int offset = 0)
            => ReadS64(array.AsSpan(), offset);

        /// <summary>
        /// Read unsigned 64-bit value from stream
        /// </summary>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public ulong ReadU64(Stream stream = null) {
            stream ??= InputStream;
            if (stream is MemoryStream) {
                Read(stream, 8, out var span2, false);
                return ReadU64(span2);
            }

            Span<byte> span = stackalloc byte[8];
            Read(stream, span, false);
            return ReadU64NoCopy(span);
        }

        /// <summary>
        /// Read unsigned 64-bit value from stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public ulong ReadU64(long offset, Stream stream = null) {
            stream ??= InputStream;
            if (stream is MemoryStream) {
                Read(stream, offset, 8, out var span2, false);
                return ReadU64(span2);
            }

            Span<byte> span = stackalloc byte[8];
            Read(stream, offset, span, false);
            return ReadU64NoCopy(span);
        }

        /// <summary>
        /// Read unsigned 64-bit value from span at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public ulong ReadU64(Span<byte> span, int offset = 0) {
            if (!_swap)
                return MemoryMarshal.Cast<byte, ulong>(span.Slice(offset, 8))[0];
            Span<byte> span2 = stackalloc byte[8];
            span.Slice(offset, 8).CopyTo(span2);
            span2.Reverse();
            return MemoryMarshal.Cast<byte, ulong>(span2)[0];
        }

        /// <summary>
        /// Read unsigned 64-bit value from span at the specified offset, reversing span in-place if necessary
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public ulong ReadU64NoCopy(Span<byte> span, int offset = 0) {
            var sub = span.Slice(offset, 8);
            if (_swap)
                sub.Reverse();
            return MemoryMarshal.Cast<byte, ulong>(sub)[0];
        }

        /// <summary>
        /// Read unsigned 64-bit value from array at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="array">Array to read from</param>
        /// <returns>Value</returns>
        public ulong ReadU64(byte[] array, int offset = 0)
            => ReadU64(array.AsSpan(), offset);

        /// <summary>
        /// Read 16-bit float value from stream
        /// </summary>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public float ReadHalf(Stream stream = null) {
            stream ??= InputStream;
            if (stream is MemoryStream) {
                Read(stream, 2, out var span2, false);
                return ReadSingle(span2);
            }

            Span<byte> span = stackalloc byte[2];
            Read(stream, span, false);
            return ReadSingle(span);
        }

        /// <summary>
        /// Read 16-bit float value from stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public float ReadHalf(long offset, Stream stream = null) {
            stream ??= InputStream;
            if (stream is MemoryStream) {
                Read(stream, offset, 2, out var span2, false);
                return ReadSingle(span2);
            }

            Span<byte> span = stackalloc byte[2];
            Read(stream, offset, span, false);
            return ReadSingle(span);
        }

        /// <summary>
        /// Read 16-bit float value from span at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public static float ReadHalf(Span<byte> span, int offset = 0)
            => HalfHelper.HalfToSingle(MemoryMarshal.Read<ushort>(span.Slice(offset, 2)));

        /// <summary>
        /// Read 16-bit float value from array at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="array">Array to read from</param>
        /// <returns>Value</returns>
        public float ReadHalf(byte[] array, int offset = 0)
            => ReadHalf(array.AsSpan(), offset);

        /// <summary>
        /// Read 32-bit float value from stream
        /// </summary>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public float ReadSingle(Stream stream = null) {
            stream ??= InputStream;
            if (stream is MemoryStream) {
                Read(stream, 4, out var span2, false);
                return ReadSingle(span2);
            }

            Span<byte> span = stackalloc byte[4];
            Read(stream, span, false);
            return ReadSingle(span);
        }

        /// <summary>
        /// Read 32-bit float value from stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public float ReadSingle(long offset, Stream stream = null) {
            stream ??= InputStream;
            if (stream is MemoryStream) {
                Read(stream, offset, 4, out var span2, false);
                return ReadSingle(span2);
            }

            Span<byte> span = stackalloc byte[4];
            Read(stream, offset, span, false);
            return ReadSingle(span);
        }

        /// <summary>
        /// Read 32-bit float value from span at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public static float ReadSingle(Span<byte> span, int offset = 0)
            => MemoryMarshal.Read<float>(span.Slice(offset, 4));

        /// <summary>
        /// Read 32-bit float value from array at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="array">Array to read from</param>
        /// <returns>Value</returns>
        public float ReadSingle(byte[] array, int offset = 0)
            => ReadSingle(array.AsSpan(), offset);

        /// <summary>
        /// Read 64-bit float value from stream
        /// </summary>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public double ReadDouble(Stream stream = null) {
            stream ??= InputStream;
            Span<byte> span = stackalloc byte[8];
            Read(stream, span, false);
            return ReadDouble(span);
        }

        /// <summary>
        /// Read 64-bit float value from stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Value</returns>
        public double ReadDouble(long offset, Stream stream = null) {
            stream ??= InputStream;
            if (stream is MemoryStream) {
                Read(stream, offset, 8, out var span2, false);
                return ReadDouble(span2);
            }

            Span<byte> span = stackalloc byte[8];
            Read(stream, offset, span, false);
            return ReadDouble(span);
        }

        /// <summary>
        /// Read 64-bit float value from span at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="span">Span to read from</param>
        /// <returns>Value</returns>
        public static double ReadDouble(Span<byte> span, int offset = 0)
            => MemoryMarshal.Read<float>(span.Slice(offset, 8));

        /// <summary>
        /// Read 64-bit float value from array at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="array">Array to read from</param>
        /// <returns>Value</returns>
        public double ReadDouble(byte[] array, int offset = 0)
            => ReadDouble(array.AsSpan(), offset);

        /// <summary>
        /// Read array of signed 16-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS16Array(Span<short> span, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, MemoryMarshal.Cast<short, byte>(span), false);
            ConvertS16Array(span);
        }

        /// <summary>
        /// Read array of signed 16-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS16Array(Span<short> span, long offset, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, offset, MemoryMarshal.Cast<short, byte>(span), false);
            ConvertS16Array(span);
        }

        /// <summary>
        /// Convert endianness of signed 16-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertS16Array(Span<short> span) {
            if (!_swap) return;
            var span2 = MemoryMarshal.Cast<short, byte>(span);
            for (var i = 0; i < span2.Length; i += 2) span2.Slice(i, 2).Reverse();
        }

        /// <summary>
        /// Read array of signed 16-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS16Array(Span<byte> span, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, span, false);
            ConvertS16Array(span);
        }

        /// <summary>
        /// Read array of signed 16-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS16Array(Span<byte> span, long offset, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, offset, span, false);
            ConvertS16Array(span);
        }

        /// <summary>
        /// Convert endianness of signed 16-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertS16Array(Span<byte> span) {
            if (!_swap) return;
            for (var i = 0; i < span.Length; i += 2) span.Slice(i, 2).Reverse();
        }

        /// <summary>
        /// Read array of signed 16-bit values from stream
        /// </summary>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public short[] ReadS16Array(int count, Stream stream = null) {
            var arr = new short[count];
            var span = MemoryMarshal.Cast<short, byte>(arr);
            ReadS16Array(span, stream);
            return arr;
        }

        /// <summary>
        /// Read array of signed 16-bit values from stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public short[] ReadS16Array(long offset, int count, Stream stream = null) {
            var arr = new short[count];
            var span = MemoryMarshal.Cast<short, byte>(arr);
            ReadS16Array(span, offset, stream);
            return arr;
        }

        /// <summary>
        /// Read array of unsigned 16-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU16Array(Span<ushort> span, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, MemoryMarshal.Cast<ushort, byte>(span), false);
            ConvertU16Array(span);
        }

        /// <summary>
        /// Read array of unsigned 16-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU16Array(Span<ushort> span, long offset, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, offset, MemoryMarshal.Cast<ushort, byte>(span), false);
            ConvertU16Array(span);
        }

        /// <summary>
        /// Convert endianness of unsigned 16-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertU16Array(Span<ushort> span) {
            if (!_swap) return;
            var span2 = MemoryMarshal.Cast<ushort, byte>(span);
            for (var i = 0; i < span2.Length; i += 2) span2.Slice(i, 2).Reverse();
        }

        /// <summary>
        /// Read array of unsigned 16-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU16Array(Span<byte> span, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, span, false);
            ConvertU16Array(span);
        }

        /// <summary>
        /// Read array of unsigned 16-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU16Array(Span<byte> span, long offset, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, offset, span, false);
            ConvertU16Array(span);
        }

        /// <summary>
        /// Convert endianness of unsigned 16-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertU16Array(Span<byte> span) {
            if (!_swap) return;
            for (var i = 0; i < span.Length; i += 2) span.Slice(i, 2).Reverse();
        }

        /// <summary>
        /// Read array of unsigned 16-bit values from stream
        /// </summary>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public ushort[] ReadU16Array(int count, Stream stream = null) {
            var arr = new ushort[count];
            var span = MemoryMarshal.Cast<ushort, byte>(arr);
            ReadU16Array(span, stream);
            return arr;
        }

        /// <summary>
        /// Read array of unsigned 16-bit values from stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public ushort[] ReadU16Array(long offset, int count, Stream stream = null) {
            var arr = new ushort[count];
            var span = MemoryMarshal.Cast<ushort, byte>(arr);
            ReadU16Array(span, offset, stream);
            return arr;
        }

        /// <summary>
        /// Read array of signed 32-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS32Array(Span<int> span, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, MemoryMarshal.Cast<int, byte>(span), false);
            ConvertS32Array(span);
        }

        /// <summary>
        /// Read array of signed 32-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS32Array(Span<int> span, long offset, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, offset, MemoryMarshal.Cast<int, byte>(span), false);
            ConvertS32Array(span);
        }

        /// <summary>
        /// Convert endianness of signed 32-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertS32Array(Span<int> span) {
            if (!_swap) return;
            var span2 = MemoryMarshal.Cast<int, byte>(span);
            for (var i = 0; i < span2.Length; i += 4) span2.Slice(i, 4).Reverse();
        }

        /// <summary>
        /// Read array of signed 32-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS32Array(Span<byte> span, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, span, false);
            ConvertS32Array(span);
        }

        /// <summary>
        /// Read array of signed 32-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS32Array(Span<byte> span, long offset, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, offset, span, false);
            ConvertS32Array(span);
        }

        /// <summary>
        /// Convert endianness of signed 32-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertS32Array(Span<byte> span) {
            if (!_swap) return;
            for (var i = 0; i < span.Length; i += 4) span.Slice(i, 4).Reverse();
        }

        /// <summary>
        /// Read array of signed 32-bit values from stream
        /// </summary>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public int[] ReadS32Array(int count, Stream stream = null) {
            var arr = new int[count];
            var span = MemoryMarshal.Cast<int, byte>(arr);
            ReadS32Array(span, stream);
            return arr;
        }

        /// <summary>
        /// Read array of signed 32-bit values from stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public int[] ReadS32Array(long offset, int count, Stream stream = null) {
            var arr = new int[count];
            var span = MemoryMarshal.Cast<int, byte>(arr);
            ReadS32Array(span, offset, stream);
            return arr;
        }

        /// <summary>
        /// Read array of unsigned 32-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU32Array(Span<uint> span, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, MemoryMarshal.Cast<uint, byte>(span), false);
            ConvertU32Array(span);
        }

        /// <summary>
        /// Read array of unsigned 32-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU32Array(Span<uint> span, long offset, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, offset, MemoryMarshal.Cast<uint, byte>(span), false);
            ConvertU32Array(span);
        }

        /// <summary>
        /// Convert endianness of unsigned 32-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertU32Array(Span<uint> span) {
            if (!_swap) return;
            var span2 = MemoryMarshal.Cast<uint, byte>(span);
            for (var i = 0; i < span2.Length; i += 4) span2.Slice(i, 4).Reverse();
        }

        /// <summary>
        /// Read array of unsigned 32-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU32Array(Span<byte> span, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, span, false);
            ConvertU32Array(span);
        }

        /// <summary>
        /// Read array of unsigned 32-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU32Array(Span<byte> span, long offset, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, offset, span, false);
            ConvertU32Array(span);
        }

        /// <summary>
        /// Convert endianness of unsigned 32-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertU32Array(Span<byte> span) {
            if (!_swap) return;
            for (var i = 0; i < span.Length; i += 4) span.Slice(i, 4).Reverse();
        }

        /// <summary>
        /// Read array of unsigned 32-bit values from stream
        /// </summary>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public uint[] ReadU32Array(int count, Stream stream = null) {
            var arr = new uint[count];
            var span = MemoryMarshal.Cast<uint, byte>(arr);
            ReadU32Array(span, stream);
            return arr;
        }

        /// <summary>
        /// Read array of unsigned 32-bit values from stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public uint[] ReadU32Array(long offset, int count, Stream stream = null) {
            var arr = new uint[count];
            var span = MemoryMarshal.Cast<uint, byte>(arr);
            ReadU32Array(span, offset, stream);
            return arr;
        }

        /// <summary>
        /// Read array of signed 64-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS64Array(Span<long> span, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, MemoryMarshal.Cast<long, byte>(span), false);
            ConvertS64Array(span);
        }

        /// <summary>
        /// Read array of signed 64-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS64Array(Span<long> span, long offset, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, offset, MemoryMarshal.Cast<long, byte>(span), false);
            ConvertS64Array(span);
        }

        /// <summary>
        /// Convert endianness of signed 64-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertS64Array(Span<long> span) {
            if (!_swap) return;
            var span2 = MemoryMarshal.Cast<long, byte>(span);
            for (var i = 0; i < span2.Length; i += 8) span2.Slice(i, 8).Reverse();
        }

        /// <summary>
        /// Read array of signed 64-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS64Array(Span<byte> span, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, span, false);
            ConvertS64Array(span);
        }

        /// <summary>
        /// Read array of signed 64-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadS64Array(Span<byte> span, long offset, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, offset, span, false);
            ConvertS64Array(span);
        }

        /// <summary>
        /// Convert endianness of signed 64-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertS64Array(Span<byte> span) {
            if (!_swap) return;
            for (var i = 0; i < span.Length; i += 8) span.Slice(i, 8).Reverse();
        }

        /// <summary>
        /// Read array of signed 64-bit values from stream
        /// </summary>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public long[] ReadS64Array(int count, Stream stream = null) {
            var arr = new long[count];
            var span = MemoryMarshal.Cast<long, byte>(arr);
            ReadS64Array(span, stream);
            return arr;
        }

        /// <summary>
        /// Read array of signed 64-bit values from stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public long[] ReadS64Array(long offset, int count, Stream stream = null) {
            var arr = new long[count];
            var span = MemoryMarshal.Cast<long, byte>(arr);
            ReadS64Array(span, offset, stream);
            return arr;
        }

        /// <summary>
        /// Read array of unsigned 64-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU64Array(Span<ulong> span, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, MemoryMarshal.Cast<ulong, byte>(span), false);
            ConvertU64Array(span);
        }

        /// <summary>
        /// Read array of unsigned 64-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU64Array(Span<ulong> span, long offset, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, offset, MemoryMarshal.Cast<ulong, byte>(span), false);
            ConvertU64Array(span);
        }

        /// <summary>
        /// Convert endianness of unsigned 64-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertU64Array(Span<ulong> span) {
            if (!_swap) return;
            var span2 = MemoryMarshal.Cast<ulong, byte>(span);
            for (var i = 0; i < span2.Length; i += 8) span2.Slice(i, 8).Reverse();
        }

        /// <summary>
        /// Read array of unsigned 64-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU64Array(Span<byte> span, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, span, false);
            ConvertU64Array(span);
        }

        /// <summary>
        /// Read array of unsigned 64-bit values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadU64Array(Span<byte> span, long offset, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, offset, span, false);
            ConvertU64Array(span);
        }

        /// <summary>
        /// Convert endianness of unsigned 64-bit array between source and platform's endianness
        /// </summary>
        /// <param name="span">Span to convert</param>
        public void ConvertU64Array(Span<byte> span) {
            if (!_swap) return;
            for (var i = 0; i < span.Length; i += 8) span.Slice(i, 8).Reverse();
        }

        /// <summary>
        /// Read array of unsigned 64-bit values from stream
        /// </summary>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public ulong[] ReadU64Array(int count, Stream stream = null) {
            var arr = new ulong[count];
            var span = MemoryMarshal.Cast<ulong, byte>(arr);
            ReadS16Array(span, stream);
            return arr;
        }

        /// <summary>
        /// Read array of unsigned 64-bit values from stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public ulong[] ReadU64Array(long offset, int count, Stream stream = null) {
            var arr = new ulong[count];
            var span = MemoryMarshal.Cast<ulong, byte>(arr);
            ReadS16Array(span, offset, stream);
            return arr;
        }

        /// <summary>
        /// Convert array of half-precision floating-point values to single-precision
        /// </summary>
        /// <param name="source">Source span</param>
        /// <param name="target">Target span</param>
        public static void ReadHalfArray(Span<byte> source, Span<float> target) {
            var span = MemoryMarshal.Cast<byte, ushort>(source);
            for (var i = 0; i < span.Length; i++)
                target[i] = HalfHelper.HalfToSingle(span[i]);
        }

        /// <summary>
        /// Convert array of half-precision floating-point values to single-precision
        /// </summary>
        /// <param name="source">Source span</param>
        /// <param name="target">Target span</param>
        public static void WriteHalfArray(Span<float> source, Span<byte> target) {
            var span = MemoryMarshal.Cast<byte, ushort>(target);
            for (var i = 0; i < source.Length; i++)
                span[i] = HalfHelper.SingleToHalf(source[i]);
        }

        /// <summary>
        /// Read array of half-precision floating-point values as single-precision from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadHalfArray(Span<float> span, Stream stream = null) {
            stream ??= InputStream;
            var arr = ArrayPool<byte>.Shared.Rent(span.Length * 2);
            try {
                var span2 = arr.AsSpan(0, span.Length * 2);
                Read(stream, span2, false);
                ReadHalfArray(span2, span);
            }
            finally {
                ArrayPool<byte>.Shared.Return(arr);
            }
        }

        /// <summary>
        /// Read array of single-precision floating-point values as single-precision from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadHalfArray(Span<float> span, long offset, Stream stream = null) {
            stream ??= InputStream;
            var arr = ArrayPool<byte>.Shared.Rent(span.Length * 2);
            try {
                var span2 = arr.AsSpan(0, span.Length * 2);
                Read(stream, offset, span2, false);
                ReadHalfArray(span2, span);
            }
            finally {
                ArrayPool<byte>.Shared.Return(arr);
            }
        }

        /// <summary>
        /// Read array of single-precision floating-point values as single-precision from stream
        /// </summary>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public float[] ReadHalfArray(int count, Stream stream = null) {
            var arr = new float[count];
            ReadHalfArray(arr, stream);
            return arr;
        }

        /// <summary>
        /// Read array of single-precision floating-point values as single-precision from stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public float[] ReadHalfArray(long offset, int count, Stream stream = null) {
            var arr = new float[count];
            ReadHalfArray(arr, offset, stream);
            return arr;
        }

        /// <summary>
        /// Read array of single-precision floating-point values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadSingleArray(Span<float> span, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, MemoryMarshal.Cast<float, byte>(span), false);
        }

        /// <summary>
        /// Read array of single-precision floating-point values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadSingleArray(Span<float> span, long offset, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, offset, MemoryMarshal.Cast<float, byte>(span), false);
        }

        /// <summary>
        /// Read array of single-precision floating-point values from stream
        /// </summary>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public float[] ReadSingleArray(int count, Stream stream = null) {
            var arr = new float[count];
            ReadSingleArray(arr, stream);
            return arr;
        }

        /// <summary>
        /// Read array of single-precision floating-point values from stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public float[] ReadSingleArray(long offset, int count, Stream stream = null) {
            var arr = new float[count];
            ReadSingleArray(arr, offset, stream);
            return arr;
        }

        /// <summary>
        /// Read array of double-precision floating-point values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadDoubleArray(Span<double> span, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, MemoryMarshal.Cast<double, byte>(span), false);
        }

        /// <summary>
        /// Read array of double-precision floating-point values from stream
        /// </summary>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        public void ReadDoubleArray(Span<double> span, long offset, Stream stream = null) {
            stream ??= InputStream;
            Read(stream, offset, MemoryMarshal.Cast<double, byte>(span), false);
        }

        /// <summary>
        /// Read array of double-precision floating-point values from stream
        /// </summary>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public double[] ReadDoubleArray(int count, Stream stream = null) {
            var arr = new double[count];
            ReadDoubleArray(arr, stream);
            return arr;
        }

        /// <summary>
        /// Read array of double-precision floating-point values from stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="count">Number of elements to read</param>
        /// <param name="stream">Stream to read from, uses current file if null</param>
        /// <returns>Newly allocated array</returns>
        public double[] ReadDoubleArray(long offset, int count, Stream stream = null) {
            var arr = new double[count];
            ReadDoubleArray(arr, offset, stream);
            return arr;
        }

        private static unsafe string DecodeSpan(ReadOnlySpan<byte> span, Encoding encoding) {
            if (span.Length == 0)
                return string.Empty;
            fixed (byte* spanFixed = &span.GetPinnableReference())
                return encoding.GetString(spanFixed, span.Length);
        }

        /// <summary>
        /// Read UTF-8 encoded string from stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <param name="strict">If true, enforces ending stream position to original position + <paramref name="maxLength"/></param>
        /// <returns>Value</returns>
        public static string ReadUtf8String(Stream stream, int maxLength = int.MaxValue, bool strict = false) {
            using var ms = new MemoryStream();
            var c = 0;
            do {
                var v = stream.ReadByte();
                if (v == -1 || v == 0)
                    break;
                ms.WriteByte((byte)v);
                c++;
            } while (c < maxLength);

            var str = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);

            if (strict)
                Skip(maxLength - c, stream);
            return str;
        }

        /// <summary>
        /// Read UTF-8 encoded string from stream at the specified offset
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="offset">Offset to read from</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <param name="strict">If true, enforces ending stream position to original position + <paramref name="maxLength"/></param>
        /// <returns>Value</returns>
        public static string ReadUtf8StringFromOffset(Stream stream, long offset, int maxLength = int.MaxValue,
            bool strict = false) {
            var position = stream.Position;
            stream.Position = offset;
            var str = ReadUtf8String(stream, maxLength, strict);
            stream.Position = position;
            return str;
        }

        /// <summary>
        /// Read UTF-8 encoded string from current file's input stream
        /// </summary>
        /// <param name="maxLength">Maximum string length</param>
        /// <param name="strict">If true, enforces ending stream position to original position + <paramref name="maxLength"/></param>
        /// <returns>Value</returns>
        public string ReadUtf8String(int maxLength = int.MaxValue, bool strict = false)
            => ReadUtf8String(InputStream, maxLength, strict);

        /// <summary>
        /// Read UTF-8 encoded string from current file's input stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <param name="strict">If true, enforces ending stream position to original position + <paramref name="maxLength"/></param>
        /// <returns>Value</returns>
        public string ReadUtf8StringFromOffset(long offset, int maxLength = int.MaxValue, bool strict = false) {
            return ReadUtf8StringFromOffset(InputStream, offset, maxLength, strict);
        }

        /// <summary>
        /// Read UTF-8 encoded string from span
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <returns>Value</returns>
        public static string ReadUtf8String(Span<byte> span, int maxLength = int.MaxValue) {
            var lim = Math.Min(span.Length, maxLength);
            var end = span.Slice(0, lim).IndexOf((byte)0);
            if (end == -1)
                end = lim;
            return DecodeSpan(span.Slice(0, end), Encoding.UTF8);
        }

        /// <summary>
        /// Read UTF-8 encoded string from array
        /// </summary>
        /// <param name="array">Array to read from</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <returns>Value</returns>
        // ReSharper disable once MemberCanBeProtected.Global
        public static string ReadUtf8String(byte[] array, int maxLength = int.MaxValue)
            => ReadUtf8String(array.AsSpan(), maxLength);

        /// <summary>
        /// Read UTF-8 encoded string from array
        /// </summary>
        /// <param name="array">Array to read from</param>
        /// <param name="offset">Offset to read from</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <returns>Value</returns>
        // ReSharper disable once MemberCanBeProtected.Global
        public static string ReadUtf8StringFromOffset(byte[] array, int offset = 0, int maxLength = int.MaxValue)
            => ReadUtf8String(array.AsSpan(offset), maxLength);

        /// <summary>
        /// Read UTF-16 encoded string from stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <param name="strict">If true, enforces ending stream position to original position + <paramref name="maxLength"/></param>
        /// <returns>Value</returns>
        public static string ReadUtf16String(Stream stream, int maxLength = int.MaxValue, bool strict = false) {
            var sb = new StringBuilder();
            var c = 0;
            do {
                var v = stream.ReadByte();
                if (v == -1)
                    break;
                c++;
                var w = stream.ReadByte();
                if (w == -1)
                    break;
                c++;
                if (v == 0 && w == 0)
                    break;
                sb.Append((char)(v + (w << 8)));
            } while (c < maxLength);

            if (strict)
                Skip(maxLength - c, stream);
            return sb.ToString();
        }

        /// <summary>
        /// Read UTF-16 encoded string from stream at the specified offset
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="offset">Offset to read from</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <param name="strict">If true, enforces ending stream position to original position + <paramref name="maxLength"/></param>
        /// <returns>Value</returns>
        public static string ReadUtf16StringFromOffset(Stream stream, long offset, int maxLength = int.MaxValue,
            bool strict = false) {
            var position = stream.Position;
            stream.Position = offset;
            var str = ReadUtf16String(stream, maxLength, strict);
            stream.Position = position;
            return str;
        }

        /// <summary>
        /// Read UTF-16 encoded string from current file's input stream
        /// </summary>
        /// <param name="maxLength">Maximum string length</param>
        /// <param name="strict">If true, enforces ending stream position to original position + <paramref name="maxLength"/></param>
        /// <returns>Value</returns>
        public string ReadUtf16String(int maxLength = int.MaxValue, bool strict = false)
            => ReadUtf16String(InputStream, maxLength, strict);

        /// <summary>
        /// Read UTF-16 encoded string from current file's input stream at the specified offset
        /// </summary>
        /// <param name="offset">Offset to read from</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <param name="strict">If true, enforces ending stream position to original position + <paramref name="maxLength"/></param>
        /// <returns>Value</returns>
        public string ReadUtf16StringFromOffset(long offset, int maxLength = int.MaxValue, bool strict = false) {
            return ReadUtf16StringFromOffset(InputStream, offset, maxLength, strict);
        }


        /// <summary>
        /// Read UTF-16 encoded string from span
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <returns>Value</returns>
        public static string ReadUtf16String(Span<byte> span, int maxLength = int.MaxValue) {
            var lim = Math.Min(span.Length, maxLength);
            var end = MemoryMarshal.Cast<byte, char>(span.Slice(0, lim)).IndexOf('\0');
            if (end == -1)
                end = lim;
            else
                end *= sizeof(char);
            return DecodeSpan(span.Slice(0, end), Encoding.Unicode);
        }

        /// <summary>
        /// Read UTF-16 encoded string from array
        /// </summary>
        /// <param name="array">Array to read from</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <returns>Value</returns>
        // ReSharper disable once MemberCanBeProtected.Global
        public static string ReadUtf16String(byte[] array, int maxLength = int.MaxValue)
            => ReadUtf16String(array.AsSpan(), maxLength);

        /// <summary>
        /// Read UTF-16 encoded string from array
        /// </summary>
        /// <param name="array">Array to read from</param>
        /// <param name="offset">Offset to read from</param>
        /// <param name="maxLength">Maximum string length</param>
        /// <returns>Value</returns>
        // ReSharper disable once MemberCanBeProtected.Global
        public static string ReadUtf16StringFromOffset(byte[] array, int offset = 0, int maxLength = int.MaxValue)
            => ReadUtf16String(array.AsSpan(offset), maxLength);

        #endregion

        #region Encoding utilities

        /// <summary>
        /// Write signed 16-bit value to span at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset to write to</param>
        public void GetBytes(short value, Span<byte> span, int offset = 0) {
            var sub = span.Slice(offset, 2);
            MemoryMarshal.Cast<byte, short>(sub)[0] = value;
            if (_swap)
                sub.Reverse();
        }

        /// <summary>
        /// Write unsigned 16-bit value to span at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset to write to</param>
        public void GetBytes(ushort value, Span<byte> span, int offset = 0) {
            var sub = span.Slice(offset, 2);
            MemoryMarshal.Cast<byte, ushort>(sub)[0] = value;
            if (_swap)
                sub.Reverse();
        }

        /// <summary>
        /// Write signed 16-bit value to array at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Resultant array (newly allocated if none provided)</returns>
        public byte[] GetBytes(short value, byte[] array = null, int offset = 0) {
            array ??= new byte[2];
            GetBytes(value, array.AsSpan(offset, 2));
            return array;
        }

        /// <summary>
        /// Write unsigned 16-bit value to array at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Resultant array (newly allocated if none provided)</returns>
        public byte[] GetBytes(ushort value, byte[] array = null, int offset = 0) {
            array ??= new byte[2];
            GetBytes((short)value, array.AsSpan(offset, 2));
            return array;
        }

        /// <summary>
        /// Write signed 32-bit value to span at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset to write to</param>
        public void GetBytes(int value, Span<byte> span, int offset = 0) {
            var sub = span.Slice(offset, 4);
            MemoryMarshal.Cast<byte, int>(sub)[0] = value;
            if (_swap)
                sub.Reverse();
        }

        /// <summary>
        /// Write unsigned 32-bit value to span at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset to write to</param>
        public void GetBytes(uint value, Span<byte> span, int offset = 0) {
            var sub = span.Slice(offset, 4);
            MemoryMarshal.Cast<byte, uint>(sub)[0] = value;
            if (_swap)
                sub.Reverse();
        }

        /// <summary>
        /// Write signed 32-bit value to array at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Resultant array (newly allocated if none provided)</returns>
        public byte[] GetBytes(int value, byte[] array = null, int offset = 0) {
            array ??= new byte[4];
            GetBytes(value, array.AsSpan(offset, 4));
            return array;
        }

        /// <summary>
        /// Write unsigned 32-bit value to array at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Resultant array (newly allocated if none provided)</returns>
        public byte[] GetBytes(uint value, byte[] array = null, int offset = 0) {
            array ??= new byte[4];
            GetBytes((int)value, array.AsSpan(offset, 4));
            return array;
        }

        /// <summary>
        /// Write signed 64-bit value to span at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset to write to</param>
        public void GetBytes(long value, Span<byte> span, int offset = 0) {
            var sub = span.Slice(offset, 2);
            MemoryMarshal.Cast<byte, long>(sub)[0] = value;
            if (_swap)
                sub.Reverse();
        }

        /// <summary>
        /// Write unsigned 64-bit value to span at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset to write to</param>
        public void GetBytes(ulong value, Span<byte> span, int offset = 0) {
            var sub = span.Slice(offset, 2);
            MemoryMarshal.Cast<byte, ulong>(sub)[0] = value;
            if (_swap)
                sub.Reverse();
        }

        /// <summary>
        /// Write signed 64-bit value to array at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Resultant array (newly allocated if none provided)</returns>
        public byte[] GetBytes(long value, byte[] array = null, int offset = 0) {
            array ??= new byte[8];
            GetBytes(value, array.AsSpan(offset, 8));
            return array;
        }

        /// <summary>
        /// Write unsigned 64-bit value to array at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Resultant array (newly allocated if none provided)</returns>
        public byte[] GetBytes(ulong value, byte[] array = null, int offset = 0) {
            array ??= new byte[8];
            GetBytes((long)value, array.AsSpan(offset, 8));
            return array;
        }

        /// <summary>
        /// Write 32-bit float value to span at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset to write to</param>
        public static void GetBytes(float value, Span<byte> span, int offset = 0) =>
            MemoryMarshal.Cast<byte, float>(span.Slice(offset, 4))[0] = value;

        /// <summary>
        /// Write 32-bit float value to array at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Resultant array (newly allocated if none provided)</returns>
        public byte[] GetBytes(float value, byte[] array = null, int offset = 0) {
            array ??= new byte[4];
            GetBytes(value, array.AsSpan(offset, 4));
            return array;
        }

        /// <summary>
        /// Write 64-bit float value to span at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="span">Span to write to</param>
        /// <param name="offset">Offset to write to</param>
        public static void GetBytes(double value, Span<byte> span, int offset = 0) =>
            MemoryMarshal.Cast<byte, double>(span.Slice(offset, 8))[0] = value;

        /// <summary>
        /// Write 64-bit float value to array at specified offset
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="array">Array to write to</param>
        /// <param name="offset">Offset to write to</param>
        /// <returns>Resultant array (newly allocated if none provided)</returns>
        public byte[] GetBytes(double value, byte[] array = null, int offset = 0) {
            array ??= new byte[8];
            GetBytes(value, array.AsSpan(offset, 8));
            return array;
        }

        #endregion

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
        public static IEnumerable<long> Match(Stream stream, long streamOffset, long streamMaxOffset, byte[] match,
            int matchOffset, int matchLength, int maxCount = int.MaxValue, int bufferLength = 4096) {
            if (maxCount < 1)
                throw new ArgumentException($"{nameof(maxCount)} has value {maxCount} but must be at least 1");
            var count = 0;
            var initPos = stream.Position;
            byte[][] buffers = null;
            try {
                buffers = new byte[2][];
                var realPositions = new long[2];
                for (var i = 0; i < buffers.Length; i++)
                    buffers[i] = ArrayPool<byte>.Shared.Rent(Math.Max(matchLength, bufferLength));
                var basePos = streamOffset;
                realPositions[0] = basePos;
                var curPos = basePos;
                var currentReadBufferIndex = 0;
                var currentReadBufferOffset = 0;
                var latest = 0;

                // Loop while offset allows checking for another match
                while (basePos + matchLength < streamMaxOffset) {
                    int read;
                    var latestFilled = 0;
                    stream.Position = streamOffset;
                    // Read to fill buffer
                    do
                        latestFilled += read = stream.Read(buffers[latest], latestFilled,
                            (int)Math.Min(streamMaxOffset - basePos, buffers[latest].Length) - latestFilled);
                    while (read != 0);
                    // Leave on failure to read (reached end)
                    if (latestFilled == 0)
                        break;
                    streamOffset += latestFilled;
                    // Loop while loaded buffers allow read
                    while (curPos + matchLength <= realPositions[latest] + latestFilled) {
                        var tempBufIndex = currentReadBufferIndex;
                        var tempBufOffset = currentReadBufferOffset;
                        var ok = true;
                        // Check for current offset
                        for (var i = 0; i < matchLength && ok; i++) {
                            if (buffers[tempBufIndex][tempBufOffset] != match[matchOffset + i]) {
                                ok = false;
                            }
                            else {
                                // Update current read buffer
                                tempBufOffset++;
                                if (tempBufOffset < buffers[tempBufIndex].Length) continue;
                                tempBufIndex = (tempBufIndex + 1) % 2;
                                tempBufOffset = 0;
                            }
                        }

                        if (ok) {
                            yield return curPos;
                            count++;
                            if (count == maxCount)
                                yield break;
                            curPos += matchLength;
                            currentReadBufferOffset += matchLength;
                        }
                        else {
                            curPos++;
                            currentReadBufferOffset++;
                        }

                        // Update current read buffer
                        if (currentReadBufferOffset < buffers[currentReadBufferIndex].Length) continue;
                        var over = currentReadBufferOffset - buffers[currentReadBufferIndex].Length;
                        // Safe to increment buffer index by 1 because buffer length is at least the length of match subarray
                        currentReadBufferIndex = (currentReadBufferIndex + 1) % 2;
                        currentReadBufferOffset = over;
                    }

                    basePos += latestFilled;
                    // Check if current buffer was fully populated (prepare for next)
                    if (latestFilled != buffers[latest].Length) continue;
                    latest = (latest + 1) % 2;
                    realPositions[latest] = basePos;
                }
            }
            finally {
                if (buffers != null)
                    foreach (var t in buffers)
                        if (t != null)
                            ArrayPool<byte>.Shared.Return(t);

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
        public static List<long> Match(Stream stream, long streamOffset, long streamMaxOffset, Span<byte> match,
            int maxCount = int.MaxValue, int bufferLength = 4096) {
            if (maxCount < 1)
                throw new ArgumentException($"{nameof(maxCount)} has value {maxCount} but must be at least 1");
            var count = 0;
            var res = new List<long>();
            var initPos = stream.Position;
            var matchLength = match.Length;
            byte[][] buffers = null;
            try {
                buffers = new byte[2][];
                var realPositions = new long[2];
                for (var i = 0; i < buffers.Length; i++)
                    buffers[i] = ArrayPool<byte>.Shared.Rent(Math.Max(matchLength, bufferLength));
                var basePos = streamOffset;
                realPositions[0] = basePos;
                var curPos = basePos;
                var currentReadBufferIndex = 0;
                var currentReadBufferOffset = 0;
                var latest = 0;

                // Loop while offset allows checking for another match
                while (basePos + matchLength < streamMaxOffset) {
                    int read;
                    var latestFilled = 0;
                    stream.Position = streamOffset;
                    // Read to fill buffer
                    do
                        latestFilled += read = stream.Read(buffers[latest], latestFilled,
                            (int)Math.Min(streamMaxOffset - basePos, buffers[latest].Length) - latestFilled);
                    while (read != 0);
                    // Leave on failure to read (reached end)
                    if (latestFilled == 0)
                        break;
                    streamOffset += latestFilled;
                    // Loop while loaded buffers allow read
                    while (curPos + matchLength <= realPositions[latest] + latestFilled) {
                        var tempBufIndex = currentReadBufferIndex;
                        var tempBufOffset = currentReadBufferOffset;
                        var ok = true;
                        // Check for current offset
                        for (var i = 0; i < matchLength && ok; i++) {
                            if (buffers[tempBufIndex][tempBufOffset] != match[i]) {
                                ok = false;
                            }
                            else {
                                // Update current read buffer
                                tempBufOffset++;
                                if (tempBufOffset < buffers[tempBufIndex].Length) continue;
                                tempBufIndex = (tempBufIndex + 1) % 2;
                                tempBufOffset = 0;
                            }
                        }

                        if (ok) {
                            res.Add(curPos);
                            count++;
                            if (count == maxCount)
                                return res;
                            curPos += matchLength;
                            currentReadBufferOffset += matchLength;
                        }
                        else {
                            curPos++;
                            currentReadBufferOffset++;
                        }

                        // Update current read buffer
                        if (currentReadBufferOffset < buffers[currentReadBufferIndex].Length) continue;
                        var over = currentReadBufferOffset - buffers[currentReadBufferIndex].Length;
                        // Safe to increment buffer index by 1 because buffer length is at least the length of match subarray
                        currentReadBufferIndex = (currentReadBufferIndex + 1) % 2;
                        currentReadBufferOffset = over;
                    }

                    basePos += latestFilled;
                    // Check if current buffer was fully populated (prepare for next)
                    if (latestFilled != buffers[latest].Length) continue;
                    latest = (latest + 1) % 2;
                    realPositions[latest] = basePos;
                }

                return res;
            }
            finally {
                if (buffers != null)
                    foreach (var t in buffers)
                        if (t != null)
                            ArrayPool<byte>.Shared.Return(t);

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
        public static IEnumerable<long> Match(Stream stream, long streamOffset, byte[] match, int matchOffset,
            int matchLength,
            int bufferLength = 4096)
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
        public static IEnumerable<long> Match(Stream stream, byte[] match, int matchOffset, int matchLength,
            int bufferLength = 4096)
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
        public static IEnumerable<long> Match(Stream stream, long streamOffset, long streamMaxOffset, byte[] match,
            int bufferLength = 4096)
            => Match(stream, streamOffset, streamMaxOffset, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> Match(Stream stream, long streamOffset, byte[] match, int bufferLength = 4096)
            => Match(stream, streamOffset, long.MaxValue, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> Match(Stream stream, byte[] match, int bufferLength = 4096)
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
            int bufferLength = 4096) {
            var matchArr = Encoding.ASCII.GetBytes(match);
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
        public static IEnumerable<long> Match(Stream stream, long streamOffset, string match, int bufferLength = 4096) {
            var matchArr = Encoding.ASCII.GetBytes(match);
            return Match(stream, streamOffset, long.MaxValue, matchArr, 0, matchArr.Length, bufferLength);
        }

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public static IEnumerable<long> Match(Stream stream, string match, int bufferLength = 4096) {
            var matchArr = Encoding.ASCII.GetBytes(match);
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
        public static long MatchFirst(Stream stream, long streamOffset, long streamMaxOffset, byte[] match,
            int matchOffset,
            int matchLength, int bufferLength = 4096) {
            foreach (var v in Match(stream, streamOffset, streamMaxOffset, match, matchOffset, matchLength,
                bufferLength))
                return v;
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
        public static long MatchFirst(Stream stream, long streamOffset, byte[] match, int matchOffset, int matchLength,
            int bufferLength = 4096)
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
        public static long MatchFirst(Stream stream, byte[] match, int matchOffset, int matchLength,
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
        public static long MatchFirst(Stream stream, long streamOffset, long streamMaxOffset, byte[] match,
            int bufferLength = 4096)
            => MatchFirst(stream, streamOffset, streamMaxOffset, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long MatchFirst(Stream stream, long streamOffset, byte[] match, int bufferLength = 4096)
            => MatchFirst(stream, streamOffset, long.MaxValue, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long MatchFirst(Stream stream, byte[] match, int bufferLength = 4096)
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
            int bufferLength = 4096) {
            var matchArr = Encoding.ASCII.GetBytes(match);
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
        public static long MatchFirst(Stream stream, long streamOffset, string match, int bufferLength = 4096) {
            var matchArr = Encoding.ASCII.GetBytes(match);
            return MatchFirst(stream, streamOffset, long.MaxValue, matchArr, 0, matchArr.Length, bufferLength);
        }

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long MatchFirst(Stream stream, string match, int bufferLength = 4096) {
            var matchArr = Encoding.ASCII.GetBytes(match);
            return MatchFirst(stream, 0, long.MaxValue, matchArr, 0, matchArr.Length, bufferLength);
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
        public static long MatchLast(Stream stream, long streamOffset, long streamMaxOffset, byte[] match,
            int matchOffset,
            int matchLength, int bufferLength = 4096) {
            long u = -1;
            foreach (var v in Match(stream, streamOffset, streamMaxOffset, match, matchOffset, matchLength,
                bufferLength))
                u = v;
            return u;
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
        public static long MatchLast(Stream stream, long streamOffset, byte[] match, int matchOffset, int matchLength,
            int bufferLength = 4096)
            => MatchLast(stream, streamOffset, long.MaxValue, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long MatchLast(Stream stream, byte[] match, int matchOffset, int matchLength,
            int bufferLength = 4096)
            => MatchLast(stream, 0, long.MaxValue, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long MatchLast(Stream stream, long streamOffset, long streamMaxOffset, byte[] match,
            int bufferLength = 4096)
            => MatchLast(stream, streamOffset, streamMaxOffset, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long MatchLast(Stream stream, long streamOffset, byte[] match, int bufferLength = 4096)
            => MatchLast(stream, streamOffset, long.MaxValue, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long MatchLast(Stream stream, byte[] match, int bufferLength = 4096)
            => MatchLast(stream, 0, long.MaxValue, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long MatchLast(Stream stream, long streamOffset, long streamMaxOffset, string match,
            int bufferLength = 4096) {
            var matchArr = Encoding.ASCII.GetBytes(match);
            return MatchLast(stream, streamOffset, streamMaxOffset, matchArr, 0, matchArr.Length, bufferLength);
        }

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long MatchLast(Stream stream, long streamOffset, string match, int bufferLength = 4096) {
            var matchArr = Encoding.ASCII.GetBytes(match);
            return MatchLast(stream, streamOffset, long.MaxValue, matchArr, 0, matchArr.Length, bufferLength);
        }

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public static long MatchLast(Stream stream, string match, int bufferLength = 4096) {
            var matchArr = Encoding.ASCII.GetBytes(match);
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
        public IEnumerable<long> Match(long streamOffset, long streamMaxOffset, byte[] match,
            int matchOffset, int matchLength, int bufferLength = 4096)
            => Match(InputStream, streamOffset, streamMaxOffset, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public IEnumerable<long> Match(long streamOffset, byte[] match, int matchOffset, int matchLength,
            int bufferLength = 4096)
            => Match(InputStream, streamOffset, long.MaxValue, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public IEnumerable<long> Match(byte[] match, int matchOffset, int matchLength,
            int bufferLength = 4096)
            => Match(InputStream, 0, long.MaxValue, match, matchOffset, matchLength, bufferLength);

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
            => Match(InputStream, streamOffset, streamMaxOffset, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public IEnumerable<long> Match(long streamOffset, byte[] match, int bufferLength = 4096)
            => Match(InputStream, streamOffset, long.MaxValue, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public IEnumerable<long> Match(byte[] match, int bufferLength = 4096)
            => Match(InputStream, 0, long.MaxValue, match, 0, match.Length, bufferLength);

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
            => Match(InputStream, streamOffset, streamMaxOffset, match, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public IEnumerable<long> Match(long streamOffset, string match, int bufferLength = 4096)
            => Match(InputStream, streamOffset, long.MaxValue, match, bufferLength);

        /// <summary>
        /// Enumerate all occurrences of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Enumerator for matches</returns>
        public IEnumerable<long> Match(string match, int bufferLength = 4096)
            => Match(InputStream, 0, long.MaxValue, match, bufferLength);

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
        public long MatchFirst(long streamOffset, long streamMaxOffset, byte[] match, int matchOffset,
            int matchLength, int bufferLength = 4096) {
            foreach (var v in Match(InputStream, streamOffset, streamMaxOffset, match, matchOffset, matchLength,
                bufferLength))
                return v;
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
        public long MatchFirst(long streamOffset, byte[] match, int matchOffset, int matchLength,
            int bufferLength = 4096)
            => MatchFirst(InputStream, streamOffset, long.MaxValue, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchFirst(byte[] match, int matchOffset, int matchLength, int bufferLength = 4096)
            => MatchFirst(InputStream, 0, long.MaxValue, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchFirst(long streamOffset, long streamMaxOffset, byte[] match,
            int bufferLength = 4096)
            => MatchFirst(InputStream, streamOffset, streamMaxOffset, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchFirst(long streamOffset, byte[] match, int bufferLength = 4096)
            => MatchFirst(InputStream, streamOffset, long.MaxValue, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchFirst(byte[] match, int bufferLength = 4096)
            => MatchFirst(InputStream, 0, long.MaxValue, match, 0, match.Length, bufferLength);

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
            => MatchFirst(InputStream, streamOffset, streamMaxOffset, match, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchFirst(long streamOffset, string match, int bufferLength = 4096)
            => MatchFirst(InputStream, streamOffset, long.MaxValue, match, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchFirst(string match, int bufferLength = 4096)
            => MatchFirst(InputStream, 0, long.MaxValue, match, bufferLength);

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
        public long MatchLast(long streamOffset, long streamMaxOffset, byte[] match, int matchOffset,
            int matchLength, int bufferLength = 4096) {
            long u = -1;
            foreach (var v in Match(InputStream, streamOffset, streamMaxOffset, match, matchOffset, matchLength,
                bufferLength))
                u = v;
            return u;
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
        public long MatchLast(long streamOffset, byte[] match, int matchOffset, int matchLength,
            int bufferLength = 4096)
            => MatchLast(InputStream, streamOffset, long.MaxValue, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="matchOffset">Offset in target to start matching</param>
        /// <param name="matchLength">Length of target</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchLast(byte[] match, int matchOffset, int matchLength, int bufferLength = 4096)
            => MatchLast(InputStream, 0, long.MaxValue, match, matchOffset, matchLength, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchLast(long streamOffset, long streamMaxOffset, byte[] match,
            int bufferLength = 4096)
            => MatchLast(InputStream, streamOffset, streamMaxOffset, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchLast(long streamOffset, byte[] match, int bufferLength = 4096)
            => MatchLast(InputStream, streamOffset, long.MaxValue, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchLast(byte[] match, int bufferLength = 4096)
            => MatchLast(InputStream, 0, long.MaxValue, match, 0, match.Length, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="streamMaxOffset">Upper bound (exclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchLast(long streamOffset, long streamMaxOffset, string match,
            int bufferLength = 4096)
            => MatchLast(InputStream, streamOffset, streamMaxOffset, match, bufferLength);

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="streamOffset">Lower bound (inclusive) of stream positions to search</param>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchLast(long streamOffset, string match, int bufferLength = 4096) {
            var matchArr = Encoding.ASCII.GetBytes(match);
            return MatchLast(InputStream, streamOffset, long.MaxValue, matchArr, 0, matchArr.Length, bufferLength);
        }

        /// <summary>
        /// Find first occurrence of a pattern
        /// </summary>
        /// <param name="match">Target to match</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Position of first match or -1 if no match found</returns>
        public long MatchLast(string match, int bufferLength = 4096)
            => MatchLast(InputStream, 0, long.MaxValue, match, bufferLength);

        private static long WriteBaseStream(Stream stream, long length, Stream outputStream, bool lenient,
            int bufferLength) {
            long outLen = 0;
            var buffer = ArrayPool<byte>.Shared.Rent(bufferLength);
            try {
                var left = length;
                int read;
                do {
                    read = stream.Read(buffer, 0, (int)Math.Min(left, buffer.Length));
                    outputStream.Write(buffer, 0, read);
                    left -= read;
                    outLen += read;
                } while (left > 0 && read != 0);

                if (left > 0 && read != 0 && !lenient)
                    throw new ProcessorException(
                        $"Failed to read required number of bytes! 0x{read:X} read, 0x{left:X} left, 0x{stream.Position:X} end position");
            }
            finally {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            return outLen;
        }

        #endregion

        #region Decryption utilities

        /// <summary>
        /// Block cipher padding mode
        /// </summary>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public enum PaddingMode {
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

        private static int GetDepaddedLengthZero(Span<byte> span) {
            for (var i = span.Length; i > 0; i--)
                if (span[i - 1] != 0)
                    return i;
            return 0;
        }


        private static int GetDepaddedLengthIso_Iec_7816_4(Span<byte> span) {
            for (var i = span.Length - 1; i >= 0; i--)
                switch (span[i]) {
                    case 0x00:
                        break;
                    case 0x80:
                        return i;
                    default:
                        throw new ArgumentException(
                            $"Invalid padding byte for {nameof(PaddingMode.Iso_Iec_7816_4)}, need 0x80 or 0x00 but got 0x{span[i]:X2}");
                }

            throw new ArgumentException(
                $"Message is all null bytes and {nameof(PaddingMode.Iso_Iec_7816_4)} requires 0x80 to mark beginning of padding");
        }

        private static int GetDepaddedLengthLastByteSubtract(Span<byte> span) =>
            span.Length == 0 ? 0 : span.Length - span[span.Length - 1];

        /// <summary>
        /// Create byte array from hex string
        /// </summary>
        /// <param name="hex">Hex string to decode</param>
        /// <param name="validate">Validate characters</param>
        /// <returns>Array with decoded hex string</returns>
        /// <exception cref="ArgumentException">If string has odd length</exception>
        public static unsafe byte[] DecodeHex(string hex, bool validate = true) {
            var len = hex.Length;
            if (len == 0)
                return new byte[0];
            if (len % 2 != 0)
                throw new ArgumentException($"Hex string has length {hex.Length}, must be even");
            len /= 2;
            fixed (char* buf = &hex.AsSpan().GetPinnableReference()) {
                var rBuf = buf;
                if (len != 0 && rBuf[0] == '0' && (rBuf[1] == 'x' || rBuf[1] == 'X')) {
                    rBuf += 2;
                    len--;
                }

                var res = new byte[len];
                char c;
                if (validate)
                    for (var i = 0; i < len; i++) {
                        c = *rBuf++;
                        if (c > 0x60) {
                            if (c > 0x66)
                                throw new ArgumentException($"Illegal character {c} at position {rBuf - buf}");
                            res[i] = (byte)((c + 9) << 4);
                        }
                        else if (c > 0x40) {
                            if (c > 0x46)
                                throw new ArgumentException($"Illegal character {c} at position {rBuf - buf}");
                            res[i] = (byte)((c + 9) << 4);
                        }
                        else if (c > 0x2F) {
                            if (c > 0x39)
                                throw new ArgumentException($"Illegal character {c} at position {rBuf - buf}");
                            res[i] = (byte)(c << 4);
                        }
                        else
                            throw new ArgumentException($"Illegal character {c} at position {rBuf - buf}");

                        c = *rBuf++;
                        if (c > 0x60) {
                            if (c > 0x66)
                                throw new ArgumentException($"Illegal character {c} at position {rBuf - buf}");
                            res[i] += (byte)((c + 9) & 0xf);
                        }
                        else if (c > 0x40) {
                            if (c > 0x46)
                                throw new ArgumentException($"Illegal character {c} at position {rBuf - buf}");
                            res[i] += (byte)((c + 9) & 0xf);
                        }
                        else if (c > 0x2F) {
                            if (c > 0x39)
                                throw new ArgumentException($"Illegal character {c} at position {rBuf - buf}");
                            res[i] += (byte)(c & 0xf);
                        }
                        else
                            throw new ArgumentException($"Illegal character {c} at position {rBuf - buf}");
                    }
                else
                    for (var i = 0; i < len; i++) {
                        c = *rBuf++;
                        if (c < 0x3A)
                            res[i] = (byte)(c << 4);
                        else
                            res[i] = (byte)((c + 9) << 4);
                        c = *rBuf++;
                        if (c < 0x3A)
                            res[i] += (byte)(c & 0xf);
                        else
                            res[i] += (byte)((c + 9) & 0xf);
                    }

                return res;
            }
        }

        #endregion

        #region Output to stream utilities

        /// <summary>
        /// Output data from stream to stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="outputStream">Stream to write to</param>
        /// <returns>Length of written data</returns>
        public static long OutputAll(Stream stream, Stream outputStream) {
            var pos = outputStream.Position;
            stream.CopyTo(outputStream);
            return outputStream.Position - pos;
        }

        /// <summary>
        /// Output data from stream to file
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File name or relative path</param>
        /// <returns>Length of written data</returns>
        public long OutputAll(Stream stream, string extension = null, string filename = null) {
            using var fileStream = OpenOutputFile(false, extension, filename);
            return OutputAll(stream, fileStream);
        }

        /// <summary>
        /// Output data from current file's input stream to file
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File name or relative path</param>
        /// <returns>Length of written data</returns>
        public long OutputAll(string extension = null, string filename = null)
            => OutputAll(InputStream, extension, filename);

        /// <summary>
        /// Output data from stream to file under folder named by current file's name
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File name or relative path</param>
        /// <returns>Length of written data</returns>
        public long OutputAllSub(Stream stream, string extension = null, string filename = null) {
            using var fileStream = OpenOutputSubFile(false, extension, filename);
            return OutputAll(stream, fileStream);
        }

        /// <summary>
        /// Output data from current file's input stream to file under folder named by current file's name
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File name or relative path</param>
        /// <returns>Length of written data</returns>
        public long OutputAllSub(string extension = null, string filename = null)
            => OutputAllSub(InputStream, extension, filename == null
                ? null
                : BasicJoin(
                    Path.GetFileName(InputFile) ??
                    throw new ProcessorException($"Null filename for path {InputFile}"), filename));

        /// <summary>
        /// Output data from stream to stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="length">Number of bytes to read</param>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="lenient">If false, throws upon failure to read required number of bytes</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Length of written data</returns>
        /// <exception cref="ProcessorException"> if <paramref name="lenient"/> is false and not enough bytes are available from <paramref name="stream"/></exception>
        public long Output(Stream stream, long length, Stream outputStream = null, bool lenient = true,
            int bufferLength = 4096)
            => WriteBaseStream(stream, length, outputStream ?? OutputStream, lenient, bufferLength);

        /// <summary>
        /// Output data from stream to stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="length">Number of bytes to read</param>
        /// <param name="outputStream">Stream to write to</param>
        /// <param name="lenient">If false, throws upon failure to read required number of bytes</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Length of written data</returns>
        /// <exception cref="ProcessorException"> if <paramref name="lenient"/> is false and not enough bytes are available from <paramref name="stream"/></exception>
        /// <remarks>Original position of <paramref name="stream"/> is restored on completion</remarks>
        public long Output(Stream stream, long offset, long length, Stream outputStream = null, bool lenient = true,
            int bufferLength = 4096) {
            var origPos = stream.Position;
            stream.Position = offset;
            var outLen = Output(stream, length, outputStream ?? OutputStream, lenient, bufferLength);
            stream.Position = origPos;
            return outLen;
        }

        /// <summary>
        /// Output data from current file's input stream to output stream
        /// </summary>
        /// <param name="lenient">If false, throws upon failure to read required number of bytes</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Length of written data</returns>
        /// <exception cref="ProcessorException"> if <paramref name="lenient"/> is false and not enough bytes are available from input stream</exception>
        public long Output(bool lenient = true, int bufferLength = 4096)
            => Output(InputStream, long.MaxValue, OutputStream, lenient, bufferLength);

        /// <summary>
        /// Output data from current file's input stream to output stream
        /// </summary>
        /// <param name="length">Number of bytes to read</param>
        /// <param name="lenient">If false, throws upon failure to read required number of bytes</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Length of written data</returns>
        /// <exception cref="ProcessorException"> if <paramref name="lenient"/> is false and not enough bytes are available from input stream</exception>
        public long Output(long length, bool lenient = true, int bufferLength = 4096)
            => Output(InputStream, length, OutputStream, lenient, bufferLength);

        /// <summary>
        /// Output data from current file's input stream to output stream
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="length">Number of bytes to read</param>
        /// <param name="lenient">If false, throws upon failure to read required number of bytes</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Length of written data</returns>
        /// <exception cref="ProcessorException"> if <paramref name="lenient"/> is false and not enough bytes are available from input stream</exception>
        /// <remarks>Original position of input stream is restored on completion</remarks>
        public long Output(long offset, long length, bool lenient = true, int bufferLength = 4096) {
            var origPos = InputStream.Position;
            InputStream.Position = offset;
            var outLen = Output(InputStream, length, OutputStream, lenient, bufferLength);
            InputStream.Position = origPos;
            return outLen;
        }

        /// <summary>
        /// Output data from stream to file
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="filename">File to write to</param>
        /// <returns>Length of written data</returns>
        public long Output(Stream stream, string filename)
            => OutputAll(stream, null, filename);

        /// <summary>
        /// Output data from stream to file
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="length">Number of bytes to read</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File or relative path to write to</param>
        /// <param name="lenient">If false, throws upon failure to read required number of bytes</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Length of written data</returns>
        /// <exception cref="ProcessorException"> if <paramref name="lenient"/> is false and not enough bytes are available from <paramref name="stream"/></exception>
        public long Output(Stream stream, long length, string extension = null, string filename = null,
            bool lenient = true, int bufferLength = 4096) {
            using var fileStream = OpenOutputFile(false, extension, filename);
            return Output(stream, length, fileStream, lenient, bufferLength);
        }

        /// <summary>
        /// Output data from stream to file
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="length">Number of bytes to read</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File or relative path to write to</param>
        /// <param name="lenient">If false, throws upon failure to read required number of bytes</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Length of written data</returns>
        /// <exception cref="ProcessorException"> if <paramref name="lenient"/> is false and not enough bytes are available from <paramref name="stream"/></exception>
        /// <remarks>Original position of <paramref name="stream"/> is restored on completion</remarks>
        public long Output(Stream stream, long offset, long length, string extension = null, string filename = null,
            bool lenient = true, int bufferLength = 4096) {
            var origPos = stream.Position;
            stream.Position = offset;
            var outLen = Output(stream, length, extension, filename, lenient, bufferLength);
            stream.Position = origPos;
            return outLen;
        }

        /// <summary>
        /// Output data from current file's input stream to file
        /// </summary>
        /// <param name="length">Number of bytes to read</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File or relative path to write to</param>
        /// <param name="lenient">If false, throws upon failure to read required number of bytes</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Length of written data</returns>
        /// <exception cref="ProcessorException"> if <paramref name="lenient"/> is false and not enough bytes are available from input stream</exception>
        public long Output(long length, string extension = null, string filename = null, bool lenient = true,
            int bufferLength = 4096)
            => Output(InputStream, length, extension, filename, lenient, bufferLength);

        /// <summary>
        /// Output data from current file's input stream to file
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="length">Number of bytes to read</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File or relative path to write to</param>
        /// <param name="lenient">If false, throws upon failure to read required number of bytes</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Length of written data</returns>
        /// <exception cref="ProcessorException"> if <paramref name="lenient"/> is false and not enough bytes are available from input stream</exception>
        /// <remarks>Original position of input stream is restored on completion</remarks>
        public long Output(long offset, long length, string extension = null, string filename = null,
            bool lenient = true, int bufferLength = 4096) {
            var origPos = InputStream.Position;
            InputStream.Position = offset;
            var outLen = Output(InputStream, offset, length, extension, filename, lenient, bufferLength);
            InputStream.Position = origPos;
            return outLen;
        }

        /// <summary>
        /// Output data from stream to file under folder named by current file's name
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="length">Number of bytes to read</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File or relative path to write to</param>
        /// <param name="lenient">If false, throws upon failure to read required number of bytes</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Length of written data</returns>
        /// <exception cref="ProcessorException"> if <paramref name="lenient"/> is false and not enough bytes are available from <paramref name="stream"/></exception>
        public long OutputSub(Stream stream, long length, string extension = null, string filename = null,
            bool lenient = true,
            int bufferLength = 4096) {
            using var fileStream = OpenOutputSubFile(false, extension, filename);
            return Output(stream, length, fileStream, lenient, bufferLength);
        }

        /// <summary>
        /// Output data from stream to file under folder named by current file's name
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="length">Number of bytes to read</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File or relative path to write to</param>
        /// <param name="lenient">If false, throws upon failure to read required number of bytes</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Length of written data</returns>
        /// <exception cref="ProcessorException"> if <paramref name="lenient"/> is false and not enough bytes are available from <paramref name="stream"/></exception>
        /// <remarks>Original position of <paramref name="stream"/> is restored on completion</remarks>
        public long OutputSub(Stream stream, long offset, long length, string extension = null, string filename = null,
            bool lenient = true, int bufferLength = 4096) {
            var origPos = stream.Position;
            stream.Position = offset;
            var outLen = OutputSub(stream, length, extension, filename, lenient, bufferLength);
            stream.Position = origPos;
            return outLen;
        }

        /// <summary>
        /// Output data from current file's input stream to file under folder named by current file's name
        /// </summary>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="length">Number of bytes to read</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File or relative path to write to</param>
        /// <param name="lenient">If false, throws upon failure to read required number of bytes</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Length of written data</returns>
        /// <exception cref="ProcessorException"> if <paramref name="lenient"/> is false and not enough bytes are available from input stream</exception>
        /// <remarks>Original position of input stream is restored on completion</remarks>
        public long OutputSub(long offset, long length, string extension = null, string filename = null,
            bool lenient = true, int bufferLength = 4096)
            => OutputSub(InputStream, offset, length, extension, filename, lenient, bufferLength);

        #endregion

        #region Output to array utilities

        private static void WriteBaseArray(Stream stream, byte[] array, int offset, int length) =>
            stream.Write(array, offset, length);

        /// <summary>
        /// Output data from array to stream
        /// </summary>
        /// <param name="array">Array to read from</param>
        /// <param name="outputStream">Stream to write to</param>
        public void OutputAll(byte[] array, Stream outputStream)
            => Output(array, 0, array.Length, outputStream);

        /// <summary>
        /// Output data from array to file
        /// </summary>
        /// <param name="array">Array to read from</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File name or relative path</param>
        public void OutputAll(byte[] array, string extension = null, string filename = null) {
            using var fileStream = OpenOutputFile(false, extension, filename);
            Output(array, 0, array.Length, fileStream);
        }

        /// <summary>
        /// Output data from array to file under folder named by current file's name
        /// </summary>
        /// <param name="array">Array to read from</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File name or relative path</param>
        public void OutputAllSub(byte[] array, string extension = null, string filename = null) {
            using var fileStream = OpenOutputSubFile(false, extension, filename);
            OutputAll(array, fileStream);
        }

        /// <summary>
        /// Output data from array to stream
        /// </summary>
        /// <param name="array">Array to read from</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="length">Number of bytes to read</param>
        /// <param name="outputStream">Stream to write to</param>
        public void Output(byte[] array, int offset, int length, Stream outputStream = null) =>
            WriteBaseArray(outputStream ?? OutputStream, array, offset, length);

        /// <summary>
        /// Output data from array to file
        /// </summary>
        /// <param name="array">Array to read from</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File to write to</param>
        public void Output(byte[] array, string extension = null, string filename = null)
            => Output(array, 0, array.Length, extension, filename);

        /// <summary>
        /// Output data from stream to file
        /// </summary>
        /// <param name="array">Array to read from</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="length">Number of bytes to read</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File or relative path to write to</param>
        public void Output(byte[] array, int offset, int length, string extension = null, string filename = null) {
            using var fileStream = OpenOutputFile(false, extension, filename);
            Output(array, offset, length, fileStream);
        }

        /// <summary>
        /// Output data from stream to file under folder named by current file's name
        /// </summary>
        /// <param name="array">Array to read from</param>
        /// <param name="offset">Offset in array to read from</param>
        /// <param name="length">Number of bytes to read</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File or relative path to write to</param>
        public void OutputSub(byte[] array, int offset, int length, string extension = null, string filename = null) {
            using var fileStream = OpenOutputSubFile(false, extension, filename);
            Output(array, offset, length, fileStream);
        }

        #endregion

        #region Output to span utilities

        internal static void WriteBaseSpan(Stream stream, Span<byte> span) {
            var buf = ArrayPool<byte>.Shared.Rent(4096);
            var bufSpan = buf.AsSpan();
            var bufLen = buf.Length;
            try {
                int left = span.Length, tot = 0;
                do {
                    var toWrite = Math.Min(left, bufLen);
                    span.Slice(tot, toWrite).CopyTo(bufSpan);
                    stream.Write(buf, 0, toWrite);
                    tot += toWrite;
                    left -= toWrite;
                } while (left > 0);
            }
            finally {
                ArrayPool<byte>.Shared.Return(buf);
            }
        }

        /// <summary>
        /// Output data from span to stream
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="outputStream">Stream to write to</param>
        public void OutputAll(Span<byte> span, Stream outputStream = null) =>
            WriteBaseSpan(outputStream ?? OutputStream, span);

        /// <summary>
        /// Output data from array to file
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File name or relative path</param>
        public void OutputAll(Span<byte> span, string extension = null, string filename = null) {
            using var fileStream = OpenOutputFile(false, extension, filename);
            OutputAll(span, fileStream);
        }

        /// <summary>
        /// Output data from array to file under folder named by current file's name
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File name or relative path</param>
        public void OutputAllSub(Span<byte> span, string extension = null, string filename = null)
            => OutputAll(span, extension, filename == null
                ? null
                : BasicJoin(
                    Path.GetFileName(InputFile) ??
                    throw new ProcessorException($"Null filename for path {InputFile}"), filename));

        #endregion

        #region Filesystem utilities

        /// <summary>
        /// Get seekable stream (closes base stream if replaced)
        /// </summary>
        /// <param name="stream">Base stream</param>
        /// <returns>Seekable stream</returns>
        public static Stream GetSeekableStream(Stream stream) {
            if (stream.CanSeek) return stream;
            var ms = new MemoryStream();
            stream.CopyTo(ms);
            stream.Close();
            ms.Position = 0;
            stream = ms;
            return stream;
        }

        /// <summary>
        /// Open file for reading
        /// </summary>
        /// <param name="asMain">If true, sets <see cref="InputStream"/></param>
        /// <param name="file">File to open, <see cref="InputFile"/> by default</param>
        /// <returns>Created stream</returns>
        public Stream OpenFile(bool asMain, string file = null) {
            file = Path.Combine(InputDirectory, file ?? InputFile);
            if (!FileSystem.FileExists(file))
                return null;
            var stream = FileSystem.OpenRead(file);
            if (Preload && (!(stream is MemoryStream alreadyMs) || !alreadyMs.TryGetBuffer(out _) || alreadyMs.Capacity != alreadyMs.Length)) {
                var ms = new MemoryStream(new byte[stream.Length]);
                stream.CopyTo(ms);
                stream.Dispose();
                stream = ms;
            }

            if (!asMain) return stream;
            InputStream?.Dispose();
            InputStream = stream;
            InputLength = InputStream.Length;

            return stream;
        }

        /// <summary>
        /// Open file for reading and set <see cref="InputStream"/>
        /// </summary>
        /// <param name="file">File to open, <see cref="InputFile"/> by default</param>
        /// <returns>Created stream</returns>
        public Stream OpenFile(string file = null)
            => OpenFile(true, file);

        /// <summary>
        /// Open file in <see cref="InputDirectory"/> for reading
        /// </summary>
        /// <param name="name">File to open</param>
        /// <returns>Created stream</returns>
        public Stream OpenSibling(string name)
            => OpenFile(false, name);

        /// <summary>
        /// Close stream
        /// </summary>
        /// <param name="asMain">If true, clear <see cref="InputStream"/></param>
        /// <param name="stream">Stream to close</param>
        public void CloseFile(bool asMain, Stream stream = null) {
            stream ??= InputStream;
            stream?.Dispose();
            if (asMain)
                InputStream = null;
        }

        /// <summary>
        /// Close stream and clear <see cref="InputStream"/>
        /// </summary>
        /// <param name="stream">Stream to close</param>
        public void CloseFile(Stream stream = null)
            => CloseFile(true, stream);

        private Stream OpenOutputFileInternal(bool sub, bool asMain, string extension = null, string filename = null) {
            filename = sub ? GenPathSub(extension, filename) : GenPath(extension, filename);
            var stream = FileSystem.OpenWrite(filename);
            if (!asMain) return stream;
            OutputStream?.Dispose();
            OutputStream = stream;

            return stream;
        }

        /// <summary>
        /// Open file for writing
        /// </summary>
        /// <param name="asMain">If true, sets <see cref="OutputStream"/></param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File to open, generates path by default</param>
        /// <returns>Created stream</returns>
        public Stream OpenOutputFile(bool asMain, string extension = null, string filename = null)
            => OpenOutputFileInternal(false, asMain, extension, filename);

        /// <summary>
        /// Open file for writing and set <see cref="OutputStream"/>
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File to open, generates path by default</param>
        /// <returns>Created stream</returns>
        public Stream OpenOutputFile(string extension = null, string filename = null)
            => OpenOutputFileInternal(false, true, extension, filename);

        /// <summary>
        /// Open file for writing under folder named by current file's name
        /// </summary>
        /// <param name="asMain">If true, sets <see cref="OutputStream"/></param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File to open, generates path by default</param>
        /// <returns>Created stream</returns>
        public Stream OpenOutputSubFile(bool asMain, string extension = null, string filename = null)
            => OpenOutputFileInternal(true, asMain, extension, filename);

        /// <summary>
        /// Open file for writing under folder named by current file's name and set <see cref="OutputStream"/>
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File to open, generates path by default</param>
        /// <returns>Created stream</returns>
        public Stream OpenOutputSubFile(string extension = null, string filename = null)
            => OpenOutputFileInternal(true, true, extension, filename);

        /// <summary>
        /// Close stream
        /// </summary>
        /// <param name="asMain">If true, clear <see cref="OutputStream"/></param>
        /// <param name="stream">Stream to close</param>
        public void CloseOutputFile(bool asMain, Stream stream = null) {
            stream ??= OutputStream;
            stream?.Dispose();
            if (asMain)
                OutputStream = null;
        }

        /// <summary>
        /// Close stream and clear <see cref="OutputStream"/>
        /// </summary>
        /// <param name="stream">Stream to close</param>
        public void CloseOutputFile(Stream stream = null)
            => CloseOutputFile(true, stream);

        /// <summary>
        /// Make directories above path
        /// </summary>
        /// <param name="file">Path to make parents for</param>
        /// <exception cref="IOException"> when failed to create directories</exception>
        public void MkParents(string file) => MkDirs(Path.GetDirectoryName(file));

        /// <summary>
        /// Make directories up to path
        /// </summary>
        /// <param name="dir">Path to make directories to</param>
        /// <exception cref="IOException"> when failed to create directories</exception>
        public void MkDirs(string dir = null) {
            dir ??= OutputDirectory;
            if (!FileSystem.CreateDirectory(dir))
                throw new IOException($"Failed to create target directory {dir}");
        }

        /// <summary>
        /// Generate native path by replacing \ and / with native separators
        /// </summary>
        /// <param name="path">Path to change</param>
        /// <returns>Native OS compatible path</returns>
        public string MakeNative(string path)
            => path.Replace('\\', Path.AltDirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        // Change to Normalize, kill ./.. traversal

        /// <summary>
        /// Generate path under folder named by specified main file's name
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File name</param>
        /// <param name="mainFile">File to use for folder name and file name (if <paramref name="filename"/> not specified)</param>
        /// <param name="mkDirs">If true, create directories in filesystem</param>
        /// <returns>Generated path</returns>
        public string GenPathSub(string extension = null, string filename = null, string mainFile = null,
            bool mkDirs = true) {
            mainFile ??= InputFile;
            filename = filename == null
                ? $"{Path.GetFileNameWithoutExtension(mainFile)}_{OutputCounter++:D8}{extension}"
                : $"{filename}{extension}";
            var path = BasicJoin(OutputDirectory,
                Path.GetFileName(mainFile) ?? throw new ProcessorException($"Null {nameof(mainFile)} provided"),
                filename);
            if (mkDirs)
                MkParents(path);
            return path;
        }

        /// <summary>
        /// Generate path 
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File name</param>
        /// <param name="directory">Main output directory</param>
        /// <param name="mainFile">File to use for file name (if <paramref name="filename"/> not specified)</param>
        /// <param name="mkDirs">If true, create directories in filesystem</param>
        /// <returns>Generated path</returns>
        public string GenPath(string extension = null, string filename = null, string directory = null,
            string mainFile = null, bool mkDirs = true) {
            mainFile ??= InputFile;
            filename = filename == null
                ? $"{Path.GetFileNameWithoutExtension(mainFile)}_{OutputCounter++:D8}{extension}"
                : $"{filename}{extension}";
            var path = Path.Combine(OutputDirectory, directory ?? string.Empty, filename);
            if (mkDirs)
                MkParents(path);
            return path;
        }

        /// <summary>
        /// Create path from components
        /// </summary>
        /// <param name="paths">Elements to join</param>
        /// <returns>Path</returns>
        /// <exception cref="Exception">If separator is encountered by itself</exception>
        public string BasicJoin(params string[] paths)
            => BasicJoin(SupportBackSlash, paths);

        /// <summary>
        /// Create path from components
        /// </summary>
        /// <param name="supportBackSlash">Whether to allow backslashes as separators</param>
        /// <param name="paths">Elements to join</param>
        /// <returns>Path</returns>
        /// <exception cref="Exception">If separator is encountered by itself</exception>
        public static unsafe string BasicJoin(bool supportBackSlash, params string[] paths) {
            if (paths.Length < 2)
                return paths.Length == 0 ? string.Empty : paths[0] ?? throw new ArgumentException("Null element");
            var capacity = paths.Sum(path => (path ?? throw new ArgumentException("Null element")).Length);
            var buf = ArrayPool<char>.Shared.Rent(capacity + paths.Length - 1);
            try {
                var bufSpan = buf.AsSpan();
                var cIdx = 0;
                var prevEndWithSeparator = false;
                foreach (var path in paths) {
                    var pathLength = path.Length;
                    if (pathLength == 0)
                        continue;
                    var span = path.AsSpan();
                    var first = span[0];
                    if (first == '/' || supportBackSlash && first == '\\') {
                        if (pathLength == 1)
                            throw new Exception("Joining single-character separator disallowed");
                        if (prevEndWithSeparator)
                            span = span.Slice(1, --pathLength);
                    }
                    else if (cIdx != 0 && !prevEndWithSeparator)
                        bufSpan[cIdx++] = supportBackSlash ? '\\' : '/';

                    span.CopyTo(bufSpan.Slice(cIdx));
                    cIdx += span.Length;
                    var last = span[pathLength - 1];
                    prevEndWithSeparator = last == '/' || supportBackSlash && last == '\\';
                }

                fixed (char* ptr = &bufSpan.GetPinnableReference())
                    return new string(ptr, 0, cIdx);
            }
            finally {
                ArrayPool<char>.Shared.Return(buf);
            }
        }

        #endregion

        #region Logging utilities

        /// <summary>
        /// Invoke logger with formatted string containing specified log
        /// </summary>
        /// <param name="log">Message</param>
        public void LogInfo(string log) => Logger?.Invoke($"[{WorkerId:X2}][~]<INFO>: {log}");

        /// <summary>
        /// Invoke logger with formatted string containing specified log
        /// </summary>
        /// <param name="log">Message</param>
        public void LogWarn(string log) => Logger?.Invoke($"[{WorkerId:X2}][!]<WARN>: {log}");

        /// <summary>
        /// Invoke logger with formatted string containing specified log
        /// </summary>
        /// <param name="log">Message</param>
        public void LogFail(string log) => Logger?.Invoke($"[{WorkerId:X2}][X]<FAIL>: {log}");

        #endregion

        #region Lifecycle

        /// <summary>
        /// Cleanup resources
        /// </summary>
        public void SrcCleanup() {
            InputStream?.Dispose();
            OutputStream?.Dispose();
        }

        #endregion

        #region Dispose pattern

        /// <summary>
        /// Dispose resources
        /// </summary>
        /// <param name="disposing">False if called from finalizer</param>
        // ReSharper disable once VirtualMemberNeverOverridden.Global
        // ReSharper disable once UnusedParameter.Global
        protected virtual void Dispose(bool disposing) {
        }

        /// <inheritdoc />
        public void Dispose() {
            SrcCleanup();
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}