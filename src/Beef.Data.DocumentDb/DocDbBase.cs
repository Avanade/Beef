// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Beef.Entities;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;

namespace Beef.Data.DocumentDb
{
    /// <summary>
    /// Extends <see cref="DocDbBase"/> adding <see cref="Register"/> and <see cref="Default"/> capabilities for <b>DocumentDb/CosmosDb</b>.
    /// </summary>
    /// <typeparam name="TDefault">The <see cref="Default"/> <see cref="Type"/>.</typeparam>
    public abstract class DocDb<TDefault> : DocDbBase where TDefault : DocDb<TDefault>
    {
        private static readonly object _lock = new object();
        private static TDefault _default;
        private static Func<TDefault> _create;

        /// <summary>
        /// Registers the <see cref="Default"/> <see cref="DocDbBase"/> instance.
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
        /// Gets the current default <see cref="DocDbBase"/> instance.
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
        /// Initializes a new instance of the <see cref="DocDb{TDefault}"/> class.
        /// </summary>
        /// <param name="client">The <see cref="DocumentClient"/>.</param>
        /// <param name="databaseId">The database identifier.</param>
        public DocDb(DocumentClient client, string databaseId) : base(client, databaseId) { }
    }

    /// <summary>
    /// Represents the base class for <b>DocumentDb/CosmosDb</b> access; being a lightweight direct <b>DocumentDb/CosmosDb</b> access layer.
    /// </summary>
    public abstract class DocDbBase
    {
        private Action<RequestOptions> _updateRequestOptionsAction;
        private Action<FeedOptions> _updateFeedOptionsAction;

