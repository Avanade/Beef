// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.AspNetCore.WebApi
{
    /// <summary>
    /// Provides capabilities for working with the query string.
    /// </summary>
    public static class WebApiQueryString
    {
        /// <summary>
        /// Gets or sets the list of possible <see cref="PagingArgs.Page"/> query string names.
        /// </summary>
        public static List<string> PagingArgsPageQueryStringNames { get; } = new List<string>(new string[] { "$page", "$pageNumber" });

        /// <summary>
        /// Gets or sets the list of possible <see cref="PagingArgs.Skip"/> query string names.
        /// </summary>
        public static List<string> PagingArgsSkipQueryStringNames { get; } = new List<string>(new string[] { "$skip", "$offset" });

        /// <summary>
        /// Gets or sets the list of possible <see cref="PagingArgs.Take"/> query string names.
        /// </summary>
        public static List<string> PagingArgsTakeQueryStringNames { get; } = new List<string>(new string[] { "$take", "$top", "$size", "$pageSize", "$limit" });

        /// <summary>
        /// Gets or sets the list of possible <see cref="PagingArgs.Take"/> query string names.
        /// </summary>
        public static List<string> PagingArgsCountQueryStringNames { get; } = new List<string>(new string[] { "$count", "$totalCount" });

        /// <summary>
        /// Gets or sets the list of possible "include field" query string names.
        /// </summary>
        public static List<string> IncludeFieldsQueryStringNames { get; } = new List<string>(new string[] { "$fields", "$includeFields", "$include" });

        /// <summary>
        /// Gets or sets the list of possible "exclude field" query string names.
        /// </summary>
        public static List<string> ExcludeFieldsQueryStringNames { get; } = new List<string>(new string[] { "$excludeFields", "$exclude" });

        /// <summary>
        /// Gets or sets the list of possible "include inactive" query string names.
        /// </summary>
        public static List<string> IncludeInactiveQueryStringNames { get; } = new List<string>(new string[] { "$inactive", "$includeInactive" });

        /// <summary>
        /// Gets or sets the list of possible reference data "include texts" query string names.
        /// </summary>
        public static List<string> IncludeRefDataTextQueryStringNames { get; } = new List<string>(new string[] { "$text", "$includeText" });

        /// <summary>
        /// Creates the <see cref="PagingArgs"/> from the query string.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/> that has the request url.</param>
        /// <returns>The <see cref="PagingArgs"/>.</returns>
        /// <remarks>Will return the <see cref="ExecutionContext"/> <see cref="ExecutionContext.PagingArgs"/> where already set; otherwise, will update it once value inferred.</remarks>
        public static PagingArgs CreatePagingArgs(this ControllerBase controller)
        {
            Check.NotNull(controller, nameof(controller));
            if (ExecutionContext.HasCurrent && ExecutionContext.Current.PagingArgs != null)
                return ExecutionContext.Current.PagingArgs;

#pragma warning disable CA1062 // Validate arguments of public methods; see earlier Check.
            var q = controller.HttpContext?.Request?.Query;
#pragma warning restore CA1062
            PagingArgs pa;

            if (q == null || q.Count == 0)
                pa = new PagingArgs();
            else
            {
                long? skip = ParseLongValue(GetNamedQueryString(controller, PagingArgsSkipQueryStringNames));
                long? take = ParseLongValue(GetNamedQueryString(controller, PagingArgsTakeQueryStringNames));
                long? page = skip.HasValue ? null : ParseLongValue(GetNamedQueryString(controller, PagingArgsPageQueryStringNames));

                if (skip == null && page == null)
                    pa = (take.HasValue) ? PagingArgs.CreateSkipAndTake(0, take) : new PagingArgs();
                else
                    pa = (skip.HasValue) ? PagingArgs.CreateSkipAndTake(skip.Value, take) : PagingArgs.CreatePageAndSize(page == null ? 0 : page.Value, take);

                pa.IsGetCount = ParseBoolValue(GetNamedQueryString(controller, PagingArgsCountQueryStringNames));
            }

            if (ExecutionContext.HasCurrent && ExecutionContext.Current.PagingArgs == null)
                ExecutionContext.Current.PagingArgs = pa;

            return pa;
        }

        /// <summary>
        /// Gets the other known request options.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/> that has the request url.</param>
        /// <returns>The other known request options.</returns>
        internal static (List<string> includeFields, List<string> excludeFields) GetOtherRequestOptions(this ControllerBase controller)
        {
            var includeFields = new List<string>();
            var excludeFields = new List<string>();

            var fields = GetNamedQueryString(controller, IncludeFieldsQueryStringNames);
            if (!string.IsNullOrEmpty(fields))
                includeFields.AddRange(fields.Split(',', StringSplitOptions.RemoveEmptyEntries));

            fields = GetNamedQueryString(controller, ExcludeFieldsQueryStringNames);
            if (!string.IsNullOrEmpty(fields))
                excludeFields.AddRange(fields.Split(',', StringSplitOptions.RemoveEmptyEntries));

            return (includeFields, excludeFields);
        }

        /// <summary>
        /// Gets the value for the named query string.
        /// </summary>
        private static string? GetNamedQueryString(ControllerBase controller, IEnumerable<string> names)
        {
            var q = controller.HttpContext.Request.Query.Where(x => names.Contains(x.Key, StringComparer.InvariantCultureIgnoreCase)).ToArray();
            return (q.Length != 1 || q[0].Value.Count != 1) ? null : q[0].Value[0];
        }

        /// <summary>
        /// Parses the value as a <see cref="long"/>.
        /// </summary>
        private static long? ParseLongValue(string? value)
        {
            if (value == null)
                return null;

            if (!long.TryParse(value, out long val))
                return null;

            return val;
        }

        /// <summary>
        /// Parses the value as a <see cref="bool"/>.
        /// </summary>
        private static bool ParseBoolValue(string? value)
        {
            if (value == null)
                return false;

            if (!bool.TryParse(value, out bool val))
                return false;

            return val;
        }

        /// <summary>
        /// Gets the <see cref="IncludeInactiveQueryStringNames"/> value.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/> that has the request url.</param>
        /// <returns>The corresponding value.</returns>
        public static bool IncludeInactive(this ControllerBase controller)
        {
            Check.NotNull(controller, nameof(controller));
#pragma warning disable CA1062 // Validate arguments of public methods; see Check above.
            return ParseBoolValue(GetNamedQueryString(controller, IncludeInactiveQueryStringNames));
#pragma warning restore CA1062 
        }

        /// <summary>
        /// Gets the <see cref="IncludeRefDataTextQueryStringNames"/> value.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/> that has the request url.</param>
        /// <returns>The corresponding value.</returns>
        public static bool IncludeRefDataText(this ControllerBase controller)
        {
            Check.NotNull(controller, nameof(controller));
#pragma warning disable CA1062 // Validate arguments of public methods; see Check above.
            return ParseBoolValue(GetNamedQueryString(controller, IncludeRefDataTextQueryStringNames));
#pragma warning restore CA1062 
        }

        /// <summary>
        /// Gets the reference data selection from the query string.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/> that has the request url.</param>
        /// <returns>The resulting selection.</returns>
        public static IEnumerable<KeyValuePair<string, StringValues>> ReferenceDataSelection(this ControllerBase controller)
        {
            Check.NotNull(controller, nameof(controller));

            var dict = new Dictionary<string, KeyValuePair<string, StringValues>>();
#pragma warning disable CA1062 // Validate arguments of public methods; see Check above.
            if (!controller.HttpContext.Request.Query.Any())
#pragma warning restore CA1062
            {
                ExecutionContext.Current.Messages.AddInfo("A query string is required to filter selection; e.g. /ref?entity=codeX,codeY&entity2=codeZ&entity3");
                return dict.Values;
            }

            foreach (var q in controller.HttpContext.Request.Query.Where(x => !string.IsNullOrEmpty(x.Key)))
            {
                if (string.Compare(q.Key, "names", StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    foreach (var v in q.Value.Where(x => !string.IsNullOrEmpty(x)))
                    {
                        if (!dict.ContainsKey(v))
                        {
                            dict.Add(v, new KeyValuePair<string, StringValues>(v, new StringValues()));
                        }
                    }
                }
                else
                {
                    dict[q.Key] = new KeyValuePair<string, StringValues>(q.Key, q.Value);
                }
            }

            return dict.Values;
        }
    }
}