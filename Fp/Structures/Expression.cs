using System.Collections.Generic;
using Fp.Intermediate;

namespace Fp.Structures
{
    public abstract partial record Expression : Element
    {
        public abstract T? Read<T>(StructureContext context);
        public virtual T ReadUnmanaged<T>(StructureContext context) where T : unmanaged =>
            Data.CastNumber<object, T>(Read<object>(context)!);

        public virtual Expression GetMetaExpression(IReadOnlyDictionary<Element, Expression> mapping) =>
            this;

        public Expression GetSelfMetaExpression(IReadOnlyDictionary<Element, Expression> mapping) =>
            mapping.TryGetValue(this, out var res) ? res : GetMetaExpression(mapping);
    }
}
