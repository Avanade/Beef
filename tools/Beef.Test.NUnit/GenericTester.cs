// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Test.NUnit.Tests;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Manages any generic testing with integrated mocking of services as required.
    /// </summary>
    /// <remarks><see cref="TestSetUp.SetDefaultLocalReferenceData"/> is required to enable the reference data to function correctly. Where a <see cref="ExecutionContext.ServiceProvider"/> is 
    /// currently available this will be used versus creating new, minimizing the new to maintain (duplicate) the <see cref="ConfigureServices(Action{IServiceCollection})"/> logic.</remarks>
    [System.Diagnostics.DebuggerStepThrough]
    public sealed class GenericTester : TesterBase
    {
        private readonly string? _username;
        private readonly object? _args;
        private OperationType _operationType = Beef.OperationType.Unspecified;

        /// <summary>
        /// Create a new <see cref="GenericTester"/> for a named <paramref name="username"/>.
        /// </summary>
        /// <param name="username">The username (<c>null</c> indicates to use the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        /// <param name="includeLoggingScopesInOutput">Indicates whether to include scopes in log output.</param>
        /// <returns>A <see cref="GenericTester"/> instance.</returns>
        public static GenericTester Test(string? username = null, object? args = null, bool? includeLoggingScopesInOutput = null) => new GenericTester(username, args, includeLoggingScopesInOutput);

        /// <summary>
        /// Create a new <see cref="GenericTester"/> for a named <paramref name="userIdentifier"/> (converted using <see cref="TestSetUp.ConvertUsername(object?)"/>).
        /// </summary>
        /// <param name="userIdentifier">The user identifier (<c>null</c> indicates to use the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        /// <param name="includeLoggingScopesInOutput">Indicates whether to include scopes in log output.</param>
        /// <returns>A <see cref="GenericTester"/> instance.</returns>
        public static GenericTester Test(object? userIdentifier, object? args = null, bool? includeLoggingScopesInOutput = null) => new GenericTester(TestSetUp.ConvertUsername(userIdentifier), args, includeLoggingScopesInOutput);

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericTester"/> class.
        /// </summary>
        /// <param name="username">The username (<c>null</c> indicates to use the <see cref="ExecutionContext.Current"/> <see cref="ExecutionContext.Username"/>).</param>
        /// <param name="args">Optional argument that can be referenced within the test.</param>
        /// <param name="includeLoggingScopesInOutput">Indicates whether to include scopes in log output.</param>
        private GenericTester(string? username = null, object? args = null, bool? includeLoggingScopesInOutput = null) 
            : base(configureLocalRefData: true, inheritServiceCollection: true, includeLoggingScopesInOutput: includeLoggingScopesInOutput)
        {
            _username = username;
            _args = args;
        }

        /// <summary>
        /// Provides the opportunity to configure the services.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> action.</param>
        public GenericTester ConfigureServices(Action<IServiceCollection> serviceCollection)
        {
            ConfigureLocalServices(Check.NotNull(serviceCollection, nameof(serviceCollection)));
            return this;
        }

        /// <summary>
        /// Provides the opportunity to configure the services.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> function.</param>
        public GenericTester ConfigureServices(Func<IServiceCollection, IServiceCollection> serviceCollection)
        {
            ConfigureLocalServices(sc => Check.NotNull(serviceCollection, nameof(serviceCollection))(sc));
            return this;
        }

        /// <summary>
        /// Adds, or replaces (where existing), a singleton service <paramref name="mockInstance"/>. 
        /// </summary>
        /// <typeparam name="TService">The service <see cref="Type"/>.</typeparam>
        /// <param name="mockInstance">The instance value.</param>
        public GenericTester AddSingletonService<TService>(TService mockInstance) where TService : class
        {
            ConfigureLocalServices(sc => sc.ReplaceSingleton(mockInstance));
            return this;
        }

        /// <summary>
        /// Adds, or replaces (where existing), a singleton service <paramref name="mock"/>. 
        /// </summary>
        /// <typeparam name="TService">The service <see cref="Type"/>.</typeparam>
        /// <param name="mock">The mock.</param>
        public GenericTester AddSingletonService<TService>(Moq.Mock<TService> mock) where TService : class
        {
            ConfigureLocalServices(sc => sc.ReplaceSingleton(Check.NotNull(mock, nameof(mock)).Object));
            return this;
        }

        /// <summary>
        /// Adds, or replaces (where existing), a scoped service <paramref name="mockInstance"/>. 
        /// </summary>
        /// <typeparam name="TService">The service <see cref="Type"/>.</typeparam>
        /// <param name="mockInstance">The instance value.</param>
        public GenericTester AddScopedService<TService>(TService mockInstance) where TService : class
        {
            ConfigureLocalServices(sc => sc.ReplaceScoped(mockInstance));
            return this;
        }

        /// <summary>
        /// Adds, or replaces (where existing), a scoped service <paramref name="mock"/>. 
        /// </summary>
        /// <typeparam name="TService">The service <see cref="Type"/>.</typeparam>
        /// <param name="mock">The mock.</param>
        public GenericTester AddScopedService<TService>(Moq.Mock<TService> mock) where TService : class
        {
            ConfigureLocalServices(sc => sc.ReplaceScoped(Check.NotNull(mock, nameof(mock)).Object));
            return this;
        }

        /// <summary>
        /// Adds, or replaces (where existing), a transient service <paramref name="mockInstance"/>. 
        /// </summary>
        /// <typeparam name="TService">The service <see cref="Type"/>.</typeparam>
        /// <param name="mockInstance">The instance value.</param>
        public GenericTester AddTransientService<TService>(TService mockInstance) where TService : class
        {
            ConfigureLocalServices(sc => sc.ReplaceTransient(mockInstance));
            return this;
        }

        /// <summary>
        /// Adds, or replaces (where existing), a transient service <paramref name="mock"/>. 
        /// </summary>
        /// <typeparam name="TService">The service <see cref="Type"/>.</typeparam>
        /// <param name="mock">The mock.</param>
        public GenericTester AddTransientService<TService>(Moq.Mock<TService> mock) where TService : class
        {
            ConfigureLocalServices(sc => sc.ReplaceTransient(Check.NotNull(mock, nameof(mock)).Object));
            return this;
        }

        /// <summary>
        /// Sets the <see cref="ExecutionContext.OperationType"/> to the specified <paramref name="operationType"/>.
        /// </summary>
        /// <param name="operationType">The <see cref="OperationType"/>.</param>
        /// <returns>The <see cref="GenericTester"/> instance to support fluent/chaining usage.</returns>
        public GenericTester OperationType(OperationType operationType)
        {
            _operationType = operationType;
            return this;
        }

        /// <summary>
        /// Runs the <paramref name="func"/> to perform the generic testing.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        public async Task RunAsync(Func<Task> func)
        {
            PrepareExecutionContext(_username, _args);
            ExecutionContext.Current.OperationType = _operationType;

            await Check.NotNull(func, nameof(func))().ConfigureAwait(false);
        }
    }
}