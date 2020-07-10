using Beef.Data.Database;
using Beef.Demo.Business.Data.EfModel;
using Microsoft.EntityFrameworkCore;

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Represents the Entity Framework <see cref="DbContext"/>.
    /// </summary>
    public class EfDbContext : DbContext
    {
        private readonly IDatabase _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="EfDbContext"/> class.
        /// </summary>
        /// <param name="options">The <see cref="DbContextOptions{EfDbContext}"/>.</param>
        /// <param name="db"></param>
        public EfDbContext(DbContextOptions<EfDbContext> options, IDatabase db) : base(options) => _db = db;

        /// <summary>
        /// Overrides the <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)"/> to leverage the <see cref="Beef.Demo.Business.Data.Database"/> connection management.
        /// </summary>
        /// <param name="optionsBuilder">The <see cref="DbContextOptionsBuilder"/>.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Uses the DB connection management from the Database class - ensures DB connection management and required DB session context setting.
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlServer(_db.CreateConnection());
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