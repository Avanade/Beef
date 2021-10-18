// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using System;

namespace OnRamp.Config
{
    /// <summary>
    /// Represents the <i>code-generation</i> property collection configuration.
    /// </summary>
    /// <remarks>The property should be either a <c>List&lt;string&gt;</c> or <c>List&lt;T&gt;</c> where <c>T</c> inherits from <see cref="ConfigBase"/>.</remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CodeGenPropertyCollectionAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenPropertyCollectionAttribute"/> class.
        /// </summary>
        /// <param name="category">The grouping category.</param>
        public CodeGenPropertyCollectionAttribute(string category) => Category = category;

        /// <summary>
        /// Gets or sets the category.
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

        /// <summary>
        /// Gets or sets the markdown.
        /// </summary>
        public string? Markdown { get; set; }

        /// <summary>
        /// Indicates whether the property is mandatory.
        /// </summary>
        public bool IsMandatory { get; set; }

        /// <summary>
        /// Inidicates whether the property is considered important.
        /// </summary>
        public bool IsImportant { get; set; }
    }
}