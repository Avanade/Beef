// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events;
using Beef.RefData;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.Test.NUnit.Events
{
    /// <summary>
    /// Represents the expect event logic that leverages the <see cref="ExpectEventPublisher"/>.
    /// </summary>
    public static class ExpectEvent
    {
        /// <summary>
        /// Gets or sets the singleton <see cref="ExpectEventPublisher"/> instance.
        /// </summary>
        public static ExpectEventPublisher EventPublisher { get; set; } = new ExpectEventPublisher();

        /// <summary>
        /// Gets the events for the <paramref name="correlationId"/>.
        /// </summary>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <param name="removeEvents">Indicates whether to also <see cref="ExpectEventPublisher.Remove(string?)"/> the events.</param>
        /// <returns>An <see cref="Beef.Events.EventData"/> array.</returns>
        public static List<EventData> GetEvents(string? correlationId = null, bool removeEvents = false) => ExpectEventPublisher.GetEvents(correlationId, removeEvents);

        /// <summary>
        /// Verifies that at least one event was published that matched the <paramref name="template"/> which may contain wildcards (<see cref="IEventPublisher.TemplateWildcard"/>) and optional <paramref name="action"/>.
        /// </summary>
        /// <param name="template">The expected subject template (or fully qualified subject).</param>
        /// <param name="action">The optional expected action; <c>null</c> indicates any.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        public static void IsPublished(string template, string? action = null, string? correlationId = null)
        {
            Check.NotEmpty(template, nameof(template));

            var events = GetEvents(correlationId);
            if (events == null || events.Count == 0 || !events.Any(x => EventSubjectMatcher.Match(EventPublisher.TemplateWildcard, EventPublisher.PathSeparator, template, x.Subject) && (action == null || StringComparer.OrdinalIgnoreCase.Compare(action, x.Action) == 0)))
                Assert.Fail($"Event with a subject template '{template}' and Action '{action ?? "any"}' was expected and did not match one of the events published.");
        }

        /// <summary>
        /// Verifies that the no event was published that matches the <paramref name="template"/> which may contain wildcards (<see cref="IEventPublisher.TemplateWildcard"/>) and optional <paramref name="action"/>.
        /// </summary>
        /// <param name="template">The expected subject template (or fully qualified subject).</param>
        /// <param name="action">The optional expected action; <c>null</c> indicates any.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        public static void IsNotPublished(string template, string? action = null, string? correlationId = null)
        {
            Check.NotEmpty(template, nameof(template));

            var events = GetEvents(correlationId);
            if (events == null || events.Count == 0)
                return;

            if (events.Any(x => EventSubjectMatcher.Match(EventPublisher.TemplateWildcard, EventPublisher.PathSeparator, template, x.Subject) && (action == null || StringComparer.OrdinalIgnoreCase.Compare(action, x.Action) == 0)))
                Assert.Fail($"Event with a subject template '{template}' and Action '{action ?? "any"}' was not expected; however, it was published.");
        }

        /// <summary>
        /// Verifies that the <paramref name="expectedEvents"/> are published (in order specified). The expected events can use wildcards for <see cref="EventData.Subject"/> and
        /// optionally define <see cref="EventData.Action"/>. Use <see cref="EventData{T}"/> where <see cref="EventData{T}.Value"/> comparisons are required (otherwise no comparison will occur). 
        /// Finally, the remaining <see cref="EventData"/> properties are not compared.
        /// </summary>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <param name="expectedEvents">The <see cref="ExpectedEvent"/> list.</param>
        public static void ArePublished(List<ExpectedEvent> expectedEvents, string? correlationId = null)
        {
            if (expectedEvents == null)
                throw new ArgumentNullException(nameof(expectedEvents));

            var actualEvents = GetEvents(correlationId);

            if (actualEvents.Count != expectedEvents.Count)
                Assert.Fail($"Expected {expectedEvents.Count} Event(s) to be published; there were {actualEvents.Count} published.");

            for (int i = 0; i < actualEvents.Count; i++)
            {
                // Assert subject and action.
                var exp = expectedEvents[i].EventData;
                var act = actualEvents[i];

                if (!EventSubjectMatcher.Match(EventPublisher.TemplateWildcard, EventPublisher.PathSeparator, exp.Subject, act.Subject))
                    Assert.Fail($"Expected published Event[{i}].Subject '{exp.Subject}' is not equal to actual '{act.Subject}'.");

                if (!string.IsNullOrEmpty(exp.Action) && string.CompareOrdinal(exp.Action, act.Action) != 0)
                    Assert.Fail($"Expected published Event[{i}].Action '{exp.Action}' is not equal to actual '{act.Action}'.");

                // Where there is *no* expected value then skip value comparison.
                if (!exp.HasValue)
                    continue;

                // Assert value.
                var eVal = exp.GetValue();
                var aVal = act.GetValue();

                var comparisonConfig = TestSetUp.GetDefaultComparisonConfig();
                comparisonConfig.AttributesToIgnore.AddRange(new Type[] { typeof(ReferenceDataInterfaceAttribute) });

                var type = eVal?.GetType() ?? aVal?.GetType();
                if (type != null)
                    TestSetUp.InferAdditionalMembersToIgnore(comparisonConfig, type);

                var cl = new CompareLogic(comparisonConfig);
                var cr = cl.Compare(eVal, aVal);
                if (!cr.AreEqual)
                    Assert.Fail($"Expected published Event[{i}].Value is not equal to actual: {cr.DifferencesString}");
            }
        }

        /// <summary>
        /// Verifies that no events were published.
        /// </summary>
        /// <param name="correlationId">The correlation identifier.</param>
        public static void NonePublished(string? correlationId = null)
        {
            var events = GetEvents(correlationId);
            if (events == null || events.Count == 0)
                return;

            Assert.Fail($"Expected no events to be published, there were '{events.Count}' published.");
        }
    }

    /// <summary>
    /// Provides the configuration for the expected event (see <see cref="ExpectEvent.ArePublished(List{ExpectedEvent}, string?)"/>).
    /// </summary>
    public class ExpectedEvent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpectedEvent"/> class.
        /// </summary>
        /// <param name="eventData"></param>
        public ExpectedEvent(EventData eventData) => EventData = eventData ?? throw new ArgumentNullException(nameof(eventData));

        /// <summary>
        /// Gets or sets the <see cref="Beef.Events.EventData"/>.
        /// </summary>
        public EventData EventData { get; }

        /// <summary>
        /// Gets or sets the members to ignore for the <see cref="EventData.GetValue"/> comparison.
        /// </summary>
        public List<string> MembersToIgnore { get; private set; } = new List<string>();
    }
}