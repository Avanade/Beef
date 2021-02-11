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
    /// Provides an <see cref="AzureEventHubs.EventData"/> <see cref="IEventRepository"/> using <b>Azure Storage</b>.
    /// </summary>
    public class AzureStorageRepository : IEventRepository<AzureEventHubs.EventData>
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
        /// Indicates whether poison event/message support is enabled.
        /// </summary>
        /// <remarks>Where not supported then <b>no</b> poison message management will occur.</remarks>
        public bool IsPoisonSupportEnabled { get; set; }

        /// <summary>
        /// Writes the event/message to the audit repository.
        /// </summary>
        /// <param name="event">The event/message.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        public async Task WriteAuditAsync(AzureEventHubs.EventData @event, Result result)
        {
            var audit = CreateEventAudit(Check.NotNull(@event, nameof(@event)), Check.NotNull(result, nameof(result)));
            await WriteAuditAsync(audit, null).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the <see cref="EventAudit"/> to the audit repository.
        /// </summary>
        private async Task WriteAuditAsync(EventAudit audit, Result? overrideResult)
        {
            // Override result where specified.
            if (overrideResult != null)
            {
                audit.Reason = overrideResult.Reason;
                if (overrideResult.Exception != null)
                    audit.Exception = overrideResult.Exception.ToString()[0..64000]; //  Substring to a 64K (64,000 char) limit allowed by Azure Storage.
            }

            var prevRowKey = audit.RowKey;

            while (true)
            {
                // Get the high-precision audit time.
                long tickCount = System.Diagnostics.Stopwatch.GetTimestamp();
                var highResDateTime = new DateTime(tickCount);

                // Insert the audit message table.
                var at = await GetAuditMessageTableAsync().ConfigureAwait(false);
                audit.RowKey = new DateTime(tickCount).ToString("o", System.Globalization.CultureInfo.InvariantCulture) + "-" + audit.RowKey;
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
        /// Create (instantiate) the <see cref="EventAudit"/>.
        /// </summary>
        private EventAudit CreateEventAudit(AzureEventHubs.EventData @event, Result result)
        {
            return new EventAudit(_storagePartitionKey, CreateRowKey(@event))
            {
                Offset = @event.SystemProperties.Offset,
                SequenceNumber = @event.SystemProperties.SequenceNumber,
                EnqueuedTimeUtc = @event.SystemProperties.EnqueuedTimeUtc,
                Subject = result.Subject,
                Action = result.Action,
                Reason = result.Reason,
                Body = Encoding.UTF8.GetString(@event.Body.Array)[0..64000], //  Substring to a 64K (64,000 char) limit allowed by Azure Storage.
                Exception = result.Exception?.ToString()[0..64000] //  Substring to a 64K (64,000 char) limit allowed by Azure Storage.
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
        /// Gets the poisoned <see cref="EventAudit"/>.
        /// </summary>
        private async Task<EventAudit> GetPoisonedAsync(CloudTable poisonTable, AzureEventHubs.EventData @event)
        {
            var r = await poisonTable.ExecuteAsync(TableOperation.Retrieve<EventAudit>(_storagePartitionKey, CreateRowKey(@event))).ConfigureAwait(false);
            return (EventAudit)r.Result;
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
        /// Remove the poisoned <see cref="EventAudit"/>.
        /// </summary>
        private async Task RemovePoisonedAsync(CloudTable poisonTable, EventAudit ea)
        {
            var r = await poisonTable.ExecuteAsync(TableOperation.Delete(new EventAudit(_storagePartitionKey, ea.RowKey) { ETag = "*" })).ConfigureAwait(false);
            _ = r.Result;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="event"><inheritdoc/></param>
        /// <param name="result"><inheritdoc/></param>
        /// <returns><inheritdoc/></returns>
        public async Task<PoisonMessageAction> MarkAsPoisonedAsync(AzureEventHubs.EventData @event, Result result)
        {
            var audit = CreateEventAudit(Check.NotNull(@event, nameof(@event)), Check.NotNull(result, nameof(result)));
        }

        /// <summary>
        /// Gets (creates) the <b>Audit</b> <see cref="CloudTable"/> (table name is <see cref="AuditTableName"/>).
        /// </summary>
        /// <returns>The <see cref="EventAudit"/> <see cref="CloudTable"/>.</returns>
        public async Task<CloudTable> GetAuditMessageTableAsync()
        {
            if (_auditTable != null)
                return _auditTable;

            var csa = CloudStorageAccount.Parse(_args.StorageConnectionString));
            var tc = csa.CreateCloudTableClient();
            var t = tc.GetTableReference(AuditTableName);
            await t.CreateIfNotExistsAsync().ConfigureAwait(false);
            return _auditTable ??= t;
        }

        /// <summary>
        /// Gets (creates) the <b>Poison</b> <see cref="CloudTable"/> (table name is <see cref="PoisonTableName"/>).
        /// </summary>
        /// <returns>The <see cref="EventAudit"/> <see cref="CloudTable"/>.</returns>
        public async Task<CloudTable> GetPoisonMessageTableAsync()
        {
            if (!IsPoisonSupportEnabled)
                throw new NotSupportedException("IsPoisonSupportEnabled is set to false; as such any Poison-related operation is not allowed.");

            if (_poisonTable != null)
                return _poisonTable;

            var csa = CloudStorageAccount.Parse(_args.StorageConnectionString));
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