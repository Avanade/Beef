namespace Company.AppName.Business.Data;

public partial class PersonData
{
#if (implement_entityframework | implement_cosmos)
    private static readonly QueryArgsConfig _config = QueryArgsConfig.Create()
        .WithFilter(filter => filter
            .AddField<string>(nameof(Person.LastName), c => c.WithOperators(QueryFilterOperator.AllStringOperators).WithUpperCase())
            .AddField<string>(nameof(Person.FirstName), c => c.WithOperators(QueryFilterOperator.AllStringOperators).WithUpperCase())
#if (implement_entityframework)
            .AddReferenceDataField<Gender>(nameof(Person.Gender), nameof(EfModel.Person.GenderCode)))
#else
            .AddReferenceDataField<Gender>(nameof(Person.Gender)))
#endif
        .WithOrderBy(orderby => orderby
            .AddField(nameof(Person.LastName))
            .AddField(nameof(Person.FirstName))
            .WithDefault($"{nameof(Person.LastName)}, {nameof(Person.FirstName)}"));

#endif
    /// <summary>
    /// Bind the implementation(s) to the corresponding extension(s) for runtime invocation.
    /// </summary>
    partial void PersonDataCtor()
    {
        _getByArgsOnQuery = GetByArgsOnQuery;
#if (implement_entityframework | implement_cosmos)
        _getByQueryOnQuery = (q, args) => q.Where(_config, args).OrderBy(_config, args);
#endif
    }

    /// <summary>
    /// Performs the query filtering.
    /// </summary>
#if (implement_database)
    private void GetByArgsOnQuery(DatabaseParameterCollection p, PersonArgs? args)
    {
        p.ParamWithWildcard(args?.FirstName, "FirstName")
         .ParamWithWildcard(args?.LastName, "LastName")
         .JsonParamWith(args?.Genders, "GenderCodes", () => args!.Genders!.ToCodeList());
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