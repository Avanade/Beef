// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
using Beef.CodeGen.Scripts;
using Beef.CodeGen.Utility;
using HandlebarsDotNet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.Generators
{
    /// <summary>
    /// Provides the base code-generation capabilities leveraging <see cref="Handlebars"/>.
    /// </summary>
    /// <typeparam name="TRootConfig">The root <see cref="ConfigBase"/> <see cref="Type"/>.</typeparam>
    /// <typeparam name="TGenConfig">The code-generation <see cref="ConfigBase"/> <see cref="Type"/> required by the underlying template.</typeparam>
    public abstract class CodeGeneratorBase<TRootConfig, TGenConfig> : CodeGeneratorBase where TRootConfig : ConfigBase where TGenConfig : ConfigBase
    {
        /// <summary>
        /// Gets the root <see cref="Type"/>.
        /// </summary>
        internal override Type RootType => typeof(TRootConfig);

        /// <summary>
        /// Selects (filters) the <typeparamref name="TRootConfig"/> to access the underlying <typeparamref name="TGenConfig"/>. 
        /// </summary>
        /// <param name="config">The root configuration.</param>
        /// <returns>The selected <typeparamref name="TGenConfig"/>.</returns>
        protected abstract IEnumerable<TGenConfig> SelectGenConfig(TRootConfig config);

        /// <summary>
        /// Performs the code generation.
        /// </summary>
        /// <param name="script">The <see cref="CodeGenScript"/>.</param>
        /// <param name="config">The root <see cref="ConfigBase"/>.</param>
        /// <param name="artefactGenerated">The <see cref="Action{T}"/> to invoked per artefact generated to be output.</param>
        public override void Generate(CodeGenScript script, ConfigBase config, Action<CodeGenOutputArgs> artefactGenerated) => Generate(script, (TRootConfig)config, artefactGenerated);

        /// <summary>
        /// Performs the code generation.
        /// </summary>
        /// <param name="script">The <see cref="CodeGenScript"/>.</param>
        /// <param name="config">The root <see cref="ConfigBase"/>.</param>
        /// <param name="artefactGenerated">The <see cref="Action{T}"/> to invoked per artefact generated to be output.</param>
        public void Generate(CodeGenScript script, TRootConfig config, Action<CodeGenOutputArgs> artefactGenerated)
        {
            if (script == null)
                throw new ArgumentNullException(nameof(script));

            if (config == null)
                throw new ArgumentNullException(nameof(config));

            var values = SelectGenConfig(config);
            if (!values.Any())
                return;

            Handlebars.Configuration.TextEncoder = null;

            using var stream = StreamLocator.GetTemplateStream(script.Template!, script.Root!.CodeGenArgs!.Assemblies);
            var contentHandlebars = new HandlebarsCodeGenerator(stream!);
            var fileNameHandlebars = new HandlebarsCodeGenerator(script.File!);
            var directoryNameHandlebars = string.IsNullOrEmpty(script.Directory) ? null : new HandlebarsCodeGenerator(script.Directory);

            foreach (var val in values)
            {
                var args = new CodeGenOutputArgs(script, directoryNameHandlebars?.Generate(val), fileNameHandlebars.Generate(val), contentHandlebars.Generate(val));
                artefactGenerated(args);
            }
        }
    }

    /// <summary>
    /// Provides the base code-generation capabilities leveraging <see cref="Handlebars"/> for where the root and code-gen <see cref="ConfigBase"/> are the same.
    /// </summary>
    /// <typeparam name="TRootConfig">The root <see cref="ConfigBase"/> <see cref="Type"/>.</typeparam>
    public abstract class CodeGeneratorBase<TRootConfig> : CodeGeneratorBase<TRootConfig, TRootConfig> where TRootConfig : ConfigBase, IRootConfig
    {
        /// <summary>
        /// The selection is the <typeparamref name="TRootConfig"/> itself.
        /// </summary>
        /// <param name="config">The root configuration.</param>
        /// <returns>The root configuration.</returns>
        protected override IEnumerable<TRootConfig> SelectGenConfig(TRootConfig config) => new TRootConfig[] { config };
    }
}