// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Newtonsoft.Json;
using System;
using System.Linq;

namespace Beef.CodeGen.Config.Database
{
    /// <summary>
    /// Represents the table join on condition configuration.
    /// </summary>
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("CdcJoinOn", Title = "'CdcJoinOn' object (database-driven)",
        Description = "The `CdcJoinOn` object defines the join on characteristics for a CDC join.",
        ExampleMarkdown = @"A YAML configuration example is as follows:
``` yaml

```")]
    [CategorySchema("Key", Title = "Provides the _key_ configuration.")]
    public class CdcJoinOnConfig : ConfigBase<CodeGenConfig, CdcJoinConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks><inheritdoc/></remarks>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("CdcJoinOn", Name);

        #region Key

        /// <summary>
        /// Gets or sets the name of the join column (from the `Join` table).
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the join column (from the `Join` table).", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the join to column.
        /// </summary>
        [JsonProperty("toColumn", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The name of the join to column.", IsImportant = true,
            Description = "Defaults to `Name`; i.e. assumes same name.")]
        public string? ToColumn { get; set; }

        /// <summary>
        /// Gets or sets the SQL statement for the join on bypassing the corresponding `Column` specification.
        /// </summary>
        [JsonProperty("toStatement", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The SQL statement for the join on bypassing the corresponding `Column` specification.")]
        public string? ToStatement { get; set; }

        #endregion

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            CheckKeyHasValue(Name);
            CheckOptionsProperties();

            if (Name != null && Name.StartsWith("@", StringComparison.OrdinalIgnoreCase))
                Name = Name[1..];

            var c = Parent!.DbTable!.Columns.Where(x => x.Name == Name).SingleOrDefault();
            if (c == null)
                throw new CodeGenException(this, nameof(Name), $"JoinOn '{Name}' (Schema.Table '{Parent!.Schema}.{Parent!.Name}') not found in database.");

            if (string.IsNullOrEmpty(ToStatement))
            {
                ToColumn = DefaultWhereNull(ToColumn, () => Name);

                c = Root!.DbTables.Where(x => x.Schema == Parent.JoinToSchema && x.Name == Parent.JoinTo).SingleOrDefault()?.Columns.Where(x => x.Name == ToColumn).SingleOrDefault();
                if (c == null)
                    throw new CodeGenException(this, nameof(ToColumn), $"ToColumn '{ToColumn}' (Schema.Table '{Parent.JoinToSchema}.{Parent.JoinTo}') not found in database.");

                if (Parent.JoinToSchema == Parent!.Parent!.Schema && Parent.JoinTo == Parent!.Parent!.Name)
                {
                    if (Parent!.Parent!.DbTable!.Columns.Where(x => x.Name == ToColumn).SingleOrDefault() == null)
                        throw new CodeGenException(this, nameof(ToColumn), $"JoinOn To '{ToColumn}' (Schema.Table '{Parent.JoinToSchema}.{Parent.JoinTo}') not found in Table/Join configuration.");
                }
                else
                {
                    var t = Parent!.Parent!.Joins!.Where(x => Parent.JoinToSchema == x.Schema && Parent.JoinTo == x.Name).SingleOrDefault();
                    if (t == null || t.DbTable!.Columns.Where(x => x.Name == ToColumn).SingleOrDefault() == null)
                        throw new CodeGenException(this, nameof(ToColumn), $"JoinOn To '{ToColumn}' (Schema.Table '{Parent.JoinToSchema}.{Parent.JoinTo}') not found in Table/Join configuration.");
                }
            }
        }
    }
}