// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Mapper;
using System;
using System.Collections.Specialized;
using System.Net;
using Soc = Simple.OData.Client;

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
        /// Gets the <see cref="IEntityMapper"/>.
        /// </summary>
        IEntityMapper Mapper { get; }

        /// <summary>
        /// Gets the entity collection name.
        /// </summary>
        string? CollectionName { get; }

        /// <summary>
        /// Indicates that a <c>null</c> is to be returned where the <b>response</b> has a <see cref="HttpStatusCode"/> of <see cref="HttpStatusCode.NotFound"/>.
        /// </summary>
        bool NullOnNotFoundResponse { get; }
    }

    /// <summary>
    /// Provides the base <b>OData</b> arguments capabilities.
    /// </summary>
    public class ODataArgs<T, TModel> : IODataArgs where T : class, new() where TModel : class, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataArgs{T, TModel}"/> class with a <paramref name="mapper"/>.
        /// </summary>
        /// <param name="mapper">The <see cref="IEntityMapper{T, TModel}"/>.</param>
        /// <param name="collectionName">The entity collection name where overridding default.</param>
        public ODataArgs(IEntityMapper<T, TModel> mapper, string? collectionName = null)
        {
            Mapper = Check.NotNull(mapper, nameof(mapper));
            CollectionName = collectionName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataArgs{T, TModel}"/> class with a <paramref name="mapper"/> and <paramref name="paging"/>.
        /// </summary>
        /// <param name="mapper">The <see cref="IEntityMapper{T, TModel}"/>.</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <param name="collectionName">The entity collection name where overridding default.</param>
        public ODataArgs(IEntityMapper<T, TModel> mapper, PagingArgs paging, string? collectionName = null) 
            : this(mapper, new PagingResult(Check.NotNull(paging, nameof(paging))), collectionName) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataArgs{T, TModel}"/> class.
        /// </summary>
        /// <param name="mapper">The <see cref="IEntityMapper{T, TModel}"/>.</param>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="collectionName">The entity collection name where overridding default.</param>
        public ODataArgs(IEntityMapper<T, TModel> mapper, PagingResult paging, string? collectionName = null) : this(mapper, collectionName)
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
        IEntityMapper IODataArgs.Mapper => Mapper;

        /// <summary>
        /// Gets or sets the <see cref="IEntityMapper{T, TModel}"/>.
        /// </summary>
        public IEntityMapper<T, TModel> Mapper { get; private set; }

        /// <summary>
        /// Gets the <see cref="PagingResult"/> (where paging is required for a <b>query</b>).
        /// </summary>
        public PagingResult? Paging { get; private set; }

        /// <summary>
        /// Gets or sets the entity collection name where overridding the default.
        /// </summary>
        public string? CollectionName { get; set; }

        /// <summary>
        /// Gets the converted <b>OData</b> keys from the specified keys.
        /// </summary>
        /// <param name="keys">The key values.</param>
        /// <returns>The converted OData key values.</returns>
        internal object[] GetODataKeys(IComparable?[] keys)
        {
            if (keys == null || keys.Length == 0)
                throw new ArgumentNullException(nameof(keys));

            if (keys.Length != Mapper.UniqueKey.Count)
                throw new ArgumentException($"The specified keys count '{keys.Length}' does not match the Mapper UniqueKey count '{Mapper.UniqueKey.Count}'.", nameof(keys));

            var okeys = new object[keys.Length];
            for (int i = 0; i < keys.Length; i++)
            {
                okeys[i] = Mapper.UniqueKey[0].ConvertToDestValue(keys[0], OperationTypes.Unspecified)!;
            }

            return okeys;
        }

        /// <summary>
        /// Gets the converted <b>OData</b> keys from the entity value.
        /// </summary>
        /// <param name="value">The entity value.</param>
        /// <returns>The OData OData key values.</returns>
        internal object[] GetODataKeys(T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var okeys = new object[Mapper.UniqueKey.Count];
            for (int i = 0; i < Mapper.UniqueKey.Count; i++)
            {
                var v = Mapper.UniqueKey[i].GetSrceValue(value, OperationTypes.Unspecified);
                okeys[i] = Mapper.UniqueKey[i].ConvertToDestValue(v, OperationTypes.Unspecified)!;
            }

            return okeys;
        }
    }
}