// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;

namespace Beef.Events.Subscribe
{
    /// <summary>
    /// Enables the writing (persistence) of a <see cref="Result"/> audit record.
    /// </summary>
    public interface IAuditWriter
    {
        /// <summary>
        /// Writes the <paramref name="result"/> for an <paramref name="originatingEvent"/> to the audit repository.
        /// </summary>
        /// <param name="originatingEvent">The originating event.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        Task WriteAuditAsync(object originatingEvent, Result result);
    }

    /// <summary>
    /// Enables the writing (persistence) of a <see cref="Result"/> audit record where the event is of <see cref="Type"/> <typeparamref name="TEvent"/>.
    /// </summary>
    /// <typeparam name="TEvent">The event <see cref="Type"/>.</typeparam>
    public interface IAuditWriter<TEvent> : IAuditWriter where TEvent : class
    {
        /// <summary>
        /// Writes the <paramref name="result"/> for an <paramref name="originatingEvent"/> to the audit repository.
        /// </summary>
        /// <param name="originatingEvent">The originating event.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        Task IAuditWriter.WriteAuditAsync(object originatingEvent, Result result) => WriteAuditAsync((TEvent)originatingEvent, result);

        /// <summary>
        /// Writes the <paramref name="result"/> for an <paramref name="originatingEvent"/> to the audit repository.
        /// </summary>
        /// <param name="originatingEvent">The originating event.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        Task WriteAuditAsync(TEvent originatingEvent, Result result);
    }
}