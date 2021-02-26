using System.IO;

namespace Fp.Structures
{
    /// <summary>
    /// Represents a context for operating on a structure.
    /// </summary>
    public record StructureContext(Stream Stream, long Offset)
    {
        /// <summary>
        /// Seek using the specified offset relative to this context.
        /// </summary>
        /// <param name="offset">Offset.</param>
        public void Seek(long offset)
        {
            Stream.Position = Offset + offset;
        }
    }
}
