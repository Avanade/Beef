# `Beef.Database.Core`

The `Beef.Database.Core` tool is a console application provided to automate the management of the SQL Server Database as part of the end-to-end development process.

<br/>

## Key elements

This is tool automates three key elements:
- **Migrations** - being the upgrading of a database overtime using order-based migration scripts; the tool leverages the philosophy and NuGet packages of [DbUp](https://dbup.readthedocs.io/en/latest/philosophy-behind-dbup/) to enable.
- **Schema** - there are a number of database schema objects that can be managed outside of the above migrations, that are dropped and applied to the database using their native `Create` statement.
- **Data** - there is data, for example *Reference Data* that needs to be applied to a database. This provides a simpler configuration than specifying the required SQL statements directly. This is also useful for setting up Master and Transaction data for the likes of testing scenarios.

<br/>

### Migrations

As stated, [DbUp](https://dbup.readthedocs.io/en/latest/) is used enabling a database to be dropped, created and migrated. The migration is managed by tracking order-based migration scripts. It tracks which SQL scripts have been run already, and runs the change scripts that are needed to get the database up to date. 

Over time there will be more than one script updating a single object, for example a `Table`. In this case the first script operation will be a `Create`, followed by subsequent `Alter` operations. The scripts should be considered immutable, in that they cannot be changed once they have been applied; ongoing changes will need additional scripts.

The migration scripts must be marked as embedded resources, and reside under the `Migrations` folder within the c# project. A naming convention should be used to ensure they are to be executed in the correct order; it is recommended that the name be prefixed by the date and time, following by a description of the purpose. For example: `20181218-081540-create-demo-person-table.sql`

It is recommended that each script be enclosed by a transaction that can be rolled back in the case of error; otherwise, a script could be partially applied and will then need manual intervention to resolve.

<br/>

### Schema

There are some key schema objects that can be dropped and created overtime without causing side-effects. Equally, these objects can be code-generated reducing the effort to create and maintain over time. As such, these objects fall outside of the *Migrations* above.

The currently supported objects are (order specified implies order in which they are applied, and reverse when dropped to allow for dependencies):
1. [Type](https://docs.microsoft.com/en-us/sql/t-sql/statements/create-type-transact-sql)
2. [Function](https://docs.microsoft.com/en-us/sql/t-sql/statements/create-function-transact-sql)
3. [View](https://docs.microsoft.com/en-us/sql/t-sql/statements/create-view-transact-sql)
4. [Procedure](https://docs.microsoft.com/en-us/sql/t-sql/statements/create-procedure-transact-sql)

The schema scripts must be marked as embedded resources, and reside under the `Schema` folder within the c# project. Each script should only contain a single `Create` statement. Each script will be parsed to determine type so that the appropriate order can be applied.

The `Schema` folder is used to encourage the usage of database schemas. Therefore, directly under should be the schema name, for example `dbo` or `Ref`. Then sub-folders for the object types as per [Azure Data Studio](https://docs.microsoft.com/en-au/sql/azure-data-studio/what-is), for example `Functions`, `Stored Procedures` or `Types\User-Defined Table Types`. 

Code generation is also supported / enabled using the _Beef_ [Code-Gen](../Beef.CodeGen.Core/README.md) capabilities. The tooling looks for the schema objects in the file system (as well as embedded resources) to allow for additions/changes during the code generation execution.

<br/>

### Data

Data can be defined using [YAML](https://en.wikipedia.org/wiki/YAML) to enable simplified configuration that will be used to generate the required SQL statements to apply to the database.

The data specified follows a basic indenting/levelling rule to enable:
1. **Schema** - specifies Schema name.
2. **Table** - specifies the Table name within the Schema; this will be validated to ensure it exists within the database as the underlying table schema (columns) will be inferred. The underyling rows will be [inserted](https://docs.microsoft.com/en-us/sql/t-sql/statements/insert-transact-sql) by default, by prefixing with a `$` character a [merge](https://docs.microsoft.com/en-us/sql/t-sql/statements/merge-transact-sql) operation will be performed instead.
3. **Rows** - each row specifies the column name and the corresponding values (except for reference data described below). The tooling will parse each column value according to the underying SQL type.

*Reference Data* is treated as a special case; generally identified by being in the `Ref` schema. The first column name and value pair are treated as the `Code` and `Text` columns. Also the `IsActive` column will automatically be set to `true`, and the `SortOrder` column to the index (1-based) in which it is specified. 

Where a column is a *Reference Data* reference the reference data code can be specified, with the identifier being determined at runtime (using a sub-query) as it is unlikely to be known at configuration time. The tooling determines this by the column name being suffixed by `Id` and a corresponding table name in the `Ref` schema; example `GenderId` column and corresponding table `Ref.Gender`.

Example YAML configuration for *merging* reference data is as follows:
``` YAML
Ref:
  - $Gender:
    - M: Male
    - F: Female
```

Example YAML configuration for *inserting* data (also inferring the `GenderId` from the specified reference data code) is as follows:
``` YAML
Demo:
  - Person:
    - { FirstName: Wendy, LastName: Jones, Gender: F, Birthday: 1985-03-18 }
    - { FirstName: Brian, LastName: Smith, Gender: M, Birthday: 1994-11-07 }
    - { FirstName: Rachael, LastName: Browne, Gender: F, Birthday: 1972-06-28, Street: 25 Upoko Road, City: Wellington }
    - { FirstName: Waylon, LastName: Smithers, Gender: M, Birthday: 1952-02-21 }
  - WorkHistory:
    - { PersonId: 2, Name: Telstra, StartDate: 2015-05-23, EndDate: 2016-04-06 }
    - { PersonId: 2, Name: Optus, StartDate: 2016-04-16 }
```

<br/>

### Other considerations

To simplify the database management here are some further considerations that may make life easier over time; especially where you adopt the philosophy that the underlying busines logic (within the application APIs) is primarily responsible for the consistency of the data; and the data source (the database) is being largely used for storage and advanced query:

- **Nullable everything** - all columns (except) the primary key should be defined as nullable. The business logic should validate the request to ensure data is provided where mandatory. Makes changes to the database schema easier over time without this constraint.
- **Minimise constraints** - do not use database constraints unless absolutely necessary; only leverage where the database is the best and/or most efficient means to perform; i.e. uniqueness. The business logic should validate the request to ensure that any related data is provided, is valid and consistent. 
- **No cross-schema referencing** - avoid referencing across `Schemas` where possible as this will impact the Migrations as part of this tooling; and we should not be using constraints as per prior point. Each schema is considered independent of others except `dbo` or `sec` (security where used).
- **Standardise column lengths** - use a standard set of column lengths within the database and have the business logic manage the length constraint. As such the column length must be the same or greater that what is required.
- **JSON for schema-less** - where there is data that needs to be persisted, but rarely searched on, a schema-less approach should be considered such that a JSON object is persisted versus having to define columns. This can simplify the database requirements where the data is hierarchical in nature.

<br/>

## Console application

The `Beef.Database.Core` can be executed as a console application directly; however, the experience has been optimised so that a new console application can reference and inherit the capabilities. Then simply add the `Data`, `Migrations` and `Schema` folders and embed the required resources. See the sample [`Beef.Demo.Database`](../../samples/Demo/Beef.Demo.Database) as an example.

<br/>

### Commands

The following [commands](DatabaseExecutorCommand.cs) are automatically enabled for the console application:
- `Drop` - drop the existing database (where it alredy exists).
- `Create` - create the database (where it does not already exist).
- `Migrate` - migrate the database using the **Migrations** scripts (those that have not already been executed).
- `CodeGen` - generates database **Schema** objects via code generation.
- `Schema` - drops and creates the known database **Schema** objects.
- `Reset` - resets the database by deleting all existing data and reseting all identities (IDENT) to 0.
- `Data` - inserts or merges **Data** from embedded YAML files.

The remainder are common combinations of the above:
- `All` - performs **all** commands as follows; `Create`, `Migrate`, `CodeGen`, `Schema` and `Data`.
- `DropAndAll` - performs `Drop` and `All`.
- `ResetAndAll` - performs `Reset` and `All`.
- `Database` - performs only the **database** commands as follows: `Create`, `Migrate`, `Schema` and `Data`.
- `DropAndDatabase` - performs `Drop` and `Database`.
- `ResetAndDatabase` - performs `Reset` and `Database`.
- `ScriptNew` - preates a new script file using the defined naming convention.

<br/>

### Program.cs

The `Program.cs` for the new console application should be updated similar to the following. The connection string is provided as the default used at runtime. An environment variable `{Company}{AppName}ConnectionString` can be updated to override (where the `{Company}` and `{AppName}` values are specified); or alternatively use the command line option `-cs "connection-string-info"`. 

``` csharp
public class Program
{
    static int Main(string[] args)
    {
        return DatabaseConsoleWrapper.Create("Data Source=.;Initial atalog=Beef.Test;Integrated Security=True", "Beef", "Demo").Run(args);
    }
}
```

To automatically added artefacts as embedded resources make the following change to your `.csproj` file:

``` xml
  <ItemGroup>
    <EmbeddedResource Include="Schema\**\*" />
    <EmbeddedResource Include="Migrations\**\*" />
    <EmbeddedResource Include="Data\**\*" />
  </ItemGroup>
```

To run the console application, simply specify the required command; e.g:
```
dotnet run dropandall
dotnet run all
dotnet run database -cs "Data Source=.;Initial atalog=Beef.Test;Integrated Security=True"
```