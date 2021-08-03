// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Mapper;
using Microsoft.OData;
using Simple.OData.Client;
using System;
using System.Threading.Tasks;
using Soc = Simple.OData.Client;

namespace Beef.Data.OData
{
    /// <summary>
    /// Represents the base class for <b>OData</b> access; being a lightweight direct <b>OData</b> access layer.
    /// </summary>
    public abstract class ODataBase : IOData
    {
        #region static

        /// <summary>
        /// Transforms and throws the <see cref="IBusinessException"/> equivalent for an <see cref="Soc.WebRequestException"/>.
        /// </summary>
        /// <param name="wrex">The <see cref="Soc.WebRequestException"/>.</param>
        public static void ThrowTransformedODataException(Soc.WebRequestException wrex)
        {
            if (wrex == null)
                throw new ArgumentNullException(nameof(wrex));
        }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBase"/> class with a <paramref name="baseUri"/> automatically creating the <see cref="ClientSettings"/>.
        /// </summary>
        /// <param name="baseUri">The base <see cref="Uri"/> for the OData endpoint.</param>
        /// <param name="invoker">Enables the <see cref="Invoker"/> to be overridden; defaults to <see cref="ODataInvoker"/>.</param>
        protected ODataBase(Uri baseUri, ODataInvoker? invoker = null) : this(new Soc.ODataClientSettings(Check.NotNull(baseUri, nameof(baseUri))), invoker) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataBase"/> class with a <paramref name="clientSettings"/>.
        /// </summary>
        /// <param name="clientSettings">The <see cref="Soc.ODataClientSettings"/>.</param>
        /// <param name="invoker">Enables the <see cref="Invoker"/> to be overridden; defaults to <see cref="ODataInvoker"/>.</param>
        /// <remarks><i>Note:</i> Overrides the <see cref="Soc.ODataClientSettings.IgnoreResourceNotFoundException"/> to <c>true</c> by design.</remarks>
        protected ODataBase(Soc.ODataClientSettings clientSettings, ODataInvoker? invoker = null)
        {
            ClientSettings = Check.NotNull(clientSettings, nameof(clientSettings));
            ClientSettings.IgnoreResourceNotFoundException = true;
            ClientSettings.PreferredUpdateMethod = Soc.ODataUpdateMethod.Patch;
            Client = new Soc.ODataClient(ClientSettings);
            Invoker = invoker ?? new ODataInvoker();
        }

        /// <summary>
        /// Gets the underlying <see cref="Soc.ODataClient"/>.
        /// </summary>
        public Soc.ODataClient Client { get; private set; }

        /// <summary>
        /// Gets the <see cref="Soc.ODataClientSettings"/>.
        /// </summary>
        public Soc.ODataClientSettings ClientSettings { get; private set; }

