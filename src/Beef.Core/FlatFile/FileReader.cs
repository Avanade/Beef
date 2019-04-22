// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.FlatFile.Internal;
using Beef.FlatFile.Reflectors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Beef.FlatFile
{
    /// <summary>
    /// Represents the file reader.
    /// </summary>
    /// <typeparam name="TContent">The primary content <see cref="Type"/>.</typeparam>
    /// <remarks>Supports <see cref="IEnumerable{TContent}"/> to simplify the reading operations and to enable <b>LINQ</b> capabilities. Iterates over
    /// the file returning only <see cref="FileContentStatus.Content"/> records that have no errors (see <see cref="FileOperationResult.HasErrors"/>). The 
    /// <see cref="FileReaderBase.OnRecordRead"/> event will be invoked for every corresponding record read regardless of whether it has errors or not.
    /// <para>The <see cref="FileReaderBase.StopOnError"/> will cause the enumerator to stop when an error is encountered.</para></remarks>
    public sealed class FileReader<TContent> : FileReaderBase, IEnumerable<FileOperationResult<TContent>> where TContent : class, new()
    {
        private TextReader _textReader;
        private FileFormat<TContent> _fileFormat;
        private FileRecordReflector _contentReflector;
        private Dictionary<string, FileRecordHierarchyItem> _hierarchy;
        private bool _hasHierarchy;
        private bool _startedReading;
        private bool _readContent = false;
        private bool _readTrailer = false;
        private FileRecord _currentRecord;
        private FileRecord _nextRecord;
        private long _lineNumber;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileReader{TContent}"/>.
        /// </summary>
        /// <param name="tr">The <see cref="TextReader"/>.</param>
        /// <param name="ff">The <see cref="FileFormat{TContent}"/>.</param>
        /// <param name="stopOnError">Indicates whether to <see cref="FileReaderBase.StopOnError"/> (defaults to <c>false</c>).</param>
        public FileReader(TextReader tr, FileFormat<TContent> ff, bool stopOnError = false) : base(stopOnError)
        {
            _textReader = tr ?? throw new ArgumentNullException(nameof(tr));
            _fileFormat = ff ?? throw new ArgumentNullException(nameof(ff));
        }

        /// <summary>
        /// Read the next record(s) group.
        /// </summary>
        /// <returns>The <see cref="FileOperationResult{T}"/>.</returns>
        /// <exception cref="FileValidationException">Thrown when the file contents invalidate the <see cref="FileFormatBase"/> <see cref="FileFormatBase.FileValidation"/> specification.</exception>
        public override FileOperationResult Read()
        {
            var result = ReadInternal();
            OnRecordRead(result);
            return result;
        }

        /// <summary>
        /// Internal read logic.
        /// </summary>
        private FileOperationResult ReadInternal()
        { 
            // Check and see if we are already at the end of the file.
            if (IsEndOfFile)
                throw new InvalidOperationException("Attempt made to read past the end of the file.");

            // Make sure the configuration is valid.
            if (!_startedReading)
                InitialConfiguration();

            // Read the next record(s) group.
            _startedReading = true;
            var records = new List<FileRecord>();
            FileRecordHierarchyLinker linker = null;

            while (true)
            {
                ReadRecord();

                // When end of file is reached, check corresponding file validations.
                if (IsEndOfFile)
                    return ProcessEndOfFile();

                // Grab the current record.
                var record = _currentRecord;

                // Check and see if the first record is a header.
                if (record.LineNumber == 1)
                {
                    var header = ProcessHeaderRecord(record);
                    if (header != null)
                        return header;
                }

                // Check and see if the last record is a trailer.
                if (_nextRecord == null)
                {
                    var trailer = ProcessTrailerRecord(record);
                    if (trailer != null)
                        return trailer;
                }

                // Where no hierarchy each record is processed as-is; or where hierarchy is in place and top-level is unknown.
                if (!_hasHierarchy || (records.Count == 0 && record.RecordIdentifier != _fileFormat.ContentRecordIdentifier))
                    return ProcessNonHierarchicalContent(record);

                // We are all hierarchy all of the time from here on in...
                if (records.Count == 0)
                {
                    record.Level = 0;
                    record.Value = _fileFormat.ReadCreateRecordValueInternal(record, _fileFormat.ContentRowType);
                    _contentReflector.Validate(record);
                    linker = new FileRecordHierarchyLinker(_hierarchy[_fileFormat.ContentRecordIdentifier], null, 0)
                    {
                        Value = record.Value
                    };
                }
                else
                    linker = ProcessHierarchicalContent(linker, records, record);

                records.Add(record);
                _readContent = true;

                // Check and see if the record is the last for the result set; if so, then exit.
                if (CheckCurrentIsLast())
                {
                    var curr = _hierarchy[linker.RecordIdentifier];
                    var root = _hierarchy[_fileFormat.ContentRecordIdentifier];
                    MoveUpHierarchy(linker, curr, root, record, true);
                    break;
                }
            }

            return new FileOperationResult<TContent>(FileContentStatus.Content, records.ToArray());
        }

        /// <summary>
        /// Read the next record; we read ahead as we need to know what the next record is.
        /// </summary>
        private void ReadRecord()
        {
            if (_nextRecord == null && _lineNumber > 0)
            {
                _currentRecord = null;
                return;
            }

            _currentRecord = _nextRecord;
            _nextRecord = CreateFileRecord(_lineNumber + 1, _textReader.ReadLine());

            if (_lineNumber == 0)
            {
                if (_nextRecord == null)
                    return;

                _lineNumber++;
                _currentRecord = _nextRecord;
                _nextRecord = CreateFileRecord(_lineNumber + 1, _textReader.ReadLine());
            }

            if (_nextRecord != null)
                _lineNumber++;
        }

        /// <summary>
        /// Creates a <see cref="FileRecord"/> from the record data.
        /// </summary>
        private FileRecord CreateFileRecord(long lineNumber, string data)
        {
            if (data == null)
                return null;

            var record = new FileRecord { LineNumber = lineNumber, LineData = data };
            record.RecordIdentifier = _fileFormat.ReadRecordIdentifierInternal(record);
            return record;
        }

        /// <summary>
        /// Perform the initial configuration.
        /// </summary>
        private void InitialConfiguration()
        {
            if (_fileFormat.FileValidation.HasFlag(FileValidation.MustHaveHeaderRow) && _fileFormat.HeaderRowType == null)
                throw new InvalidOperationException("FileFormat specifies FileValidation with MustHaveHeaderRow; no corresponding HeaderRowType specified.");

            if (_fileFormat.FileValidation.HasFlag(FileValidation.MustHaveTrailerRow) && _fileFormat.TrailerRowType == null)
                throw new InvalidOperationException("FileFormat specifies FileValidation with MustHaveTrailerRow; no corresponding TrailerRowType specified.");

            _contentReflector = _fileFormat.GetFileRecordReflector(_fileFormat.ContentRowType);
            if (_fileFormat.ContentValidator != null)
                _contentReflector.SetValidator(_fileFormat.ContentValidator);

            _hasHierarchy = _fileFormat.IsHierarchical && _contentReflector.Children.Length > 0;
            if (_hasHierarchy)
                _hierarchy = FileRecordHierarchyItem.GetHierarchy(_fileFormat);
        }

        /// <summary>
        /// Process the end of the file.
        /// </summary>
        private FileOperationResult ProcessEndOfFile()
        {
            if (_lineNumber == 0)
            {
                if (_fileFormat.FileValidation.HasFlag(FileValidation.MustHaveRows))
                    throw new FileValidationException(FileValidation.MustHaveRows, "The file must contain at least one row; i.e. cannot be empty.");
            }
            else
            {
                if (!_readContent && _fileFormat.FileValidation.HasFlag(FileValidation.MustHaveAtLeastOneContentRow))
                    throw new FileValidationException(FileValidation.MustHaveAtLeastOneContentRow, "The file must contain at least one content row.");

                if (!_readTrailer && _fileFormat.FileValidation.HasFlag(FileValidation.MustHaveTrailerRow))
                    throw new FileValidationException(FileValidation.MustHaveTrailerRow, "The file must contain a trailer row as the last record.");
            }

            return new FileOperationResult(FileContentStatus.EndOfFile, null, _lineNumber);
        }

        /// <summary>
        /// Process the Header record.
        /// </summary>
        private FileOperationResult ProcessHeaderRecord(FileRecord record)
        {
            if (_fileFormat.HeaderRowType == null)
                return null;

            if (_fileFormat.HeaderRecordIdentifier != null && record.RecordIdentifier != _fileFormat.HeaderRecordIdentifier)
            {
                if (_fileFormat.FileValidation.HasFlag(FileValidation.MustHaveHeaderRow))
                    throw new FileValidationException(FileValidation.MustHaveHeaderRow, "The first record must be identified as a header row.", record);

                // Not considered a Header row; carry on!
                return null;
            }

            record.Level = 0;
            record.Value = _fileFormat.ReadCreateRecordValueInternal(record, _fileFormat.HeaderRowType);

            if (_fileFormat.HeaderValidator != null)
            {
                var frf = _fileFormat.GetFileRecordReflector(_fileFormat.HeaderRowType);
                frf.SetValidator(_fileFormat.HeaderValidator);
                frf.Validate(record);
            }

            RecordProcess(record);

            return new FileOperationResult(FileContentStatus.Header, new FileRecord[] { record });
        }

        /// <summary>
        /// Process the Trailer record.
        /// </summary>
        private FileOperationResult ProcessTrailerRecord(FileRecord record)
        {
            if (_fileFormat.TrailerRowType == null)
                return null;

            if (_fileFormat.TrailerRecordIdentifier != null && record.RecordIdentifier != _fileFormat.TrailerRecordIdentifier)
            {
                if (_fileFormat.FileValidation.HasFlag(FileValidation.MustHaveTrailerRow))
                    throw new FileValidationException(FileValidation.MustHaveTrailerRow, "The last record must be identified as a trailer row.", record);

                // Not considered a Trailer row; carry on!
                return null;
            }

            if (!_readContent && _fileFormat.FileValidation.HasFlag(FileValidation.MustHaveAtLeastOneContentRow))
                throw new FileValidationException(FileValidation.MustHaveAtLeastOneContentRow, "The file must contain at least one content row.", record);

            record.Level = 0;
            record.Value = _fileFormat.ReadCreateRecordValueInternal(record, _fileFormat.TrailerRowType);
            _readTrailer = true;

            if (_fileFormat.TrailerValidator != null)
            {
                var frf = _fileFormat.GetFileRecordReflector(_fileFormat.TrailerRowType);
                frf.SetValidator(_fileFormat.TrailerValidator);
                frf.Validate(record);
            }

            RecordProcess(record);

            return new FileOperationResult(FileContentStatus.Trailer, new FileRecord[] { record });
        }

        /// <summary>
        /// Check if the record is a header or trailer is in the wrong position.
        /// </summary>
        private bool CheckNotHeaderOrTrailer(FileRecord record)
        {
            if (_fileFormat.HeaderRecordIdentifier != null && record.RecordIdentifier == _fileFormat.HeaderRecordIdentifier)
            {
                record.Messages.Add(MessageType.Error, "Record identified as Header; is not first record in the file.");
                return false;
            }

            if (_fileFormat.TrailerRecordIdentifier != null && record.RecordIdentifier == _fileFormat.TrailerRecordIdentifier)
            {
                record.Messages.Add(MessageType.Error, "Record identified as Trailer; is not last record in the file.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Process a non-hierarchical singular Content record.
        /// </summary>
        private FileOperationResult<TContent> ProcessNonHierarchicalContent(FileRecord record)
        {
            if (CheckNotHeaderOrTrailer(record))
            {
                if (_fileFormat.ContentRecordIdentifier != null && record.RecordIdentifier != _fileFormat.ContentRecordIdentifier)
                {
                    if (record.RecordIdentifier != null)
                        record.Messages.Add(MessageType.Error, "Record identifier is unknown.");
                }
                else
                {
                    record.Level = 0;
                    record.Value = _fileFormat.ReadCreateRecordValueInternal(record, _fileFormat.ContentRowType);
                    _contentReflector.Validate(record);
                }
            }

            RecordProcess(record);

            _readContent = true;
            return new FileOperationResult<TContent>(FileContentStatus.Content, new FileRecord[] { record });
        }

        /// <summary>
        /// Process a hierarchical Content record.
        /// </summary>
        private FileRecordHierarchyLinker ProcessHierarchicalContent(FileRecordHierarchyLinker linker, List<FileRecord> records, FileRecord record)
        {
            if (!CheckNotHeaderOrTrailer(record))
                return linker;

            // Make sure we know what the record is.
            if (!_hierarchy.ContainsKey(record.RecordIdentifier))
            {
                record.Messages.Add(MessageType.Error, "Record identifier is unknown.");
                return linker;
            }

            // Ensure that the record is valid from a hierarchy position perspective.
            var isValid = true;
            var curr = _hierarchy[record.RecordIdentifier];
            var prev = _hierarchy[records.Last(x => x.Level >= 0).RecordIdentifier];

            if (curr.Level == prev.Level)
            {
                if (prev.Parent.Children.ContainsKey(record.RecordIdentifier))
                    linker = linker.Parent;
                else
                    isValid = record.Messages.Add(MessageType.Error, "Record identifier is not a valid peer of the previous record.") == null;
            }
            else if (curr.Level < prev.Level)
            {
                if (FindUpHierachy(prev, record))
                    linker = MoveUpHierarchy(linker, prev, curr.Parent, record);
                else
                    isValid = record.Messages.Add(MessageType.Error, "Record identifier is not valid within current traversed hierarchy.") == null;
            }
            else
            {
                if (!prev.Children.ContainsKey(record.RecordIdentifier))
                    isValid = record.Messages.Add(MessageType.Error, "Record identifier is not a direct descendent of the previous record.") == null;
            }

            // Check that record is in alignment with the hierarchy attribute configuration.
            var parentValue = linker.Value;
            if (isValid)
            {
                linker = linker.AddChild(curr, record);
                if (linker.Index > 0 && !curr.HierarchyReflector.IsCollection)
                {
                    isValid = false;
                    curr.HierarchyReflector.CreateErrorMessage(record, "{0} does not support multiple records; too many provided.");
                }

                if (curr.HierarchyReflector.IsCollection && curr.HierarchyReflector.FileHierarchy.MaxCount > 0 && linker.Index >= curr.HierarchyReflector.FileHierarchy.MaxCount)
                    curr.HierarchyReflector.CreateErrorMessage(record, "{0} must not exceed {2} records(s); too many provided.", curr.HierarchyReflector.FileHierarchy.MaxCount);

                record.Level = curr.Level;
            }
            else
                record.Level = -1;

            // Parse the record to get the value and only update the linker where valid.
            record.Value = _fileFormat.ReadCreateRecordValueInternal(record, curr.HierarchyReflector.FileHierarchy.ChildType ?? curr.RecordReflector.Type);
            curr.RecordReflector.Validate(record);
            if (isValid)
                linker.Value = record.Value;

            RecordProcess(record);

            return linker;
        }

        /// <summary>
        /// Checks whether the record is valid when considered in context of the parent stack.
        /// </summary>
        private bool FindUpHierachy(FileRecordHierarchyItem item, FileRecord record)
        {
            if (item.Children.ContainsKey(record.RecordIdentifier))
                return true;

            if (item.IsRoot)
                return false;

            return FindUpHierachy(item.Parent, record);
        }

        /// <summary>
        /// Move/traverse up hierarchy for the item and confirm alignment with the hierarchy attribute configuration(s) and update the values accordingly.
        /// </summary>
        private FileRecordHierarchyLinker MoveUpHierarchy(FileRecordHierarchyLinker linker, FileRecordHierarchyItem from, FileRecordHierarchyItem to, FileRecord record, bool final = false)
        {
            if (from == null)
                return null;

            if (!final && from == to)
                return linker;

            foreach (var child in from.Children)
            {
                var fhr = child.Value.HierarchyReflector;
                var fha = child.Value.HierarchyReflector.FileHierarchy;
                var count = linker.GetChildCount(fhr.RecordIdentifier);

                // Validate alignment to configuration.
                if (fha.IsMandatory && count == 0)
                {
                    fhr.CreateErrorMessage(record, "{0} is required; no record found.");
                    continue;
                }

                // Minimum count check only honoured where at least one record found; otherwise, use IsMandatory to catch.
                if (fhr.IsCollection && count > 0 && fha.MinCount > 0 && count < fha.MinCount)
                    fhr.CreateErrorMessage(record, "{0} must have at least {2} records(s); additional required.", fha.MinCount);

                // Update the values as we move up the hierarchy.
                var vals = linker.GetChildValues(fhr.RecordIdentifier);
                fhr.SetValue(linker.Value, vals);
            }

            return MoveUpHierarchy(linker.Parent, from.Parent, to, record, final);
        }

        /// <summary>
        /// Determine if the current record is the last for the result.
        /// </summary>
        private bool CheckCurrentIsLast()
        {
            return
                _nextRecord == null 
                || _nextRecord.RecordIdentifier == _fileFormat.ContentRecordIdentifier 
                || (_fileFormat.HeaderRecordIdentifier != null && _nextRecord.RecordIdentifier == _fileFormat.HeaderRecordIdentifier)
                || (_fileFormat.TrailerRecordIdentifier != null && _nextRecord.RecordIdentifier == _fileFormat.TrailerRecordIdentifier);
        }

        /// <summary>
        /// Process the record where IFileRecord implemented.
        /// </summary>
        private void RecordProcess(FileRecord record)
        {
            if (record == null)
                return;

            if (record.Value is IFileRecord ifr)
                ifr.OnRead(_fileFormat, record);
        }

        /// <summary>
        /// Indicates whether the end of file has been reached.
        /// </summary>
        public override bool IsEndOfFile
        {
            get { return _currentRecord == null && _startedReading; }
        }

        /// <summary>
        /// Indicates whether the current record is the last record.
        /// </summary>
        public override bool IsLastRecord
        {
            get { return _startedReading && _nextRecord == null && !IsEndOfFile; }
        }

        /// <summary>
        /// Gets an enumerator that supports iteration over the file returning only <see cref="FileContentStatus.Content"/> records that have no errors.
        /// </summary>
        /// <returns>The <see cref="IEnumerator{T}"/>.</returns>
        IEnumerator<FileOperationResult<TContent>> IEnumerable<FileOperationResult<TContent>>.GetEnumerator()
        {
            if (IsEndOfFile)
                yield break;

            while (true)
            {
                var result = Read();
                switch (result.Status)
                {
                    case FileContentStatus.Content:
                        if (StopOnError && result.HasErrors)
                            yield break;

                        if (!result.HasErrors)
                            yield return (FileOperationResult<TContent>)result;

                        continue;

                    case FileContentStatus.Header:
                    case FileContentStatus.Trailer:
                        if (StopOnError && result.HasErrors)
                            yield break;

                        continue;

                    case FileContentStatus.EndOfFile:
                        yield break;
                }
            }
        }

        /// <summary>
        /// Supports an iteration over the file returning only <b>content</b> records that have no errors (<see cref="FileOperationResult.HasErrors"/> is <c>false</c>). 
        /// The <see cref="FileReaderBase.RecordRead"/> event will be raised for every corresponding record found regardless of whether it has errors or not.
        /// </summary>
        /// <returns>The <see cref="IEnumerator{T}"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<FileOperationResult<TContent>>)this).GetEnumerator();
        }

        /// <summary>
        /// Reads the remainder of the data until the <see cref="FileContentStatus.EndOfFile"/> ensuring all records including any trailer record is read.
        /// </summary>
        /// <returns>The total number of records read (a value of -1 indicates that the end of file has already been reached).</returns>
        /// <remarks>Will not stop on error; resets <see cref="FileReaderBase.StopOnError"/> to <c>false</c>.</remarks>
        /// <exception cref="FileValidationException">Thrown when the file contents invalidate the <see cref="FileFormatBase"/> <see cref="FileFormatBase.FileValidation"/> specification.</exception>
        public override long ReadToEnd()
        {
            var start = _lineNumber;
            base.ReadToEnd();
            if (IsEndOfFile)
                return -1;

            foreach (var val in this)
                continue;

            return _lineNumber - start;
        }
    }
}
