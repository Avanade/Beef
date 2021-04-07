// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Events;
using Beef.Json;
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
    /// Manages the Change Data Capture (CDC) data orchestration.
    /// </summary>
    /// <typeparam name="TCdcEntity">The CDC database entity <see cref="Type"/>.</typeparam>
    /// <typeparam name="TCdcEntityWrapperColl">The <typeparamref name="TCdcEntityWrapper"/> collection <see cref="Type"/>.</typeparam>
    /// <typeparam name="TCdcEntityWrapper">The <typeparamref name="TCdcEntity"/> wrapper <see cref="Type"/>.</typeparam>
    /// <typeparam name="TCdcTrackingMapper">The tracking database mapper <see cref="Type"/>.</typeparam>
    public abstract class CdcDataOrchestrator<TCdcEntity, TCdcEntityWrapperColl, TCdcEntityWrapper, TCdcTrackingMapper> : ICdcDataOrchestrator
        where TCdcEntity : class, ITableKey, IETag, new()
        where TCdcEntityWrapperColl : List<TCdcEntityWrapper>, new()
        where TCdcEntityWrapper : class, TCdcEntity, ICdcWrapper, new()
        where TCdcTrackingMapper : ITrackingTvp, new()
    {
        private const string MaxQuerySizeParamName = "MaxQuerySize";
        private const string ContinueWithDataLossParamName = "ContinueWithDataLoss";
        private const string OutboxIdParamName = "OutboxId";
        private const string TrackingListParamName = "TrackingList";
        private const string IdentifierListParamName = "IdentifierList";

        private static readonly DatabaseMapper<CdcOutbox> _outboxMapper = DatabaseMapper.CreateAuto<CdcOutbox>();
        private static readonly TCdcTrackingMapper _trackingMapper = new TCdcTrackingMapper();
        private string? _name;
        private int _maxQuerySize = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="CdcDataOrchestrator{TCdcEntity, TCdcEntityWrapper, TCdcEntityWrapperColl, TCdcTrackingMapper}"/> class with no identifier mapping support.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="executeStoredProcedureName">The name of the execute outbox stored procedure.</param>
        /// <param name="completeStoredProcedureName">The name of the complete outbox stored procedure.</param>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public CdcDataOrchestrator(IDatabase db, string executeStoredProcedureName, string completeStoredProcedureName, IEventPublisher eventPublisher, ILogger logger)
        {
            Db = Check.NotNull(db, nameof(db));
            ExecuteStoredProcedureName = Check.NotEmpty(executeStoredProcedureName, nameof(executeStoredProcedureName));
            CompleteStoredProcedureName = Check.NotEmpty(completeStoredProcedureName, nameof(completeStoredProcedureName));
            EventPublisher = Check.NotNull(eventPublisher, nameof(eventPublisher));
            Logger = Check.NotNull(logger, nameof(logger));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CdcDataOrchestrator{TCdcEntity, TCdcEntityWrapper, TCdcEntityWrapperColl, TCdcTrackingMapper}"/> class that requires identifier mapping support.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="executeStoredProcedureName">The name of the execute outbox stored procedure.</param>
        /// <param name="completeStoredProcedureName">The name of the complete outbox stored procedure.</param>
        /// <param name="eventPublisher">The <see cref="IEventPublisher"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        /// <param name="identifierMappingStoredProcedureName">The name of the optional identifier mapping stored procedure.</param>
        /// <param name="identifierGenerator">The <see cref="IStringIdentifierGenerator"/>.</param>
        /// <param name="identifierMappingTvp">The <see cref="IIdentifierMappingTvp"/>.</param>
        public CdcDataOrchestrator(IDatabase db, string executeStoredProcedureName, string completeStoredProcedureName, IEventPublisher eventPublisher, ILogger logger, string identifierMappingStoredProcedureName, IStringIdentifierGenerator identifierGenerator, IIdentifierMappingTvp identifierMappingTvp)
        {
            Db = Check.NotNull(db, nameof(db));
            ExecuteStoredProcedureName = Check.NotEmpty(executeStoredProcedureName, nameof(executeStoredProcedureName));
            CompleteStoredProcedureName = Check.NotEmpty(completeStoredProcedureName, nameof(completeStoredProcedureName));
            EventPublisher = Check.NotNull(eventPublisher, nameof(eventPublisher));
            Logger = Check.NotNull(logger, nameof(logger));
            IdentifierMappingStoredProcedureName = Check.NotEmpty(identifierMappingStoredProcedureName, nameof(identifierMappingStoredProcedureName));
            IdentifierGenerator = Check.NotNull(identifierGenerator, nameof(identifierGenerator));
            IdentifierMappingTvp = Check.NotNull(identifierMappingTvp, nameof(identifierMappingTvp));
        }

        /// <summary>
        /// Gets the <see cref="IDatabase"/>.
        /// </summary>
        public IDatabase Db { get; private set; }

        /// <summary>
        /// Gets the name of the <b>execute</b> outbox stored procedure.
        /// </summary>
        public string ExecuteStoredProcedureName { get; private set; }

        /// <summary>
        /// Gets the name of the <b>complete</b> outbox stored procedure.
        /// </summary>
        public string CompleteStoredProcedureName { get; private set; }

        /// <summary>
        /// Gets the name of the <b>identifier mapping</b> outbox stored procedure.
        /// </summary>
        /// <remarks>A <c>null</c> value indicates that no <b>identifier mapping</b> support is required.</remarks>
        public string? IdentifierMappingStoredProcedureName { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEventPublisher"/>.
        /// </summary>
        public IEventPublisher EventPublisher { get; private set; }

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        public ILogger Logger { get; private set; }

        /// <summary>
        /// Gets the <see cref="IStringIdentifierGenerator"/>.
        /// </summary>
        public IStringIdentifierGenerator? IdentifierGenerator { get; private set; }

        /// <summary>
        /// Gets the <see cref="IIdentifierMappingTvp"/>.
        /// </summary>
        public IIdentifierMappingTvp? IdentifierMappingTvp { get; private set; }

        /// <summary>
        /// Gets the service name (used for logging).
        /// </summary>
        protected virtual string ServiceName => _name ??= GetType().Name;

        /// <summary>
        /// Gets the <see cref="EventData.Subject"/>.
        /// </summary>
        protected abstract string EventSubject { get; }

        /// <summary>
        /// Gets the <see cref="Beef.Events.EventData.Subject"/> format.
        /// </summary>
        protected virtual EventSubjectFormat EventSubjectFormat { get; } = EventSubjectFormat.NameAndKey;

        /// <summary>
        /// Gets the <see cref="Events.EventActionFormat"/>.
        /// </summary>
        protected virtual EventActionFormat EventActionFormat { get; } = EventActionFormat.None;

        /// <summary>
        /// Gets the list of property names that should be excluded from the serialized JSON <see cref="IETag"/> generation.
        /// </summary>
        protected virtual string[]? ExcludePropertiesFromETag { get; }

        /// <summary>
        /// Gets or sets the maximum query size to limit the number of CDC (Change Data Capture) rows that are batched in a <see cref="CdcOutbox"/>.
        /// </summary>
        /// <remarks>Defaults to 100.</remarks>
        public int MaxQuerySize { get => _maxQuerySize; set => _maxQuerySize = value < 1 ? 100 : value; }

        /// <summary>
        /// Indicates whether to ignore any data loss and continue using the CDC (Change Data Capture) data that is available.
        /// </summary>
        /// <remarks>For more information as to why data loss may occur see: https://docs.microsoft.com/en-us/sql/relational-databases/track-changes/administer-and-monitor-change-data-capture-sql-server </remarks>
        public bool ContinueWithDataLoss { get; set; }

        /// <summary>
        /// Completes an existing outbox updating the corresponding <paramref name="tracking"/> where appropriate.
        /// </summary>
        /// <param name="outboxId">The outbox identifer.</param>
        /// <param name="tracking">The <see cref="CdcTracker"/> list.</param>
        /// <returns>The <see cref="CdcDataOrchestratorResult"/>.</returns>
        Task<CdcDataOrchestratorResult> ICdcDataOrchestrator.CompleteAsync(int outboxId, List<CdcTracker> tracking) => CompleteAsync(outboxId, tracking);

        /// <summary>
        /// Completes an existing outbox updating the corresponding <paramref name="tracking"/> where appropriate.
        /// </summary>
        /// <param name="outboxId">The outbox identifer.</param>
        /// <param name="tracking">The <see cref="CdcTracker"/> list.</param>
        /// <returns>The <see cref="CdcDataOrchestratorResult"/>.</returns>
        public async Task<CdcDataOrchestratorResult> CompleteAsync(int outboxId, List<CdcTracker> tracking)
        {
            var result = new CdcDataOrchestratorResult<TCdcEntityWrapperColl, TCdcEntityWrapper>();
            var msa = new List<IMultiSetArgs> { new MultiSetSingleArgs<CdcOutbox>(_outboxMapper, r => result.Outbox = r, isMandatory: false, stopOnNull: true) };

            try
            {
                await Db.StoredProcedure(CompleteStoredProcedureName)
                    .Param(OutboxIdParamName, outboxId)
                    .TableValuedParam(TrackingListParamName, _trackingMapper.CreateTableValuedParameter(tracking))
                    .SelectQueryMultiSetAsync(msa.ToArray()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException || (ex is AggregateException aex && aex.InnerException is TaskCanceledException))
                {
                    Logger.LogWarning($"{ServiceName}: Task canceled.");
                    throw;
                }

                result.Exception = ex;

                if (ex is IBusinessException)
                    Logger.LogError($"{ServiceName}: {result.Exception.Message}");
                else
                    Logger.LogCritical($"{ServiceName}: Unexpected Exception encountered: {result.Exception.Message}");
            }

            return result;
        }

        /// <summary>
        /// Executes the next (new) outbox, or reprocesses the last incomplete, then <see cref="CompleteAsync(int, List{CdcTracker})">completes</see> on success.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="CdcDataOrchestratorResult"/>.</returns>
        async Task<CdcDataOrchestratorResult> ICdcDataOrchestrator.ExecuteAsync(CancellationToken? cancellationToken)
            => await ExecuteAsync(cancellationToken).ConfigureAwait(false);

        /// <summary>
        /// Executes the next (new) outbox, or reprocesses the last incomplete, then <see cref="CompleteAsync(int, List{CdcTracker})">completes</see> on success.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The <see cref="CdcDataOrchestratorResult{TCdcEntityWrapper, TCdcEntityWrapperColl}"/>.</returns>
        public async Task<CdcDataOrchestratorResult<TCdcEntityWrapperColl, TCdcEntityWrapper>> ExecuteAsync(CancellationToken? cancellationToken = null)
        {
            // Get the requested outbox data.
            CdcDataOrchestratorResult<TCdcEntityWrapperColl, TCdcEntityWrapper> result;
            Logger.LogInformation($"{ServiceName} Query for next (new) Change Data Capture outbox (MaxQuerySize = {MaxQuerySize}, ContinueWithDataLoss = {ContinueWithDataLoss}).");

            try
            {
                result = await GetOutboxEntityDataAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (ex is TaskCanceledException || (ex is AggregateException aex && aex.InnerException is TaskCanceledException))
                {
                    Logger.LogWarning($"{ServiceName}: Task canceled.");
                    throw;
                }

                result = new CdcDataOrchestratorResult<TCdcEntityWrapperColl, TCdcEntityWrapper> { Exception = ex };

                if (ex is IBusinessException)
                    Logger.LogError($"{ServiceName}: {result.Exception.Message}");
                else
                    Logger.LogCritical($"{ServiceName}: Unexpected Exception encountered: {result.Exception.Message}");

                return result;
            }

            if (result.Outbox == null)
            {
                Logger.LogInformation($"{ServiceName} Outbox 'none': No new Change Data Capture data was found.");
                return result;
            }

            Logger.LogInformation($"{ServiceName} Outbox '{result.Outbox.Id}': {result.Result.Count} entity(s) were found.");
            if ((cancellationToken ??= CancellationToken.None).IsCancellationRequested)
            {
                Logger.LogWarning($"{ServiceName} Outbox '{result.Outbox.Id}': Incomplete as a result of Cancellation.");
                return await Task.FromCanceled<CdcDataOrchestratorResult<TCdcEntityWrapperColl, TCdcEntityWrapper>>(cancellationToken.Value).ConfigureAwait(false);
            }

            // Consolidate the results to the set that is to be sent (i.e. ignore unneccessary).
            var coll = new TCdcEntityWrapperColl();
            foreach (var grp in result.Result.GroupBy(x => new { x.UniqueKey }))
            {
                // Find delete and use.
                var item = grp.Where(x => x.DatabaseOperationType == OperationType.Delete).FirstOrDefault();
                if (item != null && grp.Any(x => x.DatabaseOperationType == OperationType.Create))
                    continue;  // Created and deleted in quick succession; no need to publish.

                // Where there is no delete then just use the first.
                if (item == null)
                {
                    item = grp.First();

                    // Where the primary key is initial (being the actual table primary key columns versus from CDC) then it has been _subsequently_ deleted (physically) so skip.
                    if (item is ITableKey tk && tk.TableKey.IsInitial)
                        continue;
                }

                // Where supports logical delete and IsDeleted, then override DatabaseOperationType.
                if (item is ILogicallyDeleted ild && ild.IsDeleted)
                {
                    item.DatabaseOperationType = OperationType.Delete;
                    ild.ClearWhereDeleted();
                }

                coll.Add(item);
            }

            // Where performing identifier mapping then enact as required.
            if ((cancellationToken ??= CancellationToken.None).IsCancellationRequested)
            {
                Logger.LogWarning($"{ServiceName} Outbox '{result.Outbox.Id}': Incomplete as a result of Cancellation.");
                return await Task.FromCanceled<CdcDataOrchestratorResult<TCdcEntityWrapperColl, TCdcEntityWrapper>>(cancellationToken.Value).ConfigureAwait(false);
            }

            if (IdentifierMappingStoredProcedureName != null)
                await AssignIdentityMappingAsync(coll).ConfigureAwait(false);

            // Determine whether anything may have been sent before and exclude (i.e. do not send again).
            var coll2 = new TCdcEntityWrapperColl();
            var tracking = new List<CdcTracker>();
            foreach (var item in coll)
            {
                // Where there is a ETag/RowVersion column use; otherwise, calculate (serialized hash).
                var entity = item as TCdcEntity;
                if (entity.ETag == null)
                {
                    if (ExcludePropertiesFromETag == null || ExcludePropertiesFromETag.Length == 0)
                        entity.ETag = ETagGenerator.Generate(entity);
                    else
                    {
                        var json = JsonPropertyFilter.Apply(entity, null, ExcludePropertiesFromETag);
                        entity.ETag = ETagGenerator.Generate(json!.ToString(Newtonsoft.Json.Formatting.None));
                    }
                }

                // Where the ETag and TrackingHash match then skip (has already been published).
                if (item.DatabaseTrackingHash == null || item.DatabaseTrackingHash != entity.ETag)
                {
                    coll2.Add(item);
                    tracking.Add(new CdcTracker { Key = CreateValueKey(entity), Hash = entity.ETag });
                }
            }

            if ((cancellationToken ??= CancellationToken.None).IsCancellationRequested)
            {
                Logger.LogWarning($"{ServiceName} Outbox '{result.Outbox.Id}': Incomplete as a result of Cancellation.");
                return await Task.FromCanceled<CdcDataOrchestratorResult<TCdcEntityWrapperColl, TCdcEntityWrapper>>(cancellationToken.Value).ConfigureAwait(false);
            }

            // Publish & send the events.
            if (coll2.Count > 0)
            {
                result.Events = (await CreateEventsAsync(coll2, cancellationToken.Value).ConfigureAwait(false)).ToArray();
                await EventPublisher.Publish(result.Events).SendAsync().ConfigureAwait(false);
                Logger.LogInformation($"{ServiceName} Outbox '{result.Outbox.Id}': {result.Events.Length} event(s) were published/sent successfully.");
            }
            else
                Logger.LogInformation($"{ServiceName} Outbox '{result.Outbox.Id}': No event(s) were published; no unique tracking hash found.");

            // Complete the outbox (ignore any further 'cancel' as event(s) have been published and we *must* complete to minimise chance of sending more than once).
            await CompleteAsync(result.Outbox.Id, tracking).ConfigureAwait(false);
            Logger.LogInformation($"{ServiceName} Outbox '{result.Outbox.Id}': Marked as Completed.");

            return result;
        }

        /// <summary>
        /// Executes a multi-dataset query command with one or more <paramref name="multiSetArgs"/>; whilst also outputing the resulting return value.
        /// </summary>
        /// <param name="multiSetArgs">One or more <see cref="IMultiSetArgs"/>.</param>
        /// <returns>The resultant <see cref="CdcDataOrchestratorResult{TCdcEntityWrapperColl, TCdcEntityWrapper}"/>.</returns>
        protected async Task<CdcDataOrchestratorResult<TCdcEntityWrapperColl, TCdcEntityWrapper>> SelectQueryMultiSetAsync(params IMultiSetArgs[] multiSetArgs)
        {
            var result = new CdcDataOrchestratorResult<TCdcEntityWrapperColl, TCdcEntityWrapper>();
            var msa = new List<IMultiSetArgs> { new MultiSetSingleArgs<CdcOutbox>(_outboxMapper, r => result.Outbox = r, isMandatory: false, stopOnNull: true) };
            msa.AddRange(multiSetArgs);

            await Db.StoredProcedure(ExecuteStoredProcedureName)
                .Param(MaxQuerySizeParamName, MaxQuerySize)
                .Param(ContinueWithDataLossParamName, ContinueWithDataLoss)
                .SelectQueryMultiSetWithValueAsync(msa.ToArray())
                .ConfigureAwait(false);

            return result;
        }

        /// <summary>
        /// Gets the outbox entity data from the database.
        /// </summary>
        /// <returns>A <typeparamref name="TCdcEntityWrapperColl"/>.</returns>
        protected abstract Task<CdcDataOrchestratorResult<TCdcEntityWrapperColl, TCdcEntityWrapper>> GetOutboxEntityDataAsync();

        /// <summary>
        /// Assigns the identity mapping by adding <i>new</i> for those items that do not currentyly have a global identifier currently assigned.
        /// </summary>
        /// <param name="coll">The wrapper entity collection.</param>
        protected async Task AssignIdentityMappingAsync(TCdcEntityWrapperColl coll)
        {
            // Find all the instances where there is currently no global identifier assigned.
            var vimc = new CdcValueIdentifierMappingCollection();
            coll.OfType<ICdcLinkIdentifierMapping>().ForEach(async item => await item.LinkIdentifierMappingsAsync(vimc, IdentifierGenerator!).ConfigureAwait(false));

            if (vimc.Count == 0)
                return;

            // There could be multiple references to same Schema/Table/Key; these need to filtered out; i.e. send only a distinct list.
            var imcd = new Dictionary<(string?, string?, string?), CdcIdentifierMapping>();
            vimc.ForEach(item => imcd.TryAdd((item.Schema, item.Table, item.Key), item));
            var tvp = IdentifierMappingTvp!.CreateTableValuedParameter(imcd.Values);

            // Execute the stored procedure and get the updated list.
            var imc = await Db.StoredProcedure(IdentifierMappingStoredProcedureName!)
                .TableValuedParam(IdentifierListParamName, tvp)
                .SelectQueryAsync(DatabaseMapper.CreateAuto<CdcIdentifierMapping>())
                .ConfigureAwait(false);

            if (imc.Count() != imcd.Count())
                throw new InvalidOperationException($"Stored procedure '{IdentifierMappingStoredProcedureName}' returned an unexpected result.");

            // Re-link the identifier mappings with the final value.
            vimc.ForEach(item => item.GlobalId = imc.Single(x => x.Schema == item.Schema && x.Table == item.Table && x.Key == item.Key).GlobalId);
            coll.OfType<ICdcLinkIdentifierMapping>().ForEach(item => item.RelinkIdentifierMappings(vimc));
        }

        /// <summary>
        /// Creates an <see cref="EventData{T}"/> for the specified <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">The <paramref name="value"/> <see cref="Type"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="subjectName">The <see cref="EventData.Subject"/> name (will be formated as per <see cref="EventSubjectFormat"/>).</param>
        /// <param name="operationType">The <see cref="OperationType"/> to infer the <see cref="EventData.Action"/>.</param>
        /// <returns>The <see cref="EventData{T}"/>.</returns>
        protected EventData<T> CreateValueEvent<T>(T value, string subjectName, OperationType operationType) where T : class
            => EventData.CreateValueEvent(value, CreateValueFormattedSubject(value, subjectName), EventActionFormatter.Format(operationType, EventActionFormat));

        /// <summary>
        /// Creates an <see cref="EventData"/> with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="subjectName">The <see cref="EventData.Subject"/> name (will be formated as per <see cref="EventSubjectFormat"/>).</param>
        /// <param name="operationType">The <see cref="OperationType"/> to infer the <see cref="EventData.Action"/>.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The <see cref="EventData"/>.</returns>
        protected EventData CreateEvent(string subjectName, OperationType operationType, params IComparable?[] key)
            => EventData.CreateEvent(CreateFormattedSubject(subjectName, key), EventActionFormatter.Format(operationType, EventActionFormat), key);

        /// <summary>
        /// Creates a fully qualified event subject as per <see cref="EventSubjectFormat"/>.
        /// </summary>
        /// <typeparam name="T">The <paramref name="value"/> <see cref="Type"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="subjectName">The <see cref="EventData.Subject"/> name.</param>
        /// <returns>The fully qualified subject.</returns>
        /// <remarks><typeparamref name="T"/> must implement at least one of the following: <see cref="IIdentifier"/>, <see cref="IGuidIdentifier"/>, <see cref="IStringIdentifier"/> or <see cref="IUniqueKey"/>.</remarks>
        protected string CreateValueFormattedSubject<T>(T value, string subjectName) where T : class => EventSubjectFormat == EventSubjectFormat.NameOnly ? subjectName : subjectName + "." + CreateValueKey(value);

        /// <summary>
        /// Creates the key (as a <see cref="string"/>) for the <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T">The <paramref name="value"/> <see cref="Type"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>The key for the <paramref name="value"/>.</returns>
        protected static string CreateValueKey<T>(T value) where T : class => value is IGlobalIdentifier gi && gi.GlobalId != null ? gi.GlobalId : value.CreateFormattedKey();

        /// <summary>
        /// Creates a fully qualified event subject by appending the <paramref name="key"/> to the <paramref name="subjectName"/>.
        /// </summary>
        /// <param name="subjectName">The <see cref="EventData.Subject"/> prefix.</param>
        /// <param name="key">The event key.</param>
        /// <returns>The fully qualified subject.</returns>
        protected string CreateFormattedSubject(string subjectName, params IComparable?[] key)
        {
            if (EventSubjectFormat == EventSubjectFormat.NameOnly)
                return subjectName;

            var sb = new StringBuilder(subjectName + ".");
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
        protected virtual Task<IEnumerable<EventData>> CreateEventsAsync(TCdcEntityWrapperColl coll, CancellationToken cancellationToken)
        {
            var events = new List<EventData>();

            foreach (var item in coll ?? throw new ArgumentNullException(nameof(coll)))
            {
                events.Add(CreateValueEvent(item, EventSubject, item.DatabaseOperationType));
            }

            return Task.FromResult(events.AsEnumerable());
        }

        /// <summary>
        /// Provides the capability to create none or more <see cref="EventData">events</see> from the entity <paramref name="coll">collection.</paramref> using the <paramref name="getEntityFunc"/>
        /// to get the entity value to publish. A <c>null</c> function response indicates entity is not found and/or do not publish (i.e. skip).
        /// </summary>
        /// <param name="coll">The wrapper entity collection.</param>
        /// <param name="getEntityFunc">The function to get the entity value to publish.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>None or more <see cref="EventData">events</see> to be published.</returns>
        protected async Task<IEnumerable<EventData>> CreateEventsWithGetAsync<TEntity>(TCdcEntityWrapperColl coll, Func<TCdcEntityWrapper, Task<TEntity>> getEntityFunc, CancellationToken cancellationToken)
            where TEntity : class
        {
            var events = new List<EventData>();

            foreach (var item in coll ?? throw new ArgumentNullException(nameof(coll)))
            {
                if (cancellationToken.IsCancellationRequested)
                    await Task.FromCanceled(cancellationToken).ConfigureAwait(false);

                switch (item.DatabaseOperationType)
                {
                    case OperationType.Delete:
                        events.Add(CreateValueEvent(item, EventSubject, OperationType.Delete));
                        break;

                    default:
                        var entity = await getEntityFunc(item).ConfigureAwait(false);
                        if (entity != null)
                            events.Add(CreateValueEvent(entity, EventSubject, item.DatabaseOperationType));

                        break;
                }
            }

            return events;
        }
    }
}