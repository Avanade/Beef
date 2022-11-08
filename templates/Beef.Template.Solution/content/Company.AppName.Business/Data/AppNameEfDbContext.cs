using Company.AppName.Business.Data.EfModel;

namespace Company.AppName.Business.Data;

/// <summary>
/// Represents the Entity Framework <see cref="DbContext"/>.
/// </summary>
public class AppNameEfDbContext : DbContext, IEfDbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppNameEfDbContext"/> class.
    /// </summary>
    /// <param name="options">The <see cref="DbContextOptions{AppNameEfDbContext}"/>.</param>
    /// <param name="db">The base <see cref="IDatabase"/>.</param>
    public AppNameEfDbContext(DbContextOptions<AppNameEfDbContext> options, IDatabase db) : base(options) => BaseDatabase = db ?? throw new ArgumentNullException(nameof(db));

    /// <summary>
    /// Gets the base <see cref="IDatabase"/>.
    /// </summary>
    public IDatabase BaseDatabase { get; }

    /// <summary>
    /// Overrides the <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)"/> to leverage the <see cref="Company.AppName.Business.Data.Database"/> connection management.
    /// </summary>
    /// <param name="optionsBuilder">The <see cref="DbContextOptionsBuilder"/>.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Uses the DB connection management from the database class - ensures DB connection pooling and required DB session context setting.
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer(BaseDatabase.GetConnection());
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