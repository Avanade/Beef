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
            var r = AgentTester.Create<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => new Contact { Id = 1.ToGuid(), FirstName = "Jenny", LastName = "Cuthbert" })
                .Run((a) => a.Agent.GetAsync(1.ToGuid()));

            Assert.NotNull(r.Response.Headers?.ETag?.Tag);
            var etag = r.Response.Headers?.ETag?.Tag;

            r = AgentTester.Create<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => new Contact { Id = 1.ToGuid(), FirstName = "Jenny", LastName = "Cuthbert" })
                .Run((a) => a.Agent.GetAsync(1.ToGuid()));

            Assert.NotNull(r.Response.Headers?.ETag?.Tag);
            Assert.AreEqual(etag, r.Response.Headers?.ETag?.Tag);
        }

        [Test, TestSetUp]
        public void A120_Update()
        {
            var r = AgentTester.Create<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => new Contact { Id = 1.ToGuid(), FirstName = "Jenny", LastName = "Cuthbert" })
                .Run((a) => a.Agent.GetAsync(1.ToGuid()));

            Assert.NotNull(r.Response.Headers?.ETag?.Tag);
            var etag = r.Response.Headers?.ETag?.Tag;

            var v = r.Value;
            v.LastName += "X";

            r = AgentTester.Create<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => v)
                .Run((a) => a.Agent.UpdateAsync(v, 1.ToGuid()));

            Assert.NotNull(r.Response.Headers?.ETag?.Tag);
            Assert.AreNotEqual(etag, r.Response.Headers?.ETag?.Tag);
        }

        [Test, TestSetUp]
        public void A130_GetAll()
        {
            var r = AgentTester.Create<ContactAgent, ContactCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAllAsync());

            Assert.NotNull(r.Response.Headers?.ETag?.Tag);
            var etag = r.Response.Headers?.ETag?.Tag;

            r = AgentTester.Create<ContactAgent, ContactCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAllAsync());

            Assert.NotNull(r.Response.Headers?.ETag?.Tag);
            Assert.AreEqual(etag, r.Response.Headers?.ETag?.Tag);

            var v = r.Value.Result[0];
            v.LastName += "X";

            var r2 = AgentTester.Create<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => v)
                .Run((a) => a.Agent.UpdateAsync(v, v.Id));

            Assert.NotNull(r2.Response.Headers?.ETag?.Tag);
            Assert.AreNotEqual(etag, r2.Response.Headers?.ETag?.Tag);
        }
    }
}
