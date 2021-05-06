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
using My.Hr.Business.Entities;
using My.Hr.Business.DataSvc;
using My.Hr.Business.Validation;
using RefDataNamespace = My.Hr.Business.Entities;

namespace My.Hr.Business
{
    /// <summary>
    /// Provides the <see cref="Employee"/> business functionality.
    /// </summary>
    public partial class EmployeeManager : IEmployeeManager
    {
        private readonly IEmployeeDataSvc _dataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeeManager"/> class.
        /// </summary>
        /// <param name="dataService">The <see cref="IEmployeeDataSvc"/>.</param>
        public EmployeeManager(IEmployeeDataSvc dataService)
            { _dataService = Check.NotNull(dataService, nameof(dataService)); EmployeeManagerCtor(); }

        partial void EmployeeManagerCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the specified <see cref="Employee"/>.
        /// </summary>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <returns>The selected <see cref="Employee"/> where found.</returns>
        public async Task<Employee?> GetAsync(Guid id)
        {
            return await ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                Cleaner.CleanUp(id);
                (await id.Validate(nameof(id)).Mandatory().RunAsync().ConfigureAwait(false)).ThrowOnError();
                return Cleaner.Clean(await _dataService.GetAsync(id).ConfigureAwait(false));
            }, BusinessInvokerArgs.Read).ConfigureAwait(false);
        }

        /// <summary>
        /// Creates a new <see cref="Employee"/>.
        /// </summary>
        /// <param name="value">The <see cref="Employee"/>.</param>
        /// <returns>The created <see cref="Employee"/>.</returns>
        public async Task<Employee> CreateAsync(Employee value)
        {
            (await value.Validate(nameof(value)).Mandatory().RunAsync().ConfigureAwait(false)).ThrowOnError();

            return await ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                Cleaner.CleanUp(value);
                (await value.Validate(nameof(value)).Entity().With<IValidator<Employee>>().RunAsync().ConfigureAwait(false)).ThrowOnError();
                return Cleaner.Clean(await _dataService.CreateAsync(value).ConfigureAwait(false));
            }, BusinessInvokerArgs.Create).ConfigureAwait(false);
        }

        /// <summary>
        /// Updates an existing <see cref="Employee"/>.
        /// </summary>
        /// <param name="value">The <see cref="Employee"/>.</param>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <returns>The updated <see cref="Employee"/>.</returns>
        public async Task<Employee> UpdateAsync(Employee value, Guid id)
        {
            (await value.Validate(nameof(value)).Mandatory().RunAsync().ConfigureAwait(false)).ThrowOnError();

            return await ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                value.Id = id;
                Cleaner.CleanUp(value);
                (await value.Validate(nameof(value)).Entity().With<IValidator<Employee>>().RunAsync().ConfigureAwait(false)).ThrowOnError();
                return Cleaner.Clean(await _dataService.UpdateAsync(value).ConfigureAwait(false));
            }, BusinessInvokerArgs.Update).ConfigureAwait(false);
        }

        /// <summary>
        /// Deletes the specified <see cref="Employee"/>.
        /// </summary>
        /// <param name="id">The Id.</param>
        public async Task DeleteAsync(Guid id)
        {
            await ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                Cleaner.CleanUp(id);
                (await id.Validate(nameof(id)).Mandatory().Common(EmployeeValidator.CanDelete).RunAsync().ConfigureAwait(false)).ThrowOnError();
                await _dataService.DeleteAsync(id).ConfigureAwait(false);
            }, BusinessInvokerArgs.Delete).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets the <see cref="EmployeeBaseCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Common.Entities.EmployeeArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="EmployeeBaseCollectionResult"/>.</returns>
        public async Task<EmployeeBaseCollectionResult> GetByArgsAsync(EmployeeArgs? args, PagingArgs? paging)
        {
            return await ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                Cleaner.CleanUp(args);
                (await args.Validate(nameof(args)).Entity().With<IValidator<EmployeeArgs>>().RunAsync().ConfigureAwait(false)).ThrowOnError();
                return Cleaner.Clean(await _dataService.GetByArgsAsync(args, paging).ConfigureAwait(false));
            }, BusinessInvokerArgs.Read).ConfigureAwait(false);
        }

        /// <summary>
        /// Terminates an existing <see cref="Employee"/>.
        /// </summary>
        /// <param name="value">The <see cref="TerminationDetail"/>.</param>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <returns>The updated <see cref="Employee"/>.</returns>
        public async Task<Employee> TerminateAsync(TerminationDetail value, Guid id)
        {
            (await value.Validate(nameof(value)).Mandatory().RunAsync().ConfigureAwait(false)).ThrowOnError();

            return await ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                Cleaner.CleanUp(value, id);
                (await value.Validate(nameof(value)).Entity().With<IValidator<TerminationDetail>>().RunAsync().ConfigureAwait(false)).ThrowOnError();
                return Cleaner.Clean(await _dataService.TerminateAsync(value, id).ConfigureAwait(false));
            }, BusinessInvokerArgs.Update).ConfigureAwait(false);
        }
    }
}

#pragma warning restore
#nullable restore