namespace Company.AppName.Business.Data;

public partial class PersonData
{
    /// <summary>
    /// Bind the implementation(s) to the corresponding extension(s) for runtime invocation.
    /// </summary>
    partial void PersonDataCtor()
    {
        _getByArgsOnQuery = GetByArgsOnQuery;
    }

    /// <summary>
    /// Performs the query filtering.
    /// </summary>
#if (implement_database)
    private void GetByArgsOnQuery(DatabaseParameterCollection p, PersonArgs? args)
    {
        p.ParamWithWildcard(args?.FirstName, "FirstName")
         .ParamWithWildcard(args?.LastName, "LastName")
         .TableValuedParamWith(args?.Genders, "GenderCodes", () => _db.CreateTableValuedParameter(args!.Genders!.ToCodeList()));
    }
#endif
#if (implement_sqlserver || implement_mysql)
    private IQueryable<EfModel.Person> GetByArgsOnQuery(IQueryable<EfModel.Person> q, PersonArgs? args)
    {
        _ef.WithWildcard(args?.FirstName, (w) => q = q.Where(x => EF.Functions.Like(x.FirstName!, w)));
        _ef.WithWildcard(args?.LastName, (w) => q = q.Where(x => EF.Functions.Like(x.LastName!, w)));
        _ef.With(args?.Genders, () => q = q.Where(x => args!.Genders!.ToCodeList().Contains(x.GenderCode)));
        return q.OrderBy(x => x.LastName).ThenBy(x => x.FirstName);
    }
#endif
#if (implement_postgres)
    private IQueryable<EfModel.Person> GetByArgsOnQuery(IQueryable<EfModel.Person> q, PersonArgs? args)
    {
        _ef.WithWildcard(args?.FirstName, (w) => q = q.Where(x => EF.Functions.ILike(x.FirstName!, w)));
        _ef.WithWildcard(args?.LastName, (w) => q = q.Where(x => EF.Functions.ILike(x.LastName!, w)));
        _ef.With(args?.Genders, () => q = q.Where(x => args!.Genders!.ToCodeList().Contains(x.GenderCode)));
        return q.OrderBy(x => x.LastName).ThenBy(x => x.FirstName);
    }
#endif
#if (implement_cosmos)
    private IQueryable<Model.Person> GetByArgsOnQuery(IQueryable<Model.Person> q, PersonArgs? args)
    {
        q = q.WhereWildcard(x => x.FirstName, args?.FirstName);
        q = q.WhereWildcard(x => x.LastName, args?.LastName);
        q = q.WhereWith(args?.Genders, x => args!.Genders!.ToCodeList().Contains(x.Gender));
        return q.OrderBy(x => x.LastName);
    }
#endif
}