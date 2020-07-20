using Beef.Demo.Api;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Test.NUnit;
using Beef.WebApi;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Net;

namespace Beef.Demo.Test
{
    /// <summary>
    /// Fully parallelizable test; seeing how it performs (perf/stability) under load.
    /// </summary>
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class XParallelPersonTest
    {
        private AgentTesterServer<Startup> _agentTester;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestSetUp.Reset(); 
            _agentTester = AgentTester.CreateServer<Startup>();

            _agentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(404.ToGuid()));
        }

        [Test, TestSetUp, Parallelizable]
        public void B110_Get_NotFound()
        {
            _agentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(404.ToGuid()));
        }

        [Test, TestSetUp, Parallelizable]
        public void B120_Get_FoundAndUpdate()
        {
            var pr = _agentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid())).Value;

            pr.FirstName = pr.FirstName + "X";

            var r = _agentTester.Test<PersonAgent, Person>()
                .Run(a => a.UpdateAsync(pr, 1.ToGuid()));

            Assert.NotNull(r);
            if (r.IsSuccess) // i.e. HttpStatusCode.OK
                return;

            if (r.StatusCode == HttpStatusCode.PreconditionFailed)
                Assert.Inconclusive("PreconditionFailed - attempted to update under a concurrency situation; this is an expected outcome.");
            else
                Assert.Fail($"Unexpected status code: {r.StatusCode}");
        }

        [Test, TestSetUp, Parallelizable]
        public void B130_CreateAndUpdateAndDelete()
        {
            var p = new Person
            {
                FirstName = "Bill",
                LastName = "Gates",
                GenderSid = "M",
                EyeColorSid = "BROWN",
                Birthday = new DateTime(1955, 10, 28),
                UniqueCode = Guid.NewGuid().ToString()
            };

            // Create a person.
            p = _agentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .Run(a => a.CreateAsync(p)).Value;

            // Update a person.
            p.LastName = p.LastName + "X";
            p.GenderSid = "F";

            p = _agentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.UpdateAsync(p, p.Id)).Value;

            // Get the person (not-modified).
            _agentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .Run(a => a.GetAsync(p.Id, new WebApiRequestOptions { ETag = p.ETag }));

            p.FirstName = p.FirstName + "X";

            // Patch the person.
            _agentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.PatchAsync(WebApiPatchOption.MergePatch,
                    JToken.Parse($"{{ \"firstName\": \"{p.FirstName}\", \"address\": {{ \"street\": \"Simpsons Road\", \"city\": \"Bardon\" }} }}"),
                    p.Id, new WebApiRequestOptions { ETag = p.ETag }));

            // Delete a person.
            _agentTester.Test<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.DeleteAsync(p.Id));
        }

        [Test, TestSetUp, Parallelizable]
        public void B140_GetByArgs()
        {
            var args = new PersonArgs { LastName = "sm*" };
            var pcr = _agentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(args));

            var etag = pcr.Response.Headers.ETag.Tag;

            _agentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .Run(a => a.GetByArgsAsync(args, requestOptions: new WebApiRequestOptions { ETag = etag }));
        }

        [Test, TestSetUp, Parallelizable]
        public void C110_Get_NotFound() => B110_Get_NotFound();

        [Test, TestSetUp, Parallelizable]
        public void C120_Get_FoundAndUpdate() => B120_Get_FoundAndUpdate();

        [Test, TestSetUp, Parallelizable]
        public void C130_CreateAndUpdateAndDelete() => B130_CreateAndUpdateAndDelete();

        [Test, TestSetUp, Parallelizable]
        public void C140_GetByArgs() => B140_GetByArgs();

        [Test, TestSetUp, Parallelizable]
        public void D110_Get_NotFound() => B110_Get_NotFound();

        [Test, TestSetUp, Parallelizable]
        public void D120_Get_FoundAndUpdate() => B120_Get_FoundAndUpdate();

        [Test, TestSetUp, Parallelizable]
        public void D130_CreateAndUpdateAndDelete() => B130_CreateAndUpdateAndDelete();

        [Test, TestSetUp, Parallelizable]
        public void D140_GetByArgs() => B140_GetByArgs();

        [Test, TestSetUp, Parallelizable]
        public void E110_Get_NotFound() => B110_Get_NotFound();

        [Test, TestSetUp, Parallelizable]
        public void E120_Get_FoundAndUpdate() => B120_Get_FoundAndUpdate();

        [Test, TestSetUp, Parallelizable]
        public void E130_CreateAndUpdateAndDelete() => B130_CreateAndUpdateAndDelete();

        [Test, TestSetUp, Parallelizable]
        public void E140_GetByArgs() => B140_GetByArgs();

        [Test, TestSetUp, Parallelizable]
        public void F110_Get_NotFound() => B110_Get_NotFound();

        [Test, TestSetUp, Parallelizable]
        public void F120_Get_FoundAndUpdate() => B120_Get_FoundAndUpdate();

        [Test, TestSetUp, Parallelizable]
        public void F130_CreateAndUpdateAndDelete() => B130_CreateAndUpdateAndDelete();

        [Test, TestSetUp, Parallelizable]
        public void F140_GetByArgs() => B140_GetByArgs();

        [Test, TestSetUp, Parallelizable]
        public void G110_Get_NotFound() => B110_Get_NotFound();

        [Test, TestSetUp, Parallelizable]
        public void G120_Get_FoundAndUpdate() => B120_Get_FoundAndUpdate();

        [Test, TestSetUp, Parallelizable]
        public void G130_CreateAndUpdateAndDelete() => B130_CreateAndUpdateAndDelete();

        [Test, TestSetUp, Parallelizable]
        public void G140_GetByArgs() => B140_GetByArgs();
    }
}
