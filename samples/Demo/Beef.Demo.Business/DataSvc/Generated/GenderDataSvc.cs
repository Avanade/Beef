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
    /// Provides the Gender data repository services.
    /// </summary>
    public static partial class GenderDataSvc
    {
        #region Private
        #pragma warning disable CS0649 // Defaults to null by design; can be overridden in constructor.

        private static readonly Func<Gender?, Guid, Task>? _getOnAfterAsync;
        private static readonly Func<Gender, Task>? _createOnAfterAsync;
        private static readonly Func<Gender, Task>? _updateOnAfterAsync;

        #pragma warning restore CS0649
        #endregion

        /// <summary>
        /// Gets the <see cref="Gender"/> object that matches the selection criteria.
        /// </summary>
        /// <param name="id">The <see cref="Gender"/> identifier.</param>
        /// <returns>The selected <see cref="Gender"/> object where found; otherwise, <c>null</c>.</returns>
        public static Task<Gender?> GetAsync(Guid id)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(GenderDataSvc), async () => 
            {
                var __key = new UniqueKey(id);
                if (ExecutionContext.Current.TryGetCacheValue<Gender>(__key, out Gender __val))
                    return __val;

                var __result = await Factory.Create<IGenderData>().GetAsync(id).ConfigureAwait(false);
                ExecutionContext.Current.CacheSet(__key, __result!);
                if (_getOnAfterAsync != null) await _getOnAfterAsync(__result, id).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Creates the <see cref="Gender"/> object.
        /// </summary>
        /// <param name="value">The <see cref="Gender"/> object.</param>
        /// <returns>A refreshed <see cref="Gender"/> object.</returns>
        public static Task<Gender> CreateAsync(Gender value)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(GenderDataSvc), async () => 
            {
                var __result = await Factory.Create<IGenderData>().CreateAsync(Check.NotNull(value, nameof(value))).ConfigureAwait(false);
                await Beef.Events.Event.PublishValueAsync(__result, $"Demo.Gender.{__result.Id}", "Create").ConfigureAwait(false);
                ExecutionContext.Current.CacheSet(__result.UniqueKey, __result);
                if (_createOnAfterAsync != null) await _createOnAfterAsync(__result).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Updates the <see cref="Gender"/> object.
        /// </summary>
        /// <param name="value">The <see cref="Gender"/> object.</param>
        /// <returns>A refreshed <see cref="Gender"/> object.</returns>
        public static Task<Gender> UpdateAsync(Gender value)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(GenderDataSvc), async () => 
            {
                var __result = await Factory.Create<IGenderData>().UpdateAsync(Check.NotNull(value, nameof(value))).ConfigureAwait(false);
                await Beef.Events.Event.PublishValueAsync(__result, $"Demo.Gender.{__result.Id}", "Update").ConfigureAwait(false);
                ExecutionContext.Current.CacheSet(__result.UniqueKey, __result);
                if (_updateOnAfterAsync != null) await _updateOnAfterAsync(__result).ConfigureAwait(false);
                return __result;
            });
        }
    }
}

#nullable restore