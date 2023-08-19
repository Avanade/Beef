using Beef.Demo.Api;
using Beef.Demo.Business.Data;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Test.NUnit;
using CoreEx.Abstractions;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using UnitTestEx.Expectations;
using UnitTestEx.NUnit;

namespace Beef.Demo.Test
{
    [TestFixture, NonParallelizable]
    public class PostalInfoTest : UsingAgentTesterServer<Startup>
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ApiTester.UseExpectedEvents();
        }

        [Test, TestSetUp]
        public void B110_GetPostCodes_NotFound()
        {
            AgentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(ErrorType.NotFoundError)
                .Run(a => a.GetPostCodesAsync("NZ", "Y", "Z"));
        }

        [Test, TestSetUp]
        public void B120_GetPostCodes_Found()
        {
            var p = AgentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(_ => new PostalInfo { Country = "US", City = "Redmond", State = "WA", Places = new PlaceInfoCollection { new PlaceInfo { Name = "Redmond", PostCode = "98052" }, new PlaceInfo { Name = "Redmond", PostCode = "98053" }, new PlaceInfo { Name = "Redmond", PostCode = "98073" } } })
                .Run(a => a.GetPostCodesAsync("US", "WA", "Redmond"));
        }

        [Test, TestSetUp]
        public void B130_GetPostCodes_MockedAgent()
        {
            //var v = new PostalInfo { Country = "US", City = "Redmond", State = "WA", Places = new PlaceInfoCollection { new PlaceInfo { Name = "Redmond", PostCode = "98052" }, new PlaceInfo { Name = "Redmond", PostCode = "98053" }, new PlaceInfo { Name = "Redmond", PostCode = "98073" } } };

            //// Example of direct mocking...
            //var mock = new Mock<ZippoAgent>();
            //mock.Setup(x => x.SendMappedResponseAsync<PostalInfo, Business.Data.Model.PostalInfo>(It.Is<HttpSendArgs>(x => x.HttpMethod == HttpMethod.Get && x.UrlSuffix == "US/WA/Redmond"))).ReturnsHttpAgentResultAsync(v);

            //using var agentTester = Beef.Test.NUnit.AgentTester.CreateWaf<Startup>(sc => sc.ReplaceScoped(mock.Object));

            //agentTester.Test<PostalInfoAgent, PostalInfo>()
            //    .ExpectStatusCode(HttpStatusCode.OK)
            //    .ExpectValue(_ => v)
            //    .Run(a => a.GetPostCodesAsync("US", "WA", "Redmond"));
        }

        [Test, TestSetUp]
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

        [Test, TestSetUp]
        public void B140_GetPostCodes_MockedHttpClient_Found() // The benefit of validating using HttpClient is any deserialization and automapping etc. is performed/exercised.
        {
            var mcf = MockHttpClientFactory.Create();
            mcf.CreateClient("zippo", new Uri("http://api.zippopotam.us/"))
                .Request(HttpMethod.Get, "US/WA/Redmond").Respond.WithJsonResource("B140_GetPostCodes_MockedHttpClient_Found.json");

            using var agentTester = Beef.Test.NUnit.AgentTester.CreateWaf<Startup>(sc => mcf.Replace(sc));

            agentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetPostCodesAsync("US", "WA", "Redmond"))
                .AssertFromJsonResource("B140_GetPostCodes_MockedHttpClient_Found_Response.json");
        }

        [Test, TestSetUp]
        public void C110_CreatePostCodes_MockedHttpClient()
        {
            var v = UnitTestEx.Resource.GetJsonValue<PostalInfo>("B140_GetPostCodes_MockedHttpClient_Found_Response.json");

            var mcf = MockHttpClientFactory.Create();
            mcf.CreateClient("zippo", new Uri("http://api.zippopotam.us/"))
                .Request(HttpMethod.Post, "US/WA/Bananas").WithJsonResourceBody("C110_CreatePostCodes_MockedHttpClient_Request.json")
                .Respond.WithJsonResource("B140_GetPostCodes_MockedHttpClient_Found.json", HttpStatusCode.Created, r => r.Headers.ETag = new System.Net.Http.Headers.EntityTagHeaderValue("\"MyTestETag\""));

            using var agentTester = Beef.Test.NUnit.AgentTester.CreateWaf<Startup>(sc => mcf.Replace(sc));

            var res = agentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .ExpectValue(_ => v)
                .Run(a => a.CreatePostCodesAsync(v, "US", "WA", "Bananas"))
                .AssertFromJsonResource("B140_GetPostCodes_MockedHttpClient_Found_Response.json")
                .AssertETagHeader("\"MyTestETag\"");

            // Check that the etag flows all the way through.
            Assert.AreEqual("\"MyTestETag\"", res.Response.Headers.ETag.Tag);
        }

        [Test, TestSetUp]
        public void D110_UpdatePostCodes_MockHttpClient_ETagError()
        {
            var mcf = MockHttpClientFactory.Create();
            var mc = mcf.CreateClient("zippo", new Uri("http://api.zippopotam.us/"));
            mc.Request(HttpMethod.Get, "US/WA/Redmond").Respond.WithJsonResource("B140_GetPostCodes_MockedHttpClient_Found.json");

            using var agentTester = Beef.Test.NUnit.AgentTester.CreateWaf<Startup>(sc => mcf.Replace(sc));

            var res = agentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetPostCodesAsync("US", "WA", "Redmond"));

            var v = res.Value;
            v.Places[0].PostCode += "0";
            v.Places[1].PostCode += "0";
            v.Places[2].PostCode += "0";

            res = agentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .Run(a => a.UpdatePostCodesAsync(v, "US", "WA", "Redmond"));
        }

        [Test, TestSetUp]
        public void D120_UpdatePostCodes_MockHttpClient_ConcurrencyError()
        {
            var mcf = MockHttpClientFactory.Create();
            var mc = mcf.CreateClient("zippo", new Uri("http://api.zippopotam.us/"));
            mc.Request(HttpMethod.Get, "US/WA/Redmond").Respond.WithJsonResource("B140_GetPostCodes_MockedHttpClient_Found.json");

            using var agentTester = Beef.Test.NUnit.AgentTester.CreateWaf<Startup>(sc => mcf.Replace(sc));

            var res = agentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetPostCodesAsync("US", "WA", "Redmond"));

            var v = res.Value;
            v.Places[0].PostCode += "0";
            v.Places[1].PostCode += "0";
            v.Places[2].PostCode += "0";

            res = agentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .Run(a => a.UpdatePostCodesAsync(v, "US", "WA", "Redmond", new CoreEx.Http.HttpRequestOptions { ETag = "XXX" }));
        }

        [Test, TestSetUp]
        public void D130_UpdatePostCodes_MockHttpClient_NotModifiedSuccess()
        {
            Assert.Warn("Update not modified no longer supported; just returns as updated for overall consistency.");
            //var mcf = MockHttpClientFactory.Create();
            //var mc = mcf.CreateClient("zippo", new Uri("http://api.zippopotam.us/"));
            //mc.Request(HttpMethod.Get, "US/WA/Redmond").Respond.WithJsonResource("B140_GetPostCodes_MockedHttpClient_Found.json");

            //using var agentTester = Beef.Test.NUnit.AgentTester.CreateWaf<Startup>(sc => mcf.Replace(sc));

            //var res = agentTester.Test<PostalInfoAgent, PostalInfo>()
            //    .ExpectStatusCode(HttpStatusCode.OK)
            //    .Run(a => a.GetPostCodesAsync("US", "WA", "Redmond"));

            //var v = res.Value;

            //res = agentTester.Test<PostalInfoAgent, PostalInfo>()
            //    .ExpectStatusCode(HttpStatusCode.OK)
            //    .ExpectValue(v)
            //    .Run(a => a.UpdatePostCodesAsync(v, "US", "WA", "Redmond", new CoreEx.Http.HttpRequestOptions { ETag = res.Response.Headers.ETag.Tag }));
        }

        [Test, TestSetUp]
        public void D140_UpdatePostCodes_MockHttpClient_ModifiedSuccess()
        {
            var mcf = MockHttpClientFactory.Create();
            var mc = mcf.CreateClient("zippo", new Uri("http://api.zippopotam.us/"));
            mc.Request(HttpMethod.Get, "US/WA/Redmond").Respond.WithJsonResource("B140_GetPostCodes_MockedHttpClient_Found.json");
            mc.Request(HttpMethod.Put, "US/WA/Redmond").WithJsonResourceBody("E130_PatchPostCodes_MockHttpClient_Success_Request.json").Respond.WithJsonResource("E130_PatchPostCodes_MockHttpClient_Success_Response.json");

            using var agentTester = Beef.Test.NUnit.AgentTester.CreateWaf<Startup>(sc => mcf.Replace(sc));

            var res = agentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetPostCodesAsync("US", "WA", "Redmond"));

            var v = res.Value;
            v.Places[0].PostCode += "0";
            v.Places[1].PostCode += "0";
            v.Places[2].PostCode += "0";

            res = agentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(v)
                .Run(a => a.UpdatePostCodesAsync(v, "US", "WA", "Redmond", new CoreEx.Http.HttpRequestOptions { ETag = res.Response.Headers.ETag.Tag }));
        }

        [Test, TestSetUp]
        public void E110_PatchPostCodes_MockHttpClient_ETagError()
        {
            var mcf = MockHttpClientFactory.Create();
            var mc = mcf.CreateClient("zippo", new Uri("http://api.zippopotam.us/"));
            mc.Request(HttpMethod.Get, "US/WA/Redmond").Respond.WithJsonResource("B140_GetPostCodes_MockedHttpClient_Found.json");

            using var agentTester = Beef.Test.NUnit.AgentTester.CreateWaf<Startup>(sc => mcf.Replace(sc));

            var res = agentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetPostCodesAsync("US", "WA", "Redmond"));

            res = agentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .Run(a => a.PatchPostCodesAsync(CoreEx.Http.HttpPatchOption.MergePatch, UnitTestEx.Resource.GetString("E110_PatchPostCodes_Request.json"), "US", "WA", "Redmond"));
        }

        [Test, TestSetUp]
        public void E120_PatchPostCodes_MockHttpClient_ConcurrenyError()
        {
            var mcf = MockHttpClientFactory.Create();
            var mc = mcf.CreateClient("zippo", new Uri("http://api.zippopotam.us/"));
            mc.Request(HttpMethod.Get, "US/WA/Redmond").Respond.WithJsonResource("B140_GetPostCodes_MockedHttpClient_Found.json");

            using var agentTester = Beef.Test.NUnit.AgentTester.CreateWaf<Startup>(sc => mcf.Replace(sc));

            var res = agentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetPostCodesAsync("US", "WA", "Redmond"));

            res = agentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .Run(a => a.PatchPostCodesAsync(CoreEx.Http.HttpPatchOption.MergePatch, UnitTestEx.Resource.GetString("E110_PatchPostCodes_Request.json"), "US", "WA", "Redmond", new CoreEx.Http.HttpRequestOptions { ETag = "XXX" }));
        }

        [Test, TestSetUp]
        public void E130_PatchPostCodes_MockHttpClient_Success()
        {
            var mcf = MockHttpClientFactory.Create();
            var mc = mcf.CreateClient("zippo", new Uri("http://api.zippopotam.us/"));
            mc.Request(HttpMethod.Get, "US/WA/Redmond").Respond.WithJsonResource("B140_GetPostCodes_MockedHttpClient_Found.json");
            mc.Request(HttpMethod.Put, "US/WA/Redmond").WithJsonResourceBody("E130_PatchPostCodes_MockHttpClient_Success_Request.json").Respond.WithJsonResource("E130_PatchPostCodes_MockHttpClient_Success_Response.json");

            using var agentTester = Beef.Test.NUnit.AgentTester.CreateWaf<Startup>(sc => mcf.Replace(sc));

            var res = agentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetPostCodesAsync("US", "WA", "Redmond"));

            res = agentTester.Test<PostalInfoAgent, PostalInfo>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.PatchPostCodesAsync(CoreEx.Http.HttpPatchOption.MergePatch, UnitTestEx.Resource.GetString("E110_PatchPostCodes_Request.json"), "US", "WA", "Redmond", new CoreEx.Http.HttpRequestOptions { ETag = res.Response.Headers.ETag.Tag }));
        }
    }
}