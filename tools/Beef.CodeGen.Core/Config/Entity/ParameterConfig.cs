// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.Entities;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace Beef.CodeGen.Config.Entity
{
    /// <summary>
    /// Represents the <b>Parameter</b> code-generation configuration.
    /// </summary>
    [ClassSchema("Parameter", Title = "The **Parameter** is used to define an Operation's Parameter and its charateristics.", Description = "", Markdown = "")]
    [CategorySchema("Key", Title = "Provides the **key** configuration.")]
    [CategorySchema("Parameter", Title = "Provides the **Parameter** configuration.")]
    [CategorySchema("RefData", Title = "Provides the **Reference Data** configuration.")]
    [CategorySchema("Manager", Title = "Provides the generic **Manager-layer** configuration.")]
    [CategorySchema("Data", Title = "Provides the generic **Data-layer** configuration.")]
    [CategorySchema("WebApi", Title = "Provides the data **Web API** configuration.")]
    public class ParameterConfig : ConfigBase<CodeGenConfig, OperationConfig>
    {
        #region Key

        /// <summary>
        /// Gets or sets the unique parameter name.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The unique parameter name.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the overridding text for use in comments.
        /// </summary>
        [JsonProperty("text", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The overriding text for use in comments.",
            Description = "By default the `Text` will be the `Name` reformatted as sentence casing.")]
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the overriding private name.
        /// </summary>
        [JsonProperty("privateName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The overriding private name.",
            Description = "Overrides the `Name` to be used for private fields. By default reformatted from `Name`; e.g. `FirstName` as `_firstName`.")]
        public string? PrivateName { get; set; }

        /// <summary>
        /// Gets or sets the overriding argument name.
        /// </summary>
        [JsonProperty("argumentName", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The overriding argument name.",
            Description = "Overrides the `Name` to be used for argument parameters. By default reformatted from `Name`; e.g. `FirstName` as `firstName`.")]
        public string? ArgumentName { get; set; }

        /// <summary>
        /// Gets or sets the `Property.Name` within the parent `Entity` to copy (set) the configuration/characteristics from where not already defined.
        /// </summary>
        [JsonProperty("property", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The `Property.Name` within the parent `Entity` to copy (set) the configuration/characteristics from where not already defined.")]
        public string? Property { get; set; }

        #endregion

        #region Parameter

        /// <summary>
        /// Gets or sets the .NET <see cref="Type"/>.
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Property", Title = "The .NET `Type`.", IsImportant = true,
            Description = "Defaults to `string`. To reference a Reference Data `Type` always prefix with `RefDataNamespace` (e.g. `RefDataNamespace.Gender`). This will ensure that the appropriate Reference Data " +
            "using statement is used. Shortcut: Where the `Type` starts with (prefix) `RefDataNamespace.` and the correspondong `RefDataType` attribute is not specified it will automatically default the `RefDataType` to `string.`")]
        public string? Type { get; set; }

        /// <summary>
        /// Indicates whether the .NET <see cref="Type"/> should be declared as nullable.
        /// </summary>
        [JsonProperty("nullable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Property", Title = "Indicates whether the .NET `Type should be declared as nullable; e.g. `string?`.", IsImportant = true)]
        public bool? Nullable { get; set; }

        /// <summary>
        /// Gets or sets the C# code to default the value.
        /// </summary>
        [JsonProperty("default", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Property", Title = "The C# code to default the value.",
            Description = "Where the `Type` is `string` then the specified default value will need to be delimited. Any valid value assignment C# code can be used.")]
        public string? Default { get; set; }

        #endregion

        #region RefData

        /// <summary>
        /// Gets or sets the underlying Reference Data Type that is also used as the Reference Data serialization identifier (SID).
        /// </summary>
        [JsonProperty("refDataType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Property", Title = "The underlying Reference Data Type that is also used as the Reference Data serialization identifier (SID).", Options = new string[] { "string", "int", "Guid" },
            Description = "Defaults to `string` where not specified and the corresponding `Type` starts with (prefix) `RefDataNamespace.`.")]
        public string? RefDataType { get; set; }

        /// <summary>
        /// Indicates that the Reference Data property is to be a serializable list (ReferenceDataSidList). 
        /// </summary>
        [JsonProperty("refDataList", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Property", Title = "Indicates that the Reference Data property is to be a serializable list (`ReferenceDataSidList`).",
            Description = "This is required to enable a list of Reference Data values (as per `RefDataType`) to be passed as an argument for example.")]
        public bool? RefDataList { get; set; }

        #endregion

        #region Manager

        /// <summary>
        /// Gets or sets the name of the .NET `Type` that will perform the validation.
        /// </summary>
        [JsonProperty("validator", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Manager", Title = "The name of the .NET `Type` that will perform the validation.", IsImportant = true)]
        public string? Validator { get; set; }

        /// <summary>
        /// Gets or sets the fluent-style method-chaining C# validator code to append to `IsMandatory` and `Validator` (where specified).
        /// </summary>
        [JsonProperty("validatorCode", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Manager", Title = "The fluent-style method-chaining C# validator code to append to `IsMandatory` and `Validator` (where specified).")]
        public string? ValidatorCode { get; set; }

        /// <summary>
        /// Indicates whether a <see cref="ValidationException"/> should be thrown when the parameter value has its default value (null, zero, etc).
        /// </summary>
        [JsonProperty("isMandatory", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Manager", Title = "Indicates whether a `ValidationException` should be thrown when the parameter value has its default value (null, zero, etc).")]
        public bool? IsMandatory { get; set; }

        /// <summary>
        /// Gets or sets the option that determines the layers in which the parameter is passed.
        /// </summary>
        [JsonProperty("layerPassing", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Manager", Title = "The option that determines the layers in which the parameter is passed.", Options = new string[] { "All", "ToManagerSet", "ToManagerCollSet" },
            Description = "Defaults to `All`. To further describe, `All` passes the parameter through all layeys, `ToManagerSet` only passes the parameter to the `Manager` layer and overrides the same named property within the corresponding `value` parameter, " +
            "`ToManagerCollSet` only passes the parameter to the `Manager` layer and overrides the same named property within the corresponding `value` collection parameter. " +
            "Where using the `UniqueKey` option to automatically set `Parameters`, and the `Operation.Type` is `Create` or `Update` it will default to `ToManagerSet`.")]
        public string? LayerPassing { get; set; }

        #endregion

        #region Data

        /// <summary>
        /// Gets or sets the data `Converter` class name where specific data conversion is required.
        /// </summary>
        [JsonProperty("dataConverter", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "The data `Converter` class name where specific data conversion is required.",
            Description = "A `Converter` is used to convert a data source value to/from a .NET `Type` where the standard data type conversion is not applicable.")]
        public string? DataConverter { get; set; }

        /// <summary>
        /// Indicates whether the data `Converter` is a generic class and will automatically use the corresponding property `Type` as the generic `T`.
        /// </summary>
        [JsonProperty("dataConverterIsGeneric", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "Indicates whether the data `Converter` is a generic class and will automatically use the corresponding property `Type` as the generic `T`.")]
        public bool? DataConverterIsGeneric { get; set; }

        #endregion

        #region WebApi


        /// <summary>
        /// Gets or sets the option for how the parameter will be delcared within the Web API Controller.
        /// </summary>
        [JsonProperty("webApiFrom", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Data", Title = "The option for how the parameter will be delcared within the Web API Controller.", Options = new string[] { "FromQuery", "FromBody", "FromRoute", "FromEntityProperties" },
            Description = "Defaults to `FromQuery`; unless the parameter `Type` has also been defined as an `Entity` within the code-gen config file then it will default to `FromEntityProperties`. Specifies that the parameter will be declared with corresponding `FromQueryAttribute`, `FromBodyAttribute` or `FromRouteAttribute` for the Web API method. The `FromEntityProperties` will declare all properties of the `Entity` as query parameters.")]
        public string? WebApiFrom { get; set; }

        #endregion

        /// <summary>
        /// Indicates whether the parameter is the auto-added value.
        /// </summary>
        public bool? IsValueArg { get; set; }

        /// <summary>
        /// Indicates whether the parameter is the auto-enabled <see cref="PagingArgs"/>.
        /// </summary>
        public bool? IsPagingArgs { get; set; } 

        /// <summary>
        /// Gets the formatted summary text.
        /// </summary>
        public string? SummaryText => CodeGenerator.ToComments($"{(Type == "bool" ? "Indicates whether" : "The")} {Text}.");

        /// <summary>
        /// Gets the computed declared parameter type.
        /// </summary>
        public string? ParameterType => CompareValue(Nullable, true) ? $"{Type}?" : Type;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            var pc = Property == null ? null : Parent!.Parent!.Properties.FirstOrDefault(x => x.Name == Name);

            Type = DefaultWhereNull(Type, () => pc == null ? "string" : pc.Type);
            Text = CodeGenerator.ToComments(DefaultWhereNull(Text, () =>
            {
                if (Type!.StartsWith("RefDataNamespace.", StringComparison.InvariantCulture))
                    return $"{StringConversion.ToSentenceCase(Name)} (see {CodeGenerator.ToSeeComments(Type)})";

                var ent = Root!.Entities.FirstOrDefault(x => x.Name == Type);
                if (ent != null)
                {
                    if (ent.EntityScope == null || ent.EntityScope == "Common")
                        return $"{StringConversion.ToSentenceCase(Name)} (see {CodeGenerator.ToSeeComments("Common.Entities." + Type)})";
                    else
                        return $"{StringConversion.ToSentenceCase(Name)} (see {CodeGenerator.ToSeeComments("Business.Entities." + Type)})";
                }

                return StringConversion.ToSentenceCase(Name);
            }));

            PrivateName = DefaultWhereNull(PrivateName, () => pc == null ? StringConversion.ToPrivateCase(Name) : pc.Name);
            ArgumentName = DefaultWhereNull(ArgumentName, () => pc == null ? StringConversion.ToCamelCase(Name) : pc.ArgumentName);
            Nullable = DefaultWhereNull(Nullable, () => pc == null ? !Beef.CodeGen.CodeGenConfig.IgnoreNullableTypes.Contains(Type!) : pc.Nullable);
            LayerPassing = DefaultWhereNull(LayerPassing, () => "All");
            RefDataList = DefaultWhereNull(RefDataList, () => pc?.RefDataList);
            DataConverter = DefaultWhereNull(DataConverter, () => pc?.DataConverter);
            DataConverterIsGeneric = DefaultWhereNull(DataConverterIsGeneric, () => pc?.DataConverterIsGeneric);

            RefDataType = DefaultWhereNull(RefDataType, () => pc?.RefDataType);
            if (Type!.StartsWith("RefDataNamespace.", StringComparison.InvariantCulture))
                RefDataType = DefaultWhereNull(RefDataType, () => "string");
        }
    }
}