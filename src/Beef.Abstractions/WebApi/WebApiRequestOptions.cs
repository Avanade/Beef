// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.RefData;
using System.Collections.Generic;
using System.Net.Http;

namespace Beef.WebApi
{
    /// <summary>
    /// Represents additional (optional) request options for a Web API request.
    /// </summary>
    public class WebApiRequestOptions : IETag
    {
        private string? _etag;

        /// <summary>
        /// Gets or sets the <see cref="IncludeFields"/> query string name.
        /// </summary>
        public static string IncludeFieldsQueryStringName { get; set; } = "$fields";

        /// <summary>
        /// Gets or sets the <see cref="ExcludeFields"/> query string name.
        /// </summary>
        public static string ExcludeFieldsQueryStringName { get; set; } = "$exclude";

        /// <summary>
        /// Gets or sets the <see cref="IncludeRefDataText"/> query string name.
        /// </summary>
        public static string IncludeRefDataTextQueryStringName { get; set; } = "$text";

        /// <summary>
        /// Gets or sets the <see cref="IncludeInactive"/> query string name.
        /// </summary>
        public static string IncludeInactiveQueryStringName { get; set; } = "$inactive";

        /// <summary>
        /// Gets or sets the entity tag that will be passed as either a <c>If-None-Match</c> header where <see cref="HttpMethod.Get"/>; otherwise, an <c>If-Match</c> header.
        /// </summary>
        /// <remarks>Automatically adds quoting to be ETag format compliant.</remarks>
        public string? ETag 
        {
            get => _etag;
            set => _etag = value == null ? null : (value.StartsWith('\"') && value.StartsWith('\"') ? value : $"\"{value}\"");
        }

        /// <summary>
        /// Gets or sets the list of <b>included</b> fields (JSON property names) to limit the serialized data payload (results in url query string: "$fields=x,y,z").
        /// </summary>
        public List<string> IncludeFields { get; } = new List<string>();

        /// <summary>
        /// Gets or sets the list of <b>excluded</b> fields (JSON property names) to limit the serialized data payload (results in url query string: "$excludefields=x,y,z").
        /// </summary>
        public List<string> ExcludeFields { get; } = new List<string>();

        /// <summary>
        /// Indicates whether to include the related <see cref="ReferenceDataBase.Text"/> for any <b>ReferenceData</b> values returned in the JSON payload.
        /// </summary>
        public bool IncludeRefDataText { get; set; }

        /// <summary>
        /// Indicates whether to include any <b>ReferenceData</b> entities (items) where <see cref="ReferenceDataBase.IsActive"/> is <c>false</c>.
        /// </summary>
        public bool IncludeInactive { get; set; }

        /// <summary>
        /// Appends the <paramref name="fields"/> to the <see cref="IncludeFields"/>.
        /// </summary>
        /// <param name="fields">The fields to append.</param>
        /// <returns>The current <see cref="WebApiRequestOptions"/> instance to support fluent-style method-chaining.</returns>
        public WebApiRequestOptions Include(params string[] fields)
        {
            IncludeFields.AddRange(fields);
            return this;
        }

        /// <summary>
        /// Appends the <paramref name="fields"/> to the <see cref="ExcludeFields"/>.
        /// </summary>
        /// <param name="fields">The fields to append.</param>
        /// <returns>The current <see cref="WebApiRequestOptions"/> instance to support fluent-style method-chaining.</returns>
        public WebApiRequestOptions Exclude(params string[] fields)
        {
            ExcludeFields.AddRange(fields);
            return this;
        }

        /// <summary>
        /// Gets or sets the optional value to append to the end of URL query string.
        /// </summary>
        public string? UrlQueryString { get; set; }
    }
}