// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Beef.Mapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Web;

namespace Beef.Data.Database
{
    /// <summary>
    /// Encapsulates the <see cref="DbCommand"/> adding additional features to support the likes of method chaining (fluent interface).
    /// </summary>
    public class DatabaseCommand
    {
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
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities; by-design, trusts that the consumer has validated the command text where applicable.
            DbCommand.CommandText = commandText;
#pragma warning restore CA2100 
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
        /// <remarks>This specifically implies that the <see cref="System.Data.SqlClient.SqlParameter"/> is being used; if not then an exception will be thrown.</remarks>
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
        /// <remarks>This specifically implies that the <see cref="System.Data.SqlClient.SqlParameter"/> is being used; if not then an exception will be thrown.</remarks>
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
            action?.Invoke(new DatabaseParameters(this));
            return this;
        }

        /// <summary>
        /// Add one or more parameters by invoking a delegate using the passed value.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="action">The delegate to enable parameter addition.</param>
        /// <param name="operationType">The single <see cref="Mapper.OperationTypes"/> being performed to enable selection.</param>
        /// <returns>The current <see cref="DatabaseCommand"/> instance to support chaining (fluent interface).</returns>
        public DatabaseCommand Params<T>(T value, Action<T, DatabaseParameters, OperationTypes, object> action, OperationTypes operationType)
        {
            action?.Invoke(value, new DatabaseParameters(this), operationType, null);
            return this;
        }

        #endregion

        /// <summary>
        /// Wraps the "DbCommand" invocation.
        /// </summary>
        private T DbCommandWrapper<T>(Func<CommandBehavior, T> func)
        {
            CommandBehavior behavior = CommandBehavior.Default;

            return DatabaseInvoker.Default.Invoke(this, () =>
            {
                using (new DatabasePerformanceTimer(this))
                {
                    try
                    {
                        behavior = DetermineBehavior(DbCommand);
                        return func(behavior);
                    }
                    finally
                    {
                        // Close the connection where specified in behavior.
                        if (behavior == CommandBehavior.CloseConnection)
                            DbCommand.Connection.Close();

                        DbCommand.Dispose();
                    }
                }
            }, Database);
        }

