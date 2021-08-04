// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching.Policy;
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
        private class TestRd : ReferenceDataBaseInt32
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
            UnitTest.Caching.Policy.CachePolicyManagerTest.TestSetUp();
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

        [Test]
        public void Exercise_SlidingCache()
        {
            UnitTest.Caching.Policy.CachePolicyManagerTest.TestSetUp();

            var coll = new TestRdCollection
                {
                    new TestRd { Id = 1, Code = "A" },
                    new TestRd { Id = 2, Code = "B" }
                };

            var policy = new SlidingCachePolicy
            {
                Duration = new TimeSpan(0, 0, 3),
                MaxDuration = new TimeSpan(0, 0, 4)
            };

            int dataRequests = 0;
            var rdc = new ReferenceDataCache<TestRdCollection, TestRd>(() => { dataRequests++; return Task.FromResult(coll); });

            CachePolicyManager.Current.Set(rdc.PolicyKey, policy);

            // Nothing loaded.
            Assert.AreEqual(0, dataRequests);
            Assert.AreEqual(0, rdc.Count);
            Assert.IsTrue(rdc.IsExpired);

            // Now loaded.
            var c = rdc.GetCollection();
            Assert.AreEqual(1, dataRequests);
            Assert.IsNotNull(c);
            Assert.AreEqual(2, c.ActiveList.Count);
            Assert.AreEqual(2, rdc.Count);
            Assert.IsFalse(rdc.IsExpired);

            // Multiple cycles to test Max cache
            for (int cycle = 0; cycle < 3; cycle++)
            {
                coll.Add(new TestRd { Id = 3 + cycle, Code = $"X{3 + cycle}" });

                // Running till max cache expires.
                while (!rdc.IsExpired)
                {
                    c = rdc.GetCollection();
                    Assert.AreEqual(1 + cycle, dataRequests);
                    Assert.IsNotNull(c);
                    Assert.AreEqual(3 + cycle, c.ActiveList.Count);
                    Assert.AreEqual(3 + cycle, rdc.Count);
                    Assert.IsFalse(rdc.IsExpired);
                    System.Threading.Thread.Sleep((int)Math.Floor(policy.Duration.TotalMilliseconds / 2));
                }

                // reload collection cached.
                c = rdc.GetCollection();
                Assert.AreEqual(2 + cycle, dataRequests);
                Assert.IsNotNull(c);
                Assert.AreEqual(3 + cycle, c.ActiveList.Count);
                Assert.AreEqual(3 + cycle, rdc.Count);
                Assert.IsFalse(rdc.IsExpired);

                // New collection cached.
                c = rdc.GetCollection();
                Assert.AreEqual(2 + cycle, dataRequests);
                Assert.IsNotNull(c);
                Assert.AreEqual(3 + cycle, c.ActiveList.Count);
                Assert.AreEqual(3 + cycle, rdc.Count);
                Assert.IsFalse(rdc.IsExpired);
            }
        }
    }
}