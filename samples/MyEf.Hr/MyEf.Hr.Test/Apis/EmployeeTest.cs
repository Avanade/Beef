﻿using MyEf.Hr.Common.Entities;

namespace MyEf.Hr.Test.Apis;

[TestFixture, NonParallelizable]
public class EmployeeTest : UsingApiTester<Startup>
{
    [OneTimeSetUp]
    public void OneTimeSetUp() => TestSetUp.Default.SetUp();

    #region Get

    [Test]
    public void A110_Get_NotFound()
    {
        Agent<EmployeeAgent, Employee?>()
            .ExpectStatusCode(HttpStatusCode.NotFound)
            .Run(a => a.GetAsync(404.ToGuid()));
    }

    [Test]
    public void A120_Get_Found_NoAddress()
    {
        Agent<EmployeeAgent, Employee?>()
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
        Agent<EmployeeAgent, Employee?>()
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
                EmergencyContacts = [new EmergencyContact { Id = 401.ToGuid(), FirstName = "Michael", LastName = "Manners", PhoneNo = "(234) 297 9834", Relationship = "FRD" }]
            })
            .Run(a => a.GetAsync(4.ToGuid()));
    }

    [Test]
    public void A140_Get_Modified_NotModified()
    {
        var v = Agent<EmployeeAgent, Employee?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetAsync(1.ToGuid(), new HttpRequestOptions { ETag = TestSetUp.Default.ConcurrencyErrorETag })).Value!;

        Assert.That(v, Is.Not.Null);

        Agent<EmployeeAgent, Employee?>()
            .ExpectStatusCode(HttpStatusCode.NotModified)
            .Run(a => a.GetAsync(1.ToGuid(), new HttpRequestOptions { ETag = v.ETag }));
    }

    [Test]
    public void A150_Get_IncludeRefDataText()
    {
        var v = Agent<EmployeeAgent, Employee?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetAsync(1.ToGuid(), new HttpRequestOptions { IncludeText = true})).Value!;

        Assert.That(v, Is.Not.Null);
        Assert.That(v.GenderText, Is.EqualTo("Female"));
    }

    [Test]
    public void A160_Get_IncludeFields()
    {
        Agent<EmployeeAgent>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetAsync(1.ToGuid(), new HttpRequestOptions().Include(["firstName", "lastName"])))
            .AssertJson("{\"firstName\":\"Wendy\",\"lastName\":\"Jones\"}");
    }

    #endregion

    #region GetByArgs

    [Test]
    public void A210_GetByArgs_All()
    {
        var v = Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(null)).Value;

        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v.Items, Is.Not.Null.And.Count.EqualTo(3));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Browne", "Jones", "Smithers" }));
        });
    }

    [Test]
    public void A220_GetByArgs_All_Paging()
    {
        var r = Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(new EmployeeArgs { IsIncludeTerminated = true }, PagingArgs.CreateSkipAndTake(1,2)));

        var v = r.Value;
        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v.Items, Is.Not.Null.And.Count.EqualTo(2));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Jones", "Smith" }));
        });

        // Query again with etag and ensure not modified.
        Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.NotModified)
            .Run(a => a.GetByArgsAsync(new EmployeeArgs { IsIncludeTerminated = true }, PagingArgs.CreateSkipAndTake(1, 2), new HttpRequestOptions { ETag = r.Response!.Headers!.ETag!.Tag }));
    }

    [Test]
    public void A230_GetByArgs_FirstName()
    {
        var v = Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(new EmployeeArgs { FirstName = "*a*" })).Value;

        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v.Items, Is.Not.Null.And.Count.EqualTo(2));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Browne", "Smithers" }));
        });
    }

    [Test]
    public void A240_GetByArgs_LastName()
    {
        var v = Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(new EmployeeArgs { LastName = "s*" })).Value;

        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v.Items, Is.Not.Null.And.Count.EqualTo(1));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Smithers" }));
        });
    }

    [Test]
    public void A250_GetByArgs_LastName_IncludeTerminated()
    {
        var v = Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(new EmployeeArgs { LastName = "s*", IsIncludeTerminated = true })).Value;

        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v.Items, Is.Not.Null.And.Count.EqualTo(2));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Smith", "Smithers" }));
        });
    }

    [Test]
    public void A260_GetByArgs_Gender()
    {
        var v = Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(new EmployeeArgs { Genders = ["F"] })).Value;

        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v.Items, Is.Not.Null.And.Count.EqualTo(2));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Browne", "Jones" }));
        });
    }

    [Test]
    public void A270_GetByArgs_Empty()
    {
        Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(new EmployeeArgs { LastName = "s*", FirstName = "b*", Genders = ["F"] }))
            .AssertJson("[]");
    }

    [Test]
    public void A280_GetByArgs_FieldSelection()
    {
        var r = Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(new EmployeeArgs { Genders = ["F"] }, requestOptions: new HttpRequestOptions().Include("firstname", "lastname")))
            .AssertJson("[{\"firstName\":\"Rachael\",\"lastName\":\"Browne\"},{\"firstName\":\"Wendy\",\"lastName\":\"Jones\"}]");
    }

    [Test]
    public void A290_GetByArgs_RefDataText()
    {
        var r = Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(new EmployeeArgs { Genders = ["F"] }, requestOptions: new HttpRequestOptions { IncludeText = true }));

        Assert.That(r.Value, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(r.Value.Items, Is.Not.Null.And.Count.EqualTo(2));
            Assert.That(r.Value.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Browne", "Jones" }));
            Assert.That(r.Value.Items.Select(x => x.GenderText).ToArray(), Is.EqualTo(new string[] { "Female", "Female" }));
        });
    }

    [Test]
    public void A300_GetByArgs_ArgsError()
    {
        Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.BadRequest)
            .ExpectErrors("Genders contains one or more invalid items.")
            .Run(a => a.GetByArgsAsync(new EmployeeArgs { Genders = ["Z"] }));
    }

    #endregion

    #region GetByQuery

    [Test]
    public void A410_GetByQuery_All()
    {
        var v = Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByQueryAsync()).Value;

        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v.Items, Is.Not.Null.And.Count.EqualTo(3));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Browne", "Jones", "Smithers" }));
        });
    }

    [Test]
    public void A420_GetByQuery_All_Paging()
    {
        var r = Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByQueryAsync(QueryArgs.Create("termination eq null or termination ne null"), PagingArgs.CreateSkipAndTake(1, 2)));

        var v = r.Value;
        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v.Items, Is.Not.Null.And.Count.EqualTo(2));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Jones", "Smith" }));
        });

        // Query again with etag and ensure not modified.
        Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.NotModified)
            .Run(a => a.GetByQueryAsync(QueryArgs.Create("termination eq null or termination ne null"), PagingArgs.CreateSkipAndTake(1, 2), new HttpRequestOptions { ETag = r.Response!.Headers!.ETag!.Tag }));
    }

    [Test]
    public void A430_GetByQuery_FirstName()
    {
        var v = Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByQueryAsync(QueryArgs.Create("contains(firstname, 'a')"))).Value;

        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v.Items, Is.Not.Null.And.Count.EqualTo(2));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Browne", "Smithers" }));
        });
    }

    [Test]
    public void A440_GetByQuery_LastName()
    {
        var v = Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByQueryAsync(QueryArgs.Create("startswith(lastname, 's')"))).Value;

        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v.Items, Is.Not.Null.And.Count.EqualTo(1));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Smithers" }));
        });
    }

    [Test]
    public void A450_GetByQuery_LastName_IncludeTerminated()
    {
        var v = Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByQueryAsync(QueryArgs.Create("startswith(lastname, 's') and (termination eq null or termination ne null)"))).Value;

        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v.Items, Is.Not.Null.And.Count.EqualTo(2));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Smith", "Smithers" }));
        });
    }

    [Test]
    public void A460_GetByQuery_Gender()
    {
        var v = Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByQueryAsync(QueryArgs.Create("gender in ('f')"))).Value;

        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v.Items, Is.Not.Null.And.Count.EqualTo(2));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Browne", "Jones" }));
        });
    }

    [Test]
    public void A470_GetByQuery_Empty()
    {
        Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByQueryAsync(QueryArgs.Create("startswith(lastname, 's') and startswith(firstname, 'b') and gender eq 'f'")))
            .AssertJson("[]");
    }

    [Test]
    public void A480_GetByQuery_FieldSelection()
    {
        var r = Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByQueryAsync(QueryArgs.Create("gender eq 'f'").Include("firstname", "lastname")))
            .AssertJson("[{\"firstName\":\"Rachael\",\"lastName\":\"Browne\"},{\"firstName\":\"Wendy\",\"lastName\":\"Jones\"}]");
    }

    [Test]
    public void A490_GetByQuery_RefDataText()
    {
        var r = Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByQueryAsync(QueryArgs.Create("gender eq 'f'"), requestOptions: new HttpRequestOptions { IncludeText = true }));

        Assert.That(r.Value, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(r.Value.Items, Is.Not.Null.And.Count.EqualTo(2));
            Assert.That(r.Value.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Browne", "Jones" }));
            Assert.That(r.Value.Items.Select(x => x.GenderText).ToArray(), Is.EqualTo(new string[] { "Female", "Female" }));
        });
    }

    [Test]
    public void A500_GetByQuery_ArgsError()
    {
        Agent<EmployeeAgent, EmployeeBaseCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.BadRequest)
            .ExpectErrors("Field 'gender' with value 'z' is invalid: Not a valid Gender.")
            .Run(a => a.GetByQueryAsync(QueryArgs.Create("gender eq 'z'")));
    }

    #endregion

    #region Create

    [Test]
    public void B110_Create()
    {
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
            EmergencyContacts = [new EmergencyContact { FirstName = "Danny", LastName = "Keen", PhoneNo = "(234) 297 9834", Relationship = "FRD" }]
        };

        // Create value.
        v = Agent<EmployeeAgent, Employee>()
            .ExpectStatusCode(HttpStatusCode.Created)
            .ExpectChangeLogCreated()
            .ExpectETag()
            .ExpectIdentifier()
            .ExpectValue(_ => v, "emergencyContacts.id")
            .ExpectEventValue(v, "myef.hr.employee", "created", "value.id", "value.etag", "value.changeLog", "value.emergencyContacts.id")
            .Run(a => a.CreateAsync(v)).Value!;

        // Check the value was created properly.
        Agent<EmployeeAgent, Employee?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .ExpectValue(_ => v)
            .Run(a => a.GetAsync(v.Id));
    }

    [Test]
    public void B120_Create_Duplicate()
    {
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
        Agent<EmployeeAgent, Employee>()
            .ExpectStatusCode(HttpStatusCode.Conflict)
            .Run(a => a.CreateAsync(v));
    }

    #endregion

    #region Update

    [Test]
    public void C110_Update_NotFound()
    {
        // Get an existing value.
        var v = Agent<EmployeeAgent, Employee?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetAsync(4.ToGuid())).Value!;

        // Try updating with an invalid identifier.
        Agent<EmployeeAgent, Employee>()
            .ExpectStatusCode(HttpStatusCode.NotFound)
            .Run(a => a.UpdateAsync(v, 404.ToGuid()));
    }

    [Test]
    public void C120_Update_Concurrency()
    {
        // Get an existing value.
        var id = 4.ToGuid();
        var v = Agent<EmployeeAgent, Employee?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetAsync(id)).Value!;

        // Try updating the value with an invalid eTag (if-match).
        Agent<EmployeeAgent, Employee>()
            .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
            .Run(a => a.UpdateAsync(v, id, new HttpRequestOptions { ETag = TestSetUp.Default.ConcurrencyErrorETag }));

        // Try updating the value with an invalid eTag.
        v.ETag = TestSetUp.Default.ConcurrencyErrorETag;
        Agent<EmployeeAgent, Employee>()
            .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
            .Run(a => a.UpdateAsync(v, id));
    }

    [Test]
    public void C130_Update()
    {
        // Get an existing value.
        var id = 4.ToGuid();
        var v = Agent<EmployeeAgent, Employee?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetAsync(id)).Value!;

        // Make some changes to the data.
        v.FirstName += "X";
        v.LastName += "Y";
        v.Address!.Street2 = "Street 2";
        v.EmergencyContacts![0].FirstName += "Y";
        v.EmergencyContacts.Add(new EmergencyContact { FirstName = "Danny", LastName = "Keen", PhoneNo = "(234) 297 9834", Relationship = "FRD" });

        // Update the value.
        v = Agent<EmployeeAgent, Employee>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .ExpectChangeLogUpdated()
            .ExpectETag(v.ETag)
            .ExpectIdentifier()
            .ExpectValue(_ => v, "emergencyContacts.id")
            .ExpectEvent($"myef.hr.employee", "updated")
            .Run(a => a.UpdateAsync(v, id)).Value!;

        // Check the value was updated properly.
        Agent<EmployeeAgent, Employee?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .ExpectValue(_ => v)
            .Run(a => a.GetAsync(id));
    }

    [Test]
    public void C140_Update_AlreadyTerminated()
    {
        // Get an existing value.
        var id = 2.ToGuid();
        var v = Agent<EmployeeAgent, Employee?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetAsync(id)).Value!;

        // Make some changes to the data.
        v.FirstName += "X";
        v.LastName += "Y";

        // Update the value.
        var r = Agent<EmployeeAgent, Employee>()
            .Run(a => a.UpdateAsync(v, id))
            .Assert(HttpStatusCode.BadRequest, "Once an Employee has been Terminated the data can no longer be updated.");
    }

    #endregion

    #region Patch

    [Test]
    public void D110_Patch_NotFound()
    {
        // Get an existing value.
        var v = Agent<EmployeeAgent, Employee?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetAsync(4.ToGuid())).Value!;

        // Try patching with an invalid identifier.
        Agent<EmployeeAgent, Employee>()
            .ExpectStatusCode(HttpStatusCode.NotFound)
            .Run(a => a.PatchAsync(HttpPatchOption.MergePatch, "{\"lastName\":\"Smithers\"}", 404.ToGuid()));
    }

    [Test]
    public void D120_Patch_Concurrency()
    {
        // Get an existing value.
        var id = 4.ToGuid();
        var v = Agent<EmployeeAgent, Employee?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetAsync(id)).Value!;

        // Try updating the value with an invalid eTag (if-match).
        Agent<EmployeeAgent, Employee>()
            .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
            .Run(a => a.PatchAsync(HttpPatchOption.MergePatch, "{\"lastName\":\"Smithers\"}", id, new HttpRequestOptions { ETag = TestSetUp.Default.ConcurrencyErrorETag }));

        // Try updating the value with an eTag header (json payload eTag is ignored).
        v.ETag = TestSetUp.Default.ConcurrencyErrorETag;
        Agent<EmployeeAgent, Employee>()
            .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
            .Run(a => a.PatchAsync(HttpPatchOption.MergePatch, $"{{\"lastName\":\"Smithers\",\"etag\":\"{TestSetUp.Default.ConcurrencyErrorETag}\"}}", id));
    }

    [Test]
    public void D130_Patch()
    {
        // Get an existing value.
        var id = 4.ToGuid();
        var v = Agent<EmployeeAgent, Employee?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetAsync(id)).Value!;

        // Make some changes to the data.
        v.LastName = "Bartholomew";
        v.EmergencyContacts = null;

        // Update the value.
        v = Agent<EmployeeAgent, Employee>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .ExpectChangeLogUpdated()
            .ExpectETag(v.ETag)
            .ExpectIdentifier()
            .ExpectValue(_ => v)
            .ExpectEvent($"myef.hr.employee", "updated")
            .Run(a => a.PatchAsync(HttpPatchOption.MergePatch, $"{{\"lastName\":\"{v.LastName}\",\"emergencyContacts\":null}}", id, new HttpRequestOptions { ETag = v.ETag })).Value;

        // Check the value was updated properly.
        Agent<EmployeeAgent, Employee?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .ExpectValue(_ => v)
            .Run(a => a.GetAsync(id));
    }

    #endregion

    #region Delete

    [Test]
    public void E110_Delete()
    {
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
            EmergencyContacts = [new EmergencyContact { FirstName = "Danny", LastName = "Keen", PhoneNo = "(234) 297 9834", Relationship = "FRD" }]
        };

        // Create an employee in the future.
        v = Agent<EmployeeAgent, Employee>()
            .ExpectStatusCode(HttpStatusCode.Created)
            .ExpectEvent("myef.hr.employee", "created")
            .Run(a => a.CreateAsync(v)).Value!;

        // Confirm employee exists.
        Agent<EmployeeAgent, Employee?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .ExpectNoEvents()
            .Run(a => a.GetAsync(v.Id));

        // Delete value.
        Agent<EmployeeAgent>()
            .ExpectStatusCode(HttpStatusCode.NoContent)
            .ExpectEvent($"myef.hr.employee", "deleted")
            .Run(a => a.DeleteAsync(v.Id));

        // Check value no longer exists.
        Agent<EmployeeAgent, Employee?>()
            .ExpectStatusCode(HttpStatusCode.NotFound)
            .ExpectNoEvents()
            .Run(a => a.GetAsync(v.Id));

        // Delete again (should still be successful as a Delete is idempotent); note there should be no corresponding event as nothing actually happened.
        Agent<EmployeeAgent>()
            .ExpectStatusCode(HttpStatusCode.NoContent)
            .ExpectNoEvents()
            .Run(a => a.DeleteAsync(v.Id));
    }

    #endregion

    #region Terminate

    [Test]
    public void F110_Terminate_NotFound()
    {
        Agent<EmployeeAgent, Employee>()
            .ExpectStatusCode(HttpStatusCode.NotFound)
            .Run(a => a.TerminateAsync(new TerminationDetail { Date = DateTime.Now, Reason = "RD" }, 404.ToGuid()));
    }

    [Test]
    public void F120_Terminate_MoreThanOnce()
    {
        Agent<EmployeeAgent, Employee>()
            .Run(a => a.TerminateAsync(new TerminationDetail { Date = DateTime.Now, Reason = "RD" }, 2.ToGuid()))
            .Assert(HttpStatusCode.BadRequest, "An Employee can not be terminated more than once.");
    }

    [Test]
    public void F130_Terminate_BeforeStart()
    {
        Agent<EmployeeAgent, Employee>()
            .Run(a => a.TerminateAsync(new TerminationDetail { Date = new DateTime(1999, 12, 31), Reason = "RD" }, 1.ToGuid()))
            .Assert(HttpStatusCode.BadRequest, "An Employee can not be terminated prior to their start date.");
    }

    [Test]
    public void F140_Terminate()
    {
        var v = Agent<EmployeeAgent, Employee?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetAsync(1.ToGuid())).Value!;

        v.Termination = new TerminationDetail { Date = Cleaner.Clean(DateTime.Now, DateTimeTransform.DateOnly), Reason = "RD" };

        v = Agent<EmployeeAgent, Employee>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .ExpectChangeLogUpdated()
            .ExpectETag(v.ETag)
            .ExpectIdentifier()
            .ExpectValue(_ => v)
            .ExpectEvent($"myef.hr.employee", "terminated")
            .Run(a => a.TerminateAsync(v.Termination, 1.ToGuid())).Value!;

        Agent<EmployeeAgent, Employee?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .ExpectNoEvents()
            .ExpectValue(_ => v)
            .Run(a => a.GetAsync(1.ToGuid()));
    }

    #endregion
}