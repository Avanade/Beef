# `Beef.Database.Core`

The `Beef.Database.Core` tool is a console application provided to automate the management of a Microsoft SQL Server Database as part of the end-to-end development process.

<br/>

## Data-tier Application vs DbUp

- **[Data-tier Application DAC](https://docs.microsoft.com/en-us/sql/relational-databases/data-tier-applications/data-tier-applications?view=sql-server-ver15)** is a logical database management entity that defines all of the SQL Server objects - like tables, views, and instance objects, including logins - associated with a database. A DAC is a self-contained unit of SQL Server database deployment that enables data-tier developers and database administrators to package SQL Server objects into a portable artifact called a DAC package, also known as a DACPAC. This is the traditional means to manage a database.
- **[DbUp](https://dbup.readthedocs.io/en/latest/)** is a .NET library that is used to deploy changes to SQL Server databases. It tracks which SQL scripts have been run already, and runs the change scripts that are needed to get a database up to date. 

DbUp is directly integrated into `Beef.Database.Core` to simplify usage; and acts as the default approach. Usage is further integrated into the likes of the [Intra-domain Integration Testing](./../Beef.Test.NUnit/README.md) etc.

A Data-tier Application (DAC) can still be leveraged to manage a database; with `Beef.Database.Core` still playing a key role in the code-generation of [Schema](#Schema) objects where applicable.

<br/>

## Key elements

This is tool automates three key elements:
- [**Migrations**](#Migrations) - being the upgrading of a database overtime using order-based migration scripts; the tool leverages the philosophy and NuGet packages of [DbUp](https://dbup.readthedocs.io/en/latest/philosophy-behind-dbup/) to enable.
- [**Schema**](#Schema) - there are a number of database schema objects that can be managed outside of the above migrations, that are dropped and (re-)applied to the database using their native `Create` statement.
- [**Data**](#Data) - there is data, for example *Reference Data* that needs to be applied to a database. This provides a simpler configuration than specifying the required SQL statements directly. This is _also_ useful for setting up Master and Transaction data for the likes of testing scenarios.

<br/>

### Migrations

As stated, [DbUp](https://dbup.readthedocs.io/en/latest/) is used enabling a database to be dropped, created and migrated. The migration is managed by tracking order-based migration scripts. It tracks which SQL scripts have been run already, and runs the change scripts that are needed to get the database up to date. 

Over time there will be more than one script updating a single object, for example a `Table`. In this case the first script operation will be a `Create`, followed by subsequent `Alter` operations. The scripts should be considered immutable, in that they cannot be changed once they have been applied; ongoing changes will need additional scripts.

The migration scripts must be marked as embedded resources, and reside under the `Migrations` folder within the c# project. A naming convention should be used to ensure they are to be executed in the correct order; it is recommended that the name be prefixed by the date and time, followed by a brief description of the purpose. For example: `20181218-081540-create-demo-person-table.sql`

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

_Note_: There is a _special case_ where a [Table](https://docs.microsoft.com/en-us/sql/t-sql/statements/create-table-transact-sql) object create script is found, then this will only be enacted where it does not previously exist. An `IF NOT EXIST` statement is automatically wrapped (integrated) prior to executing. These will be applied in advance of the objects listed earlier (_no_ attempt to delete or update will occur).

Code generation is also supported / enabled using the _Beef_ [Code-Gen](../Beef.CodeGen.Core/README.md) capabilities. The tooling looks for the schema objects in the file system (as well as embedded resources) to allow for additions/changes during the code generation execution.

<br/>

### Data

Data can be defined using [YAML](https://en.wikipedia.org/wiki/YAML) to enable simplified configuration that will be used to generate the required SQL statements to apply to the database.

The data specified follows a basic indenting/levelling rule to enable:
1. **Schema** - specifies Schema name.
2. **Table** - specifies the Table name within the Schema; this will be validated to ensure it exists within the database as the underlying table schema (columns) will be inferred. The underyling rows will be [inserted](https://docs.microsoft.com/en-us/sql/t-sql/statements/insert-transact-sql) by default; or alternatively by prefixing with a `$` character a [merge](https://docs.microsoft.com/en-us/sql/t-sql/statements/merge-transact-sql) operation will be performed instead.
3. **Rows** - each row specifies the column name and the corresponding values (except for reference data described below). The tooling will parse each column value according to the underying SQL type.

<br/>

#### Reference data

[*Reference Data*](../../docs/Reference-Data.md) is treated as a special case. The first column name and value pair are treated as the `Code` and `Text` columns. Also the `IsActive` column will automatically be set to `true`, and the `SortOrder` column to the index (1-based) in which it is specified. 

Where a column is a *Reference Data* reference the reference data code can be specified, with the identifier being determined at runtime (using a sub-query) as it is unlikely to be known at configuration time. The tooling determines this by the column name being suffixed by `Id` and a corresponding table name in the `Ref` schema; example `GenderId` column and corresponding table `Ref.Gender`.

Alternatively, a *Reference Data* reference could be the code itself, typically named XxxCode (e.g. `GenderCode`). This has the advantage of decoupling the reference data references from the underlying identifier. Where data is persisted as JSON then the **code** is used; this would ensure consistency. The primary disadvantage is that the **code** absolutely becomes _immutable_ and therefore not easily changed; for the most part this would not be an issue.

<br/>

#### Yaml configuration

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
- **No cross-schema referencing** - avoid referencing across `Schemas` where possible as this will impact the Migrations as part of this tooling; and we should not be using constraints as per prior point. Each schema is considered independent of others except in special cases, such as `dbo` or `sec` (security where used) for example.
- **Standardise column lengths** - use a standard set of column lengths within the database and have the business logic manage the length constraint. As such the column length must be the same or greater that what is required.
- **JSON for schema-less** - where there is data that needs to be persisted, but rarely searched on, a schema-less approach should be considered such that a JSON object is persisted versus having to define columns. This can further simplify the database requirements where the data is hierarchical in nature. To enable the [`ObjectToJsonConverter`](../../src/Beef.Core/Mapper/Converters/ObjectToJsonConverter.cs) should be used within the corresponding mapper (e.g. [`DatabasePropertyMapper`](../../src/Beef.Data.Database/DatabasePropertyMapper.cs)).

<br/>

## Console application

The `Beef.Database.Core` can be executed as a console application directly; however, the experience has been optimised so that a new console application can reference and inherit the capabilities. Then simply add the `Data`, `Migrations` and `Schema` folders and embed the required resources.

See the sample [`Beef.Demo.Database`](../../samples/Demo/Beef.Demo.Database) as an example.

<br/>

### Commands

The following [commands](DatabaseExecutorCommand.cs) are automatically enabled for the console application:

Command | Description
-|-
`Drop` | Drop the existing database (where it alredy exists).
`Create` | Create the database (where it does not already exist).
`Migrate` | Migrate the database using the **Migrations** scripts (those that have not already been executed).
`CodeGen` | Generates database **Schema** objects via code generation.
`Schema` | Drops and creates the known database **Schema** objects.
`Reset` | Resets the database by deleting all existing data.
`Data` | Inserts or merges **Data** from embedded YAML files.

The following are common combinations of the above.

Command | Description
-|-
`All` | Performs **all** commands as follows; `Create`, `Migrate`, `CodeGen`, `Schema` and `Data`.
`DropAndAll` | Performs `Drop` and `All`.
`ResetAndAll` | Performs `Reset` and `All`.
`Database` | Performs only the **database** commands as follows: `Create`, `Migrate`, `Schema` and `Data`.
`DropAndDatabase` | Performs `Drop` and `Database`.
`ResetAndDatabase` | Performs `Reset` and `Database`.

Additionally, there are a number of command line options that can be used.

Option | Description
-|-
`--connectionString` | Overrides the connection string for the database.
`--entry-assembly-only` | Overrides the assemblies to use the entry assembly only. This will avoid any dependent Scripts and Schema being (re-)invoked.
`--xmlToYaml` | Convert the XML configuration into YAML equivalent (will not codegen).
`--param` | Additional parameter with a `Name=Value` pair value.
`--script` | Overrides the script resource name.

<br/>

### New migration script file

To simplify the process for the developer _Beef_ enables the creation of new migration script files into the `Migrations` folder. This will name the script file correctly and output the basic SQL statements to perform the selected function. The date and time stamp will use [DateTime.UtcNow](https://docs.microsoft.com/en-us/dotnet/api/system.datetime.utcnow) as this should avoid conflicts where being co-developed across time zones. 

This requires the usage of the `ScriptNew` command, plus optional sub-command, and zero or more optional arguments (these are will depend on the sub-command). The optional arguments must appear in the order listed; where not specified it will appear as blank within the script file.

Sub-command | Argument(s) | Description
-|-
N/A | N/A | Creates a new empty skeleton script file.
`Create` | `Schema` and `Table` | Creates a new table create script file for the named schema and table.
`CreateRef` | `Schema` and `Table` | Creates a new reference data table create script file for the named schema and table.
`Alter` | `Schema` and `Table` | Creates a new table alter script file for the named schema and table.
`CdcDb` | N/A | Creates a new `sys.sp_cdc_enable_db` script file for the database.
`Cdc` | `Schema` and `Table` | Creates a new `sys.sp_cdc_enable_table` script file for the named schema and table.

Examples as follows.

```
dotnet run scriptnew
dotnet run scriptnew create Foo Bar
dotnet run scriptnew alter Foo Bar
dotnet run scriptnew createref Foo Gender
dotnet run scriptnew cdcdb
dotnet run scriptnew cdc Foo Bar
```

<br/>

### Environment Variable

Finally, the connection string can be overriden using an environment variable. This is useful where a developer can not use the default instance, or within automated deployments to a build or destination server where specifying the connection string on the command would not be considered good security practice.

The default environment variable is named `{Company}_{AppName}_ConnectionString`. Any `.` characters will be automatically replaced by an `_` character.

For example, where `Company` is `Foo.Bar` and `AppName` is `Blah`, then the environment variable would be `Foo_Bar_Blah_ConnectionString`.

There is an additional command line option to enable overriding of the environment variable name: `-evn` or `--environmentVariableName`

<br/>

### Program.cs

The `Program.cs` for the new console application should be updated similar to the following. The connection string is provided as the default used at runtime. An environment variable `{Company}_{AppName}_ConnectionString` can be updated to override (any `.` characters will be replaced with `_`); or alternatively use the command line option `-cs "connection-string-info"`. 

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
dotnet run scriptnew -createref Ref.Eyecolor
```

<br/>

## Direct / consolidated execution

The `Beef.Core.Database` can be invoked directly using the customized assemblies as a source to provide multiple Scripts and Schema within a single invocation. This is useful where needing to perform a single consolidated deployment versus invoking one-by-one.

One of the previously described [commands](DatabaseExecutorCommand.cs) is required. Additionally, a `connectionString` command is also required; unless the `--environmenVariableName` option is supplied to override.

 Additionally, there are the following command line options that can also be used.

Option | Description
-|-
`--assembly` | One or more [Assembly Names](https://docs.microsoft.com/en-us/dotnet/standard/assembly/names); being the assemblies that contain the required Scripts and Schema. These should be specified in the order in which they should be executed. Where the _Beef_ standard `dbo` objects should be added then the `Beef.Database.Core` assembly must also be specified.
`--schemaorder` | One or more Schema names in the order in which they should be executed (otherwise, the default is alphabetical). This provides an additional level of control in addition to the specified Assembly order.
`--param` | Additional parameter with a `Name=Value` pair value.