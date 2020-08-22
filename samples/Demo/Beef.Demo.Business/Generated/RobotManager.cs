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
using Beef.Demo.Business.Validation;
using RefDataNamespace = Beef.Demo.Common.Entities;

namespace Beef.Demo.Business
{
    /// <summary>
    /// Provides the <see cref="Robot"/> business functionality.
    /// </summary>
    public partial class RobotManager : IRobotManager
    {
        private readonly IRobotDataSvc _dataService;

        /// <summary>
        /// Initializes a new instance of the <see cref="RobotManager"/> class.
        /// </summary>
        /// <param name="dataService">The <see cref="IRobotDataSvc"/>.</param>
        private RobotManager(IRobotDataSvc dataService) { _dataService = Check.NotNull(dataService, nameof(dataService)); RobotManagerCtor(); }

        partial void RobotManagerCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the specified <see cref="Robot"/>.
        /// </summary>
        /// <param name="id">The <see cref="Robot"/> identifier.</param>
        /// <returns>The selected <see cref="Robot"/> where found; otherwise, <c>null</c>.</returns>
        public Task<Robot?> GetAsync(Guid id)
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
        /// Creates a new <see cref="Robot"/>.
        /// </summary>
        /// <param name="value">The <see cref="Robot"/>.</param>
        /// <returns>A refreshed <see cref="Robot"/>.</returns>
        public Task<Robot> CreateAsync(Robot value)
        {
            value.Validate(nameof(value)).Mandatory().Run().ThrowOnError();

            return ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                ExecutionContext.Current.OperationType = OperationType.Create;
                Cleaner.CleanUp(value);
                MultiValidator.Create()
                    .Add(value.Validate(nameof(value)).Entity(RobotValidator.Default))
                    .Run().ThrowOnError();

                return Cleaner.Clean(await _dataService.CreateAsync(value).ConfigureAwait(false));
            });
        }

        /// <summary>
        /// Updates an existing <see cref="Robot"/>.
        /// </summary>
        /// <param name="value">The <see cref="Robot"/>.</param>
        /// <param name="id">The <see cref="Robot"/> identifier.</param>
        /// <returns>A refreshed <see cref="Robot"/>.</returns>
        public Task<Robot> UpdateAsync(Robot value, Guid id)
        {
            value.Validate(nameof(value)).Mandatory().Run().ThrowOnError();

            return ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                ExecutionContext.Current.OperationType = OperationType.Update;
                value.Id = id;
                Cleaner.CleanUp(value);
                MultiValidator.Create()
                    .Add(value.Validate(nameof(value)).Entity(RobotValidator.Default))
                    .Run().ThrowOnError();

                return Cleaner.Clean(await _dataService.UpdateAsync(value).ConfigureAwait(false));
            });
        }

        /// <summary>
        /// Deletes the specified <see cref="Robot"/>.
        /// </summary>
        /// <param name="id">The <see cref="Robot"/> identifier.</param>
        public Task DeleteAsync(Guid id)
        {
            return ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                ExecutionContext.Current.OperationType = OperationType.Delete;
                Cleaner.CleanUp(id);
                MultiValidator.Create()
                    .Add(id.Validate(nameof(id)).Mandatory())
                    .Run().ThrowOnError();

                await _dataService.DeleteAsync(id).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Gets the <see cref="RobotCollectionResult"/> that includes the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Common.Entities.RobotArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="RobotCollectionResult"/>.</returns>
        public Task<RobotCollectionResult> GetByArgsAsync(RobotArgs? args, PagingArgs? paging)
        {
            return ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                ExecutionContext.Current.OperationType = OperationType.Read;
                Cleaner.CleanUp(args);
                MultiValidator.Create()
                    .Add(args.Validate(nameof(args)).Entity(RobotArgsValidator.Default))
                    .Run().ThrowOnError();

                return Cleaner.Clean(await _dataService.GetByArgsAsync(args, paging).ConfigureAwait(false));
            });
        }

        /// <summary>
        /// Raises a <see cref="Robot.PowerSource"/> change event.
        /// </summary>
        /// <param name="id">The <see cref="Robot"/> identifier.</param>
        /// <param name="powerSource">The Power Source (see <see cref="RefDataNamespace.PowerSource"/>).</param>
        public Task RaisePowerSourceChangeAsync(Guid id, RefDataNamespace.PowerSource? powerSource)
        {
            return ManagerInvoker.Current.InvokeAsync(this, async () =>
            {
                ExecutionContext.Current.OperationType = OperationType.Unspecified;
                await RaisePowerSourceChangeOnImplementationAsync(id, powerSource).ConfigureAwait(false);
            });
        }
    }
}

#pragma warning restore IDE0005
#nullable restore