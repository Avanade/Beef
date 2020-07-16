// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events;
using Beef.RefData;
using KellermanSoftware.CompareNetObjects;
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
        public static EventData[] GetEvents() => _localEvents.Value == null ? Array.Empty<EventData>() : _localEvents.Value.ToArray();

        /// <summary>
        /// Verifies that at least one event was published that matched the <paramref name="template"/> which may contain wildcards (<see cref="Event.TemplateWildcard"/>) and optional <paramref name="action"/>.
        /// </summary>
        /// <param name="template">The expected subject template (or fully qualified subject).</param>
        /// <param name="action">The optional expected action; <c>null</c> indicates any.</param>
        public static void IsPublished(string template, string? action = null)
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
        public static void IsNotPublished(string template, string? action = null)
        {
            Check.NotEmpty(template, nameof(template));
            if (_localEvents.Value == null || _localEvents.Value.Count == 0)
                return;

            if (_localEvents.Value.Any(x => Event.Match(template, x.Subject) && (action == null || StringComparer.OrdinalIgnoreCase.Compare(action, x.Action) == 0)))
                Assert.Fail($"Event with a subject template '{template}' and Action '{action ?? "any"}' was not expected; however, it was published.");
        }

        /// <summary>
        /// Verifies that the <paramref name="expectedEvents"/> are published (in order specified). The expected events can use wildcards for <see cref="EventData.Subject"/> and
        /// optionally define <see cref="EventData.Action"/>. Use <see cref="EventData{T}"/> where <see cref="EventData{T}.Value"/> comparisons are required (otherwise no comparison will occur). 
        /// Finally, the remaining <see cref="EventData"/> properties are not compared.
        /// </summary>
        /// <param name="expectedEvents">The <see cref="ExpectedEvent"/> list.</param>
        public static void ArePublished(List<ExpectedEvent> expectedEvents)
        {
            if (expectedEvents == null)
                throw new ArgumentNullException(nameof(expectedEvents));

            var actualEvents = GetEvents();
            if (actualEvents.Length != expectedEvents.Count)
                Assert.Fail($"Expected {expectedEvents.Count} Event(s) to be published; there were {actualEvents.Length} published.");

            for (int i = 0; i < actualEvents.Length; i++)
            {
                // Assert subject and action.
                var exp = expectedEvents[i].EventData;
                var act = actualEvents[i];

                if (!Event.Match(exp.Subject, act.Subject))
                    Assert.Fail($"Expected published Event[{i}].Subject '{exp.Subject}' is not equal to actual '{act.Subject}'.");

                if (!string.IsNullOrEmpty(exp.Action) && string.CompareOrdinal(exp.Action, act.Action) != 0)
                    Assert.Fail($"Expected published Event[{i}].Action '{exp.Action}' is not equal to actual '{act.Action}'.");

                // Where there is *no* expected value then skip value comparison.
                //if (!exp.HasValue)
                //    continue;

                // Assert value.
                //var eVal = exp.GetValue();
                //var aVal = act.GetValue();

                //var comparisonConfig = AgentTester.GetDefaultComparisonConfig();
                //comparisonConfig.TypesToIgnore.AddRange(ReferenceDataManager.Current.GetAllTypes());
                //var type = eVal?.GetType() ?? aVal?.GetType();
                //if (type != null)
                //    AgentTester.InferAdditionalMembersToIgnore(comparisonConfig, type);

                //var cl = new CompareLogic(comparisonConfig);
                //var cr = cl.Compare(eVal, aVal);
                //if (!cr.AreEqual)
                //    Assert.Fail($"Expected published Event[{i}].Value is not equal to actual: {cr.DifferencesString}");
            }
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

    /// <summary>
    /// Provides the configuration for the expected event (see <see cref="ExpectEvent.ArePublished(List{ExpectedEvent})"/>).
    /// </summary>
    public class ExpectedEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpectedEvent"/> class.
        /// </summary>
        /// <param name="eventData"></param>
        public ExpectedEvent(EventData eventData) => EventData = eventData ?? throw new ArgumentNullException(nameof(eventData));

        /// <summary>
        /// Gets or sets the <see cref="Events.EventData"/>.
        /// </summary>
        public EventData EventData { get; }

        /// <summary>
        /// Gets or sets the members to ignore for the <see cref="EventData.GetValue"/> comparison.
        /// </summary>
        public List<string> MembersToIgnore { get; private set; } = new List<string>();
    }
}