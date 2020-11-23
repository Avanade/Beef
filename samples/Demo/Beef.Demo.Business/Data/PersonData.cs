using Beef.Data.Database;
using Beef.Data.EntityFrameworkCore;
using Beef.Demo.Common.Entities;
using Beef.Entities;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Beef.Demo.Business.Data
{
    public partial class PersonData
    {
        private readonly ILogger _logger;
        private readonly Common.Agents.IPersonAgent _agent;

        public PersonData(IDatabase db, IEfDb ef, ILogger<PersonData> logger, Common.Agents.IPersonAgent agent) : this(db, ef)
        { 
            _logger = Check.NotNull(logger, nameof(logger));
            _agent = Check.NotNull(agent, nameof(agent));
        }

        partial void PersonDataCtor()
        {
            _getByArgsOnQuery = GetByArgsOnQuery;
            _getByArgsWithEfOnQuery = GetByArgsWithEfOnQuery;
            _markOnException = MarkOnException;
        }

        private void GetByArgsOnQuery(DatabaseParameters p, PersonArgs args, IDatabaseArgs dbArgs)
        {
            p.ParamWithWildcard(args?.FirstName, DbMapper.Default[nameof(Person.FirstName)])
             .ParamWithWildcard(args?.LastName, DbMapper.Default[nameof(Person.LastName)])
             .TableValuedParamWith(args?.Genders, "GenderIds", () => TableValuedParameter.Create(args.Genders.ToGuidIdList()));
        }

        private IQueryable<EfModel.Person> GetByArgsWithEfOnQuery(IQueryable<EfModel.Person> q, PersonArgs args, IEfDbArgs efArgs)
        {
            _ef.WithWildcard(args?.FirstName, (w) => q = q.Where(x => EF.Functions.Like(x.FirstName, w)));
            _ef.WithWildcard(args?.LastName, (w) => q = q.Where(x => EF.Functions.Like(x.LastName, w)));
            _ef.With(args?.Genders, () => q = q.Where(x => args.Genders.ToGuidIdList().Contains(x.GenderId.Value)));
            return q.OrderBy(x => x.LastName).ThenBy(x => x.FirstName);
        }

        private async Task<Person> MergeOnImplementationAsync(Guid personFromId, Guid personToId)
        {
            // This is an example (illustrative) of executing an Agent from an API - this should be used for cross-domain calls only; otherwise, use database (performance).
            // TODO: Add back in.
            //var pf = await new Common.Agents.PersonAgent().GetAsync(personFromId).ConfigureAwait(false);
            //if (pf.Value == null)
            //    throw new ValidationException($"Person from does not exist.");

            //var pt = await new Common.Agents.PersonAgent().GetAsync(personToId).ConfigureAwait(false);
            //if (pt.Value == null)
            //    throw new ValidationException($"Person to does not exist.");

            // Pretend a merge actually occured.

            //return pt.Value;
            return await Task.FromResult((Person)null).ConfigureAwait(false);
        }

        private Task MarkOnImplementationAsync()
        {
            _logger.LogWarning("Mark operation implementation currently does not exist.");
            return Task.CompletedTask;
        }

        private Task<MapCoordinates> MapOnImplementationAsync(MapArgs args)
        {
            return Task.FromResult(args.Coordinates);
        }

        private void MarkOnException(Exception ex)
        {
            if (ex is NotImplementedException)
                throw new NotSupportedException();
        }

        private Task<Person> GetNoArgsOnImplementationAsync()
        {
            return Task.FromResult(new Person { FirstName = "No", LastName = "Args" });
        }

        private Task ThrowErrorOnImplementationAsync()
        {
            _logger.LogWarning("The data is beyond corrupt and we cannot continue.");
            throw new InvalidOperationException("Data corruption error!");
        }

        private async Task<string> InvokeApiViaAgentOnImplementationAsync(Guid id)
        {
            var result = await _agent.GetAsync(id).ConfigureAwait(false);
            return result.Value.LastName;
        }

        private async Task<PersonDetailCollectionResult> GetDetailByArgsOnImplementationAsync(PersonArgs args, PagingArgs paging)
        {
            var pdcr = new PersonDetailCollectionResult(new PagingResult(paging));

            await _db.StoredProcedure("[Demo].[spPersonGetDetailByArgs]")
                .Params(p =>
                {
                    p.ParamWithWildcard(args?.FirstName, DbMapper.Default[nameof(Person.FirstName)])
                     .ParamWithWildcard(args?.LastName, DbMapper.Default[nameof(Person.LastName)])
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

            System.Diagnostics.Debug.WriteLine($"One, Thread: {System.Threading.Thread.CurrentThread.ManagedThreadId }");
            await GetAsync(id);

            System.Diagnostics.Debug.WriteLine($"Two, Thread: {System.Threading.Thread.CurrentThread.ManagedThreadId }");
            await _db.StoredProcedure("[Demo].[spPersonGetDetail]")
                .Param(DbMapper.Default.GetParamName(nameof(PersonDetail.Id)), id)
                .SelectQueryMultiSetAsync(
                    new MultiSetSingleArgs<Person>(PersonData.DbMapper.Default, (r) => { pd = new PersonDetail(); pd.CopyFrom(r); }, isMandatory: false),
                    new MultiSetCollArgs<WorkHistoryCollection, WorkHistory>(WorkHistoryData.DbMapper.Default, (r) => pd.History = r));

            return pd;
        }

        private async Task<PersonDetail> UpdateDetailOnImplementationAsync(PersonDetail value)
        {
            PersonDetail pd = null;

            await _db.StoredProcedure("[Demo].[spPersonUpdateDetail]")
                .Params((p) => PersonData.DbMapper.Default.MapToDb(value, p, Mapper.OperationTypes.Update))
                .TableValuedParam("@WorkHistoryList", WorkHistoryData.DbMapper.Default.CreateTableValuedParameter(value.History))
                .ReselectRecordParam()
                .SelectQueryMultiSetAsync(
                    new MultiSetSingleArgs<Person>(PersonData.DbMapper.Default, (r) => { pd = new PersonDetail(); pd.CopyFrom(r); }, false, true),
                    new MultiSetCollArgs<WorkHistoryCollection, WorkHistory>(WorkHistoryData.DbMapper.Default, (r) => pd.History = r));

            return pd;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        private Task<Person> GetNullOnImplementationAsync(string _, List<string> __)
        {
            return Task.FromResult<Person>(null);
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
