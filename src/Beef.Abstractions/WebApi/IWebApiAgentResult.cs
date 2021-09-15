// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using System.Net;

namespace Beef.WebApi
{
    /// <summary>
    /// Provides the common <b>Web API</b> result.
    /// </summary>
    public interface IWebApiAgentResult
    {
        /// <summary>
        /// Gets or sets the <see cref="HttpStatusCode"/>.
        /// </summary>
        HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets or sets the <see cref="MessageItemCollection"/>.
        /// </summary>
        MessageItemCollection? Messages { get; }

        /// <summary>
        /// Gets or sets the known <see cref="ErrorType"/>; otherwise, <c>null</c> indicates an unknown error type.
        /// </summary>
        ErrorType? ErrorType { get; }

        /// <summary>
        /// Gets or sets the error message for the corresponding <see cref="ErrorType"/>.
        /// </summary>
        string? ErrorMessage { get; }

        /// <summary>
        /// Indicates whether the request was successful.
        /// </summary>
        bool IsSuccess { get; }

        /// <summary>
        /// Throws an exception if the request was not successful (see <see cref="IsSuccess"/>).
        /// </summary>
        /// <returns>The <see cref="WebApiAgentResult"/> instance to support fluent/chaining usage.</returns>
        IWebApiAgentResult ThrowOnError();
    }

    /// <summary>
    /// Provides the common <b>Web API</b> result with a <see cref="Value"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="Value"/> <see cref="System.Type"/>.</typeparam>
    public interface IWebApiAgentResult<T> : IWebApiAgentResult
    {
        /// <summary>
        /// Gets the response value.
        /// </summary>
        /// <remarks>Performs a <see cref="IWebApiAgentResult.ThrowOnError"/> before returning the resuluting value.</remarks>
        T Value { get; }
    }
}