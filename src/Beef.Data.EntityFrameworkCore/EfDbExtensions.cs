// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Beef.Data.EntityFrameworkCore
{
    /// <summary>
    /// Enables the <b>Beef</b> database extension(s).
    /// </summary>
    public static class EfDbExtensions
    {
        /// <summary>
        /// Adds the required <b>entity framework</b> <i>scoped</i> services.
        /// </summary>
        /// <typeparam name="TDbContext">The entity framework <see cref="DbContext"/> <see cref="Type"/>.</typeparam>
        /// <typeparam name="TEfDb">The corresponding entity framework <see cref="IEfDb"/> <see cref="Type"/>.</typeparam>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddBeefEntityFrameworkServices<TDbContext, TEfDb>(this IServiceCollection serviceCollection) where TDbContext : DbContext where TEfDb : class, IEfDb<TDbContext>
            => serviceCollection.AddDbContext<TDbContext>()
                                .AddScoped<IEfDb, TEfDb>();
    }
}