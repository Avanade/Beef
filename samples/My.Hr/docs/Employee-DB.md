# Step 1 - Employee DB

This will walk through the process of creating the required tables, and stored procedures, etc. needed for the `Employee` within a Microsoft SQL Server database. All of this work will occur within the context of the `My.Hr.Database` project.

The [`Beef.Database.Core`](../../../tools/Beef.Database.Core/README.md) provides the capabilities that will be leveraged. The underlying documentation describes these capabuilities and the database approach in greater detail.

_Note:_ Any time that command line execution is requested, this should be performed from the base `My.Hr.Database` folder.

<br/>

## Clean up existing migrations

Within the `Migrations` folder there will four entries that were created during the initial solution skeleton creation. The last two of these should be removed. The first two create the `Ref` and `Hr` database schemas. The `Ref` is for the reference data, and `Hr` is for the master data; in some scenarios it may make sense to use only the `Hr` schema which will house both types of data. For this sample two schemas will be used.

```
└── Migrations
  └── 20190101-000000-create-Ref-schema.sql    <- leave
  └── 20190101-000001-create-Hr-schema.sql     <- leave
  └── 20190101-000002-create-Ref-Gender.sql    <- remove
  └── 20190101-000003-create-Hr-Person.sql     <- remove
```

<br/>

## Create Employee table

First step is to create the `Employee` table migration script itself, following a similar naming convention to ensure it is executed (applied) in the correct order. This will create the migration script using a pre-defined nameing convention and templated T-SQL to aid development.

```
dotnet run scriptnew -create Hr.Employee
```

For the purposes of this step, open the newly created migration script and replace its contents with the following. Additional notes have been added to give context/purpose where applicable.

``` SQL
-- Migration Script

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

Use the following command line to create the `EmergencyContact` table migration script.

```
dotnet run scriptnew -create Hr.EmergencyContact
```

Replace the contents with the following. _Note_: that we removed the row version and auditing columns as these are not required as this table is to be tightly-coupled to the `Employee`, and therefore can only (and should only) be updated in that context (i.e. is a sub-table).

``` SQL
-- Migration Script

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
- `Ref.Gender`
- `Ref.TerminationReason`
- `Ref.RelationshipType`
- `Ref.USState`

At the command line execute the following commands. This will automatically create the tables as required using the reference data template given the `Ref` schema specified. No further changes will be needed for these tables.

```
dotnet run scriptnew -create Ref.Gender
dotnet run scriptnew -create Ref.TerminationReason
dotnet run scriptnew -create Ref.RelationshipType
dotnet run scriptnew -create Ref.USState
```

<br/>

## Reference Data data

Now that the Reference Data tables exist they will need to be populated. It is recommended that where possible that the Production environment values are specified (as these are intended to be deployed to all environments).

These values (database rows) are specified using YAML. For brevity in this document, copy the data for the above tables **only** (for now) from [`RefData.yaml`](../My.Hr.Database/Data/RefData.yaml) replacing the contents of the prefilled `RefData.yaml` within the `My.Hr.Database/Data` folder.

<br/>

## Reference Data query

