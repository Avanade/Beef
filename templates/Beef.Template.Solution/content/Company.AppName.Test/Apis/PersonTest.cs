﻿using Company.AppName.Common.Entities;

namespace Company.AppName.Test.Apis;

[TestFixture, NonParallelizable]
public class PersonTest : UsingApiTester<Startup>
{
#if (!implement_httpagent)
    [OneTimeSetUp]
    public void OneTimeSetUp() => Assert.That(TestSetUp.Default.SetUp(), Is.True);

#endif
    #region Get

    [Test]
    public void A110_Get_NotFound()
    {
#if (implement_httpagent)
        var mcf = MockHttpClientFactory.Create();
        mcf.CreateClient("Xxx", "https://backend/").Request(HttpMethod.Get, $"People/{404.ToGuid()}").Respond.With(HttpStatusCode.NotFound);

        ApiTester.ResetHost().ReplaceHttpClientFactory(mcf);

#endif
        Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.NotFound)
#if (!implement_mysql && !implement_postgres)
            .Run(a => a.GetAsync(404.ToGuid()));
#endif
#if (implement_mysql || implement_postgres)
            .Run(a => a.GetAsync(404));
#endif
    }

    [Test]
    public void A120_Get_Found()
    {
#if (implement_httpagent)
        var dmp = new Business.Data.Model.Person { Id = 1.ToGuid(), FirstName = "Wendy", LastName = "Jones", Gender = "F", Birthday = new DateTime(1985, 03, 18, 0, 0, 0, DateTimeKind.Unspecified) };

        var mcf = MockHttpClientFactory.Create();
        mcf.CreateClient("Xxx", "https://backend/").Request(HttpMethod.Get, $"People/{1.ToGuid()}").Respond.WithJson(dmp);

        ApiTester.ResetHost().ReplaceHttpClientFactory(mcf);

#endif
        Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .IgnoreChangeLog()
            .IgnoreETag()
            .ExpectValue(_ => new Person
            {
#if (!implement_mysql && !implement_postgres)
                Id = 1.ToGuid(),
#endif
#if (implement_mysql || implement_postgres)
                Id = 1,
#endif
                FirstName = "Wendy",
                LastName = "Jones",
                Gender = "F",
                Birthday = new DateTime(1985, 03, 18)
            })
#if (!implement_mysql && !implement_postgres)
            .Run(a => a.GetAsync(1.ToGuid()));
#endif
#if (implement_mysql || implement_postgres)
            .Run(a => a.GetAsync(1));
#endif
    }

