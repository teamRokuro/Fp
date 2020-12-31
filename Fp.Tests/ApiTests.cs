using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fp.Ciphers.BlowFish;
using Fp.Intermediate;
using NUnit.Framework;

namespace Fp.Tests
{
    public class ApiTests
    {
        [Test]
        public void TestCircleBuffer()
        {
            // Test some operations with circlebuffer
            CircleBuffer<byte> cb = new CircleBuffer<byte>(100);
            Random r = new Random();
            byte[] a = new byte[60];
            r.NextBytes(a);
            foreach (byte b in a)
                cb.Add(b);
            List<byte> list = new List<byte>(a);
            Assert.IsTrue(cb.SequenceEqual(list));
            cb.RemoveAt(40);
            list.RemoveAt(40);
            Assert.IsTrue(cb.SequenceEqual(list));
            cb.RemoveAt(10);
            list.RemoveAt(10);
            Assert.IsTrue(cb.SequenceEqual(list));
            cb.Insert(5, 10);
            list.Insert(5, 10);
            Assert.IsTrue(cb.SequenceEqual(list));
            cb.Insert(50, 60);
            list.Insert(50, 60);
            Assert.IsTrue(cb.SequenceEqual(list));
        }

        [Test]
        public void TestMultiBufferStream()
        {
            // Test a bunch of random location reads
            Random r = new Random();
            byte[] a = new byte[4096];
            r.NextBytes(a);
            MemoryStream ms = new MemoryStream(a);
            MultiBufferStream mbs = new MultiBufferStream(ms, true, 8, 128);
            mbs.LargeReadOverride = false;
            byte[] temp = new byte[256];
            for (int i = 0; i < 128; i++)
            {
                int position = 16 * (r.Next() % 256);
                mbs.Position = position;
                int read = mbs.Read(temp, 0, 256);
                //Console.WriteLine($"{i} {position} {read}");
                Assert.AreEqual(new ArraySegment<byte>(a, position, read), new ArraySegment<byte>(temp, 0, read));
            }

            // Test full read
            mbs.Position = 0;
            MemoryStream ms2 = new MemoryStream();
            mbs.CopyTo(ms2);
            ms2.TryGetBuffer(out ArraySegment<byte> ms2b);
            Assert.AreEqual(new ArraySegment<byte>(a), ms2b);
        }

        [Test]
        public void TestNumberCast()
        {
            object number1 = 1;
            Assert.AreEqual(1, Data.CastNumber<object, byte>(number1));
            Assert.AreEqual(3841, Data.CastNumber<uint, short>(3841));
            Assert.AreEqual(17, Data.CastNumber<object, uint>("17"));
            Assert.AreEqual(1.0f, Data.CastNumber<object, float>("1.0"), 0.001f);
        }

        [Test]
        public void TestBlowfish()
        {
            byte[] data;
            byte[] tmp = File.ReadAllBytes("Watch_Dogs2020-4-3-0-57-53.png");
            data = new byte[Processor.GetPaddedLength(tmp.Length, Processor.PaddingMode.Zero, 8)];
            tmp.AsSpan().CopyTo(data);

            byte[] dataEnc = new byte[data.Length];
            Buffer.BlockCopy(data, 0, dataEnc, 0, data.Length);
            Blowfish bf = new Blowfish();
            var ptkey = Processor.DecodeHex("1010ffff");
            bf.SetBlankIv();
            bf.SetKey(ptkey);
            bf.EncryptCbc(dataEnc);
            bf.SetBlankIv();
            bf.SetKey(ptkey);
            bf.DecryptCbc(dataEnc);
            Assert.IsTrue(data.AsSpan().SequenceEqual(dataEnc));
            bf.SetKey(ptkey);
            bf.EncryptEcb(dataEnc);
            bf.SetKey(ptkey);
            bf.DecryptEcb(dataEnc);
            Assert.IsTrue(data.AsSpan().SequenceEqual(dataEnc));
        }
    }
}
