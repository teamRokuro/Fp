using System.Diagnostics.CodeAnalysis;

namespace Fp
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public partial class Processor
    {
        #region Pattern matching utilities

        /// <summary>
        /// Get byte array from string assuming 8-bit characters.
        /// </summary>
        /// <param name="text">String to process.</param>
        /// <returns>Byte array containing lower byte of each code unit in the string.</returns>
        public static byte[] Ascii(string text)
        {
            char[] arr = text.ToCharArray();
            int l = arr.Length;
            byte[] result = new byte[l];
            for (int i = 0; i < l; i++) result[i] = (byte)arr[i];
            return result;
        }

        #endregion
    }
}
