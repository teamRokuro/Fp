using System.IO;

namespace Fp.Helpers {
    /// <summary>
    /// Base data helper.
    /// </summary>
    public abstract record Helper
    {
        /// <summary>
        /// Current input stream.
        /// </summary>
        public abstract Stream InputStream { get; }

        /// <summary>
        /// Current output stream.
        /// </summary>
        public abstract Stream OutputStream { get; }
    }
}