        /// <summary>
        /// Wraps the "DbDataReader" invocation.
        /// </summary>
        private T DbDataReaderWrapper<T>(Func<DbDataReader, T> func)
        {
            DbDataReader dr = null;
            CommandBehavior behavior = CommandBehavior.Default;

            return DatabaseInvoker.Default.Invoke(this, () =>
            {
                using (new DatabasePerformanceTimer(this))
                {
                    try
                    {
                        // Execute the data reader!
                        dr = DbCommand.ExecuteReader(DetermineBehavior(DbCommand));

                        // Execute the specific action.
                        return func(dr);
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
                }
            }, Database);
        }

        /// <summary>
        /// Determine CommandBehavior from the DbCommand state.
        /// </summary>
        private CommandBehavior DetermineBehavior(DbCommand dbCommand)
        {
            // Check if there is a connection and whether it is already open and set behavior accordingly.
            if (dbCommand.Connection == null)
                dbCommand.Connection = Database.CreateConnection();

            // Where not open, we'll open and immediately close after the command has executed.
            if (dbCommand.Connection.State != ConnectionState.Open)
            {
                dbCommand.Connection.Open();
                return CommandBehavior.CloseConnection;
            }

            // Leave the connection in its current state.
            return CommandBehavior.Default;
        }

        #region SelectSingle/SelectFirst

        /// <summary>
        /// Selects a single item.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mapFromDb">The <see cref="DatabaseRecord"/> delegate invoked for the record.</param>
        /// <returns>The single item.</returns>
        public T SelectSingle<T>(Func<DatabaseRecord, T> mapFromDb)
        {
            T item = SelectSingleSelectInternal<T>(mapFromDb, true);
            if (item == null)
                throw new InvalidOperationException("SelectSingle request has not returned a row.");

            return item;
        }

        /// <summary>
        /// Selects a single item using a <paramref name="mapper"/>.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <returns>The single item.</returns>
        public T SelectSingle<T>(IDatabaseMapper<T> mapper) where T : class, new()
        {
            Check.NotNull(mapper, nameof(mapper));
            return SelectSingle((dr) => mapper.MapFromDb(dr, OperationTypes.Get, null));
        }

        /// <summary>
        /// Selects a single item or default.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mapFromDb">The <see cref="DatabaseRecord"/> delegate invoked for the record.</param>
        /// <returns>The single item or default.</returns>
        public T SelectSingleOrDefault<T>(Func<DatabaseRecord, T> mapFromDb)
        {
            return SelectSingleSelectInternal<T>(mapFromDb, true);
        }

        /// <summary>
        /// Selects a single item or default using a <paramref name="mapper"/>.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <returns>The single item.</returns>
        public T SelectSingleOrDefault<T>(IDatabaseMapper<T> mapper) where T : class, new()
        {
            Check.NotNull(mapper, nameof(mapper));
            return SelectSingleOrDefault((dr) => mapper.MapFromDb(dr, OperationTypes.Get, null));
        }

        /// <summary>
        /// Selects first item.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mapFromDb">The <see cref="DatabaseRecord"/> delegate invoked for the record.</param>
        /// <returns>The first item.</returns>
        public T SelectFirst<T>(Func<DatabaseRecord, T> mapFromDb)
        {
            T item = SelectSingleSelectInternal<T>(mapFromDb, false);
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
        public T SelectFirst<T>(IDatabaseMapper<T> mapper) where T : class, new()
        {
            Check.NotNull(mapper, nameof(mapper));
            return SelectFirst((dr) => mapper.MapFromDb(dr, OperationTypes.Get, null));
        }

        /// <summary>
        /// Selects first item or default.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mapFromDb">The <see cref="DatabaseRecord"/> delegate invoked for the record.</param>
        /// <returns>The single item or default.</returns>
        public T SelectFirstOrDefault<T>(Func<DatabaseRecord, T> mapFromDb)
        {
            return SelectSingleSelectInternal<T>(mapFromDb, false);
        }

        /// <summary>
        /// Selects first item or default using a <paramref name="mapper"/>.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <returns>The single item.</returns>
        public T SelectFirstOrDefault<T>(IDatabaseMapper<T> mapper) where T : class, new()
        {
            Check.NotNull(mapper, nameof(mapper));
            return SelectFirstOrDefault((dr) => mapper.MapFromDb(dr, OperationTypes.Get, null));
        }

        /// <summary>
        /// Performs the database select single command.
        /// </summary>
        private T SelectSingleSelectInternal<T>(Func<DatabaseRecord, T> mapFromDb, bool throwWhereMulti)
        {
            if (mapFromDb == null)
                throw new ArgumentNullException(nameof(mapFromDb));

            return DbDataReaderWrapper(dr =>
            {
                T item = default;
                int i = 0;

                while (dr.Read())
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
        public void SelectQuery(Action<DatabaseRecord> databaseRecord)
        {
            Check.NotNull(databaseRecord, nameof(databaseRecord));

            DbDataReaderWrapper(dr =>
            {
                while (dr.Read())
                {
                    databaseRecord(new DatabaseRecord(this, (IDataRecord)dr));
                }

                return (object)null;
            });
        }

        /// <summary>
        /// Executes a query command; whilst also outputing the resulting <see cref="ParameterDirection.ReturnValue"/>.
        /// </summary>
        /// <param name="databaseRecord">The <see cref="DatabaseRecord"/> delegate invoked for each record.</param>
        /// <param name="returnValue">The resultant return value.</param>
        public void SelectQuery(Action<DatabaseRecord> databaseRecord, out int returnValue)
        {
            var rvp = Parameters.AddReturnValueParameter();

            SelectQuery(databaseRecord);

            returnValue = rvp.Value == null ? -1 : (int)rvp.Value;
        }

        /// <summary>
        /// Executes a query command.
        /// </summary>
        /// <typeparam name="TItem">The record <see cref="Type"/>.</typeparam>
        /// <param name="mapFromDb">The <see cref="DatabaseRecord"/> delegate invoked for each record.</param>
        /// <returns>An <see cref="IEnumerable{TItem}"/>.</returns>
        public IEnumerable<TItem> SelectQuery<TItem>(Func<DatabaseRecord, TItem> mapFromDb)
        {
            if (mapFromDb == null)
                throw new ArgumentNullException(nameof(mapFromDb));

            return DbDataReaderWrapper(dr =>
            {
                var coll = new List<TItem>();

                while (dr.Read())
                {
                    coll.Add(mapFromDb(new DatabaseRecord(this, (IDataRecord)dr)));
                }

                return coll;
            });
        }

        /// <summary>
        /// Executes a query command using a <paramref name="mapper"/>.
        /// </summary>
        /// <typeparam name="TItem">The record <see cref="Type"/>.</typeparam>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <returns>An <see cref="IEnumerable{TItem}"/>.</returns>
        public IEnumerable<TItem> SelectQuery<TItem>(IDatabaseMapper<TItem> mapper) where TItem : class, new()
        {
            return SelectQuery((dr) => mapper.MapFromDb(dr, OperationTypes.Get, null));
        }

        /// <summary>
        /// Executes a query command; whilst also outputing the resulting <see cref="ParameterDirection.ReturnValue"/>.
        /// </summary>
        /// <param name="mapFromDb">The <see cref="DatabaseRecord"/> delegate invoked for each record.</param>
        /// <param name="returnValue">The resultant return value.</param>
        /// <returns>An <see cref="IEnumerable{TItem}"/>.</returns>
        public IEnumerable<TItem> SelectQuery<TItem>(Func<DatabaseRecord, TItem> mapFromDb, out int returnValue)
        {
            var rvp = Parameters.AddReturnValueParameter();

            var coll = SelectQuery<TItem>(mapFromDb);

            returnValue = rvp.Value == null ? -1 : (int)rvp.Value;
            return coll;
        }

        /// <summary>
        /// Executes a query command using a <paramref name="mapper"/>; whilst also outputing the resulting <see cref="ParameterDirection.ReturnValue"/>.
        /// </summary>
        /// <typeparam name="TItem">The record <see cref="Type"/>.</typeparam>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <param name="returnValue">The resultant return value.</param>
        /// <returns>An <see cref="IEnumerable{TItem}"/>.</returns>
        public IEnumerable<TItem> SelectQuery<TItem>(IDatabaseMapper<TItem> mapper, out int returnValue) where TItem : class, new()
        {
            return SelectQuery((dr) => mapper.MapFromDb(dr, OperationTypes.Get, null), out returnValue);
        }

        /// <summary>
        /// Executes a query command creating a resultant collection. 
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="mapFromDb">The <see cref="DatabaseRecord"/> delegate invoked for each record.</param>
        /// <returns>The resultant collection.</returns>
        public TColl SelectQuery<TColl, TItem>(Func<DatabaseRecord, TItem> mapFromDb) where TColl : ICollection<TItem>, new()
        {
            var coll = new TColl();

            SelectQuery<TColl, TItem>(coll, mapFromDb);

            return coll;
        }

        /// <summary>
        /// Executes a query command using a <paramref name="mapper"/> creating a resultant collection. 
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <returns>The resultant collection.</returns>
        public TColl SelectQuery<TColl, TItem>(IDatabaseMapper<TItem> mapper) where TItem : class, new() where TColl : ICollection<TItem>, new()
        {
            return SelectQuery<TColl, TItem>((dr) => mapper.MapFromDb(dr, OperationTypes.Get, null));
        }

        /// <summary>
        /// Executes a query command creating a resultant collection; whilst also outputing the resulting <see cref="ParameterDirection.ReturnValue"/>.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="returnValue">The resultant return value.</param>
        /// <param name="mapFromDb">The <see cref="DatabaseRecord"/> delegate invoked for each record.</param>
        /// <returns>The resultant collection.</returns>
        public TColl SelectQuery<TColl, TItem>(Func<DatabaseRecord, TItem> mapFromDb, out int returnValue) where TColl : ICollection<TItem>, new()
        {
            var coll = new TColl();

            SelectQuery<TColl, TItem>(coll, mapFromDb, out returnValue);

            return coll;
        }

        /// <summary>
        /// Executes a query command using a <paramref name="mapper"/> creating a resultant collection; whilst also outputing the resulting <see cref="ParameterDirection.ReturnValue"/>.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="returnValue">The resultant return value.</param>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <returns>The resultant collection.</returns>
        public TColl SelectQuery<TColl, TItem>(IDatabaseMapper<TItem> mapper, out int returnValue) where TItem : class, new() where TColl : ICollection<TItem>, new()
        {
            return SelectQuery<TColl, TItem>((dr) => mapper.MapFromDb(dr, OperationTypes.Get, null), out returnValue);
        }

        /// <summary>
        /// Executes a query command adding to the passed collection.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="coll">The collection to add items to.</param>
        /// <param name="mapFromDb">The <see cref="DatabaseRecord"/> delegate invoked for each record.</param>
        public void SelectQuery<TColl, TItem>(TColl coll, Func<DatabaseRecord, TItem> mapFromDb) where TColl : ICollection<TItem>
        {
            if (coll == null)
                throw new ArgumentNullException(nameof(coll));

            if (mapFromDb == null)
                throw new ArgumentNullException(nameof(mapFromDb));

            DbDataReaderWrapper(dr =>
            {
                while (dr.Read())
                {
                    coll.Add(mapFromDb(new DatabaseRecord(this, (IDataRecord)dr)));
                }

                return (object)null;
            });
        }

        /// <summary>
        /// Executes a query command using a <paramref name="mapper"/> adding to the passed collection.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="coll">The collection to add items to.</param>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        public void SelectQuery<TColl, TItem>(TColl coll, IDatabaseMapper<TItem> mapper) where TItem : class, new() where TColl : ICollection<TItem>
        {
            SelectQuery<TColl, TItem>(coll, (dr) => mapper.MapFromDb(dr, OperationTypes.Get, null));
        }

        /// <summary>
        /// Executes a query command adding to the passed collection; whilst also outputing the resulting <see cref="ParameterDirection.ReturnValue"/>.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="coll">The collection to add items to.</param>
        /// <param name="mapFromDb">The <see cref="DatabaseRecord"/> delegate invoked for each record.</param>
        /// <param name="returnValue">The resultant return value.</param>
        public void SelectQuery<TColl, TItem>(TColl coll, Func<DatabaseRecord, TItem> mapFromDb, out int returnValue) where TColl : ICollection<TItem>
        {
            var rvp = Parameters.AddReturnValueParameter();

            SelectQuery<TColl, TItem>(coll, mapFromDb);

            returnValue = rvp.Value == null ? -1 : (int)rvp.Value;
        }

        /// <summary>
        /// Executes a query command using a <paramref name="mapper"/> adding to the passed collection; whilst also outputing the resulting <see cref="ParameterDirection.ReturnValue"/>.
        /// </summary>
        /// <typeparam name="TColl">The collection <see cref="Type"/>.</typeparam>
        /// <typeparam name="TItem">The item <see cref="Type"/>.</typeparam>
        /// <param name="coll">The collection to add items to.</param>
        /// <param name="mapper">The <see cref="IDatabaseMapper{T}"/>.</param>
        /// <param name="returnValue">The resultant return value.</param>
        public void SelectQuery<TColl, TItem>(TColl coll, IDatabaseMapper<TItem> mapper, out int returnValue) where TItem : class, new() where TColl : ICollection<TItem>
        {
            SelectQuery<TColl, TItem>(coll, (dr) => mapper.MapFromDb(dr, OperationTypes.Get, null), out returnValue);
        }

        #endregion

        #region SelectQueryMultiSet

        /// <summary>
        /// Executes a multi-dataset query command.
        /// </summary>
        /// <param name="datasetRecord">An array of <see cref="DatabaseRecord"/> delegates invoked for each record for their respective dataset.</param>
        /// <remarks>The number of delegates specified must match the number of returned datasets. A null dataset indicates to ignore (skip) a dataset.</remarks>
        public void SelectQueryMultiSet(params Action<DatabaseRecord>[] datasetRecord)
        {
            if (datasetRecord == null)
                throw new ArgumentNullException(nameof(datasetRecord));

            DbDataReaderWrapper(dr =>
            {
                var index = 0;
                do
                {
                    if (index >= datasetRecord.Length)
                        throw new InvalidOperationException($"SelectQueryMultiSet has returned more record sets than expected ({datasetRecord.Length}).");

                    if (datasetRecord[index] != null)
                    {
                        while (dr.Read())
                        {
                            datasetRecord[index](new DatabaseRecord(this, (IDataRecord)dr));
                        }
                    }

                    index++;
                } while (dr.NextResult());

                if (index < datasetRecord.Length)
                    throw new InvalidOperationException($"SelectQueryMultiSet has returned less ({index}) record sets than expected ({datasetRecord.Length}).");

                return (object)null;
            });
        }

        /// <summary>
        /// Executes a multi-dataset query command; whilst also outputing the resulting <see cref="ParameterDirection.ReturnValue"/>.
        /// </summary>
        /// <param name="datasetRecord">An array of <see cref="DatabaseRecord"/> delegates invoked for each record for their respective dataset.</param>
        /// <param name="returnValue">The resultant return value.</param>
        /// <remarks>The number of delegates specified must match the number of returned datasets. A null dataset indicates to ignore (skip) a dataset.</remarks>
        public void SelectQueryMultiSet(out int returnValue, params Action<DatabaseRecord>[] datasetRecord)
        {
            var rvp = Parameters.AddReturnValueParameter();

            SelectQueryMultiSet(datasetRecord);

            returnValue = rvp.Value == null ? -1 : (int)rvp.Value;
        }

        /// <summary>
        /// Executes a multi-dataset query command with one or more <see cref="IMultiSetArgs"/>.
        /// </summary>
        /// <param name="multiSetArgs">One or more <see cref="IMultiSetArgs"/>.</param>
        /// <remarks>The number of <see cref="IMultiSetArgs"/> specified must match the number of returned datasets. A null dataset indicates to ignore (skip) a dataset.</remarks>
        public void SelectQueryMultiSet(params IMultiSetArgs[] multiSetArgs)
        {
            Check.NotNull(multiSetArgs, nameof(multiSetArgs));

            DbDataReaderWrapper(dr =>
            {
                var index = 0;
                var records = 0;
                IMultiSetArgs multiSetArg = null;
                do
                {
                    if (index >= multiSetArgs.Length)
                        throw new InvalidOperationException($"SelectQueryMultiSet has returned more record sets than expected ({multiSetArgs.Length}).");

                    if (multiSetArgs[index] != null)
                    {
                        records = 0;
                        multiSetArg = multiSetArgs[index];
                        while (dr.Read())
                        {
                            records++;
                            if (multiSetArg.MaxRows.HasValue && records > multiSetArg.MaxRows.Value)
                                throw new InvalidOperationException($"SelectQueryMultiSet (multiSetArgs[{index}]) has returned more records than expected ({multiSetArg.MaxRows.Value}).");

                            multiSetArg.DatasetRecord(new DatabaseRecord(this, (IDataRecord)dr));
                        }

                        if (records < multiSetArg.MinRows)
                            throw new InvalidOperationException($"SelectQueryMultiSet (multiSetArgs[{index}]) has returned less records ({records}) than expected ({multiSetArg.MinRows}).");

                        if (records == 0 && multiSetArg.StopOnNull)
                            return (object)null;

                        multiSetArg.InvokeResult();
                    }

                    index++;
                } while (dr.NextResult());

                if (index < multiSetArgs.Length)
                    throw new InvalidOperationException($"SelectQueryMultiSet has returned less ({index}) record sets than expected ({multiSetArgs.Length}).");

                return (object)null;
            });
        }

        /// <summary>
        /// Executes a multi-dataset query command with one or more <see cref="IMultiSetArgs"/>; whilst also outputing the resulting <see cref="ParameterDirection.ReturnValue"/>.
        /// </summary>
        /// <param name="multiSetArgs">One or more <see cref="IMultiSetArgs"/>.</param>
        /// <param name="returnValue">The resultant return value.</param>
        /// <remarks>The number of <see cref="IMultiSetArgs"/> specified must match the number of returned datasets. A null dataset indicates to ignore (skip) a dataset.</remarks>
        public void SelectQueryMultiSet(out int returnValue, params IMultiSetArgs[] multiSetArgs)
        {
            var rvp = Parameters.AddReturnValueParameter();

            SelectQueryMultiSet(multiSetArgs);

            returnValue = rvp.Value == null ? -1 : (int)rvp.Value;
        }

        /// <summary>
        /// Executes a multi-dataset query command with one or more <see cref="IMultiSetArgs"/> that supports <paramref name="paging"/>.
        /// </summary>
        /// <param name="paging">The <see cref="PagingResult"/>.</param>
        /// <param name="multiSetArgs">One or more <see cref="IMultiSetArgs"/>.</param>
        /// <returns>The resulting <see cref="PagingResult"/>.</returns>
        public void SelectQueryMultiSet(PagingResult paging, params IMultiSetArgs[] multiSetArgs)
        {
            Parameters.PagingParams(paging);

            SelectQueryMultiSet(out int rv, multiSetArgs);
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
        public int NonQuery(Action<DbParameterCollection> action = null)
        {
            return DbCommandWrapper(behaviour =>
            {
                int result = DbCommand.ExecuteNonQuery();
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
        public T Scalar<T>(Action<DbParameterCollection> action = null)
        {
            return DbCommandWrapper(behaviour =>
            {
                var result = DbCommand.ExecuteScalar();
                var value = result is DBNull ? default : (T)result;
                action?.Invoke(DbCommand.Parameters);
                return value;
            });
        }

        #endregion
    }
}
