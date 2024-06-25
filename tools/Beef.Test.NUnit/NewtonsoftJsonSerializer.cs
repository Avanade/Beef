// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.IO;
using UnitTestEx.Json;
using Nsj = Newtonsoft.Json;

namespace Beef.Test.NUnit
{
    /// <summary>
    /// Provides the <see cref="Nsj.JsonSerializer"/> encapsulated implementation.
    /// </summary>
    public class NewtonsoftJsonSerializer : IJsonSerializer
    {
        /// <summary>
        /// Gets or sets the default <see cref="Nsj.JsonSerializerSettings"/>.
        /// </summary>
        public static Nsj.JsonSerializerSettings DefaultSettings { get; set; } = new Nsj.JsonSerializerSettings
        {
            DefaultValueHandling = Nsj.DefaultValueHandling.Ignore,
            NullValueHandling = Nsj.NullValueHandling.Ignore,
            Formatting = Nsj.Formatting.None,
            Converters = { new Nsj.Converters.StringEnumConverter() },
            ContractResolver = new Nsj.Serialization.CamelCasePropertyNamesContractResolver(),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="NewtonsoftJsonSerializer"/> class.
        /// </summary>
        public NewtonsoftJsonSerializer() => Settings = DefaultSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="NewtonsoftJsonSerializer"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="Nsj.JsonSerializerSettings"/>. Defaults to <see cref="DefaultSettings"/>.</param>
        public NewtonsoftJsonSerializer(Nsj.JsonSerializerSettings? settings = null) => Settings = settings ?? DefaultSettings;

        /// <inheritdoc/>
        object IJsonSerializer.Options => Settings;

        /// <summary>
        /// Gets the <see cref="Nsj.JsonSerializerSettings"/>.
        /// </summary>
        public Nsj.JsonSerializerSettings Settings { get; }

        /// <inheritdoc/>
        public object? Deserialize(string json) => Deserialize<dynamic>(json);

        /// <inheritdoc/>
        public object? Deserialize(string json, Type type)
        {
            using var sr = new StringReader(json);
            using var jtr = new Nsj.JsonTextReader(sr);
            return Nsj.JsonSerializer.Create(Settings).Deserialize(jtr, type);
        }

        /// <inheritdoc/>
        public T? Deserialize<T>(string json) => (T?)Deserialize(json, typeof(T));

        /// <inheritdoc/>
        public string Serialize<T>(T value, JsonWriteFormat? format = null) => Nsj.JsonConvert.SerializeObject(value, format is null ? Settings : CopySettings(format.Value));

        /// <summary>
        /// Copies the settings.
        /// </summary>
        private Nsj.JsonSerializerSettings CopySettings(JsonWriteFormat format)
        {
            var s = new Nsj.JsonSerializerSettings
            {
                ReferenceLoopHandling = Settings.ReferenceLoopHandling,
                MissingMemberHandling = Settings.MissingMemberHandling,
                ObjectCreationHandling = Settings.ObjectCreationHandling,
                NullValueHandling = Settings.NullValueHandling,
                DefaultValueHandling = Settings.DefaultValueHandling,
                PreserveReferencesHandling = Settings.PreserveReferencesHandling,
                TypeNameHandling = Settings.TypeNameHandling,
                MetadataPropertyHandling = Settings.MetadataPropertyHandling,
                TypeNameAssemblyFormatHandling = Settings.TypeNameAssemblyFormatHandling,
                ConstructorHandling = Settings.ConstructorHandling,
                ContractResolver = Settings.ContractResolver,
                EqualityComparer = Settings.EqualityComparer,
                ReferenceResolverProvider = Settings.ReferenceResolverProvider,
                TraceWriter = Settings.TraceWriter,
                SerializationBinder = Settings.SerializationBinder,
                Error = Settings.Error,
                Context = Settings.Context,
                DateFormatString = Settings.DateFormatString,
                MaxDepth = Settings.MaxDepth,
                Formatting = format == JsonWriteFormat.None ? Nsj.Formatting.None : Nsj.Formatting.Indented,
                DateFormatHandling = Settings.DateFormatHandling,
                DateTimeZoneHandling = Settings.DateTimeZoneHandling,
                DateParseHandling = Settings.DateParseHandling,
                FloatFormatHandling = Settings.FloatFormatHandling,
                FloatParseHandling = Settings.FloatParseHandling,
                StringEscapeHandling = Settings.StringEscapeHandling,
                Culture = Settings.Culture,
                CheckAdditionalContent = Settings.CheckAdditionalContent,
            };

            if (Settings.Converters != null)
                s.Converters = new List<Nsj.JsonConverter>(Settings.Converters);

            return s;
        }
    }
}