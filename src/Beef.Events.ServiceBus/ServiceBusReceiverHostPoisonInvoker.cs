// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Beef.Events.ServiceBus
{
    /// <summary>
    /// Represents an <see cref="ServiceBusReceiverHostInvoker"/> that uses an <see cref="IServiceBusStorageRepository"/> to manage poison events (both retry and skipping).
    /// </summary>
    public class ServiceBusReceiverHostPoisonInvoker : ServiceBusReceiverHostInvoker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusReceiverHostPoisonInvoker"/> class.
        /// </summary>
        /// <param name="storage">The <see cref="IServiceBusStorageRepository"/>.</param>
        public ServiceBusReceiverHostPoisonInvoker(IServiceBusStorageRepository storage) => Storage = Check.NotNull(storage, nameof(storage));

        /// <summary>
        /// Gets the <see cref="ServiceBusAzureStorageRepository"/>.
        /// </summary>
        public IServiceBusStorageRepository Storage { get; }

        /// <summary>
        /// Invokes a <paramref name="func"/> asynchronously.
        /// </summary>
        /// <param name="caller">The calling (invoking) object.</param>
        /// <param name="func">The function to invoke.</param>
        /// <param name="message">The <see cref="ServiceBusData"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        protected async override Task WrapInvokeAsync(object caller, Func<Task> func, ServiceBusData? message, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            // Update the ServiceBusReceiverHost ILogger instance where applicable.
            if (caller is ServiceBusReceiverHost sbrh && sbrh is IUseLogger ul)
                ul.UseLogger(sbrh.Logger);

            // Where previously marked as poison and is skip, then we can simply skip (ignore) the poison event (it is deleted and audited by storage).
            var aa = await Storage.CheckPoisonedAsync(message).ConfigureAwait(false);
            if (aa.Action == PoisonMessageAction.PoisonSkip)
                return;

            // Update the attempt count as we are about to attempt to process.
            message.Attempt = aa.Attempts + 1;

            // Invoke the worker function.
            try
            {
                await func().ConfigureAwait(false);

                // Where previously poisoned and is now successful remove from the underlying repository.
                if (aa.Action == PoisonMessageAction.PoisonRetry)
                    await Storage.RemovePoisonedAsync(message).ConfigureAwait(false);
            }
            catch (EventSubscriberUnhandledException esuex)
            {
                // Mark as poisoned (will be audited by storage).
                await Storage.MarkAsPoisonedAsync(message, esuex.Result).ConfigureAwait(false);

                // In the end, let it bubble up as there is nothing further we can do; our host's host will take it from here!
                throw;
            }
        }
    }
}