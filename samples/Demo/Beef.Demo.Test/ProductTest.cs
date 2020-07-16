using Beef.Demo.Api;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Test.NUnit;
using NUnit.Framework;
using System.Linq;
using System.Net;

namespace Beef.Demo.Test
{
    [TestFixture]
    public class ProductTest
    {
        private AgentTesterServer<Startup> _agentTester;

        [OneTimeSetUp]
        public void OneTimeSetUp() { AgentTester.ResetNoSetup(); _agentTester = AgentTester.CreateServer<Startup>(); }

        [OneTimeTearDown]
        public void OneTimeTearDown() => _agentTester.Dispose();

        [Test]
        public void B110_Get_NotFound()
        {
            _agentTester.Test<ProductAgent, Product>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .Run(a => a.GetAsync(404));
        }

        [Test]
        public void B120_Get()
        {
            _agentTester.Test<ProductAgent, Product>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => new Product { Id = 1, Name = "Milk", Description = "Low fat milk" })
                .Run(a => a.GetAsync(1));
        }

        [Test]
        public void C110_GetByArgs_Null()
        {
            var pcr = _agentTester.Test<ProductAgent, ProductCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(null));

            // Check 11 are returned.
            Assert.AreEqual(11, pcr?.Value?.Result?.Count);
        }

        [Test]
        public void C120_GetByArgs_Null_Paging()
        {
            var pcr = _agentTester.Test<ProductAgent, ProductCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(null, Entities.PagingArgs.CreateSkipAndTake(4, 2, true)));

            // Check paging and total count.
            Assert.AreEqual(2, pcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "Fruit Punch", "Cranberry Juice" }, pcr.Value.Result.Select(x => x.Name).ToArray());
            Assert.AreEqual(11, pcr.Value.Paging.TotalCount);
        }

        [Test]
        public void C130_GetByArgs_Wildcard1()
        {
            var pcr = _agentTester.Test<ProductAgent, ProductCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new ProductArgs { Name = "l*" }));

            // Check 2 are returned.
            Assert.AreEqual(2, pcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "LCD HDTV", "Lemonade" }, pcr.Value.Result.Select(x => x.Name).ToArray());
        }

        [Test]
        public void C140_GetByArgs_Wildcard2()
        {
            var pcr = _agentTester.Test<ProductAgent, ProductCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new ProductArgs { Name = "l*", Description = "*er" }));

            // Check 1 is returned.
            Assert.AreEqual(1, pcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "LCD HDTV" }, pcr.Value.Result.Select(x => x.Name).ToArray());
        }
    }
}