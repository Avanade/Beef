// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events.Triggers.Config;
using Beef.Events.Triggers.PoisonMessages;
using EventHubs = Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Events.Triggers.Listener
{
    /// <summary>
    /// Represents the "resilient event hub" processor that performs the actual work of managing the function invocation ensuring in-order, at least-once execution of each event.
    /// </summary>
    public class ResilientEventHubProcessor : IEventProcessor
    {
        private static readonly Random jitterer = new Random();

        private readonly ITriggeredFunctionExecutor _executor;
        private readonly ResilientEventHubOptions _options;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private AsyncRetryPolicy<FunctionResult> _policyAsync;
        private IPoisonMessagePersistence _poisonOrchestrator;
        private PoisonMessageAction _currPoisonAction = PoisonMessageAction.Undetermined;
        private EventHubs.EventData _currEventData;
        private EventHubs.EventData _lastEventData;
        private EventHubs.EventData _lastCheckpoint;

        /// <summary>
        /// Gets the standard log information for a <see cref="PartitionContext"/>.
        /// </summary>
        private static string GetEventDataLogInfo(PartitionContext context, EventHubs.EventData @event) => $"[PartitionId: '{context.PartitionId}', Offset: '{@event.SystemProperties?.Offset}', SequenceNumber: '{@event.SystemProperties?.SequenceNumber}']";

        /// <summary>
        /// Gets the standard log information for a <see cref="PartitionContext"/>.
        /// </summary>
        private static string GetPartitionContextLogInfo(PartitionContext context) => $"[PartitionId: '{context.PartitionId}']";

        /// <summary>
        /// Overrides the retry <see cref="TimeSpan"/> versus using the built-in retry calculation (2^n seconds where n is the retry count) until the
        /// <see cref="ResilientEventHubOptions.MaxRetryTimespan"/> is reached. This is generally enabled to support overriding for the likes of testing purposes.
        /// </summary>
        public static TimeSpan? OverrideRetryTimespan { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResilientEventHubProcessor"/> class.
        /// </summary>
        /// <param name="executor">The <see cref="ITriggeredFunctionExecutor"/>.</param>
        /// <param name="options">The <see cref="ResilientEventHubOptions"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public ResilientEventHubProcessor(ITriggeredFunctionExecutor executor, ResilientEventHubOptions options, IConfiguration config, ILogger logger)
        {
            _executor = executor;
            _options = options;
            _config = config;
            _logger = logger;
        }

        /// <summary>
        /// Starts processing.
        /// </summary>
        public Task OpenAsync(PartitionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _logger.LogInformation($"Processor starting. {GetPartitionContextLogInfo(context)}");

            // Configure the retry policy for use.
            _policyAsync = Policy
                .HandleResult<FunctionResult>(fr => !fr.Succeeded)
                .WaitAndRetryForeverAsync(
                    (count, ctx) =>
                    {
                        if (count > 16) // 2^16 is 65,536 which is the biggest allowed within our 1 day (86,400s) constraint therefore no need to calculate.
                            return _options.MaxRetryTimespan;
                        else
                        {
                            // Use a jitterer to randomise the retrys to limit retry concurrency across the underlying threads (key for the early retries).
                            var ts = TimeSpan.FromSeconds(Math.Pow(2, count)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 100));
                            return ts < _options.MaxRetryTimespan ? ts : _options.MaxRetryTimespan;
                        }
                    },
                    async (dr, count, timespan, ctx) =>
                    {
                        var isPoisoned = _currPoisonAction == PoisonMessageAction.PoisonRetry || count >= _options.LogPoisonMessageAfterRetryCount;
                        var msg = $"Failure retry{(isPoisoned ? " (Poisoned)" : "")} in {timespan.TotalSeconds}s (attempt {count}) {GetEventDataLogInfo(context, _currEventData)}.";

                        switch (count)
                        {
                            case var val when val == _options.LogPoisonMessageAfterRetryCount:
                                // Set the poison message now that we have (possibly) attempted enough times that it may not be transient in nature and some needs to be alerted.
                                await _poisonOrchestrator.SetAsync(_currEventData, dr.Result.Exception);
                                _currPoisonAction = PoisonMessageAction.PoisonRetry;
                                _logger.LogError(dr.Result.Exception, msg);
                                break;

                            case var val when val > _options.LogPoisonMessageAfterRetryCount:
                                // Keep logging advising the error is still in play.
                                _logger.LogError(dr.Result.Exception, msg);
                                break;

                            default:
                                // It could be a transient error, so report as a warning until identified as poison.
                                if (isPoisoned)
                                    _logger.LogError(dr.Result.Exception, msg);
                                else
                                    _logger.LogWarning(dr.Result.Exception, msg);

                                break;
                        }
                    });

            // Instantiate the poison message orchestration.
            _poisonOrchestrator = PoisonMessagePersistence.Create(new PoisonMessageCreatePersistenceArgs { Config = _config, Context = context, Logger = _logger, Options = _options });

            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops processing.
        /// </summary>
        public async Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            _logger.LogInformation($"Processor stopping. {GetPartitionContextLogInfo(context)}");

            // Make sure the last successful execution was checkpointed before finishing up.
            await CheckpointAsync(context, _lastEventData);
        }

        /// <summary>
        /// Process the error (<see cref="Exception"/>).
        /// </summary>
        public Task ProcessErrorAsync(PartitionContext context, Exception exception)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            // For EventProcessorHost these exceptions can happen as part of normal partition balancing across instances, so we want to trace them, but not treat them as errors.
            if (exception is EventHubs.ReceiverDisconnectedException || exception is LeaseLostException)
                _logger.LogInformation($"'{exception.GetType().Name}' was thrown; this exception type is typically a result of Event Hub processor rebalancing and can be safely ignored {GetPartitionContextLogInfo(context)}: {exception.Message}");
            else if (exception is TaskCanceledException)
                _logger.LogInformation($"'{exception.GetType().Name}' was thrown to stop the executing task as a result of a task cancel {GetPartitionContextLogInfo(context)}.");

            // Otherwise, the error will be logged by the "polly retry" logic; so do not log again.

            return Task.CompletedTask;
        }

        /// <summary>
        /// Process the events.
        /// </summary>
        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventHubs.EventData> events)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            if (events == null)
                throw new ArgumentNullException(nameof(events));

            // Note that checkpointing is only performed where an error occurs, and at the end of processing all events, to minimise chattiness to storage. 
            try
            {
                // Convert to an array and make sure there is work to do.
                var array = events.ToArray();
                if (array.Length == 0)
                    return;

                // Reset "last" as we are executing a new set of events.
                _lastCheckpoint = _lastEventData = null;

                // Process all events (note: we want to minimise the checkpointing, so will only do when all processed, or where we encounter an error to retry.
                for (int i = 0; i < array.Length; i++)
                {
                    // Stop where a cancel has been requested.
                    if (context.CancellationToken.IsCancellationRequested)
                        break;

                    // Cancellation token passed into polly, as well as the function, as either may need to cancel on request.
                    _currEventData = array[i];
                    await _policyAsync.ExecuteAsync(async (ct) => await ExecuteCurrentEvent(context, ct), context.CancellationToken);

                    // Remember, remember the 5th of November (https://www.youtube.com/watch?v=LF1951pENdk) and the last event data that was successful.
                    _lastEventData = _currEventData;
                    _currEventData = null;
                }

                // Array complete (or cancelled), so checkpoint on the last.
                await CheckpointAsync(context, _lastEventData);
                _logger.LogInformation($"Batch of '{array.Length}' event(s) completed {GetPartitionContextLogInfo(context)}.");
            }
            catch (TaskCanceledException) { throw; } // Expected; carry on.
            catch (EventHubs.ReceiverDisconnectedException) { throw; } // Expected; carry on.
            catch (LeaseLostException) { throw; } // Expected; carry on.
            catch (Exception ex) // Catch all, log, and carry on.
            {
                _logger.LogCritical(ex, $"Unexpected/unhandled exception occured during processing {GetPartitionContextLogInfo(context)}: {ex.Message}");
                throw;
            }
            finally
            {
                // Dispose all of the events as we are done with them (regardless of whether all processed).
                foreach (var e in events)
                {
                    e.Dispose();
                }
            }
        }

        /// <summary>
        /// Executes the "current" event.
        /// </summary>
        private async Task<FunctionResult> ExecuteCurrentEvent(PartitionContext context, CancellationToken ct)
        {
            // Where the poison action state is unknown or retry then check to see what the current state is; if skip, then bypass current.
            if (_currPoisonAction == PoisonMessageAction.Undetermined || _currPoisonAction == PoisonMessageAction.PoisonRetry)
            {
                _currPoisonAction = await _poisonOrchestrator.CheckAsync(_currEventData);
                if (_currPoisonAction == PoisonMessageAction.PoisonSkip)
                {
                    await _poisonOrchestrator.RemoveAsync(_currEventData, PoisonMessageAction.PoisonSkip);
                    _logger.LogWarning($"EventData that was previously identified as Poison is being skipped (not processed) {GetEventDataLogInfo(context, _currEventData)}.");
                    _currPoisonAction = PoisonMessageAction.NotPoison;
                    return new FunctionResult(true);
                }
                else if (_currPoisonAction == PoisonMessageAction.Undetermined)
                    throw new InvalidOperationException("The IPoisonMessageOrchestrator.CheckAsync must not return PoisonMessageAction.Undetermined.");
            }

            // Execute the function (maybe again if previously failed).
            var data = new TriggeredFunctionData { TriggerValue = new ResilientEventHubData { EventData = _currEventData } };
            var fr = await _executor.TryExecuteAsync(data, ct);

            // Where we have a failure then checkpoint the last so we will at least restart back at this point.
            if (fr.Succeeded)
            {
                if (_currPoisonAction != PoisonMessageAction.NotPoison)
                {
                    await _poisonOrchestrator.RemoveAsync(_currEventData, PoisonMessageAction.NotPoison);
                    _currPoisonAction = PoisonMessageAction.NotPoison;
                }
            }
            else
                await CheckpointAsync(context, _lastEventData);

            return fr;
        }

        /// <summary>
        /// Enables the Checkpoint functionality to be overridden; provided explicitly for unit testing only.
        /// </summary>
        public Func<PartitionContext, EventHubs.EventData, Task> Checkpointer { get; set; }

        /// <summary>
        /// Perform the checkpoint.
        /// </summary>
        private async Task CheckpointAsync(PartitionContext context, EventHubs.EventData @event)
        {
            // Make sure the checkpoint is resulting in a change; avoid the storage perf cost.
            if (@event == null || (_lastCheckpoint != null && @event == _lastCheckpoint))
                return;

            _lastCheckpoint = @event;
            _lastEventData = null;

            if (Checkpointer == null)
                await context.CheckpointAsync(_lastCheckpoint);
            else
                await Checkpointer(context, @event);
        }
    }
}