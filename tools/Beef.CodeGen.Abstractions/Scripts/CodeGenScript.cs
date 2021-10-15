// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
using Beef.CodeGen.Generators;
using Beef.CodeGen.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Beef.CodeGen.Scripts
{
    /// <summary>
    /// Represents the <see cref="HandlebarsCodeGenerator"/> script arguments used to define a <see cref="CodeGeneratorBase"/> (as specified by the <see cref="Type"/>) and other associated code-generation arguments.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [CodeGenClass("Generate", Title = "'Generate' command.", Description = "The `Generate` command defines the execution parameters for a code-generation execution.")]
    [CodeGenCategory("Key", Title = "Provides the _Key_ configuration.")]
    public class CodeGenScript : ConfigBase<CodeGenScripts, CodeGenScripts>
    {
        private CodeGeneratorBase? _generator;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks><inheritdoc/></remarks>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("Generate");

        /// <summary>
        /// Gets or sets the <see cref="CodeGeneratorBase"/> <see cref="System.Type"/>..
        /// </summary>
        [JsonProperty("type")]
        [CodeGenProperty("Key", Title = "The .NET Generator (CodeGeneratorBase) Type.", IsMandatory = true)]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the template resource name.
        /// </summary>
        [JsonProperty("template")]
        [CodeGenProperty("Key", Title = "The template resource name.", IsMandatory = true)]
        public string? Template { get; set; }

        /// <summary>
        /// Gets or sets the file name.
        /// </summary>
        /// <remarks>The name supports <b>Handlebars</b> syntax.</remarks>
        [JsonProperty("file")]
        [CodeGenProperty("Key", Title = "The file name.", IsMandatory = true, Description = "The name supports _Handlebars_ syntax.")]
        public string? File { get; set; }

        /// <summary>
        /// Gets or sets the directory name.
        /// </summary>
        /// <remarks>The name supports <b>Handlebars</b> syntax.</remarks>
        [JsonProperty("directory")]
        [CodeGenProperty("Key", Title = "The directory name.", Description = "The name supports _Handlebars_ syntax.")]
        public string? Directory { get; set; }

        /// <summary>
        /// Indicates whether the file is only generated once; i.e. only created where it does not already exist.
        /// </summary>
        [JsonProperty("genOnce")]
        [CodeGenProperty("Key", Title = "Indicates whether the file is only generated once; i.e. only created where it does not already exist.")]
        public bool IsGenOnce { get; set; }

        /// <summary>
        /// Gets or sets the help text.
        /// </summary>
        [JsonProperty("text")]
        [CodeGenProperty("Key", Title = "The additional text written to the log to enable additional context.")]
        public string? Text { get; set; }

        /// <summary>
        /// Gets the runtime parameters (as specified via <see cref="ConfigBase.ExtraProperties"/>).
        /// </summary>
        public Dictionary<string, string?> RuntimeParameters { get; } = new Dictionary<string, string?>();

        /// <summary>
        /// Gets the <see cref="CodeGeneratorBase"/> as specified by <see cref="Type"/>.
        /// </summary>
        /// <returns>The <see cref="CodeGeneratorBase"/>.</returns>
        public CodeGeneratorBase GetGenerator() => _generator ?? throw new InvalidOperationException("Prepare operation must be performed before this property can be accessed.");

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            // Make sure generator type exists and is CodeGeneratorBase.
            Type type;
            try
            {
                type = System.Type.GetType(Type);

                if (type == null)
                    throw new CodeGenException(this, nameof(Type), $"Type '{Type}' does not exist.");

                if (!IsSubclassOfBaseType(typeof(CodeGeneratorBase), type) || type.GetConstructor(Array.Empty<Type>()) == null)
                    throw new CodeGenException(this, nameof(Type), $"Type '{Type}' does not implement CodeGeneratorBase and/or have a default parameterless constructor.");

                _generator = (CodeGeneratorBase)Activator.CreateInstance(type) ?? throw new CodeGenException(this, nameof(Type), $"Type '{Type}' was unable to be instantiated.");
                if (_generator.RootType != Root!.GetConfigType())
                    throw new CodeGenException(this, nameof(Type), $"Type '{Type}' RootType '{_generator.RootType.Name}' must be the same as the ConfigType '{Root!.GetConfigType().Name}'.");
            }
            catch (CodeGenException) { throw; }
            catch (Exception ex) { throw new CodeGenException(this, nameof(Type), $"Type '{Type}' is invalid: {ex.Message}"); }

            // Make sure the template exists.
            if (!StreamLocator.HasTemplateStream(Template!, Root!.CodeGenArgs!.Assemblies.ToArray()))
                throw new CodeGenException(this, nameof(Template), $"Template '{Template}' does not exist.");

            // Convert any extra properties as runtime parameters.
            try
            {
                if (ExtraProperties != null)
                {
                    foreach (var json in ExtraProperties)
                    {
                        RuntimeParameters.Add(json.Key, json.Value.ToObject<string?>());
                    }
                }
            }
            catch (Exception ex) { throw new CodeGenException(this, nameof(ExtraProperties), $"Error converting into RuntimeParameters: {ex.Message}"); }
        }
    }
}