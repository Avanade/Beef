using Beef.Demo.Api;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using CoreEx.Database;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Internal;
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
        public void OneTimeSetUp() => Assert.That(TestSetUp.Default.SetUp(), Is.True);

        [Test]
        public void A110_Get()
        {
            using var test = ApiTester.Create<Startup>();

            var r = test.Agent().With<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => new Contact { Id = 1.ToGuid(), FirstName = "Jenny", LastName = "Cuthbert", Status = "P", StatusDescription = "Pending", Communications = new ContactCommCollection { { "home", new ContactComm { Value = "411671953", IsPreferred = true } }, { "fax", new ContactComm { Value = "411123789" } } } })
                .Run(a => a.GetAsync(1.ToGuid()));

            Assert.That(r.Response.Headers?.ETag?.Tag, Is.Not.Null);
            var etag = r.Response.Headers?.ETag?.Tag;

            r = test.Agent().With<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => new Contact { Id = 1.ToGuid(), FirstName = "Jenny", LastName = "Cuthbert", Status = "P", StatusDescription = "Pending", Communications = new ContactCommCollection { { "home", new ContactComm { Value = "411671953", IsPreferred = true } }, { "fax", new ContactComm { Value = "411123789" } } } })
                .Run(a => a.GetAsync(1.ToGuid()));

            Assert.That(r.Response.Headers?.ETag?.Tag, Is.Not.Null);
            Assert.That(r.Response.Headers?.ETag?.Tag, Is.EqualTo(etag));
        }

        [Test]
        public void A112_Patch_Dict()
        {
            using var test = ApiTester.Create<Startup>();

            var v = test.Agent().With<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid())).Value!;

            v.Communications.Remove("fax");
            v.Communications.Add("mobile", new ContactComm { Value = "4258762983" });

            test.Agent().With<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.PatchAsync(CoreEx.Http.HttpPatchOption.MergePatch, "{\"communications\":{\"mobile\":{\"value\":\"4258762983\"},\"fax\":null}}", 1.ToGuid()));

            test.Agent().With<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(v)
                .Run(a => a.GetAsync(1.ToGuid()));
        }

        [Test]
        public void A112a_Patch_Dict_KeyError()
        {
            using var test = ApiTester.Create<Startup>();

            test.Agent().With<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid()));

            test.Agent().With<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectErrors("Communication Type is invalid.")
                .Run(a => a.PatchAsync(CoreEx.Http.HttpPatchOption.MergePatch, "{\"communications\":{\"xyz\":{\"value\":\"4258762983\"},\"fax\":null}}", 1.ToGuid()));
        }

        [Test]
        public void A112b_Patch_Dict_ValueError()
        {
            using var test = ApiTester.Create<Startup>();

            test.Agent().With<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid()));

            test.Agent().With<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectErrors(
                    "Value is required.",
                    "Only one of the Communications can be set as Preferred.")
                .Run(a => a.PatchAsync(CoreEx.Http.HttpPatchOption.MergePatch, "{\"communications\":{\"work\":{\"isPreferred\":true}}}", 1.ToGuid()));
        }

        [Test]
        public void A120_Get_LogicallyDeleted()
        {
            using var test = ApiTester.Create<Startup>();

            var r = test.Agent().With<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(2.ToGuid()));
        }

        [Test]
        public void A130_Update_LogicallyDeleted()
        {
            using var test = ApiTester.Create<Startup>();

            var r = test.Agent().With<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.UpdateAsync(new Contact { Id = 2.ToGuid(), FirstName = "Jenny", LastName = "Cuthbert" }, 2.ToGuid()));
        }

        [Test]
        public void A140_UpdateAndCheckEventOutboxDequeue()
        {
            using var test = ApiTester.Create<Startup>();

            var r = test.Agent().With<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => new Contact { Id = 1.ToGuid(), FirstName = "Jenny", LastName = "Cuthbert", Status = "P", StatusDescription = "Pending" }, "Communications")
                .Run(a => a.GetAsync(1.ToGuid()));

            Assert.That(r.Response.Headers?.ETag?.Tag, Is.Not.Null);
            var etag = r.Response.Headers?.ETag?.Tag;

            using var scope = test.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDatabase>();
            db.SqlStatement("DELETE FROM [Outbox].[EventOutbox]").NonQueryAsync().GetAwaiter().GetResult();
            db.SqlStatement("DELETE FROM [Outbox].[EventOutboxData]").NonQueryAsync().GetAwaiter().GetResult();
            scope.Dispose();

            var v = r.Value;
            v.LastName += "X";

            r = test.Agent().With<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => v)
                .Run(a => a.UpdateAsync(v, 1.ToGuid()));

            Assert.That(r.Response.Headers?.ETag?.Tag, Is.Not.Null);
            Assert.That(r.Response.Headers?.ETag?.Tag, Is.Not.EqualTo(etag));

            // Make sure the event is sent according to the outbox.
            using var scope2 = test.Services.CreateScope();
            db = scope2.ServiceProvider.GetRequiredService<IDatabase>();
            Assert.That(db.SqlStatement("SELECT * FROM [Outbox].[EventOutbox]").SelectQueryAsync(dr =>
            {
                Assert.That(dr.GetValue<DateTime?>("DequeuedDate"), Is.Not.Null);
                return 1;
            }).GetAwaiter().GetResult(), Is.EqualTo(new int[] { 1 }));

            v = null;

            Assert.That(db.SqlStatement("SELECT * FROM [Outbox].[EventOutboxData]").SelectQueryAsync(dr =>
            {
                Assert.Multiple(() =>
                {
                    Assert.That(dr.GetValue<string>("Subject"), Is.EqualTo("Demo.Contact.00000001-0000-0000-0000-000000000000"));
                    Assert.That(dr.GetValue<string>("Action"), Is.EqualTo("Update"));
                });
                return 1;
            }).GetAwaiter().GetResult(), Is.EqualTo(new int[] { 1 }));
        }

        [Test]
        public void A150_GetAll()
        {
            using var test = ApiTester.Create<Startup>();

            var r = test.Agent().With<ContactAgent, ContactCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAllAsync());

            Assert.That(r.Response.Headers?.ETag?.Tag, Is.Not.Null);
            var etag = r.Response.Headers?.ETag?.Tag;

            r = test.Agent().With<ContactAgent, ContactCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAllAsync());

            Assert.That(r.Response.Headers?.ETag?.Tag, Is.Not.Null);
            Assert.That(r.Response.Headers?.ETag?.Tag, Is.EqualTo(etag));

            var v = r.Value.Items[0];
            v.LastName += "X";

            // Update and ensure that the etag has changed as a result.
            var r2 = test.Agent().With<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => v)
                .Run(a => a.UpdateAsync(v, v.Id));

            r = test.Agent().With<ContactAgent, ContactCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAllAsync());

            Assert.That(r2.Response.Headers?.ETag?.Tag, Is.Not.Null);
            Assert.That(r2.Response.Headers?.ETag?.Tag, Is.Not.EqualTo(etag));
        }

        [Test]
        public void A160_Delete()
        {
            using var test = ApiTester.Create<Startup>();

            test.Agent().With<ContactAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.DeleteAsync(1.ToGuid()));

            var r = test.Agent().With<ContactAgent, Contact>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(1.ToGuid()));

            test.Agent().With<ContactAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.DeleteAsync(1.ToGuid()));

            using var scope = test.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDatabase>();

            // Assert that there is only 1 single delete event.
            var count = db.SqlStatement($"SELECT COUNT(*) FROM [Outbox].[EventOutboxData] WHERE [Subject] = 'Demo.Contact.{1.ToGuid()}' and [Action] = 'Delete'").ScalarAsync<int>().GetAwaiter().GetResult();
            Assert.That(count, Is.EqualTo(1));

            // Make sure that the contact was logically deleted; not physically.
            count = db.SqlStatement($"SELECT COUNT(*) FROM [Demo].[Contact] WHERE [ContactId] = '{1.ToGuid()}' and [IsDeleted] = '1'").ScalarAsync<int>().GetAwaiter().GetResult();
            Assert.That(count, Is.EqualTo(1));
        }

        [Test]
        public void A200_RaiseEvent_EventOutboxFailure()
        {
            using var test = ApiTester.Create<Startup>();

            var r = test.Agent().With<ContactAgent>()
                .ExpectStatusCode(HttpStatusCode.InternalServerError)
                .Run(a => a.RaiseEventAsync(true));

            using var scope = test.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDatabase>();
            var count = db.SqlStatement("SELECT COUNT(*) FROM [Outbox].[EventOutboxData] WHERE [Subject] = 'Contact' and [Action] = 'Made'").ScalarAsync<int>().GetAwaiter().GetResult();
            Assert.That(count, Is.EqualTo(0));
        }

        [Test]
        public void A210_RaiseEvent_EventOutboxSuccess()
        {
            using var test = ApiTester.Create<Startup>();

            var r = test.Agent().With<ContactAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.RaiseEventAsync(false));

            using var scope = test.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDatabase>();
            var count = db.SqlStatement("SELECT COUNT(*) FROM [Outbox].[EventOutboxData] WHERE [Subject] = 'Contact' and [Action] = 'Made'").ScalarAsync<int>().GetAwaiter().GetResult();
            Assert.That(count, Is.EqualTo(1));
        }
    }
}