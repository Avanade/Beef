// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Beef.AspNetCore.WebApi
{
    /// <summary>
    /// Provides a <b>Beef</b> oriented <see cref="Exception"/> handling middleware that is <see cref="IBusinessException"/> aware also enabling the exception response to be configured.
    /// </summary>
    /// <remarks>Each <see cref="Exception"/> will be logged using the <see cref="Beef.Diagnostics.Logger"/>.</remarks>
    public class WebApiExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;

        /// <summary>
        /// Gets or sets the <see cref="HttpStatusCode"/> for an unhandled <see cref="Exception"/>.
        /// </summary>
        public static HttpStatusCode UnhandledExceptionStatusCode { get; set; } = HttpStatusCode.InternalServerError;

        /// <summary>
        /// Gets or sets the message for an unhandled <see cref="Exception"/>.
        /// </summary>
        public static string UnhandledExceptionMessage { get; set; } = "An unexpected internal server error has occurred.";

        /// <summary>
        /// Indicates whether to include the unhandled <see cref="Exception"/> details in the response.
        /// </summary>
        public static bool IncludeUnhandledExceptionInResponse { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebApiExceptionHandlerMiddleware"/>.
        /// </summary>
        /// <param name="next">The next <see cref="RequestDelegate"/>.</param>
        public WebApiExceptionHandlerMiddleware(RequestDelegate next)
        {
            _next = Check.NotNull(next, nameof(next));
        }

        /// <summary>
        /// Invokes the <see cref="WebApiExceptionHandlerMiddleware"/>.
        /// </summary>
        /// <param name="context">The <see cref="HttpContext"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types; by-design, is intended to catch *any* unhandled exception.
            catch (Exception ex)
            {
                var ac = new ActionContext(context, new RouteData(), new ActionDescriptor());
                var ar = WebApiActionBase.CreateResultFromException(ac, ex);
                if (ar == null)
                {
                    ar = new ObjectResult(IncludeUnhandledExceptionInResponse ? ex.ToString() : UnhandledExceptionMessage) { StatusCode = (int)UnhandledExceptionStatusCode };
                    Beef.Diagnostics.Logger.Default.Exception(ex, UnhandledExceptionMessage);
                }

                await ar.ExecuteResultAsync(ac).ConfigureAwait(false);
            }
#pragma warning restore CA1031
        }
    }
}