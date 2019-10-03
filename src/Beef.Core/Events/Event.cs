// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.Events
{
    /// <summary>
    /// Provides the standardised <b>Event</b> processing/publishing.
    /// </summary>
    public static class Event
    {
        private static readonly List<Func<EventData[], Task>> _publishFuncs = new List<Func<EventData[], Task>>();

        /// <summary>
        /// Gets the path seperator <see cref="string"/>.
        /// </summary>
        public const string PathSeparator = ".";

        /// <summary>
        /// Gets the template wildcard <see cref="string"/>.
        /// </summary>
        public const string TemplateWildcard = "*";

        /// <summary>
        /// Creates a subject from the <paramref name="template"/> replacing any '{key}' placeholders with the corresponding value found within the <paramref name="keyValuePairs"/>;
        /// where {key} enclosed by curly brackets is the corresponding value from the <paramref name="keyValuePairs"/>.
        /// </summary>
        /// <param name="template">The subject template.</param>
        /// <param name="keyValuePairs">The key/value pairs.</param>
        /// <returns>The corresponding subject.</returns>
        public static string CreateSubjectFromTemplate(string template, params KeyValuePair<string, object>[] keyValuePairs)
        {
            int start = -1;
            int end = -1;
            var subject = Check.NotEmpty(template, nameof(template)); ;

            while (true)
            {
                start = subject.IndexOf("{");
                end = subject.IndexOf("}");

                if (start < 0 && end < 0)
                    return subject;

                if (start < 0 || end < 0 || end < start)
                    throw new ArgumentException("Start and End { } parameter mismatch.", template);

                string str = subject.Substring(start, end - start + 1);
                string key = subject.Substring(start + 1, end - start - 1);

                var kvp = keyValuePairs.Where(x => StringComparer.InvariantCultureIgnoreCase.Compare(x.Key, key) == 0).FirstOrDefault();
                if (kvp.Key == null)
                    throw new ArgumentException($"Template references key '{str}' that has not been provided.", nameof(keyValuePairs));

                subject = subject.Replace(str, kvp.Value.ToString());
            }
        }

        /// <summary>
        /// Splits a subject into one or more parts.
        /// </summary>
        /// <param name="subject">The subject.</param>
        /// <returns>The subject parts.</returns>
        public static string[] SplitSubjectIntoParts(string subject)
        {
            Check.NotEmpty(subject, nameof(subject));
            return subject.Split(new string[] { PathSeparator }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Matches the <paramref name="template"/> which may contain wildcards (<see cref="TemplateWildcard"/>) with the fully defined (actual) <paramref name="subject"/>.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <param name="subject">The subject.</param>
        /// <returns><c>true</c> where there is a match; otherwise, <c>false</c></returns>
        public static bool Match(string template, string subject)
        {
            return Match(SplitSubjectIntoParts(template), SplitSubjectIntoParts(subject));
        }

        /// <summary>
        /// Matches the <paramref name="templateParts"/> which may contain wildcards (<see cref="TemplateWildcard"/>) with the fully defined (actual) <paramref name="subjectParts"/>.
        /// </summary>
        /// <param name="templateParts">The template parts.</param>
        /// <param name="subjectParts">The subject parts.</param>
        /// <returns><c>true</c> where there is a match; otherwise, <c>false</c></returns>
        public static bool Match(string[] templateParts, string[] subjectParts)
        {
            // No match where template has more parts than the subject to match.
            if (templateParts.Length > subjectParts.Length)
                return false;

            // Compare each part for an exact match or wildcard.
            for (int i = 0; i < templateParts.Length; i++)
            {
                if (i > subjectParts.Length)
                    return false;

                var match = templateParts[i] == TemplateWildcard || StringComparer.InvariantCultureIgnoreCase.Compare(templateParts[i], subjectParts[i]) == 0;
                if (!match)
                    return false;
            }

            // Where longer make sure last is a wildcard.
            if (subjectParts.Length > templateParts.Length && templateParts[templateParts.Length - 1] != TemplateWildcard)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Registers (adds) the function that will perform the <i>actual</i> <b>publish</b>; zero or more can be registered.
        /// </summary>
        /// <param name="publishFunc">The function that will perform the <b>publish</b>.</param>
        public static void Register(Func<EventData[], Task> publishFunc)
        {
            _publishFuncs.Add(publishFunc);
        }

        /// <summary>
        /// Resets the <see cref="Register(Func{EventData[], Task})"/>; removes all previously registered functions.
        /// </summary>
        public static void RegisterReset()
        {
            _publishFuncs.Clear();
        }

        /// <summary>
        /// Indicates whether the the registered (see <see cref="Register"/>) functions are executed synchronously or asynchronously (default).
        /// </summary>
        public static bool PublishSynchronously { get; set; } = false;

        /// <summary>
        /// Publishes an <see cref="EventData"/> instance using a <see cref="EventData.Subject"/> <paramref name="template"/>.
        /// </summary>
        /// <param name="template">The <see cref="EventData.Subject"/> template (see <see cref="CreateSubjectFromTemplate(string, KeyValuePair{string, object}[])"/>).</param>
        /// <param name="action">The event action.</param>
        /// <param name="keyValuePairs">The key/value pairs.</param>
        /// <returns>he <see cref="Task"/>.</returns>
        public static Task PublishAsync(string template, string action, params KeyValuePair<string, object>[] keyValuePairs)
        {
            if (_publishFuncs.Count == 0)
                return Task.CompletedTask;
            else
                return PublishAsync(new EventData[] { EventData.Create(template, action, keyValuePairs) });
        }

        /// <summary>
        /// Publishes an <see cref="EventData"/> instance using a <paramref name="value"/> and <see cref="EventData.Subject"/> <paramref name="template"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value.</param>
        /// <param name="template">The <see cref="EventData.Subject"/> template (see <see cref="CreateSubjectFromTemplate(string, KeyValuePair{string, object}[])"/>).</param>
        /// <param name="action">The event action.</param>
        /// <param name="keyValuePairs">The key/value pairs.</param>
        /// <returns>he <see cref="Task"/>.</returns>
        public static Task PublishAsync<T>(T value, string template, string action, params KeyValuePair<string, object>[] keyValuePairs)
        {
            if (_publishFuncs.Count == 0)
                return Task.CompletedTask;
            else
                return PublishAsync(new EventData[] { EventData.Create(value, template, action, keyValuePairs) });
        }

        /// <summary>
        /// Publishes an <see cref="EventData"/> instance using the <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public static Task PublishAsync<T>(T value, string subject, string action = null)
        {
            if (_publishFuncs.Count == 0)
                return Task.CompletedTask;
            else
                return PublishAsync(new EventData[] { EventData.Create(value, subject, action) });
        }

        /// <summary>
        /// Publishes an <see cref="EventData"/> instance.
        /// </summary>
        /// <param name="subject">The event subject.</param>
        /// <param name="action">The event action.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public static Task PublishAsync(string subject, string action = null)
        {
            if (_publishFuncs.Count == 0)
                return Task.CompletedTask;
            else
                return PublishAsync(new EventData[] { EventData.Create(subject, action) });
        }

        /// <summary>
        /// Publishes one of more <see cref="EventData"/> objects.
        /// </summary>
        /// <param name="data">One or more <see cref="EventData"/> objects.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public static async Task PublishAsync(params EventData[] data)
        {
            if (_publishFuncs.Count == 0 || data == null || data.Length == 0)
                return;

            Check.IsFalse(data.Any(x => string.IsNullOrEmpty(x.Subject)), nameof(data), "EventData must have a Subject.");

            if (PublishSynchronously)
            {
                foreach (var pf in _publishFuncs)
                {
                    await pf(data);
                }
            }
            else
            {
                Parallel.ForEach(_publishFuncs, (pf) =>
                {
                    pf(data).Wait();
                });
            }
        }
    }
}