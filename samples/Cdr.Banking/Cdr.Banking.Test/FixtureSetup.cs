using Beef.Data.Cosmos;
using Beef.Test.NUnit;
using Microsoft.Extensions.Configuration;
using Cosmos = Microsoft.Azure.Cosmos;
using NUnit.Framework;
using System.Threading.Tasks;
using Cdr.Banking.Api;
using Cdr.Banking.Common.Entities;
using Cdr.Banking.Business.Data;
using Beef.WebApi;
using Cdr.Banking.Common.Agents;

namespace Cdr.Banking.Test
{
    [SetUpFixture]
    public class FixtureSetUp
    {
        private bool _removeAfterUse;
        private CosmosDb? _cosmosDb;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestSetUp.SetDefaultLocalReferenceData<IReferenceData, ReferenceDataAgentProvider, IReferenceDataAgent, ReferenceDataAgent>();
            TestSetUp.RegisterSetUp(async (count, _) =>
            {
                // Setup and load cosmos once only.
                if (count == 0)
                {
                    var config = AgentTester.BuildConfiguration<Startup>("Billing").GetSection("CosmosDb");
                    _removeAfterUse = config.GetValue<bool>("RemoveAfterUse");
                    _cosmosDb = new CosmosDb(new Cosmos.CosmosClient(config.GetValue<string>("EndPoint"), config.GetValue<string>("AuthKey")),
                        config.GetValue<string>("Database"), createDatabaseIfNotExists: true);

                    var ac = await _cosmosDb.ReplaceOrCreateContainerAsync(
                        new Cosmos.ContainerProperties
                        {
                            Id = "Account",
                            PartitionKeyPath = "/_partitionKey"
                        }, 400).ConfigureAwait(false);

                    await ac.ImportBatchAsync<AccountTest, Business.Data.Model.Account>("Account.yaml", "Account").ConfigureAwait(false);

                    var tc = await _cosmosDb.ReplaceOrCreateContainerAsync(
                        new Cosmos.ContainerProperties
                        {
                            Id = "Transaction",
                            PartitionKeyPath = "/accountId"
                        }, 400).ConfigureAwait(false);

                    await tc.ImportBatchAsync<AccountTest, Business.Data.Model.Transaction>("Transaction.yaml", "Transaction").ConfigureAwait(false);

                    var rdc = await _cosmosDb.ReplaceOrCreateContainerAsync(
                        new Cosmos.ContainerProperties
                        {
                            Id = "RefData",
                            PartitionKeyPath = "/_partitionKey",
                            UniqueKeyPolicy = new Cosmos.UniqueKeyPolicy { UniqueKeys = { new Cosmos.UniqueKey { Paths = { "/type", "/value/code" } } } }
                        }, 400).ConfigureAwait(false);

                    await rdc.ImportValueRefDataBatchAsync<AccountTest, IReferenceData>("RefData.yaml").ConfigureAwait(false);
                }

                return true;
            });

            TestSetUp.DefaultExpectNoEvents = true;

            // TODO: Passing the username as an http header for all requests; this would be replaced with OAuth integration, etc.
            AgentTester.RegisterBeforeRequest(r => r.Headers.Add("cdr-user", Beef.ExecutionContext.Current.Username));

            // Set "page" and "page-size" as the supported paging query string parameters as defined by the CDR specification.
            WebApiPagingArgsArg.PagingArgsPageQueryStringName = "page";
            WebApiPagingArgsArg.PagingArgsSizeQueryStringName = "page-size";
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            if (_cosmosDb != null && _removeAfterUse)
                await _cosmosDb.Database.DeleteAsync().ConfigureAwait(false);
        }
    }
}