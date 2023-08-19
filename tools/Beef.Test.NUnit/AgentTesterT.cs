using Microsoft.AspNetCore.TestHost;
using System;
using System.Net.Http;
using UnitTestEx.NUnit.Internal;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Provides existing <c>Test</c> methods to support backwards compatibility.
    /// </summary>
    public sealed class AgentTester<TEntryPoint> : IDisposable where TEntryPoint : class
    {
        private readonly ApiTester<TEntryPoint> _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgentTester{TEntryPoint}"/> class.
        /// </summary>
        /// <param name="parent">The parent <see cref="ApiTester{TEntryPoint}"/>.</param>
        internal AgentTester(ApiTester<TEntryPoint> parent) => _parent = parent;

        /// <inheritdoc/>
        public void Dispose() => _parent.Dispose();

        /// <summary>
        /// Gets the parent (owning) <see cref="ApiTester{TEntryPoint}"/>.
        /// </summary>
        public ApiTester<TEntryPoint> Parent => _parent;

        /// <summary>
        /// Enables an agent (<see cref="CoreEx.Http.TypedHttpClientBase"/>) to be used to send a <see cref="HttpRequestMessage"/> to the underlying <see cref="TestServer"/>.
        /// </summary>
        /// <typeparam name="TAgent">The <see cref="CoreEx.Http.TypedHttpClientBase"/> <see cref="Type"/>.</typeparam>
        /// <param name="userName">The overridding test user name.</param>
        /// <returns>The <see cref="UnitTestEx.AspNetCore.AgentTester{TAgent}"/>.</returns>
        public UnitTestEx.AspNetCore.AgentTester<TAgent> Test<TAgent>(string? userName = null) where TAgent : CoreEx.Http.TypedHttpClientBase
        {
            var at = _parent.Agent<TAgent>();
            if (userName != null)
                at.WithUser(userName);

            return at;
        }

        /// <summary>
        /// Enables an agent (<see cref="CoreEx.Http.TypedHttpClientBase"/>) to be used to send a <see cref="HttpRequestMessage"/> to the underlying <see cref="TestServer"/>.
        /// </summary>
        /// <typeparam name="TAgent">The <see cref="CoreEx.Http.TypedHttpClientBase"/> <see cref="Type"/>.</typeparam>
        /// <param name="userIdentifier">The overridding test user identifier.</param>
        /// <returns>The <see cref="UnitTestEx.AspNetCore.AgentTester{TAgent}"/>.</returns>
        public UnitTestEx.AspNetCore.AgentTester<TAgent> Test<TAgent>(object? userIdentifier) where TAgent : CoreEx.Http.TypedHttpClientBase
        {
            var at = _parent.Agent<TAgent>();
            if (userIdentifier != null)
                at.WithUser(userIdentifier);

            return at;
        }

        /// <summary>
        /// Enables an agent (<see cref="CoreEx.Http.TypedHttpClientBase"/>) to be used to send a <see cref="HttpRequestMessage"/> to the underlying <see cref="TestServer"/>.
        /// </summary>
        /// <typeparam name="TAgent">The <see cref="CoreEx.Http.TypedHttpClientBase"/> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TResponse">The response value <see cref="Type"/>.</typeparam>
        /// <param name="userName">The overridding test user name.</param>
        /// <returns>The <see cref="UnitTestEx.AspNetCore.AgentTester{TAgent, TValue}"/>.</returns>
        public UnitTestEx.AspNetCore.AgentTester<TAgent, TResponse> Test<TAgent, TResponse>(string? userName = null) where TAgent : CoreEx.Http.TypedHttpClientBase
        {
            var at = _parent.Agent<TAgent, TResponse>();
            if (userName != null)
                at.WithUser(userName);

            return at;
        }

        /// <summary>
        /// Enables an agent (<see cref="CoreEx.Http.TypedHttpClientBase"/>) to be used to send a <see cref="HttpRequestMessage"/> to the underlying <see cref="TestServer"/>.
        /// </summary>
        /// <typeparam name="TAgent">The <see cref="CoreEx.Http.TypedHttpClientBase"/> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TResponse">The response value <see cref="Type"/>.</typeparam>
        /// <param name="userIdentifier">The overridding test user identifier.</param>
        /// <returns>The <see cref="UnitTestEx.AspNetCore.AgentTester{TAgent, TValue}"/>.</returns>
        public UnitTestEx.AspNetCore.AgentTester<TAgent, TResponse> Test<TAgent, TResponse>(object? userIdentifier) where TAgent : CoreEx.Http.TypedHttpClientBase
        {
            var at = _parent.Agent<TAgent, TResponse>();
            if (userIdentifier != null)
                at.WithUser(userIdentifier);

            return at;
        }
    }
}