using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;
using Fp;
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
        public void TestDecode() {
            new DecodeTestProcessor().Process(null);
        }

        private class DecodeTestProcessor : Processor {
            protected override void ProcessImpl(IReadOnlyList<string> args) {
                // Check basic UTF-8
                const string pontoonString = "pontoon";
                var pontoonArr = Encoding.UTF8.GetBytes(pontoonString);
                var pontoonRes = ReadUtf8String(pontoonArr);
                Assert.AreEqual(pontoonString, pontoonRes);

                // Check basic UTF-16 with null
                const string floaterinoString = "floaterino";
                const string floaterinoString2 = "floaterino\0planerino";
                var floaterinoArr = Encoding.Unicode.GetBytes(floaterinoString2);
                var floaterinoRes = ReadUtf16String(floaterinoArr);
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
                var buf2 = new byte[4 * png.Width * png.Height];
                var buf2S = buf2.AsSpan();
                unsafe {
                    for (var x = 0; x < png.Width; x++)
                    for (var y = 0; y < png.Height; y++) {
                        var pixel = png.GetPixel(x, y);
                        new Span<byte>((byte*) &pixel, 4).CopyTo(buf2S.Slice(4 * (y * png.Width + x)));
                    }
                }

                byte[] data2;
                byte[] data3;
                using (var ms = new MemoryStream()) {
                    WritePng(data, png.Width, png.Height, png.HasAlphaChannel, ms);
                    ms.Position = 0;
                    data2 = ReadPng(ms).Data.Data;
                    ms.SetLength(0);
                    WritePngPlainRgba(buf2S, png.Width, png.Height, CompressionLevel.Optimal, ms);
                    ms.Position = 0;
                    data3 = ReadPng(ms).Data.Data;
                }

                Assert.IsTrue(data.AsSpan().SequenceEqual(data2));
                Assert.IsTrue(data.AsSpan().SequenceEqual(data3));
            }
        }
    }
}