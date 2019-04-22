// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.FlatFile
{
    /// <summary>
    /// The <see cref="FileOperationResult"/> <see cref="EventArgs"/>.
    /// </summary>
    public class FileOperationEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the <see cref="FileOperationResult"/>.
        /// </summary>
        public FileOperationResult OperationResult { get; set; }
    }
}
