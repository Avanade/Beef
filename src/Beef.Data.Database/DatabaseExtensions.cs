// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.DependencyInjection;
using System;

namespace Beef.Data.Database
{
    /// <summary>
    /// Enables the <b>Beef</b> database extension(s).
    /// </summary>
    public static class DatabaseExtensions
    {
        /// <summary>
        /// Adds the required <b>database</b> <i>scoped</i> services.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
        /// <param name="createInstance">The function to create the <see cref="IDatabase"/> instance.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddBeefDatabaseServices(this IServiceCollection serviceCollection, Func<IDatabase> createInstance)
            => serviceCollection.AddScoped(_ => createInstance());

        /// <summary>
        /// Adds the specified <see cref="DatabaseEventOutboxPublisherService"/> as a hosted service.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
        /// <param name="outboxPublisher">The <see cref="DatabaseEventOutboxPublisherService"/> function.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddBeefDatabaseEventOutboxPublisherService(this IServiceCollection serviceCollection, Func<IServiceProvider, DatabaseEventOutboxPublisherService> outboxPublisher)
            => serviceCollection.AddHostedService(sp => Check.NotNull(outboxPublisher, nameof(outboxPublisher)).Invoke(sp));

        /// <summary>
        /// Adds a <see cref="DatabaseEventOutboxPublisherService"/> using default configuration as a hosted service.
        /// </summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
        /// <param name="additional">An opportunity to perform additional configuration on the <see cref="DatabaseEventOutboxPublisherService"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddBeefDatabaseEventOutboxPublisherService(this IServiceCollection serviceCollection, Action<IServiceProvider, DatabaseEventOutboxPublisherService>? additional = null)
            => AddBeefDatabaseEventOutboxPublisherService(serviceCollection, sp =>
            {
                var deops = new DatabaseEventOutboxPublisherService(sp);
                additional?.Invoke(sp, deops);
                return deops;
            });
    }
}