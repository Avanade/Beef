// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Data.OData.Linq;
using Beef.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Beef.Data.OData
{
    /// <summary>
    /// Extends <see cref="ODataBase"/> adding <see cref="Register"/> and <see cref="Default"/> capabilities for <b>OData</b>.
    /// </summary>
    /// <typeparam name="TDefault">The <see cref="Default"/> <see cref="Type"/>.</typeparam>
    public abstract class OData<TDefault> : ODataBase where TDefault : OData<TDefault>
    {
        private static readonly object _lock = new object();
        private static TDefault _default;
        private static Func<TDefault> _create;

        /// <summary>
        /// Registers the <see cref="Default"/> <see cref="ODataBase"/> instance.
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
        /// Gets the current default <see cref="ODataBase"/> instance.
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
        /// Initializes a new instance of the <see cref="OData{TDefault}"/> class.
        /// </summary>
        /// <param name="baseUri">The base URI string.</param>
        public OData(string baseUri) : base(baseUri) { }
    }

    /// <summary>
    /// Represents the base class for <b>OData</b> access; being a lightweight direct <b>OData</b> access layer.
    /// </summary>
    public abstract class ODataBase
    {
        /// <summary>
        /// Transforms and throws the <see cref="IBusinessException"/> equivalent for a <see cref="HttpRequestException"/>.
        /// </summary>
        /// <param name="hrex">The <see cref="HttpRequestException"/>.</param>
        public static void ThrowTransformedHttpRequestException(HttpRequestException hrex)
        {
            // TODO: Add exception logic.
        }

        /// <summary>
        /// Ensures the <see cref="HttpResponseMessage"/> is valid by checking the <see cref="HttpResponseMessage.StatusCode"/> (see cref="HttpResponseMessage.IsSuccessStatusCode"/>) and
        /// throwing an <see cref="HttpRequestException"/> where not.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
        public static void EnsureSuccessStatusCodeForResponse(HttpResponseMessage response)
        {
            var hrex = GetHttpRequestExceptionForResponse(response);
            if (hrex != null)
                throw hrex;
        }

        /// <summary>
        /// Gets the <see cref="HttpRequestException"/> from the <see cref="HttpResponseMessage"/> where the <see cref="HttpResponseMessage.StatusCode"/> (see cref="HttpResponseMessage.IsSuccessStatusCode"/>)
        /// is not valid.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
        /// <returns>The <see cref="HttpRequestException"/> where not valid; otherwise, <c>null</c>.</returns>
        public static HttpRequestException GetHttpRequestExceptionForResponse(HttpResponseMessage response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (response.IsSuccessStatusCode)
                return null;

            var t = Task.Run(async () =>
            {
                Exception exception = null;
                if (response.Content?.Headers?.ContentLength > 0)
                {
                    using (var s = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    using (var sr = new StreamReader(s))
                    {
                        var content = await sr.ReadToEndAsync().ConfigureAwait(false);
                        exception = new Exception(content);
                    }
                }

                return new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}).", exception);
            });

            t.Wait();
            return t.Result;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBase"/> class.
        /// </summary>
        /// <param name="baseUri">The base URI string.</param>
        public ODataBase(string baseUri)
        {
            BaseUri = !string.IsNullOrEmpty(baseUri) ? baseUri : throw new ArgumentNullException(nameof(baseUri));
            HttpClient = new HttpClient { BaseAddress = new Uri(baseUri) };
            HttpClient.DefaultRequestHeaders.Accept.Clear();
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            HttpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
        }

        /// <summary>
        /// Gets the base URI string.
        /// </summary>
        public string BaseUri { get; private set; }

        /// <summary>
        /// Gets the <see cref="System.Net.Http.HttpClient"/>.
        /// </summary>
        /// <returns>The <see cref="System.Net.Http.HttpClient"/>.</returns>
        public HttpClient HttpClient { get; private set; }

        /// <summary>
        /// Gets the <see cref="HttpMethod"/> used for <see cref="GetAsync{T}"/> and <see cref="SelectQueryAsync{TColl, T}(ODataArgs, Func{IQueryable{T}, IQueryable{T}})"/> (defaults to 'GET').
        /// </summary>
        public HttpMethod GetHttpMethod { get; set; } = HttpMethod.Get;

        /// <summary>
        /// Gets the <see cref="HttpMethod"/> used for <see cref="CreateAsync{T}"/> (defaults to 'POST').
        /// </summary>
        public HttpMethod CreateHttpMethod { get; set; } = HttpMethod.Post;

        /// <summary>
        /// Gets the <see cref="HttpMethod"/> used for <see cref="UpdateAsync{T}"/> (defaults to 'PATCH').
        /// </summary>
        public HttpMethod UpdateHttpMethod { get; set; } = new HttpMethod("PATCH");

        /// <summary>
        /// Gets the <see cref="HttpMethod"/> used for <see cref="DeleteAsync{T}"/> (defaults to 'DELETE').
        /// </summary>
        public HttpMethod DeleteHttpMethod { get; set; } = HttpMethod.Delete;

        /// <summary>
        /// Gets or sets the <see cref="HttpRequestException"/> handler (by default set up to execute <see cref="ThrowTransformedHttpRequestException(HttpRequestException)"/>).
        /// </summary>
        public Action<HttpRequestException> ExceptionHandler { get; set; } = (hrex) => ThrowTransformedHttpRequestException(hrex);

        /// <summary>
        /// Raises the <see cref="CreatingRequest"/> event.
        /// </summary>
        /// <param name="requestMessage">The <see cref="HttpRequestMessage"/>.</param>
        protected virtual void OnCreatingRequest(HttpRequestMessage requestMessage)
        {
            CreatingRequest?.Invoke(this, new OdmRequestMessageEventArgs(requestMessage));
        }

        /// <summary>
        /// Raises the <see cref="CreatingRequest"/> event - for internal use only!
        /// </summary>
        /// <param name="requestMessage">The <see cref="HttpRequestMessage"/>.</param>
        internal void InvokeOnCreatingRequest(HttpRequestMessage requestMessage)
        {
            OnCreatingRequest(requestMessage);
        }

        /// <summary>
        /// Raised when creating a <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <remarks>Raised when creating all request messages; including those within a batch (see <see cref="ODataBatchManager"/>).</remarks>
        public event EventHandler<OdmRequestMessageEventArgs> CreatingRequest;

        /// <summary>
        /// Raises the <see cref="SendingRequest"/> event.
        /// </summary>
        /// <param name="requestMessage">The <see cref="HttpRequestMessage"/>.</param>
        protected virtual void OnSendingRequest(HttpRequestMessage requestMessage)
        {
            SendingRequest?.Invoke(this, new OdmRequestMessageEventArgs(requestMessage));
        }

        /// <summary>
        /// Raised when sending a <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <remarks>Raised when sending a request messages; does not include those within a batch (see <see cref="ODataBatchManager"/>).</remarks>
        public event EventHandler<OdmRequestMessageEventArgs> SendingRequest;

        /// <summary>
        /// Creates the <see cref="HttpRequestMessage"/>.
        /// </summary>
        /// <param name="args">The <see cref="IODataArgs"/>.</param>
        /// <param name="method">The HTTP method.</param>
        /// <param name="odataEntityName">The OData entity name.</param>
        /// <param name="url">The url path and query suffix.</param>
        internal HttpRequestMessage CreateRequestMessage(IODataArgs args, string method, string odataEntityName, string url)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(HttpClient.BaseAddress.AbsolutePath))
            {
                sb.Append(HttpClient.BaseAddress.AbsolutePath);
                sb.Append("/");
            }

            if (!string.IsNullOrEmpty(odataEntityName))
                sb.Append(odataEntityName);

            if (!string.IsNullOrEmpty(url))
                sb.Append(url);

            var req = new HttpRequestMessage(new HttpMethod(method), new Uri(HttpClient.BaseAddress, sb.ToString()));

            if (args.HasHeaders)
            {
                for (int i = 0; i < args.Headers.Count; i++)
                {
                    req.Headers.TryAddWithoutValidation(args.Headers.GetKey(i), args.Headers.GetValues(i));
                }
            }

            args.RequestMessage = req;
            args.ResponseMessage = null;

            return req;
        }

        /// <summary>
        /// Determines the HTTP Method to use.
        /// </summary>
        private string DetermineHttpMethod(string fallbackMethod, HttpMethod configuredMethod = null, IODataArgs odmArgs = null)
        {
            var m = odmArgs?.OverrideHttpMethod?.Method;
            if (m != null)
                return m;

            m = configuredMethod?.Method;
            if (m != null)
                return configuredMethod.Method;

            return fallbackMethod;
        }

        /// <summary>
        /// Sets the IF-MATCH condition for the eTag.
        /// </summary>
        private void SetIfMatchCondition(HttpRequestMessage request, ODataArgs args, object value)
        {
            if (value == null)
                return;

            switch (args.IfMatch)
            {
                case ODataIfMatch.UpdateEtag:
                    var etag = value as IETag;
                    if (etag == null)
                        goto case ODataIfMatch.UpdateAny;

                    request.Headers.IfMatch.Add(new EntityTagHeaderValue(etag.ETag));
                    break;

                case ODataIfMatch.UpdateAny:
                    request.Headers.IfMatch.Add(EntityTagHeaderValue.Any);
                    break;
            }
        }

        /// <summary>
        /// Sets up (creates) and auto-maps the <paramref name="args"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="ODataArgs"/>.</param>
        /// <returns>An updated or created <see cref="ODataArgs"/>.</returns>
        internal ODataArgs SetUpArgs<T>(ODataArgs args)
        {
            var oa = args ?? new ODataArgs();
            if (oa.Mapper == null)
                oa.Mapper = ODataAutoMapper.GetMapper(typeof(T));
            else if (oa.Mapper.SrceType != typeof(T))
                throw new ArgumentException($"The Entity Type is '{typeof(T).Name}' and the Mapper Type is '{oa.Mapper.SrceType.Name}'; these must be the same.");

            args = oa;
            return args;
        }

        /// <summary>
        /// Executes the HTTP request send.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/>.</param>
        internal async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request)
        {
            OnSendingRequest(request);
            return await HttpClient.SendAsync(request).ConfigureAwait(false);
        }

        #region Query

        /// <summary>
        /// Creates a <see cref="ODataQueryable{T}"/> to enable LINQ-style queries.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="queryArgs">The optional <see cref="ODataArgs"/>.</param>
        /// <returns>The <see cref="ODataQueryable{T}"/>.</returns>
        public ODataQueryable<T> CreateQuery<T>(ODataArgs queryArgs = null) where T : class
        {
            Remotion.Linq.IQueryExecutor executor = null;
            return new ODataQueryable<T>(this, SetUpArgs<T>(queryArgs), ref executor);
        }

        /// <summary>
        /// Creates and executes a select query creating a resultant collection.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="queryArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="query">An optional function to enable in-place query selection.</param>
        /// <returns>A resultant collection.</returns>
        public async Task<TColl> SelectQueryAsync<TColl, T>(ODataArgs queryArgs, Func<IQueryable<T>, IQueryable<T>> query = null) where TColl : ICollection<T>, new() where T : class
        {
            var coll = new TColl();

            await SelectQueryAsync(queryArgs, coll, query);

            return coll;
        }

        /// <summary>
        /// Creates and executes a select query adding to the passed collection.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="queryArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="coll">The collection to add items to.</param>
        /// <param name="query">An optional function to enable in-place query selection.</param>
        public Task SelectQueryAsync<TColl, T>(ODataArgs queryArgs, TColl coll, Func<IQueryable<T>, IQueryable<T>> query = null) where TColl : ICollection<T> where T : class
        {
            var q = CreateQuery<T>(queryArgs);
            if (query != null)
            {
                var q2 = query(q) as ODataQueryable<T>;
                q = q2 ?? throw new InvalidOperationException("The query function must return an instance of OdmQueryable<T>.");
            }

            return Task.Run(() =>
            {
                foreach (var item in q.AsEnumerable())
                {
                    coll.Add(item);
                }
            });
        }

        /// <summary>
        /// Invoke the GET query using the <paramref name="queryAggregator"/> and add to the <paramref name="coll"/>.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="queryArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="queryAggregator">The <see cref="ODataQueryAggregator"/>.</param>
        /// <param name="coll">The collection to add items to.</param>
        internal async Task Query<T>(ODataArgs queryArgs, ODataQueryAggregator queryAggregator, ICollection<T> coll)
        {
            await ODataInvoker.Default.InvokeAsync(this, async () =>
            {
                var request = await BuildQueryRequestAsync(queryArgs, queryAggregator.ToString());
                OnCreatingRequest(request);
                var response = await SendRequestAsync(request);
                queryArgs.ResponseMessage = response;
                await ProcessQueryResponse(response, queryArgs, queryAggregator, coll);
            }, this);
        }

        /// <summary>
        /// Builds the <b>Odata</b> <i>query</i> request for the specified <paramref name="queryUrl"/>.
        /// </summary>
        /// <param name="queryArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="queryUrl">The query URL string.</param>
        /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
        internal async Task<HttpRequestMessage> BuildQueryRequestAsync(ODataArgs queryArgs, string queryUrl)
        {
            return await Task.FromResult(CreateRequestMessage(queryArgs, DetermineHttpMethod("GET", GetHttpMethod, queryArgs), queryArgs.Mapper.ODataEntityName, queryUrl)).ConfigureAwait(false);
        }

        /// <summary>
        /// Processes the <b>Odata</b> <i>query</i> <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
        /// <param name="queryArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="queryAggregator">The <see cref="ODataQueryAggregator"/>.</param>
        /// <param name="coll">The collection to add to.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        internal async Task ProcessQueryResponse<T>(HttpResponseMessage response, ODataArgs queryArgs, ODataQueryAggregator queryAggregator, ICollection<T> coll)
        {
            EnsureSuccessStatusCodeForResponse(response);

            var type = typeof(T);
            IODataMapper mapper = null;
            PropertyInfo pi = null; 

            switch (queryAggregator.SelectClause.Selector.NodeType)
            {
                case System.Linq.Expressions.ExpressionType.MemberAccess:
                    mapper = queryArgs.Mapper;
                    var me = (System.Linq.Expressions.MemberExpression)queryAggregator.SelectClause.Selector;
                    pi = (PropertyInfo)me.Member;
                    break;

                default:
                    mapper = queryArgs.Mapper.SrceType == type ? queryArgs.Mapper : ODataAutoMapper.GetMapper(type);
                    break;
            }

            int? count = null;

            using (var s = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var sr = new StreamReader(s))
            using (var jr = new JsonTextReader(sr))
            {
                var json = await JObject.LoadAsync(jr).ConfigureAwait(false);
                count = json.Value<int?>("@odata.count");

                foreach (var jt in json.GetValue("value"))
                {
                    var obj = mapper.MapFromOData(jt, Mapper.OperationTypes.Get);
                    if (pi == null)
                        coll.Add((T)obj);
                    else
                        coll.Add((T)pi.GetValue(obj));
                }
            }

            if (count.HasValue && queryArgs.Paging != null && queryArgs.Paging.IsGetCount)
                queryArgs.Paging.TotalCount = count.Value;
        }

        #endregion

        #region Get

        /// <summary>
        /// Gets the <b>Odata</b> entity for the specified <paramref name="keys"/> mapping to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="getArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="keys">The key values.</param>
        /// <returns>The entity value where found; otherwise, <c>null</c>.</returns>
        public async Task<T> GetAsync<T>(ODataArgs getArgs, params IComparable[] keys) where T : class, new()
        {
            return await ODataInvoker<T>.Default.InvokeAsync(this, async () =>
            {
                var request = await BuildGetRequestAsync(SetUpArgs<T>(getArgs), keys);
                OnCreatingRequest(request);
                var response = await SendRequestAsync(request);
                getArgs.ResponseMessage = response;
                return await ProcessGetResponseAsync<T>(response, getArgs);
            }, this);
        }

        /// <summary>
        /// Builds the <b>Odata</b> <i>get</i> request for the specified <paramref name="keys"/>.
        /// </summary>
        /// <param name="args">The <see cref="ODataArgs"/>.</param>
        /// <param name="keys">The key values.</param>
        /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
        internal async Task<HttpRequestMessage> BuildGetRequestAsync(ODataArgs args, params IComparable[] keys)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (keys == null || keys.Length == 0)
                throw new ArgumentNullException(nameof(keys));

            string q = $"{args.Mapper.GetKeyUrl(keys)}?{args.GetODataQuery()}";
            return await Task.FromResult(CreateRequestMessage(args, DetermineHttpMethod("GET", GetHttpMethod, args), args.Mapper.ODataEntityName, q));
        }

        /// <summary>
        /// Processes the <b>Odata</b> <i>get</i> <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="getArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
        /// <returns>The entity value where found; otherwise, <c>null</c>.</returns>
        internal async Task<T> ProcessGetResponseAsync<T>(HttpResponseMessage response, ODataArgs getArgs) where T : class, new()
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            EnsureSuccessStatusCodeForResponse(response);

            using (var s = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            using (var sr = new StreamReader(s))
            using (var jr = new JsonTextReader(sr))
            {
                var json = await JObject.LoadAsync(jr).ConfigureAwait(false);
                return (T)getArgs.Mapper.MapFromOData(json, Mapper.OperationTypes.Get);
            }
        }

        #endregion

        #region Create

        /// <summary>
        /// Creates (HTTP-POST) the <b>Odata</b> entity.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="value">The value to create.</param>
        /// <returns>The value (re-queried where specified).</returns>
        public async Task<T> CreateAsync<T>(ODataArgs saveArgs, T value) where T : class, new()
        {
            return await ODataInvoker<T>.Default.InvokeAsync(this, async () =>
            {
                var request = await BuildCreateRequestAsync<T>(SetUpArgs<T>(saveArgs), value);
                OnCreatingRequest(request);
                var response = await SendRequestAsync(request);
                saveArgs.ResponseMessage = response;
                EnsureSuccessStatusCodeForResponse(response);
                return value;
            }, this);
        }

        /// <summary>
        /// Builds the <b>Odata</b> <i>create</i> request for the entity.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="value">The value to create.</param>
        /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
        internal async Task<HttpRequestMessage> BuildCreateRequestAsync<T>(ODataArgs saveArgs, T value)
        {
            if (saveArgs == null)
                throw new ArgumentNullException(nameof(saveArgs));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var request = CreateRequestMessage(saveArgs, DetermineHttpMethod("POST", CreateHttpMethod, saveArgs), saveArgs.Mapper.ODataEntityName, null);
            request.Headers.IfMatch.Add(EntityTagHeaderValue.Any);

            var json = saveArgs.Mapper.MapToOData(value, Mapper.OperationTypes.Create);

            using (var ms = new StringWriter())
            using (var jw = new JsonTextWriter(ms) { AutoCompleteOnClose = true })
            {
                await json.WriteToAsync(jw);
                request.Content = new StringContent(ms.ToString(), null, "application/json");
            }

            return request;
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates (<see cref="UpdateHttpMethod"/>) the <b>Odata</b> entity.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="value">The value to create.</param>
        /// <returns>The value (re-queried where specified).</returns>
        public async Task<T> UpdateAsync<T>(ODataArgs saveArgs, T value) where T : class, new()
        {
            return await ODataInvoker<T>.Default.InvokeAsync(this, async () =>
            {
                var request = await BuildUpdateRequestAsync<T>(SetUpArgs<T>(saveArgs), value);
                OnCreatingRequest(request);
                var response = await SendRequestAsync(request);
                saveArgs.ResponseMessage = response;
                EnsureSuccessStatusCodeForResponse(response);
                return value;
            }, this);
        }

        /// <summary>
        /// Builds the <b>Odata</b> <i>update</i> request for the entity.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="value">The value to create.</param>
        /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
        internal async Task<HttpRequestMessage> BuildUpdateRequestAsync<T>(ODataArgs saveArgs, T value)
        {
            if (saveArgs == null)
                throw new ArgumentNullException(nameof(saveArgs));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var request = CreateRequestMessage(saveArgs, DetermineHttpMethod("PATCH", UpdateHttpMethod, saveArgs), saveArgs.Mapper.ODataEntityName, saveArgs.Mapper.GetKeyUrl(value));
            SetIfMatchCondition(request, saveArgs, value);

            var json = saveArgs.Mapper.MapToOData(value, Mapper.OperationTypes.Update);

            using (var ms = new StringWriter())
            using (var jw = new JsonTextWriter(ms) { Formatting = Formatting.None })
            {
                await json.WriteToAsync(jw);
                request.Content = new StringContent(ms.ToString(), null, "application/json");
            }

            return request;
        }

        #endregion

        #region Delete

        /// <summary>
        /// Deletes (<see cref="DeleteHttpMethod"/>) the <b>Odata</b> entity for the specified <paramref name="keys"/>.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="keys">The key values.</param>
        public async Task DeleteAsync<T>(ODataArgs saveArgs, params IComparable[] keys) where T : class, new()
        {
            await ODataInvoker.Default.InvokeAsync(this, async () =>
            {
                var request = await BuildDeleteRequestAsync(SetUpArgs<T>(saveArgs), keys);
                OnCreatingRequest(request);
                var response = await SendRequestAsync(request);
                saveArgs.ResponseMessage = response;
                EnsureSuccessStatusCodeForResponse(response);
            }, this);
        }

        /// <summary>
        /// Builds the <b>Odata</b> <i>delete</i> request for the specified <paramref name="keys"/>.
        /// </summary>
        /// <param name="saveArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="keys">The key values.</param>
        /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
        internal async Task<HttpRequestMessage> BuildDeleteRequestAsync(ODataArgs saveArgs, params IComparable[] keys)
        {
            if (saveArgs == null)
                throw new ArgumentNullException(nameof(saveArgs));

            if (keys == null || keys.Length == 0)
                throw new ArgumentNullException(nameof(keys));

            return await Task.FromResult(CreateRequestMessage(saveArgs, DetermineHttpMethod("DELETE", DeleteHttpMethod, saveArgs), saveArgs.Mapper.ODataEntityName, saveArgs.Mapper.GetKeyUrl(keys)));
        }

        #endregion

        #region Execute

        /// <summary>
        /// Executes an <b>OData</b> request for a specified <paramref name="pathAndQuery"/>.
        /// </summary>
        /// <param name="exeArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="pathAndQuery">The url path and query <see cref="string"/> (excluding the base URI path).</param>
        /// <returns>The <see cref="Task"/> with no response value.</returns>
        /// <remarks>The <see cref="HttpMethod"/> defaults to a <see cref="HttpMethod.Post"/>. This is overridden using the <see cref="ODataArgs.OverrideHttpMethod"/>.</remarks>
        public async Task ExecuteAsync(ODataArgs exeArgs, string pathAndQuery)
        {
            await ExecuteAsync(exeArgs, pathAndQuery, null);
        }

        /// <summary>
        /// Executes an <b>OData</b> request for a specified <paramref name="pathAndQuery"/> with a <typeparamref name="TRes"/> response.
        /// </summary>
        /// <typeparam name="TRes">The response <see cref="Type"/>.</typeparam>
        /// <param name="exeArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="pathAndQuery">The url path and query <see cref="string"/> (excluding the base URI path).</param>
        /// <returns>The resulting value.</returns>
        /// <remarks>The <see cref="HttpMethod"/> defaults to a <see cref="HttpMethod.Post"/>. This is overridden using the <see cref="ODataArgs.OverrideHttpMethod"/>.</remarks>
        public async Task<TRes> ExecuteAsync<TRes>(ODataArgs exeArgs, string pathAndQuery)
        {
            var json = await ExecuteAsync(exeArgs, pathAndQuery, null);
            if (json == null)
                return default(TRes);

            return (TRes)exeArgs.Mapper.MapFromOData(json, Mapper.OperationTypes.Any);
        }

        /// <summary>
        /// Executes an <b>OData</b> request for a specified <paramref name="pathAndQuery"/> with a <typeparamref name="TReq"/> request value.
        /// </summary>
        /// <typeparam name="TReq">The request <see cref="Type"/>.</typeparam>
        /// <param name="exeArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="pathAndQuery">The url path and query <see cref="string"/> (excluding the base URI path).</param>
        /// <param name="value">The request value.</param>
        /// <returns>The <see cref="Task"/> with no response value.</returns>
        /// <remarks>The <see cref="HttpMethod"/> defaults to a <see cref="HttpMethod.Post"/>. This is overridden using the <see cref="ODataArgs.OverrideHttpMethod"/>.</remarks>
        public async Task ExecuteAsync<TReq>(ODataArgs exeArgs, string pathAndQuery, TReq value)
        {
            await ODataInvoker.Default.InvokeAsync(this, async () =>
            {
                var request = await BuildExecuteRequestAsync<TReq>(exeArgs, pathAndQuery, value);
                OnCreatingRequest(request);
                var response = await SendRequestAsync(request);
                exeArgs.ResponseMessage = response;
                EnsureSuccessStatusCodeForResponse(response);
            }, this);
        }

        /// <summary>
        /// Executes an <b>OData</b> request for a specified <paramref name="pathAndQuery"/> with a <typeparamref name="TReq"/> request and <typeparamref name="TRes"/> response value.
        /// </summary>
        /// <typeparam name="TReq">The request <see cref="Type"/>.</typeparam>
        /// <typeparam name="TRes">The response <see cref="Type"/>.</typeparam>
        /// <param name="exeArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="pathAndQuery">The url path and query <see cref="string"/> (excluding the base URI path).</param>
        /// <param name="value">The request value.</param>
        /// <returns>The <see cref="Task"/> with no response value.</returns>
        /// <remarks>The <see cref="HttpMethod"/> defaults to a <see cref="HttpMethod.Post"/>. This is overridden using the <see cref="ODataArgs.OverrideHttpMethod"/>.</remarks>
        public async Task<TRes> ExecuteAsync<TReq, TRes>(ODataArgs exeArgs, string pathAndQuery, TReq value)
        {
            return await ODataInvoker<TRes>.Default.InvokeAsync(this, async () =>
            {
                var request = await BuildExecuteRequestAsync<TReq>(exeArgs, pathAndQuery, value);
                OnCreatingRequest(request);
                var response = await SendRequestAsync(request);
                exeArgs.ResponseMessage = response;
                return await ProcessExecuteResponseAsync<TRes>(response, exeArgs);
            }, this);
        }

        /// <summary>
        /// Executes an <b>OData</b> request for a specified <paramref name="pathAndQuery"/> using a <see cref="JObject"/> for the request and response.
        /// </summary>
        /// <param name="exeArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="pathAndQuery">The url path and query <see cref="string"/> (excluding the base URI path).</param>
        /// <param name="json">Optional JSON request content.</param>
        /// <returns>The resulting <see cref="JObject"/>.</returns>
        /// <remarks>The <see cref="HttpMethod"/> defaults to a <see cref="HttpMethod.Post"/>. This is overridden using the <see cref="ODataArgs.OverrideHttpMethod"/>.</remarks>
        public async Task<JObject> ExecuteAsync(ODataArgs exeArgs, string pathAndQuery, JObject json)
        {
            return await ODataInvoker<JObject>.Default.InvokeAsync(this, async () =>
            {
                var request = await BuildExecuteRequestAsync(exeArgs, pathAndQuery, json);
                OnCreatingRequest(request);
                var response = await SendRequestAsync(request);
                exeArgs.ResponseMessage = response;
                return await ProcessExecuteResponseAsync(response, exeArgs);
            }, this);
        }

        /// <summary>
        /// Builds the <b>Odata</b> <i>execute</i> request.
        /// </summary>
        /// <param name="exeArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="pathAndQuery">The url path and query <see cref="string"/> (excluding the base URI path).</param>
        /// <param name="json">Optional JSON request content.</param>
        /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
        /// <remarks>The <see cref="HttpMethod"/> defaults to a <see cref="HttpMethod.Post"/>. This is overridden using the <see cref="ODataArgs.OverrideHttpMethod"/>.</remarks>
        internal async Task<HttpRequestMessage> BuildExecuteRequestAsync(ODataArgs exeArgs, string pathAndQuery, JObject json = null)
        {
            if (exeArgs == null)
                throw new ArgumentNullException(nameof(exeArgs));

            if (string.IsNullOrEmpty(pathAndQuery))
                throw new ArgumentNullException(nameof(pathAndQuery));

            var request = CreateRequestMessage(exeArgs, DetermineHttpMethod("POST", null, exeArgs), null, pathAndQuery);
            if (json != null)
                request.Content = new StringContent(json.ToString(), null, "application/json");

            return await Task.FromResult(request);
        }

        /// <summary>
        /// Builds the <b>Odata</b> <i>execute</i> request for the entity.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="exeArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="pathAndQuery">The url path and query <see cref="string"/> (excluding the base URI path).</param>
        /// <param name="value">The value to create.</param>
        /// <returns>The <see cref="HttpRequestMessage"/>.</returns>
        /// <remarks>The <see cref="HttpMethod"/> defaults to a <see cref="HttpMethod.Post"/>. This is overridden using the <see cref="ODataArgs.OverrideHttpMethod"/>.</remarks>
        internal async Task<HttpRequestMessage> BuildExecuteRequestAsync<T>(ODataArgs exeArgs, string pathAndQuery, T value)
        {
            if (exeArgs == null)
                throw new ArgumentNullException(nameof(exeArgs));

            var request = CreateRequestMessage(exeArgs, DetermineHttpMethod("POST", CreateHttpMethod, exeArgs), exeArgs.Mapper.ODataEntityName, null);
            request.Headers.IfMatch.Add(EntityTagHeaderValue.Any);

            if (value != null)
            {
                var json = exeArgs.Mapper.MapToOData(value, Mapper.OperationTypes.Any);

                using (var ms = new StringWriter())
                using (var jw = new JsonTextWriter(ms) { AutoCompleteOnClose = true })
                {
                    await json.WriteToAsync(jw);
                    request.Content = new StringContent(ms.ToString(), null, "application/json");
                }
            }

            return request;
        }

        /// <summary>
        /// Processes the <b>Odata</b> <i>execute</i> <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <param name="exeArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
        /// <returns>The resulting <see cref="JObject"/>.</returns>
        internal async Task<JObject> ProcessExecuteResponseAsync(HttpResponseMessage response, ODataArgs exeArgs)
        {
            if (response.StatusCode == HttpStatusCode.NotFound && exeArgs.NullOnNotFoundResponse)
                return null;

            EnsureSuccessStatusCodeForResponse(response);

            if (response.Content?.Headers?.ContentLength != null && response.Content?.Headers?.ContentLength.Value > 0)
            {
                using (var s = await response.Content.ReadAsStreamAsync())
                using (var sr = new StreamReader(s))
                using (var jr = new JsonTextReader(sr))
                {
                    var json = await JObject.LoadAsync(jr);
                    return json;
                }
            }
            else
                return null;
        }

        /// <summary>
        /// Processes the <b>Odata</b> <i>execute</i> <see cref="HttpResponseMessage"/>.
        /// </summary>
        /// <typeparam name="TRes">The response <see cref="Type"/>.</typeparam>
        /// <param name="exeArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="response">The <see cref="HttpResponseMessage"/>.</param>
        /// <returns>The resulting value.</returns>
        internal async Task<TRes> ProcessExecuteResponseAsync<TRes>(HttpResponseMessage response, ODataArgs exeArgs)
        {
            if (response.StatusCode == HttpStatusCode.NotFound && exeArgs.NullOnNotFoundResponse)
                return default(TRes);

            EnsureSuccessStatusCodeForResponse(response);

            if (response.Content?.Headers?.ContentLength != null && response.Content?.Headers?.ContentLength.Value > 0)
            {
                using (var s = await response.Content.ReadAsStreamAsync())
                using (var sr = new StreamReader(s))
                using (var jr = new JsonTextReader(sr))
                {
                    var json = await JObject.LoadAsync(jr).ConfigureAwait(false);
                    return (TRes)exeArgs.Mapper.MapFromOData(json, Mapper.OperationTypes.Any);
                }
            }
            else
                return default(TRes);
        }

        /// <summary>
        /// Creates an <see cref="ODataBatchManager"/> to enable the batching of operations.
        /// </summary>
        /// <param name="isChangeSet">Indicates whether the batch is a change set (an atomic unit of work).</param>
        /// <returns>The <see cref="ODataBatchManager"/>.</returns>
        public ODataBatchManager CreateBatch(bool isChangeSet = false)
        {
            return new ODataBatchManager(this, isChangeSet);
        }

        #endregion
    }
}
