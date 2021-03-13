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
            => (_inputStream ?? throw new InvalidOperationException()).Seek(bytes, SeekOrigin.Current);

        internal static int ReadBaseArray(Stream stream, byte[] array, int offset, int length, bool lenient)
        {
            int left = length, read, tot = 0;
            do
            {
                read = stream.Read(array, offset + tot, left);
                left -= read;
                tot += read;
            } while (left > 0 && read != 0);

            if (left > 0 && read == 0 && !lenient)
            {
                throw new ProcessorException(
                    $"Failed to read required number of bytes! 0x{read:X} read, 0x{left:X} left, 0x{stream.Position:X} end position");
            }

            return tot;
        }

        internal static int ReadBaseSpan(Stream stream, Span<byte> span, bool lenient)
        {
#if NET5_0
            int left = span.Length, read, tot = 0;
            do
            {
                read = stream.Read(span.Slice(tot));
                left -= read;
                tot += read;
            } while (left > 0 && read != 0);

            if (left > 0 && !lenient)
            {
                throw new ProcessorException(
                    $"Failed to read required number of bytes! 0x{read:X} read, 0x{left:X} left, 0x{stream.Position:X} end position");
            }

            return tot;
#else
            byte[] buf = span.Length <= sizeof(long) ? TempBuffer : Shared.Rent(4096);
            Span<byte> bufSpan = buf.AsSpan();
            int bufLen = buf.Length;
            try
            {
                int left = span.Length, read, tot = 0;
                do
                {
                    read = stream.Read(buf, 0, Math.Min(left, bufLen));
                    bufSpan.Slice(0, read).CopyTo(span.Slice(tot));
                    left -= read;
                    tot += read;
                } while (left > 0 && read != 0);

                if (left > 0 && !lenient)
                {
                    throw new ProcessorException(
                        $"Failed to read required number of bytes! 0x{read:X} read, 0x{left:X} left, 0x{stream.Position:X} end position");
                }

                return tot;
            }
            finally
            {
                if (buf != TempBuffer) Shared.Return(buf);
            }
#endif
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
        public static int Read(Stream stream, ref Span<byte> span, bool lenient = true, bool forceNew = false)
        {
            if (forceNew || !(stream is MemoryStream ms) || !ms.TryGetBuffer(out ArraySegment<byte> buf))
            {
                return ReadBaseSpan(stream, span, lenient);
            }

            try
            {
                return (span = buf.AsSpan((int)ms.Position, span.Length)).Length;
            }
            catch (ArgumentOutOfRangeException exception)
            {
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
            bool forceNew = false)
        {
            if (!forceNew && stream is MemoryStream ms && ms.TryGetBuffer(out ArraySegment<byte> buf))
            {
                try
                {
                    return (span = buf.AsSpan((int)stream.Position, length)).Length;
                }
                catch (ArgumentOutOfRangeException exception)
                {
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
        public static int Read(Stream stream, Span<byte> span, bool lenient = true)
        {
            if (!(stream is MemoryStream ms) || !ms.TryGetBuffer(out ArraySegment<byte> buf))
            {
                return ReadBaseSpan(stream, span, lenient);
            }

            try
            {
                buf.AsSpan((int)ms.Position, span.Length).CopyTo(span);
                return span.Length;
            }
            catch (ArgumentOutOfRangeException exception)
            {
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
            => Read(_inputStream ?? throw new InvalidOperationException(), ref span, lenient, forceNew);

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
            => Read(_inputStream ?? throw new InvalidOperationException(), length, out span, lenient, forceNew);

        /// <summary>
        /// Read data from current file's input stream
        /// </summary>
        /// <param name="span">Target to copy to</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public int Read(Span<byte> span, bool lenient = true)
            => Read(_inputStream ?? throw new InvalidOperationException(), span, lenient);

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
        public static int Read(Stream stream, long offset, Span<byte> span, bool lenient = true)
        {
            long position = stream.Position;
            try
            {
                stream.Position = offset;
                int count = Read(stream, span, lenient);
                return count;
            }
            finally
            {
                stream.Position = position;
            }
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
        public int Read(long offset, Span<byte> span, bool lenient = true)
            => Read(_inputStream ?? throw new InvalidOperationException(), offset, span, lenient);

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
            bool forceNew = false)
        {
            long position = stream.Position;
            try
            {
                stream.Position = offset;
                int count = Read(stream, ref span, lenient, forceNew);
                return count;
            }
            finally
            {
                stream.Position = position;
            }
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
        public int Read(long offset, ref Span<byte> span, bool lenient = true, bool forceNew = false)
            => Read(_inputStream ?? throw new InvalidOperationException(), offset, ref span, lenient, forceNew);

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
        public int Read(Stream stream, long offset, int length, out Span<byte> span, bool lenient = true,
            bool forceNew = false)
        {
            long position = stream.Position;
            try
            {
                stream.Position = offset;
                int count = Read(stream, length, out span, lenient, forceNew);
                return count;
            }
            finally
            {
                stream.Position = position;
            }
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
        public int Read(long offset, int length, out Span<byte> span, bool lenient = true, bool forceNew = false)
            => Read(_inputStream ?? throw new InvalidOperationException(), offset, length, out span, lenient, forceNew);

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
            => Read(_inputStream ?? throw new InvalidOperationException(), array, arrayOffset, arrayLength, lenient);

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
            bool lenient = true)
        {
            long position = stream.Position;
            try
            {
                stream.Position = offset;
                int count = Read(stream, array, arrayOffset, arrayLength, lenient);
                return count;
            }
            finally
            {
                stream.Position = position;
            }
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
        public int Read(long offset, byte[] array, int arrayOffset, int arrayLength, bool lenient = true)
            => Read(_inputStream ?? throw new InvalidOperationException(), offset, array, arrayOffset, arrayLength,
                lenient);

        /// <summary>
        /// Read data from stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="array">Target to copy to</param>
        /// <param name="lenient">If false, throws when failed to fill target</param>
        /// <returns>Number of bytes read</returns>
        /// <exception cref="ProcessorException"> when <paramref name="lenient"/> is false
        /// and stream cannot provide enough data to fill target</exception>
        public static int Read(Stream stream, byte[] array, bool lenient = true)
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
            => Read(_inputStream ?? throw new InvalidOperationException(), array, 0, array.Length, lenient);

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
        /// Get byte array from stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="forceNew">Force use newly allocated buffer</param>
        /// <returns>Array with file contents</returns>
        public static byte[] GetArray(Stream stream, bool forceNew = false)
        {
            if (!stream.CanSeek)
                throw new NotSupportedException("Getting memory from non-seekable stream is unsupported");
            switch (stream)
            {
                case MStream mes:
                    return mes.GetMemory().ToArray();
                case MemoryStream ms when !forceNew:
                    return ms.Capacity == ms.Length && ms.TryGetBuffer(out _) ? ms.GetBuffer() : ms.ToArray();
                default:
                    stream.Position = 0;
                    try
                    {
                        byte[] arr = new byte[stream.Length];
                        Read(stream, arr, false);
                        return arr;
                    }
                    catch (Exception)
                    {
                        // Fallback to MemoryStream copy
                        stream.Position = 0;
                        MemoryStream ms2 = new();
                        stream.CopyTo(ms2);
                        return ms2.ToArray();
                    }
            }
        }

        /// <summary>
        /// Get byte array from input stream
        /// </summary>
        /// <param name="forceNew">Force use newly allocated buffer</param>
        /// <returns>Array with file contents</returns>
        public byte[] GetArray(bool forceNew = false)
            => GetArray(_inputStream ?? throw new InvalidOperationException(), forceNew);

        /// <summary>
        /// Get byte array from stream
        /// </summary>
        /// <param name="offset">Offset in stream</param>
        /// <param name="length">Length of segment</param>
        /// <param name="stream">Stream to read from</param>
        /// <param name="forceNew">Force use newly allocated buffer</param>
        /// <returns>Array with file contents</returns>
        public static byte[] GetArray(int offset, int length, Stream stream, bool forceNew = false)
        {
            if (!stream.CanSeek)
                throw new NotSupportedException("Getting memory from non-seekable stream is unsupported");
            switch (stream)
            {
                case MStream mes:
                    return mes.GetMemory().Slice(offset, length).ToArray();
                case MemoryStream ms when !forceNew:
                    return offset == 0 && ms.Length == length && ms.Capacity == ms.Length && ms.TryGetBuffer(out _)
                        ? ms.GetBuffer()
                        : ms.ToArray();
                default:
                    if (offset + length > stream.Length)
                        throw new IOException("Target range exceeds stream bounds");
                    stream.Position = offset;
                    try
                    {
                        byte[] arr = new byte[length];
                        Read(stream, arr, false);
                        return arr;
                    }
                    catch (Exception)
                    {
                        // Fallback to MemoryStream copy
                        stream.Position = offset;
                        MemoryStream ms2 = new();
                        new SStream(stream, length).CopyTo(ms2);
                        return ms2.ToArray();
                    }
            }
        }

        /// <summary>
        /// Get byte array from input stream
        /// </summary>
        /// <param name="offset">Offset in stream</param>
        /// <param name="length">Length of segment</param>
        /// <param name="forceNew">Force use newly allocated buffer</param>
        /// <returns>Array with file contents</returns>
        public byte[] GetArray(int offset, int length, bool forceNew = false)
            => GetArray(offset, length, _inputStream ?? throw new InvalidOperationException(), forceNew);

        /// <summary>
        /// Dump remaining content from stream to byte array
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <param name="maxLength">Maximum length.</param>
        /// <returns>Array with file contents</returns>
        public static byte[] DumpArray(Stream stream, int maxLength = int.MaxValue)
        {
            MemoryStream ms2 = new();
            new SStream(stream, maxLength, false).CopyTo(ms2);
            return ms2.ToArray();
        }

        /// <summary>
        /// Dump remaining content from input stream to byte array
        /// </summary>
        /// <param name="maxLength">Maximum length.</param>
        /// <returns>Array with file contents</returns>
        public byte[] DumpArray(int maxLength = int.MaxValue)
            => DumpArray(_inputStream ?? throw new InvalidOperationException(), maxLength);

        /// <summary>
        /// Get read-only memory from stream
        /// </summary>
        /// <param name="stream">Stream to read from</param>
        /// <returns>Array with file contents</returns>
        /// <remarks>Non-allocating requisition of memory from <see cref="MemoryStream"/> and <see cref="MStream"/> is supported</remarks>
        public static ReadOnlyMemory<byte> GetMemory(Stream stream)
        {
            if (!stream.CanSeek)
                throw new NotSupportedException("Getting memory from non-seekable stream is unsupported");
            switch (stream)
            {
                case MStream mes:
                    return mes.GetMemory();
                case MemoryStream ms when ms.TryGetBuffer(out ArraySegment<byte> buffer):
                    return buffer;
                default:
                    stream.Position = 0;
                    try
                    {
                        byte[] arr = new byte[stream.Length];
                        Read(stream, arr, false);
                        return arr;
                    }
                    catch (Exception)
                    {
                        // Fallback to MemoryStream copy
                        stream.Position = 0;
                        MemoryStream ms2 = new();
                        stream.CopyTo(ms2);
                        return ms2.GetBuffer().AsMemory(0, (int)ms2.Length);
                    }
            }
        }

        /// <summary>
        /// Get read-only memory from input stream
        /// </summary>
        /// <returns>Array with file contents</returns>
        /// <remarks>Non-allocating requisition of memory from <see cref="MemoryStream"/> and <see cref="MStream"/> is supported</remarks>
        public ReadOnlyMemory<byte> GetMemory()
            => GetMemory(_inputStream ?? throw new InvalidOperationException());

        /// <summary>
        /// Get read-only memory from stream
        /// </summary>
        /// <param name="offset">Offset in stream</param>
        /// <param name="length">Length of segment</param>
        /// <param name="stream">Stream to read from</param>
        /// <returns>Array with file contents</returns>
        /// <remarks>Non-allocating requisition of memory from <see cref="MemoryStream"/> and <see cref="MStream"/> is supported</remarks>
        public static ReadOnlyMemory<byte> GetMemory(long offset, int length, Stream stream)
        {
            if (!stream.CanSeek)
                throw new NotSupportedException("Getting memory from non-seekable stream is unsupported");
            switch (stream)
            {
                case MStream mes:
                    return mes.GetMemory().Slice((int)offset, length);
                case MemoryStream ms when ms.TryGetBuffer(out ArraySegment<byte> buffer):
                    return buffer.AsMemory((int)offset, length);
                default:
                    if (offset + length > stream.Length)
                        throw new IOException("Target range exceeds stream bounds");
                    stream.Position = offset;
                    try
                    {
                        byte[] arr = new byte[length];
                        Read(stream, arr, false);
                        return arr;
                    }
                    catch (Exception)
                    {
                        // Fallback to MemoryStream copy
                        stream.Position = offset;
                        MemoryStream ms2 = new();
                        new SStream(stream, length).CopyTo(ms2);
                        return ms2.GetBuffer().AsMemory(0, length);
                    }
            }
        }

        /// <summary>
        /// Get read-only memory from input stream
        /// </summary>
        /// <param name="offset">Offset in stream</param>
        /// <param name="length">Length of segment</param>
        /// <returns>Array with file contents</returns>
        /// <remarks>Non-allocating requisition of memory from <see cref="MemoryStream"/> and <see cref="MStream"/> is supported</remarks>
        public ReadOnlyMemory<byte> GetMemory(long offset, int length)
            => GetMemory(offset, length, _inputStream ?? throw new InvalidOperationException());

        #endregion
    }
}
