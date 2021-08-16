// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;

namespace Beef.RefData
{
    /// <summary>
    /// Provides a means to manage and group one or more <b>ReferenceData</b> entities for use by the centralised <see cref="ReferenceDataManager"/>.
    /// </summary>
    public interface IReferenceDataProvider
    {
        /// <summary>
        /// Gets the provider <see cref="Type"/>.
        /// </summary>
        Type ProviderType { get; }

        /// <summary>
        /// Gets the <see cref="IReferenceDataCollection"/> for the associated <see cref="ReferenceDataBase"/> <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The associated <see cref="ReferenceDataBase"/> <see cref="Type"/>.</param>
        /// <returns>The corresponding <see cref="IReferenceDataCollection"/>.</returns>
        IReferenceDataCollection this[Type type] { get; }

        /// <summary>
        /// Prefetches all of the named <see cref="ReferenceDataBase"/> objects. 
        /// </summary>
        /// <param name="names">The list of <see cref="ReferenceDataBase"/> names.</param>
        /// <remarks>Note for implementers; should only fetch where not already cached or expired. This is provided to improve performance
        /// for consuming applications to reduce the overhead of making multiple individual invocations, i.e. reduces chattiness across a 
        /// potentially high-latency connection.</remarks>
        Task PrefetchAsync(params string[] names);
    }
}