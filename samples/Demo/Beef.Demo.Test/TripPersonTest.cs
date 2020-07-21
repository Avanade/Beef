using Beef.Demo.Api;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Test.NUnit;
using NUnit.Framework;
using System.Net;

namespace Beef.Demo.Test
{
    [TestFixture, NonParallelizable]
    public class TripPersonTest
    {
        private AgentTesterServer<Startup> _agentTester;

        [OneTimeSetUp]
        public void OneTimeSetUp() { TestSetUp.Reset(); _agentTester = AgentTester.CreateServer<Startup>("Beef"); }

        [OneTimeTearDown]
        public void OneTimeTearDown() => _agentTester.Dispose();

        [Test, TestSetUp, Parallelizable]
        public void B110_Get_NotFound()
        {
            _agentTester.Test<TripPersonAgent, TripPerson>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .Run(a => a.GetAsync("rando"));
        }

        [Test, TestSetUp, Parallelizable]
        public void B120_Get_Found()
        {
            var p = _agentTester.Test<TripPersonAgent, TripPerson>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => new TripPerson { Id = "willieashmore", FirstName = "Willie", LastName = "Ashmore" })
                .Run(a => a.GetAsync("willieashmore")).Value;
        }

        [Test, TestSetUp, Parallelizable]
        public void C110_Create()
        {
            var p = new TripPerson { Id = "masm", FirstName = "Mary", LastName = "Smith" };
            _agentTester.Test<TripPersonAgent, TripPerson>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .Run(a => a.CreateAsync(p));
        }

        [Test, TestSetUp, Parallelizable]
        public void D110_Update_NotFound()
        {
            var p = new TripPerson { Id = "willieashmore", FirstName = "WillieXXX", LastName = "AshmoreYYY" };
            _agentTester.Test<TripPersonAgent, TripPerson>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.UpdateAsync(p, "xyz"));
        }

        [Test, TestSetUp, Parallelizable]
        public void D120_Update()
        {
            var p = new TripPerson { Id = "willieashmore", FirstName = "Willie", LastName = "AshmoreYYY" };
            _agentTester.Test<TripPersonAgent, TripPerson>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => p)
                .Run(a => a.UpdateAsync(p, p.Id));
        }

        [Test, TestSetUp, Parallelizable]
        public void D120_Delete()
        {
            _agentTester.Test<TripPersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.DeleteAsync("willieashmore"));

            // Should be able to delete many times...
            _agentTester.Test<TripPersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.DeleteAsync("willieashmore"));
        }
    }
}