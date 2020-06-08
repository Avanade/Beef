// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Data.Common;

namespace Beef.Data.Database
{
    /// <summary>
    /// Extends <see cref="DatabaseBase"/> adding <see cref="Register"/>, <see cref="Default"/>, <see cref="OnConnectionOpen(DbConnection)"/> and <see cref="SetSqlSessionContext(DbConnection)"/> capabilities.
    /// </summary>
    /// <typeparam name="TDefault">The <see cref="DatabaseBase"/> <see cref="Type"/>.</typeparam>
#pragma warning disable CA1724 // Type name conflicts with namespace; by-design, is ok as it is a generic type.
    public abstract class Database<TDefault> : DatabaseBase where TDefault : Database<TDefault>
#pragma warning restore CA1724
    {
        private static readonly object _lock = new object();
        private static TDefault? _default;
        private static Func<TDefault>? _create;

#pragma warning disable CA1000 // Do not declare static members on generic types; by-design, is ok.

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

#pragma warning restore CA1000

        /// <summary>
        /// Initializes a new instance of the <see cref="Database{T}"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="provider">The optional data provider (e.g. Microsoft.Data.SqlClient); defaults to <see cref="SqlClientFactory"/>.</param>
        protected Database(string connectionString, DbProviderFactory? provider = null) : base(connectionString, provider) { }

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
        /// <param name="timestamp">The timestamp <see cref="DateTime"/> (where <c>null</c> the value will default to <see cref="DateTime.Now"/>).</param>
        /// <param name="tenantId">The tenant identifer (where <c>null</c> the value will not be used).</param>
        /// <param name="userId">The unique user identifier.</param>
        public void SetSqlSessionContext(DbConnection dbConnection, string username, DateTime? timestamp, Guid? tenantId = null, Guid? userId = null)
        {
            if (dbConnection == null)
                throw new ArgumentNullException(nameof(dbConnection));

            if (string.IsNullOrEmpty(SessionContextStoredProcedure))
                throw new InvalidOperationException("The SessionContextStoredProcedure property must have a value.");

            var cmd = dbConnection.CreateCommand();
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities; by-design, is a stored procedure command type.
            cmd.CommandText = SessionContextStoredProcedure;
#pragma warning restore CA2100
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

            if (userId.HasValue)
            {
                p = cmd.CreateParameter();
                p.ParameterName = "@" + DatabaseColumns.SessionContextUserId;
                p.Value = userId.Value;
                cmd.Parameters.Add(p);
            }

            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Sets the SQL session context using the <see cref="ExecutionContext"/> (invokes <see cref="SetSqlSessionContext(DbConnection, string, DateTime?, Guid?, Guid?)"/> using
        /// <see cref="ExecutionContext.Username"/>, <see cref="ExecutionContext.Timestamp"/> and <see cref="ExecutionContext.TenantId"/>).
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        public void SetSqlSessionContext(DbConnection dbConnection)
        {
            var ec = ExecutionContext.Current ?? throw new InvalidOperationException("The ExecutionContext.Current must have an instance to SetSqlSessionContext.");
            SetSqlSessionContext(dbConnection, ec.Username, ec.Timestamp, ec.TenantId, ec.UserId);
        }

        /// <summary>
        /// Occurs when a connection is opened before any corresponding data access is performed.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/>.</param>
        /// <remarks>This is where the <see cref="SetSqlSessionContext(DbConnection)"/> should be invoked; nothing is performed by default.</remarks>
        public virtual void OnConnectionOpen(DbConnection dbConnection) { }
    }
}