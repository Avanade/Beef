// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.RefData
{
    /// <summary>
    /// Provides the standard <b>ReferenceData</b> properties
    /// </summary>
    public interface IReferenceData
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <remarks>Once set this value cannot be updated (it becomes immutable). The underlying <see cref="Type"/> for the <b>Id</b> is determined by the
        /// implementing class.</remarks>
        object? Id { get; set; }

        /// <summary>
        /// Gets or sets the code.
        /// </summary>
        string? Code { get; set; }

        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        string? Text { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        string? Description { get; set; }

        /// <summary>
        /// Gets or sets the sort order.
        /// </summary>
        int SortOrder { get; set; }

        /// <summary>
        /// Indicates whether the <see cref="ReferenceDataBase"/> is Active.
        /// </summary>
        /// <value><c>true</c> where Active; otherwise, <c>false</c>.</value>
        bool IsActive { get; set; }

        /// <summary>
        /// Indicates whether the <see cref="ReferenceDataBase"/> is valid.
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Gets of sets the validity start date.
        /// </summary>
        DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets of sets the validity end date.
        /// </summary>
        DateTime? EndDate { get; set; }

        /// <summary>
        /// Gets or sets the entity tag.
        /// </summary>
        string? ETag { get; set; }
    }
}
