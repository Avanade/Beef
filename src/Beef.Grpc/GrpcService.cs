// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using grpc = Grpc.Core;

namespace Beef.Grpc
{
    /// <summary>
    /// Provides the base <b>gRPC</b> Service (server) capability.
    /// </summary>
    public abstract class GrpcServiceBase
    {
        /// <summary>
        /// Gets or sets the <see cref="HttpStatusCode"/> for an unhandled <see cref="Exception"/>.
        /// </summary>
        public static grpc.StatusCode UnhandledExceptionStatusCode { get; set; } = grpc.StatusCode.Internal;

        /// <summary>
        /// Gets or sets the message for an unhandled <see cref="Exception"/>.
        /// </summary>
        public static string UnhandledExceptionMessage { get; set; } = "An unexpected internal server error has occurred.";

        /// <summary>
        /// Indicates whether to include the unhandled <see cref="Exception"/> details in the response.
        /// </summary>
        public static bool IncludeUnhandledExceptionInResponse { get; set; } = false;

        /// <summary>
        /// Gets the <see cref="ExecutionContext.Properties"/> key for storing <c>this</c> (<see cref="GrpcServiceBase"/>) request within the <see cref="ExecutionContext"/>.
        /// </summary>
        public const string ExecutionContextPropertyKey = "Beef.Grpc.GrpcServiceBase";

        /// <summary>
        /// Gets or sets the <see cref="Exception"/> handler that throws a  corresponding <see cref="grpc.RpcException"/> (by default set up to execute <see cref="ThrowRpcExceptionFromException(GrpcServiceBase, Exception)"/>).
        /// </summary>
        public Action<GrpcServiceBase, Exception> ExceptionHandler { get; set; } = (gs, ex) => ThrowRpcExceptionFromException(gs, ex);

        /// <summary>
        /// Create <see cref="grpc.Status"/> from the <see cref="Exception"/>.
        /// </summary>
        /// <param name="service">The <see cref="GrpcServiceBase"/>.</param>
        /// <param name="exception">The <see cref="Exception"/>.</param>
        /// <returns>The corresponding <see cref="grpc.Status"/>.</returns>
        public static void ThrowRpcExceptionFromException(GrpcServiceBase service, Exception exception)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            IBusinessException? ex = null;

            // Unwind to a known exception type if we can.
            if (exception is IBusinessException)
                ex = exception as IBusinessException;

            if (ex == null && exception is AggregateException aex)
            {
                if (aex.InnerExceptions.Count == 1 && aex.InnerException is IBusinessException)
                    ex = aex.InnerException as IBusinessException;
            }

            // Where it is not known then "action" as unhandled.
            if (ex != null)
            {
                if (ex.ShouldBeLogged)
                    Diagnostics.Logger.Create<GrpcServiceBase>().LogError(exception, UnhandledExceptionMessage);

                grpc.Status? status = ex.ErrorType switch
                {
                    ErrorType.AuthenticationError => new grpc.Status(grpc.StatusCode.Unauthenticated, exception.Message),
                    ErrorType.AuthorizationError => new grpc.Status(grpc.StatusCode.PermissionDenied, exception.Message),
                    ErrorType.BusinessError => new grpc.Status(grpc.StatusCode.InvalidArgument, exception.Message),
                    ErrorType.ConcurrencyError => new grpc.Status(grpc.StatusCode.Aborted, exception.Message),
                    ErrorType.ConflictError => new grpc.Status(grpc.StatusCode.FailedPrecondition, exception.Message),
                    ErrorType.DuplicateError => new grpc.Status(grpc.StatusCode.AlreadyExists, exception.Message),
                    ErrorType.NotFoundError => new grpc.Status(grpc.StatusCode.NotFound, exception.Message),
                    ErrorType.ValidationError => new grpc.Status(grpc.StatusCode.InvalidArgument, exception.Message),
                    _ => null,
                };

                if (status != null)
                {
                    service.Context.ResponseTrailers.Add(GrpcConsts.ErrorTypeHeaderName, ex.ErrorType.ToString());
                    service.Context.ResponseTrailers.Add(GrpcConsts.ErrorCodeHeaderName, ((int)ex.ErrorType).ToString(System.Globalization.CultureInfo.InvariantCulture));

                    if (ex is ValidationException vex)
                        AddMessagesToResponseTrailers(service, vex.Messages);

                    throw new grpc.RpcException(status.Value);
                }
            }

            if (ex == null)
                Diagnostics.Logger.Create<GrpcServiceBase>().LogError(exception, UnhandledExceptionMessage);

