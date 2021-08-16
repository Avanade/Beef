// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Mapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using System.Web;

namespace Beef.Data.Database
{
    /// <summary>
    /// Encapsulates the <see cref="DbCommand"/> adding additional features to support the likes of method chaining (fluent interface).
    /// </summary>
    public class DatabaseCommand
    {
        private const string _mapperNullResultMessage = "Mapper must result in a non-null result.";

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseCommand"/> class.
        /// </summary>
        /// <param name="database">The <see cref="DatabaseBase"/>.</param>
        /// <param name="commandText">The command text.</param>
        /// <param name="commandType">The <see cref="CommandType"/>.</param>
        public DatabaseCommand(DatabaseBase database, string commandText, CommandType commandType = CommandType.StoredProcedure)
        {
            if (string.IsNullOrEmpty(commandText))
                throw new ArgumentNullException(nameof(commandText));

            Database = database ?? throw new ArgumentNullException(nameof(database));
            DbCommand = Database.Provider.CreateCommand();
            DbCommand.CommandText = commandText;
            DbCommand.CommandType = commandType;
            Parameters = new DatabaseParameters(this);
        }

        /// <summary>
        /// Gets the <see cref="DatabaseBase"/>.
        /// </summary>
        public DatabaseBase Database { get; private set; }

        /// <summary>
        /// Gets the underlying <see cref="DbCommand"/>.
        /// </summary>
        public DbCommand DbCommand { get; private set; }

        /// <summary>
        /// Gets the <see cref="DatabaseParameters"/>.
        /// </summary>
        public DatabaseParameters Parameters { get; private set; }

        #region Param

        /// <summary>
        /// Adds a named parameter and value.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseCommand"/> instance to support chaining (fluent interface).</returns>
        public DatabaseCommand Param(string name, object value, ParameterDirection direction = ParameterDirection.Input)
        {
            Parameters.AddParameter(name, value, direction);
            return this;
        }

        /// <summary>
        /// Adds a named parameter, <see cref="DbType"/> and value.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="dbType">The parameter <see cref="DbType"/>.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseCommand"/> instance to support chaining (fluent interface).</returns>
        public DatabaseCommand Param(string name, object value, DbType dbType, ParameterDirection direction = ParameterDirection.Input)
        {
            Parameters.AddParameter(name, value, dbType, direction);
            return this;
        }

        /// <summary>
        /// Adds a named parameter, <see cref="SqlDbType"/> and value.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="sqlDbType">The parameter <see cref="SqlDbType"/>.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseCommand"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>This specifically implies that the <see cref="Microsoft.Data.SqlClient.SqlParameter"/> is being used; if not then an exception will be thrown.</remarks>
        public DatabaseCommand Param(string name, object value, SqlDbType sqlDbType, ParameterDirection direction = ParameterDirection.Input)
        {
            Parameters.AddParameter(name, value, sqlDbType, direction);
            return this;
        }

        /// <summary>
        /// Adds a named parameter with specified <see cref="DbType"/>.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="dbType">The <see cref="DbType"/>.</param>
        /// <param name="size">The maximum size (in bytes).</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseCommand"/> instance to support chaining (fluent interface).</returns>
        public DatabaseCommand Param(string name, DbType dbType, int size = 0, ParameterDirection direction = ParameterDirection.Input)
        {
            Parameters.AddParameter(name, dbType, size, direction);
            return this;
        }

        /// <summary>
        /// Adds a named parameter with specified <see cref="SqlDbType"/>.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="sqlDbType">The <see cref="SqlDbType"/>.</param>
        /// <param name="size">The maximum size (in bytes).</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseCommand"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>This specifically implies that the <see cref="Microsoft.Data.SqlClient.SqlParameter"/> is being used; if not then an exception will be thrown.</remarks>
        public DatabaseCommand Param(string name, SqlDbType sqlDbType, int size = 0, ParameterDirection direction = ParameterDirection.Input)
        {
            Parameters.AddParameter(name, sqlDbType, size, direction);
            return this;
        }

        /// <summary>
        /// Adds a named <b>SQL Server</b> <see cref="TableValuedParameter"/>.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="tvp">The <see cref="TableValuedParameter"/>.</param>
        /// <returns>The current <see cref="DatabaseCommand"/> instance to support chaining (fluent interface).</returns>
        public DatabaseCommand Param(string name, TableValuedParameter tvp)
        {
            Parameters.AddTableValuedParameter(name, tvp);
            return this;
        }

        #endregion

        #region RowVersionParam

