// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

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
        public static string GetResourceContent(string name, params Assembly[] assemblies)
        {
            return GetResourceContent(name, "Resources", assemblies);
        }

        /// <summary>
        /// Gets the <b>Resource</b> content XML from the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <param name="assemblies">Assemblies to use to probe for assembly resource; will check this assembly also (no need to specify).</param>
        /// <returns>The resource content XML where found; otherwise, <c>null</c>.</returns>
        public static XElement GetResourceContentXml(string name, params Assembly[] assemblies)
        {
            var c = GetResourceContent(name, "Resources", assemblies);
            return (c == null) ? null : XElement.Parse(c);
        }

        /// <summary>
        /// Gets the <b>Template</b> content from the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <param name="assemblies">Assemblies to use to probe for assembly resource; will check this assembly also (no need to specify).</param>
        /// <returns>The resource content where found; otherwise, <c>null</c>.</returns>
        public static string GetTemplateContent(string name, params Assembly[] assemblies)
        {
            return GetResourceContent(name, "Templates", assemblies);
        }

        /// <summary>
        /// Gets the <b>Template</b> content XML from the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <param name="assemblies">assemblies to use to probe for assembly resource; will check this assembly also (no need to specify).</param>
        /// <returns>The resource content XML where found; otherwise, <c>null</c>.</returns>
        public static XElement GetTemplateContentXml(string name, params Assembly[] assemblies)
        {
            var c = GetResourceContent(name, "Templates", assemblies);
            return (c == null) ? null : XElement.Parse(c);
        }

        /// <summary>
        /// Gets the <b>Template</b> content from the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <param name="assemblies">Assemblies to use to probe for assembly resource; will check this assembly also (no need to specify).</param>
        /// <returns>The resource content where found; otherwise, <c>null</c>.</returns>
        public static string GetScriptContent(string name, params Assembly[] assemblies)
        {
            return GetResourceContent(name, "Scripts", assemblies);
        }

        /// <summary>
        /// Gets the <b>Template</b> content XML from the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <param name="assemblies">assemblies to use to probe for assembly resource; will check this assembly also (no need to specify).</param>
        /// <returns>The resource content XML where found; otherwise, <c>null</c>.</returns>
        public static XElement GetScriptContentXml(string name, params Assembly[] assemblies)
        {
            var c = GetResourceContent(name, "Scripts", assemblies);
            return (c == null) ? null : XElement.Parse(c);
        }

        /// <summary>
        /// Gets the specified resource content.
        /// </summary>
        private static string GetResourceContent(string name, string resourceType, params Assembly[] assemblies)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (string.IsNullOrEmpty(resourceType))
                throw new ArgumentNullException(nameof(resourceType));

            foreach (var ass in new List<Assembly>(assemblies) { typeof(ResourceManager).Assembly })
            {
                var rn = ass.GetManifestResourceNames().Where(x => x.EndsWith($".{resourceType}.{name}")).FirstOrDefault();
                if (rn != null)
                {
                    var ri = ass.GetManifestResourceInfo(rn);
                    if (ri != null)
                    {
                        using (var sr = new StreamReader(ass.GetManifestResourceStream(rn)))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }

            return null;
        }
    }
}