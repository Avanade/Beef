# Change Data Capture (CDC)

The purpose of this sample is to demonstrate the usage of _Beef_ CDC on an existing database. In this scenario, we intend to leverage CDC to monitor table changes and send a corresponding event to Azure Service Bus so a downstream system can process.

The following will be covered in this sample:
- [Scope](#Scope)
- [Legacy database](#Legacy-database)
- [Solution structure](#Solution-structure)
- [Xyz.Legacy.CdcDatabase project](#Xyz.Legacy.CdcDatabase)
- [Xyz.Legacy.CdcPublisher project](#Xyz.Legacy.CdcPublisher)
- [Xyz.Legacy.CdcCodeGen project](#Xyz.Legacy.CdcCodeGen)
- [Database artefact publish](#Database-artefact-publish)
- [Execute the publisher](#Execute-the-publisher)

Where required (optional) a sample event/message receiver is also covered:
- [Xyz.Legacy.CdcReceiver project](#Xyz.Legacy.CdcReceiver)

Enjoy!

<br/>

## Scope

A fictitious database is required to act as what is termed the legacy source. For this sample, the database is comprised of the following tables (within the `Legacy` schema). 

- `Person` - contains the Person information.
- `PersonAddress` - contains one-or-more Addresses for a Person.
- `AddressType` - is reference data describing the type of Address.

The key requirement is to publish an _event message_ whenever there is a change that occurs to the `Person`, and/or their `PersonAddress` collection. To simplify the messaging, a _message_ should encapsulate both the `Person` and their `PersonAddress` collection as a `Person` _entity_, versus sending messages for each individually - as it is the composite _entity_ that represents a **Person** in this scenario (regardless of the underlying database structure).

<br/>

## Legacy database

A SQL Server [`CreateDb`](./CreateDb.sql) script is provided to create this fictitious database, with the aforementioned tables, and their initial data rows. This script will create a database `XyzLegacy` that will be used for the remainder of this sample. Please execute this script using the database tool of your choice; see [`sqlcmd`](https://docs.microsoft.com/en-us/sql/ssms/scripting/sqlcmd-run-transact-sql-script-files) as an option.

<br/>

## Solution structure

The underlying solution project structure will be as follows.

```
└── <root>
  └── Company.AppName.CdcCodeGen    # Code-gen console
  └── Company.AppName.CdcDatabase   # Database DACPAC
  └── Company.AppName.CdcPublisher  # Hosted service to publish events
  └── Company.AppName.CdcReceiver   # Azure function to receive and process events (Optional)
```

For the purposes of this sample the `Company` is `Xyz` and the `AppName` is `Legacy`. Note the types of projects that will be used in parenthesis.

```
└── <root>
  └── Xyz.Legacy.CdcCodeGen    # C# Console application
  └── Xyz.Legacy.CdcDatabase   # SQL Server Database Project (DACPAC)
  └── Xyz.Legacy.CdcPublisher  # C# Console application
  └── Xyz.Legacy.CdcReceiver   # C# Azure Function
```

</br>

## Xyz.Legacy.CdcDatabase

Create new using the `SQL Server Database Project` template within Visual Studio and name `Xyz.Legacy.CdcDatabase`. Once created, goto the _Project Properties_ page and confirm/update the _Target Platform_ - make sure this matches the version of SQL Server being used. The remainder of the properies etc. can largely remain as-is.

The key steps that need to be performed are:
- [Enable CDC](#Enable-Cdc) for the database and required tables.
- [CDC Schema](#Cdc-Schema) to house the CDC related artefacts.

The remainder of the CDC related artefacts will be created and updated using [code-generation](#Xyz.Legacy.CdcCodeGen).

<br/>

### Enable CDC

Before CDC can be leveraged it first needs to be set up in the database. First step is to turn it on for the database, then set it up for each of the tables that are to be monitored, being `Person` and `PersonAddress`. The `AddressType` table does not require CDC as it will be referenced only; i.e. no monitoring of changes to the data is needed.

To perform, a new SQL script will need to be created. Within Visual Studio, add _Script..._ using the _Pre-deployment Script_ template, and rename as [`Script.PreDeployment.CdcSetup.sql`](./Xyz.Legacy.CdcDatabase/Script.PreDeployment.CdcSetup.sql). The contents should be as follows.

``` sql
-- Enable for the database.
IF (SELECT TOP 1 is_cdc_enabled FROM sys.databases WHERE [name] = N'XyzLegacy' = 0
BEGIN
  EXEC sp_changedbowner 'sa'
  EXEC sys.sp_cdc_enable_db
END

-- Enable for the Legacy.Person table.
IF (SELECT TOP 1 is_tracked_by_cdc FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'Legacy.Person')) = 0
BEGIN
  EXEC sys.sp_cdc_enable_table  
    @source_schema = N'Legacy',  
    @source_name = N'Person',  
    @role_name = NULL,
    @supports_net_changes = 0
END

-- Enable for the Legacy.PersonAddress table.
IF (SELECT TOP 1 is_tracked_by_cdc FROM sys.tables WHERE [OBJECT_ID] = OBJECT_ID(N'Legacy.PersonAddress')) = 0
BEGIN
  EXEC sys.sp_cdc_enable_table  
    @source_schema = N'Legacy',  
    @source_name = N'PersonAddress',  
    @role_name = NULL,
    @supports_net_changes = 0
END
```

<br/>

### CDC Schema

To separate the new CDC related artefacts from the existing it is recommended that a new database schema is created to house. This will be named `XCdc` as unfortunately `Cdc` is largely reserved within SQL. Within Visual Studio, add _Script..._ using the _Script (Build)_ template, and rename as [`Schema.XCdc.sql`](./Xyz.Legacy.CdcDatabase/Schema.XCdc.sql). The contents should be as follows.

``` sql
CREATE SCHEMA [XCdc]
```

</br>

## Xyz.Legacy.CdcPublisher

Create new using the _C# Console Application_ template within Visual Studio and name `Xyz.Legacy.CdcPublisher`. Once created, add the [`Beef.Data.Database.Cdc`](../../src/Beef.Data.Database.Cdc) and [`Beef.Events.ServiceBus`](../../src/Beef.Events.ServiceBus) NuGet packages to the project. Replace the contents of `Program.cs` with the following.

``` csharp
using Azure.Messaging.ServiceBus;
using Beef;
using Beef.Data.Database;
using Beef.Entities;
using Beef.Events.ServiceBus;
using Xyz.Legacy.CdcPublisher.Data;
using Xyz.Legacy.CdcPublisher.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Xyz.Legacy.CdcPublisher
{
    class Program
    {
        static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    // Services configuration as follows:
                    // - .AddBeefExecutionContext - enables the `ExecutionContext` required internally by _Beef_.
                    // - .AddBeefDatabaseServices - adds the `IDatabase` instance for the CDC database.
                    // - .AddSingleton<IStringIdentifierGenerator>` - used to create Global Identifiers where required; default is a GUID.
                    // - .AddGeneratedCdcDataServices` - adds the generated CDC data services; these are the primary CDC data orchestrators.
                    // - .AddGeneratedCdcHostedServices` - adds the generated CDC hosted services; enables background-style execution.

                    services.AddBeefExecutionContext();
                    services.AddBeefDatabaseServices(() => new Database(hostContext.Configuration.GetValue<string>("CdcDb")));
                    services.AddSingleton<IStringIdentifierGenerator>(new StringIdentifierGenerator());
                    services.AddGeneratedCdcDataServices();
                    services.AddGeneratedCdcHostedServices(hostContext.Configuration);

                    // - .AddBeefLoggerEventPublisher - adds an `ILogger` implementation to output the published events versus actually sending (used for debugging only).
                    // - .AddBeefServiceBusSender - adds the capability to publish Azure Service Bus messages (as [CloudEvents](https://cloudevents.io/)).
                    services.AddBeefLoggerEventPublisher();
                    //var sbc = new ServiceBusClient(hostContext.Configuration.GetValue<string>("ServiceBus"));
                    //services.AddBeefServiceBusSender(sbc);
                });
    }
}
```

At this point it will look like there are code errors (i.e. it will not compile). This is expected as the `Database.cs` is created next, and the remainder of the c# classes will be created and updated using [code-generation](#Xyz.Legacy.CdcCodeGen).

</br>

### Database.cs

To enable database access, a _Beef_ [IDatabase](../../src/Beef.Data.Database/IDatabase.cs) instance is required. This functionality is enabled via the [DatabaseBase](../../src/Beef.Data.Database/DatabaseBase.cs).

First, create a new `Data` folder in the project and add a `Database.cs` file. The contents should be as follows.

``` csharp
using Beef.Data.Database;
using System.Data.Common;

namespace Xyz.Legacy.CdcPublisher.Data
{
    /// <summary>
    /// Represents the <b>Beef</b>-enabled database.
    /// </summary>
    public class Database : DatabaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Database"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="provider">The optional data provider.</param>
        public Database(string connectionString, DbProviderFactory provider = null) : base(connectionString, provider, new SqlRetryDatabaseInvoker()) { }
    }
}
```

</br>

## Xyz.Legacy.CdcCodeGen

Create new using the _C# Console Application_ template within Visual Studio and name `Xyz.Legacy.CdcCodeGen`. Once created, add the [`Beef.CodeGen.Core`](../../src/Beef.CodeGen.Core) NuGet package to the project.

The key steps that need to be performed are:
- [Program.cs](#Program.cs) implements the code to execute the _Beef_ code generator.
- [YAML configuration](#YAML-configuration) to house the CDC related artefacts.
- [Execute code-gen](#Execute-code-gen) to output the CDC related artefacts.

<br/>

### Program.cs

The `CodeGenConsoleWrapper` provides a simplified means to encapsulate the execution of the unrderlying code generator. Replace the contents of `Program.cs` with the following.

``` csharp
using Beef.CodeGen;
using System.Threading.Tasks;

namespace Xyz.Legacy.CdcCodeGen
{
    class Program
    {
        // To run execute command line: dotnet run database
        static Task<int> Main(string[] args) => CodeGenConsoleWrapper
            // Code generation configuration as follows:
            // - Create - creates the code-generator instance and sets the `Company` and `AppName` parameters.
            // - Supports - turns off the default `entity` support, and turns on `database` code-gen support only.
            // - DatabaseScript - configures the code-gen to use the `DatabaseCdcDacpac.xml` that drives the code-gen templates to be used; this is a CDC-only script designed for DACPAC output.
            // - DatabaseConnectionString - defaults the database connection string; will be overridden by command line arguments '-cs|--connectionString' or environment variable: Xyz_Legacy_ConnectionString
            .Create("Xyz", "Legacy")
            .Supports(entity: false, database: true)
            .DatabaseScript("DatabaseCdcDacpac.xml")
            .DatabaseConnectionString("Data Source=.;Initial Catalog=XyzLegacy;Integrated Security=True")
            .RunAsync(args);
    }
}
```

</br>

### YAML configuration

The YAML configuration provides the input that drives the code-generation. Create a new `database.beef.yaml` file. If you use an editor, such as Visual Studio Code, that supports YAML and related JSON schemas this will automatically support intellisense. Unfortunately, Visual Studio 2019 or lower does not.

Replace the contents of `database.beef.yaml` with the following. The comments have been added to aid the reader and are not neccessary for the code generation to function; as such they can be removed.

``` yaml
# CDC global configuration
schema: Legacy                                 # Defaults the schema for the non-CDC related artefacts.
cdcSchema: XCdc                                # Specifies the schema for all the CDC related artefacts.
cdcIdentifierMapping: true                     # Indicates to include the generation of the global identifier mapping artefacts.
cdcExcludeColumnsFromETag: [ rowversion ]      # Default list of columns to exclude from the generated ETag (duplicate send tracking).
pathDatabaseSchema: Xyz.Legacy.CdcDatabase     # Path (directory) for the database-related artefacts (relative to parent).
hasBeefDbo: false                              # Indicates that the database does not contain the standard _Beef_ dbo schema artefacts.
eventSubjectFormat: NameOnly                   # Event subject should include name only; not append the key.
eventSubjectRoot: Xyz.Legacy                   # Event subject root prepended to all published events.
eventActionFormat: PastTense                   # Event action should be formatted in the past tense.
eventSourceRoot: /legacy_db                    # Event source URI root prepended to all published events.
eventSourceKind: Relative                      # Event source URI is relative versus absolute.
cdc:                                           # Zero or more CDC entities...

# CDC entity configuration for primary table 'Legacy.Person':
# - name - the name of the table (uses default schema above).
# - identifierMapping - indicates that a global identifier is to be mapped/assigned to the primary key.
# - excludeColumns - lists the columns that should not be part of the entity published.
# - joins - zero or more joins to be included within the entity.
- { name: Person, identifierMapping: true, excludeColumns: [ SapId, RowVersion ],
    joins: [

      # CDC-related join for secondary table 'Legacy.PersonAddress':
      # - name - the name of the table (uses default schema above).
      # - propertyName - the .NET property name so it feels more natural within the entity.
      # - excludeColumns - lists the columns that should not be part of the entity published.
      # - on - List the column(s) that should be used for the join:
      #   - name - the name of the column from this join table to use for the join; 'toColumn' defaults where not explicitly specified.
      { name: PersonAddress, propertyName: Addresses, excludeColumns: [ RowVersion ], on: [ { name: PersonId } ] },

      # Non CDC-related join for secondary table 'Legacy.AddressType':
      # - name - the name of the table (uses default schema above).
      # - type - specifies the type of join, in this case Left Outer.
      # - joinTo - specifies the table name (previously defined) to join to; defaults to primarty table unless explicitly specified.
      # - includeColumns - list the columns that should be included in the entity published (versus 'excludeColumns').
      # - on - List the column(s) that should be used for the join:
      #   - name - the name of the column from this join table to use for the join; 'toColumn' defaults where not explicitly specified.
      { name: AddressType, type: Left, joinTo: Person_Address, includeColumns: [ Code ], on: [ { name: AddressTypeId } ] }
    ]
  }
```

There are many additional options within the YAML to further fine-tune the CDC code-gen; see the documentation as follows.

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


<br/>

### Execute code-gen

Once the YAML configuration is complete the code-gen can be executed. As the _Beef_ code-gen is _gen-many_ the configuration can continue to be maintained and the corresponding code-gen executed as many times are required over the lifetime of the code base. For example, if there is a schema change to add a new column, then executing again will automatically pick that column up for inclusion (unless excluded).

To execute using the debugger, open the project properties, and then navigate to the _Debug_ tab. Set the _Application arguments_ as follows. Then run the application.

```
database --connectionString "Data Source=.;Initial Catalog=XyzLegacy;Integrated Security=True"
```

Alternatively, the application can be run directly from the command line as follows:

```
dotnet run database --connectionString "Data Source=.;Initial Catalog=XyzLegacy;Integrated Security=True"

-- Otherwise, set an Environment Variable named 'Xyz_Legacy_ConnectionString' to the connection string value and run:
dotnet run database
```

The output from the code generator should look similar as follows. This provides an audit of all the templates that were executed, the state of the artefacts (either created, updated or unchanged), plus some basic runtime statistics. If there were any errors with the configuration an appropriate error message will be output and the code-gen will not occur.

```
╔╗ ┌─┐┌─┐┌─┐  ╔═╗┌─┐┌┬┐┌─┐  ╔═╗┌─┐┌┐┌  ╔╦╗┌─┐┌─┐┬
╠╩╗├┤ ├┤ ├┤   ║  │ │ ││├┤───║ ╦├┤ │││   ║ │ ││ ││
╚═╝└─┘└─┘└    ╚═╝└─┘─┴┘└─┘  ╚═╝└─┘┘└┘   ╩ └─┘└─┘┴─┘

Business Entity Execution Framework (Beef) Code Generator.

  Config = database.beef.yaml
  Script = DatabaseCdcDacpac.xml
  Template =
  Output = C:\Users\eric\source\repos\Beef\samples\Cdc
  ExpectNoChange = False
  Params:
    Company = Xyz
    AppName = Legacy
    AppDir = Legacy
    ConnectionString = Data Source=.;Initial Catalog=XyzLegacy;Integrated Security=True


  Querying database to infer table(s)/column(s) configuration...
    Database query complete [1022ms]

  Template: DbCdcTrackingTvp_cs.hb (DatabaseCdcRootCodeGenerator: Cdc/Data)
    Created -> \Xyz.Legacy.CdcPublisher\Data\Generated\CdcTrackingDbMapper.cs
   [Files: Unchanged = 0, Updated = 0, Created = 1]
  Template: DbCdcEntity_cs.hb (DatabaseCdcCodeGenerator: Cdc/Entities)
    Created -> \Xyz.Legacy.CdcPublisher\Entities\Generated\PersonCdc.cs
   [Files: Unchanged = 0, Updated = 0, Created = 1]
  Template: DbCdcData_cs.hb (DatabaseCdcCodeGenerator: Cdc/Data)
    Created -> \Xyz.Legacy.CdcPublisher\Data\Generated\PersonCdcData.cs
   [Files: Unchanged = 0, Updated = 0, Created = 1]
  Template: DbCdcHostedService_cs.hb (DatabaseCdcHostedServiceCodeGenerator: Cdc/Services)
    Created -> \Xyz.Legacy.CdcPublisher\Services\Generated\PersonCdcHostedService.cs
   [Files: Unchanged = 0, Updated = 0, Created = 1]
  Template: DbCdcDataSce_cs.hb (DatabaseCdcRootCodeGenerator: Cdc/Data)
    Created -> \Xyz.Legacy.CdcPublisher\Data\Generated\ServiceCollectionsExtension.cs
   [Files: Unchanged = 0, Updated = 0, Created = 1]
  Template: DbCdcHostedExtensions_cs.hb (DatabaseCdcRootCodeGenerator: Cdc/Services)
    Created -> \Xyz.Legacy.CdcPublisher\Services\Generated\CdcHostedServiceExtensions.cs
   [Files: Unchanged = 0, Updated = 0, Created = 1]
  Template: DbCdcTrackingUdt_sql.hb (DatabaseCdcRootCodeGenerator: Database/Schema/Xxx/Types/User-Defined Table Types)
    Created -> \Xyz.Legacy.CdcDatabase\XCdc\Types\User-Defined Table Types\Generated\udtCdcTrackingList.sql
   [Files: Unchanged = 0, Updated = 0, Created = 1]
  Template: DbCdcSpExecuteOutbox_sql.hb (DatabaseCdcCodeGenerator: Database/Schema/Xxx/Stored Procedures)
    Created -> \Xyz.Legacy.CdcDatabase\XCdc\Stored Procedures\Generated\spExecutePersonCdcOutbox.sql
   [Files: Unchanged = 0, Updated = 0, Created = 1]
  Template: DbCdcSpCompleteOutbox_sql.hb (DatabaseCdcCodeGenerator: Database/Schema/Xxx/Stored Procedures)
    Created -> \Xyz.Legacy.CdcDatabase\XCdc\Stored Procedures\Generated\spCompletePersonCdcOutbox.sql
   [Files: Unchanged = 0, Updated = 0, Created = 1]
  Template: DbCdcIdentifierMappingUdt_sql.hb (DatabaseCdcIdentifierMappingCodeGenerator: Database/Schema/Xxx/Types/User-Defined Table Types)
    Created -> \Xyz.Legacy.CdcDatabase\XCdc\Types\User-Defined Table Types\Generated\udtCdcIdentifierMappingList.sql
   [Files: Unchanged = 0, Updated = 0, Created = 1]
  Template: DbCdcSpCreateIdentifierMapping_sql.hb (DatabaseCdcIdentifierMappingCodeGenerator: Database/Schema/Xxx/Types/Stored Procedures)
    Created -> \Xyz.Legacy.CdcDatabase\XCdc\Stored Procedures\Generated\spCreateCdcIdentifierMapping.sql
   [Files: Unchanged = 0, Updated = 0, Created = 1]
  Template: DbCdcIdentifierMappingTvp_cs.hb (DatabaseCdcIdentifierMappingCodeGenerator: Cdc/Data)
    Created -> \Xyz.Legacy.CdcPublisher\Data\Generated\CdcIdentifierMappingDbMapper.cs
   [Files: Unchanged = 0, Updated = 0, Created = 1]
  Template: DbCdcTrackingTableCreate_sql.hb (DatabaseCdcRootCodeGenerator: Database/Schema/Xxx/Tables)
    Created -> \Xyz.Legacy.CdcDatabase\XCdc\Tables\Generated\CdcTracking.sql
   [Files: Unchanged = 0, Updated = 0, Created = 1]
  Template: DbCdcIdentifierMappingTableCreate_sql.hb (DatabaseCdcIdentifierMappingCodeGenerator: Database/Schema/Xxx/Tables)
    Created -> \Xyz.Legacy.CdcDatabase\XCdc\Tables\Generated\CdcIdentifierMapping.sql
   [Files: Unchanged = 0, Updated = 0, Created = 1]
  Template: DbCdcOutboxTableCreate_sql.hb (DatabaseCdcCodeGenerator: Database/Schema/Xxx/Tables)
    Created -> \Xyz.Legacy.CdcDatabase\XCdc\Tables\Generated\PersonOutbox.sql
   [Files: Unchanged = 0, Updated = 0, Created = 1]

Beef Code-Gen Tool complete [2850ms, Unchanged = 0, Updated = 0, Created = 15, TotalLines = 901].
```

<br/>

## Database artefact publish

_Tip:_ If any of the generated files are not automatically added to the Visual Studio Project structure, the _Show All Files_ in the _Solution Explorer_ can be used to view, and then added by selecting and using the _Include In Project_ function.

The database artefacts must now be deployed to the `XyzLegacy` database using the DACPAC _Build_ and then _Publish..._ functions. Ensure that the publish completes successfully before continuing.

</br>

## Execute the publisher

Now that the code generation has completed successfully, the required artefacts needed to compile the `Xyz.Legacy.CdcPublisher` project should now exist. Build the project and ensure it compiles successfully.

The key steps that need to be performed are:
- [Execute with logged events](#Execute-with-logged-events) to validate functioning without Azure Service Bus integration.
- [Execute using Azure Service Bus](#Execute-using-Azure-Service-Bus) to validate the event send/publish.

<br/>

### Execute with logged events

To execute using the debugger, open the project properties, and then navigate to the _Debug_ tab. Set the _Application arguments_ as follows. Then run the application.

```
ContinueWithDataLoss=true IntervalSeconds=3 MaxQuerySize=50 CdcDb="Data Source=.;Initial Catalog=XyzLegacy;Integrated Security=True"
```

Once the application is executing, make a change to the `Person` or `PersonAddress` data in the database. For example, add an `X` to the end of the `FirstName` column. After a few seconds the console application will output some information indicating that an entity was found and published. To stop the program hit `ctrl-c`. The console output should look similar as follows.

```
info: Xyz.Legacy.CdcPublisher.Services.PersonCdcHostedService[0]
      PersonCdcHostedService service started. Execution interval 00:00:03.
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: C:\Users\eric\source\repos\Beef\samples\Cdc\Xyz.Legacy.CdcPublisher\bin\Debug\netcoreapp3.1
info: Xyz.Legacy.CdcPublisher.Data.PersonCdcData[0]
      PersonCdcData Outbox '1': 1 entity(s) were found. [MaxQuerySize=50, ContinueWithDataLoss=True, CorrelationId=d1afe27b-d708-4a34-9ea6-9bbd094ed086, 65ms]
info: Xyz.Legacy.CdcPublisher.Data.PersonCdcData[0]
      PersonCdcData Outbox '1': Global identifier mapping assignment. [CorrelationId=d1afe27b-d708-4a34-9ea6-9bbd094ed086, 6ms]
info: Beef.Events.LoggerEventPublisher[0]
      Subject: Xyz.Legacy.Person, Action: Updated, Value: {"globalId":"ed5fc9de-2042-46b5-9749-36b0e2346788","firstName":"JohnX","lastName":"Doe","phone":"425 647 1234","email":"jd@hotmail.com","active":true,"addresses":[{"id":1,"personId":1,"addressTypeId":1,"street1":"8000 Main Rd","city":"Redmond","state":"98052","postalZipCode":"WA","code":"HOME"},{"id":2,"personId":1,"addressTypeId":2,"street1":"1001 1ST AVE N","city":"Seattle","state":"98109","postalZipCode":"WA","code":"POST"}],"etag":"+GC46Y1fb4DfzNXnyr6Aow=="}
info: Xyz.Legacy.CdcPublisher.Data.PersonCdcData[0]
      PersonCdcData Outbox '1': 1 event(s) were published/sent successfully. [CorrelationId=d1afe27b-d708-4a34-9ea6-9bbd094ed086, 4289ms]
info: Xyz.Legacy.CdcPublisher.Data.PersonCdcData[0]
      PersonCdcData Outbox '1': Marked as Completed. [CorrelationId=d1afe27b-d708-4a34-9ea6-9bbd094ed086, 61ms]
info: Microsoft.Hosting.Lifetime[0]
      Application is shutting down...
info: Xyz.Legacy.CdcPublisher.Services.PersonCdcHostedService[0]
      PersonCdcHostedService service stop requested.
info: Xyz.Legacy.CdcPublisher.Services.PersonCdcHostedService[0]
      PersonCdcHostedService service stopped.
```

<br/>

### Execute using Azure Service Bus

Next step is to publish the event to the likes of Azure Service Bus so one of more independent services can consume and process accordingly. The _Beef_ [`ServiceBusSender`](../../src/Beef.Events.ServiceBus/ServiceBusSender.cs) can accept either a specified queue name, or can infer from the event subject (i.e. a queue per entity). The second option is the approach that will be used here.

An Azure Service Bus Namespace with a queue name `xyz.legacy.person` is required to be set up. Once configured get the [_Shared access policy_](https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-sas) for the Service Bus Namespace (not the queue itself) that has _Send_ and _Listen_ permissions (required for this sample for simplicity).

Make the following change to `Program.cs` (effectively reversing out the commented out code):

``` csharp
//services.AddBeefLoggerEventPublisher();
var sbc = new ServiceBusClient(hostContext.Configuration.GetValue<string>("ServiceBus"));
services.AddBeefEventServiceBusSender(sbc);
```

To execute using the debugger, open the project properties, and then navigate to the _Debug_ tab. Set the _Application arguments_ as follows; essentially adding `ServiceBus="azure_secret_key"`. The `azure_secret_key` must be replaced with the _Connection String_ from the _Shared access policy_. Then run the application.

```
ContinueWithDataLoss=true IntervalSeconds=3 MaxQuerySize=50 CdcDb="Data Source=.;Initial Catalog=XyzLegacy;Integrated Security=True" ServiceBus="azure_secret_key"
```

The program output will not have logged the event JSON explicity like before. Log back into _Azure Portal_ and navigate to the `xyz.legacy.person` queue. Select the _Service Bus Explorer_ and _Peek_ the messages. The message content type should be `application/json` with the content similar to as follows. Note that the `type` is the event subject and action combined, the _globalId_ is the assigned identifier, with the `source` having the existing identifier. Additionally, _Beef_ adds additional [`EventMetadata`](../../src/Beef.Core/Events/EventData.cs) (selectable properties) as an extension such as a `CorrelationId`; there is an option to turn this off where not required.

``` json
{
  "specversion": "1.0",
  "type": "xyz.legacy.person.updated",
  "source": "/legacy_db/person/1",
  "id": "d28fdd61-fc25-4a29-924f-136596a47049",
  "time": "2021-04-22T22:20:37.112817Z",
  "datacontenttype": "application/json",
  "subject": "xyz.legacy.person",
  "action": "updated",
  "correlationid": "d1afe27b-d708-4a34-9ea6-9bbd094ed086",
  "data":
    {
      "globalId": "ed5fc9de-2042-46b5-9749-36b0e2346788",
      "firstName": "JohnX",
      "lastName": "Doe",
      "phone": "425 647 1234",
      "email": "jd@hotmail.com",
      "active": true,
      "addresses":
        [
          {
            "id": 1,
            "personId": 1,
            "addressTypeId": 1,
            "street1": "8000 Main Rd",
            "city": "Redmond",
            "state": "98052",
            "postalZipCode": "WA",
            "code": "HOME",
          },
          {
            "id": 2,
            "personId": 1,
            "addressTypeId": 2,
            "street1": "1001 1ST AVE N",
            "city": "Seattle",
            "state": "98109",
            "postalZipCode": "WA",
            "code": "POST",
          },
        ],
      "etag": "+GC46Y1fb4DfzNXnyr6Aow==",
    },
}
```

<br/>

## Xyz.Legacy.CdcReceiver

Create new using the `Azure Functions template` template (select `Empty` on the second screen of the wizard) within Visual Studio and name `Xyz.Legacy.CdcReceiver`. Once created, add the [`Beef.Events.ServiceBus`](../../src/Beef.Events.ServiceBus) and `Microsoft.Azure.WebJobs.Extensions.ServiceBus` NuGet packages to the project. Also, add a project reference to `Xyz.Legacy.CdcPublisher` as the `PersonCdc` entity will be used when receiving the data. This project now provides the basic foundation to build out the message receiving capability.

The key steps that need to be performed are:
- [Startup.cs](#Startup.cs) to provide the Dependency Injection (DI).
- [PersonReceiver.cs](#PersonReceiver.cs) to act as the Azure Service Bus _Receiver_.
- [PersonEditSubscriber.cs](#PersonEditSubscriber.cs) to act as the `created` and `updated` event/message subscriber (handler).
- [PersonDeleteSubscriber.cs](#PersonDeleteSubscriber.cs) to act as the `deleted` event/message subscriber (handler).
- [Execute receiver](#Execute-receiver) to demonstrate the receiving of an event/message and execution of the appropriate subscriber.

To assist with understanding, consider the following:
- _Receiver_ - responsible for receiving a message from a messaging system (i.e. Azure Service Bus) and orchestrating its processing via a message _Subscriber_.
- _Subscriber_ - responsible for the processing of a message, largely independent of how it was orginally received. The _Receiver_ will determine which _Subscriber_ if any should be invoked for a message based on the underlying event subject (and optionally action). There must only be a single _Subscriber_ per subject and action combination; otherwise, a runtime exception will be thrown. An advantage of this separation is that the _Subscriber_ can be easily unit tested without the need for a _Subscriber_ and its required for the likes of Azure Service Bus, etc. 

<br/>

### Startup.cs

Dependency Injection (DI) is needed to add the required services. For Azure Functions this is achieved using the Azure [`FunctionsStartup`](https://github.com/Azure/azure-functions-dotnet-extensions/blob/main/src/Extensions/DependencyInjection/FunctionsStartup.cs) base class. Add a `Startup.cs` file with the following contents.

``` csharp
using Beef;
using Beef.Events;
using Beef.Events.ServiceBus;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(Xyz.Legacy.CdcReceiver.Startup))] // Required to ensure the Dependency Injection (DI) Startup is executed.

namespace Xyz.Legacy.CdcReceiver
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Services configuration as follows:
            // - .AddBeefExecutionContext - enables the `ExecutionContext` required internally by _Beef_.
            // - .AddBeefServiceBusReceiverHost - adds the capability to encapsulate the receive orchestration Azure Service Bus messages.
            builder.Services.AddBeefExecutionContext();
            builder.Services.AddBeefServiceBusReceiverHost(EventSubscriberHostArgs.Create<Startup>());
        }
    }
}
```

<br/>

### PersonReceiver.cs

This is the actual Azure Function itself that is responsible for receiving the messages from Azure Service Bus. In the context of this sample, this is optional - this provides a how-to using _Beef_ to receive and process an event/message.

Code of note:
- The constructor leverages Dependency Injection (DI) to get the [`ServiceBusReceiverHost`](../../src/Beef.Events.ServiceBus/ServiceBusReceiverHost.cs) instance which was configured within the `Startup.cs`. This is required to receive and process the message via the `ReceiveAsync` method.
- The `UseLogger` is used to pass the `ILogger` parameter to be used by the receiver. 
- The `CreateServiceBusData` uses the same approach to get the queue name and connection string as the `ServiceBusTriggerAttribute`; these are needed and used for auditing and logging where applicable (therefore, the values must be the same).
- The `ReceiveAsync` will receive the message and based on the underlying subject and action will invoke the appropriate subscriber (discussed in next [sub-section](#PersonEditSubscriber.cs)).

Add a `PersonReceiver.cs` file with the following contents.

``` csharp
using Beef;
using Beef.Events.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using AzureServiceBus = Microsoft.Azure.ServiceBus;

namespace Xyz.Legacy.CdcReceiver
{
    public class PersonReceiver
    {
        private readonly ServiceBusReceiverHost _receiver;

        public PersonReceiver(ServiceBusReceiverHost receiver) => _receiver = Check.NotNull(receiver, nameof(receiver));

        [FunctionName(nameof(PersonReceiver))]
        [ExponentialBackoffRetry(10, "00:00:05", "00:00:30")]
        public async Task Run([ServiceBusTrigger("xyz.legacy.person", Connection = "ServiceBusConnectionString")] AzureServiceBus.Message message, ILogger logger)
            => await _receiver.UseLogger(logger).ReceiveAsync(_receiver.CreateServiceBusData(message, "xyz.legacy.person", "ServiceBusConnectionString"));
    }
}
```

</br>

### PersonEditSubscriber.cs

To logically separate the message subscriber(s) from the message recevier(s) it is recommended that these are housed in a `Subscribers` folder. Therefore, add a new folder within the project named `Subscribers`. Then add a `PersonEditSubscriber.cs` file with the following contents that will subscribe to either a `created` or `updated` action.

``` csharp
using Beef.Events;
using Xyz.Legacy.CdcPublisher.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Xyz.Legacy.CdcReceiver.Subscribers
{
    // EventSubscriberAttribute(s) define the subject and optional actions that uniquely link processing logic to subject.
    // Subscribers inherit from EventSubscriber or EventSubscriber<T> (specifying expected data type for automatic deserialization).
    [EventSubscriber("xyz.legacy.person.created")]
    [EventSubscriber("xyz.legacy.person.updated")]
    [EventSubscriber("xyz.legacy.person", "created", "updated")]
    public class PersonEditSubscriber : EventSubscriber<PersonCdc>
    {
        // Override the ReceiveAsync and implement logic; when finished return a Result.Success() to indicate successful execution. 
        public override Task<Result> ReceiveAsync(EventData<PersonCdc> eventData)
        {
            // Under normal circumstances there would be no need to log unless neccessary; this is for illustrative purposes only. Replace with logic to update system accordingly.
            Logger.LogInformation($"Subject: {eventData.Subject}, Action: {eventData.Action}, Source: {eventData.Source}, CorrelationId: {eventData.CorrelationId}, Value: {System.Environment.NewLine}{JsonConvert.SerializeObject(eventData.Value, Formatting.Indented)}");
            return Task.FromResult(Result.Success());
        }
    }
}
```

</br>

### PersonDeleteSubscriber.cs

To further demonstrate separation a delete specific subscriber is provided. Add a `PersonDeleteSubscriber.cs` file with the following contents that will subscribe to the `deleted` action.

``` csharp
using Beef.Events;
using Xyz.Legacy.CdcPublisher.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Xyz.Legacy.CdcReceiver.Subscribers
{
    // EventSubscriberAttribute(s) define the subject and optional actions that uniquely link processing logic to subject.
    // Subscribers inherit from EventSubscriber or EventSubscriber<T> (specifying expected data type for automatic deserialization).
    [EventSubscriber("xyz.legacy.person.deleted")]
    [EventSubscriber("xyz.legacy.person", "deleted")]
    public class PersonDeleteSubscriber : EventSubscriber<PersonCdc>
    {
        // Override the ReceiveAsync and implement logic; when finished return a Result.Success() to indicate successful execution. 
        public override Task<Result> ReceiveAsync(EventData<PersonCdc> eventData)
        {
            // Under normal circumstances there would be no need to log unless neccessary; this is for illustrative purposes only. Replace with logic to update system accordingly.
            Logger.LogInformation($"Subject: {eventData.Subject}, Action: {eventData.Action}, Source: {eventData.Source}, CorrelationId: {eventData.CorrelationId}, Value: {System.Environment.NewLine}{JsonConvert.SerializeObject(eventData.Value, Formatting.Indented)}");
            return Task.FromResult(Result.Success());
        }
    }
}
```

<br/>

### Execute receiver

The final requirement is to add the `ServiceBusConnectionString` configuration as referenced within the `PersonReceiver`. Within the `local.settings.json` file, add the `ServiceBusConnectionString` setting similar to following. The `azure_secret_key` must be replaced with the _Connection String_ from the Azure Service Bus _Shared access policy_.

``` json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "ServiceBusConnectionString": "azure_secret_key"
  }
}
``` 

Finally, execute (debug) the Azure Function from Visual Studio. The Azure Function runtime output should look similar as follows. _Note:_ the `CorrelationId` has flowed all the way from the publisher, into the `[XCdc].[PersonOutbox]` table, housed within the event metadata, and finally into the subcriber to enable end-to-end tracing.

```
Executing 'PersonReceiver' (Reason='(null)', Id=10685d5e-e1e0-4f60-b21e-33a41b868d41)
Trigger Details: MessageId: a0dcee6a-8e4d-41eb-a302-53d34e05e673, SequenceNumber: 20, DeliveryCount: 1, EnqueuedTimeUtc: 2021-04-22T22:27:12.1320000Z, LockedUntilUtc: 2021-04-22T22:27:42.1320000Z, SessionId: (null)
Subject: xyz.legacy.person, Action: updated, Source: /legacy_db/person/1, CorrelationId: d1afe27b-d708-4a34-9ea6-9bbd094ed086, Value:
{
  "globalId": "ed5fc9de-2042-46b5-9749-36b0e2346788",
  "firstName": "John",
  "lastName": "Doe",
  "phone": "425 647 1234",
  "email": "jd@hotmail.com",
  "active": true,
  "addresses": [
    {
      "id": 1,
      "personId": 1,
      "addressTypeId": 1,
      "street1": "8000 Main Rd",
      "city": "Redmond",
      "state": "98052",
      "postalZipCode": "WA",
      "code": "HOME"
    },
    {
      "id": 2,
      "personId": 1,
      "addressTypeId": 2,
      "street1": "1001 1ST AVE N",
      "city": "Seattle",
      "state": "98109",
      "postalZipCode": "WA",
      "code": "POST"
    }
  ],
  "etag": "+GC46Y1fb4DfzNXnyr6Aow=="
}
Executed 'PersonReceiver' (Succeeded, Id=10685d5e-e1e0-4f60-b21e-33a41b868d41, Duration=207ms)
```