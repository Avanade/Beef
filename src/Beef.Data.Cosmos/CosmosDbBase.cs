﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Beef.Entities;
using Microsoft.Azure.Cosmos;

namespace Beef.Data.Cosmos
{
    /// <summary>
    /// Extends <see cref="CosmosDbBase"/> adding <see cref="Register"/> and <see cref="Default"/> capabilities for <b>CosmosDb/DocumentDb</b>.
    /// </summary>
    /// <typeparam name="TDefault">The <see cref="Default"/> <see cref="Type"/>.</typeparam>
    public abstract class CosmosDb<TDefault> : CosmosDbBase where TDefault : CosmosDb<TDefault>
    {
        private static readonly object _lock = new object();
        private static TDefault _default;
        private static Func<TDefault> _create;

        /// <summary>
        /// Registers the <see cref="Default"/> <see cref="CosmosDbBase"/> instance.
        /// </summary>
        /// <param name="create">Function to create the <see cref="Default"/> instance.</param>
        public static void Register(Func<TDefault> create)
        {
            lock (_lock)
            {
                if (_default != null)
                    throw new InvalidOperationException("The Register method can only be invoked once.");

                _create = create ?? throw new ArgumentNullException(nameof(create));
            }
        }

        /// <summary>
        /// Gets the current default <see cref="CosmosDbBase"/> instance.
        /// </summary>
        public static TDefault Default
        {
            get
            {
                if (_default != null)
                    return _default;

                lock (_lock)
                {
                    if (_default != null)
                        return _default;

                    if (_create == null)
                        throw new InvalidOperationException("The Register method must be invoked before this property can be accessed.");

                    _default = _create() ?? throw new InvalidOperationException("The registered create function must create a default instance.");
                    return _default;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDb{TDefault}"/> class.
        /// </summary>
        /// <param name="client">The <see cref="CosmosClient"/>.</param>
        /// <param name="databaseId">The database identifier.</param>
        /// <param name="createDatabaseIfNotExists">Indicates whether the database shoould be created if it does not exist.</param>
        /// <param name="throughput">The throughput (RU/S).</param>
        protected CosmosDb(CosmosClient client, string databaseId, bool createDatabaseIfNotExists = false, int? throughput = 400) 
            : base(client, databaseId, createDatabaseIfNotExists, throughput) { }
    }

    /// <summary>
    /// Represents the base class for <b>CosmosDb/DocumentDb</b> access; being a lightweight direct <b>CosmosDb/DocumentDb</b> access layer.
    /// </summary>
    public abstract class CosmosDbBase
    {
        private Action<RequestOptions> _updateRequestOptionsAction;
        private Action<QueryRequestOptions> _updateQueryRequestOptionsAction;

        #region Static

        /// <summary>
        /// Transforms and throws the <see cref="IBusinessException"/> equivalent for a <see cref="HttpRequestException"/>.
        /// </summary>
        /// <param name="cex">The <see cref="HttpRequestException"/>.</param>
        public static void ThrowTransformedDocumentClientException(CosmosException cex)
        {
            switch (cex.StatusCode)
            {
                case System.Net.HttpStatusCode.NotFound:
                    throw new NotFoundException(null, cex);

                case System.Net.HttpStatusCode.Conflict:
                    throw new DuplicateException(null, cex);

                case System.Net.HttpStatusCode.PreconditionFailed:
                    throw new ConcurrencyException(null, cex);
            }
        }

        /// <summary>
        /// Gets the <b>value</b> from the response updating any special properties as required.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="resp">The response.</param>
        /// <returns>The response value.</returns>
        public static T GetResponseValue<T>(Response<T> resp)
        {
            if (resp == null)
                return default;

            return GetAndFormatValue(resp.Resource);
        }

        /// <summary>
        /// Gets the <b>value</b> formatting/updating any special properties as required.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The response value.</returns>
        public static T GetAndFormatValue<T>(T value)
        {
            if (value == null)
                return value;

            if (value is IETag etag && etag.ETag != null)
                etag.ETag = (etag.ETag.StartsWith("\"") && etag.ETag.EndsWith("\"")) ? etag.ETag.Substring(1, etag.ETag.Length - 2) : etag.ETag;

            if (value is CosmosDbTypeValue cdv)
                cdv.PrepareAfter();

            return value;
        }

        /// <summary>
        /// Prepares the entity value for a Create by setting the IChangeLog and IIdentifer.
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <param name="setIdentifier">Indicates whether to override the <c>Id</c> where entity implements <see cref="IIdentifier"/>.</param>
        /// <returns>The entity value.</returns>
        internal static void PrepareEntityForCreate(object value, bool setIdentifier)
        {
            if (value is IChangeLog cl)
            {
                cl.ChangeLog = new ChangeLog
                {
                    CreatedBy = ExecutionContext.Current.Username,
                    CreatedDate = ExecutionContext.Current.Timestamp
                };
            }

            if (setIdentifier)
            {
                if (value is IGuidIdentifier gid)
                    gid.Id = Guid.NewGuid();
                else if (value is IStringIdentifier sid)
                    sid.Id = Guid.NewGuid().ToString();
                else
                    throw new InvalidOperationException("An identifier cannot be automatically generated for this Type.");
            }
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbBase"/> class.
        /// </summary>
        /// <param name="client">The <see cref="DocumentClient"/>.</param>
        /// <param name="databaseId">The database identifier.</param>
        /// <param name="createDatabaseIfNotExists">Indicates whether the database shoould be created if it does not exist.</param>
        /// <param name="throughput">The throughput (RU/S).</param>
        protected CosmosDbBase(CosmosClient client, string databaseId, bool createDatabaseIfNotExists = false, int? throughput = 400)
        {
            Client = Check.NotNull(client, nameof(client));
            Database = createDatabaseIfNotExists ?
                Client.CreateDatabaseIfNotExistsAsync(databaseId, throughput).Result.Database :
                Client.GetDatabase(Check.NotEmpty(databaseId, nameof(databaseId)));
        }

        /// <summary>
        /// Gets the underlying <see cref="T:CosmosClient"/>.
        /// </summary>
        public CosmosClient Client { get; private set; }

        /// <summary>
        /// Gets the <see cref="T:Database"/>.
        /// </summary>
        public Database Database { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="CosmosException"/> handler (by default set up to execute <see cref="ThrowTransformedDocumentClientException(CosmosException)"/>).
        /// </summary>
        public Action<CosmosException> ExceptionHandler { get; set; } = (cex) => ThrowTransformedDocumentClientException(cex);

        /// <summary>
        /// Gets the specified <see cref="Container"/>.
        /// </summary>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <returns>The selected <see cref="Container"/>.</returns>
        public Container GetContainer(string containerId) => Database.GetContainer(containerId);

        /// <summary>
        /// Replace or create the <see cref="Container"/> asynchronously.
        /// </summary>
        /// <param name="containerProperties">The <see cref="ContainerProperties"/> used for the create.</param>
        /// <param name="throughput">The throughput (RU/S).</param>
        /// <returns>The replaced/created <see cref="Container"/>.</returns>
        public async Task<Container> ReplaceOrCreateContainerAsync(ContainerProperties containerProperties, int? throughput = 400)
        {
            var container = GetContainer(containerProperties.Id);

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
            return await Database.CreateContainerIfNotExistsAsync(containerProperties, throughput);
        }

        #region RequestOptions

        /// <summary>
        /// Sets the <see cref="Action"/> to update the <see cref="RequestOptions"/> for the selected operation.
        /// </summary>
        /// <param name="updateRequestOptionsAction">The <see cref="Action"/> to update the <see cref="RequestOptions"/>.</param>
        /// <returns>This <see cref="CosmosDbBase"/> instance to support fluent-style method-chaining.</returns>
        public CosmosDbBase RequestOptions(Action<RequestOptions> updateRequestOptionsAction)
        {
            _updateRequestOptionsAction = Check.NotNull(updateRequestOptionsAction, nameof(updateRequestOptionsAction));
            return this;
        }

        /// <summary>
        /// Updates the <paramref name="requestOptions"/> using the <see cref="Action"/> set with <see cref="RequestOptions(Action{RequestOptions})"/>.
        /// </summary>
        /// <param name="requestOptions">The <see cref="T:RequestOptions"/>.</param>
        public void UpdateRequestOptions(RequestOptions requestOptions)
        {
            Check.NotNull(requestOptions, nameof(requestOptions));
            _updateRequestOptionsAction?.Invoke(requestOptions);
        }

        /// <summary>
        /// Gets or instantiates the <see cref="ItemRequestOptions"/>.
        /// </summary>
        /// <typeparam name="T">The entiy <see cref="Type"/>.</typeparam>
        /// <param name="dbArgs">The <see cref="CosmosDbArgs{T}"/>.</param>
        internal protected ItemRequestOptions GetItemRequestOptions<T>(CosmosDbArgs<T> dbArgs = null) where T : class, new()
        {
            var iro = dbArgs != null && dbArgs.ItemRequestOptions != null ? dbArgs.ItemRequestOptions : new ItemRequestOptions();
            UpdateRequestOptions(iro);
            return iro;
        }

        #endregion

        #region QueryRequestOptions

        /// <summary>
        /// Sets the <see cref="Action"/> to update the <see cref="QueryRequestOptions"/> for the selected operation.
        /// </summary>
        /// <param name="updateFeedOptionsAction">The <see cref="Action"/> to update the <see cref="QueryRequestOptions"/>.</param>
        /// <returns>This <see cref="CosmosDbBase"/> instance to support fluent-style method-chaining.</returns>
        public CosmosDbBase QueryRequestOptions(Action<QueryRequestOptions> updateFeedOptionsAction)
        {
            _updateQueryRequestOptionsAction = Check.NotNull(updateFeedOptionsAction, nameof(updateFeedOptionsAction));
            return this;
        }

        /// <summary>
        /// Updates the <paramref name="requestOptions"/> using the <see cref="Action"/> set with <see cref="QueryRequestOptions(Action{QueryRequestOptions})"/>.
        /// </summary>
        /// <param name="requestOptions">The <see cref="T:QueryRequestOptions"/>.</param>
        public void UpdateQueryRequestOptions(QueryRequestOptions requestOptions)
        {
            Check.NotNull(requestOptions, nameof(requestOptions));
            _updateQueryRequestOptionsAction?.Invoke(requestOptions);
        }

        /// <summary>
        /// Gets or instantiates the <see cref="QueryRequestOptions"/>.
        /// </summary>
        /// <typeparam name="T">The entiy <see cref="Type"/>.</typeparam>
        /// <param name="dbArgs">The <see cref="CosmosDbArgs{T}"/>.</param>
        internal protected QueryRequestOptions GetQueryRequestOptions<T>(CosmosDbArgs<T> dbArgs = null) where T : class, new()
        {
            var ro = dbArgs != null && dbArgs.QueryRequestOptions != null ? dbArgs.QueryRequestOptions : new QueryRequestOptions() { MaxConcurrency = 2 };
            UpdateQueryRequestOptions(ro);
            return ro;
        }

        #endregion

        #region Query

        /// <summary>
        /// Creates a <see cref="CosmosDbQuery{T}"/> to enable LINQ-style queries.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="queryArgs">The <see cref="CosmosDbArgs{T}"/>.</param>
        /// <param name="query">The function to perform additional query execution.</param>
        /// <returns>The <see cref="CosmosDbQuery{T}"/>.</returns>
        public CosmosDbQuery<T> Query<T>(CosmosDbArgs<T> queryArgs, Func<IQueryable<T>, IQueryable<T>> query = null) where T : class, IIdentifier, new()
        {
            return new CosmosDbQuery<T>(this, queryArgs, query);
        }

        #endregion

        /// <summary>
        /// Gets the <b>CosmosDb/DocumentDb</b> entity for the specified <paramref name="keys"/> converting to <typeparamref name="T"/> asynchronously.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="getArgs">The <see cref="CosmosDbArgs{T}"/>.</param>
        /// <param name="keys">The key values.</param>
        /// <returns>The entity value where found; otherwise, <c>null</c> (see <see cref="ICosmosDbArgs.NullOnNotFoundResponse"/>).</returns>
        public Task<T> GetAsync<T>(CosmosDbArgs<T> getArgs, params IComparable[] keys) where T : class, IIdentifier, new()
        {
            return new CosmosDbContainer<T>(this, Check.NotNull(getArgs, nameof(getArgs))).GetAsync(keys);
        }

        /// <summary>
        /// Creates the <b>CosmosDb/DocumentDb</b> entity asynchronously.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="CosmosDbArgs{T}"/>.</param>
        /// <param name="value">The value to create.</param>
        /// <returns>The value (re-queried where specified).</returns>
        public Task<T> CreateAsync<T>(CosmosDbArgs<T> saveArgs, T value) where T : class, IIdentifier, new()
        {
            return new CosmosDbContainer<T>(this, Check.NotNull(saveArgs, nameof(saveArgs))).CreateAsync(value);
        }

        /// <summary>
        /// Updates the <b>CosmosDb/DocumentDb</b> entity asynchronously.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="CosmosDbArgs{T}"/>.</param>
        /// <param name="value">The value to create.</param>
        /// <returns>The value (re-queried where specified).</returns>
        public Task<T> UpdateAsync<T>(CosmosDbArgs<T> saveArgs, T value) where T : class, IIdentifier, new()
        {
            return new CosmosDbContainer<T>(this, Check.NotNull(saveArgs, nameof(saveArgs))).UpdateAsync(value);
        }

        /// <summary>
        /// Deletes the <b>CosmosDb/DocumentDb</b> entity asynchronously.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="CosmosDbArgs{T}"/>.</param>
        /// <param name="keys">The key values.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public Task DeleteAsync<T>(CosmosDbArgs<T> saveArgs, params IComparable[] keys) where T : class, IIdentifier, new()
        {
            return new CosmosDbContainer<T>(this, Check.NotNull(saveArgs, nameof(saveArgs))).DeleteAsync(keys);
        }
    }
}