using Beef.Demo.Api;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using NUnit.Framework;
using System;
using System.Net;
using UnitTestEx;
using UnitTestEx.Expectations;

namespace Beef.Demo.Test
{
    /// <summary>
    /// Fully parallelizable test; seeing how it performs (perf/stability) under load.
    /// </summary>
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class XParallelPersonTest : UsingApiTester<Startup>
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestSetUp.Default.SetUp();

            Agent<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(404.ToGuid()));
        }

        [Test, Parallelizable]
        public void B110_Get_NotFound()
        {
            Agent<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(404.ToGuid()));
        }

        [Test, Parallelizable]
        public void B120_Get_FoundAndUpdate()
        {
            var pr = Agent<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid())).Value;

            pr.FirstName += "X";

            var r = Agent<PersonAgent, Person>()
                .Run(a => a.UpdateAsync(pr, 1.ToGuid())).Response;

            Assert.That(r, Is.Not.Null);
            if (r.IsSuccessStatusCode) // i.e. HttpStatusCode.OK
                return;

            if (r.StatusCode == HttpStatusCode.PreconditionFailed)
                Assert.Inconclusive("PreconditionFailed - attempted to update under a concurrency situation; this is an expected outcome.");
            else
                Assert.Fail($"Unexpected status code: {r.StatusCode}");
        }

        [Test, Parallelizable]
        public void B130_CreateAndUpdateAndDelete()
        {
            var p = new Person
            {
                FirstName = "Bill",
                LastName = "Gates",
                Gender = "M",
                EyeColor = "BROWN",
                Birthday = new DateTime(1955, 10, 28),
                UniqueCode = Guid.NewGuid().ToString()
            };

            // Create a person.
            p = Agent<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .Run(a => a.CreateAsync(p)).Value;

            // Update a person.
            p.LastName += "X";
            p.Gender = "F";

            p = Agent<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.UpdateAsync(p, p.Id)).Value;

            // Get the person (not-modified).
            Agent<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .Run(a => a.GetAsync(p.Id, new CoreEx.Http.HttpRequestOptions { ETag = p.ETag }));

            p.FirstName += "X";

            // Patch the person.
            Agent<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.PatchAsync(CoreEx.Http.HttpPatchOption.MergePatch,
                    $"{{ \"firstName\": \"{p.FirstName}\", \"address\": {{ \"street\": \"Simpsons Road\", \"city\": \"Bardon\" }} }}",
                    p.Id, new CoreEx.Http.HttpRequestOptions { ETag = p.ETag }));

            // Delete a person.
            Agent<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.DeleteAsync(p.Id));
        }

        [Test, Parallelizable]
        public void B140_GetByArgs()
        {
            var args = new PersonArgs { LastName = "sm*" };
            var pcr = Agent<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(args));

            var etag = pcr.Response.Headers.ETag.Tag;

            Agent<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .Run(a => a.GetByArgsAsync(args, requestOptions: new CoreEx.Http.HttpRequestOptions { ETag = etag }));
        }

        [Test, Parallelizable]
        public void C110_Get_NotFound() => B110_Get_NotFound();

        [Test, Parallelizable]
        public void C120_Get_FoundAndUpdate() => B120_Get_FoundAndUpdate();

        [Test, Parallelizable]
        public void C130_CreateAndUpdateAndDelete() => B130_CreateAndUpdateAndDelete();

        [Test, Parallelizable]
        public void C140_GetByArgs() => B140_GetByArgs();

        [Test, Parallelizable]
        public void D110_Get_NotFound() => B110_Get_NotFound();

        [Test, Parallelizable]
        public void D120_Get_FoundAndUpdate() => B120_Get_FoundAndUpdate();

        [Test, Parallelizable]
        public void D130_CreateAndUpdateAndDelete() => B130_CreateAndUpdateAndDelete();

        [Test, Parallelizable]
        public void D140_GetByArgs() => B140_GetByArgs();

        [Test, Parallelizable]
        public void E110_Get_NotFound() => B110_Get_NotFound();

        [Test, Parallelizable]
        public void E120_Get_FoundAndUpdate() => B120_Get_FoundAndUpdate();

        [Test, Parallelizable]
        public void E130_CreateAndUpdateAndDelete() => B130_CreateAndUpdateAndDelete();

        [Test, Parallelizable]
        public void E140_GetByArgs() => B140_GetByArgs();

        [Test, Parallelizable]
        public void F110_Get_NotFound() => B110_Get_NotFound();

        [Test, Parallelizable]
        public void F120_Get_FoundAndUpdate() => B120_Get_FoundAndUpdate();

        [Test, Parallelizable]
        public void F130_CreateAndUpdateAndDelete() => B130_CreateAndUpdateAndDelete();

        [Test, Parallelizable]
        public void F140_GetByArgs() => B140_GetByArgs();

        [Test, Parallelizable]
        public void G110_Get_NotFound() => B110_Get_NotFound();

        [Test, Parallelizable]
        public void G120_Get_FoundAndUpdate() => B120_Get_FoundAndUpdate();

        [Test, Parallelizable]
        public void G130_CreateAndUpdateAndDelete() => B130_CreateAndUpdateAndDelete();

        [Test, Parallelizable]
        public void G140_GetByArgs() => B140_GetByArgs();
    }
}
