using AutoMapper;
using Beef.Entities;
using Beef.RefData;
using Beef.Test.NUnit;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using AzCosmos = Microsoft.Azure.Cosmos;

namespace Beef.Data.Cosmos.UnitTest
{
    public class CosmosDb : CosmosDbBase
    {
        private readonly IMapper _mapper;

        public CosmosDb() : base(new AzCosmos.CosmosClient("https://localhost:8081", "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="), "Beef.UnitTest", true)
        {
            _mapper = new AutoMapper.Mapper(new MapperConfiguration(c =>
            {
                c.AddProfile<Mapper.AutoMapperProfile>();
                c.CreateMap<Person1, Person1>();
                c.CreateMap<Person2, Person2>();
                c.CreateMap<Person3, Person3>();
            }));

            Persons1 = new CosmosDbContainer<Person1, Person1>(this, CosmosDbArgs.Create(_mapper, "Persons1"));
            Persons2 = new CosmosDbContainer<Person2, Person2>(this, CosmosDbArgs.Create(_mapper, "Persons2"));
            Persons3 = new CosmosDbValueContainer<Person3, Person3>(this, CosmosDbArgs.Create(_mapper, "Persons3"));
        }

        public async Task SetUp()
        {
            var c1 = await ReplaceOrCreateContainerAsync(
                new AzCosmos.ContainerProperties
                {
                    Id = "Persons1",
                    PartitionKeyPath = "/_partitionKey",
                    UniqueKeyPolicy = new AzCosmos.UniqueKeyPolicy { UniqueKeys = { new AzCosmos.UniqueKey { Paths = { "/name" } } } }
                }, 400);

            await c1.ImportBatchAsync<CosmosDb, Person1>("Data.yaml");

            var c2 = await ReplaceOrCreateContainerAsync(
                new AzCosmos.ContainerProperties
                {
                    Id = "Persons2",
                    PartitionKeyPath = "/_partitionKey",
                    UniqueKeyPolicy = new AzCosmos.UniqueKeyPolicy { UniqueKeys = { new AzCosmos.UniqueKey { Paths = { "/name" } } } }
                }, 400);

            await c2.ImportBatchAsync<CosmosDb, Person2>("Data.yaml");

            var c3 = await ReplaceOrCreateContainerAsync(
                new AzCosmos.ContainerProperties
                {
                    Id = "Persons3",
                    PartitionKeyPath = "/_partitionKey",
                    UniqueKeyPolicy = new AzCosmos.UniqueKeyPolicy { UniqueKeys = { new AzCosmos.UniqueKey { Paths = { "/type", "/value/name" } } } }
                }, 400);

            await c3.ImportValueBatchAsync<CosmosDb, Person3>("Data.yaml");

            // Add other random "type" to Person3.
            var c = new CosmosDbValueContainer<Person1, Person1>(this, CosmosDbArgs.Create(_mapper, "Persons3"));
            await c.Container.ImportValueBatchAsync(new Person1[] { new Person1 { Id = 100.ToGuid(), Name = "Greg" } });

            // Load the reference data.
            var rd = await ReplaceOrCreateContainerAsync(
                new AzCosmos.ContainerProperties
                {
                    Id = "RefData",
                    PartitionKeyPath = "/_partitionKey",
                    UniqueKeyPolicy = new AzCosmos.UniqueKeyPolicy { UniqueKeys = { new AzCosmos.UniqueKey { Paths = { "/type", "/value/code" } } } }
                }, 400);

            await rd.ImportValueRefDataBatchAsync<ReferenceDataProvider, ReferenceDataProvider>("RefData.yaml");
        }

        public CosmosDbContainer<Person1, Person1> Persons1 { get; private set; }

        public CosmosDbContainer<Person2, Person2> Persons2 { get; private set; }

        public CosmosDbValueContainer<Person3, Person3> Persons3 { get; private set; }
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

    public class Person3 : Person2 { }

    public class Gender : ReferenceDataBaseGuid
    {
        public override object Clone() => throw new NotImplementedException();
    }

    public class GenderCollection : ReferenceDataCollectionBase<Gender> { }

    public interface IReferenceData : IReferenceDataProvider
    {
        public GenderCollection Gender { get; }
    }

    public class ReferenceDataProvider : IReferenceData
    {
        public IReferenceDataCollection this[Type type] => throw new NotImplementedException();

        public Type ProviderType => typeof(ReferenceDataProvider);

        public static Type[] GetAllTypes() => new Type[] { typeof(Gender) };

        public GenderCollection Gender { get; } = new GenderCollection();

        public Task PrefetchAsync(params string[] names) => throw new NotImplementedException();
    }
}