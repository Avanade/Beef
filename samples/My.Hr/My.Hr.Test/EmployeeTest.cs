using Beef;
using Beef.Entities;
using Beef.Test.NUnit;
using Beef.WebApi;
using My.Hr.Api;
using My.Hr.Business.Validation;
using My.Hr.Common.Agents;
using My.Hr.Common.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace My.Hr.Test
{
    [TestFixture, NonParallelizable]
    public class EmployeeTest : UsingAgentTesterServer<Startup>
    {
        #region Validators

        [Test, TestSetUp]
        public void A110_Validation_Empty()
        {
            ExecutionContext.Current.OperationType = OperationType.Create;
            ExpectValidationException.Throws(
                () => EmployeeValidator.Default.Validate(new Employee()).ThrowOnError(),
                "First Name is required.",
                "Email is required.",
                "Last Name is required.",
                "Gender is required.",
                "Birthday is required.",
                "Start Date is required.",
                "Phone No is required.");
        }

        [Test, TestSetUp]
        public void A120_Validation_Invalid()
        {
            ExecutionContext.Current.OperationType = OperationType.Create;
            ExpectValidationException.Throws(
                () => EmployeeValidator.Default.Validate(new Employee { Email = "xxx", FirstName = 'x'.ToLongString(), LastName = 'x'.ToLongString(), GenderSid = "X", Birthday = DateTime.Now.AddYears(10), StartDate = new DateTime(1996, 12, 31), PhoneNo = "(425) 333 4444" }).ThrowOnError(),
                "Email is invalid.",
                "First Name must not exceed 100 characters in length.",
                "Last Name must not exceed 100 characters in length.",
                "Gender is invalid.",
                "Birthday is invalid as the Employee must be at least 18 years of age.",
                "Start Date must be greater than or equal to January 1, 1999.");
        }

        #endregion

        #region Get

        [Test, TestSetUp]
        public void B110_Get_NotFound()
        {
            AgentTester.Test<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .Run(a => a.GetAsync(404.ToGuid()));
        }

        [Test, TestSetUp]
        public void B120_Get_Found_NoAddress()
        {
            AgentTester.Test<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue(_ => new Employee
                {
                    Id = 1.ToGuid(),
                    Email = "w.jones@org.com",
                    FirstName = "Wendy",
                    LastName = "Jones",
                    GenderSid = "F",
                    Birthday = new DateTime(1985, 03, 18),
                    StartDate = new DateTime(2000, 12, 11),
                    PhoneNo = "(425) 612 8113"
                })
                .Run(a => a.GetAsync(1.ToGuid()));
        }

        [Test, TestSetUp]
        public void B120_Get_Found_WithAddress()
        {
            AgentTester.Test<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue(_ => new Employee
                {
                    Id = 4.ToGuid(),
                    Email = "w.smither@org.com",
                    FirstName = "Waylon",
                    LastName = "Smithers",
                    GenderSid = "M",
                    Birthday = new DateTime(1952, 02, 21),
                    StartDate = new DateTime(2001, 01, 22),
                    PhoneNo = "(428) 893 2793",
                    Address = new Address { Street1 = "8365 851 PL NE", City = "Redmond", StateSid = "WA", PostCode = "98052" },
                    EmergencyContacts = new EmergencyContactCollection { new EmergencyContact { Id = 401.ToGuid(), FirstName = "Michael", LastName = "Manners", PhoneNo = "(234) 297 9834", RelationshipSid = "FRD" } }
                })
                .Run(a => a.GetAsync(4.ToGuid()));
        }

        [Test, TestSetUp]
        public void B120_Get_Modified_NotModified()
        {
            var v = AgentTester.Test<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid(), new WebApiRequestOptions { ETag = TestSetUp.ConcurrencyErrorETag })).Value!;

            Assert.NotNull(v);

            AgentTester.Test<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .Run(a => a.GetAsync(1.ToGuid(), new WebApiRequestOptions { ETag = v.ETag }));
        }

        #endregion

        #region GetByArgs

        [Test, TestSetUp]
        public void B210_GetByArgs_All()
        {
            var v = AgentTester.Test<EmployeeAgent, EmployeeBaseCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(null)).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(3, v.Result.Count);
            Assert.AreEqual(new string[] { "Browne", "Jones", "Smithers" }, v.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void B210_GetByArgs_All_Paging()
        {
            var v = AgentTester.Test<EmployeeAgent, EmployeeBaseCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new EmployeeArgs { IsIncludeTerminated = true }, PagingArgs.CreateSkipAndTake(1,2))).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(2, v.Result.Count);
            Assert.AreEqual(new string[] { "Jones", "Smith" }, v.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void B220_GetByArgs_FirstName()
        {
            var v = AgentTester.Test<EmployeeAgent, EmployeeBaseCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new EmployeeArgs { FirstName = "*a*" })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(2, v.Result.Count);
            Assert.AreEqual(new string[] { "Browne", "Smithers" }, v.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void B230_GetByArgs_LastName()
        {
            var v = AgentTester.Test<EmployeeAgent, EmployeeBaseCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new EmployeeArgs { LastName = "s*" })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(1, v.Result.Count);
            Assert.AreEqual(new string[] { "Smithers" }, v.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void B230_GetByArgs_LastName_IncludeTerminated()
        {
            var v = AgentTester.Test<EmployeeAgent, EmployeeBaseCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new EmployeeArgs { LastName = "s*", IsIncludeTerminated = true })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(2, v.Result.Count);
            Assert.AreEqual(new string[] { "Smith", "Smithers" }, v.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void B240_GetByArgs_Gender()
        {
            var v = AgentTester.Test<EmployeeAgent, EmployeeBaseCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new EmployeeArgs { GendersSids = new List<string> { "F" } })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(2, v.Result.Count);
            Assert.AreEqual(new string[] { "Browne", "Jones" }, v.Result.Select(x => x.LastName).ToArray());
        }

        [Test, TestSetUp]
        public void B250_GetByArgs_Empty()
        {
            var v = AgentTester.Test<EmployeeAgent, EmployeeBaseCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new EmployeeArgs { LastName = "s*", FirstName = "b*", GendersSids = new List<string> { "F" } })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v.Result);
            Assert.AreEqual(0, v.Result.Count);
        }

        [Test, TestSetUp]
        public void B260_GetByArgs_FieldSelection()
        {
            var r = AgentTester.Test<EmployeeAgent, EmployeeBaseCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new EmployeeArgs { GendersSids = new List<string> { "F" } }, requestOptions: new WebApiRequestOptions().Include("firstname", "lastname")));

            Assert.IsNotNull(r.Value);
            Assert.IsNotNull(r.Value.Result);
            Assert.AreEqual(2, r.Value.Result.Count);
            Assert.AreEqual(new string[] { "Browne", "Jones" }, r.Value.Result.Select(x => x.LastName).ToArray());

            Assert.AreEqual("[{\"firstName\":\"Rachael\",\"lastName\":\"Browne\"},{\"firstName\":\"Wendy\",\"lastName\":\"Jones\"}]", r.Content);
        }

        [Test, TestSetUp]
        public void B270_GetByArgs_RefDataText()
        {
            var r = AgentTester.Test<EmployeeAgent, EmployeeBaseCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new EmployeeArgs { GendersSids = new List<string> { "F" } }, requestOptions: new WebApiRequestOptions { IncludeRefDataText = true }));

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
            var v = new Employee
            {
                Email = "j.smith@org.com",
                FirstName = "Jill",
                LastName = "Smith",
                GenderSid = "F",
                Birthday = new DateTime(1955, 10, 28),
                StartDate = DateTime.Today,
                PhoneNo = "(456) 789 0123",
                Address = new Address { Street1 = "2732 85 PL NE", City = "Bellevue", StateSid = "WA", PostCode = "98101" },
                EmergencyContacts = new EmergencyContactCollection { new EmergencyContact { FirstName = "Danny", LastName = "Keen", PhoneNo = "(234) 297 9834", RelationshipSid = "FRD" } }
            };

            // Create value.
            v = AgentTester.Test<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .ExpectChangeLogCreated()
                .ExpectETag()
                .ExpectUniqueKey()
                .ExpectValue(_ => v)
                .ExpectEvent("My.Hr.Employee.*", "Created")
                .Run(a => a.CreateAsync(v)).Value!;

            // Check the value was created properly.
            AgentTester.Test<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => v)
                .Run(a => a.GetAsync(v.Id));
        }

        #endregion

        #region Update

        [Test, TestSetUp]
        public void D110_Update_NotFound()
        {
            // Get an existing value.
            var v = AgentTester.Test<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(4.ToGuid())).Value!;

            // Try updating with an invalid identifier.
            AgentTester.Test<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(ErrorType.NotFoundError)
                .Run(a => a.UpdateAsync(v, 404.ToGuid()));
        }

        [Test, TestSetUp]
        public void D120_Update_Concurrency()
        {
            // Get an existing value.
            var id = 4.ToGuid();
            var v = AgentTester.Test<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id)).Value!;

            // Try updating the value with an invalid eTag (if-match).
            AgentTester.Test<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .ExpectErrorType(ErrorType.ConcurrencyError)
                .Run(a => a.UpdateAsync(v, id, new WebApiRequestOptions { ETag = TestSetUp.ConcurrencyErrorETag }));

            // Try updating the value with an invalid eTag.
            v.ETag = TestSetUp.ConcurrencyErrorETag;
            AgentTester.Test<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .ExpectErrorType(ErrorType.ConcurrencyError)
                .Run(a => a.UpdateAsync(v, id));
        }

        [Test, TestSetUp]
        public void D130_Update()
        {
            // Get an existing value.
            var id = 4.ToGuid();
            var v = AgentTester.Test<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id)).Value!;

            // Make some changes to the data.
            v.FirstName += "X";
            v.LastName += "Y";
            v.Address!.Street2 = "Street 2";
            v.EmergencyContacts![0].FirstName += "Y";
            v.EmergencyContacts.Add(new EmergencyContact { FirstName = "Danny", LastName = "Keen", PhoneNo = "(234) 297 9834", RelationshipSid = "FRD" });

            // Update the value.
            v = AgentTester.Test<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(v.ETag)
                .ExpectUniqueKey()
                .ExpectValue(_ => v)
                .ExpectEvent($"My.Hr.Employee.{id}", "Updated")
                .Run(a => a.UpdateAsync(v, id)).Value!;

            // Check the value was updated properly.
            AgentTester.Test<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => v)
                .Run(a => a.GetAsync(id));
        }

        [Test, TestSetUp]
        public void D140_Update_AlreadyTerminated()
        {
            // Get an existing value.
            var id = 2.ToGuid();
            var v = AgentTester.Test<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id)).Value!;

            // Make some changes to the data.
            v.FirstName += "X";
            v.LastName += "Y";

            // Update the value.
            var r = AgentTester.Test<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .Run(a => a.UpdateAsync(v, id));

            Assert.AreEqual("Once an Employee has been Terminated the data can no longer be updated.", r.Content);
        }

        #endregion

        #region Patch

        [Test, TestSetUp]
        public void E110_Patch_NotFound()
        {
            // Get an existing value.
            var v = AgentTester.Test<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(4.ToGuid())).Value!;

            // Try patching with an invalid identifier.
            AgentTester.Test<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(ErrorType.NotFoundError)
                .Run(a => a.PatchAsync(WebApiPatchOption.MergePatch, "{ \"lastName\": \"Smithers\" }", 404.ToGuid()));
        }

        [Test, TestSetUp]
        public void E120_Patch_Concurrency()
        {
            // Get an existing value.
            var id = 4.ToGuid();
            var v = AgentTester.Test<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id)).Value!;

            // Try updating the value with an invalid eTag (if-match).
            AgentTester.Test<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .ExpectErrorType(ErrorType.ConcurrencyError)
                .Run(a => a.PatchAsync(WebApiPatchOption.MergePatch, "{ \"lastName\": \"Smithers\" }", id, new WebApiRequestOptions { ETag = TestSetUp.ConcurrencyErrorETag }));

            // Try updating the value with an eTag header (json payload eTag is ignored).
            v.ETag = TestSetUp.ConcurrencyErrorETag;
            AgentTester.Test<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .ExpectErrorType(ErrorType.ConcurrencyError)
                .Run(a => a.PatchAsync(WebApiPatchOption.MergePatch, "{{ \"lastName\": \"Smithers\", \"etag\": {TestSetUp.ConcurrencyErrorETag} }}", id));
        }

        [Test, TestSetUp]
        public void E130_Patch()
        {
            // Get an existing value.
            var id = 4.ToGuid();
            var v = AgentTester.Test<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id)).Value!;

            // Make some changes to the data.
            v.LastName = "Bartholomew";
            v.EmergencyContacts = null;

            // Update the value.
            v = AgentTester.Test<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(v.ETag)
                .ExpectUniqueKey()
                .ExpectValue(_ => v)
                .ExpectEvent($"My.Hr.Employee.{id}", "Updated")
                .Run(a => a.PatchAsync(WebApiPatchOption.MergePatch, $"{{ \"lastName\": \"{v.LastName}\", \"emergencyContacts\": null }}", id, new WebApiRequestOptions { ETag = v.ETag })).Value;

            // Check the value was updated properly.
            AgentTester.Test<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => v)
                .Run(a => a.GetAsync(id));
        }

        #endregion

        #region Delete

        [Test, TestSetUp]
        public void F110_Delete()
        {
            // Check value exists.
            var id = 4.ToGuid();
            AgentTester.Test<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id));

            // Delete value.
            AgentTester.Test<EmployeeAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .ExpectEvent($"My.Hr.Employee.{id}", "Deleted")
                .Run(a => a.DeleteAsync(id));

            // Check value no longer exists.
            AgentTester.Test<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .Run(a => a.GetAsync(id));

            // Delete again (should still be successful as a Delete is idempotent). 
            AgentTester.Test<EmployeeAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .ExpectEvent($"My.Hr.Employee.{id}", "Deleted")
                .Run(a => a.DeleteAsync(id));
        }

        #endregion

        #region Terminate

        [Test, TestSetUp]
        public void G110_Terminate_NotFound()
        {
            AgentTester.Test<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.TerminateAsync(new TerminationDetail { Date = DateTime.Now, ReasonSid = "RD" }, 404.ToGuid()));
        }

        [Test, TestSetUp]
        public void G120_Terminate_MoreThanOnce()
        {
            var r = AgentTester.Test<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .Run(a => a.TerminateAsync(new TerminationDetail { Date = DateTime.Now, ReasonSid = "RD" }, 2.ToGuid()));

            Assert.AreEqual("An Employee can not be terminated more than once.", r.Content);
        }

        [Test, TestSetUp]
        public void G130_Terminate_BeforeStart()
        {
            var r = AgentTester.Test<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .Run(a => a.TerminateAsync(new TerminationDetail { Date = new DateTime(1999, 12, 31), ReasonSid = "RD" }, 1.ToGuid()));

            Assert.AreEqual("An Employee can not be terminated prior to their start date.", r.Content);
        }

        [Test, TestSetUp]
        public void G140_Terminate()
        {
            var v = AgentTester.Test<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid())).Value!;

            v.Termination = new TerminationDetail { Date = DateTime.Now, ReasonSid = "RD" };

            v = AgentTester.Test<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(v.ETag)
                .ExpectUniqueKey()
                .ExpectValue(_ => v)
                .ExpectEvent($"My.Hr.Employee.{v.Id}", "Terminated")
                .Run(a => a.TerminateAsync(v.Termination, 1.ToGuid())).Value!;

            AgentTester.Test<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => v)
                .Run(a => a.GetAsync(1.ToGuid()));
        }

        #endregion
    }
}