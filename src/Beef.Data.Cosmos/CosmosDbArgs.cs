// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace Beef.Data.Cosmos
{
    /// <summary>
    /// Enables the <b>DocumentDb/CosmosDb</b> arguments capabilities.
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
    }

    /// <summary>
    /// Provides the base <b>DocumentDb/CosmosDb</b> arguments capabilities.
    /// </summary>
    public class CosmosDbArgs : ICosmosDbArgs
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CosmosDbArgs"/> class with the <paramref name="containerId"/> and <paramref name="requestOptions"/>.
        /// </summary>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="partitionKey">The optional <see cref="PartitionKey"/>.</param>
        /// <param name="requestOptions">The optional <see cref="T:ItemRequestOptions"/>.</param>
        /// <returns>The <see cref="CosmosDbArgs"/>.</returns>
        public static CosmosDbArgs Create(string containerId, PartitionKey partitionKey, ItemRequestOptions requestOptions = null)
        {
            return new CosmosDbArgs(containerId, partitionKey, requestOptions);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CosmosDbArgs"/> class with the <paramref name="containerId"/>, <see cref="PagingArgs"/> and <paramref name="requestOptions"/>.
        /// </summary>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="partitionKey">The optional <see cref="PartitionKey"/>.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="requestOptions">The optional <see cref="QueryRequestOptions"/>.</param>
        /// <returns>The <see cref="CosmosDbArgs"/>.</returns>
        public static CosmosDbArgs Create(string containerId, PartitionKey partitionKey, PagingArgs paging, QueryRequestOptions requestOptions = null)
        {
            return new CosmosDbArgs(containerId, partitionKey, paging, requestOptions);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CosmosDbArgs"/> class with the <paramref name="containerId"/>, <see cref="PagingResult"/> and <paramref name="requestOptions"/>.
        /// </summary>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="partitionKey">The optional <see cref="PartitionKey"/>.</param>
        /// <param name="requestOptions">The optional <see cref="QueryRequestOptions"/>.</param>
        /// <returns>The <see cref="CosmosDbArgs"/>.</returns>
        public static CosmosDbArgs Create(string containerId, PartitionKey partitionKey, PagingResult paging, QueryRequestOptions requestOptions = null)
        {
            return new CosmosDbArgs(containerId, partitionKey, paging, requestOptions);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbArgs"/> class.
        /// </summary>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="partitionKey">The optional <see cref="PartitionKey"/>.</param>
        /// <param name="requestOptions">The optional <see cref="T:ItemRequestOptions"/>.</param>
        public CosmosDbArgs(string containerId, PartitionKey partitionKey, ItemRequestOptions requestOptions = null)
        {
            ContainerId = Check.NotEmpty(containerId, nameof(containerId));
            PartitionKey = partitionKey;
            ItemRequestOptions = requestOptions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbArgs"/> class.
        /// </summary>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="partitionKey">The optional <see cref="PartitionKey"/>.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="requestOptions">The optional <see cref="FeedOptions"/>.</param>
        public CosmosDbArgs(string containerId, PartitionKey partitionKey, PagingArgs paging, QueryRequestOptions requestOptions = null) 
            : this(containerId, partitionKey, new PagingResult(Check.NotNull(paging, (nameof(paging)))), requestOptions) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbArgs"/> class.
        /// </summary>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="partitionKey">The optional <see cref="PartitionKey"/>.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="requestOptions">The optional <see cref="FeedOptions"/>.</param>
        public CosmosDbArgs(string containerId, PartitionKey partitionKey, PagingResult paging, QueryRequestOptions requestOptions = null)
        {
            ContainerId = Check.NotEmpty(containerId, nameof(containerId));
            PartitionKey = partitionKey;
            Paging = Check.NotNull(paging, nameof(paging));
            QueryRequestOptions = requestOptions;
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
        /// Indicates that a <c>null</c> is to be returned where the <b>response</b> has a <see cref="HttpStatusCode"/> of <see cref="HttpStatusCode.NotFound"/>.
        /// </summary>
        public bool NullOnNotFoundResponse { get; set; }

        /// <summary>
        /// Indicates whether to the set (override) the identifier on <b>Create</b> where the entity implements <see cref="IGuidIdentifier"/> or <see cref="IStringIdentifier"/>.
        /// </summary>
        /// <remarks>The value will be set using <see cref="System.Guid.NewGuid"/>.</remarks>
        public bool SetIdentifierOnCreate { get; set; } = true;
    }
}