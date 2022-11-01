using Beef.Demo.Api;
using Beef.Demo.Business.Data;
using Beef.Demo.Business.Entities;
using CoreEx.Events;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using UnitTestEx;
using UnitTestEx.NUnit;

namespace Beef.Demo.Test
{
    [TestFixture, NonParallelizable]
    public class EventOutboxTest
    {
        [Test]
        public async Task A100_EnqueueAndDequeue()
        {
            TestSetUp.Default.SetUp();

            var ims = new InMemorySender();
            var test = ApiTester.Create<Startup>().ReplaceScoped<IEventSender>(_ => ims).ReplaceScoped<EventOutboxDequeue>();
            using var scope = test.Services.CreateScope();
            var eoe = scope.ServiceProvider.CreateInstance<EventOutboxEnqueue>();
            var eds = new CoreEx.Text.Json.EventDataSerializer();
            var ep = new EventPublisher(null, eds, eoe);

            ep.Publish(new EventData { Subject = "xxx" }, new EventData<int> { Subject = "yyy", Value = 88 }, new EventData<Person> { Subject = "zzz", Value = new Person { FirstName = "Josh" } });
            await ep.SendAsync().ConfigureAwait(false);

            var eod = scope.ServiceProvider.GetRequiredService<EventOutboxDequeue>();
            var r = await eod.DequeueAndSendAsync(2).ConfigureAwait(false);
            var es = ims.GetEvents();
            Assert.AreEqual(2, es.Length);
            Assert.AreEqual("xxx", es.First().Subject);
            Assert.AreEqual("yyy", es.Last().Subject);

            ims.Reset();
            r = await eod.DequeueAndSendAsync(2).ConfigureAwait(false);
            es = ims.GetEvents();
            Assert.AreEqual(1, es.Length);
            Assert.AreEqual("zzz", es.First().Subject);

            var ed = await eds.DeserializeAsync<Person>(es.First().Data).ConfigureAwait(false);
            Assert.IsNull(ed.Subject); // Reason is that only the value was serialized.
            Assert.AreEqual("Josh", ed.Value.FirstName);

            ims.Reset();
            r = await eod.DequeueAndSendAsync(2).ConfigureAwait(false);
            es = ims.GetEvents();
            Assert.AreEqual(0, es.Length);
        }
    }
}