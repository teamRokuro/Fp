using System.IO;

namespace Fp.Structures
{
    /// <summary>
    /// Utility class for basing structure definitions.
    /// </summary>
    public abstract partial class Structure
    {
        /// <summary>
        /// Read data of specified type from a stream.
        /// </summary>
        /// <param name="stream">Stream to read from (uses current <see cref="Stream.Position"/>).</param>
        /// <typeparam name="T">Structure instance type.</typeparam>
        /// <returns>Read data.</returns>
        public static T Read<T>(Stream stream) where T : StructureInstance, new()
        {
            T res = new();
            res.Read(new StructureContext(stream, stream.Position));
            return res;
        }
    }
}
