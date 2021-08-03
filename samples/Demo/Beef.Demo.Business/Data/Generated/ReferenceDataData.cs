/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Beef;
using Beef.Business;
using Beef.Data.Cosmos;
using Beef.Data.Database;
using Beef.Data.EntityFrameworkCore;
using Beef.Mapper;
using Beef.Mapper.Converters;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Business.Data
{
    /// <summary>
    /// Provides the <b>ReferenceData</b> data access.
    /// </summary>
    public partial class ReferenceDataData : IReferenceDataData
    {
        private readonly IDatabase _db;
        private readonly IEfDb _ef;
        private readonly ICosmosDb _cosmos;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataData"/> class.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="ef">The <see cref="IEfDb"/>.</param>
        /// <param name="cosmos">The <see cref="ICosmosDb"/>.</param>
        public ReferenceDataData(IDatabase db, IEfDb ef, ICosmosDb cosmos)
            { _db = Check.NotNull(db, nameof(db)); _ef = Check.NotNull(ef, nameof(ef)); _cosmos = Check.NotNull(cosmos, nameof(cosmos)); DataCtor(); }

        partial void DataCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets all the <see cref="RefDataNamespace.Country"/> items.
        /// </summary>
        /// <returns>The <see cref="RefDataNamespace.CountryCollection"/>.</returns>
        public async Task<RefDataNamespace.CountryCollection> CountryGetAllAsync()
        {
            var __coll = new RefDataNamespace.CountryCollection();
            await DataInvoker.Current.InvokeAsync(this, async () => 
            {
                await _db.GetRefDataAsync<RefDataNamespace.CountryCollection, RefDataNamespace.Country>(__coll, "[Ref].[spCountryGetAll]", "CountryId");
            }, BusinessInvokerArgs.TransactionSuppress).ConfigureAwait(false);

            return __coll;
        }

        /// <summary>
        /// Gets all the <see cref="RefDataNamespace.USState"/> items.
        /// </summary>
        /// <returns>The <see cref="RefDataNamespace.USStateCollection"/>.</returns>
        public async Task<RefDataNamespace.USStateCollection> USStateGetAllAsync()
        {
            var __coll = new RefDataNamespace.USStateCollection();
            await DataInvoker.Current.InvokeAsync(this, async () => 
            {
                await _db.GetRefDataAsync<RefDataNamespace.USStateCollection, RefDataNamespace.USState>(__coll, "[Ref].[spUSStateGetAll]", "USStateId");
            }, BusinessInvokerArgs.TransactionSuppress).ConfigureAwait(false);

            return __coll;
        }

        /// <summary>
        /// Gets all the <see cref="RefDataNamespace.Gender"/> items.
        /// </summary>
        /// <returns>The <see cref="RefDataNamespace.GenderCollection"/>.</returns>
        public async Task<RefDataNamespace.GenderCollection> GenderGetAllAsync()
        {
            var __coll = new RefDataNamespace.GenderCollection();
            await DataInvoker.Current.InvokeAsync(this, async () => 
            {
                await _db.GetRefDataAsync<RefDataNamespace.GenderCollection, RefDataNamespace.Gender>(__coll, "[Ref].[spGenderGetAll]", "GenderId", additionalProperties: (dr, item, fields) =>
                {
                    item.AlternateName = dr.GetValue<string>("AlternateName");
                    item.TripCode = dr.GetValue<string>("TripCode");
                    item.Country = ReferenceDataNullableGuidIdConverter<RefDataNamespace.Country>.Default.ConvertToSrce(dr.GetValue<Guid?>("CountryId"));
                });
            }, BusinessInvokerArgs.TransactionSuppress).ConfigureAwait(false);

            return __coll;
        }

        /// <summary>
        /// Gets all the <see cref="RefDataNamespace.EyeColor"/> items.
        /// </summary>
        /// <returns>The <see cref="RefDataNamespace.EyeColorCollection"/>.</returns>
        public async Task<RefDataNamespace.EyeColorCollection> EyeColorGetAllAsync()
        {
            var __coll = new RefDataNamespace.EyeColorCollection();
            await DataInvoker.Current.InvokeAsync(this, async () => { _ef.Query<RefDataNamespace.EyeColor, EfModel.EyeColor>(EyeColorMapper.CreateArgs()).SelectQuery(__coll); await Task.CompletedTask.ConfigureAwait(false); }, BusinessInvokerArgs.TransactionSuppress).ConfigureAwait(false);
            return __coll;
        }

        /// <summary>
        /// Gets all the <see cref="RefDataNamespace.PowerSource"/> items.
        /// </summary>
        /// <returns>The <see cref="RefDataNamespace.PowerSourceCollection"/>.</returns>
        public async Task<RefDataNamespace.PowerSourceCollection> PowerSourceGetAllAsync()
        {
            var __coll = new RefDataNamespace.PowerSourceCollection();
            await DataInvoker.Current.InvokeAsync(this, async () => { _cosmos.ValueQuery<RefDataNamespace.PowerSource, Model.PowerSource>(PowerSourceMapper.CreateArgs("RefData")).SelectQuery(__coll); await Task.CompletedTask.ConfigureAwait(false); }).ConfigureAwait(false);
            return __coll;
        }

        /// <summary>
        /// Gets all the <see cref="RefDataNamespace.Company"/> items.
        /// </summary>
        /// <returns>The <see cref="RefDataNamespace.CompanyCollection"/>.</returns>
        public async Task<RefDataNamespace.CompanyCollection> CompanyGetAllAsync()
        {
            var __coll = new RefDataNamespace.CompanyCollection();
            await DataInvoker.Current.InvokeAsync(this, async () => await CompanyGetAll_OnImplementation(__coll).ConfigureAwait(false)).ConfigureAwait(false);
            return __coll;
        }

        /// <summary>
        /// Gets all the <see cref="RefDataNamespace.Status"/> items.
        /// </summary>
        /// <returns>The <see cref="RefDataNamespace.StatusCollection"/>.</returns>
        public async Task<RefDataNamespace.StatusCollection> StatusGetAllAsync()
        {
            var __coll = new RefDataNamespace.StatusCollection();
            await DataInvoker.Current.InvokeAsync(this, async () => { _ef.Query<RefDataNamespace.Status, EfModel.Status>(StatusMapper.CreateArgs()).SelectQuery(__coll); await Task.CompletedTask.ConfigureAwait(false); }, BusinessInvokerArgs.TransactionSuppress).ConfigureAwait(false);
            return __coll;
        }

        /// <summary>
        /// Provides the <see cref="RefDataNamespace.EyeColor"/> and Entity Framework <see cref="EfModel.EyeColor"/> property mapping.
        /// </summary>
        public static EfDbMapper<RefDataNamespace.EyeColor, EfModel.EyeColor> EyeColorMapper => EfDbMapper.CreateAuto<RefDataNamespace.EyeColor, EfModel.EyeColor>()
            .HasProperty(s => s.Id, d => d.EyeColorId)
            .AddStandardProperties();

        /// <summary>
        /// Provides the <see cref="RefDataNamespace.PowerSource"/> and Cosmos <see cref="Model.PowerSource"/> property mapping.
        /// </summary>
        public static CosmosDbMapper<RefDataNamespace.PowerSource, Model.PowerSource> PowerSourceMapper => CosmosDbMapper.CreateAuto<RefDataNamespace.PowerSource, Model.PowerSource>()
            .HasProperty(s => s.AdditionalInfo, d => d.AdditionalInfo, p => p.SetOperationTypes(OperationTypes.Any))
            .AddStandardProperties();

        /// <summary>
        /// Provides the <see cref="RefDataNamespace.Status"/> and Entity Framework <see cref="EfModel.Status"/> property mapping.
        /// </summary>
        public static EfDbMapper<RefDataNamespace.Status, EfModel.Status> StatusMapper => EfDbMapper.CreateAuto<RefDataNamespace.Status, EfModel.Status>()
            .HasProperty(s => s.Id, d => d.StatusId)
            .AddStandardProperties();
    }
}

#pragma warning restore
#nullable restore