// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events.Repository;
using Microsoft.Azure.Cosmos.Table;
using System.Text;
using System.Threading.Tasks;

namespace Beef.Events.EventHubs
{
    /// <summary>
    /// Provides the <b>Event Hubs</b> <see cref="AzureStorageRepository{TData, TAudit}"/>.
    /// </summary>
    public class EventHubAzureStorageRepository : AzureStorageRepository<EventHubData, EventHubAuditRecord>, IEventHubStorageRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EventHubAzureStorageRepository"/> class.
        /// </summary>
        /// <param name="storageConnectionString">The <b>Cosmos Table</b> connection string.</param>
        /// <param name="auditTableName">The <b>Audit</b> storage <see cref="CloudTable"/> name (defaults to 'EventHubAuditMessages').</param>
        /// <param name="poisonTableName">The <b>Poison</b> storage <see cref="CloudTable"/> name (defaults to 'EventHubPoisonMessages').</param>
        public EventHubAzureStorageRepository(string storageConnectionString, string auditTableName = "EventHubAuditMessages", string poisonTableName = "EventHubPoisonMessages")
           : base(storageConnectionString, auditTableName, poisonTableName) { }

        /// <summary>
        /// Create the partition key.
        /// </summary>
        /// <param name="data">The <see cref="EventHubData"/>.</param>
        /// <returns>The partition key.</returns>
        public override string CreatePartitionKey(EventHubData data) => data.EventHubName + "-" + data.ConsumerGroupName;

        /// <summary>
        /// Create the row key.
        /// </summary>
        /// <param name="data">The <see cref="EventHubData"/>.</param>
        /// <returns>The row key.</returns>
        public override string CreateRowKey(EventHubData data) => data.PartitionId;

        /// <summary>
        /// Creates the <see cref="EventHubAuditRecord"/> from the event <paramref name="data"/> <paramref name="result"/>.
        /// </summary>
        /// <param name="data">The event data.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        /// <returns>The audit record.</returns>
        protected override EventHubAuditRecord CreateAuditRecord(EventHubData data, Result result) =>
            new EventHubAuditRecord(CreatePartitionKey(data), CreateRowKey(data))
            {
                EventHubName = data.EventHubName,
                ConsumerGroupName = data.ConsumerGroupName,
                PartitionId = data.PartitionId,
                Offset = data.Originating.SystemProperties.Offset,
                SequenceNumber = data.Originating.SystemProperties.SequenceNumber,
                EnqueuedTimeUtc = data.Originating.SystemProperties.EnqueuedTimeUtc,
                EventId = data.Metadata.EventId,
                Attempts = data.Attempt <= 0 ? 1 : data.Attempt,
                Subject = result.Subject,
                Action = result.Action,
                Reason = result.Reason,
                Status = result.Status.ToString(),
                Body = TruncateText(Encoding.UTF8.GetString(data.Originating.Body)),
                Exception = TruncateText(result.Exception?.ToString()),
            };

        /// <summary>
        /// Checks whether the poisoned event exists with a different sequence number and recalibrates accordingly.
        /// </summary>
        /// <param name="data">The event data.</param>
        /// <param name="audit">The current audit record.</param>
        /// <returns>The <see cref="PoisonMessageAction"/> and number of previous attempts.</returns>
        protected override async Task<(PoisonMessageAction Action, int Attempts)> CheckPoisonedAdditionalAsync(EventHubData data, EventHubAuditRecord audit)
        {
            // Where the message (event) exists with a different sequence number - this means things are slightly out of whack! Remove, audit and assume not poison.
            if (data.Originating.SystemProperties.SequenceNumber != audit.SequenceNumber)
            {
                var reason = $"Current EventData (Seq#: '{data.Originating.SystemProperties.SequenceNumber}' Offset#: '{data.Originating.SystemProperties.Offset}') being processed is out of sync with previous Poison (Seq#: '{audit.SequenceNumber}' Offset#: '{audit.Offset}'); current assumed correct with previous Poison now deleted.";
                var result = EventSubscriberHost.CreatePoisonMismatchResult(audit.Subject, audit.Action, reason);
                await WriteAuditAsync(audit, result, null).ConfigureAwait(false);
                var pt = await GetPoisonMessageTableAsync().ConfigureAwait(false);
                await RemovePoisonedAsync(pt, audit).ConfigureAwait(false);
                await LoggerAuditWriter.WriteFormattedAuditAsync(Logger, data, result).ConfigureAwait(false);
                return (PoisonMessageAction.NotPoison, 0);
            }

            return await base.CheckPoisonedAdditionalAsync(data, audit).ConfigureAwait(false);
        }
    }
}