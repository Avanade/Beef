// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.FlatFile.Reflectors;
using Beef.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beef.FlatFile
{
    /// <summary>
    /// Represents a fixed-width (or fixed-length) file format.
    /// </summary>
    /// <typeparam name="TContent">The primary content <see cref="Type"/>.</typeparam>
    public class FixedFileFormat<TContent> : FileFormat<TContent> where TContent : class, new()
    {
        private static readonly object _lock = new object();
        private static readonly Dictionary<Type, int> _recordLengths = new Dictionary<Type, int>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedFileFormat{TContent}"/> class with no hierarchy.
        /// </summary>
        /// <param name="contentValidator">The content <see cref="ValidatorBase{TEntity}">validator</see>.</param>
        public FixedFileFormat(ValidatorBase<TContent> contentValidator = null)
            : base(null, contentValidator) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FixedFileFormat{TContent}"/> class with the specified hierarchy configuration.
        /// </summary>
        /// <param name="contentRecordIdentifier">The record identifier for the content row.</param>
        /// <param name="columnPosition">The <see cref="HierarchyColumnPosition"/>.</param>
        /// <param name="columnLength">The <see cref="HierarchyColumnLength"/>.</param>
        /// <param name="contentValidator">The content <see cref="ValidatorBase{TEntity}">validator</see>.</param>
        public FixedFileFormat(string contentRecordIdentifier, int columnPosition, int columnLength, ValidatorBase<TContent> contentValidator = null)
            : base(contentRecordIdentifier, contentValidator)
        {
            if (string.IsNullOrEmpty(contentRecordIdentifier))
                throw new ArgumentNullException(nameof(contentRecordIdentifier));

            if (columnPosition < 0)
                throw new ArgumentException("Hierarchy column position must be greater than or equal to zero.", nameof(columnPosition));

            if (columnLength < 1)
                throw new ArgumentException("Hierarchy column length must be greater than or equal to one.", nameof(columnLength));

            HierarchyColumnPosition = columnPosition;
            HierarchyColumnLength = columnLength;
        }

        /// <summary>
        /// Gets or sets the hierarchy column position (see <see cref="FileFormatBase.IsHierarchical"/>) to determine the <see cref="FileRecord.RecordIdentifier"/>.
        /// </summary>
        public int? HierarchyColumnPosition { get; private set; }

        /// <summary>
        /// Gets or sets the hierarchy column length (see <see cref="FileFormatBase.IsHierarchical"/>) to determine the <see cref="FileRecord.RecordIdentifier"/>.
        /// </summary>
        public int? HierarchyColumnLength { get; private set; }

        /// <summary>
        /// Gets or sets the padding character rused for a write to fill in any remaining characters to meet the column width requirement (defaults to <see cref="FileFormatBase.SpaceCharacter"/>); 
        /// </summary>
        public char PaddingChar { get; set; } = SpaceCharacter;

        /// <summary>
        /// Reads the <see cref="FileRecord"/> extracting the <see cref="FileRecord.RecordIdentifier"/>.
        /// </summary>
        /// <param name="record">The <see cref="FileRecord"/>.</param>
        /// <returns>The record identifier where applicable; otherwise, <c>null</c>.</returns>
        protected override string ReadRecordIdentifier(FileRecord record)
        {
            if (!IsHierarchical)
                return null;

            if (HierarchyColumnPosition >= record.LineData.Length)
            {
                record.Messages.Add(MessageType.Error, "Unable to determine the code identitier as the hierarchy column position is outside the bounds (length) of the record.");
                return null;
            }

            return (HierarchyColumnPosition.Value + HierarchyColumnLength.Value > record.LineData.Length)
                ? record.LineData.Substring(HierarchyColumnPosition.Value)
                : record.LineData.Substring(HierarchyColumnPosition.Value, HierarchyColumnLength.Value);
        }

        /// <summary>
        /// Reads the <see cref="FileRecord"/> creating the corresponding value (<paramref name="type"/> instance).
        /// </summary>
        /// <param name="record">The <see cref="FileRecord"/>.</param>
        /// <param name="type">The identified <see cref="Type"/> to be created.</param>
        /// <returns>The <see cref="Type"/> instance.</returns>
        protected override object ReadCreateRecordValue(FileRecord record, Type type)
        {
            var frr = GetFileRecordReflector(type);
            var val = frr.CreateInstance();
            var pos = 0;
            var cols = new List<string>();
            string col = null;

            foreach (var fcr in frr.Columns)
            {
                // When less columns in record data than expected; we update with null only. 
                if (pos >= record.LineData.Length)
                {
                    fcr.SetValue(record, CleanString(null, fcr.FileColumn), val);
                    continue;
                }

                if (fcr.FileColumn.Width <= 0)
                    throw new InvalidOperationException(string.Format("Type '{0}' has column '{1}' with no width specified; this is required for a fixed file format.", type.Name, fcr.PropertyInfo.Name));

                col = pos + fcr.FileColumn.Width > record.LineData.Length ? record.LineData.Substring(pos) : record.LineData.Substring(pos, fcr.FileColumn.Width);
                cols.Add(col);
                fcr.SetValue(record, CleanString(col, fcr.FileColumn), val);
                pos += fcr.FileColumn.Width;
            }

            // Where more data than expected add remainder as an extra column.
            if (pos < record.LineData.Length - 1)
                cols.Add(record.LineData.Substring(pos));

            record.Columns = cols.ToArray();

            // Validate that the actual and expected column counts match as per configuration.
            ValidateColumnCount(record, frr);

            return val;
        }

        /// <summary>
        /// Writes the indexed <paramref name="column"/> from the <paramref name="record"/> to the line data <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="fcr">The corresponding <see cref="FileColumnReflector"/> configuration.</param>
        /// <param name="record">The related <see cref="FileRecord"/>.</param>
        /// <param name="column">The column index.</param>
        /// <param name="sb">The line data <see cref="StringBuilder"/>.</param>
        /// <returns><c>true</c> indicates that the column write was successful; otherwise, <c>false</c>.</returns>
        protected override bool WriteColumnToLineData(FileColumnReflector fcr, FileRecord record, int column, StringBuilder sb)
        {
            // Get the string value and correct the width if needed.
            var str = column > record.Columns.Length ? null : record.Columns[column];

            if (string.IsNullOrEmpty(str))
                sb.Append(PaddingChar, fcr.FileColumn.Width);
            else
            {
                // Validate/correct the string value to ensure column width conformance.
                if (!fcr.StringWidthCorrector(record, ref str))
                    return false;

                sb.Append(str.PadRight(fcr.FileColumn.Width, PaddingChar));
            }

            return true;
        }

        /// <summary>
        /// Post-processing when writing the line data <see cref="StringBuilder"/> to modify the record identifier value where <see cref="FileFormatBase.IsHierarchical"/>.
        /// </summary>
        /// <param name="record">The related <see cref="FileRecord"/>.</param>
        /// <param name="sb">The line data <see cref="StringBuilder"/>.</param>
        protected override void WritePostProcessLineData(FileRecord record, StringBuilder sb)
        {
            base.WritePostProcessLineData(record, sb);

            // Add the Hierarchy data.
            if (HierarchyColumnPosition.HasValue)
            {
                sb.Remove(HierarchyColumnPosition.Value, HierarchyColumnLength.Value);
                sb.Insert(HierarchyColumnPosition.Value, record.RecordIdentifier.PadRight(HierarchyColumnLength.Value, PaddingChar));
            }
        }

        /// <summary>
        /// Gets the record length.
        /// </summary>
        private int GetRecordLength(Type type)
        {
            if (_recordLengths.ContainsKey(type))
                return _recordLengths[type];

            int length = 0;
            var frr = GetFileRecordReflector(type);
            foreach (var fcr in frr.Columns)
            {
                if (fcr.FileColumn.Width <= 0)
                    throw new InvalidOperationException(string.Format("Type '{0}' has column '{1}' with no width specified; this is required for a fixed file format.", type.Name, fcr.PropertyInfo.Name));

                length += fcr.FileColumn.Width;
            }

            lock (_lock)
            {
                if (_recordLengths.ContainsKey(type))
                    return _recordLengths[type];

                _recordLengths.Add(type, length);
                return length;
            }
        }
    }
}
