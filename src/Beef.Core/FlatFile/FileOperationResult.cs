// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Beef.FlatFile
{
    /// <summary>
    /// Represents the result for a file operation; where a single read/write contains the full hierarchical record(s) group.
    /// </summary>
    [DebuggerDisplay("Status = {Status}, HasErrors = {HasErrors}, Records = {Records.Count}")]
    public class FileOperationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileOperationResult"/> class.
        /// </summary>
        /// <param name="status">The <see cref="FileContentStatus"/>.</param>
        /// <param name="records">The resulting <see cref="FileRecord"/> list.</param>
        /// <param name="totalLines">The total number of lines read/written (where less than zero will derive from <paramref name="records"/>).</param>
        public FileOperationResult(FileContentStatus status, List<FileRecord> records, long totalLines = -1)
        {
            Status = status;
            Records = records;
            HasErrors = Records == null ? false : Records.Any(x => x.HasErrors);

            if (totalLines < 0)
            {
                if (records == null || records.Count == 0)
                    throw new ArgumentException("Total lines must be specified where there are no corresponding records.", nameof(totalLines));

                TotalLines = records.Last().LineNumber;
            }
            else
                TotalLines = totalLines;
        }

        /// <summary>
        /// Indicates the <see cref="FileContentStatus"/>.
        /// </summary>
        public FileContentStatus Status { get; private set; }

        /// <summary>
        /// Gets the <see cref="FileRecord"/> list that represents the hierarchical record(s) group.
        /// </summary>
        public List<FileRecord> Records { get; private set; }

        /// <summary>
        /// Indicates whether the result has errors (that there were one or more errors encountered during the <b>read</b>, or preparing for the <b>write</b>,
        /// of the <see cref="Records"/>);
        /// </summary>
        public bool HasErrors { get; private set; }

        /// <summary>
        /// Gets the line number for the first record (see <see cref="Records"/>).
        /// </summary>
        public long LineNumber => Records == null || !Records.Any() ? -1 : Records.First().LineNumber;

        /// <summary>
        /// Gets the total number of lines read/written.
        /// </summary>
        public long TotalLines { get; private set; }

        /// <summary>
        /// Gets the primary deserialized value (references the first <see cref="FileRecord"/> <see cref="FileRecord.Value"/>) where no errors (see <see cref="HasErrors"/>);
        /// otherwise, <c>null</c>.
        /// </summary>
        public object Value
        {
            get { return HasErrors || Records == null || !Records.Any() ? null : Records.First().Value; }
        }
    }

    /// <summary>
    /// Represents the result for a file operation with a typed <see cref="Value"/>.
    /// </summary>
    /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
    public class FileOperationResult<T> : FileOperationResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileOperationResult{T}"/> class.
        /// </summary>
        /// <param name="status">The <see cref="FileContentStatus"/>.</param>
        /// <param name="records">The resulting <see cref="FileRecord"/> list.</param>
        /// <param name="totalLines">The total number of lines read/written (where less than zero will derive from <paramref name="records"/>).</param>
        public FileOperationResult(FileContentStatus status, List<FileRecord> records, long totalLines = -1)
            : base(status, records, totalLines) { }

        /// <summary>
        /// Gets the typed value (see <see cref="FileOperationResult.Value"/>.
        /// </summary>
        public new T Value => (T)base.Value;
    }
}
