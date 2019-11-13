﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.FlatFile
{
#pragma warning disable CA1032 // Implement standard exception constructors; only the specified constructor is required.
    /// <summary>
    /// Represents a <see cref="FileValidation"/> <see cref="Exception"/>.
    /// </summary>
    public class FileValidationException : Exception
#pragma warning restore CA1032
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileValidationException"/> class.
        /// </summary>
        /// <param name="fileValidation">The <see cref="FileValidation"/> rule that caused the exception.</param>
        /// <param name="message">The message.</param>
        /// <param name="record">The <see cref="FileRecord"/> where applicable.</param>
        internal FileValidationException(FileValidation fileValidation, string message, FileRecord record = null) : base(message)
        {
            FileValidation = fileValidation;
            Record = record;
        }

        /// <summary>
        /// Gets the single <see cref="Beef.FlatFile.FileValidation"/> rule that caused the exception.
        /// </summary>
        public FileValidation FileValidation { get; private set; }

        /// <summary>
        /// Gets the corresponding <see cref="FileRecord"/>.
        /// </summary>
        public FileRecord Record { get; private set; }
    }
}
