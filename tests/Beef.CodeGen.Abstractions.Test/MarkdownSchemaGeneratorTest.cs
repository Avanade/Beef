using Beef.CodeGen.Abstractions.Test.Config;
using Beef.CodeGen.Utility;
using NUnit.Framework;
using System.IO;

namespace Beef.CodeGen.Abstractions.Test
{
    [TestFixture]
    public class MarkdownSchemaGeneratorTest
    {
        [Test]
        public void Generate()
        {
            if (Directory.Exists("MSG"))
                Directory.Delete("MSG", true);

            Directory.CreateDirectory("MSG");
            MarkdownSchemaGenerator.Generate<EntityConfig>(directory: "MSG", addBreaksBetweenSections: true);

            var fn = Path.Combine("MSG", "Entity.md");
            Assert.IsTrue(File.Exists(fn));
            Assert.AreEqual(File.ReadAllText(Path.Combine("Expected", "Entity.md")), File.ReadAllText(fn));

            fn = Path.Combine("MSG", "Property.md");
            Assert.IsTrue(File.Exists(fn));
            Assert.AreEqual(File.ReadAllText(Path.Combine("Expected", "Property.md")), File.ReadAllText(fn));
        }
    }
}