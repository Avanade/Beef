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
    /// Provides the Person data repository services.
    /// </summary>
    public static partial class PersonDataSvc
    {
        #region Private
        #pragma warning disable CS0649 // Defaults to null by design; can be overridden in constructor.

        private static readonly Func<Person, Task>? _createOnAfterAsync;
        private static readonly Func<Guid, Task>? _deleteOnAfterAsync;
        private static readonly Func<Person?, Guid, Task>? _getOnAfterAsync;
        private static readonly Func<Person, Task>? _updateOnAfterAsync;
        private static readonly Func<PersonCollectionResult, PagingArgs?, Task>? _getAllOnAfterAsync;
        private static readonly Func<PersonCollectionResult, Task>? _getAll2OnAfterAsync;
        private static readonly Func<PersonCollectionResult, PersonArgs?, PagingArgs?, Task>? _getByArgsOnAfterAsync;
        private static readonly Func<PersonDetailCollectionResult, PersonArgs?, PagingArgs?, Task>? _getDetailByArgsOnAfterAsync;
        private static readonly Func<Person, Guid, Guid, Task>? _mergeOnAfterAsync;
        private static readonly Func<Task>? _markOnAfterAsync;
        private static readonly Func<MapCoordinates, MapArgs?, Task>? _mapOnAfterAsync;
        private static readonly Func<PersonDetail?, Guid, Task>? _getDetailOnAfterAsync;
        private static readonly Func<PersonDetail, Task>? _updateDetailOnAfterAsync;
        private static readonly Func<Person?, string?, Task>? _getNullOnAfterAsync;
        private static readonly Func<PersonCollectionResult, PersonArgs?, PagingArgs?, Task>? _getByArgsWithEfOnAfterAsync;
        private static readonly Func<Person?, Guid, Task>? _getWithEfOnAfterAsync;
        private static readonly Func<Person, Task>? _createWithEfOnAfterAsync;
        private static readonly Func<Person, Task>? _updateWithEfOnAfterAsync;
        private static readonly Func<Guid, Task>? _deleteWithEfOnAfterAsync;

        #pragma warning restore CS0649
        #endregion

        /// <summary>
        /// Creates the <see cref="Person"/> object.
        /// </summary>
        /// <param name="value">The <see cref="Person"/> object.</param>
        /// <returns>A refreshed <see cref="Person"/> object.</returns>
        public static Task<Person> CreateAsync(Person value)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
            {
                var __result = await Factory.Create<IPersonData>().CreateAsync(Check.NotNull(value, nameof(value))).ConfigureAwait(false);
                await Beef.Events.Event.PublishValueAsync(__result, $"Demo.Person.{__result.Id}", "Create").ConfigureAwait(false);
                ExecutionContext.Current.CacheSet(__result.UniqueKey, __result);
                if (_createOnAfterAsync != null) await _createOnAfterAsync(__result).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Deletes the <see cref="Person"/> object.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        public static Task DeleteAsync(Guid id)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
            {
                await Factory.Create<IPersonData>().DeleteAsync(id).ConfigureAwait(false);
                await Beef.Events.Event.PublishAsync($"Demo.Person.{id}", "Delete", id).ConfigureAwait(false);
                ExecutionContext.Current.CacheRemove<Person>(new UniqueKey(id));
                if (_deleteOnAfterAsync != null) await _deleteOnAfterAsync(id).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Gets the <see cref="Person"/> object that matches the selection criteria.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <returns>The selected <see cref="Person"/> object where found; otherwise, <c>null</c>.</returns>
        public static Task<Person?> GetAsync(Guid id)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
            {
                var __key = new UniqueKey(id);
                if (ExecutionContext.Current.TryGetCacheValue<Person>(__key, out Person __val))
                    return __val;

                var __result = await Factory.Create<IPersonData>().GetAsync(id).ConfigureAwait(false);
                ExecutionContext.Current.CacheSet(__key, __result!);
                if (_getOnAfterAsync != null) await _getOnAfterAsync(__result, id).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Updates the <see cref="Person"/> object.
        /// </summary>
        /// <param name="value">The <see cref="Person"/> object.</param>
        /// <returns>A refreshed <see cref="Person"/> object.</returns>
        public static Task<Person> UpdateAsync(Person value)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
            {
                var __result = await Factory.Create<IPersonData>().UpdateAsync(Check.NotNull(value, nameof(value))).ConfigureAwait(false);
                await Beef.Events.Event.PublishValueAsync(__result, $"Demo.Person.{__result.Id}", "Update").ConfigureAwait(false);
                ExecutionContext.Current.CacheSet(__result.UniqueKey, __result);
                if (_updateOnAfterAsync != null) await _updateOnAfterAsync(__result).ConfigureAwait(false);
                return __result;
            }, new BusinessInvokerArgs { IncludeTransactionScope = true });
        }

        /// <summary>
        /// Gets the <see cref="Person"/> collection object that matches the selection criteria.
        /// </summary>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>A <see cref="PersonCollectionResult"/>.</returns>
        public static Task<PersonCollectionResult> GetAllAsync(PagingArgs? paging)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
            {
                var __result = await Factory.Create<IPersonData>().GetAllAsync(paging).ConfigureAwait(false);
                if (_getAllOnAfterAsync != null) await _getAllOnAfterAsync(__result, paging).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Gets the <see cref="Person"/> collection object that matches the selection criteria.
        /// </summary>
        /// <returns>A <see cref="PersonCollectionResult"/>.</returns>
        public static Task<PersonCollectionResult> GetAll2Async()
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
            {
                var __result = await Factory.Create<IPersonData>().GetAll2Async().ConfigureAwait(false);
                if (_getAll2OnAfterAsync != null) await _getAll2OnAfterAsync(__result).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Gets the <see cref="Person"/> collection object that matches the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="PersonArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>A <see cref="PersonCollectionResult"/>.</returns>
        public static Task<PersonCollectionResult> GetByArgsAsync(PersonArgs? args, PagingArgs? paging)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
            {
                var __result = await Factory.Create<IPersonData>().GetByArgsAsync(args, paging).ConfigureAwait(false);
                if (_getByArgsOnAfterAsync != null) await _getByArgsOnAfterAsync(__result, args, paging).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Gets the <see cref="PersonDetail"/> collection object that matches the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="PersonArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>A <see cref="PersonDetailCollectionResult"/>.</returns>
        public static Task<PersonDetailCollectionResult> GetDetailByArgsAsync(PersonArgs? args, PagingArgs? paging)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
            {
                var __result = await Factory.Create<IPersonData>().GetDetailByArgsAsync(args, paging).ConfigureAwait(false);
                if (_getDetailByArgsOnAfterAsync != null) await _getDetailByArgsOnAfterAsync(__result, args, paging).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Merge first <see cref="Person"/> into second.
        /// </summary>
        /// <param name="fromId">The from <see cref="Person"/> identifier.</param>
        /// <param name="toId">The to <see cref="Person"/> identifier.</param>
        /// <returns>A resultant <see cref="Person"/>.</returns>
        public static Task<Person> MergeAsync(Guid fromId, Guid toId)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
            {
                var __result = await Factory.Create<IPersonData>().MergeAsync(fromId, toId).ConfigureAwait(false);
                await Beef.Events.Event.PublishAsync(
                    Beef.Events.EventData.CreateValue(__result, "Demo.Person.{fromId}", "Merge", fromId, toId)).ConfigureAwait(false);
                ExecutionContext.Current.CacheSet(__result.UniqueKey, __result);
                if (_mergeOnAfterAsync != null) await _mergeOnAfterAsync(__result, fromId, toId).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Mark <see cref="Person"/>.
        /// </summary>
        public static Task MarkAsync()
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
            {
                await Factory.Create<IPersonData>().MarkAsync().ConfigureAwait(false);
                if (_markOnAfterAsync != null) await _markOnAfterAsync().ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Get <see cref="Person"/> at specified <see cref="MapCoordinates"/>.
        /// </summary>
        /// <param name="args">The Args (see <see cref="MapArgs"/>).</param>
        /// <returns>A resultant <see cref="MapCoordinates"/>.</returns>
        public static Task<MapCoordinates> MapAsync(MapArgs? args)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
            {
                var __result = await Factory.Create<IPersonData>().MapAsync(args).ConfigureAwait(false);
                if (_mapOnAfterAsync != null) await _mapOnAfterAsync(__result, args).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Gets the <see cref="PersonDetail"/> object that matches the selection criteria.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <returns>The selected <see cref="PersonDetail"/> object where found; otherwise, <c>null</c>.</returns>
        public static Task<PersonDetail?> GetDetailAsync(Guid id)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
            {
                var __key = new UniqueKey(id);
                if (ExecutionContext.Current.TryGetCacheValue<PersonDetail>(__key, out PersonDetail __val))
                    return __val;

                var __result = await Factory.Create<IPersonData>().GetDetailAsync(id).ConfigureAwait(false);
                ExecutionContext.Current.CacheSet(__key, __result!);
                if (_getDetailOnAfterAsync != null) await _getDetailOnAfterAsync(__result, id).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Updates the <see cref="PersonDetail"/> object.
        /// </summary>
        /// <param name="value">The <see cref="PersonDetail"/> object.</param>
        /// <returns>A refreshed <see cref="PersonDetail"/> object.</returns>
        public static Task<PersonDetail> UpdateDetailAsync(PersonDetail value)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
            {
                var __result = await Factory.Create<IPersonData>().UpdateDetailAsync(Check.NotNull(value, nameof(value))).ConfigureAwait(false);
                await Beef.Events.Event.PublishValueAsync(__result, $"Demo.Person.{__result.Id}", "Update").ConfigureAwait(false);
                ExecutionContext.Current.CacheSet(__result.UniqueKey, __result);
                if (_updateDetailOnAfterAsync != null) await _updateDetailOnAfterAsync(__result).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Validate a DataSvc Custom generation.
        /// </summary>
        /// <returns>A resultant <see cref="int"/>.</returns>
        public static Task<int> DataSvcCustomAsync()
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
            {
                var __result = await DataSvcCustomOnImplementationAsync().ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Get Null.
        /// </summary>
        /// <param name="name">The Name.</param>
        /// <returns>A resultant <see cref="Person?"/>.</returns>
        public static Task<Person?> GetNullAsync(string? name)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
            {
                var __result = await Factory.Create<IPersonData>().GetNullAsync(name).ConfigureAwait(false);
                ExecutionContext.Current.CacheSet(__result?.UniqueKey ?? UniqueKey.Empty, __result);
                if (_getNullOnAfterAsync != null) await _getNullOnAfterAsync(__result, name).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Gets the <see cref="Person"/> collection object that matches the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="PersonArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>A <see cref="PersonCollectionResult"/>.</returns>
        public static Task<PersonCollectionResult> GetByArgsWithEfAsync(PersonArgs? args, PagingArgs? paging)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
            {
                var __result = await Factory.Create<IPersonData>().GetByArgsWithEfAsync(args, paging).ConfigureAwait(false);
                if (_getByArgsWithEfOnAfterAsync != null) await _getByArgsWithEfOnAfterAsync(__result, args, paging).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Gets the <see cref="Person"/> object that matches the selection criteria.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        /// <returns>The selected <see cref="Person"/> object where found; otherwise, <c>null</c>.</returns>
        public static Task<Person?> GetWithEfAsync(Guid id)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
            {
                var __key = new UniqueKey(id);
                if (ExecutionContext.Current.TryGetCacheValue<Person>(__key, out Person __val))
                    return __val;

                var __result = await Factory.Create<IPersonData>().GetWithEfAsync(id).ConfigureAwait(false);
                ExecutionContext.Current.CacheSet(__key, __result!);
                if (_getWithEfOnAfterAsync != null) await _getWithEfOnAfterAsync(__result, id).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Creates the <see cref="Person"/> object.
        /// </summary>
        /// <param name="value">The <see cref="Person"/> object.</param>
        /// <returns>A refreshed <see cref="Person"/> object.</returns>
        public static Task<Person> CreateWithEfAsync(Person value)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
            {
                var __result = await Factory.Create<IPersonData>().CreateWithEfAsync(Check.NotNull(value, nameof(value))).ConfigureAwait(false);
                await Beef.Events.Event.PublishValueAsync(__result, $"Demo.Person.{__result.Id}", "Create").ConfigureAwait(false);
                ExecutionContext.Current.CacheSet(__result.UniqueKey, __result);
                if (_createWithEfOnAfterAsync != null) await _createWithEfOnAfterAsync(__result).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Updates the <see cref="Person"/> object.
        /// </summary>
        /// <param name="value">The <see cref="Person"/> object.</param>
        /// <returns>A refreshed <see cref="Person"/> object.</returns>
        public static Task<Person> UpdateWithEfAsync(Person value)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
            {
                var __result = await Factory.Create<IPersonData>().UpdateWithEfAsync(Check.NotNull(value, nameof(value))).ConfigureAwait(false);
                await Beef.Events.Event.PublishValueAsync(__result, $"Demo.Person.{__result.Id}", "Update").ConfigureAwait(false);
                ExecutionContext.Current.CacheSet(__result.UniqueKey, __result);
                if (_updateWithEfOnAfterAsync != null) await _updateWithEfOnAfterAsync(__result).ConfigureAwait(false);
                return __result;
            });
        }

        /// <summary>
        /// Deletes the <see cref="Person"/> object.
        /// </summary>
        /// <param name="id">The <see cref="Person"/> identifier.</param>
        public static Task DeleteWithEfAsync(Guid id)
        {
            return DataSvcInvoker.Default.InvokeAsync(typeof(PersonDataSvc), async () => 
            {
                await Factory.Create<IPersonData>().DeleteWithEfAsync(id).ConfigureAwait(false);
                await Beef.Events.Event.PublishAsync(
                    Beef.Events.EventData.Create("Demo.Person.{id}", "Delete", id)).ConfigureAwait(false);
                ExecutionContext.Current.CacheRemove<Person>(new UniqueKey(id));
                if (_deleteWithEfOnAfterAsync != null) await _deleteWithEfOnAfterAsync(id).ConfigureAwait(false);
            });
        }
    }
}

#nullable restore