#if (implement_httpagent)
    /* The remainder of the tests for an HttpAgent are commented out and are provided as a guide; MockHttpClientFactory will be needed per test to enable. 
#endif

    [Test]
    public void A120_Get_Modified_NotModified()
    {
        var v = Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.OK)
#if (!implement_mysql && !implement_postgres)
            .Run(a => a.GetAsync(1.ToGuid(), new HttpRequestOptions { ETag = TestSetUp.Default.ConcurrencyErrorETag })).Value!;
#endif
#if (implement_mysql || implement_postgres)
            .Run(a => a.GetAsync(1, new HttpRequestOptions { ETag = TestSetUp.Default.ConcurrencyErrorETag })).Value!;
#endif

        Assert.That(v, Is.Not.Null);

        Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.NotModified)
#if (!implement_mysql && !implement_postgres)
            .Run(a => a.GetAsync(1.ToGuid(), new HttpRequestOptions { ETag = v.ETag }));
#endif
#if (implement_mysql || implement_postgres)
            .Run(a => a.GetAsync(1, new HttpRequestOptions { ETag = v.ETag }));
#endif
    }

    #endregion

    #region GetByArgs

    [Test]
    public void A210_GetByArgs_All()
    {
        var v = Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(null)).Value!;

        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v!.Items, Is.Not.Null.And.Count.EqualTo(4));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Browne", "Jones", "Smith", "Smithers" }));
        });
    }

    [Test]
    public void A220_GetByArgs_Paging()
    {
        var v = Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(null, PagingArgs.CreateSkipAndTake(1, 2))).Value!;

        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v!.Items, Is.Not.Null.And.Count.EqualTo(2));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Jones", "Smith" }));
        });
    }

    [Test]
    public void A230_GetByArgs_FirstName()
    {
        var v = Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(new PersonArgs { FirstName = "*a*" })).Value!;

        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v!.Items, Is.Not.Null.And.Count.EqualTo(3));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Browne", "Smith", "Smithers" }));
        });
    }

    [Test]
    public void A240_GetByArgs_LastName()
    {
        var v = Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(new PersonArgs { LastName = "s*" })).Value!;

        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v!.Items, Is.Not.Null.And.Count.EqualTo(2));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Smith", "Smithers" }));
        });
    }

    [Test]
    public void A250_GetByArgs_Gender()
    {
        var v = Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(new PersonArgs { Genders = ["F"] })).Value!;

        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v!.Items, Is.Not.Null.And.Count.EqualTo(2));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Browne", "Jones" }));
        });
    }

    [Test]
    public void A260_GetByArgs_Empty()
    {
        Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(new PersonArgs { LastName = "s*", FirstName = "b*", Genders = ["F"] }))
            .AssertJson("[]");
    }

    [Test]
    public void A270_GetByArgs_FieldSelection()
    {
        Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(new PersonArgs { Genders = ["F"] }, requestOptions: new HttpRequestOptions().Include("firstname", "lastname")))
            .AssertJson("[{\"firstName\":\"Rachael\",\"lastName\":\"Browne\"},{\"firstName\":\"Wendy\",\"lastName\":\"Jones\"}]");
    }

    [Test]
    public void A280_GetByArgs_RefDataText()
    {
        var r = Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByArgsAsync(new PersonArgs { Genders = ["F"] }, requestOptions: new HttpRequestOptions { IncludeText = true }))
            .AssertJsonFromResource("Person_A280_GetByArgs_Response.json", "etag", "changeLog");
    }

    #endregion

#if (implement_entityframework | implement_cosmos)
    #region GetByQuery

    [Test]
    public void A310_GetByQuery_All()
    {
        var v = Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByQueryAsync(null)).Value!;

        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v!.Items, Is.Not.Null.And.Count.EqualTo(4));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Browne", "Jones", "Smith", "Smithers" }));
        });
    }

    [Test]
    public void A320_GetByQuery_Paging()
    {
        var v = Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByQueryAsync(null, PagingArgs.CreateSkipAndTake(1, 2))).Value!;

        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v!.Items, Is.Not.Null.And.Count.EqualTo(2));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Jones", "Smith" }));
        });
    }

    [Test]
    public void A330_GetByQuery_FirstName()
    {
        var v = Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByQueryAsync(QueryArgs.Create("contains(firstname, 'a')"))).Value!;

        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v!.Items, Is.Not.Null.And.Count.EqualTo(3));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Browne", "Smith", "Smithers" }));
        });
    }

    [Test]
    public void A340_GetByQuery_LastName()
    {
        var v = Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByQueryAsync(QueryArgs.Create("startswith(lastname, 's')"))).Value!;

        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v!.Items, Is.Not.Null.And.Count.EqualTo(2));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Smith", "Smithers" }));
        });
    }

    [Test]
    public void A350_GetByQuery_Gender()
    {
        var v = Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByQueryAsync(QueryArgs.Create("gender eq 'f'"))).Value!;

        Assert.That(v, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(v!.Items, Is.Not.Null.And.Count.EqualTo(2));
            Assert.That(v.Items.Select(x => x.LastName).ToArray(), Is.EqualTo(new string[] { "Browne", "Jones" }));
        });
    }

    [Test]
    public void A360_GetByQuery_Empty()
    {
        Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByQueryAsync(QueryArgs.Create("startswith(lastname, 's') and startswith(firstname, 'b') and gender eq 'f'")))
            .AssertJson("[]");
    }

    [Test]
    public void A370_GetByQuery_FieldSelection()
    {
        Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByQueryAsync(QueryArgs.Create("gender eq 'f'").Include("firstname", "lastname")))
            .AssertJson("[{\"firstName\":\"Rachael\",\"lastName\":\"Browne\"},{\"firstName\":\"Wendy\",\"lastName\":\"Jones\"}]");
    }

    [Test]
    public void A880_GetByQuery_RefDataText()
    {
        var r = Agent<PersonAgent, PersonCollectionResult>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetByQueryAsync(QueryArgs.Create("gender eq 'f'"), requestOptions: new HttpRequestOptions { IncludeText = true }))
            .AssertJsonFromResource("Person_A280_GetByArgs_Response.json", "etag", "changeLog");
    }

    #endregion

#endif
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
            .ExpectEvent("lowercom.lowerapp.person", "created")
            .Run(a => a.CreateAsync(v)).Value!;

        // Check the value was created properly.
        Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.OK)
#if (implement_mysql || implement_postgres)
            .ExpectValue(_ => v, "changeLog")
#else
            .ExpectValue(_ => v)
#endif
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
#if (!implement_mysql && !implement_postgres)
            .Run(a => a.GetAsync(2.ToGuid())).Value!;
#endif
#if (implement_mysql || implement_postgres)
            .Run(a => a.GetAsync(2)).Value!;
#endif

        // Try updating with an invalid identifier.
        Agent<PersonAgent, Person>()
            .ExpectStatusCode(HttpStatusCode.NotFound)
#if (!implement_mysql && !implement_postgres)
            .Run(a => a.UpdateAsync(v, 404.ToGuid()));
#endif
#if (implement_mysql || implement_postgres)
            .Run(a => a.UpdateAsync(v, 404));
#endif
    }

    [Test]
    public void C120_Update_Concurrency()
    {
        // Get an existing value.
#if (!implement_mysql && !implement_postgres)
        var id = 2.ToGuid();
#endif
#if (implement_mysql || implement_postgres)
        var id = 2;
#endif
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
#if (!implement_mysql && !implement_postgres)
        var id = 2.ToGuid();
#endif
#if (implement_mysql || implement_postgres)
        var id = 2;
#endif
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
            .ExpectEvent("lowercom.lowerapp.person", "updated")
            .Run(a => a.UpdateAsync(v, id)).Value!;

        // Check the value was updated properly.
        Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.OK)
#if (implement_mysql || implement_postgres)
            .ExpectValue(_ => v, "changeLog")
#else
            .ExpectValue(_ => v)
#endif
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
#if (!implement_mysql && !implement_postgres)
            .Run(a => a.GetAsync(2.ToGuid())).Value!;
#endif
#if (implement_mysql || implement_postgres)
            .Run(a => a.GetAsync(2)).Value!;
#endif

        // Try patching with an invalid identifier.
        Agent<PersonAgent, Person>()
            .ExpectStatusCode(HttpStatusCode.NotFound)
#if (!implement_mysql && !implement_postgres)
            .Run(a => a.PatchAsync(HttpPatchOption.MergePatch, "{ \"lastName\": \"Smithers\" }", 404.ToGuid()));
#endif
#if (implement_mysql || implement_postgres)
            .Run(a => a.PatchAsync(HttpPatchOption.MergePatch, "{ \"lastName\": \"Smithers\" }", 404));
#endif
    }

    [Test]
    public void D120_Patch_Concurrency()
    {
        // Get an existing value.
#if (!implement_mysql && !implement_postgres)
        var id = 2.ToGuid();
#endif
#if (implement_mysql || implement_postgres)
        var id = 2;
#endif
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
#if (!implement_mysql && !implement_postgres)
        var id = 2.ToGuid();
#endif
#if (implement_mysql || implement_postgres)
        var id = 2;
#endif
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
            .ExpectEvent("lowercom.lowerapp.person", "updated")
            .Run(a => a.PatchAsync(HttpPatchOption.MergePatch, $"{{ \"lastName\": \"{v.LastName}\" }}", id, new HttpRequestOptions { ETag = v.ETag })).Value!;

        // Check the value was updated properly.
        Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.OK)
#if (implement_mysql || implement_postgres)
            .ExpectValue(_ => v, "changeLog")
#else
            .ExpectValue(_ => v)
#endif
            .Run(a => a.GetAsync(id));
    }

#endregion

    #region Delete

    [Test]
    public void E110_Delete()
    {
        // Check value exists.
#if (!implement_mysql && !implement_postgres)
        var id = 4.ToGuid();
#endif
#if (implement_mysql || implement_postgres)
        var id = 4;
#endif
        Agent<PersonAgent, Person?>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetAsync(id));

        // Delete value.
        Agent<PersonAgent>()
            .ExpectStatusCode(HttpStatusCode.NoContent)
            .ExpectEvent("lowercom.lowerapp.person", "deleted")
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

#if (implement_httpagent)
    */
#endif

    #endregion
}