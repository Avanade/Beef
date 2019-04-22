// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using System;
using System.Diagnostics;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Sets up the <see cref="ExecutionContext"/> for an <see cref="AgentTester"/> test execution. 
    /// </summary>
    [DebuggerStepThrough()]
    public class TestSetUpAttribute : PropertyAttribute, IWrapSetUpTearDown, ICommandWrapper
    {
        private readonly string _username;
        private readonly bool _needsSetUp;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestSetUpAttribute"/> class for a named <paramref name="username"/>.
        /// </summary>
        /// <param name="username">The username (<c>null</c> indicates to use <see cref=" AgentTester.DefaultUsername"/>).</param>
        /// <param name="needsSetUp">Indicates whether the registered set up is required to be invoked for the test.</param>
        public TestSetUpAttribute(string username = null, bool needsSetUp = true) : base(username ?? AgentTester.DefaultUsername)
        {
            _username = username ?? AgentTester.DefaultUsername;
            _needsSetUp = needsSetUp;
        }

        /// <summary>
        /// Wraps a command and returns the result.
        /// </summary>
        /// <param name="command">The <see cref="TestCommand"/> to be wrapped.</param>
        /// <returns>The wrapped <see cref="TestCommand"/>.</returns>
        public TestCommand Wrap(TestCommand command)
        {
            TestSetUp.ShouldContinueRunningTestsAssert();
            return new TestSetUpAttribute.ExecutionContextCommand(command, _username, _needsSetUp);
        }

        /// <summary>
        /// The test command for the <see cref="TestSetUpAttribute"/>.
        /// </summary>
        [DebuggerStepThrough()]
        public class ExecutionContextCommand : DelegatingTestCommand
        {
            private static Action<ExecutionContext, object> _onBeforeAction;

            private readonly string _username;
            private readonly bool _needsSetUp;
            private readonly object _data;

            /// <summary>
            /// Register an <i>on before</i> <see cref="Execute(TestExecutionContext)"/> action to perform any confguration etc. before executing.
            /// </summary>
            /// <param name="onBeforeAction">The action that accepts the <see cref="ExecutionContext"/> and optional <b>data</b> passed through the constructor.</param>
            public void RegisterOnBefore(Action<ExecutionContext, object> onBeforeAction)
            {
                _onBeforeAction = onBeforeAction;
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ExecutionContextCommand"/> class.
            /// </summary>
            /// <param name="innerCommand">The inner <see cref="TestCommand"/>.</param>
            /// <param name="username">The username.</param>
            /// <param name="needsSetUp">Indicates whether the registered set up is required to be invoked.</param>
            /// <param name="data">Optional data.</param>
            public ExecutionContextCommand(TestCommand innerCommand, string username = null, bool needsSetUp = true, object data = null) : base(innerCommand)
            {
                _username = username;
                _needsSetUp = needsSetUp;
                _data = data;
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

                    ExecutionContext.Reset(true);
                    ExecutionContext.Current.Username = _username;
                    ExecutionContext.Current.Properties["InvokeRegisteredSetUp"] = _needsSetUp;

                    _onBeforeAction?.Invoke(ExecutionContext.Current, _data);

                    context.CurrentResult = this.innerCommand.Execute(context);
                }
                catch (Exception exception)
                {
                    Exception ex = exception;
                    if (ex is AggregateException aex && aex.InnerExceptions.Count == 1)
                        ex = aex.InnerException;

                    if (ex is NUnitException nex && nex.InnerException is AggregateException aex2)
                        ex = aex2.InnerException;

                    if (context.CurrentResult == null)
                        context.CurrentResult = context.CurrentTest.MakeTestResult();

                    context.CurrentResult.RecordException(ex);
                }
                finally
                {
                    ExecutionContext.Reset(false);
                }

                // Remove any extraneous assertion results as this clutters/confuses the output.
                for (int i = context.CurrentResult.AssertionResults.Count - 1; i > 0; i--)
                {
                    context.CurrentResult.AssertionResults.RemoveAt(i);
                }

                return context.CurrentResult;
            }
        }
    }
}
