using System;
using System.IO;

namespace Fp
{
    /// <summary>
    /// Represents a reading context over a <see cref="ReadOnlySpan{T}"/>.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    public ref struct ReadContext<T> where T : unmanaged
    {
        /// <summary>
        /// Source span.
        /// </summary>
        public readonly ReadOnlySpan<T> Source;

        /// <summary>
        /// Current offset.
        /// </summary>
        public int Offset;

        /// <summary>
        /// Creates new instance of <see cref="ReadContext{T}"/>.
        /// </summary>
        /// <param name="span">Source span.</param>
        /// <param name="offset">Initial offset.</param>
        public ReadContext(ReadOnlySpan<T> span, int offset = 0)
        {
            Source = span;
            Offset = offset;
        }

        /// <summary>
        /// Checks if provided length is available from current position.
        /// </summary>
        /// <param name="length">Length.</param>
        /// <returns>True if available.</returns>
        public bool IsAvailable(int length) =>
            Offset >= 0 ? Offset + length <= Source.Length : throw new InvalidOperationException();

        /// <summary>
        /// Checks if provided position is valid.
        /// </summary>
        /// <param name="offset">Offset.</param>
        /// <param name="endIsOk">Allow end position to return true.</param>
        /// <returns>True if position is valid.</returns>
        public bool IsValid(int offset, bool endIsOk = true) =>
            offset == Source.Length && endIsOk || offset < Source.Length && offset >= 0;

        /// <summary>
        /// Advance context.
        /// </summary>
        /// <param name="value">Relative offset to advance by.</param>
        /// <param name="endIsOk">Allow end position to return true.</param>
        /// <param name="dontCommitInvalid">Do not commit state if invalid.</param>
        /// <returns>True if final state is valid.</returns>
        public bool Advance(int value, bool endIsOk = true, bool dontCommitInvalid = false)
        {
            if (!dontCommitInvalid) return IsValid(Offset += value, endIsOk);
            if (!IsValid(Offset + value, endIsOk)) return false;
            Offset += value;
            return true;
        }

        /// <summary>
        /// Gets backing span from current offset.
        /// </summary>
        /// <returns>Backing span starting at current offset.</returns>
        public ReadOnlySpan<T> GetSpan() => Source.Slice(Offset);

        /// <summary>
        /// Get backing span from current offset with specified length.
        /// </summary>
        /// <param name="length">Target length.</param>
        /// <returns>Backing span starting at current offset with specified length.</returns>
        public ReadOnlySpan<T> GetSpan(int length) => Source.Slice(Offset, length);

        /// <summary>
        /// Read and advance.
        /// </summary>
        /// <returns>Read value.</returns>
        /// <exception cref="EndOfStreamException">End has been reached.</exception>
        public T ReadAdvance()
        {
            if (!IsAvailable(1)) throw new EndOfStreamException();
            return Source[Offset++];
        }

        /// <summary>
        /// Read and advance.
        /// </summary>
        /// <param name="count">Number of elements.</param>
        /// <returns>Read value.</returns>
        /// <exception cref="EndOfStreamException">End has been reached.</exception>
        public ReadOnlySpan<T> ReadAdvance(int count)
        {
            if (!IsAvailable(count)) throw new EndOfStreamException();
            int offset = Offset;
            Offset += count;
            return Source.Slice(offset, count);
        }

        /// <summary>
        /// Read value.
        /// </summary>
        /// <param name="index">Offset.</param>
        public T this[int index] => Source[Offset + index];

        /// <summary>
        /// Represent context as span offset from backing span.
        /// </summary>
        /// <param name="value">Context.</param>
        /// <returns>Offset span.</returns>
        public static implicit operator ReadOnlySpan<T>(ReadContext<T> value) => value.Source.Slice(value.Offset);
    }
}
