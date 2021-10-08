// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using AutoMapper;
using Beef.Entities;
using System;
using System.Net;
using System.Net.Http;

namespace Beef.WebApi
{
    /// <summary>
    /// Provides the <see cref="HttpAgentBase.SendAsync(HttpSendArgs)"/> arguments capabilities using an optional <i>AutoMapper</i> <see cref="IMapper"/> where required.
    /// </summary>
    public class HttpSendArgs
    {
        /// <summary>
        /// Create a <see cref="HttpSendArgs"/> without a corresponding <see cref="Mapper"/>.
        /// </summary>
        /// <param name="httpMethod">The <see cref="HttpMethod"/>.</param>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="nullOnNotFoundResponse">Indicates that a <c>null</c> is to be returned where the <b>response</b> has a <see cref="HttpStatusCode"/> of <see cref="HttpStatusCode.NotFound"/>.</param>
        /// <returns>The <see cref="HttpSendArgs"/>.</returns>
        public static HttpSendArgs Create(HttpMethod httpMethod, string? urlSuffix, bool nullOnNotFoundResponse = false) => new(httpMethod, urlSuffix) { NullOnNotFoundResponse = nullOnNotFoundResponse };

        /// <summary>
        /// Create a <see cref="HttpSendArgs"/> with <paramref name="paging"/> and without a corresponding <see cref="Mapper"/>.
        /// </summary>
        /// <param name="httpMethod">The <see cref="HttpMethod"/>.</param>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="nullOnNotFoundResponse">Indicates that a <c>null</c> is to be returned where the <b>response</b> has a <see cref="HttpStatusCode"/> of <see cref="HttpStatusCode.NotFound"/>.</param>
        /// <returns>The <see cref="HttpSendArgs"/>.</returns>
        public static HttpSendArgs Create(HttpMethod httpMethod, string? urlSuffix, PagingArgs paging, bool nullOnNotFoundResponse = false) => new(httpMethod, urlSuffix, paging) { NullOnNotFoundResponse = nullOnNotFoundResponse };

        /// <summary>
        /// Create a <see cref="HttpSendArgs"/> with <paramref name="paging"/> and without a corresponding <see cref="Mapper"/>.
        /// </summary>
        /// <param name="httpMethod">The <see cref="HttpMethod"/>.</param>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="nullOnNotFoundResponse">Indicates that a <c>null</c> is to be returned where the <b>response</b> has a <see cref="HttpStatusCode"/> of <see cref="HttpStatusCode.NotFound"/>.</param>
        /// <returns>The <see cref="HttpSendArgs"/>.</returns>
        public static HttpSendArgs Create(HttpMethod httpMethod, string? urlSuffix, PagingResult paging, bool nullOnNotFoundResponse = false) => new(httpMethod, urlSuffix, paging) { NullOnNotFoundResponse = nullOnNotFoundResponse };

        /// <summary>
        /// Create a <see cref="HttpSendArgs"/> without a corresponding <see cref="Mapper"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="httpMethod">The <see cref="HttpMethod"/>.</param>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="nullOnNotFoundResponse">Indicates that a <c>null</c> is to be returned where the <b>response</b> has a <see cref="HttpStatusCode"/> of <see cref="HttpStatusCode.NotFound"/>.</param>
        /// <returns>The <see cref="HttpSendArgs"/>.</returns>
        public static HttpSendArgs Create(IMapper mapper, HttpMethod httpMethod, string? urlSuffix, bool nullOnNotFoundResponse = false) => new(mapper, httpMethod, urlSuffix) { NullOnNotFoundResponse = nullOnNotFoundResponse };

        /// <summary>
        /// Create a <see cref="HttpSendArgs"/> with <paramref name="paging"/> and without a corresponding <see cref="Mapper"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="httpMethod">The <see cref="HttpMethod"/>.</param>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="nullOnNotFoundResponse">Indicates that a <c>null</c> is to be returned where the <b>response</b> has a <see cref="HttpStatusCode"/> of <see cref="HttpStatusCode.NotFound"/>.</param>
        /// <returns>The <see cref="HttpSendArgs"/>.</returns>
        public static HttpSendArgs Create(IMapper mapper, HttpMethod httpMethod, string? urlSuffix, PagingArgs paging, bool nullOnNotFoundResponse = false) => new(mapper, httpMethod, urlSuffix, paging) { NullOnNotFoundResponse = nullOnNotFoundResponse };

