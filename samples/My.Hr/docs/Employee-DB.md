# Step 1 - Employee DB

This will walk through the process of creating the required tables, and stored procedures, etc. needed for the `Employee` within a Microsoft SQL Server database. All of this work will occur within the context of the `My.Hr.Database` project.

The [`Beef.Database.Core`](../../../tools/Beef.Database.Core/README.md) provides the capabilities that will be leveraged. The underlying documentation describes these capabuilities and the database approach in greater detail.

_Note:_ Any time that command line execution is requested, this should be performed from the base `My.Hr.Database` folder.

<br/>

## Entity relationship diagram

The following provides a visual (ERD) for the database tables that will be created. A relationship label of _refers_ indicates a reference data relationship. The _(via JSON)_ implies that the relating table references invisibly to the database via a JSON data column. 

``` mermaid
erDiagram
    Employee ||--o{ EmergencyContact : has
    Employee }|..|{ Gender : refers
    Employee }|..|{ TerminationReason : refers
    Employee }|..|{ USState : "refers (via JSON)" 
    EmergencyContact }|..|{ RelationshipType : refers
```

<br/>

## Clean up existing migrations

Within the `Migrations` folder there will three entries that were created during the initial solution skeleton creation. The last two of these should be removed. The first creates the `Hr` database schema. 

```
└── Migrations
  └── 20190101-000001-create-Hr-schema.sql     <- leave
  └── 20190101-000002-create-Hr-gender.sql     <- remove
  └── 20190101-000003-create-Hr-person.sql     <- remove
```

<br/>

## Create Employee table

First step is to create the migration script for the `Employee` table table within the `Hr` schema following a similar naming convention to ensure it is executed (applied) in the correct order. This following command will create the migration script using the pre-defined naming convention and templated T-SQL to aid development.

```
dotnet run scriptnew create Hr Employee
```

For the purposes of this step, open the newly created migration script and replace its contents with the following. Additional notes have been added to give context/purpose where applicable.

``` SQL
-- Create table: Hr.Employee

BEGIN TRANSACTION

CREATE TABLE [Hr].[Employee] (
  [EmployeeId] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,  -- This is the primary key
  [Email] NVARCHAR(250) NULL UNIQUE,                                               -- This is the employee's unique email address
  [FirstName] NVARCHAR(100) NULL,
  [LastName] NVARCHAR(100) NULL,
  [GenderCode] NVARCHAR(50) NULL,                                                  -- This is the related Gender code; see Ref.Gender table
  [Birthday] DATE NULL,    
  [StartDate] DATE NULL,
  [TerminationDate] DATE NULL,
  [TerminationReasonCode] NVARCHAR(50) NULL,                                       -- This is the related Termination Reason code; see Ref.TerminationReason table
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
dotnet run scriptnew create Hr EmergencyContact
```

Replace the contents with the following. _Note_: that we removed the row version and auditing columns as these are not required as this table is to be tightly-coupled to the `Employee`, and therefore can only (and should only) be updated in that context (i.e. is a sub-table).

``` SQL
-- Create table: Hr.EmergencyContact

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
dotnet run scriptnew createref Hr Gender
dotnet run scriptnew createref Hr TerminationReason
dotnet run scriptnew createref Hr RelationshipType
dotnet run scriptnew createref Hr USState
```

<br/>

## Reference Data data

Now that the Reference Data tables exist they will need to be populated. It is recommended that where possible that the Production environment values are specified (as these are intended to be deployed to all environments).

These values (database rows) are specified using YAML. For brevity in this document, copy the data for the above tables **only** (for now) from [`RefData.yaml`](../My.Hr.Database/Data/RefData.yaml) replacing the contents of the prefilled `RefData.yaml` within the `My.Hr.Database/Data` folder.

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

