# Upgrading to Beef 4.0.1

The _Beef_ solution went through a significant enhancement to introduce [Dependency Injection (DI)](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) throughout the solution. This required a number of changes to the existing approach where the code-generated classes were instantiated via a `Factory` around a parameterless constructor. Many _Beef_ framework classes had to be refactored as a result to remove the `static` capabilities as these were counter to the DI approach. Because of this, there was a flow on requirement to review and refactor the corresponding testing to leverage DI, etc.

## Breaking changes

As a result of above, there was no simple way to avoid **breaking changes** to the solution. This article will guide a developer, as best it can, through the process of upgrading a _Beef v3.1.x_ solution to _v4.0.1_.

Also, any other code that had been preivously marked using the `ObsoluteAttribute` has since been removed.

Where other code/capabilities were deemed obsolete, or had seen limited use, these have also been removed. For example, the `Beef.Diagnostics.Logger` has been largely replaced as a result of the DI changes; and as such should no longer be considered the go to logging enabler.

