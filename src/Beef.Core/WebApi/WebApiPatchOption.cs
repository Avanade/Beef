// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.WebApi
{
    /// <summary>
    /// Specifies the <b>HTTP PATCH</b> option.
    /// </summary>
    public enum WebApiPatchOption
    {
        /// <summary>
        /// Indicates that no valid patch option has been specified.
        /// </summary>
        NotSpecified,

        /// <summary>
        /// Indicates a <b>json-patch</b>. Requires a Content-Type of 'application/json-patch+json'. See https://tools.ietf.org/html/rfc6902 for more details.
        /// </summary>
        JsonPatch,

        /// <summary>
        /// Indicates a <b>merge-patch</b>. Requires a Content-Type of 'application/merge-patch+json'. See https://tools.ietf.org/html/rfc7396 for more details.
        /// </summary>
        MergePatch
    }
}