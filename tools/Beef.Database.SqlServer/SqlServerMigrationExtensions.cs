// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

namespace Beef.Database.SqlServer
{
    /// <summary>
    /// Provides extension methods for the <see cref="SqlServerMigration"/> et al.
    /// </summary>
    public static class SqlServerMigrationExtensions
    {
        /// <summary>
        /// Include the SQL Server extended <b>Schema</b> scripts (stored procedures and functions) from <see href="https://github.com/Avanade/DbEx/tree/main/src/DbEx.SqlServer/Resources/ExtendedSchema"/>.
        /// </summary>
        /// <param name="args">The <see cref="MigrationArgs"/>.</param>
        /// <returns>The <see cref="MigrationArgs"/> to support fluent-style method-chaining.</returns>
        /// <remarks>See <see cref="DbEx.SqlServer.Console.MigrationArgsExtensions.AddExtendedSchemaScripts"/>.</remarks>
        public static MigrationArgs IncludeExtendedSchemaScripts(this MigrationArgs args)
        {
            DbEx.SqlServer.Console.MigrationArgsExtensions.AddExtendedSchemaScripts(args);
            return args;
        }
    }
}