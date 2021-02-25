namespace Fp.Structures
{
    public abstract partial record Expression : Element
    {
        public abstract T Read<T>(StructureContext context);

        public virtual T ReadUnmanaged<T>(StructureContext context) where T : unmanaged =>
            Read<T>(context);
    }
}
