// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using AutoMapper;
using Beef.Entities;
using System.Net;

namespace Beef.Data.OData
{
    /// <summary>
    /// Provides the base <b>OData</b> arguments capabilities.
    /// </summary>
    public class ODataArgs
    {
        /// <summary>
        /// Creates a new instance of the <see cref="ODataArgs"/> class with a <paramref name="mapper"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="collectionName">The entity collection name where overriding default.</param>
        /// <returns>The <see cref="ODataArgs"/>.</returns>
        public static ODataArgs Create(IMapper mapper, string? collectionName = null)
            => new ODataArgs(mapper, collectionName);

        /// <summary>
        /// Creates a new instance of the <see cref="ODataArgs"/> class with a <paramref name="mapper"/> and <paramref name="paging"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="collectionName">The entity collection name where overriding default.</param>
        /// <returns>The <see cref="ODataArgs"/>.</returns>
        public static ODataArgs Create(IMapper mapper, PagingArgs paging, string? collectionName = null)
            => new ODataArgs(mapper, paging, collectionName);

        /// <summary>
        /// Creates a new instance of the <see cref="ODataArgs"/> class with a <paramref name="mapper"/> and <paramref name="paging"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="collectionName">The entity collection name where overriding default.</param>
        /// <returns>The <see cref="ODataArgs"/>.</returns>
        public static ODataArgs Create(IMapper mapper, PagingResult paging, string? collectionName = null)
            => new ODataArgs(mapper, paging, collectionName);

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataArgs"/> class with a <paramref name="mapper"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="collectionName">The entity collection name where overriding default.</param>
        public ODataArgs(IMapper mapper, string? collectionName = null)
        {
            Mapper = Check.NotNull(mapper, nameof(mapper));
            CollectionName = collectionName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataArgs"/> class with a <paramref name="mapper"/> and <paramref name="paging"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="collectionName">The entity collection name where overriding default.</param>
        public ODataArgs(IMapper mapper, PagingArgs paging, string? collectionName = null) 
            : this(mapper, new PagingResult(Check.NotNull(paging, nameof(paging))), collectionName) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataArgs"/> class with a <paramref name="mapper"/> and <paramref name="paging"/>.
        /// </summary>
        /// <param name="mapper">The <i>AutoMapper</i> <see cref="IMapper"/>.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="collectionName">The entity collection name where overriding default.</param>
        public ODataArgs(IMapper mapper, PagingResult paging, string? collectionName = null) : this(mapper, collectionName)
        {
            Paging = Check.NotNull(paging, nameof(paging));
        }

        /// <summary>
        /// Indicates that a <c>null</c> is to be returned where the <b>response</b> has a <see cref="HttpStatusCode"/> of <see cref="HttpStatusCode.NotFound"/>.
        /// </summary>
        public bool NullOnNotFoundResponse { get; set; } = true;

        /// <summary>
        /// Gets or sets the <see cref="IMapper"/>.
        /// </summary>
        public IMapper Mapper { get; private set; }

        /// <summary>
        /// Gets the <see cref="PagingResult"/> (where paging is required for a <b>query</b>).
        /// </summary>
        public PagingResult? Paging { get; private set; }

        /// <summary>
        /// Gets or sets the entity collection name where overriding the default.
        /// </summary>
        public string? CollectionName { get; set; }
    }
}