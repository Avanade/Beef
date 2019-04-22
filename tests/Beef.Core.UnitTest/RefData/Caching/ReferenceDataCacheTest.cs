// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using Beef.RefData.Caching;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Beef.Core.UnitTest.RefData.Caching
{
    [TestFixture]
    public class ReferenceDataCacheTest
    {
        private class TestRd : ReferenceDataBaseInt
        {
            public override object Clone()
            {
                throw new NotImplementedException();
            }
        }

        private class TestRdCollection : ReferenceDataCollectionBase<TestRd>
        { }

        [Test]
        public void Exercise()
        {
            var coll = new TestRdCollection
            {
                new TestRd { Id = 1, Code = "A" },
                new TestRd { Id = 2, Code = "B" }
            };

            int i = 0;
            var rdc = new ReferenceDataCache<TestRdCollection, TestRd>(() => { i++; return Task.FromResult(coll); });

            // Nothing loaded.
            Assert.AreEqual(0, i);
            Assert.AreEqual(0, rdc.Count);
            Assert.IsTrue(rdc.IsExpired);

            // Now loaded.
            var c = rdc.GetCollection();
            Assert.AreEqual(1, i);
            Assert.IsNotNull(c);
            Assert.AreEqual(2, c.ActiveList.Count);
            Assert.AreEqual(2, rdc.Count);
            Assert.IsFalse(rdc.IsExpired);

            // Same cached version.
            c = rdc.GetCollection();
            Assert.AreEqual(1, i);
            Assert.IsNotNull(c);
            Assert.AreEqual(2, c.ActiveList.Count);
            Assert.AreEqual(2, rdc.Count);
            Assert.IsFalse(rdc.IsExpired);

            // Expire the cache; entries remain.
            rdc.GetPolicy().Refresh();
            Assert.AreEqual(1, i);
            Assert.IsTrue(rdc.IsExpired);
            Assert.AreEqual(2, rdc.Count);

            // New collection cached.
            c = rdc.GetCollection();
            Assert.AreEqual(2, i);
            Assert.IsNotNull(c);
            Assert.AreEqual(2, c.ActiveList.Count);
            Assert.AreEqual(2, rdc.Count);
            Assert.IsFalse(rdc.IsExpired);
        }
    }
}
