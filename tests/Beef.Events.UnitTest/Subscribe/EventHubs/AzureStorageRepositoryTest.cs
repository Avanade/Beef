using Beef.Events.Subscribe;
using Beef.Events.Subscribe.EventHubs;
using Beef.Test.NUnit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.WindowsAzure.Storage.Table;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AzureEventHubs = Microsoft.Azure.EventHubs;

namespace Beef.Events.UnitTest.Subscribers.EventHubs
{
    [TestFixture]
    public class AzureStorageRepositoryTest
    {
        private IConfiguration _config;

        private IConfiguration GetConfig()
        {
            if (_config == null)
            {
                _config = new ConfigurationBuilder()
                    .AddJsonFile(new EmbeddedFileProvider(typeof(AzureStorageRepositoryTest).Assembly), "webjobs.settings.json", true, false)
                    .Build();
            }

            return _config;
        }

        private static AzureEventHubs.EventData CreateEventData(string offset, long seqNo)
        {
            var e = new EventData
            {
                Subject = "Unit.Test",
                Action = "Verify",
            }.ToEventHubsEventData();

            var type = typeof(AzureEventHubs.EventData);
            var pi = type.GetProperty("SystemProperties");
            pi.SetValue(e, Activator.CreateInstance(pi.PropertyType, true));

            var dict = (Dictionary<string, object>)e.SystemProperties;
            dict.Add("x-opt-enqueued-time", DateTime.UtcNow);
            dict.Add("x-opt-offset", offset);
            dict.Add("x-opt-sequence-number", seqNo);
            dict.Add("x-opt-partition-key", "0");

            return e;
        }

        public static async Task<List<EventAuditRecord>> GetAuditRecords(CloudTable ct)
        {
            var r = await ct.ExecuteQuerySegmentedAsync(new TableQuery<EventAuditRecord>(), null).ConfigureAwait(false);
            return r.Results;
        }

        public static async Task DeleteAuditRecords(CloudTable ct)
        {
            var records = await GetAuditRecords(ct).ConfigureAwait(false);
            foreach (var r in records)
            {
                await ct.ExecuteAsync(TableOperation.Delete(r));
            }
        }

        public Result HandleResult(Result result)
        {
            result.Subject = "Unit.Test";
            result.Action = "Verify";
            return result;
        }

