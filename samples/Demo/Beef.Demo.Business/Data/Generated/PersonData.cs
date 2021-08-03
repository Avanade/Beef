/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Beef;
using Beef.Business;
using Beef.Data.Database;
using Beef.Data.EntityFrameworkCore;
using Beef.Entities;
using Beef.Mapper;
using Beef.Mapper.Converters;
using Beef.Demo.Common.Entities;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Provides the <see cref="Person"/> data access.
    /// </summary>
    public partial class PersonData : IPersonData
    {
        private readonly IDatabase _db;
        private readonly IEfDb _ef;
        private readonly AutoMapper.IMapper _mapper;
        private readonly Microsoft.Extensions.Logging.ILogger<PersonData> _logger;
        private readonly Common.Agents.IPersonAgent _personAgent;

        #region Extensions

        private Func<Person, IDatabaseArgs, Task>? _createOnBeforeAsync;
        private Func<Person, Task>? _createOnAfterAsync;
        private Action<Exception>? _createOnException;
        private Func<Guid, IDatabaseArgs, Task>? _deleteOnBeforeAsync;
        private Func<Guid, Task>? _deleteOnAfterAsync;
        private Action<Exception>? _deleteOnException;
        private Func<Person, IDatabaseArgs, Task>? _updateWithRollbackOnBeforeAsync;
        private Func<Person, Task>? _updateWithRollbackOnAfterAsync;
        private Action<Exception>? _updateWithRollbackOnException;
        private Action<DatabaseParameters, IDatabaseArgs>? _getAllOnQuery;
        private Func<IDatabaseArgs, Task>? _getAllOnBeforeAsync;
        private Func<PersonCollectionResult, Task>? _getAllOnAfterAsync;
        private Action<Exception>? _getAllOnException;
        private Action<DatabaseParameters, IDatabaseArgs>? _getAll2OnQuery;
        private Func<IDatabaseArgs, Task>? _getAll2OnBeforeAsync;
        private Func<PersonCollectionResult, Task>? _getAll2OnAfterAsync;
        private Action<Exception>? _getAll2OnException;
        private Action<DatabaseParameters, PersonArgs?, IDatabaseArgs>? _getByArgsOnQuery;
        private Func<PersonArgs?, IDatabaseArgs, Task>? _getByArgsOnBeforeAsync;
        private Func<PersonCollectionResult, PersonArgs?, Task>? _getByArgsOnAfterAsync;
        private Action<Exception>? _getByArgsOnException;
        private Action<Exception>? _getDetailByArgsOnException;
        private Action<Exception>? _mergeOnException;
        private Action<Exception>? _markOnException;
        private Action<Exception>? _mapOnException;
        private Action<Exception>? _getNoArgsOnException;
        private Action<Exception>? _getDetailOnException;
        private Action<Exception>? _updateDetailOnException;
        private Action<Exception>? _getNullOnException;
        private Func<IQueryable<EfModel.Person>, PersonArgs?, IEfDbArgs, IQueryable<EfModel.Person>>? _getByArgsWithEfOnQuery;
        private Func<PersonArgs?, IEfDbArgs, Task>? _getByArgsWithEfOnBeforeAsync;
        private Func<PersonCollectionResult, PersonArgs?, Task>? _getByArgsWithEfOnAfterAsync;
        private Action<Exception>? _getByArgsWithEfOnException;
        private Action<Exception>? _throwErrorOnException;
        private Action<Exception>? _invokeApiViaAgentOnException;
        private Func<Guid, IEfDbArgs, Task>? _getWithEfOnBeforeAsync;
        private Func<Person?, Guid, Task>? _getWithEfOnAfterAsync;
        private Action<Exception>? _getWithEfOnException;
        private Func<Person, IEfDbArgs, Task>? _createWithEfOnBeforeAsync;
        private Func<Person, Task>? _createWithEfOnAfterAsync;
        private Action<Exception>? _createWithEfOnException;
        private Func<Person, IEfDbArgs, Task>? _updateWithEfOnBeforeAsync;
        private Func<Person, Task>? _updateWithEfOnAfterAsync;
        private Action<Exception>? _updateWithEfOnException;
        private Func<Guid, IEfDbArgs, Task>? _deleteWithEfOnBeforeAsync;
        private Func<Guid, Task>? _deleteWithEfOnAfterAsync;
        private Action<Exception>? _deleteWithEfOnException;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonData"/> class.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="ef">The <see cref="IEfDb"/>.</param>
        /// <param name="mapper">The <see cref="AutoMapper.IMapper"/>.</param>
        /// <param name="logger">The <see cref="Microsoft.Extensions.Logging.ILogger{PersonData}"/>.</param>
        /// <param name="personAgent">The <see cref="Common.Agents.IPersonAgent"/>.</param>
        public PersonData(IDatabase db, IEfDb ef, AutoMapper.IMapper mapper, Microsoft.Extensions.Logging.ILogger<PersonData> logger, Common.Agents.IPersonAgent personAgent)
        {
            _db = Check.NotNull(db, nameof(db));
            _ef = Check.NotNull(ef, nameof(ef));
            _mapper = Check.NotNull(mapper, nameof(mapper));
            _logger = Check.NotNull(logger, nameof(logger));
            _personAgent = Check.NotNull(personAgent, nameof(personAgent));
            PersonDataCtor();
        }

        partial void PersonDataCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Creates a new <see cref="Person"/>.
        /// </summary>
        /// <param name="value">The <see cref="Person"/>.</param>
        /// <returns>The created <see cref="Person"/>.</returns>
        public Task<Person> CreateAsync(Person value) => DataInvoker.Current.InvokeAsync(this, async () =>
        {
            Person __result;
            var __dataArgs = DbMapper.Default.CreateArgs("[Demo].[spPersonCreate]");
            await (_createOnBeforeAsync?.Invoke(value, __dataArgs) ?? Task.CompletedTask).ConfigureAwait(false);
            __result = await _db.CreateAsync(__dataArgs, Check.NotNull(value, nameof(value))).ConfigureAwait(false);
            await (_createOnAfterAsync?.Invoke(__result) ?? Task.CompletedTask).ConfigureAwait(false);
            return __result;
        }, new BusinessInvokerArgs { ExceptionHandler = _createOnException });

        /// <summary>
        /// Deletes the specified <see cref="Person"/>.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        public Task DeleteAsync(Guid id) => DataInvoker.Current.InvokeAsync(this, async () =>
        {
            var __dataArgs = DbMapper.Default.CreateArgs("[Demo].[spPersonDelete]");
            await (_deleteOnBeforeAsync?.Invoke(id, __dataArgs) ?? Task.CompletedTask).ConfigureAwait(false);
            await _db.DeleteAsync(__dataArgs, id).ConfigureAwait(false);
            await (_deleteOnAfterAsync?.Invoke(id) ?? Task.CompletedTask).ConfigureAwait(false);
        }, new BusinessInvokerArgs { ExceptionHandler = _deleteOnException });

        /// <summary>
        /// Gets the specified <see cref="Person"/>.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <returns>The selected <see cref="Person"/> where found.</returns>
        public Task<Person?> GetAsync(Guid id) => DataInvoker.Current.InvokeAsync(this, async () =>
        {
            var __dataArgs = DbMapper.Default.CreateArgs("[Demo].[spPersonGet]");
            return await _db.GetAsync(__dataArgs, id).ConfigureAwait(false);
        });

        /// <summary>
        /// Updates an existing <see cref="Person"/>.
        /// </summary>
        /// <param name="value">The <see cref="Person"/>.</param>
        /// <returns>The updated <see cref="Person"/>.</returns>
        public Task<Person> UpdateAsync(Person value) => DataInvoker.Current.InvokeAsync(this, async () =>
        {
            var __dataArgs = DbMapper.Default.CreateArgs("[Demo].[spPersonUpdate]");
            return await _db.UpdateAsync(__dataArgs, Check.NotNull(value, nameof(value))).ConfigureAwait(false);
        });

        /// <summary>
        /// Updates an existing <see cref="Person"/>.
        /// </summary>
        /// <param name="value">The <see cref="Person"/>.</param>
        /// <returns>The updated <see cref="Person"/>.</returns>
        public Task<Person> UpdateWithRollbackAsync(Person value) => DataInvoker.Current.InvokeAsync(this, async () =>
        {
            Person __result;
            var __dataArgs = DbMapper.Default.CreateArgs("[Demo].[spPersonUpdate]");
            await (_updateWithRollbackOnBeforeAsync?.Invoke(value, __dataArgs) ?? Task.CompletedTask).ConfigureAwait(false);
            __result = await _db.UpdateAsync(__dataArgs, Check.NotNull(value, nameof(value))).ConfigureAwait(false);
            await (_updateWithRollbackOnAfterAsync?.Invoke(__result) ?? Task.CompletedTask).ConfigureAwait(false);
            return __result;
        }, new BusinessInvokerArgs { ExceptionHandler = _updateWithRollbackOnException });

        /// <summary>
        /// Gets the <see cref="PersonCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="PersonCollectionResult"/>.</returns>
        public Task<PersonCollectionResult> GetAllAsync(PagingArgs? paging) => DataInvoker.Current.InvokeAsync(this, async () =>
        {
            PersonCollectionResult __result = new PersonCollectionResult(paging);
            var __dataArgs = DbMapper.Default.CreateArgs("[Demo].[spPersonGetAll]", __result.Paging!);
            await (_getAllOnBeforeAsync?.Invoke(__dataArgs) ?? Task.CompletedTask).ConfigureAwait(false);
            __result.Result = await _db.Query(__dataArgs, p => _getAllOnQuery?.Invoke(p, __dataArgs)).SelectQueryAsync<PersonCollection>().ConfigureAwait(false);
            await (_getAllOnAfterAsync?.Invoke(__result) ?? Task.CompletedTask).ConfigureAwait(false);
            return __result;
        }, new BusinessInvokerArgs { ExceptionHandler = _getAllOnException });

        /// <summary>
        /// Gets the <see cref="PersonCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <returns>The <see cref="PersonCollectionResult"/>.</returns>
        public Task<PersonCollectionResult> GetAll2Async() => DataInvoker.Current.InvokeAsync(this, async () =>
        {
            PersonCollectionResult __result = new PersonCollectionResult();
            var __dataArgs = DbMapper.Default.CreateArgs("[Demo].[spPersonGetAll]");
            await (_getAll2OnBeforeAsync?.Invoke(__dataArgs) ?? Task.CompletedTask).ConfigureAwait(false);
            __result.Result = await _db.Query(__dataArgs, p => _getAll2OnQuery?.Invoke(p, __dataArgs)).SelectQueryAsync<PersonCollection>().ConfigureAwait(false);
            await (_getAll2OnAfterAsync?.Invoke(__result) ?? Task.CompletedTask).ConfigureAwait(false);
            return __result;
        }, new BusinessInvokerArgs { ExceptionHandler = _getAll2OnException });

        /// <summary>
        /// Gets the <see cref="PersonCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Entities.PersonArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="PersonCollectionResult"/>.</returns>
        public Task<PersonCollectionResult> GetByArgsAsync(PersonArgs? args, PagingArgs? paging) => DataInvoker.Current.InvokeAsync(this, async () =>
        {
            PersonCollectionResult __result = new PersonCollectionResult(paging);
            var __dataArgs = DbMapper.Default.CreateArgs("[Demo].[spPersonGetByArgs]", __result.Paging!);
            await (_getByArgsOnBeforeAsync?.Invoke(args, __dataArgs) ?? Task.CompletedTask).ConfigureAwait(false);
            __result.Result = await _db.Query(__dataArgs, p => _getByArgsOnQuery?.Invoke(p, args, __dataArgs)).SelectQueryAsync<PersonCollection>().ConfigureAwait(false);
            await (_getByArgsOnAfterAsync?.Invoke(__result, args) ?? Task.CompletedTask).ConfigureAwait(false);
            return __result;
        }, new BusinessInvokerArgs { ExceptionHandler = _getByArgsOnException });

        /// <summary>
        /// Gets the <see cref="PersonDetailCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Entities.PersonArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="PersonDetailCollectionResult"/>.</returns>
        public Task<PersonDetailCollectionResult> GetDetailByArgsAsync(PersonArgs? args, PagingArgs? paging) => DataInvoker.Current.InvokeAsync(this, () => GetDetailByArgsOnImplementationAsync(args, paging), new BusinessInvokerArgs { ExceptionHandler = _getDetailByArgsOnException });

        /// <summary>
        /// Merge first <see cref="Person"/> into second.
        /// </summary>
        /// <param name="fromId">The from <see cref="Person"/> identifier.</param>
        /// <param name="toId">The to <see cref="Person"/> identifier.</param>
        /// <returns>A resultant <see cref="Person"/>.</returns>
        public Task<Person> MergeAsync(Guid fromId, Guid toId) => DataInvoker.Current.InvokeAsync(this, () => MergeOnImplementationAsync(fromId, toId), new BusinessInvokerArgs { ExceptionHandler = _mergeOnException });

        /// <summary>
        /// Mark <see cref="Person"/>.
        /// </summary>
        public Task MarkAsync() => DataInvoker.Current.InvokeAsync(this, () => MarkOnImplementationAsync(), new BusinessInvokerArgs { ExceptionHandler = _markOnException });

        /// <summary>
        /// Get <see cref="Person"/> at specified <see cref="MapCoordinates"/>.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Entities.MapArgs"/>).</param>
        /// <returns>A resultant <see cref="MapCoordinates"/>.</returns>
        public Task<MapCoordinates> MapAsync(MapArgs? args) => DataInvoker.Current.InvokeAsync(this, () => MapOnImplementationAsync(args), new BusinessInvokerArgs { ExceptionHandler = _mapOnException });

        /// <summary>
        /// Get no arguments.
        /// </summary>
        /// <returns>The selected <see cref="Person"/> where found.</returns>
        public Task<Person?> GetNoArgsAsync() => DataInvoker.Current.InvokeAsync(this, () => GetNoArgsOnImplementationAsync(), new BusinessInvokerArgs { ExceptionHandler = _getNoArgsOnException });

        /// <summary>
        /// Gets the specified <see cref="PersonDetail"/>.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <returns>The selected <see cref="PersonDetail"/> where found.</returns>
        public Task<PersonDetail?> GetDetailAsync(Guid id) => DataInvoker.Current.InvokeAsync(this, () => GetDetailOnImplementationAsync(id), new BusinessInvokerArgs { ExceptionHandler = _getDetailOnException });

        /// <summary>
        /// Updates an existing <see cref="PersonDetail"/>.
        /// </summary>
        /// <param name="value">The <see cref="PersonDetail"/>.</param>
        /// <returns>The updated <see cref="PersonDetail"/>.</returns>
        public Task<PersonDetail> UpdateDetailAsync(PersonDetail value) => DataInvoker.Current.InvokeAsync(this, () => UpdateDetailOnImplementationAsync(Check.NotNull(value, nameof(value))), new BusinessInvokerArgs { ExceptionHandler = _updateDetailOnException });

        /// <summary>
        /// Get Null.
        /// </summary>
        /// <param name="name">The Name.</param>
        /// <param name="names">The Names.</param>
        /// <returns>A resultant <see cref="Person"/>.</returns>
        public Task<Person?> GetNullAsync(string? name, List<string>? names) => DataInvoker.Current.InvokeAsync(this, () => GetNullOnImplementationAsync(name, names), new BusinessInvokerArgs { ExceptionHandler = _getNullOnException });

        /// <summary>
        /// Gets the <see cref="PersonCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Entities.PersonArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="PersonCollectionResult"/>.</returns>
        public Task<PersonCollectionResult> GetByArgsWithEfAsync(PersonArgs? args, PagingArgs? paging) => DataInvoker.Current.InvokeAsync(this, async () =>
        {
            PersonCollectionResult __result = new PersonCollectionResult(paging);
            var __dataArgs = EfDbArgs.Create(_mapper, __result.Paging!);
            await (_getByArgsWithEfOnBeforeAsync?.Invoke(args, __dataArgs) ?? Task.CompletedTask).ConfigureAwait(false);
            __result.Result = _ef.Query<Person, EfModel.Person>(__dataArgs, q => _getByArgsWithEfOnQuery?.Invoke(q, args, __dataArgs) ?? q).SelectQuery<PersonCollection>();
            await (_getByArgsWithEfOnAfterAsync?.Invoke(__result, args) ?? Task.CompletedTask).ConfigureAwait(false);
            return __result;
        }, new BusinessInvokerArgs { ExceptionHandler = _getByArgsWithEfOnException });

        /// <summary>
        /// Throw Error.
        /// </summary>
        public Task ThrowErrorAsync() => DataInvoker.Current.InvokeAsync(this, () => ThrowErrorOnImplementationAsync(), new BusinessInvokerArgs { ExceptionHandler = _throwErrorOnException });

        /// <summary>
        /// Invoke Api Via Agent.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <returns>A resultant <see cref="string"/>.</returns>
        public Task<string?> InvokeApiViaAgentAsync(Guid id) => DataInvoker.Current.InvokeAsync(this, () => InvokeApiViaAgentOnImplementationAsync(id), new BusinessInvokerArgs { ExceptionHandler = _invokeApiViaAgentOnException });

        /// <summary>
        /// Gets the specified <see cref="Person"/>.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <returns>The selected <see cref="Person"/> where found.</returns>
        public Task<Person?> GetWithEfAsync(Guid id) => DataInvoker.Current.InvokeAsync(this, async () =>
        {
            Person? __result;
            var __dataArgs = EfDbArgs.Create(_mapper);
            await (_getWithEfOnBeforeAsync?.Invoke(id, __dataArgs) ?? Task.CompletedTask).ConfigureAwait(false);
            __result = await _ef.GetAsync<Person, EfModel.Person>(__dataArgs, id).ConfigureAwait(false);
            await (_getWithEfOnAfterAsync?.Invoke(__result, id) ?? Task.CompletedTask).ConfigureAwait(false);
            return __result;
        }, new BusinessInvokerArgs { ExceptionHandler = _getWithEfOnException });

        /// <summary>
        /// Creates a new <see cref="Person"/>.
        /// </summary>
        /// <param name="value">The <see cref="Person"/>.</param>
        /// <returns>The created <see cref="Person"/>.</returns>
        public Task<Person> CreateWithEfAsync(Person value) => DataInvoker.Current.InvokeAsync(this, async () =>
        {
            Person __result;
            var __dataArgs = EfDbArgs.Create(_mapper);
            await (_createWithEfOnBeforeAsync?.Invoke(value, __dataArgs) ?? Task.CompletedTask).ConfigureAwait(false);
            __result = await _ef.CreateAsync<Person, EfModel.Person>(__dataArgs, Check.NotNull(value, nameof(value))).ConfigureAwait(false);
            await (_createWithEfOnAfterAsync?.Invoke(__result) ?? Task.CompletedTask).ConfigureAwait(false);
            return __result;
        }, new BusinessInvokerArgs { ExceptionHandler = _createWithEfOnException });

        /// <summary>
        /// Updates an existing <see cref="Person"/>.
        /// </summary>
        /// <param name="value">The <see cref="Person"/>.</param>
        /// <returns>The updated <see cref="Person"/>.</returns>
        public Task<Person> UpdateWithEfAsync(Person value) => DataInvoker.Current.InvokeAsync(this, async () =>
        {
            Person __result;
            var __dataArgs = EfDbArgs.Create(_mapper);
            await (_updateWithEfOnBeforeAsync?.Invoke(value, __dataArgs) ?? Task.CompletedTask).ConfigureAwait(false);
            __result = await _ef.UpdateAsync<Person, EfModel.Person>(__dataArgs, Check.NotNull(value, nameof(value))).ConfigureAwait(false);
            await (_updateWithEfOnAfterAsync?.Invoke(__result) ?? Task.CompletedTask).ConfigureAwait(false);
            return __result;
        }, new BusinessInvokerArgs { ExceptionHandler = _updateWithEfOnException });

        /// <summary>
        /// Deletes the specified <see cref="Person"/>.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        public Task DeleteWithEfAsync(Guid id) => DataInvoker.Current.InvokeAsync(this, async () =>
        {
            var __dataArgs = EfDbArgs.Create(_mapper);
            await (_deleteWithEfOnBeforeAsync?.Invoke(id, __dataArgs) ?? Task.CompletedTask).ConfigureAwait(false);
            await _ef.DeleteAsync<Person, EfModel.Person>(__dataArgs, id).ConfigureAwait(false);
            await (_deleteWithEfOnAfterAsync?.Invoke(id) ?? Task.CompletedTask).ConfigureAwait(false);
        }, new BusinessInvokerArgs { ExceptionHandler = _deleteWithEfOnException });

        /// <summary>
        /// Provides the <see cref="Person"/> property and database column mapping.
        /// </summary>
        public partial class DbMapper : DatabaseMapper<Person, DbMapper>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DbMapper"/> class.
            /// </summary>
            public DbMapper()
            {
                Property(s => s.Id, "PersonId").SetUniqueKey(false);
                Property(s => s.FirstName);
                Property(s => s.LastName);
                Property(s => s.UniqueCode);
                Property(s => s.Gender, "GenderId").SetConverter(ReferenceDataNullableGuidIdConverter<RefDataNamespace.Gender>.Default!);
                Property(s => s.EyeColorSid, "EyeColorCode");
                Property(s => s.Birthday);
                Property(s => s.Address).SetMapper(AddressData.DbMapper.Default!);
                Property(s => s.Metadata, "MetadataJson").SetConverter(ObjectToJsonConverter<Dictionary<string,string>>.Default!);
                AddStandardProperties();
                DbMapperCtor();
            }
            
            partial void DbMapperCtor(); // Enables the DbMapper constructor to be extended.
        }

        /// <summary>
        /// Provides the <see cref="Person"/> and Entity Framework <see cref="EfModel.Person"/> <i>AutoMapper</i> mapping.
        /// </summary>
        public partial class EfMapperProfile : AutoMapper.Profile
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="EfMapperProfile"/> class.
            /// </summary>
            public EfMapperProfile()
            {
                var s2d = CreateMap<Person, EfModel.Person>();
                s2d.ForMember(d => d.PersonId, o => o.MapFrom(s => s.Id));
                s2d.ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName));
                s2d.ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName));
                s2d.ForMember(d => d.UniqueCode, o => o.MapFrom(s => s.UniqueCode));
                s2d.ForMember(d => d.GenderId, o => o.ConvertUsing(ReferenceDataNullableGuidIdConverter<RefDataNamespace.Gender>.Default.ToDest, s => s.Gender));
                s2d.ForMember(d => d.EyeColorCode, o => o.MapFrom(s => s.EyeColorSid));
                s2d.ForMember(d => d.Birthday, o => o.MapFrom(s => s.Birthday));
                s2d.ForMember(d => d.MetadataJson, o => o.ConvertUsing(ObjectToJsonConverter<Dictionary<string,string>>.Default.ToDest, s => s.Metadata));
                s2d.ForMember(d => d.RowVersion, o => o.ConvertUsing(StringToBase64Converter.Default.ToDest, s => s.ETag));
                s2d.ForMember(d => d.CreatedBy, o => o.OperationTypes(OperationTypes.AnyExceptUpdate).MapFrom(s => s.ChangeLog.CreatedBy));
                s2d.ForMember(d => d.CreatedDate, o => o.OperationTypes(OperationTypes.AnyExceptUpdate).MapFrom(s => s.ChangeLog.CreatedDate));
                s2d.ForMember(d => d.UpdatedBy, o => o.OperationTypes(OperationTypes.AnyExceptCreate).MapFrom(s => s.ChangeLog.UpdatedBy));
                s2d.ForMember(d => d.UpdatedDate, o => o.OperationTypes(OperationTypes.AnyExceptCreate).MapFrom(s => s.ChangeLog.UpdatedDate));

                var d2s = CreateMap<EfModel.Person, Person>();
                d2s.ForMember(s => s.Id, o => o.MapFrom(d => d.PersonId));
                d2s.ForMember(s => s.FirstName, o => o.MapFrom(d => d.FirstName));
                d2s.ForMember(s => s.LastName, o => o.MapFrom(d => d.LastName));
                d2s.ForMember(s => s.UniqueCode, o => o.MapFrom(d => d.UniqueCode));
                d2s.ForMember(s => s.Gender, o => o.ConvertUsing(ReferenceDataNullableGuidIdConverter<RefDataNamespace.Gender>.Default.ToSrce, d => d.GenderId));
                d2s.ForMember(s => s.EyeColorSid, o => o.MapFrom(d => d.EyeColorCode));
                d2s.ForMember(s => s.Birthday, o => o.MapFrom(d => d.Birthday));
                d2s.ForMember(s => s.Address, o => o.Ignore());
                d2s.ForMember(s => s.Metadata, o => o.ConvertUsing(ObjectToJsonConverter<Dictionary<string,string>>.Default.ToSrce, d => d.MetadataJson));
                d2s.ForMember(s => s.ETag, o => o.ConvertUsing(StringToBase64Converter.Default.ToSrce, d => d.RowVersion));
                d2s.ForPath(s => s.ChangeLog.CreatedBy, o => o.OperationTypes(OperationTypes.AnyExceptUpdate).MapFrom(d => d.CreatedBy));
                d2s.ForPath(s => s.ChangeLog.CreatedDate, o => o.OperationTypes(OperationTypes.AnyExceptUpdate).MapFrom(d => d.CreatedDate));
                d2s.ForPath(s => s.ChangeLog.UpdatedBy, o => o.OperationTypes(OperationTypes.AnyExceptCreate).MapFrom(d => d.UpdatedBy));
                d2s.ForPath(s => s.ChangeLog.UpdatedDate, o => o.OperationTypes(OperationTypes.AnyExceptCreate).MapFrom(d => d.UpdatedDate));

                EfMapperProfileCtor(s2d, d2s);
            }

            partial void EfMapperProfileCtor(AutoMapper.IMappingExpression<Person, EfModel.Person> s2d, AutoMapper.IMappingExpression<EfModel.Person, Person> d2s); // Enables the constructor to be extended.
        }
    }
}

#pragma warning restore
#nullable restore