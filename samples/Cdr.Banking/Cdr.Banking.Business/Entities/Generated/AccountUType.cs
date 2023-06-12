/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Cdr.Banking.Business.Entities
{
    /// <summary>
    /// Represents the Account U Type entity.
    /// </summary>
    public partial class AccountUType : ReferenceDataBaseEx<Guid, AccountUType>
    {
        /// <summary>
        /// An implicit cast from an <see cref="IIdentifier.Id"> to <see cref="AccountUType"/>.
        /// </summary>
        /// <param name="id">The <see cref="IIdentifier.Id">.</param>
        /// <returns>The corresponding <see cref="AccountUType"/>.</returns>
        public static implicit operator AccountUType?(Guid id) => ConvertFromId(id);

        /// <summary>
        /// An implicit cast from a <see cref="IReferenceData.Code"> to <see cref="AccountUType"/>.
        /// </summary>
        /// <param name="code">The <see cref="IReferenceData.Code">.</param>
        /// <returns>The corresponding <see cref="AccountUType"/>.</returns>
        public static implicit operator AccountUType?(string? code) => ConvertFromCode(code);
    }

    /// <summary>
    /// Represents the <see cref="AccountUType"/> collection.
    /// </summary>
    public partial class AccountUTypeCollection : ReferenceDataCollectionBase<Guid, AccountUType, AccountUTypeCollection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccountUTypeCollection"/> class.
        /// </summary>
        public AccountUTypeCollection() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountUTypeCollection"/> class with <paramref name="items"/> to add.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public AccountUTypeCollection(IEnumerable<AccountUType> items) => AddRange(items);
    }
}

#pragma warning restore
#nullable restore