// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using AutoMapper;
using Beef.WebApi;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Beef.Grpc
{
    /// <summary>
    /// Provides the base service agent (client) capabilites to <b>invoke gRPC</b> operations.
    /// </summary>
    public abstract class GrpcAgentBase 
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcAgentBase"/> class.
        /// </summary>
        /// <param name="args">The <see cref="IWebApiAgentArgs"/>.</param>
        /// <param name="mapper">The <see cref="IMapper"/>.</param>
        protected GrpcAgentBase(IWebApiAgentArgs args, IMapper mapper)
        {
            Args = args ?? throw new ArgumentNullException(nameof(args));
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Gets the underlying <see cref="System.Net.Http.HttpClient"/>.
        /// </summary>
        public IWebApiAgentArgs Args { get; private set; }

        /// <summary>
        /// Gets the <see cref="IMapper"/>.
        /// </summary>
        public IMapper Mapper { get; private set; }
    }

    /// <summary>
    /// Provides the base service agent (client) capabilites to <b>invoke gRPC</b> operations for a specified <typeparamref name="TClient"/>.
    /// </summary>
    /// <remarks>Each <b>invoke</b> is wrapped by a <see cref="GrpcAgentInvoker"/> to support additional logic where required.</remarks>
    public abstract class GrpcAgentBase<TClient> : GrpcAgentBase where TClient : ClientBase<TClient>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcAgentBase{TClient}"/> class.
        /// </summary>
        /// <param name="args">The <see cref="IWebApiAgentArgs"/>.</param>
        /// <param name="mapper">The <see cref="IMapper"/>.</param>
        protected GrpcAgentBase(IWebApiAgentArgs args, IMapper mapper) : base(args, mapper)
        {
            // Create the channel and the client.
            var channel = GrpcChannel.ForAddress(args.HttpClient.BaseAddress, new GrpcChannelOptions { HttpClient = Args.HttpClient });
            Client = (TClient)Activator.CreateInstance(typeof(TClient), channel)!;
        }

        /// <summary>
        /// Gets the <see cref="ClientBase{TClient}"/>.
        /// </summary>
        public TClient Client { get; private set; }

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

            return GrpcAgentInvoker.Current.InvokeAsync(this, async () =>
            {
                try
                {
                    var options = new CallOptions(CreateRequestHeaders());
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
        /// <param name="requestOptions">The optional <see cref="GrpcRequestOptions"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        /// <returns>The <see cref="GrpcAgentResult{T}"/>.</returns>
        public Task<GrpcAgentResult<TResult>> InvokeAsync<TResult, TResponse>(Func<TClient, CallOptions, AsyncUnaryCall<TResponse>> func, IMessage? request, GrpcRequestOptions? requestOptions = null, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            where TResult : class, new() where TResponse : class, new()
        {
            if (requestOptions?.ETag != null)
                throw new NotImplementedException();

            return GrpcAgentInvoker.Current.InvokeAsync(this, async () =>
            {
                try
                {
                    var options = new CallOptions(CreateRequestHeaders());
                    using var call = Check.NotNull(func, nameof(func)).Invoke(Client, options);
                    var response = await call.ResponseAsync.ConfigureAwait(false);
                    var result = Mapper.Map<TResponse, TResult>(response);
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

            return GrpcAgentInvoker.Current.InvokeAsync(this, async () =>
            {
                try
                {
                    var options = new CallOptions(CreateRequestHeaders());
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

        /// <summary>
        /// Create request headers.
        /// </summary>
        private static Metadata CreateRequestHeaders()
        {
            var headers = new Metadata();
            if (ExecutionContext.HasCurrent && !string.IsNullOrEmpty(ExecutionContext.Current.CorrelationId))
                headers.Add(WebApiConsts.CorrelationIdHeaderName, ExecutionContext.Current.CorrelationId);

            return headers;
        }
    }
}