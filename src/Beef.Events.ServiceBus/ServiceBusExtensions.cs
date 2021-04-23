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
    public static class ServiceBusExtensions
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
        /// Adds a scoped service to instantiate a new <see cref="IEventPublisher"/> using a <see cref="ServiceBusSender"/> where the quere will be inferred from the corresponding <see cref="EventMetadata.Subject"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="client">The <see cref="ServiceBusClient"/>.</param>
        /// <param name="removeKeyFromSubject">Indicates whether to remove the key queue name from the <see cref="EventMetadata.Subject"/>. This is achieved by removing the last part (typically the key) to provide the base path;
        /// for example a Subject of <c>Beef.Demo.Person.1234</c> would result in <c>Beef.Demo.Person</c>.</param>
        /// <param name="additional">Optyional (additional) opportunity to further configure the instantiated <see cref="ServiceBusSender"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        public static IServiceCollection AddBeefServiceBusSender(this IServiceCollection services, ServiceBusClient client, bool removeKeyFromSubject = false, Action<ServiceBusSender>? additional = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddScoped<IEventPublisher>(_ =>
            {
                var sbs = new ServiceBusSender(Check.NotNull(client, nameof(client)), removeKeyFromSubject);
                additional?.Invoke(sbs);
                return sbs;
            });
        }

        /// <summary>
        /// Adds a scoped service to instantiate a new <see cref="IEventPublisher"/> using a <see cref="ServiceBusSender"/> instance with the specified <paramref name="queueName"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="client">The <see cref="ServiceBusClient"/>.</param>
        /// <param name="queueName">The queue name.</param>
        /// <param name="additional">Optyional (additional) opportunity to further configure the instantiated <see cref="ServiceBusSender"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> for fluent-style method-chaining.</returns>
        public static IServiceCollection AddBeefServiceBusSender(this IServiceCollection services, ServiceBusClient client, string queueName, Action<ServiceBusSender>? additional = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            return services.AddScoped<IEventPublisher>(_ =>
            {
                var sbs = new ServiceBusSender(Check.NotNull(client, nameof(client)), Check.NotEmpty(queueName, nameof(queueName)));
                additional?.Invoke(sbs);
                return sbs;
            });
        }
    }
}