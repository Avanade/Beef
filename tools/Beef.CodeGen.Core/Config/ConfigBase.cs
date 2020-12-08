// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Globalization;

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

        /// <summary>
        /// Validate (extend) the configuration.
        /// </summary>
        /// <param name="args">The <see cref="ConfigValidatorArgs"/>.</param>
        void Validate(ConfigValidatorArgs args);
    }

    /// <summary>
    /// Provides base configuration capabilities.
    /// </summary>
    public abstract class ConfigBase
    {
        #region static

        /// <summary>
        /// The <b>Yes</b> option.
        /// </summary>
        public const string YesOption = "Yes";

        /// <summary>
        /// The <b>No</b> option.
        /// </summary>
        public const string NoOption = "No";

        /// <summary>
        /// Gets the <see cref="YesOption"/> and <see cref="NoOption"/> options.
        /// </summary>
        public static readonly string[] YesNoOptions = new string[] { "No", "Yes" };

        /// <summary>
        /// The list of standard system <see cref="Type"/> names.
        /// </summary>
        public static List<string> SystemTypes => new List<string>
        {
            "void", "bool", "byte", "char", "decimal", "double", "float", "int", "long",
            "sbyte", "short", "unit", "ulong", "ushort", "string", "DateTime", "TimeSpan", "Guid"
        };

        /// <summary>
        /// The list of system <see cref="Type"/> names that should not be nullable by default.
        /// </summary>
        public static List<string> IgnoreNullableTypes => new List<string>
        {
            "bool", "byte", "char", "decimal", "double", "float", "int", "long",
            "sbyte", "short", "unit", "ulong", "ushort", "DateTime", "TimeSpan", "Guid"
        };

        /// <summary>
        /// Checks whether the <see cref="string"/> value is <see cref="NoOption"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        protected static bool IsNoOption(string? value) => string.IsNullOrEmpty(value) || string.Compare(value, NoOption, StringComparison.OrdinalIgnoreCase) == 0;

        /// <summary>
        /// Checks whether the <see cref="string"/> value is <see cref="YesOption"/>.
        /// </summary>
        /// <param name="value">The value.</param>
        protected static bool IsYesOption(string? value) => !string.IsNullOrEmpty(value) && string.Compare(value, YesOption, StringComparison.OrdinalIgnoreCase) == 0;

        /// <summary>
        /// Defaults the <see cref="string"/> <paramref name="value"/> where <c>null</c> using the <paramref name="defaultValue"/> function.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value function.</param>
        public static string? DefaultWhereNull(string? value, Func<string?> defaultValue) => string.IsNullOrEmpty(value) ? Check.NotNull(defaultValue, nameof(defaultValue))() : value;

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

        #endregion

        /// <summary>
        /// Prepares the configuration properties in advance of the code-generation execution (Internal use!).
        /// </summary>
        /// <param name="root">The root <see cref="ConfigBase"/>.</param>
        /// <param name="parent">The parent <see cref="ConfigBase"/>.</param>
        protected internal abstract void Prepare(object root, object parent);

        /// <summary>
        /// Validate (extend) the configuration.
        /// </summary>
        /// <param name="args">The <see cref="ConfigValidatorArgs"/>.</param>
        /// <remarks>The validation occurs after all other validations have occured (properties and nested collections).</remarks>
        public virtual void Validate(ConfigValidatorArgs args) { }
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
        public TRoot? Root { get; private set; }

        /// <summary>
        /// Gets the <b>Parent</b> (set via <see cref="Prepare(TRoot, TParent)"/> execution).
        /// </summary>
        public TParent? Parent { get; private set; }

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
            Root = root;
            Parent = parent;
            Prepare();
        }

        /// <summary>
        /// Performs the actual prepare logic.
        /// </summary>
        protected abstract void Prepare();
    }
}