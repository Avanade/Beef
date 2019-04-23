// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Net.Http;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Beef.Data.OData
{
    /// <summary>
    /// Represents an <b>OData</b> batch response.
    /// </summary>
    public class ODataBatchResponse
    {
        #region Create

        /// <summary>
        /// Creates the <see cref="ODataBatchResponse"/>.
        /// </summary>
        /// <param name="batchManager">The <see cref="ODataBatchManager"/>.</param>
        /// <param name="batchResponseMessage">The <see cref="HttpResponseMessage"/>.</param>
        /// <returns></returns>
        static internal async Task<ODataBatchResponse> CreateAsync(ODataBatchManager batchManager, HttpResponseMessage batchResponseMessage)
        {
            var obr = new ODataBatchResponse() { BatchManager = batchManager, BatchResponseMessage = batchResponseMessage };
            if (obr.BatchResponseMessage.IsSuccessStatusCode)
                await ParseResponseAsync(obr);

            return obr;
        }

        /// <summary>
        /// Parse the batch response to get the individual responses.
        /// </summary>
        /// <param name="batchResponse">The <see cref="ODataBatchResponse"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        static internal async Task ParseResponseAsync(ODataBatchResponse batchResponse)
        {
            if (!DetermineContentTypeBoundary(batchResponse.BatchResponseMessage, out string boundary))
                throw new InvalidOperationException("Batch response must have a Content-Type of 'multipart/mixed' with a corresponding 'boundary' specified.");

            using (var rs = await batchResponse.BatchResponseMessage.Content.ReadAsStreamAsync())
            using (var sr = new StreamReader(rs))
            using (var mr = new HttpMultiPartResponseReader(sr))
            {
                int i = 0;
                HttpResponseMessage response = null;

                while ((response = await mr.ReadNextAsync()) != null)
                {
                    if (i > batchResponse.BatchManager.Items.Count)
                        break;

                    response.RequestMessage = batchResponse.BatchManager.Items[i].RequestMessage;
                    batchResponse.BatchManager.Items[i].ResponseMessage = response;
                    i++;
                }

                if (i > batchResponse.BatchManager.Items.Count)
                    throw new InvalidOperationException("Batch response request and response mismatch; the number of requests and responses differ.");
            }
        }

        /// <summary>
        /// Determines the boundary from the content type.
        /// </summary>
        static private bool DetermineContentTypeBoundary(HttpResponseMessage response, out string boundary)
        {
            boundary = null;
            if (response?.Content == null || !response.Content.Headers.Contains("Content-Type"))
                return false;

            bool multipartMixed = false;
            foreach (var hv in response.Content.Headers.GetValues("Content-Type"))
            {
                foreach (var v in hv.Split(new char[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var kv = v.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (string.Compare(kv[0], "multipart/mixed") == 0 && kv.Length == 1)
                        multipartMixed = true;
                    else if (string.Compare(kv[0], "boundary") == 0 && kv.Length == 2)
                        boundary = kv[1];
                }
            }

            return multipartMixed && !string.IsNullOrEmpty(boundary);
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBatchResponse"/> class.
        /// </summary>
        private ODataBatchResponse() { }

        /// <summary>
        /// Gets the <see cref="ODataBatchManager"/>.
        /// </summary>
        public ODataBatchManager BatchManager { get; internal set; }

        /// <summary>
        /// Gets the batch <see cref="HttpResponseMessage"/>.
        /// </summary>
        public HttpResponseMessage BatchResponseMessage { get; internal set; }

        /// <summary>
        /// Gets the resultant <see cref="ODataBatchItem"/> array.
        /// </summary>
        public ODataBatchItem[] Items { get => BatchManager.Items.ToArray(); }

        /// <summary>
        /// Gets the resultant <see cref="ODataBatchItem"/> array where the underlying <see cref="ODataBatchItem.IsSuccessStatusCode"/> is <c>false</c>.
        /// </summary>
        public ODataBatchItem[] ItemsInError { get => BatchManager.Items.Where(x => !x.IsSuccessStatusCode).ToArray(); }

        /// <summary>
        /// Indicates whether any of the responses within the batch have an error (where the underlying <see cref="ODataBatchItem.IsSuccessStatusCode"/> is <c>false</c>). 
        /// </summary>
        public bool HasItemErrors { get => BatchResponseMessage != null && BatchManager.Items.Where(x => !x.IsSuccessStatusCode).FirstOrDefault() != null; }

        /// <summary>
        /// Throws an exception if the <see cref="IsSuccessStatusCode"/> property for the batch is false (see <see cref="BatchResponseMessage"/>).
        /// </summary>
        /// <returns>The <see cref="ODataBatchResponse"/>.</returns>
        /// <remarks>Use this versus the underlying <see cref="HttpResponseMessage"/> <see cref="HttpResponseMessage.EnsureSuccessStatusCode"/> as this will leverage
        /// the <see cref="ODataBase"/> <see cref="ODataBase.ExceptionHandler"/> to enable exception transformation.</remarks>
        public ODataBatchResponse EnsureBatchSuccessStatusCode()
        {
            if (BatchResponseMessage == null)
                return this;

            try
            {
                ODataBase.EnsureSuccessStatusCodeForResponse(BatchResponseMessage);
            }
            catch (HttpRequestException hrex)
            {
                BatchManager.OData.ExceptionHandler?.Invoke(hrex);
                throw;
            }

            return this;
        }

        /// <summary>
        /// Gets the batch <see cref="HttpRequestException"/> where not valid (see <see cref="IsSuccessStatusCode"/>).
        /// </summary>
        /// <returns>The batch <see cref="HttpRequestException"/> where not valid; otherwise, <c>null</c>.</returns>
        public HttpRequestException GetBatchHttpRequestException()
        {
            if (IsSuccessStatusCode)
                return null;

            return ODataBase.GetHttpRequestExceptionForResponse(BatchResponseMessage);
        }

        /// <summary>
        /// Gets the first <see cref="ItemsInError"/> <see cref="HttpRequestException"/> for the batch where <see cref="HasItemErrors"/>.
        /// </summary>
        /// <returns>The first <see cref="HttpRequestException"/> where <see cref="HasItemErrors"/>; otherwise, <c>null</c>.</returns>
        public HttpRequestException GetItemsHttpRequestException()
        {
            if (!HasItemErrors || BatchResponseMessage == null)
                return null;

            foreach (var e in ItemsInError)
            {
                return ODataBase.GetHttpRequestExceptionForResponse(e.ResponseMessage);
            }

            return null;
        }

        /// <summary>
        /// Indicates whether the batch HTTP response was successful (see <see cref="BatchResponseMessage"/>).
        /// </summary>
        public bool IsSuccessStatusCode { get => BatchResponseMessage == null ? true : BatchResponseMessage.IsSuccessStatusCode; }

        /// <summary>
        /// Gets the batch <see cref="HttpStatusCode"/> (see <see cref="BatchResponseMessage"/>).
        /// </summary>
        public HttpStatusCode StatusCode { get => BatchResponseMessage == null ? HttpStatusCode.OK : BatchResponseMessage.StatusCode; }
    }
}
