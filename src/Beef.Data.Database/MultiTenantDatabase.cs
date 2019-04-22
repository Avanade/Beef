// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Beef.Data.Database
{
    /// <summary>
    /// Extends <see cref="DatabaseBase"/> adding <see cref="Register"/>, <see cref="Default"/>, <see cref="OnConnectionOpen(DbConnection)"/> and <see cref="SetSqlSessionContext(DbConnection)"/> capabilities
    /// specifically within a multi-tenancy database scenario using the <see cref="ExecutionContext"/> <see cref="ExecutionContext.TenantId"/> as means to identify each unique tenant. Per tenant <see cref="DatabaseBase"/> 
    /// is cached for each tenant after first creation. The <see cref="Default"/> will represent the <see cref="DatabaseBase"/> for the current tenant.
    /// </summary>
    /// <typeparam name="TDefault">The <see cref="DatabaseBase"/> <see cref="Type"/>.</typeparam>
    public abstract class MultiTenantDatabase<TDefault> : DatabaseBase where TDefault : MultiTenantDatabase<TDefault>
    {
        private static readonly object _lock = new object();

        private static Dictionary<Guid, TDefault> _tenantDBs = new Dictionary<Guid, TDefault>();
        private static Func<Guid, TDefault> _create;

        /// <summary>
        /// Registers (creates) the <see cref="Default"/> <see cref="DatabaseBase"/> instance; as well as registering with the <see cref="DataContextScope"/> for connection management
        /// and initiating the <see cref="OnConnectionOpen(DbConnection)"/>.
        /// </summary>
        /// <param name="create">Function to create the <see cref="Default"/> instance for the specified tenant identifier.</param>
        public static void Register(Func<Guid, TDefault> create)
        {
            lock (_lock)
            {
                if (_create != null)
                    throw new InvalidOperationException("The Register method can only be invoked once.");

                _create = create ?? throw new ArgumentNullException(nameof(create));

                DataContextScope.RegisterContext<TDefault, DbConnection>(() =>
                {
                    var conn = Default.CreateConnection(false);
                    conn.Open();

                    Default.OnConnectionOpen(conn);

                    return conn;
                });
            }
        }

        /// <summary>
        /// Gets the current default <see cref="DatabaseBase"/> instance.
        /// </summary>
        public static TDefault Default => GetTenantDatabase(GetTenantId());

        /// <summary>
        /// Gets the tenant identifier (see <see cref="ExecutionContext.TenantId"/> from the <see cref="ExecutionContext"/> ensuring the value has been set.
        /// </summary>
        /// <returns>The tenant identifier.</returns>
        public static Guid GetTenantId()
        {
            if (!ExecutionContext.HasCurrent)
                throw new InvalidOperationException("The ExecutionContext needs a Current instance for the Tenant identifier to be established.");

            var ec = ExecutionContext.Current;
            if (!ec.TenantId.HasValue)
                throw new InvalidOperationException("The ExecutionContext.TenantId must be set on the Current instance for the Tenant identifier to be established.");

            return ec.TenantId.Value;
        }

        /// <summary>
        /// Gets the <see cref="DatabaseBase"/> instance for the specified tenant identifier.
        /// </summary>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <returns>The <see cref="DatabaseBase"/> instance.</returns>
        public static TDefault GetTenantDatabase(Guid tenantId)
        {
            if (_tenantDBs.TryGetValue(tenantId, out TDefault database))
                return database;

            lock (_lock)
            {
                if (_tenantDBs.TryGetValue(tenantId, out database))
                    return database;

                if (_create == null)
                    throw new InvalidOperationException("The Register method must be invoked before this property can be accessed.");

                database = _create(tenantId) ?? throw new InvalidOperationException("The registered create function must create an instance.");
                _tenantDBs.Add(tenantId, database);
                return database;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MultiTenantDatabase{T}"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="provider">The optional data provider (e.g. System.Data.SqlClient); defaults to <see cref="SqlClientFactory"/>.</param>
        public MultiTenantDatabase(string connectionString, DbProviderFactory provider = null) : base(connectionString, provider) { }

        /// <summary>
        /// Gets or sets the stored procedure name used by <see cref="SetSqlSessionContext(DbConnection)"/>; defaults to '[dbo].[spSetSessionContext]'.
        /// </summary>
        public string SessionContextStoredProcedure { get; set; } = "[dbo].[spSetSessionContext]";

        /// <summary>
        /// Sets the SQL session context using the specified values by invoking the <see cref="SessionContextStoredProcedure"/> using parameters named
        /// <see cref="DatabaseColumns.SessionContextUsername"/> and <see cref="DatabaseColumns.SessionContextTimestamp"/>.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        /// <param name="tenantId">The tenant identifier.</param>
        /// <param name="username">The username.</param>
        /// <param name="timestamp">The timestamp <see cref="DateTime"/>.</param>
        /// <remarks>Where both the <paramref name="username"/> and <paramref name="timestamp"/> are <c>null</c> the stored procedure will not be invoked.</remarks>
        public void SetSqlSessionContext(DbConnection dbConnection, Guid tenantId, string username, DateTime? timestamp)
        {
            if (string.IsNullOrEmpty(SessionContextStoredProcedure))
                throw new InvalidOperationException("The SessionContextStoredProcedure property must have a value.");

            var cmd = dbConnection.CreateCommand();
            cmd.CommandText = SessionContextStoredProcedure;
            cmd.CommandType = CommandType.StoredProcedure;

            var p = cmd.CreateParameter();
            p.ParameterName = "@" + DatabaseColumns.SessionContextTenantId;
            p.Value = tenantId;
            cmd.Parameters.Add(p);

            p = cmd.CreateParameter();
            p.ParameterName = "@" + DatabaseColumns.SessionContextUsername;
            p.Value = username;
            cmd.Parameters.Add(p);

            p = cmd.CreateParameter();
            p.ParameterName = "@" + DatabaseColumns.SessionContextTimestamp;
            p.Value = timestamp;
            cmd.Parameters.Add(p);

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Sets the SQL session context using the <see cref="ExecutionContext"/> (invokes <see cref="SetSqlSessionContext(DbConnection, Guid, string, DateTime?)"/> using
        /// <see cref="ExecutionContext.TenantId"/>, <see cref="ExecutionContext.Username"/> and <see cref="ExecutionContext.Timestamp"/>).
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        public void SetSqlSessionContext(DbConnection dbConnection)
        {
            var ec = ExecutionContext.Current ?? throw new InvalidOperationException("The ExecutionContext.Current must have an instance to SetSqlSessionContext.");
            SetSqlSessionContext(dbConnection, ec.TenantId ?? throw new InvalidOperationException("The ExecutionContext.Current must have a TenantId specified."), ec.Username, ec.Timestamp);
        }

        /// <summary>
        /// Occurs when a connection is opened before any corresponding data access is performed.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        /// <remarks>This is where the <see cref="SetSqlSessionContext(DbConnection)"/> should be invoked; nothing is performed by default.</remarks>
        public virtual void OnConnectionOpen(DbConnection dbConnection) { }
    }
}
