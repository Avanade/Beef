// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Logging;
using System;

namespace Beef.Diagnostics
{
    /// <summary>
    /// Provides a shortcut for creating an <see cref="ILogger"/> instance.
    /// </summary>
    /// <remarks>
    /// A logger can only be created where <see cref="ExecutionContext.HasCurrent"/> and the <see cref="ExecutionContext.ServiceProvider"/> has been set. <para>This is intended for internal use; therefore, users of <i>Beef</i> should look to
    /// leverage constructor-based dependency injection to access an <see cref="ILogger"/> where possible.</para></remarks>
    public static class Logger
    {
        /// <summary>
        /// Gets or sets the default <see cref="Logger"/>. The <see cref="Create{T}"/> will only be used as a backup where the <see cref="ExecutionContext.ServiceProvider"/> is not configured.
        /// </summary>
        public static ILogger? Default { get; set; }

        /// <summary>
        /// Creates an <see cref="ILogger"/> leveraging the <see cref="ExecutionContext.GetService{T}(bool)"/>.
        /// </summary>
        public static ILogger Create<T>()
            => ExecutionContext.GetService<ILogger<T>>(false) ?? Default ?? throw new InvalidOperationException("A logger was unable to be instantiated as there is no ExecutionContext.Current instance, or the ExecutionContext.Current instance has not been configured with a ServiceProvider to enable, or finally the Default logger has not been specified.");
    }
}