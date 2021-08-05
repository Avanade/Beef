using Beef.Demo.Common.Grpc;
using Beef.Demo.Common.Entities;
using Beef.Entities;
using Beef.Test.NUnit;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Beef.Demo.Api;

namespace Beef.Demo.Test
{
    [TestFixture, NonParallelizable]
    public class RobotGrpcTest : UsingAgentTesterServer<Startup>
    {
        private readonly RobotTest _robotTest = new RobotTest();

        [OneTimeSetUp]
        public async Task OneTimeSetUp() { await _robotTest.CosmosOneTimeSetUp(); ConfigureLocalServices(sc => sc.AddAutoMapper(typeof(Transformers.AutoMapperProfile))); }

        [OneTimeTearDown]
        public async Task OneTimeTearDown() => await _robotTest.OneTimeTearDown();

        #region Validation

        [Test, TestSetUp]
        public void A110_Invalid()
        {
            // Done 3 times to monitor performance.
            AgentTester.TestGrpc<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectMessages(
                    "Model No is required.",
                    "Serial No is required.",
                    "Eye Color is invalid.",
                    "Power Source is invalid.")
                .Run(a => a.CreateAsync(new Robot { EyeColor = "XX", PowerSource = "YY" }));

            AgentTester.TestGrpc<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectMessages(
                    "Model No is required.",
                    "Serial No is required.",
                    "Eye Color is invalid.",
                    "Power Source is invalid.")
                .Run(a => a.CreateAsync(new Robot { EyeColor = "XX", PowerSource = "YY" }));

            AgentTester.TestGrpc<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.BadRequest)
                .ExpectMessages(
                    "Model No is required.",
                    "Serial No is required.",
                    "Eye Color is invalid.",
                    "Power Source is invalid.")
                .Run(a => a.CreateAsync(new Robot { EyeColor = "XX", PowerSource = "YY" }));
        }

        #endregion

        #region Get

        [Test, TestSetUp]
        public void B110_Get_NotFound()
        {
            AgentTester.TestGrpc<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .Run(a => a.GetAsync(404.ToGuid()));
        }

        [Test, TestSetUp]
        public void B120_Get_Found()
        {
            AgentTester.TestGrpc<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue((t) => new Robot { Id = 1.ToGuid(), ModelNo = "T1000", SerialNo = "123456", PowerSource = "F" })
                .Run(a => a.GetAsync(1.ToGuid()));
        }

        #endregion

        #region GetByArgs

