using Beef.Caching.Policy;
using NUnit.Framework;
using System;

namespace Beef.Core.UnitTest.Caching.Policy
{
    [TestFixture]
    public class CachePolicyConfigTest
    {
        [Test]
        public void SetCachePolicyManager()
        {
            var cpc = new CachePolicyConfig
            {
                Policies = new CachePolicyConfigPolicy[]
                {
                    new CachePolicyConfigPolicy
                    {
                        Name = "MyTest",
                        Policy = "Beef.Caching.Policy.SlidingCachePolicy, Beef.Core",
                        Properties = new CachePolicyConfigPolicyProperty[]
                        {
                            new CachePolicyConfigPolicyProperty { Name = "Duration", Value = "00:30:00" },
                            new CachePolicyConfigPolicyProperty { Name = "MaxDuration", Value = "02:00:00" },
                            new CachePolicyConfigPolicyProperty { Name = "RandomizerOffset", Value = "00:10:00" }
                        },
                        Caches = new string[] { "BlahX" }
                    }
                }
            };

            CachePolicyManager.SetFromCachePolicyConfig(cpc);

            var pol = CachePolicyManager.Get("BlahX");

            Assert.NotNull(pol);
            Assert.IsInstanceOf<SlidingCachePolicy>(pol);

            var scp = (SlidingCachePolicy)pol;
            Assert.AreEqual(new TimeSpan(00, 30, 00), scp.Duration);
            Assert.AreEqual(new TimeSpan(02, 00, 00), scp.MaxDuration);
            Assert.AreEqual(new TimeSpan(00, 10, 00), scp.RandomizerOffset);
        }
    }
}
