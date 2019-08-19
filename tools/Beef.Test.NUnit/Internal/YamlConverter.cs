// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;

namespace Beef.Test.NUnit.Internal
{
    /// <summary>
    /// Provides the capabilitity to convert <b>YAML</b> (or <b>JSON</b>) into a typed collection.
    /// </summary>
    public class YamlConverter
    {
        private readonly JObject _json;
        
        /// <summary>
        /// Gets or sets the default <see cref="ChangeLog.CreatedDate"/> value.
        /// </summary>
        public static DateTime DefaultCreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the default <see cref="ChangeLog.CreatedBy"/> value.
        /// </summary>
        public static string DefaultCreatedBy { get; set; } = Environment.UserDomainName == null ? Environment.UserName : Environment.UserDomainName + "\\" + Environment.UserName;

        /// <summary>
        /// Reads and parses the YAML <see cref="string"/>.
        /// </summary>
        /// <param name="yaml">The YAML <see cref="string"/>.</param>
        /// <returns>The <see cref="YamlConverter"/>.</returns>
        public static YamlConverter ReadYaml(string yaml)
        {
            return ReadYaml(new StringReader(yaml));
        }

        /// <summary>
        /// Reads and parses the YAML <see cref="Stream"/>.
        /// </summary>
        /// <param name="s">The YAML <see cref="Stream"/>.</param>
        /// <returns>The <see cref="YamlConverter"/>.</returns>
        public static YamlConverter ReadYaml(Stream s)
        {
            return ReadYaml(new StreamReader(s));
        }

        /// <summary>
        /// Reads and parses the YAML <see cref="TextReader"/>.
        /// </summary>
        /// <param name="tr">The YAML <see cref="TextReader"/>.</param>
        /// <returns>The <see cref="YamlConverter"/>.</returns>
        public static YamlConverter ReadYaml(TextReader tr)
        {
            var yaml = new DeserializerBuilder().Build().Deserialize(tr);
            var json = new SerializerBuilder().JsonCompatible().Build().Serialize(yaml);
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
            _json = json;
        }

        /// <summary>
        /// Converts the injested data into a collection of the specified <see cref="Type"/>.
        /// </summary>
        /// <param name="name">The name of the node that constains the items.</param>
        /// <param name="replaceAllShorthandGuids">Indicates whether to recursively replaces all '^n' values where 'n' is an integer with n.ToGuid() equivalent.</param>
        /// <param name="setCreatedChangeLog">Indicates whether to set the <see cref="ChangeLog.CreatedBy"/> and <see cref="ChangeLog.CreatedDate"/> where <typeparamref name="T"/> implements <see cref="IChangeLog"/>.</param>
        /// <param name="itemAction">An action to allow further updating to each item.</param>
        /// <returns>The corresponding collection.</returns>
        public IEnumerable<T> Convert<T>(string name, bool replaceAllShorthandGuids = true, bool setCreatedChangeLog = true, Action<T> itemAction = null) where T : class, new()
        {
            Check.NotEmpty(name, nameof(name));
            var json = _json?.Descendants().OfType<JProperty>().Where(x => x.Name == name).FirstOrDefault()?.Value;
            if (json == null || json.Ancestors().Count() != 4 || json.Type != JTokenType.Array)
                return default;

            if (replaceAllShorthandGuids)
                ReplaceAllShorthandGuids(json);

            var list = new List<T>();
            foreach (var j in json.Children<JObject>())
            {
                var val = j.ToObject<T>();
                if (setCreatedChangeLog && val is IChangeLog cl)
                {
                    if (cl.ChangeLog == null)
                        cl.ChangeLog = new ChangeLog();

                    cl.ChangeLog.CreatedBy = DefaultCreatedBy;
                    cl.ChangeLog.CreatedDate = DefaultCreatedDate;
                }

                itemAction?.Invoke(val);

                list.Add(val);
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
                    if (!string.IsNullOrEmpty(s) && s.StartsWith("^") && int.TryParse(s.Substring(1), out var i))
                        jv.Value = i.ToGuid();
                }
                else
                    ReplaceAllShorthandGuids(j);
            }
        }
    }
}
