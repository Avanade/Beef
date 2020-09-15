/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0005, IDE0044 // Using directive is unnecessary; are required depending on code-gen options

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beef;
using Beef.Business;
using Beef.Caching;
using Beef.Entities;
using Beef.Events;
using My.Hr.Business.Data;
using My.Hr.Common.Entities;
using RefDataNamespace = My.Hr.Common.Entities;

namespace My.Hr.Business.DataSvc
{
    /// <summary>
    /// Provides the <see cref="Employee"/> data repository services.
    /// </summary>
    public partial class EmployeeDataSvc : IEmployeeDataSvc
    {
        private readonly IEmployeeData _data;
        private readonly IEventPublisher _evtPub;
        private readonly IRequestCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeDataSvc"/> class.
        /// </summary>
        /// <param name="data">The <see cref="IEmployeeData"/>.</param>
        /// <param name="evtPub">The <see cref="IEventPublisher"/>.</param>
        /// <param name="cache">The <see cref="IRequestCache"/>.</param>
        public EmployeeDataSvc(IEmployeeData data, IEventPublisher evtPub, IRequestCache cache)
            { _data = Check.NotNull(data, nameof(data)); _evtPub = Check.NotNull(evtPub, nameof(evtPub)); _cache = Check.NotNull(cache, nameof(cache)); EmployeeDataSvcCtor(); }

        partial void EmployeeDataSvcCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the specified <see cref="Employee"/>.
        /// </summary>
        /// <param name="id">The Id.</param>
        /// <returns>The selected <see cref="Employee"/> where found.</returns>
        public Task<Employee?> GetAsync(Guid id)
        {
            return DataSvcInvoker.Current.InvokeAsync(this, async () =>
            {
                var __key = new UniqueKey(id);
                if (_cache.TryGetValue(__key, out Employee? __val))
                    return __val;

                var __result = await _data.GetAsync(id).ConfigureAwait(false);
                _cache.SetValue(__key, __result);
                return __result;
            });
        }

        /// <summary>
        /// Creates a new <see cref="Employee"/>.
        /// </summary>
        /// <param name="value">The <see cref="Employee"/>.</param>
        /// <returns>The created <see cref="Employee"/>.</returns>
        public Task<Employee> CreateAsync(Employee value)
        {
            return DataSvcInvoker.Current.InvokeAsync(this, async () =>
            {
                var __result = await _data.CreateAsync(Check.NotNull(value, nameof(value))).ConfigureAwait(false);
                await _evtPub.PublishValueAsync(__result, $"My.Hr.Employee.{__result.Id}", "Created").ConfigureAwait(false);
                _cache.SetValue(__result.UniqueKey, __result);
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
            return DataSvcInvoker.Current.InvokeAsync(this, async () =>
            {
                var __result = await _data.UpdateAsync(Check.NotNull(value, nameof(value))).ConfigureAwait(false);
                await _evtPub.PublishValueAsync(__result, $"My.Hr.Employee.{__result.Id}", "Updated").ConfigureAwait(false);
                _cache.SetValue(__result.UniqueKey, __result);
                return __result;
            });
        }

        /// <summary>
        /// Deletes the specified <see cref="Employee"/>.
        /// </summary>
        /// <param name="id">The Id.</param>
        public Task DeleteAsync(Guid id)
        {
            return DataSvcInvoker.Current.InvokeAsync(this, async () =>
            {
                await _data.DeleteAsync(id).ConfigureAwait(false);
                await _evtPub.PublishAsync($"My.Hr.Employee.{id}", "Deleted", id).ConfigureAwait(false);
                _cache.Remove<Employee>(new UniqueKey(id));
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
            return DataSvcInvoker.Current.InvokeAsync(this, async () =>
            {
                var __result = await _data.GetByArgsAsync(args, paging).ConfigureAwait(false);
                return __result;
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
            return DataSvcInvoker.Current.InvokeAsync(this, async () =>
            {
                var __result = await _data.TerminateAsync(Check.NotNull(value, nameof(value)), id).ConfigureAwait(false);
                await _evtPub.PublishValueAsync(__result, $"My.Hr.Employee.{id}", "Terminated", id).ConfigureAwait(false);
                _cache.SetValue(__result.UniqueKey, __result);
                return __result;
            });
        }
    }
}

#pragma warning restore IDE0005, IDE0044
#nullable restore