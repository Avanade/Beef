// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Validation;
using System;

namespace Beef.FlatFile
{
    /// <summary>
    /// Represents the file format configuration.
    /// </summary>
    /// <typeparam name="TContent">The primary content <see cref="Type"/>.</typeparam>
    public abstract class FileFormat<TContent> : FileFormatBase where TContent : class, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileFormat{TContent}"/> class.
        /// </summary>
        /// <param name="contentRecordIdentifier">The record identifier for the content row.</param>
        /// <param name="contentValidator">The content <see cref="ValidatorBase{TEntity}">validator</see>.</param>
        /// <remarks>Where a file requires a <paramref name="contentRecordIdentifier"/> it is then defined as hierarchical (see <see cref="FileFormatBase.IsHierarchical"/>).</remarks>
        public FileFormat(string contentRecordIdentifier = null, ValidatorBase<TContent> contentValidator = null)
            : base(typeof(TContent), contentRecordIdentifier, contentValidator) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileFormat{TContent}"/> class (not hierarchical) with a specified <paramref name="contentValidator"/>.
        /// </summary>
        /// <param name="contentValidator">The content <see cref="ValidatorBase{TEntity}">validator</see>.</param>
        public FileFormat(ValidatorBase<TContent> contentValidator)
            : base(typeof(TContent), contentValidator: contentValidator) { }
    }
}
