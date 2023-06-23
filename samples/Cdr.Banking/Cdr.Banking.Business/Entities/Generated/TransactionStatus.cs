/*
 * This file is automatically generated; any changes will be lost. 
 */

namespace Cdr.Banking.Business.Entities;

/// <summary>
/// Represents the Transaction Status entity.
/// </summary>
public partial class TransactionStatus : ReferenceDataBaseEx<Guid, TransactionStatus>
{
    /// <summary>
    /// An implicit cast from an <see cref="IIdentifier.Id"> to <see cref="TransactionStatus"/>.
    /// </summary>
    /// <param name="id">The <see cref="IIdentifier.Id">.</param>
    /// <returns>The corresponding <see cref="TransactionStatus"/>.</returns>
    public static implicit operator TransactionStatus?(Guid id) => ConvertFromId(id);

    /// <summary>
    /// An implicit cast from a <see cref="IReferenceData.Code"> to <see cref="TransactionStatus"/>.
    /// </summary>
    /// <param name="code">The <see cref="IReferenceData.Code">.</param>
    /// <returns>The corresponding <see cref="TransactionStatus"/>.</returns>
    public static implicit operator TransactionStatus?(string? code) => ConvertFromCode(code);
}

/// <summary>
/// Represents the <see cref="TransactionStatus"/> collection.
/// </summary>
public partial class TransactionStatusCollection : ReferenceDataCollectionBase<Guid, TransactionStatus, TransactionStatusCollection>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionStatusCollection"/> class.
    /// </summary>
    public TransactionStatusCollection() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="TransactionStatusCollection"/> class with <paramref name="items"/> to add.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public TransactionStatusCollection(IEnumerable<TransactionStatus> items) => AddRange(items);
}