// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Expects and asserts a <see cref="ValidationException"/> and its corresponding messages.
    /// </summary>
    public static class ExpectValidationException
    {
        /// <summary>
        /// Verifies the <paramref name="action"/> throws a <see cref="ValidationException"/> containing the passed <paramref name="messages"/>.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="messages">The expected <see cref="MessageType.Error">error</see> message texts.</param>
        [System.Diagnostics.DebuggerStepThrough]
        public static void Throws(Action action, params string[] messages)
        {
            var mic = new MessageItemCollection();
            foreach (var text in messages)
            {
                mic.AddError(text);
            }

            Throws(action, mic);
        }

        /// <summary>
        /// Verifies the <paramref name="action"/> throws a <see cref="ValidationException"/> containing the passed <paramref name="messages"/>.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="messages">The expected <see cref="MessageItemCollection"/> collection.</param>
        /// <remarks>Will only check the <see cref="MessageItem.Property"/> where specified (not <c>null</c>).</remarks>
        [System.Diagnostics.DebuggerStepThrough]
        public static void Throws(Action action, MessageItemCollection messages)
        {
            Check.NotNull(action, nameof(action));
            Check.NotNull(messages, nameof(messages));

            try
            {
                action?.Invoke();
                Assert.Fail("A ValidationException is expected.");
            }
            catch (ValidationException vex)
            {
                CompareExpectedVsActual(messages, vex);
            }
        }

        /// <summary>
        /// Verifies the asynchonrous <paramref name="func"/> throws a <see cref="ValidationException"/> containing the passed <paramref name="messages"/>.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <param name="messages">The expected <see cref="MessageType.Error">error</see> message texts.</param>
        /// <returns>The corresponding <see cref="Task"/>.</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public static void Throws(Func<Task> func, params string[] messages)
        {
            var mic = new MessageItemCollection();
            foreach (var text in messages)
            {
                mic.AddError(text);
            }

            Throws(func, mic);
        }

        /// <summary>
        /// Verifies the asynchonrous <paramref name="func"/> throws a <see cref="ValidationException"/> containing the passed <paramref name="messages"/>.
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <param name="messages">The <see cref="MessageItemCollection"/> collection.</param>
        /// <returns>The corresponding <see cref="Task"/>.</returns>
        /// <remarks>Will only check the <see cref="MessageItem.Property"/> where specified (not <c>null</c>).</remarks>
        [System.Diagnostics.DebuggerStepThrough]
        public static void Throws(Func<Task> func, MessageItemCollection messages)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (messages == null)
                throw new ArgumentNullException(nameof(messages));

            try
            {
                func().Wait();
                Assert.Fail("A ValidationException is expected.");
            }
            catch (AggregateException aex)
            {
                if (aex.InnerException is ValidationException vex)
                    CompareExpectedVsActual(messages, vex.Messages);
                else
                    throw;
            }
            catch (ValidationException vex)
            {
                CompareExpectedVsActual(messages, vex);
            }
        }

        /// <summary>
        /// Verifies the <paramref name="func"/> throws a <see cref="ValidationException"/> containing the passed <paramref name="messages"/> (asynchronous).
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <param name="messages">The expected <see cref="MessageType.Error">error</see> message texts.</param>
        /// <returns>The corresponding <see cref="Task"/>.</returns>
        [System.Diagnostics.DebuggerStepThrough]
        public static Task ThrowsAsync(Func<Task> func, params string[] messages)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (messages == null)
                throw new ArgumentNullException(nameof(messages));

            var mic = new MessageItemCollection();
            foreach (var text in messages)
            {
                mic.AddError(text);
            }

            return ThrowsAsync(func, mic);
        }

        /// <summary>
        /// Verifies the <paramref name="func"/> throws a <see cref="ValidationException"/> containing the passed <paramref name="messages"/> (asynchronous).
        /// </summary>
        /// <param name="func">The function to execute.</param>
        /// <param name="messages">The <see cref="MessageItemCollection"/> collection.</param>
        /// <returns>The corresponding <see cref="Task"/>.</returns>
        /// <remarks>Will only check the <see cref="MessageItem.Property"/> where specified (not <c>null</c>).</remarks>
        [System.Diagnostics.DebuggerStepThrough]
        public static async Task ThrowsAsync(Func<Task> func, MessageItemCollection messages)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (messages == null)
                throw new ArgumentNullException(nameof(messages));

            try
            {
                await func();
                Assert.Fail("A ValidationException is expected.");
            }
            catch (AggregateException aex)
            {
                if (aex.InnerException is ValidationException vex)
                    CompareExpectedVsActual(messages, vex.Messages);
                else
                    throw;
            }
            catch (ValidationException vex)
            {
                CompareExpectedVsActual(messages, vex);
            }
        }

        /// <summary>
        /// Compares the expected vs. actual messages and reports the differences.
        /// </summary>
        /// <param name="expectedMessages">The expected messages.</param>
        /// <param name="vex">The validation exception.</param>
        static public void CompareExpectedVsActual(MessageItemCollection expectedMessages, ValidationException vex)
        {
            if (vex == null)
                throw new ArgumentNullException(nameof(vex));

            CompareExpectedVsActual(expectedMessages, vex.Messages);
        }

        /// <summary>
        /// Compares the expected versus actual messages and reports the differences.
        /// </summary>
        /// <param name="expectedMessages">The expected messages.</param>
        /// <param name="actualMessages">The actual messages.</param>
        static public void CompareExpectedVsActual(MessageItemCollection expectedMessages, MessageItemCollection actualMessages)
        {
            var exp = (from e in expectedMessages ?? new MessageItemCollection()
                       where !actualMessages.Any(a => a.Type == e.Type && a.Text == e.Text && (e.Property == null || a.Property == e.Property))
                       select e).ToList();

            var act = (from a in actualMessages ?? new MessageItemCollection()
                       where !expectedMessages.Any(e => a.Type == e.Type && a.Text == e.Text && (e.Property == null || a.Property == e.Property))
                       select a).ToList();

            var sb = new StringBuilder();
            if (exp.Count > 0)
            {
                sb.AppendLine(" Expected messages not matched:");
                exp.ForEach(m => sb.AppendLine($"  {m.Type}: {m.Text} {(m.Property != null ? $"[{m.Property}]" : null)}"));
            }

            if (act.Count > 0)
            {
                sb.AppendLine(" Actual messages not matched:");
                act.ForEach(m => sb.AppendLine($"  {m.Type}: {m.Text} {(m.Property != null ? $"[{m.Property}]" : null)}"));
            }

            if (sb.Length > 0)
                Assert.Fail($"Messages mismatch:{System.Environment.NewLine}{sb.ToString()}");
        }
    }
}
