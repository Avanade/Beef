// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Mapper;
using Beef.RefData;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Beef.Data.Database
{
    /// <summary>
    /// Represents the base class for encapsulating the database access layer using old skool ADO.NET - because sometimes it is all you need, and it is super efficient.
    /// </summary>
    /// <remarks>Provides automatic database connection management opening on first use and closing on <see cref="Dispose(bool)"/>.</remarks>
    public abstract class DatabaseBase : IDatabase
    {
        private readonly object _lock = new object();
        private DbConnection? _connection;
        private bool _disposed;
        private ILogger? _logger;
        private DatabaseEventOutboxInvoker? _eventOutboxInvoker;

        /// <summary>
        /// Transforms and throws the <see cref="IBusinessException"/> equivalent for the <see cref="SqlException"/> known list.
        /// </summary>
        /// <param name="sex">The <see cref="SqlException"/>.</param>
        public static void ThrowTransformedSqlException(SqlException sex)
        {
            if (sex == null)
                throw new ArgumentNullException(nameof(sex));

            var msg = sex.Message?.TrimEnd();
            if (string.IsNullOrEmpty(msg))
                msg = null;

            switch (sex.Number)
            {
                case 56001: throw new ValidationException(msg, sex);
                case 56002: throw new BusinessException(msg, sex);
                case 56003: throw new AuthorizationException(msg, sex);
                case 56004: throw new ConcurrencyException(msg, sex);
                case 56005: throw new NotFoundException(msg, sex);
                case 56006: throw new ConflictException(msg, sex);
                case 56007: throw new DuplicateException(msg, sex);

                default:
                    if (AlwaysCheckSqlDuplicateErrorNumbers && SqlDuplicateErrorNumbers.Contains(sex.Number))
                        throw new DuplicateException(null, sex);

                    break;
            }
        }

        /// <summary>
        /// Indicates whether to always check the <see cref="SqlDuplicateErrorNumbers"/> when executing the <see cref="ThrowTransformedSqlException(SqlException)"/> method.
        /// </summary>
        public static bool AlwaysCheckSqlDuplicateErrorNumbers { get; set; } = true;

        /// <summary>
        /// Gets or sets the list of known <see cref="SqlException.Number"/> values for the <see cref="ThrowTransformedSqlException(SqlException)"/> method.
        /// </summary>
        /// <remarks>See https://docs.microsoft.com/en-us/sql/relational-databases/errors-events/database-engine-events-and-errors
        /// and https://docs.microsoft.com/en-us/azure/sql-database/sql-database-develop-error-messages </remarks>
        public static List<int> SqlDuplicateErrorNumbers { get; } = new List<int>(new int[] { 2601, 2627 });

        /// <summary>
        /// Gets or sets the list of known <see cref="SqlException.Number"/> values that are considered transient; candidates for a retry. 
        /// </summary>
        /// <remarks>See https://docs.microsoft.com/en-us/sql/relational-databases/errors-events/database-engine-events-and-errors 
        /// and https://docs.microsoft.com/en-us/azure/sql-database/sql-database-develop-error-messages </remarks>
        public static List<int> SqlTransientErrorNumbers { get; } = new List<int>(new int[]
        {
            -1, -2, 701, 1204, 1205, 1222, 8645, 8651, 30053, // https://stackoverflow.com/questions/4821668/what-is-good-c-sharp-coding-style-for-catching-sqlexception-and-retrying
            10928, 10929, 10053, 10054, 10060, 40540, 40143, 233, 64, // https://github.com/Azure/elastic-db-tools/blob/master/Src/ElasticScale.Client/ElasticScale.Common/TransientFaultHandling/Implementation/SqlDatabaseTransientErrorDetectionStrategy.cs
            4060, 40197, 40501, 40613, 49918, 49919, 49920, 4221, // https://docs.microsoft.com/en-us/azure/sql-database/sql-database-develop-error-messages
            1222, 1421, 1807, 3928, 5030, 7604, 8628, 8654, 17197, 17830, 17889, 18486 // https://github.com/marinoscar/CommonHelpers/blob/master/SqlErrorCodes.cs
        });

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseBase"/> class for a <paramref name="connectionString"/> and <paramref name="provider"/>.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="provider">The optional data provider (e.g. Microsoft.Data.SqlClient); defaults to <see cref="SqlClientFactory"/>.</param>
        /// <param name="invoker">Enables the <see cref="Invoker"/> to be overridden; defaults to <see cref="DatabaseInvoker"/>.</param>
        protected DatabaseBase(string connectionString, DbProviderFactory? provider = null, DatabaseInvoker? invoker = null)
        {
            ConnectionString = !string.IsNullOrEmpty(connectionString) ? connectionString : throw new ArgumentNullException(nameof(connectionString));
            Provider = provider ?? SqlClientFactory.Instance;
            Invoker = invoker ?? new DatabaseInvoker();
        }

        /// <summary>
        /// Gets the database connection string.
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Gets the <see cref="DbProviderFactory"/>.
        /// </summary>
        public DbProviderFactory Provider { get; private set; }

        /// <summary>
        /// Gets the <see cref="DatabaseInvoker"/>.
        /// </summary>
        public DatabaseInvoker Invoker { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="DatabaseEventOutboxInvoker"/>. This defaults where not explicitly overridden.
        /// </summary>
        /// <returns>The <see cref="IDatabase.EventOutboxInvoker"/>.</returns>
        public DatabaseEventOutboxInvoker EventOutboxInvoker
        {
            get => _eventOutboxInvoker ??= new DatabaseEventOutboxInvoker(this);
            set
            {
                if (value != null && value.Database != this)
                    throw new ArgumentException("The DatabaseEventOutboxInvoker.Database property must be the same instance as this DatabaseBase instance.", nameof(EventOutboxInvoker));

                _eventOutboxInvoker = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DatabaseWildcard"/> to enable wildcard replacement.
        /// </summary>
        public DatabaseWildcard Wildcard { get; set; } = new DatabaseWildcard();

        /// <summary>
        /// Determines (overrides) the <see cref="Entities.DateTimeTransform"/> to be used when retrieving (see <see cref="DatabaseRecord.GetValue{DateTime}(string)"/>) a <see cref="DateTime"/> value
        /// from a <see cref="DatabaseRecord"/>.
        /// </summary>
        public DateTimeTransform DateTimeTransform { get; set; } = DateTimeTransform.UseDefault;

        /// <summary>
        /// Gets or sets the <see cref="SqlException"/> handler (by default set up to execute <see cref="ThrowTransformedSqlException(SqlException)"/>).
        /// </summary>
        public Action<SqlException> ExceptionHandler { get; set; } = (sex) => ThrowTransformedSqlException(sex);

        /// <summary>
        /// Gets or sets the stored procedure name used by <see cref="SetSqlSessionContext(DbConnection)"/>; defaults to '[dbo].[spSetSessionContext]'.
        /// </summary>
        public string SessionContextStoredProcedure { get; set; } = "[dbo].[spSetSessionContext]";

        /// <summary>
        /// Gets the <see cref="ILogger"/>.
        /// </summary>
        internal ILogger Logger => _logger ??= Diagnostics.Logger.Create<DatabaseBase>();

        /// <summary>
        /// Gets the unique database instance identifier.
        /// </summary>
        public Guid DatabaseId { get; } = Guid.NewGuid();

        /// <summary>
        /// Sets the SQL session context using the specified values by invoking the <see cref="SessionContextStoredProcedure"/> using parameters named
        /// <see cref="DatabaseColumns.SessionContextUsername"/> and <see cref="DatabaseColumns.SessionContextTimestamp"/>.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        /// <param name="username">The username.</param>
        /// <param name="timestamp">The timestamp <see cref="DateTime"/> (where <c>null</c> the value will default to <see cref="DateTime.Now"/>).</param>
        /// <param name="tenantId">The tenant identifer (where <c>null</c> the value will not be used).</param>
        /// <param name="userId">The unique user identifier.</param>
        public void SetSqlSessionContext(DbConnection dbConnection, string username, DateTime? timestamp, Guid? tenantId = null, string? userId = null)
        {
            if (dbConnection == null)
                throw new ArgumentNullException(nameof(dbConnection));

            if (string.IsNullOrEmpty(SessionContextStoredProcedure))
                throw new InvalidOperationException("The SessionContextStoredProcedure property must have a value.");

            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = SessionContextStoredProcedure;
            cmd.CommandType = CommandType.StoredProcedure;

            var p = cmd.CreateParameter();
            p.ParameterName = "@" + DatabaseColumns.SessionContextUsername;
            p.Value = username;
            cmd.Parameters.Add(p);

            p = cmd.CreateParameter();
            p.ParameterName = "@" + DatabaseColumns.SessionContextTimestamp;
            p.Value = timestamp ?? Entities.Cleaner.Clean(DateTime.Now);
            cmd.Parameters.Add(p);

            if (tenantId.HasValue)
            {
                p = cmd.CreateParameter();
                p.ParameterName = "@" + DatabaseColumns.SessionContextTenantId;
                p.Value = tenantId.Value;
                cmd.Parameters.Add(p);
            }

            if (!string.IsNullOrEmpty(userId))
            {
                p = cmd.CreateParameter();
                p.ParameterName = "@" + DatabaseColumns.SessionContextUserId;
                p.Value = userId;
                cmd.Parameters.Add(p);
            }

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Sets the SQL session context using the <see cref="ExecutionContext"/> (invokes <see cref="SetSqlSessionContext(DbConnection, string, DateTime?, Guid?, string?)"/> using
        /// <see cref="ExecutionContext.Username"/>, <see cref="ExecutionContext.Timestamp"/> and <see cref="ExecutionContext.TenantId"/>).
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        public void SetSqlSessionContext(DbConnection dbConnection)
        {
            var ec = ExecutionContext.Current ?? throw new InvalidOperationException("The ExecutionContext.Current must have an instance to SetSqlSessionContext.");
            SetSqlSessionContext(dbConnection, ec.Username, ec.Timestamp, ec.TenantId, ec.UserId);
        }

        /// <summary>
        /// Gets the <see cref="DbConnection"/> (automatically creates and opens on first access, then closes when disposed).
        /// </summary>
        /// <returns>The underlying <see cref="DbConnection"/>.</returns>
        public DbConnection GetConnection()
        {
            if (_connection != null)
                return _connection;

            lock (_lock)
            {
                if (_connection != null)
                    return _connection;

                Logger.LogDebug("Creating and opening the database connection. DatabaseId: {0}", DatabaseId);
                DbConnection conn = Provider.CreateConnection();
                conn.ConnectionString = ConnectionString;
                conn.Open();
                OnConnectionOpen(conn);
                return _connection = conn;
            }
        }

        /// <summary>
        /// Occurs when a connection is opened before any corresponding data access is performed.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        /// <remarks>This is where the <see cref="SetSqlSessionContext(DbConnection)"/> should be invoked; nothing is performed by default.</remarks>
        public virtual void OnConnectionOpen(DbConnection dbConnection) { }

        /// <summary>
        /// Creates a stored procedure <see cref="DatabaseCommand"/>.
        /// </summary>
        /// <param name="storedProcedure">The stored procedure name.</param>
        /// <returns>A <see cref="DatabaseCommand"/>.</returns>
        public DatabaseCommand StoredProcedure(string storedProcedure) => new DatabaseCommand(this, storedProcedure, CommandType.StoredProcedure);

        /// <summary>
        /// Creates a SQL statement <see cref="DatabaseCommand"/>.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <returns>A <see cref="DatabaseCommand"/>.</returns>
        public DatabaseCommand SqlStatement(string sqlStatement) => new DatabaseCommand(this, sqlStatement, CommandType.Text);

        /// <summary>
        /// Creates a <see cref="DatabaseQuery{T}"/> to enable select-like capabilities.
        /// </summary>
        /// <param name="queryArgs">The <see cref="DatabaseArgs{T}"/>.</param>
        /// <param name="queryParams">The query <see cref="DatabaseParameters"/> delegate.</param>
        /// <returns>A <see cref="DatabaseQuery{T}"/>.</returns>
        public DatabaseQuery<T> Query<T>(DatabaseArgs<T> queryArgs, Action<DatabaseParameters>? queryParams = null) where T : class, new() => new DatabaseQuery<T>(this, queryArgs, queryParams);

        /// <summary>
        /// Gets the entity for the specified <paramref name="keys"/> mapping to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="getArgs">The <see cref="DatabaseArgs{T}"/>.</param>
        /// <param name="keys">The key values.</param>
        /// <returns>The entity value where found; otherwise, <c>null</c>.</returns>
        public Task<T?> GetAsync<T>(DatabaseArgs<T> getArgs, params IComparable[] keys) where T : class, new()
        {
            if (getArgs == null)
                throw new ArgumentNullException(nameof(getArgs));

            return StoredProcedure(getArgs.StoredProcedure).Params((p) => getArgs.Mapper.GetKeyParams(p, OperationTypes.Get, keys)).SelectFirstOrDefaultAsync<T>(getArgs.Mapper);
        }

        /// <summary>
        /// Performs a create for the specified stored procedure and value (reselects where specified).
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="DatabaseArgs{T}"/>.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>The value (reselected where specified).</returns>
        /// <remarks>Automatically invokes <see cref="DatabaseParameters.AddChangeLogParameters(Entities.ChangeLog, bool, bool, ParameterDirection)"/>.</remarks>
        public async Task<T> CreateAsync<T>(DatabaseArgs<T> saveArgs, T value) where T : class, new()
        {
            if (saveArgs == null)
                throw new ArgumentNullException(nameof(saveArgs));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var cmd = StoredProcedure(saveArgs.StoredProcedure);
            saveArgs.Mapper.GetKeyParams(cmd.Parameters, OperationTypes.Create, value);
            cmd.Params(value, saveArgs.Mapper.MapToDb, OperationTypes.Create);

            if (saveArgs.Refresh)
            {
                cmd.Param("@" + DatabaseColumns.ReselectRecordName, true);
                return await cmd.SelectFirstOrDefaultAsync(saveArgs.Mapper).ConfigureAwait(false) ?? throw new NotFoundException();
            }

            // NOTE: without refresh, fields like IDs and RowVersion are not automatically updated.
            await cmd.NonQueryAsync().ConfigureAwait(false);
            return value;
        }

        /// <summary>
        /// Performs an update for the specified stored procedure and value (reselects where specified).
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="DatabaseArgs{T}"/>.</param>
        /// <param name="value">The value to update.</param>
        /// <returns>The value (reselected where specified).</returns>
        /// <remarks>Automatically invokes <see cref="DatabaseParameters.AddRowVersionParameter{T}(T, ParameterDirection)"/> and <see cref="DatabaseParameters.AddChangeLogParameters{T}(T, bool, bool, ParameterDirection)"/>.</remarks>
        public async Task<T> UpdateAsync<T>(DatabaseArgs<T> saveArgs, T value) where T : class, new()
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (saveArgs == null)
                throw new ArgumentNullException(nameof(saveArgs));

            var cmd = StoredProcedure(saveArgs.StoredProcedure);
            saveArgs.Mapper.GetKeyParams(cmd.Parameters, OperationTypes.Update, value);
            cmd.Params(value, saveArgs.Mapper.MapToDb, OperationTypes.Update);

            if (saveArgs.Refresh)
            {
                cmd.Param("@" + DatabaseColumns.ReselectRecordName, true);
                return await cmd.SelectFirstOrDefaultAsync(saveArgs.Mapper).ConfigureAwait(false) ?? throw new NotFoundException();
            }

            // NOTE: without refresh, fields like IDs and RowVersion are not automatically updated.
            await cmd.NonQueryAsync().ConfigureAwait(false);
            return value;
        }

        /// <summary>
        /// Performs a delete for the specified <paramref name="keys"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="DatabaseArgs{T}"/>.</param>
        /// <param name="keys">The key values.</param>
        public async Task DeleteAsync<T>(DatabaseArgs<T> saveArgs, params IComparable[] keys) where T : class, new()
        {
            if (saveArgs == null)
                throw new ArgumentNullException(nameof(saveArgs));

            var rowsAffected = await StoredProcedure(saveArgs.StoredProcedure).Params((p) => saveArgs.Mapper.GetKeyParams(p, OperationTypes.Delete, keys)).NonQueryAsync().ConfigureAwait(false);
            if (rowsAffected == 0)
                throw new NotFoundException();
        }

        /// <summary>
        /// Executes a <b>ReferenceData</b> query updating the <paramref name="coll"/>.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="ReferenceDataBase"/> <see cref="Type"/>.</typeparam>
        /// <param name="coll">The <see cref="ReferenceDataCollectionBase{TItem}"/>.</param>
        /// <param name="storedProcedure">The stored procedure name.</param>
        /// <param name="idColumnName">The <see cref="ReferenceDataBase.Id"/> column name override; defaults to <see cref="DatabaseRefDataColumns.IdColumnName"/>.</param>
        /// <param name="additionalProperties">The additional properties action that enables non-standard properties to be updated from the <see cref="DatabaseRecord"/>.</param>
        /// <param name="additionalDatasetRecords">The additional dataset record delegates where additional datasets are returned.</param>
        /// <param name="confirmItemIsToBeAdded">The action to confirm whether the item is to be added (defaults to <c>true</c>).</param>
        public async Task GetRefDataAsync<TColl, TItem>(TColl coll, string storedProcedure, string? idColumnName = null,
            Action<DatabaseRecord, TItem, DatabaseRecordFieldCollection>? additionalProperties = null,
            Action<DatabaseRecord>[]? additionalDatasetRecords = null,
            Func<DatabaseRecord, TItem, bool>? confirmItemIsToBeAdded = null)
                where TColl : ReferenceDataCollectionBase<TItem>
                where TItem : ReferenceDataBase, new()
        {
            Check.NotNull(coll, nameof(coll));

            DatabaseRecordFieldCollection? fields = null;
            var idCol = idColumnName ?? DatabaseRefDataColumns.IdColumnName;
            var typeCode = ReferenceDataBase.GetIdTypeCode(typeof(TItem));

            var list = new List<Action<DatabaseRecord>>
            {
                (dr) =>
                {
                    if (fields == null)
                    {
                        fields = dr.GetFields();
                        if (!fields.Contains(idCol) || !fields.Contains(DatabaseRefDataColumns.CodeColumnName))
                            throw new InvalidOperationException("The query must return as a minimum the Id and Code columns as per the configured names.");
                    }

                    TItem item = new TItem()
                    {
                        Id = typeCode switch
                        {
                            ReferenceDataIdTypeCode.Guid => (object)dr.GetValue<Guid>(fields[idCol].Index),
                            ReferenceDataIdTypeCode.Int32 => (object)dr.GetValue<int>(fields[idCol].Index),
                            ReferenceDataIdTypeCode.Int64 => (object)dr.GetValue<long>(fields[idCol].Index),
                            _ => (object)dr.GetValue<string>(fields[idCol].Index)
                        },
                        Code = dr.GetValue<string>(fields[DatabaseRefDataColumns.CodeColumnName].Index),
                        Text = !fields.Contains(DatabaseRefDataColumns.TextColumnName) ? null : dr.GetValue<string>(fields[DatabaseRefDataColumns.TextColumnName].Index),
                        Description = !fields.Contains(DatabaseRefDataColumns.DescriptionColumnName) ? null : dr.GetValue<string>(fields[DatabaseRefDataColumns.DescriptionColumnName].Index),
                        SortOrder = !fields.Contains(DatabaseRefDataColumns.SortOrderColumnName) ? 0 : dr.GetValue<int>(fields[DatabaseRefDataColumns.SortOrderColumnName].Index),
                        IsActive = !fields.Contains(DatabaseRefDataColumns.IsActiveColumnName) || dr.GetValue<bool>(fields[DatabaseRefDataColumns.IsActiveColumnName].Index),
                        StartDate = !fields.Contains(DatabaseRefDataColumns.StartDateColumnName) ? null : dr.GetValue<DateTime?>(fields[DatabaseRefDataColumns.StartDateColumnName].Index),
                        EndDate = !fields.Contains(DatabaseRefDataColumns.EndDateColumnName) ? null : dr.GetValue<DateTime?>(fields[DatabaseRefDataColumns.EndDateColumnName].Index),
                        ETag = !fields.Contains(DatabaseRefDataColumns.ETagColumnName) ? null : dr.GetRowVersion(fields[DatabaseRefDataColumns.ETagColumnName].Index)
                    };

                    var cl = new Beef.Entities.ChangeLog
                    {
                        CreatedBy = !fields.Contains(DatabaseColumns.CreatedByName) ? null : dr.GetValue<string>(fields[DatabaseColumns.CreatedByName].Index),
                        CreatedDate = !fields.Contains(DatabaseColumns.CreatedDateName) ? (DateTime?)null : dr.GetValue<DateTime>(fields[DatabaseColumns.CreatedDateName].Index),
                        UpdatedBy = !fields.Contains(DatabaseColumns.UpdatedByName) ? null : dr.GetValue<string>(fields[DatabaseColumns.UpdatedByName].Index),
                        UpdatedDate = !fields.Contains(DatabaseColumns.UpdatedDateName) ? (DateTime?)null : dr.GetValue<DateTime>(fields[DatabaseColumns.UpdatedDateName].Index)
                    };

                    if (!cl.IsInitial)
                        item.ChangeLog = cl;

                    additionalProperties?.Invoke(dr, item, fields);

                    if (confirmItemIsToBeAdded == null || confirmItemIsToBeAdded(dr, item))
                        coll.Add(item);
                }
            };

            if (additionalDatasetRecords != null && additionalDatasetRecords.Length > 0)
                list.AddRange(additionalDatasetRecords);

            await StoredProcedure(storedProcedure).SelectQueryMultiSetAsync(list.ToArray()).ConfigureAwait(false);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="DatabaseBase"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_connection != null)
                    {
                        Logger.LogDebug("Closing and disposing the database connection. DatabaseId: {0}", DatabaseId);
                        _connection.Close();
                        _connection.Dispose();
                    }
                }

                _disposed = true;
            }
        }

        /// <summary>
        /// Closes and disposes the <see cref="DatabaseBase"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}