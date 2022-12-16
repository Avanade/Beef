# Step 1 - Employee DB

This will walk through the process of creating the required tables, and stored procedures, etc. needed for the `Employee` within a Microsoft SQL Server database. All of this work will occur within the context of the `My.Hr.Database` project.

The [`Beef.Database.SqlServer`](../../../tools/Beef.Database.SqlServer) and [`DbEx`](https://github.com/Avanade/dbex) provide the capabilities that will be leveraged. The underlying [documentation](https://github.com/Avanade/dbex#readme) describes these capabilities and the database approach in greater detail.

_Note:_ Any time that command line execution is requested, this should be performed from the base `My.Hr.Database` folder.

<br/>

## Entity relationship diagram

The following provides a visual (ERD) for the database tables that will be created. A relationship label of _refers_ indicates a reference data relationship. The _(via JSON)_ implies that the relating entity references via a JSON data column (_not_ a referenced database table). 

``` mermaid
erDiagram
    Employee ||--o{ EmergencyContact : has
    Employee }|..|{ Gender : refers
    Employee }|..|{ TerminationReason : refers
    Employee }|..|{ USState : refers
    Employee ||--o{ Address : has (via JSON)
    EmergencyContact }|..|{ RelationshipType : refers
```

<br/>

## Clean up existing migrations

Within the `Migrations` folder there will three entries that were created during the initial solution skeleton creation. These should all be removed. 

```
└── Migrations
  └── 20190101-000001-create-Hr-schema.sql     <- remove
  └── 20190101-000002-create-Hr-Gender.sql     <- remove
  └── 20190101-000003-create-Hr-Person.sql     <- remove
```

<br/>

## Create HR schema

Create the `Hr` schema using the database tooling. This following command will create the migration script using the pre-defined naming convention and templated T-SQL to aid development.

```
dotnet run script schema Hr
```


## Create Employee table

Create the migration script for the `Employee` table table within the `Hr` schema following a similar naming convention to ensure it is executed (applied) in the correct order. This following command will create the migration script using the pre-defined naming convention and templated T-SQL to aid development.

```
dotnet run script create Hr Employee
```

For the purposes of this step, open the newly created migration script and replace its contents with the following. Additional notes have been added to give context/purpose where applicable. Note that the reference data values use `Code` and no database constraint is added as this relationship (and consistency) is managed by the owning business logic.

``` SQL
-- Create table: [Hr].[Employee]

BEGIN TRANSACTION

CREATE TABLE [Hr].[Employee] (
  [EmployeeId] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,  -- This is the primary key
  [Email] NVARCHAR(250) NULL UNIQUE,                                               -- This is the employee's unique email address
  [FirstName] NVARCHAR(100) NULL,
  [LastName] NVARCHAR(100) NULL,
  [GenderCode] NVARCHAR(50) NULL,                                                  -- This is the related Gender code; see Hr.Gender table
  [Birthday] DATE NULL,    
  [StartDate] DATE NULL,
  [TerminationDate] DATE NULL,
  [TerminationReasonCode] NVARCHAR(50) NULL,                                       -- This is the related Termination Reason code; see Hr.TerminationReason table
  [PhoneNo] NVARCHAR(50) NULL,
  [AddressJson] NVARCHAR(500) NULL,                                                -- This is the full address persisted as JSON.
  [RowVersion] TIMESTAMP NOT NULL,                                                 -- This is used for concurrency version checking. 
  [CreatedBy] NVARCHAR(250) NULL,                                                  -- The following are standard audit columns.
  [CreatedDate] DATETIME2 NULL,
  [UpdatedBy] NVARCHAR(250) NULL,
  [UpdatedDate] DATETIME2 NULL
);
	
COMMIT TRANSACTION
```

<br/>

## Create Emergency Contacts table

Use the following command line to generate the migration script to create the `EmergencyContact` table within the `Hr` schema.

```
dotnet run script create Hr EmergencyContact
```

Replace the contents with the following. _Note_: that we removed the row version and auditing columns as these are not required as this table is to be tightly-coupled to the `Employee`, and therefore can only (and should only) be updated in that context (i.e. is a sub-table).

``` SQL
-- Create table: [Hr].[EmergencyContact]

BEGIN TRANSACTION

CREATE TABLE [Hr].[EmergencyContact] (
  [EmergencyContactId] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,
  [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
  [FirstName] NVARCHAR(100) NULL,
  [LastName] NVARCHAR(100) NULL,
  [PhoneNo] NVARCHAR(50) NULL,
  [RelationshipTypeCode] NVARCHAR(50) NULL
);
	
COMMIT TRANSACTION
```

<br/>

## Create Reference Data tables

To support the capabilities of the tables above the following Reference Data tables are also required.
- `Hr.Gender`
- `Hr.TerminationReason`
- `Hr.RelationshipType`
- `Hr.USState`

At the command line execute the following commands. This will automatically create the tables as required using the reference data template given the `creatref` option specified. No further changes will be needed for these tables.

```
dotnet run script refdata Hr Gender
dotnet run script refdata Hr TerminationReason
dotnet run script refdata Hr RelationshipType
dotnet run script refdata Hr USState
```

<br/>

## Reference Data data

Now that the Reference Data tables exist they will need to be populated. It is recommended that where possible that the Production environment values are specified (as these are intended to be deployed to all environments).

These values (database rows) are specified using YAML. For brevity in this document, copy the data for the above tables **only** (for now) from [`RefData.yaml`](../MyEf.Hr.Database/Data/RefData.yaml) replacing the contents of the prefilled `RefData.yaml` within the `My.Hr.Database/Data` folder. Finally, remove the `PerformanceOutcome` lines at the end of the file.

_Note:_ The format and hierarchy for the YAML, is: Schema, Table, Row. For reference data tables where only `Code: Text` is provided, this is treated as a special case shorthand to update those two columns accordingly (the other columns will be updated automatically). The `$` prefix for a table indicates a `merge` versus an `insert` (default).

``` yaml
Hr:
  - $Gender:
    - F: Female
    - M: Male
    - N: Not specified
  ...
```

<br/>

## Reference Data query

To support the requirement to query the Reference Data values from the database we will use Entity Framework (EF) to simplify. The Reference Data table configuration will drive the EF .NET (C#) model code-generation via the `efModel: true` attribute. 

Remove all existing configuration from `database.beef.yaml` and replace. Each table configuration is referencing the underlying table and schema, then requesting an EF model is created for all related columns found within the database. _Beef_ will query the database to infer the columns during code-generation to ensure it "understands" the latest configuration.

``` yaml
# Configuring the code-generation global settings
# - Schema defines the default for all tables unless explicitly defined.
# 
schema: Hr
tables:
  # Reference data tables/models.
- { name: Gender, efModel: true }
- { name: TerminationReason, efModel: true }
- { name: RelationshipType, efModel: true }
- { name: USState, efModel: true }
```

<br/>

## Entity Framework CRUD and query

Entity Framework will be used for the primary `Employee` [CRUD](https://en.wikipedia.org/wiki/Create,_read,_update_and_delete) as this also allows a simplified (and performant) means to select and update related tables as required, in this case `EmergencyContact`.

Copy the following configuration and append (after reference data) to the `database.beef-5.yaml`; see comments within for the details. Again, _Beef_ will query the database to infer the columns during code-generation.

``` yaml
  # References the Employee and related tables to implement the EF Model and infer the underlying schema.
  # - Relationships can be code-generated (basic functionality), or handcrafted in the .NET using the standard EntityFramework capabilities. 
- { name: Employee, efModel: true,
    relationships: [
      # Relationships can be code-generated (basic functionality), or handcrafted in the .NET using the standard EntityFramework capabilities.
      # - One-to-many to EmergencyContacts table foreign key using EmployeeId column. Cascade the delete. Auto include collection on get and track for updates. 
      { name: EmergencyContact, propertyName: EmergencyContacts, foreignKeyColumns: [ EmployeeId ], onDelete: ClientCascade, autoInclude: true }
    ]
  }

- { name: EmergencyContact, efModel: true }
```

<br/>

## Database management

Once the configuration has been completed then the database can be created/updated, the code-generation performed, and the corresponding reference data loaded into the corresponding tables.

At the command line execute the following command to perform. The log output will describe all actions that were performed.

```
dotnet run all
```

If at any stage the database becomes corrupted or you need to rebuild, execute the following to drop and start again.

```
dotnet run drop
```

<br/>

## Indexes, etc.

Where tables need indexes and other constraints added these would be created using additional migration scripts. None have been included in the sample for brevity.

<br/>

## Event outbox

To support the [transactional outbox pattern](https://microservices.io/patterns/data/transactional-outbox.html) there is the need to have backing event queue tables. The migration scripts (and .NET code) to support this requirement can be code generated by adding the following to the top of the `database.beef-5.yaml` file directly under  the `schema: hr` line.

``` yaml
outbox: true
```

Execute the code-generation again using `codegen` option to generate the newly configured migration scripts. Once generated the database can be updated to use by using the `database` option. Run both of the following commands.

```
dotnet run codegen
dotnet run database
```

This should create migrations script files with names similar as follows (as well as a number of other SQL and .NET related artefacts).

```
└── Migrations
  └── 20210430-170605-create-01-create-outbox-schema.sql
  └── 20210430-170605-create-01-create-outbox-eventoutbox-table.sql
  └── 20210430-170605-create-01-create-outbox-eventoutboxdata-table.sql
```

<br/>

## Conclusion

At this stage we now have a working database ready for the consuming API logic to be added. The required database tables exist, the Reference Data data has been loaded, the required stored procedures and user-defined type (UDT) have been generated and added to the database. The .NET (C#) Entity Framework models have been generated and added to the `My.Hr.Business` project, including the requisite table-valued parameter (TVP). 

Next we need to create the [employee API](./Employee-Api.md) endpoint to perform the desired CRUD operations.