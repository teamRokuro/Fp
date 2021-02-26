using System;
using System.IO;
using System.Text;
using Fp.Sg.TestBase;
using NUnit.Framework;

namespace Fp.Tests
{
    public class StructureTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestModel1()
        {
            byte[] data = {4, 0, 0, 0, 8, 0, 0, 0, 1, 0, 0, 0, 2, 0, 0, 0, 0x11, 0x22, 0x33, 0x44};
            var ms = new MStream(new ReadOnlyMemory<byte>(data));
            var instance = Model1Instance.Read(ms);
            Assert.AreEqual(4, instance.Ref0);
            Assert.AreEqual(1, instance.Ref1);
            Assert.AreEqual(8, instance.Ref2);
            Assert.AreEqual(2, instance.Ref2_2);
            Assert.AreEqual(0x2211, instance.Ref3);
            Assert.AreEqual(0x4433, instance.Ref4);
            byte[] data2 = new byte[20];
            var ms2 = new MemoryStream(data2);
            instance.Write(ms2);
            Assert.AreEqual(data, data2);
        }

        [Test]
        public void TestModel2()
        {
            byte[] d2 = Encoding.UTF8.GetBytes("Hello there");
            var ms0 = new MemoryStream();
            ms0.WriteByte(0);
            ms0.Write(d2);
            byte[] data = ms0.ToArray();
            var ms = new MStream(new ReadOnlyMemory<byte>(data));
            var instance = Model2Instance.Read(ms);
            Assert.AreEqual("Hello there", instance.Ref0);
            byte[] data2 = new byte[data.Length];
            var ms2 = new MemoryStream(data2);
            instance.Write(ms2);
            Assert.AreEqual(data, data2);
        }
    }
}
