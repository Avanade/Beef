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
        /// Performs <b>all</b> commands as follows; <see cref="Create"/>, <see cref="Migrate"/>, <see cref="CodeGen"/>, <see cref="Schema"/> and <see cref="Data"/>.
        /// </summary>
        All = Create | Migrate | CodeGen | Schema | Data,

        /// <summary>
        /// Performs <see cref="Migrate"/> and <see cref="Schema"/>.
        /// </summary>
        Deploy = Migrate | Schema,

        /// <summary>
        /// Performs <see cref="Deploy"/> with <see cref="Data"/>.
        /// </summary>
        DeployWithData = Deploy | Data,

        /// <summary>
        /// Performs <see cref="Drop"/> and <see cref="All"/>.
        /// </summary>
        DropAndAll = Drop | All,

        /// <summary>
        /// Performs <see cref="Reset"/> and <see cref="All"/>.
        /// </summary>
        ResetAndAll = Reset | All,

        /// <summary>
        /// Performs only the <b>database</b> commands as follows: <see cref="Create"/>, <see cref="Migrate"/>, <see cref="Schema"/> and <see cref="Data"/>.
        /// </summary>
        Database = Create | Migrate | Schema | Data,

        /// <summary>
        /// Performs <see cref="Drop"/> and <see cref="Database"/>.
        /// </summary>
        DropAndDatabase = Drop | Database,

        /// <summary>
        /// Performs <see cref="Reset"/> and <see cref="Database"/>.
        /// </summary>
        ResetAndDatabase = Reset | Database,

        /// <summary>
        /// Performs <see cref="Reset"/> and <see cref="Data"/>.
        /// </summary>
        ResetAndData = Reset | Data,

        /// <summary>
        /// Executes the SQL statement passed as the first additional argument.
        /// </summary>
        /// <remarks>This can not be used with any of the other commands.</remarks>
        Execute = 1024,

        /// <summary>
        /// Creates a new script file using the defined naming convention.
        /// </summary>
        /// <remarks>This can not be used with any of the other commands.</remarks>
        Script = 2048
    }
}