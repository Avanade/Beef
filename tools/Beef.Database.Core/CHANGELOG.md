# Change log

Represents the **NuGet** versions.

## v4.1.1
- *Enhancement:* Introduction of Dependency Injection support.

## v3.1.10
- *Fixed*: Issue [66](https://github.com/Avanade/Beef/issues/63) fixed. Changed the path separator to be `/` so that is compatible on Windows and Linux. By using `/` this matches the `Path.AltDirectorySeparatorChar`(https://docs.microsoft.com/en-us/dotnet/api/system.io.path.altdirectoryseparatorchar) for Windows and `Path.DirectorySeparatorChar`(https://docs.microsoft.com/en-us/dotnet/api/system.io.path.directoryseparatorchar) for Linux, making it universally compatible.

## v3.1.9
- *Enhancement:* Added `useBeefDbo` option to the `DatabaseExecutor.RunAsync` to include the standard _Beef_ `dbo` schema objects. Avoids having to specify the `Beef.Database.Core` assembly.
- *Fix:* Threading issue with the Insert or Merge of Yaml data process that has been resolved. Also updated to return the number of rows affected and output to the log.

## v3.1.8
- *Fixed:* Issue [55](https://github.com/Avanade/Beef/issues/55) has been resolved. Refactored database reset to no longer use `sp_MSforeachtable` which is not available in Azure SQL.

## v3.1.7
- *Fixed:* Issue [53](https://github.com/Avanade/Beef/issues/53) has been resolved. The `Beef.Database.Core` environment variable naming for connection string has been renamed to be more consistent and compatible with linux.

## v3.1.6
- *Fixed*: Code-gen templates updated to correct warnings identified by FxCop. Where no direct fix, or by intention, these have been explicitly ignored.

## v3.1.5
- *Enhancement:* Updated all dependent NuGet packages to their latest respective version.

## v3.1.4
- *Enhancement:* Added `fnGetUserId` and updated `spSetSessionContext` to support the new `ExecutionContext.UserId`.

## v3.1.3
- *Enhancement:* Updated all dependent NuGet packages to their latest respective version.

## v3.1.2
- *Fixed:* Migrations scripts were being executed by `DbUp` in alphabetical order regardless of Assembly order (`DbUp` default); now will execute in Assembly, then alphabetical order (override).
- *Fixed:* Migrations scripts were being executed by `DbUp` as `WithTransactionPerScript` which has been updated to `WithoutTransaction` as some DDL statements cannot run within a transactional context. However, it is recommended where possible that each migration script is transactional to avoid partial update on script failure; i.e. each script should only perform a single unit of work to ensure database consistency at all times.

## v3.1.1
- *Upgrade:* Upgraded the project to .NET Standard 2.1 (compatible with .NET Core 3.1).
- *Enhancement:* Tool updated to execute asynchoronously. Both `DatabaseConsole` and `DatabaseConsoleWrapper` have breaking change; `Run` has been removed, replaced with `RunAsync`.

## v2.1.9
- *Fixed:* `fnGetUsername` updated to support usernames as `nvarchar(1024)` to enable any reasonable username size to be supported. This is the same as what is currently supported by `spSetSessionContext`.

## v2.1.8
- *Enhancement:* Sprinkled `Task.ConfigureAwait(false)` as per https://devblogs.microsoft.com/dotnet/configureawait-faq/.

## v2.1.7
- *Fixed:* Introduced FxCop Analysis to `Beef.Database.Core`; this version represents the remediation based on the results.

## v2.1.6
- *Fixed:* Database merge statements updated to include `AND EXISTS (...)` for a `WHEN MATCHED` to avoid updates where column data has not changed.

## v2.1.5
- *Enhancement:* All usernames will be set to `ExecutionContext.EnvironmentUsername` (this results in the same outcome).

## v2.1.4
- *Fixed:* Internal *Beef* dependencies changed as a result of other changes.

## v2.1.3
- *Fixed:* Create of new script will no longer make the schema and table names lowercase for the script file name and will honour the input case.

## v2.1.2
- *Fixed:* Inconsistent version numbers.

## v2.1.1
- *New:* Initial publish to GitHub.