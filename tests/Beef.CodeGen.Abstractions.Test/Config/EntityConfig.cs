using Beef.CodeGen.Config;
using Newtonsoft.Json;
using System.Collections.Generic;

#nullable enable

namespace Beef.CodeGen.Abstractions.Test.Config
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [ClassSchema("Entity", Title = "'Entity' object.", Description = "The `Entity` object.", Markdown = "This is a _sample_ markdown.", ExampleMarkdown = "This is an `example` markdown.")]
    [CategorySchema("Key", Title = "Provides the _Key_ configuration.")]
    [CategorySchema("Collection", Title = "Provides related child (hierarchical) configuration.")]
    public class EntityConfig : ConfigRootBase<EntityConfig>
    {
        [JsonProperty("name")]
        [PropertySchema("Key", Title = "The entity name.", IsMandatory = true)]
        public string? Name { get; set; }

        [JsonProperty("properties")]
        [PropertyCollectionSchema("Collection", Title = "The `Property` collection.", IsImportant = true)]
        public List<PropertyConfig>? Properties { get; set; }

        protected override void Prepare()
        {
            Properties = PrepareCollection(Properties);
        }
    }
}

#nullable disable