// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Beef.Data.OData
{
    /// <summary>
    /// Represents an <b>OData</b> request/response item.
    /// </summary>
    public class ODataBatchItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBatchItem"/> class.
        /// </summary>
        /// <param name="batchManager">The <see cref="ODataBatchManager"/>.</param>
        internal ODataBatchItem(ODataBatchManager batchManager)
        {
            BatchManager = batchManager;
        }

        /// <summary>
        /// Gets the <see cref="ODataBatchManager"/>.
        /// </summary>
        public ODataBatchManager BatchManager { get; private set; }

        /// <summary>
        /// Gets the <see cref="HttpRequestMessage"/>.
        /// </summary>
        public HttpRequestMessage RequestMessage { get; internal set; }

        /// <summary>
        /// Gets the <see cref="HttpResponseMessage"/>.
        /// </summary>
        public HttpResponseMessage ResponseMessage { get; internal set; }

        /// <summary>
        /// Indicates whether the response message has been updated as a result of a successful bacth send.
        /// </summary>
        public bool HasResponseMessage { get => ResponseMessage != null; }

        /// <summary>
        /// Gets or sets the <b>GetValue</b> function.
        /// </summary>
        internal Func<Task<object>> GetValueFunc { get; set; }

        /// <summary>
        /// Indicates whether <see cref="GetValueAsync"/> will return a value from a successful response.
        /// </summary>
        public bool CanGetValue { get => GetValueFunc != null; }

        /// <summary>
        /// Indicates whether the batch HTTP response was successful (see <see cref="ResponseMessage"/>); will return <c>false</c> where there is no <see cref="ResponseMessage"/>.
        /// </summary>
        public bool IsSuccessStatusCode { get => HasResponseMessage ? ResponseMessage.IsSuccessStatusCode : false; }

        /// <summary>
        /// Gets the response <see cref="HttpStatusCode"/> (see <see cref="ResponseMessage"/>); will return <see cref="HttpStatusCode.OK"/> where there is no <see cref="ResponseMessage"/>.
        /// </summary>
        public HttpStatusCode StatusCode { get => HasResponseMessage ? ResponseMessage.StatusCode : HttpStatusCode.OK; }

        /// <summary>
        /// Gets the <see cref="HttpRequestException"/> for the <see cref="ResponseMessage"/> where not valid (see <see cref="IsSuccessStatusCode"/>).
        /// </summary>
        /// <returns>The <see cref="HttpRequestException"/> where not valid; otherwise, <c>null</c>.</returns>
        public HttpRequestException GetHttpRequestException()
        {
            return HasResponseMessage ? ODataBase.GetHttpRequestExceptionForResponse(ResponseMessage) : null;
        }

        /// <summary>
        /// Throws an exception if the <see cref="IsSuccessStatusCode"/> property for the batch item is false (see <see cref="ResponseMessage"/>).
        /// </summary>
        /// <returns>The <see cref="ODataBatchItem"/>.</returns>
        /// <remarks>Use this versus the underlying <see cref="HttpResponseMessage"/> <see cref="HttpResponseMessage.EnsureSuccessStatusCode"/> as this will leverage
        /// the <see cref="ODataBase"/> <see cref="ODataBase.ExceptionHandler"/> to enable exception transformation.</remarks>
        public ODataBatchItem EnsureBatchSuccessStatusCode()
        {
            if (HasResponseMessage)
            {
                try
                {
                    ODataBase.EnsureSuccessStatusCodeForResponse(ResponseMessage);
                }
                catch (HttpRequestException hrex)
                {
                    BatchManager.OData.ExceptionHandler?.Invoke(hrex);
                    throw;
                }
            }

            return this;
        }

        /// <summary>
        /// Gets the value from the <see cref="ResponseMessage"/> (only where <see cref="CanGetValue"/> is <c>true</c>).
        /// </summary>
        /// <returns>The value from the response.</returns>
        /// <remarks>Where the <see cref="HttpResponseMessage.IsSuccessStatusCode"/> is <c>false</c> an exception will be thrown.</remarks>
        public async Task<object> GetValueAsync()
        {
            if (!HasResponseMessage)
                throw new InvalidOperationException("A value cannot be returned where the Batch has not been sent and a corresponding ResponseMessage returned.");

            if (!CanGetValue)
                throw new InvalidOperationException("A value cannot be returned from the ResponseMessage; see CanGetValue property.");

            return await GetValueFunc().ConfigureAwait(false);
        }
    }
}
