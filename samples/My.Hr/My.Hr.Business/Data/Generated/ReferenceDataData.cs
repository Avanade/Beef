/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beef;
using Beef.Business;
using Beef.Data.EntityFrameworkCore;
using Beef.Mapper;
using Beef.Mapper.Converters;
using RefDataNamespace = My.Hr.Common.Entities;

namespace My.Hr.Business.Data
{
    /// <summary>
    /// Provides the <b>ReferenceData</b> data access.
    /// </summary>
    public partial class ReferenceDataData : IReferenceDataData
    {
        private readonly IEfDb _ef;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDataData"/> class.
        /// </summary>
        /// <param name="ef">The <see cref="IEfDb"/>.</param>
        public ReferenceDataData(IEfDb ef)
            { _ef = Check.NotNull(ef, nameof(ef)); DataCtor(); }

        partial void DataCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets all the <see cref="RefDataNamespace.Gender"/> items.
        /// </summary>
        /// <returns>The <see cref="RefDataNamespace.GenderCollection"/>.</returns>
        public async Task<RefDataNamespace.GenderCollection> GenderGetAllAsync()
        {
            var __coll = new RefDataNamespace.GenderCollection();
            await DataInvoker.Current.InvokeAsync(this, async () => { _ef.Query(GenderMapper.CreateArgs()).SelectQuery(__coll); await Task.CompletedTask.ConfigureAwait(false); }, BusinessInvokerArgs.TransactionSuppress).ConfigureAwait(false);
            return __coll;
        }

        /// <summary>
        /// Gets all the <see cref="RefDataNamespace.TerminationReason"/> items.
        /// </summary>
        /// <returns>The <see cref="RefDataNamespace.TerminationReasonCollection"/>.</returns>
        public async Task<RefDataNamespace.TerminationReasonCollection> TerminationReasonGetAllAsync()
        {
            var __coll = new RefDataNamespace.TerminationReasonCollection();
            await DataInvoker.Current.InvokeAsync(this, async () => { _ef.Query(TerminationReasonMapper.CreateArgs()).SelectQuery(__coll); await Task.CompletedTask.ConfigureAwait(false); }, BusinessInvokerArgs.TransactionSuppress).ConfigureAwait(false);
            return __coll;
        }

        /// <summary>
        /// Gets all the <see cref="RefDataNamespace.RelationshipType"/> items.
        /// </summary>
        /// <returns>The <see cref="RefDataNamespace.RelationshipTypeCollection"/>.</returns>
        public async Task<RefDataNamespace.RelationshipTypeCollection> RelationshipTypeGetAllAsync()
        {
            var __coll = new RefDataNamespace.RelationshipTypeCollection();
            await DataInvoker.Current.InvokeAsync(this, async () => { _ef.Query(RelationshipTypeMapper.CreateArgs()).SelectQuery(__coll); await Task.CompletedTask.ConfigureAwait(false); }, BusinessInvokerArgs.TransactionSuppress).ConfigureAwait(false);
            return __coll;
        }

        /// <summary>
        /// Gets all the <see cref="RefDataNamespace.USState"/> items.
        /// </summary>
        /// <returns>The <see cref="RefDataNamespace.USStateCollection"/>.</returns>
        public async Task<RefDataNamespace.USStateCollection> USStateGetAllAsync()
        {
            var __coll = new RefDataNamespace.USStateCollection();
            await DataInvoker.Current.InvokeAsync(this, async () => { _ef.Query(USStateMapper.CreateArgs()).SelectQuery(__coll); await Task.CompletedTask.ConfigureAwait(false); }, BusinessInvokerArgs.TransactionSuppress).ConfigureAwait(false);
            return __coll;
        }

        /// <summary>
        /// Gets all the <see cref="RefDataNamespace.PerformanceOutcome"/> items.
        /// </summary>
        /// <returns>The <see cref="RefDataNamespace.PerformanceOutcomeCollection"/>.</returns>
        public async Task<RefDataNamespace.PerformanceOutcomeCollection> PerformanceOutcomeGetAllAsync()
        {
            var __coll = new RefDataNamespace.PerformanceOutcomeCollection();
            await DataInvoker.Current.InvokeAsync(this, async () => { _ef.Query(PerformanceOutcomeMapper.CreateArgs()).SelectQuery(__coll); await Task.CompletedTask.ConfigureAwait(false); }, BusinessInvokerArgs.TransactionSuppress).ConfigureAwait(false);
            return __coll;
        }

        /// <summary>
        /// Provides the <see cref="RefDataNamespace.Gender"/> and Entity Framework <see cref="EfModel.Gender"/> property mapping.
        /// </summary>
        public static EfDbMapper<RefDataNamespace.Gender, EfModel.Gender> GenderMapper => EfDbMapper.CreateAuto<RefDataNamespace.Gender, EfModel.Gender>()
            .HasProperty(s => s.Id, d => d.GenderId)
            .AddStandardProperties();

        /// <summary>
        /// Provides the <see cref="RefDataNamespace.TerminationReason"/> and Entity Framework <see cref="EfModel.TerminationReason"/> property mapping.
        /// </summary>
        public static EfDbMapper<RefDataNamespace.TerminationReason, EfModel.TerminationReason> TerminationReasonMapper => EfDbMapper.CreateAuto<RefDataNamespace.TerminationReason, EfModel.TerminationReason>()
            .HasProperty(s => s.Id, d => d.TerminationReasonId)
            .AddStandardProperties();

        /// <summary>
        /// Provides the <see cref="RefDataNamespace.RelationshipType"/> and Entity Framework <see cref="EfModel.RelationshipType"/> property mapping.
        /// </summary>
        public static EfDbMapper<RefDataNamespace.RelationshipType, EfModel.RelationshipType> RelationshipTypeMapper => EfDbMapper.CreateAuto<RefDataNamespace.RelationshipType, EfModel.RelationshipType>()
            .HasProperty(s => s.Id, d => d.RelationshipTypeId)
            .AddStandardProperties();

        /// <summary>
        /// Provides the <see cref="RefDataNamespace.USState"/> and Entity Framework <see cref="EfModel.USState"/> property mapping.
        /// </summary>
        public static EfDbMapper<RefDataNamespace.USState, EfModel.USState> USStateMapper => EfDbMapper.CreateAuto<RefDataNamespace.USState, EfModel.USState>()
            .HasProperty(s => s.Id, d => d.USStateId)
            .AddStandardProperties();

        /// <summary>
        /// Provides the <see cref="RefDataNamespace.PerformanceOutcome"/> and Entity Framework <see cref="EfModel.PerformanceOutcome"/> property mapping.
        /// </summary>
        public static EfDbMapper<RefDataNamespace.PerformanceOutcome, EfModel.PerformanceOutcome> PerformanceOutcomeMapper => EfDbMapper.CreateAuto<RefDataNamespace.PerformanceOutcome, EfModel.PerformanceOutcome>()
            .HasProperty(s => s.Id, d => d.PerformanceOutcomeId)
            .AddStandardProperties();
    }
}

#pragma warning restore
#nullable restore