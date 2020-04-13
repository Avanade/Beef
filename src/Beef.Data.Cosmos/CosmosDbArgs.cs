// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Mapper;
using Microsoft.Azure.Cosmos;
using System;
using System.Linq;
using System.Net;

namespace Beef.Data.Cosmos
{
    /// <summary>
    /// Enables the <b>CosmosDb/DocumentDb Container</b> arguments capabilities.
    /// </summary>
    public interface ICosmosDbArgs
    {
        /// <summary>
        /// Gets the <see cref="IEntityMapper"/>.
        /// </summary>
        IEntityMapper Mapper { get; }

        /// <summary>
        /// Gets the <see cref="Container"/> identifier.
        /// </summary>
        string ContainerId { get; }

        /// <summary>
        /// Gets or sets the <see cref="Microsoft.Azure.Cosmos.PartitionKey"/>.
        /// </summary>
        PartitionKey? PartitionKey { get; set; }

        /// <summary>
        /// Gets the <see cref="PagingResult"/>.
        /// </summary>
        PagingResult? Paging { get; }

        /// <summary>
        /// Indicates that a <c>null</c> is to be returned where the <b>response</b> from <b>Cosmos</b> has a <see cref="HttpStatusCode"/> of <see cref="HttpStatusCode.NotFound"/>.
        /// </summary>
        bool NullOnNotFoundResponse { get; }

        /// <summary>
        /// Indicates whether to the set (override) the identifier on <b>Create</b> where the entity implements <see cref="IGuidIdentifier"/> or <see cref="IStringIdentifier"/>.
        /// </summary>
        bool SetIdentifierOnCreate { get; }

        /// <summary>
        /// Gets or sets the <see cref="Microsoft.Azure.Cosmos.RequestOptions"/>.
        /// </summary>
        ItemRequestOptions? ItemRequestOptions { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Microsoft.Azure.Cosmos.QueryRequestOptions"/>.
        /// </summary>
        QueryRequestOptions? QueryRequestOptions { get; set; }
    }

