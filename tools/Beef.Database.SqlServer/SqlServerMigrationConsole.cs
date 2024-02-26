// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using CoreEx;
using Microsoft.Extensions.Logging;
using System;
using System.Reflection;

namespace Beef.Database.SqlServer
{
    /// <summary>
    /// Provides the SQL Server migration console capability.
    /// </summary>
    /// <param name="args">The default <see cref="MigrationArgs"/> that will be overridden/updated by the command-line argument values.</param>
    public class SqlServerMigrationConsole(MigrationArgs? args = null) : MigrationConsoleBase<SqlServerMigrationConsole>(args)
    {
        /// <summary>
        /// Creates a new <see cref="SqlServerMigrationConsole"/> using <typeparamref name="T"/> to default the probing <see cref="Assembly"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/>.</typeparam>
        /// <param name="connectionString">The database connection string.</param>
        /// <returns>A new <see cref="SqlServerMigrationConsole"/>.</returns>
        public static SqlServerMigrationConsole Create<T>(string connectionString) => new(new MigrationArgs { ConnectionString = connectionString }.AddAssembly(typeof(T).Assembly));

        /// <summary>
        /// Creates a new instance of the <see cref="SqlServerMigrationConsole"/> class using the specified parameters.
        /// </summary>
        /// <param name="connectionString">The default connection string.</param>
        /// <param name="company">The company name.</param>
        /// <param name="appName">The application/domain name.</param>
        /// <returns>The <see cref="SqlServerMigrationConsole"/> instance.</returns>
        public static SqlServerMigrationConsole Create(string connectionString, string company, string appName)
            => new(new MigrationArgs { ConnectionString = connectionString.ThrowIfNull(nameof(connectionString)) }
                .AddParameter(CodeGen.CodeGenConsole.CompanyParamName, company.ThrowIfNull(nameof(company)))
                .AddParameter(CodeGen.CodeGenConsole.AppNameParamName, appName.ThrowIfNull(nameof(appName))));

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlServerMigrationConsole"/> class that provides a default for the <paramref name="connectionString"/>.
        /// </summary>
        /// <param name="connectionString">The database connection string.</param>
        public SqlServerMigrationConsole(string connectionString) : this(new MigrationArgs { ConnectionString = connectionString.ThrowIfNull(nameof(connectionString)) }) { }

        /// <inheritdoc/>
        protected override DbEx.Migration.DatabaseMigrationBase CreateMigrator() => new SqlServerMigration(Args);

        /// <inheritdoc/>
        public override string AppTitle => base.AppTitle + " [SQL Server]";

        /// <inheritdoc/>
        protected override void OnWriteHelp()
        {
            base.OnWriteHelp();
            new DbEx.SqlServer.Console.SqlServerMigrationConsole(new DbEx.Migration.MigrationArgs { Logger = Logger }).WriteScriptHelp();
            Logger?.LogInformation("{help}", string.Empty);
            Logger?.LogInformation("{help}", "Extended CodeGen command and argument(s):");
            Logger?.LogInformation("{help}", "  codegen yaml <Schema> <Table> [<Table>...]   Creates a temporary Beef entity code-gen YAML file for the specified table(s).");
            Logger?.LogInformation("{help}", "                                               - A table name with a prefix ! denotes that no CRUD operations are required.");
            Logger?.LogInformation("{help}", "                                               - A table name with a prefix * denotes that a 'GetByArgs' operation is required.");
            Logger?.LogInformation("{help}", string.Empty);
        }
    }
}