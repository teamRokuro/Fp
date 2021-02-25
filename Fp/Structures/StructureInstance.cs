using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fp.Structures
{
    public abstract class StructureInstance
    {
        public abstract void Read(StructureContext context);
        public abstract void Write(StructureContext context);

        public void Write(Stream stream) => Write(new StructureContext(stream, stream.Position));

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
                if (removed == 0) throw new Exception("Failed to reduce dependencies");
            }

            return members;
        }
    }
}
