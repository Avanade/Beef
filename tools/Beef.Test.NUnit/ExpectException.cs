// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Expects and asserts an <see cref="Exception"/> and its corresponding exception message.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static class ExpectException
    {
        private static readonly ExpectExceptionTest _test = new ExpectExceptionTest();

        /// <summary>
        /// Verifies that a delegate throws a particular exception with the specified <paramref name="exceptionMessage"/> when called.
        /// </summary>
        /// <typeparam name="TException">The <see cref="Exception"/> <see cref="Type"/>.</typeparam>
        /// <param name="exceptionMessage">The expected exception message; "*" indicates any.</param>
        /// <param name="action">The action to execute.</param>
        [System.Diagnostics.DebuggerStepThrough]
        public static void Throws<TException>(string exceptionMessage, Action action) where TException : Exception => _test.Throws<TException>(exceptionMessage, action);

        /// <summary>
        /// Verifies that a delegate throws a particular exception with the specified <paramref name="exceptionMessage"/> when called.
        /// </summary>
        /// <typeparam name="TException">The <see cref="Exception"/> <see cref="Type"/>.</typeparam>
        /// <param name="exceptionMessage">The expected exception message; "*" indicates any.</param>
        /// <param name="funcAsync">The asynchronous function to execute.</param>
        [System.Diagnostics.DebuggerStepThrough]
        public static void Throws<TException>(string exceptionMessage, Func<Task> funcAsync) => _test.Throws<TException>(exceptionMessage, funcAsync);

        /// <summary>
        /// Verifies that a delegate throws a particular exception with the specified <paramref name="exceptionMessage"/> when called.
        /// </summary>
        /// <typeparam name="TException">The <see cref="Exception"/> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="exceptionMessage">The expected exception message; "*" indicates any.</param>
        /// <param name="func">The function to execute.</param>
        [System.Diagnostics.DebuggerStepThrough]
        public static void Throws<TException, TResult>(string exceptionMessage, Func<TResult> func) where TException : Exception => _test.Throws<TException, TResult>(exceptionMessage, func);
    }
}