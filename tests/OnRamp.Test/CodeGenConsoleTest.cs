using OnRamp.Console;
using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;

namespace OnRamp.Test
{
    [TestFixture]
    public class CodeGenConsoleTest
    {
        [Test]
        public async Task A100_HelpDefaultOptions()
        {
            var c = CodeGenConsole.Create<CodeGenConsoleTest>();
            var r = await c.RunAsync("--help");
            Assert.AreEqual(0, r);
        }

        [Test]
        public async Task A110_HelpSupportedOptions()
        {
            var c = CodeGenConsole.Create<CodeGenConsoleTest>(options: SupportedOptions.IsSimulation | SupportedOptions.ExpectNoChanges);
            var r = await c.RunAsync("--help");
            Assert.AreEqual(0, r);
        }

        [Test]
        public async Task A120_InvalidOptionScript()
        {
            var c = CodeGenConsole.Create<CodeGenConsoleTest>();
            var r = await c.RunAsync("-s");
            Assert.AreEqual(1, r);
        }

        [Test]
        public async Task A130_InvalidOptionConfig()
        {
            var c = CodeGenConsole.Create<CodeGenConsoleTest>();
            var r = await c.RunAsync("-c");
            Assert.AreEqual(1, r);
        }

        [Test]
        public async Task A140_InvalidOptionDirectory()
        {
            var c = CodeGenConsole.Create<CodeGenConsoleTest>();
            var r = await c.RunAsync("-d ../../Bad");
            Assert.AreEqual(1, r);
        }

        [Test]
        public async Task A150_InvalidOptionAssembly()
        {
            var c = CodeGenConsole.Create<CodeGenConsoleTest>();
            var r = await c.RunAsync("-a NotExists");
            Assert.AreEqual(1, r);
        }

        [Test]
        public async Task A160_InvalidOptionConnectionString()
        {
            var c = CodeGenConsole.Create<CodeGenConsoleTest>();
            var r = await c.RunAsync("-db");
            Assert.AreEqual(1, r);
        }

        [Test]
        public async Task A170_InvalidOptionScriptSupportedOptions()
        {
            var c = CodeGenConsole.Create<CodeGenConsoleTest>(options: SupportedOptions.IsSimulation | SupportedOptions.ExpectNoChanges);
            var r = await c.RunAsync("-c ValidEntity.yaml");
            Assert.AreEqual(1, r);
        }

        [Test]
        public async Task A200_CodeGenException()
        {
            var a = CodeGeneratorArgs.Create<CodeGeneratorTest>("ValidEntity.yaml");
            var c = CodeGenConsole.Create<CodeGenConsoleTest>(a);
            var r = await c.RunAsync();
            Assert.AreEqual(2, r);
        }

        [Test]
        public async Task B100_SuccessWithNoCmdLineArgs()
        {
            if (Directory.Exists("XB100"))
                Directory.Delete("XB100", true);

            var a = CodeGeneratorArgs.Create<CodeGeneratorTest>("ValidEntity.yaml", "Data/ValidEntity.yaml").AddParameter("Directory", "XB100").AddParameter("AppName", "Zzz");
            var c = CodeGenConsole.Create<CodeGenConsoleTest>(a);
            var r = await c.RunAsync();
            Assert.AreEqual(0, r);

            Assert.IsTrue(Directory.Exists("XB100"));
            Assert.AreEqual(4, Directory.GetFiles("XB100").Length);

            Assert.IsTrue(File.Exists("XB100/Person.txt"));
            Assert.AreEqual("Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary", File.ReadAllText("XB100/Person.txt"));

            Assert.IsTrue(File.Exists("XB100/Name.txt"));
            Assert.AreEqual("Name: Person.Name, Type: string", File.ReadAllText("XB100/Name.txt"));

            Assert.IsTrue(File.Exists("XB100/Age.txt"));
            Assert.AreEqual("Name: Person.Age, Type: int", File.ReadAllText("XB100/Age.txt"));

            Assert.IsTrue(File.Exists("XB100/Salary.txt"));
            Assert.AreEqual("Name: Person.Salary, Type: decimal?", File.ReadAllText("XB100/Salary.txt"));
        }

        [Test]
        public async Task B110_SuccessWithCmdLineArgs()
        {
            if (Directory.Exists("XB110"))
                Directory.Delete("XB110", true);

            var c = CodeGenConsole.Create<CodeGenConsoleTest>();
            var r = await c.RunAsync("-s ValidEntity.yaml -c Data/ValidEntity.yaml -p Directory=XB110 -p AppName=Zzz -a \"OnRamp.Test, Version=1.2.3.0, Culture=neutral, PublicKeyToken=null\"");
            Assert.AreEqual(0, r);

            Assert.IsTrue(Directory.Exists("XB110"));
            Assert.AreEqual(4, Directory.GetFiles("XB110").Length);

            Assert.IsTrue(File.Exists("XB110/Person.txt"));
            Assert.AreEqual("Name: Person, CompanyName: Xxx, AppName: Zzz, Properties: Name, Age, Salary", File.ReadAllText("XB110/Person.txt"));

            Assert.IsTrue(File.Exists("XB110/Name.txt"));
            Assert.AreEqual("Name: Person.Name, Type: string", File.ReadAllText("XB110/Name.txt"));

            Assert.IsTrue(File.Exists("XB110/Age.txt"));
            Assert.AreEqual("Name: Person.Age, Type: int", File.ReadAllText("XB110/Age.txt"));

            Assert.IsTrue(File.Exists("XB110/Salary.txt"));
            Assert.AreEqual("Name: Person.Salary, Type: decimal?", File.ReadAllText("XB110/Salary.txt"));
        }

        [Test]
        public async Task C100_ErrorExpectNoChanges()
        {
            if (Directory.Exists("XC100"))
                Directory.Delete("XC100", true);

            var c = CodeGenConsole.Create<CodeGenConsoleTest>();
            var r = await c.RunAsync("-s ValidEntity.yaml -c Data/ValidEntity.yaml -enc -p Directory=XC100 -p AppName=Zzz -a \"OnRamp.Test, Version=1.2.3.0, Culture=neutral, PublicKeyToken=null\"");
            Assert.AreEqual(3, r);
        }
    }
}