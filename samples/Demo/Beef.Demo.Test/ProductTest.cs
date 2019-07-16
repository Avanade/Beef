using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Test.NUnit;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net;
using System.Text;

namespace Beef.Demo.Test
{
    [TestFixture, Parallelizable]
    public class ProductTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestSetUp.Reset(false);
        }

        [Test, Parallelizable, TestSetUp(needsSetUp: false)]
        public void B110_Get_NotFound()
        {
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .Run((a) => a.Agent.GetAsync(404.ToGuid()));
        }

        [Test, Parallelizable, TestSetUp(needsSetUp: false)]
        public void B120_Get()
        {
            AgentTester.Create<ProductAgent, Product>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => new Product { Id = 1, Name = "Milk", Description = "Low fat milk" })
                .Run((a) => a.Agent.GetAsync(1));
        }

        [Test, Parallelizable, TestSetUp(needsSetUp: false)]
        public void C110_GetByArgs_Null()
        {
            var pcr = AgentTester.Create<ProductAgent, ProductCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetByArgsAsync(null));

            // Check 11 are returned.
            Assert.AreEqual(11, pcr?.Value?.Result?.Count);
        }

        [Test, Parallelizable, TestSetUp(needsSetUp: false)]
        public void C110_GetByArgs_Wildcard1()
        {
            var pcr = AgentTester.Create<ProductAgent, ProductCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetByArgsAsync(new ProductArgs { Name = "l*" }));

            // Check 2 are returned.
            Assert.AreEqual(2, pcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "LCD HDTV", "Lemonade" }, pcr.Value.Result.Select(x => x.Name).ToArray());
        }

        [Test, Parallelizable, TestSetUp(needsSetUp: false)]
        public void C110_GetByArgs_Wildcard2()
        {
            var pcr = AgentTester.Create<ProductAgent, ProductCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetByArgsAsync(new ProductArgs { Name = "l*", Description = "*er" }));

            // Check 1 is returned.
            Assert.AreEqual(1, pcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "LCD HDTV" }, pcr.Value.Result.Select(x => x.Name).ToArray());
        }
    }
}
