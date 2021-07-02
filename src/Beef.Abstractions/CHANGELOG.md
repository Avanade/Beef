# Change log

Represents the **NuGet** versions.

## v4.2.1
- *New:* Initial publish to GitHub made in conjunction with `Beef.Core` version `v4.2.1`. The core abstractions (nucleus) and capabilities have been moved to this assembly to have a more static core code base, decoupling from `Beef.Core` (which continues to have regular enhancements/fixes). As a minimum, a `Common` project should only have a dependency on this (not `Beef.Core`) to minimize assembly versioning challenges when using `Common` projects from other domains/applications. See Issue [139](https://github.com/Avanade/Beef/issues/139) for more details.
- *Fixed:* The `WebApiAgentBase` was appending a `\` character even where no `urlSuffix` was specified creating an invalid URL; this was an issue if the originating base URL contained a query string for example.
