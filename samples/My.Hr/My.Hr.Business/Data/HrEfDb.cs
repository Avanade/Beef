using Beef.Data.EntityFrameworkCore;

namespace My.Hr.Business.Data
{
    /// <summary>
    /// Represents the <b>My.Hr</b> database using Entity Framework.
    /// </summary>
    public class HrEfDb : EfDbBase<HrEfDbContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HrEfDb"/> class.
        /// </summary>
        /// <param name="dbContext">The entity framework database context.</param>
        public HrEfDb(HrEfDbContext dbContext) : base(dbContext) => OnUpdatePreReadForNotFound = true;
    }
}