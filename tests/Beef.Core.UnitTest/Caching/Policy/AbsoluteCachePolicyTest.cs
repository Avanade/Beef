// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching.Policy;
using NUnit.Framework;
using System;
using System.Threading;

namespace Beef.Core.UnitTest.Caching.Policy
{
    [TestFixture]
    public class AbsoluteCachePolicyTest
    {
        [Test]
        public void HasExpired()
        {
            var acp = new AbsoluteCachePolicy { Duration = new TimeSpan(0, 0, 1) };

            acp.Reset();
            Assert.IsTrue(acp.Expiry <= Beef.Entities.Cleaner.Clean(DateTime.Now).AddMilliseconds(1000));

            Assert.IsFalse(((ICachePolicy)acp).HasExpired());
            Assert.IsFalse(acp.IsExpired);
            Thread.Sleep(750);
            Assert.IsFalse(((ICachePolicy)acp).HasExpired());
            Assert.IsFalse(acp.IsExpired);
            Thread.Sleep(750);
            Assert.IsTrue(((ICachePolicy)acp).HasExpired());
            Assert.IsTrue(acp.IsExpired);
            Thread.Sleep(750);
            Assert.IsTrue(((ICachePolicy)acp).HasExpired());
            Assert.IsTrue(acp.IsExpired);

            acp.Reset();
            Assert.IsTrue(acp.Expiry <= Beef.Entities.Cleaner.Clean(DateTime.Now).AddMilliseconds(1000));
        }

        [Test]
        public void Setup_WithRandomizer()
        {
            var now = Beef.Entities.Cleaner.Clean(DateTime.Now);
            var acp = new AbsoluteCachePolicy { Duration = new TimeSpan(0, 1, 0), RandomizerOffset = new TimeSpan(0, 30, 0) };
            acp.Reset();

            Assert.IsTrue(acp.Expiry.HasValue);
            Assert.IsTrue(acp.Expiry.Value > now && acp.Expiry.Value < now.AddMinutes(31));
        }
    }
}
