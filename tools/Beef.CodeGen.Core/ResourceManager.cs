// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Beef.CodeGen
{
    /// <summary>
    /// Resource management functions.
    /// </summary>
    public static class ResourceManager
    {
        /// <summary>
        /// Gets the <b>Resource</b> content from the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <param name="assemblies">Assemblies to use to probe for assembly resource; will check this assembly also (no need to specify).</param>
        /// <returns>The resource content where found; otherwise, <c>null</c>.</returns>
        public static Task<string?> GetResourceContentAsync(string name, params Assembly[] assemblies)
        {
            return GetResourceContentAsync(name, "Resources", assemblies);
        }

        /// <summary>
        /// Gets the <b>Script</b> content from the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <param name="assemblies">Assemblies to use to probe for assembly resource; will check this assembly also (no need to specify).</param>
        /// <returns>The resource content where found; otherwise, <c>null</c>.</returns>
        public static Task<string?> GetScriptContentAsync(string name, params Assembly[] assemblies)
        {
            return GetResourceContentAsync(name, "Scripts", assemblies);
        }

        /// <summary>
        /// Gets the <b>Template</b> content from the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <param name="assemblies">Assemblies to use to probe for assembly resource; will check this assembly also (no need to specify).</param>
        /// <returns>The resource content where found; otherwise, <c>null</c>.</returns>
        public static Task<string?> GetTemplateContentAsync(string name, params Assembly[] assemblies)
        {
            return GetResourceContentAsync(name, "Templates", assemblies);
        }

        /// <summary>
        /// Gets the specified resource content from the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <param name="resourceType">The resource type.</param>
        /// <param name="assemblies">The assemblies to use to probe for the assembly resource; will check this assembly also (no need to specify).</param>
        /// <returns>The resource content where found; otherwise, <c>null</c>.</returns>
        public static async Task<string?> GetResourceContentAsync(string name, string resourceType, params Assembly[] assemblies)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrEmpty(resourceType))
                throw new ArgumentNullException(nameof(resourceType));

            foreach (var ass in new List<Assembly>(assemblies) { typeof(ResourceManager).Assembly })
            {
                var rn = ass.GetManifestResourceNames().Where(x => x.EndsWith($".{resourceType}.{name}", StringComparison.InvariantCulture)).FirstOrDefault();
                if (rn != null)
                {
                    var ri = ass.GetManifestResourceInfo(rn);
                    if (ri != null)
                    {
                        using var sr = new StreamReader(ass.GetManifestResourceStream(rn)!);
                        return await sr.ReadToEndAsync().ConfigureAwait(false);
                    }
                }
            }

            return null;
        }
    }
}