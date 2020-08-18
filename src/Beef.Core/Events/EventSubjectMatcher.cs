// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Events
{
    /// <summary>
    /// Provides event subject matching capability.
    /// </summary>
    public static class EventSubjectMatcher
    {
        /// <summary>
        /// Splits a subject into one or more parts.
        /// </summary>
        /// <param name="pathSeparator">The path separator (see <see cref="IEventPublisher.PathSeparator"/>.</param>
        /// <param name="subject">The subject.</param>
        /// <returns>The subject parts.</returns>
        public static string[] SplitSubjectIntoParts(string pathSeparator, string subject)
        {
            Check.NotEmpty(subject, nameof(subject));
            return subject.Split(new string[] { Check.NotEmpty(pathSeparator, nameof(pathSeparator)) }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Matches the <paramref name="template"/> which may contain wildcards (<see cref="IEventPublisher.TemplateWildcard"/>) with the fully defined (actual) <paramref name="subject"/>.
        /// </summary>
        /// <param name="templateWildcard">The template wildcard (see <see cref="IEventPublisher.TemplateWildcard"/>).</param>
        /// <param name="pathSeparator">The path separator (see <see cref="IEventPublisher.PathSeparator"/>.</param>
        /// <param name="template">The template.</param>
        /// <param name="subject">The subject.</param>
        /// <returns><c>true</c> where there is a match; otherwise, <c>false</c></returns>
        public static bool Match(string templateWildcard, string pathSeparator, string? template, string? subject)
        {
            if (string.IsNullOrEmpty(pathSeparator))
                throw new ArgumentNullException(nameof(pathSeparator));

            return Match(templateWildcard, template == null ? Array.Empty<string>() : SplitSubjectIntoParts(pathSeparator, template), subject == null ? Array.Empty<string>() : SplitSubjectIntoParts(pathSeparator, subject));
        }

        /// <summary>
        /// Matches the <paramref name="templateParts"/> which may contain wildcards (<see cref="IEventPublisher.TemplateWildcard"/>) with the fully defined (actual) <paramref name="subjectParts"/>.
        /// </summary>
        /// <param name="templateWildcard">The template wildcard (see <see cref="IEventPublisher.TemplateWildcard"/>).</param>
        /// <param name="templateParts">The template parts.</param>
        /// <param name="subjectParts">The subject parts.</param>
        /// <returns><c>true</c> where there is a match; otherwise, <c>false</c></returns>
        public static bool Match(string templateWildcard, string[] templateParts, string[] subjectParts)
        {
            if (string.IsNullOrEmpty(templateWildcard))
                throw new ArgumentNullException(nameof(templateWildcard));

            // No match where template has more parts than the subject to match.
            if (Check.NotNull(templateParts, nameof(templateParts)).Length > Check.NotNull(subjectParts, nameof(subjectParts)).Length)
                return false;

            // Compare each part for an exact match or wildcard.
            for (int i = 0; i < templateParts.Length; i++)
            {
                if (i > subjectParts.Length)
                    return false;

                var match = templateParts[i] == templateWildcard || StringComparer.InvariantCultureIgnoreCase.Compare(templateParts[i], subjectParts[i]) == 0;
                if (!match)
                    return false;
            }

            // Where longer make sure last is a wildcard.
            if (subjectParts.Length > templateParts.Length && templateParts[^1] != templateWildcard)
                return false;
            else
                return true;
        }
    }
}