// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.CodeGen.Config
{
    /// <summary>
    /// Represents the class category schema configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class CategorySchemaAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CategorySchemaAttribute"/> class.
        /// </summary>
        /// <param name="category">The grouping category.</param>
        public CategorySchemaAttribute(string category) => Category = category;

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
    }
}