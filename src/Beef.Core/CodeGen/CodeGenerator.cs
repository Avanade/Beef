// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Beef.CodeGen
{
    /// <summary>
    /// The code generator.
    /// </summary>
    public class CodeGenerator
    {
        private static Func<string, string>? _pluralizer;

        /// <summary>
        /// The <see cref="Regex"/> expression pattern for split strings into words.
        /// </summary>
        public const string WordSplitPattern = "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))";

        /// <summary>
        /// The reserved system <see cref="ICodeGenConfigLoader"/> name.
        /// </summary>
        public const string SystemConfigName = "System";

        /// <summary>
        /// Gets or sets the <see cref="ToPlural"/> function.
        /// </summary>
        /// <param name="pluralizer">The pluralizer.</param>
        public static void SetPluralizer(Func<string, string> pluralizer)
        {
            _pluralizer = pluralizer ?? throw new ArgumentNullException(nameof(pluralizer));
        }

        /// <summary>
        /// Pluralizes the <paramref name="text"/>.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The pluralized text.</returns>
        public static string? ToPlural(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (_pluralizer == null)
                throw new InvalidOperationException("SetPluralizer must be used to set the Pluralizer before it can be invoked.");

            return _pluralizer(text);
        }

        /// <summary>
        /// Converts <paramref name="text"/> to camelCase (e.g. 'SomeValue' would return 'someValue').
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The converted text.</returns>
        public static string? ToCamelCase(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (text.StartsWith("OData", StringComparison.InvariantCultureIgnoreCase))
                return $"{char.ToLower(text[0], CultureInfo.InvariantCulture)}{char.ToLower(text[1], CultureInfo.InvariantCulture)}{text.Substring(2)}";
            else
                return char.ToLower(text[0], CultureInfo.InvariantCulture) + text.Substring(1);
        }

        /// <summary>
        /// Converts <paramref name="text"/> to _camelCase (e.g. 'SomeValue' would return '_someValue').
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The converted text.</returns>
        public static string? ToPrivateCase(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return "_" + ToCamelCase(text);
        }

        /// <summary>
        /// Converts <paramref name="text"/> to PascalCase (e.g. 'someValue' would return 'SomeValue').
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The converted text.</returns>
        public static string? ToPascalCase(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            if (text.StartsWith("OData", StringComparison.InvariantCultureIgnoreCase))
                return $"{char.ToUpper(text[0], CultureInfo.InvariantCulture)}{char.ToUpper(text[1], CultureInfo.InvariantCulture)}{text.Substring(2)}";
            else
                return char.ToUpper(text[0], CultureInfo.InvariantCulture) + text.Substring(1);
        }

        /// <summary>
        /// Converts <paramref name="text"/> to a Sentence Case ('someValueXML' would return 'Some Value XML'); splits on capitals and attempts to keep acronyms.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The converted text.</returns>
        public static string? ToSentenceCase(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var s = Regex.Replace(text, WordSplitPattern, "$1 "); // Split the string into words.
            s = SpecialCaseHandling(s);
            return char.ToUpper(s[0], CultureInfo.InvariantCulture) + s.Substring(1); // Make sure the first character is always upper case.
        }

        /// <summary>
        /// Converts <paramref name="text"/> to a Snake Case ('someValueXML' would return 'some_value_xml'); splits on capitals and attempts to keep acronyms.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The converted text.</returns>
        public static string? ToSnakeCase(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var s = Regex.Replace(text, WordSplitPattern, "$1 "); // Split the string into words.
            s = SpecialCaseHandling(s);
#pragma warning disable CA1308 // Normalize strings to uppercase; lowercase is correct!
            return s.Replace(" ", "_", StringComparison.InvariantCulture).ToLowerInvariant(); // Replace space with _ and make lowercase.
#pragma warning restore CA1308 
        }

        /// <summary>
        /// Converts <paramref name="text"/> to a Kebab Case ('someValueXML' would return 'some-value-xml'); splits on capitals and attempts to keep acronyms.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The converted text.</returns>
        public static string? ToKebabCase(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var s = Regex.Replace(text, WordSplitPattern, "$1 "); // Split the string into words.
            s = SpecialCaseHandling(s);
#pragma warning disable CA1308 // Normalize strings to uppercase; lowercase is correct!
            return s.Replace(" ", "-", StringComparison.InvariantCulture).ToLowerInvariant(); // Replace space with _ and make lowercase.
#pragma warning restore CA1308 
        }

        /// <summary>
        /// Special cas handling.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string SpecialCaseHandling(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var s = text.Replace("E Tag", "ETag", StringComparison.InvariantCulture); // Special case where we will put back together.
            return s.Replace("O Data", "OData", StringComparison.InvariantCulture); // Special case where we will put back together.
        }

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
        /// Converts a text to past tense (english).
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The converted text.</returns>
        public static string? ToPastTense(string? text)
        {
            if (string.IsNullOrEmpty(text) || text.Length < 3 || text.EndsWith("ed", StringComparison.InvariantCultureIgnoreCase))
                return text;

            // Ends with the letter e, then remove the final e and add the -ed suffix: tie->tied, like->liked, agree->agreed.
            if (text[^1] == 'e')
                return text + "d";

            // Ends with the letter y preceded by a consonant, then change the y to an i and add the -ed suffix: apply->applied, pry->pried, study->studied.
            if (text[^1] == 'y' && !IsVowel(text[^2]))
                return text[0..^2] + "ied";

            // Ends with a single consonant other than w or y preceded by a single vowel, then double the final consonant and add the -ed suffix: drop->dropped, admit->admitted, concur->concured.
            if (!IsVowel(text[^1]) && text[^1] != 'w' && text[^1] != 'y' && IsVowel(text[^2]) && !IsVowel(text[^3]))
                return text + text[^1] + "ed";

            // Ends with the letter c, then add the letter k followed by the -ed suffix: frolic->frolicked, picnic->picnicked.
            if (text[^1] == 'c')
                return text + "ked";

            // Add the -ed suffix.
            return text + "ed";
        }

        /// <summary>
        /// Determine whether the character is a vowel.
        /// </summary>
        private static bool IsVowel(char c) => char.IsLetter(c) && (c == 'a' || c == 'e' || c == 'i' || c == 'o' || c == 'u');

        /// <summary>
        /// Creates a new <see cref="CodeGenerator"/>.
        /// </summary>
        /// <param name="configXml">The configuration <see cref="XElement"/>.</param>
        /// <param name="loaders">The <see cref="ICodeGenConfigLoader"/> dictionary.</param>
        /// <returns>A <see cref="CodeGenerator"/>.</returns>
        public static CodeGenerator Create(XElement configXml, IEnumerable<ICodeGenConfigLoader>? loaders = null)
        {
            if (configXml == null)
                throw new ArgumentNullException(nameof(configXml));

            // Create the code generator.
            var cg = new CodeGenerator(configXml);

            // Load the Loaders making sure it does not contain 'System'.
            if (loaders != null)
            {
                foreach (var l in loaders)
                {
                    if (l.Name == SystemConfigName)
                        throw new ArgumentException($"A ConfigLoader with the Name of '{SystemConfigName}' is reserved for internal use only.", nameof(loaders));

                    if (cg.Loaders.ContainsKey(l.Name))
                        throw new ArgumentException($"A ConfigLoader with the Name of '{l.Name}' has already been defined (must be unique).", nameof(loaders));

                    cg.Loaders.Add(l.Name, l);
                }
            }

            return cg;
        }

        /// <summary>
        /// Private constructor.
        /// </summary>
        private CodeGenerator(XElement configXml) => ConfigXml = configXml;

        /// <summary>
        /// Gets or sets the <b>root</b> (top-most) <see cref="CodeGenConfig"/>.
        /// </summary>
        internal CodeGenConfig? Root { get; set; }

        /// <summary>
        /// Gets the <b>System</b> <see cref="CodeGenConfig"/>.
        /// </summary>
        internal CodeGenConfig System { get; private set; } = new CodeGenConfig(SystemConfigName, null);

        /// <summary>
        /// Gets the configuration <see cref="XElement"/>.
        /// </summary>
        public XElement ConfigXml { get; private set; }

        /// <summary>
        /// Gets the <see cref="ICodeGenConfigLoader"/> loaders.
        /// </summary>
        internal Dictionary<string, ICodeGenConfigLoader> Loaders { get; } = new Dictionary<string, ICodeGenConfigLoader>();

        /// <summary>
        /// Gets the parameter overrides.
        /// </summary>
        public Dictionary<string, string> Parameters { get; internal set; } = new Dictionary<string, string>();

        /// <summary>
        /// Copies the <paramref name="parameters"/> into <see cref="Parameters"/>.
        /// </summary>
        /// <param name="parameters">The parameters to copy.</param>
        public void CopyParameters(Dictionary<string, string> parameters)
        {
            if (parameters == null)
                return;

            foreach (var p in parameters)
            {
                Parameters.Add(p.Key, p.Value);
            }
        }

        /// <summary>
        /// Clears the current <see cref="Parameters"/>.
        /// </summary>
        public void ClearParameters()
        {
            Parameters.Clear();
        }

        /// <summary>
        /// Generates the output.
        /// </summary>
        /// <param name="xmlTemplate">The template <see cref="XElement"/>.</param>
        public async Task GenerateAsync(XElement xmlTemplate)
        {
            if (xmlTemplate == null)
                throw new ArgumentNullException(nameof(xmlTemplate));

            // Ready the 'System' configuration.
            System = new CodeGenConfig(SystemConfigName, null);
            System.AttributeAdd("Index", "0");

            // Creates the root configuration.
            await CodeGenConfig.CreateAsync(this).ConfigureAwait(false);

            using var t = new CodeGenTemplate(this, xmlTemplate);
            t.Execute();
        }

        /// <summary>
        /// Raises the <see cref="CodeGenerated"/> event.
        /// </summary>
        internal void RaiseCodeGenerated(CodeGeneratorEventArgs e)
        {
            CodeGenerated?.Invoke(this, e);
        }

        /// <summary>
        /// Occurs when a output has been successfully generated.
        /// </summary>
        public event EventHandler<CodeGeneratorEventArgs>? CodeGenerated;
    }

    /// <summary>
    /// The <see cref="CodeGenerator"/> event arguments.
    /// </summary>
    public class CodeGeneratorEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the generated directory name.
        /// </summary>
        public string? OutputGenDirName { get; internal set; }

        /// <summary>
        /// Gets the optional directory name.
        /// </summary>
        public string? OutputDirName { get; internal set; }

        /// <summary>
        /// Gets the generated file name.
        /// </summary>
        public string? OutputFileName { get; internal set; }

        /// <summary>
        /// Indicates whether the file is only output when new; i.e. does not already exist.
        /// </summary>
        public bool IsOutputNewOnly { get; internal set; }

        /// <summary>
        /// Gets the generated output content.
        /// </summary>
        public string? Content { get; internal set; }
    }
}
