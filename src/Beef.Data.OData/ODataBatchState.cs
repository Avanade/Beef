// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Data.OData
{
    /// <summary>
    /// Represents the <b>OData</b> batch state.
    /// </summary>
    public enum ODataBatchState
    {
        /// <summary>
        /// Identifies that a batch is ready to accept request.
        /// </summary>
        Ready,

        /// <summary>
        /// Identifies that the batch is being sent; can not longer accept requests.
        /// </summary>
        Sending,

        /// <summary>
        /// Identifies that the batch has been sent; can not longer accept requests.
        /// </summary>
        Sent
    }
}
