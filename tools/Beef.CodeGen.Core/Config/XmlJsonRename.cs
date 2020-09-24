// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace Beef.CodeGen.Config
{
    /// <summary>
    /// Represents a temporary capability to rename XML attribute names with a new JSON property name. For the most part the names will not change other than PascalCase versus camelCase.
    /// </summary>
    public static class XmlJsonRename
    {
        private static readonly List<(ConfigurationEntity Entity, string XmlName, string JsonName)> _config = new List<(ConfigurationEntity, string, string)>(new (ConfigurationEntity, string, string)[] 
        {
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
            (ConfigurationEntity.Parameter, "ValidatorFluent", "validatorCode")
        });

        private static readonly List<(ConfigurationEntity Entity, string XmlName, Func<string?, string?> Converter)> _xmlToJsonConvert = new List<(ConfigurationEntity, string, Func<string?, string?>)>(new (ConfigurationEntity, string, Func<string?, string?>)[]
        {
            (ConfigurationEntity.CodeGen, "WebApiAuthorize", (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? "Authorize" : (xml == "false" ? "Authorize" : xml))),
            (ConfigurationEntity.Entity, "ExcludeData", (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? "Yes" : "Mapper")),
            (ConfigurationEntity.Entity, "WebApiAuthorize", (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? "Authorize" : (xml == "false" ? "Authorize" : xml))),
            (ConfigurationEntity.Operation, "WebApiAuthorize", (xml) => string.IsNullOrEmpty(xml) ? null : (xml == "true" ? "Authorize" : (xml == "false" ? "Authorize" : xml)))
        });

        /// <summary>
        /// Gets the JSON name from the XML name.
        /// </summary>
        public static string GetJsonName(ConfigurationEntity entity, string xmlName)
        {
            var item = _config.FirstOrDefault(x => x.Entity == entity && x.XmlName == xmlName);
            return item.JsonName ?? (StringConversion.ToCamelCase(xmlName)!);
        }

        /// <summary>
        /// Gets the XML name from the JSON name.
        /// </summary>
        public static string GetXmlName(ConfigurationEntity entity, string jsonName)
        {
            var item = _config.FirstOrDefault(x => x.Entity == entity && x.JsonName == jsonName);
            return item.XmlName ?? (StringConversion.ToPascalCase(jsonName)!);
        }

        /// <summary>
        /// Gets the JSON value from the XML value.
        /// </summary>
        public static string? GetJsonValue(ConfigurationEntity entity, string xmlName, string? xmlValue)
        {
            var item = _xmlToJsonConvert.FirstOrDefault(x => x.Entity == entity && x.XmlName == xmlName);
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
        /// <summary>Parameter configuration.</summary>
        Parameter
    }
}