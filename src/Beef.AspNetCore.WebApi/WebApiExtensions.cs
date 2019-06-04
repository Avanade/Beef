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
        /// Registers the action to <paramref name="update"/> the <see cref="ExecutionContext"/> for a request. 
        /// </summary>
        /// <param name="builder">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="update">The function to update the <see cref="ExecutionContext"/> for a request; <c>null</c> will <see cref="ExecutionContext.Reset(bool)"/> the <b>ExecutionContext</b>.</param>
        /// <returns>The <see cref="IApplicationBuilder"/>.</returns>
        public static IApplicationBuilder UseExecutionContext(this IApplicationBuilder builder, Action<HttpContext, ExecutionContext> update = null)
        {
            return builder.Use((context, next) =>
            {
                ExecutionContext.Reset(true);
                update?.Invoke(context, ExecutionContext.Current);
                return next();
            });
        }
    }
}