        [Test]
        public async Task A100_EndToEnd()
        {
            var cfg = GetConfig();
            var lgr = TestSetUp.CreateLogger();

            var asrArgs = new EventHubsRepositoryArgs("testhub", cfg);

            var asr = new AzureStorageRepository(asrArgs, cfg, lgr)
            {
                PoisonTableName = cfg.GetValue<string>("EventHubPoisonMessagesTable"),
                AuditTableName = cfg.GetValue<string>("EventHubAuditMessagesTable")
            };

            // Make sure there are no AuditRecords to begin with.
            var pmt = await asr.GetPoisonMessageTableAsync().ConfigureAwait(false);
            await DeleteAuditRecords(pmt).ConfigureAwait(false);

            var amt = await asr.GetAuditMessageTableAsync().ConfigureAwait(false);
            await DeleteAuditRecords(amt).ConfigureAwait(false);

            var ed = CreateEventData("100", 1);

            // Checking and removing with unknown is a-ok.
            Assert.AreEqual(PoisonMessageAction.NotPoison, await asr.CheckPoisonedAsync(ed).ConfigureAwait(false));
            await asr.RemovePoisonedAsync(ed).ConfigureAwait(false);
            Assert.AreEqual(0, (await GetAuditRecords(pmt)).Count);
            Assert.AreEqual(0, (await GetAuditRecords(amt)).Count);

            // Add an event as poison.
            await asr.MarkAsPoisonedAsync(ed, HandleResult(Result.DataNotFound("Data not found."))).ConfigureAwait(false);
            Assert.AreEqual(PoisonMessageAction.PoisonRetry, await asr.CheckPoisonedAsync(ed).ConfigureAwait(false));
            Assert.AreEqual(1, (await GetAuditRecords(pmt).ConfigureAwait(false)).Count);
            Assert.AreEqual(0, (await GetAuditRecords(amt).ConfigureAwait(false)).Count);

            var ear = (await GetAuditRecords(pmt).ConfigureAwait(false)).First();
            Assert.AreEqual("unittest.servicebus.windows.net-testhub", ear.PartitionKey);
            Assert.AreEqual("$Default-0", ear.RowKey);
            Assert.NotNull(ear.Body);
            Assert.NotNull(ear.PoisonedTimeUtc);
            Assert.IsNull(ear.SkippedTimeUtc);
            Assert.AreEqual("DataNotFound", ear.Status);
            Assert.AreEqual("Data not found.", ear.Reason);
            Assert.IsNull(ear.OriginatingStatus);
            Assert.IsNull(ear.OriginatingReason);
            Assert.AreEqual("100", ear.Offset);
            Assert.AreEqual(1, ear.SequenceNumber);
            Assert.AreEqual(false, ear.SkipProcessing);
            Assert.AreEqual(0, ear.Retries);

            // Update again an event as poison.
            await asr.MarkAsPoisonedAsync(ed, HandleResult(Result.InvalidData("Bad data."))).ConfigureAwait(false);
            Assert.AreEqual(PoisonMessageAction.PoisonRetry, await asr.CheckPoisonedAsync(ed).ConfigureAwait(false));
            Assert.AreEqual(1, (await GetAuditRecords(pmt).ConfigureAwait(false)).Count);
            Assert.AreEqual(0, (await GetAuditRecords(amt).ConfigureAwait(false)).Count);

            ear = (await GetAuditRecords(pmt).ConfigureAwait(false)).First();
            Assert.AreEqual("unittest.servicebus.windows.net-testhub", ear.PartitionKey);
            Assert.AreEqual("$Default-0", ear.RowKey);
            Assert.NotNull(ear.Body);
            Assert.NotNull(ear.PoisonedTimeUtc);
            Assert.IsNull(ear.SkippedTimeUtc);
            Assert.AreEqual("InvalidData", ear.Status);
            Assert.AreEqual("Bad data.", ear.Reason);
            Assert.IsNull(ear.OriginatingStatus);
            Assert.IsNull(ear.OriginatingReason);
            Assert.AreEqual("100", ear.Offset);
            Assert.AreEqual(1, ear.SequenceNumber);
            Assert.AreEqual(false, ear.SkipProcessing);
            Assert.AreEqual(1, ear.Retries);

            // Update to skip.
            await asr.SkipPoisonedAsync(ed).ConfigureAwait(false);
            Assert.AreEqual(1, (await GetAuditRecords(pmt).ConfigureAwait(false)).Count);
            Assert.AreEqual(0, (await GetAuditRecords(amt).ConfigureAwait(false)).Count);

            ear = (await GetAuditRecords(pmt).ConfigureAwait(false)).First();
            Assert.AreEqual("unittest.servicebus.windows.net-testhub", ear.PartitionKey);
            Assert.AreEqual("$Default-0", ear.RowKey);
            Assert.NotNull(ear.Body);
            Assert.NotNull(ear.PoisonedTimeUtc);
            Assert.IsNull(ear.SkippedTimeUtc);
            Assert.AreEqual("InvalidData", ear.Status);
            Assert.AreEqual("Bad data.", ear.Reason);
            Assert.IsNull(ear.OriginatingStatus);
            Assert.IsNull(ear.OriginatingReason);
            Assert.AreEqual("100", ear.Offset);
            Assert.AreEqual(1, ear.SequenceNumber);
            Assert.AreEqual(true, ear.SkipProcessing);
            Assert.AreEqual(1, ear.Retries);

            // Check poisoned which will remove and audit.
            Assert.AreEqual(PoisonMessageAction.PoisonSkip, await asr.CheckPoisonedAsync(ed).ConfigureAwait(false));
            Assert.AreEqual(0, (await GetAuditRecords(pmt).ConfigureAwait(false)).Count);
            Assert.AreEqual(1, (await GetAuditRecords(amt).ConfigureAwait(false)).Count);

            ear = (await GetAuditRecords(amt).ConfigureAwait(false)).First();
            Assert.AreEqual("unittest.servicebus.windows.net-testhub", ear.PartitionKey);
            Assert.IsTrue(ear.RowKey.EndsWith("-$Default-0"));
            Assert.NotNull(ear.Body);
            Assert.NotNull(ear.PoisonedTimeUtc);
            Assert.NotNull(ear.SkippedTimeUtc);
            Assert.AreEqual("PoisonSkipped", ear.Status);
            Assert.AreEqual("EventData was identified as Poison and was marked as SkipMessage; this event is skipped (i.e. not processed).", ear.Reason);
            Assert.AreEqual("InvalidData", ear.OriginatingStatus);
            Assert.AreEqual("Bad data.", ear.OriginatingReason);
            Assert.AreEqual("100", ear.Offset);
            Assert.AreEqual(1, ear.SequenceNumber);
            Assert.AreEqual(true, ear.SkipProcessing);
            Assert.AreEqual(1, ear.Retries);
        }

