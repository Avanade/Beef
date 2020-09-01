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
    /// Provides the <see cref="Gender"/> business functionality.
    /// </summary>
    public partial class GenderManager : IGenderManager
    {
        private readonly IGenderDataSvc _dataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenderManager"/> class.
        /// </summary>
        /// <param name="dataService">The <see cref="IGenderDataSvc"/>.</param>
        public GenderManager(IGenderDataSvc dataService) { _dataService = Check.NotNull(dataService, nameof(dataService)); GenderManagerCtor(); }

        partial void GenderManagerCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the specified <see cref="Gender"/>.
        /// </summary>
        /// <param name="id">The <see cref="Gender"/> identifier.</param>
        /// <returns>The selected <see cref="Gender"/> where found; otherwise, <c>null</c>.</returns>
        public Task<Gender?> GetAsync(Guid id)
        {
            return ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                ExecutionContext.Current.OperationType = OperationType.Read;
                Cleaner.CleanUp(id);
                MultiValidator.Create()
                    .Add(id.Validate(nameof(id)).Mandatory())
                    .Run().ThrowOnError();

                return Cleaner.Clean(await _dataService.GetAsync(id).ConfigureAwait(false));
            });
        }

        /// <summary>
        /// Creates a new <see cref="Gender"/>.
        /// </summary>
        /// <param name="value">The <see cref="Gender"/>.</param>
        /// <returns>A refreshed <see cref="Gender"/>.</returns>
        public Task<Gender> CreateAsync(Gender value)
        {
            value.Validate(nameof(value)).Mandatory().Run().ThrowOnError();

            return ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                ExecutionContext.Current.OperationType = OperationType.Create;
                Cleaner.CleanUp(value);
                MultiValidator.Create()
                    .Run().ThrowOnError();

                return Cleaner.Clean(await _dataService.CreateAsync(value).ConfigureAwait(false));
            });
        }

        /// <summary>
        /// Updates an existing <see cref="Gender"/>.
        /// </summary>
        /// <param name="value">The <see cref="Gender"/>.</param>
        /// <param name="id">The <see cref="Gender"/> identifier.</param>
        /// <returns>A refreshed <see cref="Gender"/>.</returns>
        public Task<Gender> UpdateAsync(Gender value, Guid id)
        {
            value.Validate(nameof(value)).Mandatory().Run().ThrowOnError();

            return ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                ExecutionContext.Current.OperationType = OperationType.Update;
                value.Id = id;
                Cleaner.CleanUp(value);
                MultiValidator.Create()
                    .Run().ThrowOnError();

                return Cleaner.Clean(await _dataService.UpdateAsync(value).ConfigureAwait(false));
            });
        }
    }
}

#pragma warning restore IDE0005
#nullable restore