        /// <summary>
        /// Adds a named <b>RowVersion</b> parameter.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The <b>RowVersion</b> parameter value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (defaults to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseCommand"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>The <b>RowVersion</b> <see cref="byte"/> array will be converted from an <see cref="HttpUtility.UrlDecode(string)">encoded</see> <see cref="string"/> value.</remarks>
        public DatabaseCommand RowVersionParam(string name, string value, ParameterDirection direction = ParameterDirection.Input)
        {
            Parameters.AddRowVersionParameter(name, value, direction);
            return this;
        }

        /// <summary>
        /// Adds a <b>RowVersion</b> (named <see cref="DatabaseColumns.RowVersionName"/>) parameter.
        /// </summary>
        /// <param name="value">The <b>RowVersion</b> parameter value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (defaults to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseCommand"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>The <b>RowVersion</b> <see cref="byte"/> array will be converted from an <see cref="HttpUtility.UrlDecode(string)">encoded</see> <see cref="string"/> value.</remarks>
        public DatabaseCommand RowVersionParam(string value, ParameterDirection direction = ParameterDirection.Input)
        {
            Parameters.AddRowVersionParameter(value, direction);
            return this;
        }

        /// <summary>
        /// Adds a named <b>RowVersion</b> parameter where the <paramref name="value"/> implements <see cref="IETag"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (defaults to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseCommand"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>The <b>RowVersion</b> <see cref="byte"/> array will be converted from an <see cref="HttpUtility.UrlDecode(string)">encoded</see> <see cref="string"/> value.</remarks>
        public DatabaseCommand RowVersionParam<T>(string name, T value, ParameterDirection direction = ParameterDirection.Input) where T : class
        {
            Parameters.AddRowVersionParameter<T>(name, value, direction);
            return this;
        }

        /// <summary>
        /// Adds a named <b>RowVersion</b> parameter (named <see cref="DatabaseColumns.RowVersionName"/>) where the <paramref name="value"/> implements <see cref="IETag"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (defaults to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseCommand"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>The <b>RowVersion</b> <see cref="byte"/> array will be converted from an <see cref="HttpUtility.UrlDecode(string)">encoded</see> <see cref="string"/> value.</remarks>
        public DatabaseCommand RowVersionParam<T>(T value, ParameterDirection direction = ParameterDirection.Input) where T : class
        {
            Parameters.AddRowVersionParameter<T>(value, direction);
            return this;
        }

        #endregion

        #region ReturnValueParam

        /// <summary>
        /// Adds a <see cref="ParameterDirection.ReturnValue"/> (named <see cref="DatabaseColumns.ReturnValueName"/>) parameter to the command.
        /// </summary>
        /// <returns>The current <see cref="DatabaseCommand"/> instance to support chaining (fluent interface).</returns>
        public DatabaseCommand ReturnValueParam()
        {
            Parameters.AddReturnValueParameter();
            return this;
        }

        #endregion

        #region ChangeLogParams

        /// <summary>
        /// Adds the <see cref="ChangeLog"/> parameters.
        /// </summary>
        /// <param name="changeLog">The <see cref="ChangeLog"/> value.</param>
        /// <param name="addCreatedParams">Indicates whether to add the <b>created</b> <see cref="IChangeLog"/> parameters.</param>
        /// <param name="addUpdatedParams">Indicates whether to add the <b>updated</b> <see cref="IChangeLog"/> parameters.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (defaults to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseCommand"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>Uses the following parameter names: <see cref="DatabaseColumns.CreatedByName"/>, <see cref="DatabaseColumns.CreatedDateName"/>,
        /// <see cref="DatabaseColumns.UpdatedByName"/> and <see cref="DatabaseColumns.UpdatedDateName"/>.</remarks>
        public DatabaseCommand ChangeLogParams(ChangeLog changeLog, bool addCreatedParams = false, bool addUpdatedParams = false, ParameterDirection direction = ParameterDirection.Input)
        {
            Parameters.AddChangeLogParameters(changeLog, addCreatedParams, addUpdatedParams, direction);
            return this;
        }

        /// <summary>
        /// Adds the <see cref="ChangeLog"/> parameters where the <paramref name="value"/> implements <see cref="IChangeLog"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="addCreatedParams">Indicates whether to add the <b>created</b> <see cref="IChangeLog"/> parameters.</param>
        /// <param name="addUpdatedParams">Indicates whether to add the <b>updated</b> <see cref="IChangeLog"/> parameters.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (defaults to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseCommand"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>Uses the following parameter names: <see cref="DatabaseColumns.CreatedByName"/>, <see cref="DatabaseColumns.CreatedDateName"/>,
        /// <see cref="DatabaseColumns.UpdatedByName"/> and <see cref="DatabaseColumns.UpdatedDateName"/>.</remarks>
        public DatabaseCommand ChangeLogParams<T>(T value, bool addCreatedParams = false, bool addUpdatedParams = false, ParameterDirection direction = ParameterDirection.Input) where T : class
        {
            Parameters.AddChangeLogParameters<T>(value, addCreatedParams, addUpdatedParams, direction);
            return this;
        }

