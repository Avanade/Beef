using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Test.NUnit;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Beef.Demo.Test
{
    [TestFixture, NonParallelizable]
    public class TripPersonTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestSetUp.Reset();
        }

        [Test, Parallelizable, TestSetUp()]
        public void B110_Get_NotFound()
        {
            AgentTester.Create<TripPersonAgent, TripPerson>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .Run((a) => a.Agent.GetAsync("rando"));
        }

        [Test, Parallelizable, TestSetUp()]
        public void B120_Get_Found()
        {
            var p = AgentTester.Create<TripPersonAgent, TripPerson>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => new TripPerson { Id = "willieashmore", FirstName = "Willie", LastName = "Ashmore" })
                .Run((a) => a.Agent.GetAsync("willieashmore")).Value;
        }

        [Test, Parallelizable, TestSetUp()]
        public void C110_Create()
        {
            var p = new TripPerson { Id = "masm", FirstName = "Mary", LastName = "Smith" };
            AgentTester.Create<TripPersonAgent, TripPerson>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .Run((a) => a.Agent.CreateAsync(p));
        }

        [Test, Parallelizable, TestSetUp()]
        public void D110_Update_NotFound()
        {
            var p = new TripPerson { Id = "willieashmore", FirstName = "WillieXXX", LastName = "AshmoreYYY" };
            AgentTester.Create<TripPersonAgent, TripPerson>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run((a) => a.Agent.UpdateAsync(p, "xyz"));
        }

        [Test, Parallelizable, TestSetUp()]
        public void D120_Update()
        {
            var p = new TripPerson { Id = "willieashmore", FirstName = "Willie", LastName = "AshmoreYYY" };
            AgentTester.Create<TripPersonAgent, TripPerson>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => p)
                .Run((a) => a.Agent.UpdateAsync(p, p.Id));
        }

        [Test, Parallelizable, TestSetUp()]
        public void D120_Delete()
        {
            AgentTester.Create<TripPersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run((a) => a.Agent.DeleteAsync("willieashmore"));

            // Should be able to delete many times...
            AgentTester.Create<TripPersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run((a) => a.Agent.DeleteAsync("willieashmore"));
        }
    }
}