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
        /// Verifies that a delegate throws a particular exception with the specified <paramref name="expectedExceptionMessage"/> when called.
        /// </summary>
        /// <typeparam name="TException">The <see cref="Exception"/> <see cref="Type"/>.</typeparam>
        /// <param name="expectedExceptionMessage">The expected exception message; "*" indicates any.</param>
        /// <param name="action">The action to execute.</param>
        [System.Diagnostics.DebuggerStepThrough]
        public void Throws<TException>(string expectedExceptionMessage, Action action) where TException : Exception
        {
            if (expectedExceptionMessage == null)
                throw new ArgumentNullException(nameof(expectedExceptionMessage));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            try
            {
                action();
                Assert.Fail($"{typeof(TException).Name} expected: {expectedExceptionMessage}");
            }
            catch (Exception ex)
            {
                if (!VerifyExpectedException<TException>(expectedExceptionMessage, ex))
                    throw;
            }
        }

        /// <summary>
        /// Verifies that a delegate throws a particular exception with the specified <paramref name="expectedExceptionMessage"/> when called.
        /// </summary>
        /// <typeparam name="TException">The <see cref="Exception"/> <see cref="Type"/>.</typeparam>
        /// <param name="expectedExceptionMessage">The expected exception message; "*" indicates any.</param>
        /// <param name="funcAsync">The asynchronous function to execute.</param>
        [System.Diagnostics.DebuggerStepThrough]
        public void Throws<TException>(string expectedExceptionMessage, Func<Task> funcAsync)
        {
            if (expectedExceptionMessage == null)
                throw new ArgumentNullException(nameof(expectedExceptionMessage));

            if (funcAsync == null)
                throw new ArgumentNullException(nameof(funcAsync));

            try
            {
                funcAsync.Invoke().Wait();
                Assert.Fail($"{typeof(TException).Name} expected: {expectedExceptionMessage}");
            }
            catch (Exception ex)
            {
                if (!VerifyExpectedException<TException>(expectedExceptionMessage, ex))
                    throw;
            }
        }

        /// <summary>
        /// Verifies that a delegate throws a particular exception with the specified <paramref name="expectedExceptionMessage"/> when called.
        /// </summary>
        /// <typeparam name="TException">The <see cref="Exception"/> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="expectedExceptionMessage">The expected exception message; "*" indicates any.</param>
        /// <param name="func">The function to execute.</param>
        [System.Diagnostics.DebuggerStepThrough]
        public void Throws<TException, TResult>(string expectedExceptionMessage, Func<TResult> func) where TException : Exception
        {
            if (expectedExceptionMessage == null)
                throw new ArgumentNullException(nameof(expectedExceptionMessage));

            if (func == null)
                throw new ArgumentNullException(nameof(func));

            try
            {
                func();
                Assert.Fail($"{typeof(TException).Name} expected: {expectedExceptionMessage}");
            }
            catch (Exception ex)
            {
                if (!VerifyExpectedException<TException>(expectedExceptionMessage, ex))
                    throw;
            }
        }

        /// <summary>
        /// Verifies that the <paramref name="actualException"/> matches the expected <typeparamref name="TException"/> including the <paramref name="expectedExceptionMessage"/>.
        /// </summary>
        /// <typeparam name="TException">The expected <see cref="Exception"/> <see cref="Type"/>.</typeparam>
        /// <param name="expectedExceptionMessage">The expected exception message; "*" indicates any.</param>
        /// <param name="actualException">The actual <see cref="Exception"/> to verify.</param>
        /// <returns><c>true</c> indicates that the exception matches; otherwise, <c>false</c>.</returns>
        public static bool VerifyExpectedException<TException>(string expectedExceptionMessage, Exception actualException)
            => VerifyExpectedException(typeof(TException), expectedExceptionMessage, actualException);

        /// <summary>
        /// Verifies that the <paramref name="actualException"/> matches the <paramref name="expectedExceptionType"/> including the <paramref name="expectedExceptionMessage"/>.
        /// </summary>
        /// <param name="expectedExceptionType">The expected <see cref="Exception"/> <see cref="Type"/>.</param>
        /// <param name="expectedExceptionMessage">The expected exception message; "*" indicates any.</param>
        /// <param name="actualException">The actual <see cref="Exception"/> to verify.</param>
        /// <returns><c>true</c> indicates that the exception matches; otherwise, <c>false</c>.</returns>
        public static bool VerifyExpectedException(Type expectedExceptionType, string expectedExceptionMessage, Exception actualException)
        {
            if (expectedExceptionType == null)
                throw new ArgumentNullException(nameof(expectedExceptionType));

            if (expectedExceptionMessage == null)
                throw new ArgumentNullException(nameof(expectedExceptionMessage));

            if (actualException == null)
                throw new ArgumentNullException(nameof(actualException));

            if (actualException.GetType() == expectedExceptionType)
            {
                if (expectedExceptionType == typeof(ArgumentException) || expectedExceptionType == typeof(ArgumentNullException))
                {
                    if (!expectedExceptionMessage.Contains("\r\n", StringComparison.InvariantCulture) && actualException.Message.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)[0] == expectedExceptionMessage)
                        return true;
                }

                if (expectedExceptionMessage == "*" || actualException.Message == expectedExceptionMessage)
                    return true;
            }
            else if (actualException is AggregateException aex && aex.InnerException!.GetType() == expectedExceptionType)
            {
                if (expectedExceptionType == typeof(ArgumentException) || expectedExceptionType == typeof(ArgumentNullException))
                {
                    if (!expectedExceptionMessage.Contains("\r\n", StringComparison.InvariantCulture) && aex.InnerException.Message.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)[0] == expectedExceptionMessage)
                        return true;
                }

                if (expectedExceptionMessage == "*" || aex.InnerException.Message == expectedExceptionMessage)
                    return true;
            }

            return false;
        }
    }
}