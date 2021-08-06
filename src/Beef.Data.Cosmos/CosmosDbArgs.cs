// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using AutoMapper;
using Beef.Entities;
using Microsoft.Azure.Cosmos;
using System;
using System.Linq;
using System.Net;

namespace Beef.Data.Cosmos
{
    /// <summary>
    /// Provides the <b>CosmosDb/DocumentDb Container</b> arguments capabilities using an <i>AutoMapper</i> <see cref="IMapper"/>.
    /// </summary>
    public class CosmosDbArgs
    {
        /// <summary>
        /// Creates a <see cref="CosmosDbArgs"/> with an <i>AutoMapper</i> <paramref name="mapper"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="partitionKey">The <see cref="PartitionKey"/>.</param>
        /// <param name="requestOptions">The optional <see cref="Microsoft.Azure.Cosmos.ItemRequestOptions"/>.</param>
        /// <param name="onCreate">Optional action to perform additional processing.</param>
        /// <returns>The <see cref="CosmosDbArgs"/>.</returns>
        public static CosmosDbArgs Create(IMapper mapper, string containerId, PartitionKey? partitionKey = null, ItemRequestOptions? requestOptions = null, Action<CosmosDbArgs>? onCreate = null)
        {
            var dbArgs = new CosmosDbArgs(mapper, containerId, partitionKey, requestOptions);
            onCreate?.Invoke(dbArgs);
            return dbArgs;
        }

        /// <summary>
        /// Creates a <see cref="CosmosDbArgs"/> with an <i>AutoMapper</i> <paramref name="mapper"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="partitionKey">The <see cref="PartitionKey"/>.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="requestOptions">The optional <see cref="FeedOptions"/>.</param>
        /// <param name="onCreate">Optional action to perform additional processing.</param>
        /// <returns>The <see cref="CosmosDbArgs"/>.</returns>
        public static CosmosDbArgs Create(IMapper mapper, string containerId, PagingArgs paging, PartitionKey? partitionKey, QueryRequestOptions? requestOptions = null, Action<CosmosDbArgs>? onCreate = null)
        {
            var dbArgs = new CosmosDbArgs(mapper, containerId, paging, partitionKey, requestOptions);
            onCreate?.Invoke(dbArgs);
            return dbArgs;
        }

        /// <summary>
        /// Creates a <see cref="CosmosDbArgs"/> with an <i>AutoMapper</i> <paramref name="mapper"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="partitionKey">The <see cref="Microsoft.Azure.Cosmos.PartitionKey"/>.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="requestOptions">The optional <see cref="FeedOptions"/>.</param>
        /// <param name="onCreate">Optional action to perform additional processing.</param>
        /// <returns>The <see cref="CosmosDbArgs"/>.</returns>
        public static CosmosDbArgs Create(IMapper mapper, string containerId, PagingResult paging, PartitionKey? partitionKey, QueryRequestOptions? requestOptions = null, Action<CosmosDbArgs>? onCreate = null)
        {
            var dbArgs = new CosmosDbArgs(mapper, containerId, paging, partitionKey, requestOptions);
            onCreate?.Invoke(dbArgs);
            return dbArgs;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbArgs"/> class.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="partitionKey">The <see cref="Microsoft.Azure.Cosmos.PartitionKey"/>.</param>
        /// <param name="requestOptions">The optional <see cref="Microsoft.Azure.Cosmos.ItemRequestOptions"/>.</param>
        public CosmosDbArgs(IMapper mapper, string containerId, PartitionKey? partitionKey = null, ItemRequestOptions? requestOptions = null)
        {
            Mapper = Check.NotNull(mapper, nameof(mapper));
            ContainerId = Check.NotEmpty(containerId, nameof(containerId));
            PartitionKey = partitionKey;
            ItemRequestOptions = requestOptions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbArgs"/> class.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="partitionKey">The <see cref="Microsoft.Azure.Cosmos.PartitionKey"/>.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="requestOptions">The optional <see cref="FeedOptions"/>.</param>
        public CosmosDbArgs(IMapper mapper, string containerId, PagingArgs paging, PartitionKey? partitionKey, QueryRequestOptions? requestOptions = null)
            : this(mapper, containerId, new PagingResult(Check.NotNull(paging, (nameof(paging)))), partitionKey, requestOptions) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbArgs"/> class.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="partitionKey">The <see cref="PartitionKey"/>.</param>
        /// <param name="requestOptions">The optional <see cref="FeedOptions"/>.</param>
        public CosmosDbArgs(IMapper mapper, string containerId, PagingResult paging, PartitionKey? partitionKey, QueryRequestOptions? requestOptions = null)
        {
            Mapper = Check.NotNull(mapper, nameof(mapper));
            ContainerId = Check.NotEmpty(containerId, nameof(containerId));
            PartitionKey = partitionKey;
            Paging = Check.NotNull(paging, nameof(paging));
            QueryRequestOptions = requestOptions;
        }

        /// <summary>
        /// Gets the <i>AutoMapper</i> <see cref="IMapper"/>.
        /// </summary>
        public IMapper Mapper { get; }

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
        /// Sets the filter for all operations to ensure authorisation is applied. Applies automatically to all queries, plus create, update, delete and get. Overrides any filter defined using
        /// <see cref="CosmosDbBase.SetAuthorizeFilter{TModel}(string, Func{IQueryable, IQueryable})"/>.
        /// </summary>
        /// <param name="filter">The filter query.</param>
        public void SetAuthorizeFilter(Func<IQueryable, IQueryable> filter) => AuthorizeFilter = Check.NotNull(filter, nameof(filter));

        /// <summary>
        /// Gets the authorisation filter (see <see cref="SetAuthorizeFilter"/>.
        /// </summary>
        public Func<IQueryable, IQueryable>? AuthorizeFilter { get; private set; }
    }
}