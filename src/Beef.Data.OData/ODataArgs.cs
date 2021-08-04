// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using AutoMapper;
using Beef.Entities;
using Beef.Mapper;
using System;
using System.Net;

namespace Beef.Data.OData
{
    /// <summary>
    /// Enables the <b>OData</b> arguments capabilities.
    /// </summary>
    public interface IODataArgs
    {
        /// <summary>
        /// Gets the <see cref="PagingResult"/>.
        /// </summary>
        PagingResult? Paging { get; }

        /// <summary>
        /// Gets the <i>AutoMapper</i> <see cref="IMapper"/>.
        /// </summary>
        IMapper Mapper { get; }

        /// <summary>
        /// Gets the entity collection name.
        /// </summary>
        string? CollectionName { get; }

        /// <summary>
        /// Indicates that a <c>null</c> is to be returned where the <b>response</b> has a <see cref="HttpStatusCode"/> of <see cref="HttpStatusCode.NotFound"/>.
        /// </summary>
        bool NullOnNotFoundResponse { get; }

        /// <summary>
        /// Gets the <b>OData</b> keys from the specified keys.
        /// </summary>
        /// <param name="keys">The key values.</param>
        /// <returns>The OData key values.</returns>
        internal object?[] GetODataKeys(IComparable?[] keys);

        /// <summary>
        /// Gets the <b>OData</b> keys from the entity value.
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <returns>The OData OData key values.</returns>
        internal object?[] GetODataKeys(object value);
    }

    /// <summary>
    /// Provides the base <b>OData</b> arguments capabilities.
    /// </summary>
    public class ODataArgs : IODataArgs
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
        /// Gets the <see cref="IEntityMapper"/>.
        /// </summary>
        IMapper IODataArgs.Mapper => Mapper;

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

        /// <summary>
        /// Gets the <b>OData</b> keys from the specified keys.
        /// </summary>
        /// <param name="keys">The key values.</param>
        /// <returns>The OData key values.</returns>
        object?[] IODataArgs.GetODataKeys(IComparable?[] keys)
        {
            if (keys == null || keys.Length == 0)
                throw new ArgumentNullException(nameof(keys));

            return keys;
        }

        /// <summary>
        /// Gets the converted <b>OData</b> keys from the entity value.
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <returns>The OData OData key values.</returns>
        object?[] IODataArgs.GetODataKeys(object value) => value switch
        {
            IStringIdentifier si => new object?[] { si.Id! },
            IGuidIdentifier gi => new object?[] { gi.Id },
            IInt32Identifier ii => new object?[] { ii.Id! },
            IInt64Identifier il => new object?[] { il.Id! },
            IUniqueKey uk => uk.UniqueKey.Args,
            _ => throw new NotSupportedException($"Value Type must be {nameof(IStringIdentifier)}, {nameof(IGuidIdentifier)}, {nameof(IInt32Identifier)}, {nameof(IInt64Identifier)}, or {nameof(IUniqueKey)}."),
        };
    }
}