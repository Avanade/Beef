using Beef;
#if (implement_database)
using Beef.Data.Database;
#endif
#if (implement_entityframework)
using Beef.Data.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
#endif
#if (implement_cosmos)
using Beef.Data.Cosmos;
using System.Linq;
#endif
using Company.AppName.Business.Entities;

namespace Company.AppName.Business.Data
{
    public partial class PersonData
    {
        /// <summary>
        /// Bind the implementation(s) to the corresponding extension(s) for invocation.
        /// </summary>
        partial void PersonDataCtor()
        {
            _getByArgsOnQuery = GetByArgsOnQuery;
        }

        /// <summary>
        /// Performs the query filtering.
        /// </summary>
#if (implement_database)
        private void GetByArgsOnQuery(DatabaseParameters p, PersonArgs? args, IDatabaseArgs dbArgs)
        {
            p.ParamWithWildcard(args?.FirstName, DbMapper.Default[nameof(Person.FirstName)])
             .ParamWithWildcard(args?.LastName, DbMapper.Default[nameof(Person.LastName)])
             .TableValuedParamWith(args?.Genders, "GenderCodes", () => TableValuedParameter.Create(args!.Genders!.ToCodeList()));
        }
#endif
#if (implement_entityframework)
        private IQueryable<EfModel.Person> GetByArgsOnQuery(IQueryable<EfModel.Person> q, PersonArgs? args, EfDbArgs efArgs)
        {
            _ef.WithWildcard(args?.FirstName, (w) => q = q.Where(x => EF.Functions.Like(x.FirstName, w)));
            _ef.WithWildcard(args?.LastName, (w) => q = q.Where(x => EF.Functions.Like(x.LastName, w)));
            _ef.With(args?.Genders, () => q = q.Where(x => args!.Genders!.ToCodeList().Contains(x.GenderCode)));
            return q.OrderBy(x => x.LastName).ThenBy(x => x.FirstName);
        }
#endif
#if (implement_cosmos)
        private IQueryable<Model.Person> GetByArgsOnQuery(IQueryable<Model.Person> q, PersonArgs? args, CosmosDbArgs dbArgs)
        {
            q = q.WhereWildcard(x => x.FirstName, args?.FirstName);
            q = q.WhereWildcard(x => x.LastName, args?.LastName);
            q = q.WhereWith(args?.Genders, x => args!.Genders!.ToCodeList().Contains(x.Gender));
            return q.OrderBy(x => x.LastName);
        }
#endif
    }
}