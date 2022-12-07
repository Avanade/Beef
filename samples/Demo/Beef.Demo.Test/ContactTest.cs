using Beef.Demo.Api;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using CoreEx.Database;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Net;
using UnitTestEx;
using UnitTestEx.Expectations;
using UnitTestEx.NUnit;

namespace Beef.Demo.Test
{
    [TestFixture, NonParallelizable]
    public class ContactTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => Assert.IsTrue(TestSetUp.Default.SetUp());

        [Test]
        public void A110_Get()
        {
            using var test = ApiTester.Create<Startup>();

            var r = test.Agent<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => new Contact { Id = 1.ToGuid(), FirstName = "Jenny", LastName = "Cuthbert", Status = "P", StatusDescription = "Pending" })
                .Run(a => a.GetAsync(1.ToGuid()));

            Assert.NotNull(r.Response.Headers?.ETag?.Tag);
            var etag = r.Response.Headers?.ETag?.Tag;

            r = test.Agent<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => new Contact { Id = 1.ToGuid(), FirstName = "Jenny", LastName = "Cuthbert", Status = "P", StatusDescription = "Pending" })
                .Run(a => a.GetAsync(1.ToGuid()));

            Assert.NotNull(r.Response.Headers?.ETag?.Tag);
            Assert.AreEqual(etag, r.Response.Headers?.ETag?.Tag);
        }

        [Test]
        public void A120_Get_LogicallyDeleted()
        {
            using var test = ApiTester.Create<Startup>();

            var r = test.Agent<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(2.ToGuid()));
        }

        [Test]
        public void A130_Update_LogicallyDeleted()
        {
            using var test = ApiTester.Create<Startup>();

            var r = test.Agent<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.UpdateAsync(new Contact { Id = 2.ToGuid(), FirstName = "Jenny", LastName = "Cuthbert" }, 2.ToGuid()));
        }

        [Test]
        public void A140_UpdateAndCheckEventOutboxDequeue()
        {
            using var test = ApiTester.Create<Startup>();

            var r = test.Agent<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => new Contact { Id = 1.ToGuid(), FirstName = "Jenny", LastName = "Cuthbert", Status = "P", StatusDescription = "Pending" })
                .Run(a => a.GetAsync(1.ToGuid()));

            Assert.NotNull(r.Response.Headers?.ETag?.Tag);
            var etag = r.Response.Headers?.ETag?.Tag;

            using var scope = test.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDatabase>();
            db.SqlStatement("DELETE FROM [Outbox].[EventOutbox]").NonQueryAsync().GetAwaiter().GetResult();
            db.SqlStatement("DELETE FROM [Outbox].[EventOutboxData]").NonQueryAsync().GetAwaiter().GetResult();
            scope.Dispose();

            var v = r.Value;
            v.LastName += "X";

            r = test.Agent<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => v)
                .Run(a => a.UpdateAsync(v, 1.ToGuid()));

            Assert.NotNull(r.Response.Headers?.ETag?.Tag);
            Assert.AreNotEqual(etag, r.Response.Headers?.ETag?.Tag);

            // Make sure the event is sent according to the outbox.
            using var scope2 = test.Services.CreateScope();
            db = scope2.ServiceProvider.GetRequiredService<IDatabase>();
            Assert.AreEqual(new int[] { 1 }, db.SqlStatement("SELECT * FROM [Outbox].[EventOutbox]").SelectQueryAsync(dr =>
            {
                Assert.IsNotNull(dr.GetValue<DateTime?>("DequeuedDate"));
                return 1;
            }).GetAwaiter().GetResult());

            Assert.AreEqual(new int[] { 1 }, db.SqlStatement("SELECT * FROM [Outbox].[EventOutboxData]").SelectQueryAsync(dr =>
            {
                Assert.AreEqual("Demo.Contact.00000001-0000-0000-0000-000000000000", dr.GetValue<string>("Subject"));
                Assert.AreEqual("Update", dr.GetValue<string>("Action"));
                return 1;
            }).GetAwaiter().GetResult());
        }

        [Test]
        public void A150_GetAll()
        {
            using var test = ApiTester.Create<Startup>();

            var r = test.Agent<ContactAgent, ContactCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAllAsync());

            Assert.NotNull(r.Response.Headers?.ETag?.Tag);
            var etag = r.Response.Headers?.ETag?.Tag;

            r = test.Agent<ContactAgent, ContactCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAllAsync());

            Assert.NotNull(r.Response.Headers?.ETag?.Tag);
            Assert.AreEqual(etag, r.Response.Headers?.ETag?.Tag);

            var v = r.Value.Items[0];
            v.LastName += "X";

            // Update and ensure that the etag has changed as a result.
            var r2 = test.Agent<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => v)
                .Run(a => a.UpdateAsync(v, v.Id));

            r = test.Agent<ContactAgent, ContactCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAllAsync());

            Assert.NotNull(r2.Response.Headers?.ETag?.Tag);
            Assert.AreNotEqual(etag, r2.Response.Headers?.ETag?.Tag);
        }

        [Test]
        public void A160_Delete()
        {
            using var test = ApiTester.Create<Startup>();

            test.Agent<ContactAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.DeleteAsync(1.ToGuid()));

            var r = test.Agent<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(1.ToGuid()));

            test.Agent<ContactAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.DeleteAsync(1.ToGuid()));

            using var scope = test.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDatabase>();

            // Assert that there is only 1 single delete event.
            var count = db.SqlStatement($"SELECT COUNT(*) FROM [Outbox].[EventOutboxData] WHERE [Subject] = 'Demo.Contact.{1.ToGuid()}' and [Action] = 'Delete'").ScalarAsync<int>().GetAwaiter().GetResult();
            Assert.AreEqual(1, count);

            // Make sure that the contact was logically deleted; not physically.
            count = db.SqlStatement($"SELECT COUNT(*) FROM [Demo].[Contact] WHERE [ContactId] = '{1.ToGuid()}' and [IsDeleted] = '1'").ScalarAsync<int>().GetAwaiter().GetResult();
            Assert.AreEqual(1, count);
        }

        [Test]
        public void A200_RaiseEvent_EventOutboxFailure()
        {
            using var test = ApiTester.Create<Startup>();

            var r = test.Agent<ContactAgent>()
                .ExpectStatusCode(HttpStatusCode.InternalServerError)
                .Run(a => a.RaiseEventAsync(true));

            using var scope = test.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDatabase>();
            var count = db.SqlStatement("SELECT COUNT(*) FROM [Outbox].[EventOutboxData] WHERE [Subject] = 'Contact' and [Action] = 'Made'").ScalarAsync<int>().GetAwaiter().GetResult();
            Assert.AreEqual(0, count);
        }

        [Test]
        public void A210_RaiseEvent_EventOutboxSuccess()
        {
            using var test = ApiTester.Create<Startup>();

            var r = test.Agent<ContactAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.RaiseEventAsync(false));

            using var scope = test.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDatabase>();
            var count = db.SqlStatement("SELECT COUNT(*) FROM [Outbox].[EventOutboxData] WHERE [Subject] = 'Contact' and [Action] = 'Made'").ScalarAsync<int>().GetAwaiter().GetResult();
            Assert.AreEqual(1, count);
        }
    }
}