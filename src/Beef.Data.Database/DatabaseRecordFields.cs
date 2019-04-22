// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.ObjectModel;

namespace Beef.Data.Database
{
    /// <summary>
    /// Represents the collection of <see cref="DatabaseRecord"/> <see cref="DatabaseRecordField">fields</see> returned.
    /// </summary>
    public class DatabaseRecordFields : KeyedCollection<string, DatabaseRecordField>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseRecordFields"/> class determining all the fields from the <see cref="DatabaseRecord"/>.
        /// </summary>
        /// <param name="dr">The <see cref="DatabaseRecord"/>.</param>
        public DatabaseRecordFields(DatabaseRecord dr)
        {
            if (dr == null)
                throw new ArgumentNullException(nameof(dr));

            for (int i = 0; i < dr.DataRecord.FieldCount; i++)
            {
                Add(new DatabaseRecordField { Name = dr.DataRecord.GetName(i), Index = i });
            }
        }

        /// <summary>
        /// Gets the key (see <see cref="DatabaseRecordField"/> <see cref="DatabaseRecordField.Name"/>) for the item.
        /// </summary>
        /// <param name="item">The <see cref="DatabaseRecordField"/>.</param>
        /// <returns>The <see cref="DatabaseRecordField"/> <see cref="DatabaseRecordField.Name"/>.</returns>
        protected override string GetKeyForItem(DatabaseRecordField item)
        {
            return item.Name;
        }
    }

    /// <summary>
    /// Represents the <see cref="DatabaseRecord"/> field/column.
    /// </summary>
    public class DatabaseRecordField
    {
        /// <summary>
        /// Gets or sets the field name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the field index.
        /// </summary>
        public int Index { get; set; }
    }
}
