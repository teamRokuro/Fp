using System;
using System.IO;

namespace Fp.Helpers
{
    /// <summary>
    /// Base single-unit data helper.
    /// </summary>
    /// <typeparam name="T">Element type.</typeparam>
    public abstract unsafe record BaseUnmanagedHelper<T> : BaseHelper<T> where T : unmanaged
    {
        /// <inheritdoc />
        public override T this[long offset, Stream stream]
        {
            get
            {
                Span<byte> span = stackalloc byte[sizeof(T)];
                if (offset != -1) Processor.Read(stream, offset, span, false);
                else Processor.Read(stream, span, false);
                return this[span];
            }
            set
            {
                Span<byte> span = stackalloc byte[sizeof(T)];
                this[span] = value;
                if (offset != -1) Processor.Write(stream, offset, span);
                else Processor.Write(stream, span);
            }
        }
    }
}
