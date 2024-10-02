using NUnit.Framework;
using System;
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

        private static void OneTimeSetUp()
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
            Assert.That(exitCode, Is.AtLeast(0));

            var nugets = new DirectoryInfo(stdOut.Replace("info : global-packages: ", string.Empty).Replace("global-packages: ", string.Empty));
            Assert.That(nugets.Exists, Is.True);
            foreach (var di in nugets.EnumerateDirectories().Where(x => x.Name.StartsWith("beef.")))
            {
                di.Delete(true);
            }

            // Build Beef and package (nuget) - only local package, no deployment.
            Assert.That(ExecuteCommand("powershell", $"{Path.Combine(_rootDir.FullName, "nuget-publish.ps1")} -configuration 'Release' -IncludeSymbols -IncludeSource").exitCode, Is.AtLeast(0), "nuget publish");

            // Uninstall any previous beef templates (failure is ok here)
            ExecuteCommand("dotnet", "new uninstall beef.template.solution");

            // Determine the "actual" version to publish so we are explicit.
            var pf = Directory.GetFiles(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "nuget-publish"), "Beef.Template.Solution.*.nupkg").LastOrDefault();
            Assert.That(pf, Is.Not.Null, "Beef.Template.Solution.*.nupkg could not be found.");

            // Install the Beef template solution from local package.
            // dotnet new -i beef.template.solution --nuget-source https://api.nuget.org/v3/index.json
            Assert.That(ExecuteCommand("dotnet", $"new install beef.template.solution::{new FileInfo(pf).Name[23..^6]} --nuget-source {nugets}").exitCode, Is.AtLeast(0), "install beef.template.solution");
        }

        [Test]
        public void SqlServerProcs()
        {
            OneTimeSetUp();
            SolutionCreateGenerateTest("Foo.Db", "Bar", "SqlServerProcs");
        }

        [Test]
        public void SqlServer()
        {
            OneTimeSetUp();
            SolutionCreateGenerateTest("Foo.Ef", "Bar", "SqlServer");
        }

        [Test]
        public void SqlServer_WithServices()
        {
            OneTimeSetUp();
            SolutionCreateGenerateTest("Foo.EfWs", "Bar", "SqlServer", "AzFunction");
        }

        [Test]
        public void MySQL()
        {
            OneTimeSetUp();
            SolutionCreateGenerateTest("Foo.My", "Bar", "MySQL");
        }

        [Test]
        public void Postgres()
        {
            OneTimeSetUp();
            SolutionCreateGenerateTest("Foo.Ps", "Bar", "Postgres");
        }

        [Test]
        public void Cosmos()
        {
            OneTimeSetUp();
            SolutionCreateGenerateTest("Foo.Co", "Bar", "Cosmos");
        }

        [Test]
        public void HttpAgent()
        {
            OneTimeSetUp();
            SolutionCreateGenerateTest("Foo.Ha", "Bar", "HttpAgent");
        }

        private static void SolutionCreateGenerateTest(string company, string appName, string datasource, string services = null)
        {
            // Mkdir and create solution from template. 
            var dir = Path.Combine(_unitTests.FullName, $"{company}.{appName}");
            Directory.CreateDirectory(dir);
            Assert.That(ExecuteCommand("dotnet", $"new beef --company {company} --appname {appName} --datasource {datasource} {(string.IsNullOrEmpty(services) ? "" : $"--services {services}")}", dir).exitCode, Is.Zero, "dotnet new beef");

            // Restore nuget packages from our repository.
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "nuget-publish");
            Assert.That(ExecuteCommand("dotnet", $"restore -s {path}", dir).exitCode, Is.Zero, "dotnet restore");

            // CodeGen: Execute code-generation.
            Assert.That(ExecuteCommand("dotnet", "run all", Path.Combine(dir, $"{company}.{appName}.CodeGen")).exitCode, Is.Zero, "dotnet run all [entity]");

            // Database: Execute code-generation.
            if (datasource == "SqlServerProcs" || datasource == "SqlServer" || datasource == "MySQL" || datasource == "Postgres")
            {
                Assert.That(ExecuteCommand("dotnet", "run drop --accept-prompts", Path.Combine(dir, $"{company}.{appName}.Database")).exitCode, Is.Zero, "dotnet run drop [database]");
                Assert.That(ExecuteCommand("dotnet", "run createmigrateandcodegen", Path.Combine(dir, $"{company}.{appName}.Database")).exitCode, Is.Zero, "dotnet run all [database]");
                Assert.That(ExecuteCommand("dotnet", "run all", Path.Combine(dir, $"{company}.{appName}.Database")).exitCode, Is.Zero, "dotnet run all [database]");
            }

            // Run the intra-integration tests.
            Assert.That(ExecuteCommand("dotnet", $"test {company}.{ appName}.Test.csproj", Path.Combine(dir, $"{company}.{appName}.Test")).exitCode, Is.Zero, "dotnet test");

            if (services is not null)
                Assert.That(ExecuteCommand("dotnet", $"test {company}.{appName}.Services.Test.csproj", Path.Combine(dir, $"{company}.{appName}.Services.Test")).exitCode, Is.Zero, "dotnet test");
        }
    } 
}