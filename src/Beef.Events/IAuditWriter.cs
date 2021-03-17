// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
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
        /// <param name="data">The event data.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        Task WriteAuditAsync(object data, Result result);
    }

    /// <summary>
    /// Enables the writing (persistence) of a <see cref="Result"/> audit record where the event data is of <see cref="Type"/> <typeparamref name="TData"/>.
    /// </summary>
    /// <typeparam name="TData">The event data <see cref="Type"/>.</typeparam>
    public interface IAuditWriter<TData> : IAuditWriter where TData : class
    {
        /// <summary>
        /// Writes the event <paramref name="data"/> <paramref name="result"/> to the audit repository.
        /// </summary>
        /// <param name="data">The event data.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        Task IAuditWriter.WriteAuditAsync(object data, Result result) => WriteAuditAsync((TData)data, result);

        /// <summary>
        /// Writes the <paramref name="result"/> for an <paramref name="originatingEvent"/> to the audit repository.
        /// </summary>
        /// <param name="originatingEvent">The event data.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        Task WriteAuditAsync(TData originatingEvent, Result result);
    }
}