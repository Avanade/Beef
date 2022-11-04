namespace Company.AppName.Business.Data;

/// <summary>
/// Represents the <b>Company.AppName</b> database using Entity Framework.
/// </summary>
public class AppNameEfDb : EfDb<AppNameEfDbContext>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppNameEfDb"/> class.
    /// </summary>
    /// <param name="dbContext">The entity framework database context.</param>
    /// <param name="mapper">The <see cref="IMapper"/>.</param>
    public AppNameEfDb(AppNameEfDbContext dbContext, IMapper mapper) : base(dbContext, mapper) { }
}