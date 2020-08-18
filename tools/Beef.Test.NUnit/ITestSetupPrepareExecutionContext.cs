// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Test.NUnit.Tests;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Provides the underlying <see cref="AgentTester"/>. Enables the <see cref="TestSetUpAttribute"/> to automatically invoke the <see cref="TesterBase.PrepareExecutionContext(string?, object?)"/>.
    /// </summary>
    public interface ITestSetupPrepareExecutionContext
    {
        /// <summary>
        /// Gets the underlying <see cref="AgentTesterBase"/>.
        /// </summary>
        AgentTesterBase AgentTester { get; }
    }
}