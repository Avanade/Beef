// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Microsoft.Azure.Cosmos;
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
        /// Gets the <see cref="Container"/> identifier.
        /// </summary>
        string ContainerId { get; }

        /// <summary>
        /// Gets the <see cref="T:PartitionKey"/>.
        /// </summary>
        PartitionKey PartitionKey { get; }

        /// <summary>
        /// Gets the <see cref="PagingResult"/>.
        /// </summary>
        PagingResult Paging { get; }

        /// <summary>
        /// Indicates that a <c>null</c> is to be returned where the <b>response</b> has a <see cref="HttpStatusCode"/> of <see cref="HttpStatusCode.NotFound"/>.
        /// </summary>
        bool NullOnNotFoundResponse { get; }

        /// <summary>
        /// Indicates whether to the set (override) the identifier on <b>Create</b> where the entity implements <see cref="IGuidIdentifier"/> or <see cref="IStringIdentifier"/>.
        /// </summary>
        bool SetIdentifierOnCreate { get; }

        /// <summary>
        /// Gets the <see cref="T:RequestOptions"/>.
        /// </summary>
        ItemRequestOptions ItemRequestOptions { get; }

        /// <summary>
        /// Gets the <see cref="T:QueryRequestOptions"/>.
        /// </summary>
        QueryRequestOptions QueryRequestOptions { get; }

        /// <summary>
        /// Indicates whether the entity <see cref="System.Type"/> inherits from <see cref="CosmosDbTypeValue"/>.
        /// </summary>
        bool IsTypeValue { get; }

        /// <summary>
        /// Gets the <see cref="CosmosDbTypeValue"/> <see cref="System.Type"/> (see <see cref="ICosmosDbArgs.IsTypeValue"/>). Where the <b>Type</b> is explicitly constructed from the generic
        /// <see cref="CosmosDbTypeValue{T}"/> at runtime then the <see cref="CosmosDbTypeValue{T}.Value"/> <b>Type</b> is used; otherwise, the primary object <b>Type</b> itself is used.
        /// </summary>
        string TypeValueType { get; }
    }

    /// <summary>
    /// Provides the base <b>CosmosDb/DocumentDb Container</b> arguments capabilities.
    /// </summary>
    public class CosmosDbArgs<T> : ICosmosDbArgs where T : class, new()
    {
        private readonly bool _isTypeValue;
        private readonly string _typeValueType;

        /// <summary>
        /// Creates a new instance of the <see cref="CosmosDbArgs{T}"/> class with the <paramref name="containerId"/>, <paramref name="partitionKey"/> and <paramref name="requestOptions"/>.
        /// </summary>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="partitionKey">The optional <see cref="PartitionKey"/> (defaults to <see cref="PartitionKey.None"/>).</param>
        /// <param name="requestOptions">The optional <see cref="T:ItemRequestOptions"/>.</param>
        /// <returns>The <see cref="CosmosDbArgs{T}"/>.</returns>
        public static CosmosDbArgs<T> Create(string containerId, PartitionKey? partitionKey = null, ItemRequestOptions requestOptions = null)
        {
            return new CosmosDbArgs<T>(containerId, partitionKey == null ? PartitionKey.None : partitionKey.Value, requestOptions);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CosmosDbArgs{T}"/> class with the <paramref name="containerId"/>, <paramref name="partitionKey"/>, <see cref="PagingArgs"/> and <paramref name="requestOptions"/>.
        /// </summary>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="partitionKey">The optional <see cref="PartitionKey"/>.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="requestOptions">The optional <see cref="QueryRequestOptions"/>.</param>
        /// <returns>The <see cref="CosmosDbArgs{T}"/>.</returns>
        public static CosmosDbArgs<T> Create(string containerId, PartitionKey partitionKey, PagingArgs paging, QueryRequestOptions requestOptions = null)
        {
            return new CosmosDbArgs<T>(containerId, partitionKey, paging, requestOptions);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CosmosDbArgs{T}"/> class with the <paramref name="containerId"/>, <see cref="PartitionKey.None"/>, <see cref="PagingArgs"/> and <paramref name="requestOptions"/>.
        /// </summary>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="requestOptions">The optional <see cref="QueryRequestOptions"/>.</param>
        /// <returns>The <see cref="CosmosDbArgs{T}"/>.</returns>
        public static CosmosDbArgs<T> Create(string containerId, PagingArgs paging, QueryRequestOptions requestOptions = null)
        {
            return new CosmosDbArgs<T>(containerId, PartitionKey.None, paging, requestOptions);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CosmosDbArgs{T}"/> class with the <paramref name="containerId"/>, <paramref name="partitionKey"/>, <see cref="PagingResult"/> and <paramref name="requestOptions"/>.
        /// </summary>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="partitionKey">The optional <see cref="PartitionKey"/>.</param>
        /// <param name="requestOptions">The optional <see cref="QueryRequestOptions"/>.</param>
        /// <returns>The <see cref="CosmosDbArgs{T}"/>.</returns>
        public static CosmosDbArgs<T> Create(string containerId, PartitionKey partitionKey, PagingResult paging, QueryRequestOptions requestOptions = null)
        {
            return new CosmosDbArgs<T>(containerId, partitionKey, paging, requestOptions);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CosmosDbArgs{T}"/> class with the <paramref name="containerId"/>, <see cref="PartitionKey.None"/>, <see cref="PagingResult"/> and <paramref name="requestOptions"/>.
        /// </summary>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="requestOptions">The optional <see cref="QueryRequestOptions"/>.</param>
        /// <returns>The <see cref="CosmosDbArgs{T}"/>.</returns>
        public static CosmosDbArgs<T> Create(string containerId, PagingResult paging, QueryRequestOptions requestOptions = null)
        {
            return new CosmosDbArgs<T>(containerId, PartitionKey.None, paging, requestOptions);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbArgs{T}"/> class.
        /// </summary>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="partitionKey">The <see cref="PartitionKey"/>.</param>
        /// <param name="requestOptions">The optional <see cref="T:ItemRequestOptions"/>.</param>
        public CosmosDbArgs(string containerId, PartitionKey partitionKey, ItemRequestOptions requestOptions = null)
        {
            ContainerId = Check.NotEmpty(containerId, nameof(containerId));
            PartitionKey = partitionKey;
            ItemRequestOptions = requestOptions;

            var t = typeof(T);
            _isTypeValue = typeof(CosmosDbTypeValue).IsAssignableFrom(t);
            _typeValueType = _isTypeValue ?
                t.IsGenericType && t.BaseType == typeof(CosmosDbTypeValue) ? t.GetGenericArguments().First().Name : t.Name 
                : null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbArgs{T}"/> class.
        /// </summary>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="partitionKey">The <see cref="PartitionKey"/>.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="requestOptions">The optional <see cref="FeedOptions"/>.</param>
        public CosmosDbArgs(string containerId, PartitionKey partitionKey, PagingArgs paging, QueryRequestOptions requestOptions = null) 
            : this(containerId, partitionKey, new PagingResult(Check.NotNull(paging, (nameof(paging)))), requestOptions) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbArgs{T}"/> class.
        /// </summary>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="partitionKey">The <see cref="PartitionKey"/>.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="requestOptions">The optional <see cref="FeedOptions"/>.</param>
        public CosmosDbArgs(string containerId, PartitionKey partitionKey, PagingResult paging, QueryRequestOptions requestOptions = null)
        {
            ContainerId = Check.NotEmpty(containerId, nameof(containerId));
            PartitionKey = partitionKey;
            Paging = Check.NotNull(paging, nameof(paging));
            QueryRequestOptions = requestOptions;

            var t = typeof(T);
            _isTypeValue = typeof(CosmosDbTypeValue).IsAssignableFrom(t);
            _typeValueType = _isTypeValue ?
                t.IsGenericType && t.BaseType == typeof(CosmosDbTypeValue) ? t.GetGenericArguments().First().Name : t.Name
                : null;
        }

        /// <summary>
        /// Gets the <see cref="Container"/> identifier.
        /// </summary>
        public string ContainerId { get; private set; }

        /// <summary>
        /// Gets the <see cref="T:PartitionKey"/>.
        /// </summary>
        public PartitionKey PartitionKey { get; private set; }

        /// <summary>
        /// Gets the <see cref="PagingResult"/> (where paging is required for a <b>query</b>).
        /// </summary>
        public PagingResult Paging { get; private set; }

        /// <summary>
        /// Gets the <see cref="T:ItemRequestOptions"/> used for <b>Get</b>, <b>Create</b>, <b>Update</b>, and <b>Delete</b> (<seealso cref="QueryRequestOptions"/>).
        /// </summary>
        public ItemRequestOptions ItemRequestOptions { get; private set; }

        /// <summary>
        /// Gets the <see cref="T:QueryRequestOptions"/> used for <b>Query</b> (<seealso cref="ItemRequestOptions"/>).
        /// </summary>
        public QueryRequestOptions QueryRequestOptions { get; private set; }

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
        /// Indicates whether the entity <see cref="System.Type"/> inherits from <see cref="CosmosDbTypeValue"/>.
        /// </summary>
        bool ICosmosDbArgs.IsTypeValue => _isTypeValue;

        /// <summary>
        /// Gets the <see cref="CosmosDbTypeValue"/> <see cref="System.Type"/> (see <see cref="ICosmosDbArgs.IsTypeValue"/>). Where the <b>Type</b> is explicitly constructed from the generic
        /// <see cref="CosmosDbTypeValue{T}"/> at runtime then the <see cref="CosmosDbTypeValue{T}.Value"/> <b>Type</b> is used; otherwise, the primary object <b>Type</b> itself is used.
        /// </summary>
        string ICosmosDbArgs.TypeValueType => _typeValueType;
    }
}