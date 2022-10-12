namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Represents the <b>Beef.Demo</b> database using Entity Framework.
    /// </summary>
    public class EfDb : EfDb<EfDbContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EfDb"/> class.
        /// </summary>
        /// <param name="dbContext">The entity framework database context.</param>
        /// <param name="mapper">The <see cref="IMapper"/>.</param>
        public EfDb(EfDbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }
    }
}