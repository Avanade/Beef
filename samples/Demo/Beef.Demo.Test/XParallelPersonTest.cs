using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Test.NUnit;
using Beef.WebApi;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Beef.Demo.Test
{
    /// <summary>
    /// Fully parallelizable test; seeing how it performs (perf/stability) under load.
    /// </summary>
    [TestFixture, Parallelizable]
    public class XParallelPersonTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestSetUp.Reset();

            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run((a) => a.Agent.GetAsync(404.ToGuid()));
        }

        [Test, Parallelizable, TestSetUp]
        public void B110_Get_NotFound()
        {
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run((a) => a.Agent.GetAsync(404.ToGuid()));
        }

        [Test, Parallelizable, TestSetUp]
        public void B120_Get_FoundAndUpdate()
        {
            var pr = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(1.ToGuid())).Value;

            pr.FirstName = pr.FirstName + "X";

            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.UpdateAsync(pr, 1.ToGuid()));
        }

        [Test, Parallelizable, TestSetUp]
        public void B130_CreateAndUpdateAndDelete()
        {
            var p = new Person
            {
                FirstName = "Bill",
                LastName = "Gates",
                Gender = "M",
                Birthday = new DateTime(1955, 10, 28),
                UniqueCode = Guid.NewGuid().ToString()
            };

            // Create a person.
            p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .Run((a) => a.Agent.CreateAsync(p)).Value;

            // Update a person.
            p.LastName = p.LastName + "X";
            p.Gender = "F";

            p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.UpdateAsync(p, p.Id)).Value;

            // Get the person (not-modified).
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .Run((a) => a.Agent.GetAsync(p.Id, new WebApiRequestOptions { ETag = p.ETag }));

            p.FirstName = p.FirstName + "X";

            // Patch the person.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.PatchAsync(WebApiPatchOption.MergePatch,
                    JToken.Parse($"{{ \"firstName\": \"{p.FirstName}\", \"address\": {{ \"street\": \"Simpsons Road\", \"city\": \"Bardon\" }} }}"),
                    p.Id, new WebApiRequestOptions { ETag = p.ETag }));

            // Delete a person.
            AgentTester.Create<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run((a) => a.Agent.DeleteAsync(p.Id));
        }

        [Test, Parallelizable, TestSetUp]
        public void B140_GetByArgs()
        {
            var args = new PersonArgs { LastName = "sm*" };
            var pcr = AgentTester.Create<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetByArgsAsync(args));

            var etag = pcr.Response.Headers.ETag.Tag;

            AgentTester.Create<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .Run((a) => new PersonAgent(a.Client, (r) =>
                {
                    r.Headers.IfNoneMatch.Add(new System.Net.Http.Headers.EntityTagHeaderValue(etag));
                }).GetByArgsAsync(args));
        }

        [Test, Parallelizable, TestSetUp]
        public void C110_Get_NotFound() => B110_Get_NotFound();

        [Test, Parallelizable, TestSetUp]
        public void C120_Get_FoundAndUpdate() => B120_Get_FoundAndUpdate();

        [Test, Parallelizable, TestSetUp]
        public void C130_CreateAndUpdateAndDelete() => B130_CreateAndUpdateAndDelete();

        [Test, Parallelizable, TestSetUp]
        public void C140_GetByArgs() => B140_GetByArgs();

        [Test, Parallelizable, TestSetUp]
        public void D110_Get_NotFound() => B110_Get_NotFound();

        [Test, Parallelizable, TestSetUp]
        public void D120_Get_FoundAndUpdate() => B120_Get_FoundAndUpdate();

        [Test, Parallelizable, TestSetUp]
        public void D130_CreateAndUpdateAndDelete() => B130_CreateAndUpdateAndDelete();

        [Test, Parallelizable, TestSetUp]
        public void D140_GetByArgs() => B140_GetByArgs();

        [Test, Parallelizable, TestSetUp]
        public void E110_Get_NotFound() => B110_Get_NotFound();

        [Test, Parallelizable, TestSetUp]
        public void E120_Get_FoundAndUpdate() => B120_Get_FoundAndUpdate();

        [Test, Parallelizable, TestSetUp]
        public void E130_CreateAndUpdateAndDelete() => B130_CreateAndUpdateAndDelete();

        [Test, Parallelizable, TestSetUp]
        public void E140_GetByArgs() => B140_GetByArgs();

        [Test, Parallelizable, TestSetUp]
        public void F110_Get_NotFound() => B110_Get_NotFound();

        [Test, Parallelizable, TestSetUp]
        public void F120_Get_FoundAndUpdate() => B120_Get_FoundAndUpdate();

        [Test, Parallelizable, TestSetUp]
        public void F130_CreateAndUpdateAndDelete() => B130_CreateAndUpdateAndDelete();

        [Test, Parallelizable, TestSetUp]
        public void F140_GetByArgs() => B140_GetByArgs();

        [Test, Parallelizable, TestSetUp]
        public void G110_Get_NotFound() => B110_Get_NotFound();

        [Test, Parallelizable, TestSetUp]
        public void G120_Get_FoundAndUpdate() => B120_Get_FoundAndUpdate();

        [Test, Parallelizable, TestSetUp]
        public void G130_CreateAndUpdateAndDelete() => B130_CreateAndUpdateAndDelete();

        [Test, Parallelizable, TestSetUp]
        public void G140_GetByArgs() => B140_GetByArgs();
    }
}
