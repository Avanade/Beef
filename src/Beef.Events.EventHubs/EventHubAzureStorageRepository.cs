// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AzureEventHubs = Azure.Messaging.EventHubs;

namespace Beef.Events.EventHubs
{
    /// <summary>
    /// Provides the <see cref="AzureEventHubs.EventData"/> <b>Azure Storage</b> repository.
    /// </summary>
    /// <remarks>Also provides the underlying <see cref="IAuditWriter"/> capability to audit directly to the <b>Azure Storage</b> repository.</remarks>
    public class EventHubAzureStorageRepository : IEventHubStorageRepository, IUseLogger
    {
        private readonly string _storageConnectionString;
        private CloudTable? _auditTable;
        private CloudTable? _poisonTable;
        private ILogger? _logger;

        /// <summary>
        /// Create the partition key.
        /// </summary>
        /// <param name="eventData">The <see cref="EventHubData"/>.</param>
        /// <returns>The partition key.</returns>
        public static string CreatePartitionKey(EventHubData eventData) => CreatePartitionKey((eventData ?? throw new ArgumentNullException(nameof(eventData))).EventHubName, eventData.ConsumerGroupName);

        /// <summary>
        /// Create the partition key.
        /// </summary>
        /// <param name="eventHubPath">The event hubs name.</param>
        /// <param name="consumerGroupName">The consumer group name.</param>
        /// <returns>The partition key.</returns>
        public static string CreatePartitionKey(string eventHubPath, string consumerGroupName) => Check.NotNull(eventHubPath, nameof(eventHubPath)) + "-" + Check.NotNull(consumerGroupName, nameof(consumerGroupName));

        /// <summary>
        /// Create the row key.
        /// </summary>
        /// <param name="eventData">The <see cref="EventHubData"/>.</param>
        /// <returns>The row key.</returns>
        public static string CreateRowKey(EventHubData eventData) => CreateRowKey((eventData ?? throw new ArgumentNullException(nameof(eventData))).PartitionId);

        /// <summary>
        /// Create the row key.
        /// </summary>
        /// <param name="partitionId">The partition identifier.</param>
        /// <returns>The row key.</returns>
        public static string CreateRowKey(string partitionId) => partitionId;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubAzureStorageRepository"/> class.
        /// </summary>
        /// <param name="storageConnectionString">The Azure storage connection string.</param>
        public EventHubAzureStorageRepository(string storageConnectionString) 
            => _storageConnectionString = string.IsNullOrEmpty(storageConnectionString) ? throw new ArgumentNullException(nameof(storageConnectionString)) : storageConnectionString;

        /// <summary>
        /// Gets or sets the <b>Audit</b> storage <see cref="CloudTable"/> name; defaults to "EventHubPoisonMessages". 
        /// </summary>
        public string AuditTableName { get; set; } = "EventHubAuditMessages";

