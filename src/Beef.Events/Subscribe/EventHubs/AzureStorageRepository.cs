// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AzureEventHubs = Microsoft.Azure.EventHubs;

namespace Beef.Events.Subscribe.EventHubs
{
    /// <summary>
    /// Provides the <see cref="AzureEventHubs.EventData"/> <b>Azure Storage</b> repository.
    /// </summary>
    public class AzureStorageRepository
    {
        private readonly EventHubsRepositoryArgs _args;
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        private readonly string _storagePartitionKey;
        private CloudTable? _auditTable;
        private CloudTable? _poisonTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageRepository"/> class.
        /// </summary>
        /// <param name="args">The <see cref="EventHubsRepositoryArgs"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public AzureStorageRepository(EventHubsRepositoryArgs args, IConfiguration config, ILogger<AzureStorageRepository> logger)
        {
            _args = Check.NotNull(args, nameof(args));
            _config = Check.NotNull(config, nameof(config));
            _logger = Check.NotNull(logger, nameof(logger));
            _storagePartitionKey = $"{_args.EventHubPath}-{_args.EventHubName}";
        }

        /// <summary>
        /// Gets or sets the <b>Audit</b> storage <see cref="CloudTable"/> name; defaults to "EventHubPoisonMessages". 
        /// </summary>
        public string AuditTableName { get; set; } = "EventHubAuditMessages";

        /// <summary>
        /// Gets or sets the <b>Poison</b> storage <see cref="CloudTable"/> name; defaults to "EventHubPoisonMessages". 
        /// </summary>
        public string PoisonTableName { get; set; } = "EventHubPoisonMessages";

