// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using CoreEx;
using CoreEx.Entities;
using OnRamp;
using OnRamp.Config;
using OnRamp.Utility;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Beef.CodeGen.Config.Entity
{
    /// <summary>
    /// Represents the <b>Parameter</b> code-generation configuration.
    /// </summary>
    [CodeGenClass("Parameter", Title = "'Parameter' object (entity-driven)",
        Description = "The `Parameter` object defines an `Operation` parameter and its charateristics.", 
        ExampleMarkdown = @"A YAML configuration [example](../samples/My.Hr/My.Hr.CodeGen/entity.beef.yaml) is as follows:
``` yaml
parameters: [
  { name: Id, property: Id, isMandatory: true, validatorCode: Common(EmployeeValidator.CanDelete) }
]
```")]
    [CodeGenCategory("Key", Title = "Provides the _key_ configuration.")]
    [CodeGenCategory("Property", Title = "Provides the _Property_ reference configuration.")]
    [CodeGenCategory("RefData", Title = "Provides the _Reference Data_ configuration.")]
    [CodeGenCategory("Manager", Title = "Provides the _Manager-layer_ configuration.")]
    [CodeGenCategory("Data", Title = "Provides the _data_ configuration.")]
    [CodeGenCategory("WebApi", Title = "Provides the _Web API_ configuration.")]
    [CodeGenCategory("gRPC", Title = "Provides the _gRPC_ configuration.")]
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
        [JsonPropertyName("name")]
        [CodeGenProperty("Key", Title = "The unique parameter name.", IsMandatory = true, IsImportant = true)]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the overriding text for use in comments.
        /// </summary>
        [JsonPropertyName("text")]
        [CodeGenProperty("Key", Title = "The overriding text for use in comments.",
            Description = "By default the `Text` will be the `Name` reformatted as sentence casing. To have the text used as-is prefix with a `+` plus-sign character.")]
        public string? Text { get; set; }

        /// <summary>
        /// Gets or sets the .NET <see cref="Type"/>.
        /// </summary>
        [JsonPropertyName("type")]
        [CodeGenProperty("Key", Title = "The .NET `Type`.", IsImportant = true,
            Description = "Defaults to `string`. To reference a Reference Data `Type` always prefix with `RefDataNamespace` (e.g. `RefDataNamespace.Gender`) or shortcut `^` (e.g. `^Gender`). This will ensure that the appropriate Reference Data " +
            "`using` statement is used. _Shortcut:_ Where the `Type` starts with (prefix) `RefDataNamespace.` or `^`, and the correspondong `RefDataType` attribute is not specified it will automatically default the `RefDataType` to `string.`")]
        public string? Type { get; set; }

        /// <summary>
        /// Indicates whether the .NET <see cref="Type"/> should be declared as nullable.
        /// </summary>
        [JsonPropertyName("nullable")]
        [CodeGenProperty("Key", Title = "Indicates whether the .NET Type should be declared as nullable; e.g. `int?`. Will be inferred where the `Type` is denoted as nullable; i.e. suffixed by a `?`. Where the .NET Type is not considered as an intrinsic type then will default to `true`.", IsImportant = true)]
        public bool? Nullable { get; set; }

        /// <summary>
        /// Gets or sets the C# code to default the value.
        /// </summary>
        [JsonPropertyName("default")]
        [CodeGenProperty("Key", Title = "The C# code to default the value.",
            Description = "Where the `Type` is `string` then the specified default value will need to be delimited. Any valid value assignment C# code can be used.")]
        public string? Default { get; set; }

        /// <summary>
        /// Gets or sets the overriding private name.
        /// </summary>
        [JsonPropertyName("privateName")]
        [CodeGenProperty("Key", Title = "The overriding private name.",
            Description = "Overrides the `Name` to be used for private fields. By default reformatted from `Name`; e.g. `FirstName` as `_firstName`.")]
        public string? PrivateName { get; set; }

        /// <summary>
        /// Gets or sets the overriding argument name.
        /// </summary>
        [JsonPropertyName("argumentName")]
        [CodeGenProperty("Key", Title = "The overriding argument name.",
            Description = "Overrides the `Name` to be used for argument parameters. By default reformatted from `Name`; e.g. `FirstName` as `firstName`.")]
        public string? ArgumentName { get; set; }

        #endregion

        #region Property

        /// <summary>
        /// Gets or sets the `Property.Name` within the parent `Entity` to copy (set) the configuration/characteristics from where not already defined.
        /// </summary>
        [JsonPropertyName("property")]
        [CodeGenProperty("Property", Title = "The `Property.Name` within the parent `Entity` to copy (set) the configuration/characteristics from where not already defined.")]
        public string? Property { get; set; }

        #endregion

        #region RefData

        /// <summary>
        /// Gets or sets the underlying Reference Data Type that is also used as the Reference Data serialization identifier (SID).
        /// </summary>
        [JsonPropertyName("refDataType")]
        [CodeGenProperty("RefData", Title = "The underlying Reference Data Type that is also used as the Reference Data serialization identifier (SID).", Options = ["string", "int", "Guid"],
            Description = "Defaults to `string` where not specified and the corresponding `Type` starts with (prefix) `RefDataNamespace.`.")]
        public string? RefDataType { get; set; }

        /// <summary>
        /// Indicates that the Reference Data property is to be a serializable list (ReferenceDataSidList). 
        /// </summary>
        [JsonPropertyName("refDataList")]
        [CodeGenProperty("RefData", Title = "Indicates that the Reference Data property is to be a serializable list (`ReferenceDataSidList`).",
            Description = "This is required to enable a list of Reference Data values (as per `RefDataType`) to be passed as an argument for example.")]
        public bool? RefDataList { get; set; }

        #endregion

        #region Manager

        /// <summary>
        /// Gets or sets the name of the .NET `Type` that will perform the validation.
        /// </summary>
        [JsonPropertyName("validator")]
        [CodeGenProperty("Manager", Title = "The name of the .NET `Type` that will perform the validation.", IsImportant = true)]
        public string? Validator { get; set; }

        /// <summary>
        /// Gets or sets the fluent-style method-chaining C# validator code to append to `IsMandatory` and `Validator` (where specified).
        /// </summary>
        [JsonPropertyName("validatorCode")]
        [CodeGenProperty("Manager", Title = "The fluent-style method-chaining C# validator code to append to `IsMandatory` and `Validator` (where specified).")]
        public string? ValidatorCode { get; set; }

        /// <summary>
        /// Gets or sets the `Validation` framework. 
        /// </summary>
        [JsonPropertyName("validationFramework")]
        [CodeGenProperty("Manager", Title = "The `Validation` framework to use for the entity-based validation.", Options = ["CoreEx", "FluentValidation"],
            Description = "Defaults to `Operation.ValidationFramework`.")]
        public string? ValidationFramework { get; set; }

        /// <summary>
        /// Indicates whether a <see cref="ValidationException"/> should be thrown when the parameter value has its default value (null, zero, etc).
        /// </summary>
        [JsonPropertyName("isMandatory")]
        [CodeGenProperty("Manager", Title = "Indicates whether a `ValidationException` should be thrown when the parameter value has its default value (null, zero, etc).")]
        public bool? IsMandatory { get; set; }

        /// <summary>
        /// Gets or sets the option that determines the layers in which the parameter is passed.
        /// </summary>
        [JsonPropertyName("layerPassing")]
        [CodeGenProperty("Manager", Title = "The option that determines the layers in which the parameter is passed.", Options = ["All", "ToManagerSet", "ToManagerCollSet"],
            Description = "Defaults to `All`. To further describe, `All` passes the parameter through all layeys, `ToManagerSet` only passes the parameter to the `Manager` layer and overrides the same named property within the corresponding `value` parameter, " +
            "`ToManagerCollSet` only passes the parameter to the `Manager` layer and overrides the same named property within the corresponding `value` collection parameter. " +
            "Where using the `PrimaryKey` option to automatically set `Parameters`, and the `Operation.Type` is `Create` or `Update` it will default to `ToManagerSet`.")]
        public string? LayerPassing { get; set; }

        #endregion

        #region Data

        /// <summary>
        /// Gets or sets the data `Converter` class name where specific data conversion is required.
        /// </summary>
        [JsonPropertyName("dataConverter")]
        [CodeGenProperty("Data", Title = "The data `Converter` class name where specific data conversion is required.",
            Description = "A `Converter` is used to convert a data source value to/from a .NET `Type` where no standard data conversion can be applied. Where this value is suffixed by `<T>` or `{T}` this will automatically set `Type`.")]
        public string? DataConverter { get; set; }

        #endregion

        #region WebApi

        /// <summary>
        /// Gets or sets the option for how the parameter will be delcared within the Web API Controller.
        /// </summary>
        [JsonPropertyName("webApiFrom")]
        [CodeGenProperty("WebApi", Title = "The option for how the parameter will be delcared within the Web API Controller.", Options = ["FromQuery", "FromBody", "FromRoute", "FromEntityProperties"],
            Description = "Defaults to `FromQuery`; unless the parameter `Type` has also been defined as an `Entity` within the code-gen config file then it will default to `FromEntityProperties`. Specifies that the parameter will be declared with corresponding `FromQueryAttribute`, `FromBodyAttribute` or `FromRouteAttribute` for the Web API method. The `FromEntityProperties` will declare all properties of the `Entity` as query parameters.")]
        public string? WebApiFrom { get; set; }

        /// <summary>
        /// Gets or sets the overriding text for use in comments.
        /// </summary>
        [JsonPropertyName("webApiText")]
        [CodeGenProperty("WebApi", Title = "The overriding text for use in the Web API comments.",
            Description = "By default the `WbeApiText` will be the `Name` reformatted as sentence casing. To have the text used as-is prefix with a `+` plus-sign character.")]
        public string? WebApiText { get; set; }

        #endregion

        #region Grpc

        /// <summary>
        /// Gets or sets the underlying gRPC data type.
        /// </summary>
        [JsonPropertyName("grpcType")]
        [CodeGenProperty("gRPC", Title = "The underlying gRPC data type; will be inferred where not specified.")]
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
        /// Indicates whether the parameter is the auto-enabled <c>QueryArgs</c>.
        /// </summary>
        public bool IsQueryArgs { get; set; }

        /// <summary>
        /// Gets the formatted summary text.
        /// </summary>
        public string? SummaryText => CodeGenConfig.GetSentenceText(CodeGenConfig.GetFormattedText(Text, text => IsValueArg && Parent!.Type == "Patch" 
            ? StringConverter.ToComments($"The {{{{string}}}} that contains the patch content for the {text}.")
            : StringConverter.ToComments($"{(Type == "bool" ? "Indicates whether" : "The")} {text}.")));

        /// <summary>
        /// Gets the formatted WebApi summary text.
        /// </summary>
        public string? WebApiSummaryText => CodeGenConfig.GetSentenceText(CodeGenConfig.GetFormattedText(WebApiText, text => IsValueArg && Parent!.Type == "Patch"
            ? StringConverter.ToComments($"The {{{{string}}}} that contains the patch content for the {text}.")
            : StringConverter.ToComments($"{(Type == "bool" ? "Indicates whether" : "The")} {text}.")));

        /// <summary>
        /// Gets the computed declared parameter type.
        /// </summary>
        public string? ParameterType => CompareValue(Nullable, true) ? $"{Type}?" : Type;

        /// <summary>
        /// Gets the WebApi parameter type.
        /// </summary>
        public string WebApiParameterType => IsValueArg && Parent!.Type == "Patch" ? "string" : string.IsNullOrEmpty(RefDataType) ? ParameterType! : (CompareValue(Nullable, true) ? $"{RefDataType}?" : RefDataType!);

        /// <summary>
        /// Gets the WebApi Agent parameter type.
        /// </summary>
        public string WebApiAgentParameterType => IsValueArg && Parent!.Type == "Patch" ? "string" : WebApiParameterType!;

        /// <summary>
        /// Gets the <see cref="WebApiFrom"/> for use in an Agent.
        /// </summary>
        public string? WebApiAgentFrom => WebApiFrom switch { "FromBody" => "FromBody", "FromEntityProperties" => "FromUriUseProperties", _ => null };

        /// <summary>
        /// Gets the parameter argument using the specified converter.
        /// </summary>
        public string ParameterConverted => string.IsNullOrEmpty(DataConverter) ? ArgumentName! : $"{DataConverter}.Default.ConvertToDestination({ArgumentName})";

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
        protected override Task PrepareAsync()
        {
            var pc = Property == null ? null : Parent!.Parent!.Properties?.FirstOrDefault(x => x.Name == Property);
            if (Property != null && pc == null)
                throw new CodeGenException(this, nameof(Property), $"Specified Property '{Property}' not found in Entity.");

            Type = DefaultWhereNull(Type, () => pc == null ? "string" : pc.Type);
            if (Type!.EndsWith("?", StringComparison.InvariantCulture))
            {
                Type = Type[0..^1];
                Nullable = true;
            }

            RelatedEntity = Root?.Entities!.FirstOrDefault(x => x.Name == Type);

            PrivateName = DefaultWhereNull(PrivateName, () => pc == null ? StringConverter.ToPrivateCase(Name) : pc.Name);
            ArgumentName = DefaultWhereNull(ArgumentName, () => pc == null ? StringConverter.ToCamelCase(Name) : pc.ArgumentName);
            Nullable = DefaultWhereNull(Nullable, () => pc == null ? !DotNet.IgnoreNullableTypes.Contains(Type!) : pc.Nullable);
            LayerPassing = DefaultWhereNull(LayerPassing, () => "All");
            RefDataList = DefaultWhereNull(RefDataList, () => pc?.RefDataList);
            DataConverter = DefaultWhereNull(DataConverter, () => pc?.DataConverter);
            DataConverter = PropertyConfig.ReformatDataConverter(DataConverter, Type, RefDataType, null).DataConverter;
            WebApiFrom = DefaultWhereNull(WebApiFrom, () => RelatedEntity == null ? "FromQuery" : "FromEntityProperties");
            ValidationFramework = DefaultWhereNull(ValidationFramework, () => Parent?.ValidationFramework);

            RefDataType = DefaultWhereNull(RefDataType, () => pc?.RefDataType);
            if (Type!.StartsWith("^"))
                Type = $"RefDataNamespace.{Type[1..]}";

            if (Type!.StartsWith("RefDataNamespace.", StringComparison.InvariantCulture))
                RefDataType = DefaultWhereNull(RefDataType, () => "string");

            if (string.IsNullOrEmpty(WebApiText))
                WebApiText = Text;

            Text = StringConverter.ToComments(DefaultWhereNull(Text, () =>
            {
                if (!string.IsNullOrEmpty(pc?.Text))
                    return pc.Text;

                if (Type!.StartsWith("RefDataNamespace.", StringComparison.InvariantCulture))
                    return $"{StringConverter.ToSentenceCase(Name)} (see {StringConverter.ToSeeComments(Type)})";

                if (RelatedEntity != null)
                    return $"{StringConverter.ToSentenceCase(Name)} (see {StringConverter.ToSeeComments("Entities." + Type)})";

                var relatedPropery = Parent!.Parent!.Properties?.FirstOrDefault(x => x.Name == Name);
                if (relatedPropery is not null)
                    return relatedPropery.Text;

                return StringConverter.ToSentenceCase(Name);
            }));

            WebApiText = DefaultWhereNull(WebApiText, () =>
            {
                if (string.IsNullOrEmpty(RefDataType) && !string.IsNullOrEmpty(pc?.Text))
                    return pc.Text;

                if (IsValueArg || IsPagingArgs || IsQueryArgs || Type == "ChangeLog")
                    return $"{StringConverter.ToSeeComments(Type)}";

                if (RelatedEntity != null)
                    return $"{StringConverter.ToSentenceCase(Name)} (see {StringConverter.ToSeeComments(Type)})";

                if (Name == "Id")
                    return "identifier";

                return StringConverter.ToSentenceCase(Name);
            });

            GrpcType = DefaultWhereNull(GrpcType, () => PropertyConfig.InferGrpcType(string.IsNullOrEmpty(RefDataType) ? Type! : RefDataType!, RefDataType, RefDataList));
            GrpcMapper = DotNet.SystemTypes.Contains(Type) || RefDataType != null ? null : Type;
            GrpcConverter = Type switch
            {
                "DateTime" => $"{(CompareValue(Nullable, true) ? "Nullable" : "")}DateTimeToTimestamp",
                "Guid" => $"{(CompareValue(Nullable, true) ? "Nullable" : "")}GuidToStringConverter",
                "decimal" => $"{(CompareValue(Nullable, true) ? "Nullable" : "")}DecimalToDecimalConverter",
                _ => null
            };

            CodeGenConfig.WarnWhereDeprecated(Root!, this, "dataConverterIsGeneric", "ivalidator");

            return Task.CompletedTask;
        }
    }
}