using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Beef.CodeGen.OpenApi
{
    /// <summary>
    /// Provides the base YAML configuration.
    /// </summary>
    public abstract class YamlBase(string name)
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        public string? Name { get; } = name;

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
    }

    /// <summary>
    /// Represents the YAML root configuration.
    /// </summary>
    public class YamlConfig(string name, OpenApiArgs args) : YamlBase(name)
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
    public class YamlEntity(string name) : YamlBase(name)
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
    public class YamlProperty(string name) : YamlBase(name) { }

    /// <summary>
    /// Represents the YAML operation.
    /// </summary>
    public class YamlOperation(string name) : YamlBase(name)
    {
        /// <summary>
        /// Gets the parameters.
        /// </summary>
        public List<YamlParameter> Parameters { get; } = [];
    }

    /// <summary>
    /// Represents the YAML parameter.
    /// </summary>
    public class YamlParameter(string name) : YamlBase(name) { }

    /// <summary>
    /// Represents the YAML enum.
    /// </summary>
    public class YamlEnum(string name) : YamlBase(name)
    {
        /// <summary>
        /// Gets the enum values.
        /// </summary>
        public List<string> Values { get; } = [];
    }
}