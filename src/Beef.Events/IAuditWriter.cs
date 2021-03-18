// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Threading.Tasks;

namespace Beef.Events
{
    /// <summary>
    /// Enables the writing (persistence) of a <see cref="Result"/> audit record.
    /// </summary>
    public interface IAuditWriter
    {
        /// <summary>
        /// Writes the event <paramref name="data"/> <paramref name="result"/> to the audit repository.
        /// </summary>
        /// <param name="data">The <see cref="IEventSubscriberData"/>.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        Task WriteAuditAsync(IEventSubscriberData data, Result result);
    }
}