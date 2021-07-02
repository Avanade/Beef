// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Business;
using Beef.Caching;
using Beef.Caching.Policy;
using Beef.Events;
using Beef.Hosting;
using Beef.WebApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

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
        /// <remarks>Use the <paramref name="createExecutionContext"/> function to instantiate a custom <see cref="ExecutionContext"/> (inherited) <see cref="Type"/> where required; otherwise, by default the <i>Beef</i>
        /// <see cref="ExecutionContext"/> will be used.</remarks>
        public static IServiceCollection AddBeefExecutionContext(this IServiceCollection services, Func<IServiceProvider, ExecutionContext>? createExecutionContext = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddScoped(sp => createExecutionContext?.Invoke(sp) ?? new ExecutionContext());
        }

        /// <summary>
        /// Adds a scoped service to instantiate a new <see cref="IRequestCache"/> <see cref="RequestCache"/> instance.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="createRequestCache">The function to override the creation of the <see cref="IRequestCache"/> instance; defaults to <see cref="RequestCache"/> where not specified.</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        /// <remarks>The <see cref="IRequestCache"/> enables the short-lived request caching; intended to reduce data chattiness within the context of a request scope.</remarks>
        public static IServiceCollection AddBeefRequestCache(this IServiceCollection services, Func<IServiceProvider, IRequestCache>? createRequestCache = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddScoped(sp => createRequestCache?.Invoke(sp) ?? new RequestCache());
        }

        /// <summary>
        /// Adds a scoped service to instantiate a new <see cref="ISystemTime"/> <see cref="SystemTime"/> instance.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="createSystemTime">The function to override the creation of the <see cref="ISystemTime"/> instance; defaults to <see cref="SystemTime"/> where not specified.</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        public static IServiceCollection AddBeefSystemTime(this IServiceCollection services, Func<IServiceProvider, ISystemTime>? createSystemTime = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddScoped(sp => createSystemTime?.Invoke(sp) ?? new SystemTime());
        }

        /// <summary>
        /// Adds a singleton service to instantiate a new <see cref="CachePolicyManager"/> instance with the specified <paramref name="config"/> and starts the corresponding <see cref="CachePolicyManagerServiceHost"/> with the <paramref name="firstInterval"/> and <paramref name="interval"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="config">The optional <see cref="CachePolicyConfig"/>.</param>
        /// <param name="firstInterval">The optional <see cref="CachePolicyManagerServiceHost"/> <see cref="TimerHostedServiceBase.FirstInterval"/> before <see cref="CachePolicyManager.Flush"/> is invoked for the first time (defaults to <see cref="CachePolicyManager.TenMinutes"/>).</param>
        /// <param name="interval">The optional <see cref="CachePolicyManagerServiceHost"/> <see cref="TimerHostedServiceBase.FirstInterval"/> between subsequent invocations of <see cref="CachePolicyManager.Flush"/> (defaults to <see cref="CachePolicyManager.FiveMinutes"/>).</param>
        /// <param name="useCachePolicyManagerTimer">Indicates whether the <see cref="CachePolicyManager.StartFlushTimer(TimeSpan, TimeSpan, ILogger{CachePolicyManager}?)"/> should be used; versus, being managed via <c>"IServiceCollection.AddHostedService"</c> <see cref="CachePolicyManagerServiceHost"/> (default).</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        /// <remarks>The <see cref="CachePolicyManager"/> enables the centralised management of <see cref="ICachePolicy"/> caches.</remarks>
        public static IServiceCollection AddBeefCachePolicyManager(this IServiceCollection services, CachePolicyConfig? config = null, TimeSpan? firstInterval = null, TimeSpan? interval = null, bool useCachePolicyManagerTimer = false)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var cpm = new CachePolicyManager();
            if (config != null)
                cpm.SetFromCachePolicyConfig(config);

            firstInterval ??= CachePolicyManager.TenMinutes;
            interval ??= CachePolicyManager.FiveMinutes;

            services.AddSingleton(sp =>
            {
                if (useCachePolicyManagerTimer)
                    cpm.StartFlushTimer(firstInterval.Value, interval.Value, sp.GetService<ILogger<CachePolicyManager>>());

                return cpm;
            });

            if (!useCachePolicyManagerTimer)
                services.AddHostedService(sp => new CachePolicyManagerServiceHost(cpm, sp, sp.GetService<ILogger<CachePolicyManager>>()) { FirstInterval = firstInterval, Interval = interval.Value });

            return services;
        }

        /// <summary>
        /// Adds a scoped service to instantiate a new <see cref="IEventPublisher"/> <see cref="NullEventPublisher"/> instance.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        public static IServiceCollection AddBeefNullEventPublisher(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddScoped<IEventPublisher, NullEventPublisher>();
        }

        /// <summary>
        /// Adds a scoped service to instantiate a new <see cref="IEventPublisher"/> <see cref="LoggerEventPublisher"/> instance.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        public static IServiceCollection AddBeefLoggerEventPublisher(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddScoped<IEventPublisher, LoggerEventPublisher>();
        }

        /// <summary>
        /// Adds the required <i>Business</i> singleton services (being the <see cref="ManagerInvoker"/>, <see cref="DataSvcInvoker"/> and <see cref="DataInvoker"/>).
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        public static IServiceCollection AddBeefBusinessServices(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddSingleton(_ => new ManagerInvoker())
                           .AddSingleton(_ => new DataSvcInvoker())
                           .AddSingleton(_ => new DataInvoker());
        }

        /// <summary>
        /// Adds the required <i>Agent</i> (client-side) services (being the <see cref="WebApiAgentInvoker"/>).
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        public static IServiceCollection AddBeefAgentServices(this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddSingleton(_ => new WebApiAgentInvoker());
        }

        /// <summary>
        /// Adds a singleton service to instantiate a new <see cref="TextProviderBase"/> instance.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="createTextProvider">The function to create the <see cref="TextProviderBase"/> instance; defaults to <see cref="DefaultTextProvider"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        public static IServiceCollection AddBeefTextProviderAsSingleton(this IServiceCollection services, Func<IServiceProvider, ITextProvider>? createTextProvider = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddSingleton(sp => createTextProvider?.Invoke(sp) ?? new DefaultTextProvider());
        }

        /// <summary>
        /// Adds a scoped service to instantiate a new <see cref="TextProviderBase"/> instance.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="createTextProvider">The function to create the <see cref="TextProviderBase"/> instance; defaults to <see cref="DefaultTextProvider"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        public static IServiceCollection AddBeefTextProviderAsScoped(this IServiceCollection services, Func<IServiceProvider, ITextProvider>? createTextProvider = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddScoped(sp => createTextProvider?.Invoke(sp) ?? new DefaultTextProvider());
        }
    }
}