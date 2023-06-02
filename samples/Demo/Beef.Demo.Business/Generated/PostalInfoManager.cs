/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

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
            { _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService)); PostalInfoManagerCtor(); }

        partial void PostalInfoManagerCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the specified <see cref="PostalInfo"/>.
        /// </summary>
        /// <param name="country">The Country.</param>
        /// <param name="state">The State.</param>
        /// <param name="city">The City.</param>
        /// <returns>The selected <see cref="PostalInfo"/> where found.</returns>
        public Task<PostalInfo?> GetPostCodesAsync(RefDataNamespace.Country? country, string? state, string? city) => ManagerInvoker.Current.InvokeAsync(this, async _ =>
        {
            Cleaner.CleanUp(country, state, city);
            await MultiValidator.Create()
                .Add(country.Validate(nameof(country)).Mandatory().IsValid())
                .Add(state.Validate(nameof(state)).Mandatory())
                .Add(city.Validate(nameof(city)).Mandatory())
                .ValidateAsync(true).ConfigureAwait(false);

            return Cleaner.Clean(await _dataService.GetPostCodesAsync(country, state, city).ConfigureAwait(false));
        }, InvokerArgs.Read);

        /// <summary>
        /// Creates a new <see cref="PostalInfo"/>.
        /// </summary>
        /// <param name="value">The <see cref="PostalInfo"/>.</param>
        /// <param name="country">The Country.</param>
        /// <param name="state">The State.</param>
        /// <param name="city">The City.</param>
        /// <returns>The created <see cref="PostalInfo"/>.</returns>
        public Task<PostalInfo> CreatePostCodesAsync(PostalInfo value, RefDataNamespace.Country? country, string? state, string? city) => ManagerInvoker.Current.InvokeAsync(this, async _ =>
        {
            Cleaner.CleanUp(value.Required(), country, state, city);
            await MultiValidator.Create()
                .Add(country.Validate(nameof(country)).Mandatory().IsValid())
                .Add(state.Validate(nameof(state)).Mandatory())
                .Add(city.Validate(nameof(city)).Mandatory())
                .ValidateAsync(true).ConfigureAwait(false);

            return Cleaner.Clean(await _dataService.CreatePostCodesAsync(value, country, state, city).ConfigureAwait(false));
        }, InvokerArgs.Create);

        /// <summary>
        /// Updates an existing <see cref="PostalInfo"/>.
        /// </summary>
        /// <param name="value">The <see cref="PostalInfo"/>.</param>
        /// <param name="country">The Country.</param>
        /// <param name="state">The State.</param>
        /// <param name="city">The City.</param>
        /// <returns>The updated <see cref="PostalInfo"/>.</returns>
        public Task<PostalInfo> UpdatePostCodesAsync(PostalInfo value, RefDataNamespace.Country? country, string? state, string? city) => ManagerInvoker.Current.InvokeAsync(this, async _ =>
        {
            Cleaner.CleanUp(value.Required(), country, state, city);
            await MultiValidator.Create()
                .Add(country.Validate(nameof(country)).Mandatory().IsValid())
                .Add(state.Validate(nameof(state)).Mandatory())
                .Add(city.Validate(nameof(city)).Mandatory())
                .ValidateAsync(true).ConfigureAwait(false);

            return Cleaner.Clean(await _dataService.UpdatePostCodesAsync(value, country, state, city).ConfigureAwait(false));
        }, InvokerArgs.Update);

        /// <summary>
        /// Deletes the specified <see cref="PostalInfo"/>.
        /// </summary>
        /// <param name="country">The Country.</param>
        /// <param name="state">The State.</param>
        /// <param name="city">The City.</param>
        public Task DeletePostCodesAsync(RefDataNamespace.Country? country, string? state, string? city) => ManagerInvoker.Current.InvokeAsync(this, async _ =>
        {
            Cleaner.CleanUp(country, state, city);
            await MultiValidator.Create()
                .Add(country.Validate(nameof(country)).Mandatory().IsValid())
                .Add(state.Validate(nameof(state)).Mandatory())
                .Add(city.Validate(nameof(city)).Mandatory())
                .ValidateAsync(true).ConfigureAwait(false);

            await _dataService.DeletePostCodesAsync(country, state, city).ConfigureAwait(false);
        }, InvokerArgs.Delete);
    }
}

#pragma warning restore
#nullable restore