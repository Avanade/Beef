// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Beef.Events.ServiceBus
{
    /// <summary>
    /// Represents an <see cref="InvokerBase{EventHubData}">invoker</see> that uses an <see cref="IServiceBusStorageRepository"/> to manage poison events (both retry and skipping).
    /// </summary>
    public class ServiceBusReceiverHostPoisonInvoker : InvokerBase<ServiceBusData, Result>
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
        /// <param name="data">The <see cref="ServiceBusData"/>.</param>
        /// <param name="memberName">The method or property name of the caller to the method.</param>
        /// <param name="filePath">The full path of the source file that contains the caller.</param>
        /// <param name="lineNumber">The line number in the source file at which the method is called.</param>
        protected async override Task<Result> WrapInvokeAsync(object caller, Func<Task<Result>> func, ServiceBusData? data, [CallerMemberName] string? memberName = null, [CallerFilePath] string? filePath = null, [CallerLineNumber] int lineNumber = 0)
        {
            if (func == null)
                throw new ArgumentNullException(nameof(func));

            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // Update the ServiceBusReceiverHost ILogger instance where applicable.
            if (caller is ServiceBusReceiverHost sbrh && sbrh is IUseLogger ul)
                ul.UseLogger(sbrh.Logger);

            // Where previously marked as poison and is skip, then we can simply skip (ignore) the poison event (it is deleted and audited by storage).
            var aa = await Storage.CheckPoisonedAsync(data).ConfigureAwait(false);
            if (aa.Action == PoisonMessageAction.PoisonSkip)
                return EventSubscriberHost.CreatePoisonSkippedResult(data.Metadata.Subject, data.Metadata.Action);

            // Update the attempt count as we are about to attempt to process.
            data.Attempt = aa.Attempts + 1;

            // Invoke the worker function.
            try
            {
                var result = await func().ConfigureAwait(false);

                // Where previously poisoned and is now successful remove from the underlying repository.
                if (aa.Action == PoisonMessageAction.PoisonRetry)
                    await Storage.RemovePoisonedAsync(data).ConfigureAwait(false);

                return result;
            }
            catch (EventSubscriberUnhandledException esuex)
            {
                // Mark as poisoned (will be audited by storage).
                var result = await Storage.MarkAsPoisonedAsync(data, esuex.Result, (caller as EventSubscriberHost)?.MaxAttempts).ConfigureAwait(false);

                // In the end, if we can't continue, then let it bubble up as there is nothing further we can do; our host's host will take it from here!
                if (result == UnhandledExceptionHandling.ThrowException)
                    throw;

                return esuex.Result;
            }
        }
    }
}