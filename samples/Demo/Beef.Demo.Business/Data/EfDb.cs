using Beef.Data.EntityFrameworkCore;

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Represents the <b>Beef.Demo</b> database using Entity Framework.
    /// </summary>
    public class EfDb : EfDbBase<EfDbContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EfDb"/> class.
        /// </summary>
        /// <param name="dbContext">The entity framework database context.</param>
        public EfDb(EfDbContext dbContext) : base(dbContext) { }
    }
}