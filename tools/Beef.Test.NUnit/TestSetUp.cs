// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching.Policy;
using Beef.Diagnostics;
using Moq;
using NUnit.Framework;
using System;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Orchestrates the set up for testing.
    /// </summary>
    public sealed class TestSetUp
    {
        private static readonly object _lock = new object();
        private static Func<int, object, bool> _registeredSetUp;
        private static object _registeredSetUpData;
        private static bool _registeredSetUpInvoked;
        private static int _registeredSetUpCount;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static TestSetUp()
        {
            RegisterGlobalLogger();
            Reset();
        }

        #region Logging

        /// <summary>
        /// Execute the <see cref="Logger.RegisterGlobal(Action{LoggerArgs})"/> and bind output to the console.
        /// </summary>
        public static void RegisterGlobalLogger()
        {
            Logger.RegisterGlobal((largs) =>
            {
                switch (largs.Type)
                {
                    case LogMessageType.Critical:
                    case LogMessageType.Error:
                        ConsoleWriteLine(largs.ToString(), ConsoleColor.Red);
                        break;

                    case LogMessageType.Warning:
                        ConsoleWriteLine(largs.ToString(), ConsoleColor.Yellow);
                        break;

                    case LogMessageType.Info:
                        ConsoleWriteLine(largs.ToString());
                        break;

                    case LogMessageType.Debug:
                    case LogMessageType.Trace:
                        ConsoleWriteLine(largs.ToString(), ConsoleColor.Cyan);
                        break;
                }
            });
        }

        /// <summary>
        /// Writes the specified text to the console.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="foregroundColor">The foreground <see cref="ConsoleColor"/>.</param>
        private static void ConsoleWriteLine(string text = null, ConsoleColor? foregroundColor = null)
        {
            if (string.IsNullOrEmpty(text))
                Console.WriteLine();
            else
            {
                var currColor = Console.ForegroundColor;
                Console.ForegroundColor = foregroundColor ?? currColor;
                Console.WriteLine(text);
                Console.ForegroundColor = currColor;
            }
        }

        #endregion

        #region SetUp

        /// <summary>
        /// Reset (set up) the <see cref="TestSetUp"/> to a known initial state; will result in the <see cref="RegisterSetUp(Func{int, object, bool})">registered</see> set up function being executed.
        /// </summary>
        /// <param name="setUpIfAlreadyDone">Indicates whether to perform the setup if already done; defaults to <c>true</c>.</param>
        /// <param name="data">Optional data to be passed to the resgitered set up function.</param>
        /// <remarks>This also invokes the <see cref="CachePolicyManager.ForceFlush"/> and <see cref="DependencyGroupAttribute.Refresh"/>.</remarks>
        public static void Reset(bool setUpIfAlreadyDone = true, object data = null)
        {
            lock (_lock)
            {
                if (!_registeredSetUpInvoked || setUpIfAlreadyDone)
                {
                    ShouldContinueRunningTests = true;
                    CachePolicyManager.ForceFlush();
                    DependencyGroupAttribute.Refresh();
                    Factory.ResetLocal();
                    _registeredSetUpData = data;
                    _registeredSetUpInvoked = false;
                }
            }
        }

        /// <summary>
        /// Registers the <paramref name="setUpFunc"/> that will be invoked once only (during the next <b>Run</b>) until <see cref="Reset(bool, object)"/> is invoked to reset.
        /// </summary>
        /// <param name="setUpFunc">The function to invoke. The first argument is the current count of invocations, and second is the optional data object. The return value is used to set
        /// <see cref="ShouldContinueRunningTests"/>.</param>
        public static void RegisterSetUp(Func<int, object, bool> setUpFunc)
        {
            lock (_lock)
            {
                if (_registeredSetUp != null)
                    throw new InvalidOperationException("The RegisterSetUp can only be invoked once.");

                _registeredSetUp = setUpFunc;
            }
        }

        /// <summary>
        /// Invokes the registered set up action.
        /// </summary>
        internal static void InvokeRegisteredSetUp()
        {
            lock (_lock)
            {
                Factory.ResetLocal();

                ShouldContinueRunningTestsAssert();

                if (ExecutionContext.Current.Properties.TryGetValue("InvokeRegisteredSetUp", out object needsSetUp) && !(bool)needsSetUp)
                    return;

                if (!_registeredSetUpInvoked)
                {
                    try
                    {
                        if (_registeredSetUp != null)
                        {
                            Logger.Default.Info(null);
                            Logger.Default.Info("Invocation of registered set up action.");
                            Logger.Default.Info(new string('=', 80));

                            ShouldContinueRunningTests = _registeredSetUp.Invoke(_registeredSetUpCount++, _registeredSetUpData);
                            if (!ShouldContinueRunningTests)
                                Assert.Fail("This RegisterSetUp function failed to execute successfully.");
                        }
                    }
                    catch (Exception ex)
                    {
                        ShouldContinueRunningTests = false;
                        Logger.Default.Exception(ex, $"This RegisterSetUp function failed to execute successfully: {ex.Message}");
                        Assert.Fail($"This RegisterSetUp function failed to execute successfully: {ex.Message}");
                    }
                    finally
                    {
                        _registeredSetUpInvoked = true;
                        if (_registeredSetUp != null)
                        {
                            Logger.Default.Info(null);
                            Logger.Default.Info(new string('=', 80));
                            Logger.Default.Info(null);
                        }
                    }
                }
            }
        }

        #endregion

        #region ContinueRunning

        /// <summary>
        /// Indicates whether tests should continue running; otherwise, set to <c>false</c> for all other remaining tests to return inconclusive.
        /// </summary>
        public static bool ShouldContinueRunningTests { get; set; } = true;

        /// <summary>
        /// Checks the <see cref="ShouldContinueRunningTests"/> and performs an <see cref="Assert"/> <see cref="Assert.Inconclusive(string)"/> where <c>false</c>.
        /// </summary>
        public static void ShouldContinueRunningTestsAssert()
        {
            if (!ShouldContinueRunningTests)
                Assert.Inconclusive("This test cannot be executed as AgentTester.ShouldContinueRunningTests has been set to false.");
        }

        /// <summary>
        /// Checks whether the tests should continue running (sets <see cref="ShouldContinueRunningTests"/> to the passed value) post the current test.
        /// </summary>
        /// <param name="shouldContinueRunningTests">Indicates whether tests should continue running; otherwise, set to <c>false</c> for all other remaining tests to return inclusive.</param>
        /// <returns>The <paramref name="shouldContinueRunningTests"/> value.</returns>
        public static bool ContinueRunning(bool shouldContinueRunningTests)
        {
            ShouldContinueRunningTests = shouldContinueRunningTests;
            return shouldContinueRunningTests;
        }

        #endregion

        /// <summary>
        /// Creates a <see cref="Mock{T}"/> and updates the <see cref="Factory"/> (see <see cref="Factory.SetLocal{T}(T)"/>) with the mock instance.
        /// </summary>
        /// <typeparam name="T">The type to mock.</typeparam>
        /// <returns>The <see cref="Mock{T}"/> instance.</returns>
        public static Mock<T> CreateMock<T>() where T : class
        {
            var mock = new Mock<T>();
            Factory.SetLocal<T>(mock.Object);
            return mock;
        }
    }
}
