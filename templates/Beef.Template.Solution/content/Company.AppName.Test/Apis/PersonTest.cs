using Beef;
using Beef.Test.NUnit;
using Beef.WebApi;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
#if (implement_httpagent)
using System.Net.Http;
#endif
using Company.AppName.Api;
using Company.AppName.Common.Agents;
using Company.AppName.Common.Entities;

namespace Company.AppName.Test.Apis
{
    [TestFixture, NonParallelizable]
    public class PersonTest
    {
#if (!implement_httpagent)
#       region Get

        [Test, TestSetUp]
        public void A110_Get_NotFound()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            agentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(404.ToGuid()));
        }

        [Test, TestSetUp]
        public void A120_Get_Found()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            agentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue(_ => new Person
                {
                    Id = 1.ToGuid(),
                    FirstName = "Wendy",
                    LastName = "Jones",
                    Gender = "F",
                    Birthday = new DateTime(1985, 03, 18)
                })
                .Run(a => a.GetAsync(1.ToGuid()));
        }

        [Test, TestSetUp]
        public void A120_Get_Modified_NotModified()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var v = agentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid(), new WebApiRequestOptions { ETag = TestSetUp.ConcurrencyErrorETag })).Value!;

            Assert.NotNull(v);

            agentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .Run(a => a.GetAsync(1.ToGuid(), new WebApiRequestOptions { ETag = v.ETag }));
        }

        #endregion

        #region GetByArgs

        [Test, TestSetUp]
        public void A210_GetByArgs_All()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var v = agentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(null)).Value!;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(4, v.Result.Count);
            Assert.AreEqual(new string[] { "Browne", "Jones", "Smith", "Smithers" }, v.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void A220_GetByArgs_FirstName()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var v = agentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new PersonArgs { FirstName = "*a*" })).Value!;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(3, v.Result.Count);
            Assert.AreEqual(new string[] { "Browne", "Smith", "Smithers" }, v.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void A230_GetByArgs_LastName()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var v = agentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new PersonArgs { LastName = "s*" })).Value!;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(2, v.Result.Count);
            Assert.AreEqual(new string[] { "Smith", "Smithers" }, v.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void A240_GetByArgs_Gender()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var v = agentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new PersonArgs { Genders = new List<string> { "F" } })).Value!;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(2, v.Result.Count);
            Assert.AreEqual(new string[] { "Browne", "Jones" }, v.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void A250_GetByArgs_Empty()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var v = agentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new PersonArgs { LastName = "s*", FirstName = "b*", Genders = new List<string> { "F" } })).Value!;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(0, v.Result.Count);
        }

        [Test, TestSetUp]
        public void A260_GetByArgs_FieldSelection()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var r = agentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new PersonArgs { Genders = new List<string> { "F" } }, requestOptions: new WebApiRequestOptions().Include("firstname", "lastname")));

            Assert.IsNotNull(r.Value);
            Assert.IsNotNull(r.Value.Result);
            Assert.AreEqual(2, r.Value.Result.Count);
            Assert.AreEqual(new string[] { "Browne", "Jones" }, r.Value.Result.Select(x => x.LastName).ToArray());

            Assert.AreEqual("[{\"firstName\":\"Rachael\",\"lastName\":\"Browne\"},{\"firstName\":\"Wendy\",\"lastName\":\"Jones\"}]", r.Content);
        }

        [Test, TestSetUp]
        public void A270_GetByArgs_RefDataText()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var r = agentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new PersonArgs { Genders = new List<string> { "F" } }, requestOptions: new WebApiRequestOptions { IncludeRefDataText = true }));

            Assert.IsNotNull(r.Value);
            Assert.IsNotNull(r.Value.Result);
            Assert.AreEqual(2, r.Value.Result.Count);
            Assert.AreEqual(new string[] { "Browne", "Jones" }, r.Value.Result.Select(x => x.LastName).ToArray());

            Assert.AreEqual(2, Newtonsoft.Json.Linq.JArray.Parse(r.Content!).Descendants().OfType<Newtonsoft.Json.Linq.JProperty>().Where(p => p.Name == "genderText").Count());
        }

        #endregion

        #region Create

        [Test, TestSetUp]
        public void B110_Create()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            var v = new Person
            {
                FirstName = "Jill",
                LastName = "Smith",
                Gender = "F",
                Birthday = new DateTime(1955, 10, 28)
            };

            // Create value.
            v = agentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .ExpectChangeLogCreated()
                .ExpectETag()
                .ExpectUniqueKey()
                .ExpectValue(_ => v)
                .ExpectEvent("Company.AppName.Person", "Created")
                .Run(a => a.CreateAsync(v)).Value!;

            // Check the value was created properly.
            agentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => v)
                .Run(a => a.GetAsync(v.Id));
        }

        #endregion

        #region Update

        [Test, TestSetUp]
        public void C110_Update_NotFound()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            // Get an existing value.
            var v = agentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(2.ToGuid())).Value!;

            // Try updating with an invalid identifier.
            agentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.UpdateAsync(v, 404.ToGuid()));
        }

        [Test, TestSetUp]
        public void C120_Update_Concurrency()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            // Get an existing value.
            var id = 2.ToGuid();
            var v = agentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id)).Value!;

            // Try updating the value with an invalid eTag (if-match).
            agentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .Run(a => a.UpdateAsync(v, id, new WebApiRequestOptions { ETag = TestSetUp.ConcurrencyErrorETag }));

            // Try updating the value with an invalid eTag.
            v.ETag = TestSetUp.ConcurrencyErrorETag;
            agentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .Run(a => a.UpdateAsync(v, id));
        }

        [Test, TestSetUp]
        public void C130_Update()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            // Get an existing value.
            var id = 2.ToGuid();
            var v = agentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id)).Value!;

            // Make some changes to the data.
            v.FirstName += "X";
            v.LastName += "Y";

            // Update the value.
            v = agentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(v.ETag)
                .ExpectUniqueKey()
                .ExpectValue(_ => v)
                .ExpectEvent($"Company.AppName.Person", "Updated")
                .Run(a => a.UpdateAsync(v, id)).Value!;

            // Check the value was updated properly.
            agentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => v)
                .Run(a => a.GetAsync(id));
        }

        #endregion

        #region Patch

        [Test, TestSetUp]
        public void D110_Patch_NotFound()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            // Get an existing value.
            var v = agentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(2.ToGuid())).Value!;

            // Try patching with an invalid identifier.
            agentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.PatchAsync(WebApiPatchOption.MergePatch, "{ \"lastName\": \"Smithers\" }", 404.ToGuid()));
        }

        [Test, TestSetUp]
        public void D120_Patch_Concurrency()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            // Get an existing value.
            var id = 2.ToGuid();
            var v = agentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id)).Value!;

            // Try updating the value with an invalid eTag (if-match).
            agentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .Run(a => a.PatchAsync(WebApiPatchOption.MergePatch, "{ \"lastName\": \"Smithers\" }", id, new WebApiRequestOptions { ETag = TestSetUp.ConcurrencyErrorETag }));

            // Try updating the value with an eTag header (json payload eTag is ignored).
            agentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .Run(a => a.PatchAsync(WebApiPatchOption.MergePatch, $"{{ \"lastName\": \"Smithers\", \"etag\": \"{TestSetUp.ConcurrencyErrorETag}\" }}", id));
        }

        [Test, TestSetUp]
        public void D130_Patch()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            // Get an existing value.
            var id = 2.ToGuid();
            var v = agentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id)).Value!;

            // Make some changes to the data.
            v.LastName = "Smithers";

            // Update the value.
            v = agentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(v.ETag)
                .ExpectUniqueKey()
                .ExpectValue(_ => v)
                .ExpectEvent($"Company.AppName.Person", "Updated")
                .Run(a => a.PatchAsync(WebApiPatchOption.MergePatch, $"{{ \"lastName\": \"{v.LastName}\" }}", id, new WebApiRequestOptions { ETag = v.ETag })).Value!;

            // Check the value was updated properly.
            agentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => v)
                .Run(a => a.GetAsync(id));
        }

        #endregion

        #region Delete

        [Test, TestSetUp]
        public void E110_Delete()
        {
            using var agentTester = AgentTester.CreateWaf<Startup>();

            // Check value exists.
            var id = 4.ToGuid();
            agentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id));

            // Delete value.
            agentTester.Test<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .ExpectEvent($"Company.AppName.Person", "Deleted")
                .Run(a => a.DeleteAsync(id));

            // Check value no longer exists.
            agentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(id));

            // Delete again (should still be successful as a Delete is idempotent); but no event should be raised. 
            agentTester.Test<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.DeleteAsync(id));
        }

        #endregion
    }
