# Change log

Represents the **NuGet** versions.

## v3.1.1
- *Upgrade:* Upgraded the project to .NET Standard 2.1 (compatible with .NET Core 3.1).

## v2.1.3
- *Enhancement:* Sprinkled `Task.ConfigureAwait(false)` as per https://devblogs.microsoft.com/dotnet/configureawait-faq/.

## v2.1.2
- *Fixed:* Introduced FxCop Analysis to `Beef.Events`; this version represents the remediation based on the results.

## v2.1.1
- *New:* Initial publish to GitHub. New capability to support an Event-driven Architecture; specifically leveraging Azure EventHubs.