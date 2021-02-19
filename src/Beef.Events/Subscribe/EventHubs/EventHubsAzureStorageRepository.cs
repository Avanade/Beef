﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

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
    /// <remarks>Also provides the underlying <see cref="IAuditWriter"/> capability to audit directly to the <b>Azure Storage</b> repository.</remarks>
    public class EventHubsAzureStorageRepository : IAuditWriter<AzureEventHubs.EventData>
    {
        private readonly EventHubsRepositoryArgs _args;
        private readonly ILogger _logger;
        private readonly string _storagePartitionKey;
        private CloudTable? _auditTable;
        private CloudTable? _poisonTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubsAzureStorageRepository"/> class.
        /// </summary>
        /// <param name="args">The <see cref="EventHubsRepositoryArgs"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public EventHubsAzureStorageRepository(EventHubsRepositoryArgs args, ILogger logger)
        {
            _args = Check.NotNull(args, nameof(args));
            _logger = Check.NotNull(logger, nameof(logger));
            _storagePartitionKey = $"{_args.EventHubPath}-{_args.EventHubName}-{_args.ConsumerGroup}";
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
        /// Writes the <paramref name="event"/> <paramref name="result"/> to the audit repository.
        /// </summary>
        /// <param name="event">The <see cref="AzureEventHubs.EventData"/>.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        /// <remarks>A corresponding log message for the <i>audit</i> will be written to the <see cref="ILogger"/> using <see cref="LoggerAuditWriter.WriteAuditAsync(ILogger, object, Result)"/>.</remarks>
        public async Task WriteAuditAsync(AzureEventHubs.EventData @event, Result result)
        {
            var audit = CreateEventAuditRecord(Check.NotNull(@event, nameof(@event)), Check.NotNull(result, nameof(result)));
            await WriteAuditAsync(audit, null).ConfigureAwait(false);
            await LoggerAuditWriter.WriteAuditAsync(_logger, @event, result).ConfigureAwait(false);
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
        private static void OverrideEventAuditRecordResult(EventAuditRecord audit, Result? overrideResult)
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
                Status = result.Status.ToString(),
                Body = Substring(Encoding.UTF8.GetString(@event.Body.Array)),
                Exception = Substring(result.Exception?.ToString()),
            };
        }

        /// <summary>
        /// Substring to a 64K (64,000 char) limit allowed by Azure Storage.
        /// </summary>
        private static string? Substring(string? text) => string.IsNullOrEmpty(text) ? null : (text.Length >= 64000 ? text[0..64000] : text);

        /// <summary>
        /// Checks whether the <paramref name="event"/> is considered poison.
        /// </summary>
        /// <param name="event">The <see cref="AzureEventHubs.EventData"/>.</param>
        /// <returns>The <see cref="PoisonMessageAction"/>.</returns>
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
                var reason = $"Current EventData (Seq#: '{@event.SystemProperties.SequenceNumber}' Offset#: '{@event.SystemProperties.Offset}') being processed is out of sync with previous Poison (Seq#: '{cpe.SequenceNumber}' Offset#: '{cpe.Offset}'); current assumed correct with previous Poison now deleted.";
                var result = EventSubscriberHost.CreatePoisonMismatchResult(cpe.Subject, cpe.Action, reason);
                await WriteAuditAsync(cpe, result).ConfigureAwait(false);
                await RemovePoisonedAsync(pt, cpe).ConfigureAwait(false);
                await LoggerAuditWriter.WriteAuditAsync(_logger, @event, result).ConfigureAwait(false);
                return PoisonMessageAction.NotPoison;
            }

            // Determine action; where skipping remove poison, then audit and carry on.
            if (cpe.SkipProcessing)
            {
                cpe.SkippedTimeUtc = DateTime.UtcNow;
                var result = EventSubscriberHost.CreatePoisonSkippedResult(cpe.Subject, cpe.Action);
                await WriteAuditAsync(cpe, result).ConfigureAwait(false);
                await RemovePoisonedAsync(pt, cpe).ConfigureAwait(false);
                await LoggerAuditWriter.WriteAuditAsync(_logger, @event, result).ConfigureAwait(false);
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
        /// Removes the poisoned <paramref name="event"/>.
        /// </summary>
        /// <param name="event">The <see cref="AzureEventHubs.EventData"/>.</param>
        public async Task RemovePoisonedAsync(AzureEventHubs.EventData @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            // Get the poison message table.
            var pt = await GetPoisonMessageTableAsync().ConfigureAwait(false);
            var ear = await GetPoisonedAsync(pt, @event).ConfigureAwait(false);
            if (ear != null)
                await RemovePoisonedAsync(pt, ear).ConfigureAwait(false);
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
        /// Marks the <paramref name="event"/> with a poisoned <paramref name="result"/>.
        /// </summary>
        /// <param name="event">The <see cref="AzureEventHubs.EventData"/>.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
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
                    ear.PoisonedTimeUtc = DateTime.UtcNow;

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
                        throw new InvalidOperationException($"MarkAsPoisonedAsync failed with HttpStatusCode: {r.HttpStatusCode}.");
                }
            }
        }

        /// <summary>
        /// Marks the previously poisoned <paramref name="event"/> to skip.
        /// </summary>
        /// <param name="event">The <see cref="AzureEventHubs.EventData"/>.</param>
        public async Task SkipPoisonedAsync(AzureEventHubs.EventData @event)
        {
            // Get the poison message table.
            var pt = await GetPoisonMessageTableAsync().ConfigureAwait(false);

            while (true)
            {
                // Get the current poison event (if there is one).
                var ear = await GetPoisonedAsync(pt, @event).ConfigureAwait(false);
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
        /// Create the row key (PartitionKey-SequenceNumber).
        /// </summary>
        private static string CreateRowKey(AzureEventHubs.EventData message) => $"{message.SystemProperties.PartitionKey}-{message.SystemProperties.SequenceNumber}";
    }
}