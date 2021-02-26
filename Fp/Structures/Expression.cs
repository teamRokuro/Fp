using System.Collections.Generic;
using Fp.Intermediate;

namespace Fp.Structures
{
    /// <summary>
    /// Represents an element of a structure that can be evaluated.
    /// </summary>
    public abstract partial record Expression : Element
    {
        /// <summary>
        /// Read the expression as the specified type.
        /// </summary>
        /// <param name="context">Context to use.</param>
        /// <typeparam name="T">Type to evaluate as.</typeparam>
        /// <returns>Evaluated expression.</returns>
        public abstract T? Read<T>(StructureContext context);
        /// <summary>
        /// Read the expression as the specified type.
        /// </summary>
        /// <param name="context">Context to use.</param>
        /// <typeparam name="T">Type to evaluate as.</typeparam>
        /// <returns>Evaluated expression.</returns>
        public virtual T ReadUnmanaged<T>(StructureContext context) where T : unmanaged =>
            Data.CastNumber<object, T>(Read<object>(context)!);

        /// <summary>
        /// Get expression with replacements from provided dictionary. Does not replace self.
        /// </summary>
        /// <param name="mapping">Replacements.</param>
        /// <returns>Expression with replacements.</returns>
        public virtual Expression GetMetaExpression(IReadOnlyDictionary<Element, Expression> mapping) =>
            this;

        /// <summary>
        /// Get expression with replacements from provided dictionary. Replaces self.
        /// </summary>
        /// <param name="mapping">Replacements.</param>
        /// <returns>Expression with replacements.</returns>
        public Expression GetSelfMetaExpression(IReadOnlyDictionary<Element, Expression> mapping) =>
            mapping.TryGetValue(this, out var res) ? res : GetMetaExpression(mapping);
    }
}
