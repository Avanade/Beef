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
        private static readonly ExpectEventPublisher _eventPublisher = new ExpectEventPublisher();

        /// <summary>
        /// Gets the sent events for the <paramref name="correlationId"/>.
        /// </summary>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <returns>An <see cref="Beef.Events.EventData"/> array.</returns>
        public static List<EventData> GetSentEvents(string? correlationId = null) => ExpectEventPublisher.GetSentEvents(correlationId);

        /// <summary>
        /// Gets the published events for the <paramref name="correlationId"/>.
        /// </summary>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <returns>An <see cref="Beef.Events.EventData"/> array.</returns>
        public static List<EventData> GetPublishedEvents(string? correlationId = null) => ExpectEventPublisher.GetPublishedEvents(correlationId);

        /// <summary>
        /// Gets the count of <see cref="IEventPublisher.SendAsync">sends</see> that were performed for the specified <paramref name="correlationId"/>.
        /// </summary>
        public static int GetSendCount(string? correlationId = null) => ExpectEventPublisher.GetSendCount(correlationId);

        /// <summary>
        /// Verifies that at least one event was sent that matched the <paramref name="template"/> which may contain wildcards (<see cref="IEventPublisher.TemplateWildcard"/>) and optional <paramref name="action"/>.
        /// </summary>
        /// <param name="template">The expected subject template (or fully qualified subject).</param>
        /// <param name="action">The optional expected action; <c>null</c> indicates any.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        public static void IsSent(string template, string? action = null, string? correlationId = null)
        {
            Check.NotEmpty(template, nameof(template));

            var events = GetSentEvents(correlationId);
            if (events == null || events.Count == 0 || !events.Any(x => EventSubjectMatcher.Match(_eventPublisher.TemplateWildcard, _eventPublisher.PathSeparator, template, x.Subject) && (action == null || StringComparer.OrdinalIgnoreCase.Compare(action, x.Action) == 0)))
                Assert.Fail($"Event with a subject template '{template}' and Action '{action ?? "any"}' was expected and did not match one of the events sent.");
        }

        /// <summary>
        /// Verifies that the no event was sent that matches the <paramref name="template"/> which may contain wildcards (<see cref="IEventPublisher.TemplateWildcard"/>) and optional <paramref name="action"/>.
        /// </summary>
        /// <param name="template">The expected subject template (or fully qualified subject).</param>
        /// <param name="action">The optional expected action; <c>null</c> indicates any.</param>
        /// <param name="correlationId">The correlation identifier.</param>
        public static void IsNotSent(string template, string? action = null, string? correlationId = null)
        {
            Check.NotEmpty(template, nameof(template));

            var events = GetSentEvents(correlationId);
            if (events == null || events.Count == 0)
                return;

            if (events.Any(x => EventSubjectMatcher.Match(_eventPublisher.TemplateWildcard, _eventPublisher.PathSeparator, template, x.Subject) && (action == null || StringComparer.OrdinalIgnoreCase.Compare(action, x.Action) == 0)))
                Assert.Fail($"Event with a subject template '{template}' and Action '{action ?? "any"}' was not expected; however, it was sent.");
        }

        /// <summary>
        /// Verifies that the <paramref name="expectedEvents"/> are sent (in order specified). The expected events can use wildcards for <see cref="EventMetadata.Subject"/> and
        /// optionally define <see cref="EventMetadata.Action"/>. Use <see cref="EventData{T}"/> where <see cref="EventData{T}.Value"/> comparisons are required (otherwise no comparison will occur). 
        /// Finally, the remaining <see cref="EventData"/> properties are not compared.
        /// </summary>
        /// <param name="correlationId">The correlation identifier.</param>
        /// <param name="expectedEvents">The <see cref="ExpectedEvent"/> list.</param>
        public static void AreSent(List<ExpectedEvent> expectedEvents, string? correlationId = null)
            => AreSentCompare(expectedEvents, correlationId, "Expected", "sent", "sent", "to be");

        /// <summary>
        /// Verify/compare expected versus sent.
        /// </summary>
        private static void AreSentCompare(List<ExpectedEvent> expectedEvents, string? correlationId, string testText, string expectText, string actualText, string checkText)
        {
            if (expectedEvents == null)
                throw new ArgumentNullException(nameof(expectedEvents));

            var actualEvents = GetSentEvents(correlationId);

            if (actualEvents.Count != expectedEvents.Count)
                Assert.Fail($"{testText} {expectedEvents.Count} Event(s) {checkText} {expectText}; there were {actualEvents.Count} {actualText}.");

            for (int i = 0; i < actualEvents.Count; i++)
            {
                // Assert subject and action.
                var exp = expectedEvents[i].EventData;
                var membersToIgnore = expectedEvents[i].MembersToIgnore;
                var act = actualEvents[i];

                if (!EventSubjectMatcher.Match(_eventPublisher.TemplateWildcard, _eventPublisher.PathSeparator, exp.Subject, act.Subject))
                    Assert.Fail($"{testText} {expectText} Event[{i}].Subject '{exp.Subject}' is not equal to actual '{act.Subject}' {actualText}.");

                if (!string.IsNullOrEmpty(exp.Action) && string.CompareOrdinal(exp.Action, act.Action) != 0)
                    Assert.Fail($"{testText} {expectText} Event[{i}].Action '{exp.Action}' is not equal to actual '{act.Action}' {actualText}.");

                // Where there is *no* expected value then skip value comparison.
                if (!exp.HasValue)
                    continue;

                // Assert value.
                var eVal = exp.GetValue();
                var aVal = act.GetValue();

                var comparisonConfig = TestSetUp.GetDefaultComparisonConfig();
                comparisonConfig.AttributesToIgnore.AddRange(new Type[] { typeof(ReferenceDataInterfaceAttribute) });
                if(membersToIgnore != null)
                    comparisonConfig.MembersToIgnore.AddRange(membersToIgnore);

                var type = eVal?.GetType() ?? aVal?.GetType();
                if (type != null)
                    TestSetUp.InferAdditionalMembersToIgnore(comparisonConfig, type);

                var cl = new CompareLogic(comparisonConfig);
                var cr = cl.Compare(eVal, aVal);
                if (!cr.AreEqual)
                    Assert.Fail($"{testText} {expectText} Event[{i}].Value is not equal to actual {actualText}: {cr.DifferencesString}");
            }
        }

        /// <summary>
        /// Verifies that no events were sent.
        /// </summary>
        /// <param name="correlationId">The correlation identifier.</param>
        public static void NoneSent(string? correlationId = null)
        {
            var events = GetSentEvents(correlationId);
            if (events == null || events.Count == 0)
                return;

            Assert.Fail($"Expected no events to be sent, there were '{events.Count}' sent.");
        }

        /// <summary>
        /// Verifies that all the events that were published were sent.
        /// </summary>
        /// <param name="correlationId">The correlation identifier.</param>
        public static void PublishedVersusSent(string? correlationId = null)
        {
            var expectedEvents = new List<ExpectedEvent>();
            foreach (var pe in GetPublishedEvents(correlationId))
            {
                expectedEvents.Add(new ExpectedEvent(pe));
            }

            AreSentCompare(expectedEvents, correlationId, "Publish/Send mismatch", "published", "sent", "were");
        }
    }

    /// <summary>
    /// Provides the configuration for the expected event (see <see cref="ExpectEvent.AreSent(List{ExpectedEvent}, string?)"/>).
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