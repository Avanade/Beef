using My.Hr.Common.Entities;

namespace My.Hr.Test.Apis;

[TestFixture, NonParallelizable]
public class ReferenceDataTest : UsingApiTester<Startup>
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        TestSetUp.Default.SetUp();
        ApiTester.UseJsonSerializer(new CoreEx.Text.Json.ReferenceDataContentJsonSerializer());
    }

    [Test]
    public void A110_GendersAll()
    {
        Agent<ReferenceDataAgent, GenderCollection>()
            .Run(a => a.GenderGetAllAsync())
            .AssertOK()
            .AssertJsonFromResource("RefDataGendersAll_Response.json", "id", "etag");
    }

    [Test]
    public void A120_GendersFilter()
    {
        Agent<ReferenceDataAgent, GenderCollection>()
            .Run(a => a.GenderGetAllAsync(new ReferenceDataFilter { Codes = new string[] { "F" } }))
            .AssertOK()
            .AssertJsonFromResource("RefDataGendersFilter_Response.json", "id", "etag");
    }

    [Test]
    public void A130_GetNamed()
    {
        Agent<ReferenceDataAgent>()
            .Run(a => a.GetNamedAsync(new string[] { "Gender" }))
            .AssertOK()
            .AssertJsonFromResource("RefDataGetNamed_Response.json", "items.id", "items.etag");
    }
}