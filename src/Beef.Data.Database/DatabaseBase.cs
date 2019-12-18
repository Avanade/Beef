﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Mapper;
using Beef.RefData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace Beef.Data.Database
{
    /// <summary>
    /// Represents the base class for encapsulating the database access layer using old skool ADO.NET - because sometimes it is all you need, and it is super efficient.
    /// </summary>
    /// <remarks>Provides automatic database connection management by leveraging the <see cref="DataContextScope"/>.</remarks>
    public abstract class DatabaseBase
    {
        private DbConnection _connection;

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
        /// <param name="provider">The optional data provider (e.g. System.Data.SqlClient); defaults to <see cref="SqlClientFactory"/>.</param>
        protected DatabaseBase(string connectionString, DbProviderFactory provider = null)
        {
            ConnectionString = !string.IsNullOrEmpty(connectionString) ? connectionString : throw new ArgumentNullException(nameof(connectionString));
            Provider = provider ?? SqlClientFactory.Instance;
        }

        /// <summary>
        /// Gets the name of the connection string settings as defined in the application configuration.
        /// </summary>
        public string ConnectionName { get; private set; }

        /// <summary>
        /// Gets the database connection string.
        /// </summary>
        public string ConnectionString { get; private set; }

        /// <summary>
        /// Gets the <see cref="DbProviderFactory"/>.
        /// </summary>
        public DbProviderFactory Provider { get; private set; }

        /// <summary>
        /// Gets or sets the <see cref="DatabaseWildcard"/> to enable wildcard replacement.
        /// </summary>
        public DatabaseWildcard Wildcard { get; set; } = new DatabaseWildcard();

        /// <summary>
        /// Defines the default <see cref="DateTimeKind"/> to be used when retrieving (see <see cref="DatabaseRecord.GetValue{DateTime}(string)"/>)
        /// a <see cref="DateTime"/> value from a <see cref="DatabaseRecord"/>.
        /// </summary>
        public DateTimeKind DefaultDateTimeKind { get; set; } = DateTimeKind.Local;

        /// <summary>
        /// Gets or sets the <see cref="SqlException"/> handler (by default set up to execute <see cref="ThrowTransformedSqlException(SqlException)"/>).
        /// </summary>
        public Action<SqlException> ExceptionHandler { get; set; } = (sex) => ThrowTransformedSqlException(sex);

        /// <summary>
        /// Sets up the <see cref="DatabaseBase"/> to always use the <paramref name="connection"/> bypassing the <see cref="DataContextScope"/>.
        /// </summary>
        /// <param name="connection">The <see cref="DbConnection"/> (where <c>null</c> it will create automatically bypassing the <see cref="DataContextScope"/>).</param>
        /// <returns>A <see cref="DbConnection"/>.</returns>
        public DbConnection SetBypassDataContextScopeDbConnection(DbConnection connection = null)
        {
            _connection = connection ?? CreateConnection(false);
            return _connection;
        }

        /// <summary>
        /// Creates a <see cref="DbConnection"/>.
        /// </summary>
        /// <param name="useDataContextScope">Indicates whether to use the <see cref="DataContextScope"/>; defaults to <c>true</c>.</param>
        /// <returns>A <see cref="DbConnection"/>.</returns>
        /// <remarks>Where <see cref="SetBypassDataContextScopeDbConnection"/> has been invoked this connection will always be returned; no new connection will be created.</remarks>
        public DbConnection CreateConnection(bool useDataContextScope = true)
        {
            if (_connection != null)
                return _connection;

            if (!useDataContextScope || DataContextScope.Current == null)
            {
                DbConnection conn = Provider.CreateConnection();
                conn.ConnectionString = ConnectionString;
                return conn;
            }
            else
                return (DbConnection)DataContextScope.Current.GetContext(this.GetType());
        }

        /// <summary>
        /// Creates a stored procedure <see cref="DatabaseCommand"/>.
        /// </summary>
        /// <param name="storedProcedure">The stored procedure name.</param>
        /// <returns>A <see cref="DatabaseCommand"/>.</returns>
        public DatabaseCommand StoredProcedure(string storedProcedure)
        {
            return new DatabaseCommand(this, storedProcedure, CommandType.StoredProcedure);
        }

        /// <summary>
        /// Creates a SQL statement <see cref="DatabaseCommand"/>.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <returns>A <see cref="DatabaseCommand"/>.</returns>
        public DatabaseCommand SqlStatement(string sqlStatement)
        {
            return new DatabaseCommand(this, sqlStatement, CommandType.Text);
        }

        /// <summary>
        /// Creates a <see cref="DatabaseQuery{T}"/> to enable select-like capabilities.
        /// </summary>
        /// <param name="queryArgs">The <see cref="DatabaseArgs{T}"/>.</param>
        /// <param name="queryParams">The query <see cref="DatabaseParameters"/> delegate.</param>
        /// <returns>A <see cref="DatabaseQuery{T}"/>.</returns>
        public DatabaseQuery<T> Query<T>(DatabaseArgs<T> queryArgs, Action<DatabaseParameters> queryParams = null) where T : class, new()
        {
            return new DatabaseQuery<T>(this, queryArgs, queryParams);
        }

        /// <summary>
        /// Gets the entity for the specified <paramref name="keys"/> mapping to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="getArgs">The <see cref="DatabaseArgs{T}"/>.</param>
        /// <param name="keys">The key values.</param>
        /// <returns>The entity value where found; otherwise, <c>null</c>.</returns>
        public T Get<T>(DatabaseArgs<T> getArgs, params IComparable[] keys) where T : class, new()
        {
            if (getArgs == null)
                throw new ArgumentNullException(nameof(getArgs));

            return StoredProcedure(getArgs.StoredProcedure).Params((p) => getArgs.Mapper.GetKeyParams(p, OperationTypes.Get, keys)).SelectFirstOrDefault<T>(getArgs.Mapper);
        }

        /// <summary>
        /// Performs a create for the specified stored procedure and value (reselects where specified).
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="DatabaseArgs{T}"/>.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>The value (reselected where specified).</returns>
        /// <remarks>Automatically invokes <see cref="DatabaseParameters.AddChangeLogParameters(Entities.ChangeLog, bool, bool, ParameterDirection)"/>.</remarks>
        public T Create<T>(DatabaseArgs<T> saveArgs, T value) where T : class, new()
        {
            if (saveArgs == null)
                throw new ArgumentNullException(nameof(saveArgs));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var cmd = StoredProcedure(saveArgs.StoredProcedure);
            saveArgs.Mapper.GetKeyParams(cmd.Parameters, OperationTypes.Create, value);
            cmd.Params<T>(value, saveArgs.Mapper.MapToDb, OperationTypes.Create);

            if (saveArgs.Refresh)
            {
                cmd.Param("@" + DatabaseColumns.ReselectRecordName, true);
                return cmd.SelectFirstOrDefault<T>(saveArgs.Mapper) ?? throw new NotFoundException();
            }

            // NOTE: without refresh, fields like IDs and RowVersion are not automatically updated.
            cmd.NonQuery();
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
        public T Update<T>(DatabaseArgs<T> saveArgs, T value) where T : class, new()
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
                return cmd.SelectFirstOrDefault<T>(saveArgs.Mapper) ?? throw new NotFoundException();
            }

            // NOTE: without refresh, fields like IDs and RowVersion are not automatically updated.
            cmd.NonQuery();
            return value;
        }

        /// <summary>
        /// Performs a delete for the specified <paramref name="keys"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="DatabaseArgs{T}"/>.</param>
        /// <param name="keys">The key values.</param>
        public void Delete<T>(DatabaseArgs<T> saveArgs, params IComparable[] keys) where T : class, new()
        {
            if (saveArgs == null)
                throw new ArgumentNullException(nameof(saveArgs));

            StoredProcedure(saveArgs.StoredProcedure).Params((p) => saveArgs.Mapper.GetKeyParams(p, OperationTypes.Delete, keys)).NonQuery();
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
        public void GetRefData<TColl, TItem>(TColl coll, string storedProcedure, string idColumnName = null,
            Action<DatabaseRecord, TItem, DatabaseRecordFieldCollection> additionalProperties = null,
            Action<DatabaseRecord>[] additionalDatasetRecords = null,
            Func<DatabaseRecord, TItem, bool> confirmItemIsToBeAdded = null)
                where TColl : ReferenceDataCollectionBase<TItem>
                where TItem : ReferenceDataBase, new()
        {
            Check.NotNull(coll, nameof(coll));

            DatabaseRecordFieldCollection fields = null;
            var idCol = idColumnName ?? DatabaseRefDataColumns.IdColumnName;
            var isInt = ReferenceDataBase.GetIdTypeCode(typeof(TItem)) == ReferenceDataIdTypeCode.Int32;

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
                        Id = isInt ? (object)dr.GetValue<int>(fields[idCol].Index) : (object)dr.GetValue<Guid>(fields[idCol].Index),
                        Code = dr.GetValue<string>(fields[DatabaseRefDataColumns.CodeColumnName].Index),
                        Text = !fields.Contains(DatabaseRefDataColumns.TextColumnName) ? null : dr.GetValue<string>(fields[DatabaseRefDataColumns.TextColumnName].Index),
                        Description = !fields.Contains(DatabaseRefDataColumns.DescriptionColumnName) ? null : dr.GetValue<string>(fields[DatabaseRefDataColumns.DescriptionColumnName].Index),
                        SortOrder = !fields.Contains(DatabaseRefDataColumns.SortOrderColumnName) ? 0 : dr.GetValue<int>(fields[DatabaseRefDataColumns.SortOrderColumnName].Index),
                        IsActive = !fields.Contains(DatabaseRefDataColumns.IsActiveColumnName) ? true : dr.GetValue<bool>(fields[DatabaseRefDataColumns.IsActiveColumnName].Index),
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

            StoredProcedure(storedProcedure).SelectQueryMultiSet(list.ToArray());
        }
    }
}
