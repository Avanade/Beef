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
    public class ProductTest : UsingAgentTesterServer<Startup>
    {
        [Test, TestSetUp]
        public void B110_Get_NotFound()
        {
            AgentTester.Test<ProductAgent, Product>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .Run(a => a.GetAsync(404));
        }

        [Test, TestSetUp]
        public void B120_Get()
        {
            AgentTester.Test<ProductAgent, Product>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => new Product { Id = 1, Name = "Milk", Description = "Low fat milk" })
                .Run(a => a.GetAsync(1));
        }

        [Test, TestSetUp]
        public void C110_GetByArgs_Null()
        {
            var pcr = AgentTester.Test<ProductAgent, ProductCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(null));

            // Check 11 are returned.
            Assert.AreEqual(11, pcr?.Value?.Result?.Count);
        }

        [Test, TestSetUp]
        public void C120_GetByArgs_Null_Paging()
        {
            var pcr = AgentTester.Test<ProductAgent, ProductCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(null, Entities.PagingArgs.CreateSkipAndTake(4, 2, true)));

            // Check paging and total count.
            Assert.AreEqual(2, pcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "Fruit Punch", "Cranberry Juice" }, pcr.Value.Result.Select(x => x.Name).ToArray());
            Assert.AreEqual(11, pcr.Value.Paging.TotalCount);
        }

        [Test, TestSetUp]
        public void C130_GetByArgs_Wildcard1()
        {
            var pcr = AgentTester.Test<ProductAgent, ProductCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new ProductArgs { Name = "l*" }));

            // Check 2 are returned.
            Assert.AreEqual(2, pcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "LCD HDTV", "Lemonade" }, pcr.Value.Result.Select(x => x.Name).ToArray());
        }

        [Test, TestSetUp]
        public void C140_GetByArgs_Wildcard2()
        {
            var pcr = AgentTester.Test<ProductAgent, ProductCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new ProductArgs { Name = "l*", Description = "*er" }));

            // Check 1 is returned.
            Assert.AreEqual(1, pcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "LCD HDTV" }, pcr.Value.Result.Select(x => x.Name).ToArray());
        }
    }
}