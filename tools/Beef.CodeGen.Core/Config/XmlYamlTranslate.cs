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
    public static class XmlYamlTranslate
    {
        private static readonly List<(ConfigurationEntity Entity, string XmlName, string YamlName)> _config = new List<(ConfigurationEntity, string, string)>(new (ConfigurationEntity, string, string)[] 
        {
            // Entity oriented configuration.
            (ConfigurationEntity.CodeGen, "AppendToNamespace", "refDataAppendToNamespace"),
            (ConfigurationEntity.CodeGen, "MapperDefaultRefDataConverter", "refDataDefaultMapperConverter"),

            (ConfigurationEntity.Entity, "AutoInferImplements", "implementsAutoInfer"),
            (ConfigurationEntity.Entity, "EntityFrameworkEntity", "entityFrameworkModel"),
            (ConfigurationEntity.Entity, "CosmosEntity", "cosmosModel"),
            (ConfigurationEntity.Entity, "ODataEntity", "odataModel"),
            (ConfigurationEntity.Entity, "DataDatabaseMapperInheritsFrom", "databaseMapperInheritsFrom"),
            (ConfigurationEntity.Entity, "DataDatabaseCustomMapper", "databaseCustomMapper"),
            (ConfigurationEntity.Entity, "DataEntityFrameworkMapperInheritsFrom", "entityFrameworkMapperInheritsFrom"),
            (ConfigurationEntity.Entity, "DataEntityFrameworkCustomMapper", "entityFrameworkCustomMapper"),
            (ConfigurationEntity.Entity, "DataCosmosValueContainer", "cosmosValueContainer"),
            (ConfigurationEntity.Entity, "DataCosmosMapperInheritsFrom", "cosmosMapperInheritsFrom"),
            (ConfigurationEntity.Entity, "DataCosmosCustomMapper", "cosmosCustomMapper"),
            (ConfigurationEntity.Entity, "DataODataMapperInheritsFrom", "odataMapperInheritsFrom"),
            (ConfigurationEntity.Entity, "DataODataCustomMapper", "odataCustomMapper"),
 
            (ConfigurationEntity.Property, "IgnoreSerialization", "serializationIgnore"),
            (ConfigurationEntity.Property, "EmitDefaultValue", "serializationEmitDefault"),
            (ConfigurationEntity.Property, "IsDataConverterGeneric", "dataConverterIsGeneric"),
            (ConfigurationEntity.Property, "DataDatabaseMapper", "databaseMapper"),
            (ConfigurationEntity.Property, "DataDatabaseIgnore", "databaseIgnore"),
            (ConfigurationEntity.Property, "DataEntityFrameworkMapper", "entityFrameworkMapper"),
            (ConfigurationEntity.Property, "DataEntityFrameworkIgnore", "entityFrameworkIgnore"),
            (ConfigurationEntity.Property, "DataCosmosMapper", "cosmosMapper"),
            (ConfigurationEntity.Property, "DataCosmosIgnore", "cosmosIgnore"),
            (ConfigurationEntity.Property, "DataODataMapper", "odataMapper"),
            (ConfigurationEntity.Property, "DataODataIgnore", "odataIgnore"),

            (ConfigurationEntity.Operation, "OperationType", "type"),
            (ConfigurationEntity.Operation, "PagingArgs", "paging"),
            (ConfigurationEntity.Operation, "DataCosmosContainerId", "cosmosContainerId"),
            (ConfigurationEntity.Operation, "DataCosmosPartitionKey", "cosmosPartitionKey"),

            (ConfigurationEntity.Parameter, "IsDataConverterGeneric", "dataConverterIsGeneric"),
            (ConfigurationEntity.Parameter, "ValidatorFluent", "validatorCode"),

            // Database oriented configuration.
            (ConfigurationEntity.StoredProcedure, "OrderBy", "orderby"),

            (ConfigurationEntity.Parameter, "IsNullable", "nullable"),
            (ConfigurationEntity.Parameter, "IsCollection", "collection")
        });

        private static readonly List<(ConfigurationEntity Entity, string XmlName, Func<string?, string?> Converter)> _xmlToYamlConvert = new List<(ConfigurationEntity, string, Func<string?, string?>)>(new (ConfigurationEntity, string, Func<string?, string?>)[]
        {
            (ConfigurationEntity.CodeGen, "WebApiAuthorize", (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? "Authorize" : (xml == "false" ? "AllowAnonymous" : xml))),

            (ConfigurationEntity.Entity, "ExcludeEntity", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigurationEntity.Entity, "ExcludeAll", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigurationEntity.Entity, "ExcludeIData", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigurationEntity.Entity, "ExcludeData", (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? ConfigBase.YesOption : "RequiresMapper")),
            (ConfigurationEntity.Entity, "ExcludeIDataSvc", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigurationEntity.Entity, "ExcludeDataSvc", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigurationEntity.Entity, "ExcludeIManager", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigurationEntity.Entity, "ExcludeManager", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigurationEntity.Entity, "ExcludeWebApi", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigurationEntity.Entity, "ExcludeWebApiAgent", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigurationEntity.Entity, "ExcludeGrpcAgent", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigurationEntity.Entity, "WebApiAuthorize", (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? "Authorize" : (xml == "false" ? "AllowAnonymous" : xml))),

            (ConfigurationEntity.Operation, "ExcludeIData", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigurationEntity.Operation, "ExcludeData", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigurationEntity.Operation, "ExcludeIDataSvc", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigurationEntity.Operation, "ExcludeDataSvc", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigurationEntity.Operation, "ExcludeIManager", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigurationEntity.Operation, "ExcludeManager", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigurationEntity.Operation, "ExcludeWebApi", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigurationEntity.Operation, "ExcludeWebApiAgent", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigurationEntity.Operation, "ExcludeGrpcAgent", (xml) => ConvertBoolToYesNo(xml)),
            (ConfigurationEntity.Operation, "WebApiAuthorize", (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? "Authorize" : (xml == "false" ? "AllowAnonymous" : xml))),

            (ConfigurationEntity.Table, "IncludeColumns", (xml) => string.IsNullOrEmpty(xml) ? null : $"[ {xml} ]"),
            (ConfigurationEntity.Table, "ExcludeColumns", (xml) => string.IsNullOrEmpty(xml) ? null : $"[ {xml} ]"),
            (ConfigurationEntity.Table, "GetAllOrderBy", (xml) => string.IsNullOrEmpty(xml) ? null : XmlToGetAllOrderBy(xml)),
            (ConfigurationEntity.Table, "UdtExcludeColumns", (xml) => string.IsNullOrEmpty(xml) ? null : $"[ {xml} ]"),

            (ConfigurationEntity.StoredProcedure, "Type", (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "GetAll" ? "GetColl" : xml)),
            (ConfigurationEntity.StoredProcedure, "MergeOverrideIdentityColumns", (xml) => string.IsNullOrEmpty(xml) ? null : $"[ {xml} ]"),

            (ConfigurationEntity.OrderBy, "Order", (xml) => string.IsNullOrEmpty(xml) ? null : (xml.StartsWith("Des", StringComparison.OrdinalIgnoreCase) ? "Descending" : "Ascending"))
        });

        private static string? ConvertBoolToYesNo(string? xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? ConfigBase.YesOption : null);

        private static readonly List<(ConfigurationEntity Entity, string XmlName, Type OverrideType, PropertySchemaAttribute Attribute)> _xmlSpecificPropertySchema = new List<(ConfigurationEntity, string, Type, PropertySchemaAttribute)>(new (ConfigurationEntity, string, Type, PropertySchemaAttribute)[]
        {
            (ConfigurationEntity.CodeGen, "WebApiAuthorize", typeof(string), new PropertySchemaAttribute("WebApi") 
                { 
                    Title = "The authorize attribute value to be used for the corresponding entity Web API controller; generally `Authorize` (or `true`), otherwise `AllowAnonymous` (or `false`).",
                    Description = "Defaults to `AllowAnonymous`. This can be overidden within the `Entity`(s) and/or their corresponding `Operation`(s)."
                }),

            (ConfigurationEntity.Entity, "ExcludeEntity", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `Entity` class (`Xxx.cs`)." }),
            (ConfigurationEntity.Entity, "ExcludeAll", typeof(bool?), new PropertySchemaAttribute("ExcludeAll") 
            { 
                Title = "The option to exclude the generation of all `Operation` related artefacts; excluding the `Entity` class.",
                Description = "Is a shorthand means for setting all of the other `Exclude*` properties (with the exception of `ExcludeEntity`) to `true`."
            }),
            (ConfigurationEntity.Entity, "ExcludeIData", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `IData` interface (`IXxxData.cs`)." }),
            (ConfigurationEntity.Entity, "ExcludeData", typeof(bool?), new PropertySchemaAttribute("Exclude")
                {
                    Title = "Indicates whether to exclude the generation of the `Data` class (`XxxData.cs`).",
                    Description = "An unspecified (null) value indicates _not_ to exclude. A value of `true` indicates to exclude all output; alternatively, where `false` is specifically specified it indicates to at least output the corresponding `Mapper` class."
                }),
            (ConfigurationEntity.Entity, "ExcludeIDataSvc", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `IDataSvc` interface (`IXxxDataSvc.cs`)." }),
            (ConfigurationEntity.Entity, "ExcludeDataSvc", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `DataSvc` class (`IXxxDataSvc.cs`)." }),
            (ConfigurationEntity.Entity, "ExcludeIManager", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `IManager` interface (`IXxxManager.cs`)." }),
            (ConfigurationEntity.Entity, "ExcludeManager", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `Manager` class (`XxxManager.cs`)." }),
            (ConfigurationEntity.Entity, "ExcludeWebApi", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `XxxController` class (`IXxxController.cs`)." }),
            (ConfigurationEntity.Entity, "ExcludeWebApiAgent", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `XxxAgent` class (`XxxAgent.cs`)." }),
            (ConfigurationEntity.Entity, "ExcludeGrpcAgent", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `XxxAgent` class (`XxxAgent.cs`)." }),
            (ConfigurationEntity.Entity, "WebApiAuthorize", typeof(string), new PropertySchemaAttribute("WebApi") 
                { 
                    Title = "The authorize attribute value to be used for the corresponding entity Web API controller; generally `Authorize` (or `true`), otherwise `AllowAnonymous` (or `false`).",
                    Description = "Defaults to the `CodeGeneration.WebApiAuthorize` configuration property (inherits) where not specified; can be overridden at the `Operation` level also."
                }),

            (ConfigurationEntity.Operation, "ExcludeAll", typeof(bool?), new PropertySchemaAttribute("ExcludeAll")
            {
                Title = "The option to exclude the generation of all `Operation` related output; excluding the `Entity` class.",
                Description = "Is a shorthand means for setting all of the other `Exclude*` properties (with the exception of `ExcludeEntity`) to `true`."
            }),
            (ConfigurationEntity.Operation, "ExcludeIData", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `IData` interface (`IXxxData.cs`) output." }),
            (ConfigurationEntity.Operation, "ExcludeData", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `Data` class (`XxxData.cs`) output." }),
            (ConfigurationEntity.Operation, "ExcludeIDataSvc", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `IDataSvc` interface (`IXxxDataSvc.cs`) output." }),
            (ConfigurationEntity.Operation, "ExcludeDataSvc", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `DataSvc` class (`IXxxDataSvc.cs`) output." }),
            (ConfigurationEntity.Operation, "ExcludeIManager", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `IManager` interface (`IXxxManager.cs`) output." }),
            (ConfigurationEntity.Operation, "ExcludeManager", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `Manager` class (`XxxManager.cs`) output." }),
            (ConfigurationEntity.Operation, "ExcludeWebApi", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `XxxController` class (`IXxxController.cs`) output." }),
            (ConfigurationEntity.Operation, "ExcludeWebApiAgent", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `XxxAgent` class (`XxxAgent.cs`) output." }),
            (ConfigurationEntity.Operation, "ExcludeGrpcAgent", typeof(bool?), new PropertySchemaAttribute("Exclude") { Title = "Indicates whether to exclude the generation of the `XxxAgent` class (`XxxAgent.cs`) output." }),
            (ConfigurationEntity.Operation, "WebApiAuthorize", typeof(string), new PropertySchemaAttribute("WebApi")
                {
                    Title = "The authorize attribute value to be used for the corresponding entity Web API controller; generally `Authorize` (or `true`), otherwise `AllowAnonymous` (or `false`).",
                    Description = "Defaults to the `Entity.WebApiAuthorize` configuration property (inherits) where not specified."
                }),

            (ConfigurationEntity.Table, "IncludeColumns", typeof(string), new PropertySchemaAttribute("Key")
                {
                    Title = "The comma separated list of `Column` names to be included in the underlying generated output.", IsImportant = true,
                    Description = "Where not specified this Indicates whether all `Columns` are to be included."
                }),
            (ConfigurationEntity.Table, "ExcludeColumns", typeof(string), new PropertySchemaAttribute("Key")
                {
                    Title = "The comma seperated list of `Column` names to be excluded from the underlying generated output.", IsImportant = true,
                    Description = "Where not specified this indicates no `Columns` are to be excluded."
                }),
            (ConfigurationEntity.Table, "GetAllOrderBy", typeof(string), new PropertySchemaAttribute("Key")
                {
                    Title = "The comma seperated list of `Column` names (including sort order ASC/DESC) to be used as the `GetAll` query sort order"
                }),
            (ConfigurationEntity.Table, "UdtExcludeColumns", typeof(string), new PropertySchemaAttribute("Udt")
                {
                    Title = "The comma seperated list of `Column` names to be excluded from the `User Defined Table (UDT)`.",
                    Description = "Where not specified this indicates that no `Columns` are to be excluded."
                }),

            (ConfigurationEntity.StoredProcedure, "Type", typeof(string), new PropertySchemaAttribute("Key")
                {
                    Title = "The stored procedure operation type.",
                    Options = new string[] { "Get", "GetAll", "Create", "Update", "Upsert", "Delete", "Merge" },
                    Description = "Defaults to `GetAll`."
                }),
            (ConfigurationEntity.StoredProcedure, "MergeOverrideIdentityColumns", typeof(string), new PropertySchemaAttribute("Key")
                {
                    Title = "The comma seperated list of `Column` names to be used in the `Merge` statement to determine whether to _insert_, _update_ or _delete_.",
                    Description = "This is used to override the default behaviour of using the primary key column(s)."
                }),

            (ConfigurationEntity.OrderBy, "Order", typeof(string), new PropertySchemaAttribute("Key")
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
        public static string GetYamlName(ConfigurationEntity entity, string xmlName)
        {
            var item = _config.FirstOrDefault(x => x.Entity == entity && x.XmlName == xmlName);
            return item.YamlName ?? (StringConversion.ToCamelCase(xmlName)!);
        }

        /// <summary>
        /// Gets the XML name from the YAML name.
        /// </summary>
        public static string GetXmlName(ConfigurationEntity entity, string jsonName)
        {
            var item = _config.FirstOrDefault(x => x.Entity == entity && x.YamlName == jsonName);
            return item.XmlName ?? (StringConversion.ToPascalCase(jsonName)!);
        }

        /// <summary>
        /// Gets the YAML value from the XML value.
        /// </summary>
        public static string? GetYamlValue(ConfigurationEntity entity, string xmlName, string? xmlValue)
        {
            var item = _xmlToYamlConvert.FirstOrDefault(x => x.Entity == entity && x.XmlName == xmlName);
            return item.Converter == null ? xmlValue : item.Converter(xmlValue);
        }

        /// <summary>
        /// Gets the <see cref="PropertySchemaAttribute"/> for the specified XML name.
        /// </summary>
        public static (Type Type, PropertySchemaAttribute Attribute) GetXmlPropertySchemaAttribute(ConfigurationEntity entity, string xmlName)
        {
            var item = _xmlSpecificPropertySchema.FirstOrDefault(x => x.Entity == entity && x.XmlName == xmlName);
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