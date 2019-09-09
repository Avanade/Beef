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
#if (implement_cosmos)
using System.Threading.Tasks;
#endif
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
            TestSetUp.RegisterSetUp((count, data) =>
            {
                return DatabaseExecutor.Run(
                    count == 0 ? DatabaseExecutorCommand.ResetAndDatabase : DatabaseExecutorCommand.ResetAndData, 
                    AgentTester.Configuration["ConnectionStrings:Database"],
                    typeof(DatabaseExecutor).Assembly, typeof(Database.Program).Assembly, Assembly.GetExecutingAssembly()) == 0;
            });

            AgentTester.StartupTestServer<Startup>(environmentVariablesPrefix: "AppName_");
        }
#endif
#if (implement_cosmos)
        private bool _removeAfterUse;
        private CosmosDb _cosmosDb;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            TestSetUp.RegisterSetUp(async (count, data) =>
            {
                var config = AgentTester.Configuration.GetSection("CosmosDb");
                _removeAfterUse = config.GetValue<bool>("RemoveAfterUse");
                _cosmosDb = new CosmosDb(new Cosmos.CosmosClient(config.GetValue<string>("EndPoint"), config.GetValue<string>("AuthKey")),
                    config.GetValue<string>("Database"), createDatabaseIfNotExists: true);

                var rc = await _cosmosDb.ReplaceOrCreateContainerAsync(
                    new Cosmos.ContainerProperties
                    {
                        Id = "Person",
                        PartitionKeyPath = "/_partitionKey"
                    }, 400);

                await rc.ImportBatchAsync<PersonTest, Person>("Person.yaml", "Person");

                var rdc = await _cosmosDb.ReplaceOrCreateContainerAsync(
                    new Cosmos.ContainerProperties
                    {
                        Id = "RefData",
                        PartitionKeyPath = "/_partitionKey",
                        UniqueKeyPolicy = new Cosmos.UniqueKeyPolicy { UniqueKeys = { new Cosmos.UniqueKey { Paths = { "/type", "/value/code" } } } }
                    }, 400);

                await rdc.ImportValueRefDataBatchAsync<PersonTest>(ReferenceData.Current, "RefData.yaml");

                return true;
            });

            AgentTester.StartupTestServer<Startup>(environmentVariablesPrefix: "AppName_");
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown()
        {
            if (_removeAfterUse)
                await _cosmosDb.Database.DeleteAsync();
        }
#endif
    }
}