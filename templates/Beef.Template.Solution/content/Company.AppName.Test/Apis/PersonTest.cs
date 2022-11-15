using Company.AppName.Common.Entities;

namespace Company.AppName.Test.Apis;

[TestFixture, NonParallelizable]
public class PersonTest : UsingApiTester<Startup>
{
#if (!implement_httpagent)
    [OneTimeSetUp]
    public void OneTimeSetUp() => Assert.IsTrue(TestSetUp.Default.SetUp());

    #region Get

    [Test]
    public void A110_Get_NotFound()
    {
        Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.NotFound)
            .Run(a => a.GetAsync(404.ToGuid()));
    }

    [Test]
    public void A120_Get_Found()
    {
        Agent<PersonAgent, Person?>()
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

    [Test]
    public void A120_Get_Modified_NotModified()
    {
        var v = Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetAsync(1.ToGuid(), new HttpRequestOptions { ETag = TestSetUp.Default.ConcurrencyErrorETag })).Value!;

        Assert.NotNull(v);

        Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.NotModified)
            .Run(a => a.GetAsync(1.ToGuid(), new HttpRequestOptions { ETag = v.ETag }));
    }

    #endregion

    #region GetByArgs

    [Test]
    public void A210_GetByArgs_All()
    {
        var v = Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(null)).Value!;

        Assert.IsNotNull(v);
        Assert.IsNotNull(v.Items);
        Assert.AreEqual(4, v.Items.Count);
        Assert.AreEqual(new string[] { "Browne", "Jones", "Smith", "Smithers" }, v.Items.Select(x => x.LastName).ToArray());
    }

    [Test]
    public void A220_GetByArgs_Paging()
    {
        var v = Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(null, PagingArgs.CreateSkipAndTake(1, 2))).Value!;

        Assert.IsNotNull(v);
        Assert.IsNotNull(v.Items);
        Assert.AreEqual(2, v.Items.Count);
        Assert.AreEqual(new string[] { "Jones", "Smith" }, v.Items.Select(x => x.LastName).ToArray());
    }

    [Test]
    public void A230_GetByArgs_FirstName()
    {
        var v = Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(new PersonArgs { FirstName = "*a*" })).Value!;

        Assert.IsNotNull(v);
        Assert.IsNotNull(v.Items);
        Assert.AreEqual(3, v.Items.Count);
        Assert.AreEqual(new string[] { "Browne", "Smith", "Smithers" }, v.Items.Select(x => x.LastName).ToArray());
    }

    [Test]
    public void A240_GetByArgs_LastName()
    {
        var v = Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(new PersonArgs { LastName = "s*" })).Value!;

        Assert.IsNotNull(v);
        Assert.IsNotNull(v.Items);
        Assert.AreEqual(2, v.Items.Count);
        Assert.AreEqual(new string[] { "Smith", "Smithers" }, v.Items.Select(x => x.LastName).ToArray());
    }

    [Test]
    public void A250_GetByArgs_Gender()
    {
        var v = Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(new PersonArgs { Genders = new List<string?> { "F" } })).Value!;

        Assert.IsNotNull(v);
        Assert.IsNotNull(v.Items);
        Assert.AreEqual(2, v.Items.Count);
        Assert.AreEqual(new string[] { "Browne", "Jones" }, v.Items.Select(x => x.LastName).ToArray());
    }

    [Test]
    public void A260_GetByArgs_Empty()
    {
        Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(new PersonArgs { LastName = "s*", FirstName = "b*", Genders = new List<string?> { "F" } }))
            .AssertJson("[]");
    }

    [Test]
    public void A270_GetByArgs_FieldSelection()
    {
        Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(new PersonArgs { Genders = new List<string?> { "F" } }, requestOptions: new HttpRequestOptions().Include("firstname", "lastname")))
            .AssertJson("[{\"firstName\":\"Rachael\",\"lastName\":\"Browne\"},{\"firstName\":\"Wendy\",\"lastName\":\"Jones\"}]");
    }

    [Test]
    public void A280_GetByArgs_RefDataText()
    {
        var r = Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(new PersonArgs { Genders = new List<string?> { "F" } }, requestOptions: new HttpRequestOptions { IncludeText = true }))
            .AssertJsonFromResource("A270_GetByArgs_RefDataText-Response.json", "etag", "changeLog");
    }

    #endregion

    #region Create

    [Test]
    public void B110_Create()
    {
        var v = new Person
        {
            FirstName = "Jill",
            LastName = "Smith",
            Gender = "F",
            Birthday = new DateTime(1955, 10, 28)
        };

        // Create value.
        v = Agent<PersonAgent, Person>()
            .ExpectStatusCode(HttpStatusCode.Created)
            .ExpectChangeLogCreated()
            .ExpectETag()
            .ExpectIdentifier()
            .ExpectValue(_ => v)
            .ExpectEvent("Company.AppName.Person".ToLowerInvariant(), "created")
            .Run(a => a.CreateAsync(v)).Value!;

        // Check the value was created properly.
        Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .ExpectValue(_ => v)
            .Run(a => a.GetAsync(v.Id));
    }

    #endregion

    #region Update

    [Test]
    public void C110_Update_NotFound()
    {
        // Get an existing value.
        var v = Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetAsync(2.ToGuid())).Value!;

        // Try updating with an invalid identifier.
        Agent<PersonAgent, Person>()
            .ExpectStatusCode(HttpStatusCode.NotFound)
            .Run(a => a.UpdateAsync(v, 404.ToGuid()));
    }

    [Test]
    public void C120_Update_Concurrency()
    {
        // Get an existing value.
        var id = 2.ToGuid();
        var v = Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetAsync(id)).Value!;

        // Try updating the value with an invalid eTag (if-match).
        Agent<PersonAgent, Person>()
            .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
            .Run(a => a.UpdateAsync(v, id, new HttpRequestOptions { ETag = TestSetUp.Default.ConcurrencyErrorETag }));

        // Try updating the value with an invalid eTag.
        v.ETag = TestSetUp.Default.ConcurrencyErrorETag;
        Agent<PersonAgent, Person>()
            .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
            .Run(a => a.UpdateAsync(v, id));
    }

    [Test]
    public void C130_Update()
    {
        // Get an existing value.
        var id = 2.ToGuid();
        var v = Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetAsync(id)).Value!;

        // Make some changes to the data.
        v.FirstName += "X";
        v.LastName += "Y";

        // Update the value.
        v = Agent<PersonAgent, Person>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .ExpectChangeLogUpdated()
            .ExpectETag(v.ETag)
            .ExpectIdentifier()
            .ExpectValue(_ => v)
            .ExpectEvent($"Company.AppName.Person".ToLowerInvariant(), "updated")
            .Run(a => a.UpdateAsync(v, id)).Value!;

        // Check the value was updated properly.
        Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .ExpectValue(_ => v)
            .Run(a => a.GetAsync(id));
    }

    #endregion

    #region Patch

    [Test]
    public void D110_Patch_NotFound()
    {
        // Get an existing value.
        var v = Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetAsync(2.ToGuid())).Value!;

        // Try patching with an invalid identifier.
        Agent<PersonAgent, Person>()
            .ExpectStatusCode(HttpStatusCode.NotFound)
            .Run(a => a.PatchAsync(HttpPatchOption.MergePatch, "{ \"lastName\": \"Smithers\" }", 404.ToGuid()));
    }

    [Test]
    public void D120_Patch_Concurrency()
    {
        // Get an existing value.
        var id = 2.ToGuid();
        var v = Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetAsync(id)).Value!;

        // Try updating the value with an invalid eTag (if-match).
        Agent<PersonAgent, Person>()
            .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
            .Run(a => a.PatchAsync(HttpPatchOption.MergePatch, "{ \"lastName\": \"Smithers\" }", id, new HttpRequestOptions { ETag = TestSetUp.Default.ConcurrencyErrorETag }));

        // Try updating the value with an eTag header (json payload eTag is ignored).
        Agent<PersonAgent, Person>()
            .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
            .Run(a => a.PatchAsync(HttpPatchOption.MergePatch, $"{{ \"lastName\": \"Smithers\", \"etag\": \"{TestSetUp.Default.ConcurrencyErrorETag}\" }}", id));
    }

    [Test]
    public void D130_Patch()
    {
        // Get an existing value.
        var id = 2.ToGuid();
        var v = Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetAsync(id)).Value!;

        // Make some changes to the data.
        v.LastName = "Smithers";

        // Update the value.
        v = Agent<PersonAgent, Person>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .ExpectChangeLogUpdated()
            .ExpectETag(v.ETag)
            .ExpectIdentifier()
            .ExpectValue(_ => v)
            .ExpectEvent($"Company.AppName.Person".ToLowerInvariant(), "updated")
            .Run(a => a.PatchAsync(HttpPatchOption.MergePatch, $"{{ \"lastName\": \"{v.LastName}\" }}", id, new HttpRequestOptions { ETag = v.ETag })).Value!;

        // Check the value was updated properly.
        Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .ExpectValue(_ => v)
            .Run(a => a.GetAsync(id));
    }

    #endregion

    #region Delete

    [Test]
    public void E110_Delete()
    {
        // Check value exists.
        var id = 4.ToGuid();
        Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetAsync(id));

        // Delete value.
        Agent<PersonAgent>()
            .ExpectStatusCode(HttpStatusCode.NoContent)
            .ExpectEvent($"Company.AppName.Person".ToLowerInvariant(), "deleted")
            .Run(a => a.DeleteAsync(id));

        // Check value no longer exists.
        Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.NotFound)
            .Run(a => a.GetAsync(id));

        // Delete again (should still be successful as a Delete is idempotent); but no event should be raised. 
        Agent<PersonAgent>()
            .ExpectStatusCode(HttpStatusCode.NoContent)
            .Run(a => a.DeleteAsync(id));
    }

    #endregion
}