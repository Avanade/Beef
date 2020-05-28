using Beef;
using Beef.Test.NUnit;
using Beef.WebApi;
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
    public class PersonTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => TestSetUp.Reset();

        #region Validators

        [Test, TestSetUp]
        public void A110_Validation_Empty()
        {
            ExpectValidationException.Throws(
                () => PersonValidator.Default.Validate(new Person()).ThrowOnError(),
                "First Name is required.",
                "Last Name is required.",
                "Gender is required.",
                "Birthday is required.");
        }

        [Test, TestSetUp]
        public void A120_Validation_Invalid()
        {
            ExpectValidationException.Throws(
                () => PersonValidator.Default.Validate(new Person { FirstName = 'x'.ToLongString(), LastName = 'x'.ToLongString(), Gender = "X", Birthday = DateTime.Now.AddDays(1) }).ThrowOnError(),
                "First Name must not exceed 100 characters in length.",
                "Last Name must not exceed 100 characters in length.",
                "Gender is invalid.",
                "Birthday must be less than or equal to Today.");
        }

        #endregion

        #region Get

        [Test, TestSetUp]
        public void B110_Get_NotFound()
        {
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .Run((a) => a.Agent.GetAsync(404.ToGuid()));
        }

        [Test, TestSetUp]
        public void B120_Get_Found()
        {
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue((t) => new Person
                {
                    Id = 1.ToGuid(),
                    FirstName = "Wendy",
                    LastName = "Jones",
                    Gender = "F",
                    Birthday = new DateTime(1985, 03, 18)
                })
                .Run((a) => a.Agent.GetAsync(1.ToGuid()));
        }

        [Test, TestSetUp]
        public void B120_Get_Modified_NotModified()
        {
            var v = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(1.ToGuid(), new WebApiRequestOptions { ETag = AgentTester.ConcurrencyErrorETag })).Value;

            Assert.NotNull(v);

            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .Run((a) => a.Agent.GetAsync(1.ToGuid(), new WebApiRequestOptions { ETag = v.ETag }));
        }

        #endregion

        #region GetByArgs

        [Test, TestSetUp]
        public void B210_GetByArgs_All()
        {
            var v = AgentTester.Create<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetByArgsAsync(null)).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(4, v.Result.Count);
            Assert.AreEqual(new string[] { "Browne", "Jones", "Smith", "Smithers" }, v.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void B220_GetByArgs_FirstName()
        {
            var v = AgentTester.Create<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetByArgsAsync(new PersonArgs { FirstName = "*a*" })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(3, v.Result.Count);
            Assert.AreEqual(new string[] { "Browne", "Smith", "Smithers" }, v.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void B230_GetByArgs_LastName()
        {
            var v = AgentTester.Create<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetByArgsAsync(new PersonArgs { LastName = "s*" })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(2, v.Result.Count);
            Assert.AreEqual(new string[] { "Smith", "Smithers" }, v.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void B240_GetByArgs_Gender()
        {
            var v = AgentTester.Create<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetByArgsAsync(new PersonArgs { GendersSids = new List<string> { "F" } })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(2, v.Result.Count);
            Assert.AreEqual(new string[] { "Browne", "Jones" }, v.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void B250_GetByArgs_Empty()
        {
            var v = AgentTester.Create<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetByArgsAsync(new PersonArgs { LastName = "s*", FirstName = "b*", GendersSids = new List<string> { "F" } })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(0, v.Result.Count);
        }

        [Test, TestSetUp]
        public void B260_GetByArgs_FieldSelection()
        {
            var r = AgentTester.Create<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetByArgsAsync(new PersonArgs { GendersSids = new List<string> { "F" } }, requestOptions: new WebApiRequestOptions().Include("firstname", "lastname")));

            Assert.IsNotNull(r.Value);
            Assert.IsNotNull(r.Value.Result);
            Assert.AreEqual(2, r.Value.Result.Count);
            Assert.AreEqual(new string[] { "Browne", "Jones" }, r.Value.Result.Select(x => x.LastName).ToArray());

            Assert.AreEqual("[{\"firstName\":\"Rachael\",\"lastName\":\"Browne\"},{\"firstName\":\"Wendy\",\"lastName\":\"Jones\"}]", r.Content);
        }

        [Test, TestSetUp]
        public void B270_GetByArgs_RefDataText()
        {
            var r = AgentTester.Create<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetByArgsAsync(new PersonArgs { GendersSids = new List<string> { "F" } }, requestOptions: new WebApiRequestOptions { IncludeRefDataText = true }));

            Assert.IsNotNull(r.Value);
            Assert.IsNotNull(r.Value.Result);
            Assert.AreEqual(2, r.Value.Result.Count);
            Assert.AreEqual(new string[] { "Browne", "Jones" }, r.Value.Result.Select(x => x.LastName).ToArray());

            Assert.AreEqual(2, Newtonsoft.Json.Linq.JArray.Parse(r.Content!).Descendants().OfType<Newtonsoft.Json.Linq.JProperty>().Where(p => p.Name == "genderText").Count());
        }

        #endregion

        #region Create

        [Test, TestSetUp]
        public void C110_Create()
        {
            var v = new Person
            {
                FirstName = "Jill",
                LastName = "Smith",
                Gender = "F",
                Birthday = new DateTime(1955, 10, 28)
            };

            // Create value.
            v = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .ExpectChangeLogCreated()
                .ExpectETag()
                .ExpectUniqueKey()
                .ExpectValue((t) => v)
                .ExpectEvent("Company.AppName.Person.*", "Create")
                .Run((a) => a.Agent.CreateAsync(v)).Value;

            // Check the value was created properly.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => v)
                .Run((a) => a.Agent.GetAsync(v.Id));
        }

        #endregion

        #region Update

        [Test, TestSetUp]
        public void D110_Update_NotFound()
        {
            // Get an existing value.
            var v = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(2.ToGuid())).Value;

            // Try updating with an invalid identifier.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(ErrorType.NotFoundError)
                .Run((a) => a.Agent.UpdateAsync(v, 404.ToGuid()));
        }

        [Test, TestSetUp]
        public void D120_Update_Concurrency()
        {
            // Get an existing value.
            var id = 2.ToGuid();
            var v = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(id)).Value;

            // Try updating the value with an invalid eTag (if-match).
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .ExpectErrorType(ErrorType.ConcurrencyError)
                .Run((a) => a.Agent.UpdateAsync(v, id, new WebApiRequestOptions { ETag = AgentTester.ConcurrencyErrorETag }));

            // Try updating the value with an invalid eTag.
            v.ETag = AgentTester.ConcurrencyErrorETag;
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .ExpectErrorType(ErrorType.ConcurrencyError)
                .Run((a) => a.Agent.UpdateAsync(v, id));
        }

        [Test, TestSetUp]
        public void D130_Update()
        {
            // Get an existing value.
            var id = 2.ToGuid();
            var v = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(id)).Value;

            // Make some changes to the data.
            v.FirstName += "X";
            v.LastName += "Y";

            // Update the value.
            v = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(v.ETag)
                .ExpectUniqueKey()
                .ExpectValue((t) => v)
                .ExpectEvent($"Company.AppName.Person.{id}", "Update")
                .Run((a) => a.Agent.UpdateAsync(v, id)).Value;

            // Check the value was updated properly.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => v)
                .Run((a) => a.Agent.GetAsync(id));
        }

        #endregion

        #region Patch

        [Test, TestSetUp]
        public void E110_Patch_NotFound()
        {
            // Get an existing value.
            var v = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(2.ToGuid())).Value;

            // Try patching with an invalid identifier.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(ErrorType.NotFoundError)
                .Run((a) => a.Agent.PatchAsync(WebApiPatchOption.MergePatch, "{ \"lastName\": \"Smithers\" }", 404.ToGuid()));
        }

        [Test, TestSetUp]
        public void E120_Patch_Concurrency()
        {
            // Get an existing value.
            var id = 2.ToGuid();
            var v = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(id)).Value;

            // Try updating the value with an invalid eTag (if-match).
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .ExpectErrorType(ErrorType.ConcurrencyError)
                .Run((a) => a.Agent.PatchAsync(WebApiPatchOption.MergePatch, "{ \"lastName\": \"Smithers\" }", id, new WebApiRequestOptions { ETag = AgentTester.ConcurrencyErrorETag }));

            // Try updating the value with an eTag header (json payload eTag is ignored).
            v.ETag = AgentTester.ConcurrencyErrorETag;
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .ExpectErrorType(ErrorType.ConcurrencyError)
                .Run((a) => a.Agent.PatchAsync(WebApiPatchOption.MergePatch, "{{ \"lastName\": \"Smithers\", \"etag\": {AgentTester.ConcurrencyErrorETag} }}", id));
        }

        [Test, TestSetUp]
        public void E130_Patch()
        {
            // Get an existing value.
            var id = 2.ToGuid();
            var v = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(id)).Value;

            // Make some changes to the data.
            v.LastName = "Smithers";

            // Update the value.
            v = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(v.ETag)
                .ExpectUniqueKey()
                .ExpectValue((t) => v)
                .ExpectEvent($"Company.AppName.Person.{id}", "Update")
                .Run((a) => a.Agent.PatchAsync(WebApiPatchOption.MergePatch, $"{{ \"lastName\": \"{v.LastName}\" }}", id, new WebApiRequestOptions { ETag = v.ETag })).Value;

            // Check the value was updated properly.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => v)
                .Run((a) => a.Agent.GetAsync(id));
        }

        #endregion

        #region Delete

        [Test, TestSetUp]
        public void F110_Delete()
        {
            // Check value exists.
            var id = 4.ToGuid();
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(id));

            // Delete value.
            AgentTester.Create<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .ExpectEvent($"Company.AppName.Person.{id}", "Delete")
                .Run((a) => a.Agent.DeleteAsync(id));

            // Check value no longer exists.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .Run((a) => a.Agent.GetAsync(id));

            // Delete again (should still be successful as a Delete is idempotent). 
            AgentTester.Create<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .ExpectEvent($"Company.AppName.Person.{id}", "Delete")
                .Run((a) => a.Agent.DeleteAsync(id));
        }

        #endregion
    }
}