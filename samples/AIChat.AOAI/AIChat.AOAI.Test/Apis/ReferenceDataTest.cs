using AIChat.AOAI.Common.Entities;

namespace AIChat.AOAI.Test.Apis;

[TestFixture]
public class ReferenceDataTest : UsingApiTester<Startup>
{
    [OneTimeSetUp]
    public void OneTimeSetUp() => Assert.That(TestSetUp.Default.SetUp(), Is.True);

    [Test]
    public void A110_Roles()
    {
        Agent<ReferenceDataAgent, RoleCollection>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.RoleGetAllAsync())
            .AssertJsonFromResource("ReferenceData_A110_Genders_Response.json", "id", "etag");
    }

    [Test]
    public void B110_GetNamed()
    {
        Agent<ReferenceDataAgent>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GetNamedAsync(["gender"]))
            .AssertJsonFromResource("ReferenceData_B110_GetNamed_Response.json", "gender.id", "gender.etag");
    }
}