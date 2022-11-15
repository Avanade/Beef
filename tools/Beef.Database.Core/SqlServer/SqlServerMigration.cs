// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using DbEx;
using System.Threading;
using System.Threading.Tasks;

namespace Beef.Database.Core.SqlServer
{
    /// <summary>
    /// Provides the <see href="https://docs.microsoft.com/en-us/sql/connect/ado-net/microsoft-ado-net-sql-server">SQL Server</see> migration orchestration extending <see cref="DbEx.Migration.SqlServer.SqlServerMigration"/>
    /// to further enable <see cref="MigrationCommand.CodeGen"/>.
    /// </summary>
    public class SqlServerMigration : DbEx.Migration.SqlServer.SqlServerMigration
    {
        /// <summary>
        /// Initializes an instance of the <see cref="SqlServerMigration"/> class.
        /// </summary>
        /// <param name="args">The <see cref="MigrationArgs"/>.</param>
        public SqlServerMigration(MigrationArgs args) : base(args)
        {
            IsCodeGenEnabled = true;
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