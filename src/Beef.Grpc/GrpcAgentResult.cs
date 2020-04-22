// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.WebApi;
using System;

namespace Beef.Grpc
{
    /// <summary>
    /// Represents a result for the <see cref="GrpcServiceAgentBase"/>.
    /// </summary>
    public class GrpcAgentResult : IWebApiAgentResult
    {
#pragma warning disable CA2227 // Collection properties should be read only; by-design, can be updated.
        /// <summary>
        /// Gets or sets the <see cref="MessageItemCollection"/>.
        /// </summary>
        public MessageItemCollection? Messages { get; set; }
#pragma warning restore CA2227

        /// <summary>
        /// Gets or sets the known <see cref="ErrorType"/>; otherwise, <c>null</c> indicates an unknown error type.
        /// </summary>
        public ErrorType? ErrorType { get; set; }

        /// <summary>
        /// Gets or sets the error message for the corresponding <see cref="ErrorType"/>.
        /// </summary>
        public string? ErrorMessage { get; set; }

        /// <summary>
        /// Indicates whether the request was successful.
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// Throws an exception if the request was not successful (see <see cref="IsSuccess"/>).
        /// </summary>
        /// <returns>The <see cref="WebApiAgentResult"/> instance to support fluent/chaining usage.</returns>
        IWebApiAgentResult IWebApiAgentResult.ThrowOnError() => ThrowOnError();

        /// <summary>
        /// Throws an exception if the request was not successful (see <see cref="IsSuccess"/>).
        /// </summary>
        /// <returns>The <see cref="WebApiAgentResult"/> instance to support fluent/chaining usage.</returns>
        public IWebApiAgentResult ThrowOnError()
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Represents a result for the <see cref="GrpcServiceAgentBase"/>.
    /// </summary>
    public class GrpcAgentResult<T> : GrpcAgentResult, IWebApiAgentResult<T>
    {
        private bool _isValueSet = false;
        private T _value = default!;

        /// <summary>
        /// Gets the response value.
        /// </summary>
        /// <remarks>Performs a <see cref="IWebApiAgentResult.ThrowOnError"/> before returning the resuluting value.</remarks>
        public T Value
        {
            get
            {
                ThrowOnError();
                return _value;
            }
        }
    }
}