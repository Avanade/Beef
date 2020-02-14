// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;

namespace Beef.AspNetCore.WebApi
{
    /// <summary>
    /// Enables the <b>Beef</b> Web API extension(s).
    /// </summary>
    public static class WebApiExtensions
    {
        /// <summary>
        /// Registers the action to <paramref name="updateAction"/> the <see cref="ExecutionContext"/> for a request. 
        /// </summary>
        /// <param name="builder">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="updateAction">The optional <see cref="Action{HttpContext, ExecutionContext}"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseExecutionContext(this IApplicationBuilder builder, Action<HttpContext, ExecutionContext>? updateAction = null)
        {
            return builder.UseMiddleware<WebApiExecutionContextMiddleware>(updateAction ?? WebApiExecutionContextMiddleware.DefaultUpdateAction);
        }

        /// <summary>
        /// Adds <see cref="WebApiExceptionHandlerMiddleware"/> to the <see cref="IApplicationBuilder"/> request execution pipeline.
        /// </summary>
        /// <param name="builder">The <see cref="IApplicationBuilder"/>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseWebApiExceptionHandler(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<WebApiExceptionHandlerMiddleware>();
        }
    }
}