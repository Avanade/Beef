using Beef.Entities;
using Beef.Test.NUnit;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using AzCosmos = Microsoft.Azure.Cosmos;

namespace Beef.Data.Cosmos.UnitTest
{
    public class CosmosDb : CosmosDbBase
    {
        public CosmosDb() : base(new AzCosmos.CosmosClient("https://localhost:8081", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="), "Beef.UnitTest", true)
        {
            Persons1 = new CosmosDbContainer<Person1>(this, CosmosDbArgs<Person1>.Create("Persons1"));
            Persons2 = new CosmosDbContainer<Person2>(this, CosmosDbArgs<Person2>.Create("Persons2"));
            Persons3 = new CosmosDbContainer<Person3>(this, CosmosDbArgs<Person3>.Create("Persons3"));
        }

        public async Task SetUp()
        {
            var rc = await ReplaceOrCreateContainerAsync(
                new AzCosmos.ContainerProperties
                {
                    Id = "Persons1",
                    PartitionKeyPath = "/_partitionKey",
                    UniqueKeyPolicy = new AzCosmos.UniqueKeyPolicy { UniqueKeys = { new AzCosmos.UniqueKey { Paths = { "/name" } } } }
                }, 400);

            await rc.ImportBatchAsync<CosmosDb, Person1>("Data.yaml");

            var rdc = await ReplaceOrCreateContainerAsync(
                new AzCosmos.ContainerProperties
                {
                    Id = "Persons2",
                    PartitionKeyPath = "/_partitionKey",
                    UniqueKeyPolicy = new AzCosmos.UniqueKeyPolicy { UniqueKeys = { new AzCosmos.UniqueKey { Paths = { "/name" } } } }
                }, 400);

            await rdc.ImportBatchAsync<CosmosDb, Person2>("Data.yaml");

            var rdvc = await ReplaceOrCreateContainerAsync(
                new AzCosmos.ContainerProperties
                {
                    Id = "Persons3",
                    PartitionKeyPath = "/_partitionKey",
                    UniqueKeyPolicy = new AzCosmos.UniqueKeyPolicy { UniqueKeys = { new AzCosmos.UniqueKey { Paths = { "/type", "/value/name" } } } }
                }, 400);

            await rdvc.ImportBatchAsync<CosmosDb, Person3>("Data.yaml");

            // Add other random "type" to Person3.
            var oth = new Person3 { Id = 100.ToGuid().ToString(), Type = "Other", Value = new Person2 { Name = "Greg" } };
            await this.Persons3.Container.CreateItemAsync(oth, AzCosmos.PartitionKey.None);
        }

        public CosmosDbContainer<Person1> Persons1 { get; private set; }

        public CosmosDbContainer<Person2> Persons2 { get; private set; }

        public CosmosDbContainer<Person3> Persons3 { get; private set; }
    }

    public class Person1 : IGuidIdentifier
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("birthday")]
        public DateTime Birthday { get; set; }

        [JsonProperty("salary")]
        public decimal Salary { get; set; }
    }

    public class Person2 : Person1, IChangeLog, IETag
    {
        [JsonProperty("changelog")]
        public ChangeLog ChangeLog { get; set; }

        [JsonProperty("_etag")]
        public string ETag { get; set; }
    }

    public class Person3 : CosmosDbTypeValue<Person2> { }
}