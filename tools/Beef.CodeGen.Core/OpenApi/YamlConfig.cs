using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Linq;

namespace Beef.CodeGen.OpenApi
{
    /// <summary>
    /// Provides the base YAML configuration.
    /// </summary>
    public abstract class YamlBase(string? originalName, string name)
    {
        /// <summary>
        /// Gets the original name.
        /// </summary>
        public string? OriginalName { get; } = originalName;

        /// <summary>
        /// Gets the .NET name.
        /// </summary>
        public string? Name { get; internal set; } = name;

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        public OrderedDictionary Attributes { get; } = [];

        /// <summary>
        /// Adds the attribute to the entity where the <paramref name="value"/> is not its default value (unless <paramref name="allowDefault"/> is <c>true</c>).
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="allowDefault">Indicates whether to allow the default value to be added.</param>
        /// <param name="defaults">Options defaults to potentially skip.</param>
        /// <remarks>Where the attribute with the specified <paramref name="key"/> already exists then no action will occur; i.e. will not override.</remarks>
        public void AddAttribute<T>(string key, T? value, bool allowDefault = false, params T[] defaults)
        {
            if (Attributes.Contains(key))
                return;

            if (value is null)
            {
                if (allowDefault)
                    Attributes.Add(key, "null");

                return;
            }

            if (value is string s)
            {
                if (string.IsNullOrWhiteSpace(s) && !allowDefault)
                    return;

                if (defaults.Contains(value) && !allowDefault)
                    return;

                Attributes.Add(key, value);
            }
            else if (value is bool b)
            {
                if (!b && !allowDefault)
                    return;

                Attributes.Add(key, b.ToString().ToLowerInvariant());
            }
            else
                throw new NotSupportedException($"The value type '{value?.GetType().Name}' is not supported.");
        }

        /// <summary>
        /// Gets the summary text (if available).
        /// </summary>
        public string? SummaryText => Attributes["text"]?.ToString()?[2..^1];
    }

    /// <summary>
    /// Represents the YAML root configuration.
    /// </summary>
    public class YamlConfig(string? originalName, string name, OpenApiArgs args) : YamlBase(originalName, name)
    {
        /// <summary>
        /// Gets the <see cref="OpenApiArgs"/>.
        /// </summary>
        public OpenApiArgs Args { get; } = args;

        /// <summary>
        /// Gets the entities.
        /// </summary>
        public List<YamlEntity> Entities { get; } = [];

        /// <summary>
        /// Gets the enums.
        /// </summary>
        public List<YamlEnum> Enums { get; } = [];
    }

    /// <summary>
    /// Represents the YAML entity.
    /// </summary>
    public class YamlEntity(string? originalName, string name) : YamlBase(originalName, name)
    {
        /// <summary>
        /// Gets or sets the JSON V3 representation to enable uniqueness comparison.
        /// </summary>
        internal string? Json { get; set; }

        /// <summary>
        /// Gets the properties.
        /// </summary>
        public List<YamlProperty> Properties { get; } = [];

        /// <summary>
        /// Gets the operations.
        /// </summary>
        public List<YamlOperation> Operations { get; } = [];
    }

    /// <summary>
    /// Represents the YAML property.
    /// </summary>
    public class YamlProperty(string? originalName, string name) : YamlBase(originalName, name) { }

    /// <summary>
    /// Represents the YAML operation.
    /// </summary>
    public class YamlOperation(string? originalName, string name) : YamlBase(originalName, name)
    {
        /// <summary>
        /// Gets the parameters.
        /// </summary>
        public List<YamlParameter> Parameters { get; } = [];
    }

    /// <summary>
    /// Represents the YAML parameter.
    /// </summary>
    public class YamlParameter(string? originalName, string name) : YamlBase(originalName, name) { }

    /// <summary>
    /// Represents the YAML enum.
    /// </summary>
    public class YamlEnum(string? originalName, string name) : YamlBase(originalName, name)
    {
        /// <summary>
        /// Gets the enum values.
        /// </summary>
        public List<YamlEnumValue> Values { get; } = [];
    }

    /// <summary>
    /// Represents the YAML enum value.
    /// </summary>
    public class YamlEnumValue(string? originalName, string name) : YamlBase(originalName, name) { }
}