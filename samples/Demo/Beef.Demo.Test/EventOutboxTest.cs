using Beef.Demo.Api;
using Beef.Demo.Business.Data;
using Beef.Demo.Business.Entities;
using CoreEx.Events;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using UnitTestEx;

namespace Beef.Demo.Test
{
    [TestFixture, NonParallelizable]
    public class EventOutboxTest
    {
        [Test]
        public async Task A100_EnqueueAndDequeue()
        {
            Assert.That(TestSetUp.Default.SetUp(), Is.True);

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
            Assert.That(es, Has.Length.EqualTo(2));
            Assert.Multiple(() =>
            {
                Assert.That(es.First().Subject, Is.EqualTo("xxx"));
                Assert.That(es.Last().Subject, Is.EqualTo("yyy"));
            });

            ims.Reset();
            r = await eod.DequeueAndSendAsync(2).ConfigureAwait(false);
            es = ims.GetEvents();
            Assert.That(es, Has.Length.EqualTo(1));
            Assert.That(es.First().Subject, Is.EqualTo("zzz"));

            var ed = await eds.DeserializeAsync<Person>(es.First().Data).ConfigureAwait(false);
            Assert.Multiple(() =>
            {
                Assert.That(ed.Subject, Is.Null); // Reason is that only the value was serialized.
                Assert.That(ed.Value.FirstName, Is.EqualTo("Josh"));
            });

            ims.Reset();
            r = await eod.DequeueAndSendAsync(2).ConfigureAwait(false);
            es = ims.GetEvents();
            Assert.That(es, Is.Empty);
        }
    }
}