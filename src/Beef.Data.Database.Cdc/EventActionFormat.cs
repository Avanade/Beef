// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Defines the event subject format.
    /// </summary>
    public enum EventActionFormat
    {
        /// <summary>
        /// No formatting, leave action as-is.
        /// </summary>
        None,

        /// <summary>
        /// The action as past-tense.
        /// </summary>
        PastTense
    }

    /// <summary>
    /// Provides the <see cref="EventActionFormat"/> formatting capability. 
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
            EventActionFormat.PastTense => StringConversion.ToPastTense(Check.NotEmpty(action, nameof(action)))!,
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