# Beef.Data.Database.Cdc

[![NuGet version](https://badge.fury.io/nu/Beef.Data.Database.Cdc.svg)](https://badge.fury.io/nu/Beef.Data.Database.Cdc)

Adds Microsoft SQL Server [Change Data Capture](https://docs.microsoft.com/en-us/sql/relational-databases/track-changes/about-change-data-capture-sql-server) (CDC) capabilities. Specifically the ability to monitor changes to one or more related tables, that represent an _entity_, to be published to an event stream.

Similar to core _Beef_, the CDC capability is enabled by both code-generation and supporting framework to enable.

<br/>

## Additional reading

This [article](https://www.mssqltips.com/sqlservertip/5212/sql-server-temporal-tables-vs-change-data-capture-vs-change-tracking--part-2/) provides an excellent overview of CDC and walks through the process of setting up and using to aid in the fundamental understanding.

<br/>

## Approach

The CDC approach taken here is to consolidate the tracking of individual tables (one or more) into a central entity to simplify the publishing to an event stream (or equivalent). The advantage of this is where a change occurs to any of the rows related to an entity, even where multiples rows are updated, this will only result in a single event. This makes it easier (more logical) for downstream subscribers to consume.

This is achieved by defining (configuring) the entity, being the primary (parent) table, and it's related secondary (child) tables. For example, a Sales Order, may be made up multiple tables - when any of these change then a single _SalesOrder_ event should occur. These relationships are also defined with a cardinality of either `OneToMany` or `OneToOne`.

```
SalesOrder             // Parent
└── SalesOrderAddress  // Child 1:n - One or more addresses (e.g. Billing and Shipping)
└── SalesOrderItem     // Child 1:n - One or more items
``` 

The CDC capability is used specifically as a trigger for change (being `Create`, `Update` or `Delete`). The resulting data that is published is the latest, not a snapshot in time (CDC captured). The reason for this is two-fold, a) given how the CDC data is retrieved there is no guarantee that the interim data represents a final intended state suitable for publishing; and b) this process should be running near real-time so getting the latest version will produce the current committed version as at that time.

To further guarantee only a single event for a specific version is published the resulting entity is JSON serialized and hashed; this value is checked (and saved) against the prior version to ensure a publish contains data that is actionable. This will minimize redundant publishing, whilst also making the underlying processing more efficient.

<br/>

## Set up

The first activity is to enable CDC on the database and then enable on each of the tables; using the SQL Server system (native) stored procedures:
- [`sys.sp_cdc_enable_db`](https://docs.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sys-sp-cdc-enable-db-transact-sql) - enables the database.
- [`sys.sp_cdc_enable_table`](https://docs.microsoft.com/en-us/sql/relational-databases/system-stored-procedures/sys-sp-cdc-enable-table-transact-sql) - enables the table. Please note that  `@supports_net_changes` **must** be selected (set to `1`).

An example is as follows.

``` sql
-- Enable for the database.
EXEC sp_changedbowner 'sa'
EXEC sys.sp_cdc_enable_db

-- Enable for the seleted table.
EXEC sys.sp_cdc_enable_table  
  @source_schema = N'SchemaName',  
  @source_name   = N'TableName',  
  @role_name     = null,
  @supports_net_changes = 1
```

Where using [Migration](./../../tools/Beef.Database.Core/README.md) Scripts [code-generation](#Code-generation) can be leveraged to accelerate. The following command line executions will create the migration scripts that contain the contents described above.

```
-- Create sys.sp_cdc_enable_db script.
dotnet run scriptnew cdcdb

-- Create sys.sp_cdc_enable_table script; replace with actual Schema and Table names.
dotnet run scriptnew cdc SchemaName TableName
```

<br/>

## Code-generation

The remainder of the database objects required to support CDC event publishing is generated using [database code-generation](../../tools/Beef.Database.Core/README.md); as is the corresponding .NET code to process. This is achieved using the CDC-related configuration.

The code-generation is applicable whether using a Data-tier application (DAC) or DbUp to manage the database.

<br/>

### Configuration

This CDC-related configuration is as follows.

```
CodeGeneration
└── Cdc(s)
  └── CdcJoin(s)
    └── CdcJoinOn(s)
```

Configuration details for each of the above are as follows:
- CodeGeneration - [YAML/JSON](../../docs/Database-CodeGeneration-Config.md) or [XML](../../docs/Database-CodeGeneration-Config-Xml.md)
- Cdc - [YAML/JSON](../../docs/Database-Cdc-Config.md) or [XML](../../docs/Database-Cdc-Config-Xml.md)
- CdcJoin - [YAML/JSON](../../docs/Database-CdcJoin-Config.md) or [XML](../../docs/Database-CdcJoin-Config-Xml.md)
- CdcJoinOn - [YAML/JSON](../../docs/Database-CdcJoinOn-Config.md) or [XML](../../docs/Database-CdcJoinOn-Config-Xml.md)

The following represents an [example](../../samples/Demo/Beef.Demo.Database/Beef.Demo.Database.xml) of the XML configuration.

``` xml
<! -- Define default CdcSchema, and default Event configuration. -->
<CodeGeneration CdcSchema="DemoCdc" EventSubjectRoot="Legacy" EventActionFormat="PastTense" ... >

  <!-- Set up CDC for primary table Legacy.Contact. -->
  <Cdc Name="Contact" Schema="Legacy">
    <!-- Set up secondary One-To-One relationship from Legacy.Contact to Legacy.Address (1:1).
         Join on Address.Id = Contact.AddressId. -->
    <CdcJoin Name="Address" JoinCardinality="OneToOne">
      <CdcJoinOn Name="Id" ToColumn="AddressId" />
    </CdcJoin>
    
    <!-- Set up inner join relationship from Legacy.Contact to Legacy.ContactMapping.
         Inner Join (only outputs where exists) on ContactMapping.ContactId = Contact.ContactId. 
         Only include the joined 'UniqueId' column. -->
    <CdcJoin Name="ContactMapping" Type="Inner" IncludeColumns="UniqueId">
      <CdcJoinOn Name="ContactId" />
    </CdcJoin>
  </Cdc>

  <!-- Set up CDC for primary table Legacy.Posts. 
       Relational hierarchy: Legacy.Posts
                              - Legacy.Comments (1:n)
                                 - Legacy.Tags (1:n)
                              - Legacy.Tags (1:n). -->
  <Cdc Name="Posts" Schema="Legacy">
    <!-- Set up secondary One-To-Many relationship from Legacy.Posts to Legacy.Comments (1:n).
         Join on Comments.PostsId = Posts.PostsId. -->
    <CdcJoin Name="Comments" Schema="Legacy" JoinTo="Posts">
      <CdcJoinOn Name="PostsId" ToColumn="PostsId" />
    </CdcJoin>
    
    <!-- Set up secondary One-To-Many relationship from Legacy.Comments to Legacy.Tags (1:n).
         Name as 'CommentsTags' for uniqueness for join referencing.
         Exclude the 'ParentType' column as not necessary for publishing (i.e. internal to database).
         Rename 'ParentId' column to `CommentsId`.
         Join on Tags.ParentType = 'C' AND Tags.ParentId = Comments.CommentsId. -->
    <CdcJoin Name="CommentsTags" Schema="Legacy" TableName="Tags" JoinTo="Comments" ExcludeColumns="ParentType" AliasColumns="ParentId^CommentsId">
      <CdcJoinOn Name="ParentType" ToStatement="'C'" />
      <CdcJoinOn Name="ParentId" ToColumn="CommentsId" />
    </CdcJoin>
    
    <!-- Set up secondary One-To-Many relationship from Legacy.Posts to Legacy.Tags (1:n).
         Name as 'PostsTags' for uniqueness for join referencing.
         Exclude the 'ParentType' column as not necessary for publishing (i.e. internal to database).
         Rename 'ParentId' column to `PostsId`.
         Join on Tags.ParentType = 'P' AND Tags.ParentId = Posts.PostsId. -->
    <CdcJoin Name="PostsTags" Schema="Legacy" TableName="Tags" JoinTo="Posts" ExcludeColumns="ParentType" AliasColumns="ParentId^PostsId">
      <CdcJoinOn Name="ParentType" ToStatement="'P'" />
      <CdcJoinOn Name="ParentId" ToColumn="PostsId" />
    </CdcJoin>
  </Cdc>
```

<br/>

### Database code generation

The underlying [`Program.cs`](./../../samples/Demo/Beef.Demo.Database/Program.cs) should be updated to override the `DatabaseScript` depending on whether using a Data-tier application (DAC) or DbUp to manage the database. Where using DbUp the required database tables will need to be added as Migration scripts explicitly, versus being generated automatically.

``` csharp
// Using DbUp; use: DatabaseWithCdc.xml
return DatabaseConsoleWrapper
    .Create("Data Source=.;Initial Catalog=Beef.Demo;Integrated security=True", "Beef", "Demo")
    .DatabaseScript("DatabaseWithCdc.xml")
    .RunAsync(args);

// Using DACPAC; use: DatabaseWithCdcDacpac.xml
return DatabaseConsoleWrapper
    .Create("Data Source=.;Initial Catalog=Beef.Demo;Integrated security=True", "Beef", "Demo")
    .DatabaseScript("DatabaseWithCdcDacpac.xml")
    .RunAsync(args);
```

Where _only_ looking to leverage CDC capabilities, and not the other _Beef_ related database artefacts, then substitute the `DatabaseScript` with the following.

- `DatabaseScript("DatabaseWithCdc.xml")` -> `DatabaseScript("DatabaseCdc.xml")`.
- `DatabaseScript("DatabaseWithCdcDacpac.xml")` -> `DatabaseScript("DatabaseCdcDacpac.xml")`.

<br/>

### Database generated artefacts

The following database artefacts are generated.

Type | Name | Description
-|-|-
`Table` | `CdcTracking.sql` | Represents the related _Entity Hash_ tracking table used to identify whether a version of a specific entity has been previously (successfully) processed. See [example](../../samples/Demo/Beef.Demo.Database/Migrations/20210111-163724-create-democdc-cdctracking.sql).
`Table` | `XxxOutbox.sql` | Represents the _Entity_ outbox table used to track the log sequence number (LSN) for the primary and secondary tables. This acts as a pointer of where the processing is at in relation to each table to aid both reprocessing, and to determine where to begin processing of next outbox set. An outbox set is essentially just a batch of one or more entities for processing. See [example](../../samples/Demo/Beef.Demo.Database/Migrations/20210111-163747-create-democdc-postsoutbox.sql).
`Type` | `udtTrackingList.sql` | Represents the user-defined type / table-valued parameter required to pass a list of key/hash values from .NET code to a SQL Stored Procedure. See [example](../../samples/Demo/Beef.Demo.Database/Schema/DemoCdc/Types/User-Defined%20Table%20Types/Generated/UdtCdcTrackingList.sql).
`Stored Procedure` | `spExecuteXxxCdcOutbox.sql` | Represents the **key** CDC-related logic. This stored procedure is responsible for getting the next outbox set for an _Entity_, retrying an existing outbox set, and completing an existing outbox set. See [example](../../samples/Demo/Beef.Demo.Database/Schema/DemoCdc/Stored%20Procedures/Generated/spExecuteContactCdcOutbox.sql).

_Tip:_ If any of the generated files are not automatically added to the Visual Studio Project structure, the _Show All Files_ in the _Solution Explorer_ can be used to view, and then individually added using _Include In Project_.

As [stated](#Database-code-generation) earlier, where using DbUp the [Migration](./../../tools/Beef.Database.Core/README.md) Scripts [code-generation](#Code-generation) must be explicitly executed. The following represents the command line executions required.

```
-- Create the CdcTracking.sql
dotnet run codegen --script DatabaseCdcTracking.xml

-- Create (each) `XxxOutbox.sql` by specifying the unique CdcName as configured.
dotnet run codegen --script DatabaseCdcOutbox.xml --param CdcName=Contact
dotnet run codegen --script DatabaseCdcOutbox.xml --param CdcName=Posts
```

<br/>

### .NET generated artefacts

The following .NET aretfacts are generated. These artefacts are generated into a .NET Project named `Company.AppName.Cdc`; where `Company` and `AppName` are the configurable values. 

Type | Name | Description
-|-|-
`Entity` | `XxxCdc.cs` | Represents the database tables as .NET entities (classes), where the columns map to properties, and are marked up for JSON serialization. The classes also implement [`IUniqueKey`](../Beef.Core/Entities/IUniqueKey.cs) and [`IETag`](../Beef.Core/Entities/IETag.cs) where applicable so that other _Beef_ related capabilities can be leveraged. See [example](../../samples/Demo/Beef.Demo.Cdc/Entities/Generated/ContactCdc.cs). 
`Data` | `XxxCdcData.cs` | Represents the key data and event orchestration for an _Entity_. Inherits the base capabilities from [`CdcDataOrchestrator`](../Beef.Data.Database.Cdc/CdcDataOrchestrator.cs). This is largely responsible for implementing the data reader into the resulting _Entity_, creating the corresponding [`EventData`](../Beef.Core/Events/EventData.cs) (can be overridden), and invoking the [`IEventPublisher`](../Beef.Core/Events/IEventPublisher.cs) to publish (this can be implemented to publish to any streaming/messaging capability as required). See [example](../../samples/Demo/Beef.Demo.Cdc/Data/Generated/ContactCdcData.cs).
`Data` | `ServiceCollectionsExtension.cs` | Represents the addition of the scoped services ([IServiceCollection](https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection)) for the aforementioned `XxxCdcData` classes. See [example](../../samples/Demo/Beef.Demo.Cdc/Data/Generated/ServiceCollectionsExtension.cs).
`Service` | `XxxCdcBackgroundService.cs` | Represents the .NET background service for CDC-related processing. Inherits the base capabailities from [`CdcBackgroundService`](../Beef.Data.Database.Cdc/CdcBackgroundService.cs) which is an [`IHostedService`](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services) implementation with a timer-based processing trigger. See [example](../../samples/Demo/Beef.Demo.Cdc/Services/Generated/ContactCdcBackgroundService.cs).


