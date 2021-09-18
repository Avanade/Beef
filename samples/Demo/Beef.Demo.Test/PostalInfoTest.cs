using Beef.Demo.Api;
using Beef.Demo.Business.Data;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Test.NUnit;
using Beef.WebApi;
using Moq;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;

namespace Beef.Demo.Test
{
    [TestFixture, NonParallelizable]
    public class PostalInfoTest : UsingAgentTesterServer<Startup>
    {
        [Test, TestSetUp]
        public void B110_GetPostCodes_NotFound()
        {
            AgentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .Run(a => a.GetPostCodesAsync("NZ", "Y", "Z"));
        }

        [Test, TestSetUp]
        public void B120_GetPostCodes_Found()
        {
            var p = AgentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => new PostalInfo { CountrySid = "US", City = "Redmond", State = "WA", Places = new PlaceInfoCollection { new PlaceInfo { Name = "Redmond", PostCode = "98052" }, new PlaceInfo { Name = "Redmond", PostCode = "98053" }, new PlaceInfo { Name = "Redmond", PostCode = "98073" } } })
                .Run(a => a.GetPostCodesAsync("US", "WA", "Redmond"));
        }

        [Test, TestSetUp]
        public void B130_GetPostCodes_Mocked()
        {
            var v = new PostalInfo { CountrySid = "US", City = "Redmond", State = "WA", Places = new PlaceInfoCollection { new PlaceInfo { Name = "Redmond", PostCode = "98052" }, new PlaceInfo { Name = "Redmond", PostCode = "98053" }, new PlaceInfo { Name = "Redmond", PostCode = "98073" } } };

            var mock = new Mock<IZippoAgent>();
            mock.Setup(x => x.SendMappedResponseAsync<PostalInfo, Business.Data.Model.PostalInfo>(It.Is<HttpSendArgs>(x => x.HttpMethod == HttpMethod.Get && x.UrlSuffix == "US/WA/Redmond"))).ReturnsHttpAgentResultAsync(v);

            using var agentTester = Beef.Test.NUnit.AgentTester.CreateWaf<Startup>(sc => sc.ReplaceScoped(mock.Object));

            agentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => v)
                .Run(a => a.GetPostCodesAsync("US", "WA", "Redmond"));
        }
    }
}