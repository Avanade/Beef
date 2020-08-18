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
        /// <typeparam name="TModel">The model <see cref="Type"/>.</typeparam>
        /// <param name="container">The <see cref="Container"/>.</param>
        /// <param name="items">The items to import.</param>
        /// <param name="partitionKey">The optional partition key; where not specified <see cref="PartitionKey.None"/> is used.</param>
        /// <param name="itemRequestOptions">The optional <see cref="ItemRequestOptions"/>.</param>
        /// <param name="setIdentifier">Indicates whether to override the <c>Id</c> where entity implements <see cref="Beef.Entities.IIdentifier"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        /// <remarks>Each item is added individually and is not transactional.</remarks>
        public static async Task ImportBatchAsync<TModel>(this Container container, IEnumerable<TModel> items, Func<TModel, PartitionKey?>? partitionKey = null, ItemRequestOptions? itemRequestOptions = null, bool setIdentifier = false) where TModel : class, new()
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            if (items == null)
                return;

            foreach (var item in items)
            {
                CosmosDbBase.PrepareEntityForCreate(item, setIdentifier);
                await container.CreateItemAsync(item, partitionKey?.Invoke(item), itemRequestOptions).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Imports a batch (creates) of <see cref="CosmosDbValue{TModel}"/> <paramref name="items"/> into the <see cref="Container"/>.
        /// </summary>
        /// <typeparam name="TModel">The model <see cref="Type"/>.</typeparam>
        /// <param name="container">The <see cref="Container"/>.</param>
        /// <param name="items">The items to import.</param>
        /// <param name="partitionKey">The optional partition key; where not specified <see cref="PartitionKey.None"/> is used.</param>
        /// <param name="itemRequestOptions">The optional <see cref="ItemRequestOptions"/>.</param>
        /// <param name="setIdentifier">Indicates whether to override the <c>Id</c> where entity implements <see cref="Beef.Entities.IIdentifier"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        /// <remarks>Each item is added individually and is not transactional.</remarks>
        public static async Task ImportValueBatchAsync<TModel>(this Container container, IEnumerable<TModel> items, Func<TModel, PartitionKey?>? partitionKey = null, ItemRequestOptions? itemRequestOptions = null, bool setIdentifier = false) where TModel : class, new()
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            if (items == null)
                return;

            foreach (var item in items)
            {
                var cdv = new CosmosDbValue<TModel>(item);
                CosmosDbBase.PrepareEntityForCreate(cdv.Value, setIdentifier);
                ((ICosmosDbValue)cdv).PrepareBefore();
                await container.CreateItemAsync(cdv, partitionKey?.Invoke(item), itemRequestOptions).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Imports a batch (creates) of items specified within a <b>YAML</b> resource (see <see cref="Beef.Yaml.YamlConverter"/>) into the <see cref="Container"/>.
        /// </summary>
        /// <typeparam name="TResource">The <see cref="Type"/> to infer the <see cref="Assembly"/> to find manifest resources (see <see cref="Assembly.GetManifestResourceStream(string)"/>).</typeparam>
        /// <typeparam name="TModel">The model <see cref="Type"/>.</typeparam>
        /// <param name="container">The <see cref="Container"/>.</param>
        /// <param name="yamlResourceName">The YAML resource name (must reside in <c>Cosmos</c> folder within the <typeparamref name="TResource"/> <see cref="Assembly"/>).</param>
        /// <param name="name">The YAML node name to load; where <c>null</c> will infer the name from the <typeparamref name="TModel"/>.</param>
        /// <param name="partitionKey">The optional partition key; where not specified <see cref="PartitionKey.None"/> is used.</param>
        /// <param name="itemRequestOptions">The optional <see cref="ItemRequestOptions"/>.</param>
        /// <param name="setIdentifier">Indicates whether to override the <c>Id</c> where entity implements <see cref="Beef.Entities.IIdentifier"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        /// <remarks>Each item is added individually and is not transactional.</remarks>
        public static async Task ImportBatchAsync<TResource, TModel>(this Container container, string yamlResourceName, string? name = null, Func<TModel, PartitionKey?>? partitionKey = null, ItemRequestOptions? itemRequestOptions = null, bool setIdentifier = false) where TModel : class, new()
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            using var rs = GetResourceStream<TResource>(Check.NotEmpty(yamlResourceName, nameof(yamlResourceName)));
            var yc = Beef.Yaml.YamlConverter.ReadYaml(rs);
            await ImportBatchAsync(container, yc.Convert<TModel>(name ?? typeof(TModel).Name), partitionKey, itemRequestOptions, setIdentifier).ConfigureAwait(false);
        }

        /// <summary>
        /// Imports a batch (creates) of <see cref="CosmosDbValue{TModel}"/> items specified within a <b>YAML</b> resource (see <see cref="Beef.Yaml.YamlConverter"/>) into the <see cref="Container"/>.
        /// </summary>
        /// <typeparam name="TResource">The <see cref="Type"/> to infer the <see cref="Assembly"/> to find manifest resources (see <see cref="Assembly.GetManifestResourceStream(string)"/>).</typeparam>
        /// <typeparam name="TModel">The item <see cref="Type"/>.</typeparam>
        /// <param name="container">The <see cref="Container"/>.</param>
        /// <param name="yamlResourceName">The YAML resource name (must reside in <c>Cosmos</c> folder within the <typeparamref name="TResource"/> <see cref="Assembly"/>).</param>
        /// <param name="name">The YAML node name to load; where <c>null</c> will infer the name from the <typeparamref name="TModel"/>.</param>
        /// <param name="partitionKey">The optional partition key; where not specified <see cref="PartitionKey.None"/> is used.</param>
        /// <param name="itemRequestOptions">The optional <see cref="ItemRequestOptions"/>.</param>
        /// <param name="setIdentifier">Indicates whether to override the <c>Id</c> where entity implements <see cref="Beef.Entities.IIdentifier"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        /// <remarks>Each item is added individually and is not transactional.</remarks>
        public static async Task ImportValueBatchAsync<TResource, TModel>(this Container container, string yamlResourceName, string? name = null, Func<TModel, PartitionKey?>? partitionKey = null, ItemRequestOptions? itemRequestOptions = null, bool setIdentifier = false) where TModel : class, new()
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            using var rs = GetResourceStream<TResource>(Check.NotEmpty(yamlResourceName, nameof(yamlResourceName)));
            var yc = Beef.Yaml.YamlConverter.ReadYaml(rs);
            await ImportValueBatchAsync(container, yc.Convert<TModel>(name ?? typeof(TModel).Name), partitionKey, itemRequestOptions, setIdentifier).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the resource stream.
        /// </summary>
        private static System.IO.Stream GetResourceStream<TResource>(string yamlResourceName)
        {
            if (string.IsNullOrEmpty(yamlResourceName))
                throw new ArgumentNullException(nameof(yamlResourceName));

            var ass = typeof(TResource).Assembly;
            var rn = ass.GetManifestResourceNames().Where(x => x.EndsWith($"Cosmos.{Check.NotNull(yamlResourceName, nameof(yamlResourceName))}", StringComparison.InvariantCultureIgnoreCase));
            if (rn == null || rn.Count() != 1)
                throw new ArgumentException($"A single Resource with name ending in 'Cosmos.{yamlResourceName}' not found in Assembly '{ass.FullName}'.", nameof(yamlResourceName));

            return ass.GetManifestResourceStream(rn.First()) ?? throw new ArgumentException($"A single Resource with name ending in 'Cosmos.{yamlResourceName}' not found in Assembly '{ass.FullName}'.", nameof(yamlResourceName));
        }

        /// <summary>
        /// Imports a batch (creates) of <see cref="CosmosDbValue{TModel}"/> <b>reference data</b> items specified within a <b>YAML</b> resource (see <see cref="Beef.Yaml.YamlConverter"/>) into the <see cref="Container"/>.
        /// </summary>
        /// <typeparam name="TResource">The <see cref="Type"/> to infer the <see cref="Assembly"/> to find manifest resources (see <see cref="Assembly.GetManifestResourceStream(string)"/>).</typeparam>
        /// <typeparam name="TRefData">The <see cref="IReferenceDataProvider"/> concrete <see cref="Type"/> (requires static <c>GetAllTypes</c> method).</typeparam>
        /// <param name="container">The <see cref="Container"/>.</param>
        /// <param name="yamlResourceName">The YAML resource name (must reside in <c>Cosmos</c> folder within the <typeparamref name="TResource"/> <see cref="Assembly"/>).</param>
        /// <param name="partitionKey">The optional partition key; where not specified <see cref="PartitionKey.None"/> is used.</param>
        /// <param name="itemRequestOptions">The optional <see cref="ItemRequestOptions"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        /// <remarks>Each item is added individually and is not transactional. Also, the data is created using a <see cref="CosmosDbValue{ReferenceDataBase}"/> so that multiple reference data types
        /// can co-exist within the same collection.</remarks>
        public static async Task ImportValueRefDataBatchAsync<TResource, TRefData>(this Container container, string yamlResourceName, Func<ReferenceDataBase, PartitionKey?>? partitionKey = null, ItemRequestOptions? itemRequestOptions = null)
            where TRefData : class, IReferenceDataProvider
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));

            if (string.IsNullOrEmpty(yamlResourceName))
                throw new ArgumentNullException(nameof(yamlResourceName));

            using var rs = GetResourceStream<TResource>(yamlResourceName);
            var yc = Beef.Yaml.YamlConverter.ReadYaml(rs);
            var yt = typeof(Beef.Yaml.YamlConverter);
            var ct = typeof(Container);
            var emi = typeof(CosmosDbContainerExtensions)
                .GetMethods().Where(x => x.Name == nameof(CosmosDbContainerExtensions.ImportValueBatchAsync) && x.GetGenericArguments().Length == 1).Single();

            foreach (var rdt in GetAllTypes<TRefData>())
            {
                var vals = (System.Collections.IEnumerable?)yt.GetMethod("Convert")?.MakeGenericMethod(rdt).Invoke(yc, new object[] { rdt.Name, true, true, true, null! });
                if (vals != null)
                {
                    var r = emi.MakeGenericMethod(rdt).Invoke(null, new object[] { container, vals!, partitionKey!, itemRequestOptions!, false });
                    if (r != null)
                        await ((Task)r).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Gets all the types for the reference data interface.
        /// </summary>
        private static Type[] GetAllTypes<TRefData>() where TRefData : IReferenceDataProvider
        {
            var mi = typeof(TRefData).GetMethod("GetAllTypes", BindingFlags.Public | BindingFlags.Static);
            if (mi == null)
                throw new InvalidOperationException($"The {typeof(TRefData).Name} Type must have a static 'GetAllTypes' method.");

            return (Type[])(mi.Invoke(null, null) ?? Array.Empty<Type>());
        }
    }
}