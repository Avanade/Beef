// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Beef.CodeGen.Config
{
    /// <summary>
    /// Enables the root configuration capabilities.
    /// </summary>
    public interface IRootConfig
    {
        /// <summary>
        /// Gets the parameter overrides.
        /// </summary>
        Dictionary<string, string> RuntimeParameters { get; }

        /// <summary>
        /// Replaces the <see cref="RuntimeParameters"/> with the specified <paramref name="parameters"/> (copies values).
        /// </summary>
        /// <param name="parameters">The parameters to copy.</param>
        void ReplaceRuntimeParameters(Dictionary<string, string> parameters);

        /// <summary>
        /// Resets the runtime parameters.
        /// </summary>
        void ResetRuntimeParameters();
    }

    /// <summary>
    /// Provides base configuration capabilities.
    /// </summary>
    public abstract class ConfigBase
    {
        #region static

        /// <summary>
        /// The list of standard system <see cref="Type"/> names.
        /// </summary>
        public static List<string> SystemTypes => new List<string>
        {
            "void", "bool", "byte", "char", "decimal", "double", "float", "int", "long",
            "sbyte", "short", "unit", "ulong", "ushort", "string", "DateTime", "DateTimeOffset", "TimeSpan", "Guid"
        };

        /// <summary>
        /// The list of system <see cref="Type"/> names that should not be nullable by default.
        /// </summary>
        public static List<string> IgnoreNullableTypes => new List<string>
        {
            "bool", "byte", "char", "decimal", "double", "float", "int", "long",
            "sbyte", "short", "unit", "ulong", "ushort", "DateTime", "DateTimeOffset", "TimeSpan", "Guid"
        };

        /// <summary>
        /// Check whether the nullable boolean is true.
        /// </summary>
        protected static bool IsTrue(bool? value) => value.HasValue && value.Value;

        /// <summary>
        /// Check whether the nullable boolean is null or false.
        /// </summary>
        protected static bool IsFalse(bool? value) => !value.HasValue || !value.Value;

        /// <summary>
        /// Defaults the <see cref="string"/> <paramref name="value"/> where <c>null</c> using the <paramref name="defaultValue"/> function.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value function.</param>
        public static string? DefaultWhereNull(string? value, Func<string?> defaultValue) => value ?? Check.NotNull(defaultValue, nameof(defaultValue))();

        /// <summary>
        /// Defaults the <see cref="bool"/> <paramref name="value"/> where <c>null</c> using the <paramref name="defaultValue"/> function.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value function.</param>
        public static bool? DefaultWhereNull(bool? value, Func<bool?> defaultValue) => value.HasValue ? value : Check.NotNull(defaultValue, nameof(defaultValue))();

        /// <summary>
        /// Compares the <see cref="string"/> <paramref name="propertyValue"/> and <paramref name="compareTo"/> for equality, or whether <paramref name="propertyValue"/> is <c>null</c>.
        /// </summary>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="compareTo">The value to compare to.</param>
        /// <returns><c>true</c> where equal or <paramref name="propertyValue"/> is <c>null</c>; otherwise, <c>false</c>.</returns>
        public static bool CompareNullOrValue(string? propertyValue, string compareTo) => propertyValue == null || propertyValue == compareTo;

        /// <summary>
        /// Compares the <see cref="bool"/> <paramref name="propertyValue"/> and <paramref name="compareTo"/> for equality, or whether <paramref name="propertyValue"/> is <c>null</c>.
        /// </summary>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="compareTo">The value to compare to.</param>
        /// <returns><c>true</c> where equal or <paramref name="propertyValue"/> is <c>null</c>; otherwise, <c>false</c>.</returns>
        public static bool CompareNullOrValue(bool? propertyValue, bool compareTo) => propertyValue == null || propertyValue == compareTo;

        /// <summary>
        /// Compares the <see cref="string"/> <paramref name="propertyValue"/> and <paramref name="compareTo"/> for equality.
        /// </summary>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="compareTo">The value to compare to.</param>
        /// <returns><c>true</c> where equal; otherwise, <c>false</c>.</returns>
        public static bool CompareValue(string? propertyValue, string compareTo) => propertyValue != null && propertyValue == compareTo;

        /// <summary>
        /// Compares the <see cref="bool"/> <paramref name="propertyValue"/> and <paramref name="compareTo"/> for equality.
        /// </summary>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="compareTo">The value to compare to.</param>
        /// <returns><c>true</c> where equal; otherwise, <c>false</c>.</returns>
        public static bool CompareValue(bool? propertyValue, bool compareTo) => propertyValue != null && propertyValue == compareTo;

        /// <summary>
        /// Converts <paramref name="text"/> to c# 'see cref=' Comments ('List&lt;int&gt;' would become 'List{int}' respectively). 
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The converted text.</returns>
        private static string? ReplaceGenericsBracketWithCommentsBracket(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var s = text.Replace("<", "{", StringComparison.InvariantCulture);
            s = s.Replace(">", "}", StringComparison.InvariantCulture);
            return s;
        }

        /// <summary>
        /// Converts <paramref name="text"/> to c# Comments ('{{xyx}}' would become 'see cref=' XML, and any &lt;&gt; within the xyz would become {} respectively). 
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The converted text.</returns>
        public static string? ToComments(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var s = text;
            while (true)
            {
                var start = s.IndexOf("{{", StringComparison.InvariantCultureIgnoreCase);
                var end = s.IndexOf("}}", StringComparison.InvariantCultureIgnoreCase);

                if (start < 0 && end < 0)
                    break;

                if (start < 0 || end < 0 || end < start)
                    throw new CodeGenException("Start and End {{ }} parameter mismatch.", text);

                string sub = s.Substring(start, end - start + 2);
                string? mid = ReplaceGenericsBracketWithCommentsBracket(sub[2..^2]);

                s = s.Replace(sub, string.Format(CultureInfo.InvariantCulture, "<see cref=\"{0}\"/>", mid), StringComparison.InvariantCulture);
            }

            return s;
        }

        /// <summary>
        /// Converts <paramref name="text"/> to c# 'see cref=' comments ('List&lt;int&gt;' would become '&lt;see cref="List{int}/&gt;' respectively). 
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The converted text.</returns>
        public static string? ToSeeComments(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return $"<see cref=\"{ReplaceGenericsBracketWithCommentsBracket(text)}\"/>";
        }

        /// <summary>
        /// Build the standardized qualified key name.
        /// </summary>
        /// <param name="configName">The <see cref="ConfigBase"/> name.</param>
        /// <param name="keyValue">The key value.</param>
        /// <param name="keyName">The corresponding key name; defaults to 'Name'.</param>
        /// <returns></returns>
        protected static string BuildQualifiedKeyName(string configName, string? keyValue, string keyName = "Name")
            => $"{configName}({keyName}='{(string.IsNullOrEmpty(keyValue) ? "<not specified>" : keyValue)}')";

        #endregion

        /// <summary>
        /// Gets the <b>Root</b> configuration.
        /// </summary>
        protected internal ConfigBase? RootConfig { get; set; }

        /// <summary>
        /// Gets the <b>Parent</b> configuration.
        /// </summary>
        protected internal ConfigBase? ParentConfig { get; set; }

        /// <summary>
        /// Gets the qualified key name for the configuration.
        /// </summary>
        /// <remarks>Used in error messages to assist navigation within configuration.</remarks>
        public virtual string? QualifiedKeyName { get; } = null;

        /// <summary>
        /// Build the fully qualified name for the specified <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns>The fully qualified name.</returns>
        public string? BuildFullyQualifiedName(string propertyName)
        {
            var hier = new List<string?> { propertyName };
            ConfigBase? cb = this;
            while (true)
            {
                hier.Add(cb.QualifiedKeyName);
                if (cb.ParentConfig == null || cb == cb.ParentConfig)
                    break;

                cb = cb.ParentConfig;
            }

            var sb = new StringBuilder();
            foreach (var qn in hier.Reverse<string?>())
            {
                if (!string.IsNullOrEmpty(qn))
                {
                    if (sb.Length > 0)
                        sb.Append('.');

                    sb.Append(qn);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Check that the key has a value and throw an error where not specified.
        /// </summary>
        /// <param name="keyValue">The key value.</param>
        /// <param name="keyName">The corresponding key name; defaults to 'Name'.</param>
        protected void CheckKeyHasValue(string? keyValue, string keyName = "Name")
        {
            if (string.IsNullOrEmpty(keyValue))
                throw new CodeGenException(this, keyName, "Value is mandatory.");
        }

        /// <summary>
        /// Checks all properties that use the <see cref="PropertySchemaAttribute.Options"/> to ensure the value is considered valid.
        /// </summary>
        protected void CheckOptionsProperties()
        {
            foreach (var pi in GetType().GetProperties())
            {
                var psa = pi.GetCustomAttribute<PropertySchemaAttribute>();
                if (psa != null && psa.Options != null && psa.Options.Length > 0 && pi.GetValue(this) is string val && !psa.Options.Contains(val))
                    throw new CodeGenException(this, pi.Name, $"Value '{val}' is invalid; valid values are: {string.Join(", ", psa.Options)}.");
            }
        }

        /// <summary>
        /// Prepares the configuration properties in advance of the code-generation execution (Internal use!).
        /// </summary>
        /// <param name="root">The root <see cref="ConfigBase"/>.</param>
        /// <param name="parent">The parent <see cref="ConfigBase"/>.</param>
        protected internal abstract void Prepare(object root, object parent);

        /// <summary>
        /// Gets or sets the <see cref="Dictionary{TKey, TValue}"/> that houses any additional/extra properties/attributes deserialized within the configuration.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JToken>? ExtraProperties { get; set; }

        /// <summary>
        /// Gets the property value from <see cref="ExtraProperties"/> using the specified <paramref name="key"/> as <see cref="Type"/> <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The property <see cref="Type"/>.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value where the property is not found.</param>
        /// <returns>The value.</returns>
        public T GetExtraProperty<T>(string key, T defaultValue = default!)
        {
            if (ExtraProperties != null && ExtraProperties.TryGetValue(key, out var val))
                return (T)Convert.ChangeType(val.ToString(), typeof(T));
            else
                return defaultValue!;
        }

        /// <summary>
        /// Trys to get the property value from <see cref="ExtraProperties"/> using the specified <paramref name="key"/> as <see cref="Type"/> <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The property <see cref="Type"/>.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The corresponding value.</param>
        /// <returns><c>true</c> if the <paramref name="key"/> is found; otherwise, <c>false</c>.</returns>
        public bool TryGetExtraProperty<T>(string key, out T value)
        {
            if (ExtraProperties != null && ExtraProperties.TryGetValue(key, out var val))
            {
                value = (T)Convert.ChangeType(val.ToString(), typeof(T));
                return true;
            }
            else
            {
                value = default!;
                return false;
            }
        }

        /// <summary>
        /// Gets the <see cref="Dictionary{TKey, TValue}"/> that allows for custom property values to be manipulated at runtime.
        /// </summary>
        [JsonIgnore]
        public Dictionary<string, object> CustomProperties { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets the property value from <see cref="CustomProperties"/> using the specified <paramref name="key"/> as <see cref="Type"/> <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The property <see cref="Type"/>.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">The default value where the property is not found.</param>
        /// <returns>The value.</returns>
        public T GetCustomProperty<T>(string key, T defaultValue = default!)
        {
            if (CustomProperties != null && CustomProperties.TryGetValue(key, out var val))
                return (T)Convert.ChangeType(val, typeof(T));
            else
                return defaultValue!;
        }

        /// <summary>
        /// Trys to get the property value from <see cref="CustomProperties"/> using the specified <paramref name="key"/> as <see cref="Type"/> <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The property <see cref="Type"/>.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The corresponding value.</param>
        /// <returns><c>true</c> if the <paramref name="key"/> is found; otherwise, <c>false</c>.</returns>
        public bool TryGetCustomProperty<T>(string key, out T value)
        {
            if (CustomProperties != null && CustomProperties.TryGetValue(key, out var val))
            {
                value = (T)Convert.ChangeType(val, typeof(T));
                return true;
            }
            else
            {
                value = default!;
                return false;
            }
        }

        /// <summary>
        /// Gets the <see cref="DateTime.UtcNow"/> as a formatted timestamp.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "By design.")]
        public string UtcDateTimeStamp => DateTime.UtcNow.ToString("yyyyMMdd-HHmmss", System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Provides the base <see cref="Prepare(TRoot, TParent)"/> configuration capabilities.
    /// </summary>
    /// <typeparam name="TRoot">The root <see cref="Type"/>.</typeparam>
    /// <typeparam name="TParent">The parent <see cref="Type"/>.</typeparam>
    public abstract class ConfigBase<TRoot, TParent> : ConfigBase where TRoot : ConfigBase where TParent : ConfigBase
    {
        /// <summary>
        /// Gets the <b>Root</b> (set via <see cref="Prepare(TRoot, TParent)"/> execution).
        /// </summary>
        public TRoot? Root { get; set; }

        /// <summary>
        /// Gets the <b>Parent</b> (set via <see cref="Prepare(TRoot, TParent)"/> execution).
        /// </summary>
        public TParent? Parent { get; set; }

        /// <summary>
        /// Prepares the configuration properties in advance of the code-generation execution (Internal use!).
        /// </summary>
        /// <param name="root">The root <see cref="ConfigBase"/>.</param>
        /// <param name="parent">The parent <see cref="ConfigBase"/>.</param>
        protected internal override void Prepare(object root, object parent) => Prepare((TRoot)root, (TParent)parent);

        /// <summary>
        /// Prepares the configuration properties in advance of the code-generation execution.
        /// </summary>
        /// <param name="root">The root <see cref="ConfigBase"/>.</param>
        /// <param name="parent">The parent <see cref="ConfigBase"/>.</param>
        public void Prepare(TRoot root, TParent parent)
        {
            RootConfig = Root = root;
            ParentConfig = Parent = parent;
            Prepare();
        }

        /// <summary>
        /// Performs the actual prepare logic.
        /// </summary>
        protected abstract void Prepare();
    }
}