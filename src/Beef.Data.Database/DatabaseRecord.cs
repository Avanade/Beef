// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System;
using System.Data;

namespace Beef.Data.Database
{
    /// <summary>
    /// Extends the <see cref="IDataRecord"/> and provides easier <see cref="GetValue{T}(string)"/> capabilities.
    /// </summary>
    public class DatabaseRecord
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseRecord"/> class.
        /// </summary>
        /// <param name="dataCommand">The owning <see cref="DatabaseCommand"/>.</param>
        /// <param name="dataRecord">The underlying <see cref="IDataRecord"/>.</param>
        public DatabaseRecord(DatabaseCommand dataCommand, IDataRecord dataRecord)
        {
            DatabaseCommand = dataCommand ?? throw new ArgumentNullException(nameof(dataCommand));
            DataRecord = dataRecord ?? throw new ArgumentNullException(nameof(dataRecord));
        }

        /// <summary>
        /// Gets the owning <see cref="Database.DatabaseCommand"/>.
        /// </summary>
        public DatabaseCommand DatabaseCommand { get; set; }

        /// <summary>
        /// Gets the underlying <see cref="IDataRecord"/>.
        /// </summary>
        public IDataRecord DataRecord { get; private set; }

        /// <summary>
        /// Gets the ordinal index for the named field.
        /// </summary>
        /// <param name="name">The field name.</param>
        /// <returns>The ordinal index for the column.</returns>
        public int GetOrdinal(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            try
            {
                return DataRecord.GetOrdinal(name);
            }
            catch (IndexOutOfRangeException iore)
            {
                throw new IndexOutOfRangeException($"Value not available for field '{name}'.", iore);
            }
        }

        /// <summary>
        /// Gets the <see cref="IDataRecord"/> value for the named field.
        /// </summary>
        /// <typeparam name="T">The resultant value <see cref="Type"/>.</typeparam>
        /// <param name="name">The field name.</param>
        /// <returns>The resultant value.</returns>
        public T GetValue<T>(string name) => GetValue<T>(GetOrdinal(name));

        /// <summary>
        /// Gets the <see cref="IDataRecord"/> value for the specified ordinal index.
        /// </summary>
        /// <typeparam name="T">The resultant value <see cref="Type"/>.</typeparam>
        /// <param name="index">The ordinal index for the column.</param>
        /// <returns>The value.</returns>
        public T GetValue<T>(int index)
        {
            if (index < 0 || index >= DataRecord.FieldCount)
                throw new IndexOutOfRangeException("Index is not within the bounds of the underlying IDataRecord.FieldCount.");

            object dbVal = DataRecord.GetValue(index);
            if (dbVal is DateTime datetime)
                dbVal = Cleaner.Clean(datetime, DatabaseCommand.Database.DateTimeTransform);

            if (dbVal is DBNull)
                return default!;

            Type nt = Nullable.GetUnderlyingType(typeof(T));
            if (nt == null)
            {
                if (typeof(T).IsEnum)
                    return (T)Enum.ToObject(typeof(T), dbVal);
                else
                    return (T)Convert.ChangeType(dbVal, typeof(T), System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                if (nt.IsEnum)
                    return (T)Enum.ToObject(nt, dbVal);
                else
                    return (T)Convert.ChangeType(dbVal, nt, System.Globalization.CultureInfo.InvariantCulture);
            }
        }

        /// <summary>
        /// Indicates whether the column value is <see cref="DBNull"/> for the specified ordinal index.
        /// </summary>
        /// <param name="index">The ordinal index for the column.</param>
        /// <returns><c>true</c> indicates that the column value has a <see cref="DBNull"/> value; otherwise, <c>false</c>.</returns>
        public bool IsDBNull(int index)
        {
            return DataRecord.IsDBNull(index);
        }

        /// <summary>
        /// Indicates whether the column value is <see cref="DBNull"/> for the specified column name.
        /// </summary>
        /// <param name="columnName">The name of the column.</param>
        /// <returns><c>true</c> indicates that the column value has a <see cref="DBNull"/> value; otherwise, <c>false</c>.</returns>
        public bool IsDBNull(string columnName)
        {
            return DataRecord.IsDBNull(GetOrdinal(columnName));
        }

        /// <summary>
        /// Gets the named <b>RowVersion</b> <see cref="IDataRecord"/> value for the specified column.
        /// </summary>
        /// <param name="columnName">The name of the column (defaults to <see cref="DatabaseColumns.RowVersionName"/>.</param>
        /// <returns>The resultant value.</returns>
        /// <remarks>The <b>RowVersion</b> <see cref="byte"/> array will be converted to an <see cref="Convert.ToBase64String(byte[])">encoded</see> <see cref="string"/> value.</remarks>
        public string GetRowVersion(string? columnName = null)
        {
            int ordinal = DataRecord.GetOrdinal(string.IsNullOrEmpty(columnName) ? DatabaseColumns.RowVersionName : columnName);
            return GetRowVersion(ordinal);
        }

        /// <summary>
        /// Gets the <b>RowVersion</b> <see cref="IDataRecord"/> value for the specified ordinal index.
        /// </summary>
        /// <param name="index">The ordinal index for the column.</param>
        /// <returns>The resultant value.</returns>
        /// <remarks>The <b>RowVersion</b> <see cref="byte"/> array will be converted to an <see cref="Convert.ToBase64String(byte[])">encoded</see> <see cref="string"/> value.</remarks>
        public string GetRowVersion(int index)
        {
            return Convert.ToBase64String(GetValue<byte[]>(index));
        }

        /// <summary>
        /// Gets the <see cref="ChangeLog"/> <see cref="IDataRecord"/> value.
        /// </summary>
        /// <returns>The <see cref="ChangeLog"/> value or <c>null</c> where <see cref="ChangeLog.IsInitial"/>.</returns>
        /// <remarks>Uses the following column names: <see cref="DatabaseColumns.CreatedByName"/>, <see cref="DatabaseColumns.CreatedDateName"/>,
        /// <see cref="DatabaseColumns.UpdatedByName"/> and <see cref="DatabaseColumns.UpdatedDateName"/>.</remarks>
        public ChangeLog? GetChangeLog()
        {
            var log = new ChangeLog
            {
                CreatedDate = GetValue<DateTime?>(DatabaseColumns.CreatedDateName),
                CreatedBy = GetValue<string>(DatabaseColumns.CreatedByName),
                UpdatedDate = GetValue<DateTime?>(DatabaseColumns.UpdatedDateName),
                UpdatedBy = GetValue<string>(DatabaseColumns.UpdatedByName)
            };

            return log.IsInitial ? null : log;
        }

        /// <summary>
        /// Gets the <see cref="DatabaseRecordFieldCollection"/> for the <see cref="DatabaseRecord"/>.
        /// </summary>
        /// <returns>The <see cref="DatabaseRecordFieldCollection"/>.</returns>
        public DatabaseRecordFieldCollection GetFields()
        {
            return new DatabaseRecordFieldCollection(this);
        }
    }
}