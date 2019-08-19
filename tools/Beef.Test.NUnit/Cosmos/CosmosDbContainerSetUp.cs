using Beef.Diagnostics;
using Beef.RefData;
using Beef.Test.NUnit.Internal;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace Beef.Test.NUnit.Cosmos
{
    /// <summary>
    /// Provides <see cref="Container"/> set up capabilities for <b>Cosmos</b> testing.
    /// </summary>
    public class CosmosDbContainerSetUp
    {
        /// <summary>
        /// Opens the <b>Cosmos</b> container (assumes pre-existing).
        /// </summary>
        /// <param name="client">The <see cref="CosmosClient"/>.</param>
        /// <param name="databaseId">The database identifier.</param>
        /// <param name="containerId">The container identitier.</param>
        /// <returns>The <see cref="CosmosDbContainerSetUp"/>.</returns>
        public static CosmosDbContainerSetUp Open(CosmosClient client, string databaseId, string containerId)
        {
            Logger.Default.Info($"COSMOS > Database '{databaseId}' Container '{containerId}' Open (pre-existing).");

            Check.NotNull(client, nameof(client));
            var database = client.GetDatabase(Check.NotEmpty(databaseId, nameof(databaseId)));
            return new CosmosDbContainerSetUp(database, database.GetContainer(Check.NotEmpty(containerId, nameof(containerId))));
        }

        /// <summary>
        /// Replaces (deletes and creates) the <b>Cosmos</b> container (and optionally creates the database if it does not exist).
        /// </summary>
        /// <param name="client">The <see cref="CosmosClient"/>.</param>
        /// <param name="databaseId">The database identifier.</param>
        /// <param name="containerProperties">The <see cref="ContainerProperties"/> used for the create.</param>
        /// <param name="throughput">The throughput (RU/S).</param>
        /// <param name="createDatabaseIfNotExists">Indicates whether the database shoould be created if it does not exist.</param>
        /// <returns>The <see cref="CosmosDbContainerSetUp"/>.</returns>
        public static async Task<CosmosDbContainerSetUp> ReplaceAndOpenAsync(CosmosClient client, string databaseId, ContainerProperties containerProperties, int? throughput = 400, bool createDatabaseIfNotExists = true)
        {
            Logger.Default.Info($"COSMOS > Database '{databaseId}' Container '{containerProperties.Id}' Replace and Open.");

            Check.NotNull(client, nameof(client));
            Check.NotNull(containerProperties, nameof(containerProperties));

            var database = client.GetDatabase(Check.NotEmpty(databaseId, nameof(databaseId)));
            if (createDatabaseIfNotExists)
                database = (await client.CreateDatabaseIfNotExistsAsync(databaseId, throughput)).Database;

            var container = database.GetContainer(containerProperties.Id);

            // Remove existing container if it already exists.
            try
            {
                await container.DeleteContainerAsync();
            }
            catch (CosmosException cex)
            {
                if (cex.StatusCode != HttpStatusCode.NotFound)
                    throw;
            }

            // Create the container as specified.
            await database.CreateContainerIfNotExistsAsync(containerProperties, throughput);
            return new CosmosDbContainerSetUp(database, container);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbContainerSetUp"/> class.
        /// </summary>
        private CosmosDbContainerSetUp(Database database, Container container)
        {
            Database = database;
            Container = container;
        }

        /// <summary>
        /// Gets the underlying <see cref="T:Database"/>.
        /// </summary>
        public Database Database { get; private set; }

        /// <summary>
        /// Gets the underlying <see cref="T:Container"/>.
        /// </summary>
        public Container Container { get; private set; }

        /// <summary>
        /// Imports a batch (creates) of <paramref name="items"/> into the <see cref="Container"/>.
        /// </summary>
        /// <typeparam name="T">The item <see cref="Type"/>.</typeparam>
        /// <param name="items">The items to import.</param>
        /// <param name="partitionKey">The optional partition key; where not specified <see cref="PartitionKey.None"/> is used.</param>
        /// <param name="itemRequestOptions">The optional <see cref="ItemRequestOptions"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        /// <remarks>Each item is added individually and is not transactional.</remarks>
        public async Task ImportBatchAsync<T>(IEnumerable<T> items, PartitionKey? partitionKey = null, ItemRequestOptions itemRequestOptions = null) where T : class
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                await Container.CreateItemAsync(item, partitionKey ?? PartitionKey.None, itemRequestOptions);
            }
        }

        /// <summary>
        /// Imports a batch (creates) of items specified within a <b>YAML</b> resource (see <see cref="YamlConverter"/>) into the <see cref="Container"/>.
        /// </summary>
        /// <typeparam name="TResource">The <see cref="Type"/> to infer the <see cref="Assembly"/> to find manifest resources (see <see cref="Assembly.GetManifestResourceStream(string)"/>).</typeparam>
        /// <typeparam name="T">The item <see cref="Type"/>.</typeparam>
        /// <param name="yamlResourceName">The YAML resource name (must reside in <c>Cosmos</c> folder within the <typeparamref name="TResource"/> <see cref="Assembly"/>).</param>
        /// <param name="name">The YAML node name to load.</param>
        /// <param name="partitionKey">The optional partition key; where not specified <see cref="PartitionKey.None"/> is used.</param>
        /// <param name="itemRequestOptions">The optional <see cref="ItemRequestOptions"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        /// <remarks>Each item is added individually and is not transactional.</remarks>
        public async Task ImportBatchAsync<TResource, T>(string yamlResourceName, string name, PartitionKey? partitionKey = null, ItemRequestOptions itemRequestOptions = null) where T : class, new()
        {
            Check.NotEmpty(yamlResourceName, nameof(yamlResourceName));
            Logger.Default.Info($"COSMOS > Database '{Database.Id}' Container '{Container.Id}' data import from resource '{yamlResourceName}' with node name '{name}'.");

            var ass = typeof(TResource).Assembly;
            var rn = ass.GetManifestResourceNames().Where(x => x.EndsWith($"Cosmos.{Check.NotNull(yamlResourceName, nameof(yamlResourceName))}"));
            if (rn == null || rn.Count() > 1)
                throw new ArgumentException($"A single Resource with name ending in 'Cosmos.{yamlResourceName}' not found in Assembly '{ass.FullName}'.", nameof(yamlResourceName));

            var yc = Internal.YamlConverter.ReadYaml(ass.GetManifestResourceStream(rn.First()));

            await ImportBatchAsync(yc.Convert<T>(Check.NotEmpty(name, nameof(name))), partitionKey, itemRequestOptions);
        }

        /// <summary>
        /// Imports a batch (creates) of <b>reference data</b> items specified within a <b>YAML</b> resource (see <see cref="YamlConverter"/>) into the <see cref="Container"/>.
        /// </summary>
        /// <typeparam name="TResource">The <see cref="Type"/> to infer the <see cref="Assembly"/> to find manifest resources (see <see cref="Assembly.GetManifestResourceStream(string)"/>).</typeparam>
        /// <param name="refData">The <see cref="ReferenceDataManager"/> to infer the underlying reference data types.</param>
        /// <param name="yamlResourceName">The YAML resource name (must reside in <c>Cosmos</c> folder within the <typeparamref name="TResource"/> <see cref="Assembly"/>).</param>
        /// <param name="partitionKey">The optional partition key; where not specified <see cref="PartitionKey.None"/> is used.</param>
        /// <param name="itemRequestOptions">The optional <see cref="ItemRequestOptions"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        /// <remarks>Each item is added individually and is not transactional.</remarks>
        public async Task ImportRefDataBatch<TResource>(ReferenceDataManager refData, string yamlResourceName, PartitionKey? partitionKey = null, ItemRequestOptions itemRequestOptions = null)
        {
            Check.NotNull(refData, nameof(refData));
            Check.NotEmpty(yamlResourceName, nameof(yamlResourceName));



            await Task.CompletedTask;
        }

        /// <summary>
        /// Delete the <see cref="Database"/>.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task DeleteDatabase()
        {
            Logger.Default.Info($"COSMOS > Database '{Database.Id}' delete.");

            await Database.DeleteAsync();
        }

        /// <summary>
        /// Delete the <see cref="Container"/>.
        /// </summary>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task DeleteContainer()
        {
            Logger.Default.Info($"COSMOS > Database '{Database.Id}' Container '{Container.Id}' delete.");

            await Container.DeleteContainerAsync();
        }
    }
}
