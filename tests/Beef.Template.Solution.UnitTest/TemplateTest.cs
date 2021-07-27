using NUnit.Framework;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Beef.Template.Solution.UnitTest
{
    [TestFixture]
    public class TemplateTest
    {
        private static bool _firstTime = true;
        private static DirectoryInfo _rootDir;
        private static DirectoryInfo _unitTests;

        /// <summary>
        /// Executes the specified command.
        /// </summary>
        private static (int exitCode, string stdOut) ExecuteCommand(string filename, string arguments = null, string workingDirectory = null)
        {
            TestContext.WriteLine("**********************************************************************");
            TestContext.WriteLine($"dir> {workingDirectory ?? _rootDir.FullName}");
            TestContext.WriteLine($"cmd> {filename} {arguments}");
            TestContext.WriteLine("");

            var process = new Process();
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.WorkingDirectory = workingDirectory ?? _rootDir.FullName;
            process.StartInfo.FileName = filename;
            process.StartInfo.Arguments = arguments;
            process.Start();

            var reader = process.StandardOutput;
            var sb = new StringBuilder();
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                TestContext.WriteLine(line);
                sb.Append(line);
            }

            reader = process.StandardError;
            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                TestContext.Error.WriteLine(line);
            }

            process.WaitForExit();
            TestContext.WriteLine($"res> {process.ExitCode}");
            TestContext.WriteLine("**********************************************************************");

            return (process.ExitCode, sb.ToString());
        }

        public void OneTimeSetUp()
        {
            if (!_firstTime)
                return;

            _firstTime = false;

            // Determine directories.
            _rootDir = new DirectoryInfo(TestContext.CurrentContext.WorkDirectory);
            while (_rootDir.Name != "Beef")
            {
                _rootDir = _rootDir.Parent;
            }

            _unitTests = new DirectoryInfo(Path.Combine(_rootDir.FullName, ".unittests"));

            // Remove previous tests and create fresh.
            if (_unitTests.Exists)
                _unitTests.Delete(true);

            _unitTests.Create();

            // Remove existing cached Beef nuget packages.
            var (exitCode, stdOut) = ExecuteCommand("dotnet", "nuget locals global-packages --list");
            Assert.GreaterOrEqual(0, exitCode);

            var nugets = new DirectoryInfo(stdOut.Replace("info : global-packages: ", string.Empty).Replace("global-packages: ", string.Empty));
            Assert.IsTrue(nugets.Exists);
            foreach (var di in nugets.EnumerateDirectories().Where(x => x.Name.StartsWith("beef.")))
            {
                di.Delete(true);
            }

            // Build Beef and package (nuget) - only local package, no deployment.
            Assert.GreaterOrEqual(0, ExecuteCommand("powershell", $"{Path.Combine(_rootDir.FullName, "nuget-publish.ps1")} packageonly").exitCode, "nuget publish");

            // Install the Beef template solution from local package.
            // dotnet new -i beef.template.solution --nuget-source https://api.nuget.org/v3/index.json
            Assert.GreaterOrEqual(0, ExecuteCommand("dotnet", $"new -i beef.template.solution --nuget-source {Path.Combine(_rootDir.FullName, "packageonly")}").exitCode, "install beef.template.solution");
        }

        [Test]
        public void Database()
        {
            OneTimeSetUp();
            SolutionCreateGenerateTest("Foo.Db", "Bar", "Database");
        }

        [Test]
        public void EntityFramework()
        {
            OneTimeSetUp();
            SolutionCreateGenerateTest("Foo.Ef", "Bar", "EntityFramework");
        }

        [Test]
        public void CosmosDb()
        {
            OneTimeSetUp();
            SolutionCreateGenerateTest("Foo.Co", "Bar", "Cosmos");
        }

        private static void SolutionCreateGenerateTest(string company, string appName, string datasource)
        {
            // Mkdir and create solution from template. 
            var dir = Path.Combine(_unitTests.FullName, $"{company}.{appName}");
            Directory.CreateDirectory(dir);
            Assert.Zero(ExecuteCommand("dotnet", $"new beef --company {company} --appname {appName} --datasource {datasource}", dir).exitCode, "dotnet new beef");

            // Restore nuget packages from our repository.
            Assert.Zero(ExecuteCommand("dotnet", $"restore -s {Path.Combine(_rootDir.FullName, "packageonly")}", dir).exitCode, "dotnet restore");

            // CodeGen: Execute code-generation.
            Assert.Zero(ExecuteCommand("dotnet", "run all", Path.Combine(dir, $"{company}.{appName}.CodeGen")).exitCode, "dotnet run all [entity]");

            // Database: Execute code-generation.
            if (datasource == "Database" || datasource == "EntityFramework")
            {
                Assert.Zero(ExecuteCommand("dotnet", "run drop", Path.Combine(dir, $"{company}.{appName}.Database")).exitCode, "dotnet run drop [database]");
                Assert.Zero(ExecuteCommand("dotnet", "run all", Path.Combine(dir, $"{company}.{appName}.Database")).exitCode, "dotnet run all [database]");
                Assert.Zero(ExecuteCommand("dotnet", "run codegen --script DatabaseEventOutbox.xml", Path.Combine(dir, $"{company}.{appName}.Database")).exitCode, "dotnet run codegen --script DatabaseEventOutbox.xml [database]");
            }

            // Run the intra-integration tests.
            Assert.Zero(ExecuteCommand("dotnet", $"test {company}.{ appName}.Test.csproj -v n", Path.Combine(dir, $"{company}.{appName}.Test")).exitCode, "dotnet test");
        }
    }
}