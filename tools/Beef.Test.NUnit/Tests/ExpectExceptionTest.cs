// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Expects and asserts an <see cref="Exception"/> and its corresponding exception message.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "By-design, want to appear as instance methods.")]
    public class ExpectExceptionTest
    {
        /// <summary>
        /// Verifies that a delegate throws a particular exception with the specified <paramref name="exceptionMessage"/> when called.
        /// </summary>
        /// <typeparam name="TException">The <see cref="Exception"/> <see cref="Type"/>.</typeparam>
        /// <param name="exceptionMessage">The expected exception message; "*" indicates any.</param>
        /// <param name="action">The action to execute.</param>
        [System.Diagnostics.DebuggerStepThrough]
        public void Throws<TException>(string exceptionMessage, Action action) where TException : Exception
        {
            if (exceptionMessage == null)
                throw new ArgumentNullException(nameof(exceptionMessage));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                action();
                Assert.Fail($"{typeof(TException).Name} expected: {exceptionMessage}");
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(TException) && exceptionMessage == "*" || ex.Message.StartsWith(exceptionMessage, StringComparison.InvariantCulture))
                    return;

                throw;
            }
        }

        /// <summary>
        /// Verifies that a delegate throws a particular exception with the specified <paramref name="exceptionMessage"/> when called.
        /// </summary>
        /// <typeparam name="TException">The <see cref="Exception"/> <see cref="Type"/>.</typeparam>
        /// <param name="exceptionMessage">The expected exception message; "*" indicates any.</param>
        /// <param name="funcAsync">The asynchronous function to execute.</param>
        [System.Diagnostics.DebuggerStepThrough]
        public void Throws<TException>(string exceptionMessage, Func<Task> funcAsync)
        {
            if (exceptionMessage == null)
                throw new ArgumentNullException(nameof(exceptionMessage));

            if (funcAsync == null)
                throw new ArgumentNullException(nameof(funcAsync));

            try
            {
                funcAsync.Invoke().Wait();
                Assert.Fail($"{typeof(TException).Name} expected: {exceptionMessage}");
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(TException))
                {
                    if (typeof(TException) == typeof(ArgumentException) || typeof(TException) == typeof(ArgumentNullException))
                    {
                        if (!exceptionMessage.Contains("\r\n", StringComparison.InvariantCulture) && ex.Message.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)[0] == exceptionMessage)
                            return;
                    }

                    if (exceptionMessage == "*" || ex.Message == exceptionMessage)
                        return;
                }
                else if (ex is AggregateException aex && aex.InnerException!.GetType() == typeof(TException))
                {
                    if (typeof(TException) == typeof(ArgumentException) || typeof(TException) == typeof(ArgumentNullException))
                    {
                        if (!exceptionMessage.Contains("\r\n", StringComparison.InvariantCulture) && aex.InnerException.Message.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)[0] == exceptionMessage)
                            return;
                    }

                    if (exceptionMessage == "*" || aex.InnerException.Message == exceptionMessage)
                        return;
                }

                throw;
            }
        }

        /// <summary>
        /// Verifies that a delegate throws a particular exception with the specified <paramref name="exceptionMessage"/> when called.
        /// </summary>
        /// <typeparam name="TException">The <see cref="Exception"/> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="exceptionMessage">The expected exception message; "*" indicates any.</param>
        /// <param name="func">The function to execute.</param>
        [System.Diagnostics.DebuggerStepThrough]
        public void Throws<TException, TResult>(string exceptionMessage, Func<TResult> func) where TException : Exception
        {
            if (exceptionMessage == null)
                throw new ArgumentNullException(nameof(exceptionMessage));

            if (func == null)
                throw new ArgumentNullException(nameof(func));

            try
            {
                func();
                Assert.Fail($"{typeof(TException).Name} expected: {exceptionMessage}");
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(TException))
                {
                    if (typeof(TException) == typeof(ArgumentException) || typeof(TException) == typeof(ArgumentNullException))
                    {
                        if (!exceptionMessage.Contains("\r\n", StringComparison.InvariantCulture) && ex.Message.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)[0] == exceptionMessage)
                            return;
                    }

                    if (exceptionMessage == "*" || ex.Message == exceptionMessage)
                        return;
                }

                throw;
            }
        }
    }
}