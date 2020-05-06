using Beef.Demo.Business;
using Beef.Demo.Business.Data;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Entities;
using Beef.Test.NUnit;
using Beef.WebApi;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Beef.Demo.Test
{
    [TestFixture, NonParallelizable]
    public class PersonTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestSetUp.Reset();
        }

        #region Validators

        [Test, TestSetUp]
        public void A110_Validation_Null()
        {
            TestSetUp.CreateMock<IPersonData>();
            ExpectValidationException.Throws(
                () => (new PersonManager()).CreateAsync(null),
                "Value is required.");

            ExpectValidationException.Throws(
                () => (new PersonManager()).UpdateAsync(null, 1.ToGuid()),
                "Value is required.");
        }

        [Test, TestSetUp]
        public async Task A110_Validation_Empty()
        {
            TestSetUp.CreateMock<IPersonData>();
            await ExpectValidationException.ThrowsAsync(
                () => (new PersonManager()).CreateAsync(new Person()),
                "First Name is required.",
                "Last Name is required.",
                "Gender is required.",
                "Birthday is required.");

            await ExpectValidationException.ThrowsAsync(
                () => (new PersonManager()).UpdateAsync(new Person(), 1.ToGuid()),
                "First Name is required.",
                "Last Name is required.",
                "Gender is required.",
                "Birthday is required.");
        }

        [Test, TestSetUp]
        public void A130_Validation_Invalid()
        {
            TestSetUp.CreateMock<IPersonData>();
            ExpectValidationException.Throws(
                () => (new PersonManager()).CreateAsync(new Person() { FirstName = 'x'.ToLongString(), LastName = 'x'.ToLongString(), Birthday = DateTime.Now.AddDays(1), Gender = "X", EyeColor = "Y" }),
                "First Name must not exceed 50 characters in length.",
                "Last Name must not exceed 50 characters in length.",
                "Gender is invalid.",
                "Eye Color is invalid.",
                "Birthday must be less than or equal to Today.");
        }

        [Test, TestSetUp]
        public void A140_Validation_ServiceAgentInvalid()
        {
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectErrorType(ErrorType.ValidationError)
                .ExpectMessages(
                    "First Name must not exceed 50 characters in length.",
                    "Last Name must not exceed 50 characters in length.",
                    "Gender is invalid.",
                    "Eye Color is invalid.",
                    "Birthday must be less than or equal to Today.")
                .Run((a) => a.Agent.UpdateAsync(new Person() { FirstName = 'x'.ToLongString(), LastName = 'x'.ToLongString(), Birthday = DateTime.Now.AddDays(1), Gender = "X", EyeColor = "Y" }, 1.ToGuid()));
        }

        [Test, TestSetUp]
        public void A150_Validation_Detail_History_Invalid()
        {
            AgentTester.Create<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectErrorType(ErrorType.ValidationError)
                .ExpectMessages(
                    "End Date must be greater than or equal to Start Date.",
                    "Start Date must be less than or equal to today.")
                .Run((a) => a.Agent.UpdateDetailAsync(new PersonDetail()
                {
                    FirstName = "Barry",
                    LastName = "Smith",
                    Birthday = DateTime.Now.AddDays(-5000),
                    Gender = "M",
                    History = new WorkHistoryCollection { new WorkHistory { Name = "Amazon", StartDate = new DateTime(1990, 12, 31), EndDate = new DateTime(1980, 10, 31) },
                    new WorkHistory { Name = "Google", StartDate = new DateTime(2999, 12, 31), EndDate = new DateTime(2000, 10, 31) } }
                }, 1.ToGuid()));
        }

        [Test, TestSetUp]
        public void A160_Validation_Detail_History_Duplicate()
        {
            AgentTester.Create<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectErrorType(ErrorType.ValidationError)
                .ExpectMessages("History contains duplicates; Name value 'Google' specified more than once.")
                .Run((a) => a.Agent.UpdateDetailAsync(new PersonDetail() { FirstName = "Barry", LastName = "Smith", Birthday = DateTime.Now.AddDays(-5000), Gender = "M", EyeColor = "BROWN",
                    History = new WorkHistoryCollection { new WorkHistory { Name = "Google", StartDate = new DateTime(1990, 12, 31) },
                    new WorkHistory { Name = "Google", StartDate = new DateTime(1992, 12, 31) } } }, 1.ToGuid()));
        }

        #endregion

        #region Get/GetDetail

        [Test, TestSetUp]
        public void B110_Get_NotFound()
        {
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .Run((a) => a.Agent.GetAsync(404.ToGuid()));
        }

        [Test, TestSetUp]
        public void B120_Get_Found_No_Address()
        {
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue((t) => new Person { Id = 1.ToGuid(), FirstName = "Wendy", LastName = "Jones", Gender = "F", UniqueCode = "A1234", Birthday = new DateTime(1985, 03, 18) })
                .Run((a) => a.Agent.GetAsync(1.ToGuid()));
        }

        [Test, TestSetUp]
        public void B130_Get_Found_With_Address()
        {
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue((t) => new Person { Id = 3.ToGuid(), FirstName = "Rachael", LastName = "Browne", Gender = "F", Birthday = new DateTime(1972, 06, 28), Address = new Address { Street = "25 Upoko Road", City = "Wellington" } })
                .Run((a) => a.Agent.GetAsync(3.ToGuid()));
        }

        [Test, TestSetUp]
        public void B140_Get_NotModified()
        {
            var p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(3.ToGuid())).Value;

            Assert.NotNull(p);

            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .Run((a) => new PersonAgent(a.Client, (r) =>
                {
                    r.Headers.IfNoneMatch.Add(new System.Net.Http.Headers.EntityTagHeaderValue("\"" + p.ETag + "\""));
                }).GetAsync(3.ToGuid()));
        }

        [Test, TestSetUp]
        public void B140_Get_NotModified_Modified()
        {
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => new PersonAgent(a.Client, (r) =>
                {
                    r.Headers.IfNoneMatch.Add(new System.Net.Http.Headers.EntityTagHeaderValue("\"ABCDEFG\""));
                }).GetAsync(3.ToGuid()));
        }

        [Test, TestSetUp]
        public void B210_GetDetail_NotFound()
        {
            AgentTester.Create<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .Run((a) => a.Agent.GetDetailAsync(404.ToGuid()));
        }

        [Test, TestSetUp]
        public void B220_GetDetail_NoWorkHistory()
        {
            AgentTester.Create<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue((t) => new PersonDetail { Id = 1.ToGuid(), FirstName = "Wendy", LastName = "Jones", Gender = "F", UniqueCode = "A1234", Birthday = new DateTime(1985, 03, 18) })
                .Run((a) => a.Agent.GetDetailAsync(1.ToGuid()));
        }

        [Test, TestSetUp]
        public void B230_GetDetail_WithWorkHistory()
        {
            var pd = new PersonDetail
            {
                Id = 2.ToGuid(),
                FirstName = "Brian",
                LastName = "Smith",
                Gender = "M",
                EyeColor = "BLUE",
                UniqueCode = "B2345",
                Birthday = new DateTime(1994, 11, 07),
                History = new WorkHistoryCollection {
                    new WorkHistory { Name = "Optus", StartDate = new DateTime(2016, 04, 16) },
                    new WorkHistory { Name = "Telstra", StartDate = new DateTime(2015, 05, 23), EndDate = new DateTime(2016, 04, 06) } }
            };

            AgentTester.Create<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue((t) => pd)
                .Run((a) => a.Agent.GetDetailAsync(2.ToGuid()));
        }

        [Test, TestSetUp]
        public void B310_GetWithEf_NotFound()
        {
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .Run((a) => a.Agent.GetWithEfAsync(404.ToGuid()));
        }

        [Test, TestSetUp]
        public void B320_GetWithEf_Found_No_Address()
        {
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue((t) => new Person { Id = 1.ToGuid(), FirstName = "Wendy", LastName = "Jones", Gender = "F", UniqueCode = "A1234", Birthday = new DateTime(1985, 03, 18) })
                .Run((a) => a.Agent.GetWithEfAsync(1.ToGuid()));
        }

        [Test, TestSetUp]
        public void B320_GetWithEf_Found_With_Address()
        {
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue((t) => new Person { Id = 3.ToGuid(), FirstName = "Rachael", LastName = "Browne", Gender = "F", Birthday = new DateTime(1972, 06, 28), Address = new Address { Street = "25 Upoko Road", City = "Wellington" } })
                .Run((a) => a.Agent.GetWithEfAsync(3.ToGuid()));
        }

        #endregion

        #region GetAll/GetAll2

        [Test, TestSetUp]
        public void C110_GetAll_NoPaging()
        {
            var pcr = AgentTester.Create<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAllAsync());

            // Check all 4 are returned in the sorted order.
            Assert.AreEqual(4, pcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "Browne", "Jones", "Smith", "Smithers" }, pcr.Value.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void C110_GetAll_Paging()
        {
            var pcr = AgentTester.Create<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAllAsync(PagingArgs.CreateSkipAndTake(1, 2)));

            // Check only 2 are returned in the sorted order.
            Assert.AreEqual(2, pcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "Jones", "Smith", }, pcr.Value.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void C120_GetAll_PagingAndFieldFiltering()
        {
            var pa = PagingArgs.CreateSkipAndTake(1, 2);
            var ro = new WebApiRequestOptions().Include("lastName", "firstName");

            var pcr = AgentTester.Create<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAllAsync(pa, ro));

            // Check only 2 are returned in the sorted order.
            Assert.AreEqual(2, pcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "Jones", "Smith", }, pcr.Value.Result.Select(x => x.LastName).ToArray());
            Assert.IsFalse(pcr.Value.Result.Any(x => x.Id != Guid.Empty));
        }

        [Test, TestSetUp]
        public void C130_GetAll2()
        {
            var pcr = AgentTester.Create<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAll2Async());

            // Check all 4 are returned in the sorted order.
            Assert.AreEqual(4, pcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "Browne", "Jones", "Smith", "Smithers" }, pcr.Value.Result.Select(x => x.LastName).ToArray());
        }

        #endregion

        #region GetByArgs/GetByArgsWithEf/GetDetailByArgs

        [Test, TestSetUp]
        public void D110_GetByArgs_NullArgs() => GetByArgs_NullArgs(false);

        [Test, TestSetUp]
        public void D210_GetByArgsWithEf_NullArgs() => GetByArgs_NullArgs(true);

        private void GetByArgs_NullArgs(bool useEf)
        {
            // Test with null args.
            var pcr = AgentTester.Create<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => useEf ? a.Agent.GetByArgsWithEfAsync(null) : a.Agent.GetByArgsAsync(null));

            // Check all 4 are returned in the sorted order.
            Assert.AreEqual(4, pcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "Browne", "Jones", "Smith", "Smithers" }, pcr.Value.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void D120_GetByArgs_EmptyArgs() => GetByArgs_EmptyArgs(false);

        [Test, TestSetUp]
        public void D220_GetByArgsWithEf_EmptyArgs() => GetByArgs_EmptyArgs(true);

        private void GetByArgs_EmptyArgs(bool useEf)
        {
            // Test with null args.
            var args = new PersonArgs { };
            var pcr = AgentTester.Create<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => useEf ? a.Agent.GetByArgsWithEfAsync(args) : a.Agent.GetByArgsAsync(args));

            // Check all 4 are returned in the sorted order.
            Assert.AreEqual(4, pcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "Browne", "Jones", "Smith", "Smithers" }, pcr.Value.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void D130_GetByArgs_Args_LastName() => GetByArgs_Args_LastName(false);

        [Test, TestSetUp]
        public void D230_GetByArgsWithEf_Args_LastName() => GetByArgs_Args_LastName(true);

        private void GetByArgs_Args_LastName(bool useEf)
        {
            // Test with null args.
            var args = new PersonArgs { LastName = "sm*" };
            var pcr = AgentTester.Create<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => useEf ? a.Agent.GetByArgsWithEfAsync(args) : a.Agent.GetByArgsAsync(args));

            // Check 2 are returned in the sorted order.
            Assert.AreEqual(2, pcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "Smith", "Smithers" }, pcr.Value.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void D140_GetByArgs_Args_FirstNameAndGender() => GetByArgs_Args_FirstNameAndGender(false);

        [Test, TestSetUp]
        public void D240_GetByArgsWithEf_Args_FirstNameAndGender() => GetByArgs_Args_FirstNameAndGender(true);

        private void GetByArgs_Args_FirstNameAndGender(bool useEf)
        {
            // Test with null args.
            var args = new PersonArgs { FirstName = "*a*", Genders = new RefData.ReferenceDataSidList<Gender, string>(new Gender[] { "F" }) };
            var pcr = AgentTester.Create<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => useEf ? a.Agent.GetByArgsWithEfAsync(args) : a.Agent.GetByArgsAsync(args));

            // Check 1 is returned in the sorted order.
            Assert.AreEqual(1, pcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "Browne" }, pcr.Value.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void D310_GetDetailByArgs_LastName()
        {
            var args = new PersonArgs { LastName = "sm*" };
            var pdcr = AgentTester.Create<PersonAgent, PersonDetailCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetDetailByArgsAsync(args, PagingArgs.CreateSkipAndTake(0, 2, true)));

            Assert.AreEqual(2, pdcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "Smith", "Smithers" }, pdcr.Value.Result.Select(x => x.LastName).ToArray());
            Assert.AreEqual(2, pdcr.Value.Result[0].History.Count);
            Assert.AreEqual(2, pdcr.Value.Result[1].History.Count);

            Assert.AreEqual(2, pdcr.Value.Paging.TotalCount);
        }

        #endregion

        #region Create

        [Test, TestSetUp]
        public void E110_Create()
        {
            var p = new Person
            {
                FirstName = "Bill",
                LastName = "Gates",
                Gender = "M",
                Birthday = new DateTime(1955, 10, 28),
                UniqueCode = "B7890"
            };

            // Create a person.
            p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .ExpectChangeLogCreated()
                .ExpectETag()
                .ExpectUniqueKey()
                .ExpectValue((t) => p)
                .Run((a) => a.Agent.CreateAsync(p)).Value;

            // Check the person was created properly.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => p)
                .Run((a) => a.Agent.GetAsync(p.Id));
        }

        [Test, TestSetUp]
        public void E120_Create_Duplicate()
        {
            var p = new Person
            {
                FirstName = "Bill",
                LastName = "Gates",
                Gender = "M",
                Birthday = new DateTime(1955, 10, 28),
                UniqueCode = "A1234"
            };

            // Try to create a person which will result in a duplicate.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.Conflict)
                .ExpectErrorType(ErrorType.DuplicateError)
                .Run((a) => a.Agent.CreateAsync(p));
        }

        [Test, TestSetUp]
        public void E210_CreateWithEf()
        {
            var p = new Person
            {
                FirstName = "Bill",
                LastName = "Gates",
                Gender = "M",
                Birthday = new DateTime(1955, 10, 28),
                UniqueCode = "C5678"
            };

            // Create a person.
            p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .ExpectChangeLogCreated()
                .ExpectETag()
                .ExpectUniqueKey()
                .ExpectValue((t) => p)
                .Run((a) => a.Agent.CreateWithEfAsync(p)).Value;

            // Check the person was created properly.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => p)
                .Run((a) => a.Agent.GetWithEfAsync(p.Id));
        }

        [Test, TestSetUp]
        public void E220_CreateWithEf_Duplicate()
        {
            var p = new Person
            {
                FirstName = "Bill",
                LastName = "Gates",
                Gender = "M",
                Birthday = new DateTime(1955, 10, 28),
                UniqueCode = "A1234"
            };

            // Try to create a person which will result in a duplicate.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.Conflict)
                .ExpectErrorType(ErrorType.DuplicateError)
                .Run((a) => a.Agent.CreateWithEfAsync(p));
        }

        #endregion

        #region Update

        [Test, TestSetUp]
        public void F110_Update_NotFound()
        {
            // Get an existing person.
            var p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(1.ToGuid())).Value;

            // Update with an invalid identifier.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(ErrorType.NotFoundError)
                .Run((a) => a.Agent.UpdateAsync(p, 404.ToGuid()));
        }

        [Test, TestSetUp]
        public void F120_Update_Concurrency()
        {
            // Get an existing person.
            var p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(1.ToGuid())).Value;

            // Try with an invalid If-Match value.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .ExpectErrorType(ErrorType.ConcurrencyError)
                .Run((a) => a.Agent.UpdateAsync(p, 1.ToGuid(), new WebApiRequestOptions { ETag = AgentTester.ConcurrencyErrorETag }));

            // Try updating the person with an invalid eTag.
            p.ETag = AgentTester.ConcurrencyErrorETag;

            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .ExpectErrorType(ErrorType.ConcurrencyError)
                .Run((a) => a.Agent.UpdateAsync(p, 1.ToGuid()));
        }

        [Test, TestSetUp]
        public void F130_Update_Duplicate()
        {
            // Get an existing person.
            var p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(1.ToGuid())).Value;

            // Try updating the person which will result in a duplicate.
            p.UniqueCode = "C3456";

            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.Conflict)
                .ExpectErrorType(ErrorType.DuplicateError)
                .Run((a) => a.Agent.UpdateAsync(p, 1.ToGuid()));
        }

        [Test, TestSetUp]
        public void F140_Update()
        {
            // Get an existing person.
            var p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(1.ToGuid())).Value;

            // Update the person with an address.
            p.FirstName += "X";
            p.LastName += "Y";
            p.Address = new Address { Street = "400 George Street", City = "Brisbane" };

            p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(p.ETag)
                .ExpectUniqueKey()
                .ExpectValue((t) => p)
                .Run((a) => a.Agent.UpdateAsync(p, 1.ToGuid())).Value;

            // Check the person was updated properly.
            p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => p)
                .Run((a) => a.Agent.GetAsync(p.Id)).Value;

            Assert.NotNull(p.Address);

            // Remove the address and update again.
            p.Address = null;

            p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(p.ETag)
                .ExpectUniqueKey()
                .ExpectValue((t) => p)
                .Run((a) => a.Agent.UpdateAsync(p, 1.ToGuid())).Value;

            // Check the person was updated properly.
            p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => p)
                .Run((a) => a.Agent.GetAsync(p.Id)).Value;

            Assert.Null(p.Address);
        }

        [Test, TestSetUp]
        public void F150_UpdateDetail()
        {
            // Get an existing person detail.
            var p = AgentTester.Create<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetDetailAsync(2.ToGuid())).Value;

            // Update the work history.
            p.History[0].StartDate = p.History[0].StartDate.AddDays(1);

            p = AgentTester.Create<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(p.ETag)
                .ExpectUniqueKey()
                .ExpectValue((t) => p)
                .Run((a) => a.Agent.UpdateDetailAsync(p, 2.ToGuid())).Value;

            // Check the person detail was updated properly.
            p = AgentTester.Create<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => p)
                .Run((a) => a.Agent.GetDetailAsync(p.Id)).Value;
        }

        [Test, TestSetUp]
        public void F210_UpdateWithEF_NotFound()
        {
            // Get an existing person.
            var p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(1.ToGuid())).Value;

            // Update with an invalid identifier.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(ErrorType.NotFoundError)
                .Run((a) => a.Agent.UpdateWithEfAsync(p, 404.ToGuid()));
        }

        [Test, TestSetUp]
        public void F220_UpdateWithEf_Concurrency()
        {
            // Get an existing person.
            var p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetWithEfAsync(1.ToGuid())).Value;

            // Try updating the person with an invalid eTag.
            p.ETag = AgentTester.ConcurrencyErrorETag;

            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .ExpectErrorType(ErrorType.ConcurrencyError)
                .Run((a) => a.Agent.UpdateWithEfAsync(p, 1.ToGuid()));
        }

        [Test, TestSetUp]
        public void F230_UpdateWithEf_Duplicate()
        {
            // Get an existing person.
            var p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetWithEfAsync(1.ToGuid())).Value;

            // Try updating the person which will result in a duplicate.
            p.UniqueCode = "C3456";

            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.Conflict)
                .ExpectErrorType(ErrorType.DuplicateError)
                .Run((a) => a.Agent.UpdateWithEfAsync(p, 1.ToGuid()));
        }

        [Test, TestSetUp]
        public void F240_UpdateWithEf()
        {
            // Get an existing person.
            var p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetWithEfAsync(1.ToGuid())).Value;

            // Update the person with an address.
            p.FirstName += "X";
            p.LastName += "Y";
            p.Address = new Address { Street = "400 George Street", City = "Brisbane" };

            p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(p.ETag)
                .ExpectUniqueKey()
                .ExpectValue((t) => p)
                .Run((a) => a.Agent.UpdateWithEfAsync(p, 1.ToGuid())).Value;

            // Check the person was updated properly.
            p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => p)
                .Run((a) => a.Agent.GetAsync(p.Id)).Value;

            Assert.NotNull(p.Address);

            // Remove the address and update again.
            p.Address = null;

            p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(p.ETag)
                .ExpectUniqueKey()
                .ExpectValue((t) => p)
                .Run((a) => a.Agent.UpdateWithEfAsync(p, 1.ToGuid())).Value;

            // Check the person was updated properly.
            p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => p)
                .Run((a) => a.Agent.GetWithEfAsync(p.Id)).Value;

            Assert.Null(p.Address);
        }

        #endregion

        #region Delete

        [Test, TestSetUp]
        public void G110_Delete_NotFound()
        {
            // Deleting a person that does not exist only reports success.
            AgentTester.Create<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run((a) => a.Agent.DeleteAsync(404.ToGuid()));
        }

        [Test, TestSetUp]
        public void G120_Delete()
        {
            // Check person exists.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(1.ToGuid()));

            // Delete a person.
            AgentTester.Create<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run((a) => a.Agent.DeleteAsync(1.ToGuid()));

            // Check person no longer exists.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .Run((a) => a.Agent.GetAsync(1.ToGuid()));
        }

        [Test, TestSetUp]
        public void G210_DeleteWithEf_NotFound()
        {
            // Deleting a person that does not exist only reports success.
            AgentTester.Create<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run((a) => a.Agent.DeleteWithEfAsync(404.ToGuid()));
        }

        [Test, TestSetUp]
        public void G220_DeleteWithEf()
        {
            // Check person exists.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetWithEfAsync(2.ToGuid()));

            // Delete a person.
            AgentTester.Create<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run((a) => a.Agent.DeleteWithEfAsync(2.ToGuid()));

            // Check person no longer exists.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .Run((a) => a.Agent.GetWithEfAsync(2.ToGuid()));
        }

        #endregion

        #region Patch

        [Test, TestSetUp]
        public void H110_Patch_NotFound()
        {
            // Patch with an invalid identifier.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(ErrorType.NotFoundError)
                .Run((a) => a.Agent.PatchAsync(WebApiPatchOption.MergePatch, JToken.Parse("{ \"firstName\": \"Barry\" }"), 404.ToGuid()));
        }

        [Test, TestSetUp]
        public void H120_Patch_Concurrency()
        {
            // Get an existing person.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(3.ToGuid()));

            // Try patching the person with an invalid eTag.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .ExpectErrorType(ErrorType.ConcurrencyError)
                .Run((a) => a.Agent.PatchAsync(WebApiPatchOption.MergePatch,
                    JToken.Parse("{ \"firstName\": \"Barry\" }"),
                    3.ToGuid(), new WebApiRequestOptions { ETag = AgentTester.ConcurrencyErrorETag }));
        }

        [Test, TestSetUp]
        public void H130_Patch_MergePatch()
        {
            // Get an existing person.
            var p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(3.ToGuid())).Value;

            p.FirstName = "Barry";
            p.Address = new Address { Street = "Simpsons Road", City = "Bardon" };

            // Try patching the person with an invalid eTag.
            p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectETag(p.ETag)
                .ExpectChangeLogUpdated()
                .ExpectValue(_ => p)
                .Run((a) => a.Agent.PatchAsync(WebApiPatchOption.MergePatch,
                    JToken.Parse("{ \"firstName\": \"Barry\", \"address\": { \"street\": \"Simpsons Road\", \"city\": \"Bardon\" } }"),
                    3.ToGuid(), new WebApiRequestOptions { ETag = p.ETag })).Value;

            // Check the person was patched properly.
            p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => p)
                .Run((a) => a.Agent.GetAsync(3.ToGuid())).Value;

            // Try a re-patch with no changes.
            p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => p)
                .Run((a) => a.Agent.PatchAsync(WebApiPatchOption.MergePatch,
                    JToken.Parse("{ \"firstName\": \"Barry\", \"address\": { \"street\": \"Simpsons Road\", \"city\": \"Bardon\" } }"),
                    3.ToGuid(), new WebApiRequestOptions { ETag = p.ETag })).Value;
        }

        [Test, TestSetUp]
        public void H140_Patch_JsonPatch()
        {
            // Get an existing person.
            var p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetAsync(3.ToGuid())).Value;

            p.LastName = "Simons";

            // Try patching the person with an invalid eTag.
            p = AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectETag(p.ETag)
                .ExpectChangeLogUpdated()
                .ExpectValue(_ => p)
                .Run((a) => a.Agent.PatchAsync(WebApiPatchOption.JsonPatch,
                    JToken.Parse("[ { op: \"replace\", \"path\": \"lastName\", \"value\": \"Simons\" } ]"),
                    3.ToGuid(), new WebApiRequestOptions { ETag = p.ETag })).Value;

            // Check the person was patched properly.
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => p)
                .Run((a) => a.Agent.GetAsync(3.ToGuid()));
        }

        [Test, TestSetUp]
        public void H150_PatchDetail_MergePatch_UniqueKeyCollection()
        {
            // Get an existing person detail.
            var p = AgentTester.Create<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetDetailAsync(4.ToGuid())).Value;

            var jt = JToken.Parse(
                "{ \"history\": [ { \"name\": \"Amazon\", \"endDate\": \"2018-04-16T00:00:00\" }, " +
                "{ \"name\": \"Microsoft\" }, " +
                "{ \"name\": \"Google\", \"startDate\": \"2018-04-30T00:00:00\" } ] }");

            p = AgentTester.Create<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.PatchDetailAsync(WebApiPatchOption.MergePatch, jt, 4.ToGuid(), new WebApiRequestOptions { ETag = p.ETag })).Value;

            Assert.IsNotNull(p);
            Assert.IsNotNull(p.History);
            Assert.AreEqual(3, p.History.Count);

            Assert.AreEqual("Google", p.History[0].Name);
            Assert.AreEqual(new DateTime(2018, 04, 30), p.History[0].StartDate);
            Assert.IsNull(p.History[0].EndDate);

            Assert.AreEqual("Amazon", p.History[1].Name);
            Assert.AreEqual(new DateTime(2016, 04, 16), p.History[1].StartDate);
            Assert.AreEqual(new DateTime(2018, 04, 16), p.History[1].EndDate);

            Assert.AreEqual("Microsoft", p.History[2].Name);
            Assert.AreEqual(new DateTime(2015, 05, 23), p.History[2].StartDate);
            Assert.AreEqual(new DateTime(2016, 04, 06), p.History[2].EndDate);
        }

        [Test, TestSetUp]
        public void H160_PatchDetail_MergePatch_Error()
        {
            // Get an existing person detail.
            var p = AgentTester.Create<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetDetailAsync(4.ToGuid())).Value;

            var jt = JToken.Parse(
                "{ \"history\": [ { \"name\": \"Amazon\", \"endDate\": \"2018-04-16T00:00:00\" }, " +
                "{ \"xxx\": \"Microsoft\" }, " +
                "{ \"name\": \"Google\", \"startDate\": \"xxx\" } ] }");

            AgentTester.Create<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectErrorType(ErrorType.ValidationError)
                .ExpectMessages(
                    "The JSON object must specify the 'name' token as required for the unique key.",
                    "The JSON token is malformed: The string 'xxx' was not recognized as a valid DateTime. There is an unknown word starting at index '0'.")
                .Run((a) => a.Agent.PatchDetailAsync(WebApiPatchOption.MergePatch, jt, 4.ToGuid(), new WebApiRequestOptions { ETag = p.ETag }));
        }

        #endregion

        #region Others

        [Test, TestSetUp]
        public void I110_Add()
        {
            // Do the 'Add' - which does nothing, just validates the passing of the data.
            var res = AgentTester.Create<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .Run((a) => a.Agent.AddAsync(new Person { FirstName = "Gary" }));

            // Make sure the content (body) is as expected.
            Assert.AreEqual("{\"firstName\":\"Gary\"}", res.Request.Content.ReadAsStringAsync().Result);
        }

        [Test, TestSetUp]
        public void I120_Mark()
        {
            AgentTester.Create<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.Accepted)
                .ExpectEvent("Demo.Mark", "Marked")
                .Run((a) => a.Agent.MarkAsync());

            AgentTester.Create<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.Accepted)
                .ExpectEvent("Demo.Mark", "Marked", "Wahlberg")
                .Run((a) => a.Agent.MarkAsync());
        }

        [Test, TestSetUp]
        public void I130_Null()
        {
            AgentTester.Create<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run((a) => a.Agent.GetNullAsync("blah"));
        }

        #endregion
    }
}