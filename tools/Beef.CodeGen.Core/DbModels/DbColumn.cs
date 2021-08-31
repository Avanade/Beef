// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Data.Database;
using System;
using System.Text;

namespace Beef.CodeGen.DbModels
{
    /// <summary>
    /// Represents the SQL Server Database <b>Column</b> schema definition.
    /// </summary>
    public class DbColumn
    {
        /// <summary>
        /// Indicates whether the database type maps to a <see cref="string"/>.
        /// </summary>
        /// <param name="dbType">The database type.</param>
        public static bool TypeIsString(string dbType)
        {
            if (dbType == null)
                return false;

            switch (dbType.ToUpperInvariant())
            {
                case "NCHAR":
                case "CHAR":
                case "NVARCHAR":
                case "VARCHAR":
                case "TEXT":
                case "NTEXT":
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Indicates whether the database type maps to a <see cref="decimal"/>.
        /// </summary>
        /// <param name="dbType">The database type.</param>
        public static bool TypeIsDecimal(string dbType)
        {
            if (dbType == null)
                return false;

            switch (dbType.ToUpperInvariant())
            {
                case "DECIMAL":
                case "MONEY":
                case "NUMERIC":
                case "SMALLMONEY":
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Indicates whether the database type maps to a <see cref="DateTime"/>.
        /// </summary>
        /// <param name="dbType">The database type.</param>
        public static bool TypeIsDateTime(string dbType)
        {
            if (dbType == null)
                return false;

            switch (dbType.ToUpperInvariant())
            {
                case "DATE":
                case "DATETIME":
                case "DATETIME2":
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Indicates whether the database type maps to an integer.
        /// </summary>
        /// <param name="dbType">The database type.</param>
        public static bool TypeIsInteger(string dbType)
        {
            if (dbType == null)
                return false;

            switch (dbType.ToUpperInvariant())
            {
                case "INT":
                case "BIGINT":
                case "SMALLINT":
                    return true;

                default:
                    return false;
            }
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

            switch (dbType.ToUpperInvariant())
            {
                case "ROWVERSION":
                case "TIMESTAMP":
                case "BINARY": return "byte[]";
                case "VARBINARY": return "byte[]";
                case "BIT": return "bool";
                case "DATETIMEOFFSET": return "DateTimeOffset";
                case "FLOAT": return "double";
                case "INT": return "int";
                case "BIGINT": return "long";
                case "SMALLINT": return "short";
                case "TINYINT": return "byte";
                case "REAL": return "float";
                case "TIME": return "TimeSpan";
                case "UNIQUEIDENTIFIER": return "Guid";

                default:
                    throw new InvalidOperationException($"Database data type '{dbType}' does not have corresponding .NET type mapping defined.");
            }
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
        /// Gets the owning (parent) <see cref="DbTable"/>.
        /// </summary>
        public DbTable? DbTable { get; set; }

        /// <summary>
        /// Gets the column name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets the SQL Server data type.
        /// </summary>
        public string? Type { get; set; }

        /// <summary>
        /// Indicates whether the column is nullable.
        /// </summary>
        public bool IsNullable { get; set; }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        public int? Length { get; set; }

        /// <summary>
        /// Gets or sets the precision.
        /// </summary>
        public int? Precision { get; set; }

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        public int? Scale { get; set; }

        /// <summary>
        /// Indicates whether the column is an auto-generated identity.
        /// </summary>
        public bool IsIdentity { get; set; }

        /// <summary>
        /// Gets or sets the identity seed value.
        /// </summary>
        public int? IdentitySeed { get; set; }

        /// <summary>
        /// Gets or sets the identity increment value;
        /// </summary>
        public int? IdentityIncrement { get; set; }

        /// <summary>
        /// Indicates whether the column is computed.
        /// </summary>
        public bool IsComputed { get; set; }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        public string? DefaultValue { get; set; }

        /// <summary>
        /// Indicates whether the column is the primary key.
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// Indicates whether the column has a unique constraint.
        /// </summary>
        public bool IsUnique { get; set; }

        /// <summary>
        /// Gets or sets the foreign key table.
        /// </summary>
        public string? ForeignTable { get; set; }

        /// <summary>
        /// Gets or sets the foreign key schema.
        /// </summary>
        public string? ForeignSchema { get; set; }

        /// <summary>
        /// Gets or sets the foreign key column name.
        /// </summary>
        public string? ForeignColumn { get; set; }

        /// <summary>
        /// Indicates whether the foreign key is references a reference data table/entity.
        /// </summary>
        public bool IsForeignRefData { get; set; }

        /// <summary>
        /// Gets the corresponding .NET <see cref="System.Type"/> name.
        /// </summary>
        public string DotNetType => string.IsNullOrEmpty(Type) ? "string" : GetDotNetTypeName(Type);

        /// <summary>
        /// Gets the fully defined SQL type.
        /// </summary>
        public string SqlType
        {
            get
            {
                var sb = new StringBuilder(Type!.ToUpperInvariant());
                if (DbColumn.TypeIsString(Type))
                    sb.Append(Length.HasValue && Length.Value > 0 ? $"({Length.Value})" : "(MAX)");

                sb.Append(Type.ToUpperInvariant() switch
                {
                    "DECIMAL" => $"({Precision}, {Scale})",
                    "NUMERIC" => $"({Precision}, {Scale})",
                    "TIME" => Scale.HasValue && Scale.Value > 0 ? $"({Scale})" : string.Empty,
                    _ => string.Empty
                });

                if (IsNullable)
                    sb.Append(" NULL");

                return sb.ToString();
            }
        }

        /// <summary>
        /// Indicates whether the column is considered an audit column.
        /// </summary>
        public bool IsAudit => Name == DatabaseColumns.CreatedByName || Name == DatabaseColumns.CreatedDateName || Name == DatabaseColumns.UpdatedByName || Name == DatabaseColumns.UpdatedDateName || Name == DatabaseColumns.DeletedByName || Name == DatabaseColumns.DeletedDateName;

        /// <summary>
        /// Clones the <see cref="DbColumn"/> creating a new instance.
        /// </summary>
        /// <returns></returns>
        public DbColumn Clone()
        {
            return new DbColumn
            {
                DbTable = DbTable,
                Name = Name,
                Type = Type,
                IsNullable = IsNullable,
                Length = Length,
                Precision = Precision,
                Scale = Scale,
                IsIdentity = IsIdentity,
                IdentityIncrement = IdentityIncrement,
                IdentitySeed = IdentitySeed,
                IsComputed = IsComputed,
                DefaultValue = DefaultValue,
                IsPrimaryKey = IsPrimaryKey,
                IsUnique = IsUnique,
                ForeignTable = ForeignTable,
                ForeignSchema = ForeignSchema,
                ForeignColumn = ForeignColumn,
                IsForeignRefData = IsForeignRefData
            };
        }
    }

    /// <summary>
    /// Represents the <see cref="DbColumn"/> database mapper.
    /// </summary>
#pragma warning disable CA1812 // Apparently never instantiated; by-design - it is!
    internal class DbColumnMapper : DatabaseMapper<DbColumn, DbColumnMapper>
#pragma warning restore CA1812
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbColumnMapper"/> class.
        /// </summary>
        public DbColumnMapper()
        {
            Property(x => x.Name, "COLUMN_NAME");
            Property(x => x.Type, "DATA_TYPE");
            Property(x => x.IsNullable).MapFromDb((dr, c, ot) => dr.GetValue<string>("IS_NULLABLE").ToUpperInvariant() == "YES");
            Property(x => x.Length, "CHARACTER_MAXIMUM_LENGTH");
            Property(x => x.Precision).MapFromDb((dr, c, ot) => dr.GetValue<int?>("NUMERIC_PRECISION") ?? dr.GetValue<int?>("DATETIME_PRECISION"));
            Property(x => x.Scale, "NUMERIC_SCALE");
            Property(x => x.DefaultValue, "COLUMN_DEFAULT");
        }
    }
}
