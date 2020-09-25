// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.Config.Database;
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
            (ConfigurationEntity.Entity, "ExcludeData", (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? "Yes" : "Mapper")),
            (ConfigurationEntity.Entity, "WebApiAuthorize", (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? "Authorize" : (xml == "false" ? "Authorize" : xml))),

            (ConfigurationEntity.Operation, "WebApiAuthorize", (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? "Authorize" : (xml == "false" ? "Authorize" : xml))),

            (ConfigurationEntity.Table, "IncludeColumns", (xml) => string.IsNullOrEmpty(xml) ? null : $"[ {xml} ]"),
            (ConfigurationEntity.Table, "ExcludeColumns", (xml) => string.IsNullOrEmpty(xml) ? null : $"[ {xml} ]"),
            (ConfigurationEntity.Table, "GetCollOrderBy", (xml) => string.IsNullOrEmpty(xml) ? null : $"[ {xml} ]"),
            (ConfigurationEntity.Table, "UdtExcludeColumns", (xml) => string.IsNullOrEmpty(xml) ? null : $"[ {xml} ]"),
            (ConfigurationEntity.Table, "GetAllOrderBy", (xml) => string.IsNullOrEmpty(xml) ? null : XmlToGetAllOrderBy(xml)),

            (ConfigurationEntity.StoredProcedure, "Type", (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "GetAll" ? "GetColl" : xml)),
            (ConfigurationEntity.StoredProcedure, "MergeOverrideIdentityColumns", (xml) => string.IsNullOrEmpty(xml) ? null : $"[ {xml} ]"),

            (ConfigurationEntity.OrderBy, "Order", (xml) => string.IsNullOrEmpty(xml) ? null : (xml.StartsWith("Des", StringComparison.OrdinalIgnoreCase) ? "Descending" : "Ascending"))
            (ConfigurationEntity.Entity, "WebApiAuthorize", (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? "Authorize" : (xml == "false" ? "AllowAnonymous" : xml))),
            (ConfigurationEntity.Operation, "WebApiAuthorize", (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? "Authorize" : (xml == "false" ? "AllowAnonymous" : xml)))
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