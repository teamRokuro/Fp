using System;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;
using NUnit.Framework;
using Aes = System.Security.Cryptography.Aes;

namespace Fp.Tests
{
    public class ProcessorTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestSearch()
        {
            byte[] arr = Encoding.UTF8.GetBytes("word");
            const int count = 10;
            var ms = new MemoryStream();
            for (int i = 0; i < count; i++)
                ms.Write(arr, 0, arr.Length);
            int next = 0;
            int mCount = 0;
            foreach (long match in Processor.Match(ms, 0, ms.Length, arr.AsMemory(), 0, arr.Length))
            {
                Assert.AreEqual(next, match);
                next += arr.Length;
                mCount++;
            }

            Assert.AreEqual(count, mCount);
        }

        [Test]
        public void TestStrings()
        {
            var processor = new Processor();
            var ms = new MemoryStream();

            // Check basic UTF-8
            const string pontoonString = "pontoon";
            processor.WriteUtf8String(pontoonString, true, ms);
            byte[] pontoonArr = ms.ToArray();
            ms.SetLength(0);
            string pontoonRes = Processor.ReadUtf8String(pontoonArr.AsSpan(), out int pontoonResL, out _);
            Assert.AreEqual(pontoonString, pontoonRes);
            Assert.AreEqual(7 + 1, pontoonResL);

            // Check basic UTF-8 with null
            const string floaterinoString = "floaterino";
            const string floaterinoString2 = "floaterino\0planerino";
            processor.WriteUtf8String(floaterinoString2, true, ms);
            byte[] floaterinoArr = ms.ToArray();
            ms.SetLength(0);
            string floaterinoRes = Processor.ReadUtf8String(floaterinoArr.AsSpan(), out int floaterinoResL, out _);
            Assert.AreEqual(floaterinoString, floaterinoRes);
            Assert.AreEqual(10 + 1, floaterinoResL);

            // Check basic UTF-16
            const string hailHydraString = "hail hydra";
            byte[] hailHydraArr = Encoding.Unicode.GetBytes(hailHydraString);
            Assert.AreEqual(10 * 2, hailHydraArr.Length);
            string hailHydraRes = Processor.ReadUtf16String(hailHydraArr.AsSpan(), out int hailHydraResL, out _);
            Assert.AreEqual(hailHydraString, hailHydraRes);
            Assert.AreEqual(10 * 2, hailHydraResL);

            // Check basic UTF-16 with null
            const string hydraString = "hydra";
            const string hydraString2 = "hydra\0hemert";
            byte[] hydraArr = Encoding.Unicode.GetBytes(hydraString2);
            string hydraRes = Processor.ReadUtf16String(hydraArr.AsSpan(), out int hydraResL, out _);
            Assert.AreEqual(hydraString, hydraRes);
            Assert.AreEqual(6 * 2, hydraResL);

            // Check UTF-16LE + bom
            const string hailHydraString2 = "hail hydra";
            processor.WriteUtf16String(hailHydraString2, true, false, true, ms);
            byte[] hailHydra2Arr = ms.ToArray();
            ms.SetLength(0);
            string hailHydra2Res = Processor.ReadUtf16String(hailHydra2Arr.AsSpan(), out int hailHydra2ResL, out _);
            Assert.AreEqual(hailHydraString2, hailHydra2Res);
            Assert.AreEqual(10 * 2 + 2, hailHydra2ResL);

            // Check UTF-16BE + bom
            const string hailHydraString3 = "hail hydra";
            processor.WriteUtf16String(hailHydraString3, true, true, true, ms);
            byte[] hailHydra3Arr = ms.ToArray();
            ms.SetLength(0);
            string hailHydra3Res = Processor.ReadUtf16String(hailHydra3Arr.AsSpan(), out int hailHydra3ResL, out _);
            Assert.AreEqual(hailHydraString3, hailHydra3Res);
            Assert.AreEqual(10 * 2 + 2, hailHydra3ResL);

            // Check UTF-16LE + bom from stream
            const string hailHydraString4 = "hail hydra";
            processor.WriteUtf16String(hailHydraString4, true, false, true, ms);
            ms.Position = 0;
            string hailHydra4Res = processor.ReadUtf16String(ms, out int hailHydra4ResL, out _);
            Assert.AreEqual(hailHydraString4, hailHydra4Res);
            Assert.AreEqual(hailHydraString4, hailHydra4Res);
            Assert.AreEqual(10 * 2 + 2, hailHydra4ResL);
        }

