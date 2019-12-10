// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System.Net.Http;

namespace Beef.WebApi
{
    /// <summary>
    /// Represents additional (optional) request options for a Web API request.
    /// </summary>
    public class WebApiRequestOptions : IETag
    {
        /// <summary>
        /// Gets or sets the entity tag that will be passed as either a <c>If-None-Match</c> header where <see cref="HttpMethod.Get"/>; otherwise, an <c>If-Match</c> header.
        /// </summary>
        public string ETag { get; set; }

#pragma warning disable CA1056 // Uri properties should not be strings; by-design, is the query string component only.
        /// <summary>
        /// Gets or sets the value to append to the URL query string.
        /// </summary>
        public string UrlQueryString { get; set; }
#pragma warning restore CA1056 
    }
}