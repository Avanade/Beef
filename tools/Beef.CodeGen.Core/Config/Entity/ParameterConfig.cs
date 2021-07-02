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
    [ClassSchema("Parameter", Title = "'Parameter' object (entity-driven)",
        Description = "The `Parameter` object defines an `Operation` parameter and its charateristics.", 
        ExampleMarkdown = @"A YAML configuration [example](../samples/My.Hr/My.Hr.CodeGen/entity.beef.yaml) is as follows:
``` yaml
parameters: [
  { name: Id, property: Id, isMandatory: true, validatorCode: Common(EmployeeValidator.CanDelete) }
]
```")]
    [CategorySchema("Key", Title = "Provides the _key_ configuration.")]
    [CategorySchema("Property", Title = "Provides the _Property_ reference configuration.")]
    [CategorySchema("RefData", Title = "Provides the _Reference Data_ configuration.")]
    [CategorySchema("Manager", Title = "Provides the _Manager-layer_ configuration.")]
    [CategorySchema("Data", Title = "Provides the _data_ configuration.")]
    [CategorySchema("WebApi", Title = "Provides the _Web API_ configuration.")]
    [CategorySchema("gRPC", Title = "Provides the _gRPC_ configuration.")]
    public class ParameterConfig : ConfigBase<CodeGenConfig, OperationConfig>
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <remarks><inheritdoc/></remarks>
        public override string? QualifiedKeyName => BuildQualifiedKeyName("Parameter", Name);

        #region Key

        /// <summary>
        /// Gets or sets the unique parameter name.
        /// </summary>
        [JsonProperty("name", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The unique parameter name.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the overriding text for use in comments.
        /// </summary>
        [JsonProperty("text", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The overriding text for use in comments.",
            Description = "By default the `Text` will be the `Name` reformatted as sentence casing.")]
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the .NET <see cref="Type"/>.
        /// </summary>
        [JsonProperty("type", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The .NET `Type`.", IsImportant = true,
            Description = "Defaults to `string`. To reference a Reference Data `Type` always prefix with `RefDataNamespace` (e.g. `RefDataNamespace.Gender`) or shortcut `^` (e.g. `^Gender`). This will ensure that the appropriate Reference Data " +
            "`using` statement is used. _Shortcut:_ Where the `Type` starts with (prefix) `RefDataNamespace.` or `^`, and the correspondong `RefDataType` attribute is not specified it will automatically default the `RefDataType` to `string.`")]
        public string? Type { get; set; }

        /// <summary>
        /// Indicates whether the .NET <see cref="Type"/> should be declared as nullable.
        /// </summary>
        [JsonProperty("nullable", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "Indicates whether the .NET `Type should be declared as nullable; e.g. `string?`. Will be inferred where the `Type` is denoted as nullable; i.e. suffixed by a `?`.", IsImportant = true)]
        public bool? Nullable { get; set; }

        /// <summary>
        /// Gets or sets the C# code to default the value.
        /// </summary>
        [JsonProperty("default", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Key", Title = "The C# code to default the value.",
            Description = "Where the `Type` is `string` then the specified default value will need to be delimited. Any valid value assignment C# code can be used.")]
        public string? Default { get; set; }

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

        #endregion

        #region Property

        /// <summary>
        /// Gets or sets the `Property.Name` within the parent `Entity` to copy (set) the configuration/characteristics from where not already defined.
        /// </summary>
        [JsonProperty("property", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Property", Title = "The `Property.Name` within the parent `Entity` to copy (set) the configuration/characteristics from where not already defined.")]
        public string? Property { get; set; }

        #endregion

        #region RefData

        /// <summary>
        /// Gets or sets the underlying Reference Data Type that is also used as the Reference Data serialization identifier (SID).
        /// </summary>
        [JsonProperty("refDataType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("RefData", Title = "The underlying Reference Data Type that is also used as the Reference Data serialization identifier (SID).", Options = new string[] { "string", "int", "Guid" },
            Description = "Defaults to `string` where not specified and the corresponding `Type` starts with (prefix) `RefDataNamespace.`.")]
        public string? RefDataType { get; set; }

        /// <summary>
        /// Indicates that the Reference Data property is to be a serializable list (ReferenceDataSidList). 
        /// </summary>
        [JsonProperty("refDataList", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("RefData", Title = "Indicates that the Reference Data property is to be a serializable list (`ReferenceDataSidList`).",
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
        /// Gets or sets the name of the .NET Interface that the `Validator` implements/inherits.
        /// </summary>
        [JsonProperty("iValidator", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("Manager", Title = "The name of the .NET Interface that the `Validator` implements/inherits.",
            Description = "Defaults to `IValidator<{Type}>` where the `{Type}` is `Type`.")]
        public string? IValidator { get; set; }

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
        [PropertySchema("WebApi", Title = "The option for how the parameter will be delcared within the Web API Controller.", Options = new string[] { "FromQuery", "FromBody", "FromRoute", "FromEntityProperties" },
            Description = "Defaults to `FromQuery`; unless the parameter `Type` has also been defined as an `Entity` within the code-gen config file then it will default to `FromEntityProperties`. Specifies that the parameter will be declared with corresponding `FromQueryAttribute`, `FromBodyAttribute` or `FromRouteAttribute` for the Web API method. The `FromEntityProperties` will declare all properties of the `Entity` as query parameters.")]
        public string? WebApiFrom { get; set; }

        #endregion

        #region Grpc

        /// <summary>
        /// Gets or sets the underlying gRPC data type.
        /// </summary>
        [JsonProperty("grpcType", DefaultValueHandling = DefaultValueHandling.Ignore)]
        [PropertySchema("gRPC", Title = "The underlying gRPC data type; will be inferred where not specified.")]
        public string? GrpcType { get; set; }

        #endregion

        /// <summary>
        /// Indicates whether the parameter is the auto-added value.
        /// </summary>
        public bool IsValueArg { get; set; }

        /// <summary>
        /// Indicates whether the parameter is the auto-enabled <see cref="PagingArgs"/>.
        /// </summary>
        public bool IsPagingArgs { get; set; } 

        /// <summary>
        /// Gets the formatted summary text.
        /// </summary>
        public string? SummaryText => IsValueArg && Parent!.Type == "Patch" 
            ? ToComments($"The {{{{JToken}}}} that contains the patch content for the {Text}.")
            : ToComments($"{(Type == "bool" ? "Indicates whether" : "The")} {Text}.");

        /// <summary>
        /// Gets the computed declared parameter type.
        /// </summary>
        public string? ParameterType => CompareValue(Nullable, true) ? $"{Type}?" : Type;

        /// <summary>
        /// Gets the WebApi parameter type.
        /// </summary>
        public string WebApiParameterType => IsValueArg && Parent!.Type == "Patch" ? "JToken" : string.IsNullOrEmpty(RefDataType) ? ParameterType! : (CompareValue(Nullable, true) ? $"{RefDataType}?" : RefDataType!);

        /// <summary>
        /// Gets the WebApi Agent parameter type.
        /// </summary>
        public string WebApiAgentParameterType => IsValueArg && Parent!.Type == "Patch" ? "JToken" : WebApiParameterType!;

        /// <summary>
        /// Gets the <see cref="WebApiFrom"/> for use in an Agent.
        /// </summary>
        public string? WebApiAgentFrom => WebApiFrom switch { "FromBody" => "FromBody", "FromEntityProperties" => "FromUriUseProperties", _ => null };

        /// <summary>
        /// Gets the parameter argument using the specified converter.
        /// </summary>
        public string ParameterConverted => string.IsNullOrEmpty(DataConverter) ? ArgumentName! : $"{DataConverter}{(CompareValue(DataConverterIsGeneric, true) ? $"<{ParameterType}>" : "")}.Default.ConvertToDest({ArgumentName})";

        /// <summary>
        /// Gets or sets the related entity.
        /// </summary>
        public EntityConfig? RelatedEntity { get; set; }

        /// <summary>
        /// Gets or sets the gRPC converter.
        /// </summary>
        public string? GrpcConverter { get; set; }

        /// <summary>
        /// Gets or sets the gRPC mapper.
        /// </summary>
        public string? GrpcMapper { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void Prepare()
        {
            CheckKeyHasValue(Name);
            CheckOptionsProperties();

            var pc = Property == null ? null : Parent!.Parent!.Properties.FirstOrDefault(x => x.Name == Property);
            if (Property != null && pc == null)
                throw new CodeGenException(this, nameof(Property), $"Specified Property '{Property}' not found in Entity.");

            Type = DefaultWhereNull(Type, () => pc == null ? "string" : pc.Type);
            if (Type!.EndsWith("?", StringComparison.InvariantCulture))
            {
                Type = Type[0..^1];
                Nullable = true;
            }

            RelatedEntity = Root!.Entities.FirstOrDefault(x => x.Name == Type);
            Text = ToComments(DefaultWhereNull(Text, () =>
            {
                if (Type!.StartsWith("RefDataNamespace.", StringComparison.InvariantCulture))
                    return $"{StringConversion.ToSentenceCase(Name)} (see {ToSeeComments(Type)})";

                if (RelatedEntity != null)
                    return $"{StringConversion.ToSentenceCase(Name)} (see {ToSeeComments("Entities." + Type)})";

                return StringConversion.ToSentenceCase(Name);
            }));

            PrivateName = DefaultWhereNull(PrivateName, () => pc == null ? StringConversion.ToPrivateCase(Name) : pc.Name);
            ArgumentName = DefaultWhereNull(ArgumentName, () => pc == null ? StringConversion.ToCamelCase(Name) : pc.ArgumentName);
            Nullable = DefaultWhereNull(Nullable, () => pc == null ? !IgnoreNullableTypes.Contains(Type!) : pc.Nullable);
            LayerPassing = DefaultWhereNull(LayerPassing, () => "All");
            RefDataList = DefaultWhereNull(RefDataList, () => pc?.RefDataList);
            IValidator = DefaultWhereNull(IValidator, () => Validator != null ? $"IValidator<{Type}>" : null);
            DataConverter = DefaultWhereNull(DataConverter, () => pc?.DataConverter);
            DataConverterIsGeneric = DefaultWhereNull(DataConverterIsGeneric, () => pc?.DataConverterIsGeneric);
            WebApiFrom = DefaultWhereNull(WebApiFrom, () => RelatedEntity == null ? "FromQuery" : "FromEntityProperties");

            RefDataType = DefaultWhereNull(RefDataType, () => pc?.RefDataType);
            if (Type!.StartsWith("^"))
                Type = $"RefDataNamespace.{Type[1..]}";

            if (Type!.StartsWith("RefDataNamespace.", StringComparison.InvariantCulture))
                RefDataType = DefaultWhereNull(RefDataType, () => "string");

            GrpcType = DefaultWhereNull(GrpcType, () => PropertyConfig.InferGrpcType(string.IsNullOrEmpty(RefDataType) ? Type! : RefDataType!, RefDataType, RefDataList));
            GrpcMapper = SystemTypes.Contains(Type) || RefDataType != null ? null : Type;
            GrpcConverter = Type switch
            {
                "DateTime" => $"{(CompareValue(Nullable, true) ? "Nullable" : "")}DateTimeToTimestamp",
                "Guid" => $"{(CompareValue(Nullable, true) ? "Nullable" : "")}GuidToStringConverter",
                "decimal" => $"{(CompareValue(Nullable, true) ? "Nullable" : "")}DecimalToDecimalConverter",
                _ => null
            };
        }
    }
}