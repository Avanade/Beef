// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Data.OData
{
    /// <summary>
    /// Determines how the ETag IF-MATCH condition processing should occur specifically for an HTTP-UPDATE. 
    /// </summary>
    /// <remarks>ETags are used on updating requests to prevent inadvertent modification of the wrong version of a resource. As a special case, the value "*" matches any current
    /// entity of the resource. If none of the entity tags match, or if "*" is given and no current entity exists, the server MUST NOT perform the requested method, and MUST
    /// return a 412 (Precondition Failed) response. Where no IF-MATCH is supplied (see <see cref="Upsert"/>) and the entity does not exist it will be created (resulting in an Upsert).</remarks>
    public enum ODataIfMatch
    {
        /// <summary>
        /// Specifies to use the ETag value condition (e.g. <code>If-Match: "entity-tag"</code>') to ensure concurrency for an HTTP-UPDATE and HTTP-DELETE. Where no ETag value has
        /// been provided it will default to <see cref="UpdateAny"/>.
        /// </summary>
        UpdateEtag,

        /// <summary>
        /// Specifies to use the <b>any</b> ETag condition (e.g. <code>If-Match: *</code>') to ensure that the entity must exist for an HTTP-UPDATE and HTTP-DELETE.
        /// </summary>
        UpdateAny,

        /// <summary>
        /// Specifies that no ETag condition is to be used; resulting in either an Update if the entity exists, or an Insert if it does not (i.e. Upsert).
        /// </summary>
        Upsert
    }
}