        #endregion

        #region PagingsParams

        /// <summary>
        /// Adds the <see cref="PagingArgs"/> parameters.
        /// </summary>
        /// <param name="paging">The <see cref="PagingArgs"/> value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (defaults to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>An <see cref="DbParameter"/> array for those that were added.</returns>
        /// <remarks>Uses the following parameter names: <see cref="DatabaseColumns.PagingSkipName"/> and <see cref="DatabaseColumns.PagingTakeName"/>.</remarks>

        public DatabaseCommand PagingParams(PagingArgs paging, ParameterDirection direction = ParameterDirection.Input)
        {
            Parameters.AddPagingParameters(paging, direction);
            return this;
        }

        #endregion

        #region TableValuedParam

        /// <summary>
        /// Adds a <b>SQL Server</b> <see cref="TableValuedParameter"/>.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="tvp">The <see cref="TableValuedParameter"/>.</param>
        /// <returns>The current <see cref="DatabaseCommand"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>This specifically implies that the <see cref="SqlParameter"/> is being used; if not then an exception will be thrown.</remarks>
        public DatabaseCommand TableValuedParam(string name, TableValuedParameter tvp)
        {
            Parameters.TableValuedParam(name, tvp);
            return this;
        }

        #endregion

        #region ReselectRecordParam

        /// <summary>
        /// Adds a <b>ReselectRecord</b> (named <see cref="DatabaseColumns.ReselectRecordName"/>) parameter to the command.
        /// </summary>
        /// <param name="reselect">Indicates whether to reselect after the operation (defaults to <c>true</c>).</param>
        /// <returns>The current <see cref="DatabaseCommand"/> instance to support chaining (fluent interface).</returns>
        public DatabaseCommand ReselectRecordParam(bool reselect = true)
        {
            Parameters.ReselectRecordParam(reselect);
            return this;
        }

        #endregion

        #region Params

        /// <summary>
        /// Add one or more parameters by invoking a delegate.
        /// </summary>
        /// <param name="action">The delegate to enable parameter addition.</param>
        /// <returns>The current <see cref="DatabaseCommand"/> instance to support chaining (fluent interface).</returns>
        public DatabaseCommand Params(Action<DatabaseParameters> action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            action.Invoke(new DatabaseParameters(this));
            return this;
        }

        /// <summary>
        /// Add one or more parameters by invoking a delegate using the passed value.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="action">The delegate to enable parameter addition.</param>
        /// <param name="operationType">The single <see cref="Beef.Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The current <see cref="DatabaseCommand"/> instance to support chaining (fluent interface).</returns>
        public DatabaseCommand Params<T>(T value, Action<T, DatabaseParameters, OperationTypes, object> action, OperationTypes operationType)
        {
            action?.Invoke(value, new DatabaseParameters(this), operationType, null!);
            return this;
        }

        #endregion

        #region DbCommandWrapper

        /// <summary>
        /// Wraps the "DbCommand" invocation.
        /// </summary>
        private Task<T> NonQueryScalarDbCommandWrapperAsync<T>(Func<CommandBehavior, Task<T>> func)
        {
            CommandBehavior behavior = CommandBehavior.Default;

            return Database.Invoker.InvokeAsync(this, async () =>
            {
                try
                {
                    behavior = DetermineBehavior(DbCommand);
                    return await func(behavior).ConfigureAwait(false);
                }
                finally
                {
                    // Close the connection where specified in behavior.
                    if (behavior == CommandBehavior.CloseConnection)
                    {
                        Database.Logger.LogInformation("Database connection is being closed (CommandBehavior.CloseConnection).");
                        DbCommand.Connection.Close();
                    }

                    DbCommand.Dispose();
                }
            }, Database);
        }

        /// <summary>
        /// Wraps the "DbDataReader" invocation.
        /// </summary>
        private Task<T> DbDataReaderWrapperWithResultAsync<T>(Func<DbDataReader, Task<T>> func)
        {
            DbDataReader? dr = null;
            CommandBehavior behavior = CommandBehavior.Default;

            return Database.Invoker.InvokeAsync(this, async () => 
            {
                try
                {
                    // Execute the data reader!
                    dr = await DbCommand.ExecuteReaderAsync(DetermineBehavior(DbCommand)).ConfigureAwait(false);

                    // Execute the specific action.
                    return await func(dr).ConfigureAwait(false);
                }
                finally
                {
                    // Close and dispose the data reader.
                    if (dr != null)
                        dr.Dispose();

                    // Close the connection where specified in behavior.
                    if (behavior == CommandBehavior.CloseConnection)
                        DbCommand.Connection.Close();

                    DbCommand.Dispose();
                }
            }, Database);
        }