            throw new grpc.RpcException(new grpc.Status(UnhandledExceptionStatusCode, IncludeUnhandledExceptionInResponse ? exception.ToString() : UnhandledExceptionMessage));
        }

        /// <summary>
        /// Adds the messages to response trailers as a binary byte array.
        /// </summary>
        private static void AddMessagesToResponseTrailers(GrpcServiceBase service, MessageItemCollection? messages)
        {
            if (messages == null || messages.Count == 0)
                return;

            using var ms = new MemoryStream();
            using var bdw = new BsonDataWriter(ms);
            var js = new JsonSerializer();
            js.Serialize(bdw, new GrpcMessages { Messages = messages });

            service.Context.ResponseTrailers.Add(GrpcConsts.MessagesHeaderName, ms.ToArray());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcServiceBase"/> class.
        /// </summary>
        /// <param name="context">The underlying <see cref="grpc.ServerCallContext"/>.</param>
        /// <param name="operationType">The <see cref="Beef.OperationType"/>.</param>
        /// <param name="statusCode">The primary <see cref="HttpStatusCode"/>.</param>
        /// <param name="alternateStatusCode">The alternate <see cref="HttpStatusCode"/> (where supported; i.e. not <c>null</c>).</param>
        /// <param name="memberName">The method or property name of the caller.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        protected GrpcServiceBase(grpc.ServerCallContext context, OperationType operationType,
            HttpStatusCode statusCode, HttpStatusCode? alternateStatusCode = null,
            [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            OperationType = operationType;
            StatusCode = statusCode;
            AlternateStatusCode = alternateStatusCode;
            CallerMemberName = memberName!;
            CallerFilePath = filePath!;
            CallerLineNumber = lineNumber;

            // Add to the ExecutionContext in case we need access to the originating request at any stage.
            ExecutionContext.Current.Properties.Add(ExecutionContextPropertyKey, this);
        }

        /// <summary>
        /// Gets the underlying <see cref="grpc.ServerCallContext"/>.
        /// </summary>
        public grpc.ServerCallContext Context { get; }

        /// <summary>
        /// Gets the <see cref="Beef.OperationType"/>.
        /// </summary>
        public OperationType OperationType { get; private set; }

        /// <summary>
        /// Gets the primary <see cref="HttpStatusCode"/>.
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }

        /// <summary>
        /// Gets the alternate <see cref="HttpStatusCode"/> (where supported; i.e. not <c>null</c>).
        /// </summary>
        public HttpStatusCode? AlternateStatusCode { get; private set; }

        /// <summary>
        /// Gets the method or property name of the caller.
        /// </summary>
        protected string CallerMemberName { get; private set; }

        /// <summary>
        /// Gets the full path of the source file that contains the caller.
        /// </summary>
        protected string CallerFilePath { get; private set; }

        /// <summary>
        /// Gets the line number in the source file at which the method is called.
        /// </summary>
        protected int CallerLineNumber { get; private set; }

        /// <summary>
        /// Executes the <paramref name="func"/> asynchronously where there is no result.
        /// </summary>
        /// <param name="func">The function to invoke.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
        protected Task ExecuteResultAsync(Func<Task> func)
        {
            return GrpcInvoker.Current.InvokeAsync(this, () => ExecuteResultAsyncInternal(func),
                memberName: CallerMemberName, filePath: CallerFilePath, lineNumber: CallerLineNumber);
        }

        /// <summary>
        /// Executes the <paramref name="func"/> asynchronously where there is a <typeparamref name="TResult"/>.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="func">The function to invoke.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
        protected Task<TResult> ExecuteResultAsync<TResult>(Func<Task<TResult>> func)
        {
            return GrpcInvoker.Current.InvokeAsync(this, () => ExecuteResultAsyncInternal(func),
                memberName: CallerMemberName, filePath: CallerFilePath, lineNumber: CallerLineNumber);
        }

        /// <summary>
        /// Does the actual execution of the <paramref name="func"/> asynchronously where there is no result.
        /// </summary>
        [DebuggerStepThrough()]
        private async Task ExecuteResultAsyncInternal(Func<Task> func)
        {
            try
            {
                ExecutionContext.Current.OperationType = OperationType;
                await func().ConfigureAwait(false);
                HandleResponseStatus(StatusCode);
            }
            catch (grpc.RpcException) { throw; }
            catch (Exception ex)
            {
                if (ex is NotFoundException && OperationType == OperationType.Delete)
                    HandleResponseStatus(HttpStatusCode.NoContent);

                ExceptionHandler(this, ex);
                throw;
            }
        }

