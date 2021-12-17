﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Beef.WebApi
{
    /// <summary>
    /// Provides the base capabilites to <b>invoke</b> Web API agent operations.
    /// </summary>
    public abstract class WebApiAgentBase : WebApiAgentCoreBase, IWebApiAgent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiAgentBase"/> class.
        /// </summary>
        /// <param name="args">The <see cref="IWebApiAgentArgs"/>.</param>
        protected WebApiAgentBase(IWebApiAgentArgs args) : base(args) { }

        /// <summary>
        /// Send a <see cref="HttpMethod.Get"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult"/>.</returns>
        public async Task<WebApiAgentResult> GetAsync(string? urlSuffix, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null)
        {
            var uri = CreateFullUri(urlSuffix, args, requestOptions);
            return await WebApiAgentInvoker.Current.InvokeAsync(this, async () =>
            {
                var value = args?.Where(x => x.ArgType == WebApiArgType.FromBody).SingleOrDefault()?.GetValue();
                var result = new WebApiAgentResult(await Args.HttpClient.SendAsync(await CreateRequestMessageAsync(HttpMethod.Get, uri, CreateJsonContentFromValue(value), requestOptions).ConfigureAwait(false)).ConfigureAwait(false));
                result.Content = await result.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return VerifyResult(result);
            }, null!).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Get"/> request as an asynchronous operation with an expected response.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        public async Task<WebApiAgentResult<TResult>> GetAsync<TResult>(string? urlSuffix, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null)
        {
            var uri = CreateFullUri(urlSuffix, args, requestOptions);
            return await WebApiAgentInvoker.Current.InvokeAsync(this, async () =>
            {
                var value = args?.Where(x => x.ArgType == WebApiArgType.FromBody).SingleOrDefault()?.GetValue();
                var result = new WebApiAgentResult(await Args.HttpClient.SendAsync(await CreateRequestMessageAsync(HttpMethod.Get, uri, CreateJsonContentFromValue(value), requestOptions).ConfigureAwait(false)).ConfigureAwait(false));
                result.Content = await result.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return new WebApiAgentResult<TResult>(VerifyResult(result));
            }, null!).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Get"/> request as an asynchronous operation converting the expected collection response to a <see cref="EntityCollectionResult{TColl, TEntity}"/>.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        public async Task<WebApiAgentResult<TResult>> GetCollectionResultAsync<TResult, TColl, TEntity>(string? urlSuffix, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null)
            where TResult : IEntityCollectionResult<TColl, TEntity>, new()
            where TColl : IEnumerable<TEntity>, new()
            where TEntity : class
        {
            var result = await GetAsync<TColl>(urlSuffix, requestOptions, args).ConfigureAwait(false);
            if (!result.Response.IsSuccessStatusCode)
                return new WebApiAgentResult<TResult>(result);

            var collResult = new TResult()
            {
                Result = result.Value
            };

            var skip = GetHeaderValueLong(result, WebApiConsts.PagingSkipHeaderName);
            var page = skip.HasValue ? null : GetHeaderValueLong(result, WebApiConsts.PagingPageNumberHeaderName);

            if (skip.HasValue)
                collResult.Paging = new PagingResult(PagingResult.CreateSkipAndTake(skip.Value, GetHeaderValueLong(result, WebApiConsts.PagingTakeHeaderName)));
            else if (page.HasValue && result.Response.Headers.Contains(WebApiConsts.PagingPageNumberHeaderName))
                collResult.Paging = new PagingResult(PagingResult.CreatePageAndSize(page.Value, GetHeaderValueLong(result, WebApiConsts.PagingPageSizeHeaderName)));

            if (collResult.Paging != null)
                collResult.Paging.TotalCount = GetHeaderValueLong(result, WebApiConsts.PagingTotalCountHeaderName);

            return new WebApiAgentResult<TResult>(result, collResult);
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Put"/> <paramref name="value"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="value">The content value.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult"/>.</returns>
        public async Task<WebApiAgentResult> PutAsync(string? urlSuffix, object value, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var uri = CreateFullUri(urlSuffix, args, requestOptions);
            if (args != null && args.Any(x => x.ArgType == WebApiArgType.FromBody))
                throw new ArgumentException("No arguments can be marked as IsFromBody where a content value is used.", nameof(args));

            return await WebApiAgentInvoker.Current.InvokeAsync(this, async () =>
            {
                var result = new WebApiAgentResult(await Args.HttpClient.SendAsync(await CreateRequestMessageAsync(HttpMethod.Put, uri, CreateJsonContentFromValue(value), requestOptions).ConfigureAwait(false)).ConfigureAwait(false));
                result.Content = await result.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return VerifyResult(result);
            }, value).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Put"/> <paramref name="value"/> request as an asynchronous operation with an expected response.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="value">The content value.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        public async Task<WebApiAgentResult<TResult>> PutAsync<TResult>(string? urlSuffix, object value, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var uri = CreateFullUri(urlSuffix, args, requestOptions);
            if (args != null && args.Any(x => x.ArgType == WebApiArgType.FromBody))
                throw new ArgumentException("No arguments can be marked as IsFromBody where a content value is used.", nameof(args));

            return await WebApiAgentInvoker.Current.InvokeAsync(this, async () =>
            {
                var result = new WebApiAgentResult(await Args.HttpClient.SendAsync(await CreateRequestMessageAsync(HttpMethod.Put, uri, CreateJsonContentFromValue(value), requestOptions).ConfigureAwait(false)).ConfigureAwait(false));
                result.Content = await result.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return new WebApiAgentResult<TResult>(VerifyResult(result));
            }, value).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Put"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult"/>.</returns>
        public async Task<WebApiAgentResult> PutAsync(string? urlSuffix, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null)
        {
            var uri = CreateFullUri(urlSuffix, args, requestOptions);
            return await WebApiAgentInvoker.Current.InvokeAsync(this, async () =>
            {
                var value = args?.Where(x => x.ArgType == WebApiArgType.FromBody).SingleOrDefault()?.GetValue();
                var result = new WebApiAgentResult(await Args.HttpClient.SendAsync(await CreateRequestMessageAsync(HttpMethod.Put, uri, CreateJsonContentFromValue(value), requestOptions).ConfigureAwait(false)).ConfigureAwait(false));
                result.Content = await result.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return VerifyResult(result);
            }, null!).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Put"/> request as an asynchronous operation with an expected response.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        public async Task<WebApiAgentResult<TResult>> PutAsync<TResult>(string? urlSuffix, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null)
        {
            var uri = CreateFullUri(urlSuffix, args, requestOptions);
            return await WebApiAgentInvoker.Current.InvokeAsync(this, async () =>
            {
                var value = args?.Where(x => x.ArgType == WebApiArgType.FromBody).SingleOrDefault()?.GetValue();
                var result = new WebApiAgentResult(await Args.HttpClient.SendAsync(await CreateRequestMessageAsync(HttpMethod.Put, uri, CreateJsonContentFromValue(value), requestOptions).ConfigureAwait(false)).ConfigureAwait(false));
                result.Content = await result.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return new WebApiAgentResult<TResult>(VerifyResult(result));
            }, null!).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Post"/> <paramref name="value"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="value">The content value.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult"/>.</returns>
        public async Task<WebApiAgentResult> PostAsync(string? urlSuffix, object value, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var uri = CreateFullUri(urlSuffix, args, requestOptions);
            if (args != null && args.Any(x => x.ArgType == WebApiArgType.FromBody))
                throw new ArgumentException("No arguments can be marked as IsFromBody where a content value is used.", nameof(args));

            return await WebApiAgentInvoker.Current.InvokeAsync(this, async () =>
            {
                var result = new WebApiAgentResult(await Args.HttpClient.SendAsync(await CreateRequestMessageAsync(HttpMethod.Post, uri, CreateJsonContentFromValue(value), requestOptions).ConfigureAwait(false)).ConfigureAwait(false));
                result.Content = await result.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return VerifyResult(result);
            }, value).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Post"/> <paramref name="value"/> request as an asynchronous operation with an expected response.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="value">The content value.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        public async Task<WebApiAgentResult<TResult>> PostAsync<TResult>(string? urlSuffix, object value, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var uri = CreateFullUri(urlSuffix, args, requestOptions);
            if (args != null && args.Any(x => x.ArgType == WebApiArgType.FromBody))
                throw new ArgumentException("No arguments can be marked as IsFromBody where a content value is used.", nameof(args));

            return await WebApiAgentInvoker.Current.InvokeAsync(this, async () =>
            {
                var result = new WebApiAgentResult(await Args.HttpClient.SendAsync(await CreateRequestMessageAsync(HttpMethod.Post, uri, CreateJsonContentFromValue(value), requestOptions).ConfigureAwait(false)).ConfigureAwait(false));
                result.Content = await result.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return new WebApiAgentResult<TResult>(VerifyResult(result));
            }, value).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Post"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult"/>.</returns>
        public async Task<WebApiAgentResult> PostAsync(string? urlSuffix, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null)
        {
            var uri = CreateFullUri(urlSuffix, args, requestOptions);

            return await WebApiAgentInvoker.Current.InvokeAsync(this, async () =>
            {
                var value = args?.Where(x => x.ArgType == WebApiArgType.FromBody).SingleOrDefault()?.GetValue();
                var result = new WebApiAgentResult(await Args.HttpClient.SendAsync(await CreateRequestMessageAsync(HttpMethod.Post, uri, CreateJsonContentFromValue(value), requestOptions).ConfigureAwait(false)).ConfigureAwait(false));
                result.Content = await result.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return VerifyResult(result);
            }, null!).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Post"/> request as an asynchronous operation with an expected response.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        public async Task<WebApiAgentResult<TResult>> PostAsync<TResult>(string? urlSuffix, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null)
        {
            var uri = CreateFullUri(urlSuffix, args, requestOptions);

            return await WebApiAgentInvoker.Current.InvokeAsync(this, async () =>
            {
                var value = args?.Where(x => x.ArgType == WebApiArgType.FromBody).SingleOrDefault()?.GetValue();
                var result = new WebApiAgentResult(await Args.HttpClient.SendAsync(await CreateRequestMessageAsync(HttpMethod.Post, uri, CreateJsonContentFromValue(value), requestOptions).ConfigureAwait(false)).ConfigureAwait(false));
                result.Content = await result.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return new WebApiAgentResult<TResult>(VerifyResult(result));
            }, null!).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Delete"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult{T}"/>.</returns>
        public async Task<WebApiAgentResult> DeleteAsync(string? urlSuffix, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null)
        {
            var uri = CreateFullUri(urlSuffix, args, requestOptions);
            return await WebApiAgentInvoker.Current.InvokeAsync(this, async () =>
            {
                var result = new WebApiAgentResult(await Args.HttpClient.SendAsync(await CreateRequestMessageAsync(HttpMethod.Delete, uri, requestOptions: requestOptions).ConfigureAwait(false)).ConfigureAwait(false));
                result.Content = await result.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return VerifyResult(result);
            }, null!).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a <see cref="HttpMethod"/> <b>PATCH</b> <paramref name="json"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="patchOption">The <see cref="WebApiPatchOption"/>.</param>
        /// <param name="json">The json value.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        public async Task<WebApiAgentResult> PatchAsync(string? urlSuffix, WebApiPatchOption patchOption, JToken json, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            if (patchOption == WebApiPatchOption.NotSpecified)
                throw new ArgumentException("A valid patch option must be specified.", nameof(patchOption));

            var uri = CreateFullUri(urlSuffix, args, requestOptions);
            if (args != null && args.Any(x => x.ArgType == WebApiArgType.FromBody))
                throw new ArgumentException("No arguments can be marked as IsFromBody for a PATCH.", nameof(args));

            return await WebApiAgentInvoker.Current.InvokeAsync(this, async () =>
            {
                var content = new StringContent(json.ToString());
                content.Headers.ContentType = MediaTypeHeaderValue.Parse(patchOption == WebApiPatchOption.JsonPatch ? "application/json-patch+json" : "application/merge-patch+json");
                var result = new WebApiAgentResult(await Args.HttpClient.SendAsync(await CreateRequestMessageAsync(new HttpMethod("PATCH"), uri, content, requestOptions).ConfigureAwait(false)).ConfigureAwait(false));
                result.Content = await result.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return VerifyResult(result);
            }, json).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a <see cref="HttpMethod"/> <b>PATCH</b> <paramref name="json"/> request as an asynchronous operation with an expected response.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="patchOption">The <see cref="WebApiPatchOption"/>.</param>
        /// <param name="json">The json value.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        public async Task<WebApiAgentResult<TResult>> PatchAsync<TResult>(string? urlSuffix, WebApiPatchOption patchOption, JToken json, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            var uri = CreateFullUri(urlSuffix, args, requestOptions);
            if (args != null && args.Any(x => x.ArgType == WebApiArgType.FromBody))
                throw new ArgumentException("No arguments can be marked as IsFromBody for a PATCH.", nameof(args));

            return await WebApiAgentInvoker.Current.InvokeAsync(this, async () =>
            {
                var content = new StringContent(json.ToString());
                content.Headers.ContentType = MediaTypeHeaderValue.Parse(patchOption == WebApiPatchOption.JsonPatch ? "application/json-patch+json" : "application/merge-patch+json");
                var result = new WebApiAgentResult(await Args.HttpClient.SendAsync(await CreateRequestMessageAsync(new HttpMethod("PATCH"), uri, content, requestOptions).ConfigureAwait(false)).ConfigureAwait(false));
                result.Content = await result.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return new WebApiAgentResult<TResult>(VerifyResult(result));
            }, json).ConfigureAwait(false);
        }
    }
}