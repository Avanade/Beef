using Microsoft.EntityFrameworkCore;

namespace Company.AppName.Business.Data.EfModel
{
    /// <summary>
    /// Represents the Entity Framework <see cref="ModelBuilder"/>.
    /// </summary>
    public static class EfModelBuilder
    {
        /// <summary>
        /// Provides the <see cref="ModelBuilder"/> configuration.
        /// </summary>
        /// <param name="modelBuilder">The <see cref="ModelBuilder"/>.</param>
        public static void Configure(ModelBuilder modelBuilder)
        {
            Gender.AddToModel(modelBuilder);
            Person.AddToModel(modelBuilder);
        }
    }
}