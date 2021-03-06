using System;

namespace Fp
{
    /// <summary>
    /// Utility functions for format conversion
    /// </summary>
    public static class DataUtil
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
                CommonFormat.Generic => "", // If appending extension, generic -> no change in extension
                CommonFormat.PngDeflate => ".png",
                CommonFormat.PcmWave => ".wav",
                CommonFormat.ExportUnsupported => "",
                CommonFormat.Jpeg => ".jpg",
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
            };
        }

        /// <summary>
        /// Copy <see cref="ArraySegment{T}"/> to new compact array
        /// </summary>
        /// <param name="segment">Data to copy</param>
        /// <typeparam name="T">Type</typeparam>
        /// <returns>Newly allocated array</returns>
        public static ArraySegment<T> CloneSegment<T>(this ArraySegment<T> segment)
        {
            T[] arr = new T[segment.Count];
            segment.AsSpan(0, arr.Length).CopyTo(arr);
            return new ArraySegment<T>(arr);
        }

        /// <summary>
        /// Clone buffer to newly allocated array
        /// </summary>
        /// <param name="memory">Memory to clone</param>
        /// <returns>New array</returns>
        public static Memory<T> CloneBuffer<T>(this ReadOnlyMemory<T> memory)
        {
            T[] target = new T[memory.Length];
            memory.CopyTo(target);
            return target;
        }
    }
}
