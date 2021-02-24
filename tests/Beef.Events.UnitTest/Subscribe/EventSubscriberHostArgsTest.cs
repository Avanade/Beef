using Beef.Test.NUnit;
using NUnit.Framework;
using System;

namespace Beef.Events.UnitTest.Subscribe
{
    [TestFixture]
    public class EventSubscriberHostArgsTest
    {
        [Test]
        public void Ctor_NoIEventSubscriber()
        {
            ExpectException.Throws<ArgumentException>("*", () => EventSubscriberHostArgs.Create());
            ExpectException.Throws<ArgumentException>("*", () => EventSubscriberHostArgs.Create((Type[])null));
            ExpectException.Throws<ArgumentException>("*", () => EventSubscriberHostArgs.Create(new Type[] { }));
        }

        [Test]
        public void Ctor_NoSubscribersInAssembly()
        {
            ExpectException.Throws<ArgumentException>("*", () => EventSubscriberHostArgs.Create(typeof(TestAttribute).Assembly));
        }

        [Test]
        public void Ctor_SubscribersInAssembly()
        {
            var args = EventSubscriberHostArgs.Create(GetType().Assembly);
            var ts = args.GetSubscriberTypes();
            Assert.AreEqual(2, ts.Length);
        }
    }
}