using Beef.Demo.Business.Data.EfModel;
using Microsoft.EntityFrameworkCore;

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Represents the Entity Framework <see cref="DbContext"/>.
    /// </summary>
    public class EfDbContext : DbContext
    {
        /// <summary>
        /// Overrides the <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)"/> to leverage the <see cref="Beef.Demo.Business.Data.Database"/> connection management.
        /// </summary>
        /// <param name="optionsBuilder">The <see cref="DbContextOptionsBuilder"/>.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Uses the DB connection management from the Database class - ensures DB connection management and required DB session context setting.
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlServer(Beef.Demo.Business.Data.Database.Default.CreateConnection());
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
