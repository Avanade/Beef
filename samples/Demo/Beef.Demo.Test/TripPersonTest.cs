using Beef.Demo.Api;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Test.NUnit;
using NUnit.Framework;
using System.Net;

namespace Beef.Demo.Test
{
    [TestFixture, NonParallelizable]
    public class TripPersonTest : UsingAgentTesterServer<Startup>
    {
        [Test, TestSetUp, Parallelizable]
        public void B110_Get_NotFound()
        {
            AgentTester.Test<TripPersonAgent, TripPerson>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .Run(a => a.GetAsync("rando"));
        }

        [Test, TestSetUp, Parallelizable]
        public void B120_Get_Found()
        {
            var p = AgentTester.Test<TripPersonAgent, TripPerson>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => new TripPerson { Id = "willieashmore", FirstName = "Willie", LastName = "Ashmore" })
                .Run(a => a.GetAsync("willieashmore")).Value;
        }

        [Test, TestSetUp, Parallelizable]
        public void C110_Create()
        {
            var p = new TripPerson { Id = "masm", FirstName = "Mary", LastName = "Smith" };
            AgentTester.Test<TripPersonAgent, TripPerson>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .Run(a => a.CreateAsync(p));
        }

        [Test, TestSetUp, Parallelizable]
        public void D110_Update_NotFound()
        {
            var p = new TripPerson { Id = "willieashmore", FirstName = "WillieXXX", LastName = "AshmoreYYY" };
            AgentTester.Test<TripPersonAgent, TripPerson>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.UpdateAsync(p, "xyz"));
        }

        [Test, TestSetUp, Parallelizable]
        public void D120_Update()
        {
            var p = new TripPerson { Id = "willieashmore", FirstName = "Willie", LastName = "AshmoreYYY" };
            AgentTester.Test<TripPersonAgent, TripPerson>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => p)
                .Run(a => a.UpdateAsync(p, p.Id));
        }

        [Test, TestSetUp, Parallelizable]
        public void E110_Delete()
        {
            AgentTester.Test<TripPersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.DeleteAsync("willieashmore"));

            // Should be able to delete many times...
            AgentTester.Test<TripPersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.DeleteAsync("willieashmore"));
        }
    }
}