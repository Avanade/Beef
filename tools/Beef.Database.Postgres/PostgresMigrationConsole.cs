// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace Beef.Database.Postgres
{
    /// <summary>
    /// Provides the SQL Server migration console capability.
    /// </summary>
    /// <param name="args">The default <see cref="MigrationArgs"/> that will be overridden/updated by the command-line argument values.</param>
    public class PostgresMigrationConsole(MigrationArgs? args = null) : MigrationConsoleBase<PostgresMigrationConsole>(args)
    {
        /// <summary>
        /// Creates a new <see cref="PostgresMigrationConsole"/> using <typeparamref name="T"/> to default the probing <see cref="Assembly"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/>.</typeparam>
        /// <param name="connectionString">The database connection string.</param>
        /// <returns>A new <see cref="PostgresMigrationConsole"/>.</returns>
        public static PostgresMigrationConsole Create<T>(string connectionString) => new(new MigrationArgs { ConnectionString = connectionString }.AddAssembly(typeof(T).Assembly));

        /// <summary>
        /// Creates a new instance of the <see cref="PostgresMigrationConsole"/> class using the specified parameters.
        /// </summary>
        /// <param name="connectionString">The default connection string.</param>
        /// <param name="company">The company name.</param>
        /// <param name="appName">The application/domain name.</param>
        /// <returns>The <see cref="PostgresMigrationConsole"/> instance.</returns>
        public static PostgresMigrationConsole Create(string connectionString, string company, string appName)
            => new(new MigrationArgs { ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString)) }
                .AddParameter(CodeGen.CodeGenConsole.CompanyParamName, company ?? throw new ArgumentNullException(nameof(company)))
                .AddParameter(CodeGen.CodeGenConsole.AppNameParamName, appName ?? throw new ArgumentNullException(nameof(appName))));

        /// <summary>
        /// Initializes a new instance of the <see cref="PostgresMigrationConsole"/> class that provides a default for the <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        public PostgresMigrationConsole(string connectionString) : this(new MigrationArgs { ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString)) }) { }

        /// <inheritdoc/>
        protected override DbEx.Migration.DatabaseMigrationBase CreateMigrator() => new PostgresMigration(Args);

        /// <inheritdoc/>
        public override string AppTitle => base.AppTitle + " [PostgreSQL]";

        /// <inheritdoc/>
        protected override void OnWriteHelp()
        {
            base.OnWriteHelp();
            new DbEx.Postgres.Console.PostgresMigrationConsole(new DbEx.Migration.MigrationArgs { Logger = Logger }).WriteScriptHelp();
            Logger?.LogInformation("{help}", string.Empty);
            Logger?.LogInformation("{help}", "Extended CodeGen command and argument(s):");
            Logger?.LogInformation("{help}", "  codegen yaml <Table> [<Table>...]   Creates a temporary Beef entity code-gen YAML file for the specified table(s).");
            Logger?.LogInformation("{help}", "                                               - A table name with a prefix ! denotes that no CRUD operations are required.");
            Logger?.LogInformation("{help}", "                                               - A table name with a prefix * denotes that a 'GetByArgs' operation is required.");
            Logger?.LogInformation("{help}", string.Empty);
        }
    }
}