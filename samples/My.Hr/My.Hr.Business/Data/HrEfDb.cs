namespace My.Hr.Business.Data
{
    /// <summary>
    /// Represents the <b>My.Hr</b> database using Entity Framework.
    /// </summary>
    public class HrEfDb : EfDb<HrEfDbContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="HrEfDb"/> class.
        /// </summary>
        /// <param name="dbContext">The entity framework database context.</param>
        /// <param name="mapper">The <see cref="IMapper"/>.</param>
        public HrEfDb(HrEfDbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }
    }
}