#else
        #region Get

        [Test, TestSetUp]
        public void A110_Get_NotFound()
        {
            var mcf = MockHttpClientFactory.Create();
            mcf.CreateClient("Xxx").Request(HttpMethod.Get, $"/people/{404.ToGuid()}").Respond.With(HttpStatusCode.NotFound);

            using var agentTester = AgentTester.CreateWaf<Startup>(sc => mcf.ReplaceSingleton(sc));

            agentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(404.ToGuid()));
        }

        [Test, TestSetUp]
        public void A120_Get_Found()
        {
            var mcf = MockHttpClientFactory.Create();
            mcf.CreateClient("Xxx").Request(HttpMethod.Get, $"/people/{1.ToGuid()}").Respond.With(new Business.Data.Model.Person { Id = 1.ToGuid(), FirstName = "Wendy", LastName = "Jones", Gender = "F", Birthday = new DateTime(1985, 03, 18) });

            using var agentTester = AgentTester.CreateWaf<Startup>(sc => mcf.ReplaceSingleton(sc));

            agentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue(_ => new Person
                {
                    Id = 1.ToGuid(),
                    FirstName = "Wendy",
                    LastName = "Jones",
                    Gender = "F",
                    Birthday = new DateTime(1985, 03, 18)
                })
                .Run(a => a.GetAsync(1.ToGuid()));
        }

        #endregion
    }
#endif
}