using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Test.NUnit;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Beef.Demo.Test
{
    [TestFixture, Parallelizable(ParallelScope.Children)]
    public class ReferenceDataTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestSetUp.Reset(false);
        }

        [Test, Parallelizable]
        public void A110_GetNamed_AllList()
        {
            var names = ReferenceData.Current.GetAllTypes().Select(x => x.Name).ToArray();

            var r = AgentTester.Create<ReferenceDataAgent>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetNamedAsync(names));

            Assert.NotNull(r.Content);
            Assert.AreEqual(names.Length, JObject.Parse("{ \"content\":" + r.Content + "}")["content"].Children().Count());
        }

        [Test, Parallelizable]
        public void A120_GetNamed_AllList_NotModified()
        {
            var r = AgentTester.Create<ReferenceDataAgent>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GetNamedAsync(new string[] { ReferenceData.Property_Gender, ReferenceData.Property_Company }));

            Assert.NotNull(r.Content);

            Assert.IsTrue(r.Response.Headers.TryGetValues("ETag", out var etags));
            Assert.AreEqual(1, etags.Count());

            r = AgentTester.Create<ReferenceDataAgent>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .Run((a) => new ReferenceDataAgent(a.Client, (x) =>
                {
                    x.Headers.Add("If-None-Match", etags.First());
                }).GetNamedAsync(new string[] { ReferenceData.Property_Gender, ReferenceData.Property_Company }));
        }

        [Test, Parallelizable]
        public void A130_GetNamed_AllList_NotModified_Modified()
        {
            var r = AgentTester.Create<ReferenceDataAgent>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => new ReferenceDataAgent(a.Client, (x) =>
                {
                    x.Headers.Add("If-None-Match", new string[] { "\"ABC\"", "\"DEF\"" });
                }).GetNamedAsync(new string[] { ReferenceData.Property_Gender, ReferenceData.Property_Company }));

            Assert.NotNull(r.Content);
            Assert.AreEqual(2, JObject.Parse("{ \"content\":" + r.Content + "}")["content"].Children().Count());
        }

        [Test, Parallelizable]
        public void A140_GetGender()
        {
            var rd = AgentTester.Create<ReferenceDataAgent, GenderCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GenderGetAllAsync()).Value;

            Assert.IsNotNull(rd);
            Assert.Greater(rd.AllList.Count, 0);
        }

        [Test, Parallelizable]
        public void A150_GetGender_NotModified()
        {
            var r = AgentTester.Create<ReferenceDataAgent, GenderCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.GenderGetAllAsync());

            r.Response.Headers.TryGetValues("ETag", out var vals);
            Assert.IsNotNull(vals);
            Assert.AreEqual(1, vals.Count());

            AgentTester.Create<ReferenceDataAgent, GenderCollection>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .Run((a) => new ReferenceDataAgent(a.Client, (x) =>
                {
                    x.Headers.IfNoneMatch.Add(new System.Net.Http.Headers.EntityTagHeaderValue(vals.First()));
                }).GenderGetAllAsync());
        }

        [Test, Parallelizable]
        public void A160_GetPowerSource_FilterByCodes()
        {
            var r = AgentTester.Create<ReferenceDataAgent, PowerSourceCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.PowerSourceGetAllAsync(new RefData.ReferenceDataFilter { Codes = new List<string> { "E", "n" } }));

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Value);
            Assert.AreEqual(2, r.Value.Count());
        }

        [Test, Parallelizable]
        public void A170_GetPowerSource_FilterByText()
        {
            var r = AgentTester.Create<ReferenceDataAgent, PowerSourceCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run((a) => a.Agent.PowerSourceGetAllAsync(new RefData.ReferenceDataFilter { Text = "el*" }));

            Assert.IsNotNull(r);
            Assert.IsNotNull(r.Value);
            Assert.AreEqual(1, r.Value.Count());
        }
    }
}