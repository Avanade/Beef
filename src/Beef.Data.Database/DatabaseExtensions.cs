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
    }
}