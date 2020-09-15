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
using My.Hr.Common.Entities;
using My.Hr.Business.DataSvc;
using My.Hr.Business.Validation;
using RefDataNamespace = My.Hr.Common.Entities;

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
        /// <param name="id">The Id.</param>
        /// <returns>The selected <see cref="Employee"/> where found.</returns>
        public Task<Employee?> GetAsync(Guid id)
        {
            return ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                ExecutionContext.Current.OperationType = OperationType.Read;
                Cleaner.CleanUp(id);
                id.Validate(nameof(id)).Mandatory().Run().ThrowOnError();
                return Cleaner.Clean(await _dataService.GetAsync(id).ConfigureAwait(false));
            });
        }

        /// <summary>
        /// Creates a new <see cref="Employee"/>.
        /// </summary>
        /// <param name="value">The <see cref="Employee"/>.</param>
        /// <returns>The created <see cref="Employee"/>.</returns>
        public Task<Employee> CreateAsync(Employee value)
        {
            value.Validate(nameof(value)).Mandatory().Run().ThrowOnError();

            return ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                ExecutionContext.Current.OperationType = OperationType.Create;
                Cleaner.CleanUp(value);
                value.Validate(nameof(value)).Entity(EmployeeValidator.Default).Run().ThrowOnError();
                return Cleaner.Clean(await _dataService.CreateAsync(value).ConfigureAwait(false));
            });
        }

        /// <summary>
        /// Updates an existing <see cref="Employee"/>.
        /// </summary>
        /// <param name="value">The <see cref="Employee"/>.</param>
        /// <param name="id">The Id.</param>
        /// <returns>The updated <see cref="Employee"/>.</returns>
        public Task<Employee> UpdateAsync(Employee value, Guid id)
        {
            value.Validate(nameof(value)).Mandatory().Run().ThrowOnError();

            return ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                ExecutionContext.Current.OperationType = OperationType.Update;
                value.Id = id;
                Cleaner.CleanUp(value);
                value.Validate(nameof(value)).Entity(EmployeeValidator.Default).Run().ThrowOnError();
                return Cleaner.Clean(await _dataService.UpdateAsync(value).ConfigureAwait(false));
            });
        }

        /// <summary>
        /// Deletes the specified <see cref="Employee"/>.
        /// </summary>
        /// <param name="id">The Id.</param>
        public Task DeleteAsync(Guid id)
        {
            return ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                ExecutionContext.Current.OperationType = OperationType.Delete;
                Cleaner.CleanUp(id);
                id.Validate(nameof(id)).Mandatory().Run().ThrowOnError();
                await _dataService.DeleteAsync(id).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Gets the <see cref="EmployeeBaseCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Common.Entities.EmployeeArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="EmployeeBaseCollectionResult"/>.</returns>
        public Task<EmployeeBaseCollectionResult> GetByArgsAsync(EmployeeArgs? args, PagingArgs? paging)
        {
            return ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                ExecutionContext.Current.OperationType = OperationType.Read;
                Cleaner.CleanUp(args);
                args.Validate(nameof(args)).Entity(EmployeeArgsValidator.Default).Run().ThrowOnError();
                return Cleaner.Clean(await _dataService.GetByArgsAsync(args, paging).ConfigureAwait(false));
            });
        }

        /// <summary>
        /// Terminates an existing <see cref="Employee"/>.
        /// </summary>
        /// <param name="value">The <see cref="TerminationDetail"/>.</param>
        /// <param name="id">The <see cref="Employee"/> identifier.</param>
        /// <returns>The updated <see cref="Employee"/>.</returns>
        public Task<Employee> TerminateAsync(TerminationDetail value, Guid id)
        {
            value.Validate(nameof(value)).Mandatory().Run().ThrowOnError();

            return ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                ExecutionContext.Current.OperationType = OperationType.Update;
                Cleaner.CleanUp(value, id);
                value.Validate(nameof(value)).Entity(TerminationDetailValidator.Default).Run().ThrowOnError();
                return Cleaner.Clean(await _dataService.TerminateAsync(value, id).ConfigureAwait(false));
            });
        }
    }
}

#pragma warning restore IDE0005
#nullable restore