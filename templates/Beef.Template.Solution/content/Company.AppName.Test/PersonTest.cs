using Beef;
using Beef.Test.NUnit;
using Beef.WebApi;
using Company.AppName.Api;
using Company.AppName.Business.Validation;
using Company.AppName.Common.Agents;
using Company.AppName.Common.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Company.AppName.Test
{
    [TestFixture, NonParallelizable]
    public class PersonTest : UsingAgentTesterServer<Startup>
    {
        #region Get

        [Test, TestSetUp]
        public void A110_Get_NotFound()
        {
            AgentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(404.ToGuid()));
        }

        [Test, TestSetUp]
        public void A120_Get_Found()
        {
            AgentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue(_ => new Person
                {
                    Id = 1.ToGuid(),
                    FirstName = "Wendy",
                    LastName = "Jones",
                    GenderSid = "F",
                    Birthday = new DateTime(1985, 03, 18)
                })
                .Run(a => a.GetAsync(1.ToGuid()));
        }

        [Test, TestSetUp]
        public void A120_Get_Modified_NotModified()
        {
            var v = AgentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid(), new WebApiRequestOptions { ETag = TestSetUp.ConcurrencyErrorETag })).Value!;

            Assert.NotNull(v);

            AgentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .Run(a => a.GetAsync(1.ToGuid(), new WebApiRequestOptions { ETag = v.ETag }));
        }

        #endregion

        #region GetByArgs

        [Test, TestSetUp]
        public void A210_GetByArgs_All()
        {
            var v = AgentTester.Test<PersonAgent, PersonCollectionResult>()
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
            var v = AgentTester.Test<PersonAgent, PersonCollectionResult>()
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
            var v = AgentTester.Test<PersonAgent, PersonCollectionResult>()
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
            var v = AgentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new PersonArgs { GendersSids = new List<string> { "F" } })).Value!;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(2, v.Result.Count);
            Assert.AreEqual(new string[] { "Browne", "Jones" }, v.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void A250_GetByArgs_Empty()
        {
            var v = AgentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new PersonArgs { LastName = "s*", FirstName = "b*", GendersSids = new List<string> { "F" } })).Value!;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(0, v.Result.Count);
        }

        [Test, TestSetUp]
        public void A260_GetByArgs_FieldSelection()
        {
            var r = AgentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new PersonArgs { GendersSids = new List<string> { "F" } }, requestOptions: new WebApiRequestOptions().Include("firstname", "lastname")));

            Assert.IsNotNull(r.Value);
            Assert.IsNotNull(r.Value.Result);
            Assert.AreEqual(2, r.Value.Result.Count);
            Assert.AreEqual(new string[] { "Browne", "Jones" }, r.Value.Result.Select(x => x.LastName).ToArray());

            Assert.AreEqual("[{\"firstName\":\"Rachael\",\"lastName\":\"Browne\"},{\"firstName\":\"Wendy\",\"lastName\":\"Jones\"}]", r.Content);
        }

        [Test, TestSetUp]
        public void A270_GetByArgs_RefDataText()
        {
            var r = AgentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new PersonArgs { GendersSids = new List<string> { "F" } }, requestOptions: new WebApiRequestOptions { IncludeRefDataText = true }));

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
            var v = new Person
            {
                FirstName = "Jill",
                LastName = "Smith",
                GenderSid = "F",
                Birthday = new DateTime(1955, 10, 28)
            };

            // Create value.
            v = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .ExpectChangeLogCreated()
                .ExpectETag()
                .ExpectUniqueKey()
                .ExpectValue(_ => v)
                .ExpectEvent("Company.AppName.Person.*", "Created")
                .Run(a => a.CreateAsync(v)).Value!;

            // Check the value was created properly.
            AgentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => v)
                .Run(a => a.GetAsync(v.Id));
        }

        #endregion

        #region Update

        [Test, TestSetUp]
        public void C110_Update_NotFound()
        {
            // Get an existing value.
            var v = AgentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(2.ToGuid())).Value!;

            // Try updating with an invalid identifier.
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.UpdateAsync(v, 404.ToGuid()));
        }

        [Test, TestSetUp]
        public void C120_Update_Concurrency()
        {
            // Get an existing value.
            var id = 2.ToGuid();
            var v = AgentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id)).Value!;

            // Try updating the value with an invalid eTag (if-match).
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .Run(a => a.UpdateAsync(v, id, new WebApiRequestOptions { ETag = TestSetUp.ConcurrencyErrorETag }));

            // Try updating the value with an invalid eTag.
            v.ETag = TestSetUp.ConcurrencyErrorETag;
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .Run(a => a.UpdateAsync(v, id));
        }

        [Test, TestSetUp]
        public void C130_Update()
        {
            // Get an existing value.
            var id = 2.ToGuid();
            var v = AgentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id)).Value!;

            // Make some changes to the data.
            v.FirstName += "X";
            v.LastName += "Y";

            // Update the value.
            v = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(v.ETag)
                .ExpectUniqueKey()
                .ExpectValue(_ => v)
                .ExpectEvent($"Company.AppName.Person.{id}", "Updated")
                .Run(a => a.UpdateAsync(v, id)).Value!;

            // Check the value was updated properly.
            AgentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => v)
                .Run(a => a.GetAsync(id));
        }

        #endregion

        #region Patch

        [Test, TestSetUp]
        public void D110_Patch_NotFound()
        {
            // Get an existing value.
            var v = AgentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(2.ToGuid())).Value!;

            // Try patching with an invalid identifier.
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.PatchAsync(WebApiPatchOption.MergePatch, "{ \"lastName\": \"Smithers\" }", 404.ToGuid()));
        }

        [Test, TestSetUp]
        public void D120_Patch_Concurrency()
        {
            // Get an existing value.
            var id = 2.ToGuid();
            var v = AgentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id)).Value!;

            // Try updating the value with an invalid eTag (if-match).
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .Run(a => a.PatchAsync(WebApiPatchOption.MergePatch, "{ \"lastName\": \"Smithers\" }", id, new WebApiRequestOptions { ETag = TestSetUp.ConcurrencyErrorETag }));

            // Try updating the value with an eTag header (json payload eTag is ignored).
            v.ETag = TestSetUp.ConcurrencyErrorETag;
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .Run(a => a.PatchAsync(WebApiPatchOption.MergePatch, "{{ \"lastName\": \"Smithers\", \"etag\": {TestSetUp.ConcurrencyErrorETag} }}", id));
        }

        [Test, TestSetUp]
        public void D130_Patch()
        {
            // Get an existing value.
            var id = 2.ToGuid();
            var v = AgentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id)).Value!;

            // Make some changes to the data.
            v.LastName = "Smithers";

            // Update the value.
            v = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(v.ETag)
                .ExpectUniqueKey()
                .ExpectValue(_ => v)
                .ExpectEvent($"Company.AppName.Person.{id}", "Updated")
                .Run(a => a.PatchAsync(WebApiPatchOption.MergePatch, $"{{ \"lastName\": \"{v.LastName}\" }}", id, new WebApiRequestOptions { ETag = v.ETag })).Value!;

            // Check the value was updated properly.
            AgentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => v)
                .Run(a => a.GetAsync(id));
        }

        #endregion

        #region Delete

        [Test, TestSetUp]
        public void E110_Delete()
        {
            // Check value exists.
            var id = 4.ToGuid();
            AgentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id));

            // Delete value.
            AgentTester.Test<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .ExpectEvent($"Company.AppName.Person.{id}", "Deleted")
                .Run(a => a.DeleteAsync(id));

            // Check value no longer exists.
            AgentTester.Test<PersonAgent, Person?>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(id));

            // Delete again (should still be successful as a Delete is idempotent). 
            AgentTester.Test<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .ExpectEvent($"Company.AppName.Person.{id}", "Deleted")
                .Run(a => a.DeleteAsync(id));
        }

        #endregion
    }
}