using Beef.Test.NUnit;
using Beef.WebApi;
using Company.AppName.Business.Validation;
using Company.AppName.Common.Agents;
using Company.AppName.Common.Entities;
using NUnit.Framework;
using System;
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
            ExpectValidationException.Run(
                () => PersonValidator.Default.Validate(new Person()).ThrowOnError(),
                "First Name is required.",
                "Last Name is required.",
                "Gender is required.",
                "Birthday is required.");
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
                .ExpectValue((t) => new PersonTest
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

            // Update with an invalid identifier.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(ErrorType.NotFoundError)
                .Run((a) => a.Agent.UpdateAsync(v, 404.ToGuid()));
        }

        [Test, TestSetUp]
        public void D120_Update_Concurrency()
        {
            // Get an existing value.
            var v = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(2.ToGuid())).Value;

            // Try updating the value with an invalid eTag (if-match).
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .ExpectErrorType(ErrorType.ConcurrencyError)
                .Run((a) => a.Agent.UpdateAsync(v, 2.ToGuid(), new WebApiRequestOptions { ETag = AgentTester.ConcurrencyErrorETag }));

            // Try updating the value with an invalid eTag.
            v.ETag = AgentTester.ConcurrencyErrorETag;
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .ExpectErrorType(ErrorType.ConcurrencyError)
                .Run((a) => a.Agent.UpdateAsync(v, 2.ToGuid()));
        }

        [Test, TestSetUp]
        public void D130_Update()
        {
            // Get an existing value.
            var v = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(2.ToGuid())).Value;

            // Update the value with an address.
            v.FirstName = v.FirstName + "X";
            v.LastName = v.LastName + "Y";

            v = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(v.ETag)
                .ExpectUniqueKey()
                .ExpectValue((t) => v)
                .Run((a) => a.Agent.UpdateAsync(v, 2.ToGuid())).Value;

            // Check the value was updated properly.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => v)
                .Run((a) => a.Agent.GetAsync(2.ToGuid()));
        }

        #endregion

        #region Delete

        [Test, TestSetUp]
        public void E110_Delete()
        {
            // Check value exists.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(4.ToGuid()));

            // Delete value.
            AgentTester.Create<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run((a) => a.Agent.DeleteAsync(4.ToGuid()));

            // Check value no longer exists.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .Run((a) => a.Agent.GetAsync(4.ToGuid()));

            // Delete again. 
            AgentTester.Create<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run((a) => a.Agent.DeleteAsync(4.ToGuid()));
        }

        #endregion
    }
}