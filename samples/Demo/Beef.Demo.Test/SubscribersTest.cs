using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Demo.Functions.Subscribers;
using Beef.Events;
using Beef.Events.Subscribe;
using Beef.Test.NUnit;
using NUnit.Framework;
using System.Net;
using System.Threading.Tasks;

namespace Beef.Demo.Test
{
    [TestFixture()]
    public class SubscribersTest
    {
        private readonly RobotTest _robotTest = new RobotTest();

        [OneTimeSetUp]
        public async Task OneTimeSetUp() => await _robotTest.OneTimeSetUp();

        [OneTimeTearDown]
        public async Task OneTimeTearDown() => await _robotTest.OneTimeTearDown();

        [Test, TestSetUp]
        public void A110_PowerSourceChangeSubscriber_NoUpdated()
        {
            var r = AgentTester.Create<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.Agent.GetAsync(1.ToGuid())).Value;

            EventSubscriberTester.Create<PowerSourceChangeSubscriber>()
                .ExpectResult(SubscriberStatus.Success)
                .ExpectNoEvents()
                .Run(EventData.CreateValueEvent(r.PowerSourceSid, $"Demo.Robot.{r.Id}", "PowerSourceChange", r.Id));

            AgentTester.Create<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => r)
                .Run(a => a.Agent.GetAsync(1.ToGuid()));
        }

        [Test, TestSetUp]
        public void A120_PowerSourceChangeSubscriber_Updated()
        {
            var r = AgentTester.Create<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.Agent.GetAsync(1.ToGuid())).Value;

            r.PowerSourceSid = "N";

            EventSubscriberTester.Create<PowerSourceChangeSubscriber>()
                .ExpectResult(SubscriberStatus.Success)
                .ExpectEvent($"Demo.Robot.{r.Id}", "Update")
                .Run(EventData.CreateValueEvent(r.PowerSourceSid, $"Demo.Robot.{r.Id}", "PowerSourceChange", r.Id));

            AgentTester.Create<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated("*")
                .ExpectETag(r.ETag)
                .ExpectValue(_ => r)
                .Run(a => a.Agent.GetAsync(1.ToGuid()));
        }

        [Test, TestSetUp]
        public void A130_PowerSourceChangeSubscriber_NotFound()
        {
            EventSubscriberTester.Create<PowerSourceChangeSubscriber>()
                .ExpectResult(SubscriberStatus.DataNotFound)
                .Run(EventData.CreateValueEvent("N", $"Demo.Robot.{404.ToGuid()}", "PowerSourceChange", 404.ToGuid()));
        }

        [Test, TestSetUp]
        public void A140_PowerSourceChangeSubscriber_InvalidData_Key()
        {
            EventSubscriberTester.Create<PowerSourceChangeSubscriber>()
                .ExpectResult(SubscriberStatus.InvalidData)
                .Run(EventData.CreateValueEvent("N", $"Demo.Robot.Xyz", "PowerSourceChange", "Xyz"));
        }

        [Test, TestSetUp]
        public void A150_PowerSourceChangeSubscriber_InvalidData_Value()
        {
            EventSubscriberTester.Create<PowerSourceChangeSubscriber>()
                .ExpectResult(SubscriberStatus.InvalidData)
                .Run(EventData.CreateValueEvent("!", $"Demo.Robot.{1.ToGuid()}", "PowerSourceChange", 1.ToGuid()));
        }
    }
}