// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile.Reflectors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beef.FlatFile
{
    /// <summary>
    /// Represents an automagical column header record.
    /// <para>For a <b>read</b> it will check that the column header names match (<see cref="StringComparer.OrdinalIgnoreCase"/>) the primary
    /// <see cref="FileContentStatus.Content"/> (see <see cref="FileFormatBase"/> <see cref="FileFormatBase.ContentRowType"/>) column names (see <see cref="FileColumnAttribute"/>
    /// <see cref="FileColumnAttribute.Name"/>)</para>
    /// <para>For a <b>write</b> it will output the header record using the column header named. Can not be used where there are
    /// <see cref="FileFormatBase.IsHierarchical">hierarchical</see> records within the file.</para>
    /// </summary>
    public sealed class ColumnNameHeader : IFileRecord
    {
        /// <summary>
        /// Gets the single default instance.
        /// </summary>
        public static ColumnNameHeader Default { get; } = new ColumnNameHeader();

        /// <summary>
        /// Validates that the column header names match.
        /// </summary>
        /// <param name="fileFormat">The <see cref="FileFormatBase"/>.</param>
        /// <param name="record">The <see cref="FileRecord"/>.</param>
        void IFileRecord.OnRead(FileFormatBase fileFormat, FileRecord record)
        {
            var frr = CheckConfiguration(fileFormat, record);

            for (int i = 0; i < frr.Columns.Count; i++)
            {
                if (i >= record.Columns.Count)
                    break;

                if (StringComparer.OrdinalIgnoreCase.Compare(frr.Columns[i].Name, record.Columns[i]) != 0)
                    record.Messages.Add(Entities.MessageType.Error, string.Format(System.Globalization.CultureInfo.InvariantCulture, "Column (position {0}) content '{1}' does not match the expected name '{2}'.", i + 1, record.Columns[i], frr.Columns[i].Name));
            }
        }

        /// <summary>
        /// Writes the header record using the column names.
        /// </summary>
        /// <param name="fileFormat">The <see cref="FileFormatBase"/>.</param>
        /// <param name="record">The <see cref="FileRecord"/>.</param>
        void IFileRecord.OnWrite(FileFormatBase fileFormat, FileRecord record)
        {
            var frr = CheckConfiguration(fileFormat, record);
            var sb = new StringBuilder();

            var cols = new List<string>();
            for (int i = 0; i < frr.Columns.Count; i++)
            {
                cols.Add(frr.Columns[i].Text);
            }

            record.Columns = cols;
            for (int i = 0; i < frr.Columns.Count; i++)
            {
                if (!fileFormat.WriteColumnToLineDataInternal(frr.Columns[i], record, i, sb))
                    return;
            }

            record.LineData = sb.ToString();
        }

        /// <summary>
        /// Check the configuration is valid.
        /// </summary>
        private FileRecordReflector CheckConfiguration(FileFormatBase fileFormat, FileRecord record)
        {
            if (fileFormat.IsHierarchical)
                throw new InvalidOperationException("The ColumnNameHeader cannot be used when the File Format is hierarchical.");

            var frr = fileFormat.GetFileRecordReflector(fileFormat.ContentRowType);
            if (record.LineNumber != 1 || fileFormat.HeaderRowType == null || fileFormat.HeaderRowType != typeof(ColumnNameHeader))
                throw new InvalidOperationException("The ColumnNameHeader can only be used for a Header row.");

            if (record.Columns.Count != frr.Columns.Count)
                record.Messages.Add(Entities.MessageType.Warning, "The number of Header columns '{0}' does not match that specified for the expected Content '{1}'.", record.Columns.Count, frr.Columns.Count);

            return frr;
        }
    }
}
