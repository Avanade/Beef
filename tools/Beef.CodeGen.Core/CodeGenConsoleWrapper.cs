// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;
using System.Reflection;

namespace Beef.CodeGen
{
    /// <summary>
    /// <b>CodeGen Console</b> wrapper to simplify/standardise execution of the <see cref="CodeGenConsole"/>. 
    /// </summary>
    [Obsolete("Please use the CodeGenConsole class as this will be deprecated at the next major version.")]
    public class CodeGenConsoleWrapper
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CodeGenConsole"/> class defaulting to <see cref="Assembly.GetCallingAssembly"/>.
        /// </summary>
        /// <param name="company">The company name.</param>
        /// <param name="appName">The application/domain name.</param>
        /// <param name="apiName">The Web API name.</param>
        /// <param name="directory">The base output path/directory.</param>
        /// <returns>The <see cref="CodeGenConsole"/> instance.</returns>
        public static CodeGenConsole Create(string company, string appName, string apiName = "Api", string? directory = null) => Create(new Assembly[] { Assembly.GetCallingAssembly() }, company, appName, apiName, directory);

        /// <summary>
        /// Creates a new instance of the <see cref="CodeGenConsole"/> class.
        /// </summary>
        /// <param name="assemblies">The list of additional assemblies to probe for resources.</param>
        /// <param name="company">The company name.</param>
        /// <param name="appName">The application/domain name.</param>
        /// <param name="apiName">The Web API name.</param>
        /// <param name="directory">The base output path/directory.</param>
        /// <returns>The <see cref="CodeGenConsole"/> instance.</returns>
        public static CodeGenConsole Create(Assembly[] assemblies, string company, string appName, string apiName = "Api", string? directory = null) => CodeGenConsole.Create(assemblies, company, appName, apiName, directory);
    }
}