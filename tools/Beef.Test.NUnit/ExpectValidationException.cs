// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Test.NUnit.Tests;
using System;
using System.Threading.Tasks;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Expects and asserts a <see cref="ValidationException"/> and its corresponding messages.
    /// </summary>
    [System.Diagnostics.DebuggerStepThrough]
    public static class ExpectValidationException
    {
        private static readonly ExpectValidationExceptionTest _test = new ExpectValidationExceptionTest();

        /// <summary>
        /// Verifies the <paramref name="action"/> throws a <see cref="ValidationException"/> containing the passed <paramref name="messages"/>.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="messages">The expected <see cref="MessageType.Error">error</see> message texts.</param>
        [System.Diagnostics.DebuggerStepThrough]
        public static void Throws(Action action, params string[] messages) => _test.Throws(action, messages);

        /// <summary>
        /// Verifies the <paramref name="action"/> throws a <see cref="ValidationException"/> containing the passed <paramref name="messages"/>.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="messages">The expected <see cref="MessageItemCollection"/> collection.</param>
        /// <remarks>Will only check the <see cref="MessageItem.Property"/> where specified (not <c>null</c>).</remarks>
        [System.Diagnostics.DebuggerStepThrough]
        public static void Throws(Action action, MessageItemCollection messages) => _test.Throws(action, messages);

        /// <summary>
        /// Verifies the asynchonrous <paramref name="func"/> throws a <see cref="ValidationException"/> containing the passed <paramref name="messages"/>.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <param name="messages">The expected <see cref="MessageType.Error">error</see> message texts.</param>
        /// <returns>The corresponding <see cref="Task"/>.</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public static void Throws(Func<Task> func, params string[] messages) => _test.Throws(func, messages);

        /// <summary>
        /// Verifies the asynchonrous <paramref name="func"/> throws a <see cref="ValidationException"/> containing the passed <paramref name="messages"/>.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <param name="messages">The <see cref="MessageItemCollection"/> collection.</param>
        /// <returns>The corresponding <see cref="Task"/>.</returns>
        /// <remarks>Will only check the <see cref="MessageItem.Property"/> where specified (not <c>null</c>).</remarks>
        [System.Diagnostics.DebuggerStepThrough]
        public static void Throws(Func<Task> func, MessageItemCollection messages) => _test.Throws(func, messages);

        /// <summary>
        /// Verifies the <paramref name="func"/> throws a <see cref="ValidationException"/> containing the passed <paramref name="messages"/> (asynchronous).
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <param name="messages">The expected <see cref="MessageType.Error">error</see> message texts.</param>
        /// <returns>The corresponding <see cref="Task"/>.</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public static Task ThrowsAsync(Func<Task> func, params string[] messages) => _test.ThrowsAsync(func, messages);

        /// <summary>
        /// Verifies the <paramref name="func"/> throws a <see cref="ValidationException"/> containing the passed <paramref name="messages"/> (asynchronous).
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <param name="messages">The <see cref="MessageItemCollection"/> collection.</param>
        /// <returns>The corresponding <see cref="Task"/>.</returns>
        /// <remarks>Will only check the <see cref="MessageItem.Property"/> where specified (not <c>null</c>).</remarks>
        [System.Diagnostics.DebuggerStepThrough]
        public static Task ThrowsAsync(Func<Task> func, MessageItemCollection messages) => _test.ThrowsAsync(func, messages);

        /// <summary>
        /// Compares the expected vs. actual messages and reports the differences.
        /// </summary>
        /// <param name="expectedMessages">The expected messages.</param>
        /// <param name="vex">The validation exception.</param>
        [System.Diagnostics.DebuggerStepThrough]
        public static void CompareExpectedVsActual(MessageItemCollection expectedMessages, ValidationException vex) => _test.CompareExpectedVsActual(expectedMessages, vex);

        /// <summary>
        /// Compares the expected versus actual messages and reports the differences.
        /// </summary>
        /// <param name="expectedMessages">The expected messages.</param>
        /// <param name="actualMessages">The actual messages.</param>
        [System.Diagnostics.DebuggerStepThrough]
        public static void CompareExpectedVsActual(MessageItemCollection? expectedMessages, MessageItemCollection? actualMessages) => _test.CompareExpectedVsActual(expectedMessages, actualMessages);
    }
}