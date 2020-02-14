// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Web;

namespace Beef.Data.Database
{
    /// <summary>
    /// Represents the database parameters.
    /// </summary>
    public class DatabaseParameters
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseParameters"/> class.
        /// </summary>
        /// <param name="databaseCommand"></param>
        public DatabaseParameters(DatabaseCommand databaseCommand)
        {
            DatabaseCommand = databaseCommand ?? throw new ArgumentNullException(nameof(databaseCommand));
        }

        /// <summary>
        /// Gets the <see cref="DatabaseCommand"/>.
        /// </summary>
        public DatabaseCommand DatabaseCommand { get; private set; }

        /// <summary>
        /// Gets the named database parameter.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <returns>The <see cref="DbParameter"/>.</returns>
        public DbParameter this[string name]
        {
            get { return DatabaseCommand.DbCommand.Parameters[name]; }
        }

        /// <summary>
        /// Indicates whether a parameter with the name already exists.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <returns><c>true</c> indicates that the name already exists; otherwise, <c>false</c>.</returns>
        public bool Contains(string name)
        {
            return DatabaseCommand.DbCommand.Parameters.Contains(name);
        }

        /// <summary>
        /// Gets the underlying <see cref="DbParameterCollection"/>.
        /// </summary>
        public DbParameterCollection Parameters => DatabaseCommand.DbCommand.Parameters;

        #region AddParameter + Param

        /// <summary>
        /// Adds a named parameter and value.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>A <see cref="DbParameter"/>.</returns>
        public DbParameter AddParameter(string name, object? value, ParameterDirection direction = ParameterDirection.Input)
        {
            var p = DatabaseCommand.Database.Provider.CreateParameter();
            p.ParameterName = Check.NotEmpty(name, nameof(name));
            p.Direction = direction;
            p.Value = value;

            DatabaseCommand.DbCommand.Parameters.Add(p);
            return p;
        }

        /// <summary>
        /// Adds a named parameter, specified <see cref="DbType"/> and value.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="dbType">The parameter <see cref="DbType"/>.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>A <see cref="DbParameter"/>.</returns>
        public DbParameter AddParameter(string name, object? value, DbType dbType, ParameterDirection direction = ParameterDirection.Input)
        {
            var p = DatabaseCommand.Database.Provider.CreateParameter();
            p.ParameterName = Check.NotEmpty(name, nameof(name));
            p.DbType = dbType;
            p.Direction = direction;
            p.Value = value;

            DatabaseCommand.DbCommand.Parameters.Add(p);
            return p;
        }

        /// <summary>
        /// Adds a named parameter, specified <see cref="SqlDbType"/> and value.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="sqlDbType">The parameter <see cref="SqlDbType"/>.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>A <see cref="DbParameter"/>.</returns>
        /// <remarks>This specifically implies that the <see cref="Microsoft.Data.SqlClient.SqlParameter"/> is being used; if not then an exception will be thrown.</remarks>
        public DbParameter AddParameter(string name, object? value, SqlDbType sqlDbType, ParameterDirection direction = ParameterDirection.Input)
        {
            var p = (Microsoft.Data.SqlClient.SqlParameter)DatabaseCommand.Database.Provider.CreateParameter();
            p.ParameterName = Check.NotEmpty(name, nameof(name));
            p.SqlDbType = sqlDbType;
            p.Direction = direction;
            p.Value = value;

            DatabaseCommand.DbCommand.Parameters.Add(p);
            return p;
        }

        /// <summary>
        /// Adds a named parameter with specified <see cref="DbType"/>.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="dbType">The <see cref="DbType"/>.</param>
        /// <param name="size">The maximum size (in bytes).</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>A <see cref="DbParameter"/>.</returns>
        public DbParameter AddParameter(string name, DbType dbType, int size = 0, ParameterDirection direction = ParameterDirection.Input)
        {
            var p = DatabaseCommand.Database.Provider.CreateParameter();
            p.ParameterName = Check.NotEmpty(name, nameof(name));
            p.DbType = dbType;
            p.Size = size;
            p.Direction = direction;
            DatabaseCommand.DbCommand.Parameters.Add(p);
            return p;
        }

