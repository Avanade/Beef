// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;

namespace Beef.Data.Database
{
    /// <summary>
    /// Defines the standard <b>Reference Data</b> database column names.
    /// </summary>
    public static class DatabaseRefDataColumns
    {
        /// <summary>
        /// Gets or sets the <see cref="ReferenceDataBase.Id"/> database column name (defaults to <see cref="ReferenceDataBase.Id"/>).
        /// </summary>
        public static string IdColumnName { get; set; } = nameof(ReferenceDataBase.Id);

        /// <summary>
        /// Gets or sets the <see cref="ReferenceDataBase.Code"/> database column name (defaults to <see cref="ReferenceDataBase.Code"/>).
        /// </summary>
        public static string CodeColumnName { get; set; } = nameof(ReferenceDataBase.Code);

        /// <summary>
        /// Gets or sets the <see cref="ReferenceDataBase.Text"/> database column name (defaults to <see cref="ReferenceDataBase.Text"/>).
        /// </summary>
        public static string TextColumnName { get; set; } = nameof(ReferenceDataBase.Text);

        /// <summary>
        /// Gets or sets the <see cref="ReferenceDataBase.Description"/> database column name (defaults to <see cref="ReferenceDataBase.Description"/>).
        /// </summary>
        public static string DescriptionColumnName { get; set; } = nameof(ReferenceDataBase.Description);

        /// <summary>
        /// Gets or sets the <see cref="ReferenceDataBase.SortOrder"/> database column name (defaults to <see cref="ReferenceDataBase.SortOrder"/>).
        /// </summary>
        public static string SortOrderColumnName { get; set; } = nameof(ReferenceDataBase.SortOrder);

        /// <summary>
        /// Gets or sets the <see cref="ReferenceDataBase.IsActive"/> database column name (defaults to <see cref="ReferenceDataBase.IsActive"/>).
        /// </summary>
        public static string IsActiveColumnName { get; set; } = nameof(ReferenceDataBase.IsActive);

        /// <summary>
        /// Gets or sets the <see cref="ReferenceDataBase.StartDate"/> database column name (defaults to <see cref="ReferenceDataBase.StartDate"/>).
        /// </summary>
        public static string StartDateColumnName { get; set; } = nameof(ReferenceDataBase.StartDate);

        /// <summary>
        /// Gets or sets the <see cref="ReferenceDataBase.EndDate"/> database column name (defaults to <see cref="ReferenceDataBase.EndDate"/>).
        /// </summary>
        public static string EndDateColumnName { get; set; } = nameof(ReferenceDataBase.EndDate);

        /// <summary>
        /// Gets or sets the <see cref="ReferenceDataBase.ETag"/> database column name (defaults to "RowVersion").
        /// </summary>
        public static string ETagColumnName { get; set; } = "RowVersion";
    }
}
