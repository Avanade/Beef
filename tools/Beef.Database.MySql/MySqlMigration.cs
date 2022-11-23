// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using DbEx;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Database.MySql
{
    /// <summary>
    /// Provides the <see href="https://dev.mysql.com/">MySQL</see> migration orchestration extending <see cref="DbEx.MySql.Migration.MySqlMigration"/>
    /// to further enable <see cref="MigrationCommand.CodeGen"/>.
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
            {
                if (!args.Assemblies.Contains(typeof(MySqlMigration).Assembly))
                    args.Assemblies.Add(typeof(MySqlMigration).Assembly);
            }

            this.Initialization();
        }

        /// <summary>
        /// Gets the <see cref="MigrationArgs"/>.
        /// </summary>
        public new MigrationArgs Args => (MigrationArgs)base.Args;

        /// <inheritdoc/>
        protected override Task<(bool Success, string? Statistics)> DatabaseCodeGenAsync(CancellationToken cancellationToken = default) => this.ExecuteCodeGenAsync(cancellationToken);
    }
}