        /// <summary>
        /// Does the actual execution of the <paramref name="func"/> asynchronously where there is a <typeparamref name="TResult"/>.
        /// </summary>
        //[DebuggerStepThrough()]
        private async Task<TResult> ExecuteResultAsyncInternal<TResult>(Func<Task<TResult>> func)
        {
            try
            {
                ExecutionContext.Current.OperationType = OperationType;
                TResult result = await func().ConfigureAwait(false);

                if (result == null)
                {
                    if (AlternateStatusCode.HasValue)
                        HandleResponseStatus(AlternateStatusCode.Value);
                    else
                        throw new InvalidOperationException("Function has not returned a result; no AlternateStatusCode has been configured to return.");
                }
                else
                    HandleResponseStatus(StatusCode);

                return result;
            }
            catch (grpc.RpcException) { throw; }
            catch (Exception ex)
            {
                if (ex is NotFoundException && OperationType == OperationType.Delete)
                {
                    HandleResponseStatus(HttpStatusCode.NoContent);
                    return typeof(TResult) == typeof(Google.Protobuf.WellKnownTypes.Empty) ? (TResult)(object)new Google.Protobuf.WellKnownTypes.Empty() : default!;
                }

                ExceptionHandler(this, ex);
                throw;
            }
        }

        /// <summary>
        /// Handle the status based on the corresponding <see cref="HttpStatusCode"/>.
        /// </summary>
        private void HandleResponseStatus(HttpStatusCode statusCode)
        {
            Context.ResponseTrailers.Add(GrpcConsts.HttpStatusCodeHeaderName, ((int)statusCode).ToString(System.Globalization.CultureInfo.InvariantCulture));

            switch (statusCode)
            {
                case HttpStatusCode.OK:
                case HttpStatusCode.Created:
                case HttpStatusCode.NoContent:
                    AddMessagesToResponseTrailers(this, ExecutionContext.Current?.Messages);
                    break;

                case HttpStatusCode.NotFound:
                    throw new NotFoundException();

                default:
                    throw new InvalidOperationException($"Unable to map HttpStatusCode '{statusCode}' to an equivalent gRPC status code.");
            }
        }
    }

    /// <summary>
    /// Enables an <see cref="ExecuteAsync"/> with no result.
    /// </summary>
    public sealed class GrpcService : GrpcServiceBase
    {
        private readonly Func<Task> _func;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcService"/> class.
        /// </summary>
        /// <param name="context">The <see cref="grpc.ServerCallContext"/>.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="operationType">The <see cref="Beef.OperationType"/>.</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        public GrpcService(grpc.ServerCallContext context, Func<Task> func, OperationType operationType = OperationType.Unspecified,
            HttpStatusCode statusCode = HttpStatusCode.NoContent,
            [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            : base(context, operationType, statusCode, statusCode, memberName, filePath, lineNumber)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        /// <summary>
        /// Executes the operation asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
        public Task ExecuteAsync()
        {
            return ExecuteResultAsync(_func);
        }
    }

    /// <summary>
    /// Enables an <see cref="ExecuteAsync"/> with a <typeparamref name="TResult"/> result.
    /// </summary>
    /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
    public class GrpcService<TResult> : GrpcServiceBase
    {
        private readonly Func<Task<TResult>> _func;

        /// <summary>
        /// Initializes a new instance of the <see cref="GrpcService{TResult}"/> class.
        /// </summary>
        /// <param name="context">The <see cref="grpc.ServerCallContext"/>.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="operationType">The <see cref="Beef.OperationType"/>.</param>
        /// <param name="statusCode">The primary <see cref="HttpStatusCode"/> when there is a result.</param>
        /// <param name="alternateStatusCode">The alternate <see cref="HttpStatusCode"/> when there is no result (where supported; i.e. not <c>null</c>).</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        public GrpcService(grpc.ServerCallContext context, Func<Task<TResult>> func, OperationType operationType = OperationType.Unspecified,
            HttpStatusCode statusCode = HttpStatusCode.OK, HttpStatusCode? alternateStatusCode = HttpStatusCode.NotFound,
            [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            : base(context, operationType, statusCode, alternateStatusCode, memberName, filePath, lineNumber)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        /// <summary>
        /// Executes the operation asynchronously.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
        public Task<TResult> ExecuteAsync()
        {
            return ExecuteResultAsync(_func);
        }
    }
}