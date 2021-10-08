# Beef.Data.Database

[![NuGet version](https://badge.fury.io/nu/Beef.Data.Database.svg)](https://badge.fury.io/nu/Beef.Data.Database)

Adds additional capabilities extending ADO.NET ([`System.Data.Common`](https://docs.microsoft.com/en-us/dotnet/api/system.data.common)) that standardises and simplifies the [relational database](https://en.wikipedia.org/wiki/Relational_database) access for _Beef_.

*Note:* As this is extending ADO.NET only the primary interaction is via Stored Procedures, followed by SQL statements (secondary). LINQ-style capabilities are not supported, for this consider [Entity Framework Core](https://docs.microsoft.com/ef/core/), and the corresponding [Beef.Data.EntityFrameworkCore](../Beef.Data.EntityFrameworkCore/README.md).

*Note:* The database access is integrated into the code-generation configuration to simplify the creation of the mappers, arguments and the common data access scenarios.

*Note:* See [`Beef.Database.Core`](../../tools/Beef.Database.Core/README.md) for more information, and capabilities, around the management of a SQL Server Database specifically.

<br/>

## Database

To encapsulate the database access the [`DatabaseBase`](./DatabaseBase.cs) is inherited to enable.

The following demonstrates the usage:

```
public class Database : DatabaseBase
{
    public Database(string connectionString, DbProviderFactory provider = null) : base(connectionString, provider) { }
}
```

<br/>

### Session-Context 

Also, within Microsoft SQL Server 2016+ there is a [Session-Context](https://docs.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sp-set-session-context-transact-sql) capability that is leveraged by _Beef_. This enables variables to be set that are then available for the duration of a database connection to be accessed. The likes of `Username`, `Timestamp` and `TenantId` are great candidates for this capability. These can be set from the corresponding [`ExecutionContext`](../Beef.Core/ExecutionContext.cs) values using the `SetSqlSessionContext` method. This then reduces (removes) the need to pass these values to the database each time a Stored Procedure or SQL Statement is executed.

The following demonstrates the usage:

```
public class Database : Database<Database>
{
    public Database(string connectionString, DbProviderFactory provider = null) : base(connectionString, provider) { }

    public override void OnConnectionOpen(DbConnection dbConnection)
    {
        // Set the SQL Session Context when the connection is opened.
        SetSqlSessionContext(dbConnection);
    }
}
```

<br/>

## Mapping

A key feature is the mapping of a .NET entity to/from the database row and columns-based representation. The intention is to **avoid** mapping the database structure directly to an entity. The mapping will enable:
- Property to/from column mapping, including naming differences.
- Property to/from column data type conversions.
- Entity to/from one or more columns mappings.

Also, specific mappings can be configured to only be performed when performing a specific [operation type](../Beef.Core/Mapper/OperationTypes.cs); e.g. Create or Update, etc.

The following demonstrates the usage:

``` csharp
// Create a class to enable mapping:
public partial class DbMapper : DatabaseMapper<Person, DbMapper>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DbMapper"/> class.
    /// </summary>
    public DbMapper()
    {
        Property(s => s.Id, "PersonId").SetUniqueKey(true); // Maps Id <-> PersonId, and indicates is an auto-generated unique key.
        Property(s => s.FirstName); // Maps same name, FirstName <-> FirstName
        Property(s => s.LastName);
        Property(s => s.UniqueCode).SetOperationTypes(OperationTypes.AnyExceptCreate); // Map only when not doing a Create operation.
        Property(s => s.Gender, "GenderId").SetConverter(ReferenceDataNullableGuidIdConverter<RefDataNamespace.Gender>.Default); // Converts Gender RefData entity <-> GenderId.
        Property(s => s.EyeColorSid, "EyeColorCode");
        Property(s => s.Birthday);
        Property(s => s.Address).SetMapper(AddressData.DbMapper.Default); // Maps with another mapper changing shape.
        AddStandardProperties(); // Maps IChangeLog and IETag according to convention.
    }
}

// Fluent-style method-chaining equivalent:
var mapper = DatabaseMapper<Person>.Create()
    .HasProperty(s => s.Id, "PersonId", property: p => p.SetUniqueKey(true))
    .HasProperty(s => s.FirstName)
    .HasProperty(s => s.LastName)
    .HasProperty(s => s.UniqueCode, "UniqueCode", OperationTypes.AnyExceptCreate)
    .HasProperty(s => s.Gender, "GenderId", property: p => p.SetConverterReferenceDataNullableGuidIdConverter<RefDataNamespace.Gender>.Default))
    .HasProperty(s => s.EyeColor, "EyeColorCode")
    .HasProperty(s => s.Birthday)
    .HasProperty(s => s.Address, property: p => p.SetMapper(AddressData.DbMapper.Default))
    .Additional(m => m.AddStandardProperties());
```

</br>

## Operation arguments

The [`DatabaseArgs`](./DatabaseArgs.cs) provides the required database operation arguments:

Property | Description
-|-
`Mapper` | The mapper to perform the property to/from column mappings.
`StoredProcedure` | The name of the stored procedure to be executed.
`Paging` | The [paging](../Beef.Core/Entities/PagingResult.cs) configuration (used by **query** operation only).
`Refresh` | Indicates whether the data should be refreshed (reselected where applicable) after a **save** operation (defaults to `true`).

The following demonstrates the usage:

``` csharp
// Use the mapper to simplify creation.
var args1 = mapper.CreateArgs("[Demo].[spPersonCreate]");
var args2 = mapper.CreateArgs("[Demo].[spPersonGetAll]", paging);

// Create the arguments directly.
var args3 = new DatabaseArgs<Person>(mapper, "[Demo].[spPersonCreate]");
var args4 = new DatabaseArgs<Person>(mapper, "[Demo].[spPersonGetAll]", paging);
```

</br>

## CRUD

The primary data persistence activities are CRUD (Create, Read, Update and Delete) related; the [`DatabaseBase`](./DatabaseBase.cs) enables the following capabilities:

Operation | Description
-|-
`Get` | Executes the `StoredProcedure` and **gets** the entity for the specified key where found; otherwise, `null`. Sets the mapping operation type to `Get` and uses the `Mapper` to get the parameters that form the key, and then creates the entity instance mapping from the columns as configured.
`Create` | Executes the `StoredProcedure` and **creates** the entity. Sets the mapping operation type to `Create` and uses the `Mapper` to create the stored procedure parameters from the entity as configured; and rehydrate the corresponding result (where refreshing).
`Update` | Executes the `StoredProcedure` and **updates** the entity. Sets the mapping operation type to `Update` and uses the `Mapper` to create the stored procedure parameters from the entity as configured; and rehydrate the corresponding result (where refreshing).
`Delete` | Executes the `StoredProcedure` and **deletes** the entity. Sets the mapping operation type to `Delete` and uses the `Mapper` to get the parameters that form the key. Given a delete is idempotent it will be successful even where the entity does not exist.
`GetRefData` | Executes the `StoredProcedure` for a [**ReferenceData**](../../docs/Reference-Data.md) query updating the collection. There is also the [`DatabaseRefDataColumns`](./DatabaseRefDataColumns.cs) that contains the pre-configured reserved names of the standard refernce data columns. These can be changed where required.

<br/>

## Query

Query operations are enabled via the `DatabaseBase.Query` which will return a [`DatabaseQuery`](./DatabaseQuery.cs). This supports an optional `Action<DatabaseParameters>` to enable further parameters to be added prior to execution.

The `DatabaseQuery` enables the following LINQ-like operations:

Operation | Description
-|-
`SelectFirst` | Executes the `StoredProcedure` and **selects** the first item.
`SelectFirstOrDefault` | Executes the `StoredProcedure` and **selects** the first item or default.
`SelectSingle` | Executes the `StoredProcedure` and **selects** a single item.
`SelectSingleOrDefault` | Executes the `StoredProcedure` and **selects** a single item or default.
`SelectQuery` | Executes the `StoredProcedure` and **select** multiple items and either creates, or updates an existing, collection. Where the corresponding [`DatabaseArgs.Paging`](./DatabaseArgs.cs) is provided the configured paging, and optional get count, will be enacted.

The following demonstrates the usage:

``` csharp
var item = db.Query(mapper.CreateArgs("[Demo].[spPersonGetAll]"))
    .SelectFirstOrDefault();

var coll = db.Query(mapper.CreateArgs("[Demo].[spPersonGetAll]", paging),
    p => p.ParamWithWildcard(firstName, mapper["FirstName"]))
    .SelectQuery<PersonCollection>();
```

<br/>

## Command

The [`DatabaseCommand`](./DatabaseCommand.cs) is provided to encapsulate the logic of creating and interacting with an ADO.NET [`DbCommand`](https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbcommand). This simplifies the creation, as well as simplifies the access to the underlying `Parameters`, and corresponding execution - further integrating with the mapping to simplify .NET entity to/from database row and columns-based representation. This also includes support for fluent-style method chaining.

This enables the following:

Operation | Description
-|-
`Param` | Adds a named parameter and value.
`Params` | Add one or more parameters by invoking a delegate.
`RowVersionParam` | Adds a named `RowVersion` parameter.
`ReturnValueParam` | Adds a `ReturnValue` parameters.
`ChangeLogParams` | Adds the [`ChangeLog`](../Beef.Core/Entities/ChangeLog.cs) parameters.
`PagingParams` | Adds the [`PagingArgs`](../Beef.Core/Entities/PagingArgs.cs) parameters.
`TableValuedParam` | Adds a **SQL Server** [`TableValuedParameter`](./TableValuedParameter.cs).
`ReselectRecordParam` | Adds a `ReselectRecord` parameter.
`SelectFirst` | Selects the first item.
`SelectFirstOrDefault` | Selects the first item or default.
`SelectSingle` | Selects a single item.
`SelectSingleOrDefault` | Selects a single item or default.
`SelectQuery` | Select multiple items and either creates, or updates an existing, collection. Where the corresponding [`DatabaseArgs.Paging`](./DatabaseArgs.cs) is provided the configured paging, and optional get count, will be enacted.
`SelectQueryMultiSet` | Executes a multi-dataset query command where an array of delegates are passed to process each returned dataset. A `null` dataset indicates to ignore (skip) the dataset at the specified position. The [`MultiSetSingleArgs`](./MultiSetArgs.cs) or [`MultiSetCollArgs`](./MultiSetArgs.cs) enable per dataset configuration to simplify usage and validate dataset expectations.
`NonQuery` | Executes a non-query command.
`Scalar` | Executes the query and returns the first column of the first row in the result set returned by the query.

The following demonstrates the usage:

``` csharp
// Delete a record using a NonQuery.
Database.Default.StoredProcedure("[Demo].[spPersonDelete]")
    .Param(DbMapper.Default.GetParamName(nameof(PersonDetail.Id)), id)
    .NonQuery();

// Select with MultiSet.
Database.Default.StoredProcedure("[Demo].[spPersonGetDetail]")
    .Param(DbMapper.Default.GetParamName(nameof(PersonDetail.Id)), id)
    .SelectQueryMultiSet(
        new MultiSetSingleArgs<Person>(PersonData.DbMapper.Default, (r) => { pd = new PersonDetail(); pd.CopyFrom(r); }, isMandatory: false),
        new MultiSetCollArgs<WorkHistoryCollection, WorkHistory>(WorkHistoryData.DbMapper.Default, (r) => pd.History = r));

// Update with Reselect containing a MultiSet.
Database.Default.StoredProcedure("[Demo].[spPersonUpdateDetail]")
    .Params((p) => PersonData.DbMapper.Default.MapToDb(value, p, Mapper.OperationTypes.Update))
    .TableValuedParam("@WorkHistoryList", WorkHistoryData.DbMapper.Default.CreateTableValuedParameter(value.History))
    .ReselectRecordParam()
    .SelectQueryMultiSet(
        new MultiSetSingleArgs<Person>(PersonData.DbMapper.Default, (r) => { pd = new PersonDetail(); pd.CopyFrom(r); }, false, true),
        new MultiSetCollArgs<WorkHistoryCollection, WorkHistory>(WorkHistoryData.DbMapper.Default, (r) => pd.History = r));
```

<br/>

## Parameters

The [`DatabaseParameters`](./DatabaseParameters.cs) is provided to encapsulate the [`DbParameterCollection`](https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbparametercollection) logic.

This contains the actual **Parameter** related operations (implementation) described in the [**Command**](#Command) section above.

There is also the [`DatabaseColumns`](./DatabaseColumns.cs) that contains the pre-configured reserved names of the special columns. These can be changed where required.

<br/>

## Wildcards

The [`DatabaseWildcard`](./DatabaseWildcard.cs) is provided to simplify the replacings of the well known wildcards (`*` and `?`) with the SQL-equivalent (`%` and `_`). This also includes the escaping of other characters to ensure the SQL [`LIKE`](https://docs.microsoft.com/en-us/sql/t-sql/language-elements/like-transact-sql) statement will function correctly. This can be re-configured as required.

Additionally, the [Parameters](#Parameters) contain a `ParamWithWildcard` operation that will add a `Parameter` with wildcard text and replace as per the `DatabaseWildcard` configuration.

<br/>

## Connection management

The _Beef_ framework encapsulates the ADO.NET [`DbCommand`](https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbcommand) and manages its lifetime. As such a developer need not concern themselves with opening and closing the ADO.NET [`DbConnection`](https://docs.microsoft.com/en-us/dotnet/api/system.data.common.dbconnection) (and disposing).

<br/>

## Transactions

The [`ManagerInvoker`](../Beef.Core/Business/ManagerInvoker.cs), [`DataSvcInvoker`](../Beef.Core/Business/DataSvcInvoker.cs), and [`DataInvoker`](../Beef.Core/Business/DataInvoker.cs) accept a [`BusinessInvokerArgs`](../Beef.Core/Business/BusinessInvokerBase.cs). This also drives the connection management.

These classes are used specifically by the primary domain _business_ logic (see [`Solution Structure`](../../docs/solution-structure.md)).