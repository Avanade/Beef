// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.DbModels;
using System;
using System.Text;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Defines the column configuration.
    /// </summary>
    public interface IColumnConfig 
    {
        /// <summary>
        /// Gets the column name.
        /// </summary>
        string? Name { get; }

        /// <summary>
        /// Gets the database <see cref="DbModels.DbColumn"/> configuration.
        /// </summary>
        DbColumn? DbColumn { get; }

        /// <summary>
        /// Gets the qualified name (includes the alias).
        /// </summary>
        public string QualifiedName { get; }

        /// <summary>
        /// Gets the parameter name.
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// Gets the SQL type.
        /// </summary>
        public string? SqlType { get; }

        /// <summary>
        /// Gets the parameter SQL definition.
        /// </summary>
        public string? ParameterSql { get; }

        /// <summary>
        /// Gets the UDT SQL definition.
        /// </summary>
        public string? UdtSql { get; }

        /// <summary>
        /// Gets the where equality clause.
        /// </summary>
        public string WhereEquals { get; }

        /// <summary>
        /// Gets the SQL for defining initial value for comparisons.
        /// </summary>
        public string SqlInitialValue { get; }

        /// <summary>
        /// Indicates where the column is the "TenantId" column.
        /// </summary>
        public bool IsTenantIdColumn { get; }

        /// <summary>
        /// Indicates where the column is the "OrgUnitId" column.
        /// </summary>
        public bool IsOrgUnitIdColumn { get; }

        /// <summary>
        /// Indicates where the column is the "RowVersion" column.
        /// </summary>
        public bool IsRowVersionColumn { get; }

        /// <summary>
        /// Indicates where the column is the "IsDeleted" column.
        /// </summary>
        public bool IsIsDeletedColumn { get; }

        /// <summary>
        /// Indicates whether the column is considered an audit column.
        /// </summary>
        public bool IsAudit { get; }

        /// <summary>
        /// Indicates whether the column is "CreatedBy" or "CreatedDate".
        /// </summary>
        public bool IsCreated { get; }

        /// <summary>
        /// Indicates whether the column is "CreatedBy".
        /// </summary>
        public bool IsCreatedBy { get; }

        /// <summary>
        /// Indicates whether the column is "CreatedDate".
        /// </summary>
        public bool IsCreatedDate { get; }

        /// <summary>
        /// Indicates whether the column is "UpdatedBy" or "UpdatedDate".
        /// </summary>
        public bool IsUpdated { get; }

        /// <summary>
        /// Indicates whether the column is "UpdatedBy".
        /// </summary>
        public bool IsUpdatedBy { get; }

        /// <summary>
        /// Indicates whether the column is "UpdatedDate".
        /// </summary>
        public bool IsUpdatedDate { get; }

        /// <summary>
        /// Indicates whether the column is "DeletedBy" or "DeletedDate".
        /// </summary>
        public bool IsDeleted { get; }

        /// <summary>
        /// Indicates whether the column is "DeletedBy".
        /// </summary>
        public bool IsDeletedBy { get; }

        /// <summary>
        /// Indicates whether the column is "DeletedDate".
        /// </summary>
        public bool IsDeletedDate { get; }

        /// <summary>
        /// Indicates where the column should be considered for a 'Create' operation.
        /// </summary>
        public bool IsCreateColumn { get; }

        /// <summary>
        /// Indicates where the column should be considered for a 'Update' operation.
        /// </summary>
        public bool IsUpdateColumn { get; }

        /// <summary>
        /// Indicates where the column should be considered for a 'Delete' operation.
        /// </summary>
        public bool IsDeleteColumn { get; }

        /// <summary>
        /// Indicates that the name ends with URL.
        /// </summary>
        public bool IsNameEndsWithUrl { get; }

        /// <summary>
        /// Gets the EF SQL Type.
        /// </summary>
        public string? EfSqlType { get; }

        /// <summary>
        /// Gets the corresponding .NET <see cref="System.Type"/> name.
        /// </summary>
        public string DotNetType { get; }

        /// <summary>
        /// Indicates whether the .NET property is nullable.
        /// </summary>
        public bool IsDotNetNullable { get; }

        /// <summary>
        /// Gets the name alias.
        /// </summary>
        public string? NameAlias { get; }

        /// <summary>
        /// Gets the qualified name with the alias (used in a select).
        /// </summary>
        public string QualifiedNameWithAlias { get; }
    }

    /// <summary>
    /// Represents the base column configuration.
    /// </summary>
    /// <typeparam name="TRoot">The root <see cref="Type"/>.</typeparam>
    /// <typeparam name="TParent">The parent <see cref="Type"/>.</typeparam>
    public abstract class ColumnConfigBase<TRoot, TParent> : ConfigBase<TRoot, TParent>, IColumnConfig where TRoot : ConfigBase where TParent : ConfigBase, ITableReference, ISpecialColumns
    {
        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the database <see cref="DbModels.DbColumn"/> configuration.
        /// </summary>
        public DbColumn? DbColumn { get; set; }

        /// <summary>
        /// Gets the qualified name (includes the alias).
        /// </summary>
        public string QualifiedName => $"[{Parent!.Alias}].[{Name}]";

        /// <summary>
        /// Gets the schema/table qualified name.
        /// </summary>
        public string TableQualififiedName => "[].[]";

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
        public string WhereEquals => Name == Parent?.ColumnIsDeleted?.Name ? $"({QualifiedName} IS NULL OR {QualifiedName} = 0)" : $"{QualifiedName} = {ParameterName}";

        /// <summary>
        /// Gets the SQL for defining initial value for comparisons.
        /// </summary>
        public string SqlInitialValue => DbColumn!.Type!.ToUpperInvariant() == "UNIQUEIDENTIFIER"
            ? "CONVERT(UNIQUEIDENTIFIER, '00000000-0000-0000-0000-000000000000')"
            : (DbColumn.TypeIsInteger(DbColumn!.Type) || DbColumn.TypeIsDecimal(DbColumn!.Type) ? "0" : "''");

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
        public string DotNetType => DbColumn.GetDotNetTypeName(DbColumn!.Type);

        /// <summary>
        /// Indicates whether the .NET property is nullable.
        /// </summary>
        public bool IsDotNetNullable => DbColumn!.IsNullable || DotNetType == "string" || DotNetType == "byte[]";

        /// <summary>
        /// Gets or sets the name alias.
        /// </summary>
        public string? NameAlias { get; set; }

        /// <summary>
        /// Gets the qualified name with the alias (used in a select).
        /// </summary>
        public string QualifiedNameWithAlias => string.IsNullOrEmpty(NameAlias) || NameAlias == Name ? QualifiedName : $"{QualifiedName} AS [{NameAlias}]";

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            NameAlias = DefaultWhereNull(NameAlias, () => Name);
            UpdateSqlProperties();
        }

        /// <summary>
        /// Update the required SQL properties.
        /// </summary>
        private void UpdateSqlProperties()
        {
            var sb = new StringBuilder(DbColumn!.Type!.ToUpperInvariant());
            if (DbColumn.TypeIsString(DbColumn!.Type))
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
    }

    /// <summary>
    /// Represents the <see cref="TableConfig"/> column configuration.
    /// </summary>
    public class TableColumnConfig : ColumnConfigBase<CodeGenConfig, TableConfig> { }

    /// <summary>
    /// Represents the <see cref="QueryConfig"/> column configuration.
    /// </summary>
    public class QueryColumnConfig : ColumnConfigBase<CodeGenConfig, QueryConfig> { }

    /// <summary>
    /// Represents the <see cref="QueryJoinConfig"/> column configuration.
    /// </summary>
    public class QueryJoinColumnConfig : ColumnConfigBase<CodeGenConfig, QueryJoinConfig> { }

    /// <summary>
    /// Represents the <see cref="QueryConfig"/> column configuration.
    /// </summary>
    public class CdcColumnConfig : ColumnConfigBase<CodeGenConfig, CdcConfig> { }

    /// <summary>
    /// Represents the <see cref="QueryJoinConfig"/> column configuration.
    /// </summary>
    public class CdcJoinColumnConfig : ColumnConfigBase<CodeGenConfig, CdcJoinConfig> { }
}