# Upgrading to .NET 6.0

The _Beef_ solution has been upgraded to support both .NET Core 3.1 and .NET 6.0. Although this is a major upgrade to .NET there have been **no** breaking changes introduced within _Beef_ itself.

<br/>

## Versioning

There hase been no changes to the versioning other than a bump to all packages as any internal dependencies have been updated accordingly. As of this release the following packages will now support .NET Core 3.1 and .NET 6.0; otherwise, they are all targeting .NET Standard which enables them to be used in either version.

- `Beef.AspNetCore.WebApi` - `v4.2.6+`
- `Beef.CodeGen.Core` - `v4.2.15+`
- `Beef.Database.Core` - `v4.2.4+`
- `Beef.Test.NUnit` - `v4.2.6+`

<br/>

## Change Log

Review the `CHANGELOG.md` for each project/assembly to review all the changes made. No functionality changes introduced from last point version.

<br/>

## Upgrading existing

The following walks through the high-level process of upgrading an existing .NET Core 3.1 solution leveraging _Beef_.

1. Update all projects targeting `netcoreapp3.1` and change to `net6.0`; typically all projects except `*.Common`.
2. Updates any other dependent packages to latest where applicable.
3. Rebuild all projects and fix any errors/warnings as required.
4. Re-execute the tools `CodeGen` and `Database` (where applicable); there were no `net6.0` specific changes introduced.
5. Re-run all your tests and fix any issues.
6. Enjoy the rest of your day :-)