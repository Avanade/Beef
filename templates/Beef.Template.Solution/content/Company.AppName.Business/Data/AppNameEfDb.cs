using Beef.Data.EntityFrameworkCore;

namespace Company.AppName.Business.Data
{
    /// <summary>
    /// Represents the <b>Beef.Demo</b> database using Entity Framework.
    /// </summary>
    public class AppNameEfDb : EfDb<AppNameEfDbContext, AppNameEfDb>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AppNameEfDb"/> class.
        /// </summary>
        public AppNameEfDb() => OnUpdatePreReadForNotFound = true;
    }
}