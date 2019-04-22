// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections;

namespace Beef.FlatFile
{
    /// <summary>
    /// Provides the base file reader capabilities.
    /// </summary>
    public abstract class FileReaderBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileReaderBase"/> class.
        /// </summary>
        /// <param name="stopOnError">Indicates whether to <see cref="StopOnError"/>.</param>
        protected FileReaderBase(bool stopOnError)
        {
            StopOnError = stopOnError;
        }

        /// <summary>
        /// Indicates whether the read (see <see cref="IEnumerable.GetEnumerator"/> should immediately stop once an error
        /// (see <see cref="FileOperationResult.HasErrors"/>) has been encountered. Reading can be restarted after a stop to continue.
        /// </summary>
        /// <remarks><see cref="ReadToEnd"/> resets <see cref="StopOnError"/> to <c>false</c>.</remarks>
        public bool StopOnError { get; private set; }

        /// <summary>
        /// Indicates whether the end of file has been reached.
        /// </summary>
        public abstract bool IsEndOfFile { get; }

        /// <summary>
        /// Indicates whether the current record is the last record.
        /// </summary>
        public abstract bool IsLastRecord { get; }

        /// <summary>
        /// Occurs when any record is read.
        /// </summary>
        public event EventHandler<FileOperationEventArgs> RecordRead;

        /// <summary>
        /// Raises the <see cref="RecordRead"/> event.
        /// </summary>
        /// <param name="result">The <see cref="FileOperationResult"/>.</param>
        protected void OnRecordRead(FileOperationResult result)
        {
            RecordRead?.Invoke(this, new FileOperationEventArgs { OperationResult = result });
        }

        /// <summary>
        /// Read the next record(s) group.
        /// </summary>
        /// <returns>The <see cref="FileOperationResult"/>.</returns>
        /// <exception cref="FileValidationException">Thrown when the file contents invalidate the <see cref="FileFormatBase"/> <see cref="FileFormatBase.FileValidation"/> specification.</exception>
        public abstract FileOperationResult Read();

        /// <summary>
        /// Reads the remainder of the data until the <see cref="FileContentStatus.EndOfFile"/> ensuring all records including any trailer record is read.
        /// </summary>
        /// <returns>The total number of records read (a value of -1 indicates that the end of file has already been reached).</returns>
        /// <remarks>Will not stop on error; resets <see cref="StopOnError"/> to <c>false</c>. Inheritors must invoke this base method to reset <see cref="StopOnError"/>.</remarks>
        public virtual long ReadToEnd()
        {
            StopOnError = false;
            return -1;
        }
    }
}
