// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Converters;
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
            (ConfigType.Entity, ConfigurationEntity.Property, "DataEntityFrameworkMapper", "entityFrameworkMapper"),
            (ConfigType.Entity, ConfigurationEntity.Property, "DataEntityFrameworkIgnore", "entityFrameworkIgnore"),
            (ConfigType.Entity, ConfigurationEntity.Property, "DataCosmosMapper", "cosmosMapper"),
            (ConfigType.Entity, ConfigurationEntity.Property, "DataCosmosIgnore", "cosmosIgnore"),
            (ConfigType.Entity, ConfigurationEntity.Property, "DataODataMapper", "odataMapper"),
            (ConfigType.Entity, ConfigurationEntity.Property, "DataODataIgnore", "odataIgnore"),

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

        private static readonly List<(ConfigType ConvertType, ConfigurationEntity Entity, string XmlName, Func<string?, string?> Converter)> _xmlToYamlConvert = new List<(ConfigType, ConfigurationEntity, string, Func<string?, string?>)>(new (ConfigType, ConfigurationEntity, string, Func<string?, string?>)[]
        {
            (ConfigType.Entity, ConfigurationEntity.CodeGen, "WebApiAuthorize", (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? "Authorize" : (xml == "false" ? "AllowAnonymous" : xml))),

            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeEntity", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeAll", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeIData", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeData", (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? ConfigBase.YesOption : "RequiresMapper")),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeIDataSvc", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeDataSvc", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeIManager", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeManager", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeWebApi", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeWebApiAgent", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigType.Entity, ConfigurationEntity.Entity, "ExcludeGrpcAgent", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigType.Entity, ConfigurationEntity.Entity, "WebApiAuthorize", (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? "Authorize" : (xml == "false" ? "AllowAnonymous" : xml))),

            (ConfigType.Entity, ConfigurationEntity.Operation, "ExcludeIData", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigType.Entity, ConfigurationEntity.Operation, "ExcludeData", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigType.Entity, ConfigurationEntity.Operation, "ExcludeIDataSvc", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigType.Entity, ConfigurationEntity.Operation, "ExcludeDataSvc", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigType.Entity, ConfigurationEntity.Operation, "ExcludeIManager", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigType.Entity, ConfigurationEntity.Operation, "ExcludeManager", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigType.Entity, ConfigurationEntity.Operation, "ExcludeWebApi", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigType.Entity, ConfigurationEntity.Operation, "ExcludeWebApiAgent", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigType.Entity, ConfigurationEntity.Operation, "ExcludeGrpcAgent", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigType.Entity, ConfigurationEntity.Operation, "WebApiAuthorize", (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? "Authorize" : (xml == "false" ? "AllowAnonymous" : xml))),

            (ConfigType.Database, ConfigurationEntity.Table, "IncludeColumns", (xml) => string.IsNullOrEmpty(xml) ? null : $"[ {xml} ]"),
            (ConfigType.Database, ConfigurationEntity.Table, "ExcludeColumns", (xml) => string.IsNullOrEmpty(xml) ? null : $"[ {xml} ]"),
            (ConfigType.Database, ConfigurationEntity.Table, "GetAllOrderBy", (xml) => string.IsNullOrEmpty(xml) ? null : XmlToGetAllOrderBy(xml)),
            (ConfigType.Database, ConfigurationEntity.Table, "UdtExcludeColumns", (xml) => string.IsNullOrEmpty(xml) ? null : $"[ {xml} ]"),

            (ConfigType.Database, ConfigurationEntity.StoredProcedure, "Type", (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "GetAll" ? "GetColl" : xml)),
            (ConfigType.Database, ConfigurationEntity.StoredProcedure, "MergeOverrideIdentityColumns", (xml) => string.IsNullOrEmpty(xml) ? null : $"[ {xml} ]"),

            (ConfigType.Database, ConfigurationEntity.OrderBy, "Order", (xml) => string.IsNullOrEmpty(xml) ? null : (xml.StartsWith("Des", StringComparison.OrdinalIgnoreCase) ? "Descending" : "Ascending"))
        });

        private static string? ConvertBoolToYesNo(string? xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? ConfigBase.YesOption : null);

        private static readonly List<(ConfigType ConvertType, ConfigurationEntity Entity, string XmlName, Type OverrideType, PropertySchemaAttribute Attribute)> _xmlSpecificPropertySchema = new List<(ConfigType, ConfigurationEntity, string, Type, PropertySchemaAttribute)>(new (ConfigType, ConfigurationEntity, string, Type, PropertySchemaAttribute)[]
        {
            (ConfigType.Entity, ConfigurationEntity.CodeGen, "WebApiAuthorize", typeof(string), new PropertySchemaAttribute("WebApi") 
                { 
                    Title = "The authorize attribute value to be used for the corresponding entity Web API controller; generally `Authorize` (or `true`), otherwise `AllowAnonymous` (or `false`).",
                    Description = "Defaults to `AllowAnonymous`. This can be overidden within the `Entity`(s) and/or their corresponding `Operation`(s)."
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

            (ConfigType.Database, ConfigurationEntity.Table, "IncludeColumns", typeof(string), new PropertySchemaAttribute("Key")
                {
                    Title = "The comma separated list of `Column` names to be included in the underlying generated output.", IsImportant = true,
                    Description = "Where not specified this Indicates whether all `Columns` are to be included."
                }),
            (ConfigType.Database, ConfigurationEntity.Table, "ExcludeColumns", typeof(string), new PropertySchemaAttribute("Key")
                {
                    Title = "The comma seperated list of `Column` names to be excluded from the underlying generated output.", IsImportant = true,
                    Description = "Where not specified this indicates no `Columns` are to be excluded."
                }),
            (ConfigType.Database, ConfigurationEntity.Table, "GetAllOrderBy", typeof(string), new PropertySchemaAttribute("Key")
                {
                    Title = "The comma seperated list of `Column` names (including sort order ASC/DESC) to be used as the `GetAll` query sort order"
                }),
            (ConfigType.Database, ConfigurationEntity.Table, "UdtExcludeColumns", typeof(string), new PropertySchemaAttribute("Udt")
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
            (ConfigType.Database, ConfigurationEntity.StoredProcedure, "MergeOverrideIdentityColumns", typeof(string), new PropertySchemaAttribute("Key")
                {
                    Title = "The comma seperated list of `Column` names to be used in the `Merge` statement to determine whether to _insert_, _update_ or _delete_.",
                    Description = "This is used to override the default behaviour of using the primary key column(s)."
                }),

            (ConfigType.Database, ConfigurationEntity.OrderBy, "Order", typeof(string), new PropertySchemaAttribute("Key")
                {
                    Title = "The corresponding sort order.",
                    Options = new string[] { "Asc", "Desc" },
                    Description = "Defaults to `Asc`."
                })
        });

        /// <summary>
        /// Converts the GetAllOrderBy XML to YAML.
        /// </summary>
        private static string XmlToGetAllOrderBy(string xml)
        {
            var sb = new StringBuilder();

            foreach (var ob in xml.Split(',', StringSplitOptions.RemoveEmptyEntries))
            {
                if (sb.Length == 0)
                    sb.Append("[");
                else
                    sb.Append(",");

                var parts = ob.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                sb.Append($" {{ name: {parts[0]}");
                if (parts.Length > 1)
                    sb.Append($", order: {(parts[1].StartsWith("Des", StringComparison.OrdinalIgnoreCase) ? "Descending" : "Ascending")}");

                sb.Append(" }");
            }

            sb.Append(" ]");
            return sb.ToString();
        }

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
            return item.Converter == null ? xmlValue : item.Converter(xmlValue);
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