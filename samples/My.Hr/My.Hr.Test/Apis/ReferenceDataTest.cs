using CoreEx.RefData;
using My.Hr.Api;
using My.Hr.Common.Agents;
using My.Hr.Common.Entities;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Linq;
using System.Net;
using UnitTestEx;
using UnitTestEx.Expectations;
using UnitTestEx.NUnit;

namespace My.Hr.Test.Apis
{
    [TestFixture, NonParallelizable]
    public class ReferenceDataTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp() => TestSetUp.Default.SetUp();

        [Test]
        public void A110_GendersAll()
        {
            using var test = ApiTester.Create<Startup>().UseJsonSerializer(new CoreEx.Text.Json.ReferenceDataContentJsonSerializer());

            var v = test.Agent<ReferenceDataAgent, GenderCollection>()
                .Run(a => a.GenderGetAllAsync())
                .AssertOK()
                .AssertJsonFromResource("RefDataGendersAll_Response.json", "id", "etag")
                .Value;

            Assert.IsNotNull(v);
            Assert.AreEqual(3, v!.Count);
            Assert.AreEqual(new string[] { "F", "M", "N" }, v.Select(x => x.Code).ToArray());
        }

        [Test]
        public void A120_GendersFilter()
        {
            using var test = ApiTester.Create<Startup>().UseJsonSerializer(new CoreEx.Text.Json.ReferenceDataContentJsonSerializer());

            var v = test.Agent<ReferenceDataAgent, GenderCollection>()
                .Run(a => a.GenderGetAllAsync(new ReferenceDataFilter { Codes = new string[] { "F" } }))
                .AssertOK()
                .AssertJsonFromResource("RefDataGendersFilter_Response.json", "id", "etag")
                .Value;

            Assert.IsNotNull(v);
            Assert.AreEqual(1, v!.Count);
            Assert.AreEqual(new string[] { "F" }, v.Select(x => x.Code).ToArray());
        }

        [Test]
        public void A130_GetNamed()
        {
            using var test = ApiTester.Create<Startup>().UseJsonSerializer(new CoreEx.Text.Json.ReferenceDataContentJsonSerializer());

            test.Agent<ReferenceDataAgent>()
                .Run(a => a.GetNamedAsync(new string[] { "Gender" }))
                .AssertOK()
                .AssertJsonFromResource("RefDataGetNamed_Response.json", "items.id", "items.etag");
        }
    }
}