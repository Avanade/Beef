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
        /// Gets or sets the <see cref="ReferenceDataBase.Id"/> database column name (defaults to <see cref="ReferenceDataBase.Property_Id"/>).
        /// </summary>
        public static string IdColumnName { get; set; } = ReferenceDataBase.Property_Id;

        /// <summary>
        /// Gets or sets the <see cref="ReferenceDataBase.Code"/> database column name (defaults to <see cref="ReferenceDataBase.Property_Code"/>).
        /// </summary>
        public static string CodeColumnName { get; set; } = ReferenceDataBase.Property_Code;

        /// <summary>
        /// Gets or sets the <see cref="ReferenceDataBase.Text"/> database column name (defaults to <see cref="ReferenceDataBase.Property_Text"/>).
        /// </summary>
        public static string TextColumnName { get; set; } = ReferenceDataBase.Property_Text;

        /// <summary>
        /// Gets or sets the <see cref="ReferenceDataBase.Description"/> database column name (defaults to <see cref="ReferenceDataBase.Property_Description"/>).
        /// </summary>
        public static string DescriptionColumnName { get; set; } = ReferenceDataBase.Property_Description;

        /// <summary>
        /// Gets or sets the <see cref="ReferenceDataBase.SortOrder"/> database column name (defaults to <see cref="ReferenceDataBase.Property_SortOrder"/>).
        /// </summary>
        public static string SortOrderColumnName { get; set; } = ReferenceDataBase.Property_SortOrder;

        /// <summary>
        /// Gets or sets the <see cref="ReferenceDataBase.IsActive"/> database column name (defaults to <see cref="ReferenceDataBase.Property_IsActive"/>).
        /// </summary>
        public static string IsActiveColumnName { get; set; } = ReferenceDataBase.Property_IsActive;

        /// <summary>
        /// Gets or sets the <see cref="ReferenceDataBase.StartDate"/> database column name (defaults to <see cref="ReferenceDataBase.Property_StartDate"/>).
        /// </summary>
        public static string StartDateColumnName { get; set; } = ReferenceDataBase.Property_StartDate;

        /// <summary>
        /// Gets or sets the <see cref="ReferenceDataBase.EndDate"/> database column name (defaults to <see cref="ReferenceDataBase.Property_EndDate"/>).
        /// </summary>
        public static string EndDateColumnName { get; set; } = ReferenceDataBase.Property_EndDate;

        /// <summary>
        /// Gets or sets the <see cref="ReferenceDataBase.ETag"/> database column name (defaults to "RowVersion").
        /// </summary>
        public static string ETagColumnName { get; set; } = "RowVersion";
    }
}
