using Beef.Events.Subscribe;
using Beef.Test.NUnit;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Beef.Events.UnitTest.Subscribe
{
    [TestFixture]
    public class EventDataSubscriberHostTest
    {
        [EventSubscriber("Test.Blah.*", "create", "update")]
        public class TestSub : EventSubscriber
        {
            private readonly bool _throw;

            public TestSub() : this(RunAsUser.Originating) { }

            public TestSub(RunAsUser runAsUser = RunAsUser.Originating, bool @throw = false, UnhandledExceptionHandling unhandledExceptionHandling = UnhandledExceptionHandling.Continue) 
            {
                _throw = @throw;
                RunAsUser = runAsUser;
                UnhandledExceptionHandling = unhandledExceptionHandling;
            }

            public override Task<Result> ReceiveAsync(EventData eventData)
            {
                MessageReceived = true;
                Username = ExecutionContext.Current.Username;

                if (_throw)
                    throw new DivideByZeroException();

                return Task.FromResult(Result.Success());
            }

            public bool MessageReceived { get; set; }

            public string Username { get; set; }
        }

        [EventSubscriber("Test.Blah.*", "create", "update")]
        public class TestSubS : EventSubscriber<string>
        {
            private readonly bool _throw;

            public TestSubS() : this(RunAsUser.Originating) { }

            public TestSubS(RunAsUser runAsUser = RunAsUser.Originating, bool @throw = false, UnhandledExceptionHandling unhandledExceptionHandling = UnhandledExceptionHandling.Continue)
            {
                _throw = @throw;
                RunAsUser = runAsUser;
                UnhandledExceptionHandling = unhandledExceptionHandling;
            }

            public override Task<Result> ReceiveAsync(EventData<string> eventData)
            {
                MessageReceived = true;
                Username = ExecutionContext.Current.Username;
                Value = eventData.Value;

                if (_throw)
                    throw new DivideByZeroException();

                return Task.FromResult(Result.Success());
            }

            public bool MessageReceived { get; set; }

            public string Username { get; set; }

            public string Value { get; set; }
        }

        private EventDataSubscriberHost CreateTestHost<T>(Func<T> create) where T : class
        {
            var sp = TestSetUp.CreateServiceProvider(sc => sc.AddTransient(_ => create()));
            return new EventDataSubscriberHost(EventSubscriberHostArgs.Create(typeof(T)).UseServiceProvider(sp)).UseLogger(TestSetUp.CreateLogger());
        }

        [Test]
        public async Task A110_Unknown_Subject()
        {
            var ts = new TestSub();
            var ed = new EventData { Subject = "Other.Something", Action = "CREATE", Username = "TestUser" };
            await CreateTestHost(() => ts).ReceiveAsync(ed);

            Assert.IsFalse(ts.MessageReceived);
        }

        [Test]
        public async Task A120_Unknown_Action()
        {
            var ts = new TestSub();
            var ed = new EventData { Subject = "Test.Blah.123", Action = "OTHER", Username = "TestUser" };
            await CreateTestHost(() => ts).ReceiveAsync(ed);

            Assert.IsFalse(ts.MessageReceived);
        }

        [Test]
        public async Task A130_Receive_OK_OriginatingUser()
        {
            var ts = new TestSub(RunAsUser.Originating);
            var ed = new EventData { Subject = "Test.Blah.123", Action = "CREATE", Username = "TestUser" };
            await CreateTestHost(() => ts).ReceiveAsync(ed);

            Assert.IsTrue(ts.MessageReceived);
            Assert.AreEqual("TestUser", ts.Username);
        }

        [Test]
        public async Task A140_Receive_OK_SystemUser()
        {
            EventSubscriberHost.SystemUsername = "SystemUser";
            var ts = new TestSub(RunAsUser.System);
            var ed = new EventData { Subject = "Test.Blah.123", Action = "CREATE", Username = "TestUser" };
            await CreateTestHost(() => ts).ReceiveAsync(ed);

            Assert.IsTrue(ts.MessageReceived);
            Assert.AreEqual("SystemUser", ts.Username);
        }

        [Test]
        public async Task A150_Receive_OK_ExceptionContinue()
        {
            var ts = new TestSub(@throw: true, unhandledExceptionHandling: UnhandledExceptionHandling.Continue);
            var ed = new EventData { Subject = "Test.Blah.123", Action = "UPDATE", Username = "TestUser" };
            await CreateTestHost(() => ts).ReceiveAsync(ed);

            Assert.IsTrue(ts.MessageReceived);
        }

        [Test]
        public async Task A160_Receive_OK_ExceptionStop()
        {
            var ts = new TestSub(@throw: true, unhandledExceptionHandling: UnhandledExceptionHandling.ThrowException);
            var ed = new EventData { Subject = "Test.Blah.123", Action = "CREATE", Username = "TestUser" };

            try
            {
                await CreateTestHost(() => ts).ReceiveAsync(ed);
            }
            catch (EventSubscriberUnhandledException esuex)
            {
                if (esuex.InnerException == null || !(esuex.InnerException is DivideByZeroException))
                    Assert.Fail();

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
            await CreateTestHost(() => ts).ReceiveAsync(ed);

            Assert.IsFalse(ts.MessageReceived);
        }

        [Test]
        public async Task B120_Unknown_Action()
        {
            var ts = new TestSubS();
            var ed = new EventData<string> { Subject = "Test.Blah.123", Action = "OTHER", Username = "TestUser", Value = "TEST" };
            await CreateTestHost(() => ts).ReceiveAsync(ed);

            Assert.IsFalse(ts.MessageReceived);
        }

        [Test]
        public async Task B130_Receive_OK_OriginatingUser()
        {
            var ts = new TestSubS(RunAsUser.Originating);
            var ed = new EventData<string> { Subject = "Test.Blah.123", Action = "CREATE", Username = "TestUser", Value = "TEST" };
            await CreateTestHost(() => ts).ReceiveAsync(ed);

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
            await CreateTestHost(() => ts).ReceiveAsync(ed);

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
            await CreateTestHost(() => ts).ReceiveAsync(ed);

            Assert.IsTrue(ts.MessageReceived);
            Assert.AreEqual("TEST", ts.Value);
            Assert.AreEqual(typeof(string), ts.ValueType);
        }

        [Test]
        public async Task B160_Receive_OK_ExceptionStop()
        {
            var ts = new TestSubS(@throw: true, unhandledExceptionHandling: UnhandledExceptionHandling.ThrowException);
            var ed = new EventData<string> { Subject = "Test.Blah.123", Action = "CREATE", Username = "TestUser", Value = "TEST" };

            try
            {
                await CreateTestHost(() => ts).ReceiveAsync(ed);
            }
            catch (EventSubscriberUnhandledException esuex)
            {
                if (esuex.InnerException == null || !(esuex.InnerException is DivideByZeroException))
                    Assert.Fail();

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
            var sp = TestSetUp.CreateServiceProvider();

            ExpectException.Throws<EventSubscriberException>(
                "There are 2 IEventSubscriber instances subscribing to Subject 'Test.Blah.123' and Action 'CREATE'; there must be only a single subscriber.",
                async () => await new EventDataSubscriberHost(EventSubscriberHostArgs.Create(typeof(TestSub), typeof(TestSubS)).UseServiceProvider(sp)).ReceiveAsync(ed));
        }

        [Test]
        public void C120_DoNotAllowMultipleMessages()
        {
            var sp = TestSetUp.CreateServiceProvider();

            ExpectException.Throws<EventSubscriberException>(
                "The 'EventDataSubscriberHost' does not AllowMultipleMessages; there were 2 event messages.",
                async () => await new EventDataSubscriberHost(EventSubscriberHostArgs.Create(typeof(TestSub)).UseServiceProvider(sp)).ReceiveAsync(new EventData(), new EventData()));
        }

        [Test]
        public async Task C130_AllowMultipleMessages()
        {
            var sp = TestSetUp.CreateServiceProvider();

            await new EventDataSubscriberHost(EventSubscriberHostArgs.Create(typeof(TestSub)).UseServiceProvider(sp).AllowMultipleMessages()).ReceiveAsync(new EventData { Subject = "X" }, new EventData { Subject = "X" });
        }
    }
}