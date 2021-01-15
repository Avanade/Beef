/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable IDE0005 // Using directive is unnecessary; are required depending on code-gen options

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
    /// Provides the <see cref="TripPerson"/> business functionality.
    /// </summary>
    public partial class TripPersonManager : ITripPersonManager
    {
        private readonly ITripPersonDataSvc _dataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TripPersonManager"/> class.
        /// </summary>
        /// <param name="dataService">The <see cref="ITripPersonDataSvc"/>.</param>
        public TripPersonManager(ITripPersonDataSvc dataService)
            { _dataService = Check.NotNull(dataService, nameof(dataService)); TripPersonManagerCtor(); }

        partial void TripPersonManagerCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the specified <see cref="TripPerson"/>.
        /// </summary>
        /// <param name="id">The <see cref="TripPerson"/> identifier (username).</param>
        /// <returns>The selected <see cref="TripPerson"/> where found.</returns>
        public async Task<TripPerson?> GetAsync(string? id)
        {
            return await ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                ExecutionContext.Current.OperationType = OperationType.Read;
                Cleaner.CleanUp(id);
                (await id.Validate(nameof(id)).Mandatory().RunAsync().ConfigureAwait(false)).ThrowOnError();
                return Cleaner.Clean(await _dataService.GetAsync(id).ConfigureAwait(false));
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new <see cref="TripPerson"/>.
        /// </summary>
        /// <param name="value">The <see cref="TripPerson"/>.</param>
        /// <returns>The created <see cref="TripPerson"/>.</returns>
        public async Task<TripPerson> CreateAsync(TripPerson value)
        {
            (await value.Validate(nameof(value)).Mandatory().RunAsync().ConfigureAwait(false)).ThrowOnError();

            return await ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                ExecutionContext.Current.OperationType = OperationType.Create;
                Cleaner.CleanUp(value);
                return Cleaner.Clean(await _dataService.CreateAsync(value).ConfigureAwait(false));
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates an existing <see cref="TripPerson"/>.
        /// </summary>
        /// <param name="value">The <see cref="TripPerson"/>.</param>
        /// <param name="id">The <see cref="TripPerson"/> identifier (username).</param>
        /// <returns>The updated <see cref="TripPerson"/>.</returns>
        public async Task<TripPerson> UpdateAsync(TripPerson value, string? id)
        {
            (await value.Validate(nameof(value)).Mandatory().RunAsync().ConfigureAwait(false)).ThrowOnError();

            return await ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                ExecutionContext.Current.OperationType = OperationType.Update;
                value.Id = id;
                Cleaner.CleanUp(value);
                return Cleaner.Clean(await _dataService.UpdateAsync(value).ConfigureAwait(false));
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes the specified <see cref="TripPerson"/>.
        /// </summary>
        /// <param name="id">The <see cref="TripPerson"/> identifier (username).</param>
        public async Task DeleteAsync(string? id)
        {
            await ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                ExecutionContext.Current.OperationType = OperationType.Delete;
                Cleaner.CleanUp(id);
                (await id.Validate(nameof(id)).Mandatory().RunAsync().ConfigureAwait(false)).ThrowOnError();
                await _dataService.DeleteAsync(id).ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
}

#pragma warning restore IDE0005
#nullable restore