        [Test, TestSetUp]
        public void C110_GetByArgs_All_NoPaging()
        {
            var rcr = AgentTester.TestGrpc<RobotAgent, RobotCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new RobotArgs()));

            // Check all 4 are returned in the sorted order.
            Assert.AreEqual(4, rcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "123456", "223456", "A45768", "B45768" }, rcr.Value.Result.Select(x => x.SerialNo).ToArray());
        }

        [Test, TestSetUp]
        public void C120_GetByArgs_All_Paging()
        {
            var pcr = AgentTester.TestGrpc<RobotAgent, RobotCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new RobotArgs(), PagingArgs.CreateSkipAndTake(1, 2)));

            // Check only 2 are returned in the sorted order.
            Assert.AreEqual(2, pcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "223456", "A45768", }, pcr.Value.Result.Select(x => x.SerialNo).ToArray());
        }

        [Test, TestSetUp]
        public void C130_GetByArgs_Filtered_NoPaging()
        {
            var rcr = AgentTester.TestGrpc<RobotAgent, RobotCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new RobotArgs { ModelNo = "T1000" }));

            // Check only 2 are returned in the sorted order.
            Assert.AreEqual(2, rcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "123456", "223456" }, rcr.Value.Result.Select(x => x.SerialNo).ToArray());
        }

        [Test, TestSetUp]
        public void C130_GetByArgs_Wildcard_NoPaging()
        {
            var rcr = AgentTester.TestGrpc<RobotAgent, RobotCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new RobotArgs { SerialNo = "*68" }));

            // Check only 2 are returned in the sorted order.
            Assert.AreEqual(2, rcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "A45768", "B45768" }, rcr.Value.Result.Select(x => x.SerialNo).ToArray());
        }

        [Test, TestSetUp]
        public void C140_GetByArgs_PowerSources_NoPaging()
        {
            var rcr = AgentTester.TestGrpc<RobotAgent, RobotCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new RobotArgs { PowerSources = new RefData.ReferenceDataSidList<PowerSource, string> { "F", "N" } }));

            // Check only 2 are returned in the sorted order.
            Assert.AreEqual(2, rcr?.Value?.Result?.Count);
            Assert.AreEqual(new string[] { "123456", "223456" }, rcr.Value.Result.Select(x => x.SerialNo).ToArray());
        }

        [Test, TestSetUp]
        public void C150_GetByArgs_All_NoResult()
        {
            var rcr = AgentTester.TestGrpc<RobotAgent, RobotCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new RobotArgs { ModelNo = "ABC", SerialNo = "K*", PowerSources = new RefData.ReferenceDataSidList<PowerSource, string> { "F", "N" } }));

            // Check nothing is returned..
            Assert.AreEqual(0, rcr?.Value?.Result?.Count);
        }

        #endregion

        #region Create

        [Test, TestSetUp]
        public void E110_Create()
        {
            var r = new Robot
            {
                ModelNo = "T500",
                SerialNo = "321987",
                EyeColor = "BLUE",
                PowerSource = "N"
            };

            // Create a robot.
            r = AgentTester.TestGrpc<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .ExpectChangeLogCreated()
                .ExpectETag()
                .ExpectUniqueKey()
                .ExpectEventWithValue("Demo.Robot.*", "Create")
                .ExpectValue((t) => r)
                .Run(a => a.CreateAsync(r)).Value;

            // Check the robot was created properly.
            AgentTester.TestGrpc<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => r)
                .Run(a => a.GetAsync(r.Id));
        }

        [Test, TestSetUp]
        public void E120_Create_Duplicate()
        {
            AgentTester.PrepareExecutionContext();

            var r = new Robot
            {
                ModelNo = "T500",
                SerialNo = "123456",
                EyeColor = "BLUE",
                PowerSource = "N"
            };

            // Try to create a robot which will result in a duplicate.
            AgentTester.TestGrpc<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.Conflict)
                .ExpectErrorType(ErrorType.DuplicateError)
                .ExpectNoEvents()
                .Run(a => a.CreateAsync(r));
        }

        #endregion

        #region Update

        [Test, TestSetUp]
        public void F110_Update_NotFound()
        {
            // Get an existing Robot.
            var v = AgentTester.TestGrpc<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid())).Value;

            // Update with an invalid identifier.
            AgentTester.TestGrpc<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(ErrorType.NotFoundError)
                .ExpectNoEvents()
                .Run(a => a.UpdateAsync(v, 404.ToGuid()));
        }

        [Test, TestSetUp]
        public void F120_Update_Concurrency()
        {
            // Get an existing Robot.
            var v = AgentTester.TestGrpc<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid())).Value;

            // Try updating the Robot with an invalid eTag.
            v.ETag = TestSetUp.ConcurrencyErrorETag;

            AgentTester.TestGrpc<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .ExpectErrorType(ErrorType.ConcurrencyError)
                .ExpectNoEvents()
                .Run(a => a.UpdateAsync(v, 1.ToGuid()));
        }

        [Test, TestSetUp]
        public void F130_Update_Duplicate()
        {
            // Get an existing Robot.
            var v = AgentTester.TestGrpc<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid())).Value;

            // Try updating the Robot which will result in a duplicate.
            v.SerialNo = "A45768";

            AgentTester.TestGrpc<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.Conflict)
                .ExpectErrorType(ErrorType.DuplicateError)
                .ExpectNoEvents()
                .Run(a => a.UpdateAsync(v, 1.ToGuid()));
        }

        [Test, TestSetUp]
        public void F140_Update()
        {
            // Get an existing Robot.
            var v = AgentTester.TestGrpc<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectNoEvents()
                .Run(a => a.GetAsync(1.ToGuid())).Value;

            // Update the Robot with an address.
            v.ModelNo += "X";
            v.SerialNo += "Y";

            v = AgentTester.TestGrpc<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(v.ETag)
                .ExpectUniqueKey()
                .ExpectEventWithValue("Demo.Robot.*", "Update")
                .ExpectValue((t) => v)
                .Run(a => a.UpdateAsync(v, 1.ToGuid())).Value;

            // Check the Robot was updated properly.
            AgentTester.TestGrpc<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectNoEvents()
                .ExpectValue((t) => v)
                .Run(a => a.GetAsync(v.Id));
        }

        #endregion

        #region Delete

        [Test, TestSetUp]
        public void G110_Delete_NotFound()
        {
            // Deleting a Robot that does not exist only reports success.
            AgentTester.TestGrpc<RobotAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.DeleteAsync(404.ToGuid()));
        }

        [Test, TestSetUp]
        public void G120_Delete()
        {
            // Check Robot exists.
            AgentTester.TestGrpc<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectNoEvents()
                .Run(a => a.GetAsync(1.ToGuid()));

            // Delete a Robot.
            AgentTester.TestGrpc<RobotAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .ExpectEvent("Demo.Robot.*", "Delete")
                .Run(a => a.DeleteAsync(1.ToGuid()));

            // Check Robot no longer exists.
            AgentTester.TestGrpc<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectErrorType(Beef.ErrorType.NotFoundError)
                .ExpectNoEvents()
                .Run(a => a.GetAsync(1.ToGuid()));
        }

        #endregion
    }
}