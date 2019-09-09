// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace Beef.AspNetCore.WebApi
{
    /// <summary>
    /// Provides helper methods for the <b>ASP.NET Web API</b> startup.
    /// </summary>
    public static class WebApiStartup
    {
        /// <summary>
        /// Binds (redirects) Beef <see cref="Beef.Diagnostics.Logger"/> to the ASP.NET Core <see cref="Microsoft.Extensions.Logging.ILogger"/>.
        /// </summary>
        /// <param name="logger">The ASP.NET Core <see cref="Microsoft.Extensions.Logging.ILogger"/>.</param>
        /// <param name="args">The Beef <see cref="LoggerArgs"/>.</param>
        /// <remarks>Redirects (binds) the Beef logger to the ASP.NET logger.</remarks>
        public static void BindLogger(ILogger logger, LoggerArgs args)
        {
            Check.NotNull(logger, nameof(logger));
            Check.NotNull(args, nameof(args));

            switch (args.Type)
            {
                case LogMessageType.Critical:
                    logger.LogCritical(args.ToString());
                    break;

                case LogMessageType.Info:
                    logger.LogInformation(args.ToString());
                    break;

                case LogMessageType.Warning:
                    logger.LogWarning(args.ToString());
                    break;

                case LogMessageType.Error:
                    logger.LogError(args.ToString());
                    break;

                case LogMessageType.Debug:
                    logger.LogDebug(args.ToString());
                    break;

                case LogMessageType.Trace:
                    logger.LogTrace(args.ToString());
                    break;
            }
        }

        /// <summary>
        /// Gets the named connection string from the <paramref name="configuration"/>.
        /// </summary>
        /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
        /// <param name="name">The connection string name.</param>
        /// <returns>The connection string value.</returns>
        public static string GetConnectionString(IConfiguration configuration, string name)
        {
            Check.NotNull(configuration, nameof(configuration));
            Check.NotEmpty(name, nameof(name));

            return configuration.GetConnectionString(name) ?? throw new ArgumentException($"Connection string setting for '{name}' is not defined.", nameof(name));
        }
    }
}