// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Test.NUnit.Tests;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Provides the underlying <see cref="AgentTester"/>.
    /// </summary>
    internal interface IUsingAgentTesterServer
    {
        /// <summary>
        /// Gets the underlying <see cref="AgentTesterBase"/>.
        /// </summary>
        AgentTesterBase AgentTester { get; }
    }

    /// <summary>
    /// Simplifies the testing using an <see cref="AgentTesterServer{TStartup}"/> using system-wide defaults. Automatically manages the instantiation and disposing through the one-time setup and tear-down.
    /// </summary>
    /// <typeparam name="TStartup">The <see cref="Type"/> of the startup entry point.</typeparam>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Managed via the onetime tear down.")]
    [System.Diagnostics.DebuggerStepThrough]
    public abstract class UsingAgentTesterServer<TStartup> : IUsingAgentTesterServer where TStartup : class
    {
        private AgentTesterServer<TStartup>? _agentTester;

        /// <summary>
        /// Gets the underlying <see cref="AgentTesterBase"/>.
        /// </summary>
        AgentTesterBase IUsingAgentTesterServer.AgentTester => AgentTester;

        /// <summary>
        /// Gets the underling <see cref="AgentTesterServer{TStartup}"/>.
        /// </summary>
        public AgentTesterServer<TStartup> AgentTester => _agentTester ?? throw new InvalidOperationException("The OneTimeSetUp must have been invoked prior to instantiate.");

        /// <summary>
        /// One-time setup. Invokes the <see cref="TestSetUp.Reset(bool, object?)"/> with <c>true</c> and <c>null</c> arguments, and instantiates the <see cref="AgentTester"/> using system-wide defaults.
        /// </summary>
        [OneTimeSetUp]
        public void UsingOneTimeSetUp()
        {
            TestSetUp.Reset(true, null);
            _agentTester = new AgentTesterServer<TStartup>();
        }

        /// <summary>
        /// One-time tear-down. Disposes of the <see cref="AgentTester"/>.
        /// </summary>
        [OneTimeTearDown]
        public void UsingOneTimeTearDown() => _agentTester?.Dispose();

        /// <summary>
        /// Prepares (creates) the <see cref="ExecutionContext"/> and ensures that the <see cref="TesterBase.LocalServiceProvider"/> scope is correctly configured.
        /// </summary>
        /// <param name="username">The username (<c>null</c> indicates to use the <see cref="TestSetUp.DefaultUsername"/>).</param>
        /// <param name="args">Optional argument that will be passed into the creation of the <see cref="ExecutionContext"/> (via the <see cref="TestSetUp.CreateExecutionContext(string?, object?)"/>).</param>
        /// <returns>The <see cref="AgentTesterWaf{TStartup}"/> instance to support fluent/chaining usage.</returns>
        /// <remarks>The <see cref="ExecutionContext"/> must be created by the <see cref="AgentTesterServer{TStartup}"/> as the <see cref="ExecutionContext.ServiceProvider"/> must be set to <see cref="TesterBase.LocalServiceProvider"/>.</remarks>
        public void PrepareExecutionContext(string? username = null, object? args = null) => AgentTester.PrepareExecutionContext(username, args);

        /// <summary>
        /// Builds the configuration probing as per <see cref="NUnit.AgentTester.BuildConfiguration{TStartup}(string?, string?)"/>.
        /// </summary>
        /// <param name="environmentVariablePrefix">The prefix that the environment variables must start with (will automatically add a trailing underscore where not supplied). Defaults to <see cref="TestSetUp.DefaultEnvironmentVariablePrefix"/></param>
        /// <param name="environment">The environment to be used by the underlying web host. Defaults to <see cref="TestSetUp.DefaultEnvironment"/>.</param>
        /// <returns>The <see cref="IConfiguration"/>.</returns>
        public IConfiguration BuildConfiguration(string? environmentVariablePrefix = null, string? environment = TestSetUp.DefaultEnvironment)
            => NUnit.AgentTester.BuildConfiguration<TStartup>(environmentVariablePrefix, environment);

        /// <summary>
        /// Expects and asserts a <see cref="ValidationException"/> and its corresponding messages.
        /// </summary>
        /// <remarks>Automatically invokes <see cref="PrepareExecutionContext(string?, object?)"/> where <see cref="ExecutionContext.HasCurrent"/> is <c>false</c>.</remarks>
        public ExpectValidationExceptionTest ExpectValidationException
        {
            get
            {
                if (!ExecutionContext.HasCurrent)
                    PrepareExecutionContext();

                return new ExpectValidationExceptionTest();
            }
        }

        /// <summary>
        /// Expects and asserts an <see cref="Exception"/> and its corresponding message.
        /// </summary>
        /// <remarks>Automatically invokes <see cref="PrepareExecutionContext(string?, object?)"/> where <see cref="ExecutionContext.HasCurrent"/> is <c>false</c>.</remarks>
        public ExpectExceptionTest ExpectException
        {
            get
            {
                if (!ExecutionContext.HasCurrent)
                    PrepareExecutionContext();

                return new ExpectExceptionTest();
            }
        }
    }
}