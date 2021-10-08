// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using AutoMapper;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Beef.WebApi
{
    /// <summary>
    /// Defines the base HTTP Agent capability for invoking external HTTP RESTful (JSON) endpoints.
    /// </summary>
    public interface IHttpAgent
    {
        #region NoReponseValue

        /// <summary>
        /// Send an HTTP request asynchronously with no request body value or expected corresponding response value.
        /// </summary>
        /// <param name="args">The <see cref="HttpSendArgs"/>.</param>
        /// <returns>The <see cref="HttpAgentResult"/>.</returns>
        Task<HttpAgentResult> SendAsync(HttpSendArgs args);

        /// <summary>
        /// Send an HTTP request asynchronously with request body <paramref name="value"/> expecting no corresponding response value.
        /// </summary>
        /// <typeparam name="TReq">The request <paramref name="value"/> <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="HttpSendArgs"/>.</param>
        /// <param name="value">The request value.</param>
        /// <returns>The <see cref="HttpAgentResult"/>.</returns>
        Task<HttpAgentResult> SendAsync<TReq>(HttpSendArgs args, TReq value);

        /// <summary>
        /// Send an HTTP request asynchronously with request body <paramref name="value"/> (mapped to <typeparamref name="TReqModel"/>) expecting no corresponding response value.
        /// </summary>
        /// <typeparam name="TReq">The request <paramref name="value"/> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TReqModel">The request <paramref name="value"/> model <see cref="Type"/> that is mapped to and sent.</typeparam>
        /// <param name="args">The <see cref="HttpSendArgs"/>.</param>
        /// <param name="value">The request value.</param>
        /// <returns>The <see cref="HttpAgentResult"/>.</returns>
        Task<HttpAgentResult> SendMappedRequestAsync<TReq, TReqModel>(HttpSendArgs args, TReq value);

        #endregion

        #region NoRequestValue

        /// <summary>
        /// Send an HTTP request asynchronously with no request body value expecting a <typeparamref name="TResp"/> response value.
        /// </summary>
        /// <typeparam name="TResp">The response <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="HttpSendArgs"/>.</param>
        /// <returns>The <see cref="HttpAgentResult{TResp}"/>.</returns>
        Task<HttpAgentResult<TResp>> SendAsync<TResp>(HttpSendArgs args);

        /// <summary>
        /// Send an HTTP request asynchronously with no request body value expecting a <typeparamref name="TResp"/> response value (mapped from <typeparamref name="TRespModel"/>).
        /// </summary>
        /// <typeparam name="TResp">The response <see cref="Type"/> (mapped from <typeparamref name="TRespModel"/>).</typeparam>
        /// <typeparam name="TRespModel">The response model <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="HttpSendArgs"/>.</param>

        /// <returns>The <see cref="HttpAgentResult{TResp, TRespModel}"/>.</returns>
        Task<HttpAgentResult<TResp, TRespModel>> SendMappedResponseAsync<TResp, TRespModel>(HttpSendArgs args);

        #endregion

        #region RequestAndResponseValues

        /// <summary>
        /// Send an HTTP request asynchronously with request body <paramref name="value"/> expecting a <typeparamref name="TResp"/> response value.
        /// </summary>
        /// <typeparam name="TReq">The request <paramref name="value"/> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TResp">The response <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="HttpSendArgs"/>.</param>
        /// <param name="value">The request value.</param>
        /// <returns>The <see cref="HttpAgentResult{TResp}"/>.</returns>
        Task<HttpAgentResult<TResp>> SendAsync<TReq, TResp>(HttpSendArgs args, TReq value);

        /// <summary>
        /// Send an HTTP request asynchronously with request body <paramref name="value"/> (mapped to <typeparamref name="TReqModel"/>) expecting a <typeparamref name="TResp"/> response value.
        /// </summary>
        /// <typeparam name="TReq">The request <paramref name="value"/> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TReqModel">The request <paramref name="value"/> model <see cref="Type"/> that is mapped to and sent.</typeparam>
        /// <typeparam name="TResp">The response <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="HttpSendArgs"/>.</param>
        /// <param name="value">The request value.</param>
        /// <returns>The <see cref="HttpAgentResult{TResp}"/>.</returns>
        Task<HttpAgentResult<TResp>> SendMappedRequestAsync<TReq, TReqModel, TResp>(HttpSendArgs args, TReq value);

        /// <summary>
        /// Send an HTTP request asynchronously with request body <paramref name="value"/> expecting a <typeparamref name="TResp"/> response value (mapped from <typeparamref name="TRespModel"/>).
        /// </summary>
        /// <typeparam name="TReq">The request <paramref name="value"/> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TResp">The response <see cref="Type"/> (mapped from <typeparamref name="TRespModel"/>).</typeparam>
        /// <typeparam name="TRespModel">The response model <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="HttpSendArgs"/>.</param>
        /// <param name="value">The request value.</param>
        /// <returns>The <see cref="HttpAgentResult{TResp, TRespModel}"/>.</returns>
        Task<HttpAgentResult<TResp, TRespModel>> SendMappedResponseAsync<TReq, TResp, TRespModel>(HttpSendArgs args, TReq value);

        /// <summary>
        /// Send an HTTP request asynchronously with request body <paramref name="value"/> (mapped to <typeparamref name="TReqModel"/>) expecting a <typeparamref name="TResp"/> response value (mapped from <typeparamref name="TRespModel"/>).
        /// </summary>
        /// <typeparam name="TReq">The request <paramref name="value"/> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TReqModel">The request <paramref name="value"/> model <see cref="Type"/> that is mapped to and sent.</typeparam>
        /// <typeparam name="TResp">The response <see cref="Type"/> (mapped from <typeparamref name="TRespModel"/>).</typeparam>
        /// <typeparam name="TRespModel">The response model <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="HttpSendArgs"/>.</param>
        /// <param name="value">The request value.</param>
        /// <returns>The <see cref="HttpAgentResult{TResp, TRespModel}"/>.</returns>
        Task<HttpAgentResult<TResp, TRespModel>> SendMappedRequestResponseAsync<TReq, TReqModel, TResp, TRespModel>(HttpSendArgs args, TReq value);

        #endregion    
    }

    /// <summary>
    /// Represents the base HTTP Agent capability for invoking external HTTP RESTful (JSON) endpoints.
    /// </summary>
    /// <remarks>Extends the <see cref="WebApiAgentBase"/> adding additional generic <c>Send</c> methods, including <see cref="IMapper"/>-based for request/response mapping, to enable the invocation of external HTTP RESTful (JSON) endpoints.</remarks>
    public abstract class HttpAgentBase : WebApiAgentBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HttpAgentBase"/> class.
        /// </summary>
        /// <param name="args">The <see cref="IHttpAgentArgs"/>.</param>
        public HttpAgentBase(IHttpAgentArgs args) : base(args) { }

        /// <summary>
        /// Gets the <see cref="IHttpAgentArgs"/>.
        /// </summary>
        public new IHttpAgentArgs Args => (IHttpAgentArgs)base.Args;

        /// <summary>
        /// Gets the <see cref="WebApiAgentInvoker"/>. Where not specified will default to <see cref="WebApiAgentInvoker.Current"/>.
        /// </summary>
        public WebApiAgentInvoker? Invoker { get; set; }

        /// <summary>
        /// Check the arguments.
        /// </summary>
        private static HttpSendArgs CheckSendArgs(HttpSendArgs args, bool mapperIsMandatory = false)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (mapperIsMandatory && args.Mapper == null)
                throw new ArgumentException("The Mapper property must not be null.", nameof(args));

            return args;
        }

        /// <summary>
        /// Performs the actual <see cref="HttpClient.SendAsync(HttpRequestMessage)"/> and verifies the result.
        /// </summary>
        private async Task<HttpAgentResult> SendAsyncInternal<TReq>(HttpSendArgs args, TReq value) => await (Invoker ??= WebApiAgentInvoker.Current).InvokeAsync(this, async () =>
        {
            var uri = CreateFullUri(args.UrlSuffix);
            var result = (HttpAgentResult)VerifyResult(new HttpAgentResult(args, await Args.HttpClient.SendAsync(await CreateRequestMessageAsync(args.HttpMethod, uri, CreateJsonContentFromValue(value)).ConfigureAwait(false)).ConfigureAwait(false)));
            result.Content = await result.Response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Args.AfterResponse?.Invoke(result);
            await (Args.AfterResponseAsync?.Invoke(result) ?? Task.CompletedTask).ConfigureAwait(false);
            return result;
        }, null!).ConfigureAwait(false);

        #region NoReponseValue

        /// <summary>
        /// Send an HTTP request asynchronously with no request body value or expected corresponding response value.
        /// </summary>
        /// <param name="args">The <see cref="HttpSendArgs"/>.</param>
        /// <returns>The <see cref="HttpAgentResult"/>.</returns>
        public async Task<HttpAgentResult> SendAsync(HttpSendArgs args)
            => await SendAsyncInternal<object?>(CheckSendArgs(args), null).ConfigureAwait(false);

        /// <summary>
        /// Send an HTTP request asynchronously with request body <paramref name="value"/> expecting no corresponding response value.
        /// </summary>
        /// <typeparam name="TReq">The request <paramref name="value"/> <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="HttpSendArgs"/>.</param>
        /// <param name="value">The request value.</param>
        /// <returns>The <see cref="HttpAgentResult"/>.</returns>
        public async Task<HttpAgentResult> SendAsync<TReq>(HttpSendArgs args, TReq value) 
            => await SendAsyncInternal(CheckSendArgs(args), value).ConfigureAwait(false);

        /// <summary>
        /// Send an HTTP request asynchronously with request body <paramref name="value"/> (mapped to <typeparamref name="TReqModel"/>) expecting no corresponding response value.
        /// </summary>
        /// <typeparam name="TReq">The request <paramref name="value"/> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TReqModel">The request <paramref name="value"/> model <see cref="Type"/> that is mapped to and sent.</typeparam>
        /// <param name="args">The <see cref="HttpSendArgs"/>.</param>
        /// <param name="value">The request value.</param>
        /// <returns>The <see cref="HttpAgentResult"/>.</returns>
        public async Task<HttpAgentResult> SendMappedRequestAsync<TReq, TReqModel>(HttpSendArgs args, TReq value)
            => await SendAsync(CheckSendArgs(args, true), args.Mapper!.Map<TReq, TReqModel>(value)).ConfigureAwait(false);

        #endregion

        #region NoRequestValue

        /// <summary>
        /// Send an HTTP request asynchronously with no request body value expecting a <typeparamref name="TResp"/> response value.
        /// </summary>
        /// <typeparam name="TResp">The response <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="HttpSendArgs"/>.</param>
        /// <returns>The <see cref="HttpAgentResult{TResp}"/>.</returns>
        public async Task<HttpAgentResult<TResp>> SendAsync<TResp>(HttpSendArgs args)
            => new HttpAgentResult<TResp>(await SendAsyncInternal<object?>(CheckSendArgs(args), null).ConfigureAwait(false));

        /// <summary>
        /// Send an HTTP request asynchronously with no request body value expecting a <typeparamref name="TResp"/> response value (mapped from <typeparamref name="TRespModel"/>).
        /// </summary>
        /// <typeparam name="TResp">The response <see cref="Type"/> (mapped from <typeparamref name="TRespModel"/>).</typeparam>
        /// <typeparam name="TRespModel">The response model <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="HttpSendArgs"/>.</param>

        /// <returns>The <see cref="HttpAgentResult{TResp, TRespModel}"/>.</returns>
        public async Task<HttpAgentResult<TResp, TRespModel>> SendMappedResponseAsync<TResp, TRespModel>(HttpSendArgs args)
            => new HttpAgentResult<TResp, TRespModel>(CheckSendArgs(args, true).Mapper!, await SendAsyncInternal<object?>(args, null).ConfigureAwait(false));

        #endregion

        #region RequestAndResponseValues

        /// <summary>
        /// Send an HTTP request asynchronously with request body <paramref name="value"/> expecting a <typeparamref name="TResp"/> response value.
        /// </summary>
        /// <typeparam name="TReq">The request <paramref name="value"/> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TResp">The response <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="HttpSendArgs"/>.</param>
        /// <param name="value">The request value.</param>
        /// <returns>The <see cref="HttpAgentResult{TResp}"/>.</returns>
        public async Task<HttpAgentResult<TResp>> SendAsync<TReq, TResp>(HttpSendArgs args, TReq value)
            => new HttpAgentResult<TResp>(await SendAsyncInternal<TReq>(CheckSendArgs(args), value).ConfigureAwait(false));

        /// <summary>
        /// Send an HTTP request asynchronously with request body <paramref name="value"/> (mapped to <typeparamref name="TReqModel"/>) expecting a <typeparamref name="TResp"/> response value.
        /// </summary>
        /// <typeparam name="TReq">The request <paramref name="value"/> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TReqModel">The request <paramref name="value"/> model <see cref="Type"/> that is mapped to and sent.</typeparam>
        /// <typeparam name="TResp">The response <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="HttpSendArgs"/>.</param>
        /// <param name="value">The request value.</param>
        /// <returns>The <see cref="HttpAgentResult{TResp}"/>.</returns>
        public async Task<HttpAgentResult<TResp>> SendMappedRequestAsync<TReq, TReqModel, TResp>(HttpSendArgs args, TReq value)
            => new HttpAgentResult<TResp>(await SendAsyncInternal(CheckSendArgs(args, true), args.Mapper!.Map<TReq, TReqModel>(value)).ConfigureAwait(false));


        /// <summary>
        /// Send an HTTP request asynchronously with request body <paramref name="value"/> expecting a <typeparamref name="TResp"/> response value (mapped from <typeparamref name="TRespModel"/>).
        /// </summary>
        /// <typeparam name="TReq">The request <paramref name="value"/> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TResp">The response <see cref="Type"/> (mapped from <typeparamref name="TRespModel"/>).</typeparam>
        /// <typeparam name="TRespModel">The response model <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="HttpSendArgs"/>.</param>
        /// <param name="value">The request value.</param>
        /// <returns>The <see cref="HttpAgentResult{TResp, TRespModel}"/>.</returns>
        public async Task<HttpAgentResult<TResp, TRespModel>> SendMappedResponseAsync<TReq, TResp, TRespModel>(HttpSendArgs args, TReq value)
            => new HttpAgentResult<TResp, TRespModel>(CheckSendArgs(args, true).Mapper!, await SendAsyncInternal(args, value).ConfigureAwait(false));

        /// <summary>
        /// Send an HTTP request asynchronously with request body <paramref name="value"/> (mapped to <typeparamref name="TReqModel"/>) expecting a <typeparamref name="TResp"/> response value (mapped from <typeparamref name="TRespModel"/>).
        /// </summary>
        /// <typeparam name="TReq">The request <paramref name="value"/> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TReqModel">The request <paramref name="value"/> model <see cref="Type"/> that is mapped to and sent.</typeparam>
        /// <typeparam name="TResp">The response <see cref="Type"/> (mapped from <typeparamref name="TRespModel"/>).</typeparam>
        /// <typeparam name="TRespModel">The response model <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="HttpSendArgs"/>.</param>
        /// <param name="value">The request value.</param>
        /// <returns>The <see cref="HttpAgentResult{TResp, TRespModel}"/>.</returns>
        public async Task<HttpAgentResult<TResp, TRespModel>> SendMappedRequestResponseAsync<TReq, TReqModel, TResp, TRespModel>(HttpSendArgs args, TReq value)
            => new HttpAgentResult<TResp, TRespModel>(CheckSendArgs(args, true).Mapper!, await SendAsyncInternal(args, args.Mapper!.Map<TReq, TReqModel>(value)).ConfigureAwait(false));

        #endregion
    }
}