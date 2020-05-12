using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using Fp.Images.Png;
using NUnit.Framework;

namespace Fp.Tests {
    public class ProcessorTests {
        [SetUp]
        public void Setup() {
        }

        [Test]
        public void TestSearch() {
            new SearchTestProcessor().Process(null);
        }

        private class SearchTestProcessor : Processor {
            protected override void ProcessImpl(IReadOnlyList<string> args) {
                var arr = Encoding.UTF8.GetBytes("word");
                const int count = 10;
                var ms = new MemoryStream();
                for (var i = 0; i < count; i++)
                    ms.Write(arr, 0, arr.Length);
                var next = 0;
                var mCount = 0;
                foreach (var match in Match(ms, 0, ms.Length, arr, 0, arr.Length)) {
                    Assert.AreEqual(next, match);
                    next += arr.Length;
                    mCount++;
                }

                Assert.AreEqual(count, mCount);
            }
        }

        [Test]
        public void TestStrings() {
            new StringTestProcessor().Process(null);
        }

        private class StringTestProcessor : Processor {
            protected override void ProcessImpl(IReadOnlyList<string> args) {
                var ms = new MemoryStream();
                
                // Check basic UTF-8
                const string pontoonString = "pontoon";
                WriteUtf8String(pontoonString, true, ms);
                var pontoonArr = ms.ToArray();
                ms.SetLength(0);
                var pontoonRes = ReadUtf8String(pontoonArr);
                Assert.AreEqual(pontoonString, pontoonRes);

                // Check basic UTF-8 with null
                const string floaterinoString = "floaterino";
                const string floaterinoString2 = "floaterino\0planerino";
                WriteUtf8String(floaterinoString2, true, ms);
                var floaterinoArr = ms.ToArray();
                ms.SetLength(0);
                var floaterinoRes = ReadUtf8String(floaterinoArr);
                Assert.AreEqual(floaterinoString, floaterinoRes);

                // Check basic UTF-16
                const string hailHydraString = "hail hydra";
                var hailHydraArr = Encoding.Unicode.GetBytes(hailHydraString);
                var hailHydraRes = ReadUtf16String(hailHydraArr);
                Assert.AreEqual(hailHydraString, hailHydraRes);

                // Check basic UTF-16 with null
                const string hydraString = "hydra";
                const string hydraString2 = "hydra\0hemert";
                var hydraArr = Encoding.Unicode.GetBytes(hydraString2);
                var hydraRes = ReadUtf16String(hydraArr);
                Assert.AreEqual(hydraString, hydraRes);

                // Check UTF-16LE + bom
                const string hailHydraString2 = "hail hydra";
                WriteUtf16String(hailHydraString2, true, false, true, ms);
                var hailHydra2Arr = ms.ToArray();
                ms.SetLength(0);
                var hailHydra2Res = ReadUtf16String(hailHydra2Arr);
                Assert.AreEqual(hailHydraString2, hailHydra2Res);

                // Check UTF-16BE + bom
                const string hailHydraString3 = "hail hydra";
                WriteUtf16String(hailHydraString3, true, true, true, ms);
                var hailHydra3Arr = ms.ToArray();
                ms.SetLength(0);
                var hailHydra3Res = ReadUtf16String(hailHydra3Arr);
                Assert.AreEqual(hailHydraString3, hailHydra3Res);
                
                // Check UTF-16LE + bom from stream
                const string hailHydraString4 = "hail hydra";
                WriteUtf16String(hailHydraString4, true, false, true, ms);
                ms.Position = 0;
                var hailHydra4Res = ReadUtf16String(ms);
                Assert.AreEqual(hailHydraString4, hailHydra4Res);
                Assert.AreEqual(hailHydraString4, hailHydra4Res);
            }
        }

        [Test]
        public void TestBasicJoin() {
            Assert.AreEqual("/A/B/C/D/", Processor.BasicJoin(false, "/A", "/B", "C/", "/D/"));
            Assert.AreEqual("/A/B", Processor.BasicJoin(false, "/A", "B"));
            Assert.AreEqual("/A/B", Processor.BasicJoin(false, "/A", "", "", "B"));
            Assert.AreEqual(@"/Source/Path/\x/", Processor.BasicJoin(false, "/Source", "/Path/", @"\x/"));
            Assert.AreEqual(@"C:\A\B\C\D\E", Processor.BasicJoin(true, @"C:\A", @"\B", @"C\", @"\D\", "E"));
            Assert.AreEqual(@"C:\A\B", Processor.BasicJoin(true, @"C:\A", "B"));
            Assert.AreEqual(@"C:\A/B", Processor.BasicJoin(true, @"C:\A", "/B"));
        }

        [Test]
        public void TestPng() {
            new PngTestProcessor().Process(null);
        }

        private class PngTestProcessor : Processor {
            protected override void ProcessImpl(IReadOnlyList<string> args) {
                Png png;
                using (var fs = File.OpenRead("Watch_Dogs2020-4-3-0-57-53.png"))
                    png = ReadPng(fs);
                var data = png.Data.Data;
                var buf2 = new uint[png.Width * png.Height];
                var buf2S = buf2.AsSpan();
                unsafe {
                    for (var x = 0; x < png.Width; x++)
                    for (var y = 0; y < png.Height; y++) {
                        var pixel = png.GetPixel(x, y);
                        buf2S[y * png.Width + x] = *(uint*) &pixel;
                    }
                }

                byte[] data3;
                using (var ms = new MemoryStream()) {
                    ms.SetLength(0);
                    WritePngRgba(buf2S, png.Width, png.Height, CompressionLevel.Optimal, ms);
                    ms.Position = 0;
                    data3 = ReadPng(ms).Data.Data;
                }

                Assert.IsTrue(data.AsSpan().SequenceEqual(data3));
            }
        }

        [Test]
        public void TestAesEcb() {
            var data = new byte[128 / 8 * 5];
            var ms1 = new MemoryStream();

            using var aesAlg = Aes.Create() ?? throw new ApplicationException();
            aesAlg.Mode = CipherMode.ECB;
            aesAlg.Padding = PaddingMode.PKCS7;
            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            using var csEncrypt = new CryptoStream(ms1, encryptor, CryptoStreamMode.Write);
            csEncrypt.Write(data);
            csEncrypt.Flush();

            var encData = ms1.ToArray();
            Processor.DecryptAesEcb(encData, aesAlg.Key);
            var pos = Processor.GetDepaddedLength(encData, Processor.PaddingMode.Pkcs7);
            Assert.AreEqual(data.Length, pos);
            Assert.IsTrue(data.AsSpan().SequenceEqual(encData));
        }

        [Test]
        public void TestAesCbc() {
            var data = new byte[128 / 8 * 5];
            var ms1 = new MemoryStream();

            using var aesAlg = Aes.Create() ?? throw new ApplicationException();
            aesAlg.Mode = CipherMode.CBC;
            aesAlg.Padding = PaddingMode.PKCS7;
            var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            using var csEncrypt = new CryptoStream(ms1, encryptor, CryptoStreamMode.Write);
            csEncrypt.Write(data);
            csEncrypt.Flush();

            var encData = ms1.ToArray();
            Processor.DecryptAesCbc(encData, aesAlg.Key, aesAlg.IV);
            var pos = Processor.GetDepaddedLength(encData, Processor.PaddingMode.Pkcs7);
            Assert.AreEqual(data.Length, pos);
            Assert.IsTrue(data.AsSpan().SequenceEqual(encData));
        }
    }
}