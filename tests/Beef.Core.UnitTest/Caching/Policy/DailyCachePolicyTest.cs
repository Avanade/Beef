// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching.Policy;
using NUnit.Framework;
using System;

namespace Beef.Core.UnitTest.Caching.Policy
{
    [TestFixture]
    public class DailyCachePolicyTest
    {
        [Test]
        public void Setup()
        {
            var now = DateTime.Now;
            var dcp = new DailyCachePolicy { Duration = new TimeSpan(1, 0, 0) };
            dcp.Reset();
            var dt = now.Date.AddHours(now.Hour + 1);
            Assert.AreEqual(dt, dcp.Expiry);

            now = DateTime.Now;
            dcp = new DailyCachePolicy { Duration = new TimeSpan(4, 0, 0) };
            dcp.Reset();
            dt = now.Date.AddHours(now.Hour + 4 - (now.Hour % 4));
            Assert.AreEqual(dt, dcp.Expiry);

            now = DateTime.Now;
            dcp = new DailyCachePolicy { Duration = new TimeSpan(24, 0, 0) };
            dcp.Reset();
            Assert.AreEqual(now.Date.AddDays(1), dcp.Expiry);
        }

        [Test]
        public void Setup_WithRandomizer()
        {
            var now = DateTime.Now;
            var dcp = new DailyCachePolicy { Duration = new TimeSpan(1, 0, 0), RandomizerOffset = new TimeSpan(0, 30, 0) };
            dcp.Reset();

            Assert.IsTrue(dcp.Expiry.HasValue);
            Assert.IsTrue(dcp.Expiry.Value > new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, 0, 0) && dcp.Expiry.Value < new DateTime(now.Year, now.Month, now.Day, now.Hour + 1, 0, 0).AddMinutes(30));
        }
    }
}
