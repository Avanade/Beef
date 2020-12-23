﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

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
    /// Provides the core Change Data Capture (CDC) execution capability.
    /// </summary>
    public interface ICdcExecutor
    {
        /// <summary>
        /// Executes the next (new) envelope.
        /// </summary>
        /// <param name="maxBatchSize">The maximum batch size. Defaults to 100.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="CdcExecutorResult"/>.</returns>
        public Task<CdcExecutorResult> ExecuteNextAsync(int maxBatchSize, CancellationToken? cancellationToken);
    }

    /// <summary>
    /// Manages the Change Data Capture (CDC) execution.
    /// </summary>
    /// <typeparam name="TCdcEntity">The CDC database entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TCdcEntityWrapperColl">The <typeparamref name="TCdcEntityWrapper"/> collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TCdcEntityWrapper">The <typeparamref name="TCdcEntity"/> wrapper <see cref="Type"/>.</typeparam>
    public abstract class CdcExecutor<TCdcEntity, TCdcEntityWrapperColl, TCdcEntityWrapper> : ICdcExecutor
        where TCdcEntity : class, IUniqueKey, new() where TCdcEntityWrapperColl : List<TCdcEntityWrapper>, new() where TCdcEntityWrapper : class, TCdcEntity, ICdcOperationType, new()
    {
        private const string EnvelopeIdParamName = "EnvelopeIdToMarkComplete";
        private const string IncompleteBatchesParamName = "ReturnIncompleteBatches";
        private const string BatchSizeParamName = "MaxBatchSize";

        private static readonly DatabaseMapper<CdcEnvelope> _envelopeMapper = DatabaseMapper.CreateAuto<CdcEnvelope>();

        /// <summary>
        /// Initializes a new instance of the <see cref="CdcExecutor{TCdcEntity, TCdcEntityWrapper, TCdcEntityWrapperColl}"/> class.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="storedProcedureName">The name of the get outbox data stored procedure.</param>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public CdcExecutor(IDatabase db, string storedProcedureName, IEventPublisher eventPublisher, ILogger logger)
        {
            Db = Check.NotNull(db, nameof(db));
            StoredProcedureName = Check.NotEmpty(storedProcedureName, nameof(storedProcedureName));
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
        /// Gets the <see cref="IEventPublisher"/>.
        /// </summary>
        public IEventPublisher EventPublisher { get; private set; }

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        public ILogger Logger { get; private set; }

        /// <summary>
        /// Gets the <see cref="EventData.Subject"/>.
        /// </summary>
        protected abstract string EventSubject { get; }

        /// <summary>
        /// Gets the <see cref="Events.EventActionFormat"/>.
        /// </summary>
        protected virtual EventActionFormat EventActionFormat { get; } = EventActionFormat.None;

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
        /// Executes any previously incomplete envelope.
        /// </summary>
        /// <param name="maxBatchSize">The maximum batch size. Defaults to 100.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="CdcExecutorResult{TCdcEntityWrapper, TCdcEntityWrapperColl}"/>.</returns>
        public Task<CdcExecutorResult<TCdcEntityWrapperColl, TCdcEntityWrapper>> ExecuteIncompleteAsync(int maxBatchSize = 100, CancellationToken? cancellationToken = null)
            => ExecuteSelectedBatchAsync("incomplete envelope", true, maxBatchSize, cancellationToken ?? CancellationToken.None);

        /// <summary>
        /// Executes the next (new) envelope.
        /// </summary>
        /// <param name="maxBatchSize">The maximum batch size. Defaults to 100.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="CdcExecutorResult"/>.</returns>
        async Task<CdcExecutorResult> ICdcExecutor.ExecuteNextAsync(int maxBatchSize, CancellationToken? cancellationToken)
            => await ExecuteNextAsync(maxBatchSize < 1 ? 100 : maxBatchSize, cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Executes the next (new) envelope.
        /// </summary>
        /// <param name="maxBatchSize">The maximum batch size. Defaults to 100.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="CdcExecutorResult{TCdcEntityWrapper, TCdcEntityWrapperColl}"/>.</returns>
        public Task<CdcExecutorResult<TCdcEntityWrapperColl, TCdcEntityWrapper>> ExecuteNextAsync(int maxBatchSize = 100, CancellationToken? cancellationToken = null)
            => ExecuteSelectedBatchAsync("next envelope", false, maxBatchSize, cancellationToken ?? CancellationToken.None);

        /// <summary>
        /// Execute selected batch.
        /// </summary>
        private async Task<CdcExecutorResult<TCdcEntityWrapperColl, TCdcEntityWrapper>> ExecuteSelectedBatchAsync(string text, bool incomplete, int maxBatchSize, CancellationToken cancellationToken)
        {
            // Get the envelope data.
            Logger.LogInformation($"Query {text} with maximum batch size of {maxBatchSize}.");
            var result = await GetEnvelopeEntityDataAsync(maxBatchSize, incomplete).ConfigureAwait(false);

            if (result.ReturnCode < 0)
            {
                if (result.Envelope != null)
                    Logger.LogError($"Existing Envelope '{result.Envelope.Id}' created on '{result.Envelope.CreatedDate}' is not complete; this must be completed to continue.");
                else
                    Logger.LogCritical($"Unexpected execution outcome (return code '{result.ReturnCode}') with no incomplete envelope; this is not expected.");

                return result;
            }

            if (result.Envelope == null)
            {
                Logger.LogInformation($"No new change data capture data was found.");
                return result;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                Logger.LogWarning($"Cancellation has resulted in an incomplete envelope.");
                return await Task.FromCanceled<CdcExecutorResult<TCdcEntityWrapperColl, TCdcEntityWrapper>>(cancellationToken).ConfigureAwait(false);
            }

            // Publish the events.
            var events = (await CreateEventsAsync(result.Result, cancellationToken).ConfigureAwait(false)).ToArray();
            await EventPublisher.PublishAsync(events).ConfigureAwait(false);
            Logger.LogInformation($"{events.Length} event(s) were published successfully.");

            // Complete the batch (ignore any further cancel as event(s) published and we *must* complete to minimise chance of sending more than once).
            await MarkEnvelopeAsCompleteAsync(result.Envelope.Id).ConfigureAwait(false);
            Logger.LogInformation($"Envelope '{result.Envelope.Id}' marked as completed.");

            return result;
        }

        /// <summary>
        /// Executes a multi-dataset query command with one or more <paramref name="multiSetArgs"/>; whilst also outputing the resulting return value.
        /// </summary>
        /// <param name="maxBatchSize">The recommended maximum batch size.</param>
        /// <param name="incomplete">Indicates whether to return the last <b>incomplete</b> envelope where <c>true</c>; othewise, <c>false</c> for the next new envelope.</param>
        /// <param name="multiSetArgs">One or more <see cref="IMultiSetArgs"/>.</param>
        /// <returns>The resultant return value and <see cref="CdcEnvelope"/>.</returns>
        protected async Task<CdcExecutorResult<TCdcEntityWrapperColl, TCdcEntityWrapper>> SelectQueryMultiSetAsync(int maxBatchSize, bool incomplete, params IMultiSetArgs[] multiSetArgs)
        {
            var result = new CdcExecutorResult<TCdcEntityWrapperColl, TCdcEntityWrapper>();
            var msa = new List<IMultiSetArgs> { new MultiSetSingleArgs<CdcEnvelope>(_envelopeMapper, r => result.Envelope = r, isMandatory: false, stopOnNull: true) };
            msa.AddRange(multiSetArgs);

            result.ReturnCode = await Db.StoredProcedure(StoredProcedureName)
                .Param(BatchSizeParamName, maxBatchSize)
                .Param(IncompleteBatchesParamName, incomplete)
                .SelectQueryMultiSetWithValueAsync(msa.ToArray())
                .ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Gets the envelope entity data from the database.
        /// </summary>
        /// <param name="maxBatchSize">The recommended maximum batch size.</param>
        /// <param name="incomplete">Indicates whether to return the last <b>incomplete</b> envelope where <c>true</c>; othewise, <c>false</c> for the next new envelope.</param>
        /// <returns>A <typeparamref name="TCdcEntityWrapperColl"/>.</returns>
        protected abstract Task<CdcExecutorResult<TCdcEntityWrapperColl, TCdcEntityWrapper>> GetEnvelopeEntityDataAsync(int maxBatchSize, bool incomplete);

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
        /// Creates none or more <see cref="EventData">events</see> from the entity <paramref name="coll">collection.</paramref>.
        /// </summary>
        /// <param name="coll">The wrapper entity collection.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>None or more <see cref="EventData">events</see> to be published.</returns>
        protected virtual Task<IEnumerable<EventData>> CreateEventsAsync(TCdcEntityWrapperColl coll, CancellationToken? cancellationToken = null)
        {
            var events = new List<EventData>();

            foreach (var item in coll ?? throw new ArgumentNullException(nameof(coll)))
            {
                events.Add(CreateValueEvent(item, EventSubject, item.DatabaseOperationType));
            }

            return Task.FromResult(events.AsEnumerable());
        }
    }

    /// <summary>
    /// Represents the <c>CdcExecutor</c> result.
    /// </summary>
    public abstract class CdcExecutorResult
    {
        /// <summary>
        /// Gets or sets the database return code.
        /// </summary>
        public int ReturnCode { get; internal set; }

        /// <summary>
        /// Gets or sets the <see cref="CdcEnvelope"/>.
        /// </summary>
        public CdcEnvelope? Envelope { get; internal set; }

        /// <summary>
        /// Indicates that a envelope execution was successful and can continue.
        /// </summary>
        public bool EnvelopeExecuted => ReturnCode == 0 && Envelope != null;
    }

    /// <summary>
    /// Represents the <see cref="CdcExecutor{TCdcEntity, TCdcEntityWrapper, TCdcEntityWrapperColl}"/> result.
    /// </summary>
    /// <typeparam name="TCdcEntityWrapperColl">The <typeparamref name="TCdcEntityWrapper"/> collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TCdcEntityWrapper">The entity wrapper <see cref="Type"/>.</typeparam>
    public class CdcExecutorResult<TCdcEntityWrapperColl, TCdcEntityWrapper> : CdcExecutorResult
        where TCdcEntityWrapperColl : List<TCdcEntityWrapper>, new() where TCdcEntityWrapper : class, ICdcOperationType
    {
        /// <summary>
        /// Gets the resulting <typeparamref name="TCdcEntityWrapperColl"/>.
        /// </summary>
        public TCdcEntityWrapperColl Result { get; } = new TCdcEntityWrapperColl();
    }
}