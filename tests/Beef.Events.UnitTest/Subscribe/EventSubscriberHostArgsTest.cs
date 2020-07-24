using Beef.Events.Subscribe;
using Beef.Test.NUnit;
using NUnit.Framework;
using System;
using System.Linq;

namespace Beef.Events.UnitTest.Subscribe
{
    [TestFixture]
    public class EventSubscriberHostArgsTest
    {
        [Test]
        public void Ctor_NoIEventSubscriber()
        {
            var sp = TestSetUp.CreateServiceProvider();
            ExpectException.Throws<ArgumentException>("*", () => EventSubscriberHostArgs.Create(sp));
            ExpectException.Throws<ArgumentException>("*", () => EventSubscriberHostArgs.Create(sp, (Type[])null));
            ExpectException.Throws<ArgumentException>("*", () => EventSubscriberHostArgs.Create(sp, new Type[] { }));
        }

        [Test]
        public void Ctor_NoSubscribersInAssembly()
        {
            var sp = TestSetUp.CreateServiceProvider();
            ExpectException.Throws<ArgumentException>("*", () => EventSubscriberHostArgs.Create(sp, typeof(TestAttribute).Assembly));
        }

        [Test]
        public void Ctor_SubscribersInAssembly()
        {
            var ts = EventSubscriberHostArgs.GetSubscriberTypes(this.GetType().Assembly);
            Assert.AreEqual(2, ts.Length);
        }
    }
}