// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.CodeGen.Config
{
    /// <summary>
    /// Represents the property schema configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class PropertyCollectionSchemaAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyCollectionSchemaAttribute"/> class.
        /// </summary>
        /// <param name="category">The grouping category.</param>
        public PropertyCollectionSchemaAttribute(string category) => Category = category;

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