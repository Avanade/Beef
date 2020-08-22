/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0005 // Using directive is unnecessary; are required depending on code-gen options

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beef;
using Beef.Entities;
using Beef.Demo.Common.Entities;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Business
{
    /// <summary>
    /// Defines the <see cref="Contact"/> business functionality.
    /// </summary>
    public partial interface IContactManager
    {
        /// <summary>
        /// Gets the <see cref="ContactCollectionResult"/> that includes the items that match the selection criteria.
        /// </summary>
        /// <returns>The <see cref="ContactCollectionResult"/>.</returns>
        Task<ContactCollectionResult> GetAllAsync();

        /// <summary>
        /// Gets the specified <see cref="Contact"/>.
        /// </summary>
        /// <param name="id">The <see cref="Contact"/> identifier.</param>
        /// <returns>The selected <see cref="Contact"/> where found; otherwise, <c>null</c>.</returns>
        Task<Contact?> GetAsync(Guid id);

        /// <summary>
        /// Creates a new <see cref="Contact"/>.
        /// </summary>
        /// <param name="value">The <see cref="Contact"/>.</param>
        /// <returns>A refreshed <see cref="Contact"/>.</returns>
        Task<Contact> CreateAsync(Contact value);

        /// <summary>
        /// Updates an existing <see cref="Contact"/>.
        /// </summary>
        /// <param name="value">The <see cref="Contact"/>.</param>
        /// <param name="id">The <see cref="Contact"/> identifier.</param>
        /// <returns>A refreshed <see cref="Contact"/>.</returns>
        Task<Contact> UpdateAsync(Contact value, Guid id);

        /// <summary>
        /// Deletes the specified <see cref="Contact"/>.
        /// </summary>
        /// <param name="id">The <see cref="Contact"/> identifier.</param>
        Task DeleteAsync(Guid id);
    }
}

#pragma warning restore IDE0005
#nullable restore