using Beef.Data.EntityFrameworkCore;

namespace Company.AppName.Business.Data
{
    /// <summary>
    /// Represents the <b>Beef.Demo</b> database using Entity Framework.
    /// </summary>
    public class EfDb : EfDb<EfDbContext, EfDb>
    {
        public EfDb() => OnUpdatePreReadForNotFound = true;
    }
}