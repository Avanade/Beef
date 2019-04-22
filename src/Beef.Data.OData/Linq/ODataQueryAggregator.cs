// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Remotion.Linq.Clauses;
using System;
using System.Collections.Generic;
using System.Text;

namespace Beef.Data.OData.Linq
{
    /// <summary>
    /// Represents the <b>OData</b> query aggregator and builder.
    /// </summary>
    internal class ODataQueryAggregator
    {
        private readonly ODataArgs _args;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataQueryAggregator"/> class.
        /// </summary>
        /// <param name="args">The <see cref="ODataArgs"/>.</param>
        public ODataQueryAggregator(ODataArgs args)
        {
            _args = args ?? throw new ArgumentNullException(nameof(args));
            Paging = args.Paging ?? new PagingArgs();
        }

        /// <summary>
        /// Gets or sets the <see cref="Remotion.Linq.Clauses.SelectClause"/>.
        /// </summary>
        public SelectClause SelectClause { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="PagingArgs"/>.
        /// </summary>
        public PagingArgs Paging { get; private set; }

        /// <summary>
        /// Gets or sets the select criteria.
        /// </summary>
        public List<string> Select { get; } = new List<string>();

        /// <summary>
        /// Gets or sets the where criteria.
        /// </summary>
        public List<string> Where { get; } = new List<string>();

        /// <summary>
        /// Gets or sets the order by criteria.
        /// </summary>
        public List<string> OrderBy { get; } = new List<string>();

        /// <summary>
        /// Return the URL string representation.
        /// </summary>
        /// <returns>The URL string representation.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < Select.Count; i++)
            {
                AppendText(sb, true, () => Select[i], i == 0, i == 0 ? "$select=" : ",");
            }

            for (int i = 0; i < Where.Count; i++)
            {
                AppendText(sb, true, () => Where[i], i == 0, i == 0 ? "$filter=" : " and ");
            }

            for (int i = 0; i < OrderBy.Count; i++)
            {
                AppendText(sb, true, () => OrderBy[i], i == 0, i == 0 ? "$orderby=" : ",");
            }

            AppendText(sb, Paging.Skip > 0, () => $"$skip={Paging.Skip}");
            AppendText(sb, Paging.Take > 0, () => $"$top={Paging.Take}");
            AppendText(sb, Paging.IsGetCount, () => "$count=true");

            return sb.Length == 0 ? null : "?" + sb.ToString();
        }

        /// <summary>
        /// Appends text where condition is true (with optional prefix).
        /// </summary>
        private void AppendText(StringBuilder sb, bool condition, Func<string> func, bool newStatement = true, string prefix = null)
        {
            if (!condition)
                return;

            if (sb.Length > 0 && newStatement)
                sb.Append("&");

            if (prefix != null)
                sb.Append(prefix);

            sb.Append(func.Invoke());
        }
    }
}