To support the requirement to query the Reference Data values from the database we will use Entity Framework (EF) to simplify. The Reference Data table configuration will drive the EF .NET (C#) model code-generation via the `efModel: true` option. 

Remove all existing configuration from `database.beef.yaml` and replace. Each table configuration is referencing the underlying table and schema, then requesting an EF model is created for all related columns found within the database. _Beef_ will query the database to infer the columns during code-generation to ensure it "understands" the latest configuration.

``` yaml
# Configuring the code-generation global settings
# - Schema defines the default for al tables unless explicitly defined.
# - EventOutbox indicates whether events will publish using the outbox pattern and therefore the event outbox artefacts are required.
# - EntityScope of Autonomous will generate both business and common entities to allow each to be used autonomously; versus using shared common.
# 
schema: Hr
eventOutbox: true
entityScope: Autonomous
tables:
  # Reference data tables/models.
- { name: Gender, efModel: true }
- { name: TerminationReason, efModel: true }
- { name: RelationshipType, efModel: true }
- { name: USState, efModel: true }
```

<br/>

## Stored Procedure CRUD

Stored procedures will be used for the primary `Employee` CRUD as this also allows a simplified (and performant) means to select and update related tables as required, in this case `EmergencyContact`.

Where these related tables contain a related collection (i.e. zero or more rows), then a SQL [Merge](https://docs.microsoft.com/en-us/sql/t-sql/statements/merge-transact-sql?view=sql-server-ver15) is used as it will `insert`, `update` or `delete` each row accordingly. To generate this the _Beef code-gen_ enables a `Type` of `Merge`. Additionally, a SQL [User-Defined Type (UDT)](https://docs.microsoft.com/en-us/sql/t-sql/statements/create-type-transact-sql?view=sql-server-ver15) and corresponding .NET (C#) [Table-Valued Parameter (TVP)](https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql/table-valued-parameters) will also need to be generated to support the passing of the data (multiple rows) between .NET and SQL Server.

Copy the following configuration and append (after reference data) to the `database.beef.yaml`; see comments within for the details. Again, _Beef_ will query the database to infer the columns during code-generation.

``` yaml
  # References the Employee table to infer the underlying schema, then creates stored procedures as configured:
  # - Each then specifies an additional SQL statement to be executed after the primary action (as defined by Type).
  # - The Create and Update also specify the required SQL User-Defined Type (UDT) for the data to be passed into the stored procedure.
- { name: Employee,
    storedProcedures: [
      { name: Get, type: Get,
        execute: [
          { statement: 'EXEC [Hr].[spEmergencyContactGetByEmployeeId] @EmployeeId' }
        ]
      },
      { name: Create, type: Create,
        parameters: [
          { name: EmergencyContactList, sqlType: '[Hr].[udtEmergencyContactList] READONLY' }
        ],
        execute: [
          { statement: 'EXEC [Hr].[spEmergencyContactMerge] @EmployeeId, @EmergencyContactList' }
        ]
      },
      { name: Update, type: Update,
        parameters: [
          { name: EmergencyContactList, sqlType: '[Hr].[udtEmergencyContactList] READONLY' }
        ],
        execute: [
          { statement: 'EXEC [Hr].[spEmergencyContactMerge] @EmployeeId, @EmergencyContactList' }
        ]
      },
      { name: Delete, type: Delete,
        execute: [
          { statement: 'DELETE FROM [Hr].[EmergencyContact] WHERE [EmployeeId] = @EmployeeId' },
        ]
      }
    ]
  }

  # References the EmergencyContact table to infer the underlying schema, then creates stored procedures as configured:
  # - Specifies need for a SQL User-Defined Type (UDT) and corresponding .NET (C#) Table-Valued Parameter (TVP) excluding the EmployeeId column (as this is the merge key).
  # - GetByEmployeeId will get all rows using the specified Parameter - the characteristics of the Parameter are inferred from the underlying schema.
  # - Merge will perform a SQL merge using the specified Parameter.
- { name: EmergencyContact, udt: true, tvp: EmergencyContact, udtExcludeColumns: [ EmployeeId ],
    storedProcedures: [
      { name: GetByEmployeeId, type: GetColl,
        parameters: [
          { name: EmployeeId }
        ]
      },
      { name: Merge, type: Merge,
        parameters: [
          { name: EmployeeId }
        ]
      }
    ]
  }
```

<br/>

## Entity Framework query

To support a flexible query approach for the `Employee` Entity Framework (EF) will be used. To further optimize only the key data will be surfaced via the generated .NET (C#) data model using the include columns option. Append the following to the end of `database.beef.yaml`.

``` yaml
  # References the Employee table to infer the underlying schema, and creates .NET (C#) model for the selected columns only.
- { name: Employee, efModel: true, includeColumns: [ EmployeeId,  Email,  FirstName,  LastName,  GenderCode,  Birthday,  StartDate,  TerminationDate,  TerminationReasonCode,  PhoneNo ] }
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

To support the [transactional outbox pattern](https://microservices.io/patterns/data/transactional-outbox.html) there is the need to have the backing event queue tables. The migration scripts to create these can be code generated using the following.

```
dotnet run codegen --script DatabaseEventOutbox.xml
```

This should create two migrations script files with names similar as follows.

```
└── Migrations
  └── 20210430-170605-create-hr-eventoutbox.sql
  └── 20210430-170605-create-hr-eventoutboxdata.sql
```


The corresponding stored procedures and user defined types will be automatically code-generated as a result of the `eventOutbox: true` configuration property already set within the code-gen YAML configuration file.

At the command line execute the following command to update the database using the new migration scripts.

```
dotnet run all
```

<br/>

## Conclusion

At this stage we now have a working database ready for the consuming API logic to be added. The required database tables exist, the Reference Data data has been loaded, the required stored procedures and user-defined type (UDT) have been generated and added to the database. The .NET (C#) Entity Framework models have been generated and added to the `My.Hr.Business` project, including the requisite table-valued parameter (TVP). 

Next we need to create the [employee API](./Employee-Api.md) endpoint to perform the desired CRUD operations.