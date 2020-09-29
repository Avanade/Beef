// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Entities;
using Newtonsoft.Json;
using System;
using System.Text;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the column configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("Parameter", Title = "The **Where** statement is used to define additional filtering.", Description = "", Markdown = "")]
    [CategorySchema("Key", Title = "Provides the **key** configuration.")]
    public class ColumnConfig : ConfigBase<CodeGenConfig, TableConfig>
    {
        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the database <see cref="Column"/> configuration.
        /// </summary>
        public Column? DbColumn { get; set; }

        /// <summary>
        /// Gets the qualified name (includes the alias).
        /// </summary>
        public string QualifiedName => $"[{Parent!.Alias}].[{Name}]";

        /// <summary>
        /// Gets the parameter name.
        /// </summary>
        public string ParameterName => "@" + Name;

        /// <summary>
        /// Gets the SQL type.
        /// </summary>
        public string? SqlType { get; private set; }

        /// <summary>
        /// Gets the parameter SQL definition.
        /// </summary>
        public string? ParameterSql { get; private set; }

        /// <summary>
        /// Gets the UDT SQL definition.
        /// </summary>
        public string? UdtSql { get; private set; }

        /// <summary>
        /// Gets the where equality clause.
        /// </summary>
        public string WhereEquals => Name == Parent?.ColumnIsDeleted?.Name ? $"ISNULL({QualifiedName}, 0) = 0" : $"{QualifiedName} = {ParameterName}";

        /// <summary>
        /// Gets the SQL for defining initial value for comparisons.
        /// </summary>
        public string SqlInitialValue => DbColumn!.Type!.ToUpperInvariant() == "UNIQUEIDENTIFIER"
            ? "CONVERT(UNIQUEIDENTIFIER, '00000000-0000-0000-0000-000000000000')"
            : (Column.TypeIsInteger(DbColumn!.Type) || TypeIsDecimal(DbColumn!.Type) ? "0" : "''");

        /// <summary>
        /// Indicates whether the column is considered an audit column.
        /// </summary>
        public bool IsAudit => IsCreated || IsUpdated || IsDeleted;

        /// <summary>
        /// Indicates whether the column is "CreatedBy" or "CreatedDate".
        /// </summary>
        public bool IsCreated => IsCreatedBy || IsCreatedDate;

        /// <summary>
        /// Indicates whether the column is "CreatedBy".
        /// </summary>
        public bool IsCreatedBy => Name == Parent!.ColumnCreatedBy?.Name;

        /// <summary>
        /// Indicates whether the column is "CreatedDate".
        /// </summary>
        public bool IsCreatedDate => Name == Parent!.ColumnCreatedDate?.Name;

        /// <summary>
        /// Indicates whether the column is "UpdatedBy" or "UpdatedDate".
        /// </summary>
        public bool IsUpdated => IsUpdatedBy || IsUpdatedDate;

        /// <summary>
        /// Indicates whether the column is "UpdatedBy".
        /// </summary>
        public bool IsUpdatedBy => Name == Parent!.ColumnUpdatedBy?.Name;

        /// <summary>
        /// Indicates whether the column is "UpdatedDate".
        /// </summary>
        public bool IsUpdatedDate => Name == Parent!.ColumnUpdatedDate?.Name;

        /// <summary>
        /// Indicates whether the column is "DeletedBy" or "DeletedDate".
        /// </summary>
        public bool IsDeleted => IsDeletedBy || IsDeletedDate;

        /// <summary>
        /// Indicates whether the column is "DeletedBy".
        /// </summary>
        public bool IsDeletedBy => Name == Parent!.ColumnDeletedBy?.Name;

        /// <summary>
        /// Indicates whether the column is "DeletedDate".
        /// </summary>
        public bool IsDeletedDate => Name == Parent!.ColumnDeletedDate?.Name;

        /// <summary>
        /// Indicates where the column should be considered for a 'Create' operation.
        /// </summary>
        public bool IsCreateColumn => (!DbColumn!.IsComputed && !IsAudit) || IsCreated;

        /// <summary>
        /// Indicates where the column should be considered for a 'Update' operation.
        /// </summary>
        public bool IsUpdateColumn => (!DbColumn!.IsComputed && !IsAudit) || IsUpdated;

        /// <summary>
        /// Indicates where the column should be considered for a 'Delete' operation.
        /// </summary>
        public bool IsDeleteColumn => (!DbColumn!.IsComputed && !IsAudit) || IsDeleted;

        /// <summary>
        /// Indicates where the column is the "TenantId" column.
        /// </summary>
        public bool IsTenantIdColumn => Name == Parent!.ColumnTenantId?.Name;

        /// <summary>
        /// Indicates where the column is the "OrgUnitId" column.
        /// </summary>
        public bool IsOrgUnitIdColumn => Name == Parent!.ColumnOrgUnitId?.Name;

        /// <summary>
        /// Indicates where the column is the "RowVersion" column.
        /// </summary>
        public bool IsRowVersionColumn => Name == Parent!.ColumnRowVersion?.Name;

        /// <summary>
        /// Indicates where the column is the "IsDeleted" column.
        /// </summary>
        public bool IsIsDeletedColumn => Name == Parent!.ColumnIsDeleted?.Name;

        /// <summary>
        /// Indicates that the name ends with URL.
        /// </summary>
        public bool IsNameEndsWithUrl => Name!.EndsWith("Url", System.StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets the EF SQL Type.
        /// </summary>
        public string? EfSqlType { get; set; }

        /// <summary>
        /// Gets the corresponding .NET <see cref="System.Type"/> name.
        /// </summary>
        public string DotNetType => GetDotNetTypeName(DbColumn!.Type);

        /// <summary>
        /// Indicates whether the .NET property is nullable.
        /// </summary>
        public bool IsDotNetNullable => DbColumn!.IsNullable || DotNetType == "string" || DotNetType == "byte[]";

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            UpdateSqlProperties();
        }

        /// <summary>
        /// Update the required SQL properties.
        /// </summary>
        private void UpdateSqlProperties()
        {
            var sb = new StringBuilder(DbColumn!.Type!.ToUpperInvariant());
            if (TypeIsString(DbColumn!.Type))
                sb.Append(DbColumn!.Length.HasValue && DbColumn!.Length.Value > 0 ? $"({DbColumn!.Length.Value})" : "(MAX)");

            sb.Append(DbColumn!.Type.ToUpperInvariant() switch
            {
                "DECIMAL" => $"({DbColumn!.Precision}, {DbColumn!.Scale})",
                "NUMERIC" => $"({DbColumn!.Precision}, {DbColumn!.Scale})",
                "TIME" => DbColumn!.Scale.HasValue && DbColumn!.Scale.Value > 0 ? $"({DbColumn!.Scale})" : string.Empty,
                _ => string.Empty
            });

            EfSqlType = sb.ToString();

            if (DbColumn!.IsNullable)
                sb.Append(" NULL");

            SqlType = sb.ToString();
            ParameterSql = $"{ParameterName} AS {SqlType}";
            UdtSql = $"[{Name}] {SqlType}";
        }

        #region Static

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
                case "DATETIMEOFFSET":
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
        public static string GetDotNetTypeName(string? dbType = "NVARCHAR")
        {
            // https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-data-type-mappings

            if (string.IsNullOrEmpty(dbType))
                throw new ArgumentNullException(nameof(dbType));

            if (TypeIsString(dbType))
                return "string";
            else if (TypeIsDecimal(dbType))
                return "decimal";
            else if (TypeIsDateTime(dbType))
                return "DateTime";

            switch (dbType.ToUpperInvariant())
            {
                case "ROWVERSION":
                case "TIMESTAMP":
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

        #endregion
    }
}