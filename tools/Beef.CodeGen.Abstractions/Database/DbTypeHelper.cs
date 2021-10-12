// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Data.Common;

namespace Beef.CodeGen.Database
{
    /// <summary>
    /// Provides Database Type mappings.
    /// </summary>
    public static class DbTypeHelper
    {
        /// <summary>
        /// Indicates whether the database type maps to a <see cref="string"/>.
        /// </summary>
        /// <param name="dbType">The database type.</param>
        public static bool TypeIsString(string dbType)
        {
            if (dbType == null)
                return false;

            return dbType.ToUpperInvariant() switch
            {
                "NCHAR" or "CHAR" or "NVARCHAR" or "VARCHAR" or "TEXT" or "NTEXT" => true,
                _ => false,
            };
        }

        /// <summary>
        /// Indicates whether the database type maps to a <see cref="decimal"/>.
        /// </summary>
        /// <param name="dbType">The database type.</param>
        public static bool TypeIsDecimal(string dbType)
        {
            if (dbType == null)
                return false;

            return dbType.ToUpperInvariant() switch
            {
                "DECIMAL" or "MONEY" or "NUMERIC" or "SMALLMONEY" => true,
                _ => false,
            };
        }

        /// <summary>
        /// Indicates whether the database type maps to a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dbType">The database type.</param>
        public static bool TypeIsDateTime(string dbType)
        {
            if (dbType == null)
                return false;

            return dbType.ToUpperInvariant() switch
            {
                "DATE" or "DATETIME" or "DATETIME2" => true,
                _ => false,
            };
        }

        /// <summary>
        /// Indicates whether the database type maps to an integer.
        /// </summary>
        /// <param name="dbType">The database type.</param>
        public static bool TypeIsInteger(string dbType)
        {
            if (dbType == null)
                return false;

            return dbType.ToUpperInvariant() switch
            {
                "INT" or "BIGINT" or "SMALLINT" => true,
                _ => false,
            };
        }

        /// <summary>
        /// Gets the corresponding .NET <see cref="System.Type"/> name for the database type.
        /// </summary>
        /// <param name="dbType">The database type.</param>
        /// <returns>The .NET <see cref="System.Type"/> name.</returns>
        public static string GetDotNetTypeName(string? dbType)
        {
            // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-data-type-mappings

            if (string.IsNullOrEmpty(dbType))
                return "string";
            else if (TypeIsString(dbType))
                return "string";
            else if (TypeIsDecimal(dbType))
                return "decimal";
            else if (TypeIsDateTime(dbType))
                return "DateTime";

            return dbType.ToUpperInvariant() switch
            {
                "ROWVERSION" => "byte[]",
                "TIMESTAMP" => "byte[]",
                "BINARY" => "byte[]",
                "VARBINARY" => "byte[]",
                "BIT" => "bool",
                "DATETIMEOFFSET" => "DateTimeOffset",
                "FLOAT" => "double",
                "INT" => "int",
                "BIGINT" => "long",
                "SMALLINT" => "short",
                "TINYINT" => "byte",
                "REAL" => "float",
                "TIME" => "TimeSpan",
                "UNIQUEIDENTIFIER" => "Guid",
                _ => throw new InvalidOperationException($"Database data type '{dbType}' does not have corresponding .NET type mapping defined."),
            };
        }

        /// <summary>
        /// Gets the corresponding .NET <see cref="System.Type"/> for the database type.
        /// </summary>
        /// <param name="dbType">The database type.</param>
        /// <returns>The .NET <see cref="System.Type"/> name.</returns>
        public static Type GetDotNetType(string dbType)
        {
            return (GetDotNetTypeName(dbType).ToUpperInvariant()) switch
            {
                "STRING" => typeof(string),
                "DECIMAL" => typeof(decimal),
                "DATETIME" => typeof(DateTime),
                "BINARY" => typeof(byte[]),
                "VARBINARY" => typeof(byte[]),
                "BOOL" => typeof(bool),
                "DATETIMEOFFSET" => typeof(DateTimeOffset),
                "DOUBLE" => typeof(double),
                "INT" => typeof(int),
                "LONG" => typeof(long),
                "SHORT" => typeof(short),
                "BYTE" => typeof(byte),
                "FLOAT" => typeof(float),
                "TIMESPAN" => typeof(TimeSpan),
                "GUID" => typeof(Guid),
                _ => throw new InvalidOperationException($"Database data type '{dbType}' does not have corresponding .NET type mapping defined."),
            };
        }

        /// <summary>
        /// Gets the named <see cref="DbDataReader"/> column value.
        /// </summary>
        /// <typeparam name="T">The value <see cref="Type"/>.</typeparam>
        /// <param name="dr">The <see cref="DbDataReader"/>.</param>
        /// <param name="name">The column name.</param>
        /// <returns>The value.</returns>
        public static T GetValue<T>(this DbDataReader dr, string name)
        {
            var i = dr.GetOrdinal(name);
            if (dr.IsDBNull(i))
                return default!;

            var nt = Nullable.GetUnderlyingType(typeof(T));
            if (nt == null)
                return dr.GetFieldValue<T>(i);
            else
                return (T)Convert.ChangeType(dr.GetValue(i), nt, System.Globalization.CultureInfo.InvariantCulture);
        }
    }
}