// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Beef.Json
{
    /// <summary>
    /// Provides a means to apply a filter to include or exclude properties within a <see cref="JToken"/> (in effect removing the unwanted properties).
    /// </summary>
    public static class JsonPropertyFilter
    {
        /// <summary>
        /// Applies the inclusion and exclusion of properties (using JSON names) to a <paramref name="value"/> resulting in the corresponding <see cref="JToken"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="include">The list of JSON property names to include.</param>
        /// <param name="exclude">The list of JSON property names to exclude.</param>
        /// <returns>The resulting <see cref="JToken"/>.</returns>
        /// <remarks>The <paramref name="include"/> and <paramref name="exclude"/> arrays are mutually exclusive; the <paramref name="include"/> will take precedence where both are specified.</remarks>
        public static JToken Apply<T>(T value, IEnumerable<string> include = null, IEnumerable<string> exclude = null)
        {
            if (value == null)
                return null;

            var json = JToken.FromObject(value);
            JsonApply(json, include, exclude);

            return json;
        }

        /// <summary>
        /// Applies the inclusion and exclusion of properties (using JSON names) to an existing <see cref="JToken"/>.
        /// </summary>
        /// <param name="json">The <see cref="JObject"/> value.</param>
        /// <param name="include">The list of JSON property names to include.</param>
        /// <param name="exclude">The list of JSON property names to exclude.</param>
        public static void JsonApply(JToken json, IEnumerable<string> include = null, IEnumerable<string> exclude = null)
        {
            Check.NotNull(json, nameof(json));

            if (json.Type == JTokenType.Array)
                JsonApply((JArray)json, include, exclude);
            else if (json.Type == JTokenType.Object)
                JsonApply((JObject)json, include, exclude);
        }

        /// <summary>
        /// Applies the inclusion and exclusion of properties (using JSON names) to an existing <see cref="JArray"/>.
        /// </summary>
        /// <param name="json">The <see cref="JArray"/> value.</param>
        /// <param name="include">The list of JSON property names to include.</param>
        /// <param name="exclude">The list of JSON property names to exclude.</param>
        /// <remarks>The <paramref name="include"/> and <paramref name="exclude"/> arrays are mutually exclusive; the <paramref name="include"/> will take precedence where both are specified.</remarks>
        public static void JsonApply(JArray json, IEnumerable<string> include = null, IEnumerable<string> exclude = null)
        {
            Check.NotNull(json, nameof(json));

            bool isInclude = include != null && include.Any();
            if (isInclude || (exclude != null && exclude.Any()))
            {
                foreach (var jp in json.Children().ToArray())
                {
#pragma warning disable CA1062 // Validate arguments of public methods; is checked; false negative.
                    Filter(jp, isInclude, isInclude ? Expand(include) : exclude);
#pragma warning restore CA1062
                }
            }
        }

        /// <summary>
        /// Applies the inclusion and exclusion of properties (using JSON names) to an existing <see cref="JObject"/>.
        /// </summary>
        /// <param name="json">The <see cref="JObject"/> value.</param>
        /// <param name="include">The list of JSON property names to include.</param>
        /// <param name="exclude">The list of JSON property names to exclude.</param>
        /// <remarks>The <paramref name="include"/> and <paramref name="exclude"/> arrays are mutually exclusive; the <paramref name="include"/> will take precedence where both are specified.</remarks>
        public static void JsonApply(JObject json, IEnumerable<string> include = null, IEnumerable<string> exclude = null)
        {
            Check.NotNull(json, nameof(json));

            bool isInclude = include != null && include.Count() > 0;
            if ((isInclude) || (exclude != null && exclude.Count() > 0))
            {
                Filter(json, isInclude, isInclude ? Expand(include) : exclude);
            }
        }

        /// <summary>
        /// Expands the JSON property names by adding intermediary values within a specified hierarchical name.
        /// </summary>
        private static string[] Expand(IEnumerable<string> names)
        {
            var sb = new StringBuilder();
            var kod = new KeyOnlyDictionary<string>();
            foreach (var f in names)
            {
                sb.Clear();
                var parts = f.Split('.');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (i > 0)
                        sb.Append(".");

                    sb.Append(parts[i]);
                    kod.Add(sb.ToString());
                }
            }

            return kod.Keys.ToArray();
        }

        /// <summary>
        /// Filter the JSON properties by either including or excluding as specified.
        /// </summary>
        private static void Filter(JToken json, bool isInclude, IEnumerable<string> names = null)
        {
            foreach (var jp in json.Children().ToArray())
            {
                var path = RemoveIndexing(jp.Path);
                if (names.Contains(path, StringComparer.OrdinalIgnoreCase))
                {
                    if (!isInclude)
                    {
                        jp.Remove();
                        continue;
                    }
                }
                else
                {
                    if (isInclude)
                    {
                        jp.Remove();
                        continue;
                    }
                }

                if (!names.Any(x => x.StartsWith(path + ".", StringComparison.OrdinalIgnoreCase)))
                    continue;

                foreach (var jc in jp.Children())
                {
                    switch (jc.Type)
                    {
                        case JTokenType.Object:
                            Filter(jc, isInclude, names);
                            break;

                        case JTokenType.Array:
                            foreach (var jv in jc.Children())
                            {
                                Filter(jv, isInclude, names);
                            }

                            break;

                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Removes any indexing within a path; i.e. 'Array[0].Value' -> 'Array.Value'.
        /// </summary>
        private static string RemoveIndexing(string path)
        {
            if (path == null)
                return null;

            var txt = path;

            while (true)
            {
                var li = txt.IndexOf('[');
                if (li < 0)
                    break;

                var ri = txt.IndexOf(']');
                if (ri < 0 || ri < li)
                    break;

                txt = txt.Remove(li, ri - li + 1);
            }

            return (txt.Length > 1 && txt[0] == '.') ? txt.Substring(1) : txt;
        }
    }
}