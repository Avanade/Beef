using Beef.CodeGen.Abstractions.Test.Config;
using Beef.CodeGen.Utility;
using NUnit.Framework;
using System.IO;

namespace Beef.CodeGen.Abstractions.Test
{
    [TestFixture]
    public class JsonSchemaGeneratorTest
    {
        [Test]
        public void Generate()
        {
            if (Directory.Exists("JSG"))
                Directory.Delete("JSG", true);

            Directory.CreateDirectory("JSG");
            var fn = Path.Combine("JSG", "schema.json");

            JsonSchemaGenerator.Generate<EntityConfig>(fn, "Entity Configuration");

            Assert.IsTrue(File.Exists(fn));
            Assert.AreEqual(File.ReadAllText(Path.Combine("Expected", "Schema.json")), File.ReadAllText(fn));
        }
    }
}