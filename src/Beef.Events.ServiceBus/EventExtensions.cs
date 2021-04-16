// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace Beef.Events.ServiceBus
{
    /// <summary>
    /// Provides the extensions methods for the events capabilities.
    /// </summary>
    public static class EventExtensions
    {
        /// <summary>
        /// Adds a transient service to instantiate a new <see cref="ServiceBusReceiverHost"/> instance using the specified <paramref name="args"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="args">The <see cref="EventSubscriberHostArgs"/>.</param>
        /// <param name="addSubscriberTypeServices">Indicates whether to add all the <see cref="EventSubscriberHostArgs.GetSubscriberTypes"/> as scoped services (defaults to <c>true</c>).</param>
        /// <param name="additional">Optional (additional) opportunity to further configure the instantiated <see cref="ServiceBusReceiverHost"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        public static IServiceCollection AddBeefServiceBusReceiverHost(this IServiceCollection services, EventSubscriberHostArgs args, bool addSubscriberTypeServices = true, Action<IServiceProvider, ServiceBusReceiverHost>? additional = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (args == null)
                throw new ArgumentNullException(nameof(args));

            services.AddTransient(sp =>
            {
                var ehsh = new ServiceBusReceiverHost(args);
                args.UseServiceProvider(sp);
                additional?.Invoke(sp, ehsh);
                return ehsh;
            });

            if (addSubscriberTypeServices)
            {
                foreach (var type in args.GetSubscriberTypes())
                {
                    services.TryAddScoped(type);
                }
            }

            return services;
        }

        /// <summary>
        /// Adds a scoped service to instantiate a new <see cref="IEventPublisher"/> <see cref="ServiceBusSender"/> instance where the quere will be inferred from the <see cref="EventMetadata.Subject"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="clientOptions">The optional <see cref="ServiceBusClientOptions"/>.</param>
        /// <param name="additional">Optyional (additional) opportunity to further configure the instantiated <see cref="ServiceBusSender"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        public static IServiceCollection AddBeefEventServiceBusSender(this IServiceCollection services, string connectionString, ServiceBusClientOptions? clientOptions = null, Action<ServiceBusSender>? additional = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddScoped<IEventPublisher>(_ =>
            {
                var sbc = new ServiceBusClient(Check.NotEmpty(connectionString, nameof(connectionString)), clientOptions);
                var sbs = new ServiceBusSender(sbc);
                additional?.Invoke(sbs);
                return sbs;
            });
        }

        /// <summary>
        /// Adds a scoped service to instantiate a new <see cref="IEventPublisher"/> <see cref="ServiceBusSender"/> instance using the specified <paramref name="queueName"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="queueName">The queue name.</param>
        /// <param name="clientOptions">The optional <see cref="ServiceBusClientOptions"/>.</param>
        /// <param name="additional">Optyional (additional) opportunity to further configure the instantiated <see cref="ServiceBusSender"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        public static IServiceCollection AddBeefEventServiceBusSender(this IServiceCollection services, string connectionString, string queueName, ServiceBusClientOptions? clientOptions = null, Action<ServiceBusSender>? additional = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddScoped<IEventPublisher>(_ =>
            {
                var sbc = new ServiceBusClient(Check.NotEmpty(connectionString, nameof(connectionString)), clientOptions);
                var sbs = new ServiceBusSender(sbc, queueName);
                additional?.Invoke(sbs);
                return sbs;
            });
        }
    }
}