// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using DbEx;
using OnRamp;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Database.SqlServer
{
    /// <summary>
    /// Provides the <see href="https://docs.microsoft.com/en-us/sql/connect/ado-net/microsoft-ado-net-sql-server">SQL Server</see> migration orchestration extending <see cref="DbEx.SqlServer.Migration.SqlServerMigration"/>
    /// to further enable <see cref="MigrationCommand.CodeGen"/>.
    /// </summary>
    public class SqlServerMigration : DbEx.SqlServer.Migration.SqlServerMigration
    {
        /// <summary>
        /// Initializes an instance of the <see cref="SqlServerMigration"/> class.
        /// </summary>
        /// <param name="args">The <see cref="MigrationArgs"/>.</param>
        public SqlServerMigration(MigrationArgs args) : base(args)
        {
            IsCodeGenEnabled = true;

            // Add in the beef schema stuff where requested.
            if (args.BeefSchema)
            {
                if (!args.SchemaOrder.Contains("dbo"))
                    args.SchemaOrder.Insert(0, "dbo");

                args.AddAssemblyAfter(typeof(DbEx.SqlServer.Migration.SqlServerMigration).Assembly, typeof(SqlServerMigration).Assembly);
            }
        }

        /// <summary>
        /// Gets the <see cref="MigrationArgs"/>.
        /// </summary>
        public new MigrationArgs Args => (MigrationArgs)base.Args;

        /// <inheritdoc/>
        protected override Task<(bool Success, string? Statistics)> DatabaseCodeGenAsync(CancellationToken cancellationToken = default)
        {
            var yaml = Args.GetParameter<string>("Param0");
            if (yaml is null)
                return this.ExecuteCodeGenAsync(cancellationToken);

            var schema = Args.GetParameter<string>("Param1");
            var tables = new List<string>();
            for (int i = 2; true; i++)
            {
                var table = Args.GetParameter<string>($"Param{i}");
                if (table is null)
                    break;

                tables.Add(table);
            }

            if (schema is null || tables.Count == 0)
                throw new CodeGenException($"A '{nameof(MigrationCommand.CodeGen)}' command for 'YAML' also requires schema and at least one table argument to be specified.");

            return this.ExecuteYamlCodeGenAsync(schema, tables.ToArray(), cancellationToken);
        }
    }
}