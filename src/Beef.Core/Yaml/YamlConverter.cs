// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.RefData;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace Beef.Yaml
{
    /// <summary>
    /// Provides the capabilitity to convert <b>YAML</b> (or <b>JSON</b>) into a typed collection.
    /// </summary>
    public class YamlConverter
    {
        private readonly JArray? _json;

        /// <summary>
        /// Gets or sets the default <see cref="ChangeLog.CreatedDate"/> value.
        /// </summary>
        public static DateTime DefaultCreatedDate { get; set; } = Cleaner.Clean(DateTime.UtcNow);

        /// <summary>
        /// Gets or sets the default <see cref="ChangeLog.CreatedBy"/> value.
        /// </summary>
        public static string DefaultCreatedBy { get; set; } = ExecutionContext.EnvironmentUsername;

        /// <summary>
        /// Reads and parses the YAML <see cref="string"/>.
        /// </summary>
        /// <param name="yaml">The YAML <see cref="string"/>.</param>
        /// <returns>The <see cref="YamlConverter"/>.</returns>
        public static YamlConverter ReadYaml(string yaml)
        {
            using var sr = new StringReader(yaml);
            return ReadYaml(sr);
        }

        /// <summary>
        /// Reads and parses the YAML <see cref="Stream"/>.
        /// </summary>
        /// <param name="s">The YAML <see cref="Stream"/>.</param>
        /// <returns>The <see cref="YamlConverter"/>.</returns>
        public static YamlConverter ReadYaml(Stream s)
        {
            using var sr = new StreamReader(s);
            return ReadYaml(sr);
        }

        /// <summary>
        /// Reads and parses the YAML <see cref="TextReader"/>.
        /// </summary>
        /// <param name="tr">The YAML <see cref="TextReader"/>.</param>
        /// <returns>The <see cref="YamlConverter"/>.</returns>
        public static YamlConverter ReadYaml(TextReader tr)
        {
            var yaml = new DeserializerBuilder().Build().Deserialize(tr);
            var json = new SerializerBuilder().JsonCompatible().Build().Serialize(yaml!);
            return ReadJson(json);
        }

        /// <summary>
        /// Reads and parses the JSON <see cref="string"/>.
        /// </summary>
        /// <param name="json">The JSON <see cref="string"/>.</param>
        /// <returns>The <see cref="YamlConverter"/>.</returns>
        public static YamlConverter ReadJson(string json)
        {
            return new YamlConverter(JObject.Parse(json));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YamlConverter"/> class.
        /// </summary>
        /// <param name="json">The <see cref="JObject"/> configuration.</param>
        private YamlConverter(JObject json)
        {
            _json = Check.NotNull(json, nameof(json)).Children().FirstOrDefault()?.Children().OfType<JArray>().FirstOrDefault();
        }

        /// <summary>
        /// Converts the injested data into a collection of the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="name">The name of the node that constains the items.</param>
        /// <param name="replaceAllShorthandGuids">Indicates whether to recursively replaces all '^n' values where 'n' is an integer with n.ToGuid() equivalent.</param>
        /// <param name="initializeCreatedChangeLog">Indicates whether to set the <see cref="ChangeLog.CreatedBy"/> and <see cref="ChangeLog.CreatedDate"/> where <typeparamref name="T"/> implements <see cref="IChangeLog"/>.</param>
        /// <param name="initializeReferenceData">Indicates where to set the </param>
        /// <param name="itemAction">An action to allow further updating to each item.</param>
        /// <returns>The corresponding collection.</returns>
        public IEnumerable<T> Convert<T>(string name, bool replaceAllShorthandGuids = true, bool initializeCreatedChangeLog = true, bool initializeReferenceData = true, Action<JObject, int, T?>? itemAction = null) where T : class, new()
        {
            Check.NotEmpty(name, nameof(name));

            var json = _json?.Children().OfType<JObject>()?.Children().OfType<JProperty>().Where(x => x.Name == name)?.FirstOrDefault()?.Value;
            if (json == null || json.Type != JTokenType.Array)
                return default!;

            if (replaceAllShorthandGuids)
                ReplaceAllShorthandGuids(json);

            var list = new List<T>();
            foreach (var j in json.Children<JObject>())
            {
                var val = j.ToObject<T>();
                if (initializeCreatedChangeLog && val is IChangeLog cl)
                {
                    if (cl.ChangeLog == null)
                        cl.ChangeLog = new ChangeLog();

                    cl.ChangeLog.CreatedBy = DefaultCreatedBy;
                    cl.ChangeLog.CreatedDate = DefaultCreatedDate;
                }

                if (initializeReferenceData && val is ReferenceDataBase rdb)
                    ReferenceDataInitializer(j, list.Count, rdb);

                itemAction?.Invoke(j, list.Count, val);

                list.Add(val!);
            }

            return list.AsEnumerable();
        }

        /// <summary>
        /// Recursively replaces all '^n' values where n is an integer with n.ToGuid() equivalent.
        /// </summary>
        private void ReplaceAllShorthandGuids(JToken json)
        {
            foreach (var j in json.Children())
            {
                if (j.Type == JTokenType.String)
                {
                    var jv = (JValue)j;
                    var s = jv.Value as string;
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("^", StringComparison.InvariantCultureIgnoreCase) && int.TryParse(s[1..], out var i))
                        jv.Value = new Guid(i, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                }
                else
                    ReplaceAllShorthandGuids(j);
            }
        }

        /// <summary>
        /// Initialize reference data properties.
        /// </summary>
        private static void ReferenceDataInitializer(JObject json, int index, ReferenceDataBase value)
        {
            if (value.Code == null && json.Children().Count() == 1)
            {
                var jp = json.Children().OfType<JProperty>().First();
                if (jp != null)
                {
                    value.Code = jp.Name;
                    value.Text = jp.Value.ToString();
                }
            }

            switch (value)
            {
                case ReferenceDataBaseGuid rdg:
                    if (rdg.Id == Guid.Empty)
                        rdg.Id = Guid.NewGuid();

                    break;

                case ReferenceDataBaseInt32 rdi:
                    if (rdi.Id == 0)
                        rdi.Id = index + 1;

                    break;

                case ReferenceDataBaseInt64 rdl:
                    if (rdl.Id == 0)
                        rdl.Id = index + 1;

                    break;

                case ReferenceDataBaseString rds:
                    if (rds.Id == null)
                        rds.Id = Guid.NewGuid().ToString();

                    break;
            }

            if (value.SortOrder == 0)
                value.SortOrder = index + 1;
        }
    }
}