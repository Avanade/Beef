using MyEf.Hr.Common.Entities;

namespace MyEf.Hr.Test.Apis;

[TestFixture, NonParallelizable]
public class ReferenceDataTest : UsingApiTester<Startup>
{
    [OneTimeSetUp]
    public void OneTimeSetUp() => TestSetUp.Default.SetUp();

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
            .Run(a => a.GenderGetAllAsync(new ReferenceDataFilter { Codes = ["F"] }))
            .AssertOK()
            .AssertJsonFromResource("RefDataGendersFilter_Response.json", "id", "etag");
    }

    [Test]
    public void A130_GetNamed()
    {
        Agent<ReferenceDataAgent>()
            .Run(a => a.GetNamedAsync(["Gender"]))
            .AssertOK()
            .AssertJsonFromResource("RefDataGetNamed_Response.json", "gender.id", "gender.etag");
    }
}