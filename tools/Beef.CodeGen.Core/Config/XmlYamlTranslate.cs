// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Beef.CodeGen.Config
{
    /// <summary>
    /// Represents a temporary capability to translate XML and YAML property names and values. For the most part the names will not change other than PascalCase versus camelCase.
    /// </summary>
    internal static class XmlYamlTranslate
    {
        private static readonly List<(ConfigType ConvertType, ConfigurationEntity Entity, string XmlName, string YamlName)> _config = new List<(ConfigType, ConfigurationEntity, string, string)>(new (ConfigType, ConfigurationEntity, string, string)[]
        {
            // Entity oriented configuration.
            (ConfigType.Entity, ConfigurationEntity.CodeGen, "AppendToNamespace", "refDataAppendToNamespace"),
            (ConfigType.Entity, ConfigurationEntity.CodeGen, "MapperDefaultRefDataConverter", "refDataDefaultMapperConverter"),

            (ConfigType.Entity, ConfigurationEntity.Entity, "AutoInferImplements", "implementsAutoInfer"),
            (ConfigType.Entity, ConfigurationEntity.Entity, "EntityFrameworkEntity", "entityFrameworkModel"),
            (ConfigType.Entity, ConfigurationEntity.Entity, "CosmosEntity", "cosmosModel"),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ODataEntity", "odataModel"),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ManagerConstructor", "managerCtor"),
            (ConfigType.Entity, ConfigurationEntity.Entity, "DataSvcConstructor", "dataSvcCtor"),
            (ConfigType.Entity, ConfigurationEntity.Entity, "DataConstructor", "dataCtor"),
            (ConfigType.Entity, ConfigurationEntity.Entity, "DataDatabaseMapperInheritsFrom", "databaseMapperInheritsFrom"),
            (ConfigType.Entity, ConfigurationEntity.Entity, "DataDatabaseCustomMapper", "databaseCustomMapper"),
            (ConfigType.Entity, ConfigurationEntity.Entity, "DataEntityFrameworkMapperInheritsFrom", "entityFrameworkMapperInheritsFrom"),
            (ConfigType.Entity, ConfigurationEntity.Entity, "DataEntityFrameworkCustomMapper", "entityFrameworkCustomMapper"),
            (ConfigType.Entity, ConfigurationEntity.Entity, "DataCosmosValueContainer", "cosmosValueContainer"),
            (ConfigType.Entity, ConfigurationEntity.Entity, "DataCosmosMapperInheritsFrom", "cosmosMapperInheritsFrom"),
            (ConfigType.Entity, ConfigurationEntity.Entity, "DataCosmosCustomMapper", "cosmosCustomMapper"),
            (ConfigType.Entity, ConfigurationEntity.Entity, "DataODataMapperInheritsFrom", "odataMapperInheritsFrom"),
            (ConfigType.Entity, ConfigurationEntity.Entity, "DataODataCustomMapper", "odataCustomMapper"),

            (ConfigType.Entity, ConfigurationEntity.Property, "IgnoreSerialization", "serializationIgnore"),
            (ConfigType.Entity, ConfigurationEntity.Property, "EmitDefaultValue", "serializationEmitDefault"),
            (ConfigType.Entity, ConfigurationEntity.Property, "IsDataConverterGeneric", "dataConverterIsGeneric"),
            (ConfigType.Entity, ConfigurationEntity.Property, "DataDatabaseMapper", "databaseMapper"),
            (ConfigType.Entity, ConfigurationEntity.Property, "DataDatabaseIgnore", "databaseIgnore"),

            (ConfigType.Entity, ConfigurationEntity.Operation, "OperationType", "type"),
            (ConfigType.Entity, ConfigurationEntity.Operation, "PagingArgs", "paging"),
            (ConfigType.Entity, ConfigurationEntity.Operation, "DataCosmosContainerId", "cosmosContainerId"),
            (ConfigType.Entity, ConfigurationEntity.Operation, "DataCosmosPartitionKey", "cosmosPartitionKey"),

            (ConfigType.Entity, ConfigurationEntity.Parameter, "IsDataConverterGeneric", "dataConverterIsGeneric"),
            (ConfigType.Entity, ConfigurationEntity.Parameter, "ValidatorFluent", "validatorCode"),

            // Database oriented configuration.
            (ConfigType.Database, ConfigurationEntity.StoredProcedure, "OrderBy", "orderby"),

            (ConfigType.Database, ConfigurationEntity.Parameter, "IsNullable", "nullable"),
            (ConfigType.Database, ConfigurationEntity.Parameter, "IsCollection", "collection")
        });

        private static readonly List<(ConfigType ConvertType, ConfigurationEntity Entity, string XmlName, bool IsArray, Func<string?, string?>? Converter)> _xmlToYamlConvert = new List<(ConfigType, ConfigurationEntity, string, bool, Func<string?, string?>?)>(new (ConfigType, ConfigurationEntity, string, bool, Func<string?, string?>?)[]
        {
            (ConfigType.Entity, ConfigurationEntity.CodeGen, "xmlns", false, (xml) => NullValue()),
            (ConfigType.Entity, ConfigurationEntity.CodeGen, "xsi", false, (xml) => NullValue()),
            (ConfigType.Entity, ConfigurationEntity.CodeGen, "noNamespaceSchemaLocation", false, (xml) => NullValue()),
            (ConfigType.Entity, ConfigurationEntity.CodeGen, "WebApiAuthorize", false, (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? "Authorize" : (xml == "false" ? "AllowAnonymous" : xml))),
            (ConfigType.Entity, ConfigurationEntity.CodeGen, "EventPublish", false, (xml) => ConvertEventPublish(xml)),
            (ConfigType.Entity, ConfigurationEntity.CodeGen, "EntityUsing", false, (xml) => throw new CodeGenException("CodeGeneration.EntityUsing has been deprecated; is replaced by CodeGeneration.EntityScope, Entity.EntityScope and Entity.EntityUsing.")),

            (ConfigType.Entity, ConfigurationEntity.Entity, "ManagerCtorParams", true, null),
            (ConfigType.Entity, ConfigurationEntity.Entity, "DataSvcCtorParams", true, null),
            (ConfigType.Entity, ConfigurationEntity.Entity, "DataCtorParams", true, null),
            (ConfigType.Entity, ConfigurationEntity.Entity, "WebApiCtorParams", true, null),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeData", false, (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? "Exclude" : "RequiresMapper")),
            (ConfigType.Entity, ConfigurationEntity.Entity, "WebApiAuthorize", false, (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? "Authorize" : (xml == "false" ? "AllowAnonymous" : xml))),
            (ConfigType.Entity, ConfigurationEntity.Entity, "EventPublish", false, (xml) => ConvertEventPublish(xml)),

            (ConfigType.Entity, ConfigurationEntity.Operation, "WebApiAuthorize", false, (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? "Authorize" : (xml == "false" ? "AllowAnonymous" : xml))),
            (ConfigType.Entity, ConfigurationEntity.Operation, "WebApiOperationType", false, (xml) => throw new CodeGenException("Operation.WebApiOperationType has been renamed; please change to Operation.ManagerOperationType.")),
            (ConfigType.Entity, ConfigurationEntity.Operation, "EventPublish", false, (xml) => ConvertEventPublish(xml)),

            (ConfigType.Entity, ConfigurationEntity.Property, "Type", false, (xml) => xml != null && xml.StartsWith("RefDataNamespace.", StringComparison.InvariantCulture) ? $"^{xml[17..]}" : xml),

            (ConfigType.Entity, ConfigurationEntity.Parameter, "Type", false, (xml) => xml != null && xml.StartsWith("RefDataNamespace.", StringComparison.InvariantCulture) ? $"^{xml[17..]}" : xml),

            (ConfigType.Database, ConfigurationEntity.CodeGen, "xmlns", false, (xml) => NullValue()),
            (ConfigType.Database, ConfigurationEntity.CodeGen, "xsi", false, (xml) => NullValue()),
            (ConfigType.Database, ConfigurationEntity.CodeGen, "noNamespaceSchemaLocation", false, (xml) => NullValue()),
            (ConfigType.Database, ConfigurationEntity.CodeGen, "CdcExcludeColumnsFromETag", true, null),

            (ConfigType.Database, ConfigurationEntity.Query, "IncludeColumns", true, null),
            (ConfigType.Database, ConfigurationEntity.Query, "ExcludeColumns", true, null),
            (ConfigType.Database, ConfigurationEntity.Query, "AliasColumns", true, null),

            (ConfigType.Database, ConfigurationEntity.QueryJoin, "IncludeColumns", true, null),
            (ConfigType.Database, ConfigurationEntity.QueryJoin, "ExcludeColumns", true, null),
            (ConfigType.Database, ConfigurationEntity.QueryJoin, "AliasColumns", true, null),

            (ConfigType.Database, ConfigurationEntity.Table, "IncludeColumns", true, null),
            (ConfigType.Database, ConfigurationEntity.Table, "ExcludeColumns", true, null),
            (ConfigType.Database, ConfigurationEntity.Table, "GetAllOrderBy", true, null),
            (ConfigType.Database, ConfigurationEntity.Table, "UdtExcludeColumns", true, null),
            (ConfigType.Database, ConfigurationEntity.Table, "View", false, (xml) => throw new CodeGenException("Table.View property is no longer supported; please use the new Query capability (more advanced).")),
            (ConfigType.Database, ConfigurationEntity.Table, "ViewName", false, (xml) => throw new CodeGenException("Table.View property is no longer supported; please use the new Query capability (more advanced).")),
            (ConfigType.Database, ConfigurationEntity.Table, "ViewSchema", false, (xml) => throw new CodeGenException("Table.View property is no longer supported; please use the new Query capability (more advanced).")),

            (ConfigType.Database, ConfigurationEntity.StoredProcedure, "Type", false, (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "GetAll" ? "GetColl" : xml)),
            (ConfigType.Database, ConfigurationEntity.StoredProcedure, "IncludeColumns", true, null),
            (ConfigType.Database, ConfigurationEntity.StoredProcedure, "ExcludeColumns", true, null),
            (ConfigType.Database, ConfigurationEntity.StoredProcedure, "MergeOverrideIdentityColumns", true, null),

            (ConfigType.Database, ConfigurationEntity.OrderBy, "Order", false, (xml) => string.IsNullOrEmpty(xml) ? null : (xml.StartsWith("Des", StringComparison.OrdinalIgnoreCase) ? "Descending" : "Ascending")),

            (ConfigType.Database, ConfigurationEntity.Cdc, "IncludeColumns", true, null),
            (ConfigType.Database, ConfigurationEntity.Cdc, "ExcludeColumns", true, null),
            (ConfigType.Database, ConfigurationEntity.Cdc, "AliasColumns", true, null),
            (ConfigType.Database, ConfigurationEntity.Cdc, "DataCtorParams", true, null),
            (ConfigType.Database, ConfigurationEntity.Cdc, "IdentifierMappingColumns", true, null),
            (ConfigType.Database, ConfigurationEntity.Cdc, "IncludeColumnsOnDelete", true, null),
            (ConfigType.Database, ConfigurationEntity.Cdc, "ExcludeColumnsFromETag", true, null),

            (ConfigType.Database, ConfigurationEntity.CdcJoin, "IncludeColumns", true, null),
            (ConfigType.Database, ConfigurationEntity.CdcJoin, "ExcludeColumns", true, null),
            (ConfigType.Database, ConfigurationEntity.CdcJoin, "AliasColumns", true, null),
            (ConfigType.Database, ConfigurationEntity.CdcJoin, "IdentifierMappingColumns", true, null),
            (ConfigType.Database, ConfigurationEntity.CdcJoin, "IncludeColumnsOnDelete", true, null),
            (ConfigType.Database, ConfigurationEntity.CdcJoin, "ExcludeColumnsFromETag", true, null)
        });

        private static string? NullValue() => (string?)null!;

        private static string? ConvertEventPublish(string? xml)
        {
            if (string.IsNullOrEmpty(xml))
                return null;

            if (string.Compare(xml, "true", StringComparison.InvariantCultureIgnoreCase) == 0)
                return "DataSvc";

            if (string.Compare(xml, "false", StringComparison.InvariantCultureIgnoreCase) == 0)
                return "None";

            return xml;
        }

        private static readonly List<(ConfigType ConvertType, ConfigurationEntity Entity, string XmlName, Type OverrideType, PropertySchemaAttribute Attribute)> _xmlSpecificPropertySchema = new List<(ConfigType, ConfigurationEntity, string, Type, PropertySchemaAttribute)>(new (ConfigType, ConfigurationEntity, string, Type, PropertySchemaAttribute)[]
        {
            (ConfigType.Entity, ConfigurationEntity.CodeGen, "WebApiAuthorize", typeof(string), new PropertySchemaAttribute("WebApi") 
                { 
                    Title = "The authorize attribute value to be used for the corresponding entity Web API controller; generally `Authorize` (or `true`), otherwise `AllowAnonymous` (or `false`).",
                    Description = "Defaults to `AllowAnonymous`. This can be overridden within the `Entity`(s) and/or their corresponding `Operation`(s)."
                }),
            (ConfigType.Entity, ConfigurationEntity.CodeGen, "EventPublish", typeof(string), new PropertySchemaAttribute("Events")
                {
                    Title = "The layer to add logic to publish an event for a `Create`, `Update` or `Delete` operation.", Options = new string[] { "None", "false", "DataSvc", "true", "Data" },
                    Description = "Defaults to `DataSvc` (`true`); unless the `EventOutbox` is not `None` where it will default to `Data`. Used to enable the sending of messages to the likes of EventHub, Service Broker, SignalR, etc. This can be overridden within the `Entity`(s)."
                }),

            (ConfigType.Entity, ConfigurationEntity.Entity, "ManagerCtorParams", typeof(string), new PropertySchemaAttribute("Manager")
                {
                    Title = "The comma seperated list of additional (non-inferred) Dependency Injection (DI) parameters for the generated `Manager` constructor.", IsImportant = true,
                    Description = "Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred. " +
                        "Where the `Type` matches an already inferred value it will be ignored."
                }),
            (ConfigType.Entity, ConfigurationEntity.Entity, "DataSvcCtorParams", typeof(string), new PropertySchemaAttribute("DataSvc")
                {
                    Title = "The comma seperated list of additional (non-inferred) Dependency Injection (DI) parameters for the generated `DataSvc` constructor.", IsImportant = true,
                    Description = "Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred. " +
                        "Where the `Type` matches an already inferred value it will be ignored."
                }),
            (ConfigType.Entity, ConfigurationEntity.Entity, "DataCtorParams", typeof(string), new PropertySchemaAttribute("Data")
                {
                    Title = "The comma seperated list of additional (non-inferred) Dependency Injection (DI) parameters for the generated `Data` constructor.", IsImportant = true,
                    Description = "Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred. " +
                        "Where the `Type` matches an already inferred value it will be ignored."
                }),
            (ConfigType.Entity, ConfigurationEntity.Entity, "WebApiCtorParams", typeof(string), new PropertySchemaAttribute("WebApi")
                {
                    Title = "The comma seperated list of additional (non-inferred) Dependency Injection (DI) parameters for the generated `WebApi` constructor.", IsImportant = true,
                    Description = "Each constructor parameter should be formatted as `Type` + `^` + `Name`; e.g. `IConfiguration^Config`. Where the `Name` portion is not specified it will be inferred. " +
                        "Where the `Type` matches an already inferred value it will be ignored."
                }),

            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeEntity", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `Entity` class (`Xxx.cs`)." }),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeAll", typeof(bool?), new PropertySchemaAttribute("ExcludeAll") 
            { 
                Title = "The option to exclude the generation of all `Operation` related artefacts; excluding the `Entity` class.",
                Description = "Is a shorthand means for setting all of the other `Exclude*` properties (with the exception of `ExcludeEntity`) to `true`."
            }),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeIData", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `IData` interface (`IXxxData.cs`)." }),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeData", typeof(bool?), new PropertySchemaAttribute("Exclude")
                {
                    Title = "Indicates whether to exclude the generation of the `Data` class (`XxxData.cs`).",
                    Description = "An unspecified (null) value indicates _not_ to exclude. A value of `true` indicates to exclude all output; alternatively, where `false` is specifically specified it indicates to at least output the corresponding `Mapper` class."
                }),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeIDataSvc", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `IDataSvc` interface (`IXxxDataSvc.cs`)." }),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeDataSvc", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `DataSvc` class (`IXxxDataSvc.cs`)." }),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeIManager", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `IManager` interface (`IXxxManager.cs`)." }),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeManager", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `Manager` class (`XxxManager.cs`)." }),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeWebApi", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `XxxController` class (`IXxxController.cs`)." }),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeWebApiAgent", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `XxxAgent` class (`XxxAgent.cs`)." }),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeGrpcAgent", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `XxxAgent` class (`XxxAgent.cs`)." }),
            (ConfigType.Entity, ConfigurationEntity.Entity, "WebApiAuthorize", typeof(string), new PropertySchemaAttribute("WebApi") 
                { 
                    Title = "The authorize attribute value to be used for the corresponding entity Web API controller; generally `Authorize` (or `true`), otherwise `AllowAnonymous` (or `false`).",
                    Description = "Defaults to the `CodeGeneration.WebApiAuthorize` configuration property (inherits) where not specified; can be overridden at the `Operation` level also."
                }),
            (ConfigType.Entity, ConfigurationEntity.Entity, "EventPublish", typeof(string), new PropertySchemaAttribute("Events")
                {
                    Title = "The layer to add logic to publish an event for a `Create`, `Update` or `Delete` operation.", Options = new string[] { "None", "false", "DataSvc", "true", "Data" },
                    Description = "Defaults to the `CodeGeneration.EventPublish` configuration property (inherits) where not specified. Used to enable the sending of messages to the likes of EventHub, Service Broker, SignalR, etc. This can be overridden within the `Entity`(s)."
                }),

            (ConfigType.Entity, ConfigurationEntity.Operation, "ExcludeAll", typeof(bool?), new PropertySchemaAttribute("ExcludeAll")
            {
                Title = "The option to exclude the generation of all `Operation` related output; excluding the `Entity` class.",
                Description = "Is a shorthand means for setting all of the other `Exclude*` properties (with the exception of `ExcludeEntity`) to `true`."
            }),
            (ConfigType.Entity, ConfigurationEntity.Operation, "ExcludeIData", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `IData` interface (`IXxxData.cs`) output." }),
            (ConfigType.Entity, ConfigurationEntity.Operation, "ExcludeData", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `Data` class (`XxxData.cs`) output." }),
            (ConfigType.Entity, ConfigurationEntity.Operation, "ExcludeIDataSvc", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `IDataSvc` interface (`IXxxDataSvc.cs`) output." }),
            (ConfigType.Entity, ConfigurationEntity.Operation, "ExcludeDataSvc", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `DataSvc` class (`IXxxDataSvc.cs`) output." }),
            (ConfigType.Entity, ConfigurationEntity.Operation, "ExcludeIManager", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `IManager` interface (`IXxxManager.cs`) output." }),
            (ConfigType.Entity, ConfigurationEntity.Operation, "ExcludeManager", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `Manager` class (`XxxManager.cs`) output." }),
            (ConfigType.Entity, ConfigurationEntity.Operation, "ExcludeWebApi", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `XxxController` class (`IXxxController.cs`) output." }),
            (ConfigType.Entity, ConfigurationEntity.Operation, "ExcludeWebApiAgent", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `XxxAgent` class (`XxxAgent.cs`) output." }),
            (ConfigType.Entity, ConfigurationEntity.Operation, "ExcludeGrpcAgent", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `XxxAgent` class (`XxxAgent.cs`) output." }),
            (ConfigType.Entity, ConfigurationEntity.Operation, "WebApiAuthorize", typeof(string), new PropertySchemaAttribute("WebApi")
                {
                    Title = "The authorize attribute value to be used for the corresponding entity Web API controller; generally `Authorize` (or `true`), otherwise `AllowAnonymous` (or `false`).",
                    Description = "Defaults to the `Entity.WebApiAuthorize` configuration property (inherits) where not specified."
                }),
            (ConfigType.Entity, ConfigurationEntity.Operation, "EventPublish", typeof(string), new PropertySchemaAttribute("Events")
                {
                    Title = "The layer to add logic to publish an event for a `Create`, `Update` or `Delete` operation.", Options = new string[] { "None", "false", "DataSvc", "true", "Data" },
                    Description = "Defaults to the `Entity.EventPublish` configuration property (inherits) where not specified. Used to enable the sending of messages to the likes of EventHub, Service Broker, SignalR, etc. This can be overridden within the `Entity`(s)."
                }),

            (ConfigType.Database, ConfigurationEntity.Table, "IncludeColumns", typeof(string), new PropertySchemaAttribute("Columns")
                {
                    Title = "The comma separated list of `Column` names to be included in the underlying generated output.", IsImportant = true,
                    Description = "Where not specified this indicates that all `Columns` are to be included."
                }),
            (ConfigType.Database, ConfigurationEntity.Table, "ExcludeColumns", typeof(string), new PropertySchemaAttribute("Columns")
                {
                    Title = "The comma seperated list of `Column` names to be excluded from the underlying generated output.", IsImportant = true,
                    Description = "Where not specified this indicates no `Columns` are to be excluded."
                }),
            (ConfigType.Database, ConfigurationEntity.Table, "GetAllOrderBy", typeof(string), new PropertySchemaAttribute("CodeGen")
                {
                    Title = "The comma seperated list of `Column` names (including sort order ASC/DESC) to be used as the `GetAll` query sort order."
                }),
            (ConfigType.Database, ConfigurationEntity.Table, "UdtExcludeColumns", typeof(string), new PropertySchemaAttribute("UDT")
                {
                    Title = "The comma seperated list of `Column` names to be excluded from the `User Defined Table (UDT)`.",
                    Description = "Where not specified this indicates that no `Columns` are to be excluded."
                }),

            (ConfigType.Database, ConfigurationEntity.StoredProcedure, "Type", typeof(string), new PropertySchemaAttribute("Key")
                {
                    Title = "The stored procedure operation type.",
                    Options = new string[] { "Get", "GetAll", "Create", "Update", "Upsert", "Delete", "Merge" },
                    Description = "Defaults to `GetAll`."
                }),
            (ConfigType.Database, ConfigurationEntity.StoredProcedure, "MergeOverrideIdentityColumns", typeof(string), new PropertySchemaAttribute("Merge")
                {
                    Title = "The comma seperated list of `Column` names to be used in the `Merge` statement to determine whether to _insert_, _update_ or _delete_.",
                    Description = "This is used to override the default behaviour of using the primary key column(s)."
                }),

            (ConfigType.Database, ConfigurationEntity.OrderBy, "Order", typeof(string), new PropertySchemaAttribute("Key")
                {
                    Title = "The corresponding sort order.",
                    Options = new string[] { "Asc", "Desc" },
                    Description = "Defaults to `Asc`."
                }),

            (ConfigType.Database, ConfigurationEntity.Query, "IncludeColumns", typeof(string), new PropertySchemaAttribute("Columns")
                {
                    Title = "The comma separated list of `Column` names to be included in the underlying generated output.", IsImportant = true,
                    Description = "Where not specified this indicates that all `Columns` are to be included."
                }),
            (ConfigType.Database, ConfigurationEntity.Query, "ExcludeColumns", typeof(string), new PropertySchemaAttribute("Columns")
                {
                    Title = "The comma seperated list of `Column` names to be excluded from the underlying generated output.", IsImportant = true,
                    Description = "Where not specified this indicates no `Columns` are to be excluded."
                }),
            (ConfigType.Database, ConfigurationEntity.Query, "AliasColumns", typeof(string), new PropertySchemaAttribute("Columns")
                {
                    Title = "The comma seperated list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column aliasing/renaming.", IsImportant = true,
                    Description = "Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`"
                }),

            (ConfigType.Database, ConfigurationEntity.QueryJoin, "IncludeColumns", typeof(string), new PropertySchemaAttribute("Columns")
                {
                    Title = "The comma separated list of `Column` names to be included in the underlying generated output.", IsImportant = true,
                    Description = "Where not specified this indicates that all `Columns` are to be included."
                }),
            (ConfigType.Database, ConfigurationEntity.QueryJoin, "ExcludeColumns", typeof(string), new PropertySchemaAttribute("Columns")
                {
                    Title = "The comma seperated list of `Column` names to be excluded from the underlying generated output.", IsImportant = true,
                    Description = "Where not specified this indicates no `Columns` are to be excluded."
                }),
            (ConfigType.Database, ConfigurationEntity.QueryJoin, "AliasColumns", typeof(string), new PropertySchemaAttribute("Columns")
                {
                    Title = "The comma seperated list of `Column` and `Alias` pairs (split by a `^` lookup character) to enable column aliasing/renaming.", IsImportant = true,
                    Description = "Each alias value should be formatted as `Column` + `^` + `Alias`; e.g. `PCODE^ProductCode`"
                })
        });

        /// <summary>
        /// Gets the YAML name from the XML name.
        /// </summary>
        internal static string GetYamlName(ConfigType convertType, ConfigurationEntity entity, string xmlName)
        {
            var item = _config.FirstOrDefault(x => x.ConvertType == convertType && x.Entity == entity && x.XmlName == xmlName);
            return item.YamlName ?? (StringConversion.ToCamelCase(xmlName)!);
        }

        /// <summary>
        /// Gets the XML name from the YAML name.
        /// </summary>
        internal static string GetXmlName(ConfigType convertType, ConfigurationEntity entity, string jsonName)
        {
            var item = _config.FirstOrDefault(x => x.ConvertType == convertType && x.Entity == entity && x.YamlName == jsonName);
            return item.XmlName ?? (StringConversion.ToPascalCase(jsonName)!);
        }

        /// <summary>
        /// Gets the YAML value from the XML value.
        /// </summary>
        internal static string? GetYamlValue(ConfigType convertType, ConfigurationEntity entity, string xmlName, string? xmlValue)
        {
            var item = _xmlToYamlConvert.FirstOrDefault(x => x.ConvertType == convertType && x.Entity == entity && x.XmlName == xmlName);
            var yaml = item.Converter == null ? xmlValue : item.Converter(xmlValue);
            return item.IsArray ? FormatYamlArray(yaml) : FormatYamlValue(yaml);
        }

        /// <summary>
        /// Check YAML for special characters and format accordingly.
        /// </summary>
        internal static string? FormatYamlValue(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            if (value.IndexOfAny(new char[] { ':', '{', '}', '[', ']', ',', '&', '*', '#', '?', '|', '-', '<', '>', '=', '!', '%', '@', '\\', '\"', '\'' }) >= 0)
                value = $"'{value.Replace("'", "''", StringComparison.InvariantCultureIgnoreCase)}'";

            if (string.Compare(value, "NULL", StringComparison.InvariantCultureIgnoreCase) == 0)
                value = $"'{value}'";

            return value;
        }

        /// <summary>
        /// Splits the string on a comma, and then formats each part and then bookends with square brackets.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        internal static string? FormatYamlArray(string? value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            var sb = new StringBuilder();
            foreach (var part in value.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                sb.Append(sb.Length == 0 ? "[ " : ", ");
                var yaml = FormatYamlValue(part);
                if (!string.IsNullOrEmpty(yaml))
                    sb.Append(FormatYamlValue(part));
            }

            sb.Append(" ]");
            return sb.ToString();
        }

        /// <summary>
        /// Gets the <see cref="PropertySchemaAttribute"/> for the specified XML name.
        /// </summary>
        internal static (Type Type, PropertySchemaAttribute Attribute) GetXmlPropertySchemaAttribute(ConfigType convertType, ConfigurationEntity entity, string xmlName)
        {
            var item = _xmlSpecificPropertySchema.FirstOrDefault(x => x.ConvertType == convertType && x.Entity == entity && x.XmlName == xmlName);
            return (item.OverrideType, item.Attribute);
        }
    }

    /// <summary>
    /// The code-generation configuration entity.
    /// </summary>
    public enum ConfigurationEntity
    {
        /// <summary>Not specified.</summary>
        None,
        /// <summary>Code-generation root configuration.</summary>
        CodeGen,
        /// <summary>Entity configuration.</summary>
        Entity,
        /// <summary>Const configuration.</summary>
        Const,
        /// <summary>Property configuration.</summary>
        Property,
        /// <summary>Operation configuration.</summary>
        Operation,
        /// <summary>Table configuration.</summary>
        Table,
        /// <summary>Query configuration.</summary>
        Query,
        /// <summary>Query Join configuration.</summary>
        QueryJoin,
        /// <summary>Query Join On configuration.</summary>
        QueryJoinOn,
        /// <summary>Query Order configuration.</summary>
        QueryOrder,
        /// <summary>Query Where configuration.</summary>
        QueryWhere,
        /// <summary>Cdc configuration.</summary>
        Cdc,
        /// <summary>Cdc Join configuration.</summary>
        CdcJoin,
        /// <summary>Cdc Join On configuration.</summary>
        CdcJoinOn,
        /// <summary>Stored procedure configuration.</summary>
        StoredProcedure,
        /// <summary>Parameter configuration.</summary>
        Parameter,
        /// <summary>OrderBy configuration.</summary>
        OrderBy,
        /// <summary>Where configuration.</summary>
        Where,
        /// <summary>Execute configuration.</summary>
        Execute
    }
}