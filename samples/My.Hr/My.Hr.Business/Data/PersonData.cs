using Beef;
using Beef.Data.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using My.Hr.Common.Entities;

namespace My.Hr.Business.Data
{
    //public partial class PersonData
    //{
    //    /// <summary>
    //    /// Bind the implementation(s) to the corresponding extension(s) for invocation.
    //    /// </summary>
    //    partial void PersonDataCtor()
    //    {
    //        _getByArgsOnQuery = GetByArgsOnQuery;
    //    }

    //    /// <summary>
    //    /// Performs the query filtering.
    //    /// </summary>
    //    private IQueryable<EfModel.Person> GetByArgsOnQuery(IQueryable<EfModel.Person> q, PersonArgs? args, IEfDbArgs efArgs)
    //    {
    //        _ef.WithWildcard(args?.FirstName, (w) => q = q.Where(x => EF.Functions.Like(x.FirstName, w)));
    //        _ef.WithWildcard(args?.LastName, (w) => q = q.Where(x => EF.Functions.Like(x.LastName, w)));
    //        _ef.With(args?.Genders, () => q = q.Where(x => args!.Genders!.ToCodeList().Contains(x.GenderCode)));
    //        return q.OrderBy(x => x.LastName).ThenBy(x => x.FirstName);
    //    }
    //}
}