// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Events.Triggers.Listener;
using EventHubs = Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Beef.Events.Triggers.PoisonMessages
{
    /// <summary>
    /// Provides the <see cref="PoisonMessage"/> persistence to Azure table storage. There will be a single instance of this class per Consumer Group/Partition Id whose lifetime is managed within the <see cref="ResilientEventHubProcessor"/>.
    /// </summary>
    public class PoisonMessagePersistence : IPoisonMessagePersistence
    {
        private static Func<PoisonMessageCreatePersistenceArgs, IPoisonMessagePersistence> _create = (a) => new PoisonMessagePersistence(a);

        private readonly PoisonMessageCreatePersistenceArgs _args;
        private readonly CloudTable _poisonTable;
        private readonly CloudTable _skippedTable;
        private readonly string _storagePartitionKey;
        private readonly string _storageRowKey;

        /// <summary>
        /// Register the func to <see cref="Create"/> the <see cref="IPoisonMessagePersistence"/> instance. Defaults to using the <see cref="PoisonMessagePersistence"/> where not specified.
        /// </summary>
        /// <param name="create">The function to create.</param>
        public static void Register(Func<PoisonMessageCreatePersistenceArgs, IPoisonMessagePersistence> create)
        {
            if (create == null)
                _create = (a) => new PoisonMessagePersistence(a);
            else
                _create = create;
        }

        /// <summary>
        /// Creates an instance of the <see cref="IPoisonMessagePersistence"/> using the <see cref="Register">registered</see> function.
        /// </summary>
        /// <param name="args">The <see cref="PoisonMessageCreatePersistenceArgs"/>.</param>
        /// <returns>An <see cref="IPoisonMessagePersistence"/> instance.</returns>
        public static IPoisonMessagePersistence Create(PoisonMessageCreatePersistenceArgs args) => _create(args);

        /// <summary>
        /// Gets or sets the default storage <see cref="CloudTable"/> name; defaults to "EventHubPoisonMessage". 
        /// </summary>
        public static string DefaultTableName { get; set; } = "EventHubPoisonMessage";

        /// <summary>
        /// Gets the default skipped storage <see cref="CloudTable"/> name; named: <see cref="DefaultTableName"/> + "Skipped"; e.g. "EventHubPoisonMessageSkipped". 
        /// </summary>
        public static string DefaultSkippedTableName => DefaultTableName + "Skipped";

        /// <summary>
        /// Gets (creates) the <see cref="PoisonMessage"/> <see cref="CloudTable"/> (table name is <see cref="DefaultTableName"/>).
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The <see cref="PoisonMessage"/> <see cref="CloudTable"/>.</returns>
        public static async Task<CloudTable> GetPoisonMessageTable(string connectionString)
        {
            var csa = CloudStorageAccount.Parse(connectionString ?? throw new ArgumentNullException(nameof(connectionString)));
            var tc = csa.CreateCloudTableClient();
            var t = tc.GetTableReference(DefaultTableName);
            await t.CreateIfNotExistsAsync();
            return t;
        }

        /// <summary>
        /// Gets (creates) the <see cref="PoisonMessage"/> <b>skipped</b> <see cref="CloudTable"/> (table name is <see cref="DefaultSkippedTableName"/>).
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <returns>The <see cref="PoisonMessage"/> <b>skipped</b> <see cref="CloudTable"/>.</returns>
        public static async Task<CloudTable> GetPoisonMessageSkippedTable(string connectionString)
        {
            var csa = CloudStorageAccount.Parse(connectionString ?? throw new ArgumentNullException(nameof(connectionString)));
            var tc = csa.CreateCloudTableClient();
            var t = tc.GetTableReference(DefaultSkippedTableName);
            await t.CreateIfNotExistsAsync();
            return t;
        }

        /// <summary>
        /// Gets all the <see cref="PoisonMessage"/> items currently persisted.
        /// </summary>
        /// <param name="table">The <see cref="CloudTable"/>.</param>
        /// <returns>The <see cref="PoisonMessage"/> items.</returns>
        public static async Task<IEnumerable<PoisonMessage>> GetAllMessagesAsync(CloudTable table)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            var r = await table.ExecuteQuerySegmentedAsync(new TableQuery<PoisonMessage>(), null);
            return r.Results;
        }

        /// <summary>
        /// Updates the specified <see cref="PoisonMessage"/> <see cref="PoisonMessage.SkipMessage"/> to <c>true</c> where found (where not found no error will occur).
        /// </summary>
        /// <param name="table">The <see cref="CloudTable"/>.</param>
        /// <param name="partitionKey">The <see cref="TableEntity.PartitionKey"/>.</param>
        /// <param name="rowKey">The <see cref="TableEntity.RowKey"/>.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        public static async Task SkipMessageAsync(CloudTable table, string partitionKey, string rowKey)
        {
            if (table == null)
                throw new ArgumentNullException(nameof(table));

            while (true)
            {
                var tr = await table.ExecuteAsync(TableOperation.Retrieve<PoisonMessage>(partitionKey, rowKey));
                if (tr.Result != null && tr.Result is PoisonMessage msg && !msg.SkipMessage)
                {
                    msg.SkipMessage = true;
                    var r = await table.ExecuteAsync(TableOperation.Replace(msg));
                    switch ((HttpStatusCode)(r.HttpStatusCode))
                    {
                        case HttpStatusCode.PreconditionFailed:
                            continue; // Try again.

                        case HttpStatusCode.NotFound:
                        case HttpStatusCode.OK:
                        case HttpStatusCode.NoContent:
                            return; // All good, carry on.

                        default:
                            throw new InvalidOperationException($"SkipMessage storage replace operation failed with HttpStatusCode {r.HttpStatusCode}");
                    }
                }

                return;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PoisonMessagePersistence"/> class.
        /// </summary>
        /// <param name="args">The <see cref="PoisonMessageCreatePersistenceArgs"/>.</param>
        public PoisonMessagePersistence(PoisonMessageCreatePersistenceArgs args)
        {
            _args = args ?? throw new ArgumentNullException(nameof(args));
            _storagePartitionKey = $"{_args.Options.EventHubPath}-{_args.Options.EventHubName}";
            _storageRowKey = $"{_args.Context.ConsumerGroupName}-{_args.Context.PartitionId}";

            // Get the connection string and connect to the table storage.
            var cs = args.Config.GetWebJobsConnectionString(ConnectionStringNames.Storage);
            _poisonTable = GetPoisonMessageTable(cs).Result;
            _skippedTable = GetPoisonMessageSkippedTable(cs).Result;
        }

        /// <summary>
        /// Gets the <see cref="PoisonMessage"/>.
        /// </summary>
        private async Task<PoisonMessage> GetPoisonMessageAsync()
        {
            var tr = await _poisonTable.ExecuteAsync(TableOperation.Retrieve<PoisonMessage>(_storagePartitionKey, _storageRowKey));
            return (PoisonMessage)tr.Result;
        }

        /// <summary>
        /// A <i>potential</i> poisoned <see cref="EventHubs.EventData"/> has been identified and needs to be orchestrated. 
        /// </summary>
        /// <param name="event">The corresponding <see cref="EventHubs.EventData"/>.</param>
        /// <param name="exception">The corresponding <see cref="Exception"/>.</param>
        /// <remarks>A <see cref="PoisonMessage"/> will be written to Azure table storage.</remarks>
        public async Task SetAsync(EventHubs.EventData @event, Exception exception)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            var msg = new PoisonMessage(_storagePartitionKey, _storageRowKey)
            {
                Offset = @event.SystemProperties.Offset,
                SequenceNumber = @event.SystemProperties.SequenceNumber,
                EnqueuedTimeUtc = @event.SystemProperties.EnqueuedTimeUtc,
                PoisonedTimeUtc = DateTime.UtcNow,
                FunctionType = _args.Options.FunctionType,
                FunctionName = _args.Options.FunctionName,
                Body = Substring(Encoding.UTF8.GetString(@event.Body.Array)),
                Exception = Substring(exception.ToString())
            };

            await _poisonTable.ExecuteAsync(TableOperation.InsertOrReplace(msg));
        }

        /// <summary>
        /// Substring to a 64K (64,000 char) limit allowed by Azure Storage.
        /// </summary>
        private string Substring(string text) => text.Length >= 64000 ? text.Substring(0, 64000) : text;

        /// <summary>
        /// A previously identified poisoned <see cref="EventHubs.EventData"/> has either successfully processed or can be skipped and should be removed.
        /// </summary>
        /// <param name="event">The corresponding <see cref="EventHubs.EventData"/>.</param>
        /// <param name="action">The corresponding reason (<see cref="PoisonMessageAction"/>) for removal.</param>
        /// <remarks>The corresponding <see cref="PoisonMessage"/> will be removed/deleted from Azure table storage.</remarks>
        public async Task RemoveAsync(EventHubs.EventData @event, PoisonMessageAction action)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            var msg = await GetPoisonMessageAsync();
            if (msg == null)
                return;

            // Audit the skipped record.
            if (action == PoisonMessageAction.PoisonSkip)
            {
                msg.SkippedTimeUtc = DateTime.UtcNow;
                msg.RowKey = msg.SkippedTimeUtc.Value.ToString("o", System.Globalization.CultureInfo.InvariantCulture) + "-" + msg.RowKey;
                await _skippedTable.ExecuteAsync(TableOperation.InsertOrReplace(msg));
            }

            // Remove.
            await _poisonTable.ExecuteAsync(TableOperation.Delete(new PoisonMessage(_storagePartitionKey, _storageRowKey) { ETag = "*" }));
        }

        /// <summary>
        /// Checks whether the <see cref="EventHubs.EventData"/> is in a <b>Poison</b> state and determines the corresponding <see cref="PoisonMessageAction"/>.
        /// </summary>
        /// <param name="event">The corresponding <see cref="EventData"/>.</param>
        /// <returns>The resulting <see cref="PoisonMessageAction"/>.</returns>
        /// <remarks>Reads the <see cref="PoisonMessage"/> in Azure table storage to determine the result. Where <see cref="PoisonMessage.SkipMessage"/> is <c>true</c>, then
        /// <see cref="PoisonMessageAction.PoisonSkip"/> will be returned; otherwise, <see cref="PoisonMessageAction.PoisonRetry"/>.</remarks>
        public async Task<PoisonMessageAction> CheckAsync(EventHubs.EventData @event)
        {
            if (@event == null)
                throw new ArgumentNullException(nameof(@event));

            var msg = await GetPoisonMessageAsync();
            if (msg == null)
                return PoisonMessageAction.NotPoison;

            if (@event.SystemProperties.SequenceNumber != msg.SequenceNumber)
            {
                // Warn if event exists with different offset - this means things are slightly out of whack!
                _args.Logger.LogWarning($"EventData (Seq#: '{@event.SystemProperties.SequenceNumber}') being processed is out of sync with persisted Poison Message (Seq#: '{msg.SequenceNumber}'); EventData assumed correct and Poison Message deleted.");
                await RemoveAsync(@event, PoisonMessageAction.Undetermined);
            }

            if (msg.SkipMessage)
                return PoisonMessageAction.PoisonSkip;
            else
                return PoisonMessageAction.PoisonRetry;
        }
    }
}