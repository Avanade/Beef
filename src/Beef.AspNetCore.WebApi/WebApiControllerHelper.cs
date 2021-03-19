// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.WebApi;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Beef.AspNetCore.WebApi
{
    /// <summary>
    /// Helper function for <b>Web API</b> capabilities.
    /// </summary>
    public static class WebApiControllerHelper
    {
        /// <summary>
        /// Sets the <see cref="HttpResponse.Headers"/> <see cref="System.Net.Http.Headers.HttpResponseHeaders.ETag"/> where the <paramref name="value"/> implements <see cref="IETag"/>.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponse"/> to update.</param>
        /// <param name="value">The value that implements the <see cref="IETag.ETag"/> (where the <paramref name="value"/> does not implement an
        /// <see cref="IETag.ETag"/> the <b>ETag</b> Header will not be set).</param>
        public static void SetETag(HttpResponse response, object value)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (value != null && value is IETag tag)
                SetETag(response, tag?.ETag);
        }

        /// <summary>
        /// Sets the <see cref="HttpResponse.Headers"/> <see cref="System.Net.Http.Headers.HttpResponseHeaders.ETag"/> where the <paramref name="eTag"/> has a value.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponse"/> to update.</param>
        /// <param name="eTag">The <b>ETag</b> <see cref="String"/>.</param>
        public static void SetETag(HttpResponse response, string? eTag)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (!string.IsNullOrEmpty(eTag))
                response.GetTypedHeaders().ETag = new EntityTagHeaderValue(eTag.StartsWith("\"", StringComparison.OrdinalIgnoreCase) ? eTag : "\"" + eTag + "\"");
        }

        /// <summary>
        /// Sets the <see cref="HttpResponse.Headers"/> for the <see cref="PagingResult"/>.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponse"/> to update.</param>
        /// <param name="paging">The <see cref="PagingResult"/> value to be added to the headers; <c>null</c> indicates to remove.</param>
        public static void SetPaging(HttpResponse response, PagingResult? paging)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (paging == null)
                return;

            if (paging.IsSkipTake)
            {
                response.Headers[WebApiConsts.PagingSkipHeaderName] = paging.Skip.ToString(System.Globalization.CultureInfo.InvariantCulture);
                response.Headers[WebApiConsts.PagingTakeHeaderName] = paging.Take.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                response.Headers[WebApiConsts.PagingPageNumberHeaderName] = paging.Page!.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                response.Headers[WebApiConsts.PagingPageSizeHeaderName] = paging.Take.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            if (paging.TotalCount.HasValue)
                response.Headers[WebApiConsts.PagingTotalCountHeaderName] = paging.TotalCount.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);

            if (paging.TotalPages.HasValue)
                response.Headers[WebApiConsts.PagingTotalPagesHeaderName] = paging.TotalPages.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Sets the <see cref="HttpResponse.Headers"/> for the <see cref="PagingResult"/> where the <paramref name="value"/> implements <see cref="IPagingResult"/>.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponse"/> to update.</param>
        /// <param name="value">The value that implements <see cref="IPagingResult"/> (where the <paramref name="value"/> does not implement an
        /// <see cref="IPagingResult"/> the paging headers will not be set).</param>
        public static void SetPaging(HttpResponse response, object? value)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (value != null && value is IPagingResult result)
                SetPaging(response, result.Paging);
        }

        /// <summary>
        /// Sets the <see cref="HttpResponse.Headers"/> for the <see cref="ExecutionContext"/> (see <see cref="ExecutionContext.Messages"/> and <see cref="ExecutionContext.ETag"/>).
        /// </summary>
        /// <param name="response">The <see cref="HttpResponse"/> to update.</param>
        public static void SetExecutionContext(HttpResponse response)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (!ExecutionContext.HasCurrent)
                return;

            if (ExecutionContext.Current.Messages != null && ExecutionContext.Current.Messages.Count > 0)
                SetMessages(response, ExecutionContext.Current.Messages);

            SetETag(response, ExecutionContext.Current);
        }

        /// <summary>
        /// Sets the <see cref="HttpResponse.Headers"/> for the <see cref="MessageItemCollection"/>.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponse"/>.</param>
        /// <param name="messages">The <see cref="MessageItemCollection"/>.</param>
        public static void SetMessages(HttpResponse response, MessageItemCollection messages)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (messages != null && messages.Count > 0)
                response.Headers[WebApiConsts.MessagesHeaderName] = JsonConvert.SerializeObject(messages);
        }

        /// <summary>
        /// Sets the <see cref="HttpResponse.Headers"/> <see cref="System.Net.Http.Headers.HttpResponseHeaders.Location"/> where the <paramref name="locationUri"/> has a value.
        /// </summary>
        /// <param name="response">The <see cref="HttpResponse"/> to update.</param>
        /// <param name="locationUri">The <see cref="System.Net.Http.Headers.HttpResponseHeaders.Location"/> <see cref="Uri"/>.</param>
        public static void SetLocation(HttpResponse response, Uri? locationUri)
        {
            if (response == null)
                throw new ArgumentNullException(nameof(response));

            if (locationUri == null)
                return;

            response.GetTypedHeaders().Location = locationUri;
        }
    }
}