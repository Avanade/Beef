// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.FlatFile.Reflectors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Beef.FlatFile
{
    /// <summary>
    /// Represents the file writer.
    /// </summary>
    /// <typeparam name="TContent">The primary content <see cref="Type"/>.</typeparam>
    public sealed class FileWriter<TContent> : FileWriterBase where TContent : class, new()
    {
        private TextWriter _textWriter;
        private FileFormat<TContent> _fileFormat;
        private bool _isEndOfFile;
        private bool _writtenHeader = false;
        private bool _writtenContent = false;
        private bool _writtenTrailer = false;
        private long _lineNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileWriter{TContent}"/>.
        /// </summary>
        /// <param name="tw">The <see cref="TextWriter"/>.</param>
        /// <param name="ff">The <see cref="FileFormat{TContent}"/>.</param>
        public FileWriter(TextWriter tw, FileFormat<TContent> ff)
        {
            _textWriter = tw ?? throw new ArgumentNullException(nameof(tw));
            _fileFormat = ff ?? throw new ArgumentNullException(nameof(ff));
        }

        /// <summary>
        /// Indicates whether the end of file has been reached.
        /// </summary>
        public override bool IsEndOfFile
        {
            get { return _isEndOfFile; }
        }

        /// <summary>
        /// Writes the header record.
        /// </summary>
        /// <param name="header">The header record (must be same <see cref="Type"/> as the <see cref="FileFormat{TContent}"/> <see cref="FileFormatBase.HeaderRowType"/>).</param>
        /// <returns>The <see cref="FileOperationResult{T}"/>.</returns>
        public FileOperationResult<TContent> WriteHeader(object header)
        {
            if (header == null)
                throw new ArgumentNullException(nameof(header));

            if (_fileFormat.HeaderRowType == null)
                throw new InvalidOperationException("FileFormat has no corresponding HeaderRowType specified.");

            if (header.GetType() != _fileFormat.HeaderRowType)
                throw new ArgumentException("Header Type must be the same as the specified FileFormat HeaderRowType.", nameof(header));

            if (_lineNumber > 0)
                throw new InvalidOperationException("A Header can only be written as the first record.");

            CheckEndOfFile();
            CheckWriteState(true, false);
            _writtenHeader = true;

            return WriteRecords(_lineNumber,
                new FileOperationResult<TContent>(FileContentStatus.Header,
                    ComposeRecords(_fileFormat.HeaderRowType, header, _fileFormat.HeaderRecordIdentifier, false)));
        }

        /// <summary>
        /// Writes the trailer record.
        /// </summary>
        /// <param name="trailer">The trailer record (must be same <see cref="Type"/> as the <see cref="FileFormat{TContent}"/> <see cref="FileFormatBase.TrailerRowType"/>).</param>
        /// <returns>The <see cref="FileOperationResult{T}"/>.</returns>
        public FileOperationResult<TContent> WriteTrailer(object trailer)
        {
            if (trailer == null)
                throw new ArgumentNullException(nameof(trailer));

            if (_fileFormat.TrailerRowType == null)
                throw new InvalidOperationException("FileFormat has no corresponding TrailerRowType specified.");

            if (trailer.GetType() != _fileFormat.TrailerRowType)
                throw new ArgumentException("Trailer Type must be the same as the specified FileFormat TrailerRowType.", nameof(trailer));

            if (_writtenTrailer)
                throw new InvalidOperationException("A Trailer can only be written once.");

            CheckEndOfFile();
            CheckWriteState(false, true);
            _writtenTrailer = true;

            return WriteRecords(_lineNumber,
                new FileOperationResult<TContent>(FileContentStatus.Trailer,
                    ComposeRecords(_fileFormat.TrailerRowType, trailer, _fileFormat.TrailerRecordIdentifier, false)));
        }

        /// <summary>
        /// Writes a <paramref name="content"/> record.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns>The <see cref="FileOperationResult{T}"/>.</returns>
        /// <remarks>Note that the record(s) will only be written when there are no errors (<see cref="FileOperationResult"/> <see cref="FileOperationResult.HasErrors"/>).</remarks>
        public FileOperationResult<TContent> Write(TContent content)
        {
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            CheckEndOfFile();
            CheckWriteState(false, false);
            _writtenContent = true;

            return WriteRecords(_lineNumber,
                new FileOperationResult<TContent>(FileContentStatus.Content,
                    ComposeRecords(_fileFormat.ContentRowType, content, _fileFormat.ContentRecordIdentifier, true)));
        }

        /// <summary>
        /// Writes one or more <b>content</b> records.
        /// </summary>
        /// <param name="content">The content list.</param>
        /// <returns>The corresponding <see cref="FileOperationResult{T}"/> array.</returns>
        public FileOperationResult<TContent>[] Write(IEnumerable<TContent> content)
        {
            if (content == null)
                return new FileOperationResult<TContent>[] { };

            var results = new List<FileOperationResult<TContent>>();
            foreach (var c in content)
            {
                results.Add(Write(c));
            }

            return results.ToArray();
        }

        /// <summary>
        /// Signifies the end of the file.
        /// </summary>
        /// <returns>The <see cref="FileOperationResult{T}"/>.</returns>
        public FileOperationResult<TContent> EndOfFile()
        {
            CheckEndOfFile();

            if (_fileFormat.FileValidation.HasFlag(FileValidation.MustHaveRows) && _lineNumber == 0)
                throw new FileValidationException(FileValidation.MustHaveRows, "The file must contain at least one row; i.e. cannot be empty.");

            if (_lineNumber > 0 && _fileFormat.FileValidation.HasFlag(FileValidation.MustHaveAtLeastOneContentRow) && !_writtenContent)
                throw new FileValidationException(FileValidation.MustHaveAtLeastOneContentRow, "The file must contain at least one content row.");

            if (_lineNumber > 0 && _fileFormat.FileValidation.HasFlag(FileValidation.MustHaveTrailerRow) && !_writtenTrailer)
                throw new FileValidationException(FileValidation.MustHaveTrailerRow, "The file must contain a Trailer row as the last record.");

            _isEndOfFile = true;
            return new FileOperationResult<TContent>(FileContentStatus.EndOfFile, null, _lineNumber);
        }

        /// <summary>
        /// Checks whether end of file.
        /// </summary>
        private void CheckEndOfFile()
        {
            if (_isEndOfFile)
                throw new InvalidOperationException("Attempt made to write past the end of the file.");

            if (_lineNumber == 0)
            {
                if (_fileFormat.FileValidation.HasFlag(FileValidation.MustHaveHeaderRow) && _fileFormat.HeaderRowType == null)
                    throw new InvalidOperationException("FileFormat specifies FileValidation with MustHaveHeaderRow; no corresponding HeaderRowType specified.");

                if (_fileFormat.FileValidation.HasFlag(FileValidation.MustHaveTrailerRow) && _fileFormat.TrailerRowType == null)
                    throw new InvalidOperationException("FileFormat specifies FileValidation with MustHaveTrailerRow; no corresponding TrailerRowType specified.");
            }
        }

        /// <summary>
        /// Check to ensure valid state for a write.
        /// </summary>
        private void CheckWriteState(bool isHeader, bool isTrailer)
        {
            if (!isHeader && _lineNumber == 0 && _fileFormat.FileValidation.HasFlag(FileValidation.MustHaveHeaderRow) && !_writtenHeader)
                throw new FileValidationException(FileValidation.MustHaveHeaderRow, "The first record must be identified as a Header row.");

            if (_writtenTrailer)
                throw new InvalidOperationException("Attempt made to write past a Trailer row; Trailer must be the last record.");
        }

        /// <summary>
        /// Gets the next line number.
        /// </summary>
        private long GetNextLineNumber()
        {
            return ++_lineNumber;
        }

        /// <summary>
        /// Compose the <see cref="FileRecord"/> array from the <paramref name="value"/>.
        /// </summary>
        private FileRecord[] ComposeRecords(Type type, object value, string recordIdentifier, bool composeChildren)
        {
            if (value == null)
                return null;

            var records = new List<FileRecord>();
            ComposeRecord(records, type, 0, recordIdentifier, value, composeChildren);
            return records.ToArray();
        }

        /// <summary>
        /// Compose record.
        /// </summary>
        private FileRecord ComposeRecord(List<FileRecord> records, Type type, int level, string recordIdentifier, object value, bool composeChildren)
        {
            if (value == null)
                return null;

            var frf = _fileFormat.GetFileRecordReflector(type ?? value.GetType());
            var record = new FileRecord { LineNumber = GetNextLineNumber(), Value = value, Level = level, RecordIdentifier = recordIdentifier, Columns = new string[frf.Columns.Length] };
            records.Add(record);

            var col = 0;
            string str = null;

            foreach (var fcr in frf.Columns)
            {
                fcr.GetValue(record, out str);
                record.Columns[col++] = _fileFormat.CleanString(str, fcr.FileColumn);
            }

            IFileRecord ifr = value as IFileRecord;
            if (!record.HasErrors && ifr != null)
                ifr.OnWrite(_fileFormat, record);

            if (!record.HasErrors && record.LineData == null)
            {
                // Update column-by-column.
                col = 0;
                var sb = new StringBuilder();
                foreach (var fcr in frf.Columns)
                {
                    _fileFormat.WriteColumnToLineDataInternal(fcr, record, col++, sb);
                }

                // Allow a post processing opportunity.
                if (!record.HasErrors)
                {
                    _fileFormat.WritePostProcessLineDataInternal(record, sb);
                    if (!record.HasErrors)
                        record.LineData = sb.ToString();
                }
            }

            if (composeChildren && _fileFormat.IsHierarchical && frf.Children != null && frf.Children.Length > 0)
                ComposeChildren(records, frf, record);

            return record;
        }

        /// <summary>
        /// Compose the underlying children records.
        /// </summary>
        private void ComposeChildren(List<FileRecord> records, FileRecordReflector frf, FileRecord current)
        {
            foreach (var child in frf.Children)
            {
                var obj = child.GetValue(current.Value);
                if (child.IsCollection)
                {
                    foreach (var item in (System.Collections.IEnumerable)obj)
                    {
                        ComposeRecord(records, child.PropertyType, current.Level + 1, child.RecordIdentifier, item, true);
                    }
                }
                else
                    ComposeRecord(records, child.PropertyType, current.Level + 1, child.RecordIdentifier, obj, true);
            }
        }

        /// <summary>
        /// Write the record lines out where no errors.
        /// </summary>
        private FileOperationResult<TContent> WriteRecords(long lineNumber, FileOperationResult<TContent> result)
        {
            if (result != null && !result.HasErrors)
            {
                foreach (var rec in result.Records)
                {
                    _textWriter.WriteLine(rec.LineData);
                }
            }
            else
                // Reset the line number as the data was never written.
                _lineNumber = lineNumber;

            OnRecordWrite(result);

            return result;
        }
    }
}