        /// <summary>
        /// Wraps the "DbDataReader" invocation.
        /// </summary>
        private async Task DbDataReaderWrapperNoResultAsync(Func<DbDataReader, Task> func)
        {
            DbDataReader? dr = null;
            CommandBehavior behavior = CommandBehavior.Default;

            await Database.Invoker.InvokeAsync(this, async () =>
            {
                try
                {
                    // Execute the data reader!
                    dr = await DbCommand.ExecuteReaderAsync(DetermineBehavior(DbCommand)).ConfigureAwait(false);

                    // Execute the specific action.
                    await func(dr).ConfigureAwait(false);
                }
                finally
                {
                    // Close and dispose the data reader.
                    if (dr != null)
                        dr.Dispose();

                    // Close the connection where specified in behavior.
                    if (behavior == CommandBehavior.CloseConnection)
                    {
                        Database.Logger.LogInformation("Database connection is being closed (CommandBehavior.CloseConnection).");
                        DbCommand.Connection.Close();
                    }

                    DbCommand.Dispose();
                }

            }, Database).ConfigureAwait(false);
        }

        /// <summary>
        /// Determine CommandBehavior from the DbCommand state.
        /// </summary>
        private CommandBehavior DetermineBehavior(DbCommand dbCommand)
        {
            // Check if there is a connection and whether it is already open and set behavior accordingly.
            if (dbCommand.Connection == null)
                dbCommand.Connection = Database.GetConnection();

            // Where not open, we'll open and immediately close after the command has executed.
            if (dbCommand.Connection.State != ConnectionState.Open)
            {
                Database.Logger.LogDebug("The Command Connection is not Open, is being opened and will be set to automatic CloseConnection. DatabaseId: {0}", Database.DatabaseId);
                dbCommand.Connection.Open();
                return CommandBehavior.CloseConnection;
            }

            // Leave the connection in its current state.
            return CommandBehavior.Default;
        }

        #endregion

        #region SelectSingle/SelectFirst

        /// <summary>
        /// Selects a single item.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mapFromDb">The <see cref="DatabaseRecord"/> delegate invoked for the record.</param>
        /// <returns>The single item.</returns>
        public async Task<T> SelectSingleAsync<T>(Func<DatabaseRecord, T> mapFromDb)
        {
            T item = await SelectSingleFirstInternalAsync(mapFromDb, true).ConfigureAwait(false);
            if (Comparer<T>.Default.Compare(item, default!) == 0)
                throw new InvalidOperationException("SelectSingle request has not returned a row.");

            return item;
        }

        /// <summary>
        /// Selects a single item using a <paramref name="mapper"/>.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <returns>The single item.</returns>
        public async Task<T> SelectSingleAsync<T>(IDatabaseMapper<T> mapper) where T : class, new()
        {
            Check.NotNull(mapper, nameof(mapper));
            return await SelectSingleAsync((dr) => mapper.MapFromDb(dr, OperationTypes.Get, null) ?? throw new InvalidOperationException(_mapperNullResultMessage)).ConfigureAwait(false);
        }

        /// <summary>
        /// Selects a single item or default.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mapFromDb">The <see cref="DatabaseRecord"/> delegate invoked for the record.</param>
        /// <returns>The single item or default.</returns>
        public async Task<T> SelectSingleOrDefaultAsync<T>(Func<DatabaseRecord, T> mapFromDb)
        {
            return await SelectSingleFirstInternalAsync(mapFromDb, true).ConfigureAwait(false);
        }

        /// <summary>
        /// Selects a single item or default using a <paramref name="mapper"/>.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <returns>The single item.</returns>
        public async Task<T?> SelectSingleOrDefaultAsync<T>(IDatabaseMapper<T> mapper) where T : class, new()
        {
            Check.NotNull(mapper, nameof(mapper));
            return await SelectSingleOrDefaultAsync((dr) => mapper.MapFromDb(dr, OperationTypes.Get, null!) ?? throw new InvalidOperationException(_mapperNullResultMessage)).ConfigureAwait(false);
        }

        /// <summary>
        /// Selects first item.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mapFromDb">The <see cref="DatabaseRecord"/> delegate invoked for the record.</param>
        /// <returns>The first item.</returns>
        public async Task<T> SelectFirstAsync<T>(Func<DatabaseRecord, T> mapFromDb)
        {
            var item = await SelectSingleFirstInternalAsync(mapFromDb, false).ConfigureAwait(false);
            if (item == null)
                throw new InvalidOperationException("SelectFirst request has not returned a row.");

            return item;
        }

