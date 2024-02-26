// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using DbEx;
using OnRamp;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Database.MySql
{
    /// <summary>
    /// Provides the <see href="https://dev.mysql.com/">MySQL</see> migration orchestration extending <see cref="DbEx.MySql.Migration.MySqlMigration"/> to further enable <see cref="MigrationCommand.CodeGen"/>.
    /// </summary>
    public class MySqlMigration : DbEx.MySql.Migration.MySqlMigration
    {
        /// <summary>
        /// Initializes an instance of the <see cref="MySqlMigration"/> class.
        /// </summary>
        /// <param name="args">The <see cref="MigrationArgs"/>.</param>
        public MySqlMigration(MigrationArgs args) : base(args)
        {
            IsCodeGenEnabled = true;

            // Add in the beef schema stuff where requested.
            if (args.BeefSchema)
                args.AddAssemblyAfter(typeof(DbEx.MySql.Migration.MySqlMigration).Assembly, typeof(MySqlMigration).Assembly);
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

            var tables = new List<string>();
            for (int i = 1; true; i++)
            {
                var table = Args.GetParameter<string>($"Param{i}");
                if (table is null)
                    break;

                tables.Add(table);
            }

            if (tables.Count == 0)
                throw new CodeGenException($"A '{nameof(MigrationCommand.CodeGen)}' command for 'YAML' also requires at least one table argument to be specified.");

            return this.ExecuteYamlCodeGenAsync(null, [.. tables], cancellationToken);
        }
    }
}