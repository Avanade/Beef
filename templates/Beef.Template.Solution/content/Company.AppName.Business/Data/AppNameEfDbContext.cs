﻿using Company.AppName.Business.Data.EfModel;

namespace Company.AppName.Business.Data;

/// <summary>
/// Represents the Entity Framework <see cref="DbContext"/>.
/// </summary>
/// <param name="options">The <see cref="DbContextOptions{AppNameEfDbContext}"/>.</param>
/// <param name="db">The base <see cref="IDatabase"/>.</param>
public class AppNameEfDbContext(DbContextOptions<AppNameEfDbContext> options, IDatabase db) : DbContext(options), IEfDbContext
{
    /// <summary>
    /// Gets the base <see cref="IDatabase"/>.
    /// </summary>
    public IDatabase BaseDatabase { get; } = db ?? throw new ArgumentNullException(nameof(db));

    /// <summary>
    /// Overrides the <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)"/> to leverage the <see cref="Company.AppName.Business.Data.Database"/> connection management.
    /// </summary>
    /// <param name="optionsBuilder">The <see cref="DbContextOptionsBuilder"/>.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Uses the DB connection management from the database class to ensure the likes of DB connection pooling.
        if (!optionsBuilder.IsConfigured)
#if (implement_sqlserver)
            optionsBuilder.UseSqlServer(BaseDatabase.GetConnection());
#endif
#if (implement_mysql)
            optionsBuilder.UseMySql(BaseDatabase.GetConnection(), ServerVersion.Create(new Version(8, 0, 33), Pomelo.EntityFrameworkCore.MySql.Infrastructure.ServerType.MySql));
#endif
#if (implement_postgres)
            optionsBuilder.UseNpgsql(BaseDatabase.GetConnection());
#endif
    }

    /// <summary>
    /// Overrides the <see cref="DbContext.OnModelCreating(ModelBuilder)"/>.
    /// </summary>
    /// <param name="modelBuilder">The <see cref="ModelBuilder"/>.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Add the generated models to the model builder.
        modelBuilder.AddGeneratedModels();
    }
}