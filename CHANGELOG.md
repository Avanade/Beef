# Change log

Represents the **NuGet** versions.

## v5.1.0

Represents the initial commit for _Beef_ version 5.x. All assemblies/packages now share the same version and change log; i.e. they are now published as a set versus individually versioned (prior releases). This version is a _major_ refactoring from the prior; to achieve largely the same outcomes, in a modernized decoupled manner.

As stated in the [README](./README.md), _Beef_ is _now_ (as of version 5.x) obstensibly the code-generation engine, and solution orchestration, that ultimately takes dependencies on the following capabilities to enable the end-to-functionality and testing thereof in a standardized (albiet somewhat opinionated) manner:
- [CoreEx](https://github.com/Avanade/CoreEx) - provides the core runtime capabilties (extends .NET core);
- [UnitTestEx](https://github.com/Avanade/UnitTestEx) - provides extended unit and intra-domain integration testing;
- [DbEx](https://github.com/Avanade/DbEx) - provides extended database management capabilties;
- [OnRamp](https://github.com/Avanade/OnRamp) - provides the underlying code-generation engine functionality.

Prior to version 5.x, _Beef_ was all encompassing. These capabilities have been extracted, simplified and refactored to be first class frameworks in their own right, and made into the repos listed above. This allows them to be used and maintained independently to _Beef_; therefore, offering greater opportunities for reuse versus all-or-nothing.