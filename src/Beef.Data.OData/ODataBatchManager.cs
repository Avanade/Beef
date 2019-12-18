// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Net.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Beef.Data.OData
{
    /// <summary>
    /// Manages the creation and execution of an <b>OData</b> batch.
    /// </summary>
    public class ODataBatchManager
    {
        private readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBatchManager"/> class.
        /// </summary>
        /// <param name="odata">The <see cref="ODataBase"/>.</param>
        /// <param name="isChangeSet">Indicates whether the batch is a change set (an atomic unit of work).</param>
        internal ODataBatchManager(ODataBase odata, bool isChangeSet = false)
        {
            OData = odata ?? throw new ArgumentNullException(nameof(odata));
            IsChangeSet = isChangeSet;
        }

        /// <summary>
        /// Gets the owning <see cref="ODataBase"/> instance.
        /// </summary>
        public ODataBase OData { get; private set; }

        /// <summary>
        /// Indicates whether the batch is a change set (an atomic unit of work). 
        /// </summary>
        /// <remarks>Note that no GET requests are allowed within a change set; an exception will be thrown where attempted.</remarks>
        public bool IsChangeSet { get; private set; }

        /// <summary>
        /// Gets the batch identifier.
        /// </summary>
        public Guid BatchId { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets the current <see cref="ODataBatchState"/>.
        /// </summary>
        public ODataBatchState State { get; internal set; } = ODataBatchState.Ready;

        /// <summary>
        /// Gets the <see cref="ODataBatchItem"/> list.
        /// </summary>
        internal List<ODataBatchItem> Items { get; } = new List<ODataBatchItem>();

        #region Query

        /// <summary>
        /// Creates (<see cref="ODataBase.CreateQuery{T}(ODataArgs)"/>) and batches a <b>Select query</b> for the <b>OData</b> entity.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="queryArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="query">An optional function to enable in-place query selection.</param>
        /// <returns>The <see cref="ODataBatchItem"/>.</returns>
        public async Task<ODataBatchItem> SelectQueryAsync<TColl, T>(ODataArgs queryArgs, Func<IQueryable<T>, IQueryable<T>> query = null) where TColl : ICollection<T>, new() where T : class
        {
            var q = OData.CreateQuery<T>(queryArgs);
            if (query != null)
            {
                var q2 = query(q) as ODataQueryable<T>;
                q = q2 ?? throw new InvalidOperationException("The query function must return an instance of ODataQueryable<T>.");
            }

            var qa = q.QueryExecutor.GetQueryAggregator(q.Expression, null);
            var obi = AddRequest(await OData.BuildQueryRequestAsync(queryArgs, qa.ToString()));
            obi.GetValueFunc = async () =>
            {
                var coll = new TColl();
                await OData.ProcessQueryResponse<T>(obi.ResponseMessage, queryArgs, qa, coll);
                return coll;
            };

            return obi;
        }

        /// <summary>
        /// Batches a <b>Select query</b> for the <b>OData</b> entity.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="query">The <see cref="ODataQueryable{T}"/>.</param>
        /// <returns>The <see cref="ODataBatchItem"/>.</returns>
        public async Task<ODataBatchItem> SelectQueryAsync<T>(IQueryable<T> query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            if (!(query is ODataQueryable<T> q))
                throw new ArgumentException("The query must be an instance of ODataQueryable<T>.", nameof(query));

            if (q.QueryExecutor.OData != this.OData)
                throw new ArgumentException("The query can only be invoked by this batch where both the batch and query are created by the same ODataBase instance.");

            var qa = q.QueryExecutor.GetQueryAggregator(q.Expression, null);
            var queryArgs = q.QueryExecutor.QueryArgs;
            var obi = AddRequest(await OData.BuildQueryRequestAsync(queryArgs, q.GetODataQuery(null)));
            obi.GetValueFunc = async () =>
            {
                var coll = new List<T>();
                await OData.ProcessQueryResponse(obi.ResponseMessage, queryArgs, qa, coll);
                return coll;
            };

            return obi;
        }

        #endregion

        #region Operations

        /// <summary>
        /// Batches a <b>Get</b> of an <b>OData</b> entity for the specified <paramref name="keys"/>.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="getArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="keys">The key values.</param>
        /// <returns>The <see cref="ODataBatchItem"/>.</returns>
        public async Task<ODataBatchItem> GetAsync<T>(ODataArgs getArgs, params IComparable[] keys) where T : class, new()
        {
            var obi = AddRequest(await OData.BuildGetRequestAsync(OData.SetUpArgs<T>(getArgs), keys));
            obi.GetValueFunc = async () => await OData.ProcessGetResponseAsync<T>(obi.ResponseMessage, getArgs);
            return obi;
        }

        /// <summary>
        /// Batches a <b>Create</b> of an <b>Odata</b> entity.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="value">The entity value.</param>
        /// <returns>The <see cref="ODataBatchItem"/>.</returns>
        public async Task<ODataBatchItem> CreateAsync<T>(ODataArgs saveArgs, T value) where T : class, new()
        {
            return AddRequest(await OData.BuildCreateRequestAsync(OData.SetUpArgs<T>(saveArgs), value));
        }

        /// <summary>
        /// Batches an <b>Update</b> of an <b>Odata</b> entity.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="value">The entity value.</param>
        /// <returns>The <see cref="ODataBatchItem"/>.</returns>
        public async Task<ODataBatchItem> UpdateAsync<T>(ODataArgs saveArgs, T value) where T : class, new()
        {
            return AddRequest(await OData.BuildUpdateRequestAsync(OData.SetUpArgs<T>(saveArgs), value));
        }

        /// <summary>
        /// Batches a <b>Delete</b> of an <b>Odata</b> entity for the specified <paramref name="keys"/>.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="keys">The key values.</param>
        /// <returns>The <see cref="ODataBatchItem"/>.</returns>
        public async Task<ODataBatchItem> DeleteAsync<T>(ODataArgs saveArgs, params IComparable[] keys) where T : class, new()
        {
            return AddRequest(await OData.BuildDeleteRequestAsync(OData.SetUpArgs<T>(saveArgs), keys));
        }

        /// <summary>
        /// Batches an <b>execute</b> of an <b>OData</b> request for a specified <paramref name="pathAndQuery"/> using a <see cref="JObject"/> for the request and response.
        /// </summary>
        /// <param name="exeArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="pathAndQuery">The url path and query <see cref="string"/> (excluding the base URI path).</param>
        /// <param name="json">Optional JSON request content.</param>
        /// <returns>The <see cref="ODataBatchItem"/>.</returns>
        /// <remarks>The <see cref="HttpMethod"/> defaults to a <see cref="HttpMethod.Post"/>. This is overridden using the <see cref="ODataArgs.OverrideHttpMethod"/>.</remarks>
        public async Task<ODataBatchItem> ExecuteAsync(ODataArgs exeArgs, string pathAndQuery, JObject json)
        {
            var obi = AddRequest(await OData.BuildExecuteRequestAsync(exeArgs, pathAndQuery, json));
            obi.GetValueFunc = async () => await ODataBase.ProcessExecuteResponseAsync(obi.ResponseMessage, exeArgs);
            return obi;
        }

        /// <summary>
        /// Batches an <b>execute</b> of an <b>OData</b> request for a specified <paramref name="pathAndQuery"/>.
        /// </summary>
        /// <param name="exeArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="pathAndQuery">The url path and query <see cref="string"/> (excluding the base URI path).</param>
        /// <returns>The <see cref="ODataBatchItem"/>.</returns>
        /// <remarks>The <see cref="HttpMethod"/> defaults to a <see cref="HttpMethod.Post"/>. This is overridden using the <see cref="ODataArgs.OverrideHttpMethod"/>.</remarks>
        public async Task<ODataBatchItem> ExecuteAsync(ODataArgs exeArgs, string pathAndQuery)
        {
            return AddRequest(await OData.BuildExecuteRequestAsync(exeArgs, pathAndQuery));
        }

        /// <summary>
        /// Batches an <b>execute</b> of an <b>OData</b> request for a specified <paramref name="pathAndQuery"/> with a <typeparamref name="TRes"/> response.
        /// </summary>
        /// <typeparam name="TRes">The response <see cref="Type"/>.</typeparam>
        /// <param name="exeArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="pathAndQuery">The url path and query <see cref="string"/> (excluding the base URI path).</param>
        /// <returns>The <see cref="ODataBatchItem"/>.</returns>
        /// <remarks>The <see cref="HttpMethod"/> defaults to a <see cref="HttpMethod.Post"/>. This is overridden using the <see cref="ODataArgs.OverrideHttpMethod"/>.</remarks>
        public async Task<ODataBatchItem> ExecuteAsync<TRes>(ODataArgs exeArgs, string pathAndQuery)
        {
            var obi = AddRequest(await OData.BuildExecuteRequestAsync(exeArgs, pathAndQuery));
            obi.GetValueFunc = async () => await OData.ProcessExecuteResponseAsync<TRes>(obi.ResponseMessage, exeArgs);
            return obi;
        }

        /// <summary>
        /// Batches an <b>execute</b> of an <b>OData</b> request for a specified <paramref name="pathAndQuery"/> with a <typeparamref name="TReq"/> request value.
        /// </summary>
        /// <param name="exeArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="pathAndQuery">The url path and query <see cref="string"/> (excluding the base URI path).</param>
        /// <param name="value">The request value.</param>
        /// <returns>The <see cref="ODataBatchItem"/>.</returns>
        /// <remarks>The <see cref="HttpMethod"/> defaults to a <see cref="HttpMethod.Post"/>. This is overridden using the <see cref="ODataArgs.OverrideHttpMethod"/>.</remarks>
        public async Task<ODataBatchItem> ExecuteAsync<TReq>(ODataArgs exeArgs, string pathAndQuery, TReq value)
        {
            return AddRequest(await OData.BuildExecuteRequestAsync<TReq>(exeArgs, pathAndQuery, value));
        }

        /// <summary>
        /// Batches an <b>execute</b> of an <b>OData</b> request for a specified <paramref name="pathAndQuery"/> with a <typeparamref name="TReq"/> request and <typeparamref name="TRes"/> response value.
        /// </summary>
        /// <param name="exeArgs">The <see cref="ODataArgs"/>.</param>
        /// <param name="pathAndQuery">The url path and query <see cref="string"/> (excluding the base URI path).</param>
        /// <param name="value">The request value.</param>
        /// <returns>The <see cref="ODataBatchItem"/>.</returns>
        /// <remarks>The <see cref="HttpMethod"/> defaults to a <see cref="HttpMethod.Post"/>. This is overridden using the <see cref="ODataArgs.OverrideHttpMethod"/>.</remarks>
        public async Task<ODataBatchItem> ExecuteAsync<TReq, TRes>(ODataArgs exeArgs, string pathAndQuery, TReq value)
        {
            var obi = AddRequest(await OData.BuildExecuteRequestAsync<TReq>(exeArgs, pathAndQuery, value));
            obi.GetValueFunc = async () => await OData.ProcessExecuteResponseAsync<TRes>(obi.ResponseMessage, exeArgs);
            return obi;
        }

        #endregion

        /// <summary>
        /// Adds a <see cref="HttpRequestMessage"/> to the batch.
        /// </summary>
        /// <param name="requestMessage">The request message.</param>
        internal ODataBatchItem AddRequest(HttpRequestMessage requestMessage)
        {
            var obi = new ODataBatchItem(this) { RequestMessage = requestMessage };

            lock (_lock)
            {
                if (State != ODataBatchState.Ready)
                    throw new InvalidOperationException("Batch is not in a Ready state; no further requests can be added to the batch.");

                if (IsChangeSet && requestMessage.Method == HttpMethod.Get)
                    throw new InvalidOperationException("Batch has been configured as a change set and as such no GET requests are allowed.");

                Items.Add(obi);
            }

            OData.InvokeOnCreatingRequest(requestMessage);
            return obi;
        }

        /// <summary>
        /// Sends the batch and returns an <see cref="ODataBatchResponse"/>.
        /// </summary>
        /// <returns>The <see cref="ODataBatchResponse"/>.</returns>
        public async Task<ODataBatchResponse> SendAsync()
        {
            lock (_lock)
            {
                if (State != ODataBatchState.Ready)
                    throw new InvalidOperationException("Batch is no longer in a Ready state; no further requests can be added to the batch.");

                if (Items.Count == 0)
                    throw new InvalidOperationException("Batch must have add at least one request item added; nothing to send.");

                State = ODataBatchState.Sending;
            }

            return await ODataInvoker.Default.InvokeAsync(this, async () =>
            {
                var request = OData.CreateRequestMessage(new ODataArgs(), "POST", null, "$batch");
                request.Content = await CreateContentAsync();
                var resp = await ODataBatchResponse.CreateAsync(this, await OData.SendRequestAsync(request));
                State = ODataBatchState.Sent;
                return resp;
            }, OData);
        }

        /// <summary>
        /// Creates the batch content.
        /// </summary>
        /// <returns>The <see cref="StringContent"/>.</returns>
        internal async Task<StringContent> CreateContentAsync()
        {
            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            using (var mw = new HttpMultiPartRequestWriter(sw, BatchId, IsChangeSet))
            {
                foreach (var item in Items)
                {
                    await mw.WriteAsync(item.RequestMessage);
                }

                // Terminate the batch.
                await mw.CloseAsync();
            }

            // Create the batch content.
            var sc = new StringContent(sb.ToString());
            sc.Headers.Remove("Content-Type");
            sc.Headers.TryAddWithoutValidation("Content-Type", $"multipart/mixed;boundary=batch_{BatchId}");
            return sc;
        }
    }
}
