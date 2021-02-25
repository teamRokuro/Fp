using System.IO;
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
        public void TestMain()
        {
            byte[] data = {0, 0, 0, 0, 8, 0, 0, 0, 1, 0, 0, 0};
            var ms = new MemoryStream(data);
            ms.Position = 0;
            var instance = Model1Instance.Read(ms);
            Assert.AreEqual(4, instance.Value1);
            Assert.AreEqual(1, instance.Ref1);
            Assert.AreEqual(8, instance.Ref2);
            byte[] data2 = new byte[12];
            var ms2 = new MemoryStream(data2);
            instance.Write(ms2);
            Assert.AreEqual(data, data2);
        }
    }
}
