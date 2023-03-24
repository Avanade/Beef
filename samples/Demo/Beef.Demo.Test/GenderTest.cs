using Beef.Demo.Api;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using Beef.Test.NUnit;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnitTestEx.NUnit;
using UnitTestEx.Expectations;

namespace Beef.Demo.Test
{
    [TestFixture, NonParallelizable]
    public class GenderTest : UsingApiTester<Startup>
    {
        private static GenderCollection _genders;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ApiTester.UseJsonSerializer(new CoreEx.Text.Json.ReferenceDataContentJsonSerializer());
            ApiTester.SetUp.ExpectedEventsEnabled = true;
            Assert.IsTrue(ApiTester.SetUp.SetUp());

            _genders = Agent<ReferenceDataAgent, GenderCollection>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GenderGetAllAsync()).Value;
        }

        [Test]
        public void Get_Found()
        {
            var f = _genders.Where(x => x.Code == "F").Single();

            var r = Agent<GenderAgent, Gender>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(f)
                .Run(a => a.GetAsync(f.Id));
        }

        [Test]
        public void Update()
        {
            var f = _genders.Where(x => x.Code == "F").Single();
            f.Code = "W";
            f.Text = "Woman";

            var r = Agent<GenderAgent, Gender>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue(f)
                .ExpectETag()
                .ExpectEvent("Demo.Gender.*")
                .Run(a => a.UpdateAsync(f, f.Id));
        }
    }
}