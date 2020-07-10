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

#pragma warning disable CA1043 // Use Integral Or String Argument For Indexers; by-design.
        /// <summary>
        /// Gets the <see cref="IReferenceDataCollection"/> for the associated <see cref="ReferenceDataBase"/> <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The associated <see cref="ReferenceDataBase"/> <see cref="Type"/>.</param>
        /// <returns>The corresponding <see cref="IReferenceDataCollection"/>.</returns>
        IReferenceDataCollection this[Type type] { get; }
#pragma warning restore CA1043 

        /// <summary>
        /// Gets all the underlying <see cref="ReferenceDataBase"/> <see cref="Type">types</see>.
        /// </summary>
        /// <returns>An array of the <see cref="ReferenceDataBase"/> <see cref="Type">types</see>.</returns>
        Type[] GetAllTypes();

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