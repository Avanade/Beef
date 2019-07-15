// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Microsoft.Azure.Documents.Client;
using System;
using System.Net;

namespace Beef.Data.DocumentDb
{
    /// <summary>
    /// Enables the <b>DocumentDb/CosmosDb</b> arguments capabilities.
    /// </summary>
    public interface IDocDbArgs
    {
        /// <summary>
        /// Gets the default collection identifier.
        /// </summary>
        string CollectionId { get; }

        /// <summary>
        /// Gets the <see cref="PagingResult"/>.
        /// </summary>
        PagingResult Paging { get; }

        /// <summary>
        /// Indicates that a <c>null</c> is to be returned where the <b>response</b> has a <see cref="HttpStatusCode"/> of <see cref="HttpStatusCode.NotFound"/>.
        /// </summary>
        bool NullOnNotFoundResponse { get; }

        /// <summary>
        /// Gets the <see cref="T:RequestOptions"/>.
        /// </summary>
        RequestOptions RequestOptions { get; }

        /// <summary>
        /// Gets the <see cref="T:FeedOptions"/>.
        /// </summary>
        FeedOptions FeedOptions { get; }
    }

    /// <summary>
    /// Provides the base <b>DocumentDb/CosmosDb</b> arguments capabilities.
    /// </summary>
    public class DocDbArgs : IDocDbArgs
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DocDbArgs"/> class with the <paramref name="collectionId"/> and <paramref name="requestOptions"/>.
        /// </summary>
        /// <param name="collectionId">The collection identifier.</param>
        /// <param name="requestOptions">The optional <see cref="RequestOptions"/>.</param>
        /// <returns>The <see cref="DocDbArgs"/>.</returns>
        public static DocDbArgs Create(string collectionId = null, RequestOptions requestOptions = null)
        {
            return new DocDbArgs(collectionId, requestOptions);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DocDbArgs"/> class with the <paramref name="collectionId"/>, <see cref="PagingArgs"/> and <paramref name="feedOptions"/>.
        /// </summary>
        /// <param name="collectionId">The collection identifier.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="feedOptions">The optional <see cref="FeedOptions"/>.</param>
        /// <returns>The <see cref="DocDbArgs"/>.</returns>
        public static DocDbArgs Create(string collectionId, PagingArgs paging, FeedOptions feedOptions = null)
        {
            return new DocDbArgs(collectionId, paging, feedOptions);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="DocDbArgs"/> class with the <paramref name="collectionId"/>, <see cref="PagingResult"/> and <paramref name="feedOptions"/>.
        /// </summary>
        /// <param name="collectionId">The collection identifier.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="feedOptions">The optional <see cref="FeedOptions"/>.</param>
        /// <returns>The <see cref="DocDbArgs"/>.</returns>
        public static DocDbArgs Create(string collectionId, PagingResult paging, FeedOptions feedOptions = null)
        {
            return new DocDbArgs(collectionId, paging, feedOptions);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocDbArgs"/> class.
        /// </summary>
        /// <param name="collectionId">The collection identifier.</param>
        /// <param name="requestOptions">The optional <see cref="RequestOptions"/>.</param>
        public DocDbArgs(string collectionId = null, RequestOptions requestOptions = null)
        {
            CollectionId = Check.NotEmpty(collectionId, nameof(collectionId));
            RequestOptions = requestOptions;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocDbArgs"/> class.
        /// </summary>
        /// <param name="collectionId">The collection identifier.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="feedOptions">The optional <see cref="FeedOptions"/>.</param>
        public DocDbArgs(string collectionId, PagingArgs paging, FeedOptions feedOptions = null) : this(collectionId, new PagingResult(Check.NotNull(paging, (nameof(paging)))), feedOptions) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocDbArgs"/> class.
        /// </summary>
        /// <param name="collectionId">The collection identifier.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="feedOptions">The optional <see cref="FeedOptions"/>.</param>
        public DocDbArgs(string collectionId, PagingResult paging, FeedOptions feedOptions = null)
        {
            CollectionId = Check.NotEmpty(collectionId, nameof(collectionId));
            Paging = Check.NotNull(paging, nameof(paging));
            FeedOptions = feedOptions;
        }

        /// <summary>
        /// Gets the default collection identifier.
        /// </summary>
        public string CollectionId { get; private set; }

        /// <summary>
        /// Gets the <see cref="PagingResult"/> (where paging is required for a <b>query</b>).
        /// </summary>
        public PagingResult Paging { get; private set; }

        /// <summary>
        /// Gets the <see cref="T:RequestOptions"/> used for <b>Get</b>, <b>Create</b>, <b>Update</b>, and <b>Delete</b> (<seealso cref="FeedOptions"/>).
        /// </summary>
        public RequestOptions RequestOptions { get; private set; }

        /// <summary>
        /// Gets the <see cref="T:FeedOptions"/> used for <b>Query</b> (<seealso cref="RequestOptions"/>).
        /// </summary>
        public FeedOptions FeedOptions { get; private set; }

        /// <summary>
        /// Indicates that a <c>null</c> is to be returned where the <b>response</b> has a <see cref="HttpStatusCode"/> of <see cref="HttpStatusCode.NotFound"/>.
        /// </summary>
        public bool NullOnNotFoundResponse { get; set; }
    }

    /// <summary>
    /// Provides the typed-mapper <b>DocumentDb/CosmosDb</b> arguments capabilities.
    /// </summary>
    /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
    public class DocDbArgs<T> : DocDbArgs where T : class, new() { }
}