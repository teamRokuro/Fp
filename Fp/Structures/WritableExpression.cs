namespace Fp.Structures
{
    public abstract record WritableExpression : Expression
    {
        public abstract void Write<T>(StructureContext context, T value);
    }
}
