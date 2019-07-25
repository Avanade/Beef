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
        /// Opens the <b>Cosmos</b> container.
        /// </summary>
        /// <param name="client">The <see cref="CosmosClient"/>.</param>
        /// <param name="databaseId">The database identifier.</param>
        /// <param name="containerId">The container identitier.</param>
        /// <returns>The <see cref="CosmosDbContainerSetUp"/>.</returns>
        public static CosmosDbContainerSetUp Open(CosmosClient client, string databaseId, string containerId)
        {
            Check.NotNull(client, nameof(client));
            var database = client.GetDatabase(Check.NotEmpty(databaseId, nameof(databaseId)));
            return new CosmosDbContainerSetUp(database.GetContainer(Check.NotEmpty(containerId, nameof(containerId))));
        }

        /// <summary>
        /// Replaces (deletes and creates) the <b>Cosmos</b> container.
        /// </summary>
        /// <param name="client">The <see cref="CosmosClient"/>.</param>
        /// <param name="databaseId">The database identifier.</param>
        /// <param name="containerProperties">The <see cref="ContainerProperties"/> used for the create.</param>
        /// <param name="throughput">The throughput (RU/S).</param>
        /// <returns>The <see cref="CosmosDbContainerSetUp"/>.</returns>
        public static async Task<CosmosDbContainerSetUp> ReplaceAndOpenAsync(CosmosClient client, string databaseId, ContainerProperties containerProperties, int? throughput = 400)
        {
            Check.NotNull(client, nameof(client));
            Check.NotNull(containerProperties, nameof(containerProperties));

            var database = client.GetDatabase(Check.NotEmpty(databaseId, nameof(databaseId)));
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
            return new CosmosDbContainerSetUp(container);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbContainerSetUp"/> class.
        /// </summary>
        /// <param name="container"></param>
        private CosmosDbContainerSetUp(Container container) => Container = container;

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
        /// <param name="yamlResourceName">The YAML resource name (must reside in <c>Cosmos</c> folder within <see cref="Assembly.GetCallingAssembly"/>).</param>
        /// <param name="name"></param>
        /// <param name="partitionKey">The optional partition key; where not specified <see cref="PartitionKey.None"/> is used.</param>
        /// <param name="itemRequestOptions">The optional <see cref="ItemRequestOptions"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        /// <remarks>Each item is added individually and is not transactional.</remarks>
        public async Task ImportBatchAsync<TResource, T>(string yamlResourceName, string name = "items", PartitionKey? partitionKey = null, ItemRequestOptions itemRequestOptions = null) where T : class, new()
        {
            var ass = typeof(TResource).Assembly;
            var rn = ass.GetManifestResourceNames().Where(x => x.EndsWith($"Cosmos.{Check.NotNull(yamlResourceName, nameof(yamlResourceName))}"));
            if (rn == null || rn.Count() > 1)
                throw new ArgumentException($"A single Resource with name ending in 'Cosmos.{yamlResourceName}' not found in Assembly '{ass.FullName}'.", nameof(yamlResourceName));

            var yc = YamlConverter.ReadYaml(ass.GetManifestResourceStream(rn.First()));

            await ImportBatchAsync(yc.Convert<T>(Check.NotEmpty(name, nameof(name))), partitionKey, itemRequestOptions);
        }
    }
}
