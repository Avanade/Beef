using Company.AppName.Business.Data.EfModel;
using Microsoft.EntityFrameworkCore;

namespace Company.AppName.Business.Data
{
    /// <summary>
    /// Represents the Entity Framework <see cref="DbContext"/>.
    /// </summary>
    public class AppNameEfDbContext : DbContext
    {
        /// <summary>
        /// Overrides the <see cref="DbContext.OnConfiguring(DbContextOptionsBuilder)"/> to leverage the <see cref="Beef.Demo.Business.Data.Database"/> connection management.
        /// </summary>
        /// <param name="optionsBuilder">The <see cref="DbContextOptionsBuilder"/>.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            // Uses the DB connection management from the database project - ensures DB connection pooling and required DB session context setting.
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseSqlServer(Data.AppNameDb.Default.CreateConnection());
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
}