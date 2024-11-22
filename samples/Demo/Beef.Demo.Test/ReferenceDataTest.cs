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

namespace Beef.Demo.Test
{
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class ReferenceDataTest : UsingApiTester<Startup>
    {
        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            ApiTester.UseJsonSerializer(new CoreEx.Text.Json.ReferenceDataContentJsonSerializer());
            Assert.That(TestSetUp.Default.SetUp(), Is.True);
            await RobotTest.CosmosOneTimeSetUp(ApiTester.Services.GetService<DemoCosmosDb>()).ConfigureAwait(false);
        }

        [Test, Parallelizable]
        public void A110_GetNamed_AllList()
        {
            var r = Agent<ReferenceDataAgent>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetNamedAsync(new string[] { nameof(Country), nameof(USState), nameof(Gender), nameof(EyeColor), nameof(PowerSource), nameof(Company), nameof(Status) }));

            Assert.Multiple(() =>
            {
                Assert.That(r.GetContent(), Is.Not.Null);
                Assert.That(JObject.Parse("{ \"content\":" + r.GetContent() + "}")["content"].Children().Count(), Is.EqualTo(7));
            });
        }

        [Test, Parallelizable]
        public void A120_GetNamed_AllList_NotModified()
        {
            var r = Agent<ReferenceDataAgent>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetNamedAsync(new string[] { nameof(Gender), nameof(Company) }));

            IEnumerable<string> etags = null;
            Assert.Multiple(() =>
            {
                Assert.That(r.GetContent(), Is.Not.Null);
                Assert.That(r.Response.Headers.TryGetValues("ETag", out etags), Is.True);
                Assert.That(etags.Count(), Is.EqualTo(1));
            });

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
            Assert.Multiple(() =>
            {
                //.RunOverride(() => new ReferenceDataAgent(new DemoWebApiAgentArgs(AgentTester.GetHttpClient(), x =>
                //{
                //    x.Headers.Add("If-None-Match", new string[] { "\"ABC\"", "\"DEF\"" });
                //})).GetNamedAsync(new string[] { nameof(ReferenceData.Gender), nameof(ReferenceData.Company) }));

                Assert.That(r.GetContent(), Is.Not.Null);
                Assert.That(JObject.Parse("{ \"content\":" + r.GetContent() + "}")["content"].Children().Count(), Is.EqualTo(2));
            });
        }

        [Test, Parallelizable]
        public void A140_GetGender()
        {
            var rd = Agent<ReferenceDataAgent, GenderCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GenderGetAllAsync()).Value;

            Assert.That(rd, Is.Not.Null);
            Assert.That(rd, Is.Not.Empty);
        }

        [Test, Parallelizable]
        public void A150_GetGender_NotModified()
        {
            var r = Agent<ReferenceDataAgent, GenderCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GenderGetAllAsync());

            r.Response.Headers.TryGetValues("ETag", out var vals);
            Assert.That(vals, Is.Not.Null);
            Assert.That(vals.Count(), Is.EqualTo(1));

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

            Assert.That(r, Is.Not.Null);
            Assert.That(r.Value, Is.Not.Null);
            Assert.That(r.Value, Has.Count.EqualTo(2));
        }

        [Test, Parallelizable]
        public void A170_GetPowerSource_FilterByText()
        {
            var r = Agent<ReferenceDataAgent, PowerSourceCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.PowerSourceGetAllAsync(new ReferenceDataFilter { Text = "el*" }));

            Assert.That(r, Is.Not.Null);
            Assert.That(r.Value, Is.Not.Null);
            Assert.That(r.Value, Has.Count.EqualTo(1));
        }

        [Test, Parallelizable]
        public void A175_GetPowerSource_FilterByCodes_Inactive()
        {
            var r = Agent<ReferenceDataAgent, PowerSourceCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.PowerSourceGetAllAsync(new ReferenceDataFilter { Codes = new List<string> { "o" } }));

            Assert.That(r, Is.Not.Null);
            Assert.That(r.Value, Is.Not.Null);
            Assert.That(r.Value, Is.Empty);

            r = Agent<ReferenceDataAgent, PowerSourceCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.PowerSourceGetAllAsync(new ReferenceDataFilter { Codes = new List<string> { "o" } }, new CoreEx.Http.HttpRequestOptions { IncludeInactive = true }));

            Assert.That(r, Is.Not.Null);
            Assert.That(r.Value, Is.Not.Null);
            Assert.That(r.Value, Has.Count.EqualTo(1));
        }

        [Test, Parallelizable]
        public void A180_GetByCodes()
        {
            var r = Agent<ReferenceDataAgent>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetNamedAsync(Array.Empty<string>(), new CoreEx.Http.HttpRequestOptions { UrlQueryString = "gender=m,f&powerSource=e&powerSource=f&eyecolor&$include=gender.code,gender.text,powersource.code,powersource.text,eyecolor.code,eyecolor.text" }))
                .AssertJson("{\"gender\":[{\"code\":\"M\",\"text\":\"Male\"},{\"code\":\"F\",\"text\":\"Female\"}],\"powerSource\":[{\"code\":\"E\",\"text\":\"Electrical\"},{\"code\":\"F\",\"text\":\"Fusion\"}],\"eyeColor\":[{\"code\":\"BLUE\",\"text\":\"Blue\"},{\"code\":\"BROWN\",\"text\":\"Brown\"},{\"code\":\"GREEN\",\"text\":\"Green\"}]}");
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