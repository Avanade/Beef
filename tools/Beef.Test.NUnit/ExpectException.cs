// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using NUnit.Framework;
using System;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Expects and asserts an <see cref="Exception"/> and its corresponding exception message.
    /// </summary>
    public static class ExpectException
    {
        /// <summary>
        /// Verifies that a delegate throws a particular exception with the specified <paramref name="exceptionMessage"/> when called.
        /// </summary>
        /// <typeparam name="TException">The <see cref="Exception"/> <see cref="Type"/>.</typeparam>
        /// <param name="exceptionMessage">The expected exception message.</param>
        /// <param name="action">The action to execute.</param>
        public static void Throws<TException>(string exceptionMessage, Action action) where TException : Exception
        {
            Check.NotEmpty(exceptionMessage, nameof(exceptionMessage));
            Check.NotNull(action, nameof(action));

            try
            {
                action();
                Assert.Fail($"{typeof(TException).Name} expected: {exceptionMessage}");
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(TException))
                {
                    if (typeof(TException) == typeof(ArgumentException) || typeof(TException) == typeof(ArgumentNullException))
                    {
                        if (!exceptionMessage.Contains("\r\n") && ex.Message.Split("\r\n", StringSplitOptions.RemoveEmptyEntries)[0] == exceptionMessage)
                            return;
                    }

                    if (ex.Message == exceptionMessage)
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
        /// <param name="exceptionMessage">The expected exception message.</param>
        /// <param name="func">The function to execute.</param>
        public static void Throws<TException, TResult>(string exceptionMessage, Func<TResult> func) where TException : Exception
        {
            Check.NotEmpty(exceptionMessage, nameof(exceptionMessage));
            Check.NotNull(func, nameof(func));

            try
            {
                func();
                Assert.Fail($"{typeof(TException).Name} expected: {exceptionMessage}");
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(TException) && ex.Message == exceptionMessage)
                    return;

                throw;
            }
        }
    }
}
