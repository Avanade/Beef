// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/OnRamp

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OnRamp.Utility
{
    /// <summary>
    /// <see cref="Stream"/> locator/manager.
    /// </summary>
    public static class StreamLocator
    {
        /// <summary>
        /// Gets the <b>Resource</b> content from the file system and then <c>Resources</c> folder within the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="assemblies">Assemblies to use to probe for assembly resource (in defined sequence); will check this assembly also (no need to specify).</param>
        /// <returns>The resource <see cref="StreamReader"/> where found; otherwise, <c>null</c>.</returns>
        public static StreamReader? GetResourcesStreamReader(string fileName, params Assembly[]? assemblies) => GetStreamReader(fileName, "Resources", assemblies);

        /// <summary>
        /// Gets the <b>Script</b> content from the file system and then <c>Scripts</c> folder within the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="assemblies">Assemblies to use to probe for assembly resource (in defined sequence); will check this assembly also (no need to specify).</param>
        /// <returns>The resource <see cref="StreamReader"/> where found; otherwise, <c>null</c>.</returns>
        public static StreamReader? GetScriptStreamReader(string fileName, params Assembly[]? assemblies) => GetStreamReader(fileName, "Scripts", assemblies);

        /// <summary>
        /// Gets the <b>Template</b> content from the file system and then <c>Templates</c> folder within the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="assemblies">Assemblies to use to probe for assembly resource (in defined sequence); will check this assembly also (no need to specify).</param>
        /// <returns>The resource <see cref="StreamReader"/> where found; otherwise, <c>null</c>.</returns>
        public static StreamReader? GetTemplateStreamReader(string fileName, params Assembly[]? assemblies) => GetStreamReader(fileName, "Templates", assemblies);

        /// <summary>
        /// Gets the specified content from the file system and then <paramref name="contentType"/> folder within the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="contentType">The optional content type name.</param>
        /// <param name="assemblies">The assemblies to use to probe for the assembly resource (in defined sequence); will check this assembly also (no need to specify).</param>
        /// <returns>The resource <see cref="StreamReader"/> where found; otherwise, <c>null</c>.</returns>
        public static StreamReader? GetStreamReader(string fileName, string? contentType = null, params Assembly[]? assemblies)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var fi = new FileInfo(fileName);
            if (fi.Exists)
                return new StreamReader(fi.FullName);

            if (!string.IsNullOrEmpty(contentType))
            {
                fi = new FileInfo(Path.Combine(fi.DirectoryName, contentType, fi.Name));
                if (fi.Exists)
                    return new StreamReader(fi.FullName);

                if (assemblies != null)
                {
                    foreach (var ass in new List<Assembly>(assemblies) { typeof(StreamLocator).Assembly })
                    {
                        var rn = ass.GetManifestResourceNames().Where(x => x.EndsWith($".{contentType}.{fi.Name}", StringComparison.InvariantCulture)).FirstOrDefault();
                        if (rn != null)
                        {
                            var ri = ass.GetManifestResourceInfo(rn);
                            if (ri != null)
                                return new StreamReader(ass.GetManifestResourceStream(rn)!);
                        }
                    }
                }
            }

            return null!;
        }

        /// <summary>
        /// Indicates whether the specified <b>Resource</b> content exists within the file system and then <c>Resources</c> folder within the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="assemblies">Assemblies to use to probe for assembly resource (in defined sequence); will check this assembly also (no need to specify).</param>
        /// <returns>The resource <see cref="Stream"/> where found; otherwise, <c>null</c>.</returns>
        public static bool HasResourceStream(string fileName, params Assembly[]? assemblies) => HasStream(fileName, "Resources", assemblies);

        /// <summary>
        /// Indicates whether the specified <b>Script</b> content exists within the file system and then <c>Scripts</c> folder within the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="assemblies">Assemblies to use to probe for assembly resource (in defined sequence); will check this assembly also (no need to specify).</param>
        /// <returns>The resource <see cref="Stream"/> where found; otherwise, <c>null</c>.</returns>
        public static bool HasScriptStream(string fileName, params Assembly[]? assemblies) => HasStream(fileName, "Scripts", assemblies);

        /// <summary>
        /// Indicates whether the specified <b>Template</b> content exists within the file system and then <c>Templates</c> folder within the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="assemblies">Assemblies to use to probe for assembly resource (in defined sequence); will check this assembly also (no need to specify).</param>
        /// <returns>The resource <see cref="Stream"/> where found; otherwise, <c>null</c>.</returns>
        public static bool HasTemplateStream(string fileName, params Assembly[]? assemblies) => HasStream(fileName, "Templates", assemblies);

        /// <summary>
        /// Indicates whether the specified resource content exists within the file system or the <paramref name="contentType"/> folder within the <paramref name="assemblies"/> until found.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <param name="contentType">The optional content type name.</param>
        /// <param name="assemblies">The assemblies to use to probe for the assembly resource (in defined sequence); will check this assembly also (no need to specify).</param>
        /// <returns><c>true</c> indicates that the <see cref="Stream"/> exists; otherwise, <c>false</c>.</returns>
        public static bool HasStream(string fileName, string? contentType, params Assembly[]? assemblies)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            var fi = new FileInfo(fileName);
            if (fi.Exists)
                return true;

            if (!string.IsNullOrEmpty(contentType))
            {
                fi = new FileInfo(Path.Combine(fi.DirectoryName, contentType, fi.Name));
                if (fi.Exists)
                    return true;

                if (assemblies != null)
                {
                    foreach (var ass in new List<Assembly>(assemblies) { typeof(StreamLocator).Assembly })
                    {
                        var rn = ass.GetManifestResourceNames().Where(x => x.EndsWith($".{contentType}.{fi.Name}", StringComparison.InvariantCulture)).FirstOrDefault();
                        if (rn != null)
                            return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Gets (determines) the <see cref="StreamContentType"/> from the <paramref name="fileName"/> extension.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>The corresponding <see cref="StreamContentType"/>.</returns>
        public static StreamContentType GetContentType(string fileName) => new FileInfo(fileName).Extension.ToUpperInvariant() switch
        {
            ".YAML" => StreamContentType.Yaml,
            ".YML" => StreamContentType.Yaml,
            ".JSON" => StreamContentType.Json,
            ".JSN" => StreamContentType.Json,
            _ => StreamContentType.Unknown
        };
    }
}