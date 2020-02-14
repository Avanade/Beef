// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Json;
using Beef.WebApi;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Beef.AspNetCore.WebApi
{
    #region WebApiActionBase

    /// <summary>
    /// Provides the base <b>Web API</b> <see cref="IActionResult"/> capability.
    /// </summary>
    public abstract class WebApiActionBase : IActionResult
    {
        private const string AsciiArtRobot = " d[ o_0 ]b ";

        /// <summary>
        /// Gets the <see cref="ExecutionContext.Properties"/> key for storing <c>this</c> (<see cref="WebApiActionBase"/>) request within the <see cref="ExecutionContext"/>.
        /// </summary>
        public const string ExecutionContextPropertyKey = "Beef.AspNetCore.WebApi.WebApiActionBase";

        /// <summary>
        /// Indicates whether the <see cref="ExecutionContext"/> <see cref="ExecutionContext.Messages"/> headers should be included for an <see cref="IBusinessException"/>
        /// (defaults to <c>true</c>).
        /// </summary>
        public static bool IncludeExecutionContextMessagesForAnIBusinessException { get; set; } = true;

        /// <summary>
        /// Gets or sets the exception handler function that creates a <see cref="IActionResult"/> from an <see cref="Exception"/> (defaults to <see cref="CreateResultFromException(ActionContext, Exception)"/>.
        /// </summary>
        /// <remarks>Where no handler is specified or returns a <c>null</c> this indicates that the <see cref="Exception"/> is not handled and the exception will continue to bubble up the stack.</remarks>
        public static Func<ActionContext, Exception, IActionResult?> ExecuteExceptionHandler { get; set; } = CreateResultFromException;

        /// <summary>
        /// Creates a result for an <see cref="Exception"/>; specifically where it is a known/expected <see cref="IBusinessException"/>.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <param name="exception">The <see cref="Exception"/>.</param>
        /// <returns>The <see cref="IActionResult"/> for an <see cref="IBusinessException"/>; otherwise, <c>null</c>.</returns>
        public static IActionResult? CreateResultFromException(ActionContext context, Exception exception)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            IBusinessException? ex = null;

            // Unwind to a known exception type if we can.
            if (exception is IBusinessException)
                ex = exception as IBusinessException;

            if (ex == null && exception is AggregateException)
            {
                var aex = (AggregateException)exception;
                if (aex.InnerExceptions.Count == 1 && aex.InnerException is IBusinessException)
                    ex = aex.InnerException as IBusinessException;
            }

            // Where it is not known then "action" as unhandled.
            if (ex == null)
                return null;

            IActionResult result;
            var messages = IncludeExecutionContextMessagesForAnIBusinessException && ExecutionContext.HasCurrent ? ExecutionContext.Current.Messages : new MessageItemCollection();

            // Where it is known then update the response accordingly.
            switch (ex.ErrorType)
            {
                case ErrorType.ValidationError:
                    var vex = (ValidationException)ex;
                    var msd = new ModelStateDictionary();
                    if (vex.Messages != null)
                    {
                        foreach (var msg in vex.Messages)
                        {
                            // Model 'errors' only added where there is a 'property' specified.
                            if (msg.Type == MessageType.Error && msg.Property != null)
                                msd.AddModelError(msg.Property, msg.Text);
                            else
                                messages.Add(msg);
                        }
                    }

                    result = msd.Count == 0 ? new BadRequestObjectResult(exception.Message) : new BadRequestObjectResult(msd);
                    break;

                default:
                    result = new ObjectResult(exception.Message) { StatusCode = (int)ex.StatusCode };
                    break;
            }

            WebApiControllerHelper.SetExecutionContext(context.HttpContext.Response);
            context.HttpContext.Response.Headers.Add(WebApiConsts.ErrorTypeHeaderName, ex.ErrorType.ToString());
            context.HttpContext.Response.Headers.Add(WebApiConsts.ErrorCodeHeaderName, ((int)ex.ErrorType).ToString(System.Globalization.CultureInfo.InvariantCulture));

            return result;
        }

        /// <summary>
        /// Checks the value and applies any updates based on the HTTP request context.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The value.</returns>
        public static T Value<T>(T value)
        {
            // Get the WebApiActionBase instance from the ExecutionContext.
            if (!EqualityComparer<T>.Default.Equals(value, default) && ExecutionContext.HasCurrent && ExecutionContext.Current.Properties.TryGetValue(ExecutionContextPropertyKey, out var val) && val is WebApiActionBase api)
            {
                // Where the value implements IETag and If-Match is specified, then this should be used as the preference.
                if (api.IfMatchETags != null && api.IfMatchETags.Count > 0 && value is IETag etag && etag != null)
                    etag.ETag = api.IfMatchETags[0];

                api.BodyValue = value;
            }

            return value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiActionBase"/> class.
        /// </summary>
        /// <param name="controller">The initiating <see cref="ControllerBase"/>.</param>
        /// <param name="operationType">The <see cref="Beef.OperationType"/>.</param>
        /// <param name="statusCode">The primary <see cref="HttpStatusCode"/>.</param>
        /// <param name="alternateStatusCode">The alternate <see cref="HttpStatusCode"/> (where supported; i.e. not <c>null</c>).</param>
        /// <param name="memberName">The method or property name of the caller.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        protected WebApiActionBase(ControllerBase controller, OperationType operationType,
            HttpStatusCode statusCode, HttpStatusCode? alternateStatusCode = null,
            [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            Controller = controller ?? throw new ArgumentNullException(nameof(controller));
            OperationType = operationType;
            StatusCode = statusCode;
            AlternateStatusCode = alternateStatusCode;
            CallerMemberName = memberName!;
            CallerFilePath = filePath!;
            CallerLineNumber = lineNumber;

            // Get the list of etags specified in the header.
            if (controller.Request.Headers.TryGetValue("If-None-Match", out var vals))
            {
                var l = new List<string>();
                foreach (var v in vals)
                {
                    var etag = v?.Trim().Trim('"');
                    if (!string.IsNullOrEmpty(etag))
                        l.Add(etag);
                }

                if (l.Count > 0)
                    IfNoneMatchETags = l;
            }

            if (Controller.Request.Headers.TryGetValue("If-Match", out vals))
            {
                var l = new List<string>();
                foreach (var v in vals)
                {
                    var etag = v?.Trim().Trim('"');
                    if (!string.IsNullOrEmpty(etag))
                        l.Add(etag);
                }

                if (l.Count > 0)
                    IfMatchETags = l;
            }

            // Get the paging arguments.
            PagingArgs = WebApiQueryString.CreatePagingArgs(Controller);
            ExecutionContext.Current.PagingArgs = PagingArgs;

            // Get the other request options.
            var (include, exclude) = WebApiQueryString.GetOtherRequestOptions(Controller);
            IncludeFields.AddRange(include);
            ExcludeFields.AddRange(exclude);

            // Add to the ExecutionContext in case we need access to the originating request at any stage.
            ExecutionContext.Current.Properties.Add(ExecutionContextPropertyKey, this);
        }

        /// <summary>
        /// Gets the <see cref="ControllerBase"/>.
        /// </summary>
        public ControllerBase Controller { get; private set; }

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
        /// Gets the <see cref="IETag.ETag"/> values where the <c>If-None-Match</c> header is supplied.
        /// </summary>
        public List<string>? IfNoneMatchETags { get; private set; }

        /// <summary>
        /// Gets the <see cref="IETag.ETag"/> values where the <c>If-Match</c> header is supplied.
        /// </summary>
        public List<string>? IfMatchETags { get; private set; }

        /// <summary>
        /// Gets the <see cref="Entities.PagingArgs"/> for the request.
        /// </summary>
        public PagingArgs PagingArgs { get; private set; }

        /// <summary>
        /// Gets or sets the list of <b>included</b> fields (JSON property names) to limit the serialized data payload (url query string: "$fields=x,y,z").
        /// </summary>
        public List<string> IncludeFields { get; } = new List<string>();

        /// <summary>
        /// Gets or sets the list of <b>excluded</b> fields (JSON property names) to limit the serialized data payload (url query string: "$excludefields=x,y,z").
        /// </summary>
        public List<string> ExcludeFields { get; } = new List<string>();

        /// <summary>
        /// Gets the <see cref="FromBodyAttribute"/> value.
        /// </summary>
        public object? BodyValue { get; internal set; }

        /// <summary>
        /// Executes the result operation of the action method asynchronously.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
        public abstract Task ExecuteResultAsync(ActionContext context);

        /// <summary>
        /// Creates a result for a <see cref="HttpStatusCode"/>.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/>.</param>
        /// <returns>The <see cref="IActionResult"/>.</returns>
        protected static IActionResult CreateResult(ActionContext context, HttpStatusCode statusCode)
        {
            Check.NotNull(context, nameof(context));

            if (statusCode == HttpStatusCode.NotFound)
            {
#pragma warning disable CA1062 // Validate arguments of public methods; see Check above.
                context.HttpContext.Response.Headers.Add(WebApiConsts.ErrorTypeHeaderName, ErrorType.NotFoundError.ToString());
                context.HttpContext.Response.Headers.Add(WebApiConsts.ErrorCodeHeaderName, ((int)ErrorType.NotFoundError).ToString(System.Globalization.CultureInfo.InvariantCulture));
#pragma warning restore CA1062 
            }

            return new StatusCodeResult((int)statusCode);
        }

        /// <summary>
        /// Creates a result for a <see cref="HttpStatusCode"/> and <see cref="JToken"/> value.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/>.</param>
        /// <param name="json">The return <see cref="JToken"/>.</param>
        /// <returns>The <see cref="IActionResult"/>.</returns>
        protected static IActionResult CreateResult(ActionContext context, HttpStatusCode statusCode, JToken? json)
        {
            Check.NotNull(context, nameof(context));
            return new ObjectResult(json) { StatusCode = (int)statusCode };
        }

        /// <summary>
        /// Executes the <paramref name="func"/> asynchronously where there is no result.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <param name="func">The The function to invoke.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
        [DebuggerStepThrough()]
        protected virtual Task ExecuteResultAsync(ActionContext context, Func<Task> func)
        {
            return WebApiControllerInvoker.Default.InvokeAsync(Controller, () => ExecuteResultAsyncInternal(context, func),
                memberName: CallerMemberName, filePath: CallerFilePath, lineNumber: CallerLineNumber);
        }

        /// <summary>
        /// Does the actual execution of the <paramref name="func"/> asynchronously where there is no result.
        /// </summary>
        [DebuggerStepThrough()]
        private async Task ExecuteResultAsyncInternal(ActionContext context, Func<Task> func)
        {
            try
            {
                ExecutionContext.Current.OperationType = OperationType;
                await func().ConfigureAwait(false);

                WebApiControllerHelper.SetExecutionContext(context.HttpContext.Response);
                await CreateResult(context, StatusCode).ExecuteResultAsync(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var ai = ExecuteExceptionHandler == null ? null : ExecuteExceptionHandler(context, ex);
                if (ai == null)
                    throw;

                await ai.ExecuteResultAsync(context).ConfigureAwait(false);
                return;
            }
        }

        /// <summary>
        /// Executes the <paramref name="func"/> asynchronously where there is a <typeparamref name="TResult"/>.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <param name="func">The function to invoke.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
        [DebuggerStepThrough()]
        protected virtual Task ExecuteResultAsync<TResult>(ActionContext context, Func<Task<TResult>> func)
        {
            return WebApiControllerInvoker.Default.InvokeAsync(Controller, () => ExecuteResultAsyncInternal(context, func),
                memberName: CallerMemberName, filePath: CallerFilePath, lineNumber: CallerLineNumber);
        }

        /// <summary>
        /// Does the actual execution of the <paramref name="func"/> asynchronously where there is a <typeparamref name="TResult"/>.
        /// </summary>
        [DebuggerStepThrough()]
        private async Task ExecuteResultAsyncInternal<TResult>(ActionContext context, Func<Task<TResult>> func)
        {
            try
            {
                ExecutionContext.Current.OperationType = OperationType;

                TResult result = await func().ConfigureAwait(false);
                var (json, etag) = CreateJsonResultAndETag(context, result);

                WebApiControllerHelper.SetExecutionContext(context.HttpContext.Response);
                WebApiControllerHelper.SetETag(context.HttpContext.Response, etag);

                if (result != null && IfNoneMatchETags != null && !IsIfNoneMatchModified(etag))
                {
                    await CreateResult(context, HttpStatusCode.NotModified).ExecuteResultAsync(context).ConfigureAwait(false);
                    return;
                }

                await ((result == null ?
                    (AlternateStatusCode.HasValue ? CreateResult(context, AlternateStatusCode.Value).ExecuteResultAsync(context) : throw new InvalidOperationException("Function has not returned a result; no AlternateStatusCode has been configured to return.")) :
                    CreateResult(context, StatusCode, json).ExecuteResultAsync(context)).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                var ai = ExecuteExceptionHandler == null ? null : ExecuteExceptionHandler(context, ex);
                if (ai == null)
                    throw;

                await ai.ExecuteResultAsync(context).ConfigureAwait(false);
                return;
            }
        }

        /// <summary>
        /// Creates the <b>JSON</b> representation of the result whilst applying the <see cref="JsonPropertyFilter"/> whilst also returning the related <see cref="IETag.ETag"/>.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <param name="result">The result value.</param>
        /// <returns>The <b>JSON</b> representation and related <see cref="IETag.ETag"/>.</returns>
        /// <remarks>The resulting etag will be determined by the following: 
        /// <list type="number">
        /// <item><description>The <see cref="ExecutionContext"/> <see cref="ExecutionContext.ETag"/>.</description></item>
        /// <item><description>The <see cref="IETag"/> value where implemented for the <typeparamref name="TResult"/>.</description></item>
        /// <item><description>Where the result is an <see cref="System.Collections.IEnumerable"/> a hash of the requesting query string and each item's <see cref="IETag"/> will be hashed.</description></item>
        /// <item><description>Otherwise, will generate by hashing the resulting JSON and requesting query string.</description></item>
        /// </list></remarks>
        protected (JToken? json, string? etag) CreateJsonResultAndETag<TResult>(ActionContext context, TResult result)
        {
            Check.NotNull(context, nameof(context));

            if (result == null)
                return (null, null);

            if (ExecutionContext.HasCurrent && Controller.IncludeRefDataText())
                ExecutionContext.Current.IsRefDataTextSerializationEnabled = true;

            var json = JsonPropertyFilter.Apply(result, IncludeFields, ExcludeFields);

            if (ExecutionContext.HasCurrent && !string.IsNullOrEmpty(ExecutionContext.Current.ETag))
                return (json, ExecutionContext.Current.ETag);

            if (result is IETag ietag)
                return (json, ietag?.ETag);

            StringBuilder? sb = null;
            if (result is System.Collections.IEnumerable coll)
            {
                sb = new StringBuilder();
                var hasEtags = true;

                foreach (var item in coll)
                {
                    if (item is IETag cetag && cetag.ETag != null)
                    {
                        sb.Append(cetag.ETag);
                        sb.Append(AsciiArtRobot); // Ascii art robot
                        continue;
                    }

                    hasEtags = false;
                    break;
                }

                if (!hasEtags)
                    sb = null;
            }

#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms; not used for security, only used to calculate an etag.
            using var md5 = System.Security.Cryptography.MD5.Create();
#pragma warning restore CA5351 
            var buf = Encoding.UTF8.GetBytes($"{(sb != null && sb.Length > 0 ? sb.ToString() : json.ToString())}{AsciiArtRobot}{context?.HttpContext.Request.QueryString.Value}");
            var hash = md5.ComputeHash(buf, 0, buf.Length);
            return (json, Convert.ToBase64String(hash));
        }

        /// <summary>
        /// Indicates whether the requested data has been modified by as determined by comparing the <paramref name="etag"/> to the <see cref="IfNoneMatchETags"/>.
        /// </summary>
        /// <param name="etag">The <b>ETag</b> value to compare against the <see cref="IfNoneMatchETags"/>.</param>
        /// <returns><c>true</c> where modified; otherwise, <c>false</c>.</returns>
        protected bool IsIfNoneMatchModified(string? etag)
        {
            if (IfNoneMatchETags == null || string.IsNullOrEmpty(etag))
                return false;

            return !IfNoneMatchETags.Contains(etag);
         }

        /// <summary>
        /// Indicates whether the requested data has been modified by as determined by comparing the <paramref name="etag"/> to the <see cref="IfMatchETags"/>.
        /// </summary>
        /// <param name="etag">The <b>ETag</b> value to compare against the <see cref="IfNoneMatchETags"/>.</param>
        /// <returns><c>true</c> where modified; otherwise, <c>false</c>.</returns>
        protected bool IsIfMatchModified(string? etag)
        {
            if (IfMatchETags == null || string.IsNullOrEmpty(etag))
                return false;

            return !IfMatchETags.Contains(etag);
        }
    }

    #endregion

    #region WebApiGet

    /// <summary>
    /// Enables an invoke of a <see cref="HttpMethod.Get"/> with a <typeparamref name="TResult"/> result.
    /// </summary>
    /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
    public sealed class WebApiGet<TResult> : WebApiActionBase
    {
        private readonly Func<Task<TResult>> _func;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiGet{TResult}"/> class.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/>.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="operationType">The <see cref="Beef.OperationType"/>.</param>
        /// <param name="statusCode">The primary <see cref="HttpStatusCode"/> when there is a result.</param>
        /// <param name="alternateStatusCode">The alternate <see cref="HttpStatusCode"/> when there is no result (where supported; i.e. not <c>null</c>).</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        public WebApiGet(ControllerBase controller, Func<Task<TResult>> func, OperationType operationType = OperationType.Read,
            HttpStatusCode statusCode = HttpStatusCode.OK, HttpStatusCode? alternateStatusCode = HttpStatusCode.NotFound,
            [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            : base(controller, operationType, statusCode, alternateStatusCode, memberName, filePath, lineNumber)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        /// <summary>
        /// Executes the result operation of the action method asynchronously.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
        public override Task ExecuteResultAsync(ActionContext context)
        {
            return ExecuteResultAsync(context, _func);
        }
    }

    /// <summary>
    /// Enables an invoke of a <see cref="HttpMethod.Get"/> with a <see cref="EntityCollectionResult{TColl, TEntity}"/> result.
    /// </summary>
    /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
    /// <typeparam name="TColl">The result collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TEntity">The entity (item) <see cref="Type"/>.</typeparam>
    public sealed class WebApiGet<TResult, TColl, TEntity> : WebApiActionBase 
        where TResult : EntityCollectionResult<TColl, TEntity>
        where TColl : EntityBaseCollection<TEntity>, new()
        where TEntity : EntityBase
    {
        private readonly Func<Task<TResult>> _func;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiGet{TResult, TColl, TEntity}"/> class.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/>.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="operationType">The <see cref="Beef.OperationType"/>.</param>
        /// <param name="statusCode">The primary <see cref="HttpStatusCode"/> when there is a result.</param>
        /// <param name="alternateStatusCode">The alternate <see cref="HttpStatusCode"/> when there is no result (where supported; i.e. not <c>null</c>).</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        public WebApiGet(ControllerBase controller, Func<Task<TResult>> func, OperationType operationType = OperationType.Read,
            HttpStatusCode statusCode = HttpStatusCode.OK, HttpStatusCode? alternateStatusCode = HttpStatusCode.NoContent,
            [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            : base(controller, operationType, statusCode, alternateStatusCode, memberName, filePath, lineNumber)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        /// <summary>
        /// Executes the result operation of the action method asynchronously.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
        [DebuggerStepThrough()]
        public async override Task ExecuteResultAsync(ActionContext context)
        {
            await WebApiControllerInvoker.Default.InvokeAsync(Controller, async () =>
            {
                try
                {
                    ExecutionContext.Current.OperationType = OperationType;
                    TResult result = await _func().ConfigureAwait(false);

                    var (json, etag) = CreateJsonResultAndETag(context, result?.Result);

                    WebApiControllerHelper.SetExecutionContext(context.HttpContext.Response);
                    WebApiControllerHelper.SetPaging(context.HttpContext.Response, result);
                    WebApiControllerHelper.SetETag(context.HttpContext.Response, etag);

                    if (result?.Result != null && IfNoneMatchETags != null && !IsIfNoneMatchModified(etag))
                    {
                        await CreateResult(context, HttpStatusCode.NotModified).ExecuteResultAsync(context).ConfigureAwait(false);
                        return;
                    }

                    await (((result == null || result.Result == null) ?
                        (AlternateStatusCode.HasValue ? CreateResult(context, AlternateStatusCode.Value).ExecuteResultAsync(context) : throw new InvalidOperationException("Function has not returned a result; no AlternateStatusCode has been configured to return.")) :
                        CreateResult(context, StatusCode, json).ExecuteResultAsync(context)).ConfigureAwait(false));
                }
                catch (Exception ex)
                {
                    var ai = ExecuteExceptionHandler == null ? null : ExecuteExceptionHandler(context, ex);
                    if (ai == null)
                        throw;

                    await ai.ExecuteResultAsync(context).ConfigureAwait(false);
                    return;
                }

            }, memberName: CallerMemberName, filePath: CallerFilePath, lineNumber: CallerLineNumber).ConfigureAwait(false);
        }
    }

    #endregion

    #region WebApiPost

    /// <summary>
    /// Enables an invoke of a <see cref="HttpMethod.Post"/> with no result.
    /// </summary>
    public sealed class WebApiPost : WebApiActionBase
    {
        private readonly Func<Task> _func;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiPost"/> class.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/>.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="operationType">The <see cref="Beef.OperationType"/>.</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        public WebApiPost(ControllerBase controller, Func<Task> func, OperationType operationType = OperationType.Unspecified,
            HttpStatusCode statusCode = HttpStatusCode.NoContent,
            [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            : base(controller, operationType, statusCode, statusCode, memberName, filePath, lineNumber)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        /// <summary>
        /// Executes the result operation of the action method asynchronously.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
        [DebuggerStepThrough()]
        public override Task ExecuteResultAsync(ActionContext context)
        {
            return ExecuteResultAsync(context, _func);
        }
    }

    /// <summary>
    /// Enables an invoke of a <see cref="HttpMethod.Post"/> with a <typeparamref name="TResult"/> result.
    /// </summary>
    /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
    public class WebApiPost<TResult> : WebApiActionBase
    {
        private readonly Func<Task<TResult>> _func;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiPost{TResult}"/> class.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/>.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="operationType">The <see cref="Beef.OperationType"/>.</param>
        /// <param name="statusCode">The primary <see cref="HttpStatusCode"/> when there is a result.</param>
        /// <param name="alternateStatusCode">The alternate <see cref="HttpStatusCode"/> when there is no result (where supported; i.e. not <c>null</c>).</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        public WebApiPost(ControllerBase controller, Func<Task<TResult>> func, OperationType operationType = OperationType.Unspecified,
            HttpStatusCode statusCode = HttpStatusCode.OK, HttpStatusCode? alternateStatusCode = HttpStatusCode.NoContent,
            [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            : base(controller, operationType, statusCode, alternateStatusCode, memberName, filePath, lineNumber)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        /// <summary>
        /// Executes the result operation of the action method asynchronously.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
        [DebuggerStepThrough()]
        public override Task ExecuteResultAsync(ActionContext context)
        {
            return ExecuteResultAsync(context, _func);
        }
    }

    #endregion

    #region WebApiPut

    /// <summary>
    /// Enables an invoke of a <see cref="HttpMethod.Put"/> with no result.
    /// </summary>
    public class WebApiPut : WebApiActionBase
    {
        private readonly Func<Task> _func;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiPut"/> class.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/>.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="operationType">The <see cref="Beef.OperationType"/>.</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        public WebApiPut(ControllerBase controller, Func<Task> func, OperationType operationType = OperationType.Unspecified,
            HttpStatusCode statusCode = HttpStatusCode.OK,
            [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            : base(controller, operationType, statusCode, statusCode, memberName, filePath, lineNumber)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        /// <summary>
        /// Executes the result operation of the action method asynchronously.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
        [DebuggerStepThrough()]
        public override Task ExecuteResultAsync(ActionContext context)
        {
            return ExecuteResultAsync(context, _func);
        }
    }

    /// <summary>
    /// Enables an invoke of a <see cref="HttpMethod.Put"/> with a <typeparamref name="TResult"/> result.
    /// </summary>
    /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
    public class WebApiPut<TResult> : WebApiActionBase
    {
        private readonly Func<Task<TResult>> _func;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiPut{TResult}"/> class.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/>.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="operationType">The <see cref="Beef.OperationType"/>.</param>
        /// <param name="statusCode">The primary <see cref="HttpStatusCode"/> when there is a result.</param>
        /// <param name="alternateStatusCode">The alternate <see cref="HttpStatusCode"/> when there is no result (where supported; i.e. not <c>null</c>).</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        public WebApiPut(ControllerBase controller, Func<Task<TResult>> func, OperationType operationType = OperationType.Unspecified,
            HttpStatusCode statusCode = HttpStatusCode.OK, HttpStatusCode? alternateStatusCode = HttpStatusCode.NoContent,
            [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            : base(controller, operationType, statusCode, alternateStatusCode, memberName, filePath, lineNumber)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        /// <summary>
        /// Executes the result operation of the action method asynchronously.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
        [DebuggerStepThrough()]
        public override Task ExecuteResultAsync(ActionContext context)
        {
            return ExecuteResultAsync(context, _func);
        }
    }

    #endregion

    #region WebApiDelete

    /// <summary>
    /// Enables an invoke of a <see cref="HttpMethod.Delete"/> with no result.
    /// </summary>
    public class WebApiDelete : WebApiActionBase
    {
        private readonly Func<Task> _func;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiDelete"/> class.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/>.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="operationType">The <see cref="Beef.OperationType"/>.</param>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        public WebApiDelete(ControllerBase controller, Func<Task> func, OperationType operationType = OperationType.Delete,
            HttpStatusCode statusCode = HttpStatusCode.NoContent,
            [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            : base(controller, operationType, statusCode, statusCode, memberName, filePath, lineNumber)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        /// <summary>
        /// Executes the result operation of the action method asynchronously.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
        [DebuggerStepThrough()]
        public override Task ExecuteResultAsync(ActionContext context)
        {
            return ExecuteResultAsync(context, _func);
        }
    }

    #endregion

    #region WebApiPatch

    /// <summary>
    /// Enables an invoke of a <see cref="HttpMethod.Patch"/>.
    /// </summary>
    /// <typeparam name="T">The entity <see cref="Type"/> being patched.</typeparam>
    public class WebApiPatch<T> : WebApiActionBase where T : class
    {
        private readonly JToken _value;
        private readonly Func<Task<T>> _getFunc;
        private readonly Func<T, Task>? _updateFuncNoResult;
        private readonly Func<T, Task<T>>? _updateFuncWithResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiPatch{T}"/> class.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/>.</param>
        /// <param name="value">The <see cref="JToken"/> value that contains the content to patch.</param>
        /// <param name="getFunc">The function to invoke to perform the <b>get</b>.</param>
        /// <param name="updateFuncNoResult">The function to invoke to perform the <b>update</b> with no result.</param>
        /// <param name="operationType">The <see cref="Beef.OperationType"/>.</param>
        /// <param name="statusCode">The primary <see cref="HttpStatusCode"/> when there is a result.</param>
        /// <param name="alternateStatusCode">The alternate <see cref="HttpStatusCode"/> when there is no result (where supported; i.e. not <c>null</c>).</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        public WebApiPatch(ControllerBase controller, JToken value, Func<Task<T>> getFunc, Func<T, Task> updateFuncNoResult, OperationType operationType = OperationType.Unspecified,
            HttpStatusCode statusCode = HttpStatusCode.OK, HttpStatusCode? alternateStatusCode = HttpStatusCode.NoContent,
            [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            : base(controller, operationType, statusCode, alternateStatusCode, memberName, filePath, lineNumber)
        {
            _value = value;
            _getFunc = getFunc ?? throw new ArgumentNullException(nameof(getFunc));
            _updateFuncNoResult = updateFuncNoResult ?? throw new ArgumentNullException(nameof(updateFuncNoResult));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiPatch{T}"/> class.
        /// </summary>
        /// <param name="controller">The <see cref="ControllerBase"/>.</param>
        /// <param name="value">The <see cref="JToken"/> value that contains the content to patch.</param>
        /// <param name="getFunc">The function to invoke to perform the <b>get</b>.</param>
        /// <param name="updateFuncWithResult">The function to invoke to perform the <b>update</b> with a result.</param>
        /// <param name="operationType">The <see cref="Beef.OperationType"/>.</param>
        /// <param name="statusCode">The primary <see cref="HttpStatusCode"/> when there is a result.</param>
        /// <param name="alternateStatusCode">The alternate <see cref="HttpStatusCode"/> when there is no result (where supported; i.e. not <c>null</c>).</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        public WebApiPatch(ControllerBase controller, JToken value, Func<Task<T>> getFunc, Func<T, Task<T>> updateFuncWithResult, OperationType operationType = OperationType.Unspecified,
            HttpStatusCode statusCode = HttpStatusCode.OK, HttpStatusCode? alternateStatusCode = HttpStatusCode.NoContent,
            [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
            : base(controller, operationType, statusCode, alternateStatusCode, memberName, filePath, lineNumber)
        {
            BodyValue = _value = value;
            _getFunc = getFunc ?? throw new ArgumentNullException(nameof(getFunc));
            _updateFuncWithResult = updateFuncWithResult ?? throw new ArgumentNullException(nameof(updateFuncWithResult));
        }

        /// <summary>
        /// Throws a <see cref="NotSupportedException"/>; use <see cref="ExecuteResultAsync(ActionContext)"/>.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <param name="func">The The function to invoke.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
        [DebuggerStepThrough()]
        protected override Task ExecuteResultAsync(ActionContext context, Func<Task> func)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Throws a <see cref="NotSupportedException"/>; use <see cref="ExecuteResultAsync(ActionContext)"/>.
        /// </summary>
        /// <typeparam name="TResult">The result <see cref="Type"/>.</typeparam>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <param name="func">The function to invoke.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
        [DebuggerStepThrough()]
        protected override Task ExecuteResultAsync<TResult>(ActionContext context, Func<Task<TResult>> func)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Executes the result operation of the action method asynchronously.
        /// </summary>
        /// <param name="context">The <see cref="ActionContext"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
        [DebuggerStepThrough()]
        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (_updateFuncWithResult == null)
                return WebApiControllerInvoker.Default.InvokeAsync(Controller, () => ExecuteResultAsyncInternal(context, _updateFuncNoResult!),
                    memberName: CallerMemberName, filePath: CallerFilePath, lineNumber: CallerLineNumber);
            else
                return WebApiControllerInvoker.Default.InvokeAsync(Controller, () => ExecuteResultAsyncInternal(context, _updateFuncWithResult),
                    memberName: CallerMemberName, filePath: CallerFilePath, lineNumber: CallerLineNumber);
        }

        /// <summary>
        /// Does the actual execution of the <paramref name="func"/> asynchronously where there is no result.
        /// </summary>
        private async Task ExecuteResultAsyncInternal(ActionContext context, Func<T, Task> func)
        {
            ExecutionContext.Current.OperationType = OperationType;

            try
            {
                // Validate the patch option and json.
                var (option, patch) = await PatchValidationAsync(context).ConfigureAwait(false);
                if (option == WebApiPatchOption.NotSpecified)
                    return;

                // Get the existing value; make sure it exists and matches the supplied etag.
                var (success, value) = await GetCurrentValueAsync(context).ConfigureAwait(false);
                if (!success)
                    return;

                // Merge (patch) the json input against the entity.
                var performUpdate = true;
                var currETag = (value as IETag)?.ETag;

                if (option == WebApiPatchOption.JsonPatch)
                {
                    if (!await UpdateUsingJsonPatchAsync(context, value!, patch!).ConfigureAwait(false))
                        return;
                }
                else
                {
                    switch (await UpdateUsingMergePatchAsync(context, value!).ConfigureAwait(false))
                    {
                        case JsonEntityMergeResult.Error: return;
                        case JsonEntityMergeResult.SuccessNoChanges: performUpdate = false; break;
                    }
                }

                // Now perform the update.
                if (performUpdate)
                {
                    if (currETag != null && value is IETag ietag)
                        ietag.ETag = currETag;

                    await func(value!).ConfigureAwait(false);
                }

                WebApiControllerHelper.SetExecutionContext(context.HttpContext.Response);
                await CreateResult(context, StatusCode).ExecuteResultAsync(context).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var ai = ExecuteExceptionHandler == null ? null : ExecuteExceptionHandler(context, ex);
                if (ai == null)
                    throw;

                await ai.ExecuteResultAsync(context).ConfigureAwait(false);
                return;
            }
        }

        /// <summary>
        /// Does the actual execution of the <paramref name="func"/> asynchronously where there is no result.
        /// </summary>
        private async Task ExecuteResultAsyncInternal(ActionContext context, Func<T, Task<T>> func)
        {
            ExecutionContext.Current.OperationType = OperationType;

            try
            {
                // Validate the patch option and json.
                var (option, patch) = await PatchValidationAsync(context).ConfigureAwait(false);
                if (option == WebApiPatchOption.NotSpecified)
                    return;

                // Get the existing value; make sure it exists and matches the supplied etag.
                var (success, value) = await GetCurrentValueAsync(context).ConfigureAwait(false);
                if (!success)
                    return;

                // Patch the json input with the entity value.
                var performUpdate = true;
                var currETag = (value as IETag)?.ETag;

                if (option == WebApiPatchOption.JsonPatch)
                {
                    if (! await UpdateUsingJsonPatchAsync(context, value!, patch!).ConfigureAwait(false))
                        return;
                }
                else
                {
                    switch (await UpdateUsingMergePatchAsync(context, value!).ConfigureAwait(false))
                    {
                        case JsonEntityMergeResult.Error: return;
                        case JsonEntityMergeResult.SuccessNoChanges: performUpdate = false; break;
                    }
                }

                // Now perform the update and return the result (ignore any etag value patch).
                T result = value!;
                if (performUpdate)
                {
                    if (currETag != null && value is IETag ietag)
                        ietag.ETag = currETag;

                    result = await func(value!).ConfigureAwait(false);
                }

                var (json, etag) = CreateJsonResultAndETag(context, result);

                // Complete the success response.
                WebApiControllerHelper.SetExecutionContext(context.HttpContext.Response);
                WebApiControllerHelper.SetETag(context.HttpContext.Response, etag);

                await ((result == null ?
                    (AlternateStatusCode.HasValue ? CreateResult(context, AlternateStatusCode.Value).ExecuteResultAsync(context) : throw new InvalidOperationException("Function has not returned a result; no AlternateStatusCode has been configured to return.")) :
                    CreateResult(context, StatusCode, json).ExecuteResultAsync(context)).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                var ai = ExecuteExceptionHandler == null ? null : ExecuteExceptionHandler(context, ex);
                if (ai == null)
                    throw;

                await ai.ExecuteResultAsync(context).ConfigureAwait(false);
                return;
            }
        }

        /// <summary>
        /// Determines the <see cref="WebApiPatchOption"/> and performs initial json validation.
        /// </summary>
        private async Task<(WebApiPatchOption option, JsonPatchDocument<T>? patch)> PatchValidationAsync(ActionContext context)
        {
            if (context.HttpContext.Request.ContentType.Equals("application/json-patch+json", StringComparison.InvariantCultureIgnoreCase))
            {
                JsonPatchDocument<T> patch;
                try
                {
                    patch = _value.ToObject<JsonPatchDocument<T>>()!;
                }
                catch (JsonSerializationException jsex)
                {
                    await CreateResultFromException(context, new ValidationException(jsex.Message))!.ExecuteResultAsync(context).ConfigureAwait(false);
                    return (WebApiPatchOption.NotSpecified, null);
                }

                if (patch.Operations.Count == 0)
                {
                    await CreateResultFromException(context, new ValidationException("The JSON patch document requires one or more operations to be considered valid."))!.ExecuteResultAsync(context).ConfigureAwait(false);
                    return (WebApiPatchOption.NotSpecified, null);
                }

                return (WebApiPatchOption.JsonPatch, patch);
            }
            else if (context.HttpContext.Request.ContentType.Equals("application/merge-patch+json", StringComparison.InvariantCultureIgnoreCase)
                || context.HttpContext.Request.ContentType.Equals("application/json", StringComparison.InvariantCultureIgnoreCase))
            {
                return (WebApiPatchOption.MergePatch, null);
            }
            else
            {
                await new ObjectResult("Unsupported Content-Type for a PATCH; support JSON-Patch: 'application/json-patch+json' or, JSON-Merge: `application/merge-patch+json` or `application/json`.") { StatusCode = (int)HttpStatusCode.UnsupportedMediaType }.ExecuteResultAsync(context).ConfigureAwait(false);
                return (WebApiPatchOption.NotSpecified, null);
            }
        }

        /// <summary>
        /// Get current value before attempting to patch.
        /// </summary>
        private async Task<(bool success, T? value)> GetCurrentValueAsync(ActionContext context)
        {
            // Get the existing value.
            var value = await _getFunc().ConfigureAwait(false);
            if (value == null)
            {
                await CreateResultFromException(context, new NotFoundException())!.ExecuteResultAsync(context).ConfigureAwait(false);
                return (false, value);
            }

            // Check the concurrency etag match.
            if (value is IETag et)
            {
                if (IfMatchETags == null || IfMatchETags.Count == 0)
                {
                    await CreateResultFromException(context, new ConcurrencyException("An 'If-Match' header is required for a PATCH where the underlying entity supports concurrency (ETag)."))!.ExecuteResultAsync(context).ConfigureAwait(false);
                    return (false, default(T));
                }

                if (IsIfMatchModified(et.ETag))
                {
                    await CreateResultFromException(context, new ConcurrencyException())!.ExecuteResultAsync(context).ConfigureAwait(false);
                    return (false, value);
                }
            }

            return (true, value);
        }

        /// <summary>
        /// Update the value using a <see cref="WebApiPatchOption.JsonPatch"/>.
        /// </summary>
        private static async Task<bool> UpdateUsingJsonPatchAsync(ActionContext context, T value, JsonPatchDocument<T> patch)
        {
            var msgs = new MessageItemCollection();
            patch.ApplyTo(value, (e) => msgs.AddError(nameof(value), e.ErrorMessage));
            if (msgs.Count == 0)
                return true;

            await CreateResultFromException(context, new ValidationException(msgs))!.ExecuteResultAsync(context).ConfigureAwait(false);
            return false;
        }

        /// <summary>
        /// Update the value using a <see cref="WebApiPatchOption.MergePatch"/>.
        /// </summary>
        private async Task<JsonEntityMergeResult> UpdateUsingMergePatchAsync(ActionContext context, T value)
        {
            var msgs = new MessageItemCollection();
            var mr = JsonEntityMerge.Merge(_value, value, new JsonEntityMergeArgs()
            {
                LogAction = (m) => { m.Property = string.IsNullOrEmpty(m.Property) ? m.Property : $"{nameof(value)}.{m.Property}"; msgs.Add(m); }
            });

            if (mr == JsonEntityMergeResult.Error)
                await CreateResultFromException(context, new ValidationException(msgs))!.ExecuteResultAsync(context).ConfigureAwait(false);

            return mr;
        }
    }

    #endregion
}