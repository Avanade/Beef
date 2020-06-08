// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching.Policy;
using NUnit.Framework;
using System;
using System.Threading;

namespace Beef.Core.UnitTest.Caching.Policy
{
    [TestFixture]
    public class SlidingCachePolicyTest
    {
        [Test]
        public void Slide()
        {
            var scp = new SlidingCachePolicy { Duration = new TimeSpan(0, 0, 1) };
            scp.Reset();

            Assert.IsFalse(((ICachePolicy)scp).HasExpired());
            Thread.Sleep(500);
            Assert.IsFalse(((ICachePolicy)scp).HasExpired());
            Thread.Sleep(500);
            Assert.IsFalse(((ICachePolicy)scp).HasExpired());
            Thread.Sleep(500);
            Assert.IsFalse(((ICachePolicy)scp).HasExpired());
            Thread.Sleep(1500);
            Assert.IsTrue(scp.IsExpired);
        }

        [Test]
        public void Refresh()
        {
            var scp = new SlidingCachePolicy { Duration = new TimeSpan(0, 0, 1) };
            scp.Reset();

            Assert.IsFalse(((ICachePolicy)scp).HasExpired());
            Thread.Sleep(500);
            Assert.IsFalse(((ICachePolicy)scp).HasExpired());
            Thread.Sleep(500);

            scp.Refresh();
            Assert.IsTrue(((ICachePolicy)scp).HasExpired());

            Thread.Sleep(500);
            Assert.IsTrue(((ICachePolicy)scp).HasExpired());
        }

        [Test]
        public void Setup_WithRandomizer()
        {
            var now = Beef.Entities.Cleaner.Clean(DateTime.Now);
            var scp = new SlidingCachePolicy { Duration = new TimeSpan(0, 1, 0), RandomizerOffset = new TimeSpan(0, 0, 30) };
            scp.Reset();

            Assert.IsTrue(scp.Expiry.HasValue);
            Assert.IsTrue(scp.Expiry.Value > now.AddSeconds(60) && scp.Expiry.Value < now.AddSeconds(90));
        }
    }
}
