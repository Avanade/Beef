// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching;
using Beef.Caching.Policy;
using NUnit.Framework;
using System;
using System.Linq;

namespace Beef.Core.UnitTest.Caching
{
    [TestFixture]
    public class MultiTenantCacheTest
    {
        public static Guid ToGuid(int value)
        {
            return new Guid(value, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
        }

        [Test]
        public void CreateAndGetWithInstanceFlush()
        {
            Policy.CachePolicyManagerTest.TestSetUp();
            CachePolicyManager.Current.IsInternalTracingEnabled = true;

            // Initialize the cache.
            var mtc = new MultiTenantCache<KeyValueCache<int, string>>((g, p) =>
            {
                if (g.Equals(ToGuid(1)))
                    return new KeyValueCache<int, string>(p, (k) => k.ToString());
                else
                    return new KeyValueCache<int, string>(p, (k) => "X" + k.ToString());
            }, "MultiTenantCacheTest");

            // Check the internal nocachepolicy.
            var pa = CachePolicyManager.Current.GetPolicies();
            Assert.AreEqual(1, pa.Length);
            Assert.IsTrue(pa[0].Key.StartsWith("MultiTenantCacheTest_"));
            Assert.IsInstanceOf(typeof(NoCachePolicy), pa[0].Value);

            // Check the default policy for type.
            Assert.AreEqual(mtc.PolicyKey, "MultiTenantCacheTest");
            Assert.IsInstanceOf(typeof(NoExpiryCachePolicy), mtc.GetPolicy());

            // Check that the cache is empty.
            Assert.IsFalse(mtc.Contains(ToGuid(1)));
            Assert.AreEqual(0, mtc.Count);
            mtc.Remove(ToGuid(1));

            // Check the first tenant.
            var kvc1 = mtc.GetCache(ToGuid(1));
            Assert.IsNotNull(kvc1);
            Assert.AreEqual("1", kvc1[1]);
            Assert.AreEqual(1, mtc.Count);

            // No new PolicyManager policies should be created.
            pa = CachePolicyManager.Current.GetPolicies();
            Assert.AreEqual(2, pa.Length);

            // Check the second tenant.
            var kvc2 = mtc.GetCache(ToGuid(2));
            Assert.IsNotNull(kvc2);
            Assert.AreEqual("X1", kvc2[1]);
            Assert.AreEqual(2, mtc.Count);

            // Flush the cache - nothing should happen as they never expire.
            mtc.Flush();
            Assert.AreEqual(2, mtc.Count);

            // Remove a tenant.
            mtc.Remove(ToGuid(2));
            Assert.IsTrue(mtc.Contains(ToGuid(1)));
            Assert.IsFalse(mtc.Contains(ToGuid(2)));
            Assert.AreEqual(1, mtc.Count);
            Assert.AreEqual(0, kvc2.Count);

            // Force flush the cache - should be removed.
            mtc.Flush(true);
            Assert.IsFalse(mtc.Contains(ToGuid(1)));
            Assert.IsFalse(mtc.Contains(ToGuid(2)));
            Assert.AreEqual(0, mtc.Count);
            Assert.AreEqual(0, kvc1.Count);
        }

        [Test]
        public void CreateAndGetWithForceFlush()
        {
            Policy.CachePolicyManagerTest.TestSetUp();
            CachePolicyManager.Current.IsInternalTracingEnabled = true;

            // Initialize the cache.
            var mtc = new MultiTenantCache<KeyValueCache<int, string>>((g, p) =>
            {
                if (g.Equals(ToGuid(1)))
                    return new KeyValueCache<int, string>(p, (k) => k.ToString());
                else
                    return new KeyValueCache<int, string>(p, (k) => "X" + k.ToString());
            }, "MultiTenantCacheTest");

            var pa = CachePolicyManager.Current.GetPolicies();
            Assert.AreEqual(1, pa.Length);

            // Check the internal nocachepolicy.
            var p0 = pa.Where(x => x.Key.StartsWith("MultiTenantCacheTest_")).SingleOrDefault();
            Assert.IsNotNull(p0);
            Assert.IsInstanceOf(typeof(NoCachePolicy), p0.Value);

            // Check that the cache is empty.
            Assert.IsFalse(mtc.Contains(ToGuid(1)));
            Assert.AreEqual(0, mtc.Count);
            mtc.Remove(ToGuid(1));

            // Check the first tenant.
            var kvc1 = mtc.GetCache(ToGuid(1));
            Assert.IsNotNull(kvc1);
            Assert.AreEqual("1", kvc1[1]);
            Assert.AreEqual(1, mtc.Count);

            // Check the default policy for type.
            pa = CachePolicyManager.Current.GetPolicies();
            Assert.AreEqual(2, pa.Length);

            var p1 = pa.Where(x => x.Key == "MultiTenantCacheTest").SingleOrDefault();
            Assert.IsNotNull(p1);
            Assert.IsInstanceOf(typeof(NoExpiryCachePolicy), p1.Value);

            // Check the second tenant.
            var kvc2 = mtc.GetCache(ToGuid(2));
            Assert.IsNotNull(kvc2);
            Assert.AreEqual("X1", kvc2[1]);
            Assert.AreEqual(2, mtc.Count);

            // No new PolicyManager policies should be created.
            pa = CachePolicyManager.Current.GetPolicies();
            Assert.AreEqual(2, pa.Length);

            // Flush the cache - nothing should happen as they never expire.
            CachePolicyManager.Current.Flush();
            Assert.AreEqual(2, mtc.Count);

            // Remove a tenant.
            mtc.Remove(ToGuid(2));
            Assert.IsTrue(mtc.Contains(ToGuid(1)));
            Assert.IsFalse(mtc.Contains(ToGuid(2)));
            Assert.AreEqual(1, mtc.Count);
            Assert.AreEqual(0, kvc2.Count);

            // Force flush the cache - should be removed.
            CachePolicyManager.Current.ForceFlush();
            Assert.IsFalse(mtc.Contains(ToGuid(1)));
            Assert.IsFalse(mtc.Contains(ToGuid(2)));
            Assert.AreEqual(0, mtc.Count);
            Assert.AreEqual(0, kvc1.Count);
        }
    }
}