        /// <summary>
        /// Adds a named parameter with specified <see cref="SqlDbType"/>.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="sqlDbType">The <see cref="DbType"/>.</param>
        /// <param name="size">The maximum size (in bytes).</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>A <see cref="DbParameter"/>.</returns>
        /// <remarks>This specifically implies that the <see cref="Microsoft.Data.SqlClient.SqlParameter"/> is being used; if not then an exception will be thrown.</remarks>
        public SqlParameter AddParameter(string name, SqlDbType sqlDbType, int size = 0, ParameterDirection direction = ParameterDirection.Input)
        {
            var p = (SqlParameter)DatabaseCommand.Database.Provider.CreateParameter();
            p.ParameterName = Check.NotEmpty(name, nameof(name));
            p.SqlDbType = sqlDbType;
            p.Size = size;
            p.Direction = direction;
            DatabaseCommand.DbCommand.Parameters.Add(p);
            return p;
        }

        /// <summary>
        /// Adds a named parameter and value.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        public DatabaseParameters Param(string name, object value, ParameterDirection direction = ParameterDirection.Input)
        {
            AddParameter(name, value, direction);
            return this;
        }

        /// <summary>
        /// Adds a named parameter and value <paramref name="when"/> <c>true</c>.
        /// </summary>
        /// <typeparam name="T">The parameter <see cref="Type"/>.</typeparam>
        /// <param name="when">Adds the parameter when <c>true</c>.</param>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        public DatabaseParameters ParamWhen<T>(bool when, string name, Func<T> value, ParameterDirection direction = ParameterDirection.Input)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (when)
                AddParameter(name, value(), direction);

