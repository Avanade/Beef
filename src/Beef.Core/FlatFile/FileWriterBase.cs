// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.FlatFile
{
    /// <summary>
    /// Provides the base file writer capabilities.
    /// </summary>
    public abstract class FileWriterBase
    {
        /// <summary>
        /// Indicates whether the end of file has been reached.
        /// </summary>
        public abstract bool IsEndOfFile { get; }

        /// <summary>
        /// Occurs when any record is written.
        /// </summary>
        public event EventHandler<FileOperationEventArgs> RecordWrite;

        /// <summary>
        /// Raises the <see cref="RecordWrite"/> event.
        /// </summary>
        /// <param name="result">The <see cref="FileOperationResult"/>.</param>
        protected void OnRecordWrite(FileOperationResult result)
        {
            RecordWrite?.Invoke(this, new FileOperationEventArgs { OperationResult = result });
        }
    }
}
