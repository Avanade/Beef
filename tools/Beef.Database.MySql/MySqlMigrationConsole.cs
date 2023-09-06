// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace Beef.Database.MySql
{
    /// <summary>
    /// Provides the SQL Server migration console capability.
    /// </summary>
    public class MySqlMigrationConsole : MigrationConsoleBase<MySqlMigrationConsole>
    {
        /// <summary>
        /// Creates a new <see cref="MySqlMigrationConsole"/> using <typeparamref name="T"/> to default the probing <see cref="Assembly"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/>.</typeparam>
        /// <param name="connectionString">The database connection string.</param>
        /// <returns>A new <see cref="MySqlMigrationConsole"/>.</returns>
        public static MySqlMigrationConsole Create<T>(string connectionString) => new(new MigrationArgs { ConnectionString = connectionString }.AddAssembly(typeof(T).Assembly));

        /// <summary>
        /// Creates a new instance of the <see cref="MySqlMigrationConsole"/> class using the specified parameters.
        /// </summary>
        /// <param name="connectionString">The default connection string.</param>
        /// <param name="company">The company name.</param>
        /// <param name="appName">The application/domain name.</param>
        /// <returns>The <see cref="MySqlMigrationConsole"/> instance.</returns>
        public static MySqlMigrationConsole Create(string connectionString, string company, string appName)
            => new(new MigrationArgs { ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString)) }
                .AddParameter(CodeGen.CodeGenConsole.CompanyParamName, company ?? throw new ArgumentNullException(nameof(company)))
                .AddParameter(CodeGen.CodeGenConsole.AppNameParamName, appName ?? throw new ArgumentNullException(nameof(appName))));

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlMigrationConsole"/> class.
        /// </summary>
        /// <param name="args">The default <see cref="MigrationArgs"/> that will be overridden/updated by the command-line argument values.</param>
        public MySqlMigrationConsole(MigrationArgs? args = null) : base(args) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlMigrationConsole"/> class that provides a default for the <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        public MySqlMigrationConsole(string connectionString) : this(new MigrationArgs { ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString)) }) { }

        /// <inheritdoc/>
        protected override DbEx.Migration.DatabaseMigrationBase CreateMigrator() => new MySqlMigration(Args);

        /// <inheritdoc/>
        public override string AppTitle => base.AppTitle + " [MySQL]";

        /// <inheritdoc/>
        protected override void OnWriteHelp()
        {
            base.OnWriteHelp();
            new DbEx.MySql.Console.MySqlMigrationConsole(new DbEx.Migration.MigrationArgs { Logger = Logger }).WriteScriptHelp();
            Logger?.LogInformation("{help}", string.Empty);
            Logger?.LogInformation("{help}", "Extended CodeGen command and argument(s):");
            Logger?.LogInformation("{help}", "  codegen yaml <Table> [<Table>...]   Creates a temporary Beef entity code-gen YAML file for the specified table(s).");
            Logger?.LogInformation("{help}", "                                               - A table name with a prefix ! denotes that no CRUD operations are required.");
            Logger?.LogInformation("{help}", "                                               - A table name with a prefix * denotes that a 'GetByArgs' operation is required.");
            Logger?.LogInformation("{help}", string.Empty);
        }
    }
}