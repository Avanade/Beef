// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Beef.CodeGen
{
    /// <summary>
    /// Defines the <see cref="CodeGenerator"/> arguments.
    /// </summary>
    public class CodeGeneratorArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGeneratorArgs"/>.
        /// </summary>
        /// <param name="scriptFileName"></param>
        public CodeGeneratorArgs(string scriptFileName) => ScriptFileName = scriptFileName ?? throw new ArgumentNullException(nameof(scriptFileName));

        /// <summary>
        /// Gets the <b>Script</b> filename to load the content from the <c>Scripts</c> folder within the file system (primary) or <see cref="Assemblies"/> (secondary, recursive until found).
        /// </summary>
        public string ScriptFileName { get; }

        /// <summary>
        /// Gets or sets the base <see cref="DirectoryInfo"/> where the generated artefacts are to be written.
        /// </summary>
        /// <remarks>Where not specified will default to <see cref="Environment.CurrentDirectory"/>.</remarks>
        public DirectoryInfo? DirectoryBase { get; set; }

        /// <summary>
        /// Gets or sets the assemblies to use to probe for assembly resource (in defined sequence); will check this assembly also (no need to explicitly specify).
        /// </summary>
        public Assembly[]? Assemblies { get; set; }

        /// <summary>
        /// Dictionary of <see cref="IRootConfig.RuntimeParameters"/> name/value pairs.
        /// </summary>
        public IDictionary<string, string?>? Parameters { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="ILogger"/> to optionally log the underlying code-generation.
        /// </summary>
        public ILogger? Logger { get; set; }

        /// <summary>
        /// Indicates whether the code-generation is a simulation; i.e. does not update the artefacts.
        /// </summary>
        public bool IsSimulation { get; set; }
    }
}