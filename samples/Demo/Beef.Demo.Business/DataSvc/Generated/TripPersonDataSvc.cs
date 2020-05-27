/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Beef;
using Beef.Business;
using Beef.Entities;
using Beef.Demo.Business.Data;
using Beef.Demo.Common.Entities;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Business.DataSvc
{
    /// <summary>
    /// Provides the Trip Person data repository services.
    /// </summary>
    public static partial class TripPersonDataSvc
    {
        #region Private
        #pragma warning disable CS0649 // Defaults to null by design; can be overridden in constructor.

        private static readonly Func<TripPerson?, string?, Task>? _getOnAfterAsync;
        private static readonly Func<TripPerson, Task>? _createOnAfterAsync;
        private static readonly Func<TripPerson, Task>? _updateOnAfterAsync;
        private static readonly Func<string?, Task>? _deleteOnAfterAsync;

        #pragma warning restore CS0649
        #endregion

        /// <summary>
        /// Gets the <see cref="TripPerson"/> object that matches the selection criteria.
        /// </summary>
        /// <param name="id">The <see cref="TripPerson"/> identifier (username).</param>
        /// <returns>The selected <see cref="TripPerson"/> object where found; otherwise, <c>null</c>.</returns>
        public static Task<TripPerson?> GetAsync(string? id)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(TripPersonDataSvc), async () => 
            {
                var __key = new UniqueKey(id);
                if (ExecutionContext.Current.TryGetCacheValue<TripPerson>(__key, out TripPerson __val))
                    return __val;

                var __result = await Factory.Create<ITripPersonData>().GetAsync(id).ConfigureAwait(false);
                ExecutionContext.Current.CacheSet(__key, __result!);
                if (_getOnAfterAsync != null) await _getOnAfterAsync(__result, id).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Creates the <see cref="TripPerson"/> object.
        /// </summary>
        /// <param name="value">The <see cref="TripPerson"/> object.</param>
        /// <returns>A refreshed <see cref="TripPerson"/> object.</returns>
        public static Task<TripPerson> CreateAsync(TripPerson value)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(TripPersonDataSvc), async () => 
            {
                var __result = await Factory.Create<ITripPersonData>().CreateAsync(Check.NotNull(value, nameof(value))).ConfigureAwait(false);
                await Beef.Events.Event.PublishValueEventAsync(__result, $"Demo.TripPerson.{__result.Id}", "Create").ConfigureAwait(false);
                ExecutionContext.Current.CacheSet(__result.UniqueKey, __result);
                if (_createOnAfterAsync != null) await _createOnAfterAsync(__result).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Updates the <see cref="TripPerson"/> object.
        /// </summary>
        /// <param name="value">The <see cref="TripPerson"/> object.</param>
        /// <returns>A refreshed <see cref="TripPerson"/> object.</returns>
        public static Task<TripPerson> UpdateAsync(TripPerson value)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(TripPersonDataSvc), async () => 
            {
                var __result = await Factory.Create<ITripPersonData>().UpdateAsync(Check.NotNull(value, nameof(value))).ConfigureAwait(false);
                await Beef.Events.Event.PublishValueEventAsync(__result, $"Demo.TripPerson.{__result.Id}", "Update").ConfigureAwait(false);
                ExecutionContext.Current.CacheSet(__result.UniqueKey, __result);
                if (_updateOnAfterAsync != null) await _updateOnAfterAsync(__result).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Deletes the <see cref="TripPerson"/> object.
        /// </summary>
        /// <param name="id">The <see cref="TripPerson"/> identifier (username).</param>
        public static Task DeleteAsync(string? id)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(TripPersonDataSvc), async () => 
            {
                await Factory.Create<ITripPersonData>().DeleteAsync(id).ConfigureAwait(false);
                await Beef.Events.Event.PublishEventAsync($"Demo.TripPerson.{id}", "Delete", id).ConfigureAwait(false);
                ExecutionContext.Current.CacheRemove<TripPerson>(new UniqueKey(id));
                if (_deleteOnAfterAsync != null) await _deleteOnAfterAsync(id).ConfigureAwait(false);
            });
        }
    }
}

#nullable restore