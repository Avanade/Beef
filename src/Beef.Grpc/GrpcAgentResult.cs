// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.WebApi;
using Google.Protobuf;
using Grpc.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace Beef.Grpc
{
    /// <summary>
    /// Represents a result for the <see cref="GrpcAgentBase{TClient}"/>.
    /// </summary>
    public class GrpcAgentResult : IWebApiAgentResult
    {
        private MessageItemCollection? _messages = null;
        private HttpStatusCode? _statusCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcAgentResult"/> class that is considered successful.
        /// </summary>
        /// <param name="status">The <see cref="Status"/>.</param>
        /// <param name="trailers">The trailers <see cref="Metadata"/>.</param>
        /// <param name="request">The gRPC request value.</param>
        /// <param name="response">The gRPC response value (optional).</param>
        public GrpcAgentResult(Status status, Metadata trailers, IMessage? request, object? response = null)
        {
            IsSuccess = true;
            Status = status;
            ResponseTrailers = trailers;
            Request = request;
            Response = response;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcAgentResult"/> class that is considered unsuccessful.
        /// </summary>
        /// <param name="rex">The <see cref="RpcException"/>.</param>
        /// <param name="request">The gRPC request value.</param>
        public GrpcAgentResult(RpcException rex, IMessage? request)
        {
            Exception = Check.NotNull(rex, nameof(rex));
            Status = Exception.Status;
            ErrorMessage = Exception.Status.Detail;
            ResponseTrailers = Exception.Trailers;
            Request = request;

            if (ResponseTrailers.Count > 0)
            {
                var t = ResponseTrailers.Where(x => x.Key == GrpcConsts.ErrorTypeHeaderName).SingleOrDefault();
                if (System.Enum.TryParse<ErrorType>(t.Value, out var et))
                    ErrorType = et;
            }

            IsSuccess = false;
        }

        /// <summary>
        /// Gets the gRPC request value.
        /// </summary>
        public IMessage? Request { get; private set; }

        /// <summary>
        /// Gets the gRPC response value (optional).
        /// </summary>
        public object? Response { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="MessageItemCollection"/>.
        /// </summary>
        public MessageItemCollection Messages
        {
            get
            {
                if (_messages != null)
                    return _messages;

                _messages = new MessageItemCollection();
                var t = ResponseTrailers.Where(x => x.Key == GrpcConsts.MessagesHeaderName).SingleOrDefault();
                if (t == null)
                    return _messages;

                using var ms = new MemoryStream(t.ValueBytes);
                using var bdr = new BsonDataReader(ms);
                var js = new JsonSerializer();
                var msgs = js.Deserialize<GrpcMessages>(bdr);
                if (msgs?.Messages != null)
                    _messages.AddRange(msgs.Messages);

                return _messages;
            }
        }


        /// <summary>
        /// Gets the gRPC status.
        /// </summary>
        public Status Status { get; private set; }

        /// <summary>
        /// Gets the known <see cref="ErrorType"/>; otherwise, <c>null</c> indicates an unknown error type.
        /// </summary>
        public ErrorType? ErrorType { get; private set; }

        /// <summary>
        /// Gets the error message for the corresponding <see cref="ErrorType"/>.
        /// </summary>
        public string? ErrorMessage { get; private set; }

        /// <summary>
        /// Gets the underlying <see cref="RpcException"/>.
        /// </summary>
        public RpcException? Exception { get; private set; }

        /// <summary>
        /// Indicates whether the request was successful.
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// Gets the <see cref="StatusCode"/> that was returned in the <see cref="ResponseTrailers"/> (uses <see cref="GrpcConsts.HttpStatusCodeHeaderName"/>). Where no value was returned will
        /// attempt to infer from <see cref="Status"/>; otherwise, will default to either <see cref="HttpStatusCode.OK"/> or <see cref="HttpStatusCode.InternalServerError"/> depending on <see cref="IsSuccess"/>.
        /// </summary>
        public HttpStatusCode StatusCode
        {
            get 
            {
                if (_statusCode.HasValue)
                    return _statusCode.Value;

                var t = ResponseTrailers.Where(x => x.Key == GrpcConsts.HttpStatusCodeHeaderName).SingleOrDefault();
                if (t != null && int.TryParse(t.Value, out var hsc))
                    _statusCode = (HttpStatusCode)hsc;
                else
                {
                    _statusCode = Status.StatusCode switch
                    {
                        global::Grpc.Core.StatusCode.Unauthenticated => System.Net.HttpStatusCode.Unauthorized,
                        global::Grpc.Core.StatusCode.PermissionDenied => System.Net.HttpStatusCode.Forbidden,
                        global::Grpc.Core.StatusCode.InvalidArgument => System.Net.HttpStatusCode.BadRequest,
                        global::Grpc.Core.StatusCode.Aborted => System.Net.HttpStatusCode.PreconditionFailed,
                        global::Grpc.Core.StatusCode.FailedPrecondition => System.Net.HttpStatusCode.Conflict,
                        global::Grpc.Core.StatusCode.AlreadyExists => System.Net.HttpStatusCode.Conflict,
                        global::Grpc.Core.StatusCode.NotFound => System.Net.HttpStatusCode.NotFound,
                        _ => IsSuccess ? HttpStatusCode.OK : HttpStatusCode.InternalServerError
                    };
                }

                return _statusCode.Value;
            }
        }

        /// <summary>
        /// Gets the response trailers <see cref="Metadata"/>.
        /// </summary>
        public Metadata ResponseTrailers { get; private set; }

        /// <summary>
        /// Throws an exception if the request was not successful (see <see cref="IsSuccess"/>).
        /// </summary>
        /// <returns>The <see cref="WebApiAgentResult"/> instance to support fluent/chaining usage.</returns>
        IWebApiAgentResult IWebApiAgentResult.ThrowOnError() => ThrowOnError();

        /// <summary>
        /// Throws an <see cref="GrpcAgentException"/> if the request was not successful (see <see cref="IsSuccess"/>).
        /// </summary>
        /// <returns>The <see cref="WebApiAgentResult"/> instance to support fluent/chaining usage.</returns>
        public IWebApiAgentResult ThrowOnError()
        {
            if (IsSuccess)
                return this;

            // Throw the corresponding beef exception if a known error type.
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
            throw new GrpcAgentException(Exception!.Message, Exception!);
        }
    }

    /// <summary>
    /// Represents a result for the <see cref="GrpcAgentBase{TClient}"/> with a <see cref="Value"/>.
    /// </summary>
    public class GrpcAgentResult<T> : GrpcAgentResult, IWebApiAgentResult<T>
    {
        private readonly T _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcAgentResult{T}"/> class with a <see cref="Value"/> that is considered successful.
        /// </summary>
        /// <param name="status">The <see cref="Status"/>.</param>
        /// <param name="trailers">The trailers <see cref="Metadata"/>.</param>
        /// <param name="request">The gRPC request value.</param>
        /// <param name="response">The gRPC response value.</param>
        /// <param name="value">The response <see cref="Value"/>.</param>
        public GrpcAgentResult(Status status, Metadata trailers, IMessage? request, object? response, T value) : base(status, trailers, request, response) => _value = value;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcAgentResult{T}"/> class that is considered unsuccessful.
        /// </summary>
        /// <param name="rex">The <see cref="RpcException"/>.</param>
        /// <param name="request">The gRPC request value.</param>
        public GrpcAgentResult(RpcException rex, IMessage? request) : base(rex, request) => _value = default!;

        /// <summary>
        /// Indicates whether a <see cref="Value"/> was returned as <see cref="WebApiAgentResult.Content"/>.
        /// </summary>
        public bool HasValue => Comparer<T>.Default.Compare(_value, default!) != 0;

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