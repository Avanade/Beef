// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;

namespace Beef.Data.Database
{
    /// <summary>
    /// Provides the base <see cref="EnqueueAsync(IDatabase, IEnumerable{DatabaseEventOutboxItem})">enqueue</see> and <see cref="DequeueAsync(IDatabase, int)">dequeue</see> of <see cref="DatabaseEventOutboxItem"/>(s).
    /// </summary>
    public abstract class DatabaseEventOutboxBase : DatabaseMapper<DatabaseEventOutboxItem>
    {
        private const string EventListParamName = "EventList";
        private const string MaxDequeueCountParamName = "MaxDequeueCount";

        /// <summary>
        /// Gets the default maximum dequeue count.
        /// </summary>
        public const int DefaultMaxDequeueCount = 10;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseEventOutboxBase"/> class.
        /// </summary>
        /// <param name="dbEnqueueStoredProcedureName">The database stored procedure name for enqueueing the event into the outbox.</param>
        /// <param name="dbDequeueStoredProcedureName">The database stored procedure name for dequeueing the event from the outbox.</param>
        /// <param name="dbTypeName">The database type name for the <see cref="TableValuedParameter"/>.</param>
        public DatabaseEventOutboxBase(string dbEnqueueStoredProcedureName, string dbDequeueStoredProcedureName, string dbTypeName)
        {
            DbEnqueueStoredProcedureName = Check.NotEmpty(dbEnqueueStoredProcedureName, nameof(dbEnqueueStoredProcedureName));
            DbDequeueStoredProcedureName = Check.NotEmpty(dbDequeueStoredProcedureName, nameof(dbDequeueStoredProcedureName));
            DbTypeName = Check.NotEmpty(dbTypeName, nameof(dbTypeName));

            Property(s => s.EventId);
            Property(s => s.Subject);
            Property(s => s.Action);
            Property(s => s.CorrelationId);
            Property(s => s.TenantId);
            Property(s => s.ValueType);
            Property(s => s.EventData);
        }

        /// <summary>
        /// Gets the database stored procedure name for enqueueing the event into the outbox.
        /// </summary>
        public string DbEnqueueStoredProcedureName { get; }

        /// <summary>
        /// Gets the database stored procedure name for dequeueing the event from the outbox.
        /// </summary>
        public string DbDequeueStoredProcedureName { get; }

        /// <summary>
        /// Gets the database type name for the <see cref="TableValuedParameter"/>.
        /// </summary>
        public string DbTypeName { get; }

        /// <summary>
        /// Creates a <see cref="TableValuedParameter"/> for the <paramref name="list"/>.
        /// </summary>
        /// <param name="list">The <see cref="DatabaseEventOutboxItem"/> list.</param>
        /// <returns>The Table-Valued Parameter.</returns>
        public TableValuedParameter CreateTableValuedParameter(IEnumerable<DatabaseEventOutboxItem> list)
        {
            var dt = new DataTable();
            dt.Columns.Add("EventId", typeof(Guid));
            dt.Columns.Add("Subject", typeof(string));
            dt.Columns.Add("Action", typeof(string));
            dt.Columns.Add("CorrelationId", typeof(string));
            dt.Columns.Add("TenantId", typeof(Guid));
            dt.Columns.Add("ValueType", typeof(string));
            dt.Columns.Add("EventData", typeof(SqlBinary));

            var tvp = new TableValuedParameter(DbTypeName, dt);
            AddToTableValuedParameter(tvp, list);
            return tvp;
        }

        /// <summary>
        /// Enqueues the <see cref="DatabaseEventOutboxItem"/> <paramref name="list"/> into the database.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="list">The <see cref="DatabaseEventOutboxItem"/> list.</param>
        public async Task EnqueueAsync(IDatabase db, IEnumerable<DatabaseEventOutboxItem> list)
        {
            if (list == null || !list.Any())
                return; 

            await Check.NotNull(db, nameof(db))
                .StoredProcedure(DbEnqueueStoredProcedureName)
                .Param(EventListParamName, CreateTableValuedParameter(list))
                .NonQueryAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Dequeues the <see cref="DatabaseEventOutboxItem"/>(s) from the database.
        /// </summary>
        /// <param name="db">The <see cref="IDatabase"/>.</param>
        /// <param name="maxDequeueCount">The maximum dequeue count; defaults to <see cref="DefaultMaxDequeueCount"/>.</param>
        /// <returns></returns>
        public async Task<IEnumerable<DatabaseEventOutboxItem>> DequeueAsync(IDatabase db, int maxDequeueCount = DefaultMaxDequeueCount)
            => await Check.NotNull(db, nameof(db))
                .StoredProcedure(DbDequeueStoredProcedureName)
                .Param(MaxDequeueCountParamName, maxDequeueCount <= 0 ? DefaultMaxDequeueCount : maxDequeueCount)
                .SelectQueryAsync(this).ConfigureAwait(false);
    }
}