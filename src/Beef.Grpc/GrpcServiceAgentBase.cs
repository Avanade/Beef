// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Beef.Grpc
{
    /// <summary>
    /// Extends <see cref="GrpcServiceAgentBase{TClient}"/> adding <see cref="Register"/> and <see cref="Default"/> capabilities.
    /// </summary>
    /// <remarks>Each <b>invoke</b> is wrapped by a <see cref="GrpcServiceAgentInvoker{TClient}"/> to support additional logic where required.</remarks>
    public abstract class GrpcServiceAgentBase<TClient, TDefault> : GrpcServiceAgentBase<TClient> where TClient : ClientBase<TClient> where TDefault : GrpcServiceAgentBase<TClient, TDefault>
    {
        private static readonly object _lock = new object();
        private static Func<TDefault?>? _create;

#pragma warning disable CA1000 // Do not declare static members on generic types; by-design, a static means to define was needed and would generally be invoked in context of implementing class, therefore issue should not present itself.
        /// <summary>
        /// Registers the <see cref="HttpClient"/> <see cref="System.Net.Http.HttpClient"/> to be used as the default for all requests.
        /// </summary>
        /// <param name="create">The <see cref="Func{TDefault}"/> to create the <see cref="Default"/> instance.</param>
        /// <param name="overrideExisting">Indicates whether to override the existing where already set.</param>
        public static void Register(Func<TDefault?> create, bool overrideExisting = true)
#pragma warning restore CA1000
        {
            lock (_lock)
            {
                if (_create != null && !overrideExisting)
                    return;

                _create = create ?? throw new ArgumentNullException(nameof(create));
            }
        }

#pragma warning disable CA1000 // Do not declare static members on generic types; by-design, results in a consistent static defined default instance without the need to specify generic type to consume.
        /// <summary>
        /// Gets or sets the default <see cref="GrpcServiceAgentBase{TClient, TDefault}"/> instance.
        /// </summary>
        public static TDefault? Default { get; set; }
#pragma warning restore CA1000 

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcServiceAgentBase{TClient, TDefault}"/> class.
        /// </summary>
        /// <param name="httpClient">The <see cref="HttpClient"/>.</param>
        /// <param name="beforeRequest">The <see cref="Action{HttpRequestMessage}"/> to invoke before the <see cref="HttpRequestMessage">Http Request</see> is made (see <see cref="GrpcServiceAgentBase{TClient}.BeforeRequest"/>).</param>
        protected GrpcServiceAgentBase(HttpClient? httpClient = null, Action<HttpRequestMessage>? beforeRequest = null) : base(httpClient ?? Default?.HttpClient, beforeRequest ?? Default?.BeforeRequest, () => (_create ?? throw new InvalidOperationException("The Register method must register prior to usage.")).Invoke()!) { }
    }

    /// <summary>
    /// Provides the base service agent (client) capabilites to <b>invoke gRPC</b> operations.
    /// </summary>
    /// <remarks>Each <b>invoke</b> is wrapped by a <see cref="GrpcServiceAgentInvoker{TClient}"/> to support additional logic where required.</remarks>
    public abstract class GrpcServiceAgentBase<TClient> where TClient : ClientBase<TClient>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcServiceAgentBase{TClient}"/> class.
        /// </summary>
        /// <param name="httpClient">The <see cref="System.Net.Http.HttpClient"/>.</param>
        /// <param name="beforeRequest">The <see cref="Action{HttpRequestMessage}"/> to invoke before the client request is made.</param>
        /// <param name="registeredCreate">The registered <i>create</i> to determine the <see cref="System.Net.Http.HttpClient"/> used where <paramref name="httpClient"/> is <c>null</c>.</param>
        protected GrpcServiceAgentBase(HttpClient? httpClient = null, Action<HttpRequestMessage>? beforeRequest = null, Func<GrpcServiceAgentBase<TClient>>? registeredCreate = null)
        {
            if (httpClient == null)
            {
                var sa = registeredCreate?.Invoke();
                if (sa == null || sa.HttpClient == null)
                    throw new InvalidOperationException("The client or registeredCreate arguments must be provided.");

                HttpClient = sa.HttpClient;
                BeforeRequest = sa.BeforeRequest;
            }
            else
            {
                HttpClient = httpClient;
                BeforeRequest = beforeRequest;
            }

            // Create the channel and the client.
            var channel = GrpcChannel.ForAddress(HttpClient.BaseAddress, new GrpcChannelOptions { HttpClient = HttpClient });
            Client = (TClient)Activator.CreateInstance(typeof(TClient), channel)!;
        }

        /// <summary>
        /// Gets the <see cref="ClientBase{TClient}"/>.
        /// </summary>
        public TClient Client { get; private set; }

        /// <summary>
        /// Gets the underlying <see cref="System.Net.Http.HttpClient"/>.
        /// </summary>
        public HttpClient HttpClient { get; private set; }

        /// <summary>
        /// Gets the <see cref="Action"/> to invoke before the <see cref="HttpRequestMessage">Http Request</see> is made.
        /// </summary>
        /// <remarks>Represents an opportunity to add to the request headers for example.</remarks>
        public Action<HttpRequestMessage>? BeforeRequest { get; private set; }

        /// <summary>
        /// Invokes the gRPC call with no result asynchronously.
        /// </summary>
        /// <param name="func">The <paramref name="func"/> to perform the gRPC call.</param>
        /// <param name="request">The gRPC request value (for auditing).</param>
        /// <param name="requestOptions">The optional <see cref="GrpcRequestOptions"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The <see cref="GrpcAgentResult"/>.</returns>
        public Task<GrpcAgentResult> InvokeAsync(Func<TClient, CallOptions, AsyncUnaryCall<Empty>> func, IMessage? request, GrpcRequestOptions? requestOptions = null, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            if (requestOptions?.ETag != null)
                throw new NotImplementedException();

            return GrpcServiceAgentInvoker<TClient>.Default.InvokeAsync(this, async () =>
            {
                try
                {
                    var options = new CallOptions();
                    using var call = Check.NotNull(func, nameof(func)).Invoke(Client, options);
                    await call.ResponseAsync.ConfigureAwait(false);
                    return new GrpcAgentResult(call.GetStatus(), call.GetTrailers(), request);
                }
                catch (RpcException rex)
                {
                    return new GrpcAgentResult(rex, request);
                }
            }, null!, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Invokes the gRPC call with a result asynchronously.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="System.Type"/>.</typeparam>
        /// <typeparam name="TResponse">The gRPC response <see cref="System.Type"/>.</typeparam>
        /// <param name="func">The <paramref name="func"/> to perform the gRPC call.</param>
        /// <param name="request">The gRPC request value (for auditing).</param>
        /// <param name="mapper">The <see cref="Beef.Mapper.EntityMapper{TResult, TResponse}"/> to map the result from the response.</param>
        /// <param name="requestOptions">The optional <see cref="GrpcRequestOptions"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The <see cref="GrpcAgentResult{T}"/>.</returns>
        public Task<GrpcAgentResult<TResult>> InvokeAsync<TResult, TResponse>(Func<TClient, CallOptions, AsyncUnaryCall<TResponse>> func, IMessage? request, Beef.Mapper.EntityMapper<TResult, TResponse> mapper, GrpcRequestOptions? requestOptions = null, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            where TResult : class, new() where TResponse : class, new()
        {
            if (requestOptions?.ETag != null)
                throw new NotImplementedException();

            return GrpcServiceAgentInvoker<TClient>.Default.InvokeAsync(this, async () =>
            {
                try
                {
                    var options = new CallOptions();
                    using var call = Check.NotNull(func, nameof(func)).Invoke(Client, options);
                    var response = await call.ResponseAsync.ConfigureAwait(false);
                    var result = Check.NotNull(mapper, nameof(mapper)).MapToSrce(response);
                    return new GrpcAgentResult<TResult>(call.GetStatus(), call.GetTrailers(), request, response, result!);
                }
                catch (RpcException rex)
                {
                    return new GrpcAgentResult<TResult>(rex, request);
                }
            }, null!, memberName, filePath, lineNumber);
        }

        /// <summary>
        /// Invokes the gRPC call with a result asynchronously.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="System.Type"/>.</typeparam>
        /// <typeparam name="TResponse">The gRPC response <see cref="System.Type"/>.</typeparam>
        /// <param name="func">The <paramref name="func"/> to perform the gRPC call.</param>
        /// <param name="request">The gRPC request value (for auditing).</param>
        /// <param name="converter">The optional <see cref="Beef.Mapper.Converters.IPropertyMapperConverter{TResult, TResponse}"/>.</param>
        /// <param name="requestOptions">The optional <see cref="GrpcRequestOptions"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The <see cref="GrpcAgentResult{T}"/>.</returns>
        public Task<GrpcAgentResult<TResult>> InvokeAsync<TResult, TResponse>(Func<TClient, CallOptions, AsyncUnaryCall<TResponse>> func, IMessage? request, Beef.Mapper.Converters.IPropertyMapperConverter<TResult, TResponse>? converter, GrpcRequestOptions? requestOptions = null, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            if (requestOptions?.ETag != null)
                throw new NotImplementedException();

            return GrpcServiceAgentInvoker<TClient>.Default.InvokeAsync(this, async () =>
            {
                try
                {
                    var options = new CallOptions();
                    using var call = Check.NotNull(func, nameof(func)).Invoke(Client, options);
                    TResponse response = await call.ResponseAsync.ConfigureAwait(false);
                    return new GrpcAgentResult<TResult>(call.GetStatus(), call.GetTrailers(), request, response, converter == null 
                        ? (TResult)Convert.ChangeType(response, typeof(TResult), System.Globalization.CultureInfo.InvariantCulture) 
                        : converter.ConvertToSrce(response));
                }
                catch (RpcException rex)
                {
                    return new GrpcAgentResult<TResult>(rex, request);
                }
            }, null!, memberName, filePath, lineNumber);
        }
    }
}