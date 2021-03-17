// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Events.Repository
{
    /// <summary>
    /// Enables the <see cref="CloudTable"/> storage repository base capabilities.
    /// </summary>
    /// <typeparam name="TData">The event data <see cref="Type"/>.</typeparam>
    /// <typeparam name="TAudit">The audit record <see cref="Type"/>.</typeparam>
    /// <remarks>Also provides the underlying <see cref="IAuditWriter"/> capability to audit directly to the <see cref="CloudTable"/> storage repository.</remarks>
    public abstract class AzureStorageRepository<TData, TAudit> : IStorageRepository<TData>, IUseLogger where TData : class where TAudit : TableEntity, IAuditRecord, new()
    {
        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        private readonly string _storageConnectionString;
        private CloudTable? _auditTable;
        private CloudTable? _poisonTable;
        private ILogger? _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AzureStorageRepository{TData, TAudit}"/> class.
        /// </summary>
        /// <param name="storageConnectionString">The <b>Cosmos Table</b> connection string.</param>
        /// <param name="auditTableName">The <b>Audit</b> storage <see cref="CloudTable"/> name.</param>
        /// <param name="poisonTableName">The <b>Poison</b> storage <see cref="CloudTable"/> name.</param>
        public AzureStorageRepository(string storageConnectionString, string auditTableName, string poisonTableName)
        {
            _storageConnectionString = Check.NotEmpty(storageConnectionString, nameof(storageConnectionString));
            AuditTableName = Check.NotEmpty(auditTableName, nameof(auditTableName));
            PoisonTableName = Check.NotEmpty(poisonTableName, nameof(poisonTableName));
        }

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
        /// Gets or sets the <b>Audit</b> storage <see cref="CloudTable"/> name.
        /// </summary>
        public string AuditTableName { get; set; }

        /// <summary>
        /// Gets or sets the <b>Poison</b> storage <see cref="CloudTable"/> name.
        /// </summary>
        public string PoisonTableName { get; set; }

        /// <summary>
        /// Gets (creates on first access) the <b>Audit</b> <see cref="CloudTable"/> (table name is <see cref="AuditTableName"/>).
        /// </summary>
        /// <returns>The <see cref="AuditTableName"/> <see cref="CloudTable"/>.</returns>
        public async Task<CloudTable> GetAuditMessageTableAsync()
        {
            if (_auditTable != null)
                return _auditTable;

            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                var csa = CloudStorageAccount.Parse(_storageConnectionString);
                var tc = csa.CreateCloudTableClient();
                var t = tc.GetTableReference(AuditTableName);
                await t.CreateIfNotExistsAsync().ConfigureAwait(false);
                return _auditTable ??= t;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Gets (creates on first access) the <b>Poison</b> <see cref="CloudTable"/> (table name is <see cref="PoisonTableName"/>).
        /// </summary>
        /// <returns>The <see cref="PoisonTableName"/> <see cref="CloudTable"/>.</returns>
        public async Task<CloudTable> GetPoisonMessageTableAsync()
        {
            if (_poisonTable != null)
                return _poisonTable;

            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                var csa = CloudStorageAccount.Parse(_storageConnectionString);
                var tc = csa.CreateCloudTableClient();
                var t = tc.GetTableReference(PoisonTableName);
                await t.CreateIfNotExistsAsync().ConfigureAwait(false);
                return _poisonTable ??= t;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Create the partition key.
        /// </summary>
        /// <param name="data">The event data.</param>
        /// <returns>The partition key.</returns>
        public abstract string CreatePartitionKey(TData data);

        /// <summary>
        /// Create the row key.
        /// </summary>
        /// <param name="data">The event data.</param>
        /// <returns>The row key.</returns>
        public abstract string CreateRowKey(TData data);

        /// <summary>
        /// Create the <typeparamref name="TAudit"/> record from the event <paramref name="data"/> <paramref name="result"/>.
        /// </summary>
        /// <param name="data">The event data.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        /// <returns>The audit record.</returns>
        protected abstract TAudit CreateAuditRecord(TData data, Result result);

        /// <summary>
        /// Writes the event <paramref name="data"/> <paramref name="result"/> to the audit repository.
        /// </summary>
        /// <param name="data">The event data.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        /// <remarks>A corresponding log message for the <i>audit</i> will be written to the <see cref="ILogger"/> using <see cref="LoggerAuditWriter.WriteFormattedAuditAsync(ILogger, object, Result)"/>.</remarks>
        public async Task WriteAuditAsync(TData data, Result result)
        {
            var audit = CreateAuditRecord(Check.NotNull(data, nameof(data)), Check.NotNull(result, nameof(result)));
            if (audit == null)
                throw new InvalidOperationException("The CreateAuditRecord must return an instance.");

            await WriteAuditAsync(audit, null, null).ConfigureAwait(false);
            await LoggerAuditWriter.WriteFormattedAuditAsync(Logger, data, result).ConfigureAwait(false);
        }

        /// <summary>
        /// Writes the <typeparamref name="TAudit"/> to the audit repository (<see cref="AuditTableName"/>).
        /// </summary>
        protected async Task WriteAuditAsync(TAudit audit, Result? overrideResult, int? overrideAttempt)
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
        /// Checks whether the event is considered poison.
        /// </summary>
        /// <param name="data">The event data.</param>
        /// <returns>The <see cref="PoisonMessageAction"/> and number of previous attempts.</returns>
        public async Task<(PoisonMessageAction Action, int Attempts)> CheckPoisonedAsync(TData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // Get the poison message table.
            var pt = await GetPoisonMessageTableAsync().ConfigureAwait(false);

            // Get the current poison event (if there is one).
            var pa = await GetPoisonedAsync(pt, data).ConfigureAwait(false);
            if (pa == null)
                return (PoisonMessageAction.NotPoison, 0);

            // Perform an additional checks as required.
            var r = await CheckPoisonedAdditionalAsync(data, pa).ConfigureAwait(false);
            if (r.Action != PoisonMessageAction.Undetermined)
                return r;

            // Determine action; where skipping remove poison, then audit and carry on.
            if (pa.SkipProcessing)
            {
                pa.SkippedTimeUtc = DateTime.UtcNow;
                var result = EventSubscriberHost.CreatePoisonSkippedResult(pa.Subject, pa.Action);
                await WriteAuditAsync(pa, result, null).ConfigureAwait(false);
                await RemovePoisonedAsync(pt, pa).ConfigureAwait(false);
                await LoggerAuditWriter.WriteFormattedAuditAsync(Logger, data, result).ConfigureAwait(false);
                return (PoisonMessageAction.PoisonSkip, pa.Attempts);
            }
            else
                return (PoisonMessageAction.PoisonRetry, pa.Attempts);
        }

        /// <summary>
        /// Provides an opportunity to perform additional logic when performing a <see cref="CheckPoisonedAsync(TData)"/>.
        /// </summary>
        /// <param name="data">The event data.</param>
        /// <param name="audit">The current audit record.</param>
        /// <returns>The <see cref="PoisonMessageAction"/> and number of previous attempts.</returns>
        /// <remarks>Note to inheritors: where the additional logic does not have a specific response <b>use</b> this base method to return as this will ensure the remainder of the <see cref="CheckPoisonedAsync(TData)"/>
        /// is executed correctly.</remarks>
        protected virtual Task<(PoisonMessageAction Action, int Attempts)> CheckPoisonedAdditionalAsync(TData data, TAudit audit) => Task.FromResult((PoisonMessageAction.Undetermined, 0));

        /// <summary>
        /// Gets the poisoned <typeparamref name="TAudit"/>.
        /// </summary>
        protected async Task<TAudit> GetPoisonedAsync(CloudTable poisonTable, TData data)
        {
            var r = await poisonTable.ExecuteAsync(TableOperation.Retrieve<TAudit>(CreatePartitionKey(data), CreateRowKey(data))).ConfigureAwait(false);
            return (TAudit)r.Result;
        }

        /// <summary>
        /// Removes the poisoned event.
        /// </summary>
        /// <param name="data">The event data.</param>
        public async Task RemovePoisonedAsync(TData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            // Get the poison message table.
            var pt = await GetPoisonMessageTableAsync().ConfigureAwait(false);
            var pa = await GetPoisonedAsync(pt, data).ConfigureAwait(false);
            if (pa != null)
                await RemovePoisonedAsync(pt, pa).ConfigureAwait(false);
        }

        /// <summary>
        /// Marks the <paramref name="data"/> with a poisoned <paramref name="result"/>.
        /// </summary>
        /// <param name="data">The event data.</param>
        /// <param name="result">The subscriber <see cref="Result"/>.</param>
        /// <param name="maxAttempts">The maximum number of attempts; a <c>null</c> or any non-positive number indicates infinite.</param>
        /// <returns>The resulting <see cref="UnhandledExceptionHandling"/>.</returns>
        public async Task<UnhandledExceptionHandling> MarkAsPoisonedAsync(TData data, Result result, int? maxAttempts = null)
        {
            // Get the poison message table.
            var pt = await GetPoisonMessageTableAsync().ConfigureAwait(false);

            while (true)
            {
                // Get the current poison event (if there is one).
                var pa = await GetPoisonedAsync(pt, data ?? throw new ArgumentNullException(nameof(data))).ConfigureAwait(false);
                if (pa == null)
                    pa = CreateAuditRecord(Check.NotNull(data, nameof(data)), Check.NotNull(result, nameof(result)));
                else
                    OverrideEventAuditRecordResult(pa, result, ++pa.Attempts);

                if (pa.PoisonedTimeUtc == null)
                    pa.PoisonedTimeUtc = DateTime.UtcNow;

                // Make sure maximum attempts not exceeded where specified.
                var max = result.Subscriber?.MaxAttempts;
                if (!max.HasValue || max <= 0)
                    max = maxAttempts;

                if (max.HasValue && max > 0 && pa.Attempts >= max)
                {
                    var nr = EventSubscriberHost.CreatePoisonMaxAttemptsResult(result, pa.Attempts);
                    OverrideEventAuditRecordResult(pa, nr, null);
                    pa.SkippedTimeUtc = DateTime.UtcNow;

                    await WriteAuditAsync(pa, null, null).ConfigureAwait(false);
                    await RemovePoisonedAsync(pt, pa).ConfigureAwait(false);
                    await LoggerAuditWriter.WriteFormattedAuditAsync(Logger, data, nr).ConfigureAwait(false);
                    return UnhandledExceptionHandling.Continue;
                }

                // Create/update the poisoned message.
                var r = await pt.ExecuteAsync(TableOperation.InsertOrReplace(pa)).ConfigureAwait(false);
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
        /// Marks the previously poisoned <paramref name="data"/> to skip.
        /// </summary>
        /// <param name="data">The event data.</param>
        public async Task SkipPoisonedAsync(TData data)
        {
            // Get the poison message table.
            var pt = await GetPoisonMessageTableAsync().ConfigureAwait(false);

            while (true)
            {
                // Get the current poison event (if there is one).
                var pa = await GetPoisonedAsync(pt, data ?? throw new ArgumentNullException(nameof(data))).ConfigureAwait(false);
                if (pa == null || pa.SkipProcessing)
                    return;

                pa.SkipProcessing = true;

                var r = await pt.ExecuteAsync(TableOperation.Replace(pa)).ConfigureAwait(false);
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
        /// Removes the poisoned event.
        /// </summary>
        protected async Task RemovePoisonedAsync(CloudTable poisonTable, TAudit audit)
        {
            var ta = new TAudit
            {
                PartitionKey = audit.PartitionKey,
                RowKey = audit.RowKey,
                ETag = "*"
            };

            var r = await poisonTable.ExecuteAsync(TableOperation.Delete(ta)).ConfigureAwait(false);
            _ = r.Result;
        }

        /// <summary>
        /// Override result where specified.
        /// </summary>
        private static void OverrideEventAuditRecordResult(TAudit audit, Result? overrideResult, int? overrideAttempt)
        {
            if (overrideResult != null)
            {
                if (overrideResult.Status == SubscriberStatus.PoisonSkipped || overrideResult.Status == SubscriberStatus.PoisonMismatch || overrideResult.Status == SubscriberStatus.PoisonMaxAttempts)
                {
                    audit.OriginatingReason = audit.Reason;
                    audit.OriginatingStatus = audit.Status;
                }
                else
                    audit.Exception = overrideResult.Exception == null ? null : TruncateText(overrideResult.Exception.ToString());

                audit.Status = overrideResult.Status.ToString();
                audit.Reason = overrideResult.Reason;
            }

            if (overrideAttempt.HasValue)
                audit.Attempts = overrideAttempt.Value;
        }

        /// <summary>
        /// Truncate the <paramref name="text"/> to a 64K (64,000 char) limit allowed by Azure.
        /// </summary>
        /// <param name="text">The text to truncate.</param>
        /// <remarks>The truncated text.</remarks>
        protected static string? TruncateText(string? text) => string.IsNullOrEmpty(text) ? null : (text.Length >= 64000 ? text[0..64000] : text);
    }
}