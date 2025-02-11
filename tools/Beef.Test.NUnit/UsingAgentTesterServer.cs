﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using CoreEx;
using System;
using UnitTestEx;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Provides a wrapper for the <see cref="UsingApiTester{TEntryPoint}"/> to support backwards compatibility to earlier <i>Beef</i> versions.
    /// </summary>
    /// <typeparam name="TEntryPoint">>The API startup <see cref="Type"/>.</typeparam>
    /// <remarks>It is <b>recommended</b> that usage is upgraded to the new as this will eventually be deprecated.
    /// <para>Breaking change is that the <see cref="ExecutionContext.UserName"/> for the <see cref="AgentTester"/> reverts back to previous user post test execution where overridden.</para></remarks>
    public abstract class UsingAgentTesterServer<TEntryPoint> : UnitTestEx.UsingApiTester<TEntryPoint> where TEntryPoint : class
    {
        /// <summary>
        /// Gets the <see cref="AgentTester"/>.
        /// </summary>
        public AgentTester<TEntryPoint> AgentTester => new(this.ApiTester);
    }
}