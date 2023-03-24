using Beef.Demo.Api;
using Beef.Demo.Business.Data;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using CoreEx.RefData;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnitTestEx;
using UnitTestEx.Expectations;
using UnitTestEx.NUnit;

namespace Beef.Demo.Test
{
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class ReferenceDataTest : UsingApiTester<Startup>
    {
        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            ApiTester.UseJsonSerializer(new CoreEx.Text.Json.ReferenceDataContentJsonSerializer());
            Assert.IsTrue(TestSetUp.Default.SetUp());
            await RobotTest.CosmosOneTimeSetUp(ApiTester.Services.GetService<DemoCosmosDb>()).ConfigureAwait(false);
        }

        [Test, Parallelizable]
        public void A110_GetNamed_AllList()
        {
            var r = Agent<ReferenceDataAgent>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetNamedAsync(new string[] { nameof(Country), nameof(USState), nameof(Gender), nameof(EyeColor), nameof(PowerSource), nameof(Company), nameof(Status) }));

            Assert.NotNull(r.GetContent());
            Assert.AreEqual(7, JObject.Parse("{ \"content\":" + r.GetContent() + "}")["content"].Children().Count());
        }

        [Test, Parallelizable]
        public void A120_GetNamed_AllList_NotModified()
        {
            var r = Agent<ReferenceDataAgent>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetNamedAsync(new string[] { nameof(Gender), nameof(Company) }));

            Assert.NotNull(r.GetContent());

            Assert.IsTrue(r.Response.Headers.TryGetValues("ETag", out var etags));
            Assert.AreEqual(1, etags.Count());

            r = Agent<ReferenceDataAgent>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .Run(a => a.GetNamedAsync(new string[] { nameof(Gender), nameof(Company) }, new CoreEx.Http.HttpRequestOptions { ETag = etags.First() }));
                //.RunOverride(() => new ReferenceDataAgent(new DemoWebApiAgentArgs(AgentTester.GetHttpClient(), x =>
                //{
                //    x.Headers.Add("If-None-Match", etags.First());
                //})).GetNamedAsync(new string[] { nameof(ReferenceData.Gender), nameof(ReferenceData.Company) }));
        }

        [Test, Parallelizable]
        public void A130_GetNamed_AllList_Modified()
        {
            var r = Agent<ReferenceDataAgent>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetNamedAsync(new string[] { nameof(Gender), nameof(Company) }, new CoreEx.Http.HttpRequestOptions { ETag = "ABC" }));
                //.RunOverride(() => new ReferenceDataAgent(new DemoWebApiAgentArgs(AgentTester.GetHttpClient(), x =>
                //{
                //    x.Headers.Add("If-None-Match", new string[] { "\"ABC\"", "\"DEF\"" });
                //})).GetNamedAsync(new string[] { nameof(ReferenceData.Gender), nameof(ReferenceData.Company) }));

            Assert.NotNull(r.GetContent());
            Assert.AreEqual(2, JObject.Parse("{ \"content\":" + r.GetContent() + "}")["content"].Children().Count());
        }

        [Test, Parallelizable]
        public void A140_GetGender()
        {
            var rd = Agent<ReferenceDataAgent, GenderCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GenderGetAllAsync()).Value;

            Assert.IsNotNull(rd);
            Assert.Greater(rd.Count, 0);
        }

        [Test, Parallelizable]
        public void A150_GetGender_NotModified()
        {
            var r = Agent<ReferenceDataAgent, GenderCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GenderGetAllAsync());

            r.Response.Headers.TryGetValues("ETag", out var vals);
            Assert.IsNotNull(vals);
            Assert.AreEqual(1, vals.Count());

            Agent<ReferenceDataAgent, GenderCollection>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .Run(a => a.GenderGetAllAsync(null, new CoreEx.Http.HttpRequestOptions { ETag = vals.First() }));
                //.RunOverride(() => new ReferenceDataAgent(new DemoWebApiAgentArgs(AgentTester.GetHttpClient(), x =>
                //{
                //    x.Headers.Add("If-None-Match", vals.First());
                //})).GenderGetAllAsync());

            //Agent<ReferenceDataAgent, GenderCollection>()
            //    .ExpectStatusCode(HttpStatusCode.NotModified)
            //    .Run(a => a.GenderGetAllAsync(null, new Beef.WebApi.WebApiRequestOptions { ETag = vals.First() }));
        }

        [Test, Parallelizable]
        public void A160_GetPowerSource_FilterByCodes()
        {
            var r = Agent<ReferenceDataAgent, PowerSourceCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.PowerSourceGetAllAsync(new ReferenceDataFilter { Codes = new List<string> { "E", null, "n" } }));

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Value);
            Assert.AreEqual(2, r.Value.Count);
        }

        [Test, Parallelizable]
        public void A170_GetPowerSource_FilterByText()
        {
            var r = Agent<ReferenceDataAgent, PowerSourceCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.PowerSourceGetAllAsync(new ReferenceDataFilter { Text = "el*" }));

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Value);
            Assert.AreEqual(1, r.Value.Count);
        }

        [Test, Parallelizable]
        public void A175_GetPowerSource_FilterByCodes_Inactive()
        {
            var r = Agent<ReferenceDataAgent, PowerSourceCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.PowerSourceGetAllAsync(new ReferenceDataFilter { Codes = new List<string> { "o" } }));

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Value);
            Assert.AreEqual(0, r.Value.Count);

            r = Agent<ReferenceDataAgent, PowerSourceCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.PowerSourceGetAllAsync(new ReferenceDataFilter { Codes = new List<string> { "o" } }, new CoreEx.Http.HttpRequestOptions { IncludeInactive = true }));

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Value);
            Assert.AreEqual(1, r.Value.Count);
        }

        [Test, Parallelizable]
        public void A180_GetByCodes()
        {
            var r = Agent<ReferenceDataAgent>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetNamedAsync(Array.Empty<string>(), new CoreEx.Http.HttpRequestOptions { UrlQueryString = "gender=m,f&powerSource=e&powerSource=f&eyecolor&$include=name,items.code" }))
                .AssertJson("[{\"name\":\"Gender\",\"items\":[{\"code\":\"M\"},{\"code\":\"F\"}]},{\"name\":\"PowerSource\",\"items\":[{\"code\":\"E\"},{\"code\":\"F\"}]},{\"name\":\"EyeColor\",\"items\":[{\"code\":\"BLUE\"},{\"code\":\"BROWN\"},{\"code\":\"GREEN\"}]}]");
        }

        [Test, Parallelizable]
        public void A190_Get_NoContent()
        {
            var r = Agent<ReferenceDataAgent>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetNamedAsync(null));
        }
    }
}