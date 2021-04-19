using Beef.Data.Database;
using Beef.Data.Database.Cdc;
using Beef.Demo.Api;
using Beef.Demo.Cdc.Data;
using Beef.Entities;
using Beef.Events;
using Beef.Test.NUnit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beef.Demo.Test
{
    [TestFixture, NonParallelizable]
    public class CdcTest
    {
        private class CdcDb : DatabaseBase
        {
            public CdcDb(string cs) : base(cs) { }
        }

        private static void WriteResult(CdcDataOrchestratorResult result)
        {
            TestContext.WriteLine("=========================");
            TestContext.WriteLine("CdcDataOrchestratorResult");
            TestContext.WriteLine($"Success: {result.IsSuccessful}");
            if (result.Exception != null)
                TestContext.WriteLine($"Exception: {result.Exception.Message}");

            if (result.Outbox != null)
                TestContext.WriteLine($"Outbox - Id: {result.Outbox.Id}, IsComplete: {result.Outbox.IsComplete}, CreatedDate: {result.Outbox.CreatedDate}, CompletedDate: {result.Outbox.CompletedDate}");

            if (result.Events != null)
            {
                TestContext.WriteLine($"Events - Count: {result.Events.Length}");
                TestContext.WriteLine();
                foreach (var ed in result.Events)
                {
                    TestContext.WriteLine($"Subject: {ed.Subject}, Action: {ed.Action}, Source: {ed.Source}");
                    TestContext.WriteLine(JsonConvert.SerializeObject(ed.GetValue(), Formatting.Indented));
                }
            }
        }

        [Test, TestSetUp]
        public async Task Execute_EndToEnd()
        {
            await GenericTester
                .Test()
                .AddSingletonService(AgentTester.BuildConfiguration<Startup>("Beef"))
                .RunAsync(async () =>
            {
                var config = ExecutionContext.GetService<IConfiguration>();
                var db = new CdcDb(config.GetConnectionString("BeefDemo"));

                // Create some data.
                var script =
                    "DELETE FROM [DemoCdc].[CdcIdentifierMapping]" + Environment.NewLine +
                    "DELETE FROM [Legacy].[Contact]" + Environment.NewLine +
                    "DELETE FROM [Legacy].[Address]" + Environment.NewLine +
                    "INSERT INTO [Legacy].[Contact] ([Name], [Phone], [Active]) VALUES ('Name1', '123', 1)" + Environment.NewLine +
                    "INSERT INTO [Legacy].[Contact] ([Name], [Phone], [Active]) VALUES ('Name2', '456', 1)" + Environment.NewLine +
                    "INSERT INTO [Legacy].[Contact] ([Name], [Phone], [Active]) VALUES ('Name3', '789', 1)" + Environment.NewLine +
                    "INSERT INTO [Legacy].[Address] ([Street1], [AlternateAddressId]) VALUES ('Petherick', 88)";

                await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
                var ci = await db.SqlStatement("SELECT TOP 1 [ContactId] FROM [Legacy].[Contact] WHERE [Name] = 'Name1'").ScalarAsync<int>().ConfigureAwait(false);
                var ai = await db.SqlStatement("SELECT TOP 1 [Id] FROM [Legacy].[Address]").ScalarAsync<int>().ConfigureAwait(false);
                await Task.Delay(5000);  // Allow sql some time to do its thing.

                // Reset CDC as we do not want to include in the data capture.
                script =
                    "DELETE FROM [DemoCdc].[ContactOutbox]" + Environment.NewLine +
                    "DECLARE @Lsn BINARY(10)" + Environment.NewLine +
                    "SET @Lsn = sys.fn_cdc_get_max_lsn()" + Environment.NewLine +
                    "INSERT INTO [DemoCdc].[ContactOutbox] ([CreatedDate], [ContactMinLsn], [ContactMaxLsn], [AddressMinLsn], [AddressMaxLsn], [IsComplete], [CompletedDate], [HasDataLoss]) VALUES ('2021-01-01T00:00:00', @Lsn, @Lsn, @Lsn, @Lsn, 1, '2021-01-01T00:00:00', 0)";

                await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);

                // Make some table changes for cdc tracking.
                script = 
                    "INSERT INTO [Legacy].[Contact] ([Name], [Phone], [Active]) VALUES ('Name4', '246', 0)" + Environment.NewLine +
                    $"UPDATE [Legacy].[Contact] SET [Phone] = '468', [AlternateContactId] = {ci}, [AddressId] = {ai} WHERE [Name] = 'Name1'" + Environment.NewLine +
                    "UPDATE [Legacy].[Contact] SET [Phone] = '864' WHERE [Name] = 'Name4'" + Environment.NewLine +
                    "INSERT INTO [Legacy].[Contact] ([Name], [Phone], [Active]) VALUES ('Name5', '680', 1)" + Environment.NewLine +
                    "DELETE FROM [Legacy].[Contact] WHERE [Name] = 'Name4'" + Environment.NewLine +
                    "DELETE FROM [Legacy].[Contact] WHERE [Name] = 'Name2'" + Environment.NewLine +
                    "UPDATE [Legacy].[Contact] SET [Phone] = '468' WHERE [Name] = 'Name1'" + Environment.NewLine +
                    "DELETE FROM [Legacy].[Contact] WHERE [Name] = 'Name3'";

                await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
                await Task.Delay(5000); // Allow sql some time to do its thing.

                // Now execute the CdcData.
                var cdc = new ContactCdcData(db, new NullEventPublisher(), ExecutionContext.GetService<ILogger<ContactCdcData>>(), new StringIdentifierGenerator());
                cdc.MaxQuerySize = 6; // Only the first six changes should be picked up.
                var cdor = await cdc.ExecuteAsync().ConfigureAwait(false);
                WriteResult(cdor);

                // Assert/verify the results.
                Assert.NotNull(cdor);
                Assert.IsTrue(cdor.IsSuccessful);
                Assert.AreEqual(6, cdor.Result.Count);

                Assert.IsNull(cdor.Result[0].Name);
                Assert.AreEqual(OperationType.Create, cdor.Result[0].DatabaseOperationType);
                Assert.AreEqual("Name1", cdor.Result[1].Name);
                Assert.NotNull(cdor.Result[1].GlobalId);
                Assert.AreEqual(cdor.Result[1].GlobalId, cdor.Result[1].GlobalAlternateContactId);
                Assert.NotNull(cdor.Result[1].Address);
                Assert.NotNull(cdor.Result[1].Address.GlobalId);
                Assert.AreNotEqual(cdor.Result[1].Address.GlobalId, cdor.Result[1].GlobalId);
                Assert.NotNull(cdor.Result[1].Address.GlobalAlternateAddressId);

                var cgi = cdor.Result[1].GlobalId;
                var agi = cdor.Result[1].Address.GlobalId;

                Assert.AreEqual(OperationType.Update, cdor.Result[1].DatabaseOperationType);
                Assert.IsNull(cdor.Result[2].Name);
                Assert.AreEqual(OperationType.Update, cdor.Result[2].DatabaseOperationType);
                Assert.AreEqual("Name5", cdor.Result[3].Name);
                Assert.AreEqual(OperationType.Create, cdor.Result[3].DatabaseOperationType);
                Assert.IsNull(cdor.Result[4].Name);
                Assert.AreEqual(OperationType.Delete, cdor.Result[4].DatabaseOperationType);
                Assert.IsNull(cdor.Result[5].Name);
                Assert.AreEqual(OperationType.Delete, cdor.Result[5].DatabaseOperationType);

                // Assert/verify the events sent.
                Assert.NotNull(cdor.Events);
                Assert.AreEqual(3, cdor.Events.Length); // There was a create/update/delete of row - not sent as only (very) short lived.

                Assert.AreEqual($"Legacy.Contact.{cdor.Result[1].GlobalId}", cdor.Events[0].Subject);
                Assert.AreEqual("Updated", cdor.Events[0].Action);
                Assert.AreEqual($"Legacy.Contact.{cdor.Result[3].GlobalId}", cdor.Events[1].Subject);
                Assert.AreEqual("Created", cdor.Events[1].Action);
                Assert.AreEqual($"Legacy.Contact.{cdor.Result[5].GlobalId}", cdor.Events[2].Subject);
                Assert.AreEqual("Deleted", cdor.Events[2].Action);

                // Now execute again to get the final changes.
                cdor = await cdc.ExecuteAsync().ConfigureAwait(false);
                WriteResult(cdor);

                // Get the last update being the delete.
                Assert.NotNull(cdor);
                Assert.IsTrue(cdor.IsSuccessful);
                Assert.AreEqual(1, cdor.Result.Count); 

                Assert.IsNull(cdor.Result[0].Name);
                Assert.AreEqual(OperationType.Delete, cdor.Result[0].DatabaseOperationType);

                Assert.NotNull(cdor.Events);
                Assert.AreEqual(1, cdor.Events.Length);
                Assert.AreEqual($"Legacy.Contact.{cdor.Result[0].GlobalId}", cdor.Events[0].Subject);
                Assert.AreEqual("Deleted", cdor.Events[0].Action);

                // Let's now make that last outbox incomplete.
                script = $"UPDATE [DemoCdc].[ContactOutbox] SET [IsComplete] = 0 WHERE [OutboxId] = {cdor.Outbox.Id}";
                await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);

                // Now execute again, should find the incomplete and execute again.
                var cdor2 = await cdc.ExecuteAsync().ConfigureAwait(false);
                WriteResult(cdor2);
                Assert.NotNull(cdor2);
                Assert.IsTrue(cdor2.IsSuccessful);
                Assert.AreEqual(cdor.Outbox.Id, cdor2.Outbox.Id);
                Assert.AreEqual(1, cdor2.Result.Count);
                Assert.IsFalse(cdor2.Outbox.HasDataLoss);

                Assert.IsNull(cdor2.Result[0].Name);
                Assert.AreEqual(OperationType.Delete, cdor2.Result[0].DatabaseOperationType);

                Assert.Null(cdor2.Events); // Should _not_ send again as same data; hash / etag not changed.

                // Tweak the first entry; want to ma verify that the global ids are not changed.
                script = $"UPDATE [Legacy].[Contact] SET [Phone] = '4688' WHERE [ContactId] = {ci}";
                await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
                await Task.Delay(5000); // Allow sql some time to do its thing.

                cdor2 = await cdc.ExecuteAsync().ConfigureAwait(false);
                WriteResult(cdor2);
                Assert.NotNull(cdor2);
                Assert.IsTrue(cdor2.IsSuccessful);

                Assert.AreEqual(cdor2.Result[0].GlobalId, cgi); // Global id's should not have been regenerated.
                Assert.AreEqual(cdor2.Result[0].GlobalAlternateContactId, cgi);
                Assert.AreEqual(cdor2.Result[0].Address.GlobalId, agi);
                Assert.IsNotNull(cdor2.Result[0].Address.GlobalAlternateAddressId);
                Assert.AreNotEqual(cdor2.Result[0].Address.GlobalAlternateAddressId, cdor2.Result[0].Address.GlobalId);

                // Update so it looks like the lsn's are out of whack; i.e. data loss situation - should fail.
                script = $"UPDATE [DemoCdc].[ContactOutbox] SET [IsComplete] = 0, [ContactMinLsn] = 0x00000000000045B80003 WHERE [OutboxId] = {cdor.Outbox.Id}";
                await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);

                cdor2 = await cdc.ExecuteAsync().ConfigureAwait(false);
                WriteResult(cdor2);
                Assert.NotNull(cdor2);
                Assert.IsFalse(cdor2.IsSuccessful);
                Assert.NotNull(cdor2.Exception);
                Assert.IsInstanceOf<BusinessException>(cdor2.Exception);
                Assert.IsNull(cdor2.Outbox);

                // Try again without worrying about data loss.
                cdc.ContinueWithDataLoss = true;
                cdor2 = await cdc.ExecuteAsync().ConfigureAwait(false);
                WriteResult(cdor2);
                Assert.NotNull(cdor2);
                Assert.IsTrue(cdor2.IsSuccessful);
                Assert.IsTrue(cdor2.Outbox.HasDataLoss);

                // Let's corrput further by having more than one incomplete outbox.
                script = $"UPDATE [DemoCdc].[ContactOutbox] SET [IsComplete] = 0";
                await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);

                cdor2 = await cdc.ExecuteAsync().ConfigureAwait(false);
                WriteResult(cdor2);
                Assert.NotNull(cdor2);
                Assert.IsFalse(cdor2.IsSuccessful);
                Assert.NotNull(cdor2.Exception);
                Assert.IsInstanceOf<BusinessException>(cdor2.Exception);
            });
        }

        [Test, TestSetUp]
        public async Task Complete_Errors()
        {
            await GenericTester
                .Test()
                .AddSingletonService(AgentTester.BuildConfiguration<Startup>("Beef"))
                .RunAsync(async () =>
            {
                var config = ExecutionContext.GetService<IConfiguration>();
                var db = new CdcDb(config.GetConnectionString("BeefDemo"));

                var script =
                    "DELETE FROM [DemoCdc].[CdcIdentifierMapping]" + Environment.NewLine +
                    "DELETE FROM [DemoCdc].[ContactOutbox]" + Environment.NewLine +
                    "DECLARE @Lsn BINARY(10)" + Environment.NewLine +
                    "SET @Lsn = sys.fn_cdc_get_max_lsn()" + Environment.NewLine +
                    "INSERT INTO [DemoCdc].[ContactOutbox] ([CreatedDate], [ContactMinLsn], [ContactMaxLsn], [AddressMinLsn], [AddressMaxLsn], [IsComplete], [CompletedDate], [HasDataLoss]) VALUES('2021-01-01T00:00:00', @Lsn, @Lsn, @Lsn, @Lsn, 1, '2021-01-01T00:00:00', 0)" + Environment.NewLine +
                    "SELECT TOP 1 * FROM [DemoCdc].[ContactOutbox]";

                var outbox = await db.SqlStatement(script).SelectSingleAsync(DatabaseMapper.CreateAuto<CdcOutbox>()).ConfigureAwait(false);
                var cdc = new ContactCdcData(db, new NullEventPublisher(), ExecutionContext.GetService<ILogger<ContactCdcData>>(), new StringIdentifierGenerator());

                // Attempt to execute where already complete.
                var cdor = await cdc.CompleteAsync(outbox.Id, new List<CdcTracker>()).ConfigureAwait(false);
                WriteResult(cdor);
                Assert.NotNull(cdor);
                Assert.IsFalse(cdor.IsSuccessful);
                Assert.NotNull(cdor.Exception);
                Assert.IsInstanceOf<BusinessException>(cdor.Exception);

                // Attempt to execute the CdcData with an invalid identifier.
                cdor = await cdc.CompleteAsync(outbox.Id + 1, new List<CdcTracker>()).ConfigureAwait(false);
                WriteResult(cdor);
                Assert.NotNull(cdor);
                Assert.IsFalse(cdor.IsSuccessful);
                Assert.NotNull(cdor.Exception);
                Assert.IsInstanceOf<NotFoundException>(cdor.Exception);

                // Make it incomplete and complete it.
                script = $"UPDATE [DemoCdc].[ContactOutbox] SET [IsComplete] = 0 WHERE [OutboxId] = {outbox.Id}";
                await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);

                cdor = await cdc.CompleteAsync(outbox.Id, new List<CdcTracker>()).ConfigureAwait(false);
                WriteResult(cdor);
                Assert.NotNull(cdor);
                Assert.IsTrue(cdor.IsSuccessful);
                Assert.IsNotNull(cdor.Outbox);
                Assert.AreEqual(true, cdor.Outbox.IsComplete);
            });
        }

        [Test, TestSetUp]
        public async Task NestedTables()
        {
            await GenericTester
                .Test()
                .AddSingletonService(AgentTester.BuildConfiguration<Startup>("Beef"))
                .RunAsync(async () =>
            {
                var config = ExecutionContext.GetService<IConfiguration>();
                var db = new CdcDb(config.GetConnectionString("BeefDemo"));

                // Create some data.
                var script =
                    "DELETE FROM [Legacy].[Posts]" + Environment.NewLine +
                    "DELETE FROM [Legacy].[Comments]" + Environment.NewLine +
                    "DELETE FROM [Legacy].[Tags]" + Environment.NewLine +
                    "INSERT INTO [Legacy].[Posts] ([PostsId], [Text], [Date]) VALUES (1, 'Blah 1', '2020-01-01T15:30:42')" + Environment.NewLine +
                    "INSERT INTO [Legacy].[Posts] ([PostsId], [Text], [Date]) VALUES (2, 'Blah 2', '2020-01-02T15:30:42')" + Environment.NewLine +
                    "INSERT INTO [Legacy].[Posts] ([PostsId], [Text], [Date]) VALUES (3, 'Blah 3', '2020-01-03T15:30:42')" + Environment.NewLine +
                    "INSERT INTO [Legacy].[Comments] ([CommentsId], [PostsId], [Text], [Date]) VALUES (101, 1, 'Blah blah 101', '2020-01-01T18:30:42')" + Environment.NewLine +
                    "INSERT INTO [Legacy].[Comments] ([CommentsId], [PostsId], [Text], [Date]) VALUES (102, 1, 'Blah blah 102', '2020-01-01T19:30:42')" + Environment.NewLine +
                    "INSERT INTO [Legacy].[Comments] ([CommentsId], [PostsId], [Text], [Date]) VALUES (301, 3, 'Blah blah 301', '2020-01-03T18:30:42')" + Environment.NewLine +
                    "INSERT INTO [Legacy].[Tags] ([TagsId], [ParentType], [ParentId], [Text]) VALUES (1001, 'P', 1, '#Blah1')" + Environment.NewLine +
                    "INSERT INTO [Legacy].[Tags] ([TagsId], [ParentType], [ParentId], [Text]) VALUES (2001, 'P', 2, '#Blah2')" + Environment.NewLine +
                    "INSERT INTO [Legacy].[Tags] ([TagsId], [ParentType], [ParentId], [Text]) VALUES (2002, 'P', 2, '#Tag')" + Environment.NewLine +
                    "INSERT INTO [Legacy].[Tags] ([TagsId], [ParentType], [ParentId], [Text]) VALUES (3301, 'C', 301, '#Tag')";

                await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
                await Task.Delay(5000);  // Allow sql some time to do its thing.

                // Reset CDC as we do not want to include in the data capture.
                script =
                    "DELETE FROM [DemoCdc].[PostsOutbox]" + Environment.NewLine +
                    "DECLARE @Lsn BINARY(10)" + Environment.NewLine +
                    "SET @Lsn = sys.fn_cdc_get_max_lsn()" + Environment.NewLine +
                    "INSERT INTO [DemoCdc].[PostsOutbox] ([CreatedDate], [PostsMinLsn], [PostsMaxLsn], [CommentsMinLsn], [CommentsMaxLsn], [CommentsTagsMinLsn], [CommentsTagsMaxLsn], [PostsTagsMinLsn], [PostsTagsMaxLsn], [IsComplete], [CompletedDate], [HasDataLoss]) VALUES('2021-01-01T00:00:00', @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, @Lsn, 1, '2021-01-01T00:00:00', 0)" + Environment.NewLine +
                    "SELECT TOP 1 * FROM [DemoCdc].[PostsOutbox]";

                await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
                await Task.Delay(5000); // Allow sql some time to do its thing.

                // Make some table changes for cdc tracking.
                script =
                    "UPDATE [Legacy].[Comments] SET [Text] = 'Blah blah 101 some more' WHERE [CommentsId] = 101" + Environment.NewLine +
                    "DELETE FROM [Legacy].[Tags] WHERE [TagsId] = 3301" + Environment.NewLine +
                    "INSERT INTO [Legacy].[Posts] ([PostsId], [Text], [Date]) VALUES (4, 'Blah 4', '2020-01-01T15:30:42')" + Environment.NewLine +
                    "INSERT INTO [Legacy].[Comments] ([CommentsId], [PostsId], [Text], [Date]) VALUES (401, 4, 'Blah blah 401', '2020-01-01T18:30:42')";

                await db.SqlStatement(script).NonQueryAsync().ConfigureAwait(false);
                await Task.Delay(5000); // Allow sql some time to do its thing.

                // Now execute the CdcData.
                var cdc = new PostsCdcData(db, new NullEventPublisher(), ExecutionContext.GetService<ILogger<PostsCdcData>>());
                var cdor = await cdc.ExecuteAsync().ConfigureAwait(false);
                WriteResult(cdor);

                // Assert/verify the results.
                Assert.NotNull(cdor);
                Assert.IsTrue(cdor.IsSuccessful);
                Assert.AreEqual(3, cdor.Result.Count);

                Assert.AreEqual($"Legacy.Post", cdor.Events[0].Subject);
                Assert.AreEqual("Created", cdor.Events[0].Action);

                Assert.AreEqual($"Legacy.Post", cdor.Events[1].Subject);
                Assert.AreEqual("Updated", cdor.Events[1].Action);

                Assert.AreEqual($"Legacy.Post", cdor.Events[2].Subject);
                Assert.AreEqual("Updated", cdor.Events[2].Action);
            });
        }
    }
}