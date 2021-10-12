using Beef.CodeGen.Config;
using Newtonsoft.Json;
using System.Collections.Generic;

#nullable enable

namespace Beef.CodeGen.Abstractions.Test.Config
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    [CodeGenClass("Entity", Title = "'Entity' object.", Description = "The `Entity` object.", Markdown = "This is a _sample_ markdown.", ExampleMarkdown = "This is an `example` markdown.")]
    [CodeGenCategory("Key", Title = "Provides the _Key_ configuration.")]
    [CodeGenCategory("Collection", Title = "Provides related child (hierarchical) configuration.")]
    public class EntityConfig : ConfigRootBase<EntityConfig>
    {
        [JsonProperty("name")]
        [CodeGenProperty("Key", Title = "The entity name.", IsMandatory = true)]
        public string? Name { get; set; }

        [JsonProperty("properties")]
        [CodeGenPropertyCollection("Collection", Title = "The `Property` collection.", IsImportant = true)]
        public List<PropertyConfig>? Properties { get; set; }

        protected override void Prepare()
        {
            Properties = PrepareCollection(Properties);
        }
    }
}

#nullable disable