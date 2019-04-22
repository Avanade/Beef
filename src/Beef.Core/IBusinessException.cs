// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System.Net;

namespace Beef
{
    /// <summary>
    /// Enables the standard <i>Beef</i> exception capabilities.
    /// </summary>
    public interface IBusinessException
    {
        /// <summary>
        /// Gets the <see cref="ErrorType"/>.
        /// </summary>
        ErrorType ErrorType { get; }

        /// <summary>
        /// Gets the corresponding <see cref="HttpStatusCode"/>.
        /// </summary>
        HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Indicates whether the <see cref="IBusinessException"/> should be logged.
        /// </summary>
        bool ShouldBeLogged { get; }
    }
}
