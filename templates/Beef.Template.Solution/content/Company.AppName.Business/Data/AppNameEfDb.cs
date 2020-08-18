using Beef.Data.EntityFrameworkCore;

namespace Company.AppName.Business.Data
{
    /// <summary>
    /// Represents the <b>Company.AppName</b> database using Entity Framework.
    /// </summary>
    public class AppNameEfDb : EfDbBase<AppNameEfDbContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppNameEfDb"/> class.
        /// </summary>
        /// <param name="dbContext">The entity framework database context.</param>
        public AppNameEfDb(AppNameEfDbContext dbContext) : base(dbContext) => OnUpdatePreReadForNotFound = true;
    }
}