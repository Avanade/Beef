﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config;
using Beef.CodeGen.Scripts;
using Beef.CodeGen.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Beef.CodeGen
{
    /// <summary>
    /// Primary code-generation orchestrator.
    /// </summary>
    public class CodeGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenerator"/> class.
        /// </summary>
        /// <param name="args">The <see cref="CodeGeneratorArgs"/>.</param>
        public CodeGenerator(CodeGeneratorArgs args)
        {
            CodeGenArgs = args ?? throw new ArgumentNullException(nameof(args));
            CodeGenArgs.DirectoryBase ??= new DirectoryInfo(Environment.CurrentDirectory);

            using var s = StreamLocator.GetScriptStream(CodeGenArgs.ScriptFileName, CodeGenArgs.Assemblies) ?? throw new CodeGenException($"Script '{CodeGenArgs.ScriptFileName}' does not exist.");
            Scripts = LoadScriptStream(null, CodeGenArgs.ScriptFileName, s);
        }

        /// <summary>
        /// Load/parse the script configuration from the stream.
        /// </summary>
        private CodeGenScripts LoadScriptStream(CodeGenScripts? rootScript, string scriptFileName, Stream scriptStream)
        {
            try
            {
                // Load file and deserialize.
                CodeGenScripts scripts;
                try
                {
                    scripts = StreamLocator.GetContentType(scriptFileName) switch
                    {
                        StreamContentType.Yaml => scriptStream.DeserializeYaml<CodeGenScripts>(),
                        StreamContentType.Json => scriptStream.DeserializeJson<CodeGenScripts>(),
                        _ => throw new CodeGenException($"Stream content type is not supported.")
                    } ?? throw new CodeGenException($"Stream is empty.");
                }
                catch (CodeGenException) { throw; }
                catch (Exception ex) { throw new CodeGenException(ex.Message); }

                // Merge in the parameters and prepare/validate.
                scripts.SetCodeGenArgs(CodeGenArgs);
                scripts.MergeRuntimeParameters(CodeGenArgs.Parameters);
                scripts.Prepare(scripts, scripts);
                rootScript ??= scripts;

                // Recursively inherit (include/merge) additional scripts files.
                var inherited = new List<CodeGenScript>();
                if (rootScript.GetConfigType() != scripts.GetConfigType())
                    throw new CodeGenException(scripts, nameof(CodeGenScripts.ConfigType), $"Inherited ConfigType '{scripts.ConfigType}' must be the same as root ConfigType '{rootScript.ConfigType}'.");

                if (scripts.Inherits != null)
                {
                    foreach (var ifn in scripts.Inherits)
                    {
                        using var s = StreamLocator.GetScriptStream(ifn, CodeGenArgs.Assemblies) ?? throw new CodeGenException($"Script '{ifn}' does not exist.");
                        var inherit = LoadScriptStream(rootScript, ifn, s);
                        foreach (var iscript in inherit.Generators!)
                        {
                            iscript.Root = rootScript.Root;
                            iscript.Parent = rootScript.Parent;
                            inherited.Add(iscript);
                        }
                    }
                }

                // Merge in the generators and editors.
                scripts.Generators!.InsertRange(0, inherited);
                rootScript.MergeEditors(scripts.GetEditors());

                return scripts;
            }
            catch (CodeGenException cgex)
            {
                throw new CodeGenException($"Script '{scriptFileName}' is invalid: {cgex.Message}");
            }
        }

        /// <summary>
        /// Gets the <see cref="CodeGenScripts"/>.
        /// </summary>
        public CodeGenScripts Scripts { get; }

        /// <summary>
        /// Gets the <see cref="CodeGeneratorArgs"/>.
        /// </summary>
        public CodeGeneratorArgs CodeGenArgs { get; }

        /// <summary>
        /// Execute the code-generation; loads the configuration file and executes each of the scripted templates.
        /// </summary>
        /// <param name="configFileName">The filename to load the content from the file system (primary) or <see cref="Beef.CodeGen.CodeGeneratorArgs.Assemblies"/> (secondary, recursive until found).</param>
        /// <returns>The resultant <see cref="CodeGenStats"/>.</returns>
        public CodeGenStats Generate(string configFileName)
        {
            using var stream = StreamLocator.GetStream(configFileName, null, CodeGenArgs.Assemblies) ?? throw new CodeGenException($"Config '{configFileName}' does not exist.");
            return Generate(configFileName, stream, StreamLocator.GetContentType(configFileName));
        }

        /// <summary>
        /// Execute the code-generation; loads the configuration from <paramref name="configStream"/> and executes each of the scripted templates.
        /// </summary>
        /// <param name="configStream">The <see cref="Stream"/> containing the configuration.</param>
        /// <param name="contentType">The corresponding <see cref="StreamContentType"/>.</param>
        /// <returns>The resultant <see cref="CodeGenStats"/>.</returns>
        public CodeGenStats Generate(Stream configStream, StreamContentType contentType) =>
            Generate("<stream>", configStream, contentType);

        /// <summary>
        /// Executes the code-generation.
        /// </summary>
        private CodeGenStats Generate(string configFileName, Stream configStream, StreamContentType contentType)
        {
            ConfigBase? config;
            IRootConfig rootConfig;

            // Load, validate and prepare.
            try
            {
                try
                {
                    config = contentType switch
                    {
                        StreamContentType.Yaml => (ConfigBase?)configStream.DeserializeYaml(Scripts.GetConfigType()),
                        StreamContentType.Json => (ConfigBase?)configStream.DeserializeJson(Scripts.GetConfigType()),
                        _ => throw new CodeGenException($"Stream content type is not supported.")
                    } ?? throw new CodeGenException($"Stream is empty.");
                }
                catch (CodeGenException) { throw; }
                catch (Exception ex) { throw new CodeGenException(ex.Message); }

                rootConfig = config as IRootConfig ?? throw new InvalidOperationException("Configuration must implement IRootConfig.");
                rootConfig.SetCodeGenArgs(CodeGenArgs);

                // Instantiate and execute any 'before' custom editors.
                foreach (var cet in Scripts.GetEditors().Distinct())
                {
                    var ce = (IConfigEditor)Activator.CreateInstance(cet);
                    ce.BeforePrepare(rootConfig);
                }

                config!.Prepare(config!, config!);

                // Instantiate and execute any 'after' custom editors.
                foreach (var cet in Scripts.GetEditors().Distinct())
                {
                    var ce = (IConfigEditor)Activator.CreateInstance(cet);
                    ce.AfterPrepare(rootConfig);
                }
            }
            catch (CodeGenException cgex)
            {
                throw new CodeGenException($"Config '{configFileName}' is invalid: {cgex.Message}");
            }

            // Generate the scripted artefacts.
            var overallStopwatch = Stopwatch.StartNew();
            var overallStats = new CodeGenStats();
            Stopwatch scriptStopwatch;
            foreach (var script in Scripts.Generators!)
            {
                scriptStopwatch = Stopwatch.StartNew();

                // Reset/merge the runtime parameters.
                rootConfig.ResetRuntimeParameters();
                rootConfig.MergeRuntimeParameters(script.RuntimeParameters);
                rootConfig.MergeRuntimeParameters(Scripts.RuntimeParameters);

                var scriptStats = new CodeGenStats();
                OnBeforeScript(script, scriptStats);
                script.GetGenerator().Generate(script, config, (oa) => OnCodeGenerated(oa, scriptStats));

                scriptStopwatch.Stop();
                scriptStats.ElapsedMilliseconds = scriptStopwatch.ElapsedMilliseconds;
                OnAfterScript(script, scriptStats);

                overallStats.Add(scriptStats);
            }

            overallStopwatch.Stop();
            overallStats.ElapsedMilliseconds = overallStopwatch.ElapsedMilliseconds;
            return overallStats;
        }

        /// <summary>
        /// Handles the processing before the <paramref name="script"/> is executed.
        /// </summary>
        /// <param name="script">The <see cref="CodeGenScript"/> to be executed.</param>
        /// <param name="statistics">The corresponding <see cref="CodeGenStats"/> for the <paramref name="script"/> execution.</param>
        /// <remarks>Default implementation will <see cref="ILogger">log</see> template details where appropriate.</remarks>
        protected virtual void OnBeforeScript(CodeGenScript script, CodeGenStats statistics) => script.Root?.CodeGenArgs?.Logger?.LogInformation(" Template: {template} {helptext}", script, script.HelpText == null ? string.Empty : $"({script.HelpText})");

        /// <summary>
        /// Handles the code generated content after it has been generated.
        /// </summary>
        /// <param name="outputArgs">The <see cref="CodeGenOutputArgs"/>.</param>
        /// <param name="statistics">The <see cref="CodeGenStats"/> for the generated artefact.</param>
        /// <remarks>Default implementation will write files (on create or update), update the <paramref name="statistics"/> accordingly, and <see cref="ILogger">log</see> where appropriate.</remarks>
        protected virtual void OnCodeGenerated(CodeGenOutputArgs outputArgs, CodeGenStats statistics)
        {
            var di = string.IsNullOrEmpty(outputArgs.DirectoryName) ? outputArgs.Script.Root!.CodeGenArgs!.DirectoryBase! : new DirectoryInfo(Path.Combine(outputArgs.Script.Root!.CodeGenArgs!.DirectoryBase!.FullName, outputArgs.DirectoryName));
            if (!Scripts!.CodeGenArgs!.IsSimulation && !di.Exists)
                di.Create();

            var fi = new FileInfo(Path.Combine(di.FullName, outputArgs.FileName));
            if (fi.Exists)
            {
                if (outputArgs.Script.IsGenOnce)
                    return;

                var prevContent = File.ReadAllText(fi.FullName);
                if (string.Compare(outputArgs.Content, prevContent, StringComparison.InvariantCulture) == 0)
                    statistics.NotChangedCount++;
                else
                {
                    if (!Scripts!.CodeGenArgs!.IsSimulation)
                        File.WriteAllText(fi.FullName, outputArgs.Content);

                    statistics.UpdatedCount++;
                    outputArgs.Script.Root?.CodeGenArgs?.Logger?.LogWarning("    Updated -> {fileName}", fi.Name);
                }
            }
            else
            {
                if (!Scripts!.CodeGenArgs!.IsSimulation)
                    File.WriteAllText(fi.FullName, outputArgs.Content);

                statistics.CreatedCount++;
                outputArgs.Script.Root?.CodeGenArgs?.Logger?.LogWarning("    Created -> {fileName}", fi.Name);
            }

            using var s = new StringReader(outputArgs.Content);
            for (; s.ReadLine() != null; statistics.LinesOfCodeCount++) { }
        }

        /// <summary>
        /// Handles the processing after the <paramref name="script"/> is executed.
        /// </summary>
        /// <param name="script">The <see cref="CodeGenScript"/> to be executed.</param>
        /// <param name="statistics">The corresponding <see cref="CodeGenStats"/> for the <paramref name="script"/> execution.</param>
        /// <remarks>Default implementation will <see cref="ILogger">log</see> <paramref name="statistics"/> where appropriate.</remarks>
        protected virtual void OnAfterScript(CodeGenScript script, CodeGenStats statistics) => script.Root?.CodeGenArgs?.Logger?.LogInformation("  {stats}", statistics);
    }
}