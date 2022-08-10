﻿using CoreEx.Abstractions;
using CoreEx.Entities;
using CoreEx.Http;
using My.Hr.Api;
using My.Hr.Common.Agents;
using My.Hr.Common.Entities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnitTestEx;
using UnitTestEx.Expectations;
using UnitTestEx.NUnit;

namespace My.Hr.Test.Apis
{
    [TestFixture, NonParallelizable]
    public class EmployeeTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => TestSetUp.Default.SetUp();

        #region Get

        [Test]
        public void A110_Get_NotFound()
        {
            using var agentTester = ApiTester.Create<Startup>();

            agentTester.Agent<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(404.ToGuid()));
        }

        [Test]
        public void A120_Get_Found_NoAddress()
        {
            using var agentTester = ApiTester.Create<Startup>();

            agentTester.Agent<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue(_ => new Employee
                {
                    Id = 1.ToGuid(),
                    Email = "w.jones@org.com",
                    FirstName = "Wendy",
                    LastName = "Jones",
                    Gender = "F",
                    Birthday = new DateTime(1985, 03, 18),
                    StartDate = new DateTime(2000, 12, 11),
                    PhoneNo = "(425) 612 8113"
                })
                .Run(a => a.GetAsync(1.ToGuid()));
        }

        [Test]
        public void A130_Get_Found_WithAddress()
        {
            using var agentTester = ApiTester.Create<Startup>();

            agentTester.Agent<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue(_ => new Employee
                {
                    Id = 4.ToGuid(),
                    Email = "w.smither@org.com",
                    FirstName = "Waylon",
                    LastName = "Smithers",
                    Gender = "M",
                    Birthday = new DateTime(1952, 02, 21),
                    StartDate = new DateTime(2001, 01, 22),
                    PhoneNo = "(428) 893 2793",
                    Address = new Address { Street1 = "8365 851 PL NE", City = "Redmond", State = "WA", PostCode = "98052" },
                    EmergencyContacts = new EmergencyContactCollection { new EmergencyContact { Id = 401.ToGuid(), FirstName = "Michael", LastName = "Manners", PhoneNo = "(234) 297 9834", Relationship = "FRD" } }
                })
                .Run(a => a.GetAsync(4.ToGuid()));
        }

        [Test]
        public void A140_Get_Modified_NotModified()
        {
            using var agentTester = ApiTester.Create<Startup>();

            var v = agentTester.Agent<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid(), new HttpRequestOptions { ETag = TestSetUp.Default.ConcurrencyErrorETag })).Value!;

            Assert.NotNull(v);

            agentTester.Agent<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .Run(a => a.GetAsync(1.ToGuid(), new HttpRequestOptions { ETag = v.ETag }));
        }

        [Test]
        public void A150_Get_IncludeRefDataText()
        {
            using var agentTester = ApiTester.Create<Startup>();

            var v = agentTester.Agent<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid(), new HttpRequestOptions { IncludeText = true})).Value!;

            Assert.NotNull(v);
            Assert.AreEqual("Female", v.GenderText);
        }

        [Test]
        public void A160_Get_IncludeFields()
        {
            using var agentTester = ApiTester.Create<Startup>();

            agentTester.Agent<EmployeeAgent>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid(), new HttpRequestOptions().Include(new string[] { "firstName", "lastName" })))
                .AssertJson("{\"firstName\":\"Wendy\",\"lastName\":\"Jones\"}");
        }

        #endregion

        #region GetByArgs

        [Test]
        public void A210_GetByArgs_All()
        {
            using var agentTester = ApiTester.Create<Startup>();

            var v = agentTester.Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(null)).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v!.Collection);
            Assert.AreEqual(3, v.Collection.Count);
            Assert.AreEqual(new string[] { "Browne", "Jones", "Smithers" }, v.Collection.Select(x => x.LastName).ToArray());
        }

        [Test]
        public void A220_GetByArgs_All_Paging()
        {
            using var agentTester = ApiTester.Create<Startup>();

            var r = agentTester.Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new EmployeeArgs { IsIncludeTerminated = true }, PagingArgs.CreateSkipAndTake(1,2)));

            var v = r.Value;
            Assert.IsNotNull(v);
            Assert.IsNotNull(v!.Collection);
            Assert.AreEqual(2, v.Collection.Count);
            Assert.AreEqual(new string[] { "Jones", "Smith" }, v.Collection.Select(x => x.LastName).ToArray());

            // Query again with etag and ensure not modified.
            agentTester.Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .Run(a => a.GetByArgsAsync(new EmployeeArgs { IsIncludeTerminated = true }, PagingArgs.CreateSkipAndTake(1, 2), new HttpRequestOptions { ETag = r.Response!.Headers!.ETag!.Tag }));
        }

        [Test]
        public void A230_GetByArgs_FirstName()
        {
            using var agentTester = ApiTester.Create<Startup>();

            var v = agentTester.Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new EmployeeArgs { FirstName = "*a*" })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v!.Collection);
            Assert.AreEqual(2, v.Collection.Count);
            Assert.AreEqual(new string[] { "Browne", "Smithers" }, v.Collection.Select(x => x.LastName).ToArray());
        }

        [Test]
        public void A240_GetByArgs_LastName()
        {
            using var agentTester = ApiTester.Create<Startup>();

            var v = agentTester.Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new EmployeeArgs { LastName = "s*" })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v!.Collection);
            Assert.AreEqual(1, v.Collection.Count);
            Assert.AreEqual(new string[] { "Smithers" }, v.Collection.Select(x => x.LastName).ToArray());
        }

        [Test]
        public void A250_GetByArgs_LastName_IncludeTerminated()
        {
            using var agentTester = ApiTester.Create<Startup>();

            var v = agentTester.Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new EmployeeArgs { LastName = "s*", IsIncludeTerminated = true })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v!.Collection);
            Assert.AreEqual(2, v.Collection.Count);
            Assert.AreEqual(new string[] { "Smith", "Smithers" }, v.Collection.Select(x => x.LastName).ToArray());
        }

        [Test]
        public void A260_GetByArgs_Gender()
        {
            using var agentTester = ApiTester.Create<Startup>();

            var v = agentTester.Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new EmployeeArgs { Genders = new List<string?> { "F" } })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v!.Collection);
            Assert.AreEqual(2, v.Collection.Count);
            Assert.AreEqual(new string[] { "Browne", "Jones" }, v.Collection.Select(x => x.LastName).ToArray());
        }

        [Test]
        public void A270_GetByArgs_Empty()
        {
            using var agentTester = ApiTester.Create<Startup>();

            var v = agentTester.Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new EmployeeArgs { LastName = "s*", FirstName = "b*", Genders = new List<string?> { "F" } })).Value;

            Assert.IsNotNull(v);
            Assert.IsNotNull(v!.Collection);
            Assert.AreEqual(0, v.Collection.Count);
        }

        [Test]
        public void A280_GetByArgs_FieldSelection()
        {
            using var agentTester = ApiTester.Create<Startup>();

            var r = agentTester.Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new EmployeeArgs { Genders = new List<string?> { "F" } }, requestOptions: new HttpRequestOptions().Include("firstname", "lastname")))
                .AssertJson("[{\"firstName\":\"Rachael\",\"lastName\":\"Browne\"},{\"firstName\":\"Wendy\",\"lastName\":\"Jones\"}]");
        }

        [Test]
        public void A290_GetByArgs_RefDataText()
        {
            using var agentTester = ApiTester.Create<Startup>();

            var r = agentTester.Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new EmployeeArgs { Genders = new List<string?> { "F" } }, requestOptions: new HttpRequestOptions { IncludeText = true }));

            Assert.IsNotNull(r.Value);
            Assert.IsNotNull(r.Value!.Collection);
            Assert.AreEqual(2, r.Value.Collection.Count);
            Assert.AreEqual(new string[] { "Browne", "Jones" }, r.Value.Collection.Select(x => x.LastName).ToArray());
            Assert.AreEqual(new string[] { "Female", "Female" }, r.Value.Collection.Select(x => x.GenderText).ToArray());
        }

        [Test]
        public void A300_GetByArgs_ArgsError()
        {
            using var agentTester = ApiTester.Create<Startup>();

            agentTester.Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectErrors("Genders contains one or more invalid items.")
                .Run(a => a.GetByArgsAsync(new EmployeeArgs { Genders = new List<string?> { "Q" } }));
        }

        #endregion

        #region Create

        [Test]
        public void B110_Create()
        {
            using var agentTester = ApiTester.Create<Startup>();

            var v = new Employee
            {
                Email = "j.smith@org.com",
                FirstName = "Jill",
                LastName = "Smith",
                Gender = "F",
                Birthday = new DateTime(1955, 10, 28),
                StartDate = Cleaner.Clean(DateTime.Today, DateTimeTransform.DateOnly),
                PhoneNo = "(456) 789 0123",
                Address = new Address { Street1 = "2732 85 PL NE", City = "Bellevue", State = "WA", PostCode = "98101" },
                EmergencyContacts = new EmergencyContactCollection { new EmergencyContact { FirstName = "Danny", LastName = "Keen", PhoneNo = "(234) 297 9834", Relationship = "FRD" } }
            };

            // Create value.
            v = agentTester.Agent<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .ExpectChangeLogCreated()
                .ExpectETag()
                .ExpectIdentifier()
                .ExpectValue(_ => v)
                .ExpectEvent("my.hr.employee", "created")
                .Run(a => a.CreateAsync(v)).Value!;

            // Check the value was created properly.
            agentTester.Agent<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => v)
                .Run(a => a.GetAsync(v.Id));
        }

        [Test]
        public void B120_Create_Duplicate()
        {
            using var agentTester = ApiTester.Create<Startup>();

            var v = new Employee
            {
                Email = "w.jones@org.com",
                FirstName = "Wendy",
                LastName = "Jones",
                Gender = "F",
                Birthday = new DateTime(1985, 03, 18),
                StartDate = new DateTime(2000, 12, 11),
                PhoneNo = "(425) 612 8113"
            };

            // Create value.
            agentTester.Agent<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.Conflict)
                .Run(a => a.CreateAsync(v));
        }

        #endregion

        #region Update

        [Test]
        public void C110_Update_NotFound()
        {
            using var agentTester = ApiTester.Create<Startup>();

            // Get an existing value.
            var v = agentTester.Agent<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(4.ToGuid())).Value!;

            // Try updating with an invalid identifier.
            agentTester.Agent<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.UpdateAsync(v, 404.ToGuid()));
        }

        [Test]
        public void C120_Update_Concurrency()
        {
            using var agentTester = ApiTester.Create<Startup>();

            // Get an existing value.
            var id = 4.ToGuid();
            var v = agentTester.Agent<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id)).Value!;

            // Try updating the value with an invalid eTag (if-match).
            agentTester.Agent<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .Run(a => a.UpdateAsync(v, id, new HttpRequestOptions { ETag = TestSetUp.Default.ConcurrencyErrorETag }));

            // Try updating the value with an invalid eTag.
            v.ETag = TestSetUp.Default.ConcurrencyErrorETag;
            agentTester.Agent<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .Run(a => a.UpdateAsync(v, id));
        }

        [Test]
        public void C130_Update()
        {
            using var agentTester = ApiTester.Create<Startup>();

            // Get an existing value.
            var id = 4.ToGuid();
            var v = agentTester.Agent<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id)).Value!;

            // Make some changes to the data.
            v.FirstName += "X";
            v.LastName += "Y";
            v.Address!.Street2 = "Street 2";
            v.EmergencyContacts![0].FirstName += "Y";
            v.EmergencyContacts.Add(new EmergencyContact { FirstName = "Danny", LastName = "Keen", PhoneNo = "(234) 297 9834", Relationship = "FRD" });

            // Update the value.
            v = agentTester.Agent<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(v.ETag)
                .ExpectIdentifier()
                .ExpectValue(_ => v)
                .ExpectEvent($"my.hr.employee", "updated")
                .Run(a => a.UpdateAsync(v, id)).Value!;

            // Check the value was updated properly.
            agentTester.Agent<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => v)
                .Run(a => a.GetAsync(id));
        }

        [Test]
        public void C140_Update_AlreadyTerminated()
        {
            using var agentTester = ApiTester.Create<Startup>();

            // Get an existing value.
            var id = 2.ToGuid();
            var v = agentTester.Agent<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id)).Value!;

            // Make some changes to the data.
            v.FirstName += "X";
            v.LastName += "Y";

            // Update the value.
            var r = agentTester.Agent<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectErrorType(ErrorType.ValidationError, "Once an Employee has been Terminated the data can no longer be updated.")
                .Run(a => a.UpdateAsync(v, id));
        }

        #endregion

        #region Patch

        [Test]
        public void D110_Patch_NotFound()
        {
            using var agentTester = ApiTester.Create<Startup>();

            // Get an existing value.
            var v = agentTester.Agent<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(4.ToGuid())).Value!;

            // Try patching with an invalid identifier.
            agentTester.Agent<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.PatchAsync(HttpPatchOption.MergePatch, "{ \"lastName\": \"Smithers\" }", 404.ToGuid()));
        }

        [Test]
        public void D120_Patch_Concurrency()
        {
            using var agentTester = ApiTester.Create<Startup>();

            // Get an existing value.
            var id = 4.ToGuid();
            var v = agentTester.Agent<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id)).Value!;

            // Try updating the value with an invalid eTag (if-match).
            agentTester.Agent<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .Run(a => a.PatchAsync(HttpPatchOption.MergePatch, "{ \"lastName\": \"Smithers\" }", id, new HttpRequestOptions { ETag = TestSetUp.Default.ConcurrencyErrorETag }));

            // Try updating the value with an eTag header (json payload eTag is ignored).
            v.ETag = TestSetUp.Default.ConcurrencyErrorETag;
            agentTester.Agent<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .Run(a => a.PatchAsync(HttpPatchOption.MergePatch, $"{{ \"lastName\": \"Smithers\", \"etag\": \"{TestSetUp.Default.ConcurrencyErrorETag}\" }}", id));
        }

        [Test]
        public void D130_Patch()
        {
            using var agentTester = ApiTester.Create<Startup>();

            // Get an existing value.
            var id = 4.ToGuid();
            var v = agentTester.Agent<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(id)).Value!;

            // Make some changes to the data.
            v.LastName = "Bartholomew";
            v.EmergencyContacts = null;

            // Update the value.
            v = agentTester.Agent<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(v.ETag)
                .ExpectIdentifier()
                .ExpectValue(_ => v)
                .ExpectEvent($"my.hr.employee", "updated")
                .Run(a => a.PatchAsync(HttpPatchOption.MergePatch, $"{{ \"lastName\": \"{v.LastName}\", \"emergencyContacts\": null }}", id, new HttpRequestOptions { ETag = v.ETag })).Value;

            // Check the value was updated properly.
            agentTester.Agent<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => v)
                .Run(a => a.GetAsync(id));
        }

        #endregion

        #region Delete

        [Test]
        public void E110_Delete()
        {
            using var agentTester = ApiTester.Create<Startup>();

            var v = new Employee
            {
                Email = "j.jones@org.com",
                FirstName = "Jarrod",
                LastName = "Jones",
                Gender = "M",
                Birthday = new DateTime(1928, 10, 28),
                StartDate = DateTime.UtcNow.AddDays(1),
                PhoneNo = "(456) 789 0123",
                Address = new Address { Street1 = "2732 85 PL NE", City = "Bellevue", State = "WA", PostCode = "98101" },
                EmergencyContacts = new EmergencyContactCollection { new EmergencyContact { FirstName = "Danny", LastName = "Keen", PhoneNo = "(234) 297 9834", Relationship = "FRD" } }
            };

            // Create an employee in the future.
            v = agentTester.Agent<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .ExpectEvent("my.hr.employee", "created")
                .Run(a => a.CreateAsync(v)).Value!;

            // Delete value.
            agentTester.Agent<EmployeeAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .ExpectEvent($"my.hr.employee", "deleted")
                .Run(a => a.DeleteAsync(v.Id));

            // Check value no longer exists.
            agentTester.Agent<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(v.Id));

            // Delete again (should still be successful as a Delete is idempotent); note there should be no corresponding event as nothing actually happened.
            agentTester.Agent<EmployeeAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.DeleteAsync(v.Id));
        }

        #endregion

        #region Terminate

        [Test]
        public void F110_Terminate_NotFound()
        {
            using var agentTester = ApiTester.Create<Startup>();

            agentTester.Agent<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.TerminateAsync(new TerminationDetail { Date = DateTime.Now, Reason = "RD" }, 404.ToGuid()));
        }

        [Test]
        public void F120_Terminate_MoreThanOnce()
        {
            using var agentTester = ApiTester.Create<Startup>();

            var r = agentTester.Agent<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectErrorType(ErrorType.ValidationError, "An Employee can not be terminated more than once.")
                .Run(a => a.TerminateAsync(new TerminationDetail { Date = DateTime.Now, Reason = "RD" }, 2.ToGuid()));
        }

        [Test]
        public void F130_Terminate_BeforeStart()
        {
            using var agentTester = ApiTester.Create<Startup>();

            var r = agentTester.Agent<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectErrorType(ErrorType.ValidationError, "An Employee can not be terminated prior to their start date.")
                .Run(a => a.TerminateAsync(new TerminationDetail { Date = new DateTime(1999, 12, 31), Reason = "RD" }, 1.ToGuid()));
        }

        [Test]
        public void F140_Terminate()
        {
            using var agentTester = ApiTester.Create<Startup>();

            var v = agentTester.Agent<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid())).Value!;

            v.Termination = new TerminationDetail { Date = Cleaner.Clean(DateTime.Now, DateTimeTransform.DateOnly), Reason = "RD" };

            v = agentTester.Agent<EmployeeAgent, Employee>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(v.ETag)
                .ExpectIdentifier()
                .ExpectValue(_ => v)
                .ExpectEvent($"my.hr.employee", "terminated")
                .Run(a => a.TerminateAsync(v.Termination, 1.ToGuid())).Value!;

            agentTester.Agent<EmployeeAgent, Employee?>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => v)
                .Run(a => a.GetAsync(1.ToGuid()));
        }

        #endregion
    }
}