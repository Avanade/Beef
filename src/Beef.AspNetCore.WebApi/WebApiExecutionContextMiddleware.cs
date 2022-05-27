// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.WebApi;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.AspNetCore.WebApi
{
    /// <summary>
    /// Provides an <see cref="ExecutionContext"/> handling middleware that creates (using dependency injection) and enables additional configuration where required.
    /// </summary>
    /// <remarks>A new <see cref="ExecutionContext"/> <see cref="ExecutionContext.Current"/> is instantiated through dependency injection using the <see cref="HttpContext.RequestServices"/>.</remarks>
    public class WebApiExecutionContextMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Func<HttpContext, ExecutionContext, Task> _updateFunc;

        /// <summary>
        /// Gets the default username where it is unable to be inferred (<see cref="System.Security.Principal.IIdentity.Name"/> from the <see cref="HttpContext"/> <see cref="HttpContext.User"/>).
        /// The default is 'Anonymous'.
        /// </summary>
        public static string DefaultUsername { get; set; } = "Anonymous";

        /// <summary>
        /// Represents the default <see cref="ExecutionContext"/> update function. The <see cref="ExecutionContext.Username"/> will be set to the <see cref="System.Security.Principal.IIdentity.Name"/>
        /// from the <see cref="HttpContext"/> <see cref="HttpContext.User"/>; otherwise, <see cref="DefaultUsername"/> where <c>null</c>. The <see cref="ExecutionContext.Timestamp"/>
        /// will be set to <see cref="ISystemTime.UtcNow"/>.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <param name="ec">The <see cref="ExecutionContext"/>.</param>
        public static Task DefaultExecutionContextUpdate(HttpContext context, ExecutionContext ec)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            
            if (ec == null)
                throw new ArgumentNullException(nameof(ec));

            ec.Username = context.User?.Identity?.Name ?? DefaultUsername;
            ec.Timestamp = SystemTime.Get(context.RequestServices).UtcNow;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiExecutionContextMiddleware"/>.
        /// </summary>
        /// <param name="next">The next <see cref="RequestDelegate"/>.</param>
        /// <param name="executionContextUpdate">The optional function to update the <see cref="ExecutionContext"/>. Defaults to <see cref="DefaultExecutionContextUpdate(HttpContext, ExecutionContext)"/> where not specified.</param>
        public WebApiExecutionContextMiddleware(RequestDelegate next, Func<HttpContext, ExecutionContext, Task>? executionContextUpdate = null)
        {
            _next = Check.NotNull(next, nameof(next));
            _updateFunc = executionContextUpdate ?? DefaultExecutionContextUpdate;
        }

        /// <summary>
        /// Invokes the <see cref="WebApiExceptionHandlerMiddleware"/>.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var ec = context.RequestServices.GetRequiredService<ExecutionContext>();
            ec.ServiceProvider = context.RequestServices;
            if (context.Request.Headers.TryGetValue(WebApiConsts.CorrelationIdHeaderName, out var val))
                ec.CorrelationId = val.FirstOrDefault();
            
            await _updateFunc(context, ec).ConfigureAwait(false);

            ExecutionContext.Reset();
            ExecutionContext.SetCurrent(ec);

            await _next(context).ConfigureAwait(false);
        }
    }
}