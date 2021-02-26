namespace Fp.Structures
{
    /// <summary>
    /// Represents an expression that corresponds to a writable location.
    /// </summary>
    public abstract record WritableExpression : Expression
    {
        /// <summary>
        /// Write the specified value to the target.
        /// </summary>
        /// <param name="context">Context to use.</param>
        /// <param name="value">Value to write.</param>
        /// <typeparam name="T">Type of value to write.</typeparam>
        public abstract void Write<T>(StructureContext context, T value);
    }
}
