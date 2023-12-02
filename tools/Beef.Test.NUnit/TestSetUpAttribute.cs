// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using System;
using System.Diagnostics;
using System.Threading;
using UnitTestEx;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Sets up the test by <see cref="CoreEx.ExecutionContext.Reset()">resetting</see> the <see cref="ExecutionContext"/> to ensure <c>null</c>; then orchestrates whether the 
    /// <see cref="TestSetUp.RegisterSetUp(Func{int, object?, CancellationToken, System.Threading.Tasks.Task{bool}})">registered setup</see> is required to be invoked for the test.
    /// </summary>
    /// <remarks>Provided to support backwards compatibility to earlier <i>Beef</i> versions. It is <b>recommended</b> that usage is upgraded to the new as this will eventually be deprecated.
    /// <para>As the attribute is executed by the <i>NUnit</i> runtime outside of the context of the method itself only the <see cref="TestSetUp.Default"/> is able to be referenced. This, and the challenge of achieving consistency between
    /// <i>MSTest</i>, <i>NUnit</i> and <i>Xunit</i> is why this feature is being deprecated.</para></remarks>
    /// <param name="username">The username (<c>null</c> indicates to use the <see cref="TestSetUp.DefaultUserName"/>).</param>
    [DebuggerStepThrough]
    [AttributeUsage(AttributeTargets.Method)]
    public class TestSetUpAttribute(string? username = null) : PropertyAttribute, IWrapSetUpTearDown, ICommandWrapper
    {
        private static readonly AsyncLocal<string> _username = new();
        private readonly string _testUsername = username ?? TestSetUp.Default.DefaultUserName;

        /// <summary>
        /// Gets the username.
        /// </summary>
        internal static string Username => _username.Value ?? TestSetUp.Default.DefaultUserName;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestSetUpAttribute"/> class for a <paramref name="userIdentifier"/>.
        /// </summary>
        /// <param name="userIdentifier">The user identifier (<c>null</c> indicates to use the <see cref="TestSetUp.DefaultUserName"/>).</param>
        public TestSetUpAttribute(object? userIdentifier) 
            : this(userIdentifier == null ? TestSetUp.Default.DefaultUserName : 
                  (TestSetUp.Default.UserNameConverter == null 
                  ? throw new InvalidOperationException($"The {nameof(TestSetUp)}.{nameof(TestSetUp.UserNameConverter)} must be defined to support user identifier conversion.") 
                  : TestSetUp.Default.UserNameConverter(userIdentifier))) { }

        /// <summary>
        /// Wraps a command and returns the result.
        /// </summary>
        /// <param name="command">The <see cref="TestCommand"/> to be wrapped.</param>
        /// <returns>The wrapped <see cref="TestCommand"/>.</returns>
        public TestCommand Wrap(TestCommand command) => new ExecutionContextCommand(command, _testUsername);

        /// <summary>
        /// The test command for the <see cref="TestSetUpAttribute"/>.
        /// </summary>
        /// <param name="innerCommand">The inner <see cref="TestCommand"/>.</param>
        /// <param name="username">The username (<c>null</c> indicates to use the <see cref="TestSetUp.DefaultUserName"/>).</param>
        [DebuggerStepThrough()]
        internal class ExecutionContextCommand(TestCommand innerCommand, string? username) : DelegatingTestCommand(innerCommand)
        {
            private readonly string _testUsername = username ?? TestSetUp.Default.DefaultUserName;

            /// <summary>
            /// Executes the test, saving a <see cref="TestResult"/> in the supplied <see cref="TestExecutionContext"/>.
            /// </summary>
            /// <param name="context">The <see cref="TestExecutionContext"/>.</param>
            /// <returns>The <see cref="TestResult"/>.</returns>
            public override TestResult Execute(TestExecutionContext context)
            {
                try
                {
                    _username.Value = _testUsername;
                    
                    CoreEx.ExecutionContext.Reset();
                    CoreEx.ExecutionContext.SetCurrent(new CoreEx.ExecutionContext { UserName = _username.Value });

                    context.CurrentResult = innerCommand.Execute(context);
                }
                catch (Exception exception)
                {
                    Exception ex = exception;
                    if (ex is AggregateException aex && aex.InnerExceptions.Count == 1)
                        ex = aex.InnerException!;

                    if (ex is NUnitException nex && nex.InnerException is AggregateException aex2)
                        ex = aex2.InnerException!;

                    context.CurrentResult ??= context.CurrentTest.MakeTestResult();
                    context.CurrentResult.RecordException(ex);
                }
                finally
                {
                    CoreEx.ExecutionContext.Reset();
                }

                // Remove any extraneous assertion results as this clutters/confuses the output.
                for (int i = context.CurrentResult.AssertionResults.Count - 1; i > 0; i--)
                {
                    context.CurrentResult.AssertionResults.RemoveAt(i);
                }

                _username.Value = TestSetUp.Default.DefaultUserName;
                return context.CurrentResult;
            }
        }
    }
}