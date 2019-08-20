// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using NUnit.Framework;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;
using System;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Provides a means to manage a group of test executions such that as soon as one fails the others within the dependency group will not execute as a success dependency is required. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class DependencyGroupAttribute : PropertyAttribute, IWrapSetUpTearDown, ICommandWrapper
    {
        private static readonly KeyOnlyDictionary<string> _badGroups = new KeyOnlyDictionary<string>();

        private readonly string _group;

        /// <summary>
        /// Refreshes (clears) any previously marked errant groups.
        /// </summary>
        public static void Refresh()
        {
            _badGroups.Clear();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyGroupAttribute"/> class from a <paramref name="group"/> name.
        /// </summary>
        /// <param name="group">The group name.</param>
        public DependencyGroupAttribute(string group) : base(group)
        {
            _group = Check.NotEmpty(group, nameof(group));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DependencyGroupAttribute"/> class from a <see cref="Type"/> name.
        /// </summary>
        /// <param name="type">The <see cref="Type"/>.</param>
        public DependencyGroupAttribute(Type type) : base(type?.Name)
        {
            _group = Check.NotNull(type, nameof(type)).Name;
        }

        /// <summary>
        /// Wraps a command and returns the result.
        /// </summary>
        /// <param name="command">The <see cref="TestCommand"/> to be wrapped.</param>
        /// <returns>The wrapped <see cref="TestCommand"/>.</returns>
        public TestCommand Wrap(TestCommand command)
        {
            if (_badGroups.ContainsKey(_group))
                Assert.Inconclusive($"This test cannot be executed as an earlier test within the dependency group '{_group}' failed.");

            return new DependencyGroupAttribute.DependencyGroupCommand(command, _group);
        }

        /// <summary>
        /// The test command for the <see cref="DependencyGroupAttribute"/>.
        /// </summary>
        public class DependencyGroupCommand : DelegatingTestCommand
        {
            private readonly string _group;

            /// <summary>
            /// Initializes a new instance of the <see cref="DependencyGroupCommand"/> class.
            /// </summary>
            /// <param name="innerCommand">The inner <see cref="TestCommand"/>.</param>
            /// <param name="group">The group name.</param>
            public DependencyGroupCommand(TestCommand innerCommand, string group) : base(innerCommand)
            {
                _group = Check.NotEmpty(group, nameof(group));
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
                    context.CurrentResult = this.innerCommand.Execute(context);
                }
                catch (Exception exception)
                {
                    Exception ex = exception;
                    if (context.CurrentResult == null)
                    {
                        context.CurrentResult = context.CurrentTest.MakeTestResult();
                    }

                    context.CurrentResult.RecordException(ex);
                    _badGroups.Add(_group);
                }

                return context.CurrentResult;
            }
        }
    }
}
