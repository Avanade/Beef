// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching.Policy;
using NUnit.Framework;

namespace Beef.Core.UnitTest.Caching.Policy
{
    [TestFixture]
    public class MaxHitsCachePolicyTest
    {
        [Test]
        public void HasExpired()
        {
            var mhcp = new MaxHitsCachePolicy { MaxHits = 5 };
            Assert.IsFalse(((ICachePolicy)mhcp).HasExpired());
            Assert.IsFalse(((ICachePolicy)mhcp).HasExpired());
            Assert.IsFalse(((ICachePolicy)mhcp).HasExpired());
            Assert.IsFalse(((ICachePolicy)mhcp).HasExpired());
            Assert.IsFalse(((ICachePolicy)mhcp).HasExpired());
            Assert.AreEqual(5, mhcp.Hits);

            // Max hits so will expire; hits will continue to rise when HasExpired called.
            Assert.IsTrue(((ICachePolicy)mhcp).HasExpired());
            Assert.AreEqual(6, mhcp.Hits);
        }

        [Test]
        public void Expiry()
        {
            var mhcp = new MaxHitsCachePolicy { MaxHits = 2 };
            Assert.IsFalse(((ICachePolicy)mhcp).HasExpired());
            Assert.IsFalse(((ICachePolicy)mhcp).HasExpired());
            Assert.IsTrue(((ICachePolicy)mhcp).HasExpired());
            Assert.AreEqual(3, mhcp.Hits);

            // Still expired, still has hits recorded.
            mhcp.Refresh();
            Assert.IsTrue(mhcp.IsExpired);
            Assert.AreEqual(3, mhcp.Hits);

            // No longer expired and no hits.
            mhcp.Reset();
            Assert.IsFalse(mhcp.IsExpired);
            Assert.AreEqual(0, mhcp.Hits);
        }

        [Test]
        public void Clone()
        {
            var ccp = (MaxHitsCachePolicy)new MaxHitsCachePolicy { MaxHits = 2 }.Clone();
            Assert.AreEqual(2, ccp.MaxHits);
        }
    }
}
