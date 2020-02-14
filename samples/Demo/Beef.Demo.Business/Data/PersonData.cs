using Beef.Data.Database;
using Beef.Data.EntityFrameworkCore;
using Beef.Demo.Common.Entities;
using Beef.Entities;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

namespace Beef.Demo.Business.Data
{
    public partial class PersonData
    {
        public PersonData()
        {
            _getByArgsOnQuery = GetByArgsOnQuery;
            _getByArgsWithEfOnQuery = GetByArgsWithEfOnQuery;
            _markOnException = MarkOnException;
        }

        private void GetByArgsOnQuery(DatabaseParameters p, PersonArgs args, IDatabaseArgs dbArgs)
        {
            p.ParamWithWildcard(args?.FirstName, DbMapper.Default[Person.Property_FirstName])
             .ParamWithWildcard(args?.LastName, DbMapper.Default[Person.Property_LastName])
             .TableValuedParamWith(args?.Genders, "GenderIds", () => TableValuedParameter.Create(args.Genders.ToGuidIdList()));
        }

        private IQueryable<EfModel.Person> GetByArgsWithEfOnQuery(IQueryable<EfModel.Person> q, PersonArgs args, IEfDbArgs efArgs)
        {
            EfDb.WithWildcard(args?.FirstName, (w) => q = q.Where(x => EF.Functions.Like(x.FirstName, w)));
            EfDb.WithWildcard(args?.LastName, (w) => q = q.Where(x => EF.Functions.Like(x.LastName, w)));
            EfDb.With(args?.Genders, () => q = q.Where(x => args.Genders.ToGuidIdList().Contains(x.GenderId.Value)));
            return q.OrderBy(x => x.LastName).ThenBy(x => x.FirstName);
        }

        private async Task<Person> MergeOnImplementationAsync(Guid personFromId, Guid personToId)
        {
            // This is an example (illustrative) of executing an Agent from an API - this should be used for cross-domain calls only; otherwise, use database (performance).
            var pf = await new Common.Agents.PersonAgent().GetAsync(personFromId).ConfigureAwait(false);
            if (pf.Value == null)
                throw new ValidationException($"Person from does not exist.");

            var pt = await new Common.Agents.PersonAgent().GetAsync(personToId).ConfigureAwait(false);
            if (pt.Value == null)
                throw new ValidationException($"Person from does not exist.");

            // Pretend a merge actually occured.

            return pt.Value;
        }

        private Task MarkOnImplementationAsync()
        {
            return Task.CompletedTask;
        }

        private void MarkOnException(Exception ex)
        {
            if (ex is NotImplementedException)
                throw new NotSupportedException();
        }

        private async Task<PersonDetailCollectionResult> GetDetailByArgsOnImplementationAsync(PersonArgs args, PagingArgs paging)
        {
            var pdcr = new PersonDetailCollectionResult(new PagingResult(paging));

            await Database.Default.StoredProcedure("[Demo].[spPersonGetDetailByArgs]")
                .Params(p =>
                {
                    p.ParamWithWildcard(args?.FirstName, DbMapper.Default[Person.Property_FirstName])
                     .ParamWithWildcard(args?.LastName, DbMapper.Default[Person.Property_LastName])
                     .TableValuedParamWith(args?.Genders, "GenderIds", () => TableValuedParameter.Create(args.Genders.ToGuidIdList()));
                })
                .SelectQueryMultiSetAsync(pdcr.Paging,
                    new MultiSetCollArgs<PersonCollection, Person>(PersonData.DbMapper.Default, (r) => r.ForEach((p) => { var pd = new PersonDetail(); pd.CopyFrom(p); pdcr.Result.Add(pd); })),
                    new MultiSetCollArgs<WorkHistoryCollection, WorkHistory>(WorkHistoryData.DbMapper.Default, (r) =>
                    {
                        PersonDetail pd = null;
                        foreach (var wh in r)
                        {
                            if (pd == null || wh.PersonId != pd.Id)
                            {
                                pd = pdcr.Result.Where(x => x.Id == wh.PersonId).Single();
                                pd.History = new WorkHistoryCollection();
                            }
                            
                            pd.History.Add(wh);
                        }
                    }));

            return pdcr;
        }

        private async Task<PersonDetail> GetDetailOnImplementationAsync(Guid id)
        {
            PersonDetail pd = null;

            await Database.Default.StoredProcedure("[Demo].[spPersonGetDetail]")
                .Param(DbMapper.Default.GetParamName(PersonDetail.Property_Id), id)
                .SelectQueryMultiSetAsync(
                    new MultiSetSingleArgs<Person>(PersonData.DbMapper.Default, (r) => { pd = new PersonDetail(); pd.CopyFrom(r); }, isMandatory: false),
                    new MultiSetCollArgs<WorkHistoryCollection, WorkHistory>(WorkHistoryData.DbMapper.Default, (r) => pd.History = r));

            return pd;
        }

        private async Task<PersonDetail> UpdateDetailOnImplementationAsync(PersonDetail value)
        {
            PersonDetail pd = null;

            await Database.Default.StoredProcedure("[Demo].[spPersonUpdateDetail]")
                .Params((p) => PersonData.DbMapper.Default.MapToDb(value, p, Mapper.OperationTypes.Update))
                .TableValuedParam("@WorkHistoryList", WorkHistoryData.DbMapper.Default.CreateTableValuedParameter(value.History))
                .ReselectRecordParam()
                .SelectQueryMultiSetAsync(
                    new MultiSetSingleArgs<Person>(PersonData.DbMapper.Default, (r) => { pd = new PersonDetail(); pd.CopyFrom(r); }, false, true),
                    new MultiSetCollArgs<WorkHistoryCollection, WorkHistory>(WorkHistoryData.DbMapper.Default, (r) => pd.History = r));

            return pd;
        }

        public partial class EfMapper
        {
            private readonly EfDbMapper<Address, EfModel.Person> _addressMapper = EfDbMapper.CreateAuto<Address, EfModel.Person>();

            partial void EfMapperCtor()
            {
                SrceProperty(s => s.Address).SetMapper(_addressMapper);
            }
        }
    }
}
