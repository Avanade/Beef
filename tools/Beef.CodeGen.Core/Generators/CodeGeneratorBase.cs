// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Provides the base code-generation capabilities leveraging <see cref="Handlebars"/>.
    /// </summary>
    public abstract class CodeGeneratorBase
    {
        /// <summary>
        /// Static constructor.
        /// </summary>
        static CodeGeneratorBase() => HandlebarsHelpers.RegisterHelpers();

        /// <summary>
        /// Check whether the nullable boolean is true.
        /// </summary>
        protected static bool IsTrue(bool? value) => value.HasValue && value.Value;

        /// <summary>
        /// Check whether the nullable boolean is null or false.
        /// </summary>
        protected static bool IsFalse(bool? value) => !value.HasValue || !value.Value;

        /// <summary>
        /// Gets or sets the output file name.
        /// </summary>
        public string? OutputFileName { get; set; }

        /// <summary>
        /// Gets or sets the output directory name.
        /// </summary>
        public string? OutputDirName { get; set; }

        /// <summary>
        /// Gets or sets the output generated directory name (defaults to <c>Generated</c>).
        /// </summary>
        public string? OutputGenDirName { get; set; } = "Generated";

        /// <summary>
        /// Performs the code generation.
        /// </summary>
        /// <param name="templateContents">The template <c>handlebars.js</c> contents.</param>
        /// <param name="config">The root <see cref="ConfigBase"/>.</param>
        /// <param name="codeGenerated">The action that is invoked when a output has been successfully generated.</param>
        public abstract void Generate(string templateContents, ConfigBase config, Action<CodeGeneratorEventArgs> codeGenerated);
    }

    /// <summary>
    /// Provides the base code-generation capabilities leveraging <see cref="Handlebars"/>.
    /// </summary>
    /// <typeparam name="TRootConfig">The root <see cref="ConfigBase"/> <see cref="Type"/>.</typeparam>
    /// <typeparam name="TGenConfig">The code-generation <see cref="ConfigBase"/> <see cref="Type"/> required by the underlying template.</typeparam>
    public abstract class CodeGeneratorBase<TRootConfig, TGenConfig> : CodeGeneratorBase where TRootConfig : ConfigBase where TGenConfig : ConfigBase
    {
        /// <summary>
        /// Selects (filters) the <typeparamref name="TRootConfig"/> to access the underlying <typeparamref name="TGenConfig"/>. 
        /// </summary>
        /// <param name="config">The root configuration.</param>
        /// <returns>The selected <typeparamref name="TGenConfig"/>.</returns>
        protected abstract IEnumerable<TGenConfig> SelectGenConfig(TRootConfig config);

        /// <summary>
        /// Performs the code generation.
        /// </summary>
        /// <param name="templateContents">The template <c>handlebars.js</c> contents.</param>
        /// <param name="config">The root <see cref="ConfigBase"/>.</param>
        /// <param name="codeGenerated">The action that is invoked when a output has been successfully generated.</param>
        public override void Generate(string templateContents, ConfigBase config, Action<CodeGeneratorEventArgs> codeGenerated) => Generate(templateContents, (TRootConfig)config, codeGenerated);

        /// <summary>
        /// Performs the code generation.
        /// </summary>
        /// <param name="template">The template <c>handlebars.js</c> contents.</param>
        /// <param name="config">The root <see cref="ConfigBase"/>.</param>
        /// <param name="codeGenerated">The action that is invoked when a output has been successfully generated.</param>
        public void Generate(string template, TRootConfig config, Action<CodeGeneratorEventArgs> codeGenerated)
        {
            if (string.IsNullOrEmpty(template))
                throw new ArgumentNullException(nameof(template));

            var values = SelectGenConfig(config);
            if (!values.Any())
                return;

            Handlebars.Configuration.TextEncoder = null;

            var templateHandlebars = Handlebars.Compile(template);
            var outputFileNameHandlebars = Handlebars.Compile(OutputFileName ?? throw new InvalidOperationException($"The '{nameof(OutputFileName)}' must not be null."));
            var outputDirNameHandlebars = Handlebars.Compile(OutputDirName ?? throw new InvalidOperationException($"The '{nameof(OutputDirName)}' must not be null."));

            foreach (var val in values)
            {
                var args = new CodeGeneratorEventArgs
                {
                    Content = templateHandlebars(val),
                    OutputFileName = outputFileNameHandlebars(val),
                    OutputDirName = outputDirNameHandlebars(val),
                };

                codeGenerated?.Invoke(args);
            }
        }
    }

    /// <summary>
    /// Provides the base code-generation capabilities leveraging <see cref="Handlebars"/> for where the root and code-gen <see cref="ConfigBase"/> are the same.
    /// </summary>
    /// <typeparam name="TRootConfig">The root <see cref="ConfigBase"/> <see cref="Type"/>.</typeparam>
    public abstract class CodeGeneratorBase<TRootConfig> : CodeGeneratorBase<TRootConfig, TRootConfig> where TRootConfig : ConfigBase
    {
        /// <summary>
        /// The selection is the <typeparamref name="TRootConfig"/> itself.
        /// </summary>
        /// <param name="config">The root configuration.</param>
        /// <returns>The root configuration.</returns>
        protected override IEnumerable<TRootConfig> SelectGenConfig(TRootConfig config) => new TRootConfig[] { config };
    }
}