// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.FlatFile.Converters;
using Beef.FlatFile.Reflectors;
using Beef.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beef.FlatFile
{
    /// <summary>
    /// Represents the base file format configuration.
    /// </summary>
    public abstract class FileFormatBase
    {
        /// <summary>
        /// Defines the comma character ','.
        /// </summary>
        public const char CommaCharater = ',';

        /// <summary>
        /// Defines the double-quote character '"'.
        /// </summary>
        public const char DoubleQuoteCharacter = '"';

        /// <summary>
        /// Defines the space character ' '.
        /// </summary>
        public const char SpaceCharacter = '\u0020';

        /// <summary>
        /// Defines the tab character.
        /// </summary>
        public const char TabCharacter = '\t';

        /// <summary>
        /// Defines <b>no</b> character.
        /// </summary>
        public const char NoCharacter = char.MinValue;

        private readonly object _lock = new object();
        private readonly Dictionary<Type, FileRecordReflector> _cache = new Dictionary<Type, FileRecordReflector>();

        /// <summary>
        /// Initializes a new instance of the <see cref="FileFormat{TContent}"/> class.
        /// </summary>
        /// <param name="contentRowType">The <see cref="Type"/> for the content row.</param>
        /// <param name="contentRecordIdentifier">The record identifier for the content row.</param>
        /// <param name="contentValidator">The content <see cref="ValidatorBase{TEntity}">validator</see>.</param>
        /// <remarks>Where a file requires a <paramref name="contentRecordIdentifier"/> it is then marked as hierarchical (see <see cref="IsHierarchical"/>).</remarks>
        internal FileFormatBase(Type contentRowType, string contentRecordIdentifier = null, object contentValidator = null)
        {
            ContentRowType = contentRowType ?? throw new ArgumentNullException(nameof(contentRowType));
            if (!string.IsNullOrEmpty(contentRecordIdentifier))
            {
                IsHierarchical = true;
                ContentRecordIdentifier = contentRecordIdentifier;
            }

            ContentValidator = contentValidator;
        }

        /// <summary>
        /// Gets or sets the <see cref="FileValidation"/> that must be performed (set required flags).
        /// </summary>
        public FileValidation FileValidation { get; set; }

        /// <summary>
        /// Indicates that the data supports a hierarchical record structure.
        /// </summary>
        public bool IsHierarchical { get; private set; }

        /// <summary>
        /// Gets the <see cref="Type"/> for the content row(s).
        /// </summary>
        public Type ContentRowType { get; private set; }

        /// <summary>
        /// Gets the record identifier for the content row(s).
        /// </summary>
        public string ContentRecordIdentifier { get; private set; }

        /// <summary>
        /// Gets the <see cref="ValidatorBase{TEntity}">validator</see> for the content.
        /// </summary>
        /// <remarks>Note that this will only result in a shallow validation (see <see cref="ValidationArgs.ShallowValidation"/>); use the
        /// <see cref="FileHierarchyAttribute"/> <see cref="FileHierarchyAttribute.ValidationType"/> to enable sub-entity (deep) validations.</remarks>
        public object ContentValidator { get; private set; }

        /// <summary>
        /// Gets the <see cref="Type"/> for the header (first) row (see <see cref="SetHeaderRowType{THeader}(string, ValidatorBase{THeader})"/>).
        /// </summary>
        public Type HeaderRowType { get; private set; }

        /// <summary>
        /// Gets the record identifier for the header (first) row (see <see cref="SetHeaderRowType{THeader}(string, ValidatorBase{THeader})"/>).
        /// </summary>
        public string HeaderRecordIdentifier { get; private set; }

        /// <summary>
        /// Gets the <see cref="ValidatorBase{TEntity}">validator</see> for the header (first) row (see  see cref="SetHeaderRowType{THeader}(string, ValidatorBase{THeader})"/>).
        /// </summary>
        public object HeaderValidator { get; private set; }

        /// <summary>
        /// Gets the <see cref="Type"/> for the trailer (last) row (see <see cref="SetTrailerRowType{TTrailer}(string, ValidatorBase{TTrailer})"/>).
        /// </summary>
        public Type TrailerRowType { get; private set; }

        /// <summary>
        /// Gets the record identifier for the header (first) row (see <see cref="SetTrailerRowType{TTrailer}(string, ValidatorBase{TTrailer})"/>).
        /// </summary>
        public string TrailerRecordIdentifier { get; private set; }

        /// <summary>
        /// Gets the <see cref="ValidatorBase{TEntity}">validator</see> for the header (first) row (see  see cref="SetHeaderRowType{THeader}(string, ValidatorBase{THeader})"/>).
        /// </summary>
        public object TrailerValidator { get; private set; }

        /// <summary>
        /// Sets the <see cref="HeaderRowType"/> (and optionally the <see cref="HeaderRecordIdentifier"/>).
        /// </summary>
        /// <typeparam name="THeader">The header <see cref="Type"/>.</typeparam>
        /// <param name="recordIdentifier">The record identifier for the header row (<c>null</c> indicates that there is no specific identifier).</param>
        /// <param name="headerValidator">The header validator.</param>
        public void SetHeaderRowType<THeader>(string recordIdentifier = null, ValidatorBase<THeader> headerValidator = null) where THeader : class, new()
        {
            if (HeaderRowType != null)
                throw new InvalidOperationException("The HeaderRowType cannot be set more than once.");

            var type = typeof(THeader);
            if (type == ContentRowType)
                throw new ArgumentException("The HeaderRowType cannot be the same as the ContentRowType.");

            if (type == TrailerRowType)
                throw new ArgumentException("The HeaderRowType cannot be the same as the TrailerRowType.");

            if (string.IsNullOrEmpty(recordIdentifier))
            {
                if (IsHierarchical)
                    throw new ArgumentNullException(nameof(recordIdentifier), "The record identifier is required where the file is considered hierarchical.");
            }
            else
            {
                if (!IsHierarchical)
                    throw new ArgumentException("The record identifier can not be specified where the file is not considered hierarchical.", nameof(recordIdentifier));

                if (ContentRecordIdentifier != null && recordIdentifier == ContentRecordIdentifier)
                    throw new ArgumentException("The HeaderRecordIdentifier cannot be the same as the ContentRecordIdentifier.", nameof(recordIdentifier));

                if (TrailerRecordIdentifier != null && recordIdentifier == TrailerRecordIdentifier)
                    throw new ArgumentException("The HeaderRecordIdentifier cannot be the same as the TrailerRecordIdentifier.", nameof(recordIdentifier));
            }

            HeaderRowType = type;
            HeaderRecordIdentifier = string.IsNullOrEmpty(recordIdentifier) ? null : recordIdentifier;
            HeaderValidator = headerValidator;
        }

        /// <summary>
        /// Sets the <see cref="TrailerRowType"/> (and optionally the <see cref="TrailerRecordIdentifier"/>).
        /// </summary>
        /// <typeparam name="TTrailer">The trailer <see cref="Type"/>.</typeparam>
        /// <param name="recordIdentifier">The record identifier for the trailer row (<c>null</c> indicates that there is no specific identifier).</param>
        /// <param name="trailerValidator">The trailer validator.</param>
        public void SetTrailerRowType<TTrailer>(string recordIdentifier = null, ValidatorBase<TTrailer> trailerValidator = null) where TTrailer : class, new()
        {
            if (TrailerRowType != null)
                throw new InvalidOperationException("The TrailerRowType cannot be set more than once.");

            TrailerRowType = typeof(TTrailer);
            var type = typeof(TTrailer);
            if (type == ContentRowType)
                throw new InvalidOperationException("The TrailerRowType cannot be the same as the ContentRowType.");

            if (type == HeaderRowType)
                throw new InvalidOperationException("The TrailerRowType cannot be the same as the HeaderRowType.");

            if (string.IsNullOrEmpty(recordIdentifier))
            {
                if (IsHierarchical)
                    throw new ArgumentNullException(nameof(recordIdentifier), "The record identifier is required where the file is considered hierarchical.");
            }
            else
            {
                if (!IsHierarchical)
                    throw new ArgumentException("The record identifier can not be specified where the file is not considered hierarchical.", nameof(recordIdentifier));

                if (ContentRecordIdentifier != null && recordIdentifier == ContentRecordIdentifier)
                    throw new ArgumentException("The TrailerRecordIdentifier cannot be the same as the ContentRecordIdentifier.", nameof(recordIdentifier));

                if (HeaderRecordIdentifier != null && recordIdentifier == HeaderRecordIdentifier)
                    throw new ArgumentException("The TrailerRecordIdentifier cannot be the same as the HeaderRecordIdentifier.", nameof(recordIdentifier));
            }

            TrailerRowType = type;
            TrailerRecordIdentifier = string.IsNullOrEmpty(recordIdentifier) ? null : recordIdentifier;
            TrailerValidator = trailerValidator;
        }

        /// <summary>
        /// Gets or sets the <see cref="ColumnWidthOverflow"/> (defaults to <see cref="ColumnWidthOverflow.Error"/>) when reading/writing to/from file.
        /// </summary>
        public ColumnWidthOverflow WidthOverflow { get; set; } = ColumnWidthOverflow.Error;

        /// <summary>
        /// Gets or sets the <see cref="StringTransform"/> (defaults to <see cref="StringTransform.EmptyToNull"/>) when reading/writing to/from file.
        /// </summary>
        public StringTransform StringTransform { get; set; } = StringTransform.EmptyToNull;

        /// <summary>
        /// Gets or sets the <see cref="StringTrim"/> (defaults to <see cref="StringTrim.End"/>) when reading/writing to/from file.
        /// </summary>
        public StringTrim StringTrim { get; set; } = StringTrim.End;

        /// <summary>
        /// Gets the <see cref="ITextValueConverter{T}"/> collection.
        /// </summary>
        public TextValueConverters Converters { get; } = new TextValueConverters();

        /// <summary>
        /// Gets or sets the <see cref="FlatFile.ColumnCountValidation"/> (defaults to <see cref="ColumnCountValidation.None"/>).
        /// </summary>
        public ColumnCountValidation ColumnCountValidation { get; set; } = ColumnCountValidation.None;

        /// <summary>
        /// Reads the <see cref="FileRecord"/> extracting the <see cref="FileRecord.RecordIdentifier"/>.
        /// </summary>
        /// <param name="record">The <see cref="FileRecord"/>.</param>
        /// <returns>The record identifier where applicable; otherwise, <c>null</c>.</returns>
        internal string ReadRecordIdentifierInternal(FileRecord record)
        {
            return ReadRecordIdentifier(record);
        }

        /// <summary>
        /// Reads the <see cref="FileRecord"/> extracting the <see cref="FileRecord.RecordIdentifier"/>.
        /// </summary>
        /// <param name="record">The <see cref="FileRecord"/>.</param>
        /// <returns>The record identifier where applicable; otherwise, <c>null</c>.</returns>
        protected abstract string ReadRecordIdentifier(FileRecord record);

        /// <summary>
        /// Read the <see cref="FileRecord"/> creating the corresponding value (<paramref name="type"/> instance).
        /// </summary>
        /// <param name="record">The <see cref="FileRecord"/>.</param>
        /// <param name="type">The identified <see cref="Type"/> to be created.</param>
        /// <returns>The <see cref="Type"/> instance.</returns>
        internal object ReadCreateRecordValueInternal(FileRecord record, Type type)
        {
            return ReadCreateRecordValue(record, type);
        }

        /// <summary>
        /// Read the <see cref="FileRecord"/> creating the corresponding value (<paramref name="type"/> instance).
        /// </summary>
        /// <param name="record">The <see cref="FileRecord"/>.</param>
        /// <param name="type">The identified <see cref="Type"/> to be created.</param>
        /// <returns>The <see cref="Type"/> instance.</returns>
        protected abstract object ReadCreateRecordValue(FileRecord record, Type type);

        /// <summary>
        /// Cleans a <see cref="string"/> by applying the configured <see cref="StringTransform"/> and <see cref="StringTrim"/>.
        /// </summary>
        /// <param name="str">The string to clean.</param>
        /// <param name="fca">The <see cref="FileColumnAttribute"/>.</param>
        /// <returns>The cleaned string.</returns>
        public string CleanString(string str, FileColumnAttribute fca)
        {
            if (fca == null)
                throw new ArgumentNullException(nameof(fca));

            return Cleaner.Clean(str, fca.HasStringTrimBeenSet ? fca.StringTrim : StringTrim, fca.HasStringTransformBeenSet ? fca.StringTransform : StringTransform);
        }

        /// <summary>
        /// Writes the indexed <paramref name="column"/> from the <paramref name="record"/> to the line data <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="fcr">The corresponding <see cref="FileColumnReflector"/> configuration.</param>
        /// <param name="record">The related <see cref="FileRecord"/>.</param>
        /// <param name="column">The column index.</param>
        /// <param name="sb">The line data <see cref="StringBuilder"/>.</param>
        /// <returns><c>true</c> indicates that the column write was successful; otherwise, <c>false</c>.</returns>
        /// <remarks>Implementers must use <see cref="FileColumnReflector"/> <see cref="FileColumnReflector.StringWidthCorrector(FileRecord, ref string)"/> to 
        /// validate and correct the column string prior to updating the line data to ensure that the configured file and column formatting rules are adhered to.</remarks>
        internal bool WriteColumnToLineDataInternal(FileColumnReflector fcr, FileRecord record, int column, StringBuilder sb)
        {
            return WriteColumnToLineData(fcr, record, column, sb);
        }

        /// <summary>
        /// Writes the indexed <paramref name="column"/> from the <paramref name="record"/> to the line data <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="fcr">The corresponding <see cref="FileColumnReflector"/> configuration.</param>
        /// <param name="record">The related <see cref="FileRecord"/>.</param>
        /// <param name="column">The column index.</param>
        /// <param name="sb">The line data <see cref="StringBuilder"/>.</param>
        /// <returns><c>true</c> indicates that the column write was successful; otherwise, <c>false</c>.</returns>
        /// <remarks>Implementers must use <see cref="FileColumnReflector"/> <see cref="FileColumnReflector.StringWidthCorrector(FileRecord, ref string)"/> to 
        /// validate and correct the column string prior to updating the line data to ensure that the configured file and column formatting rules are adhered to.</remarks>
        protected abstract bool WriteColumnToLineData(FileColumnReflector fcr, FileRecord record, int column, StringBuilder sb);

        /// <summary>
        /// Enables post-processing when writing the line data <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="record">The related <see cref="FileRecord"/>.</param>
        /// <param name="sb">The line data <see cref="StringBuilder"/>.</param>
        internal void WritePostProcessLineDataInternal(FileRecord record, StringBuilder sb)
        {
            WritePostProcessLineData(record, sb);
        }

        /// <summary>
        /// Enables post-processing when writing the line data <see cref="StringBuilder"/>.
        /// </summary>
        /// <param name="record">The related <see cref="FileRecord"/>.</param>
        /// <param name="sb">The line data <see cref="StringBuilder"/>.</param>
        protected virtual void WritePostProcessLineData(FileRecord record, StringBuilder sb) { }

        /// <summary>
        /// Get the <see cref="FileRecordReflector"/> for the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The <see cref="Type"/>.</param>
        /// <returns>The <see cref="FileRecordReflector"/>.</returns>
        public FileRecordReflector GetFileRecordReflector(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (_cache.ContainsKey(type))
                return _cache[type];

            var frf = new FileRecordReflector(type, this);
            lock (_lock)
            {
                if (_cache.ContainsKey(type))
                    return _cache[type];

                _cache.Add(type, frf);
                return frf;
            }
        }

        private const string LessColumnsThanExpectedFormat = "There are less columns '{0}' within the record than expected '{1}'.";
        private const string GreaterColumnsThanExpectedFormat = "There are more columns '{0}' within the record than expected '{1}'.";

        /// <summary>
        /// Validates the record column count against the configuration (see <see cref="FileRecordReflector"/>).
        /// </summary>
        /// <param name="record">The <see cref="FileRecord"/>.</param>
        /// <param name="frr">The <see cref="FileRecordReflector"/>.</param>
        protected void ValidateColumnCount(FileRecord record, FileRecordReflector frr)
        {
            if (ColumnCountValidation == ColumnCountValidation.None || Check.NotNull(record, nameof(record)).Columns.Count == Check.NotNull(frr, nameof(frr)).Columns.Count)
                return;

            if (record.Columns.Count < frr.Columns.Count)
            {
                switch (ColumnCountValidation)
                {
                    case ColumnCountValidation.LessThanError:
                    case ColumnCountValidation.LessAndGreaterThanError:
                        record.Messages.Add(MessageType.Error, LessColumnsThanExpectedFormat, record.Columns.Count, frr.Columns.Count);
                        break;

                    case ColumnCountValidation.LessThanWarning:
                    case ColumnCountValidation.LessAndGreaterThanWarning:
                        record.Messages.Add(MessageType.Warning, LessColumnsThanExpectedFormat, record.Columns.Count, frr.Columns.Count);
                        break;
                }
            }
            else
            {
                switch (ColumnCountValidation)
                {
                    case ColumnCountValidation.GreaterThanError:
                    case ColumnCountValidation.LessAndGreaterThanError:
                        record.Messages.Add(MessageType.Error, GreaterColumnsThanExpectedFormat, record.Columns.Count, frr.Columns.Count);
                        break;

                    case ColumnCountValidation.GreaterThanWarning:
                    case ColumnCountValidation.LessAndGreaterThanWarning:
                        record.Messages.Add(MessageType.Warning, GreaterColumnsThanExpectedFormat, record.Columns.Count, frr.Columns.Count);
                        break;
                }
            }
        }
    }
}