# Change log

Represents the **NuGet** versions.

## v4.2.3
- *Enhancement:* Changed `WebApiAgentBase.CreateJsonContentFromValue` and `WebApiAgentBase.CreateRequestMessageAsync` from `private` to `protected` to improve reusability of functionality for inheriting classes.
- *Enhancement:* Updated `WebApiAgentBase.CreateFullUri` to use `StringBuilder` to reduce string allocations.
- *Enhancement:* Added `IWebApiAgentResult.StatusCode` which was previously missing.
- *Fixed:* Corrected the `UriFormat.UriFormat` to format the query string parameter name using `Uri.EscapeUriString` versus `Uri.EscapeDataString`.

## v4.2.2
- *Enhancement:* Added `Clean<T>(T value, bool overrideWithNullWhenIsInitial)` method to `Cleaner`, with the option to override the value with `null` when the value implements `ICleanUp` and `ICleanUp.IsInitial` is `true`. The existing `Clean<T>(T value)` method now invokes new passing `true`; so in effect will always override with `null` unless explicitly requested not to; see next point.
- *Enhancement:* The `EntityBasicBase.SetValue<T>(ref T propertyValue, T setValue, ...` has been updated to always _not_ override with null (i.e. `Cleaner.Clean<T>(value, false)`); this will ensure that the value is not nulled whilst being potentially updated.
- *Enhancement:* Added `ILogicallyDeleted` and `IMultiTenant` interfaces.
- *Enhancement:* Renamed `IIntIdentifier` to `IInt32Identifier`, and added new `IInt64Identifier`.
- *Enhancement:* Renamed `ReferenceDataBaseInt` to `ReferenceDataBaseInt32`, and added new `ReferenceDataBaseInt64`. Plus additional changes to support both `Int32` and `Int64` options.
- *Enhancement:* `Mapper`-related artefacts relocated to `Beef.Core`.

## v4.2.1
- *New:* Initial publish to GitHub made in conjunction with `Beef.Core` version `v4.2.1`. The core abstractions (nucleus) and capabilities have been moved to this assembly to have a more static core code base, decoupling from `Beef.Core` (which continues to have regular enhancements/fixes). As a minimum, a `Common` project should only have a dependency on this (not `Beef.Core`) to minimize assembly versioning challenges when using `Common` projects from other domains/applications. See Issue [139](https://github.com/Avanade/Beef/issues/139) for more details.
- *Fixed:* The `WebApiAgentBase` was appending a `\` character even where no `urlSuffix` was specified creating an invalid URL; this was an issue if the originating base URL contained a query string for example.