        /// <summary>
        /// Selects first item using a <paramref name="mapper"/>.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <returns>The single item.</returns>
        public async Task<T> SelectFirstAsync<T>(IDatabaseMapper<T> mapper) where T : class, new()
        {
            Check.NotNull(mapper, nameof(mapper));
            return await SelectFirstAsync((dr) => mapper.MapFromDb(dr, OperationTypes.Get, null!) ?? throw new InvalidOperationException(_mapperNullResultMessage)).ConfigureAwait(false);
        }

        /// <summary>
        /// Selects first item or default.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mapFromDb">The <see cref="DatabaseRecord"/> delegate invoked for the record.</param>
        /// <returns>The single item or default.</returns>
        public async Task<T> SelectFirstOrDefaultAsync<T>(Func<DatabaseRecord, T> mapFromDb) 
        {
            return await SelectSingleFirstInternalAsync(mapFromDb, false).ConfigureAwait(false);
        }

        /// <summary>
        /// Selects first item or default using a <paramref name="mapper"/>.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <returns>The single item.</returns>
        public async Task<T?> SelectFirstOrDefaultAsync<T>(IDatabaseMapper<T> mapper) where T : class, new()
        {
            Check.NotNull(mapper, nameof(mapper));
            return await SelectFirstOrDefaultAsync((dr) => mapper.MapFromDb(dr, OperationTypes.Get, null!) ?? throw new InvalidOperationException(_mapperNullResultMessage)).ConfigureAwait(false);
        }

        /// <summary>
        /// Performs the database select single command.
        /// </summary>
        private Task<T> SelectSingleFirstInternalAsync<T>(Func<DatabaseRecord, T> mapFromDb, bool throwWhereMulti)
        {
            if (mapFromDb == null)
                throw new ArgumentNullException(nameof(mapFromDb));

            return DbDataReaderWrapperWithResultAsync(async dr => 
            {
                T item = default!;
                int i = 0;

                while (await dr.ReadAsync().ConfigureAwait(false))
                {
                    if (i++ > 0)
                    {
                        if (throwWhereMulti)
                            throw new InvalidOperationException("SelectSingle request has returned more than one row.");

                        return item;
                    }

                    item = mapFromDb(new DatabaseRecord(this, (IDataRecord)dr));
                }

                return item;
            });
        }

        #endregion

        #region SelectQuery

