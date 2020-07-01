#if (implement_database || implement_entityframework)
using Beef.Database.Core;
#endif
#if (implement_cosmos)
using Beef.Data.Cosmos;
#endif
using Beef.Test.NUnit;
#if (implement_cosmos)
using Microsoft.Extensions.Configuration;
using Cosmos = Microsoft.Azure.Cosmos;
#endif
using NUnit.Framework;
#if (implement_database || implement_entityframework)
using System.Reflection;
#endif
using System.Threading.Tasks;
using Company.AppName.Api;
#if (implement_cosmos)
using Company.AppName.Common.Entities;
using Company.AppName.Business.Data;
#endif

namespace Company.AppName.Test
{
    [SetUpFixture]
    public class FixtureSetUp
    {
#if (implement_database || implement_entityframework)
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestSetUp.RegisterSetUp(async (count, _) =>
            {
                return await DatabaseExecutor.RunAsync(
                    count == 0 ? DatabaseExecutorCommand.ResetAndDatabase : DatabaseExecutorCommand.ResetAndData, 
                    AgentTester.Configuration["ConnectionStrings:Database"], useBeefDbo: true,
                    typeof(Database.Program).Assembly, Assembly.GetExecutingAssembly()).ConfigureAwait(false) == 0;
            });

            AgentTester.TestServerStart<Startup>("AppName");
            AgentTester.DefaultExpectNoEvents = true;
        }
#endif
#if (implement_cosmos)
        private bool _removeAfterUse;
        private AppNameCosmosDb? _cosmosDb;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestSetUp.RegisterSetUp(async (count, _) =>
            {
                var config = AgentTester.Configuration.GetSection("CosmosDb");
                _removeAfterUse = config.GetValue<bool>("RemoveAfterUse");
                _cosmosDb = new AppNameCosmosDb(new Cosmos.CosmosClient(config.GetValue<string>("EndPoint"), config.GetValue<string>("AuthKey")),
                    config.GetValue<string>("Database"), createDatabaseIfNotExists: true);

                var rc = await _cosmosDb.ReplaceOrCreateContainerAsync(
                    new Cosmos.ContainerProperties
                    {
                        Id = "Person",
                        PartitionKeyPath = "/_partitionKey"
                    }, 400).ConfigureAwait(false);

                await rc.ImportBatchAsync<PersonTest, Person>("Person.yaml", "Person").ConfigureAwait(false);

                var rdc = await _cosmosDb.ReplaceOrCreateContainerAsync(
                    new Cosmos.ContainerProperties
                    {
                        Id = "RefData",
                        PartitionKeyPath = "/_partitionKey",
                        UniqueKeyPolicy = new Cosmos.UniqueKeyPolicy { UniqueKeys = { new Cosmos.UniqueKey { Paths = { "/type", "/value/code" } } } }
                    }, 400).ConfigureAwait(false);

                await rdc.ImportValueRefDataBatchAsync<PersonTest>(ReferenceData.Current, "RefData.yaml").ConfigureAwait(false);

                return true;
            });

            AgentTester.TestServerStart<Startup>("AppName");
            AgentTester.DefaultExpectNoEvents = true;
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            if (_cosmosDb != null && _removeAfterUse)
                await _cosmosDb.Database.DeleteAsync().ConfigureAwait(false);
        }
#endif
    }
}