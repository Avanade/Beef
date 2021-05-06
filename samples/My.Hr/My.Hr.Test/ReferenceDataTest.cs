using Beef.RefData;
using Beef.Test.NUnit;
using My.Hr.Api;
using My.Hr.Common.Agents;
using My.Hr.Common.Entities;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Linq;
using System.Net;

namespace My.Hr.Test
{
    [TestFixture, NonParallelizable]
    public class ReferenceDataTest : UsingAgentTesterServer<Startup>
    {
        [Test, TestSetUp]
        public void A110_GendersAll()
        {
            var v = AgentTester.Test<ReferenceDataAgent, GenderCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GenderGetAllAsync()).Value;

            Assert.IsNotNull(v);
            Assert.AreEqual(3, v.Count);
            Assert.AreEqual(new string[] { "M", "F", "N" }, v.Select(x => x.Code).ToArray());
        }

        [Test, TestSetUp]
        public void A120_GendersFilter()
        {
            var v = AgentTester.Test<ReferenceDataAgent, GenderCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GenderGetAllAsync(new ReferenceDataFilter { Codes = new string[] { "F" } })).Value;

            Assert.IsNotNull(v);
            Assert.AreEqual(1, v.Count);
            Assert.AreEqual(new string[] { "F" }, v.Select(x => x.Code).ToArray());
        }

        [Test, TestSetUp]

        public void A130_GetNamed()
        {
            var r = AgentTester.Test<ReferenceDataAgent>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetNamedAsync(new string[] { "Gender" }));

            Assert.NotNull(r.Content);
            Assert.AreEqual(3, JObject.Parse("{ \"content\":" + r.Content + "}")["content"]?.Children().Children().Count());
        }
    }
}