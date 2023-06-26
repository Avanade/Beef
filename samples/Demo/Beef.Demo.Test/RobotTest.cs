using Beef.Demo.Api;
using Beef.Demo.Business.Data;
using Beef.Demo.Common.Agents;
using Beef.Demo.Common.Entities;
using CoreEx;
using CoreEx.Cosmos;
using CoreEx.Cosmos.Batch;
using CoreEx.Entities;
using CoreEx.Json.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnitTestEx;
using UnitTestEx.Expectations;
using UnitTestEx.NUnit;
using Cosmos = Microsoft.Azure.Cosmos;

namespace Beef.Demo.Test
{
    [TestFixture, NonParallelizable]
    public class RobotTest : UsingApiTester<Startup>
    {
        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            ApiTester.UseExpectedEvents();
            await CosmosOneTimeSetUp(ApiTester.Services.GetService<DemoCosmosDb>());
        }

        public static async Task CosmosOneTimeSetUp(DemoCosmosDb cosmosDb)
        {
            await cosmosDb.Database.Client.CreateDatabaseIfNotExistsAsync(cosmosDb.Database.Id).ConfigureAwait(false);

            var rc = await cosmosDb.Database.ReplaceOrCreateContainerAsync(
                new Cosmos.ContainerProperties
                {
                    Id = "Items",
                    PartitionKeyPath = "/_partitionKey",
                    UniqueKeyPolicy = new Cosmos.UniqueKeyPolicy { UniqueKeys = { new Cosmos.UniqueKey { Paths = { "/serialNo" } } } }
                }, 400).ConfigureAwait(false);

            var jdr = JsonDataReader.ParseYaml<FixtureSetUp>("Data.yaml");
            await cosmosDb.Items.ImportBatchAsync(jdr, "Robot").ConfigureAwait(false);

            var rdc = await cosmosDb.Database.ReplaceOrCreateContainerAsync(
                new Cosmos.ContainerProperties
                {
                    Id = "RefData",
                    PartitionKeyPath = "/_partitionKey",
                    UniqueKeyPolicy = new Cosmos.UniqueKeyPolicy { UniqueKeys = { new Cosmos.UniqueKey { Paths = { "/type", "/value/code" } } } }
                }, 400).ConfigureAwait(false);

            jdr = JsonDataReader.ParseYaml<FixtureSetUp>("RefData.yaml", new JsonDataReaderArgs(new CoreEx.Text.Json.ReferenceDataContentJsonSerializer()));
            await cosmosDb.ImportValueBatchAsync<Business.Data.Model.PowerSource>("RefData", jdr).ConfigureAwait(false);
        }

        #region Get

        [Test]
        public void B110_Get_NotFound()
        {
            Agent<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .Run(a => a.GetAsync(404.ToGuid()));
        }

        [Test]
        public void B120_Get_Found()
        {
            Agent<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue((t) => new Robot { Id = 1.ToGuid(), ModelNo = "T1000", SerialNo = "123456", PowerSource = "F" })
                .Run(a => a.GetAsync(1.ToGuid()));
        }

        [Test]
        public void B120_Get_Found_WithText()
        {
            Agent<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .IgnoreChangeLog()
                .IgnoreETag()
                .ExpectValue((t) => new Robot { Id = 1.ToGuid(), ModelNo = "T1000", SerialNo = "123456", PowerSource = "F", PowerSourceText = "Fusion" })
                .Run(a => a.GetAsync(1.ToGuid(), new CoreEx.Http.HttpRequestOptions { UrlQueryString = "$text=true" }));
        }

        [Test]
        public void B130_Get_NotModified()
        {
            var v = Agent<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(3.ToGuid())).Value;

            Assert.NotNull(v);

