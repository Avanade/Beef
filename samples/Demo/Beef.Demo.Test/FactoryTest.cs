using Beef.Demo.Business;
using Beef.Demo.Business.Data;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Agents.ServiceAgents;
using Beef.Demo.Common.Entities;
using Beef.Test.NUnit;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Beef.Demo.Test
{
    /// <summary>
    /// Tests to validate that the parallelizable Factory/Mocking capability works as expected. 
    /// </summary>
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class FactoryTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestSetUp.Reset();
        }

        [Test, TestSetUp()]
        public void Manager_TestA()
        {
            Console.WriteLine($"Start: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            TestSetUp.CreateMock<IPersonManager>().Setup(x => x.GetAsync(It.IsAny<Guid>())).ReturnsAsync(new Person { FirstName = "A" });
            System.Threading.Thread.Sleep(100);
            var v = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(System.Net.HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(1.ToGuid())).Value;

            Console.WriteLine($"Stopping: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            Assert.AreEqual("A", v.FirstName);
        }

        [Test, TestSetUp()]
        public void Manager_TestB()
        {
            Console.WriteLine($"Start: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            TestSetUp.CreateMock<IPersonManager>().Setup(x => x.GetAsync(It.IsAny<Guid>())).ReturnsAsync(new Person { FirstName = "B" });
            System.Threading.Thread.Sleep(100);
            var v = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(System.Net.HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(1.ToGuid())).Value;

            Console.WriteLine($"Stopping: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            Assert.AreEqual("B", v.FirstName);
        }

        [Test, TestSetUp()]
        public void Manager_TestC()
        {
            Console.WriteLine($"Start: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            TestSetUp.CreateMock<IPersonManager>().Setup(x => x.GetAsync(It.IsAny<Guid>())).ReturnsAsync(new Person { FirstName = "C" });
            System.Threading.Thread.Sleep(100);
            var v = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(System.Net.HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(1.ToGuid())).Value;

            Console.WriteLine($"Stopping: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            Assert.AreEqual("C", v.FirstName);
        }

        [Test, TestSetUp()]
        public void Manager_TestD()
        {
            Console.WriteLine($"Start: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            TestSetUp.CreateMock<IPersonManager>().Setup(x => x.GetAsync(It.IsAny<Guid>())).ReturnsAsync(new Person { FirstName = "D" });
            System.Threading.Thread.Sleep(100);
            var v = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(System.Net.HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(1.ToGuid())).Value;

            Console.WriteLine($"Stopping: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            Assert.AreEqual("D", v.FirstName);
        }

        [Test, TestSetUp()]
        public void Manager_TestE()
        {
            Console.WriteLine($"Start: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            TestSetUp.CreateMock<IPersonManager>().Setup(x => x.GetAsync(It.IsAny<Guid>())).ReturnsAsync(new Person { FirstName = "E" });
            System.Threading.Thread.Sleep(100);
            var v = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(System.Net.HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(1.ToGuid())).Value;

            Console.WriteLine($"Stopping: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            Assert.AreEqual("E", v.FirstName);
        }

        [Test, TestSetUp()]
        public void Manager_TestF()
        {
            Console.WriteLine($"Start: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            TestSetUp.CreateMock<IPersonManager>().Setup(x => x.GetAsync(It.IsAny<Guid>())).ReturnsAsync(new Person { FirstName = "F" });
            System.Threading.Thread.Sleep(100);
            var v = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(System.Net.HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(1.ToGuid())).Value;

            Console.WriteLine($"Stopping: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            Assert.AreEqual("F", v.FirstName);
        }

        [Test, TestSetUp()]
        public void ServiceAgent_TestA()
        {
            Console.WriteLine($"Start: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            var m = TestSetUp.CreateMock<IPersonServiceAgent>();
            m.Setup(x => x.GetAsync(It.IsAny<Guid>(), null)).ReturnsWebApiAgentResultAsync(new Person { FirstName = "A" });

            System.Threading.Thread.Sleep(100);
            Assert.AreSame(m.Object, Factory.Create<IPersonServiceAgent>());

            var v = new PersonAgent().GetAsync(1.ToGuid()).Result.Value;

            Console.WriteLine($"Stopping: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            Assert.AreEqual("A", v.FirstName);
        }

        [Test, TestSetUp()]
        public void ServiceAgent_TestB()
        {
            Console.WriteLine($"Start: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            var m = TestSetUp.CreateMock<IPersonServiceAgent>();
            m.Setup(x => x.GetAsync(It.IsAny<Guid>(), null)).ReturnsWebApiAgentResultAsync(new Person { FirstName = "B" });

            System.Threading.Thread.Sleep(100);
            Assert.AreSame(m.Object, Factory.Create<IPersonServiceAgent>());

            var v = new PersonAgent().GetAsync(1.ToGuid()).Result.Value;

            Console.WriteLine($"Stopping: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            Assert.AreEqual("B", v.FirstName);
        }

        [Test, TestSetUp()]
        public void ServiceAgent_TestC()
        {
            Console.WriteLine($"Start: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            var m = TestSetUp.CreateMock<IPersonServiceAgent>();
            m.Setup(x => x.GetAsync(It.IsAny<Guid>(), null)).ReturnsWebApiAgentResultAsync(new Person { FirstName = "C" });

            System.Threading.Thread.Sleep(100);
            Assert.AreSame(m.Object, Factory.Create<IPersonServiceAgent>());

            var v = new PersonAgent().GetAsync(1.ToGuid()).Result.Value;

            Console.WriteLine($"Stopping: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            Assert.AreEqual("C", v.FirstName);
        }

        [Test, TestSetUp()]
        public void ServiceAgent_TestD()
        {
            Console.WriteLine($"Start: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            var m = TestSetUp.CreateMock<IPersonServiceAgent>();
            m.Setup(x => x.GetAsync(It.IsAny<Guid>(), null)).ReturnsWebApiAgentResultAsync(new Person { FirstName = "D" });

            System.Threading.Thread.Sleep(100);
            Assert.AreSame(m.Object, Factory.Create<IPersonServiceAgent>());

            var v = new PersonAgent().GetAsync(1.ToGuid()).Result.Value;

            Console.WriteLine($"Stopping: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            Assert.AreEqual("D", v.FirstName);
        }

        [Test, TestSetUp()]
        public async Task ServiceAgent_TestE()
        {
            Console.WriteLine($"Start: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            var m = TestSetUp.CreateMock<IPersonServiceAgent>();
            m.Setup(x => x.GetAsync(It.IsAny<Guid>(), null)).ReturnsWebApiAgentResultAsync(new Person { FirstName = "E" });

            System.Threading.Thread.Sleep(100);
            Console.WriteLine($"Woken: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            Assert.AreSame(m.Object, Factory.Create<IPersonServiceAgent>());

            var v = (await new PersonAgent().GetAsync(1.ToGuid())).Value;

            Assert.AreEqual("E", v.FirstName);
        }

        [Test, TestSetUp()]
        public void ServiceAgent_TestF()
        {
            Console.WriteLine($"Start: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            var m = TestSetUp.CreateMock<IPersonServiceAgent>();
            m.Setup(x => x.GetAsync(It.IsAny<Guid>(), null)).ReturnsWebApiAgentResultAsync(new Person { FirstName = "F" });

            System.Threading.Thread.Sleep(100);
            Assert.AreSame(m.Object, Factory.Create<IPersonServiceAgent>());

            var v = new PersonAgent().GetAsync(1.ToGuid()).Result.Value;

            Console.WriteLine($"Stopping: {DateTime.Now.ToString("HH:mm:ss.ffffff")} [{System.Threading.Thread.CurrentThread.ManagedThreadId}]");
            Assert.AreEqual("F", v.FirstName);
        }
    }
}