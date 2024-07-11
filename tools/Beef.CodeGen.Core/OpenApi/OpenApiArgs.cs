using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Beef.CodeGen.OpenApi
{
    /// <summary>
    /// Represents the OpenAPI arguments that are used to control the resulting Beef YAML-generation.
    /// </summary>
    public class OpenApiArgs
    {
        /// <summary>
        /// Gets or sets the <see cref="StringComparer"/> to be used to determine the equality of the JSON property names to therefore include/exclude from the YAML generation.
        /// </summary>
        public StringComparer JsonNameComparer { get; set; } = StringComparer.OrdinalIgnoreCase;

        /// <summary>
        /// Indicates whether to reference the <see cref="OpenApiSchema"/> <see cref="OpenApiSchema.Enum"/> as reference data.
        /// </summary>
        /// <remarks>Where selected (default) then the underlying property data type will be denoted as reference data. Otherwise, the enums will be generated as the C# code equivalent (within the YAML).</remarks>
        public bool EnumAsReferenceData { get; set; } = false;

        /// <summary>
        /// Gets or sets the array of starting characters that indicate that the parameter should be ignored (skipped).
        /// </summary>
        /// <remarks>The parameter name is used for character comparison.</remarks>
        public char[] IgnoreParametersThatStartWith { get; set; } = ['$'];

        /// <summary>
        /// Gets or sets the type name suffix that is used where an underlying member name is the same.
        /// </summary>
        /// <remarks>Member names cannot be the same as their enclosing type: <see href="https://learn.microsoft.com/en-us/dotnet/csharp/misc/cs0542?f1url=%3FappId%3Droslyn%26k%3Dk(CS0542)"/>.</remarks>
        public string TypeWithSameMemberNameSuffix { get; set; } = "X";

        /// <summary>
        /// Gets or sets the list of types that should be ignored (skipped).
        /// </summary>
        /// <remarks>Extend (add to) using command line: <c>--param ignore=[name[,name..]]</c></remarks>
        public List<string> IgnoreTypes { get; set; } = [];

        /// <summary>
        /// Gets or sets the list of types that should be treated as reference data.
        /// </summary>
        /// <remarks>Extend (add to) using command line: <c>--param refdata=[name[,name..]]</c></remarks>
        public List<string> RefDataTypes { get; set; } = [];

        /// <summary>
        /// Indicates whether to output the OpenApi text where applicable.
        /// </summary>
        /// <remarks>Override using command line: <c>--param text=true|false</c></remarks>
        public bool OutputText { get; set; } = false;

        /// <summary>
        /// Get or sets the function to convert the OpenAPI name to the corresponding .NET name.
        /// </summary>
        public Func<OpenApiNameType, string, string>? ConvertDotNetName { get; set; }

        /// <summary>
        /// Gets or sets the action that is invoked after the <see cref="OpenApiDocument"/> has been converted to the corresponding <see cref="YamlConfig"/>. 
        /// </summary>
        public Action<YamlConfig, OpenApiDocument>? OnYamlConfig { get; set; }

        /// <summary>
        /// Gets or sets the action that is invoked after all the operations (see <see cref="OpenApiOperation"/>) have been converted and added to the primary <see cref="YamlEntity"/>.
        /// </summary>
        public Action<YamlEntity, OpenApiDocument>? OnYamlPrimaryEntity { get; set; }

        /// <summary>
        /// Gets or sets the action that is invoked after the <see cref="OpenApiOperation"/> has been converted to the corresponding <see cref="YamlOperation"/>.
        /// </summary>
        /// <remarks>Defaults to <see cref="OnYamlOperationAutoValidator"/>.</remarks>
        public Action<YamlOperation, OpenApiOperation>? OnYamlOperation { get; set; } = OnYamlOperationAutoValidator;

        /// <summary>
        /// Gets or sets the action that is invoked after the <see cref="OpenApiSchema"/> has been converted to the corresponding <see cref="YamlEntity"/>.
        /// </summary>
        /// <remarks>Defaults to <see cref="OnYamlEntityAutoSelectPrimaryKey"/>.</remarks>
        public Action<YamlEntity, OpenApiSchema>? OnYamlEntity { get; set; } = OnYamlEntityAutoSelectPrimaryKey;

        /// <summary>
        /// Gets or sets the action that is invoked after the <see cref="OpenApiSchema"/> has been converted to the corresponding <see cref="YamlProperty"/>.
        /// </summary>
        public Action<YamlProperty, OpenApiSchema>? OnYamlProperty { get; set; }

        /// <summary>
        /// Gets or sets the action that is invoked after the <see cref="OpenApiSchema"/> has been converted to the corresponding <see cref="YamlEnum"/>.
        /// </summary>
        public Action<YamlEnum, OpenApiSchema>? OnYamlEnum { get; set; }

        /// <summary>
        /// Gets or sets the action that is invoked after the <see cref="OpenApiSchema"/> has been converted to the corresponding <see cref="YamlEnumValue"/>.
        /// </summary>
        public Action<YamlEnumValue, OpenApiSchema>? OnYamlEnumValue { get; set; }

        /// <summary>
        /// Provides out-of-the-box <see cref="OnYamlEntity"/> logic to determine the primary key property.
        /// </summary>
        public static Action<YamlEntity, OpenApiSchema> OnYamlEntityAutoSelectPrimaryKey { get; } = (entity, _) =>
        {
            var first = entity.Properties.FirstOrDefault();
            if (first is not null && (first.Name!.Equals("Id", StringComparison.OrdinalIgnoreCase) || first.Name!.Equals(entity.Name! + "Id", StringComparison.OrdinalIgnoreCase)))
                first.AddAttribute("primaryKey", true);
        };

        /// <summary>
        /// Provides out-of-the-box <see cref="OnYamlEntity"/> logic to add a validator attribue.
        /// </summary>
        public static Action<YamlOperation, OpenApiOperation> OnYamlOperationAutoValidator { get; } = (operation, _) =>
        {
            var type = operation.Attributes["type"] as string;
            var valueType = operation.Attributes["valueType"] as string;
            if (type is not null && (type == "Create" || type == "Update"))
                operation.AddAttribute("validator", valueType + "Validator");
        };
    }
}