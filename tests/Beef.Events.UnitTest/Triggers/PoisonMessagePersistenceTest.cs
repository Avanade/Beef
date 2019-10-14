using Beef.Events.Triggers.PoisonMessages;
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
using EventHubs = Microsoft.Azure.EventHubs;

namespace Beef.Events.UnitTest.Triggers
{
    [TestFixture]
    public class PoisonMessagePersistenceTest
    {
        private IConfiguration _config;

        private IConfiguration GetConfig()
        {
            if (_config == null)
            {
                _config = new ConfigurationBuilder()
                    .AddJsonFile(new EmbeddedFileProvider(typeof(PoisonMessagePersistenceTest).Assembly), "webjobs.settings.json", true, false)
                    .Build();
            }

            return _config;
        }

        private static EventHubs.EventData CreateEventData(string offset, long seqNo)
        {
            var e = new EventHubs.EventData(Encoding.UTF8.GetBytes(offset));

            var type = typeof(EventHubs.EventData);
            var pi = type.GetProperty("SystemProperties");
            pi.SetValue(e, Activator.CreateInstance(pi.PropertyType, true));

            var dict = (Dictionary<string, object>)e.SystemProperties;
            dict.Add("x-opt-enqueued-time", DateTime.UtcNow);
            dict.Add("x-opt-offset", offset);
            dict.Add("x-opt-sequence-number", seqNo);

            return e;
        }

        public static async Task<List<PoisonMessage>> GetSkippedMessages(CloudTable cst)
        {
            var r = await cst.ExecuteQuerySegmentedAsync(new TableQuery<PoisonMessage>(), null);
            return r.Results;
        }

        [Test]
        public async Task A100_EndToEnd()
        {
            var tn = GetConfig().GetValue<string>("EventHubPoisonMessageTable");
            PoisonMessagePersistence.DefaultTableName = tn;

            var cs = GetConfig().GetValue<string>("AzureWebJobsStorage");
            var ct = await PoisonMessagePersistence.GetPoisonMessageTable(cs);
            var cst = await PoisonMessagePersistence.GetPoisonMessageSkippedTable(cs);
            var smc = (await GetSkippedMessages(cst)).Count;

            // Make sure there are no messages to begin with.
            var msgs = await PoisonMessagePersistence.GetAllMessagesAsync(ct);
            foreach (var m in msgs)
            {
                await ct.ExecuteAsync(TableOperation.Delete(m));
            }

            var pp = new PoisonMessagePersistence(new PoisonMessageCreatePersistenceArgs
            {
                Config = GetConfig(),
                Context = ResilientEventHubProcessorTest.CreatePartitionContext(),
                Logger = TestSetUp.CreateLogger(),
                Options = ResilientEventHubProcessorTest.CreateOptions()
            });

            var ed = CreateEventData("100", 1);

            // Checking and removing with unknown is a-ok.
            Assert.AreEqual(PoisonMessageAction.NotPoison, await pp.CheckAsync(ed));
            await pp.RemoveAsync(ed, PoisonMessageAction.NotPoison);
            Assert.AreEqual(smc, (await GetSkippedMessages(cst)).Count);

            // Add an event as poison.
            await pp.SetAsync(ed, new DivideByZeroException("My bad."));
            Assert.AreEqual(PoisonMessageAction.PoisonRetry, await pp.CheckAsync(ed));
            Assert.AreEqual(smc, (await GetSkippedMessages(cst)).Count);

            msgs = await PoisonMessagePersistence.GetAllMessagesAsync(ct);
            Assert.AreEqual(1, msgs.Count());

            var msg = msgs.First();
            Assert.AreEqual("path-eventhub", msg.PartitionKey);
            Assert.AreEqual("consumergroup-0", msg.RowKey);
            Assert.AreEqual("100", msg.Body);
            Assert.AreEqual("System.DivideByZeroException: My bad.", msg.Exception);
            Assert.AreEqual("100", msg.Offset);
            Assert.AreEqual(1, msg.SequenceNumber);
            Assert.AreEqual(false, msg.SkipMessage);
            Assert.AreEqual("ns.class", msg.FunctionType);
            Assert.AreEqual("testfunc", msg.FunctionName);

            // Update to skip.
            await PoisonMessagePersistence.SkipMessageAsync(ct, msg.PartitionKey, msg.RowKey);
            Assert.AreEqual(PoisonMessageAction.PoisonSkip, await pp.CheckAsync(ed));
            Assert.AreEqual(smc, (await GetSkippedMessages(cst)).Count);

            msgs = await PoisonMessagePersistence.GetAllMessagesAsync(ct);
            Assert.AreEqual(1, msgs.Count());

            msg = msgs.First();
            Assert.AreEqual("path-eventhub", msg.PartitionKey);
            Assert.AreEqual("consumergroup-0", msg.RowKey);
            Assert.AreEqual("100", msg.Body);
            Assert.AreEqual("System.DivideByZeroException: My bad.", msg.Exception);
            Assert.AreEqual("100", msg.Offset);
            Assert.AreEqual(1, msg.SequenceNumber);
            Assert.AreEqual(true, msg.SkipMessage);
            Assert.AreEqual("ns.class", msg.FunctionType);
            Assert.AreEqual("testfunc", msg.FunctionName);
            Assert.IsNull(msg.SkippedTimeUtc);

            // Remove the poison as no longer poison.
            await pp.RemoveAsync(ed, PoisonMessageAction.NotPoison);
            Assert.AreEqual(PoisonMessageAction.NotPoison, await pp.CheckAsync(ed));
            Assert.AreEqual(smc, (await GetSkippedMessages(cst)).Count);

            msgs = await PoisonMessagePersistence.GetAllMessagesAsync(ct);
            Assert.AreEqual(0, msgs.Count());

            // Create a new poison message.
            ed = CreateEventData("200", 2);
            await pp.SetAsync(ed, new DivideByZeroException("My bad."));
            Assert.AreEqual(PoisonMessageAction.PoisonRetry, await pp.CheckAsync(ed));
            Assert.AreEqual(smc, (await GetSkippedMessages(cst)).Count);

            msgs = await PoisonMessagePersistence.GetAllMessagesAsync(ct);
            Assert.AreEqual(1, msgs.Count());
            msg = msgs.First();

            // Remove the poison as skipped (poison).
            await pp.RemoveAsync(ed, PoisonMessageAction.PoisonSkip);
            Assert.AreEqual(PoisonMessageAction.NotPoison, await pp.CheckAsync(ed));

            var sms = await GetSkippedMessages(cst);
            Assert.AreEqual(smc + 1, sms.Count);

            var sm = sms.Where(pm => pm.PartitionKey == msg.PartitionKey && pm.RowKey.EndsWith(msg.RowKey)).OrderByDescending(pm => pm.RowKey).FirstOrDefault();
            Assert.IsNotNull(sm.SkippedTimeUtc);
        }
    }
}