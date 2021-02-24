using Beef.Demo.Business;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Demo.Functions;
using Beef.Demo.Functions.Subscribers;
using Beef.Events;
using Beef.Test.NUnit;
using Moq;
using NUnit.Framework;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Beef.Demo.Test
{
    [TestFixture]
    public class SubscribersTest : UsingAgentTesterServer<Api.Startup>
    {
        private readonly RobotTest _robotTest = new RobotTest();

        [OneTimeSetUp]
        public async Task OneTimeSetUp() => await _robotTest.CosmosOneTimeSetUp();

        [OneTimeTearDown]
        public async Task OneTimeTearDown() => await _robotTest.OneTimeTearDown();

        [Test, TestSetUp]
        public async Task A110_PowerSourceChangeSubscriber_NoUpdated()
        {
            var r = (await AgentTester.Test<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .RunAsync(a => a.GetAsync(1.ToGuid()))).Value;

            await EventSubscriberTester.Create<Startup>()
                .ExpectResult(SubscriberStatus.Success)
                .ExpectNoEvents()
                .RunAsync<PowerSourceChangeSubscriber>(EventData.CreateValueEvent(r.PowerSourceSid, $"Demo.Robot.{r.Id}", "PowerSourceChange", r.Id));

            await AgentTester.Test<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => r)
                .RunAsync(a => a.GetAsync(1.ToGuid()));
        }

        [Test, TestSetUp]
        public async Task A120_PowerSourceChangeSubscriber_Updated()
        {
            var r = (await AgentTester.Test<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .RunAsync(a => a.GetAsync(1.ToGuid()))).Value;

            r.PowerSourceSid = "N";

            await EventSubscriberTester.Create<Startup>()
                .ExpectResult(SubscriberStatus.Success)
                .ExpectEvent($"Demo.Robot.{r.Id}", "Update")
                .RunAsync<PowerSourceChangeSubscriber>(EventData.CreateValueEvent(r.PowerSourceSid, $"Demo.Robot.{r.Id}", "PowerSourceChange", r.Id));

            await AgentTester.Test<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .ExpectETag(r.ETag)
                .ExpectValue(_ => r)
                .RunAsync(a => a.GetAsync(1.ToGuid()));
        }

        [Test, TestSetUp]
        public async Task A130_PowerSourceChangeSubscriber_NotFound()
        {
            await EventSubscriberTester.Create<Startup>()
                .ExpectResult(SubscriberStatus.DataNotFound)
                .RunAsync<PowerSourceChangeSubscriber>(EventData.CreateValueEvent("N", $"Demo.Robot.{404.ToGuid()}", "PowerSourceChange", 404.ToGuid()));
        }

        [Test, TestSetUp]
        public async Task A140_PowerSourceChangeSubscriber_InvalidData_Key()
        {
            await EventSubscriberTester.Create<Startup>()
                .ExpectResult(SubscriberStatus.InvalidData)
                .RunAsync<PowerSourceChangeSubscriber>(EventData.CreateValueEvent("N", $"Demo.Robot.Xyz", "PowerSourceChange", "Xyz"));
        }

        [Test, TestSetUp]
        public async Task A150_PowerSourceChangeSubscriber_InvalidData_Value()
        {
            await EventSubscriberTester.Create<Startup>()
                .ExpectResult(SubscriberStatus.InvalidData)
                .RunAsync<PowerSourceChangeSubscriber>(EventData.CreateValueEvent("!", $"Demo.Robot.{1.ToGuid()}", "PowerSourceChange", 1.ToGuid()));
        }

        [Test]
        public async Task A160_PowerSourceChangeSubscriber_ManagerFailure()
        {
            var rm = new Mock<IRobotManager>();
            rm.Setup(x => x.GetAsync(1.ToGuid())).Throws(new InvalidOperationException("Get method failed!"));

            await EventSubscriberTester.Create<Startup>()
                .ExpectUnhandledException<InvalidOperationException>("*")
                .AddScopedService(rm.Object)
                .RunAsync<PowerSourceChangeSubscriber>(EventData.CreateValueEvent("N", $"Demo.Robot.{1.ToGuid()}", "PowerSourceChange", 1.ToGuid()));
        }
    }
}