        /// <summary>
        /// Executes a query command.
        /// </summary>
        /// <param name="databaseRecord">The <see cref="DatabaseRecord"/> delegate invoked for each record.</param>
        public async Task SelectQueryAsync(Action<DatabaseRecord> databaseRecord)
        {
            Check.NotNull(databaseRecord, nameof(databaseRecord));

            await DbDataReaderWrapperNoResultAsync(async dr =>
            {
                while (await dr.ReadAsync().ConfigureAwait(false))
                {
                    databaseRecord(new DatabaseRecord(this, dr));
                }
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a query command; whilst also returning the resulting <see cref="ParameterDirection.ReturnValue"/>.
        /// </summary>
        /// <param name="databaseRecord">The <see cref="DatabaseRecord"/> delegate invoked for each record.</param>
        /// <returns>The resultant return value.</returns>
        public async Task<int> SelectQueryWithValueAsync(Action<DatabaseRecord> databaseRecord)
        {
            var rvp = Parameters.AddReturnValueParameter();

            await SelectQueryAsync(databaseRecord).ConfigureAwait(false);

            return rvp.Value == null ? -1 : (int)rvp.Value;
        }

        /// <summary>
        /// Executes a query command.
        /// </summary>
        /// <typeparam name="T">The result <see cref="Type"/>.</typeparam>
        /// <param name="mapFromDb">The <see cref="DatabaseRecord"/> delegate invoked for each record.</param>
        /// <returns>An <see cref="IEnumerable{T}"/>.</returns>
        public async Task<IEnumerable<T>> SelectQueryAsync<T>(Func<DatabaseRecord, T> mapFromDb)
        {
            if (mapFromDb == null)
                throw new ArgumentNullException(nameof(mapFromDb));

            return await DbDataReaderWrapperWithResultAsync(async dr =>
            {
                var coll = new List<T>();

                while (await dr.ReadAsync().ConfigureAwait(false))
                {
                    coll.Add(mapFromDb(new DatabaseRecord(this, dr)) ?? throw new InvalidOperationException(_mapperNullResultMessage));
                }

                return coll;
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a query command using a <paramref name="mapper"/>.
        /// </summary>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <returns>An <see cref="IEnumerable{TItem}"/>.</returns>
        public async Task<IEnumerable<TItem>> SelectQueryAsync<TItem>(IDatabaseMapper<TItem> mapper) where TItem : class, new()
        {
            return await SelectQueryAsync((dr) => mapper.MapFromDb(dr, OperationTypes.Get, null!) ?? throw new InvalidOperationException(_mapperNullResultMessage)).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a query command; whilst also outputing the resulting <see cref="ParameterDirection.ReturnValue"/>.
        /// </summary>
        /// <typeparam name="T">The result <see cref="Type"/>.</typeparam>
        /// <param name="mapFromDb">The <see cref="DatabaseRecord"/> delegate invoked for each record.</param>
        /// <returns>An <see cref="IEnumerable{TItem}"/> collection and resultant return value.</returns>
        public async Task<(IEnumerable<T> items, int returnValue)> SelectQueryWithValueAsync<T>(Func<DatabaseRecord, T> mapFromDb)
        {
            var rvp = Parameters.AddReturnValueParameter();
            var coll = await SelectQueryAsync<T>(mapFromDb).ConfigureAwait(false);
            return (coll, rvp.Value == null ? -1 : (int)rvp.Value);
        }

        /// <summary>
        /// Executes a query command using a <paramref name="mapper"/>; whilst also outputing the resulting <see cref="ParameterDirection.ReturnValue"/>.
        /// </summary>
        /// <typeparam name="TItem">The record <see cref="Type"/>.</typeparam>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <returns>An <see cref="IEnumerable{TItem}"/> collection and resultant return value.</returns>
        public async Task<(IEnumerable<TItem> coll, int returnValue)> SelectQueryWithValueAsync<TItem>(IDatabaseMapper<TItem> mapper) where TItem : class, new()
        {
            return await SelectQueryWithValueAsync((dr) => mapper.MapFromDb(dr, OperationTypes.Get, null!)).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a query command creating a resultant collection. 
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="mapFromDb">The <see cref="DatabaseRecord"/> delegate invoked for each record.</param>
        /// <returns>The resultant collection.</returns>
        public async Task<TColl> SelectQueryAsync<TColl, TItem>(Func<DatabaseRecord, TItem> mapFromDb) where TColl : ICollection<TItem>, new()
        {
            var coll = new TColl();

            await SelectQueryAsync(coll, mapFromDb).ConfigureAwait(false);

            return coll;
        }

        /// <summary>
        /// Executes a query command using a <paramref name="mapper"/> creating a resultant collection. 
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <returns>The resultant collection.</returns>
        public async Task<TColl> SelectQueryAsync<TColl, TItem>(IDatabaseMapper<TItem> mapper) where TItem : class, new() where TColl : ICollection<TItem>, new()
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            return await SelectQueryAsync<TColl, TItem>((dr) => mapper.MapFromDb(dr, OperationTypes.Get, null!)!).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a query command creating a resultant collection; whilst also outputing the resulting <see cref="ParameterDirection.ReturnValue"/>.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="mapFromDb">The <see cref="DatabaseRecord"/> delegate invoked for each record.</param>
        /// <returns>The resultant collection and return value.</returns>
        public async Task<(TColl coll, int returnValue)> SelectQueryWithValueAsync<TColl, TItem>(Func<DatabaseRecord, TItem> mapFromDb) where TColl : ICollection<TItem>, new()
        {
            var coll = new TColl();
            var value = await SelectQueryWithValueAsync(coll, mapFromDb).ConfigureAwait(false);
            return (coll, value);
        }

        /// <summary>
        /// Executes a query command using a <paramref name="mapper"/> creating a resultant collection; whilst also outputing the resulting <see cref="ParameterDirection.ReturnValue"/>.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <returns>The resultant collection and return value.</returns>
        public async Task<(TColl coll, int returnValue)> SelectQueryWithValueAsync<TColl, TItem>(IDatabaseMapper<TItem> mapper) where TItem : class, new() where TColl : ICollection<TItem>, new()
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            return await SelectQueryWithValueAsync<TColl, TItem>((dr) => mapper.MapFromDb(dr, OperationTypes.Get, null!)!).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a query command adding to the passed collection.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="coll">The collection to add items to.</param>
        /// <param name="mapFromDb">The <see cref="DatabaseRecord"/> delegate invoked for each record.</param>
        public Task SelectQueryAsync<TColl, TItem>(TColl coll, Func<DatabaseRecord, TItem> mapFromDb) where TColl : ICollection<TItem>
        {
            if (coll == null)
                throw new ArgumentNullException(nameof(coll));

            if (mapFromDb == null)
                throw new ArgumentNullException(nameof(mapFromDb));

            return DbDataReaderWrapperNoResultAsync(async dr =>
            {
                while (await dr.ReadAsync().ConfigureAwait(false))
                {
                    var item = mapFromDb(new DatabaseRecord(this, (IDataRecord)dr));
                    if (item != null)
                        coll.Add(item);
                }
            });
        }

        /// <summary>
        /// Executes a query command using a <paramref name="mapper"/> adding to the passed collection.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="coll">The collection to add items to.</param>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        public Task SelectQueryAsync<TColl, TItem>(TColl coll, IDatabaseMapper<TItem> mapper) where TItem : class, new() where TColl : ICollection<TItem>
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));
            
            return SelectQueryAsync(coll, (dr) => mapper.MapFromDb(dr, OperationTypes.Get, null!)!);
        }

        /// <summary>
        /// Executes a query command adding to the passed collection; whilst also outputing the resulting <see cref="ParameterDirection.ReturnValue"/>.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="coll">The collection to add items to.</param>
        /// <param name="mapFromDb">The <see cref="DatabaseRecord"/> delegate invoked for each record.</param>
        /// <returns>An <see cref="IEnumerable{TItem}"/> collection and resultant return value.</returns>
        public async Task<int> SelectQueryWithValueAsync<TColl, TItem>(TColl coll, Func<DatabaseRecord, TItem> mapFromDb) where TColl : ICollection<TItem>
        {
            var rvp = Parameters.AddReturnValueParameter();

            await SelectQueryAsync(coll, mapFromDb).ConfigureAwait(false);

            return rvp.Value == null ? -1 : (int)rvp.Value;
        }

        /// <summary>
        /// Executes a query command using a <paramref name="mapper"/> adding to the passed collection; whilst also outputing the resulting <see cref="ParameterDirection.ReturnValue"/>.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="coll">The collection to add items to.</param>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <returns>The resultant return value.</returns>
        public async Task<int> SelectQueryWithValueAsync<TColl, TItem>(TColl coll, IDatabaseMapper<TItem> mapper) where TItem : class, new() where TColl : ICollection<TItem>
        {
            if (mapper == null)
                throw new ArgumentNullException(nameof(mapper));

            return await SelectQueryWithValueAsync(coll, (dr) => mapper.MapFromDb(dr, OperationTypes.Get, null!)!).ConfigureAwait(false);
        }

        #endregion

        #region SelectQueryMultiSet

        /// <summary>
        /// Executes a multi-dataset query command.
        /// </summary>
        /// <param name="datasetRecord">An array of <see cref="DatabaseRecord"/> delegates invoked for each record for their respective dataset.</param>
        /// <remarks>The number of delegates specified must match the number of returned datasets. A null dataset indicates to ignore (skip) a dataset.</remarks>
        public async Task SelectQueryMultiSetAsync(params Action<DatabaseRecord>[] datasetRecord)
        {
            if (datasetRecord == null)
                throw new ArgumentNullException(nameof(datasetRecord));

            await DbDataReaderWrapperNoResultAsync(async dr =>
            {
                var index = 0;
                do
                {
                    if (index >= datasetRecord.Length)
                        throw new InvalidOperationException($"SelectQueryMultiSet has returned more record sets than expected ({datasetRecord.Length}).");

                    if (datasetRecord[index] != null)
                    {
                        while (await dr.ReadAsync().ConfigureAwait(false))
                        {
                            datasetRecord[index](new DatabaseRecord(this, dr));
                        }
                    }

                    index++;
                } while (await dr.NextResultAsync().ConfigureAwait(false));

                if (index < datasetRecord.Length)
                    throw new InvalidOperationException($"SelectQueryMultiSet has returned less ({index}) record sets than expected ({datasetRecord.Length}).");
            }).ConfigureAwait(false);
        }

        /// <summary>
        /// Executes a multi-dataset query command; whilst also outputing the resulting <see cref="ParameterDirection.ReturnValue"/>.
        /// </summary>
        /// <param name="datasetRecord">An array of <see cref="DatabaseRecord"/> delegates invoked for each record for their respective dataset.</param>
        /// <returns>The resultant return value.</returns>
        /// <remarks>The number of delegates specified must match the number of returned datasets. A null dataset indicates to ignore (skip) a dataset.</remarks>
        public async Task<int> SelectQueryMultiSetWithValueAsync(params Action<DatabaseRecord>[] datasetRecord)
        {
            var rvp = Parameters.AddReturnValueParameter();

            await SelectQueryMultiSetAsync(datasetRecord).ConfigureAwait(false);

            return rvp.Value == null ? -1 : (int)rvp.Value;
        }

        /// <summary>
        /// Executes a multi-dataset query command with one or more <see cref="IMultiSetArgs"/>.
        /// </summary>
        /// <param name="multiSetArgs">One or more <see cref="IMultiSetArgs"/>.</param>
        /// <remarks>The number of <see cref="IMultiSetArgs"/> specified must match the number of returned datasets. A null dataset indicates to ignore (skip) a dataset.</remarks>
        public Task SelectQueryMultiSetAsync(params IMultiSetArgs[] multiSetArgs)
        {
            Check.NotNull(multiSetArgs, nameof(multiSetArgs));

            return DbDataReaderWrapperNoResultAsync(async dr =>
            {
                var index = 0;
                var records = 0;
                IMultiSetArgs? multiSetArg = null;
                do
                {
                    if (index >= multiSetArgs.Length)
                        throw new InvalidOperationException($"SelectQueryMultiSet has returned more record sets than expected ({multiSetArgs.Length}).");

                    if (multiSetArgs[index] != null)
                    {
                        records = 0;
                        multiSetArg = multiSetArgs[index];
                        while (await dr.ReadAsync().ConfigureAwait(false))
                        {
                            records++;
                            if (multiSetArg.MaxRows.HasValue && records > multiSetArg.MaxRows.Value)
                                throw new InvalidOperationException($"SelectQueryMultiSet (multiSetArgs[{index}]) has returned more records than expected ({multiSetArg.MaxRows.Value}).");

                            var databaseRecord = new DatabaseRecord(this, dr);
                            if (multiSetArg.StopOnPredicate != null && multiSetArg.StopOnPredicate(databaseRecord))
                                return;

                            multiSetArg.DatasetRecord(databaseRecord);
                        }

                        if (records < multiSetArg.MinRows)
                            throw new InvalidOperationException($"SelectQueryMultiSet (multiSetArgs[{index}]) has returned less records ({records}) than expected ({multiSetArg.MinRows}).");

                        if (records == 0 && multiSetArg.StopOnNull)
                            return;

                        multiSetArg.InvokeResult();
                    }

                    index++;
                } while (dr.NextResult());

                if (index < multiSetArgs.Length && !multiSetArgs[index].StopOnNull)
                    throw new InvalidOperationException($"SelectQueryMultiSet has returned less ({index}) record sets than expected ({multiSetArgs.Length}).");
            });
        }

        /// <summary>
        /// Executes a multi-dataset query command with one or more <see cref="IMultiSetArgs"/>; whilst also outputing the resulting <see cref="ParameterDirection.ReturnValue"/>.
        /// </summary>
        /// <param name="multiSetArgs">One or more <see cref="IMultiSetArgs"/>.</param>
        /// <returns>The resultant return value.</returns>
        /// <remarks>The number of <see cref="IMultiSetArgs"/> specified must match the number of returned datasets. A null dataset indicates to ignore (skip) a dataset.</remarks>
        public async Task<int> SelectQueryMultiSetWithValueAsync(params IMultiSetArgs[] multiSetArgs)
        {
            var rvp = Parameters.AddReturnValueParameter();

            await SelectQueryMultiSetAsync(multiSetArgs).ConfigureAwait(false);

            return rvp.Value == null ? -1 : (int)rvp.Value;
        }

        /// <summary>
        /// Executes a multi-dataset query command with one or more <see cref="IMultiSetArgs"/> that supports <paramref name="paging"/>.
        /// </summary>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="multiSetArgs">One or more <see cref="IMultiSetArgs"/>.</param>
        /// <returns>The resulting <see cref="PagingResult"/>.</returns>
        public async Task SelectQueryMultiSetAsync(PagingResult paging, params IMultiSetArgs[] multiSetArgs)
        {
            Parameters.PagingParams(paging);

            var rv = await SelectQueryMultiSetWithValueAsync(multiSetArgs).ConfigureAwait(false);
            if (paging != null && paging.IsGetCount && rv > 0)
                paging.TotalCount = rv;
        }

        #endregion

        #region NonQuery/Scalar

        /// <summary>
        /// Executes a non-query command.
        /// </summary>
        /// <param name="action">The post-execution delegate to enable parameter access.</param>
        /// <returns>The number of rows affected.</returns>
        public Task<int> NonQueryAsync(Action<DbParameterCollection>? action = null)
        {
            return NonQueryScalarDbCommandWrapperAsync(async behaviour =>
            {
                int result = await DbCommand.ExecuteNonQueryAsync().ConfigureAwait(false);
                action?.Invoke(DbCommand.Parameters);
                return result;
            });
        }

        /// <summary>
        /// Executes the query and returns the first column of the first row in the result set returned by the query.
        /// </summary>
        /// <typeparam name="T">The result <see cref="Type"/>.</typeparam>
        /// <param name="action">The post-execution delegate to enable parameter access.</param>
        /// <returns>The value of the first column of the first row in the result set.</returns>
        public Task<T> ScalarAsync<T>(Action<DbParameterCollection>? action = null)
        {
            return NonQueryScalarDbCommandWrapperAsync(async behaviour =>
            {
                var result = await DbCommand.ExecuteScalarAsync().ConfigureAwait(false);
                var value = result is DBNull ? default : (T)result;
                action?.Invoke(DbCommand.Parameters);
                return value!;
            });
        }

        #endregion
    }
}