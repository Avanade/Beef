﻿using Beef.Demo.Api;
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
using UnitTestEx.NUnit;

namespace Beef.Demo.Test
{
    [TestFixture, NonParallelizable]
    public class PostalInfoTest : UsingAgentTesterServer<Startup>
    {
        [Test, TestSetUp(needsSetUp: false)]
        public void B110_GetPostCodes_NotFound()
        {
            AgentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .Run(a => a.GetPostCodesAsync("NZ", "Y", "Z"));
        }

        [Test, TestSetUp(needsSetUp: false)]
        public void B120_GetPostCodes_Found()
        {
            var p = AgentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => new PostalInfo { CountrySid = "US", City = "Redmond", State = "WA", Places = new PlaceInfoCollection { new PlaceInfo { Name = "Redmond", PostCode = "98052" }, new PlaceInfo { Name = "Redmond", PostCode = "98053" }, new PlaceInfo { Name = "Redmond", PostCode = "98073" } } })
                .Run(a => a.GetPostCodesAsync("US", "WA", "Redmond"));
        }

        [Test, TestSetUp(needsSetUp: false)]
        public void B130_GetPostCodes_MockedAgent()
        {
            var v = new PostalInfo { CountrySid = "US", City = "Redmond", State = "WA", Places = new PlaceInfoCollection { new PlaceInfo { Name = "Redmond", PostCode = "98052" }, new PlaceInfo { Name = "Redmond", PostCode = "98053" }, new PlaceInfo { Name = "Redmond", PostCode = "98073" } } };

            // Example of direct mocking...
            var mock = new Mock<IZippoAgent>();
            mock.Setup(x => x.SendMappedResponseAsync<PostalInfo, Business.Data.Model.PostalInfo>(It.Is<HttpSendArgs>(x => x.HttpMethod == HttpMethod.Get && x.UrlSuffix == "US/WA/Redmond"))).ReturnsHttpAgentResultAsync(v);

            using var agentTester = Beef.Test.NUnit.AgentTester.CreateWaf<Startup>(sc => sc.ReplaceScoped(mock.Object));

            agentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => v)
                .Run(a => a.GetPostCodesAsync("US", "WA", "Redmond"));
        }

        [Test, TestSetUp(needsSetUp: false)]
        public void B140_GetPostCodes_MockedHttpClient_NotFound()
        {
            var mcf = MockHttpClientFactory.Create();
            mcf.CreateClient("zippo", new Uri("http://api.zippopotam.us/"))
                .Request(HttpMethod.Get, "US/WA/Bananas").Respond.With(HttpStatusCode.NotFound);

            using var agentTester = Beef.Test.NUnit.AgentTester.CreateWaf<Startup>(sc => mcf.Replace(sc));

            agentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetPostCodesAsync("US", "WA", "Bananas"));
        }

        [Test, TestSetUp(needsSetUp: false)]
        public void B140_GetPostCodes_MockedHttpClient_Found() // The benefit of validating using HttpClient is any deserialization and automapping etc. is performed/exercised.
        {
            var mcf = MockHttpClientFactory.Create();
            mcf.CreateClient("zippo", new Uri("http://api.zippopotam.us/"))
                .Request(HttpMethod.Get, "US/WA/Redmond").Respond.WithJsonResource("B140_GetPostCodes_MockedHttpClient_Found.json");

            using var agentTester = Beef.Test.NUnit.AgentTester.CreateWaf<Startup>(sc => mcf.Replace(sc));

            agentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectJsonResourceValue("B140_GetPostCodes_MockedHttpClient_Found_Response.json")
                .Run(a => a.GetPostCodesAsync("US", "WA", "Redmond"));
        }

        [Test, TestSetUp(needsSetUp: false)]
        public void C110_CreatePostCodes_MockedHttpClient()
        {
            var v = TestSetUp.GetValueFromJsonResource<PostalInfo>("B140_GetPostCodes_MockedHttpClient_Found_Response.json");

            var mcf = MockHttpClientFactory.Create();
            mcf.CreateClient("zippo", new Uri("http://api.zippopotam.us/"))
                .Request(HttpMethod.Post, "US/WA/Bananas").WithJsonResourceBody("C110_CreatePostCodes_MockedHttpClient_Request.json")
                .Respond.WithJsonResource("B140_GetPostCodes_MockedHttpClient_Found.json", HttpStatusCode.Created, r => r.Headers.ETag = new System.Net.Http.Headers.EntityTagHeaderValue("\"MyTestETag\""));

            using var agentTester = Beef.Test.NUnit.AgentTester.CreateWaf<Startup>(sc => mcf.Replace(sc));

            var res = agentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .ExpectJsonResourceValue("B140_GetPostCodes_MockedHttpClient_Found_Response.json")
                .ExpectValue(_ => v)
                .Run(a => a.CreatePostCodesAsync(v, "US", "WA", "Bananas"));

            // Check that the etag flows all the way through.
            Assert.AreEqual("\"MyTestETag\"", res.Response.Headers.ETag.Tag);
        }
    }
}