using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fp.Structures
{
    /// <summary>
    /// Represents an instance of a structure.
    /// </summary>
    public abstract class StructureInstance
    {
        /// <summary>
        /// Read data into this structure.
        /// </summary>
        /// <param name="context">Context to use.</param>
        public abstract void Read(StructureContext context);

        /// <summary>
        /// Write data from this structure.
        /// </summary>
        /// <param name="context">Context to use.</param>
        public abstract void Write(StructureContext context);

        /// <summary>
        /// Write data from this structure.
        /// </summary>
        /// <param name="stream">Stream to write to (uses current <see cref="Stream.Position"/>).</param>
        public void Write(Stream stream) => Write(new StructureContext(stream, stream.Position));

        /// <summary>
        /// Sort structure elements based on dependencies.
        /// </summary>
        /// <param name="elements">Elements to sort.</param>
        /// <typeparam name="T">Structure instance type.</typeparam>
        /// <returns>Input enumerable sorted by dependencies.</returns>
        /// <exception cref="IOException">Thrown when failed to reduce dependencies.</exception>
        protected static
            List<(Element, Action<T, Expression, StructureContext>?, Action<T, WritableExpression, StructureContext>?)>
            BuildLayout<T>(
                IEnumerable<(Element, Action<T, Expression, StructureContext>?,
                    Action<T, WritableExpression, StructureContext>?)> elements)
            where T : StructureInstance
        {
            List<(Element, Action<T, Expression, StructureContext>?, Action<T, WritableExpression, StructureContext>?)>
                members = new();
            var sub =
                new List<(Element, Action<T, Expression, StructureContext>?,
                    Action<T, WritableExpression, StructureContext>?)>(elements);
            // Build members after organizing by dependencies
            while (sub.Count > 0)
            {
                int removed = sub.RemoveAll(e =>
                {
                    bool noDeps = !e.Item1.GetDependencies().Where(d => d != e.Item1)
                        .Intersect(sub.Select(x => x.Item1)).Any();
                    if (noDeps) members.Add(e);
                    return noDeps;
                });
                if (removed == 0) throw new IOException("Failed to reduce dependencies");
            }

            return members;
        }
    }
}