        /// <summary>
        /// Create a <see cref="HttpSendArgs"/> with <paramref name="paging"/> and without a corresponding <see cref="Mapper"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="httpMethod">The <see cref="HttpMethod"/>.</param>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="nullOnNotFoundResponse">Indicates that a <c>null</c> is to be returned where the <b>response</b> has a <see cref="HttpStatusCode"/> of <see cref="HttpStatusCode.NotFound"/>.</param>
        /// <returns>The <see cref="HttpSendArgs"/>.</returns>
        public static HttpSendArgs Create(IMapper mapper, HttpMethod httpMethod, string? urlSuffix, PagingResult paging, bool nullOnNotFoundResponse = false) => new(mapper, httpMethod, urlSuffix, paging) { NullOnNotFoundResponse = nullOnNotFoundResponse };

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpSendArgs"/> without a <see cref="Mapper"/>.
        /// </summary>
        /// <param name="httpMethod">The <see cref="HttpMethod"/>.</param>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        public HttpSendArgs(HttpMethod httpMethod, string? urlSuffix)
        {
            HttpMethod = httpMethod;
            UrlSuffix = urlSuffix;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpSendArgs"/> with <paramref name="paging"/> and without a corresponding <see cref="Mapper"/>.
        /// </summary>
        /// <param name="httpMethod">The <see cref="HttpMethod"/>.</param>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        public HttpSendArgs(HttpMethod httpMethod, string? urlSuffix, PagingArgs paging) : this(httpMethod, urlSuffix, new PagingResult(paging ?? throw new ArgumentNullException(nameof(paging)))) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpSendArgs"/> without a corresponding <see cref="Mapper"/>.
        /// </summary>
        /// <param name="httpMethod">The <see cref="HttpMethod"/>.</param>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        public HttpSendArgs(HttpMethod httpMethod, string? urlSuffix, PagingResult paging) : this(httpMethod, urlSuffix) => Paging = paging ?? throw new ArgumentNullException(nameof(paging));

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpSendArgs"/> without a <see cref="Mapper"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="httpMethod">The <see cref="HttpMethod"/>.</param>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        public HttpSendArgs(IMapper mapper, HttpMethod httpMethod, string? urlSuffix) : this(httpMethod, urlSuffix) => Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpSendArgs"/> with <paramref name="paging"/> and without a corresponding <see cref="Mapper"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="httpMethod">The <see cref="HttpMethod"/>.</param>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        public HttpSendArgs(IMapper mapper, HttpMethod httpMethod, string? urlSuffix, PagingArgs paging) : this(httpMethod, urlSuffix, paging) => Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpSendArgs"/> without a corresponding <see cref="Mapper"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="httpMethod">The <see cref="HttpMethod"/>.</param>
        /// <param name="urlSuffix">The url suffix for the operation.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        public HttpSendArgs(IMapper mapper, HttpMethod httpMethod, string? urlSuffix, PagingResult paging) : this(httpMethod, urlSuffix, paging) => Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));

        /// <summary>
        /// Gets the <see cref="HttpMethod"/>.
        /// </summary>
        public HttpMethod HttpMethod { get; }

        /// <summary>
        /// Gets the url suffix for the operation.
        /// </summary>
        public string? UrlSuffix { get; }

        /// <summary>
        /// Gets the <i>AutoMapper</i> <see cref="IMapper"/>.
        /// </summary>
        public IMapper? Mapper { get; }

        /// <summary>
        /// Gets or sets the <see cref="PagingResult"/> (where paging is required for a <b>query</b>).
        /// </summary>
        public PagingResult? Paging { get; }

        /// <summary>
        /// Indicates that a <c>null</c> is to be returned where the <b>response</b> has a <see cref="HttpStatusCode"/> of <see cref="HttpStatusCode.NotFound"/>.
        /// </summary>
        public bool NullOnNotFoundResponse { get; set; } = false;
    }
}