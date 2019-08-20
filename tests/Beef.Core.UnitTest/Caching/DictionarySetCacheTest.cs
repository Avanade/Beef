// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching;
using Beef.Caching.Policy;
using NUnit.Framework;
using System.Collections.Generic;

namespace Beef.Core.UnitTest.Caching
{
    [TestFixture]
    public class DictionarySetCacheTest
    {
        [Test]
        public void GetAndContainsKey()
        {
            var dsc = new DictionarySetCache<int, string>((data) => new KeyValuePair<int, string>[] { new KeyValuePair<int, string>(1, "1"), new KeyValuePair<int, string>(2, "2") });

            Assert.IsTrue(dsc.ContainsKey(1));
            Assert.IsTrue(dsc.ContainsKey(2));
            Assert.IsFalse(dsc.ContainsKey(3));

            Assert.IsTrue(dsc.TryGetByKey(1, out string val));
            Assert.AreEqual("1", val);

            Assert.IsTrue(dsc.TryGetByKey(2, out val));
            Assert.AreEqual("2", val);

            Assert.IsFalse(dsc.TryGetByKey(3, out val));
            Assert.IsNull(val);
        }

        [Test]
        public void PolicyManager()
        {
            CachePolicyManager.Reset();

            var dsc = new DictionarySetCache<int, string>((data) => new KeyValuePair<int, string>[] { new KeyValuePair<int, string>(1, "1"), new KeyValuePair<int, string>(2, "2") });
            Assert.IsNotNull(dsc.PolicyKey);
            Assert.AreEqual(0, dsc.Count);

            Assert.IsTrue(dsc.ContainsKey(1));
            Assert.IsTrue(dsc.ContainsKey(2));
            Assert.IsFalse(dsc.ContainsKey(3));
            Assert.AreEqual(2, dsc.Count);

            var policy = new DailyCachePolicy();
            CachePolicyManager.Set(dsc.PolicyKey, policy);

            var policy2 = dsc.GetPolicy();
            Assert.IsNotNull(policy2);
            Assert.AreSame(policy, policy2);

            var pa = CachePolicyManager.GetPolicies();
            Assert.AreEqual(1, pa.Length);

            CachePolicyManager.ForceFlush();
            Assert.AreEqual(0, dsc.Count);

            Assert.IsTrue(dsc.ContainsKey(1));
            Assert.IsTrue(dsc.ContainsKey(2));
            Assert.IsFalse(dsc.ContainsKey(3));
            Assert.AreEqual(2, dsc.Count);
        }

        [Test]
        public void Flush()
        {
            CachePolicyManager.Reset();

            using (var dsc = new DictionarySetCache<int, string>((data) => new KeyValuePair<int, string>[] { new KeyValuePair<int, string>(1, "1"), new KeyValuePair<int, string>(2, "2") }))
            {
                var policy = new DailyCachePolicy();
                CachePolicyManager.Set(dsc.PolicyKey, policy);

                Assert.IsTrue(dsc.ContainsKey(1));
                Assert.IsTrue(dsc.ContainsKey(2));
                Assert.AreEqual(1, dsc.GetPolicy().Hits);

                dsc.Flush(true);

                Assert.IsTrue(dsc.ContainsKey(1));
                Assert.IsTrue(dsc.ContainsKey(2));
                Assert.AreEqual(1, dsc.GetPolicy().Hits);
            }
        }
    }
}