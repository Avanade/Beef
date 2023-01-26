/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Cdr.Banking.Business.Entities
{
    /// <summary>
    /// Represents the Maturity Instructions entity.
    /// </summary>
    public partial class MaturityInstructions : ReferenceDataBaseEx<Guid, MaturityInstructions>
    {
        /// <summary>
        /// An implicit cast from an <see cref="IIdentifier.Id"> to <see cref="MaturityInstructions"/>.
        /// </summary>
        /// <param name="id">The <see cref="IIdentifier.Id">.</param>
        /// <returns>The corresponding <see cref="MaturityInstructions"/>.</returns>
        public static implicit operator MaturityInstructions?(Guid id) => ConvertFromId(id);

        /// <summary>
        /// An implicit cast from a <see cref="IReferenceData.Code"> to <see cref="MaturityInstructions"/>.
        /// </summary>
        /// <param name="code">The <see cref="IReferenceData.Code">.</param>
        /// <returns>The corresponding <see cref="MaturityInstructions"/>.</returns>
        public static implicit operator MaturityInstructions?(string? code) => ConvertFromCode(code);
    }

    /// <summary>
    /// Represents the <see cref="MaturityInstructions"/> collection.
    /// </summary>
    public partial class MaturityInstructionsCollection : ReferenceDataCollectionBase<Guid, MaturityInstructions, MaturityInstructionsCollection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MaturityInstructionsCollection"/> class.
        /// </summary>
        public MaturityInstructionsCollection() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaturityInstructionsCollection"/> class with <paramref name="items"/> to add.
        /// </summary>
        /// <param name="items">The items to add.</param>
        public MaturityInstructionsCollection(IEnumerable<MaturityInstructions> items) => AddRange(items);
    }
}

#pragma warning restore
#nullable restore