        /// <summary>
        /// Transforms and throws the <see cref="IBusinessException"/> equivalent for a <see cref="HttpRequestException"/>.
        /// </summary>
        /// <param name="dcex">The <see cref="HttpRequestException"/>.</param>
        public static void ThrowTransformedDocumentClientException(DocumentClientException dcex)
        {
            // TODO: Add exception logic.
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocDbBase"/> class.
        /// </summary>
        /// <param name="client">The <see cref="DocumentClient"/>.</param>
        /// <param name="databaseId">The database identifier.</param>
        public DocDbBase(DocumentClient client, string databaseId)
        {
            Client = Check.NotNull(client, nameof(client));
            DatabaseId = Check.NotEmpty(databaseId, nameof(databaseId));
        }

        /// <summary>
        /// Gets the underlying <see cref="DocumentClient"/>.
        /// </summary>
        public DocumentClient Client { get; private set; }

        /// <summary>
        /// Gets the database identifier.
        /// </summary>
        public string DatabaseId { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="HttpRequestException"/> handler (by default set up to execute <see cref="ThrowTransformedDocumentClientException(DocumentClientException)"/>).
        /// </summary>
        public Action<DocumentClientException> ExceptionHandler { get; set; } = (dcex) => ThrowTransformedDocumentClientException(dcex);

        #region RequestOptions

        /// <summary>
        /// Sets the <see cref="Action"/> to update the <see cref="RequestOptions"/> for the selected operation.
        /// </summary>
        /// <param name="updateRequestOptionsAction">The <see cref="Action"/> to update the <see cref="RequestOptions"/>.</param>
        /// <returns>This <see cref="DocDbBase"/> instance to support fluent-style method-chaining.</returns>
        public DocDbBase RequestOptions(Action<RequestOptions> updateRequestOptionsAction)
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
        /// Gets or instantiates the <see cref="RequestOptions"/>.
        /// </summary>
        protected RequestOptions GetRequestOptions(DocDbArgs dbArgs = null)
        {
            var ro = dbArgs != null && dbArgs.RequestOptions != null ? dbArgs.RequestOptions : new RequestOptions();
            UpdateRequestOptions(ro);
            return ro;
        }

        #endregion

        #region FeedOptions

        /// <summary>
        /// Sets the <see cref="Action"/> to update the <see cref="FeedOptions"/> for the selected operation.
        /// </summary>
        /// <param name="updateFeedOptionsAction">The <see cref="Action"/> to update the <see cref="FeedOptions"/>.</param>
        /// <returns>This <see cref="DocDbBase"/> instance to support fluent-style method-chaining.</returns>
        public DocDbBase FeedOptions(Action<FeedOptions> updateFeedOptionsAction)
        {
            _updateFeedOptionsAction = Check.NotNull(updateFeedOptionsAction, nameof(updateFeedOptionsAction));
            return this;
        }

        /// <summary>
        /// Updates the <paramref name="feedOptions"/> using the <see cref="Action"/> set with <see cref="FeedOptions(Action{FeedOptions})"/>.
        /// </summary>
        /// <param name="feedOptions">The <see cref="T:FeedOptions"/>.</param>
        public void UpdateFeedOptions(FeedOptions feedOptions)
        {
            Check.NotNull(feedOptions, nameof(feedOptions));
            _updateFeedOptionsAction?.Invoke(feedOptions);
        }

        /// <summary>
        /// Gets or instantiates the <see cref="FeedOptions"/>.
        /// </summary>
        internal protected FeedOptions GetFeedOptions(DocDbArgs dbArgs = null)
        {
            var ro = dbArgs != null && dbArgs.FeedOptions != null ? dbArgs.FeedOptions : new FeedOptions();
            UpdateFeedOptions(ro);
            return ro;
        }

        #endregion

        /// <summary>
        /// Creates the value from the response asynchronously.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="document">The <see cref="ResourceResponse{Document}"/></param>
        /// <returns>The corresponding value.</returns>
        protected async Task<T> CreateValueFromResponseAsync<T>(ResourceResponse<Document> document) where T : class, new()
        {
            Check.NotNull(document, nameof(document));

            if (document.Resource == null)
                return default(T);

            using (var ms = new MemoryStream())
            using (var reader = new StreamReader(ms))
            {
                document.Resource.SaveTo(ms);
                ms.Position = 0;
                return ETagReformatter<T>(JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync()));
            }
        }

        /// <summary>
        /// Reformats the <see cref="IETag.ETag"/> removing the quotes.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The updated value.</returns>
        public T ETagReformatter<T>(T value)
        {
            if (value == null)
                return value;

            if (value is IETag etag)
                etag.ETag = etag.ETag == null ? null :
                    (etag.ETag.StartsWith("\"") && etag.ETag.EndsWith("\"")) ? etag.ETag.Substring(1, etag.ETag.Length - 2) : etag.ETag;

            return value;
        }

        #region Query

        /// <summary>
        /// Creates a <see cref="DocDbQuery{T}"/> to enable LINQ-style queries.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="queryArgs">The <see cref="DocDbArgs"/>.</param>
        /// <param name="query"></param>
        /// <returns></returns>
        public DocDbQuery<T> Query<T>(DocDbArgs queryArgs, Func<IOrderedQueryable<T>, IQueryable<T>> query = null) where T : class, new()
        {
            return new DocDbQuery<T>(this, queryArgs, query);
        }

        #endregion

        #region Get

        /// <summary>
        /// Gets the <b>DocumentDb/CosmosDb</b> entity for the specified <paramref name="keys"/> converting to <typeparamref name="T"/> asynchronously.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="getArgs">The <see cref="DocDbArgs"/>.</param>
        /// <param name="keys">The key values.</param>
        /// <returns>The entity value where found; otherwise, <c>null</c>.</returns>
        public async Task<T> GetAsync<T>(DocDbArgs getArgs, params IComparable[] keys) where T : class, new()
        {
            Check.NotNull(getArgs, nameof(getArgs));
            if (keys.Length != 1)
                throw new NotSupportedException("Only a single key value is currently supported.");

            return await DocDbInvoker.Default.InvokeAsync(this, async () =>
            {
                try
                {
                    var value = await Client.ReadDocumentAsync<T>(UriFactory.CreateDocumentUri(DatabaseId, getArgs.CollectionId, keys[0].ToString()), GetRequestOptions(getArgs));
                    return ETagReformatter(value);
                }
                catch (DocumentClientException dcex)
                {
                    if (dcex.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return null;

                    throw;
                }
            }, this);
        }

        #endregion

        #region Create

        /// <summary>
        /// Creates the <b>DocumentDb/CosmosDb</b> entity asynchronously.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="DocDbArgs"/>.</param>
        /// <param name="value">The value to create.</param>
        /// <returns>The value (re-queried where specified).</returns>
        public async Task<T> CreateAsync<T>(DocDbArgs saveArgs, T value) where T : class, new()
        {
            Check.NotNull(saveArgs, nameof(saveArgs));
            Check.NotNull(value, nameof(value));

            return await DocDbInvoker.Default.InvokeAsync(this, async () =>
            {
                if (value is IChangeLog cl)
                {
                    if (cl.ChangeLog == null)
                        cl.ChangeLog = new ChangeLog();

                    cl.ChangeLog.CreatedBy = ExecutionContext.Current.Username;
                    cl.ChangeLog.CreatedDate = ExecutionContext.Current.Timestamp;
                    cl.ChangeLog.UpdatedBy = cl.ChangeLog.UpdatedBy = null;
                }

                var document = await Client.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, saveArgs.CollectionId), value, GetRequestOptions(saveArgs));
                return await CreateValueFromResponseAsync<T>(document);
            }, this);
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates the <b>DocumentDb/CosmosDb</b> entity asynchronously.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="DocDbArgs"/>.</param>
        /// <param name="value">The value to create.</param>
        /// <returns>The value (re-queried where specified).</returns>
        public async Task<T> UpdateAsync<T>(DocDbArgs saveArgs, T value) where T : class, new()
        {
            Check.NotNull(saveArgs, nameof(saveArgs));
            Check.NotNull(value, nameof(value));

            return await DocDbInvoker.Default.InvokeAsync(this, async () =>
            {
                // Where supporting etag then use IfMatch for concurreny.
                var ro = GetRequestOptions(saveArgs);
                if (ro.AccessCondition == null && value is IETag etag)
                    ro.AccessCondition = new AccessCondition { Condition = etag.ETag.StartsWith("\"") ? etag.ETag : "\"" + etag.ETag + "\"", Type = AccessConditionType.IfMatch };

                string key = (value is IUniqueKey uk && uk.HasUniqueKey && uk.UniqueKey.Args.Length == 1) ? uk.UniqueKey.Args[0].ToString() :
                    throw new InvalidOperationException("Entity must implement IUniqueKey with a UniqueKey (HasUniqueKey); the UniqueKey.Args must contain only a single value (length of 1).");

                // Need to read the record to get access to the ChangeLog readonly fields.
                if (value is IChangeLog cl)
                {
                    if (cl.ChangeLog == null)
                        cl.ChangeLog = new ChangeLog();

                    var rdocument = await Client.ReadDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, saveArgs.CollectionId, key), ro);
                    cl.ChangeLog.CreatedBy = rdocument.Resource.GetPropertyValue<string>("createdBy");
                    cl.ChangeLog.CreatedDate = rdocument.Resource.GetPropertyValue<DateTime?>("createdDate");
                    cl.ChangeLog.UpdatedBy = ExecutionContext.Current.Username;
                    cl.ChangeLog.UpdatedDate = ExecutionContext.Current.Timestamp;

                    ro.SessionToken = rdocument.SessionToken;
                }

                // Replace/Update the value.
                var document = await Client.ReplaceDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, saveArgs.CollectionId, key), value, ro);
                return await CreateValueFromResponseAsync<T>(document);
            }, this);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Deletes the <b>DocumentDb/CosmosDb</b> entity asynchronously.
        /// </summary>
        /// <param name="saveArgs">The <see cref="DocDbArgs"/>.</param>
        /// <param name="keys">The key values.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task DeleteAsync(DocDbArgs saveArgs, params IComparable[] keys)
        {
            Check.NotNull(saveArgs, nameof(saveArgs));
            if (keys.Length != 1)
                throw new NotSupportedException("Only a single key value is currently supported.");

            await DocDbInvoker.Default.InvokeAsync(this, async () =>
            {
                try
                {
                    await Client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, saveArgs.CollectionId, keys[0].ToString()), GetRequestOptions(saveArgs));
                }
                catch (DocumentClientException dcex)
                {
                    if (dcex.StatusCode == System.Net.HttpStatusCode.NotFound)
                        return;

                    throw;
                }
            }, this);
        }

        #endregion
    }
}