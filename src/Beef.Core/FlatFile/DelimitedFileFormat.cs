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
    /// Represents a delimited file format.
    /// </summary>
    /// <typeparam name="TContent">The primary content <see cref="Type"/>.</typeparam>
    public class DelimitedFileFormat<TContent> : FileFormat<TContent> where TContent : class, new()
    {
        private const string QualifierInsideQualifiedText = "Text qualifier character found (position {0}) inside qualified text without being escaped correctly (e.g. double qualifier '\"\"').";
        private const string QualifierInsideUnqualifiedText = "Text qualifier character found (position {0}) inside unqualified text; text must be qualified and escaped correctly (e.g. double qualifier '\"\"').";
        private const string QualifierNotClosedText = "Text qualifier character missing to close the text qualification for the final column.";

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedFileFormat{TContent}"/> class with no hierarchy.
        /// </summary>
        /// <param name="delimiter">The <see cref="Delimiter"/>.</param>
        /// <param name="textQualifier">The <see cref="TextQualifier"/>.</param>
        /// <param name="contentValidator">The content <see cref="ValidatorBase{TEntity}">validator</see>.</param>
        public DelimitedFileFormat(char delimiter = CommaCharater, char textQualifier = DoubleQuoteCharacter, ValidatorBase<TContent> contentValidator = null)
            : base(null, contentValidator)
        {
            SetDelimiters(delimiter, textQualifier);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelimitedFileFormat{TContent}"/> class with the specified hierarchy configuration.
        /// </summary>
        /// <param name="contentRecordIdentifier">The record identifier for the content row.</param>
        /// <param name="hierarchyColumnIndex">The <see cref="HierarchyColumnIndex"/> (defaults to zero; i.e. the first column).</param>
        /// <param name="delimiter">The <see cref="Delimiter"/>.</param>
        /// <param name="textQualifier">The <see cref="TextQualifier"/>.</param>
        /// <param name="contentValidator">The content <see cref="ValidatorBase{TEntity}">validator</see>.</param>
        public DelimitedFileFormat(string contentRecordIdentifier, int hierarchyColumnIndex = 0, char delimiter = CommaCharater, char textQualifier = DoubleQuoteCharacter, ValidatorBase<TContent> contentValidator = null)
            : base(contentRecordIdentifier, contentValidator)
        {
            if (string.IsNullOrEmpty(contentRecordIdentifier))
                throw new ArgumentNullException(nameof(contentRecordIdentifier));

            if (hierarchyColumnIndex < 0)
                throw new ArgumentException("Hierarchy column index must be zero or greater.");

            HierarchyColumnIndex = hierarchyColumnIndex;
            SetDelimiters(delimiter, textQualifier);
        }

        /// <summary>
        /// Validate and set the delimiters.
        /// </summary>
        private void SetDelimiters(char delimiter, char textQualifier)
        {
            if (delimiter == NoCharacter)
                throw new ArgumentException("The delimiter can not be set to FileFormatBase.NoCharacter as a valid value is required.", nameof(delimiter));

            if (delimiter == textQualifier)
                throw new ArgumentException("The delimiter and the text qualifier characters must not be the same.", nameof(textQualifier));

            Delimiter = delimiter;
            TextQualifier = textQualifier;
        }

        /// <summary>
        /// Gets the hierarchy column index to determine the <see cref="FileRecord.RecordIdentifier"/> (see constructor).
        /// </summary>
        public int? HierarchyColumnIndex { get; private set; }

        /// <summary>
        /// Gets or sets the delimiter character; defaults to the <see cref="FileFormatBase.CommaCharater"/>.
        /// </summary>
        public char Delimiter { get; private set; } = CommaCharater;

        /// <summary>
        /// Gets or sets the text qualifier character; defaults to the <see cref="FileFormatBase.DoubleQuoteCharacter"/> (<see cref="FileFormatBase.NoCharacter"/> indicates
        /// that no text qualifier is used).
        /// </summary>
        public char TextQualifier { get; private set; } = DoubleQuoteCharacter;

        /// <summary>
        /// Indicates that when performing a <b>write</b> operation that the text will only be qualified (see <see cref="TextQualifier"/>) where it contains the
        /// <see cref="Delimiter"/> character within (defaults to <c>false</c>).
        /// </summary>
        public bool TextQualifierOnlyWithDelimiterOnWrite { get; set; } = false;

        /// <summary>
        /// Gets or sets the <see cref="TextQualifierHandling"/>; defaults to <see cref="TextQualifierHandling.Strict"/>.
        /// </summary>
        public TextQualifierHandling TextQualifierHandling { get; set; } = TextQualifierHandling.Strict;

        /// <summary>
        /// Reads the <see cref="FileRecord"/> extracting the <see cref="FileRecord.RecordIdentifier"/> and <see cref="FileRecord.Columns"/>.
        /// </summary>
        /// <param name="record">The <see cref="FileRecord"/>.</param>
        /// <returns>The record identifier where applicable; otherwise, <c>null</c>.</returns>
        protected override string ReadRecordIdentifier(FileRecord record)
        {
            char c = Char.MinValue;
            var sb = new StringBuilder(256);
            var cols = new List<string>();
            bool isQualified = false;

            // Iterate and split the record data into multiple columns.
            for (int i = 0; i < Check.NotNull(record, nameof(record)).LineData.Length; i++)
            {
                c = record.LineData[i];
                if (c == Delimiter)
                {
                    // Where not qualified it is the end of the column.
                    if (!isQualified)
                    {
                        cols.Add(sb.ToString());
                        sb.Clear();
                        continue;
                    }
                }

                if (c == TextQualifier)
                {
                    if (isQualified)
                    {
                        // Where end of record data, this is terminating the column correctly. 
                        if (i >= record.LineData.Length - 1)
                        {
                            isQualified = false;
                            break;
                        }

                        // Where next character is delimiter, then terminating column correctly.
                        if (record.LineData[i + 1] == Delimiter)
                        {
                            isQualified = false;
                            continue;
                        }

                        // Where next charater is same, then it is escaped as single char.
                        if (record.LineData[i + 1] == TextQualifier)
                        {
                            sb.Append(TextQualifier);
                            i++;
                            continue;
                        }

                        // Handle the text qualifier issue.
                        switch (TextQualifierHandling)
                        {
                            case TextQualifierHandling.LooseAllow:
                                record.Messages.Add(MessageType.Warning, QualifierInsideQualifiedText, i + 1);
                                break;

                            case TextQualifierHandling.LooseSkip:
                                record.Messages.Add(MessageType.Warning, QualifierInsideQualifiedText, i + 1);
                                continue;

                            default:
                                record.Messages.Add(MessageType.Error, QualifierInsideQualifiedText, i + 1);
                                return null;
                        }
                    }
                    else
                    {
                        if (sb.Length == 0)
                        {
                            isQualified = true;
                            continue;
                        }

                        if (i < record.LineData.Length - 1 && record.LineData[i + 1] == TextQualifier)
                        {
                            sb.Append(TextQualifier);
                            i++;
                            continue;
                        }

                        // Handle the text qualifier issue.
                        switch (TextQualifierHandling)
                        {
                            case TextQualifierHandling.LooseAllow:
                                record.Messages.Add(MessageType.Warning, QualifierInsideUnqualifiedText, i + 1);
                                break;

                            case TextQualifierHandling.LooseSkip:
                                record.Messages.Add(MessageType.Warning, QualifierInsideUnqualifiedText, i + 1);
                                continue;

                            default:
                                record.Messages.Add(MessageType.Error, QualifierInsideUnqualifiedText, i + 1);
                                return null;
                        }
                    }
                }

                sb.Append(c);
            }

            // Finish up the column processing.
            if (isQualified)
            {
                switch (TextQualifierHandling)
                {
                    case TextQualifierHandling.LooseAllow:
                    case TextQualifierHandling.LooseSkip:
                        record.Messages.Add(MessageType.Warning, QualifierNotClosedText);
                        break;

                    default:
                        record.Messages.Add(MessageType.Error, QualifierNotClosedText);
                        return null;
                }
            }

            if (sb.Length > 0)
                cols.Add(sb.ToString());

            record.Columns = cols;

            // Determine the record identifier.
            if (IsHierarchical)
            {
                if (HierarchyColumnIndex >= record.Columns.Count)
                    record.Messages.Add(MessageType.Error, "Unable to determine the code identitier as the hierarchy column index is outside the bounds of the number of columns found for the record.");
                else
                    return record.Columns[HierarchyColumnIndex.Value];
            }

            return null;
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
            var col = 0;

            if (Check.NotNull(record, nameof(record)).Columns == null)
                return val;

            foreach (var fcr in frr.Columns)
            {
                // When less columns in record data than expected; we update with null. 
                if (col >= record.Columns.Count)
                    fcr.SetValue(record, CleanString(null, fcr.FileColumn), val);
                else
                    // Set the value.
                    fcr.SetValue(record, CleanString(record.Columns[col], fcr.FileColumn), val);

                col++;
            }

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
            // Delimit each column.
            Check.NotNull(sb, nameof(sb));
            if (column > 0)
                sb.Append(Delimiter);

            // Get the string value and correct the width if needed.
            var str = column > Check.NotNull(record, nameof(record)).Columns.Count ? null : record.Columns[column];

            // Override if it is the hierarchy column.
            if (HierarchyColumnIndex.HasValue && HierarchyColumnIndex.Value == column)
                str = record.RecordIdentifier;

            // Where null this means there is nothing specific to write.
            if (string.IsNullOrEmpty(str))
                return true;

            // Validate/correct the string value to ensure column width conformance.
            if (!Check.NotNull(fcr, nameof(fcr)).StringWidthCorrector(record, ref str))
                return false;

            // Check if the column content contains the delimiter and handle accordingly.
            var qualify = fcr.PropertyTypeCode == TypeCode.String && TextQualifier != NoCharacter && !TextQualifierOnlyWithDelimiterOnWrite;
            if (str.IndexOf(Delimiter) >= 0)
            {
                if (TextQualifier == NoCharacter)
                {
                    record.Messages.Add(MessageType.Error, "Text delimiter character found inside column text; no text qualifier has been specified and would result in errant record.");
                    return false;
                }
                else
                    qualify = true;
            }

            // Double qualify a qualifier inside of the text.
            if (TextQualifier != NoCharacter)
            {
                if (!qualify && str.IndexOf(TextQualifier) >= 0)
                    qualify = true;

                if (qualify)
                    str = str.Replace(TextQualifier.ToString(System.Globalization.CultureInfo.InvariantCulture), new string(TextQualifier, 2));
            }

            if (qualify)
                sb.Append(TextQualifier);

            sb.Append(str);

            if (qualify)
                sb.Append(TextQualifier);

            return true;
        }
    }
}
