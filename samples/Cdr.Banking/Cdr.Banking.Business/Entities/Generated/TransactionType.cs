/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using CoreEx.Entities;
using CoreEx.Entities.Extended;
using CoreEx.RefData;
using RefDataNamespace = Cdr.Banking.Business.Entities;

namespace Cdr.Banking.Business.Entities
{
    /// <summary>
    /// Represents the Transaction Type entity.
    /// </summary>
    public partial class TransactionType : ReferenceDataBase<Guid, TransactionType>
    {

        /// <summary>
        /// An implicit cast from a <see cref="IReferenceData.Code"> to a <see cref="TransactionType"/>.
        /// </summary>
        /// <param name="code">The <b>Code</b>.</param>
        /// <returns>The corresponding <see cref="TransactionType"/>.</returns>
        public static implicit operator TransactionType?(string? code) => ConvertFromCode(code);
    }

    /// <summary>
    /// Represents the <see cref="TransactionType"/> collection.
    /// </summary>
    public partial class TransactionTypeCollection : ReferenceDataCollectionBase<Guid, TransactionType, TransactionTypeCollection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionTypeCollection"/> class.
        /// </summary>
        public TransactionTypeCollection() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionTypeCollection"/> class with a <paramref name="collection"/> of items to add.
        /// </summary>
        /// <param name="collection">A collection containing items to add.</param>
        public TransactionTypeCollection(IEnumerable<TransactionType> collection) => AddRange(collection);
    }
}

#pragma warning restore
#nullable restore