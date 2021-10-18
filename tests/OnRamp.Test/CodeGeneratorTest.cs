using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace OnRamp.Test
{
    [TestFixture]
    public class CodeGeneratorTest
    {
        [Test]
        public void A100_Script_DoesNotExist()
        {
            var ex = Assert.Throws<CodeGenException>(() => new CodeGenerator(new CodeGeneratorArgs("DoesNotExist.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            Assert.AreEqual("Script 'DoesNotExist.yaml' does not exist.", ex.Message);
        }

        [Test]
        public void A110_Script_InvalidFileType()
        {
            var ex = Assert.Throws<CodeGenException>(() => new CodeGenerator(new CodeGeneratorArgs("InvalidFileType.xml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            Assert.AreEqual("Script 'InvalidFileType.xml' is invalid: Stream content type is not supported.", ex.Message);
        }

        [Test]
        public void A120_Script_InvalidYamlContent()
        {
            var ex = Assert.Throws<CodeGenException>(() => new CodeGenerator(new CodeGeneratorArgs("InvalidYamlContent.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            Assert.AreEqual("Script 'InvalidYamlContent.yaml' is invalid: Error converting value \"<blah></blah>\" to type 'OnRamp.Scripts.CodeGenScripts'. Path '', line 1, position 15.", ex.Message);
        }

        [Test]
        public void A130_Script_InvalidJsonContent()
        {
            var ex = Assert.Throws<CodeGenException>(() => new CodeGenerator(new CodeGeneratorArgs("InvalidJsonContent.json").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            Assert.AreEqual("Script 'InvalidJsonContent.json' is invalid: Unexpected character encountered while parsing value: <. Path '', line 0, position 0.", ex.Message);
        }

        [Test]
        public void A140_Script_InvalidEmpty()
        {
            var ex = Assert.Throws<CodeGenException>(() => new CodeGenerator(new CodeGeneratorArgs("InvalidEmpty.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            Assert.AreEqual("Script 'InvalidEmpty.yaml' is invalid: Stream is empty.", ex.Message);
        }

        [Test]
        public void A150_Script_InvalidEmptyConfigType()
        {
            var ex = Assert.Throws<CodeGenException>(() => new CodeGenerator(new CodeGeneratorArgs("InvalidEmptyConfigType.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            Assert.AreEqual("Script 'InvalidEmptyConfigType.yaml' is invalid: [ConfigType] Value is mandatory.", ex.Message);
        }

        [Test]
        public void A160_Script_InvalidConfigType()
        {
            var ex = Assert.Throws<CodeGenException>(() => new CodeGenerator(new CodeGeneratorArgs("InvalidConfigType.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            Assert.AreEqual("Script 'InvalidConfigType.yaml' is invalid: [ConfigType] Type 'OnRamp.Scripts.CodeGenScript' must inherit from ConfigRootBase<TRoot>.", ex.Message);
        }

        [Test]
        public void B100_Generator_DoesNotExist()
        {
            var ex = Assert.Throws<CodeGenException>(() => new CodeGenerator(new CodeGeneratorArgs("GeneratorDoesNotExist.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            Assert.AreEqual("Script 'GeneratorDoesNotExist.yaml' is invalid: [Generate.Type] Type 'OnRamp.Test.Generators.DoesNotExist, OnRamp.Test' does not exist.", ex.Message);
        }

        [Test]
        public void B110_Generator_DiffConfigType()
        {
            var ex = Assert.Throws<CodeGenException>(() => new CodeGenerator(new CodeGeneratorArgs("GeneratorDiffConfigType.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            Assert.AreEqual("Script 'GeneratorDiffConfigType.yaml' is invalid: [Generate.Type] Type 'OnRamp.Test.Generators.ScriptsGenerator, OnRamp.Test' RootType 'CodeGenScripts' must be the same as the ConfigType 'EntityConfig'.", ex.Message);
        }

        [Test]
        public void B120_Generator_NotInherits()
        {
            var ex = Assert.Throws<CodeGenException>(() => new CodeGenerator(new CodeGeneratorArgs("GeneratorNotInherits.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            Assert.AreEqual("Script 'GeneratorNotInherits.yaml' is invalid: [Generate.Type] Type 'OnRamp.Test.Generators.NotInheritsGenerator, OnRamp.Test' does not implement CodeGeneratorBase and/or have a default parameterless constructor.", ex.Message);
        }

        [Test]
        public void B130_Generator_TemplateDoesNotExist()
        {
            var ex = Assert.Throws<CodeGenException>(() => new CodeGenerator(new CodeGeneratorArgs("GeneratorTemplateDoesNotExist.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            Assert.AreEqual("Script 'GeneratorTemplateDoesNotExist.yaml' is invalid: [Generate.Template] Template 'DoesNotExist.hbs' does not exist.", ex.Message);
        }

        [Test]
        public void B140_Generator_RuntimeParams()
        {
            var cg = new CodeGenerator(new CodeGeneratorArgs("GeneratorRuntimeParams.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly));
            Assert.NotNull(cg);
            Assert.NotNull(cg.Scripts?.Generators);
            Assert.AreEqual(1, cg.Scripts.Generators.Count);
            Assert.AreEqual(2, cg.Scripts.Generators[0].RuntimeParameters.Count);
            Assert.IsTrue(cg.Scripts.Generators[0].RuntimeParameters.ContainsKey("Company"));
            Assert.AreEqual("Xxx", cg.Scripts.Generators[0].RuntimeParameters["Company"]);
            Assert.IsTrue(cg.Scripts.Generators[0].RuntimeParameters.ContainsKey("AppName"));
            Assert.AreEqual("Yyy", cg.Scripts.Generators[0].RuntimeParameters["AppName"]);
        }

        [Test]
        public void C100_Inherits_DiffConfigType()
        {
            var ex = Assert.Throws<CodeGenException>(() => new CodeGenerator(new CodeGeneratorArgs("InheritsDiffConfigType.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            Assert.AreEqual("Script 'InheritsDiffConfigType.yaml' is invalid: Script 'InheritsDiffConfigType2.yaml' is invalid: [ConfigType] Inherited ConfigType 'OnRamp.Test.Config.InheritAlternateConfigType, OnRamp.Test' must be the same as root ConfigType 'OnRamp.Scripts.CodeGenScripts'.", ex.Message);
        }

        [Test]
        public void C110_Inherits_SameConfigType()
        {
            var cg = new CodeGenerator(new CodeGeneratorArgs("InheritsSameConfigType.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly));
            Assert.NotNull(cg);
        }

        [Test]
        public void D100_Editor_TypeNotFound()
        {
            var ex = Assert.Throws<CodeGenException>(() => new CodeGenerator(new CodeGeneratorArgs("EditorTypeNotFound.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            Assert.AreEqual("Script 'EditorTypeNotFound.yaml' is invalid: [EditorType] Type 'OnRamp.Scripts.NotFound' does not exist.", ex.Message);
        }

        [Test]
        public void D110_Editor_InvalidType()
        {
            var ex = Assert.Throws<CodeGenException>(() => new CodeGenerator(new CodeGeneratorArgs("EditorInvalidType.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly)));
            Assert.AreEqual("Script 'EditorInvalidType.yaml' is invalid: [EditorType] Type 'OnRamp.Scripts.CodeGenScripts' does not implement IConfigEditor and/or have a default parameterless constructor.", ex.Message);
        }

        [Test]
        public void E100_Config_DoesNotExist()
        {
            var cg = new CodeGenerator(new CodeGeneratorArgs("ValidEntity.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly));
            var ex = Assert.Throws<CodeGenException>(() => cg.Generate("DoesNotExist.yaml"));
            Assert.AreEqual("Config 'DoesNotExist.yaml' does not exist.", ex.Message);
        }

        [Test]
        public void E110_Config_InvalidFileType()
        {
            var cg = new CodeGenerator(new CodeGeneratorArgs("ValidEntity.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly));
            var ex = Assert.Throws<CodeGenException>(() => cg.Generate("Data/InvalidFileType.xml"));
            Assert.AreEqual("Config 'Data/InvalidFileType.xml' is invalid: Stream content type is not supported.", ex.Message);
        }

        [Test]
        public void E120_Config_MandatoryValue()
        {
            var cg = new CodeGenerator(new CodeGeneratorArgs("ValidEntity.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly));
            var ex = Assert.Throws<CodeGenException>(() => cg.Generate("Data/MandatoryValue.yaml"));
            Assert.AreEqual("Config 'Data/MandatoryValue.yaml' is invalid: [Property.Name] Value is mandatory.", ex.Message);
        }

        [Test]
        public void E130_Config_InvalidOption()
        {
            var cg = new CodeGenerator(new CodeGeneratorArgs("ValidEntity.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly));
            var ex = Assert.Throws<CodeGenException>(() => cg.Generate("Data/InvalidOption.yaml"));
            Assert.AreEqual("Config 'Data/InvalidOption.yaml' is invalid: [Property(Name='Salary').Type] Value 'unknown' is invalid; valid values are: 'string', 'int', 'decimal'.", ex.Message);
        }

        [Test]
        public void F100_Generate_CreateAll()
        {
            if (Directory.Exists("F100"))
                Directory.Delete("F100", true);

            var cg = new CodeGenerator(new CodeGeneratorArgs("ValidEntity.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly).AddParameter("Directory", "F100").AddParameter("AppName", "Zzz"));
            var stats = cg.Generate("Data/ValidEntity.yaml");

            Assert.NotNull(stats);
            Assert.AreEqual(4, stats.CreatedCount);
            Assert.AreEqual(0, stats.UpdatedCount);
            Assert.AreEqual(0, stats.NotChangedCount);
            Assert.AreEqual(4, stats.LinesOfCodeCount);
            Assert.NotNull(stats.ElapsedMilliseconds);

            Assert.IsTrue(Directory.Exists("F100"));
            Assert.AreEqual(4, Directory.GetFiles("F100").Length);

            Assert.IsTrue(File.Exists("F100/Person.txt"));
            Assert.AreEqual("Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary", File.ReadAllText("F100/Person.txt"));

            Assert.IsTrue(File.Exists("F100/Name.txt"));
            Assert.AreEqual("Name: Person.Name, Type: string", File.ReadAllText("F100/Name.txt"));

            Assert.IsTrue(File.Exists("F100/Age.txt"));
            Assert.AreEqual("Name: Person.Age, Type: int", File.ReadAllText("F100/Age.txt"));

            Assert.IsTrue(File.Exists("F100/Salary.txt"));
            Assert.AreEqual("Name: Person.Salary, Type: decimal?", File.ReadAllText("F100/Salary.txt"));
        }

        [Test]
        public void F110_Generate_Mix()
        {
            if (Directory.Exists("F110"))
                Directory.Delete("F110", true);

            Directory.CreateDirectory("F110");
            File.WriteAllText("F110/Person.txt", "Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary");
            File.WriteAllText("F110/Name.txt", "Name: Person.Name, Type: xxx");

            var cg = new CodeGenerator(CodeGeneratorArgs.Create<CodeGeneratorTest>("ValidEntity.yaml").AddParameter("Directory", "F110").AddParameter("AppName", "Zzz"));
            var stats = cg.Generate("Data/ValidEntity.yaml");

            Assert.NotNull(stats);
            Assert.AreEqual(2, stats.CreatedCount);
            Assert.AreEqual(1, stats.UpdatedCount);
            Assert.AreEqual(1, stats.NotChangedCount);
            Assert.AreEqual(4, stats.LinesOfCodeCount);
            Assert.NotNull(stats.ElapsedMilliseconds);

            Assert.IsTrue(Directory.Exists("F110"));
            Assert.AreEqual(4, Directory.GetFiles("F110").Length);

            Assert.IsTrue(File.Exists("F110/Person.txt"));
            Assert.AreEqual("Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary", File.ReadAllText("F110/Person.txt"));

            Assert.IsTrue(File.Exists("F110/Name.txt"));
            Assert.AreEqual("Name: Person.Name, Type: string", File.ReadAllText("F110/Name.txt"));

            Assert.IsTrue(File.Exists("F110/Age.txt"));
            Assert.AreEqual("Name: Person.Age, Type: int", File.ReadAllText("F110/Age.txt"));

            Assert.IsTrue(File.Exists("F110/Salary.txt"));
            Assert.AreEqual("Name: Person.Salary, Type: decimal?", File.ReadAllText("F110/Salary.txt"));
        }

        [Test]
        public void F120_Generate_Simulation()
        {
            if (Directory.Exists("F120"))
                Directory.Delete("F120", true);

            Directory.CreateDirectory("F120");
            File.WriteAllText("F120/Person.txt", "Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary");
            File.WriteAllText("F120/Name.txt", "Name: Person.Name, Type: xxx");

            var cg = new CodeGenerator(new CodeGeneratorArgs("ValidEntity.yaml") { IsSimulation = true }.AddAssembly(typeof(CodeGeneratorTest).Assembly).AddParameter("Directory", "F120").AddParameter("AppName", "Zzz"));
            var stats = cg.Generate("Data/ValidEntity.yaml");

            Assert.NotNull(stats);
            Assert.AreEqual(2, stats.CreatedCount);
            Assert.AreEqual(1, stats.UpdatedCount);
            Assert.AreEqual(1, stats.NotChangedCount);
            Assert.AreEqual(4, stats.LinesOfCodeCount);
            Assert.NotNull(stats.ElapsedMilliseconds);

            Assert.IsTrue(Directory.Exists("F120"));
            Assert.AreEqual(2, Directory.GetFiles("F120").Length);

            Assert.IsTrue(File.Exists("F120/Person.txt"));
            Assert.AreEqual("Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary", File.ReadAllText("F120/Person.txt"));

            Assert.IsTrue(File.Exists("F120/Name.txt"));
            Assert.AreEqual("Name: Person.Name, Type: xxx", File.ReadAllText("F120/Name.txt"));

            Assert.IsFalse(File.Exists("F120/Age.txt"));
            Assert.IsFalse(File.Exists("F120/Salary.txt"));
        }

        [Test]
        public void F130_Generate_WithConfigEditor()
        {
            if (Directory.Exists("F130"))
                Directory.Delete("F130", true);

            var cg = new CodeGenerator(new CodeGeneratorArgs("ValidEntityWithConfigEditor.yaml").AddAssembly(typeof(CodeGeneratorTest).Assembly).AddParameter("Directory", "F130"));
            var stats = cg.Generate("Data/ValidEntity.yaml");

            Assert.NotNull(stats);
            Assert.AreEqual(1, stats.CreatedCount);
            Assert.AreEqual(0, stats.UpdatedCount);
            Assert.AreEqual(0, stats.NotChangedCount);
            Assert.AreEqual(1, stats.LinesOfCodeCount);
            Assert.NotNull(stats.ElapsedMilliseconds);

            Assert.IsTrue(Directory.Exists("F130"));
            Assert.AreEqual(1, Directory.GetFiles("F130").Length);

            Assert.IsTrue(File.Exists("F130/PERSON.txt"));
            Assert.AreEqual("Name: PERSON, CompanyName: Xxx, AppName: Yyy, Properties: Name, Age, Salary", File.ReadAllText("F130/PERSON.txt"));
        }

        [Test]
        public void F140_Generate_ExpectNoChanges_Error()
        {
            if (Directory.Exists("F140"))
                Directory.Delete("F140", true);

            var cg = new CodeGenerator(new CodeGeneratorArgs("ValidEntity.yaml") { ExpectNoChanges = true }.AddAssembly(typeof(CodeGeneratorTest).Assembly).AddParameter("Directory", "F140"));
            var ex = Assert.Throws<CodeGenChangesFoundException>(() => cg.Generate("Data/ValidEntity.yaml"));
            Assert.IsTrue(ex.Message.EndsWith("F140\\Person.txt' would be created as a result of the code generation."));
        }

        [Test]
        public void F150_Generate_ExpectNoChanges_Success()
        {
            if (Directory.Exists("F150"))
                Directory.Delete("F150", true);

            Directory.CreateDirectory("F150");
            File.WriteAllText("F150/Person.txt", "Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary");
            File.WriteAllText("F150/Name.txt", "Name: Person.Name, Type: string");
            File.WriteAllText("F150/Age.txt", "Name: Person.Age, Type: int");
            File.WriteAllText("F150/Salary.txt", "Name: Person.Salary, Type: decimal?");

            var cg = new CodeGenerator(new CodeGeneratorArgs("ValidEntity.yaml") { ExpectNoChanges = true }.AddAssembly(typeof(CodeGeneratorTest).Assembly).AddParameter("Directory", "F150").AddParameter("AppName", "Zzz"));
            var stats = cg.Generate("Data/ValidEntity.yaml");

            Assert.NotNull(stats);
            Assert.AreEqual(0, stats.CreatedCount);
            Assert.AreEqual(0, stats.UpdatedCount);
            Assert.AreEqual(4, stats.NotChangedCount);
            Assert.AreEqual(4, stats.LinesOfCodeCount);
            Assert.NotNull(stats.ElapsedMilliseconds);

            Assert.IsTrue(File.Exists("F150/Person.txt"));
            Assert.AreEqual("Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary", File.ReadAllText("F150/Person.txt"));

            Assert.IsTrue(File.Exists("F150/Name.txt"));
            Assert.AreEqual("Name: Person.Name, Type: string", File.ReadAllText("F150/Name.txt"));

            Assert.IsTrue(File.Exists("F150/Age.txt"));
            Assert.AreEqual("Name: Person.Age, Type: int", File.ReadAllText("F150/Age.txt"));

            Assert.IsTrue(File.Exists("F150/Salary.txt"));
            Assert.AreEqual("Name: Person.Salary, Type: decimal?", File.ReadAllText("F150/Salary.txt"));
        }
    }
}