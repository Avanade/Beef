// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace Beef.Data.Database
{
    /// <summary>
    /// Extends <see cref="DatabaseBase"/> adding <see cref="Register"/>, <see cref="Default"/>, <see cref="OnConnectionOpen(DbConnection)"/> and <see cref="SetSqlSessionContext(DbConnection)"/> capabilities.
    /// </summary>
    /// <typeparam name="TDefault">The <see cref="DatabaseBase"/> <see cref="Type"/>.</typeparam>
    public abstract class Database<TDefault> : DatabaseBase where TDefault : Database<TDefault>
    {
        private static readonly object _lock = new object();
        private static TDefault _default;
        private static Func<TDefault> _create;

        /// <summary>
        /// Registers (creates) the <see cref="Default"/> <see cref="DatabaseBase"/> instance; as well as registering with the <see cref="DataContextScope"/> for connection management
        /// and initiating the <see cref="OnConnectionOpen(DbConnection)"/>.
        /// </summary>
        /// <param name="create">Function to create the <see cref="Default"/> instance.</param>
        public static void Register(Func<TDefault> create)
        {
            lock (_lock)
            {
                if (_default != null)
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
        public static TDefault Default
        {
            get
            {
                if (_default != null)
                    return _default;

                lock (_lock)
                {
                    if (_default != null)
                        return _default;

                    if (_create == null)
                        throw new InvalidOperationException("The Register method must be invoked before this property can be accessed.");

                    _default = _create() ?? throw new InvalidOperationException("The registered create function must create a default instance.");
                    return _default;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Database{T}"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="provider">The optional data provider (e.g. System.Data.SqlClient); defaults to <see cref="SqlClientFactory"/>.</param>
        public Database(string connectionString, DbProviderFactory provider = null) : base(connectionString, provider) { }

        /// <summary>
        /// Gets or sets the stored procedure name used by <see cref="SetSqlSessionContext(DbConnection)"/>; defaults to '[dbo].[spSetSessionContext]'.
        /// </summary>
        public string SessionContextStoredProcedure { get; set; } = "[dbo].[spSetSessionContext]";

        /// <summary>
        /// Sets the SQL session context using the specified values by invoking the <see cref="SessionContextStoredProcedure"/> using parameters named
        /// <see cref="DatabaseColumns.SessionContextUsername"/> and <see cref="DatabaseColumns.SessionContextTimestamp"/>.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        /// <param name="username">The username.</param>
        /// <param name="timestamp">The timestamp <see cref="DateTime"/>.</param>
        /// <param name="tenantId">The tenant identifer (where <c>null</c> the value will not be used).</param>
        public void SetSqlSessionContext(DbConnection dbConnection, string username, DateTime? timestamp, Guid? tenantId = null)
        {
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
            p.Value = timestamp;
            cmd.Parameters.Add(p);

            if (tenantId.HasValue)
            {
                p = cmd.CreateParameter();
                p.ParameterName = "@" + DatabaseColumns.SessionContextTenantId;
                p.Value = tenantId.Value;
                cmd.Parameters.Add(p);
            }

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Sets the SQL session context using the <see cref="ExecutionContext"/> (invokes <see cref="SetSqlSessionContext(DbConnection, string, DateTime?, Guid?)"/> using
        /// <see cref="ExecutionContext.Username"/>, <see cref="ExecutionContext.Timestamp"/> and <see cref="ExecutionContext.TenantId"/>).
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        public void SetSqlSessionContext(DbConnection dbConnection)
        {
            var ec = ExecutionContext.Current ?? throw new InvalidOperationException("The ExecutionContext.Current must have an instance to SetSqlSessionContext.");
            SetSqlSessionContext(dbConnection, ec.Username, ec.Timestamp, ec.TenantId);
        }

        /// <summary>
        /// Occurs when a connection is opened before any corresponding data access is performed.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        /// <remarks>This is where the <see cref="SetSqlSessionContext(DbConnection)"/> should be invoked; nothing is performed by default.</remarks>
        public virtual void OnConnectionOpen(DbConnection dbConnection) { }
    }
}
