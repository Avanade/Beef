/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0079, IDE0001, IDE0005, IDE0044, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beef;
using Beef.Business;
using Beef.Caching;
using Beef.Entities;
using Beef.Events;
using Beef.Demo.Business.Data;
using Beef.Demo.Common.Entities;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Business.DataSvc
{
    /// <summary>
    /// Provides the <see cref="Contact"/> data repository services.
    /// </summary>
    public partial class ContactDataSvc : IContactDataSvc
    {
        private readonly IContactData _data;
        private readonly IEventPublisher _evtPub;
        private readonly IRequestCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactDataSvc"/> class.
        /// </summary>
        /// <param name="data">The <see cref="IContactData"/>.</param>
        /// <param name="evtPub">The <see cref="IEventPublisher"/>.</param>
        /// <param name="cache">The <see cref="IRequestCache"/>.</param>
        public ContactDataSvc(IContactData data, IEventPublisher evtPub, IRequestCache cache)
            { _data = Check.NotNull(data, nameof(data)); _evtPub = Check.NotNull(evtPub, nameof(evtPub)); _cache = Check.NotNull(cache, nameof(cache)); ContactDataSvcCtor(); }

        partial void ContactDataSvcCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the <see cref="ContactCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <returns>The <see cref="ContactCollectionResult"/>.</returns>
        public Task<ContactCollectionResult> GetAllAsync()
        {
            return DataSvcInvoker.Current.InvokeAsync(this, async () =>
            {
                var __result = await _data.GetAllAsync().ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Gets the specified <see cref="Contact"/>.
        /// </summary>
        /// <param name="id">The <see cref="Contact"/> identifier.</param>
        /// <returns>The selected <see cref="Contact"/> where found.</returns>
        public Task<Contact?> GetAsync(Guid id)
        {
            return DataSvcInvoker.Current.InvokeAsync(this, async () =>
            {
                var __key = new UniqueKey(id);
                if (_cache.TryGetValue(__key, out Contact? __val))
                    return __val;

                var __result = await _data.GetAsync(id).ConfigureAwait(false);
                _cache.SetValue(__key, __result);
                return __result;
            });
        }

        /// <summary>
        /// Creates a new <see cref="Contact"/>.
        /// </summary>
        /// <param name="value">The <see cref="Contact"/>.</param>
        /// <returns>The created <see cref="Contact"/>.</returns>
        public Task<Contact> CreateAsync(Contact value)
        {
            return DataSvcInvoker.Current.InvokeAsync(this, async () =>
            {
                var __result = await _data.CreateAsync(Check.NotNull(value, nameof(value))).ConfigureAwait(false);
                await _evtPub.PublishValueAsync(__result, $"Demo.Contact.{__result.Id}", "Create").ConfigureAwait(false);
                _cache.SetValue(__result.UniqueKey, __result);
                return __result;
            });
        }

        /// <summary>
        /// Updates an existing <see cref="Contact"/>.
        /// </summary>
        /// <param name="value">The <see cref="Contact"/>.</param>
        /// <returns>The updated <see cref="Contact"/>.</returns>
        public Task<Contact> UpdateAsync(Contact value)
        {
            return DataSvcInvoker.Current.InvokeAsync(this, async () =>
            {
                var __result = await _data.UpdateAsync(Check.NotNull(value, nameof(value))).ConfigureAwait(false);
                await _evtPub.PublishValueAsync(__result, $"Demo.Contact.{__result.Id}", "Update").ConfigureAwait(false);
                _cache.SetValue(__result.UniqueKey, __result);
                return __result;
            });
        }

        /// <summary>
        /// Deletes the specified <see cref="Contact"/>.
        /// </summary>
        /// <param name="id">The <see cref="Contact"/> identifier.</param>
        public Task DeleteAsync(Guid id)
        {
            return DataSvcInvoker.Current.InvokeAsync(this, async () =>
            {
                await _data.DeleteAsync(id).ConfigureAwait(false);
                await _evtPub.PublishAsync($"Demo.Contact.{id}", "Delete", id).ConfigureAwait(false);
                _cache.Remove<Contact>(new UniqueKey(id));
            });
        }
    }
}

#pragma warning restore IDE0079, IDE0001, IDE0005, IDE0044, CA1034, CA1052, CA1056, CA1819, CA2227, CS0649
#nullable restore