// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Events
{
    /// <summary>
    /// Defines the event action format.
    /// </summary>
    public enum EventActionFormat
    {
        /// <summary>
        /// No formatting; as-is (default).
        /// </summary>
        None,

        /// <summary>
        /// Format as upper case.
        /// </summary>
        UpperCase,

        /// <summary>
        /// Format as past tense.
        /// </summary>
        PastTense,

        /// <summary>
        /// Format as past tense upper case.
        /// </summary>
        PastTenseUpperCase
    }

    /// <summary>
    /// Provides the event action formatting capability. 
    /// </summary>
    public static class EventActionFormatter
    {
        /// <summary>
        /// Formats the <paramref name="action"/> based on the specified <paramref name="format"/>.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="format">The <see cref="EventActionFormat"/>.</param>
        /// <returns>The formatted action.</returns>
        public static string Format(string action, EventActionFormat? format) => format switch
        {
            EventActionFormat.UpperCase => Check.NotEmpty(action, nameof(action)).ToUpperInvariant()!,
            EventActionFormat.PastTense => StringConversion.ToPastTense(Check.NotEmpty(action, nameof(action)))!,
            EventActionFormat.PastTenseUpperCase => StringConversion.ToPastTense(Check.NotEmpty(action, nameof(action)))!.ToUpperInvariant(),
            _ => Check.NotEmpty(action, nameof(action))
        };

        /// <summary>
        /// Formats the <paramref name="operationType"/> based on the specified <paramref name="format"/>.
        /// </summary>
        /// <param name="operationType">The <see cref="OperationType"/>.</param>
        /// <param name="format">The <see cref="EventActionFormat"/>.</param>
        /// <returns>The formatted action.</returns>
        public static string Format(OperationType operationType, EventActionFormat? format) => Format(operationType.ToString()!, format);
    }
}