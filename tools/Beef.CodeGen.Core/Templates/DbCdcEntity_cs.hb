﻿{{! Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef }}
/*
 * This file is automatically generated; any changes will be lost. 
 */

#nullable enable
#pragma warning disable {{Root.PragmaWarnings}}

{{#ifval ColumnIsDeleted}}
using Beef.Data.Database.Cdc;
{{/ifval}}
using Beef.Entities;
using Beef.Mapper;
{{#ifeq Root.JsonSerializer 'Newtonsoft'}}
using Newtonsoft.Json;
{{/ifeq}}
using System;
using System.Collections.Generic;

namespace {{Root.NamespaceCdc}}.Entities
{
    /// <summary>
    /// Represents the CDC model for the root (primary) database table '{{Schema}}.{{Name}}'.
    /// </summary>
{{#ifeq Root.JsonSerializer 'Newtonsoft'}}
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
{{/ifeq}}
    public partial class {{ModelName}}Cdc : IUniqueKey, IETag{{#ifval ColumnIsDeleted}}, ILogicallyDeleted{{/ifval}}
    {
{{#each SelectedEntityColumns}}
        /// <summary>
        /// Gets or sets the '{{Name}}' column value.
        /// </summary>
  {{#ifeq Root.JsonSerializer 'Newtonsoft'}}
        [JsonProperty("{{camel NameAlias}}", DefaultValueHandling = {{#if SerializationEmitDefault}}DefaultValueHandling.Include{{else}}DefaultValueHandling.Ignore{{/if}})]
  {{/ifeq}}
        public {{DotNetType}}{{#if IsDotNetNullable}}?{{/if}} {{pascal NameAlias}} { get; set; }
  {{#unless @last}}

  {{else}}
    {{#ifne Parent.JoinNonCdcChildren.Count 0}}

    {{/ifne}}
  {{/unless}}
{{/each}}
{{#each JoinNonCdcChildren}}
  {{#each Columns}}
        /// <summary>
        /// Gets or sets the '{{Name}}' column value (join table '{{Parent.Schema}}.{{Parent.Name}}').
        /// </summary>
    {{#ifeq Root.JsonSerializer 'Newtonsoft'}}
        [JsonProperty("{{camel NameAlias}}", DefaultValueHandling = {{#if SerializationEmitDefault}}DefaultValueHandling.Include{{else}}DefaultValueHandling.Ignore{{/if}})]
    {{/ifeq}}
        public {{DotNetType}}{{#if IsDotNetNullable}}?{{/if}} {{pascal NameAlias}} { get; set; }
    {{#unless @last}}

    {{else}}
      {{#unless @../last}}

      {{/unless}}
    {{/unless}}
  {{/each}}
{{/each}}
{{#each JoinCdcChildren}}

  {{#ifeq JoinCardinality 'OneToMany'}}
        /// <summary>
        /// Gets or sets the related (one-to-many) <see cref="{{Parent.ModelName}}Cdc.{{ModelName}}Collection"/> (database table '{{Schema}}.{{TableName}}').
        /// </summary>
        [JsonProperty("{{camel PropertyName}}", DefaultValueHandling = {{#if SerializationEmitDefault}}DefaultValueHandling.Include{{else}}DefaultValueHandling.Ignore{{/if}})]
        [MapperIgnore()]
        public {{Parent.ModelName}}Cdc.{{ModelName}}CdcCollection? {{PropertyName}} { get; set; }
  {{else}}
        /// <summary>
        /// Gets or sets the related (one-to-one) <see cref="{{Parent.ModelName}}Cdc.{{ModelName}}"/> (database table '{{Schema}}.{{TableName}}').
        /// </summary>
        [JsonProperty("{{camel PropertyName}}", DefaultValueHandling = {{#if SerializationEmitDefault}}DefaultValueHandling.Include{{else}}DefaultValueHandling.Ignore{{/if}})]
        [MapperIgnore()]
        public {{Parent.ModelName}}Cdc.{{ModelName}}Cdc? {{PropertyName}} { get; set; }
  {{/ifeq}}
{{/each}}

        /// <summary>
        /// Gets or sets the entity tag {{#ifval ColumnRowVersion}}('{{ColumnRowVersion.Name}}' column){{else}}(calculated as JSON serialized hash value){{/ifval}}.
        /// </summary>
        [JsonProperty("etag", DefaultValueHandling = {{#if SerializationEmitDefault}}DefaultValueHandling.Include{{else}}DefaultValueHandling.Ignore{{/if}})]
  {{#ifval ColumnRowVersion}}
        [MapperProperty("{{ColumnRowVersion.Name}}", ConverterType = typeof(Beef.Data.Database.DatabaseRowVersionConverter))]
  {{else}}
        [MapperIgnore()]
  {{/ifval}}
        public string? ETag { get; set; }

  {{#ifval ColumnIsDeleted}}
        /// <summary>
        /// Indicates whether the entity is logically deleted ('{{ColumnIsDeleted.Name}}' column).
        /// </summary>
        [MapperProperty("{{ColumnIsDeleted.Name}}")]
        public bool IsDeleted { get; set; }

  {{/ifval}}
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [MapperIgnore()]
        public bool HasUniqueKey => true;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [MapperIgnore()]
        public UniqueKey UniqueKey => new UniqueKey({{#each PrimaryKeyColumns}}{{#unless @first}}, {{/unless}}{{pascal NameAlias}}{{/each}});

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        [MapperIgnore()]
        public string[] UniqueKeyProperties => new string[] { {{#each PrimaryKeyColumns}}{{#unless @first}}, {{/unless}}nameof({{pascal NameAlias}}){{/each}} };
{{#each CdcJoins}}

        #region {{ModelName}}Cdc

        /// <summary>
        /// Represents the CDC model for the related (child) database table '{{Schema}}.{{TableName}}' (known uniquely as '{{Name}}').
        /// </summary>
  {{#ifeq Root.JsonSerializer 'Newtonsoft'}}
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
  {{/ifeq}}
        public partial class {{ModelName}}Cdc : IUniqueKey
        {
  {{#each Columns}}
            /// <summary>
            /// Gets or sets the '{{NameAlias}}' ({{Parent.TableName}}.{{Name}}) column value.
            /// </summary>
    {{#ifeq Root.JsonSerializer 'Newtonsoft'}}
            [JsonProperty("{{camel NameAlias}}", DefaultValueHandling = {{#if SerializationEmitDefault}}DefaultValueHandling.Include{{else}}DefaultValueHandling.Ignore{{/if}})]
    {{/ifeq}}
            public {{DotNetType}}{{#if IsDotNetNullable}}?{{/if}} {{pascal NameAlias}} { get; set; }
    {{#unless @last}}

    {{else}}
      {{#ifne Parent.JoinNonCdcChildren.Count 0}}

      {{/ifne}}
    {{/unless}}
  {{/each}}
  {{#each JoinNonCdcChildren}}
    {{#each Columns}}
            /// <summary>
            /// Gets or sets the '{{Name}}' column value (join table '{{Parent.Schema}}.{{Parent.Name}}').
            /// </summary>
      {{#ifeq Root.JsonSerializer 'Newtonsoft'}}
            [JsonProperty("{{camel NameAlias}}", DefaultValueHandling = {{#if SerializationEmitDefault}}DefaultValueHandling.Include{{else}}DefaultValueHandling.Ignore{{/if}})]
      {{/ifeq}}
            public {{DotNetType}}{{#if IsDotNetNullable}}?{{/if}} {{pascal NameAlias}} { get; set; }
      {{#unless @last}}

      {{else}}
        {{#unless @../last}}

        {{/unless}}
      {{/unless}}
    {{/each}}
  {{/each}}
  {{#each JoinCdcChildren}}

    {{#ifeq JoinCardinality 'OneToMany'}}
            /// <summary>
            /// Gets or sets the related (one-to-many) <see cref="{{Parent.ModelName}}Cdc.{{ModelName}}Collection"/> (database table '{{Schema}}.{{TableName}}').
            /// </summary>
            [JsonProperty("{{camel PropertyName}}", DefaultValueHandling = {{#if SerializationEmitDefault}}DefaultValueHandling.Include{{else}}DefaultValueHandling.Ignore{{/if}})]
            [MapperIgnore()]
            public {{Parent.ModelName}}Cdc.{{ModelName}}CdcCollection? {{PropertyName}} { get; set; }
    {{else}}
            /// <summary>
            /// Gets or sets the related (one-to-one) <see cref="{{Parent.ModelName}}Cdc.{{ModelName}}"/> (database table '{{Schema}}.{{TableName}}').
            /// </summary>
            [JsonProperty("{{camel PropertyName}}", DefaultValueHandling = {{#if SerializationEmitDefault}}DefaultValueHandling.Include{{else}}DefaultValueHandling.Ignore{{/if}})]
            [MapperIgnore()]
            public {{Parent.ModelName}}Cdc.{{ModelName}}Cdc? {{PropertyName}} { get; set; }
    {{/ifeq}}
  {{/each}}

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            [MapperIgnore()]
            public bool HasUniqueKey => true;

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            [MapperIgnore()]
            public UniqueKey UniqueKey => new UniqueKey({{#each PrimaryKeyColumns}}{{#unless @first}}, {{/unless}}{{pascal NameAlias}}{{/each}});

            /// <summary>
            /// <inheritdoc/>
            /// </summary>
            [MapperIgnore()]
            public string[] UniqueKeyProperties => new string[] { {{#each PrimaryKeyColumns}}{{#unless @first}}, {{/unless}}nameof({{pascal NameAlias}}){{/each}} };
  {{#each JoinHierarchyReverse}}
    {{#unless @last}}
      {{#each OnSelectColumns}}

            /// <summary>
            /// Gets or sets the '{{Parent.JoinTo}}_{{Name}}' additional joining column (informational); for internal join use only (not serialized).
            /// </summary>
            public {{ToDbColumn.DotNetType}} {{Parent.JoinTo}}_{{Name}} { get; set; }
      {{/each}}
    {{/unless}}
  {{/each}}
        }

        /// <summary>
        /// Represents the CDC model for the related (child) database table collection '{{Schema}}.{{Name}}'.
        /// </summary>
        public partial class {{ModelName}}CdcCollection : List<{{ModelName}}Cdc> { }

        #endregion
{{/each}}
    }
}

#pragma warning restore {{Root.PragmaWarnings}}
#nullable restore