            Agent<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.NotModified)
                .Run(a => a.GetAsync(3.ToGuid(), new CoreEx.Http.HttpRequestOptions { ETag = v.ETag }));
        }

        [Test]
        public void B140_Get_NotModified_Modified()
        {
            Agent<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(3.ToGuid(), new CoreEx.Http.HttpRequestOptions { ETag = "ABCDEFG" }));
        }

        #endregion

        #region GetByArgs

        [Test]
        public void C110_GetByArgs_All_NoPaging()
        {
            var rcr = Agent<RobotAgent, RobotCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new RobotArgs()));

            // Check all 4 are returned in the sorted order.
            Assert.AreEqual(4, rcr?.Value?.Items?.Count);
            Assert.AreEqual(new string[] { "123456", "223456", "A45768", "B45768" }, rcr.Value.Items.Select(x => x.SerialNo).ToArray());
        }

        [Test]
        public void C120_GetByArgs_All_Paging()
        {
            var pcr = Agent<RobotAgent, RobotCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new RobotArgs(), PagingArgs.CreateSkipAndTake(1, 2)));

            // Check only 2 are returned in the sorted order.
            Assert.AreEqual(2, pcr?.Value?.Items?.Count);
            Assert.AreEqual(new string[] { "223456", "A45768", }, pcr.Value.Items.Select(x => x.SerialNo).ToArray());
        }

        [Test]
        public void C130_GetByArgs_Filtered_NoPaging()
        {
            var rcr = Agent<RobotAgent, RobotCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new RobotArgs { ModelNo = "T1000" }));

            // Check only 2 are returned in the sorted order.
            Assert.AreEqual(2, rcr?.Value?.Items?.Count);
            Assert.AreEqual(new string[] { "123456", "223456" }, rcr.Value.Items.Select(x => x.SerialNo).ToArray());
        }

        [Test]
        public void C130_GetByArgs_Wildcard_NoPaging()
        {
            var rcr = Agent<RobotAgent, RobotCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new RobotArgs { SerialNo = "*68" }));

            // Check only 2 are returned in the sorted order.
            Assert.AreEqual(2, rcr?.Value?.Items?.Count);
            Assert.AreEqual(new string[] { "A45768", "B45768" }, rcr.Value.Items.Select(x => x.SerialNo).ToArray());
        }

        [Test]
        public void C140_GetByArgs_PowerSources_NoPaging()
        {
            var rcr = Agent<RobotAgent, RobotCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new RobotArgs { PowerSources = new List<string> { "F", "N" } }));

            // Check only 2 are returned in the sorted order.
            Assert.AreEqual(2, rcr?.Value?.Items?.Count);
            Assert.AreEqual(new string[] { "123456", "223456" }, rcr.Value.Items.Select(x => x.SerialNo).ToArray());
        }

        [Test]
        public void C150_GetByArgs_All_NoResult()
        {
            var rcr = Agent<RobotAgent, RobotCollectionResult>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetByArgsAsync(new RobotArgs { ModelNo = "ABC", SerialNo = "K*", PowerSources = new List<string> { "F", "N" } }));

            // Check nothing is returned..
            Assert.AreEqual(0, rcr?.Value?.Items?.Count);
        }

        #endregion

        #region Create

        [Test]
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
            r = Agent<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.Created)
                .ExpectChangeLogCreated()
                .ExpectETag()
                .ExpectIdentifier()
                .ExpectEvent("Demo.Robot.*", "Create")
                .ExpectValue((t) => r)
                .Run(a => a.CreateAsync(r)).Value;

            // Check the robot was created properly.
            Agent<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectValue((t) => r)
                .Run(a => a.GetAsync(r.Id));
        }

        [Test]
        public void E120_Create_Duplicate()
        {
            var r = new Robot
            {
                ModelNo = "T500",
                SerialNo = "123456",
                EyeColor = "BLUE",
                PowerSource = "N"
            };

            // Try to create a robot which will result in a duplicate.
            Agent<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.Conflict)
                .ExpectNoEvents()
                .Run(a => a.CreateAsync(r));
        }

        #endregion

        #region Update

        [Test]
        public void F110_Update_NotFound()
        {
            // Get an existing Robot.
            var v = Agent<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid())).Value;

            // Update with an invalid identifier.
            Agent<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectNoEvents()
                .Run(a => a.UpdateAsync(v, 404.ToGuid()));
        }

        [Test]
        public void F120_Update_Concurrency()
        {
            // Get an existing Robot.
            var v = Agent<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid())).Value;

            // Try updating the Robot with an invalid eTag.
            v.ETag = TestSetUp.Default.ConcurrencyErrorETag;

            Agent<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.PreconditionFailed)
                .ExpectNoEvents()
                .Run(a => a.UpdateAsync(v, 1.ToGuid()));
        }

        [Test]
        public void F130_Update_Duplicate()
        {
            // Get an existing Robot.
            var v = Agent<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .Run(a => a.GetAsync(1.ToGuid())).Value;

            // Try updating the Robot which will result in a duplicate.
            v.SerialNo = "A45768";

            Agent<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.Conflict)
                .ExpectNoEvents()
                .Run(a => a.UpdateAsync(v, 1.ToGuid()));
        }

        [Test]
        public void F140_Update()
        {
            // Get an existing Robot.
            var v = Agent<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectNoEvents()
                .Run(a => a.GetAsync(1.ToGuid())).Value;

            // Update the Robot with an address.
            v.ModelNo += "X";
            v.SerialNo += "Y";

            v = Agent<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectChangeLogUpdated()
                .ExpectETag(v.ETag)
                .ExpectIdentifier()
                .ExpectEvent("Demo.Robot.*", "Update")
                .ExpectValue((t) => v)
                .Run(a => a.UpdateAsync(v, 1.ToGuid())).Value;

            // Check the Robot was updated properly.
            Agent<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectNoEvents()
                .ExpectValue((t) => v)
                .Run(a => a.GetAsync(v.Id));
        }

        [Test]
        public void F150_Update_Default_Value()
        {
            Agent<RobotAgent, Robot>()
                .Run(a => a.UpdateAsync(null!, 1.ToGuid()))
                .Assert(HttpStatusCode.BadRequest, "Invalid request: content was not provided, contained invalid JSON, or was incorrectly formatted: Value is mandatory.");
        }

        [Test]
        public void F160_Update_Default_Identifier()
        {
            Agent<RobotAgent, Robot>()
                .ExpectErrors(new ApiError("id", "Identifier is required."))
                .Run(a => a.UpdateAsync(new Robot(), Guid.Empty));
        }

        #endregion

        #region Delete

        [Test]
        public void G110_Delete_NotFound()
        {
            // Deleting a Robot that does not exist only reports success.
            Agent<RobotAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .Run(a => a.DeleteAsync(404.ToGuid()));
        }

        [Test]
        public void G120_Delete()
        {
            // Check Robot exists.
            Agent<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.OK)
                .ExpectNoEvents()
                .Run(a => a.GetAsync(1.ToGuid()));

            // Delete a Robot.
            Agent<RobotAgent>()
                .ExpectStatusCode(HttpStatusCode.NoContent)
                .ExpectEvent("Demo.Robot.*", "Delete")
                .Run(a => a.DeleteAsync(1.ToGuid()));

            // Check Robot no longer exists.
            Agent<RobotAgent, Robot>()
                .ExpectStatusCode(HttpStatusCode.NotFound)
                .ExpectNoEvents()
                .Run(a => a.GetAsync(1.ToGuid()));
        }

        #endregion

        #region Other

        [Test]
        public void Z100_RaisePowerSourceChange()
        {
            Agent<RobotAgent>()
                .ExpectStatusCode(HttpStatusCode.Accepted)
                .ExpectEvent($"Demo.Robot.{3.ToGuid()}", "PowerSourceChange")
                .Run(a => a.RaisePowerSourceChangeAsync(3.ToGuid(), "F"));
        }

        #endregion
    }
}