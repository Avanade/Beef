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
using Beef.Events;
using Beef.Mapper;
using Beef.Mapper.Converters;
using My.Hr.Business.Entities;
using RefDataNamespace = My.Hr.Business.Entities;

namespace My.Hr.Business.Data
{
    /// <summary>
    /// Provides the <see cref="Employee"/> data access.
    /// </summary>
    public partial class EmployeeData : IEmployeeData
    {
        private readonly IDatabase _db;
        private readonly IEfDb _ef;
        private readonly IEventPublisher _evtPub;

        private Func<IQueryable<EfModel.Employee>, EmployeeArgs?, IEfDbArgs, IQueryable<EfModel.Employee>>? _getByArgsOnQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeData"/> class.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="ef">The <see cref="IEfDb"/>.</param>
        /// <param name="evtPub">The <see cref="IEventPublisher"/>.</param>
        public EmployeeData(IDatabase db, IEfDb ef, IEventPublisher evtPub)
            { _db = Check.NotNull(db, nameof(db)); _ef = Check.NotNull(ef, nameof(ef)); _evtPub = Check.NotNull(evtPub, nameof(evtPub)); EmployeeDataCtor(); }

        partial void EmployeeDataCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the specified <see cref="Employee"/>.
        /// </summary>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <returns>The selected <see cref="Employee"/> where found.</returns>
        public Task<Employee?> GetAsync(Guid id)
            => DataInvoker.Current.InvokeAsync(this, () => GetOnImplementationAsync(id));

        /// <summary>
        /// Creates a new <see cref="Employee"/>.
        /// </summary>
        /// <param name="value">The <see cref="Employee"/>.</param>
        /// <returns>The created <see cref="Employee"/>.</returns>
        public Task<Employee> CreateAsync(Employee value)
        {
            return _db.EventOutboxInvoker.InvokeAsync(this, async () =>
            {
                var __result = await CreateOnImplementationAsync(Check.NotNull(value, nameof(value))).ConfigureAwait(false);
                _evtPub.PublishValue(__result, new Uri($"my/hr/employee/{_evtPub.FormatKey(__result)}", UriKind.Relative), $"My.Hr.Employee", "Created");
                return __result;
            });
        }

        /// <summary>
        /// Updates an existing <see cref="Employee"/>.
        /// </summary>
        /// <param name="value">The <see cref="Employee"/>.</param>
        /// <returns>The updated <see cref="Employee"/>.</returns>
        public Task<Employee> UpdateAsync(Employee value)
        {
            return _db.EventOutboxInvoker.InvokeAsync(this, async () =>
            {
                var __result = await UpdateOnImplementationAsync(Check.NotNull(value, nameof(value))).ConfigureAwait(false);
                _evtPub.PublishValue(__result, new Uri($"my/hr/employee/{_evtPub.FormatKey(__result)}", UriKind.Relative), $"My.Hr.Employee", "Updated");
                return __result;
            });
        }

        /// <summary>
        /// Deletes the specified <see cref="Employee"/>.
        /// </summary>
        /// <param name="id">The Id.</param>
        public Task DeleteAsync(Guid id)
        {
            return _db.EventOutboxInvoker.InvokeAsync(this, async () =>
            {
                var __dataArgs = DbMapper.Default.CreateArgs("[Hr].[spEmployeeDelete]");
                await _db.DeleteAsync(__dataArgs, id).ConfigureAwait(false);
                _evtPub.Publish(new Uri($"my/hr/employee/{_evtPub.FormatKey(id)}", UriKind.Relative), $"My.Hr.Employee", "Deleted", id);
            });
        }

        /// <summary>
        /// Gets the <see cref="EmployeeBaseCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Entities.EmployeeArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="EmployeeBaseCollectionResult"/>.</returns>
        public Task<EmployeeBaseCollectionResult> GetByArgsAsync(EmployeeArgs? args, PagingArgs? paging)
        {
            return DataInvoker.Current.InvokeAsync(this, async () =>
            {
                EmployeeBaseCollectionResult __result = new EmployeeBaseCollectionResult(paging);
                var __dataArgs = EmployeeBaseData.EfMapper.Default.CreateArgs(__result.Paging!);
                __result.Result = _ef.Query(__dataArgs, q => _getByArgsOnQuery?.Invoke(q, args, __dataArgs) ?? q).SelectQuery<EmployeeBaseCollection>();
                return await Task.FromResult(__result).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Terminates an existing <see cref="Employee"/>.
        /// </summary>
        /// <param name="value">The <see cref="TerminationDetail"/>.</param>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <returns>The updated <see cref="Employee"/>.</returns>
        public Task<Employee> TerminateAsync(TerminationDetail value, Guid id)
        {
            return _db.EventOutboxInvoker.InvokeAsync(this, async () =>
            {
                var __result = await TerminateOnImplementationAsync(Check.NotNull(value, nameof(value)), id).ConfigureAwait(false);
                _evtPub.PublishValue(__result, new Uri($"my/hr/employee/{_evtPub.FormatKey(__result)}", UriKind.Relative), $"My.Hr.Employee", "Terminated", id);
                return __result;
            });
        }

        /// <summary>
        /// Provides the <see cref="Employee"/> property and database column mapping.
        /// </summary>
        public partial class DbMapper : DatabaseMapper<Employee, DbMapper>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DbMapper"/> class.
            /// </summary>
            public DbMapper()
            {
                InheritPropertiesFrom(EmployeeBaseData.DbMapper.Default);
                Property(s => s.Address, "AddressJson").SetConverter(ObjectToJsonConverter<Address>.Default!);
                AddStandardProperties();
                DbMapperCtor();
            }
            
            partial void DbMapperCtor(); // Enables the DbMapper constructor to be extended.
        }
    }
}

#pragma warning restore
#nullable restore