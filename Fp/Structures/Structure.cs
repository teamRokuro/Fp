using System.IO;

namespace Fp.Structures
{
    public abstract partial class Structure
    {
        public static T Read<T>(Stream stream) where T : StructureInstance, new()
        {
            T res = new();
            res.Read(new StructureContext(stream, stream.Position));
            return res;
        }
    }
}
