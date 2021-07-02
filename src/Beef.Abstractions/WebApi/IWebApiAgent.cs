// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Beef.WebApi
{
    /// <summary>
    /// Provides the base Web API Agent capabilities.
    /// </summary>
    public interface IWebApiAgent
    {
        /// <summary>
        /// Send a <see cref="HttpMethod.Get"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult> GetAsync(string? urlSuffix, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null);

        /// <summary>
        /// Send a <see cref="HttpMethod.Get"/> request as an asynchronous operation with an expected response.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        Task<WebApiAgentResult<TResult>> GetAsync<TResult>(string? urlSuffix, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null);

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
        Task<WebApiAgentResult<TResult>> GetCollectionResultAsync<TResult, TColl, TEntity>(string? urlSuffix, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null)
            where TResult : IEntityCollectionResult<TColl, TEntity>, new()
            where TColl : IEnumerable<TEntity>, new()
            where TEntity : class;

        /// <summary>
        /// Send a <see cref="HttpMethod.Put"/> <paramref name="value"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="value">The content value.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult> PutAsync(string? urlSuffix, object value, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null);

        /// <summary>
        /// Send a <see cref="HttpMethod.Put"/> <paramref name="value"/> request as an asynchronous operation with an expected response.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="value">The content value.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        Task<WebApiAgentResult<TResult>> PutAsync<TResult>(string? urlSuffix, object value, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null);

        /// <summary>
        /// Send a <see cref="HttpMethod.Put"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult> PutAsync(string? urlSuffix, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null);

        /// <summary>
        /// Send a <see cref="HttpMethod.Put"/> request as an asynchronous operation with an expected response.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        Task<WebApiAgentResult<TResult>> PutAsync<TResult>(string? urlSuffix, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null);

        /// <summary>
        /// Send a <see cref="HttpMethod.Post"/> <paramref name="value"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="value">The content value.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult> PostAsync(string? urlSuffix, object value, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null);

        /// <summary>
        /// Send a <see cref="HttpMethod.Post"/> <paramref name="value"/> request as an asynchronous operation with an expected response.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="value">The content value.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        Task<WebApiAgentResult<TResult>> PostAsync<TResult>(string? urlSuffix, object value, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null);

        /// <summary>
        /// Send a <see cref="HttpMethod.Post"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult"/>.</returns>
        Task<WebApiAgentResult> PostAsync(string? urlSuffix, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null);

        /// <summary>
        /// Send a <see cref="HttpMethod.Post"/> request as an asynchronous operation with an expected response.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        Task<WebApiAgentResult<TResult>> PostAsync<TResult>(string? urlSuffix, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null);

        /// <summary>
        /// Send a <see cref="HttpMethod.Delete"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult{T}"/>.</returns>
        Task<WebApiAgentResult> DeleteAsync(string? urlSuffix, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null);

        /// <summary>
        /// Send a <see cref="HttpMethod"/> <b>PATCH</b> <paramref name="json"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="patchOption">The <see cref="WebApiPatchOption"/>.</param>
        /// <param name="json">The json value.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="urlSuffix"/>.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        Task<WebApiAgentResult> PatchAsync(string? urlSuffix, WebApiPatchOption patchOption, JToken json, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null);

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
        Task<WebApiAgentResult<TResult>> PatchAsync<TResult>(string? urlSuffix, WebApiPatchOption patchOption, JToken json, WebApiRequestOptions? requestOptions = null, WebApiArg[]? args = null);
    }
}