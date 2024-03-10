// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Entity;
using OnRamp;
using OnRamp.Scripts;

namespace Beef.CodeGen
{
    /// <summary>
    /// Code-generation orchestrator.
    /// </summary>
    /// <param name="args">The <see cref="ICodeGeneratorArgs"/>.</param>
    /// <param name="scripts">The <see cref="CodeGenScript"/>.</param>
    internal class CodeGenerator(ICodeGeneratorArgs args, CodeGenScript scripts) : OnRamp.CodeGenerator(args, scripts)
    {
        /// <inheritdoc/>
        protected override void OnAfterScript(CodeGenScriptItem script, CodeGenStatistics statistics)
        {
            if (statistics.RootConfig is CodeGenConfig cgc)
            {
                if (script.Template!.StartsWith("EntityWebApiController_cs"))
                {
                    var endpoints = new EndPointStatistics();
                    endpoints.AddEntityEndPoints(cgc);
                    endpoints.WriteInline(cgc.CodeGenArgs!.Logger!);

                }
                else if (script.Template!.StartsWith("ReferenceDataWebApiController_cs"))
                {
                    var endpoints = new EndPointStatistics();
                    endpoints.AddRefDataEndPoints(cgc);
                    endpoints.WriteInline(cgc.CodeGenArgs!.Logger!);
                }
            }

            base.OnAfterScript(script, statistics);
        }
    }
}