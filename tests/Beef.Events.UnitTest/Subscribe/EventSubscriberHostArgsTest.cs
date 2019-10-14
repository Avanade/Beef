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
            ExpectException.Throws<ArgumentNullException>("*", () => new EventSubscriberHostArgs(TestSetUp.CreateLogger(), (IEventSubscriber[])null));
            ExpectException.Throws<ArgumentException>("*", () => new EventSubscriberHostArgs(TestSetUp.CreateLogger(), new IEventSubscriber[] { }));
        }

        [Test]
        public void Ctor_NoSubscribersInAssembly()
        {
            ExpectException.Throws<ArgumentException>("*", () => new EventSubscriberHostArgs(TestSetUp.CreateLogger(), typeof(TestAttribute).Assembly));
        }

        [Test]
        public void Ctor_SubscribersInAssembly()
        {
            var args = new EventSubscriberHostArgs(TestSetUp.CreateLogger(), this.GetType().Assembly);
            Assert.AreEqual(2, args.EventSubscribers.Count());

            args = new EventSubscriberHostArgs(TestSetUp.CreateLogger());
            Assert.AreEqual(2, args.EventSubscribers.Count());
        }
    }
}