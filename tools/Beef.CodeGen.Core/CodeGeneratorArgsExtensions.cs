// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using Beef.CodeGen.OpenApi;
using DbEx.Migration;
using OnRamp;
using System;
using System.Collections.Generic;

namespace Beef.CodeGen
{
    /// <summary>
    /// Provides extenstion methods to <see cref="CodeGeneratorArgs"/>.
    /// </summary>
    public static class CodeGeneratorArgsExtensions
    {
        /// <summary>
        /// Gets the <see cref="CodeGeneratorArgsBase.Parameters"/> value with a key of <see cref="CodeGenConsole.CompanyParamName"/>.
        /// </summary>
        /// <param name="args">The <see cref="ICodeGeneratorArgs"/>.</param>
        /// <param name="throwWhereNotFound">Indicates to throw a <see cref="KeyNotFoundException"/> when the specified key is not found.</param>
        public static string GetCompany(this ICodeGeneratorArgs args, bool throwWhereNotFound = false) => (args ?? throw new ArgumentNullException(nameof(args))).GetParameter<string>(CodeGenConsole.CompanyParamName, throwWhereNotFound)!;

        /// <summary>
        /// Gets the <see cref="CodeGeneratorArgsBase.Parameters"/> value with a key of <see cref="CodeGenConsole.AppNameParamName"/>.
        /// </summary>
        /// <param name="args">The <see cref="ICodeGeneratorArgs"/>.</param>
        /// <param name="throwWhereNotFound">Indicates to throw a <see cref="KeyNotFoundException"/> when the specified key is not found.</param>
        public static string GetAppName(this ICodeGeneratorArgs args, bool throwWhereNotFound = false) => (args ?? throw new ArgumentNullException(nameof(args))).GetParameter<string>(CodeGenConsole.AppNameParamName, throwWhereNotFound)!;

        /// <summary>
        /// Validate that <see cref="CodeGeneratorArgsBase.Parameters"/> with a key of <see cref="CodeGenConsole.CompanyParamName"/> and <see cref="CodeGenConsole.AppNameParamName"/> have been specified and throw a
        /// <see cref="CodeGenException"/> where invalid.
        /// </summary>
        /// <param name="args">The <see cref="ICodeGeneratorArgs"/>.</param>
        /// <exception cref="CodeGenException">A <see cref="CodeGenException"/> is thrown where not specified.</exception>
        public static void ValidateCompanyAndAppName(this ICodeGeneratorArgs args)
        {
            if (string.IsNullOrEmpty((args ?? throw new ArgumentNullException(nameof(args))).GetCompany()) || string.IsNullOrEmpty(args.GetAppName()))
                throw new CodeGenException($"Parameters '{CodeGen.CodeGenConsole.CompanyParamName}' and {CodeGen.CodeGenConsole.AppNameParamName} must be specified.");
        }

        /// <summary>
        /// Gets the <see cref="DatabaseMigrationBase"/> from the connection details within the <see cref="ICodeGeneratorDbArgs"/>.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>The <see cref="DatabaseMigrationBase"/> instance.</returns>
        public static DatabaseMigrationBase GetDatabaseMigrator(this ICodeGeneratorArgs args) => (args ?? throw new ArgumentNullException(nameof(args))).GetParameter<DatabaseMigrationBase>(CodeGenConsole.DatabaseMigratorParamName, true)!;

        /// <summary>
        /// Adds the <paramref name="migrator"/> as the <see cref="CodeGenConsole.DatabaseMigratorParamName"/> <see cref="ICodeGeneratorArgs.Parameters"/> value.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <param name="migrator">The <see cref="DatabaseMigrationBase"/> instance.</param>
        public static void AddDatabaseMigrator(this ICodeGeneratorArgs args, DatabaseMigrationBase migrator) => args.AddParameter(CodeGenConsole.DatabaseMigratorParamName, migrator ?? throw new ArgumentNullException(nameof(args)));

        /// <summary>
        /// Uses (adds) the <see cref="OpenApiArgs"/> to the <see cref="ICodeGeneratorArgs.Parameters"/>.
        /// </summary>
        /// <typeparam name="TArgs">The <see cref="ICodeGeneratorArgs"/> <see cref="Type"/>.</typeparam>
        /// <param name="args">The <see cref="ICodeGeneratorArgs"/>.</param>
        /// <param name="openApiArgs">The <see cref="OpenApiArgs"/>.</param>
        /// <returns>The <see cref="ICodeGeneratorArgs"/> instance.</returns>
        public static TArgs UseOpenApiArgs<TArgs>(this TArgs args, OpenApiArgs openApiArgs) where TArgs : ICodeGeneratorArgs
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (openApiArgs == null)
                throw new ArgumentNullException(nameof(openApiArgs));

            args.AddParameter(nameof(OpenApiArgs), openApiArgs);
            return args;
        }

        /// <summary>
        /// Gets the <see cref="OpenApiArgs"/>.
        /// </summary>
        /// <param name="args">The <see cref="ICodeGeneratorArgs"/>.</param>
        /// <returns>The <see cref="OpenApiArgs"/>.</returns>
        public static OpenApiArgs GetOpenApiArgs(this ICodeGeneratorArgs args)
        {
            var openApiArgs = args.GetParameter<OpenApiArgs>(nameof(OpenApiArgs), false)!;
            if (openApiArgs is not null)
                return openApiArgs;

            openApiArgs = new OpenApiArgs();
            args.UseOpenApiArgs(openApiArgs);
            return openApiArgs;
        }
    }
}