        [Test]
        public async Task A110_PoisonMismatch()
        {
            var cfg = GetConfig();
            var lgr = TestSetUp.CreateLogger();

            var asrArgs = new EventHubsRepositoryArgs("testhub", cfg);

            var asr = new AzureStorageRepository(asrArgs, cfg, lgr)
            {
                PoisonTableName = cfg.GetValue<string>("EventHubPoisonMessagesTable"),
                AuditTableName = cfg.GetValue<string>("EventHubAuditMessagesTable")
            };

            // Make sure there are no AuditRecords to begin with.
            var pmt = await asr.GetPoisonMessageTableAsync().ConfigureAwait(false);
            await DeleteAuditRecords(pmt).ConfigureAwait(false);

            var amt = await asr.GetAuditMessageTableAsync().ConfigureAwait(false);
            await DeleteAuditRecords(amt).ConfigureAwait(false);

            var ed = CreateEventData("100", 1);

            // Add an event as poison.
            await asr.MarkAsPoisonedAsync(ed, HandleResult(Result.DataNotFound("Data not found."))).ConfigureAwait(false);
            Assert.AreEqual(PoisonMessageAction.PoisonRetry, await asr.CheckPoisonedAsync(ed).ConfigureAwait(false));
            Assert.AreEqual(1, (await GetAuditRecords(pmt).ConfigureAwait(false)).Count);
            Assert.AreEqual(0, (await GetAuditRecords(amt).ConfigureAwait(false)).Count);

            var ear = (await GetAuditRecords(pmt).ConfigureAwait(false)).First();
            Assert.AreEqual("unittest.servicebus.windows.net-testhub", ear.PartitionKey);
            Assert.AreEqual("$Default-0", ear.RowKey);
            Assert.NotNull(ear.Body);
            Assert.NotNull(ear.PoisonedTimeUtc);
            Assert.IsNull(ear.SkippedTimeUtc);
            Assert.AreEqual("DataNotFound", ear.Status);
            Assert.AreEqual("Data not found.", ear.Reason);
            Assert.IsNull(ear.OriginatingStatus);
            Assert.IsNull(ear.OriginatingReason);
            Assert.AreEqual("100", ear.Offset);
            Assert.AreEqual(1, ear.SequenceNumber);
            Assert.AreEqual(false, ear.SkipProcessing);
            Assert.AreEqual(0, ear.Retries);

            // Pretend to check a different event to that poisoned.
            ed = CreateEventData("200", 2);
            Assert.AreEqual(PoisonMessageAction.NotPoison, await asr.CheckPoisonedAsync(ed).ConfigureAwait(false));
            Assert.AreEqual(0, (await GetAuditRecords(pmt)).Count);
            Assert.AreEqual(1, (await GetAuditRecords(amt)).Count);

            ear = (await GetAuditRecords(amt).ConfigureAwait(false)).First();
            Assert.AreEqual("unittest.servicebus.windows.net-testhub", ear.PartitionKey);
            Assert.IsTrue(ear.RowKey.EndsWith("-$Default-0"));
            Assert.NotNull(ear.Body);
            Assert.NotNull(ear.PoisonedTimeUtc);
            Assert.IsNull(ear.SkippedTimeUtc);
            Assert.AreEqual("PoisonMismatch", ear.Status);
            Assert.AreEqual("Current EventData (Seq#: '2' Offset#: '200') being processed is out of sync with previous Poison (Seq#: '1' Offset#: '100'); current assumed correct with previous Poison now deleted.", ear.Reason);
            Assert.AreEqual("DataNotFound", ear.OriginatingStatus);
            Assert.AreEqual("Data not found.", ear.OriginatingReason);
            Assert.AreEqual("100", ear.Offset);
            Assert.AreEqual(1, ear.SequenceNumber);
            Assert.AreEqual(false, ear.SkipProcessing);
            Assert.AreEqual(0, ear.Retries);

            await Task.CompletedTask;
        }
    }
}