To support the requirement to query the Reference Data values from the database we will use Entity Framework (EF) to simplify. The Reference Data table configuration will drive the EF .NET (C#) model code-generation via the `EfModel="true"` option. 

Remove all existing configuration from `My.Hr.Database.xml` and replace. Each table configuration is referencing the underlying table and schema, then requesting an EF model is created for all related columns found within the database. _Beef_ will query the database to infer the columns during code-generation to ensure it "understands" the latest configuration.

``` XML
<?xml version="1.0" encoding="utf-8" ?>
<CodeGeneration RefDatabaseSchema="Ref" xmlns="http://schemas.beef.com/codegen/2015/01/database" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xsi:noNamespaceSchemaLocation="https://github.com/Avanade/Beef/raw/master/tools/Beef.CodeGen.Core/Schema/codegen.table.xsd">

  <!-- Reference data tables/models. -->
  <Table Name="Gender" Schema="Ref" EfModel="true" />
  <Table Name="TerminationReason" Schema="Ref" EfModel="true" />
  <Table Name="RelationshipType" Schema="Ref" EfModel="true" />
  <Table Name="USState" Schema="Ref" EfModel="true" />

</CodeGeneration>
```

<br/>

## Stored Procedure CRUD

Stored procedures will be used for the primary `Employee` CRUD as this also allows a simplified (and performant) means to select and update related tables as required, such as `EmergencyContact`.

Copy the following configuration and append to the `My.Hr.Database.xml`; see comments within for the details. Again, _Beef_ will query the database to infer the columns during code-generation.

``` XML
  <!-- References the Employee table to infer the underlying schema, then creates stored procedures as configured:
       - Each then specifies an additional SQL statement to be executed after the primary action (as defined by Type). 
       - The Create and Update also specify the required SQL User-Defined Type (UDT) for the data to be passed into the stored procedure. -->
  <Table Name="Employee" Schema="Hr">
    <StoredProcedure Name="Get" Type="Get">
      <Execute Statement="EXEC [Hr].[spEmergencyContactGetByEmployeeId]" @EmployeeId />
    </StoredProcedure>
    <StoredProcedure Name="Create" Type="Create">
      <Parameter Name="EmergencyContactList" SqlType="[Hr].[udtEmergencyContactList] READONLY" />
      <Execute Statement="EXEC [Hr].[spEmergencyContactMerge] @EmployeeId, @EmergencyContactList" />
    </StoredProcedure>
    <StoredProcedure Name="Update" Type="Update">
      <Parameter Name="EmergencyContactList" SqlType="[Hr].[udtEmergencyContactList] READONLY" />
      <Execute Statement="EXEC [Hr].sp[EmergencyContactMerge] @EmployeeId, @EmergencyContactList" />
    </StoredProcedure>
    <StoredProcedure Name="Delete" Type="Delete">
      <Execute Statement="DELETE FROM [Hr].[EmergencyContact] WHERE [EmployeeId] = @EmployeeId" />
    </StoredProcedure>
  </Table>

  <!-- References the EmergencyContact table to infer the underlying schema, then creates stored procedures as configured: 
       - Specifies need for a SQL User-Defined Type (UDT) and corresponding .NET (C#) Table-Valued Parameter (TVP) excluding the EmployeeId column (as this is the merge key).
       - GetByEmployeeId will get all rows using the specified Parameter - the characteristics of the Parameter are inferred from the underlying schema.
       - Merge will perform a SQL merge using the specified Parameter. -->
  <Table Name="EmergencyContact" Schema="Hr" Udt="true" Tvp="EmergencyContact" UdtExcludeColumns="EmployeeId">
    <StoredProcedure Name="GetByEmployeeId" Type="GetAll">
      <Parameter Name="EmployeeId" />
    </StoredProcedure>
    <StoredProcedure Name="Merge" Type="Merge">
      <Parameter Name="EmployeeId" />
    </StoredProcedure>
  </Table>
```

<br/>

## Entity Framework query

To support a flexible query approach for the `Employee` Entity Franework will be used. To further optimize only the key data will be surfaced via the generated .NET (C#) data model.

``` XML
  <!-- References the Employee table to infer the underlying schema, and creates .NET (C#) model for the selected columns only. -->
  <Table Name="Employee" Schema="Hr" EfModel="true" IncludeColumns="EmployeeId, Email, FirstName, LastName, GenderCode, Birthday, StartDate, TerminationDate, TerminationReasonCode, PhoneNo" />
```

<br/>

## Database management

Once the configuration has been completed then the database can be created/updated, the code-generation performed, and the corresponding reference data loaded into the corresponding tables.

At the command line execute the following command to perform. The log output will decribe all actions that were performed.

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

## Conclusion

At this stage we now have a working database ready for the consuming API logic to be added. The required database tables exist, the Reference Data data has been loaded, the required stored procedures and user-defined type (UDT) have been generated and added to the database. The .NET (C#) Entity Framework models have been generated and added to the `My.Hr.Business` project, including the requisite table-valued parameter (TVP). 