/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable

namespace Beef.Demo.Business.DataSvc
{
    /// <summary>
    /// Provides the <see cref="Contact"/> data repository services.
    /// </summary>
    public partial class ContactDataSvc : IContactDataSvc
    {
        private readonly IContactData _data;
        private readonly IRequestCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContactDataSvc"/> class.
        /// </summary>
        /// <param name="data">The <see cref="IContactData"/>.</param>
        /// <param name="cache">The <see cref="IRequestCache"/>.</param>
        public ContactDataSvc(IContactData data, IRequestCache cache)
            { _data = data.ThrowIfNull(); _cache = cache.ThrowIfNull(); ContactDataSvcCtor(); }

        partial void ContactDataSvcCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the <see cref="ContactCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <returns>The <see cref="ContactCollectionResult"/>.</returns>
        public Task<ContactCollectionResult> GetAllAsync() => _data.GetAllAsync();

        /// <summary>
        /// Gets the specified <see cref="Contact"/>.
        /// </summary>
        /// <param name="id">The <see cref="Contact"/> identifier.</param>
        /// <returns>The selected <see cref="Contact"/> where found.</returns>
        public Task<Contact?> GetAsync(Guid id) => _cache.GetOrAddAsync(id, () => _data.GetAsync(id));

        /// <summary>
        /// Creates a new <see cref="Contact"/>.
        /// </summary>
        /// <param name="value">The <see cref="Contact"/>.</param>
        /// <returns>The created <see cref="Contact"/>.</returns>
        public async Task<Contact> CreateAsync(Contact value)
        {
            var r = await _data.CreateAsync(value).ConfigureAwait(false);
            return _cache.SetValue(r);
        }

        /// <summary>
        /// Updates an existing <see cref="Contact"/>.
        /// </summary>
        /// <param name="value">The <see cref="Contact"/>.</param>
        /// <returns>The updated <see cref="Contact"/>.</returns>
        public async Task<Contact> UpdateAsync(Contact value)
        {
            var r = await _data.UpdateAsync(value).ConfigureAwait(false);
            return _cache.SetValue(r);
        }

        /// <summary>
        /// Deletes the specified <see cref="Contact"/>.
        /// </summary>
        /// <param name="id">The <see cref="Contact"/> identifier.</param>
        public async Task DeleteAsync(Guid id)
        {
            _cache.Remove<Contact>(id);
            await _data.DeleteAsync(id).ConfigureAwait(false);
        }

        /// <summary>
        /// Raise Event.
        /// </summary>
        /// <param name="throwError">Indicates whether throw a DivideByZero exception.</param>
        public Task RaiseEventAsync(bool throwError) => _data.RaiseEventAsync(throwError);
    }
}

#pragma warning restore
#nullable restore