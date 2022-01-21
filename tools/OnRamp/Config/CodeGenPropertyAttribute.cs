// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using System;

namespace OnRamp.Config
{
    /// <summary>
    /// Represents the <i>code-generation</i> property configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CodeGenPropertyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenPropertyAttribute"/> class.
        /// </summary>
        /// <param name="category">The grouping category.</param>
        public CodeGenPropertyAttribute(string category) => Category = category ?? throw new ArgumentNullException(nameof(category));

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
        /// Indicates whether the property is mandatory.
        /// </summary>
        public bool IsMandatory { get; set; }

        /// <summary>
        /// Inidicates whether the property is considered important.
        /// </summary>
        public bool IsImportant { get; set; }

        /// <summary>
        /// Gets or sets the list of option values.
        /// </summary>
        public string[]? Options { get; set; }
    }
}