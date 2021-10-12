// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Database.Core
{
    /// <summary>
    /// Represents the <see cref="DatabaseExecutor"/> commands.
    /// </summary>
    [Flags]
    public enum DatabaseExecutorCommand
    {
        /// <summary>
        /// Nothing specified.
        /// </summary>
        None = 0,

        /// <summary>
        /// Drop the existing database (where it already exists).
        /// </summary>
        Drop = 1,

        /// <summary>
        /// Create the database (where it does not already exist).
        /// </summary>
        Create = 2,

        /// <summary>
        /// Migrate the database using the <b>Migrations</b> scripts (those that have not already been executed).
        /// </summary>
        Migrate = 4,

        /// <summary>
        /// Generates database <b>Schema</b> objects via code generation.
        /// </summary>
        CodeGen = 8,

        /// <summary>
        /// Drops and creates the known database <b>Schema</b> objects.
        /// </summary>
        Schema = 16,

        /// <summary>
        /// Resets the database by deleting all existing data.  
        /// </summary>
        Reset = 32,

        /// <summary>
        /// Inserts or merges <b>Data</b> from embedded YAML files.
        /// </summary>
        Data = 64,

        /// <summary>
        /// Performs <b>all</b> commands as follows; <see cref="DatabaseExecutorCommand.Create"/>, <see cref="DatabaseExecutorCommand.Migrate"/>, <see cref="DatabaseExecutorCommand.CodeGen"/>, <see cref="DatabaseExecutorCommand.Schema"/> and <see cref="DatabaseExecutorCommand.Data"/>.
        /// </summary>
        All = Create | Migrate | CodeGen | Schema | Data,

        /// <summary>
        /// Performs <see cref="DatabaseExecutorCommand.Drop"/> and <see cref="DatabaseExecutorCommand.All"/>.
        /// </summary>
        DropAndAll = Drop | All,

        /// <summary>
        /// Performs <see cref="DatabaseExecutorCommand.Reset"/> and <see cref="DatabaseExecutorCommand.All"/>.
        /// </summary>
        ResetAndAll = Reset | All,

        /// <summary>
        /// Performs only the <b>database</b> commands as follows: <see cref="DatabaseExecutorCommand.Create"/>, <see cref="DatabaseExecutorCommand.Migrate"/>, <see cref="DatabaseExecutorCommand.Schema"/> and <see cref="DatabaseExecutorCommand.Data"/>.
        /// </summary>
        Database = Create | Migrate | Schema | Data,

        /// <summary>
        /// Performs <see cref="DatabaseExecutorCommand.Drop"/> and <see cref="DatabaseExecutorCommand.Database"/>.
        /// </summary>
        DropAndDatabase = Drop | Database,

        /// <summary>
        /// Performs <see cref="DatabaseExecutorCommand.Reset"/> and <see cref="DatabaseExecutorCommand.Database"/>.
        /// </summary>
        ResetAndDatabase = Reset | Database,

        /// <summary>
        /// Performs <see cref="DatabaseExecutorCommand.Reset"/> and <see cref="DatabaseExecutorCommand.Data"/>.
        /// </summary>
        ResetAndData = Reset | Data,

        /// <summary>
        /// Creates a new script file using the defined naming convention.
        /// </summary>
        ScriptNew = 2048,
    }
}
