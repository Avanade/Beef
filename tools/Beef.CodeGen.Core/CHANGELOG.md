# Change log

Represents the **NuGet** versions.

## v2.1.7
- *Enhancement:* `ReferenceDataData_cs.xml` template updated to support `AutoImplement="EntityFramework"` to simplify the loading of reference data items where using Entity Framework. 

## v2.1.6
- *Enhancement:* Added `WithHints` to stored procedure configuration to output `WITH(value)` table [hint](https://docs.microsoft.com/en-us/sql/t-sql/queries/hints-transact-sql-table).
- *Enhancement:* Added support to generate data logic access using *Cosmos* DB. This follows the same pattern as *Database*, *OData* and *EntityFramework*. 
- *Fixed:* `IEntityData` code-gen did not correctly output the value type.
- *Enhancement:* Added code-gen support for snake_case and kebab-case.

## v2.1.5
- *Fixed:* `InvokerBase` was non functioning as a generic class; reimplemented. Invoker invocation code generation updated accordingly.

## v2.1.4
- *Fixed:* FromBody not applied to `ServiceAgent` code generation.

## v2.1.3
- *Fixed:* `CodeGenConsoleWrapper` was supporting database generation by default.

## v2.1.2
- *Fixed:* Inconsistent version numbers.

## v2.1.1
- *New:* Initial publish to GitHub.