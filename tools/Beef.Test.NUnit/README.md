# Beef.Test.NUnit

Unit and intra-domain integration testing framework. This capability leverages [NUnit](https://github.com/nunit/nunit) for all testing.

## Upgrading from v4.x

> **NOTE:** The _Beef v5.x_ testing has **now** been replaced by the functionality available within [UnitTestEx](https://github.com/Avanade/unittestex). Any functionality available within this assembly is intended to assist with the upgrading from _Beef v4.x_; contains a subset of the previous functionality. This assembly will likely be deprecated at the next major version.

## Intra-domain vs. inter-domain testing

**Intra-domain** essentially means within (isolated to) the domain itself; excluding any external domain-based dependencies. For example a _Billing_ domain, may be supported by a SQL Server Database for data persistence, and as such is a candidate for inclusion within the testing.

However, if within this _Billing_ domain, there is an _Invoice_ entity with a _CustomerId_ attribute where the corresponding _Customer_ resides in another domain (external domain-based dependency) which is called to validate existence, then this should be excluded from within the testing. In this example, the cross-domain invocation should be mocked-out as it is considered **Inter-domain**.

In summary, **Intra-** is about _tight-coupling_, and **Inter-** is about _loose-coupling_.

<br/>