// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using System;

namespace OnRamp.Config
{
    /// <summary>
    /// Represents the <i>code-generation</i> class configuration.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class CodeGenClassAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenClassAttribute"/> class.
        /// </summary>
        /// <param name="name">The class name.</param>
        public CodeGenClassAttribute(string name) => Name = name ?? throw new ArgumentNullException(nameof(name));

        /// <summary>
        /// Gets the class name.
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

        /// <summary>
        /// Gets or sets the example markdown.
        /// </summary>
        public string? ExampleMarkdown { get; set; }
    }
}