// Copyright (c) Avanade. Licensed under the MIT License. See https://github.com/Avanade/Beef

using System;

namespace Beef.Database.Core
{
    /// <summary>
    /// <b>Database Console</b> wrapper to simplify/standardise execution of the <see cref="DatabaseConsole"/>. 
    /// </summary>
    [Obsolete("Please use the DatabaseConsole class as this will be deprecated at the next major version.")]
    public class DatabaseConsoleWrapper
    {
        /// <summary>
        /// Creates a new instance of the <see cref="DatabaseConsole"/> class.
        /// </summary>
        /// <param name="connectionString">The default connection string.</param>
        /// <param name="company">The company name.</param>
        /// <param name="appName">The application/domain name.</param>
        /// <param name="useBeefDbo">Indicates whether to use the standard BEEF <b>dbo</b> schema objects (defaults to <c>true</c>).</param>
        /// <returns>The <see cref="DatabaseConsole"/> instance.</returns>
        public static DatabaseConsole Create(string connectionString, string company, string appName, bool useBeefDbo = true) => DatabaseConsole.Create(connectionString, company, appName, useBeefDbo);
    }
}