using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

#if !NET5_0
using static System.Buffers.ArrayPool<byte>;

#endif

namespace Fp
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public partial class Processor
    {
        #region Output from stream utilities

        /// <summary>
        /// Output data from stream to stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="outputStream">Stream to write to</param>
        /// <returns>Length of written data</returns>
        public static long OutputAll(Stream stream, Stream outputStream)
        {
            long pos = outputStream.Position;
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
        public long OutputAll(Stream stream, string? extension = null, string? filename = null)
        {
            using Stream fileStream = OpenOutputFile(false, extension, filename);
            return OutputAll(stream, fileStream);
        }

        /// <summary>
        /// Output data from current file's input stream to file
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File name or relative path</param>
        /// <returns>Length of written data</returns>
        public long OutputAll(string? extension = null, string? filename = null)
            => OutputAll(InputStream ?? throw new InvalidOperationException(), extension, filename);

        /// <summary>
        /// Output data from stream to file under folder named by current file's name
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File name or relative path</param>
        /// <returns>Length of written data</returns>
        public long OutputAllSub(Stream stream, string? extension = null, string? filename = null)
        {
            using Stream fileStream = OpenOutputSubFile(false, extension, filename);
            return OutputAll(stream, fileStream);
        }

        /// <summary>
        /// Output data from current file's input stream to file under folder named by current file's name
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File name or relative path</param>
        /// <returns>Length of written data</returns>
        public long OutputAllSub(string? extension = null, string? filename = null)
            => OutputAllSub(InputStream ?? throw new InvalidOperationException(), extension, filename == null
                ? null
                : Join(SupportBackSlash,
                    Path.GetFileName(InputFile ?? throw new InvalidOperationException()) ??
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
        public long Output(Stream stream, long length, Stream? outputStream = null, bool lenient = true,
            int bufferLength = 4096)
            => WriteBaseStream(stream, length, outputStream ?? OutputStream ?? throw new InvalidOperationException(),
                lenient, bufferLength);

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
        public long Output(Stream stream, long offset, long length, Stream? outputStream = null, bool lenient = true,
            int bufferLength = 4096)
        {
            outputStream ??= OutputStream ?? throw new InvalidOperationException();
            long origPos = stream.Position;
            try
            {
                stream.Position = offset;
                long outLen = Output(stream, length, outputStream, lenient, bufferLength);
                return outLen;
            }
            finally
            {
                stream.Position = origPos;
            }
        }

        /// <summary>
        /// Output data from current file's input stream to output stream
        /// </summary>
        /// <param name="lenient">If false, throws upon failure to read required number of bytes</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Length of written data</returns>
        /// <exception cref="ProcessorException"> if <paramref name="lenient"/> is false and not enough bytes are available from input stream</exception>
        public long Output(bool lenient = true, int bufferLength = 4096)
            => Output(InputStream ?? throw new InvalidOperationException(), long.MaxValue, OutputStream, lenient,
                bufferLength);

        /// <summary>
        /// Output data from current file's input stream to output stream
        /// </summary>
        /// <param name="length">Number of bytes to read</param>
        /// <param name="lenient">If false, throws upon failure to read required number of bytes</param>
        /// <param name="bufferLength">Minimum buffer length</param>
        /// <returns>Length of written data</returns>
        /// <exception cref="ProcessorException"> if <paramref name="lenient"/> is false and not enough bytes are available from input stream</exception>
        public long Output(long length, bool lenient = true, int bufferLength = 4096)
            => Output(InputStream ?? throw new InvalidOperationException(), length, OutputStream, lenient,
                bufferLength);

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
        public long Output(long offset, long length, bool lenient = true, int bufferLength = 4096)
        {
            long origPos = (InputStream ?? throw new InvalidOperationException()).Position;
            try
            {
                InputStream.Position = offset;
                long outLen = Output(InputStream, length, OutputStream, lenient, bufferLength);
                return outLen;
            }
            finally
            {
                InputStream.Position = origPos;
            }
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
        public long Output(Stream stream, long length, string? extension = null, string? filename = null,
            bool lenient = true, int bufferLength = 4096)
        {
            using Stream fileStream = OpenOutputFile(false, extension, filename);
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
        public long Output(Stream stream, long offset, long length, string? extension = null, string? filename = null,
            bool lenient = true, int bufferLength = 4096)
        {
            long origPos = stream.Position;
            try
            {
                stream.Position = offset;
                long outLen = Output(stream, length, extension, filename, lenient, bufferLength);
                return outLen;
            }
            finally
            {
                stream.Position = origPos;
            }
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
        public long Output(long length, string? extension = null, string? filename = null, bool lenient = true,
            int bufferLength = 4096)
            => Output(InputStream ?? throw new InvalidOperationException(), length, extension, filename, lenient,
                bufferLength);

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
        public long Output(long offset, long length, string? extension = null, string? filename = null,
            bool lenient = true, int bufferLength = 4096)
        {
            long origPos = (InputStream ?? throw new InvalidOperationException()).Position;
            try
            {
                InputStream.Position = offset;
                long outLen = Output(InputStream, offset, length, extension, filename, lenient, bufferLength);
                return outLen;
            }
            finally
            {
                InputStream.Position = origPos;
            }
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
        public long OutputSub(Stream stream, long length, string? extension = null, string? filename = null,
            bool lenient = true,
            int bufferLength = 4096)
        {
            using Stream fileStream = OpenOutputSubFile(false, extension, filename);
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
        public long OutputSub(Stream stream, long offset, long length, string? extension = null,
            string? filename = null,
            bool lenient = true, int bufferLength = 4096)
        {
            long origPos = stream.Position;
            try
            {
                stream.Position = offset;
                long outLen = OutputSub(stream, length, extension, filename, lenient, bufferLength);
                return outLen;
            }
            finally
            {
                stream.Position = origPos;
            }
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
        public long OutputSub(long offset, long length, string? extension = null, string? filename = null,
            bool lenient = true, int bufferLength = 4096)
            => OutputSub(InputStream ?? throw new InvalidOperationException(), offset, length, extension, filename,
                lenient, bufferLength);

        #endregion

        #region Output from array utilities

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
        public void OutputAll(byte[] array, string? extension = null, string? filename = null)
        {
            using Stream fileStream = OpenOutputFile(false, extension, filename);
            Output(array, 0, array.Length, fileStream);
        }

        /// <summary>
        /// Output data from array to file under folder named by current file's name
        /// </summary>
        /// <param name="array">Array to read from</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File name or relative path</param>
        public void OutputAllSub(byte[] array, string? extension = null, string? filename = null)
        {
            using Stream fileStream = OpenOutputSubFile(false, extension, filename);
            OutputAll(array, fileStream);
        }

        /// <summary>
        /// Output data from array to stream
        /// </summary>
        /// <param name="array">Array to read from</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="length">Number of bytes to read</param>
        /// <param name="outputStream">Stream to write to</param>
        public void Output(byte[] array, int offset, int length, Stream? outputStream = null) =>
            WriteBaseArray(outputStream ?? OutputStream ?? throw new InvalidOperationException(), array, offset,
                length);

        /// <summary>
        /// Output data from array to file
        /// </summary>
        /// <param name="array">Array to read from</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File to write to</param>
        public void Output(byte[] array, string? extension = null, string? filename = null)
            => Output(array, 0, array.Length, extension, filename);

        /// <summary>
        /// Output data from stream to file
        /// </summary>
        /// <param name="array">Array to read from</param>
        /// <param name="offset">Offset in stream to read from</param>
        /// <param name="length">Number of bytes to read</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File or relative path to write to</param>
        public void Output(byte[] array, int offset, int length, string? extension = null, string? filename = null)
        {
            using Stream fileStream = OpenOutputFile(false, extension, filename);
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
        public void OutputSub(byte[] array, int offset, int length, string? extension = null, string? filename = null)
        {
            using Stream fileStream = OpenOutputSubFile(false, extension, filename);
            Output(array, offset, length, fileStream);
        }

        #endregion

        #region Output from span utilities

        internal static void WriteBaseSpan(Stream stream, ReadOnlySpan<byte> span)
        {
#if NET5_0
            stream.Write(span);
#else
            byte[] buf = Shared.Rent(4096);
            Span<byte> bufSpan = buf.AsSpan();
            int bufLen = buf.Length;
            try
            {
                int left = span.Length, tot = 0;
                do
                {
                    int toWrite = Math.Min(left, bufLen);
                    span.Slice(tot, toWrite).CopyTo(bufSpan);
                    stream.Write(buf, 0, toWrite);
                    tot += toWrite;
                    left -= toWrite;
                } while (left > 0);
            }
            finally
            {
                Shared.Return(buf);
            }
#endif
        }

        /// <summary>
        /// Output data from span to stream
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="outputStream">Stream to write to</param>
        public void OutputAll(Span<byte> span, Stream? outputStream = null) =>
            WriteBaseSpan(outputStream ?? OutputStream ?? throw new InvalidOperationException(), span);

        /// <summary>
        /// Output data from array to file
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File name or relative path</param>
        public void OutputAll(Span<byte> span, string? extension = null, string? filename = null)
        {
            using Stream fileStream = OpenOutputFile(false, extension, filename);
            OutputAll(span, fileStream);
        }

        /// <summary>
        /// Output data from array to file under folder named by current file's name
        /// </summary>
        /// <param name="span">Span to read from</param>
        /// <param name="extension">File extension</param>
        /// <param name="filename">File name or relative path</param>
        public void OutputAllSub(Span<byte> span, string? extension = null, string? filename = null)
            => OutputAll(span, extension, filename == null
                ? null
                : Join(SupportBackSlash,
                    Path.GetFileName(InputFile ?? throw new InvalidOperationException()) ??
                    throw new ProcessorException($"Null filename for path {InputFile}"), filename));

        #endregion
    }
}
