// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.RefData;
using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

namespace Beef.Data.Database
{
    /// <summary>
    /// Provides the database access layer capabilities.
    /// </summary>
    public interface IDatabase
    {
        /// <summary>
        /// Creates a <see cref="DbConnection"/>.
        /// </summary>
        /// <param name="useDataContextScope">Indicates whether to use the <see cref="DataContextScope"/>; defaults to <c>true</c>.</param>
        /// <returns>A <see cref="DbConnection"/>.</returns>
        DbConnection CreateConnection(bool useDataContextScope = true);

        /// <summary>
        /// Creates a stored procedure <see cref="DatabaseCommand"/>.
        /// </summary>
        /// <param name="storedProcedure">The stored procedure name.</param>
        /// <returns>A <see cref="DatabaseCommand"/>.</returns>
        DatabaseCommand StoredProcedure(string storedProcedure);

        /// <summary>
        /// Creates a SQL statement <see cref="DatabaseCommand"/>.
        /// </summary>
        /// <param name="sqlStatement">The SQL statement.</param>
        /// <returns>A <see cref="DatabaseCommand"/>.</returns>
        DatabaseCommand SqlStatement(string sqlStatement);

        /// <summary>
        /// Creates a <see cref="DatabaseQuery{T}"/> to enable select-like capabilities.
        /// </summary>
        /// <param name="queryArgs">The <see cref="DatabaseArgs{T}"/>.</param>
        /// <param name="queryParams">The query <see cref="DatabaseParameters"/> delegate.</param>
        /// <returns>A <see cref="DatabaseQuery{T}"/>.</returns>
        DatabaseQuery<T> Query<T>(DatabaseArgs<T> queryArgs, Action<DatabaseParameters>? queryParams = null) where T : class, new();

        /// <summary>
        /// Gets the entity for the specified <paramref name="keys"/> mapping to <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The resultant <see cref="Type"/>.</typeparam>
        /// <param name="getArgs">The <see cref="DatabaseArgs{T}"/>.</param>
        /// <param name="keys">The key values.</param>
        /// <returns>The entity value where found; otherwise, <c>null</c>.</returns>
        Task<T?> GetAsync<T>(DatabaseArgs<T> getArgs, params IComparable[] keys) where T : class, new();

        /// <summary>
        /// Performs a create for the specified stored procedure and value (reselects where specified).
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="DatabaseArgs{T}"/>.</param>
        /// <param name="value">The value to insert.</param>
        /// <returns>The value (reselected where specified).</returns>
        /// <remarks>Automatically invokes <see cref="DatabaseParameters.AddChangeLogParameters(Entities.ChangeLog, bool, bool, ParameterDirection)"/>.</remarks>
        Task<T> CreateAsync<T>(DatabaseArgs<T> saveArgs, T value) where T : class, new();

        /// <summary>
        /// Performs an update for the specified stored procedure and value (reselects where specified).
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="DatabaseArgs{T}"/>.</param>
        /// <param name="value">The value to update.</param>
        /// <returns>The value (reselected where specified).</returns>
        /// <remarks>Automatically invokes <see cref="DatabaseParameters.AddRowVersionParameter{T}(T, ParameterDirection)"/> and <see cref="DatabaseParameters.AddChangeLogParameters{T}(T, bool, bool, ParameterDirection)"/>.</remarks>
        Task<T> UpdateAsync<T>(DatabaseArgs<T> saveArgs, T value) where T : class, new();

        /// <summary>
        /// Performs a delete for the specified <paramref name="keys"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="saveArgs">The <see cref="DatabaseArgs{T}"/>.</param>
        /// <param name="keys">The key values.</param>
        Task DeleteAsync<T>(DatabaseArgs<T> saveArgs, params IComparable[] keys) where T : class, new();

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
        Task GetRefDataAsync<TColl, TItem>(TColl coll, string storedProcedure, string? idColumnName = null,
            Action<DatabaseRecord, TItem, DatabaseRecordFieldCollection>? additionalProperties = null,
            Action<DatabaseRecord>[]? additionalDatasetRecords = null,
            Func<DatabaseRecord, TItem, bool>? confirmItemIsToBeAdded = null)
                where TColl : ReferenceDataCollectionBase<TItem>
                where TItem : ReferenceDataBase, new();
    }
}