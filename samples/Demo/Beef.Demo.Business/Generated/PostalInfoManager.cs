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
using Beef.Entities;
using Beef.Validation;
using Beef.Demo.Common.Entities;
using Beef.Demo.Business.DataSvc;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Business
{
    /// <summary>
    /// Provides the <see cref="PostalInfo"/> business functionality.
    /// </summary>
    public partial class PostalInfoManager : IPostalInfoManager
    {
        private readonly IPostalInfoDataSvc _dataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostalInfoManager"/> class.
        /// </summary>
        /// <param name="dataService">The <see cref="IPostalInfoDataSvc"/>.</param>
        public PostalInfoManager(IPostalInfoDataSvc dataService)
            { _dataService = Check.NotNull(dataService, nameof(dataService)); PostalInfoManagerCtor(); }

        partial void PostalInfoManagerCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the specified <see cref="PostalInfo"/>.
        /// </summary>
        /// <param name="country">The Country.</param>
        /// <param name="state">The State.</param>
        /// <param name="city">The City.</param>
        /// <returns>The selected <see cref="PostalInfo"/> where found.</returns>
        public Task<PostalInfo?> GetPostCodesAsync(RefDataNamespace.Country? country, string? state, string? city) => ManagerInvoker.Current.InvokeAsync(this, async () =>
        {
            Cleaner.CleanUp(country, state, city);
            await MultiValidator.Create()
                .Add(country.Validate(nameof(country)).Mandatory().IsValid())
                .Add(state.Validate(nameof(state)).Mandatory())
                .Add(city.Validate(nameof(city)).Mandatory())
                .RunAsync(throwOnError: true).ConfigureAwait(false);

            return Cleaner.Clean(await _dataService.GetPostCodesAsync(country, state, city).ConfigureAwait(false));
        }, BusinessInvokerArgs.Read);

        /// <summary>
        /// Creates a new <see cref="PostalInfo"/>.
        /// </summary>
        /// <param name="value">The <see cref="PostalInfo"/>.</param>
        /// <param name="country">The Country.</param>
        /// <param name="state">The State.</param>
        /// <param name="city">The City.</param>
        /// <returns>The created <see cref="PostalInfo"/>.</returns>
        public Task<PostalInfo> CreatePostCodesAsync(PostalInfo value, RefDataNamespace.Country? country, string? state, string? city) => ManagerInvoker.Current.InvokeAsync(this, async () =>
        {
            await value.Validate().Mandatory().RunAsync(throwOnError: true).ConfigureAwait(false);

            Cleaner.CleanUp(value, country, state, city);
            await MultiValidator.Create()
                .Add(country.Validate(nameof(country)).Mandatory().IsValid())
                .Add(state.Validate(nameof(state)).Mandatory())
                .Add(city.Validate(nameof(city)).Mandatory())
                .RunAsync(throwOnError: true).ConfigureAwait(false);

            return Cleaner.Clean(await _dataService.CreatePostCodesAsync(value, country, state, city).ConfigureAwait(false));
        }, BusinessInvokerArgs.Create);

        /// <summary>
        /// Updates an existing <see cref="PostalInfo"/>.
        /// </summary>
        /// <param name="value">The <see cref="PostalInfo"/>.</param>
        /// <param name="country">The Country.</param>
        /// <param name="state">The State.</param>
        /// <param name="city">The City.</param>
        /// <returns>The updated <see cref="PostalInfo"/>.</returns>
        public Task<PostalInfo> UpdatePostCodesAsync(PostalInfo value, RefDataNamespace.Country? country, string? state, string? city) => ManagerInvoker.Current.InvokeAsync(this, async () =>
        {
            await value.Validate().Mandatory().RunAsync(throwOnError: true).ConfigureAwait(false);

            Cleaner.CleanUp(value, country, state, city);
            await MultiValidator.Create()
                .Add(country.Validate(nameof(country)).Mandatory().IsValid())
                .Add(state.Validate(nameof(state)).Mandatory())
                .Add(city.Validate(nameof(city)).Mandatory())
                .RunAsync(throwOnError: true).ConfigureAwait(false);

            return Cleaner.Clean(await _dataService.UpdatePostCodesAsync(value, country, state, city).ConfigureAwait(false));
        }, BusinessInvokerArgs.Update);
    }
}

#pragma warning restore
#nullable restore