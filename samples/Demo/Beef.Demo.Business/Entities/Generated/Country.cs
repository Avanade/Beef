/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Beef.Demo.Business.Entities;

/// <summary>
/// Represents the Country entity.
/// </summary>
public partial class Country : ReferenceDataBaseEx<Guid, Country>
{
    /// <summary>
    /// An implicit cast from an <see cref="IIdentifier.Id"> to <see cref="Country"/>.
    /// </summary>
    /// <param name="id">The <see cref="IIdentifier.Id">.</param>
    /// <returns>The corresponding <see cref="Country"/>.</returns>
    public static implicit operator Country?(Guid id) => ConvertFromId(id);

    /// <summary>
    /// An implicit cast from a <see cref="IReferenceData.Code"> to <see cref="Country"/>.
    /// </summary>
    /// <param name="code">The <see cref="IReferenceData.Code">.</param>
    /// <returns>The corresponding <see cref="Country"/>.</returns>
    public static implicit operator Country?(string? code) => ConvertFromCode(code);
}

/// <summary>
/// Represents the <see cref="Country"/> collection.
/// </summary>
public partial class CountryCollection : ReferenceDataCollectionBase<Guid, Country, CountryCollection>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CountryCollection"/> class.
    /// </summary>
    public CountryCollection() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CountryCollection"/> class with <paramref name="items"/> to add.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public CountryCollection(IEnumerable<Country> items) => AddRange(items);
}

#pragma warning restore
#nullable restore