// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.CodeGen
{
    /// <summary>
    /// The <see cref="CodeGeneratorEventArgs"/> event arguments.
    /// </summary>
    public class CodeGeneratorEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the optional directory name.
        /// </summary>
        public string? OutputDirName { get; set; }

        /// <summary>
        /// Gets or sets the generated file name.
        /// </summary>
        public string? OutputFileName { get; set; }

        /// <summary>
        /// Indicates whether the file is only generated once; i.e. only created where it does not already exist.
        /// </summary>
        public bool GenOnce { get; set; }

        /// <summary>
        /// Gets or sets the generated output content.
        /// </summary>
        public string? Content { get; set; }
    }
}