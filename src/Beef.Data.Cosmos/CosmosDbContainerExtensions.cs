// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Beef.Data.Cosmos
{
    /// <summary>
    /// Provides <b>Cosmos</b> <see cref="Container"/> extensions.
    /// </summary>
    public static class CosmosDbContainerExtensions
    {
        /// <summary>
        /// Imports a batch (creates) of <paramref name="items"/> into the <see cref="Container"/>.
        /// </summary>
        /// <typeparam name="T">The item <see cref="Type"/>.</typeparam>
        /// <param name="container">The <see cref="Container"/>.</param>
        /// <param name="items">The items to import.</param>
        /// <param name="partitionKey">The optional partition key; where not specified <see cref="PartitionKey.None"/> is used.</param>
        /// <param name="itemRequestOptions">The optional <see cref="ItemRequestOptions"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        /// <remarks>Each item is added individually and is not transactional.</remarks>
        public static async Task ImportBatchAsync<T>(this Container container, IEnumerable<T> items, PartitionKey? partitionKey = null, ItemRequestOptions itemRequestOptions = null) where T : class
        {
            if (items == null)
                return;

            foreach (var item in items)
            {
                await container.CreateItemAsync(item, partitionKey ?? PartitionKey.None, itemRequestOptions);
            }
        }

        /// <summary>
        /// Imports a batch (creates) of items specified within a <b>YAML</b> resource (see <see cref="Beef.Yaml.YamlConverter"/>) into the <see cref="Container"/>.
        /// </summary>
        /// <typeparam name="TResource">The <see cref="Type"/> to infer the <see cref="Assembly"/> to find manifest resources (see <see cref="Assembly.GetManifestResourceStream(string)"/>).</typeparam>
        /// <typeparam name="T">The item <see cref="Type"/>.</typeparam>
        /// <param name="container">The <see cref="Container"/>.</param>
        /// <param name="yamlResourceName">The YAML resource name (must reside in <c>Cosmos</c> folder within the <typeparamref name="TResource"/> <see cref="Assembly"/>).</param>
        /// <param name="name">The YAML node name to load.</param>
        /// <param name="partitionKey">The optional partition key; where not specified <see cref="PartitionKey.None"/> is used.</param>
        /// <param name="itemRequestOptions">The optional <see cref="ItemRequestOptions"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        /// <remarks>Each item is added individually and is not transactional.</remarks>
        public static async Task ImportBatchAsync<TResource, T>(this Container container, string yamlResourceName, string name, PartitionKey? partitionKey = null, ItemRequestOptions itemRequestOptions = null) where T : class, new()
        {
            using (var rs = GetResourceStream<TResource>(yamlResourceName))
            {
                var yc = Beef.Yaml.YamlConverter.ReadYaml(rs);
                await ImportBatchAsync(container, yc.Convert<T>(Check.NotEmpty(name, nameof(name))), partitionKey, itemRequestOptions);
            }
        }

        /// <summary>
        /// Gets the resource strem.
        /// </summary>
        private static System.IO.Stream GetResourceStream<TResource>(string yamlResourceName)
        {
            var ass = typeof(TResource).Assembly;
            var rn = ass.GetManifestResourceNames().Where(x => x.EndsWith($"Cosmos.{Check.NotNull(yamlResourceName, nameof(yamlResourceName))}"));
            if (rn == null || rn.Count() != 1)
                throw new ArgumentException($"A single Resource with name ending in 'Cosmos.{yamlResourceName}' not found in Assembly '{ass.FullName}'.", nameof(yamlResourceName));

            return ass.GetManifestResourceStream(rn.First());
        }

        /// <summary>
        /// Imports a batch (creates) of <b>reference data</b> items specified within a <b>YAML</b> resource (see <see cref="Beef.Yaml.YamlConverter"/>) into the <see cref="Container"/>.
        /// </summary>
        /// <typeparam name="TResource">The <see cref="Type"/> to infer the <see cref="Assembly"/> to find manifest resources (see <see cref="Assembly.GetManifestResourceStream(string)"/>).</typeparam>
        /// <param name="container">The <see cref="Container"/>.</param>
        /// <param name="refData">The <see cref="ReferenceDataManager"/> to infer the underlying reference data types.</param>
        /// <param name="yamlResourceName">The YAML resource name (must reside in <c>Cosmos</c> folder within the <typeparamref name="TResource"/> <see cref="Assembly"/>).</param>
        /// <param name="partitionKey">The optional partition key; where not specified <see cref="PartitionKey.None"/> is used.</param>
        /// <param name="itemRequestOptions">The optional <see cref="ItemRequestOptions"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        /// <remarks>Each item is added individually and is not transactional. Also, the data is created using a <see cref="CosmosDbTypeValue{ReferenceDataBase}"/> so that multiple reference data types
        /// can co-exist within the same collection.</remarks>
        public static async Task ImportRefDataBatchAsync<TResource>(this Container container, ReferenceDataManager refData, string yamlResourceName, PartitionKey? partitionKey = null, ItemRequestOptions itemRequestOptions = null)
        {
            Check.NotNull(refData, nameof(refData));

            using (var rs = GetResourceStream<TResource>(yamlResourceName))
            {
                var yc = Beef.Yaml.YamlConverter.ReadYaml(rs);
                var yt = typeof(Beef.Yaml.YamlConverter);
                var tvt = typeof(CosmosDbTypeValue<>);
                var ct = typeof(Container);

                foreach (var rdt in refData.GetAllTypes())
                {
                    var vals = (System.Collections.IEnumerable)yt.GetMethod("Convert").MakeGenericMethod(rdt).Invoke(yc, new object[] { rdt.Name, true, true, true, null });
                    if (vals == null)
                        continue;

                    var tvvt = tvt.MakeGenericType(rdt);

                    foreach (var v in vals)
                    {
                        var item = (CosmosDbTypeValue)Activator.CreateInstance(tvvt, v);
                        item.PrepareBefore();
                        await (Task)ct.GetMethod("CreateItemAsync").MakeGenericMethod(tvvt).Invoke(container, new object[] { item, partitionKey ?? PartitionKey.None, itemRequestOptions, default(System.Threading.CancellationToken) });
                    }
                }
            }
        }
    }
}
