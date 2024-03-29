﻿// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using DbEx.DbSchema;
using OnRamp;
using OnRamp.Config;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents a database entity framework (EF) relationship configuration.
    /// </summary>
    [CodeGenClass("Relationship", Title = "'Relationship' object (database-driven)",
        Description = "The `Relationship` object enables the definition of an entity framework (EF) model relationship.")]
    [CodeGenCategory("Key", Title = "Provides the _key_ configuration.")]
    [CodeGenCategory("EF", Title = "Provides the _.NET Entity Framework (EF)_ specific configuration.")]
    [CodeGenCategory("DotNet", Title = "Provides the _.NET_ configuration.")]
    public class EfRelationshipConfig : ConfigBase<CodeGenConfig, TableConfig>, ITableReference
    {
        #region Key

        /// <summary>
        /// Gets or sets the related table name.
        /// </summary>
        [JsonPropertyName("name")]
        [CodeGenProperty("Key", Title = "The name of the primary table of the query.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the related schema name.
        /// </summary>
        [JsonPropertyName("schema")]
        [CodeGenProperty("Key", Title = "The schema name of the primary table of the view.",
            Description = "Defaults to `CodeGeneration.Schema`.")]
        public string? Schema { get; set; }

        /// <summary>
        /// Gets or sets the relationship type.
        /// </summary>
        [JsonPropertyName("type")]
        [CodeGenProperty("Key", Title = "The relationship type between the parent and child (self).", IsImportant = true, Options = ["OneToMany", "ManyToOne"],
            Description = "Defaults to `OneToMany`.")]
        public string? Type { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names from the related table that reference the parent.
        /// </summary>
        [JsonPropertyName("foreignKeyColumns")]
        [CodeGenPropertyCollection("Key", Title = "The list of `Column` names from the related table that reference the parent.", IsMandatory = true, IsImportant = true)]
        public List<string>? ForeignKeyColumns { get; set; }

        /// <summary>
        /// Gets or sets the list of `Column` names from the principal table that reference the child.
        /// </summary>
        [JsonPropertyName("principalKeyColumns")]
        [CodeGenPropertyCollection("Key", Title = "The list of `Column` names from the principal table that reference the child.",
            Description = " Typically this is only used where referencing property(s) other than the primary key as the principal property(s).")]
        public List<string>? PrincipalKeyColumns { get; set; }

        #endregion

        #region EF

        /// <summary>
        /// Gets or sets the operation applied on delete.
        /// </summary>
        [JsonPropertyName("onDelete")]
        [CodeGenProperty("EF", Title = "The operation applied to dependent entities in the relationship when the principal is deleted or the relationship is severed.",
            Options = ["NoAction", "Cascade", "ClientCascade", "ClientNoAction", "ClientSetNull", "Restrict", "SetNull"],
            Description = "Defaults to `NoAction`. See https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.deletebehavior for more information.")]
        public string? OnDelete { get; set; }

        /// <summary>
        /// Indicates whether to automatically include navigation to the property.
        /// </summary>
        [JsonPropertyName("autoInclude")]
        [CodeGenProperty("EF", Title = "Indicates whether to automatically include navigation to the property.",
            Description = "Defaults to `false`.")]
        public bool? AutoInclude { get; set; }

        #endregion

        #region DotNet

        /// <summary>
        /// Gets or sets the corresponding property name within the entity framework (EF) model.
        /// </summary>
        [JsonPropertyName("propertyName")]
        [CodeGenProperty("DotNet", Title = "The corresponding property name within the entity framework (EF) model.",
            Description = "Defaults to `Name` using the `CodeGeneration.AutoDotNetRename` option.")]
        public string? PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the corresponding entity framework (EF) model name (.NET Type).
        /// </summary>
        [JsonPropertyName("efModelName")]
        [CodeGenProperty("DotNet", Title = "The corresponding entity framework (EF) model name (.NET Type).",
            Description = "Defaults to `Name` using the `CodeGeneration.AutoDotNetRename` option.")]
        public string? EfModelName { get; set; }

        #endregion

        /// <summary>
        /// Gets the table name.
        /// </summary>
        public string? Table => Name;

        /// <summary>
        /// Gets the alias name.
        /// </summary>
        public string? Alias => null;

        /// <summary>
        /// Gets the corresponding (actual) database table configuration.
        /// </summary>
        public DbTableSchema? DbTable { get; private set; }
        
        /// <summary>
        /// Gets the list of foreign key(s) <see cref="DbColumnSchema"/>.
        /// </summary>
        public List<RefKeyColumn> ForeignKeyDbColumns = [];

        /// <summary>
        /// Gets the list of principal key(s) <see cref="DbColumnSchema"/>.
        /// </summary>
        public List<RefKeyColumn> PrincipalKeyDbColumns = [];

        /// <summary>
        /// Foreign/principal key column.
        /// </summary>
        public class RefKeyColumn
        {
            /// <summary>
            /// Gets the .NET property name.
            /// </summary>
            public string? PropertyName { get; set; }

            /// <summary>
            /// Gets the <see cref="DbColumnSchema"/>.
            /// </summary>
            public DbColumnSchema? DbColumn { get; set; }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override Task PrepareAsync()
        {
            Schema = DefaultWhereNull(Schema, () => Root!.Schema);
            DbTable = Root!.DbTables!.Where(x => x.Name == Name && x.Schema == Schema).SingleOrDefault();
            if (DbTable == null)
                throw new CodeGenException(this, nameof(Name), $"Specified Schema.Table '{CodeGenConfig.FormatSchemaTableName(Schema, Name)}' not found in database.");

            Type = DefaultWhereNull(Type, () => "OneToMany");
            PropertyName = DefaultWhereNull(PropertyName, () => Root!.RenameForDotNet(Name));
            EfModelName = DefaultWhereNull(EfModelName, () => Root!.RenameForDotNet(Name));
            OnDelete = DefaultWhereNull(OnDelete, () => "NoAction");

            if (ForeignKeyColumns == null || ForeignKeyColumns.Count == 0)
                throw new CodeGenException(this, nameof(ForeignKeyColumns), $"At least one foreign key column must be specified.");

            var fkt = Type == "OneToMany" ? DbTable : Parent!.DbTable!;
            var pkt = Type == "OneToMany" ? Parent!.DbTable! : DbTable;

            foreach (var fkc in ForeignKeyColumns)
            {
                var fkci = new RefKeyColumn { DbColumn = fkt.Columns.Where(x => x.Name == fkc).SingleOrDefault() };
                if (fkci.DbColumn == null)
                    throw new CodeGenException(this, nameof(ForeignKeyColumns), $"Foreign key column '{fkc}' does not exist in table '{fkt.QualifiedName}'.");

                fkci.PropertyName = Root!.RenameForDotNet(fkc);
                ForeignKeyDbColumns.Add(fkci);
            }

            if (PrincipalKeyColumns is not null)
            {
                foreach (var pkc in PrincipalKeyColumns)
                {
                    var pkci = new RefKeyColumn { DbColumn = pkt.Columns.Where(x => x.Name == pkc).SingleOrDefault() };
                    if (pkci.DbColumn == null)
                        throw new CodeGenException(this, nameof(ForeignKeyColumns), $"Principal key column '{pkc}' does not exist in table '{pkt.QualifiedName}'.");

                    pkci.PropertyName = Root!.RenameForDotNet(pkc);
                    PrincipalKeyDbColumns.Add(pkci);
                }
            }

            return Task.CompletedTask;
        }
    }
}