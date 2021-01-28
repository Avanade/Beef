/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0001, IDE0005, IDE0044, IDE0079, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beef;
using Beef.Entities;
using Beef.Demo.Common.Entities;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Defines the <see cref="Contact"/> data access.
    /// </summary>
    public partial interface IContactData
    {
        /// <summary>
        /// Gets the <see cref="ContactCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <returns>The <see cref="ContactCollectionResult"/>.</returns>
        Task<ContactCollectionResult> GetAllAsync();

        /// <summary>
        /// Gets the specified <see cref="Contact"/>.
        /// </summary>
        /// <param name="id">The <see cref="Contact"/> identifier.</param>
        /// <returns>The selected <see cref="Contact"/> where found.</returns>
        Task<Contact?> GetAsync(Guid id);

        /// <summary>
        /// Creates a new <see cref="Contact"/>.
        /// </summary>
        /// <param name="value">The <see cref="Contact"/>.</param>
        /// <returns>The created <see cref="Contact"/>.</returns>
        Task<Contact> CreateAsync(Contact value);

        /// <summary>
        /// Updates an existing <see cref="Contact"/>.
        /// </summary>
        /// <param name="value">The <see cref="Contact"/>.</param>
        /// <returns>The updated <see cref="Contact"/>.</returns>
        Task<Contact> UpdateAsync(Contact value);

        /// <summary>
        /// Deletes the specified <see cref="Contact"/>.
        /// </summary>
        /// <param name="id">The <see cref="Contact"/> identifier.</param>
        Task DeleteAsync(Guid id);
    }
}

#pragma warning restore IDE0001, IDE0005, IDE0044, IDE0079, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649
#nullable restore