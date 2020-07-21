// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Caching;
using Beef.Caching.Policy;
using Beef.Events;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Beef
{
    /// <summary>
    /// Represents the standard <i>Beef</i> <see cref="IServiceCollection"/> extensions.
    /// </summary>
    public static class ServiceExtensions
    {
        /// <summary>
        /// Adds a scoped service to instantiate a new <see cref="ExecutionContext"/> instance.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="createExecutionContext">The function to override the creation of the <see cref="ExecutionContext"/> instance to a custom <see cref="Type"/>; defaults to <see cref="ExecutionContext"/> where not specified.</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        public static IServiceCollection AddBeefExecutionContext(this IServiceCollection services, Func<ExecutionContext>? createExecutionContext = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddScoped(_ => createExecutionContext?.Invoke() ?? new ExecutionContext());
        }

        /// <summary>
        /// Adds a scoped service to instantiate a new <see cref="IRequestCache"/> <see cref="RequestCache"/> instance.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        public static IServiceCollection AddBeefRequestCache(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddScoped<IRequestCache, RequestCache>();
        }

        /// <summary>
        /// Adds a singleton service to instantiate a new <see cref="CachePolicyManager"/> instance.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="config">The optional <see cref="CachePolicyConfig"/>.</param>
        /// <param name="flushDueTime">The optional amount of time to delay before <see cref="CachePolicyManager.Flush"/> is invoked for the first time (defaults to <see cref="CachePolicyManager.TenMinutes"/>).</param>
        /// <param name="flushPeriod">The optional time interval between subsequent invocations of <see cref="CachePolicyManager.Flush"/> (defaults to <see cref="CachePolicyManager.FiveMinutes"/>).</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        public static IServiceCollection AddBeefCachePolicyManager(this IServiceCollection services, CachePolicyConfig? config = null, TimeSpan? flushDueTime = null, TimeSpan? flushPeriod = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddSingleton(_ =>
            {
                var cpm = new CachePolicyManager();
                if (config != null)
                    cpm.SetFromCachePolicyConfig(config);

                cpm.StartFlushTimer(flushDueTime ?? CachePolicyManager.TenMinutes, flushPeriod ?? CachePolicyManager.FiveMinutes);
                return cpm;
            });
        }

        /// <summary>
        /// Adds a singleton service to instantiate a new <see cref="IEventPublisher"/> <see cref="NullEventPublisher"/> instance.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        public static IServiceCollection AddBeefNullEventPublisher(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddSingleton<IEventPublisher>(_ => new NullEventPublisher());
        }
    }
}