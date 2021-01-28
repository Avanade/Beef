/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0079, IDE0001, IDE0005, IDE0044, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649

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
using My.Hr.Common.Entities;
using RefDataNamespace = My.Hr.Common.Entities;

namespace My.Hr.Business.Data
{
    /// <summary>
    /// Provides the <see cref="Employee"/> data access.
    /// </summary>
    public partial class EmployeeData : IEmployeeData
    {
        private readonly IDatabase _db;
        private readonly IEfDb _ef;

        private Func<IQueryable<EfModel.Employee>, EmployeeArgs?, IEfDbArgs, IQueryable<EfModel.Employee>>? _getByArgsOnQuery;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeData"/> class.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="ef">The <see cref="IEfDb"/>.</param>
        public EmployeeData(IDatabase db, IEfDb ef)
            { _db = Check.NotNull(db, nameof(db)); _ef = Check.NotNull(ef, nameof(ef)); EmployeeDataCtor(); }

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
            => DataInvoker.Current.InvokeAsync(this, () => CreateOnImplementationAsync(Check.NotNull(value, nameof(value))));

        /// <summary>
        /// Updates an existing <see cref="Employee"/>.
        /// </summary>
        /// <param name="value">The <see cref="Employee"/>.</param>
        /// <returns>The updated <see cref="Employee"/>.</returns>
        public Task<Employee> UpdateAsync(Employee value)
            => DataInvoker.Current.InvokeAsync(this, () => UpdateOnImplementationAsync(Check.NotNull(value, nameof(value))));

        /// <summary>
        /// Deletes the specified <see cref="Employee"/>.
        /// </summary>
        /// <param name="id">The Id.</param>
        public Task DeleteAsync(Guid id)
        {
            return DataInvoker.Current.InvokeAsync(this, async () =>
            {
                var __dataArgs = DbMapper.Default.CreateArgs("[Hr].[spEmployeeDelete]");
                await _db.DeleteAsync(__dataArgs, id).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Gets the <see cref="EmployeeBaseCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Common.Entities.EmployeeArgs"/>).</param>
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
            => DataInvoker.Current.InvokeAsync(this, () => TerminateOnImplementationAsync(Check.NotNull(value, nameof(value)), id));

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

#pragma warning restore IDE0079, IDE0001, IDE0005, IDE0044, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649
#nullable restore