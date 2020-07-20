using Beef.Demo.Api;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Test.NUnit;
using NUnit.Framework;
using System.Net;

namespace Beef.Demo.Test
{
    [TestFixture, NonParallelizable]
    public class ContactTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => TestSetUp.Reset();

        [Test, TestSetUp]
        public void A110_Get()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var r = agentTester.Test<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => new Contact { Id = 1.ToGuid(), FirstName = "Jenny", LastName = "Cuthbert" })
                .Run(a => a.GetAsync(1.ToGuid()));

            Assert.NotNull(r.Response.Headers?.ETag?.Tag);
            var etag = r.Response.Headers?.ETag?.Tag;

            r = agentTester.Test<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => new Contact { Id = 1.ToGuid(), FirstName = "Jenny", LastName = "Cuthbert" })
                .Run(a => a.GetAsync(1.ToGuid()));

            Assert.NotNull(r.Response.Headers?.ETag?.Tag);
            Assert.AreEqual(etag, r.Response.Headers?.ETag?.Tag);
        }

        [Test, TestSetUp]
        public void A120_Update()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var r = agentTester.Test<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => new Contact { Id = 1.ToGuid(), FirstName = "Jenny", LastName = "Cuthbert" })
                .Run(a => a.GetAsync(1.ToGuid()));

            Assert.NotNull(r.Response.Headers?.ETag?.Tag);
            var etag = r.Response.Headers?.ETag?.Tag;

            var v = r.Value;
            v.LastName += "X";

            r = agentTester.Test<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => v)
                .ExpectEvent("Demo.Contact.00000001-0000-0000-0000-000000000000", "Update")
                .Run(a => a.UpdateAsync(v, 1.ToGuid()));

            Assert.NotNull(r.Response.Headers?.ETag?.Tag);
            Assert.AreNotEqual(etag, r.Response.Headers?.ETag?.Tag);
        }

        [Test, TestSetUp]
        public void A130_GetAll()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var r = agentTester.Test<ContactAgent, ContactCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAllAsync());

            Assert.NotNull(r.Response.Headers?.ETag?.Tag);
            var etag = r.Response.Headers?.ETag?.Tag;

            r = agentTester.Test<ContactAgent, ContactCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAllAsync());

            Assert.NotNull(r.Response.Headers?.ETag?.Tag);
            Assert.AreEqual(etag, r.Response.Headers?.ETag?.Tag);

            var v = r.Value.Result[0];
            v.LastName += "X";

            var r2 = agentTester.Test<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => v)
                .Run(a => a.UpdateAsync(v, v.Id));

            Assert.NotNull(r2.Response.Headers?.ETag?.Tag);
            Assert.AreNotEqual(etag, r2.Response.Headers?.ETag?.Tag);
        }
    }
}