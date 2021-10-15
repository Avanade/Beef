// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.CodeGen.Console
{
    /// <summary>
    /// Provides the supported <see cref="CodeGenConsole"/> command-line console options.
    /// </summary>
    [Flags]
    public enum SupportedOptions
    {
        /// <summary>
        /// Supports overridding <see cref="CodeGeneratorArgsBase.ScriptFileName"/>.
        /// </summary>
        ScriptFileName = 1,

        /// <summary>
        /// Supports overridding the configuration file name (see <see cref="CodeGenerator.Generate(string)"/>).
        /// </summary>
        ConfigFileName = 2,

        /// <summary>
        /// Supports overridding <see cref="CodeGeneratorArgsBase.ExpectNoChanges"/>.
        /// </summary>
        ExpectNoChanges = 4,

        /// <summary>
        /// Supports overridding <see cref="CodeGeneratorArgsBase.IsSimulation"/>.
        /// </summary>
        IsSimulation = 8,

        /// <summary>
        /// Supports overridding <see cref="CodeGeneratorArgsBase.Assemblies"/>.
        /// </summary>
        Assemblies = 16,

        /// <summary>
        /// Supports overridding <see cref="CodeGeneratorArgsBase.Parameters"/>.
        /// </summary>
        Parameters = 32,

        /// <summary>
        /// Supports overridding <see cref="CodeGeneratorArgsBase.OutputDirectory"/>.
        /// </summary>
        OutputDirectory = 64,

        /// <summary>
        /// Supports overridding <see cref="CodeGeneratorArgsBase.ConnectionString"/>.
        /// </summary>
        DatabaseConnectionString = 128,

        /// <summary>
        /// Supports overridding <see cref="CodeGeneratorArgsBase.ConnectionStringEnvironmentVariableName"/>.
        /// </summary>
        DatabaseConnectionStringEnvironmentVariableName = 256,

        /// <summary>
        /// Supports all options except <see cref="DatabaseConnectionString"/> and <see cref="DatabaseConnectionStringEnvironmentVariableName"/>.
        /// </summary>
        AllExceptDatabase = ScriptFileName | ConfigFileName | ExpectNoChanges | IsSimulation | Assemblies | Parameters | OutputDirectory,

        /// <summary>
        /// Supports all options.
        /// </summary>
        All = AllExceptDatabase | DatabaseConnectionString | DatabaseConnectionStringEnvironmentVariableName
    }
}