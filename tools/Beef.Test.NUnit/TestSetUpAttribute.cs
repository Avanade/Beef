// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using System;
using System.Diagnostics;
using System.Threading;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Sets up the test by <see cref="ExecutionContext.Reset()">resetting</see> the <see cref="ExecutionContext"/> to ensure <c>null</c>; then orchestrates whether the 
    /// <see cref="TestSetUp.RegisterSetUp(Func{int, object?, bool})">registered setup</see> is required to be invoked for the test.
    /// </summary>
    [DebuggerStepThrough]
    public class TestSetUpAttribute : PropertyAttribute, IWrapSetUpTearDown, ICommandWrapper
    {
        private static readonly AsyncLocal<string> _username = new AsyncLocal<string>();
        private static readonly AsyncLocal<object?> _args = new AsyncLocal<object?>();
        private readonly string _testUsername;
        private readonly object? _testArgs;
        private readonly bool _needsSetup;

        /// <summary>
        /// Gets the username.
        /// </summary>
        internal static string Username => _username.Value ?? TestSetUp.DefaultUsername; 

        /// <summary>
        /// Gets the arguments.
        /// </summary>
        internal static object? Args => _args.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestSetUpAttribute"/> class indicating whether the <see cref="TestSetUp.RegisterSetUp(Func{int, object?, bool})">registered setup</see> is required to be invoked for the test.
        /// </summary>
        /// <param name="username">The username (<c>null</c> indicates to use the <see cref="TestSetUp.DefaultUsername"/>).</param>
        /// <param name="args">Optional argument that will be passed into the creation of the <see cref="ExecutionContext"/>.</param>
        /// <param name="needsSetUp">Indicates whether the registered set up is required to be invoked for the test.</param>
        public TestSetUpAttribute(string? username = null, object? args = null, bool needsSetUp = true)
        {
            //_username.Value = username ?? TestSetUp.DefaultUsername;
            //_args.Value = args;
            _testUsername = username ?? TestSetUp.DefaultUsername;
            _testArgs = args;
            _needsSetup = needsSetUp;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestSetUpAttribute"/> class for a <paramref name="userIdentifier"/>.
        /// </summary>
        /// <param name="userIdentifier">The user identifier (<c>null</c> indicates to use the <see cref="TestSetUp.DefaultUsername"/>).</param>
        /// <param name="args">Optional argument that will be passed into the creation of the <see cref="ExecutionContext"/>.</param>
        /// <param name="needsSetUp">Indicates whether the registered set up is required to be invoked for the test.</param>
        public TestSetUpAttribute(object? userIdentifier, object? args = null, bool needsSetUp = true) : this(TestSetUp.ConvertUsername(userIdentifier), args, needsSetUp) { }

        /// <summary>
        /// Wraps a command and returns the result.
        /// </summary>
        /// <param name="command">The <see cref="TestCommand"/> to be wrapped.</param>
        /// <returns>The wrapped <see cref="TestCommand"/>.</returns>
        public TestCommand Wrap(TestCommand command)
        {
            TestSetUp.ShouldContinueRunningTestsAssert();
            return new ExecutionContextCommand(command, _testUsername, _testArgs, _needsSetup);
        }

        /// <summary>
        /// The test command for the <see cref="TestSetUpAttribute"/>.
        /// </summary>
        [DebuggerStepThrough()]
        internal class ExecutionContextCommand : DelegatingTestCommand
        {
            private readonly string _testUsername;
            private readonly object? _testArgs;
            private readonly bool _needsSetUp;

            /// <summary>
            /// Initializes a new instance of the <see cref="ExecutionContextCommand"/> class.
            /// </summary>
            /// <param name="innerCommand">The inner <see cref="TestCommand"/>.</param>
            /// <param name="username">The username (<c>null</c> indicates to use the <see cref="TestSetUp.DefaultUsername"/>).</param>
            /// <param name="args">Optional argument that will be passed into the creation of the <see cref="ExecutionContext"/>.</param>
            /// <param name="needsSetUp">Indicates whether the registered set up is required to be invoked.</param>
            public ExecutionContextCommand(TestCommand innerCommand, string? username, object? args, bool needsSetUp = true) : base(innerCommand)
            {
                _testUsername = username ?? TestSetUp.DefaultUsername;
                _testArgs = args;
                _needsSetUp = needsSetUp;
            }

            /// <summary>
            /// Executes the test, saving a <see cref="TestResult"/> in the supplied <see cref="TestExecutionContext"/>.
            /// </summary>
            /// <param name="context">The <see cref="TestExecutionContext"/>.</param>
            /// <returns>The <see cref="TestResult"/>.</returns>
            public override TestResult Execute(TestExecutionContext context)
            {
                try
                {
                    if (_needsSetUp)
                        TestSetUp.InvokeRegisteredSetUp();

                    _username.Value = _testUsername;
                    _args.Value = _testArgs;

                    if (context.CurrentTest?.Parent?.Fixture is ITestSetupPrepareExecutionContext uats)
                        uats.AgentTester.PrepareExecutionContext();
                    else
                        ExecutionContext.Reset();

                    context.CurrentResult = innerCommand.Execute(context);
                }
                catch (Exception exception)
                {
                    Exception ex = exception;
                    if (ex is AggregateException aex && aex.InnerExceptions.Count == 1)
                        ex = aex.InnerException!;

                    if (ex is NUnitException nex && nex.InnerException is AggregateException aex2)
                        ex = aex2.InnerException!;

                    if (context.CurrentResult == null)
                        context.CurrentResult = context.CurrentTest.MakeTestResult();

                    context.CurrentResult.RecordException(ex);
                }
                finally
                {
                    ExecutionContext.Reset();
                }

                // Remove any extraneous assertion results as this clutters/confuses the output.
                for (int i = context.CurrentResult.AssertionResults.Count - 1; i > 0; i--)
                {
                    context.CurrentResult.AssertionResults.RemoveAt(i);
                }

                _username.Value = TestSetUp.DefaultUsername;
                _args.Value = null;
                return context.CurrentResult;
            }
        }
    }
}