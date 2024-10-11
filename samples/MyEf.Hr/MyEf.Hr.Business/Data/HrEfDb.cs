namespace MyEf.Hr.Business.Data;

/// <summary>
/// Represents the <b>My.Hr</b> database using Entity Framework.
/// </summary>
/// <param name="dbContext">The entity framework database context.</param>
/// <param name="mapper">The <see cref="IMapper"/>.</param>
public class HrEfDb(HrEfDbContext dbContext, IMapper mapper) : EfDb<HrEfDbContext>(dbContext, mapper) { }