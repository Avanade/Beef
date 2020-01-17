﻿# Change log

Represents the **NuGet** versions.

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