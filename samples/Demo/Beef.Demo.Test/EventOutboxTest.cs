using Beef.Data.Database;
using Beef.Demo.Api;
using Beef.Demo.Business.Data;
using Beef.Demo.Common.Entities;
using Beef.Events;
using Beef.Test.NUnit;
using Beef.Test.NUnit.Events;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.Demo.Test
{
    [TestFixture, NonParallelizable]
    public class EventOutboxTest : UsingAgentTesterServer<Startup>
    {
        [Test, TestSetUp]
        public async Task A100_EnqueueAndDequeue()
        {
            var db = new Beef.Demo.Business.Data.Database(BuildConfiguration()["ConnectionStrings:BeefDemo"]);
            var ep = new ExpectEventPublisher();
            var eo = new DatabaseEventOutbox();

            await db.EventOutboxInvoker.InvokeAsync(new object(), () => 
            {
                ep.Publish(new EventData { Subject = "xxx" }, new EventData<int> { Subject = "yyy", Value = 88 }, new EventData<Person> { Subject = "zzz", Value = new Person { FirstName = "Josh" } });
                return Task.CompletedTask;
            }, new DatabaseEventOutboxInvokerArgs { EventOutbox = eo, EventPublisher = ep });

            var eois = await eo.DequeueAsync(db, 2);
            Assert.IsNotNull(eois);
            Assert.AreEqual(2, eois.Count());
            Assert.AreEqual("xxx", eois.First().Subject);
            Assert.AreEqual("yyy", eois.Last().Subject);

            var eoi = eois.Last();
            var edi = (EventData<int>)eoi.ToEventData();
            Assert.AreEqual(88, edi.Value);

            eois = await eo.DequeueAsync(db, 2);
            Assert.IsNotNull(eois);
            Assert.AreEqual(1, eois.Count());
            Assert.AreEqual("zzz", eois.First().Subject);

            eoi = eois.Last();
            var edp = (EventData<Person>)eoi.ToEventData();
            Assert.AreEqual("Josh", edp.Value.FirstName);

            eois = await eo.DequeueAsync(db, 2);
            Assert.IsNotNull(eois);
            Assert.AreEqual(0, eois.Count());
        }
    }
}