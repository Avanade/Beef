// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Events;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Data.Database.Cdc
{
    /// <summary>
    /// Manages the Change Data Capture (CDC) execution.
    /// </summary>
    /// <typeparam name="TCdcModel">The CDC database model <see cref="Type"/>.</typeparam>
    public abstract class CdcExecutor<TCdcModel> where TCdcModel : class, ICdcModel, new()
    {
        private const string EnvelopeIdParamName = "EnvelopeIdToMarkComplete";
        private const string IncompleteBatchesParamName = "ReturnIncompleteBatches";
        private const string BatchSizeParamName = "MaxBatchSize";

        /// <summary>
        /// Initializes a new instance of the <see cref="CdcExecutor{TCdcModel}"/> class.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="storedProcedureName">The name of the get outbox data stored procedure.</param>
        /// <param name="mapper">The corresponding CDC database model <see cref="IDatabaseMapper{TCdcModel}"/>.</param>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public CdcExecutor(IDatabase db, string storedProcedureName, IDatabaseMapper<TCdcModel> mapper, IEventPublisher eventPublisher, ILogger logger)
        {
            Db = Check.NotNull(db, nameof(db));
            StoredProcedureName = Check.NotEmpty(storedProcedureName, nameof(storedProcedureName));
            Mapper = Check.NotNull(mapper, nameof(mapper));
            EventPublisher = Check.NotNull(eventPublisher, nameof(eventPublisher));
            Logger = Check.NotNull(logger, nameof(logger));
        }

        /// <summary>
        /// Gets the <see cref="IDatabase"/>.
        /// </summary>
        public IDatabase Db { get; private set; }

        /// <summary>
        /// Gets the name of the get outbox data stored procedure.
        /// </summary>
        public string StoredProcedureName { get; private set; }

        /// <summary>
        /// Gets the corresponding CDC database model mapper.
        /// </summary>
        public IDatabaseMapper<TCdcModel> Mapper { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEventPublisher"/>.
        /// </summary>
        public IEventPublisher EventPublisher { get; private set; }

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        public ILogger Logger { get; private set; }

        /// <summary>
        /// Marks an existing envelope as complete.
        /// </summary>
        /// <param name="envelopeId">The envelope identifier.</param>
        public async Task MarkEnvelopeAsCompleteAsync(int envelopeId)
        {
            await Db.StoredProcedure(StoredProcedureName)
                .Param(EnvelopeIdParamName, envelopeId)
                .Param(BatchSizeParamName, System.Data.DbType.Int32, 0)
                .NonQueryAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Executes any previously incomplete batch.
        /// </summary>
        /// <param name="maxBatchSize">The maximum batch size. Defaults to 100.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The corresponding <see cref="CdcOutboxEnvelope"/> where found; otherwise, <c>null</c>.</returns>
        public Task<CdcOutboxEnvelope?> ExecuteIncompleBatchAsync(int maxBatchSize = 100, CancellationToken? cancellationToken = null)
            => ExecuteSelectedBatchAsync("incomplete batch", true, maxBatchSize, cancellationToken ?? CancellationToken.None);

        /// <summary>
        /// Executes the next batch.
        /// </summary>
        /// <param name="maxBatchSize">The maximum batch size. Defaults to 100.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The corresponding <see cref="CdcOutboxEnvelope"/> where found; otherwise, <c>null</c>.</returns>
        public Task<CdcOutboxEnvelope?> ExecuteNextBatchAsync(int maxBatchSize = 100, CancellationToken? cancellationToken = null)
            => ExecuteSelectedBatchAsync("next batch", false, maxBatchSize, cancellationToken ?? CancellationToken.None);

        /// <summary>
        /// Execute selected batch.
        /// </summary>
        private async Task<CdcOutboxEnvelope?> ExecuteSelectedBatchAsync(string text, bool incompleteBatches, int maxBatchSize, CancellationToken cancellationToken)
        { 
            CdcOutboxEnvelope? envelope = null;
            List<TCdcModel>? items = null;

            // Get the next batch.
            Logger.LogInformation($"Query {text} with maximum batch size of {maxBatchSize}.");
            await Db.StoredProcedure(StoredProcedureName)
                .Param(BatchSizeParamName, maxBatchSize)
                .Param(IncompleteBatchesParamName, incompleteBatches)
                .SelectQueryMultiSetAsync(
                    new MultiSetSingleArgs<CdcOutboxEnvelope>(CdcOutboxEnvelopeMapper.Default, r => envelope = r, isMandatory: false, stopOnNull: true),
                    new MultiSetCollArgs<List<TCdcModel>, TCdcModel>(Mapper, r => items = r))
                .ConfigureAwait(false);

            if (envelope == null)
            {
                Logger.LogInformation($"Query did not result in any change data.");
                return null!;
            }

            // Convert the batch into events.
            if (items == null || items.Count == 0)
                Logger.LogInformation($"Envelope '{envelope.Id}' found with no change data.");
            else
            {
                Logger.LogInformation($"Envelope '{envelope.Id}' found.");

                // Group the items in the batch by operation and key - there could be more than one record; i.e. where there is a one-to-many relationship involved.
                var events = new List<EventData>();
                foreach (var grp in items.GroupBy(x => new { x.DatabaseOperationType, x.UniqueKey }).Select(g => g.ToList()))
                {
                    if (cancellationToken.IsCancellationRequested)
                        return await Task.FromCanceled<CdcOutboxEnvelope>(cancellationToken).ConfigureAwait(false);

                    events.AddRange(await CreateEventDataAsync(grp, cancellationToken).ConfigureAwait(false));
                }

                // Publish the events.
                if (cancellationToken.IsCancellationRequested)
                    return await Task.FromCanceled<CdcOutboxEnvelope>(cancellationToken).ConfigureAwait(false);

                await EventPublisher.PublishAsync(events.ToArray()).ConfigureAwait(false);
                Logger.LogInformation($"{events.Count} event(s) were published successfully.");
            }

            // Complete the batch (ignore cancel as event(s) published and we *must* complete to minimise chance of sending more than once).
            await MarkEnvelopeAsCompleteAsync(envelope.Id).ConfigureAwait(false);
            Logger.LogInformation($"Envelope '{envelope.Id}' marked as completed.");

            return envelope;
        }

        /// <summary>
        /// Gets the <see cref="Events.EventActionFormat"/>.
        /// </summary>
        protected virtual EventActionFormat EventActionFormat { get; } = EventActionFormat.None;

        /// <summary>
        /// Creates an <see cref="EventData{T}"/> for the specified <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">The <paramref name="value"/> <see cref="Type"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="subjectPrefix">The <see cref="EventData.Subject"/> prefix.</param>
        /// <param name="operationType">The <see cref="OperationType"/> to infer the <see cref="EventData.Action"/>.</param>
        /// <returns>The <see cref="EventData{T}"/>.</returns>
        protected EventData<T> CreateValueEvent<T>(T value, string subjectPrefix, OperationType operationType) where T : class
            => EventData.CreateValueEvent(value, CreateValueFullyQualifiedSubject(value, subjectPrefix), EventActionFormatter.Format(operationType, EventActionFormat));

        /// <summary>
        /// Creates an <see cref="EventData{T}"/> instance with the specified <paramref name="value"/> and <paramref name="key"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The event value</param>
        /// <param name="subjectPrefix">The <see cref="EventData.Subject"/> prefix.</param>
        /// <param name="operationType">The <see cref="OperationType"/> to infer the <see cref="EventData.Action"/>.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData{T}"/>.</returns>
        protected EventData<T> CreateValueEvent<T>(T value, string subjectPrefix, OperationType operationType, params IComparable?[] key)
            => EventData.CreateValueEvent(value, CreateFullyQualifiedSubject(subjectPrefix, key), EventActionFormatter.Format(operationType, EventActionFormat), key);

        /// <summary>
        /// Creates an <see cref="EventData"/> with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="subjectPrefix">The <see cref="EventData.Subject"/> prefix.</param>
        /// <param name="operationType">The <see cref="OperationType"/> to infer the <see cref="EventData.Action"/>.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        protected EventData CreateEvent(string subjectPrefix, OperationType operationType, params IComparable?[] key)
            => EventData.CreateEvent(CreateFullyQualifiedSubject(subjectPrefix, key), EventActionFormatter.Format(operationType, EventActionFormat), key);

        /// <summary>
        /// Creates a fully qualified event subject by appending the <paramref name="value"/> <i>key</i> to the <paramref name="subjectPrefix"/>.
        /// </summary>
        /// <typeparam name="T">The <paramref name="value"/> <see cref="Type"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="subjectPrefix">The <see cref="EventData.Subject"/> prefix.</param>
        /// <returns>The fully qualified subject.</returns>
        /// <remarks><typeparamref name="T"/> must implement at least one of the following: <see cref="IIdentifier"/>, <see cref="IGuidIdentifier"/>, <see cref="IStringIdentifier"/> or <see cref="IUniqueKey"/>.</remarks>
        protected string CreateValueFullyQualifiedSubject<T>(T value, string subjectPrefix) where T : class
        {
            var sb = new StringBuilder(subjectPrefix + ".");
            switch (value ?? throw new ArgumentNullException(nameof(value)))
            {
                case IIntIdentifier ii:
                    sb.Append(ii.Id);
                    break;

                case IGuidIdentifier gi:
                    sb.Append(gi.Id);
                    break;

                case IStringIdentifier si:
                    sb.Append(si.Id);
                    break;

                case IUniqueKey uk:
                    if (!uk.HasUniqueKey)
                        throw new InvalidOperationException("A Value that implements IUniqueKey must have one; i.e. HasUniqueKey = true.");

                    for (int i = 0; i < uk.UniqueKey.Args.Length; i++)
                    {
                        if (i > 0)
                            sb.Append(",");

                        sb.Append(uk.UniqueKey.Args[i]);
                    }

                    break;

                default:
                    throw new InvalidOperationException("Type must implement at least one of the following: IIdentifier, IGuidIdentifier, IStringIdentifier or IUniqueKey.");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates a fully qualified event subject by appending the <paramref name="key"/> to the <paramref name="subjectPrefix"/>.
        /// </summary>
        /// <param name="subjectPrefix">The <see cref="EventData.Subject"/> prefix.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The fully qualified subject.</returns>
        protected string CreateFullyQualifiedSubject(string subjectPrefix, params IComparable?[] key)
        {
            var sb = new StringBuilder(subjectPrefix + ".");
            if (key == null || key.Length == 0)
                throw new ArgumentException("There must be at least a single key value specified.", nameof(key));

            for (int i = 0; i < key.Length; i++)
            {
                if (i > 0)
                    sb.Append(",");

                sb.Append(key[i]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates none or more <see cref="EventData">events</see> from the model <paramref name="coll">collection (a model may have one or more records due to one-to-many database relationships).</paramref>.
        /// </summary>
        /// <param name="coll">The model collection.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>None or more <see cref="EventData">events</see> to be published.</returns>
        protected abstract Task<IEnumerable<EventData>> CreateEventDataAsync(List<TCdcModel> coll, CancellationToken? cancellationToken = null);
    }
}