        /// <summary>
        /// Gets or sets the <b>Poison</b> storage <see cref="CloudTable"/> name; defaults to "EventHubPoisonMessages". 
        /// </summary>
        public string PoisonTableName { get; set; } = "EventHubPoisonMessages";

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        public ILogger Logger { get => _logger ?? throw new InvalidOperationException("The UseLogger method must be invoked to set the Logger before it can be used."); }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="logger"><inheritdoc/></param>
        void IUseLogger.UseLogger(ILogger logger) => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        /// <summary>
        /// Writes the <paramref name="eventData"/> <paramref name="result"/> to the audit repository.
        /// </summary>
        /// <param name="eventData">The <see cref="EventHubData"/>.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        /// <remarks>A corresponding log message for the <i>audit</i> will be written to the <see cref="ILogger"/> using <see cref="LoggerAuditWriter.WriteFormattedAuditAsync(ILogger, object, Result)"/>.</remarks>
        public async Task WriteAuditAsync(EventHubData eventData, Result result)
        {
            var audit = CreateEventAuditRecord(Check.NotNull(eventData, nameof(eventData)), Check.NotNull(result, nameof(result)));
            await WriteAuditAsync(audit, null, null).ConfigureAwait(false);
            await LoggerAuditWriter.WriteFormattedAuditAsync(Logger, eventData, result).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the <see cref="EventHubAuditRecord"/> to the audit repository.
        /// </summary>
        private async Task WriteAuditAsync(EventHubAuditRecord audit, Result? overrideResult, int? overrideAttempt)
        {
            var prevRowKey = audit.RowKey;
            OverrideEventAuditRecordResult(audit, overrideResult, overrideAttempt);

            while (true)
            {
                // Insert the audit message table.
                var at = await GetAuditMessageTableAsync().ConfigureAwait(false);
                audit.RowKey = DateTime.UtcNow.ToString("o", System.Globalization.CultureInfo.InvariantCulture) + "-" + audit.RowKey;
                var r = await at.ExecuteAsync(TableOperation.Insert(audit)).ConfigureAwait(false);

                switch ((HttpStatusCode)r.HttpStatusCode)
                {
                    case HttpStatusCode.Conflict:
                        continue; // Try again.

                    case HttpStatusCode.OK:
                    case HttpStatusCode.NoContent:
                    case HttpStatusCode.Accepted:
                        audit.RowKey = prevRowKey;
                        return; // All good, carry on.

                    default:
                        throw new InvalidOperationException($"WriteAuditAsync failed with HttpStatusCode: {r.HttpStatusCode}.");
                }
            }
        }

        /// <summary>
        /// Override result where specified.
        /// </summary>
        private static void OverrideEventAuditRecordResult(EventHubAuditRecord audit, Result? overrideResult, int? overrideAttempt)
        {
            if (overrideResult != null)
            {
                if (overrideResult.Status == SubscriberStatus.PoisonSkipped || overrideResult.Status == SubscriberStatus.PoisonMismatch)
                {
                    audit.OriginatingReason = audit.Reason;
                    audit.OriginatingStatus = audit.Status;
                }
                else
                    audit.Exception = overrideResult.Exception == null ? null : Substring(overrideResult.Exception.ToString());

                audit.Status = overrideResult.Status.ToString();
                audit.Reason = overrideResult.Reason;
            }

            if (overrideAttempt.HasValue)
                audit.Attempts = overrideAttempt.Value;
        }

        /// <summary>
        /// Create (instantiate) the <see cref="EventHubAuditRecord"/>.
        /// </summary>
        private static EventHubAuditRecord CreateEventAuditRecord(EventHubData eventdata, Result result)
        {
            var metadata = EventDataMapper.GetBeefMetadata(eventdata.Event);

            return new EventHubAuditRecord(CreatePartitionKey(eventdata ?? throw new ArgumentNullException(nameof(eventdata))), CreateRowKey(eventdata))
            {
                EventHubName = eventdata.EventHubName,
                ConsumerGroupName = eventdata.ConsumerGroupName,
                PartitionId = eventdata.PartitionId,
                Offset = eventdata.Event.SystemProperties.Offset,
                SequenceNumber = eventdata.Event.SystemProperties.SequenceNumber,
                EnqueuedTimeUtc = eventdata.Event.SystemProperties.EnqueuedTimeUtc,
                EventId = metadata.EventId,
                Attempts = eventdata.Attempt <= 0 ? 1 : eventdata.Attempt,
                Subject = result.Subject,
                Action = result.Action,
                Reason = result.Reason,
                Status = result.Status.ToString(),
                Body = Substring(Encoding.UTF8.GetString(eventdata.Event.Body)),
                Exception = Substring(result.Exception?.ToString()),
            };
        }

        /// <summary>
        /// Substring to a 64K (64,000 char) limit allowed by Azure Storage.
        /// </summary>
        private static string? Substring(string? text) => string.IsNullOrEmpty(text) ? null : (text.Length >= 64000 ? text[0..64000] : text);

        /// <summary>
        /// Checks whether the <paramref name="eventData"/> is considered poison.
        /// </summary>
        /// <param name="eventData">The <see cref="EventHubData"/>.</param>
        /// <returns>The <see cref="PoisonMessageAction"/> and number of previous attempts.</returns>
        public async Task<(PoisonMessageAction Action, int Attempts)> CheckPoisonedAsync(EventHubData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            // Get the poison message table.
            var pt = await GetPoisonMessageTableAsync().ConfigureAwait(false);

            // Get the current poison event (if there is one).
            var cpe = await GetPoisonedAsync(pt, eventData).ConfigureAwait(false);
            if (cpe == null)
                return (PoisonMessageAction.NotPoison, 0);

            // Where the message (event) exists with a different sequence number - this means things are slightly out of whack! Remove, audit and assume not poison.
            if (eventData.Event.SystemProperties.SequenceNumber != cpe.SequenceNumber)
            {
                var reason = $"Current EventData (Seq#: '{eventData.Event.SystemProperties.SequenceNumber}' Offset#: '{eventData.Event.SystemProperties.Offset}') being processed is out of sync with previous Poison (Seq#: '{cpe.SequenceNumber}' Offset#: '{cpe.Offset}'); current assumed correct with previous Poison now deleted.";
                var result = EventSubscriberHost.CreatePoisonMismatchResult(cpe.Subject, cpe.Action, reason);
                await WriteAuditAsync(cpe, result, null).ConfigureAwait(false);
                await RemovePoisonedAsync(pt, cpe).ConfigureAwait(false);
                await LoggerAuditWriter.WriteFormattedAuditAsync(Logger, eventData, result).ConfigureAwait(false);
                return (PoisonMessageAction.NotPoison, 0);
            }

            // Determine action; where skipping remove poison, then audit and carry on.
            if (cpe.SkipProcessing)
            {
                cpe.SkippedTimeUtc = DateTime.UtcNow;
                var result = EventSubscriberHost.CreatePoisonSkippedResult(cpe.Subject, cpe.Action);
                await WriteAuditAsync(cpe, result, null).ConfigureAwait(false);
                await RemovePoisonedAsync(pt, cpe).ConfigureAwait(false);
                await LoggerAuditWriter.WriteFormattedAuditAsync(Logger, eventData, result).ConfigureAwait(false);
                return (PoisonMessageAction.PoisonSkip, cpe.Attempts);
            }
            else
                return (PoisonMessageAction.PoisonRetry, cpe.Attempts);
        }

        /// <summary>
        /// Gets the poisoned <see cref="EventHubAuditRecord"/>.
        /// </summary>
        private static async Task<EventHubAuditRecord> GetPoisonedAsync(CloudTable poisonTable, EventHubData eventData)
        {
            var r = await poisonTable.ExecuteAsync(TableOperation.Retrieve<EventHubAuditRecord>(CreatePartitionKey(eventData), eventData.PartitionId)).ConfigureAwait(false);
            return (EventHubAuditRecord)r.Result;
        }

        /// <summary>
        /// Removes the poisoned <paramref name="eventData"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="EventHubData"/>.</param>
        public async Task RemovePoisonedAsync(EventHubData eventData)
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            // Get the poison message table.
            var pt = await GetPoisonMessageTableAsync().ConfigureAwait(false);
            var ear = await GetPoisonedAsync(pt, eventData).ConfigureAwait(false);
            if (ear != null)
                await RemovePoisonedAsync(pt, ear).ConfigureAwait(false);
        }

