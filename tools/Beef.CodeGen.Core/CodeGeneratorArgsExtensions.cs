// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

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
        /// <param name="args">The <see cref="CodeGeneratorArgs"/>.</param>
        /// <param name="throwWhereNotFound">Indicates to throw a <see cref="KeyNotFoundException"/> when the specified key is not found.</param>
        public static string GetCompany(this CodeGeneratorArgsBase args, bool throwWhereNotFound = false) => (args ?? throw new ArgumentNullException(nameof(args))).GetParameter(CodeGenConsole.CompanyParamName, throwWhereNotFound)!;

        /// <summary>
        /// Gets the <see cref="CodeGeneratorArgsBase.Parameters"/> value with a key of <see cref="CodeGenConsole.AppNameParamName"/>.
        /// </summary>
        /// <param name="args">The <see cref="CodeGeneratorArgs"/>.</param>
        /// <param name="throwWhereNotFound">Indicates to throw a <see cref="KeyNotFoundException"/> when the specified key is not found.</param>
        public static string GetAppName(this CodeGeneratorArgsBase args, bool throwWhereNotFound = false) => (args ?? throw new ArgumentNullException(nameof(args))).GetParameter(CodeGenConsole.AppNameParamName, throwWhereNotFound)!;
    }
}