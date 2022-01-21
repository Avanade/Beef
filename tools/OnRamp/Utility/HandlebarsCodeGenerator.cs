﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using HandlebarsDotNet;
using System;
using System.IO;

namespace OnRamp.Utility
{
    /// <summary>
    /// The core generic code generator that manages the <b>Handlebars</b> compilation (cached for performance) and enables the corresponding <see cref="Generate"/> (one or more invocations).
    /// </summary>
    public class HandlebarsCodeGenerator
    {
        private readonly HandlebarsTemplate<object?, object?> _template;

        /// <summary>
        /// Static constructor.
        /// </summary>
        static HandlebarsCodeGenerator()
        {
            HandlebarsHelpers.RegisterHelpers();
            Handlebars.Configuration.TextEncoder = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlebarsCodeGenerator"/> with the specified <paramref name="templateContent"/>.
        /// </summary>
        /// <param name="templateContent">The template <c>handlebars.js</c> contents.</param>
        public HandlebarsCodeGenerator(string templateContent) => _template = Handlebars.Compile(templateContent ?? throw new ArgumentNullException(nameof(templateContent)));

        /// <summary>
        /// Initializes a new instance of the <see cref="HandlebarsCodeGenerator"/> from the <paramref name="sr"/>.
        /// </summary>
        /// <param name="sr">The <see cref="Stream"/> containing the <c>handlebars.js</c> contents.</param>
        public HandlebarsCodeGenerator(StreamReader sr)
        {
            if (sr == null)
                throw new ArgumentNullException(nameof(sr));

            _template = Handlebars.Compile(sr.ReadToEnd());
        }

        /// <summary>
        /// Generate content from the template using the <paramref name="context"/> and <paramref name="data"/>.
        /// </summary>
        /// <param name="context">The primary context value referenced within the template.</param>
        /// <param name="data">The optional secondary data.</param>
        /// <returns></returns>
        public string Generate(object? context, object? data = null) => _template(context, data);
    }
}