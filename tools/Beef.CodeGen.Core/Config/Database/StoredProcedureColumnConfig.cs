// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using OnRamp.Config;
using OnRamp.Database;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the stored procedure column configuration.
    /// </summary>
    public class StoredProcedureColumnConfig : ConfigBase<CodeGenConfig, StoredProcedureConfig>
    {
        /// <summary>
        /// Gets or sets the column name.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Gets the qualified name.
        /// </summary>
        public string QualifiedName => $"[{Parent!.Parent!.Alias}].[{Name}]";

        /// <summary>
        /// Gets the parameter name.
        /// </summary>
        public string ParameterName => $"@{Name}";

        /// <summary>
        /// Gets or sets the database column configuration.
        /// </summary>
        public DbColumn? DbColumn { get; set; }

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
        public bool IsCreatedBy => Name == Parent!.Parent!.ColumnCreatedBy?.Name;

        /// <summary>
        /// Indicates whether the column is "CreatedDate".
        /// </summary>
        public bool IsCreatedDate => Name == Parent!.Parent!.ColumnCreatedDate?.Name;

        /// <summary>
        /// Indicates whether the column is "UpdatedBy" or "UpdatedDate".
        /// </summary>
        public bool IsUpdated => IsUpdatedBy || IsUpdatedDate;

        /// <summary>
        /// Indicates whether the column is "UpdatedBy".
        /// </summary>
        public bool IsUpdatedBy => Name == Parent!.Parent!.ColumnUpdatedBy?.Name;

        /// <summary>
        /// Indicates whether the column is "UpdatedDate".
        /// </summary>
        public bool IsUpdatedDate => Name == Parent!.Parent!.ColumnUpdatedDate?.Name;

        /// <summary>
        /// Indicates whether the column is "DeletedBy" or "DeletedDate".
        /// </summary>
        public bool IsDeleted => IsDeletedBy || IsDeletedDate;

        /// <summary>
        /// Indicates whether the column is "DeletedBy".
        /// </summary>
        public bool IsDeletedBy => Name == Parent!.Parent!.ColumnDeletedBy?.Name;

        /// <summary>
        /// Indicates whether the column is "DeletedDate".
        /// </summary>
        public bool IsDeletedDate => Name == Parent!.Parent!.ColumnDeletedDate?.Name;

        /// <summary>
        /// Indicates where the column is the "TenantId" column.
        /// </summary>
        public bool IsTenantIdColumn => Name == Parent!.Parent!.ColumnTenantId?.Name;

        /// <summary>
        /// Indicates where the column is the "OrgUnitId" column.
        /// </summary>
        public bool IsOrgUnitIdColumn => Name == Parent!.Parent!.ColumnOrgUnitId?.Name;

        /// <summary>
        /// Indicates where the column is the "RowVersion" column.
        /// </summary>
        public bool IsRowVersionColumn => Name == Parent!.Parent!.ColumnRowVersion?.Name;

        /// <summary>
        /// Indicates where the column is the "IsDeleted" column.
        /// </summary>
        public bool IsIsDeletedColumn => Name == Parent!.Parent!.ColumnIsDeleted?.Name;

        /// <summary>
        /// Gets the corresponding Audit parameter name.
        /// </summary>
        public string? AuditParameterName => IsCreatedBy || IsUpdatedBy || IsDeletedBy ? "@AuditBy" : IsCreatedDate || IsUpdatedDate || IsDeletedDate ? "@AuditDate" : null;

        /// <summary>
        /// Gets or sets the Merge value SQL.
        /// </summary>
        public string? MergeValueSql { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare() 
        {
            MergeValueSql = DefaultWhereNull(MergeValueSql, () => AuditParameterName);
        }
    }
}