        /// <summary>
        /// Gets the <see cref="ODataInvoker"/>.
        /// </summary>
        public ODataInvoker Invoker { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="ODataException"/> handler (by default set up to execute <see cref="ThrowTransformedODataException(Soc.WebRequestException)"/>).
        /// </summary>
        public Action<Soc.WebRequestException> ExceptionHandler { get; set; } = (wrex) => ThrowTransformedODataException(wrex);

        /// <summary>
        /// Indicates whether <see cref="Beef.Entities.PagingArgs.IsGetCount"/> is supported; i.e. does the OData endpoint support <c>$count=true</c>. Defaults to <c>true</c>.
        /// </summary>
        public bool IsPagingGetCountSupported { get; set; } = true;

        /// <summary>
        /// Creates an <see cref="ODataQuery{T, TModel}"/> to enable select-like capabilities. 
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The OData model <see cref="Type"/>.</typeparam>
        /// <param name="queryArgs">The <see cref="IODataArgs"/>.</param>
        /// <param name="query">The function to further define the query.</param>
        /// <returns>A <see cref="ODataQuery{T, TModel}"/>.</returns>
        public ODataQuery<T, TModel> Query<T, TModel>(IODataArgs queryArgs, Func<Soc.IBoundClient<TModel>, Soc.IBoundClient<TModel>>? query = null) where T : class, new() where TModel : class, new()
            => new ODataQuery<T, TModel>(this, queryArgs, query);

        /// <summary>
        /// Gets the entity for the specified <paramref name="keys"/> (mapping from <typeparamref name="TModel"/> to <typeparamref name="T"/>) asynchronously.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The OData model <see cref="Type"/>.</typeparam>
        /// <param name="getArgs">The <see cref="IODataArgs"/>.</param>
        /// <param name="keys">The key values.</param>
        /// <returns>The entity value where found; otherwise, <c>null</c>.</returns>
        public async Task<T?> GetAsync<T, TModel>(IODataArgs getArgs, params IComparable?[] keys) where T : class, new() where TModel : class, new()
        {
            if (getArgs == null)
                throw new ArgumentNullException(nameof(getArgs));

            var okeys = getArgs.GetODataKeys(keys);

            return await Invoker.InvokeAsync(this, async () =>
            {
                try
                {
                    var model = await GetModelAsync<T, TModel>(getArgs, okeys).ConfigureAwait(false);
                    return GetValue<T, TModel>(getArgs, model);
                }
                catch (WebRequestException odex)
                {
                    if (odex.Code == System.Net.HttpStatusCode.NotFound && getArgs.NullOnNotFoundResponse)
                        return null;

                    throw;
                }
            }, this).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the model.
        /// </summary>
        private async Task<TModel?> GetModelAsync<T, TModel>(IODataArgs getArgs, object?[] keys) where T : class, new() where TModel : class, new()
        {
            try
            {
                return await Client.For<TModel>(getArgs.CollectionName).Key(keys).FindEntryAsync().ConfigureAwait(false);
            }
            catch (WebRequestException odex)
            {
                if (odex.Code == System.Net.HttpStatusCode.NotFound && getArgs.NullOnNotFoundResponse)
                    return null;

                throw;
            }
        }

        /// <summary>
        /// Creates the entity (mapping from <typeparamref name="T"/> to <typeparamref name="TModel"/>) asynchronously.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The OData model <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="IODataArgs"/>.</param>
        /// <param name="value">The value to create.</param>
        /// <returns>The created entity value.</returns>
        public async Task<T> CreateAsync<T, TModel>(IODataArgs saveArgs, T value) where T : class, new() where TModel : class, new()
        {
            if (saveArgs == null)
                throw new ArgumentNullException(nameof(saveArgs));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            // Note: ChangeLog is not updated (if it exists) as it is assumed the OData data source is responsible for this.

            return await Invoker.InvokeAsync(this, async () =>
            {
                var model = saveArgs.Mapper.Map<T, TModel>(value, Mapper.OperationTypes.Create) ?? throw new InvalidOperationException("Mapping to the OData model must not result in a null value.");
                var created = await Client.For<TModel>(saveArgs.CollectionName).Set(model).InsertEntryAsync(true).ConfigureAwait(false);
                return GetValue<T, TModel>(saveArgs, created)!;
            }, this).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates the entity (mapping from <typeparamref name="T"/> to <typeparamref name="TModel"/>) asynchronously.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The OData model <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="IODataArgs"/>.</param>
        /// <param name="value">The value to update.</param>
        /// <returns>The updated entity value.</returns>
        public async Task<T> UpdateAsync<T, TModel>(IODataArgs saveArgs, T value) where T : class, new() where TModel : class, new()
        {
            if (saveArgs == null)
                throw new ArgumentNullException(nameof(saveArgs));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            // Note: ChangeLog is not updated (if it exists) as it is assumed the OData data source is responsible for this.

            return await Invoker.InvokeAsync(this, async () =>
            {
                var okeys = saveArgs.GetODataKeys(value);
                var model = await GetModelAsync<T, TModel>(saveArgs, okeys).ConfigureAwait(false);
                if (model == null)
                    throw new NotFoundException();

                saveArgs.Mapper.Map<T, TModel>(value, model, Mapper.OperationTypes.Update);
                var updated = await Client.For<TModel>(saveArgs.CollectionName).Key(okeys).Set(model).UpdateEntryAsync(true).ConfigureAwait(false);
                if (updated == null)
                    throw new NotFoundException();

                return GetValue<T, TModel>(saveArgs, updated)!;
            }, this).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes the entity for the specified <paramref name="keys"/> asynchronously.
        /// </summary>
        /// <typeparam name="T">The entity <see cref="Type"/>.</typeparam>
        /// <typeparam name="TModel">The OData model <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="IODataArgs"/>.</param>
        /// <param name="keys">The key values.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public async Task DeleteAsync<T, TModel>(IODataArgs saveArgs, params IComparable?[] keys) where T : class, new() where TModel : class, new()
        {
            if (saveArgs == null)
                throw new ArgumentNullException(nameof(saveArgs));

            await Invoker.InvokeAsync(this, async () =>
            {
                var okeys = saveArgs.GetODataKeys(keys);
                try
                {
                    await Client.For<TModel>(saveArgs.CollectionName).Key(okeys).DeleteEntryAsync().ConfigureAwait(false);
                }
                catch (WebRequestException odex)
                {
                    if (odex.Code == System.Net.HttpStatusCode.NotFound)
                        throw new NotFoundException();

                    throw;
                }
            }, this).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the corresponding entity value from the model value.
        /// </summary>
        internal static T? GetValue<T, TModel>(IODataArgs args, TModel? model) where T : class, new() where TModel : class, new()   
            => (model == null) ? null : args.Mapper.Map<TModel, T>(model, Mapper.OperationTypes.Get) ?? throw new InvalidOperationException("Mapping from the OData model must not result in a null value.");
    }
}