        /// <summary>
        /// Remove the poisoned <see cref="EventHubAuditRecord"/>.
        /// </summary>
        private static async Task RemovePoisonedAsync(CloudTable poisonTable, EventHubAuditRecord ea)
        {
            var r = await poisonTable.ExecuteAsync(TableOperation.Delete(new EventHubAuditRecord(ea.PartitionKey, ea.RowKey) { ETag = "*" })).ConfigureAwait(false);
            _ = r.Result;
        }

        /// <summary>
        /// Marks the <paramref name="eventData"/> with a poisoned <paramref name="result"/>.
        /// </summary>
        /// <param name="eventData">The <see cref="EventHubData"/>.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        /// <param name="maxAttempts">The maximum number of attempts; a <c>null</c> or any non-positive number indicates infinite.</param>
        /// <returns>The resulting <see cref="UnhandledExceptionHandling"/>.</returns>
        public async Task<UnhandledExceptionHandling> MarkAsPoisonedAsync(EventHubData eventData, Result result, int? maxAttempts)
        {
            // Get the poison message table.
            var pt = await GetPoisonMessageTableAsync().ConfigureAwait(false);

            while (true)
            {
                // Get the current poison event (if there is one).
                var ear = await GetPoisonedAsync(pt, eventData ?? throw new ArgumentNullException(nameof(eventData))).ConfigureAwait(false);
                if (ear == null)
                    ear = CreateEventAuditRecord(Check.NotNull(eventData, nameof(eventData)), Check.NotNull(result, nameof(result)));
                else
                    OverrideEventAuditRecordResult(ear, result, ++ear.Attempts);

                if (ear.PoisonedTimeUtc == null)
                    ear.PoisonedTimeUtc = DateTime.UtcNow;

                // Make sure maximum attempts not exceeded where specified.
                var max = result.Subscriber?.MaxAttempts;
                if (!max.HasValue || max <= 0)
                    max = maxAttempts;

                if (max.HasValue && max > 0 && ear.Attempts >= max)
                {
                    var nr = EventSubscriberHost.CreatePoisonMaxAttemptsResult(result, ear.Attempts);
                    OverrideEventAuditRecordResult(ear, nr, null);
                    await WriteAuditAsync(ear, null, null).ConfigureAwait(false);
                    await RemovePoisonedAsync(pt, ear).ConfigureAwait(false);
                    await LoggerAuditWriter.WriteFormattedAuditAsync(Logger, eventData, nr).ConfigureAwait(false);
                    return UnhandledExceptionHandling.Continue;
                }

                // Create/update the poisoned message.
                var r = await pt.ExecuteAsync(TableOperation.InsertOrReplace(ear)).ConfigureAwait(false);
                switch ((HttpStatusCode)r.HttpStatusCode)
                {
                    case HttpStatusCode.PreconditionFailed:
                        continue; // Try again.

                    case HttpStatusCode.OK:
                    case HttpStatusCode.NoContent:
                    case HttpStatusCode.Accepted:
                        return UnhandledExceptionHandling.ThrowException; // All good, carry on.

                    default:
                        throw new InvalidOperationException($"MarkAsPoisonedAsync failed with HttpStatusCode: {r.HttpStatusCode}.");
                }
            }
        }

