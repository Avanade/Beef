// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Beef.Entities;
using Microsoft.Azure.Cosmos;

namespace Beef.Data.Cosmos
{
    /// <summary>
    /// Represents the base class for <b>CosmosDb/DocumentDb</b> access; being a lightweight direct <b>CosmosDb/DocumentDb</b> access layer.
    /// </summary>
    public abstract class CosmosDbBase : ICosmosDb
    {
        private Action<RequestOptions>? _updateRequestOptionsAction;
        private Action<QueryRequestOptions>? _updateQueryRequestOptionsAction;
        private readonly ConcurrentDictionary<Key, Func<IQueryable, IQueryable>> _filters = new ConcurrentDictionary<Key, Func<IQueryable, IQueryable>>();

        private struct Key
        {
            public Key(Type modelType, string containerId)
            {
                ModelType = modelType;
                ContainerId = containerId;
            }

            public Type ModelType { get; set; }

            public string ContainerId { get; set; }
        }

        #region Static

        /// <summary>
        /// Transforms and throws the <see cref="IBusinessException"/> equivalent for a <see cref="HttpRequestException"/>.
        /// </summary>
        /// <param name="cex">The <see cref="HttpRequestException"/>.</param>
        public static void ThrowTransformedDocumentClientException(CosmosException cex)
        {
            if (cex == null)
                throw new ArgumentNullException(nameof(cex));

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
        /// Reformats the <see cref="IETag.ETag"/> for the value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <remarks>The CosmosDB ETag value is formatted with a leading and trailing double-quote which are removed to enable consistency of handling within the <i>Beef</i> pipeline.</remarks>
        public static void ReformatValueETag(object value)
        {
            if (value is IETag etag && etag.ETag != null)
                etag.ETag = (etag.ETag.StartsWith("\"", StringComparison.InvariantCultureIgnoreCase) && etag.ETag.EndsWith("\"", StringComparison.InvariantCultureIgnoreCase)) ? etag.ETag[1..^1] : etag.ETag;
        }

        /// <summary>
        /// Prepares the entity value for a 'Create' by setting the IChangeLog.
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <returns>The entity value.</returns>
        internal static void PrepareEntityForCreate(object? value)
        {
            if (value == null)
                return;
            
            if (value is IChangeLog cl)
            {
                if (cl.ChangeLog == null)
                    cl.ChangeLog = new ChangeLog();

                cl.ChangeLog.CreatedBy = ExecutionContext.HasCurrent ? ExecutionContext.Current.Username : ExecutionContext.EnvironmentUsername;
                cl.ChangeLog.CreatedDate = ExecutionContext.HasCurrent ? ExecutionContext.Current.Timestamp : Cleaner.Clean(DateTime.Now);
            }
        }

        /// <summary>
        /// Prepares the entity value for a 'Update' by setting the IChangeLog.
        /// </summary>
        /// <param name="value">The entity value.</param>
        internal static void PrepareEntityForUpdate(object? value)
        {
            if (value == null)
                return;

            if (value is IChangeLog cl)
            {
                if (cl.ChangeLog == null)
                    cl.ChangeLog = new ChangeLog();

                cl.ChangeLog.UpdatedBy = ExecutionContext.HasCurrent ? ExecutionContext.Current.Username : ExecutionContext.EnvironmentUsername;
                cl.ChangeLog.UpdatedDate = ExecutionContext.HasCurrent ? ExecutionContext.Current.Timestamp : Cleaner.Clean(DateTime.Now);
            }
        }

        /// <summary>
        /// Gets the <b>CosmosDb/DocumentDb</b> key from the specified keys.
        /// </summary>
        /// <param name="keys">The key values.</param>
        /// <returns>The <b>CosmosDb/DocumentDb</b> key.</returns>
        public static string GetCosmosKey(IComparable?[] keys)
        {
            if (keys == null || keys.Length == 0)
                throw new ArgumentNullException(nameof(keys));

            if (keys.Length != 1)
                throw new NotSupportedException("Only a single key value is currently supported.");

            var k = keys[0]?.ToString();
            if (string.IsNullOrEmpty(k))
                throw new InvalidOperationException("A key (non null) was unable to be derived from the value.");

            return k;
        }

        /// <summary>
        /// Gets the <b>CosmosDb/DocumentDb</b> key from the entity value.
        /// </summary>
        /// <param name="value">The entity value.</param>in
        /// <returns>The <b>CosmosDb/DocumentDb</b> key.</returns>
        public static string GetCosmosKey(object value) => value switch
        {
            IStringIdentifier si => si.Id!,
            IGuidIdentifier gi => gi.Id.ToString(),
            IInt32Identifier ii => ii.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
            IInt64Identifier ii => ii.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
            IUniqueKey uk => uk.UniqueKey.Args.Length == 1 ? uk.UniqueKey.Args[0]?.ToString()! : throw new NotSupportedException("Only a single key value is currently supported."),
            _ => throw new NotSupportedException($"Value Type must be {nameof(IStringIdentifier)}, {nameof(IGuidIdentifier)}, {nameof(IInt32Identifier)}, {nameof(IInt64Identifier)}, or {nameof(IUniqueKey)}."),
        };

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CosmosDbBase"/> class.
        /// </summary>
        /// <param name="client">The <see cref="DocumentClient"/>.</param>
        /// <param name="databaseId">The database identifier.</param>
        /// <param name="createDatabaseIfNotExists">Indicates whether the database shoould be created if it does not exist.</param>
        /// <param name="throughput">The throughput (RU/S).</param>
        /// <param name="invoker">Enables the <see cref="Invoker"/> to be overridden; defaults to <see cref="CosmosDbInvoker"/>.</param>
        protected CosmosDbBase(CosmosClient client, string databaseId, bool createDatabaseIfNotExists = false, int? throughput = 400, CosmosDbInvoker? invoker = null)
        {
            Client = Check.NotNull(client, nameof(client));
            Database = createDatabaseIfNotExists ?
                Client.CreateDatabaseIfNotExistsAsync(databaseId, throughput ?? 400).Result.Database :
                Client.GetDatabase(Check.NotEmpty(databaseId, nameof(databaseId)));

            Invoker = invoker ?? new CosmosDbInvoker();
        }

        /// <summary>
        /// Gets the underlying <see cref="Microsoft.Azure.Cosmos.CosmosClient"/>.
        /// </summary>
        public CosmosClient Client { get; private set; }

        /// <summary>
        /// Gets the <see cref="Microsoft.Azure.Cosmos.Database"/>.
        /// </summary>
        public Database Database { get; private set; }

        /// <summary>
        /// Gets the <see cref="CosmosDbInvoker"/>.
        /// </summary>
        public CosmosDbInvoker Invoker { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="CosmosException"/> handler (by default set up to execute <see cref="ThrowTransformedDocumentClientException(CosmosException)"/>).
        /// </summary>
        public Action<CosmosException> ExceptionHandler { get; set; } = (cex) => ThrowTransformedDocumentClientException(cex);

        /// <summary>
        /// Gets the specified <see cref="Container"/>.
        /// </summary>
        /// <param name="containerId">The <see cref="Container"/> identifier.</param>
        /// <returns>The selected <see cref="Container"/>.</returns>
        public Container CosmosContainer(string containerId) => Database.GetContainer(containerId);

        /// <summary>
        /// Gets (creates) the <see cref="CosmosDbContainer{T, TModel}"/> using the specified <paramref name="dbArgs"/>.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The cosmos model <see cref="Type"/>.</typeparam>
        /// <param name="dbArgs">The <see cref="CosmosDbArgs"/>.</param>
        /// <returns>The <see cref="CosmosDbContainer{T, TModel}"/>.</returns>
        public CosmosDbContainer<T, TModel> Container<T, TModel>(CosmosDbArgs dbArgs) where T : class, new() where TModel : class, new()
            => new CosmosDbContainer<T, TModel>(this, dbArgs);

        /// <summary>
        /// Gets (creates) the <see cref="CosmosDbValueContainer{T, TModel}"/> using the specified <paramref name="dbArgs"/>.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The cosmos model <see cref="Type"/>.</typeparam>
        /// <param name="dbArgs">The <see cref="CosmosDbArgs"/>.</param>
        /// <returns>The <see cref="CosmosDbValueContainer{T, TModel}"/>.</returns>
        public CosmosDbValueContainer<T, TModel> ValueContainer<T, TModel>(CosmosDbArgs dbArgs) where T : class, new() where TModel : class, new()
            => new CosmosDbValueContainer<T, TModel>(this, dbArgs);

        /// <summary>
        /// Replace or create the <see cref="Container"/> asynchronously.
        /// </summary>
        /// <param name="containerProperties">The <see cref="ContainerProperties"/> used for the create.</param>
        /// <param name="throughput">The throughput (RU/S).</param>
        /// <returns>The replaced/created <see cref="Container"/>.</returns>
        public async Task<Container> ReplaceOrCreateContainerAsync(ContainerProperties containerProperties, int? throughput = 400)
        {
            if (containerProperties == null)
                throw new ArgumentNullException(nameof(containerProperties));

            var container = CosmosContainer(containerProperties.Id);

            // Remove existing container if it already exists.
            try
            {
                await container.DeleteContainerAsync().ConfigureAwait(false);
            }
            catch (CosmosException cex)
            {
                if (cex.StatusCode != HttpStatusCode.NotFound)
                    throw;
            }

            // Create the container as specified.
            return await Database.CreateContainerIfNotExistsAsync(containerProperties, throughput).ConfigureAwait(false);
        }

        #region AuthorizeFilter

        /// <summary>
        /// Sets the filter for all operations performed on the <typeparamref name="TModel"/> for the specified <paramref name="containerId"/> to ensure authorisation is applied. Applies automatically 
        /// to all queries, plus create, update, delete and get operations. Can be overridden for a specific instance using the <see cref="CosmosDbArgs.SetAuthorizeFilter(Func{IQueryable, IQueryable})"/>.
        /// </summary>
        /// <typeparam name="TModel">The model <see cref="Type"/> persisted within the container.</typeparam>
        /// <param name="containerId">The <see cref="Microsoft.Azure.Cosmos.Container"/> identifier.</param>
        /// <param name="filter">The filter query.</param>
        public void SetAuthorizeFilter<TModel>(string containerId, Func<IQueryable, IQueryable> filter)
        {
            if (!_filters.TryAdd(new Key(typeof(TModel), Check.NotEmpty(containerId, nameof(containerId))), Check.NotNull(filter, nameof(filter))))
                throw new InvalidOperationException("A filter cannot be overridden; it must be removed (RemoveAuthorizeFilter) then set (SetAuthorizeFilter).");
        }

        /// <summary>
        /// Gets the filter.
        /// </summary>
        /// <typeparam name="TModel">The model <see cref="Type"/> persisted within the container.</typeparam>
        /// <param name="containerId">The <see cref="Microsoft.Azure.Cosmos.Container"/> identifier.</param>
        /// <returns>The filter query where found; otherwise, <c>null</c>.</returns>
        public Func<IQueryable, IQueryable>? GetAuthorizeFilter<TModel>(string containerId)
        {
            if (_filters.TryGetValue(new Key(typeof(TModel), Check.NotEmpty(containerId, nameof(containerId))), out var filter))
                return filter;
            else
                return null;
        }

        /// <summary>
        /// Removes the specified filter.
        /// </summary>
        /// <typeparam name="TModel">The model <see cref="Type"/> persisted within the container.</typeparam>
        /// <param name="containerId">The <see cref="Microsoft.Azure.Cosmos.Container"/> identifier.</param>
        public void RemoveAuthorizeFilter<TModel>(string containerId)
        {
            _filters.TryRemove(new Key(typeof(TModel), Check.NotEmpty(containerId, nameof(containerId))), out _);
        }

        #endregion

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
        /// <param name="requestOptions">The <see cref="Microsoft.Azure.Cosmos.RequestOptions"/>.</param>
        public void UpdateRequestOptions(RequestOptions requestOptions)
        {
            Check.NotNull(requestOptions, nameof(requestOptions));
            _updateRequestOptionsAction?.Invoke(requestOptions);
        }

        /// <summary>
        /// Gets or instantiates the <see cref="ItemRequestOptions"/>.
        /// </summary>
        /// <typeparam name="T">The entiy <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The cosmos model <see cref="Type"/>.</typeparam>
        /// <param name="dbArgs">The <see cref="CosmosDbArgs"/>.</param>
        internal protected ItemRequestOptions GetItemRequestOptions<T, TModel>(CosmosDbArgs? dbArgs = null) where T : class, new() where TModel : class, new()
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
        /// <param name="requestOptions">The <see cref="Microsoft.Azure.Cosmos.QueryRequestOptions"/>.</param>
        public void UpdateQueryRequestOptions(QueryRequestOptions requestOptions)
        {
            Check.NotNull(requestOptions, nameof(requestOptions));
            _updateQueryRequestOptionsAction?.Invoke(requestOptions);
        }

        /// <summary>
        /// Gets or instantiates the <see cref="QueryRequestOptions"/>.
        /// </summary>
        /// <typeparam name="T">The entiy <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The cosmos model <see cref="Type"/>.</typeparam>
        /// <param name="dbArgs">The <see cref="CosmosDbArgs"/>.</param>
        internal protected QueryRequestOptions GetQueryRequestOptions<T, TModel>(CosmosDbArgs dbArgs) where T : class, new() where TModel : class, new()
        {
            if (dbArgs == null)
                throw new ArgumentNullException(nameof(dbArgs));

            var ro = dbArgs != null && dbArgs.QueryRequestOptions != null ? dbArgs.QueryRequestOptions : new QueryRequestOptions() { PartitionKey = dbArgs!.PartitionKey };
            UpdateQueryRequestOptions(ro);
            return ro;
        }

        #endregion

        #region Query

        /// <summary>
        /// Gets (creates) a <see cref="CosmosDbQuery{T, TModel}"/> to enable LINQ-style queries.
        /// </summary>
        /// <param name="dbArgs">The <see cref="CosmosDbArgs"/>.</param>
        /// <param name="query">The function to perform additional query execution.</param>
        /// <returns>The <see cref="CosmosDbQuery{T, TModel}"/>.</returns>
        public CosmosDbQuery<T, TModel> Query<T, TModel>(CosmosDbArgs dbArgs, Func<IQueryable<TModel>, IQueryable<TModel>>? query = null) where T : class, new() where TModel : class, new() =>
            Container<T, TModel>(dbArgs).Query(query);

        /// <summary>
        /// Gets (creates) a <see cref="CosmosDbValueQuery{T, TModel}"/> to enable LINQ-style queries.
        /// </summary>
        /// <param name="dbArgs">The <see cref="CosmosDbArgs"/>.</param>
        /// <param name="query">The function to perform additional query execution.</param>
        /// <returns>The <see cref="CosmosDbValueQuery{T, TModel}"/>.</returns>
        public CosmosDbValueQuery<T, TModel> ValueQuery<T, TModel>(CosmosDbArgs dbArgs, Func<IQueryable<CosmosDbValue<TModel>>, IQueryable<CosmosDbValue<TModel>>>? query = null) where T : class, new() where TModel : class, new() =>
            ValueContainer<T, TModel>(dbArgs).Query(query);

        #endregion
    }
}