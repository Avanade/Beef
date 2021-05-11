using Beef.Demo.Api;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Test.NUnit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
        public void A120_Get_Deleted()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var r = agentTester.Test<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(2.ToGuid()));
        }

        [Test, TestSetUp]
        public void A130_Update_Deleted()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var r = agentTester.Test<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.UpdateAsync(new Contact { Id = 2.ToGuid(), FirstName = "Jenny", LastName = "Cuthbert" }, 2.ToGuid()));
        }

        [Test, TestSetUp]
        public void A140_UpdateAndCheckEventOutboxDequeue()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var r = agentTester.Test<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => new Contact { Id = 1.ToGuid(), FirstName = "Jenny", LastName = "Cuthbert" })
                .Run(a => a.GetAsync(1.ToGuid()));

            Assert.NotNull(r.Response.Headers?.ETag?.Tag);
            var etag = r.Response.Headers?.ETag?.Tag;

            var db = new Beef.Demo.Business.Data.Database(agentTester.WebApplicationFactory.Services.GetService<IConfiguration>()["ConnectionStrings:BeefDemo"]);
            db.SqlStatement("DELETE FROM [Demo].[EventOutbox]").NonQueryAsync().GetAwaiter().GetResult();

            var v = r.Value;
            v.LastName += "X";

            r = agentTester.Test<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => v)
                .ExpectEvent("Demo.Contact.00000001-0000-0000-0000-000000000000", "Update")
                .Run(a => a.UpdateAsync(v, 1.ToGuid()));

            Assert.NotNull(r.Response.Headers?.ETag?.Tag);
            Assert.AreNotEqual(etag, r.Response.Headers?.ETag?.Tag);

            // Make sure the event is sent from the outbox.
            var count = db.SqlStatement("SELECT COUNT(*) FROM [Demo].[EventOutbox]").ScalarAsync<int>().GetAwaiter().GetResult();
            Assert.AreEqual(1, count);

            for (int i = 0; i < 10; i++)
            {
                count = db.SqlStatement("SELECT COUNT(*) FROM [Demo].[EventOutbox] WHERE [DequeuedDate] IS NULL").ScalarAsync<int>().GetAwaiter().GetResult();
                if (count == 0)
                    return;

                System.Threading.Thread.Sleep(1000);
            }

            Assert.Fail("It would appear that the event was not dequeued by the hosted service.");
        }

        [Test, TestSetUp]
        public void A150_GetAll()
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

            // Update and ensure that the etag has changed as a result.
            var r2 = agentTester.Test<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => v)
                .Run(a => a.UpdateAsync(v, v.Id));

            r = agentTester.Test<ContactAgent, ContactCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAllAsync());

            Assert.NotNull(r2.Response.Headers?.ETag?.Tag);
            Assert.AreNotEqual(etag, r2.Response.Headers?.ETag?.Tag);
        }

        [Test, TestSetUp]
        public void A160_Delete()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            agentTester.Test<ContactAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.DeleteAsync(1.ToGuid()));

            var r = agentTester.Test<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(1.ToGuid()));

            agentTester.Test<ContactAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.DeleteAsync(1.ToGuid()));
        }

        [Test, TestSetUp]
        public void A200_RaiseEvent_EventOutboxFailure()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var r = agentTester.Test<ContactAgent>()
                .ExpectStatusCode(HttpStatusCode.InternalServerError)
                .Run(a => a.RaiseEventAsync(true));

            var db = new Beef.Demo.Business.Data.Database(agentTester.WebApplicationFactory.Services.GetService<IConfiguration>()["ConnectionStrings:BeefDemo"]);
            var count = db.SqlStatement("SELECT COUNT(*) FROM [Demo].[EventOutboxData] WHERE [Subject] = 'Contact' and [Action] = 'Made'").ScalarAsync<int>().GetAwaiter().GetResult();
            Assert.AreEqual(0, count);
        }

        [Test, TestSetUp]
        public void A210_RaiseEvent_EventOutboxSuccess()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var r = agentTester.Test<ContactAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .ExpectEvent("Contact", "Made")
                .Run(a => a.RaiseEventAsync(false));

            var db = new Beef.Demo.Business.Data.Database(agentTester.WebApplicationFactory.Services.GetService<IConfiguration>()["ConnectionStrings:BeefDemo"]);
            var count = db.SqlStatement("SELECT COUNT(*) FROM [Demo].[EventOutboxData] WHERE [Subject] = 'Contact' and [Action] = 'Made'").ScalarAsync<int>().GetAwaiter().GetResult();
            Assert.AreEqual(1, count);
        }
    }
}