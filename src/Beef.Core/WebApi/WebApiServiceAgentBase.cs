// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Beef.WebApi
{
    /// <summary>
    /// Extends <see cref="WebApiServiceAgentBase"/> adding <see cref="Register"/> and <see cref="Default"/> capabilities.
    /// </summary>
    /// <remarks>Each <b>invoke</b> is wrapped by a <see cref="WebApiServiceAgentInvoker"/> to support additional logic where required.
    /// Also, each <b>invoke</b> is further wrapped by a <see cref="WebApiPerformanceTimer"/> to capture performance diagnostics.</remarks>
    public abstract class WebApiServiceAgentBase<TDefault> : WebApiServiceAgentBase where TDefault : WebApiServiceAgentBase<TDefault>
    {
        private static readonly object _lock = new object();
        private static Func<TDefault> _create;

        /// <summary>
        /// Registers the <see cref="HttpClient"/> <see cref="System.Net.Http.HttpClient"/> to be used as the default for all requests.
        /// </summary>
        /// <param name="create">The <see cref="Func{TDefault}"/> to create the <see cref="Default"/> instance.</param>
        /// <param name="overrideExisting">Indicates whether to override the existing where already set.</param>
        public static void Register(Func<TDefault> create, bool overrideExisting = true)
        {
            lock (_lock)
            {
                if (_create != null && !overrideExisting)
                    return;

                _create = create ?? throw new ArgumentNullException(nameof(create));
            }
        }

        /// <summary>
        /// Gets or sets the default <see cref="WebApiServiceAgentBase{TDefault}"/> instance.
        /// </summary>
        public static TDefault Default { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiServiceAgentBase{TDefault}"/> class.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/>.</param>
        /// <param name="beforeRequest">The <see cref="Action{HttpRequestMessage}"/> to invoke before the <see cref="HttpRequestMessage">Http Request</see> is made (see <see cref="WebApiServiceAgentBase.BeforeRequest"/>).</param>
        public WebApiServiceAgentBase(HttpClient client = null, Action<HttpRequestMessage> beforeRequest = null) : base(client ?? Default?.Client, beforeRequest ?? Default?.BeforeRequest, () => _create?.Invoke()) { }
    }

    /// <summary>
    /// Provides the base service agent capabilites to <b>invoke</b> Web API operations.
    /// </summary>
    public abstract class WebApiServiceAgentBase
    {
        /// <summary>
        /// Sets the accept header for the <paramref name="httpClient"/> to 'application/json'. 
        /// </summary>
        /// <param name="httpClient">The <see cref="System.Net.Http.HttpClient"/>.</param>
        public static void SetAcceptApplicationJson(HttpClient httpClient)
        {
            if (httpClient == null)
                throw new ArgumentNullException(nameof(httpClient));

            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiServiceAgentBase{TDefault}"/> class.
        /// </summary>
        /// <param name="client">The <see cref="HttpClient"/>.</param>
        /// <param name="beforeRequest">The <see cref="Action{HttpRequestMessage}"/> to invoke before the <see cref="HttpRequestMessage">Http Request</see> is made (see <see cref="WebApiServiceAgentBase.BeforeRequest"/>).</param>
        /// <param name="registeredCreate">The registered <i>create</i> to determine the <see cref="HttpClient"/> used where <paramref name="client"/> is <c>null</c>.</param>
        public WebApiServiceAgentBase(HttpClient client = null, Action<HttpRequestMessage> beforeRequest = null, Func<WebApiServiceAgentBase> registeredCreate = null)
        {
            if (client == null)
            {
                var sa = registeredCreate?.Invoke();
                if (sa == null || sa.Client == null)
                    throw new InvalidOperationException("The client or registeredCreate arguments must ");

                Client = sa.Client;
                BeforeRequest = sa.BeforeRequest;
            }
            else
            {
                Client = client;
                BeforeRequest = beforeRequest;
            }
        }

        /// <summary>
        /// Gets the underlying <see cref="HttpClient"/>.
        /// </summary>
        public HttpClient Client { get; private set; }

        /// <summary>
        /// Gets the <see cref="Action"/> to invoke before the <see cref="HttpRequestMessage">Http Request</see> is made.
        /// </summary>
        /// <remarks>Represents an opportunity to add to the request headers for example.</remarks>
        public Action<HttpRequestMessage> BeforeRequest { get; private set; }

        /// <summary>
        /// Creates the <see cref="HttpRequestMessage"/> and invokes the <see cref="BeforeRequest"/>.
        /// </summary>
        private HttpRequestMessage CreateRequestMessage(HttpMethod method, Uri uri, StringContent content = null, WebApiRequestOptions requestOptions = null)
        {
            var req = new HttpRequestMessage(method, uri);
            if (content != null)
                req.Content = content;

            ApplyWebApiOptions(req, requestOptions);

            BeforeRequest?.Invoke(req);
            return req;
        }

        /// <summary>
        /// Applys the <see cref="WebApiRequestOptions"/> to the<see cref="HttpRequestMessage"/>.
        /// </summary>
        private void ApplyWebApiOptions(HttpRequestMessage request, WebApiRequestOptions requestOptions = null)
        {
            if (requestOptions == null || string.IsNullOrEmpty(requestOptions.ETag))
                return;

            if (request.Method == HttpMethod.Get)
                request.Headers.IfNoneMatch.Add(new System.Net.Http.Headers.EntityTagHeaderValue("\"" + requestOptions.ETag + "\""));
            else
                request.Headers.IfMatch.Add(new System.Net.Http.Headers.EntityTagHeaderValue("\"" + requestOptions.ETag + "\""));
        }

        #region Get/Put/Post/Delete Async

        /// <summary>
        /// Send a <see cref="HttpMethod.Get"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="url">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="url"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The <see cref="WebApiAgentResult"/>.</returns>
        public async Task<WebApiAgentResult> GetAsync(string url, WebApiRequestOptions requestOptions = null, WebApiArg[] args = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            var uri = CreateFullUri(url, args);
            using (var pt = new WebApiPerformanceTimer(uri.AbsoluteUri))
            {
                return await WebApiServiceAgentInvoker.Default.InvokeAsync(this, async () =>
                {
                    var value = args?.Where(x => x.ArgType == WebApiArgType.FromBody).SingleOrDefault()?.GetValue();
                    var result = new WebApiAgentResult(await Client.SendAsync(CreateRequestMessage(HttpMethod.Get, uri, CreateJsonContentFromValue(value), requestOptions)).ConfigureAwait(false));
                    result.Content = await result.Response.Content.ReadAsStringAsync();
                    return VerifyResult(result);
                }, null, memberName, filePath, lineNumber);
            }
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Get"/> request as an asynchronous operation with an expected response.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="url">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="url"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        public async Task<WebApiAgentResult<TResult>> GetAsync<TResult>(string url, WebApiRequestOptions requestOptions = null, WebApiArg[] args = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            var uri = CreateFullUri(url, args);
            using (var pt = new WebApiPerformanceTimer(uri.AbsoluteUri))
            {
                return await WebApiServiceAgentInvoker.Default.InvokeAsync(this, async () =>
                {
                    var value = args?.Where(x => x.ArgType == WebApiArgType.FromBody).SingleOrDefault()?.GetValue();
                    var result = new WebApiAgentResult(await Client.SendAsync(CreateRequestMessage(HttpMethod.Get, uri, CreateJsonContentFromValue(value), requestOptions)).ConfigureAwait(false));
                    result.Content = await result.Response.Content.ReadAsStringAsync();
                    return new WebApiAgentResult<TResult>(VerifyResult(result));
                }, null, memberName, filePath, lineNumber);
            }
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Get"/> request as an asynchronous operation converting the expected collection response to a <see cref="EntityCollectionResult{TColl, TEntity}"/>.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TEntity">The entity <see cref="Type"/>.</typeparam>
        /// <param name="url">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="url"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        public async Task<WebApiAgentResult<TResult>> GetCollectionResultAsync<TResult, TColl, TEntity>(string url, WebApiRequestOptions requestOptions = null, WebApiArg[] args = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
            where TResult : EntityCollectionResult<TColl, TEntity>, new()
            where TColl : EntityBaseCollection<TEntity>, new()
            where TEntity : EntityBase
        {
            var result = await GetAsync<TColl>(url, requestOptions, args, memberName, filePath, lineNumber);
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
            else if (result.Response.Headers.Contains(WebApiConsts.PagingPageNumberHeaderName))
                collResult.Paging = new PagingResult(PagingResult.CreatePageAndSize(page.Value, GetHeaderValueLong(result, WebApiConsts.PagingPageSizeHeaderName)));

            if (collResult.Paging != null)
                collResult.Paging.TotalCount = GetHeaderValueLong(result, WebApiConsts.PagingTotalCountHeaderName);

            return new WebApiAgentResult<TResult>(result, collResult);
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Put"/> <paramref name="value"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="url">The url suffix for the operation.</param>
        /// <param name="value">The content value.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="url"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The <see cref="WebApiAgentResult"/>.</returns>
        public async Task<WebApiAgentResult> PutAsync(string url, object value, WebApiRequestOptions requestOptions = null, WebApiArg[] args = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var uri = CreateFullUri(url, args);
            if (args != null && args.Count(x => x.ArgType == WebApiArgType.FromBody) > 0)
                throw new ArgumentException("No arguments can be marked as IsFromBody where a content value is used.", nameof(args));

            using (var pt = new WebApiPerformanceTimer(uri.AbsoluteUri))
            {
                return await WebApiServiceAgentInvoker.Default.InvokeAsync(this, async () =>
                {
                    var result = new WebApiAgentResult(await Client.SendAsync(CreateRequestMessage(HttpMethod.Put, uri, CreateJsonContentFromValue(value), requestOptions)).ConfigureAwait(false));
                    result.Content = await result.Response.Content.ReadAsStringAsync();
                    return VerifyResult(result);
                }, value, memberName, filePath, lineNumber);
            }
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Put"/> <paramref name="value"/> request as an asynchronous operation with an expected response.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="url">The url suffix for the operation.</param>
        /// <param name="value">The content value.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="url"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        public async Task<WebApiAgentResult<TResult>> PutAsync<TResult>(string url, object value, WebApiRequestOptions requestOptions = null, WebApiArg[] args = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var uri = CreateFullUri(url, args);
            if (args != null && args.Count(x => x.ArgType == WebApiArgType.FromBody) > 0)
                throw new ArgumentException("No arguments can be marked as IsFromBody where a content value is used.", nameof(args));

            using (var pt = new WebApiPerformanceTimer(uri.AbsoluteUri))
            {
                return await WebApiServiceAgentInvoker.Default.InvokeAsync(this, async () =>
                {
                    var result = new WebApiAgentResult(await Client.SendAsync(CreateRequestMessage(HttpMethod.Put, uri, CreateJsonContentFromValue(value), requestOptions)).ConfigureAwait(false));
                    result.Content = await result.Response.Content.ReadAsStringAsync();
                    return new WebApiAgentResult<TResult>(VerifyResult(result));
                }, value, memberName, filePath, lineNumber);
            }
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Put"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="url">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="url"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The <see cref="WebApiAgentResult"/>.</returns>
        public async Task<WebApiAgentResult> PutAsync(string url, WebApiRequestOptions requestOptions = null, WebApiArg[] args = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            var uri = CreateFullUri(url, args);
            using (var pt = new WebApiPerformanceTimer(uri.AbsoluteUri))
            {
                return await WebApiServiceAgentInvoker.Default.InvokeAsync(this, async () =>
                {
                    var value = args?.Where(x => x.ArgType == WebApiArgType.FromBody).SingleOrDefault()?.GetValue();
                    var result = new WebApiAgentResult(await Client.SendAsync(CreateRequestMessage(HttpMethod.Put, uri, CreateJsonContentFromValue(value), requestOptions)).ConfigureAwait(false));
                    result.Content = await result.Response.Content.ReadAsStringAsync();
                    return VerifyResult(result);
                }, null, memberName, filePath, lineNumber);
            }
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Put"/> request as an asynchronous operation with an expected response.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="url">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="url"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        public async Task<WebApiAgentResult<TResult>> PutAsync<TResult>(string url, WebApiRequestOptions requestOptions = null, WebApiArg[] args = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            var uri = CreateFullUri(url, args);
            using (var pt = new WebApiPerformanceTimer(uri.AbsoluteUri))
            {
                return await WebApiServiceAgentInvoker.Default.InvokeAsync(this, async () =>
                {
                    var value = args?.Where(x => x.ArgType == WebApiArgType.FromBody).SingleOrDefault()?.GetValue();
                    var result = new WebApiAgentResult(await Client.SendAsync(CreateRequestMessage(HttpMethod.Put, uri, CreateJsonContentFromValue(value), requestOptions)).ConfigureAwait(false));
                    result.Content = await result.Response.Content.ReadAsStringAsync();
                    return new WebApiAgentResult<TResult>(VerifyResult(result));
                }, null, memberName, filePath, lineNumber);
            }
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Post"/> <paramref name="value"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="url">The url suffix for the operation.</param>
        /// <param name="value">The content value.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="url"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The <see cref="WebApiAgentResult"/>.</returns>
        public async Task<WebApiAgentResult> PostAsync(string url, object value, WebApiRequestOptions requestOptions = null, WebApiArg[] args = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var uri = CreateFullUri(url, args);
            if (args != null && args.Count(x => x.ArgType == WebApiArgType.FromBody) > 0)
                throw new ArgumentException("No arguments can be marked as IsFromBody where a content value is used.", nameof(args));

            using (var pt = new WebApiPerformanceTimer(uri.AbsoluteUri))
            {
                return await WebApiServiceAgentInvoker.Default.InvokeAsync(this, async () =>
                {
                    var result = new WebApiAgentResult(await Client.SendAsync(CreateRequestMessage(HttpMethod.Post, uri, CreateJsonContentFromValue(value), requestOptions)).ConfigureAwait(false));
                    result.Content = await result.Response.Content.ReadAsStringAsync();
                    return VerifyResult(result);
                }, value, memberName, filePath, lineNumber);
            }
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Post"/> <paramref name="value"/> request as an asynchronous operation with an expected response.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="url">The url suffix for the operation.</param>
        /// <param name="value">The content value.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="url"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        public async Task<WebApiAgentResult<TResult>> PostAsync<TResult>(string url, object value, WebApiRequestOptions requestOptions = null, WebApiArg[] args = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var uri = CreateFullUri(url, args);
            if (args != null && args.Count(x => x.ArgType == WebApiArgType.FromBody) > 0)
                throw new ArgumentException("No arguments can be marked as IsFromBody where a content value is used.", nameof(args));

            using (var pt = new WebApiPerformanceTimer(uri.AbsoluteUri))
            {
                return await WebApiServiceAgentInvoker.Default.InvokeAsync(this, async () =>
                {
                    var result = new WebApiAgentResult(await Client.SendAsync(CreateRequestMessage(HttpMethod.Post, uri, CreateJsonContentFromValue(value), requestOptions)).ConfigureAwait(false));
                    result.Content = await result.Response.Content.ReadAsStringAsync();
                    return new WebApiAgentResult<TResult>(VerifyResult(result));
                }, value, memberName, filePath, lineNumber);
            }
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Post"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="url">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="url"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The <see cref="WebApiAgentResult"/>.</returns>
        public async Task<WebApiAgentResult> PostAsync(string url, WebApiRequestOptions requestOptions = null, WebApiArg[] args = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            var uri = CreateFullUri(url, args);

            using (var pt = new WebApiPerformanceTimer(uri.AbsoluteUri))
            {
                return await WebApiServiceAgentInvoker.Default.InvokeAsync(this, async () =>
                {
                    var value = args?.Where(x => x.ArgType == WebApiArgType.FromBody).SingleOrDefault()?.GetValue();
                    var result = new WebApiAgentResult(await Client.SendAsync(CreateRequestMessage(HttpMethod.Post, uri, CreateJsonContentFromValue(value), requestOptions)).ConfigureAwait(false));
                    result.Content = await result.Response.Content.ReadAsStringAsync();
                    return VerifyResult(result);
                },  null, memberName, filePath, lineNumber);
            }
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Post"/> request as an asynchronous operation with an expected response.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="url">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="url"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        public async Task<WebApiAgentResult<TResult>> PostAsync<TResult>(string url, WebApiRequestOptions requestOptions = null, WebApiArg[] args = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            var uri = CreateFullUri(url, args);

            using (var pt = new WebApiPerformanceTimer(uri.AbsoluteUri))
            {
                return await WebApiServiceAgentInvoker.Default.InvokeAsync(this, async () =>
                {
                    var value = args?.Where(x => x.ArgType == WebApiArgType.FromBody).SingleOrDefault()?.GetValue();
                    var result = new WebApiAgentResult(await Client.SendAsync(CreateRequestMessage(HttpMethod.Post, uri, CreateJsonContentFromValue(value), requestOptions)).ConfigureAwait(false));
                    result.Content = await result.Response.Content.ReadAsStringAsync();
                    return new WebApiAgentResult<TResult>(VerifyResult(result));
                }, null, memberName, filePath, lineNumber);
            }
        }

        /// <summary>
        /// Send a <see cref="HttpMethod.Delete"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="url">The url suffix for the operation.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="url"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The <see cref="WebApiAgentResult{T}"/>.</returns>
        public async Task<WebApiAgentResult> DeleteAsync(string url, WebApiRequestOptions requestOptions = null, WebApiArg[] args = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            var uri = CreateFullUri(url, args);
            using (var pt = new WebApiPerformanceTimer(uri.AbsoluteUri))
            {
                return await WebApiServiceAgentInvoker.Default.InvokeAsync(this, async () =>
                {
                    var result = new WebApiAgentResult(await Client.SendAsync(CreateRequestMessage(HttpMethod.Delete, uri, requestOptions: requestOptions)).ConfigureAwait(false));
                    result.Content = await result.Response.Content.ReadAsStringAsync();
                    return VerifyResult(result);
                }, null, memberName, filePath, lineNumber);
            }
        }

        /// <summary>
        /// Send a <see cref="HttpMethod"/> <b>PATCH</b> <paramref name="json"/> request as an asynchronous operation.
        /// </summary>
        /// <param name="url">The url suffix for the operation.</param>
        /// <param name="patchOption">The <see cref="WebApiPatchOption"/>.</param>
        /// <param name="json">The json value.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="url"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        public async Task<WebApiAgentResult> PatchAsync(string url, WebApiPatchOption patchOption, JToken json, WebApiRequestOptions requestOptions = null, WebApiArg[] args = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            if (patchOption == WebApiPatchOption.NotSpecified)
                throw new ArgumentException("A valid patch option must be specified.", nameof(patchOption));

            var uri = CreateFullUri(url, args);
            if (args != null && args.Count(x => x.ArgType == WebApiArgType.FromBody) > 0)
                throw new ArgumentException("No arguments can be marked as IsFromBody for a PATCH.", nameof(args));

            using (var pt = new WebApiPerformanceTimer(uri.AbsoluteUri))
            {
                return await WebApiServiceAgentInvoker.Default.InvokeAsync(this, async () =>
                {
                    var content = new StringContent(json.ToString());
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse(patchOption == WebApiPatchOption.JsonPatch ? "application/json-patch+json" : "application/merge-patch+json");
                    var result = new WebApiAgentResult(await Client.SendAsync(CreateRequestMessage(new HttpMethod("PATCH"), uri, content, requestOptions)).ConfigureAwait(false));
                    result.Content = await result.Response.Content.ReadAsStringAsync();
                    return VerifyResult(result);
                }, json, memberName, filePath, lineNumber);
            }
        }

        /// <summary>
        /// Send a <see cref="HttpMethod"/> <b>PATCH</b> <paramref name="json"/> request as an asynchronous operation with an expected response.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="url">The url suffix for the operation.</param>
        /// <param name="patchOption">The <see cref="WebApiPatchOption"/>.</param>
        /// <param name="json">The json value.</param>
        /// <param name="requestOptions">The optional <see cref="WebApiRequestOptions"/>.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="url"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The <see cref="WebApiAgentResult{TResult}"/>.</returns>
        public async Task<WebApiAgentResult<TResult>> PatchAsync<TResult>(string url, WebApiPatchOption patchOption, JToken json, WebApiRequestOptions requestOptions = null, WebApiArg[] args = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            if (json == null)
                throw new ArgumentNullException(nameof(json));

            var uri = CreateFullUri(url, args);
            if (args != null && args.Count(x => x.ArgType == WebApiArgType.FromBody) > 0)
                throw new ArgumentException("No arguments can be marked as IsFromBody for a PATCH.", nameof(args));

            using (var pt = new WebApiPerformanceTimer(uri.AbsoluteUri))
            {
                return await WebApiServiceAgentInvoker.Default.InvokeAsync(this, async () =>
                {
                    var content = new StringContent(json.ToString());
                    content.Headers.ContentType = MediaTypeHeaderValue.Parse(patchOption == WebApiPatchOption.JsonPatch ? "application/json-patch+json" : "application/merge-patch+json");
                    var result = new WebApiAgentResult(await Client.SendAsync(CreateRequestMessage(new HttpMethod("PATCH"), uri, content, requestOptions)).ConfigureAwait(false));
                    result.Content = await result.Response.Content.ReadAsStringAsync();
                    return new WebApiAgentResult<TResult>(VerifyResult(result));
                }, json, memberName, filePath, lineNumber);
            }
        }

        #endregion

        /// <summary>
        /// Creates the full <see cref="Uri"/> from concatenating the <see cref="HttpClient.BaseAddress"/> and <paramref name="url"/>.
        /// </summary>
        /// <param name="url">The specific url suffix for the operation.</param>
        /// <param name="args">The operation arguments to be substituted within the <paramref name="url"/>.</param>
        protected Uri CreateFullUri(string url = null, WebApiArg[] args = null)
        {
            // Concatenate the base and specific url strings to form the full Url.
            string fullUrl = Client.BaseAddress.AbsoluteUri;
            if (fullUrl[fullUrl.Length - 1] != '/')
                fullUrl = fullUrl + "/";

            if (!string.IsNullOrEmpty(url))
                fullUrl = fullUrl + ((url[0] == '/') ? url.Substring(1) : url);

            // Replace known url tokens with passed argument values.
            if (args != null)
            {
                if (args.Count(x => x.ArgType == WebApiArgType.FromBody) > 1)
                    throw new ArgumentException("Only a single argument can have an ArgType of FromBody.", nameof(args));

                foreach (var arg in args.Where(x => x.ArgType != WebApiArgType.FromBody))
                {
                    var argUrl = "{" + arg.Name + "}";
                    if (fullUrl.Contains(argUrl))
                    {
                        fullUrl = fullUrl.Replace(argUrl, arg.ToString());
                        arg.IsUsed = true;
                    }
                }

                bool firstTime = !fullUrl.Contains("&");
                foreach (var arg in args.Where(x => !x.IsDefault && x.ArgType != WebApiArgType.FromBody && !x.IsUsed))
                {
                    var argUrl = arg.ToUrlQueryString();
                    if (!string.IsNullOrEmpty(argUrl))
                    {
                        fullUrl = string.Format("{0}{1}{2}", fullUrl, firstTime ? "?" : "&", arg.ToUrlQueryString());
                        firstTime = false;
                    }
                }
            }

            return new Uri(fullUrl.Replace(" ", "%20"));
        }

        /// <summary>
        /// Verify the response status code and handle accordingly.
        /// </summary>
        /// <param name="result">The <see cref="WebApiAgentResult"/> to verify.</param>
        protected WebApiAgentResult VerifyResult(WebApiAgentResult result)
        {
            if (result == null)
                throw new ArgumentNullException("result");

            // Extract any messages sent via the header.
            if (result.Response.Headers.TryGetValues(WebApiConsts.MessagesHeaderName, out IEnumerable<string> msgs) && msgs.Count() > 0)
                result.Messages = JsonConvert.DeserializeObject<MessageItemCollection>(msgs.First());

            // Where the status is considered a-OK then get out of here!
            if (result.Response.IsSuccessStatusCode)
                return result;

            // Determine whether the response contains our (beef) headers.
            if (result.Response.Headers.TryGetValues(WebApiConsts.ErrorTypeHeaderName, out IEnumerable<string> errorTypes) && errorTypes.Count() > 0)
            {
                // Handle and convert the known errors to their corresponding error types.
                if (Enum.TryParse<ErrorType>(errorTypes.First(), out ErrorType errortype))
                {
                    result.ErrorType = errortype;
                    JToken json = null;

                    if (result.Response.Content.Headers.ContentLength == 0)
                    { }
                    else if (result.Response.Content.Headers.ContentType.MediaType != "application/json")
                        result.ErrorMessage = result.Content;
                    else
                    {
                        json = JToken.Parse(result.Content);
                        if (json.Type == JTokenType.String)
                            result.ErrorMessage = json.Value<string>();
                        else if (json.Type == JTokenType.Object)
                            result.ErrorMessage = json["Message"]?.ToObject<string>();
                    }

                    if (result.ErrorType == ErrorType.ValidationError)
                    {
                        if (json != null && json.Type == JTokenType.Object)
                        {
                            foreach (var prop in (json["ModelState"] ?? json).ToObject<Dictionary<string, ICollection<string>>>())
                            {
                                foreach (var text in prop.Value)
                                {
                                    if (result.Messages == null)
                                        result.Messages = new MessageItemCollection();

                                    result.Messages.Add(prop.Key, MessageType.Error, text);
                                }
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Create the content by JSON serializing the request value.
        /// </summary>
        private StringContent CreateJsonContentFromValue(object value)
        {
            if (value == null)
                return null;
            
            var content = new StringContent(JsonConvert.SerializeObject(value));
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            return content;
        }

        /// <summary>
        /// Gets the named header value as a nullable long.
        /// </summary>
        private long? GetHeaderValueLong(WebApiAgentResult result, string name)
        {
            if (!result.Response.Headers.TryGetValues(name, out IEnumerable<string> values) || values.Count() == 0)
                return null;

            if (!long.TryParse(values.First(), out long val))
                return null;

            return val;
        }
    }
}
