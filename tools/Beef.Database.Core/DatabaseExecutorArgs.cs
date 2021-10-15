// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen;
using System.Reflection;

namespace Beef.Database.Core
{
    /// <summary>
    /// Represents the <see cref="DatabaseExecutorArgs"/>.
    /// </summary>
    public class DatabaseExecutorArgs : DatabaseConsoleArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseExecutorArgs"/> class.
        /// </summary>
        /// <param name="command">The <see cref="DatabaseExecutorCommand"/>.</param>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="assemblies">The <see cref="Assembly"/> array whose embedded resources will be probed.</param>
        public DatabaseExecutorArgs(DatabaseExecutorCommand command, string connectionString, params Assembly[] assemblies)
        {
            Command = command;
            ConnectionString = Check.NotNull(connectionString, nameof(connectionString));
            Assemblies.AddRange(assemblies);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseExecutorArgs"/> class from a <see cref="DatabaseConsoleArgs"/>.
        /// </summary>
        /// <param name="args">The <see cref="DatabaseConsoleArgs"/>.</param>
        public DatabaseExecutorArgs(DatabaseConsoleArgs args) => CopyFrom(args);

        /// <summary>
        /// Clone the <see cref="DatabaseExecutorArgs"/>.
        /// </summary>
        /// <returns>A new <see cref="DatabaseExecutorArgs"/> instance.</returns>
        public override CodeGeneratorArgsBase Clone() => new DatabaseExecutorArgs(this);
    }
}