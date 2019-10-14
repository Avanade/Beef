// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Represents a test that captures and verifies that an <see cref="Event"/> <see cref="EventData"/> has been published within an <see cref="AsyncLocal{T}"/> context.
    /// </summary>
    public static class ExpectEvent
    {
        private static readonly System.Threading.AsyncLocal<List<EventData>> _localEvents = new System.Threading.AsyncLocal<List<EventData>>();

        /// <summary>
        /// Static constructor.
        /// </summary>
        static ExpectEvent()
        {
            Event.Register((events) =>
            {
                if (_localEvents.Value != null)
                    _localEvents.Value.AddRange(events);

                return Task.CompletedTask;
            });
        }

        /// <summary>
        /// Resets the capturing of events for the <see cref="System.Threading.AsyncLocal{T}"/>.
        /// </summary>
        public static void SetUp() => _localEvents.Value = new List<EventData>();

        /// <summary>
        /// Gets the events for the <see cref="System.Threading.AsyncLocal{T}"/>.
        /// </summary>
        /// <returns>An <see cref="Beef.Events.EventData"/> array.</returns>
        public static EventData[] GetEvents() => _localEvents.Value == null ? new EventData[0] : _localEvents.Value.ToArray();

        /// <summary>
        /// Verifies that at least one event was published that matched the <paramref name="template"/> which may contain wildcards (<see cref="Event.TemplateWildcard"/>) and optional <paramref name="action"/>.
        /// </summary>
        /// <param name="template">The expected subject template (or fully qualified subject).</param>
        /// <param name="action">The optional expected action; <c>null</c> indicates any.</param>
        public static void IsPublished(string template, string action = null)
        {
            Check.NotEmpty(template, nameof(template));

            if (_localEvents.Value == null || _localEvents.Value.Count == 0 || !_localEvents.Value.Any(x => Event.Match(template, x.Subject) && (action == null || StringComparer.OrdinalIgnoreCase.Compare(action, x.Action) == 0)))
                Assert.Fail($"Event with a subject template '{template}' and Action '{action ?? "any"}' was expected and did not match one of the events published.");
        }

        /// <summary>
        /// Verifies that the no event was published that matches the <paramref name="template"/> which may contain wildcards (<see cref="Event.TemplateWildcard"/>) and optional <paramref name="action"/>.
        /// </summary>
        /// <param name="template">The expected subject template (or fully qualified subject).</param>
        /// <param name="action">The optional expected action; <c>null</c> indicates any.</param>
        public static void IsNotPublished(string template, string action = null)
        {
            Check.NotEmpty(template, nameof(template));
            if (_localEvents.Value == null || _localEvents.Value.Count == 0)
                return;

            if (_localEvents.Value.Any(x => Event.Match(template, x.Subject) && (action == null || StringComparer.OrdinalIgnoreCase.Compare(action, x.Action) == 0)))
                Assert.Fail($"Event with a subject template '{template}' and Action '{action ?? "any"}' was not expected; however, it was published.");
        }

        /// <summary>
        /// Verifies that no events were published.
        /// </summary>
        public static void NonePublished()
        {
            if (_localEvents.Value == null || _localEvents.Value.Count == 0)
                return;

            Assert.Fail($"Expected no events to be published, there were '{_localEvents.Value.Count}' published.");
        }
    }
}