        /// <summary>
        /// Writes the event to the audit repository.
        /// </summary>
        /// <param name="event">The event.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        public async Task WriteAuditAsync(AzureEventHubs.EventData @event, Result result)
        {
            var audit = CreateEventAuditRecord(Check.NotNull(@event, nameof(@event)), Check.NotNull(result, nameof(result)));
            await WriteAuditAsync(audit, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the <see cref="EventAuditRecord"/> to the audit repository.
        /// </summary>
        private async Task WriteAuditAsync(EventAuditRecord audit, Result? overrideResult)
        {
            var prevRowKey = audit.RowKey;
            OverrideEventAuditRecordResult(audit, overrideResult);

            while (true)
            {
                // Insert the audit message table.
                var at = await GetAuditMessageTableAsync().ConfigureAwait(false);
                audit.RowKey = GetTimestamp().ToString("o", System.Globalization.CultureInfo.InvariantCulture) + "-" + audit.RowKey;
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
        /// Gets the timestamp.
        /// </summary>
        private static DateTime GetTimestamp()
        {
            long tickCount = System.Diagnostics.Stopwatch.GetTimestamp();
            return new DateTime(tickCount);
        }

        /// <summary>
        /// Override result where specified.
        /// </summary>
        private static void OverrideEventAuditRecordResult(EventAuditRecord audit, Result? overrideResult)
        {
            if (overrideResult != null)
            {
                audit.Reason = overrideResult.Reason;
                if (overrideResult.Exception != null)
                    audit.Exception = overrideResult.Exception.ToString()[0..64000]; //  Substring to a 64K (64,000 char) limit allowed by Azure Storage.
            }
        }

        /// <summary>
        /// Create (instantiate) the <see cref="EventAuditRecord"/>.
        /// </summary>
        private EventAuditRecord CreateEventAuditRecord(AzureEventHubs.EventData @event, Result result)
        {
            return new EventAuditRecord(_storagePartitionKey, CreateRowKey(@event))
            {
                Offset = @event.SystemProperties.Offset,
                SequenceNumber = @event.SystemProperties.SequenceNumber,
                EnqueuedTimeUtc = @event.SystemProperties.EnqueuedTimeUtc,
                Subject = result.Subject,
                Action = result.Action,
                Reason = result.Reason,
                Body = Encoding.UTF8.GetString(@event.Body.Array)[0..64000], // Substring to a 64K (64,000 char) limit allowed by Azure Storage.
                Exception = result.Exception?.ToString()[0..64000] // Substring to a 64K (64,000 char) limit allowed by Azure Storage.
            };
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="event"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public async Task<PoisonMessageAction> CheckPoisonedAsync(AzureEventHubs.EventData @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            // Get the poison message table.
            var pt = await GetPoisonMessageTableAsync().ConfigureAwait(false);

            // Get the current poison event (if there is one).
            var cpe = await GetPoisonedAsync(pt, @event).ConfigureAwait(false);
            if (cpe == null)
                return PoisonMessageAction.NotPoison;

            // Where the message (event) exists with a different sequence number - this means things are slightly out of whack! Remove, audit and assume not poison.
            if (@event.SystemProperties.SequenceNumber != cpe.SequenceNumber)
            {
                var reason = $"EventData (Seq#: '{@event.SystemProperties.SequenceNumber}' Offset#: '{@event.SystemProperties.Offset}') being processed is out of sync with persisted Poison Message (Seq#: '{cpe.SequenceNumber}' Offset#: '{cpe.Offset}'); EventData assumed correct and this Poison Message deleted.";
                await WriteAuditAsync(cpe, Result.PoisonMismatch(reason)).ConfigureAwait(false);
                await RemovePoisonedAsync(pt, cpe).ConfigureAwait(false);
                return PoisonMessageAction.NotPoison;
            }

            // Determine action; where skipping remove poison, then audit and carry on.
            if (cpe.SkipMessage)
            {
                await RemovePoisonedAsync(pt, cpe).ConfigureAwait(false);
                await WriteAuditAsync(@event, Result.PoisonSkipped()).ConfigureAwait(false);
                return PoisonMessageAction.PoisonSkip;
            }
            else
                return PoisonMessageAction.PoisonRetry;
        }

        /// <summary>
        /// Gets the poisoned <see cref="EventAuditRecord"/>.
        /// </summary>
        private async Task<EventAuditRecord> GetPoisonedAsync(CloudTable poisonTable, AzureEventHubs.EventData @event)
        {
            var r = await poisonTable.ExecuteAsync(TableOperation.Retrieve<EventAuditRecord>(_storagePartitionKey, CreateRowKey(@event))).ConfigureAwait(false);
            return (EventAuditRecord)r.Result;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="event"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public Task RemovePoisonedAsync(AzureEventHubs.EventData @event)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Remove the poisoned <see cref="EventAuditRecord"/>.
        /// </summary>
        private async Task RemovePoisonedAsync(CloudTable poisonTable, EventAuditRecord ea)
        {
            var r = await poisonTable.ExecuteAsync(TableOperation.Delete(new EventAuditRecord(_storagePartitionKey, ea.RowKey) { ETag = "*" })).ConfigureAwait(false);
            _ = r.Result;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="event"><inheritdoc/></param>
        /// <param name="result"><inheritdoc/></param>
        public async Task MarkAsPoisonedAsync(AzureEventHubs.EventData @event, Result result)
        {
            // Get the poison message table.
            var pt = await GetPoisonMessageTableAsync().ConfigureAwait(false);

            while (true)
            {
                // Get the current poison event (if there is one).
                var ear = await GetPoisonedAsync(pt, @event).ConfigureAwait(false);
                if (ear == null)
                    ear = CreateEventAuditRecord(Check.NotNull(@event, nameof(@event)), Check.NotNull(result, nameof(result)));
                else
                {
                    OverrideEventAuditRecordResult(ear, result);
                    ear.Retries++;
                }

                if (ear.PoisonedTimeUtc == null)
                    ear.PoisonedTimeUtc = GetTimestamp();

                var r = await pt.ExecuteAsync(TableOperation.InsertOrReplace(ear)).ConfigureAwait(false);
                switch ((HttpStatusCode)r.HttpStatusCode)
                {
                    case HttpStatusCode.PreconditionFailed:
                        continue; // Try again.

                    case HttpStatusCode.OK:
                    case HttpStatusCode.NoContent:
                    case HttpStatusCode.Accepted:
                        return; // All good, carry on.

                    default:
                        throw new InvalidOperationException($"WriteAuditAsync failed with HttpStatusCode: {r.HttpStatusCode}.");
                }
            }
        }

        /// <summary>
        /// Gets (creates) the <b>Audit</b> <see cref="CloudTable"/> (table name is <see cref="AuditTableName"/>).
        /// </summary>
        /// <returns>The <see cref="EventAuditRecord"/> <see cref="CloudTable"/>.</returns>
        public async Task<CloudTable> GetAuditMessageTableAsync()
        {
            if (_auditTable != null)
                return _auditTable;

            var csa = CloudStorageAccount.Parse(_args.StorageConnectionString);
            var tc = csa.CreateCloudTableClient();
            var t = tc.GetTableReference(AuditTableName);
            await t.CreateIfNotExistsAsync().ConfigureAwait(false);
            return _auditTable ??= t;
        }

        /// <summary>
        /// Gets (creates) the <b>Poison</b> <see cref="CloudTable"/> (table name is <see cref="PoisonTableName"/>).
        /// </summary>
        /// <returns>The <see cref="EventAuditRecord"/> <see cref="CloudTable"/>.</returns>
        public async Task<CloudTable> GetPoisonMessageTableAsync()
        {
            if (_poisonTable != null)
                return _poisonTable;

            var csa = CloudStorageAccount.Parse(_args.StorageConnectionString);
            var tc = csa.CreateCloudTableClient();
            var t = tc.GetTableReference(PoisonTableName);
            await t.CreateIfNotExistsAsync().ConfigureAwait(false);
            return _poisonTable ??= t;
        }

        /// <summary>
        /// Create the row key (ConsumerGroup-PartitionKey).
        /// </summary>
        private string CreateRowKey(AzureEventHubs.EventData message) => $"{_args.ConsumerGroup}-{message.SystemProperties.PartitionKey}";
    }
}