        [Test]
        public void TestJoin()
        {
            Assert.AreEqual("/A/B/C/D/", Processor.Join(false, "/A", "/B", "C/", "/D/"));
            Assert.AreEqual("/A/B", Processor.Join(false, "/A", "B"));
            Assert.AreEqual("/A/B", Processor.Join(false, "/A", "", "", "B"));
            Assert.AreEqual(@"/Source/Path/\x/", Processor.Join(false, "/Source", "/Path/", @"\x/"));
            Assert.AreEqual(@"C:\A\B\C\D\E", Processor.Join(true, @"C:\A", @"\B", @"C\", @"\D\", "E"));
            Assert.AreEqual(@"C:\A\B", Processor.Join(true, @"C:\A", "B"));
            Assert.AreEqual(@"C:\A/B", Processor.Join(true, @"C:\A", "/B"));
        }

        [Test]
        public void TestAesEcb()
        {
            byte[] data = new byte[128 / 8 * 5];
            var ms1 = new MemoryStream();

            using var aesAlg = Aes.Create() ?? throw new ApplicationException();
            aesAlg.Mode = CipherMode.ECB;
            aesAlg.Padding = PaddingMode.PKCS7;
            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            using var csEncrypt = new CryptoStream(ms1, encryptor, CryptoStreamMode.Write);
            csEncrypt.Write(data);
            csEncrypt.Flush();

            byte[] encData = ms1.ToArray();
            Processor.DecryptAesEcb(encData, aesAlg.Key);
            int pos = Processor.GetDepaddedLength(encData, Processor.PaddingMode.Pkcs7);
            Assert.AreEqual(data.Length, pos);
            Assert.IsTrue(data.AsSpan().SequenceEqual(encData));
        }

        [Test]
        public void TestAesCbc()
        {
            byte[] data = new byte[128 / 8 * 5];
            var ms1 = new MemoryStream();

            using var aesAlg = Aes.Create() ?? throw new ApplicationException();
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            using var csEncrypt = new CryptoStream(ms1, encryptor, CryptoStreamMode.Write);
            csEncrypt.Write(data);
            csEncrypt.Flush();

            byte[] encData = ms1.ToArray();
            Processor.DecryptAesCbc(encData, aesAlg.Key, aesAlg.IV);
            int pos = Processor.GetDepaddedLength(encData, Processor.PaddingMode.Pkcs7);
            Assert.AreEqual(data.Length, pos);
            Assert.IsTrue(data.AsSpan().SequenceEqual(encData));
        }

        [Test]
        public void TestIntrinsicsArm()
        {
            if (!AdvSimd.IsSupported) Assert.Inconclusive("AdvSimd intrinsics not supported");

            #region Xor

            Span<byte> arr = new byte[1097];
            Span<byte> arr2 = new byte[arr.Length];
            arr.CopyTo(arr2);

            const byte xor = 48;

            Processor.ApplyXorArm(arr, xor);
            Processor.ApplyXorFallback(arr2, xor);

            Assert.IsTrue(arr.SequenceEqual(arr2));

            // Cut somewhere in 0..31 for misalignment
            Span<byte> arr3 = arr.Slice(14);
            Span<byte> arr4 = arr2.Slice(14);

            const byte xor2 = 93;

            Processor.ApplyXorArm(arr3, xor2);
            Processor.ApplyXorFallback(arr4, xor2);

            Assert.IsTrue(arr3.SequenceEqual(arr4));

            #endregion
        }

        [Test]
        public void TestIntrinsicsSse2()
        {
            if (!Sse2.IsSupported) Assert.Inconclusive("Sse2 intrinsics not supported");

            #region Xor

            Span<byte> arr = new byte[1097];
            Span<byte> arr2 = new byte[arr.Length];
            Processor p = new();
            p.GetU8(arr);
            arr.CopyTo(arr2);

            const byte xor = 48;

            Processor.ApplyXorSse2(arr, xor);
            Processor.ApplyXorFallback(arr2, xor);

            Assert.IsTrue(arr.SequenceEqual(arr2));

            // Cut somewhere in 0..31 for misalignment
            Span<byte> arr3 = arr.Slice(14);
            Span<byte> arr4 = arr2.Slice(14);

            const byte xor2 = 93;

            Processor.ApplyXorSse2(arr3, xor2);
            Processor.ApplyXorFallback(arr4, xor2);

            Assert.IsTrue(arr3.SequenceEqual(arr4));

            #endregion
        }

        [Test]
        public void TestIntrinsicsAvx2()
        {
            if (!Avx2.IsSupported) Assert.Inconclusive("Avx2 intrinsics not supported");

            #region Xor

            Span<byte> arr = new byte[1097];
            Span<byte> arr2 = new byte[arr.Length];
            arr.CopyTo(arr2);

            const byte xor = 48;

            Processor.ApplyXorAvx2(arr, xor);
            Processor.ApplyXorFallback(arr2, xor);

            Assert.IsTrue(arr.SequenceEqual(arr2));

            // Cut somewhere in 0..31 for misalignment
            Span<byte> arr3 = arr.Slice(14);
            Span<byte> arr4 = arr2.Slice(14);

            const byte xor2 = 93;

            Processor.ApplyXorAvx2(arr3, xor2);
            Processor.ApplyXorFallback(arr4, xor2);

            Assert.IsTrue(arr3.SequenceEqual(arr4));

            #endregion
        }
    }
}
