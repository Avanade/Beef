using Beef.Events.Subscribe;
using Beef.Test.NUnit;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Beef.Events.UnitTest.Subscribe
{
    [TestFixture]
    public class EventDataSubscriberHostTest
    {
        public class TestSub : EventSubscriber
        {
            private readonly bool _throw;

            public TestSub() : this(RunAsUser.Originating) { }

            public TestSub(RunAsUser runAsUser = RunAsUser.Originating, bool @throw = false, UnhandledExceptionHandling unhandledExceptionHandling = UnhandledExceptionHandling.Continue) 
                : base("Test.Blah.*", "create", "update")
            {
                _throw = @throw;
                RunAsUser = runAsUser;
                UnhandledExceptionHandling = unhandledExceptionHandling;
            }

            public override Task ReceiveAsync(EventData eventData)
            {
                MessageReceived = true;
                Username = ExecutionContext.Current.Username;

                if (_throw)
                    throw new DivideByZeroException();

                return Task.CompletedTask;
            }

            public bool MessageReceived { get; set; }

            public string Username { get; set; }
        }

        public class TestSubS : EventSubscriber<string>
        {
            private readonly bool _throw;

            public TestSubS() : this(RunAsUser.Originating) { }

            public TestSubS(RunAsUser runAsUser = RunAsUser.Originating, bool @throw = false, UnhandledExceptionHandling unhandledExceptionHandling = UnhandledExceptionHandling.Continue)
                : base("Test.Blah.*", "create", "update")
            {
                _throw = @throw;
                RunAsUser = runAsUser;
                UnhandledExceptionHandling = unhandledExceptionHandling;
            }

            public override Task ReceiveAsync(EventData<string> eventData)
            {
                MessageReceived = true;
                Username = ExecutionContext.Current.Username;
                Value = eventData.Value;

                if (_throw)
                    throw new DivideByZeroException();

                return Task.CompletedTask;
            }

            public bool MessageReceived { get; set; }

            public string Username { get; set; }

            public string Value { get; set; }
        }

        [Test]
        public async Task A110_Unknown_Subject()
        {
            var ts = new TestSub();
            var ed = new EventData { Subject = "Other.Something", Action = "CREATE", Username = "TestUser" };
            await EventDataSubscriberHost.Create(new EventSubscriberHostArgs(TestSetUp.CreateLogger(), ts)).ReceiveAsync(ed);

            Assert.IsFalse(ts.MessageReceived);
        }

        [Test]
        public async Task A120_Unknown_Action()
        {
            var ts = new TestSub();
            var ed = new EventData { Subject = "Test.Blah.123", Action = "OTHER", Username = "TestUser" };
            await EventDataSubscriberHost.Create(new EventSubscriberHostArgs(TestSetUp.CreateLogger(), ts)).ReceiveAsync(ed);

            Assert.IsFalse(ts.MessageReceived);
        }

        [Test]
        public async Task A130_Receive_OK_OriginatingUser()
        {
            var ts = new TestSub(RunAsUser.Originating);
            var ed = new EventData { Subject = "Test.Blah.123", Action = "CREATE", Username = "TestUser" };
            await EventDataSubscriberHost.Create(new EventSubscriberHostArgs(TestSetUp.CreateLogger(), ts)).ReceiveAsync(ed);

            Assert.IsTrue(ts.MessageReceived);
            Assert.AreEqual("TestUser", ts.Username);
        }

        [Test]
        public async Task A140_Receive_OK_SystemUser()
        {
            EventSubscriberHost.SystemUsername = "SystemUser";
            var ts = new TestSub(RunAsUser.System);
            var ed = new EventData { Subject = "Test.Blah.123", Action = "CREATE", Username = "TestUser" };
            await EventDataSubscriberHost.Create(new EventSubscriberHostArgs(TestSetUp.CreateLogger(), ts)).ReceiveAsync(ed);

            Assert.IsTrue(ts.MessageReceived);
            Assert.AreEqual("SystemUser", ts.Username);
        }

        [Test]
        public async Task A150_Receive_OK_ExceptionContinue()
        {
            var ts = new TestSub(@throw: true, unhandledExceptionHandling: UnhandledExceptionHandling.Continue);
            var ed = new EventData { Subject = "Test.Blah.123", Action = "UPDATE", Username = "TestUser" };
            await EventDataSubscriberHost.Create(new EventSubscriberHostArgs(TestSetUp.CreateLogger(), ts)).ReceiveAsync(ed);

            Assert.IsTrue(ts.MessageReceived);
        }

        [Test]
        public async Task A160_Receive_OK_ExceptionStop()
        {
            var ts = new TestSub(@throw: true, unhandledExceptionHandling: UnhandledExceptionHandling.Stop);
            var ed = new EventData { Subject = "Test.Blah.123", Action = "CREATE", Username = "TestUser" };

            try
            {
                await EventDataSubscriberHost.Create(new EventSubscriberHostArgs(TestSetUp.CreateLogger(), ts)).ReceiveAsync(ed);
            }
            catch (DivideByZeroException)
            {
                Assert.IsTrue(ts.MessageReceived);
                return;
            }

            Assert.Fail();
        }

        [Test]
        public async Task B110_Unknown_Subject()
        {
            var ts = new TestSubS();
            var ed = new EventData<string> { Subject = "Other.Something", Action = "CREATE", Username = "TestUser", Value = "TEST" };
            await EventDataSubscriberHost.Create(new EventSubscriberHostArgs(TestSetUp.CreateLogger(), ts)).ReceiveAsync(ed);

            Assert.IsFalse(ts.MessageReceived);
        }

        [Test]
        public async Task B120_Unknown_Action()
        {
            var ts = new TestSubS();
            var ed = new EventData<string> { Subject = "Test.Blah.123", Action = "OTHER", Username = "TestUser", Value = "TEST" };
            await EventDataSubscriberHost.Create(new EventSubscriberHostArgs(TestSetUp.CreateLogger(), ts)).ReceiveAsync(ed);

            Assert.IsFalse(ts.MessageReceived);
        }

        [Test]
        public async Task B130_Receive_OK_OriginatingUser()
        {
            var ts = new TestSubS(RunAsUser.Originating);
            var ed = new EventData<string> { Subject = "Test.Blah.123", Action = "CREATE", Username = "TestUser", Value = "TEST" };
            await EventDataSubscriberHost.Create(new EventSubscriberHostArgs(TestSetUp.CreateLogger(), ts)).ReceiveAsync(ed);

            Assert.IsTrue(ts.MessageReceived);
            Assert.AreEqual("TestUser", ts.Username);
            Assert.AreEqual("TEST", ts.Value);
            Assert.AreEqual(typeof(string), ts.ValueType);
        }

        [Test]
        public async Task B140_Receive_OK_SystemUser()
        {
            EventSubscriberHost.SystemUsername = "SystemUser";
            var ts = new TestSubS(RunAsUser.System);
            var ed = new EventData<string> { Subject = "Test.Blah.123", Action = "UPDATE", Username = "TestUser", Value = "TEST" };
            await EventDataSubscriberHost.Create(new EventSubscriberHostArgs(TestSetUp.CreateLogger(), ts)).ReceiveAsync(ed);

            Assert.IsTrue(ts.MessageReceived);
            Assert.AreEqual("SystemUser", ts.Username);
            Assert.AreEqual("TEST", ts.Value);
            Assert.AreEqual(typeof(string), ts.ValueType);
        }

        [Test]
        public async Task B150_Receive_OK_ExceptionContinue()
        {
            var ts = new TestSubS(@throw: true, unhandledExceptionHandling: UnhandledExceptionHandling.Continue);
            var ed = new EventData<string> { Subject = "Test.Blah.123", Action = "CREATE", Username = "TestUser", Value = "TEST" };
            await EventDataSubscriberHost.Create(new EventSubscriberHostArgs(TestSetUp.CreateLogger(), ts)).ReceiveAsync(ed);

            Assert.IsTrue(ts.MessageReceived);
            Assert.AreEqual("TEST", ts.Value);
            Assert.AreEqual(typeof(string), ts.ValueType);
        }

        [Test]
        public async Task B160_Receive_OK_ExceptionStop()
        {
            var ts = new TestSubS(@throw: true, unhandledExceptionHandling: UnhandledExceptionHandling.Stop);
            var ed = new EventData<string> { Subject = "Test.Blah.123", Action = "CREATE", Username = "TestUser", Value = "TEST" };

            try
            {
                await EventDataSubscriberHost.Create(new EventSubscriberHostArgs(TestSetUp.CreateLogger(), ts)).ReceiveAsync(ed);
            }
            catch (DivideByZeroException)
            {
                Assert.IsTrue(ts.MessageReceived);
                Assert.AreEqual("TEST", ts.Value);
                Assert.AreEqual(typeof(string), ts.ValueType);
                return;
            }

            Assert.Fail();
        }

        [Test]
        public void C110_TooManySubjectSubscribers()
        {
            var ed = new EventData<string> { Subject = "Test.Blah.123", Action = "CREATE", Username = "TestUser", Value = "TEST" };

            ExpectException.Throws<EventSubscriberException>(
                "There are 2 IEventSubscriber instances subscribing to Subject 'Test.Blah.123' and Action 'CREATE'; there must be only a single subscriber.",
                async () => await EventDataSubscriberHost.Create(new EventSubscriberHostArgs(TestSetUp.CreateLogger(), new TestSub(), new TestSubS())).ReceiveAsync(ed));
        }

        [Test]
        public void C120_DoNotAllowMultipleMessages()
        {
            ExpectException.Throws<EventSubscriberException>(
                "The EventDataSubscriberHost does not AllowMultipleMessages; there were 2 event messages.",
                async () => await EventDataSubscriberHost.Create(new EventSubscriberHostArgs(TestSetUp.CreateLogger(), new TestSub())).ReceiveAsync(new EventData(), new EventData()));
        }

        [Test]
        public async Task C130_AllowMultipleMessages()
        {
            await EventDataSubscriberHost.Create(new EventSubscriberHostArgs(TestSetUp.CreateLogger(), new TestSub())).AllowMultipleMessages().ReceiveAsync(new EventData { Subject = "X" }, new EventData { Subject = "X" });
        }
    }
}