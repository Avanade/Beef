// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Beef.CodeGen.Config
{
    /// <summary>
    /// Represents <see cref="ConfigValidator"/> arguments.
    /// </summary>
    public class ConfigValidatorArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigValidatorArgs"/> class.
        /// </summary>
        /// <param name="root">The root path.</param>
        /// <param name="key">Optional key to append to path.</param>
        internal ConfigValidatorArgs(string? root = null, string? key = null) => Root = root + key;

        /// <summary>
        /// Gets the root path.
        /// </summary>
        public string? Root { get; }

        /// <summary>
        /// Throws an error for the specified property name.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="message">The error message,</param>
        public void ThrowError(string name, string message) => throw new CodeGenException($"{(string.IsNullOrEmpty(Root) ? "" : Root + ".")}{name}: {message}");
    }

    /// <summary>
    /// Validates a <see cref="ConfigBase"/> to ensure it is valid prior to code-generation.
    /// </summary>
    public static class ConfigValidator
    {
        /// <summary>
        /// Validates the specified <see cref="ConfigBase"/>.
        /// </summary>
        /// <param name="config">The <see cref="ConfigBase"/> to validate.</param>
        public static void Validate(ConfigBase config) => Validate(new ConfigValidatorArgs(), Check.NotNull(config, nameof(config)));

        /// <summary>
        /// Perform the validation for each property.
        /// </summary>
        private static void Validate(ConfigValidatorArgs args, ConfigBase config)
        {
            foreach (var pi in config.GetType().GetProperties())
            {
                var jpa = pi.GetCustomAttribute<JsonPropertyAttribute>();
                if (jpa == null)
                    continue;

                var name = jpa.PropertyName ?? StringConversion.ToCamelCase(pi.Name)!;
                var psa = pi.GetCustomAttribute<PropertySchemaAttribute>();

                if (psa == null)
                {
                    var pcsa = pi.GetCustomAttribute<PropertyCollectionSchemaAttribute>();
                    if (pcsa == null)
                        continue;

                    if (pi.GetValue(config) is IEnumerable<ConfigBase> coll)
                    {
                        foreach (var item in coll)
                        {
                            var key = GetConfigKey(args, item);
                            Validate(new ConfigValidatorArgs(string.IsNullOrEmpty(args.Root) ? name : $"{args.Root}.{name}", key), item);
                        }
                    }

                    continue;
                }

                if (psa.Options != null && psa.Options.Length > 0 && pi.GetValue(config) is string val && !psa.Options.Contains(val))
                    args.ThrowError(name, $"Value '{val}' is invalid; valid values are: {string.Join(", ", psa.Options)}.");
            }

            config.Validate(args);
        }

        /// <summary>
        /// Gets the config key.
        /// </summary>
        private static string? GetConfigKey(ConfigValidatorArgs args, ConfigBase config)
        {
            string? key = null;
            foreach (var pi in config.GetType().GetProperties())
            {
                var psa = pi.GetCustomAttribute<PropertySchemaAttribute>();
                if (psa != null && psa.IsMandatory)
                {
                    var jpa = pi.GetCustomAttribute<JsonPropertyAttribute>()!;
                    var name = jpa.PropertyName ?? StringConversion.ToCamelCase(pi.Name)!;
                    string? val = pi.GetValue(config) as string;
                    if (val == null)
                        args.ThrowError(name, "Value is mandatory.");

                    key = $"({name}={val})";
                    break;
                }
            }

            return key;
        }
    }
}