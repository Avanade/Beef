// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching;
using Beef.Caching.Policy;
using NUnit.Framework;
using System.Collections.Generic;

namespace Beef.Core.UnitTest.Caching.Policy
{
    [TestFixture]
    public class CachePolicyManagerTest
    {
        [Test]
        public void UnregisterAndReuse()
        {
            CachePolicyManager.Reset();
            var dsc = new DictionarySetCache<int, string>((data) => new KeyValuePair<int, string>[] { new KeyValuePair<int, string>(1, "1"), new KeyValuePair<int, string>(2, "2") }, "CachePolicyManagerTest");

            // Asserting will load the cache on first access.
            Assert.AreEqual("1", dsc[1]);
            Assert.AreEqual("2", dsc[2]);

            dsc.Dispose();

            // Unregister so the policy name can be reused.
            //CachePolicyManager.Unregister(dsc.PolicyKey);

            dsc = new DictionarySetCache<int, string>((data) => new KeyValuePair<int, string>[] { new KeyValuePair<int, string>(1, "10"), new KeyValuePair<int, string>(2, "20") }, "CachePolicyManagerTest");
            Assert.AreEqual("10", dsc[1]);
            Assert.AreEqual("20", dsc[2]);
        }
    }
}
