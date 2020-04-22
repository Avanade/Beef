// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Beef.Grpc
{
    /// <summary>
    /// Provides the base service agent (client) capabilites to <b>invoke</b> <b>gRPC</b> operations.
    /// </summary>
    public abstract class GrpcServiceAgentBase<TClient> where TClient : ClientBase<TClient>, new()
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

        public Task InvokeAsync(Func<TClient, Task> func)
        {
            var channel = GrpcChannel.ForAddress("https://localhost:56045/", new GrpcChannelOptions { HttpClient = HttpClient );
            var client = new TClient(channel);
        }
    }
}
