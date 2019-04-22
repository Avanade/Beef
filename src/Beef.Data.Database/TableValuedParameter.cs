// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Data;

namespace Beef.Data.Database
{
    /// <summary>
    /// Represents a SQL table-valued parameter.
    /// </summary>
    public class TableValuedParameter
    {
        #region ListTVPs

        /// <summary>
        /// Gets or sets the table-value parameter type name for an <see cref="IEnumerable{String}"/>.
        /// </summary>
        public static string StringListTypeName { get; set; } = "[dbo].[udtNVarCharList]";

        /// <summary>
        /// Gets or sets the table-value parameter type name for an <see cref="IEnumerable{Int32}"/>.
        /// </summary>
        public static string Int32ListTypeName { get; set; } = "[dbo].[udtIntList]";

        /// <summary>
        /// Gets or sets the table-value parameter type name for an <see cref="IEnumerable{Int64}"/>.
        /// </summary>
        public static string Int64ListTypeName { get; set; } = "[dbo].[udtBigIntList]";

        /// <summary>
        /// Gets or sets the table-value parameter type name for an <see cref="IEnumerable{Guid}"/>.
        /// </summary>
        public static string GuidListTypeName { get; set; } = "[dbo].[udtUniqueIdentifierList]";

        /// <summary>
        /// Gets or sets the table-value parameter type name for an <see cref="IEnumerable{DateTime}"/>.
        /// </summary>
        public static string DateTimeListTypeName { get; set; } = "[dbo].[udtDateTime2]";

        /// <summary>
        /// Gets or sets the table-value parameter <see cref="DataTable"/> column name for list values.
        /// </summary>
        public static string ListValueColumnName { get; set; } = "Value";

        /// <summary>
        /// Creates a <see cref="StringListTypeName"/> <see cref="TableValuedParameter"/> for the <see cref="string"/> <paramref name="list"/>.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="TableValuedParameter"/>.</returns>
        public static TableValuedParameter Create(IEnumerable<string> list)
        {
            return Create(StringListTypeName, list);
        }

        /// <summary>
        /// Creates a <see cref="TableValuedParameter"/> for the <see cref="string"/> <paramref name="list"/>.
        /// </summary>
        /// <param name="typeName">The SQL type name of the table-valued parameter.</param>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="TableValuedParameter"/>.</returns>
        public static TableValuedParameter Create(string typeName, IEnumerable<string> list)
        {
            var dt = new DataTable();
            dt.Columns.Add(ListValueColumnName, typeof(string));

            if (list != null)
            {
                foreach (var item in list)
                {
                    dt.Rows.Add(item);
                }
            }

            return new TableValuedParameter(typeName, dt);
        }

        /// <summary>
        /// Creates a <see cref="Int32ListTypeName"/> <see cref="TableValuedParameter"/> for the <see cref="int"/> <paramref name="list"/>.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="TableValuedParameter"/>.</returns>
        public static TableValuedParameter Create(IEnumerable<int> list)
        {
            return Create(Int32ListTypeName, list);
        }

        /// <summary>
        /// Creates a <see cref="TableValuedParameter"/> for the <see cref="int"/> <paramref name="list"/>.
        /// </summary>
        /// <param name="typeName">The SQL type name of the table-valued parameter.</param>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="TableValuedParameter"/>.</returns>
        public static TableValuedParameter Create(string typeName, IEnumerable<int> list)
        {
            var dt = new DataTable();
            dt.Columns.Add(ListValueColumnName, typeof(int));

            if (list != null)
            {
                foreach (var item in list)
                {
                    dt.Rows.Add(item);
                }
            }

            return new TableValuedParameter(typeName, dt);
        }

        /// <summary>
        /// Creates a <see cref="Int64ListTypeName"/> <see cref="TableValuedParameter"/> for the <see cref="long"/> <paramref name="list"/>.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="TableValuedParameter"/>.</returns>
        public static TableValuedParameter Create(IEnumerable<long> list)
        {
            return Create(Int64ListTypeName, list);
        }

        /// <summary>
        /// Creates a <see cref="TableValuedParameter"/> for the <see cref="long"/> <paramref name="list"/>.
        /// </summary>
        /// <param name="typeName">The SQL type name of the table-valued parameter.</param>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="TableValuedParameter"/>.</returns>
        public static TableValuedParameter Create(string typeName, IEnumerable<long> list)
        {
            var dt = new DataTable();
            dt.Columns.Add(ListValueColumnName, typeof(long));

            if (list != null)
            {
                foreach (var item in list)
                {
                    dt.Rows.Add(item);
                }
            }

            return new TableValuedParameter(typeName, dt);
        }

        /// <summary>
        /// Creates a <see cref="GuidListTypeName"/> <see cref="TableValuedParameter"/> for the <see cref="Guid"/> <paramref name="list"/>.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="TableValuedParameter"/>.</returns>
        public static TableValuedParameter Create(IEnumerable<Guid> list)
        {
            return Create(GuidListTypeName, list);
        }

        /// <summary>
        /// Creates a <see cref="TableValuedParameter"/> for the <see cref="Guid"/> <paramref name="list"/>.
        /// </summary>
        /// <param name="typeName">The SQL type name of the table-valued parameter.</param>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="TableValuedParameter"/>.</returns>
        public static TableValuedParameter Create(string typeName, IEnumerable<Guid> list)
        {
            var dt = new DataTable();
            dt.Columns.Add(ListValueColumnName, typeof(Guid));

            if (list != null)
            {
                foreach (var item in list)
                {
                    dt.Rows.Add(item);
                }
            }

            return new TableValuedParameter(typeName, dt);
        }

        /// <summary>
        /// Creates a <see cref="DateTimeListTypeName"/> <see cref="TableValuedParameter"/> for the <see cref="DateTime"/> <paramref name="list"/>.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="TableValuedParameter"/>.</returns>
        public static TableValuedParameter Create(IEnumerable<DateTime> list)
        {
            return Create(GuidListTypeName, list);
        }

        /// <summary>
        /// Creates a <see cref="TableValuedParameter"/> for the <see cref="DateTime"/> <paramref name="list"/>.
        /// </summary>
        /// <param name="typeName">The SQL type name of the table-valued parameter.</param>
        /// <param name="list">The list.</param>
        /// <returns>The <see cref="TableValuedParameter"/>.</returns>
        public static TableValuedParameter Create(string typeName, IEnumerable<DateTime> list)
        {
            var dt = new DataTable();
            dt.Columns.Add(ListValueColumnName, typeof(DateTime));

            if (list != null)
            {
                foreach (var item in list)
                {
                    dt.Rows.Add(item);
                }
            }

            return new TableValuedParameter(typeName, dt);
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="TableValuedParameter"/> class.
        /// </summary>
        /// <param name="typeName">The SQL type name of the table-valued parameter.</param>
        /// <param name="value">The <see cref="System.Data.DataTable"/> value.</param>
        public TableValuedParameter(string typeName, DataTable value)
        {
            TypeName = Check.NotEmpty(typeName, nameof(typeName));
            Value = Check.NotNull(value, nameof(value));
        }

        /// <summary>
        /// Gets or sets the SQL type name of the table-valued parameter.
        /// </summary>
        public string TypeName { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="System.Data.DataTable"/> value.
        /// </summary>
        public DataTable Value { get; private set; }
    }
}
