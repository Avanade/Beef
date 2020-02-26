using Beef.Caching.Policy;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Entities;
using Beef.Test.NUnit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Beef.Demo.Test
{
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class StartUpTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestSetUp.Reset(false);
        }

        [Test, TestSetUp]
        public void Validate()
        {
            // Run a randon agent to make sure the API is up and running.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run((a) => a.Agent.GetAsync(404.ToGuid()));

            Assert.AreEqual(25, PagingArgs.DefaultTake);

            var p = CachePolicyManager.DefaultPolicy;
            Assert.NotNull(p);
            Assert.IsInstanceOf(typeof(SlidingCachePolicy), p);

            var scp = (SlidingCachePolicy)p;
            Assert.AreEqual(new TimeSpan(00, 30, 00), scp.Duration);
            Assert.AreEqual(new TimeSpan(02, 00, 00), scp.MaxDuration);
        }
    }
}