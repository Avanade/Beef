namespace Company.AppName.Business.Data;

/// <summary>
/// Represents the <b>Company.AppName</b> database using Entity Framework.
/// </summary>
/// <param name="dbContext">The entity framework database context.</param>
/// <param name="mapper">The <see cref="IMapper"/>.</param>
public class AppNameEfDb(AppNameEfDbContext dbContext, IMapper? mapper = null) : EfDb<AppNameEfDbContext>(dbContext, mapper) { }