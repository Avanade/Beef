/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Beef.Demo.Business.Entities;

/// <summary>
/// Represents the Communication Type entity.
/// </summary>
public partial class CommunicationType : ReferenceDataBaseEx<int, CommunicationType>
{
    /// <summary>
    /// An implicit cast from an <see cref="IIdentifier.Id"> to <see cref="CommunicationType"/>.
    /// </summary>
    /// <param name="id">The <see cref="IIdentifier.Id">.</param>
    /// <returns>The corresponding <see cref="CommunicationType"/>.</returns>
    public static implicit operator CommunicationType?(int id) => ConvertFromId(id);

    /// <summary>
    /// An implicit cast from a <see cref="IReferenceData.Code"> to <see cref="CommunicationType"/>.
    /// </summary>
    /// <param name="code">The <see cref="IReferenceData.Code">.</param>
    /// <returns>The corresponding <see cref="CommunicationType"/>.</returns>
    public static implicit operator CommunicationType?(string? code) => ConvertFromCode(code);
}

/// <summary>
/// Represents the <see cref="CommunicationType"/> collection.
/// </summary>
public partial class CommunicationTypeCollection : ReferenceDataCollectionBase<int, CommunicationType, CommunicationTypeCollection>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommunicationTypeCollection"/> class.
    /// </summary>
    public CommunicationTypeCollection() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommunicationTypeCollection"/> class with <paramref name="items"/> to add.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public CommunicationTypeCollection(IEnumerable<CommunicationType> items) => AddRange(items);
}

#pragma warning restore
#nullable restore