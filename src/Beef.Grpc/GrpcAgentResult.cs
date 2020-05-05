// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.WebApi;
using System;

namespace Beef.Grpc
{
    /// <summary>
    /// Represents a result for the <see cref="GrpcServiceAgentBase{TClient}"/>.
    /// </summary>
    public class GrpcAgentResult : IWebApiAgentResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcAgentResult"/> class that is considered successful.
        /// </summary>
        public GrpcAgentResult() => IsSuccess = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcAgentResult"/> class that is considered unsuccessful.
        /// </summary>
        /// <param name="ex">The <see cref="System.Exception"/>.</param>
        public GrpcAgentResult(Exception ex)
        {
            Exception = Check.NotNull(ex, nameof(ex));
            IsSuccess = true;
        }

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
        /// Gets or set the underlying <see cref="System.Exception"/>.
        /// </summary>
        public Exception? Exception { get; set; }

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
        /// Throws an <see cref="GrpcServiceAgentException"/> if the request was not successful (see <see cref="IsSuccess"/>).
        /// </summary>
        /// <returns>The <see cref="WebApiAgentResult"/> instance to support fluent/chaining usage.</returns>
        public IWebApiAgentResult ThrowOnError()
        {
            if (IsSuccess)
                return this;

            // Throw the beef exception if a known error type.
            if (ErrorType.HasValue)
            {
                switch (ErrorType.Value)
                {
                    case Beef.ErrorType.AuthorizationError:
                        throw new AuthorizationException(ErrorMessage);

                    case Beef.ErrorType.BusinessError:
                        throw new BusinessException(ErrorMessage);

                    case Beef.ErrorType.ConcurrencyError:
                        throw new ConcurrencyException(ErrorMessage);

                    case Beef.ErrorType.ConflictError:
                        throw new ConflictException(ErrorMessage);

                    case Beef.ErrorType.NotFoundError:
                        throw new NotFoundException(ErrorMessage);

                    case Beef.ErrorType.ValidationError:
                        throw new ValidationException(ErrorMessage, Messages ?? new MessageItemCollection());

                    case Beef.ErrorType.DuplicateError:
                        throw new DuplicateException(ErrorMessage);

                    case Beef.ErrorType.AuthenticationError:
                        throw new AuthenticationException(ErrorMessage);
                }
            }

            // Throw the originating exception.
            throw new GrpcServiceAgentException(Exception!.Message, Exception!);
        }
    }

    /// <summary>
    /// Represents a result for the <see cref="GrpcServiceAgentBase{TClient}"/> with a <see cref="Value"/>.
    /// </summary>
    public class GrpcAgentResult<T> : GrpcAgentResult, IWebApiAgentResult<T>
    {
        private readonly T _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcAgentResult{T}"/> class with a <see cref="Value"/> that is considered successful.
        /// </summary>
        public GrpcAgentResult(T value) : base() => _value = value;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcAgentResult{T}"/> class that is considered unsuccessful.
        /// </summary>
        /// <param name="ex">The <see cref="System.Exception"/>.</param>
        public GrpcAgentResult(Exception ex) : base(ex) => _value = default!;

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