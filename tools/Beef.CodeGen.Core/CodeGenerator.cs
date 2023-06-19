using OnRamp.Scripts;
using OnRamp;
using Microsoft.Extensions.Logging;
using Beef.CodeGen.Config.Entity;
using System.Linq;
using System.Collections.Generic;

namespace Beef.CodeGen
{
    /// <summary>
    /// Code-generation orchestrator.
    /// </summary>
    internal class CodeGenerator : OnRamp.CodeGenerator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CodeGenerator"/> class.
        /// </summary>
        /// <param name="args">The <see cref="ICodeGeneratorArgs"/>.</param>
        /// <param name="scripts">The <see cref="CodeGenScript"/>.</param>
        public CodeGenerator(ICodeGeneratorArgs args, CodeGenScript scripts) : base(args, scripts) { }

        /// <inheritdoc/>
        protected override void OnAfterScript(CodeGenScriptItem script, CodeGenStatistics statistics)
        {
            if (statistics.RootConfig is CodeGenConfig cgc)
            {
                var list = new List<(string Name, string Route, string Method, string Status, string? Auth)>();

                if (script.Template!.StartsWith("EntityWebApiController_cs"))
                {
                    var entities = cgc.Root!.Entities!.Where(x => (!x.ExcludeWebApi.HasValue || !x.ExcludeWebApi.Value) && x.Operations!.Count > 0).AsEnumerable();
                    foreach (var e in entities)
                    {
                        foreach (var o in e.Operations!.Where(x => !x.ExcludeWebApi.HasValue || !x.ExcludeWebApi.Value))
                        {
                            list.Add(($"{e.Name}.{o.Name}", o.AgentWebApiRoute, o.WebApiMethod![4..], o.WebApiStatus!, o.WebApiAuthorize ?? e.WebApiAuthorize ?? "<NoAuth>"));
                        }
                    }

                    LogEndpoints(script, list);
                }
                else if (script.Template!.StartsWith("ReferenceDataWebApiController_cs"))
                {
                    list.Add(("ReferenceData.GetNamed", cgc.RefDataWebApiRoute!, "GET", "OK", cgc.WebApiAuthorize));
                    foreach (var e in cgc.Root!.RefDataEntities!)
                    {
                        list.Add(($"ReferenceData.{e.Name}GetAll", e.WebApiRoutePrefix!, "GET", "OK", e.WebApiAuthorize));
                    }

                    LogEndpoints(script, list);
                }
            }

            base.OnAfterScript(script, statistics);
        }

        /// <summary>
        /// Log endpoints.
        /// </summary>
        private static void LogEndpoints(CodeGenScriptItem script, List<(string Name, string Route, string Method, string Status, string? Auth)> list)
        {
            script.Root?.CodeGenArgs?.Logger?.LogInformation("    {Content}", $"Endpoints:{(list.Count == 0 ? " none" : $" ({list.Count})")}");
            foreach (var (Name, Route, Method, Status, Auth) in list.OrderBy(x => x.Route).ThenBy(x => x.Method))
            {
                script.Root?.CodeGenArgs?.Logger?.LogInformation("      {Content}", $"{Route} {Method.ToUpperInvariant()} [{Status}] {(string.IsNullOrEmpty(Auth) ? "" : $"({Auth}) ")}-> {Name}");
            }
        }
    }
}