    /// <summary>
    /// Provides the base <b>CosmosDb/DocumentDb Container</b> arguments capabilities.
    /// </summary>
    /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TModel">The cosmos model.</typeparam>
    public class CosmosDbArgs<T, TModel> : ICosmosDbArgs where T : class, new() where TModel : class, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbArgs{T, TModel}"/> class.
        /// </summary>
        /// <param name="mapper">The <see cref="IEntityMapper{T, TModel}"/>.</param>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="partitionKey">The <see cref="PartitionKey"/>.</param>
        /// <param name="requestOptions">The optional <see cref="Microsoft.Azure.Cosmos.ItemRequestOptions"/>.</param>
        public CosmosDbArgs(IEntityMapper<T, TModel> mapper, string containerId, PartitionKey? partitionKey = null, ItemRequestOptions? requestOptions = null)
        {
            Mapper = Check.NotNull(mapper, nameof(mapper));
            ContainerId = Check.NotEmpty(containerId, nameof(containerId));
            PartitionKey = partitionKey;
            ItemRequestOptions = requestOptions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbArgs{T, TModel}"/> class.
        /// </summary>
        /// <param name="mapper">The <see cref="IEntityMapper{T, TModel}"/>.</param>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="partitionKey">The <see cref="PartitionKey"/>.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="requestOptions">The optional <see cref="FeedOptions"/>.</param>
        public CosmosDbArgs(IEntityMapper<T, TModel> mapper, string containerId, PartitionKey? partitionKey, PagingArgs paging, QueryRequestOptions? requestOptions = null) 
            : this(mapper, containerId, partitionKey, new PagingResult(Check.NotNull(paging, (nameof(paging)))), requestOptions) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbArgs{T, TModel}"/> class.
        /// </summary>
        /// <param name="mapper">The <see cref="IEntityMapper{T, TModel}"/>.</param>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="partitionKey">The <see cref="PartitionKey"/>.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="requestOptions">The optional <see cref="FeedOptions"/>.</param>
        public CosmosDbArgs(IEntityMapper<T, TModel> mapper, string containerId, PartitionKey? partitionKey, PagingResult paging, QueryRequestOptions? requestOptions = null)
        {
            Mapper = Check.NotNull(mapper, nameof(mapper));
            ContainerId = Check.NotEmpty(containerId, nameof(containerId));
            PartitionKey = partitionKey;
            Paging = Check.NotNull(paging, nameof(paging));
            QueryRequestOptions = requestOptions;
        }

        /// <summary>
        /// Gets the <see cref="IEntityMapper"/>.
        /// </summary>
        IEntityMapper ICosmosDbArgs.Mapper => Mapper;

        /// <summary>
        /// Gets the <see cref="IEntityMapper{T, TModel}"/>.
        /// </summary>
        public IEntityMapper<T, TModel> Mapper { get; private set; }

        /// <summary>
        /// Gets the <see cref="Container"/> identifier.
        /// </summary>
        public string ContainerId { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="Microsoft.Azure.Cosmos.PartitionKey"/>.
        /// </summary>
        public PartitionKey? PartitionKey { get; set; }

        /// <summary>
        /// Gets the <see cref="PagingResult"/> (where paging is required for a <b>query</b>).
        /// </summary>
        public PagingResult? Paging { get; private set; } = default;

        /// <summary>
        /// Gets the <see cref="Microsoft.Azure.Cosmos.ItemRequestOptions"/> used for <b>Get</b>, <b>Create</b>, <b>Update</b>, and <b>Delete</b> (<seealso cref="QueryRequestOptions"/>).
        /// </summary>
        public ItemRequestOptions? ItemRequestOptions { get; set; }

        /// <summary>
        /// Gets the <see cref="Microsoft.Azure.Cosmos.QueryRequestOptions"/> used for <b>Query</b> (<seealso cref="ItemRequestOptions"/>).
        /// </summary>
        public QueryRequestOptions? QueryRequestOptions { get; set; }

        /// <summary>
        /// Indicates that a <c>null</c> is to be returned where the <b>response</b> has a <see cref="HttpStatusCode"/> of <see cref="HttpStatusCode.NotFound"/> on <b>Get</b>.
        /// </summary>
        public bool NullOnNotFoundResponse { get; set; } = true;

        /// <summary>
        /// Indicates whether to the set (override) the identifier on <b>Create</b> where the entity implements <see cref="IGuidIdentifier"/> or <see cref="IStringIdentifier"/>.
        /// </summary>
        /// <remarks>The value will be set using <see cref="System.Guid.NewGuid"/>.</remarks>
        public bool SetIdentifierOnCreate { get; set; } = true;

        /// <summary>
        /// Gets the <b>CosmosDb/DocumentDb</b> key from the specified keys.
        /// </summary>
        /// <param name="keys">The key values.</param>
        /// <returns>The cosmos key.</returns>
        internal string GetCosmosKey(IComparable?[] keys)
        {
            if (keys == null || keys.Length == 0)
                throw new ArgumentNullException(nameof(keys));

            if (keys.Length != 1)
                throw new NotSupportedException("Only a single key value is currently supported.");

            if (keys.Length != Mapper.UniqueKey.Count)
                throw new ArgumentException($"The specified keys count '{keys.Length}' does not match the Mapper UniqueKey count '{Mapper.UniqueKey.Count}'.", nameof(keys));

            var k = (Mapper.UniqueKey[0].ConvertToDestValue(keys[0], OperationTypes.Unspecified) ?? string.Empty).ToString();

            if (string.IsNullOrEmpty(k))
                throw new InvalidOperationException("A key (non null) was unable to be derived from the value.");

            return k;
        }

        /// <summary>
        /// Gets the <b>CosmosDb/DocumentDb</b> key from the entity value.
        /// </summary>
        /// <param name="value">The entity value.</param>in
        /// <returns>The cosmos key.</returns>
        internal string GetCosmosKey(T value)
        {
            if (Mapper.UniqueKey.Count != 1)
                throw new NotSupportedException("Only a single key value is currently supported.");

            var v = Mapper.UniqueKey[0].GetSrceValue(value, OperationTypes.Unspecified);
            var kv = Mapper.UniqueKey[0].ConvertToDestValue(v, OperationTypes.Unspecified);
            var k = kv?.ToString();

            if (string.IsNullOrEmpty(k))
                throw new InvalidOperationException("A key (non null) was unable to be derived from the value.");

            return k;
        }

        /// <summary>
        /// Sets the filter for all operations to ensure authorisation is applied. Applies automatically to all queries, plus create, update, delete and get. Overrides any filter defined using
        /// <see cref="CosmosDbBase.SetAuthorizeFilter{TModel}(string, Func{IQueryable, IQueryable})"/>.
        /// </summary>
        /// <param name="filter">The filter query.</param>
        public void SetAuthorizeFilter(Func<IQueryable, IQueryable> filter)
        {
            AuthorizeFilter = Check.NotNull(filter, nameof(filter));
        }

        /// <summary>
        /// Gets the authorisation filter (see <see cref="SetAuthorizeFilter"/>.
        /// </summary>
        public Func<IQueryable, IQueryable>? AuthorizeFilter { get; private set; }
    }
}