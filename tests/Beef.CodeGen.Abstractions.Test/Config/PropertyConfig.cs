using Beef.CodeGen.Config;
using Newtonsoft.Json;

#nullable enable

namespace Beef.CodeGen.Abstractions.Test.Config
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [CodeGenClass("Property", Title = "'Property' object.", Description = "The `Property` object.")]
    [CodeGenCategory("Key", Title = "Provides the _Key_ configuration.")]
    public class PropertyConfig : ConfigBase<EntityConfig, EntityConfig>
    {
        public override string QualifiedKeyName => BuildQualifiedKeyName("Property", Name);

        [JsonProperty("name")]
        [CodeGenProperty("Key", Title = "The property name.", IsMandatory = true)]
        public string? Name { get; set; }

        [JsonProperty("type")]
        [CodeGenProperty("Key", Title = "The property type.", Description = "This is a more detailed description for the property type.", IsImportant = true, Options = new string[] { "string", "int", "decimal" })]
        public string? Type { get; set; }

        [JsonProperty("isNullable")]
        [CodeGenProperty("Key", Title = "Indicates whether the property is nullable.")]
        public bool? IsNullable { get; set; }

        protected override void Prepare()
        {
            Type = DefaultWhereNull(Type, () => "string");
        }
    }
}

#nullable disable