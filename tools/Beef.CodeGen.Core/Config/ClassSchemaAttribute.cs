// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.CodeGen.Config
{
    /// <summary>
    /// Represents the class schema configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ClassSchemaAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ClassSchemaAttribute"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public ClassSchemaAttribute(string name) => Name = Check.NotNull(name, nameof(name));

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name { get; }

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
    }
}