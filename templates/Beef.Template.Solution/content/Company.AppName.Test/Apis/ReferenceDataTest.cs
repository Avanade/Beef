using Company.AppName.Common.Entities;

namespace Company.AppName.Test.Apis;

[TestFixture]
public class ReferenceDataTest : UsingApiTester<Startup>
{
#if (!implement_httpagent)
    [OneTimeSetUp]
    public void OneTimeSetUp() => Assert.That(TestSetUp.Default.SetUp(), Is.True);
#endif
#if (implement_httpagent)
    /* The remainder of the tests for an HttpAgent are commented out and are provided as a guide; MockHttpClientFactory will be needed per test to enable. 
#endif

    [Test]
    public void A110_Genders()
    {
        Agent<ReferenceDataAgent, GenderCollection>()
            .ExpectStatusCode(HttpStatusCode.OK)
            .Run(a => a.GenderGetAllAsync())
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
#if (implement_httpagent)

    */
#endif
}