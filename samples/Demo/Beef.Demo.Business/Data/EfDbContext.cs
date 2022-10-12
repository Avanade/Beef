using Beef.Demo.Business.Data.EfModel;

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Represents the Entity Framework <see cref="DbContext"/>.
    /// </summary>
    public class EfDbContext : DbContext, IEfDbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EfDbContext"/> class.
        /// </summary>
        /// <param name="options">The <see cref="DbContextOptions{EfDbContext}"/>.</param>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        public EfDbContext(DbContextOptions<EfDbContext> options, IDatabase db) : base(options) => BaseDatabase = db ?? throw new ArgumentNullException(nameof(db));

        /// <summary>
        /// Gets the base <see cref="IDatabase"/>.
        /// </summary>
        public IDatabase BaseDatabase { get; }

        /// <summary>
        /// Overrides the <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)"/> to leverage the <see cref="Beef.Demo.Business.Data.Database"/> connection management.
        /// </summary>
        /// <param name="optionsBuilder">The <see cref="DbContextOptionsBuilder"/>.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Uses the DB connection management from the Database class - ensures DB connection management and required DB session context setting.
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

            // Perform the model building.
            modelBuilder.AddGeneratedModels();
        }
    }
}