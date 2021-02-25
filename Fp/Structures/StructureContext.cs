using System.IO;

namespace Fp.Structures
{
    public record StructureContext(Stream Stream, long Offset)
    {
        public void Seek(long offset)
        {
            Stream.Position = Offset + offset;
        }
    }
}
