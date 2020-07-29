using Beef.Demo.Api;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Test.NUnit;
using Beef.WebApi;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Beef.Demo.Test
{
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class ReferenceDataTest : UsingAgentTesterServer<Startup>
    {
        private static RobotTest _robotTest = new RobotTest();

        [OneTimeSetUp]
        public async Task OneTimeSetUp() => await _robotTest.CosmosOneTimeSetUp();

        [OneTimeTearDown]
        public async Task OneTimeTearDown() => await _robotTest.OneTimeTearDown();

        [Test, TestSetUp, Parallelizable]
        public void A110_GetNamed_AllList()
        {
            var r = AgentTester.Test<ReferenceDataAgent>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetNamedAsync(new string[] { nameof(ReferenceData.Country), nameof(ReferenceData.USState), nameof(ReferenceData.Gender), nameof(ReferenceData.EyeColor), nameof(ReferenceData.PowerSource), nameof(ReferenceData.Company) }));

            Assert.NotNull(r.Content);
            Assert.AreEqual(6, JObject.Parse("{ \"content\":" + r.Content + "}")["content"].Children().Count());
        }

        [Test, TestSetUp, Parallelizable]
        public void A120_GetNamed_AllList_NotModified()
        {
            var r = AgentTester.Test<ReferenceDataAgent>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetNamedAsync(new string[] { nameof(ReferenceData.Gender), nameof(ReferenceData.Company) }));

            Assert.NotNull(r.Content);

            Assert.IsTrue(r.Response.Headers.TryGetValues("ETag", out var etags));
            Assert.AreEqual(1, etags.Count());

            r = AgentTester.Test<ReferenceDataAgent>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .RunOverride(() => new ReferenceDataAgent(new WebApiAgentArgs(AgentTester.GetHttpClient(), x =>
                {
                    x.Headers.Add("If-None-Match", etags.First());
                })).GetNamedAsync(new string[] { nameof(ReferenceData.Gender), nameof(ReferenceData.Company) }));
        }

        [Test, TestSetUp, Parallelizable]
        public void A130_GetNamed_AllList_NotModified_Modified()
        {
            var r = AgentTester.Test<ReferenceDataAgent>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .RunOverride(() => new ReferenceDataAgent(new WebApiAgentArgs(AgentTester.GetHttpClient(), x =>
                {
                    x.Headers.Add("If-None-Match", new string[] { "\"ABC\"", "\"DEF\"" });
                })).GetNamedAsync(new string[] { nameof(ReferenceData.Gender), nameof(ReferenceData.Company) }));

            Assert.NotNull(r.Content);
            Assert.AreEqual(2, JObject.Parse("{ \"content\":" + r.Content + "}")["content"].Children().Count());
        }

        [Test, TestSetUp, Parallelizable]
        public void A140_GetGender()
        {
            var rd = AgentTester.Test<ReferenceDataAgent, GenderCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GenderGetAllAsync()).Value;

            Assert.IsNotNull(rd);
            Assert.Greater(rd.AllList.Count, 0);
        }

        [Test, TestSetUp, Parallelizable]
        public void A150_GetGender_NotModified()
        {
            var r = AgentTester.Test<ReferenceDataAgent, GenderCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GenderGetAllAsync());

            r.Response.Headers.TryGetValues("ETag", out var vals);
            Assert.IsNotNull(vals);
            Assert.AreEqual(1, vals.Count());

            AgentTester.Test<ReferenceDataAgent, GenderCollection>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .RunOverride(() => new ReferenceDataAgent(new WebApiAgentArgs(AgentTester.GetHttpClient(), x =>
                {
                    x.Headers.Add("If-None-Match", vals.First());
                })).GenderGetAllAsync());

            AgentTester.Test<ReferenceDataAgent, GenderCollection>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .Run(a => a.GenderGetAllAsync(null, new Beef.WebApi.WebApiRequestOptions { ETag = vals.First() }));
        }

        [Test, TestSetUp, Parallelizable]
        public void A160_GetPowerSource_FilterByCodes()
        {
            var r = AgentTester.Test<ReferenceDataAgent, PowerSourceCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.PowerSourceGetAllAsync(new RefData.ReferenceDataFilter { Codes = new List<string> { "E", null, "n" } }));

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Value);
            Assert.AreEqual(2, r.Value.Count());
        }

        [Test, TestSetUp, Parallelizable]
        public void A170_GetPowerSource_FilterByText()
        {
            var r = AgentTester.Test<ReferenceDataAgent, PowerSourceCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.PowerSourceGetAllAsync(new RefData.ReferenceDataFilter { Text = "el*" }));

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Value);
            Assert.AreEqual(1, r.Value.Count());
        }

        [Test, TestSetUp, Parallelizable]
        public void A175_GetPowerSource_FilterByCodes_Inactive()
        {
            var r = AgentTester.Test<ReferenceDataAgent, PowerSourceCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.PowerSourceGetAllAsync(new RefData.ReferenceDataFilter { Codes = new List<string> { "o" } }));

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Value);
            Assert.AreEqual(0, r.Value.Count());

            r = AgentTester.Test<ReferenceDataAgent, PowerSourceCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.PowerSourceGetAllAsync(new RefData.ReferenceDataFilter { Codes = new List<string> { "o" } }, new Beef.WebApi.WebApiRequestOptions { UrlQueryString = "$inactive=true" }));

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Value);
            Assert.AreEqual(1, r.Value.Count());
        }

        [Test, TestSetUp, Parallelizable]
        public void A180_GetByCodes()
        {
            var r = AgentTester.Test<ReferenceDataAgent>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetNamedAsync(Array.Empty<string>(), new Beef.WebApi.WebApiRequestOptions { UrlQueryString = "gender=m,f&powerSource=e&powerSource=f&eyecolor&$include=name,items.code" }));

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Content);
            Assert.AreEqual("[{\"name\":\"Gender\",\"items\":[{\"code\":\"M\"},{\"code\":\"F\"}]},{\"name\":\"PowerSource\",\"items\":[{\"code\":\"E\"},{\"code\":\"F\"}]},{\"name\":\"EyeColor\",\"items\":[{\"code\":\"BLUE\"},{\"code\":\"BROWN\"},{\"code\":\"GREEN\"}]}]", r.Content);
        }

        [Test, TestSetUp, Parallelizable]
        public void A190_Get_NoContent()
        {
            var r = AgentTester.Test<ReferenceDataAgent>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetNamedAsync(null));
        }
    }
}