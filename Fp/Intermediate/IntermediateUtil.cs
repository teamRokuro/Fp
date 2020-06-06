using System;

namespace Fp.Intermediate
{
    /// <summary>
    /// Utility functions for format conversion
    /// </summary>
    public static class IntermediateUtil
    {
        /// <summary>
        /// Get file extension for format
        /// </summary>
        /// <param name="format">Format to get extension of</param>
        /// <returns>File extension</returns>
        /// <exception cref="ArgumentOutOfRangeException">If no matching enum value exists</exception>
        public static string GetExtension(this CommonFormat format)
        {
            return format switch
            {
                CommonFormat.PngDeflate => ".png",
                CommonFormat.Generic => "", // If appending extension, generic -> no change in extension
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
            };
        }
    }
}
