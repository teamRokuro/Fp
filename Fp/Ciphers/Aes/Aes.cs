using System;
using System.Security.Cryptography;

// ReSharper disable once CheckNamespace
namespace Fp
{
    public partial class Processor
    {
        /// <summary>
        /// Decrypt with Aes using ECB mode and key
        /// </summary>
        /// <param name="src">Source span</param>
        /// <param name="key">Cipher key</param>
        public static unsafe void DecryptAesEcb(Span<byte> src, Span<byte> key)
        {
            using Aes aes = Aes.Create() ?? throw new ApplicationException();
            aes.Key = key.ToArray();
            aes.Padding = System.Security.Cryptography.PaddingMode.None;
            aes.Mode = CipherMode.ECB;
            ICryptoTransform decryptor = aes.CreateDecryptor();
            fixed (byte* p = &src.GetPinnableReference())
            {
                using PStream ps = new(new IntPtr(p), src.Length);
                using PStream ps2 = new(new IntPtr(p), src.Length);
                using CryptoStream cs = new(ps, decryptor, CryptoStreamMode.Read);
                cs.CopyTo(ps2);
            }
        }

        /// <summary>
        /// Decrypt with Aes using CBC mode and key/IV
        /// </summary>
        /// <param name="src">Source span</param>
        /// <param name="key">Cipher key</param>
        /// <param name="iv">IV (CBC/CTR)</param>
        public static unsafe void DecryptAesCbc(Span<byte> src, Span<byte> key, Span<byte> iv = default)
        {
            using Aes aes = Aes.Create() ?? throw new ApplicationException();
            aes.Key = key.ToArray();
            aes.Padding = System.Security.Cryptography.PaddingMode.None;
            aes.Mode = CipherMode.CBC;
            aes.IV = iv == default ? new byte[128 / 8] : iv.ToArray();
            ICryptoTransform decryptor = aes.CreateDecryptor();
            fixed (byte* p = &src.GetPinnableReference())
            {
                using PStream ps = new(new IntPtr(p), src.Length);
                using PStream ps2 = new(new IntPtr(p), src.Length);
                using CryptoStream cs = new(ps, decryptor, CryptoStreamMode.Read);
                cs.CopyTo(ps2);
            }
        }
    }
}