            return this;
        }

        /// <summary>
        /// Adds a named parameter when invoked <paramref name="with"/> a non-default value.
        /// </summary>
        /// <typeparam name="T">The parameter <see cref="Type"/>.</typeparam>
        /// <param name="with">The value <b>with</b> which to verify is non-default.</param>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        public DatabaseParameters ParamWith<T>(object with, string name, Func<T> value, ParameterDirection direction = ParameterDirection.Input)
        {
            return ParamWhen(with != null && Comparer<T>.Default.Compare((T)with, default!) != 0, name, value, direction);
        }

        /// <summary>
        /// Adds a named parameter, <see cref="DbType"/> and value.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="dbType">The parameter <see cref="DbType"/>.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        public DatabaseParameters Param(string name, object value, DbType dbType, ParameterDirection direction = ParameterDirection.Input)
        {
            AddParameter(name, value, dbType, direction);
            return this;
        }

        /// <summary>
        /// Adds a named parameter, <see cref="DbType"/> and value <paramref name="when"/> <c>true</c>.
        /// </summary>
        /// <typeparam name="T">The parameter <see cref="Type"/>.</typeparam>
        /// <param name="when">Adds the parameter when <c>true</c>.</param>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="dbType">The parameter <see cref="DbType"/>.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        public DatabaseParameters ParamWhen<T>(bool when, string name, Func<T> value, DbType dbType, ParameterDirection direction = ParameterDirection.Input)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (when)
                AddParameter(name, value(), dbType, direction);

            return this;
        }

        /// <summary>
        /// Adds a named parameter, <see cref="DbType"/> and value when invoked <paramref name="with"/> a non-default value.
        /// </summary>
        /// <typeparam name="T">The parameter <see cref="Type"/>.</typeparam>
        /// <param name="with">The value <b>with</b> which to verify is non-default.</param>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="dbType">The parameter <see cref="DbType"/>.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        public DatabaseParameters ParamWith<T>(object with, string name, Func<T> value, DbType dbType, ParameterDirection direction = ParameterDirection.Input)
        {
            return ParamWhen(with != null && Comparer<T>.Default.Compare((T)with, default!) != 0, name, value, dbType, direction);
        }

        /// <summary>
        /// Adds a named parameter, <see cref="SqlDbType"/> and value.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="sqlDbType">The parameter <see cref="SqlDbType"/>.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>This specifically implies that the <see cref="Microsoft.Data.SqlClient.SqlParameter"/> is being used; if not then an exception will be thrown.</remarks>
        public DatabaseParameters Param(string name, object value, SqlDbType sqlDbType, ParameterDirection direction = ParameterDirection.Input)
        {
            AddParameter(name, value, sqlDbType, direction);
            return this;
        }

        /// <summary>
        /// Adds a named parameter, <see cref="SqlDbType"/> and value <paramref name="when"/> <c>true</c>.
        /// </summary>
        /// <typeparam name="T">The parameter <see cref="Type"/>.</typeparam>
        /// <param name="when">Adds the parameter when <c>true</c>.</param>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="sqlDbType">The parameter <see cref="SqlDbType"/>.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>This specifically implies that the <see cref="Microsoft.Data.SqlClient.SqlParameter"/> is being used; if not then an exception will be thrown.</remarks>
        public DatabaseParameters ParamWhen<T>(bool when, string name, Func<T> value, SqlDbType sqlDbType, ParameterDirection direction = ParameterDirection.Input)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (when)
                AddParameter(name, value(), sqlDbType, direction);

            return this;
        }

        /// <summary>
        /// Adds a named parameter, <see cref="SqlDbType"/> and value when invoked <paramref name="with"/> a non-default value.
        /// </summary>
        /// <typeparam name="T">The parameter <see cref="Type"/>.</typeparam>
        /// <param name="with">The value <b>with</b> which to verify is non-default.</param>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="sqlDbType">The parameter <see cref="SqlDbType"/>.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>This specifically implies that the <see cref="Microsoft.Data.SqlClient.SqlParameter"/> is being used; if not then an exception will be thrown.</remarks>
        public DatabaseParameters ParamWith<T>(object with, string name, Func<T> value, SqlDbType sqlDbType, ParameterDirection direction = ParameterDirection.Input)
        {
            return ParamWhen(with != null && Comparer<T>.Default.Compare((T)with, default!) != 0, name, value, sqlDbType, direction);
        }

        /// <summary>
        /// Adds a named parameter with specified <see cref="DbType"/> and <paramref name="size"/>.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="dbType">The <see cref="DbType"/>.</param>
        /// <param name="size">The maximum size (in bytes).</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        public DatabaseParameters Param(string name, DbType dbType, int size = 0, ParameterDirection direction = ParameterDirection.Input)
        {
            AddParameter(name, dbType, size, direction);
            return this;
        }

        /// <summary>
        /// Adds a named parameter with specified <see cref="SqlDbType"/> and <paramref name="size"/>.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="sqlDbType">The <see cref="SqlDbType"/>.</param>
        /// <param name="size">The maximum size (in bytes).</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>This specifically implies that the <see cref="SqlParameter"/> is being used; if not then an exception will be thrown.</remarks>
        public DatabaseParameters Param(string name, SqlDbType sqlDbType, int size = 0, ParameterDirection direction = ParameterDirection.Input)
        {
            AddParameter(name, sqlDbType, size, direction);
            return this;
        }

        /// <summary>
        /// Adds a <see cref="IDatabasePropertyMapper"/> parameter.
        /// </summary>
        /// <param name="propertyMapper">The <see cref="IDatabasePropertyMapper"/>.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        public DatabaseParameters Param(IDatabasePropertyMapper propertyMapper, object? value, ParameterDirection direction = ParameterDirection.Input)
        {
            if (propertyMapper == null)
                throw new ArgumentNullException(nameof(propertyMapper));

            var val = propertyMapper.Converter == null ? value : propertyMapper.Converter.ConvertToDest(value);

            if (propertyMapper.DestDbType.HasValue)
                AddParameter(propertyMapper.DestParameterName, val, propertyMapper.DestDbType.Value, direction);
            else
                AddParameter(propertyMapper.DestParameterName, val, direction);

            return this;
        }

        /// <summary>
        /// Adds <see cref="IDatabasePropertyMapper"/> parameter <paramref name="when"/> <c>true</c>.
        /// </summary>
        /// <typeparam name="T">The parameter <see cref="Type"/>.</typeparam>
        /// <param name="when">Adds the parameter when <c>true</c>.</param>
        /// <param name="propertyMapper">The <see cref="IDatabasePropertyMapper"/>.</param>
        /// <param name="value">The parameter value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        public DatabaseParameters ParamWhen<T>(bool when, IDatabasePropertyMapper propertyMapper, Func<T> value, ParameterDirection direction = ParameterDirection.Input)
        {
            if (propertyMapper == null)
                throw new ArgumentNullException(nameof(propertyMapper));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            if (when)
                Param(propertyMapper, value(), direction);

            return this;
        }

        /// <summary>
        /// Adds <see cref="IDatabasePropertyMapper"/> parameter when invoked <paramref name="with"/> a non-default value.
        /// </summary>
        /// <typeparam name="T">The parameter <see cref="Type"/>.</typeparam>
        /// <param name="with">The value <b>with</b> which to verify is non-default.</param>
        /// <param name="propertyMapper">The <see cref="IDatabasePropertyMapper"/>.</param>
        /// <param name="value">The parameter value; where not specified the <paramref name="with"/> vaue will be used.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        public DatabaseParameters ParamWith<T>(T with, IDatabasePropertyMapper propertyMapper, Func<T>? value = null, ParameterDirection direction = ParameterDirection.Input)
        {
            return ParamWhen(Comparer<T>.Default.Compare(with, default!) != 0 && Comparer<T>.Default.Compare((T)with, default!) != 0, propertyMapper, value ?? (() => with), direction);
        }

        /// <summary>
        /// Adds <see cref="IDatabasePropertyMapper"/> parameter when invoked <paramref name="with"/> a non-default value and converts the wildcard for the database. 
        /// </summary>
        /// <param name="with">The value <b>with</b> which to verify is non-default.</param>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The parameter value; where not specified the <paramref name="with"/> vaue will be used.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        public DatabaseParameters ParamWithWildcard(string with, string name, Func<string>? value = null, ParameterDirection direction = ParameterDirection.Input)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            return ParamWith(with, name, value ?? (() => DatabaseCommand.Database.Wildcard.Replace(with)), direction);
        }

        /// <summary>
        /// Adds <see cref="IDatabasePropertyMapper"/> parameter when invoked <paramref name="with"/> a non-default value and converts the wildcard for the database. 
        /// </summary>
        /// <param name="with">The value <b>with</b> which to verify is non-default.</param>
        /// <param name="propertyMapper">The <see cref="IDatabasePropertyMapper"/>.</param>
        /// <param name="value">The parameter value; where not specified the <paramref name="with"/> vaue will be used.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (default to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        public DatabaseParameters ParamWithWildcard(string with, IDatabasePropertyMapper propertyMapper, Func<string>? value = null, ParameterDirection direction = ParameterDirection.Input)
        {
            if (propertyMapper == null)
                throw new ArgumentNullException(nameof(propertyMapper));

            return ParamWith(with, propertyMapper, value ?? (() => DatabaseCommand.Database.Wildcard.Replace(with)), direction);
        }

        #endregion

        #region AddRowVersionParameter + RowVersionParam

        /// <summary>
        /// Adds a named <b>RowVersion</b> parameter.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The <b>RowVersion</b> parameter value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (defaults to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>A <see cref="DbParameter"/>.</returns>
        /// <remarks>The <b>RowVersion</b> <see cref="byte"/> array will be converted from an <see cref="Convert.FromBase64String(string)">encoded</see> <see cref="string"/> value.</remarks>
        public DbParameter AddRowVersionParameter(string name, string value, ParameterDirection direction = ParameterDirection.Input)
        {
            if (value == null)
                return AddParameter(name, DbType.Byte, 8, direction);
            else
                return AddParameter(name, Convert.FromBase64String(value), direction);
        }

        /// <summary>
        /// Adds a <b>RowVersion</b> parameter (named <see cref="DatabaseColumns.RowVersionName"/>).
        /// </summary>
        /// <param name="value">The <b>RowVersion</b>parameter value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (defaults to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>A <see cref="DbParameter"/>.</returns>
        /// <remarks>The <b>RowVersion</b> <see cref="byte"/> array will be converted from an <see cref="HttpUtility.UrlDecode(string)">encoded</see> <see cref="string"/> value.</remarks>
        public DbParameter AddRowVersionParameter(string value, ParameterDirection direction = ParameterDirection.Input)
        {
            return AddRowVersionParameter("@" + DatabaseColumns.RowVersionName, value, direction);
        }

        /// <summary>
        /// Adds a named <b>RowVersion</b> parameter where the <paramref name="value"/> implements <see cref="IETag"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (defaults to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>A <see cref="DbParameter"/> where added; otherwise, <c>null</c>.</returns>
        /// <remarks>The <b>RowVersion</b> <see cref="byte"/> array will be converted from an <see cref="HttpUtility.UrlDecode(string)">encoded</see> <see cref="string"/> value.</remarks>
        public DbParameter? AddRowVersionParameter<T>(string name, T value, ParameterDirection direction = ParameterDirection.Input) where T : class
        {
            if (value is IETag etag)
                return AddRowVersionParameter(name, etag.ETag, direction);

            return null;
        }

        /// <summary>
        /// Adds a <b>RowVersion</b> parameter (named <see cref="DatabaseColumns.RowVersionName"/>) where the <paramref name="value"/> implements <see cref="IETag"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (defaults to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>A <see cref="DbParameter"/> where added; otherwise, <c>null</c>.</returns>
        /// <remarks>The <b>RowVersion</b> <see cref="byte"/> array will be converted from an <see cref="HttpUtility.UrlDecode(string)">encoded</see> <see cref="string"/> value.</remarks>
        public DbParameter? AddRowVersionParameter<T>(T value, ParameterDirection direction = ParameterDirection.Input) where T : class
        {
            return AddRowVersionParameter("@" + DatabaseColumns.RowVersionName, value, direction);
        }

        /// <summary>
        /// Adds a named <b>RowVersion</b> parameter.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The <b>RowVersion</b> parameter value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (defaults to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>The <b>RowVersion</b> <see cref="byte"/> array will be converted from an <see cref="HttpUtility.UrlDecode(string)">encoded</see> <see cref="string"/> value.</remarks>
        public DatabaseParameters RowVersionParam(string name, string value, ParameterDirection direction = ParameterDirection.Input)
        {
            AddRowVersionParameter(name, value, direction);
            return this;
        }

        /// <summary>
        /// Adds a <b>RowVersion</b> (named <see cref="DatabaseColumns.RowVersionName"/>) parameter.
        /// </summary>
        /// <param name="value">The <b>RowVersion</b> parameter value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (defaults to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>The <b>RowVersion</b> <see cref="byte"/> array will be converted from an <see cref="HttpUtility.UrlDecode(string)">encoded</see> <see cref="string"/> value.</remarks>
        public DatabaseParameters RowVersionParam(string value, ParameterDirection direction = ParameterDirection.Input)
        {
            AddRowVersionParameter(value, direction);
            return this;
        }

        /// <summary>
        /// Adds a named <b>RowVersion</b> parameter where the <paramref name="value"/> implements <see cref="IETag"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="name">The parameter name.</param>
        /// <param name="value">The value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (defaults to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>The <b>RowVersion</b> <see cref="byte"/> array will be converted from an <see cref="HttpUtility.UrlDecode(string)">encoded</see> <see cref="string"/> value.</remarks>
        public DatabaseParameters RowVersionParam<T>(string name, T value, ParameterDirection direction = ParameterDirection.Input) where T : class
        {
            AddRowVersionParameter(name, value, direction);
            return this;
        }

        /// <summary>
        /// Adds a named <b>RowVersion</b> parameter (named <see cref="DatabaseColumns.RowVersionName"/>) where the <paramref name="value"/> implements <see cref="IETag"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (defaults to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>The <b>RowVersion</b> <see cref="byte"/> array will be converted from an <see cref="HttpUtility.UrlDecode(string)">encoded</see> <see cref="string"/> value.</remarks>
        public DatabaseParameters RowVersionParam<T>(T value, ParameterDirection direction = ParameterDirection.Input) where T : class
        {
            AddRowVersionParameter(value, direction);
            return this;
        }

        #endregion

        #region AddReturnValueParamater + ReturnValueParam

        /// <summary>
        /// Adds an <see cref="Int32"/> <see cref="ParameterDirection.ReturnValue"/> (named <see cref="DatabaseColumns.ReturnValueName"/>) parameter to the command.
        /// </summary>
        /// <returns>A <see cref="DbParameter"/>.</returns>
        public DbParameter AddReturnValueParameter()
        {
            var p = DatabaseCommand.Database.Provider.CreateParameter();
            p.ParameterName = "@" + DatabaseColumns.ReturnValueName;
            p.DbType = DbType.Int32;
            p.Direction = ParameterDirection.ReturnValue;
            DatabaseCommand.DbCommand.Parameters.Add(p);
            return p;
        }

        /// <summary>
        /// Adds a <see cref="ParameterDirection.ReturnValue"/> (named <see cref="DatabaseColumns.ReturnValueName"/>) parameter to the command.
        /// </summary>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        public DatabaseParameters ReturnValueParam()
        {
            AddReturnValueParameter();
            return this;
        }

        #endregion

        #region AddChangeLogParameters + ChangeLogParams

        /// <summary>
        /// Adds the <see cref="ChangeLog"/> parameters.
        /// </summary>
        /// <param name="changeLog">The <see cref="ChangeLog"/> value.</param>
        /// <param name="addCreatedParams">Indicates whether to add the <b>created</b> <see cref="IChangeLog"/> parameters.</param>
        /// <param name="addUpdatedParams">Indicates whether to add the <b>updated</b> <see cref="IChangeLog"/> parameters.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (defaults to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>An <see cref="DbParameter"/> array for those that were added.</returns>
        /// <remarks>Uses the following parameter names: <see cref="DatabaseColumns.CreatedByName"/>, <see cref="DatabaseColumns.CreatedDateName"/>,
        /// <see cref="DatabaseColumns.UpdatedByName"/> and <see cref="DatabaseColumns.UpdatedDateName"/>.</remarks>
        public DbParameter[] AddChangeLogParameters(ChangeLog changeLog, bool addCreatedParams = false, bool addUpdatedParams = false, ParameterDirection direction = ParameterDirection.Input)
        {
            if (changeLog == null)
                return Array.Empty<DbParameter>();

            var list = new List<DbParameter>();
            if (addCreatedParams)
            {
                list.Add(AddParameter("@" + DatabaseColumns.CreatedByName, (string)changeLog?.CreatedBy!, direction));
                list.Add(AddParameter("@" + DatabaseColumns.CreatedDateName, (DateTime?)changeLog?.CreatedDate, direction));
            }

            if (addUpdatedParams)
            {
                list.Add(AddParameter("@" + DatabaseColumns.UpdatedByName, (string)changeLog?.UpdatedBy!, direction));
                list.Add(AddParameter("@" + DatabaseColumns.UpdatedDateName, (DateTime?)changeLog?.UpdatedDate, direction));
            }

            return list.ToArray();
        }

        /// <summary>
        /// Adds the <see cref="ChangeLog"/> parameters where the <paramref name="value"/> implements <see cref="IChangeLog"/>.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="addCreatedParams">Indicates whether to add the <b>created</b> <see cref="IChangeLog"/> parameters.</param>
        /// <param name="addUpdatedParams">Indicates whether to add the <b>updated</b> <see cref="IChangeLog"/> parameters.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (defaults to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>An <see cref="DbParameter"/> array for those that were added.</returns>
        /// <remarks>Uses the following parameter names: <see cref="DatabaseColumns.CreatedByName"/>, <see cref="DatabaseColumns.CreatedDateName"/>,
        /// <see cref="DatabaseColumns.UpdatedByName"/> and <see cref="DatabaseColumns.UpdatedDateName"/>.</remarks>
        public DbParameter[] AddChangeLogParameters<T>(T value, bool addCreatedParams = false, bool addUpdatedParams = false, ParameterDirection direction = ParameterDirection.Input) where T : class
        {
            if (!(value is IChangeLog changeLog))
                return Array.Empty<DbParameter>();

            return AddChangeLogParameters(changeLog.ChangeLog, addCreatedParams, addUpdatedParams, direction);
        }

        /// <summary>
        /// Adds the <see cref="ChangeLog"/> parameters.
        /// </summary>
        /// <param name="changeLog">The <see cref="ChangeLog"/> value.</param>
        /// <param name="addCreatedParams">Indicates whether to add the <b>created</b> <see cref="IChangeLog"/> parameters.</param>
        /// <param name="addUpdatedParams">Indicates whether to add the <b>updated</b> <see cref="IChangeLog"/> parameters.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (defaults to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>Uses the following parameter names: <see cref="DatabaseColumns.CreatedByName"/>, <see cref="DatabaseColumns.CreatedDateName"/>,
        /// <see cref="DatabaseColumns.UpdatedByName"/> and <see cref="DatabaseColumns.UpdatedDateName"/>.</remarks>
        public DatabaseParameters ChangeLogParams(ChangeLog changeLog, bool addCreatedParams = false, bool addUpdatedParams = false, ParameterDirection direction = ParameterDirection.Input)
        {
            AddChangeLogParameters(changeLog, addCreatedParams, addUpdatedParams, direction);
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
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>Uses the following parameter names: <see cref="DatabaseColumns.CreatedByName"/>, <see cref="DatabaseColumns.CreatedDateName"/>,
        /// <see cref="DatabaseColumns.UpdatedByName"/> and <see cref="DatabaseColumns.UpdatedDateName"/>.</remarks>
        public DatabaseParameters ChangeLogParams<T>(T value, bool addCreatedParams = false, bool addUpdatedParams = false, ParameterDirection direction = ParameterDirection.Input) where T : class
        {
            AddChangeLogParameters<T>(value, addCreatedParams, addUpdatedParams, direction);
            return this;
        }

        #endregion

        #region AddPagingParameters + PagingsParams

        /// <summary>
        /// Adds the <see cref="PagingArgs"/> parameters.
        /// </summary>
        /// <param name="paging">The <see cref="PagingArgs"/> value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (defaults to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>An <see cref="DbParameter"/> array for those that were added.</returns>
        /// <remarks>Uses the following parameter names: <see cref="DatabaseColumns.PagingSkipName"/> and <see cref="DatabaseColumns.PagingTakeName"/>.</remarks>
        public DbParameter[] AddPagingParameters(PagingArgs paging, ParameterDirection direction = ParameterDirection.Input)
        {
            if (paging == null)
                return Array.Empty<DbParameter>();

            var list = new DbParameter[paging.IsGetCount ? 3 : 2];

            list[0] = AddParameter("@" + DatabaseColumns.PagingSkipName, paging.Skip, direction);
            list[1] = AddParameter("@" + DatabaseColumns.PagingTakeName, paging.Take, direction);

            if (paging.IsGetCount)
                list[2] = AddParameter("@" + DatabaseColumns.PagingCountName, paging.IsGetCount, direction);

            return list;
        }

        /// <summary>
        /// Adds the <see cref="PagingArgs"/> parameters.
        /// </summary>
        /// <param name="paging">The <see cref="PagingArgs"/> value.</param>
        /// <param name="direction">The <see cref="ParameterDirection"/> (defaults to <see cref="ParameterDirection.Input"/>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>Uses the following parameter names: <see cref="DatabaseColumns.PagingSkipName"/> and <see cref="DatabaseColumns.PagingTakeName"/>.</remarks>
        public DatabaseParameters PagingParams(PagingArgs paging, ParameterDirection direction = ParameterDirection.Input)
        {
            AddPagingParameters(paging, direction);
            return this;
        }

        #endregion

        #region AddTableValuedParameters

        /// <summary>
        /// Creates a <see cref="TableValuedParameter"/> <see cref="SqlParameter"/>.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="tvp">The <see cref="TableValuedParameter"/>.</param>
        /// <returns>A <see cref="SqlParameter"/>.</returns>
        /// <remarks>This specifically implies that the <see cref="SqlParameter"/> is being used; if not then an exception will be thrown.</remarks>
        public SqlParameter AddTableValuedParameter(string name, TableValuedParameter tvp)
        {
            if (tvp == null)
                throw new ArgumentNullException(nameof(tvp));

            var p = (SqlParameter)DatabaseCommand.Database.Provider.CreateParameter();
            p.ParameterName = name ?? throw new ArgumentNullException(nameof(name));
            p.SqlDbType = SqlDbType.Structured;
            p.TypeName = tvp.TypeName;
            p.Value = tvp.Value;
            p.Direction = ParameterDirection.Input;
            DatabaseCommand.DbCommand.Parameters.Add(p);
            return p;
        }

        /// <summary>
        /// Adds a <b>SQL Server</b> <see cref="TableValuedParameter"/>.
        /// </summary>
        /// <param name="name">The parameter name.</param>
        /// <param name="tvp">The <see cref="TableValuedParameter"/>.</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>This specifically implies that the <see cref="SqlParameter"/> is being used; if not then an exception will be thrown.</remarks>
        public DatabaseParameters TableValuedParam(string name, TableValuedParameter tvp)
        {
            AddTableValuedParameter(name, tvp);
            return this;
        }

        /// <summary>
        /// Adds a <b>SQL Server</b> <see cref="TableValuedParameter"/> <paramref name="when"/> <c>true</c>.
        /// </summary>
        /// <param name="when">Adds the parameter when <c>true</c>.</param>
        /// <param name="name">The parameter name.</param>
        /// <param name="tvp">The <see cref="TableValuedParameter"/>.</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>This specifically implies that the <see cref="SqlParameter"/> is being used; if not then an exception will be thrown.</remarks>
        public DatabaseParameters TableValuedParamWhen(bool when, string name, Func<TableValuedParameter> tvp)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (tvp == null)
                throw new ArgumentNullException(nameof(tvp));

            if (when)
                TableValuedParam(name, tvp());

            return this;
        }

        /// <summary>
        /// Adds a <b>SQL Server</b> <see cref="TableValuedParameter"/> when invoked <paramref name="with"/> a non-null value.
        /// </summary>
        /// <param name="with">The value <b>with</b> which to verify is non-null.</param>
        /// <param name="name">The parameter name.</param>
        /// <param name="tvp">The <see cref="TableValuedParameter"/>.</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        /// <remarks>This specifically implies that the <see cref="SqlParameter"/> is being used; if not then an exception will be thrown.</remarks>
        public DatabaseParameters TableValuedParamWith(object with, string name, Func<TableValuedParameter> tvp)
        {
            return TableValuedParamWhen(with != null, name, tvp);
        }

        #endregion

        #region ReselectRecordParam

        /// <summary>
        /// Adds a <b>ReselectRecord</b> parameter (named <see cref="DatabaseColumns.ReselectRecordName"/>).
        /// </summary>
        /// <param name="reselect">Indicates whether to reselect after the operation (defaults to <c>true</c>).</param>
        /// <returns>A <see cref="DbParameter"/>.</returns>
        public DbParameter AddReselectRecordParam(bool reselect = true)
        {
            return AddParameter("@" + DatabaseColumns.ReselectRecordName, reselect);
        }

        /// <summary>
        /// Adds a <b>ReselectRecord</b> parameter (named <see cref="DatabaseColumns.ReselectRecordName"/>).
        /// </summary>
        /// <param name="reselect">Indicates whether to reselect after the operation (defaults to <c>true</c>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        public DatabaseParameters ReselectRecordParam(bool reselect = true)
        {
            AddReselectRecordParam(reselect);
            return this;
        }

        /// <summary>
        /// Adds a <b>ReselectRecord</b> parameter (named <see cref="DatabaseColumns.ReselectRecordName"/>) <paramref name="when"/> <c>true</c>.
        /// </summary>
        /// <param name="when">Adds the parameter when <c>true</c>.</param>
        /// <param name="reselect">Indicates whether to reselect after the operation (defaults to <c>true</c>).</param>
        /// <returns>The current <see cref="DatabaseParameters"/> instance to support chaining (fluent interface).</returns>
        public DatabaseParameters ReselectRecordParamWhen(bool when, bool reselect = true)
        {
            if (when)
                AddReselectRecordParam(reselect);

            return this;
        }

        #endregion
    }
}
