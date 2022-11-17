// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using DbEx;
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

                if (!args.Assemblies.Contains(typeof(SqlServerMigration).Assembly))
                    args.Assemblies.Add(typeof(SqlServerMigration).Assembly);
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