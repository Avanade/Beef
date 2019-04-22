// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using NUnit.Framework;

namespace Beef.Core.UnitTest.Core
{
    [TestFixture]
    public class FactoryTest
    {
        private class Test : IETag
        {
            public string ETag { get; set; }
        }

        [Test]
        public void LocalSet1()
        {
            Factory.SetLocal<IETag>(new Test { ETag = "XXX" });
            var val = Factory.Create<IETag>();
            Assert.IsNotNull(val);
            Assert.AreEqual("XXX", val.ETag);
        }

        [Test]
        public void LocalSet2()
        {
            Factory.SetLocal<IETag>(new Test { ETag = "YYY" });
            var val = Factory.Create<IETag>();
            Assert.IsNotNull(val);
            Assert.AreEqual("YYY", val.ETag);
        }

        [Test]
        public void LocalSet3()
        {
            Factory.SetLocal<IETag>(new Test { ETag = "ZZZ" });
            var val = Factory.Create<IETag>();
            Assert.IsNotNull(val);
            Assert.AreEqual("ZZZ", val.ETag);
        }
    }
}