        /// <summary>
        /// Marks the previously poisoned <paramref name="eventData"/> to skip.
        /// </summary>
        /// <param name="eventData">The <see cref="EventHubData"/>.</param>
        public async Task SkipPoisonedAsync(EventHubData eventData)
        {
            // Get the poison message table.
            var pt = await GetPoisonMessageTableAsync().ConfigureAwait(false);

            while (true)
            {
                // Get the current poison event (if there is one).
                var ear = await GetPoisonedAsync(pt, eventData ?? throw new ArgumentNullException(nameof(eventData))).ConfigureAwait(false);
                if (ear == null || ear.SkipProcessing)
                    return;

                ear.SkipProcessing = true;

                var r = await pt.ExecuteAsync(TableOperation.Replace(ear)).ConfigureAwait(false);
                switch ((HttpStatusCode)r.HttpStatusCode)
                {
                    case HttpStatusCode.PreconditionFailed:
                        continue; // Try again.

                    case HttpStatusCode.OK:
                    case HttpStatusCode.NoContent:
                    case HttpStatusCode.Accepted:
                    case HttpStatusCode.NotFound:
                        return; // All good, carry on.

                    default:
                        throw new InvalidOperationException($"SkipPoisonedAsync failed with HttpStatusCode: {r.HttpStatusCode}.");
                }
            }
        }

        /// <summary>
        /// Gets (creates) the <b>Audit</b> <see cref="CloudTable"/> (table name is <see cref="AuditTableName"/>).
        /// </summary>
        /// <returns>The <see cref="EventHubAuditRecord"/> <see cref="CloudTable"/>.</returns>
        public async Task<CloudTable> GetAuditMessageTableAsync()
        {
            if (_auditTable != null)
                return _auditTable;

            var csa = CloudStorageAccount.Parse(_storageConnectionString);
            var tc = csa.CreateCloudTableClient();
            var t = tc.GetTableReference(AuditTableName);
            await t.CreateIfNotExistsAsync().ConfigureAwait(false);
            return _auditTable ??= t;
        }

        /// <summary>
        /// Gets (creates) the <b>Poison</b> <see cref="CloudTable"/> (table name is <see cref="PoisonTableName"/>).
        /// </summary>
        /// <returns>The <see cref="EventHubAuditRecord"/> <see cref="CloudTable"/>.</returns>
        public async Task<CloudTable> GetPoisonMessageTableAsync()
        {
            if (_poisonTable != null)
                return _poisonTable;

            var csa = CloudStorageAccount.Parse(_storageConnectionString);
            var tc = csa.CreateCloudTableClient();
            var t = tc.GetTableReference(PoisonTableName);
            await t.CreateIfNotExistsAsync().ConfigureAwait(false);
            return _poisonTable ??= t;
        }
    }
}