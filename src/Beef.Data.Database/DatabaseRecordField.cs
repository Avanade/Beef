// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.ObjectModel;

namespace Beef.Data.Database
{
    /// <summary>
    /// Represents the collection of <see cref="DatabaseRecord"/> <see cref="DatabaseRecordField">fields</see> returned.
    /// </summary>
    public class DatabaseRecordFieldCollection : KeyedCollection<string, DatabaseRecordField>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseRecordFieldCollection"/> class determining all the fields from the <see cref="DatabaseRecord"/>.
        /// </summary>
        /// <param name="dr">The <see cref="DatabaseRecord"/>.</param>
        public DatabaseRecordFieldCollection(DatabaseRecord dr)
        {
            if (dr == null)
                throw new ArgumentNullException(nameof(dr));

            for (int i = 0; i < dr.DataRecord.FieldCount; i++)
            {
                Add(new DatabaseRecordField(dr.DataRecord.GetName(i), i));
            }
        }

        /// <summary>
        /// Gets the key (see <see cref="DatabaseRecordField"/> <see cref="DatabaseRecordField.Name"/>) for the item.
        /// </summary>
        /// <param name="item">The <see cref="DatabaseRecordField"/>.</param>
        /// <returns>The <see cref="DatabaseRecordField"/> <see cref="DatabaseRecordField.Name"/>.</returns>
        protected override string GetKeyForItem(DatabaseRecordField item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            return item.Name;
        }
    }

    /// <summary>
    /// Represents the <see cref="DatabaseRecord"/> field/column.
    /// </summary>
    public class DatabaseRecordField
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseRecordField"/> class.
        /// </summary>
        /// <param name="name">The field name.</param>
        /// <param name="index">The field index.</param>
        public DatabaseRecordField(string name, int index)
        {
            Name = Check.NotEmpty(name, nameof(name));
            Index = index;
        }

        /// <summary>
        /// Gets or sets the field name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the field index.
        /// </summary>
        public int Index { get; private set; }
    }
}