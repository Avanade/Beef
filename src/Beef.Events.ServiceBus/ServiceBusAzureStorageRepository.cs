// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events.Repository;
using Microsoft.Azure.Cosmos.Table;
using System.Text;

namespace Beef.Events.ServiceBus
{
    /// <summary>
    /// Provides the <b>Service Bus</b> <see cref="AzureStorageRepository{TData, TAudit}"/>.
    /// </summary>
    public class ServiceBusAzureStorageRepository : AzureStorageRepository<ServiceBusData, ServiceBusAuditRecord>, IServiceBusStorageRepository
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusAzureStorageRepository"/> class.
        /// </summary>
        /// <param name="storageConnectionString">The <b>Cosmos Table</b> connection string.</param>
        /// <param name="auditTableName">The <b>Audit</b> storage <see cref="CloudTable"/> name (defaults to 'ServiceBusAuditMessages').</param>
        /// <param name="poisonTableName">The <b>Poison</b> storage <see cref="CloudTable"/> name (defaults to 'ServiceBusPoisonMessages').</param>
        public ServiceBusAzureStorageRepository(string storageConnectionString, string auditTableName = "ServiceBusAuditMessages", string poisonTableName = "ServiceBusPoisonMessages")
           : base(storageConnectionString, auditTableName, poisonTableName) { }

        /// <summary>
        /// Create the partition key.
        /// </summary>
        /// <param name="data">The <see cref="ServiceBusData"/>.</param>
        /// <returns>The partition key.</returns>
        public override string CreatePartitionKey(ServiceBusData data) => data.IsTopic 
            ? data.ServiceBusName + "-" + data.QueueName + "-" + data.SubscriptionName 
            : data.ServiceBusName + "-" + data.QueueName;

        /// <summary>
        /// Create the row key.
        /// </summary>
        /// <param name="data">The <see cref="ServiceBusData"/>.</param>
        /// <returns>The row key.</returns>
        public override string CreateRowKey(ServiceBusData data) => data.Originating.SequenceNumber.ToString("000000000000000000#", System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>
        /// Creates the <see cref="ServiceBusAuditRecord"/> from the event <paramref name="data"/> <paramref name="result"/>.
        /// </summary>
        /// <param name="data">The event data.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        /// <returns>The audit record.</returns>
        protected override ServiceBusAuditRecord CreateAuditRecord(ServiceBusData data, Result result) =>
            new ServiceBusAuditRecord(CreatePartitionKey(data), CreateRowKey(data))
            {
                ServiceBusName = data.ServiceBusName,
                QueueName = data.QueueName,
                SubscriptionName = data.SubscriptionName,
                SequenceNumber = data.Originating.SequenceNumber,
                EnqueuedTimeUtc = data.Originating.EnqueuedTime,
                EventId = data.Metadata.EventId,
                Attempts = data.Attempt <= 0 ? 1 : data.Attempt,
                Subject = result.Subject,
                Action = result.Action,
                Reason = result.Reason,
                Status = result.Status.ToString(),
                Body = TruncateText(Encoding.UTF8.GetString(data.Originating.Body)),
                Exception = TruncateText(result.Exception?.ToString()),
            };
    }
}