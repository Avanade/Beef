// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching;
using Beef.Caching.Policy;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Collections.Generic;

namespace Beef.Core.UnitTest.Caching.Policy
{
    [TestFixture]
    public class CachePolicyManagerTest
    {
        public static System.IServiceProvider TestSetUp()
        {
            var services = new ServiceCollection();
            services.AddSingleton(_ => new CachePolicyManager());

            var sp = services.BuildServiceProvider();

            ExecutionContext.Reset(false);
            ExecutionContext.SetCurrent(new ExecutionContext { ServiceProvider = sp });

            return sp;
        }

        [Test]
        public void UnregisterAndReuse()
        {
            TestSetUp();
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
