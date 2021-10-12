// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.CodeGen.Config
{
    /// <summary>
    /// Represents the <i>code-generation</i> class category configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class CodeGenCategoryAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenCategoryAttribute"/> class.
        /// </summary>
        /// <param name="category">The grouping category name.</param>
        public CodeGenCategoryAttribute(string category) => Category = category;

        /// <summary>
        /// Gets or sets the category name.
        /// </summary>
        public string Category { get; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string? Description { get; set; }
    }
}