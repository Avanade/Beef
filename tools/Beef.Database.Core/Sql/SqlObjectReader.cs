// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Beef.Database.Core.Sql
{
    /// <summary>
    /// Represents a basic SQL object reader and parser.
    /// </summary>
    public class SqlObjectReader
    {
        private readonly TextReader _tr;
        private readonly List<string> _lines = new List<string>();
        private readonly List<Token> _tokens = new List<Token>();

        /// <summary>
        /// Gets the list of supported object types and their application order.
        /// </summary>
        private readonly List<(string Type, int Order)> _supportedObjectTypes = new List<(string, int)>
        {
            ("TYPE", 1),
            ("FUNCTION", 2),
            ("VIEW", 3),
            ("PROCEDURE", 4),
            ("PROC", 5)
        };

        /// <summary>
        /// Represents the token characteristics.
        /// </summary>
        internal class Token
        {
            /// <summary>
            /// Gets or sets the line.
            /// </summary>
            public int Line { get; set; }

            /// <summary>
            /// Gets or sets the column.
            /// </summary>
            public int Column { get; set; }

            /// <summary>
            /// Gets or sets the value.
            /// </summary>
            public string? Value { get; set; }
        }

        /// <summary>
        /// Reads and parses the SQL <see cref="string"/>.
        /// </summary>
        /// <param name="sql">The SQL <see cref="string"/>.</param>
        /// <returns>A <see cref="SqlObjectReader"/>.</returns>
        public static SqlObjectReader Read(string sql)
        {
            using var sr = new StringReader(sql);
            return Read(sr);
        }

        /// <summary>
        /// Reads and parses the SQL <see cref="Stream"/>.
        /// </summary>
        /// <param name="s">The SQL <see cref="Stream"/>.</param>
        /// <returns>A <see cref="SqlObjectReader"/>.</returns>
        public static SqlObjectReader Read(Stream s)
        {
            using var sr = new StreamReader(s);
            return Read(sr);
        }

        /// <summary>
        /// Reads and parses the SQL <see cref="TextReader"/>.
        /// </summary>
        /// <param name="tr">The SQL <see cref="TextReader"/>.</param>
        /// <returns>A <see cref="SqlObjectReader"/>.</returns>
        public static SqlObjectReader Read(TextReader tr) => new SqlObjectReader(tr);

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlObjectReader"/> class.
        /// </summary>
        /// <param name="tr"></param>
        private SqlObjectReader(TextReader tr)
        {
            _tr = Check.NotNull(tr, nameof(tr));
            Parse();
            if (!IsValid)
                return;

            Type = SqlObjectType!.Value!;
            var sot = _supportedObjectTypes.Where(x => string.Compare(x.Type, Type, StringComparison.InvariantCultureIgnoreCase) == 0).SingleOrDefault();
            Order = sot.Type == null ? -1 : sot.Order;

            var parts = SqlObjectName!.Value!.Split('.');
            if (parts.Length == 1)
                Name = parts[0].Replace('[', ' ').Replace(']', ' ').Trim();
            else if (parts.Length == 2)
            {
                Schema = parts[0].Replace('[', ' ').Replace(']', ' ').Trim();
                Name = parts[1].Replace('[', ' ').Replace(']', ' ').Trim();
            }
            else
                ErrorMessage = $"The SQL object name is not valid.";
        }

        /// <summary>
        /// Read file and parse out the primary tokens.
        /// </summary>
        private void Parse()
        {
            Token? token = null;

            while (true)
            {
                var txt = _tr.ReadLine();
                if (txt == null)
                    break;

                _lines.Add(txt);
                if (_tokens.Count >= 3)
                    continue;

                // Remove comments.
                if (txt.StartsWith("--", StringComparison.InvariantCulture))
                    continue;

                var ci = txt.IndexOf("--", StringComparison.InvariantCulture);
                if (ci >= 0)
                    txt = txt.Substring(0, ci - 1).TrimEnd();

                // Remove function component.
                ci = txt.IndexOf("(", StringComparison.InvariantCulture);
                if (ci >= 0)
                    txt = txt.Substring(0, ci - 1).TrimEnd();

                // Parse out the token(s).
                var col = 0;
                for (; col < txt.Length; col++)
                {
                    if (char.IsWhiteSpace(txt[col]))
                    {
                        if (token != null)
                        {
                            token.Value = txt[token.Column..col];
                            _tokens.Add(token);
                            token = null;
                        }
                    }
                    else if (token == null)
                    {
                        token = new Token { Line = _lines.Count - 1, Column = col };
                    }
                }

                if (token != null)
                {
                    token.Value = txt[token.Column..col];
                    _tokens.Add(token);
                    token = null;
                }
            }

            ErrorMessage = CreateErrorMessage();
        }

        /// <summary>
        /// Indicates whether the SQL Object is valid.
        /// </summary>
        public bool IsValid => ErrorMessage == null;

        /// <summary>
        /// Gets the error message where not valid (see <see cref="IsValid"/>).
        /// </summary>
        public string? ErrorMessage { get; private set; }

        /// <summary>
        /// Create the error message where not valid.
        /// </summary>
        private string? CreateErrorMessage()
        {
            if (SqlStatement == null)
                return "The SQL statement could not be determined; expecting a `CREATE` statement.";
            else if (string.Compare(SqlStatement.Value, "create", StringComparison.InvariantCultureIgnoreCase) != 0)
                return $"The SQL statement must be a `CREATE`; found '{SqlStatement.Value}'.";

            if (SqlObjectType == null)
                return "The SQL object type could not be determined.";
            else if (GetDbOperationOrder() < 0)
                return $"The SQL object type '{SqlObjectType.Value}' is not supported; this should be added as a Script.";

            if (SqlObjectName == null)
                return "The SQL object name could not be determined.";

            return null;
        }

        /// <summary>
        /// Gets the corresponding database operation order.
        /// </summary>
        public int GetDbOperationOrder()
        {
            if (SqlObjectType == null)
                return -1;

            var sot = _supportedObjectTypes.Where(x => string.Compare(x.Item1, SqlObjectType.Value, StringComparison.InvariantCultureIgnoreCase) == 0).SingleOrDefault();
            return sot.Type == null ? -1 : sot.Order;
        }

        /// <summary>
        /// Gets the primary SQL command (first token).
        /// </summary>
        private Token? SqlStatement => _tokens.Count < 1 ? null : _tokens[0];

        /// <summary>
        /// Gets the underlying SQL object type (second token).
        /// </summary>
        private Token? SqlObjectType => _tokens.Count < 2 ? null : _tokens[1];

        /// <summary>
        /// Gets the underlying SQL object name (third token).
        /// </summary>
        private Token? SqlObjectName => _tokens.Count < 3 ? null : _tokens[2];

        /// <summary>
        /// Gets the SQL object type.
        /// </summary>
        public string? Type { get; private set; }

        /// <summary>
        /// Gets the SQL object schema.
        /// </summary>
        public string? Schema { get; private set; }

        /// <summary>
        /// Gets the SQL object name.
        /// </summary>
        public string? Name { get; private set; }

        /// <summary>
        /// Gets the SQL object type order of precedence.
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// Gets the SQL <see cref="string"/>.
        /// </summary>
        /// <returns>The SQL.</returns>
        public string GetSql()
        {
            var sb = new StringBuilder();
            _lines.ForEach((l) => sb.AppendLine(l));
            return sb.ToString();
        }
    }
}