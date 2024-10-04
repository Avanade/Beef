using Beef.Demo.Api;
using Beef.Demo.Business;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Test.NUnit;
using CoreEx;
using CoreEx.Abstractions;
using CoreEx.Entities;
using CoreEx.Events;
using CoreEx.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using UnitTestEx;
using UnitTestEx.Expectations;

namespace Beef.Demo.Test
{
    [TestFixture, NonParallelizable]
    public class PersonTest : UsingAgentTesterServer<Startup>
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            Assert.That(ApiTester.SetUp.SetUp(), Is.True);
            ApiTester.UseExpectedEvents();
        }

        #region Validators

        [Test, TestSetUp]
        public async Task A110_Validation_Null()
        {
            await Type<IPersonManager>()
                .ExpectErrors("Value is required.")
                .RunAsync(x => x.CreateAsync(null));

            await Type<IPersonManager>()
                .ExpectErrors("Value is required.")
                .RunAsync(x => x.UpdateAsync(null, 1.ToGuid()));
        }

        [Test, TestSetUp]
        public async Task A120_Validation_Empty()
        {
            await Type<IPersonManager>()
                .ExpectErrors(
                    "First Name is required.",
                    "Last Name is required.",
                    "Gender is required.",
                    "Birthday is required.")
                .RunAsync(x => x.CreateAsync(new Business.Entities.Person()));

            await Type<IPersonManager>()
                .ExpectErrors(
                    "First Name is required.",
                    "Last Name is required.",
                    "Gender is required.",
                    "Birthday is required.")
                .RunAsync(x => x.UpdateAsync(new Business.Entities.Person(), 1.ToGuid()));
        }

        [Test, TestSetUp]
        public async Task A130_Validation_Invalid()
        {
            await Type<IPersonManager>()
                .ExpectErrors(
                    "First Name must not exceed 50 characters in length.",
                    "Last Name must not exceed 50 characters in length.",
                    "Gender is invalid.",
                    "Eye Color is invalid.",
                    "Birthday must be less than or equal to Today.")
                .RunAsync(x => x.CreateAsync(new Business.Entities.Person() { FirstName = 'x'.ToLongString(), LastName = 'x'.ToLongString(), Birthday = DateTime.Now.AddDays(1), Gender = "X", EyeColor = "Y" }));
        }

        [Test, TestSetUp]
        public void A140_Validation_Invalid2()
        {
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectErrorType(ErrorType.ValidationError)
                .ExpectErrors(
                    "First Name must not exceed 50 characters in length.",
                    "Last Name must not exceed 50 characters in length.",
                    "Gender is invalid.",
                    "Eye Color is invalid.",
                    "Birthday must be less than or equal to Today.")
                .Run(a => a.UpdateAsync(new Person() { FirstName = 'x'.ToLongString(), LastName = 'x'.ToLongString(), Birthday = DateTime.Now.AddDays(1), Gender = "X", EyeColor = "Y" }, 1.ToGuid()));
        }

        [Test, TestSetUp]
        public void A150_Validation_Detail_History_Invalid()
        {
            AgentTester.Test<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectErrorType(ErrorType.ValidationError)
                .ExpectErrors(
                    "End Date must be greater than or equal to Start Date.",
                    "Start Date must be less than or equal to today.")
                .Run(a => a.UpdateDetailAsync(new PersonDetail()
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
            AgentTester.Test<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectErrorType(ErrorType.ValidationError)
                .ExpectErrors("History contains duplicates; Name 'Google' specified more than once.")
                .Run(a => a.UpdateDetailAsync(new PersonDetail() { FirstName = "Barry", LastName = "Smith", Birthday = DateTime.Now.AddDays(-5000), Gender = "M", EyeColor = "BROWN",
                    History = new WorkHistoryCollection { new WorkHistory { Name = "Google", StartDate = new DateTime(1990, 12, 31) },
                    new WorkHistory { Name = "Google", StartDate = new DateTime(1992, 12, 31) } } }, 1.ToGuid()));
        }

        [Test, TestSetUp]
        public void A170_Validation_Metadata()
        {
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectErrorType(ErrorType.ValidationError)
                .ExpectErrors(
                    "Gender is invalid.",
                    "Description must not exceed 10 characters in length.")
                .Run(a => a.CreateAsync(new Person
                {
                    FirstName = "Barry",
                    LastName = "Smith",
                    Birthday = DateTime.Now.AddDays(-5000),
                    Gender = "M",
                    Metadata = new Dictionary<string, string> { { "X", "abc" }, { "F", "abcdefghijklmnop" } }
                }));
        }

        #endregion

        #region Get/GetDetail

        [Test, TestSetUp]
        public void B110_Get_NotFound()
        {
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(CoreEx.Abstractions.ErrorType.NotFoundError)
                .Run(a => a.GetAsync(404.ToGuid()));
        }

        [Test, TestSetUp]
        public void B120_Get_Found_No_Address()
        {
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue((t) => new Person { Id = 1.ToGuid(), FirstName = "Wendy", LastName = "Jones", Gender = "F", UniqueCode = "A1234", Birthday = new DateTime(1985, 03, 18), Metadata = new Dictionary<string, string> { { "f", "Value" } } })
                .Run(a => a.GetAsync(1.ToGuid()));
        }

        [Test, TestSetUp]
        public void B130_Get_Found_With_Address()
        {
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue((t) => new Person { Id = 3.ToGuid(), FirstName = "Rachael", LastName = "Browne", Gender = "F", Birthday = new DateTime(1972, 06, 28), Address = new Address { Street = "25 Upoko Road", City = "Wellington" } })
                .Run(a => a.GetAsync(3.ToGuid()));
        }

        [Test, TestSetUp]
        public void B140_Get_NotModified()
        {
            var p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(3.ToGuid())).Value;

            Assert.That(p, Is.Not.Null);

            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .Run(a => a.GetAsync(3.ToGuid(), new CoreEx.Http.HttpRequestOptions { ETag = p.ETag }));
                //.RunOverride(() => new PersonAgent(new DemoWebApiAgentArgs(AgentTester.GetHttpClient(), beforeRequestAsync: async r =>
                //{
                //    r.Headers.IfNoneMatch.Add(new System.Net.Http.Headers.EntityTagHeaderValue(p.ETag));
                //    await Task.CompletedTask;
                //})).GetAsync(3.ToGuid()));
        }

        [Test, TestSetUp]
        public void B150_Get_NotModified_Modified()
        {
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(3.ToGuid(), new CoreEx.Http.HttpRequestOptions { ETag = "ABCDEFG" }));
                //.RunOverride(() => new PersonAgent(new DemoWebApiAgentArgs(AgentTester.GetHttpClient(), r =>
                //{
                //    r.Headers.IfNoneMatch.Add(new System.Net.Http.Headers.EntityTagHeaderValue("\"ABCDEFG\""));
                //})).GetAsync(3.ToGuid()));
        }

        [Test, TestSetUp]
        public void B210_GetDetail_NotFound()
        {
            AgentTester.Test<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(ErrorType.NotFoundError)
                .Run(a => a.GetDetailAsync(404.ToGuid()));
        }

        [Test, TestSetUp]
        public void B220_GetDetail_NoWorkHistory()
        {
            AgentTester.Test<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue((t) => new PersonDetail { Id = 1.ToGuid(), FirstName = "Wendy", LastName = "Jones", Gender = "F", UniqueCode = "A1234", Birthday = new DateTime(1985, 03, 18), Metadata = new Dictionary<string, string> { { "f", "Value" } } })
                .Run(a => a.GetDetailAsync(1.ToGuid()));
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

            AgentTester.Test<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue((t) => pd)
                .Run(a => a.GetDetailAsync(2.ToGuid()));
        }

        [Test, TestSetUp]
        public void B310_GetWithEf_NotFound()
        {
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(ErrorType.NotFoundError)
                .Run(a => a.GetWithEfAsync(404.ToGuid()));
        }

        [Test, TestSetUp]
        public void B320_GetWithEf_Found_No_Address()
        {
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue((t) => new Person { Id = 1.ToGuid(), FirstName = "Wendy", LastName = "Jones", Gender = "F", UniqueCode = "A1234", Birthday = new DateTime(1985, 03, 18), Metadata = new Dictionary<string, string> { { "f", "Value" } } })
                .Run(a => a.GetWithEfAsync(1.ToGuid()));
        }

        [Test, TestSetUp]
        public void B320_GetWithEf_Found_With_Address()
        {
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue((t) => new Person { Id = 3.ToGuid(), FirstName = "Rachael", LastName = "Browne", Gender = "F", Birthday = new DateTime(1972, 06, 28), Address = new Address { Street = "25 Upoko Road", City = "Wellington" } })
                .Run(a => a.GetWithEfAsync(3.ToGuid()));
        }

        #endregion

        #region GetAll/GetAll2

        [Test, TestSetUp]
        public void C110_GetAll_NoPaging()
        {
            var pcr = AgentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAllAsync());

            Assert.Multiple(() =>
            {
                // Check all 4 are returned in the sorted order.
                Assert.That(pcr?.Value?.Items?.Count, Is.EqualTo(4));
                Assert.That(pcr.Value.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Browne", "Jones", "Smith", "Smithers" }));
            });
        }

        [Test, TestSetUp]
        public void C110_GetAll_Paging()
        {
            var pcr = AgentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAllAsync(PagingArgs.CreateSkipAndTake(1, 2)));

            Assert.Multiple(() =>
            {
                // Check only 2 are returned in the sorted order.
                Assert.That(pcr?.Value?.Items?.Count, Is.EqualTo(2));
                Assert.That(pcr.Value.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Jones", "Smith", }));
            });
        }

        [Test, TestSetUp]
        public void C120_GetAll_PagingAndFieldFiltering()
        {
            var pa = PagingArgs.CreateSkipAndTake(1, 2);
            var ro = new CoreEx.Http.HttpRequestOptions().Include("lastName", "firstName");

            var pcr = AgentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAllAsync(pa, ro));

            Assert.Multiple(() =>
            {
                // Check only 2 are returned in the sorted order.
                Assert.That(pcr?.Value?.Items?.Count, Is.EqualTo(2));
                Assert.That(pcr.Value.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Jones", "Smith", }));
                Assert.That(pcr.Value.Items.Any(x => x.Id != Guid.Empty), Is.False);
            });
        }

        [Test, TestSetUp]
        public void C130_GetAll2()
        {
            var pcr = AgentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAll2Async());

            Assert.Multiple(() =>
            {
                // Check all 4 are returned in the sorted order.
                Assert.That(pcr?.Value?.Items?.Count, Is.EqualTo(4));
                Assert.That(pcr.Value.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Browne", "Jones", "Smith", "Smithers" }));
            });
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
            var pcr = AgentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => useEf ? a.GetByArgsWithEfAsync(null) : a.GetByArgsAsync(null));

            Assert.Multiple(() =>
            {
                // Check all 4 are returned in the sorted order.
                Assert.That(pcr?.Value?.Items?.Count, Is.EqualTo(4));
                Assert.That(pcr.Value.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Browne", "Jones", "Smith", "Smithers" }));
            });
        }

        [Test, TestSetUp]
        public void D120_GetByArgs_EmptyArgs() => GetByArgs_EmptyArgs(false);

        [Test, TestSetUp]
        public void D220_GetByArgsWithEf_EmptyArgs() => GetByArgs_EmptyArgs(true);

        private void GetByArgs_EmptyArgs(bool useEf)
        {
            // Test with null args.
            var args = new PersonArgs { };
            var pcr = AgentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => useEf ? a.GetByArgsWithEfAsync(args) : a.GetByArgsAsync(args));

            Assert.Multiple(() =>
            {
                // Check all 4 are returned in the sorted order.
                Assert.That(pcr?.Value?.Items?.Count, Is.EqualTo(4));
                Assert.That(pcr.Value.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Browne", "Jones", "Smith", "Smithers" }));
            });
        }

        [Test, TestSetUp]
        public void D130_GetByArgs_Args_LastName() => GetByArgs_Args_LastName(false);

        [Test, TestSetUp]
        public void D230_GetByArgsWithEf_Args_LastName() => GetByArgs_Args_LastName(true);

        private void GetByArgs_Args_LastName(bool useEf)
        {
            // Test with null args.
            var args = new PersonArgs { LastName = "sm*" };
            var pcr = AgentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => useEf ? a.GetByArgsWithEfAsync(args) : a.GetByArgsAsync(args));

            Assert.Multiple(() =>
            {
                // Check 2 are returned in the sorted order.
                Assert.That(pcr?.Value?.Items?.Count, Is.EqualTo(2));
                Assert.That(pcr.Value.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Smith", "Smithers" }));
            });
        }

        [Test, TestSetUp]
        public void D140_GetByArgs_Args_FirstNameAndGender() => GetByArgs_Args_FirstNameAndGender(false);

        [Test, TestSetUp]
        public void D240_GetByArgsWithEf_Args_FirstNameAndGender() => GetByArgs_Args_FirstNameAndGender(true);

        private void GetByArgs_Args_FirstNameAndGender(bool useEf)
        {
            // Test with null args.
            var args = new PersonArgs { FirstName = "*a*", Genders = new List<string> { "F" } };
            var pcr = AgentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => useEf ? a.GetByArgsWithEfAsync(args) : a.GetByArgsAsync(args));

            Assert.Multiple(() =>
            {
                // Check 1 is returned in the sorted order.
                Assert.That(pcr?.Value?.Items?.Count, Is.EqualTo(1));
                Assert.That(pcr.Value.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Browne" }));
            });
        }

        [Test, TestSetUp]
        public void D310_GetDetailByArgs_LastName()
        {
            var args = new PersonArgs { LastName = "sm*" };
            var pdcr = AgentTester.Test<PersonAgent, PersonDetailCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetDetailByArgsAsync(args, PagingArgs.CreateSkipAndTake(0, 2, true)));

            Assert.Multiple(() =>
            {
                Assert.That(pdcr?.Value?.Items?.Count, Is.EqualTo(2));
                Assert.That(pdcr.Value.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Smith", "Smithers" }));
                Assert.That(pdcr.Value.Items[0].History, Has.Count.EqualTo(2));
                Assert.That(pdcr.Value.Items[1].History, Has.Count.EqualTo(2));
                Assert.That(pdcr.Value.Paging.TotalCount, Is.EqualTo(2));
            });
        }

        [Test, TestSetUp]
        public void D410_GetByArgs_WithEnum()
        {
            var args = new PersonArgs { OrderBy = OrderBy.FirstName };
            AgentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(args));

            // ASP.NET runtime will ignore invalid Enums and set to default (null).
            AgentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(null, requestOptions: new HttpRequestOptions { UrlQueryString = "orderBy=Bananas" }));

            // ASP.NET runtime will ignore invalid Enums and set to default (null).
            args = new PersonArgs { OrderBy = ((OrderBy)88) };
            AgentTester.Test<PersonAgent, PersonCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(args));
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
                UniqueCode = "B7890",
                Metadata = new Dictionary<string, string> { { "f", "Value" } }
            };

            // Create a person.
            p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .ExpectChangeLogCreated()
                .ExpectETag()
                .ExpectIdentifier()
                .ExpectEvent("Demo.Person.*", "Create")
                .ExpectValue((t) => p)
                .Run(a => a.CreateAsync(p)).Value;

            // Check the person was created properly.
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => p)
                .Run(a => a.GetAsync(p.Id));
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
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.Conflict)
                .ExpectErrorType(ErrorType.DuplicateError)
                .Run(a => a.CreateAsync(p));
        }

        [Test, TestSetUp]
        public void E130_Create_BadRequest()
        {
            var p = new Person
            {
                FirstName = "Bill",
                LastName = "Gates",
                Gender = "$",
                Birthday = new DateTime(1955, 10, 28),
                UniqueCode = "A1234"
            };

            // Try to create a person which will result in a bad request.
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectErrorType(ErrorType.ValidationError)
                .ExpectErrors("Gender is invalid.")
                .Run(a => a.CreateAsync(p));
        }

        [Test, TestSetUp]
        public void E130_Create_Galileo()
        {
            var p = new Person
            {
                FirstName = "Galileo",
                LastName = "Galilei",
                Gender = "M",
                Birthday = new DateTime(1564, 02, 15), //Date that is before the min value of SQL DateTime. Birthday is SQL Date
                UniqueCode = "C789"
            };

            // Create a person.
            p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .ExpectChangeLogCreated()
                .ExpectETag()
                .ExpectIdentifier()
                .ExpectEvent("Demo.Person.*", "Create")
                .ExpectValue((t) => p)
                .Run(a => a.CreateAsync(p)).Value;

            // Check the person was created properly.
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => p)
                .Run(a => a.GetAsync(p.Id));
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
            p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .ExpectChangeLogCreated()
                .ExpectETag()
                .ExpectIdentifier()
                .ExpectValue((t) => p)
                .Run(a => a.CreateWithEfAsync(p)).Value;

            // Check the person was created properly.
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => p)
                .Run(a => a.GetWithEfAsync(p.Id));
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
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.Conflict)
                .ExpectErrorType(ErrorType.DuplicateError)
                .Run(a => a.CreateWithEfAsync(p));
        }

        #endregion

        #region Update

        [Test, TestSetUp]
        public void F110_Update_NotFound()
        {
            // Get an existing person.
            var p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid())).Value;

            // Update with an invalid identifier.
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(ErrorType.NotFoundError)
                .Run(a => a.UpdateAsync(p, 404.ToGuid()));
        }

        [Test, TestSetUp]
        public void F120_Update_Concurrency()
        {
            // Get an existing person.
            var p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid())).Value;

            // Try with an invalid If-Match value.
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .ExpectErrorType(ErrorType.ConcurrencyError)
                .Run(a => a.UpdateAsync(p, 1.ToGuid(), new CoreEx.Http.HttpRequestOptions { ETag = "ZZZZZZZZZZZZ" }));

            // Try updating the person with an invalid eTag.
            p.ETag = "ZZZZZZZZZZZZ";

            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .ExpectErrorType(ErrorType.ConcurrencyError)
                .Run(a => a.UpdateAsync(p, 1.ToGuid()));
        }

        [Test, TestSetUp]
        public void F130_Update_Duplicate()
        {
            // Get an existing person.
            var p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid())).Value;

            // Try updating the person which will result in a duplicate.
            p.UniqueCode = "C3456";

            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.Conflict)
                .ExpectErrorType(ErrorType.DuplicateError)
                .Run(a => a.UpdateAsync(p, 1.ToGuid()));
        }

        [Test, TestSetUp]
        public void F140_Update()
        {
            // Get an existing person.
            var p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid())).Value;

            // Update the person with an address.
            p.FirstName += "X";
            p.LastName += "Y";
            p.Gender = "M";
            p.Address = new Address { Street = "400 George Street", City = "Brisbane" };

            p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(p.ETag)
                .ExpectIdentifier()
                .ExpectEvents()
                .ExpectValue((t) => p)
                .Run(a => a.UpdateAsync(p, 1.ToGuid())).Value;

            // Check the person was updated properly.
            p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => p)
                .Run(a => a.GetAsync(p.Id)).Value;

            Assert.That(p.Address, Is.Not.Null);

            // Remove the address and update again.
            p.Address = null;

            p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(p.ETag)
                .ExpectIdentifier()
                .ExpectEvents()
                .ExpectValue((t) => p)
                .Run(a => a.UpdateAsync(p, 1.ToGuid())).Value;

            // Check the person was updated properly.
            p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => p)
                .Run(a => a.GetAsync(p.Id)).Value;

            Assert.That(p.Address, Is.Null);
        }

        [Test, TestSetUp]
        public void F150_UpdateDetail()
        {
            // Get an existing person detail.
            var p = AgentTester.Test<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetDetailAsync(2.ToGuid())).Value;

            // Update the work history.
            p.History[0].StartDate = p.History[0].StartDate.AddDays(1);

            p = AgentTester.Test<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(p.ETag)
                .ExpectIdentifier()
                .ExpectEvents()
                .ExpectValue((t) => p)
                .Run(a => a.UpdateDetailAsync(p, 2.ToGuid())).Value;

            // Check the person detail was updated properly.
            p = AgentTester.Test<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => p)
                .Run(a => a.GetDetailAsync(p.Id)).Value;
        }

        [Test, TestSetUp]
        public void F160_Update_WithRollback()
        {
            // Get an existing person.
            var p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid())).Value;

            // Get another copy.
            var orig = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid())).Value;

            // Update the person with an address.
            p.FirstName += "X";
            p.LastName += "Y";
            p.Gender = "M";
            p.Address = new Address { Street = "400 George Street", City = "Brisbane" };

            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.InternalServerError)
                .Run(a => a.UpdateWithRollbackAsync(p, 1.ToGuid()));

            // Check the person was **NOT** updated.
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => orig)
                .Run(a => a.GetAsync(p.Id));
        }

        [Test, TestSetUp]
        public void F170_Update_OverrideSystemTime()
        {
            var st = new DateTime(1999, 12, 31, 12, 59, 59);
            var svc = new Action<Microsoft.Extensions.DependencyInjection.IServiceCollection>(sc => sc.ReplaceScoped<ISystemTime>(_ => SystemTime.CreateFixed(st)));

            using var agentTester = Beef.Test.NUnit.AgentTester.CreateWaf<Startup>(svc);
            agentTester.Parent.UseExpectedEvents();

            // Get an existing person.
            var p = agentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectNoEvents()
                .Run(a => a.GetAsync(1.ToGuid())).Value;

            p.FirstName += "X";
            p.LastName += "Y";

            p = agentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated("*", st)  // Should be 31-12-1999!
                .ExpectETag(p.ETag)
                .ExpectValue((t) => p)
                .ExpectEvents()
                .Run(a => a.UpdateAsync(p, 1.ToGuid())).Value;
        }

        [Test, TestSetUp]
        public void F210_UpdateWithEF_NotFound()
        {
            // Get an existing person.
            var p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid())).Value;

            // Update with an invalid identifier.
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(ErrorType.NotFoundError)
                .Run(a => a.UpdateWithEfAsync(p, 404.ToGuid()));
        }

        [Test, TestSetUp]
        public void F220_UpdateWithEf_Concurrency()
        {
            // Get an existing person.
            var p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetWithEfAsync(1.ToGuid())).Value;

            // Try updating the person with an invalid eTag.
            p.ETag = "ZZZZZZZZZZZZ";

            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .ExpectErrorType(ErrorType.ConcurrencyError)
                .Run(a => a.UpdateWithEfAsync(p, 1.ToGuid()));
        }

        [Test, TestSetUp]
        public void F230_UpdateWithEf_Duplicate()
        {
            // Get an existing person.
            var p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetWithEfAsync(1.ToGuid())).Value;

            // Try updating the person which will result in a duplicate.
            p.UniqueCode = "C3456";

            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.Conflict)
                .ExpectErrorType(ErrorType.DuplicateError)
                .Run(a => a.UpdateWithEfAsync(p, 1.ToGuid()));
        }

        [Test, TestSetUp]
        public void F240_UpdateWithEf()
        {
            // Get an existing person.
            var p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetWithEfAsync(1.ToGuid())).Value;

            // Update the person with an address.
            p.FirstName += "X";
            p.LastName += "Y";
            p.Address = new Address { Street = "400 George Street", City = "Brisbane" };

            p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(p.ETag)
                .ExpectIdentifier()
                .ExpectEvents()
                .ExpectValue((t) => p)
                .Run(a => a.UpdateWithEfAsync(p, 1.ToGuid())).Value;

            // Check the person was updated properly.
            p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => p)
                .Run(a => a.GetAsync(p.Id)).Value;

            Assert.That(p.Address, Is.Not.Null);

            // Remove the address and update again.
            p.Address = null;

            p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(p.ETag)
                .ExpectIdentifier()
                .ExpectEvents()
                .ExpectValue((t) => p)
                .Run(a => a.UpdateWithEfAsync(p, 1.ToGuid())).Value;

            // Check the person was updated properly.
            p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => p)
                .Run(a => a.GetWithEfAsync(p.Id)).Value;

            Assert.That(p.Address, Is.Null);
        }

        #endregion

        #region Delete

        [Test, TestSetUp]
        public void G110_Delete_NotFound()
        {
            // Deleting a person that does not exist only reports success.
            AgentTester.Test<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.DeleteAsync(404.ToGuid()));
        }

        [Test, TestSetUp]
        public void G120_Delete()
        {
            // Check person exists.
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid()));

            // Delete a person.
            AgentTester.Test<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .ExpectEvents()
                .Run(a => a.DeleteAsync(1.ToGuid()));

            // Check person no longer exists.
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(ErrorType.NotFoundError)
                .Run(a => a.GetAsync(1.ToGuid()));
        }

        [Test, TestSetUp]
        public void G210_DeleteWithEf_NotFound()
        {
            // Deleting a person that does not exist only reports success.
            AgentTester.Test<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.DeleteWithEfAsync(404.ToGuid()));
        }

        [Test, TestSetUp]
        public void G220_DeleteWithEf()
        {
            // Check person exists.
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetWithEfAsync(2.ToGuid()));

            // Delete a person.
            AgentTester.Test<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .ExpectEvent("Demo.Person.*", "Delete")
                .Run(a => a.DeleteWithEfAsync(2.ToGuid()));

            // Check person no longer exists.
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(ErrorType.NotFoundError)
                .Run(a => a.GetWithEfAsync(2.ToGuid()));
        }

        #endregion

        #region Patch

        [Test, TestSetUp]
        public void H110_Patch_NotFound()
        {
            // Patch with an invalid identifier.
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(ErrorType.NotFoundError)
                .Run(a => a.PatchAsync(HttpPatchOption.MergePatch, "{ \"firstName\": \"Barry\" }", 404.ToGuid()));
        }

        [Test, TestSetUp]
        public void H120_Patch_Concurrency()
        {
            // Get an existing person.
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(3.ToGuid()));

            // Try patching the person with an invalid eTag.
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .ExpectErrorType(ErrorType.ConcurrencyError)
                .Run(a => a.PatchAsync(HttpPatchOption.MergePatch, "{ \"firstName\": \"Barry\" }", 3.ToGuid(), new HttpRequestOptions { ETag = "ZZZZZZZZZZZZ" }));
        }

        [Test, TestSetUp]
        public void H130_Patch_MergePatch()
        {
            // Get an existing person.
            var p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(3.ToGuid())).Value;

            p.FirstName = "Barry";
            p.Address = new Address { Street = "Simpsons Road", City = "Bardon" };
            p.Metadata = new Dictionary<string, string> { { "m", "MVAL" } };

            // Try patching the person with an invalid eTag.
            p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectETag(p.ETag)
                .ExpectChangeLogUpdated()
                .ExpectEvents()
                .ExpectValue(_ => p)
                .Run(a => a.PatchAsync(HttpPatchOption.MergePatch,
                    "{ \"firstName\": \"Barry\", \"address\": { \"street\": \"Simpsons Road\", \"city\": \"Bardon\" }, \"metadata\": { \"m\": \"MVAL\" } }",
                    3.ToGuid(), new HttpRequestOptions { ETag = p.ETag })).Value;

            // Check the person was patched properly.
            p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => p)
                .Run(a => a.GetAsync(3.ToGuid())).Value;

            // Try a re-patch with no changes.
            p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => p)
                .Run(a => a.PatchAsync(HttpPatchOption.MergePatch,
                    "{ \"firstName\": \"Barry\", \"address\": { \"street\": \"Simpsons Road\", \"city\": \"Bardon\" } }",
                    3.ToGuid(), new HttpRequestOptions { ETag = p.ETag })).Value;
        }

        [Test, TestSetUp]
        public void H140_Patch_JsonPatch()
        {
            Assert.Warn("HttpPatchOption.JsonPatch no longer supported; requires Newtonsoft libraries to enable: https://learn.microsoft.com/en-us/aspnet/core/web-api/jsonpatch");

            //// Get an existing person.
            //var p = AgentTester.Test<PersonAgent, Person>()
            //    .ExpectStatusCode(HttpStatusCode.OK)
            //    .Run(a => a.GetAsync(3.ToGuid())).Value;

            //p.LastName = "Simons";

            //// Try patching the person with an invalid eTag.
            //p = AgentTester.Test<PersonAgent, Person>()
            //    .ExpectStatusCode(HttpStatusCode.OK)
            //    .ExpectETag(p.ETag)
            //    .ExpectChangeLogUpdated()
            //    .ExpectEvents()
            //    .ExpectValue(_ => p)
            //    .Run(a => a.PatchAsync(HttpPatchOption.JsonPatch,
            //        "[ { op: \"replace\", \"path\": \"lastName\", \"value\": \"Simons\" } ]",
            //        3.ToGuid(), new HttpRequestOptions { ETag = p.ETag })).Value;

            //// Check the person was patched properly.
            //AgentTester.Test<PersonAgent, Person>()
            //    .ExpectStatusCode(HttpStatusCode.OK)
            //    .ExpectValue(_ => p)
            //    .Run(a => a.GetAsync(3.ToGuid()));
        }

        [Test, TestSetUp]
        public void H150_PatchDetail_MergePatch_UniqueKeyCollection()
        {
            //Assert.Warn("HttpPatchOption.JsonPatch no longer supported; requires Newtonsoft libraries to enable: https://learn.microsoft.com/en-us/aspnet/core/web-api/jsonpatch");

            // Get an existing person detail.
            var p = AgentTester.Test<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetDetailAsync(4.ToGuid())).Value;

            var jt =
                "{ \"history\": [ { \"name\": \"amazon\", \"endDate\": \"2018-04-16T00:00:00\" }, " +
                "{ \"name\": \"Microsoft\" }, " +
                "{ \"name\": \"Google\", \"startDate\": \"2018-04-30T00:00:00\" } ] }";

            p = AgentTester.Test<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectEvents()
                .Run(a => a.PatchDetailAsync(HttpPatchOption.MergePatch, jt, 4.ToGuid(), new HttpRequestOptions { ETag = p.ETag })).Value;

            Assert.That(p, Is.Not.Null);
            Assert.That(p.History, Is.Not.Null);
            Assert.That(p.History, Has.Count.EqualTo(3));

            Assert.Multiple(() =>
            {
                Assert.That(p.History[0].Name, Is.EqualTo("Google"));
                Assert.That(p.History[0].StartDate, Is.EqualTo(new DateTime(2018, 04, 30)));
                Assert.That(p.History[0].EndDate, Is.Null);

                Assert.That(p.History[1].Name, Is.EqualTo("Amazon"));
                Assert.That(p.History[1].StartDate, Is.EqualTo(new DateTime(2016, 04, 16)));
                Assert.That(p.History[1].EndDate, Is.EqualTo(new DateTime(2018, 04, 16)));

                Assert.That(p.History[2].Name, Is.EqualTo("Microsoft"));
                Assert.That(p.History[2].StartDate, Is.EqualTo(new DateTime(2015, 05, 23)));
                Assert.That(p.History[2].EndDate, Is.EqualTo(new DateTime(2016, 04, 06)));
            });
        }

        [Test, TestSetUp]
        public void H160_PatchDetail_MergePatch_Error()
        {
            // Get an existing person detail.
            var p = AgentTester.Test<PersonAgent, PersonDetail>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetDetailAsync(4.ToGuid())).Value;

            var jt =
                "{ \"history\": [ { \"name\": \"Amazon\", \"endDate\": \"2018-04-16T00:00:00\" }, " +
                "{ \"xxx\": \"Microsoft\" }, " +
                "{ \"name\": \"Google\", \"startDate\": \"xxx\" } ] }";

            AgentTester.Test<PersonAgent, PersonDetail>()
                .Run(a => a.PatchDetailAsync(HttpPatchOption.MergePatch, jt, 4.ToGuid(), new HttpRequestOptions { ETag = p.ETag }))
                .AssertContentTypePlainText()
                .Assert(HttpStatusCode.BadRequest, "The JSON value could not be converted to System.DateTime. Path: $.history[2].startDate | LineNumber: 0 | BytePositionInLine: 133.");
        }

        [Test, TestSetUp]
        public void H210_PatchWithEF_MergePatch()
        {
            // Get an existing person.
            var p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(3.ToGuid())).Value;

            p.FirstName = "Bob";
            p.Address = new Address { Street = "Simpsons Road", City = "Bardon" };

            // Try patching the person.
            p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectETag(p.ETag)
                .ExpectChangeLogUpdated()
                .ExpectEvents()
                .ExpectValue(_ => p)
                .Run(a => a.PatchWithEfAsync(HttpPatchOption.MergePatch,
                    "{ \"firstName\": \"Bob\", \"address\": { \"street\": \"Simpsons Road\", \"city\": \"Bardon\" } }",
                    3.ToGuid(), new HttpRequestOptions { ETag = p.ETag })).Value;

            // Check the person was patched properly.
            p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => p)
                .Run(a => a.GetAsync(3.ToGuid())).Value;

            // Try a re-patch with no changes.
            p = AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => p)
                .Run(a => a.PatchWithEfAsync(HttpPatchOption.MergePatch,
                    "{ \"firstName\": \"Bob\", \"address\": { \"street\": \"Simpsons Road\", \"city\": \"Bardon\" } }",
                    3.ToGuid(), new HttpRequestOptions { ETag = p.ETag })).Value;
        }

        #endregion

        #region Others

        [Test, TestSetUp]
        public void I110_Add()
        {
            // Do the 'Add' - which does nothing, just validates the passing of the data.
            var res = AgentTester.Test<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .Run(a => a.AddAsync(new Person { FirstName = "Gary" }));

            // Make sure the content (body) is as expected.
            Assert.That(res.Response.RequestMessage.Content.ReadAsStringAsync().Result, Is.EqualTo("{\"firstName\":\"Gary\"}"));
        }

        [Test, TestSetUp]
        public void I120_Mark()
        {
            AgentTester.Test<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.Accepted)
                .ExpectEvent("Demo.Mark", "Marked")
                .Run(a => a.MarkAsync());

            AgentTester.Test<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.Accepted)
                .ExpectEvent(new EventData<string> { Subject = "Demo.Mark", Action = "Marked", Value = "Wahlberg" })
                .Run(a => a.MarkAsync());
        }

        [Test, TestSetUp]
        public void I130_Null()
        {
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetNullAsync("blah", null));
        }

        [Test, TestSetUp]
        public void I140_Map()
        {
            AgentTester.Test<PersonAgent, MapCoordinates>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => new MapCoordinates { Longitude = 1.234m, Latitude = -6.789m })
                .Run(a => a.MapAsync(new MapArgs { Coordinates = new MapCoordinates { Longitude = 1.234m, Latitude = -6.789m } }));
        }

        [Test, TestSetUp]
        public void I150_ThrowError()
        {
            AgentTester.Test<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.InternalServerError)
                .Run(a => a.ThrowErrorAsync());
        }

        [Test, TestSetUp]
        public void I160_GetNoArgs()
        {
            AgentTester.Test<PersonAgent, Person>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectETag()
                .ExpectValue(_ => new Person { FirstName = "No", LastName = "Args" })
                .Run(a => a.GetNoArgsAsync());
        }

        [Test, TestSetUp]
        public void I210_InvokeApiViaAgent_Mocked()
        {
            Mock<IPersonAgent> mock = new();
            mock.Setup(x => x.GetAsync(1.ToGuid(), null, CancellationToken.None)).ReturnsHttpResultAsync(new Person { LastName = "Mockulater" });

            var svc = new Action<IServiceCollection>(sc => sc.ReplaceScoped(_ => mock.Object));

            using var agentTester = Beef.Test.NUnit.AgentTester.CreateWaf<Startup>(svc);
            
            agentTester.Test<PersonAgent, string>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => "Mockulater")
                .Run(a => a.InvokeApiViaAgentAsync(1.ToGuid()));
        }

        [Test, TestSetUp]
        public void I310_EventPublishNoSend()
        {
            var ex = Assert.Throws<AssertionException>(() =>
                AgentTester.Test<PersonAgent, Person>()
                    .ExpectStatusCode(HttpStatusCode.OK)
                    .ExpectEvents()
                    .Run(a => a.EventPublishNoSendAsync(new Person { FirstName = "John", LastName = "Doe", Gender = "M", Birthday = new DateTime(200, 01, 01) })));

            Assert.That(ex, Is.Not.Null);
            Assert.That(ex.Message, Is.EqualTo("Expected Event Publish/Send mismatch; there are one or more published events that have not been sent."));
        }

        [Test, TestSetUp]
        public void I410_ParamColl_Error()
        {
            AgentTester.Test<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectErrors(
                     "Addresses must not exceed 2 item(s).",
                     "Street is required.")
                .Run(a => a.ParamCollAsync(new AddressCollection { new Address { Street = "Aaa", City = "Bbb" }, new Address { Street = "Ccc", City = "Ddd" }, new Address { City = "Xxx" } }));
        }

        [Test, TestSetUp]
        public void I410_ParamColl_Duplicate()
        {
            AgentTester.Test<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectErrors("Addresses contains duplicates; Street 'Aaa' specified more than once.")
                .Run(a => a.ParamCollAsync(new AddressCollection { new Address { Street = "Aaa", City = "Bbb" }, new Address { Street = "Aaa", City = "Ddd" }}));
        }

        [Test]
        public void I420_Simulate_Perf()
        {
            for (int i = 0; i < 100; i++)
            {
                AgentTester.Test<PersonAgent, string>()
                    .ExpectStatusCode(HttpStatusCode.OK)
                    .ExpectEvent("Work", "Simulated")
                    .Run(a => a.SimulateWorkAsync(Guid.NewGuid()));
            }
        }

        #endregion

        #region ExtendResponse

        [Test, TestSetUp]
        public void J110_ExtendResponse_Error()
        {
            AgentTester.Test<PersonAgent>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectError("Name is needed dude!")
                .Run(a => a.ExtendResponseAsync(null));
        }

        [Test, TestSetUp]
        public void J120_ExtendResponse_Success()
        {
            var resp = AgentTester.Test<PersonAgent, string>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue("Sweet!")
                .Run(a => a.ExtendResponseAsync("Sweet!"));

            if (resp.Response.Headers.TryGetValues("X-Beef-Test", out var values))
                Assert.That(values, Is.EquivalentTo(new string[] { "123" }));
        }

        #endregion
    }
}