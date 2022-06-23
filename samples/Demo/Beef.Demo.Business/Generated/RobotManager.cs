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
        private readonly Beef.Events.IEventPublisher _eventPublisher;
        private readonly IGuidIdentifierGenerator _guidIdentifierGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="RobotManager"/> class.
        /// </summary>
        /// <param name="dataService">The <see cref="IRobotDataSvc"/>.</param>
        /// <param name="eventPublisher">The <see cref="Beef.Events.IEventPublisher"/>.</param>
        /// <param name="guidIdentifierGenerator">The <see cref="IGuidIdentifierGenerator"/>.</param>
        public RobotManager(IRobotDataSvc dataService, Beef.Events.IEventPublisher eventPublisher, IGuidIdentifierGenerator guidIdentifierGenerator)
        {
            _dataService = Check.NotNull(dataService, nameof(dataService));
            _eventPublisher = Check.NotNull(eventPublisher, nameof(eventPublisher));
            _guidIdentifierGenerator = Check.NotNull(guidIdentifierGenerator, nameof(guidIdentifierGenerator));
            RobotManagerCtor();
        }

        partial void RobotManagerCtor(); // Enables additional functionality to be added to the constructor.

        /// <summary>
        /// Gets the specified <see cref="Robot"/>.
        /// </summary>
        /// <param name="id">The <see cref="Robot"/> identifier.</param>
        /// <returns>The selected <see cref="Robot"/> where found.</returns>
        public Task<Robot?> GetAsync(Guid id) => ManagerInvoker.Current.InvokeAsync(this, async () =>
        {
            Cleaner.CleanUp(id);
            await id.Validate(nameof(id)).Mandatory().RunAsync(throwOnError: true).ConfigureAwait(false);
            return Cleaner.Clean(await _dataService.GetAsync(id).ConfigureAwait(false));
        }, BusinessInvokerArgs.Read);

        /// <summary>
        /// Creates a new <see cref="Robot"/>.
        /// </summary>
        /// <param name="value">The <see cref="Robot"/>.</param>
        /// <returns>The created <see cref="Robot"/>.</returns>
        public Task<Robot> CreateAsync(Robot value) => ManagerInvoker.Current.InvokeAsync(this, async () =>
        {
            await value.Validate().Mandatory().RunAsync(throwOnError: true).ConfigureAwait(false);

            value.Id = await _guidIdentifierGenerator.GenerateIdentifierAsync<Robot>().ConfigureAwait(false);
            Cleaner.CleanUp(value);
            await value.Validate().Entity().With<IValidator<Robot>>().RunAsync(throwOnError: true).ConfigureAwait(false);
            return Cleaner.Clean(await _dataService.CreateAsync(value).ConfigureAwait(false));
        }, BusinessInvokerArgs.Create);

        /// <summary>
        /// Updates an existing <see cref="Robot"/>.
        /// </summary>
        /// <param name="value">The <see cref="Robot"/>.</param>
        /// <param name="id">The <see cref="Robot"/> identifier.</param>
        /// <returns>The updated <see cref="Robot"/>.</returns>
        public Task<Robot> UpdateAsync(Robot value, Guid id) => ManagerInvoker.Current.InvokeAsync(this, async () =>
        {
            await value.Validate().Mandatory().RunAsync(throwOnError: true).ConfigureAwait(false);

            value.Id = id;
            Cleaner.CleanUp(value);
            await value.Validate().Entity().With<IValidator<Robot>>().RunAsync(throwOnError: true).ConfigureAwait(false);
            return Cleaner.Clean(await _dataService.UpdateAsync(value).ConfigureAwait(false));
        }, BusinessInvokerArgs.Update);

        /// <summary>
        /// Deletes the specified <see cref="Robot"/>.
        /// </summary>
        /// <param name="id">The <see cref="Robot"/> identifier.</param>
        public Task DeleteAsync(Guid id) => ManagerInvoker.Current.InvokeAsync(this, async () =>
        {
            Cleaner.CleanUp(id);
            await id.Validate(nameof(id)).Mandatory().RunAsync(throwOnError: true).ConfigureAwait(false);
            await _dataService.DeleteAsync(id).ConfigureAwait(false);
        }, BusinessInvokerArgs.Delete);

        /// <summary>
        /// Gets the <see cref="RobotCollectionResult"/> that contains the items that match the selection criteria.
        /// </summary>
        /// <param name="args">The Args (see <see cref="Entities.RobotArgs"/>).</param>
        /// <param name="paging">The <see cref="PagingArgs"/>.</param>
        /// <returns>The <see cref="RobotCollectionResult"/>.</returns>
        public Task<RobotCollectionResult> GetByArgsAsync(RobotArgs? args, PagingArgs? paging) => ManagerInvoker.Current.InvokeAsync(this, async () =>
        {
            Cleaner.CleanUp(args);
            await args.Validate(nameof(args)).Entity().With<IValidator<RobotArgs>>().RunAsync(throwOnError: true).ConfigureAwait(false);
            return Cleaner.Clean(await _dataService.GetByArgsAsync(args, paging).ConfigureAwait(false));
        }, BusinessInvokerArgs.Read);

        /// <summary>
        /// Raises a <see cref="Robot.PowerSource"/> change event.
        /// </summary>
        /// <param name="id">The <see cref="Robot"/> identifier.</param>
        /// <param name="powerSource">The Power Source.</param>
        public Task RaisePowerSourceChangeAsync(Guid id, RefDataNamespace.PowerSource? powerSource) => ManagerInvoker.Current.InvokeAsync(this, async () =>
        {
            await RaisePowerSourceChangeOnImplementationAsync(id, powerSource).ConfigureAwait(false);
        }, BusinessInvokerArgs.Unspecified);
    }
}

#pragma warning restore
#nullable restore