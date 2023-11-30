# Beef.Database.Core

The `Beef.Database.Core` tool is a console application provided to automate the management of a relational database as part of the end-to-end development process. 

The primary/core capabilities are enabled by [`DbEx`](https://github.com/Avanade/DbEx); see [documentation](https://github.com/Avanade/DbEx/blob/main/README.md) for details.

<br/>

## Database provider

Depending on the underlying database provider specific capabilities are enabled; see:

- [SQL Server](../Beef.Database.SqlServer)
- [MySQL](../Beef.Database.MySql)

</br>

## Code-generation

The [`MigrationConsoleBase<TSelf>`](./MigratorConsoleBase.cs) extends the _DbEx_ capabilities enabling additional code-generation phase (see [`OnRamp`](https://github.com/Avanade/OnRamp)) that is used to generate either _Beef_-enabled database or C# code depending on the underlying database provider and/or configuration.

The respective code-generation _Templates_ are housed as embedded resources within the respective project's `Templates` folder.

<br/>

### Generate corresponding entity configuration

The database code-generation supports a `yaml` sub-command that will generate the basic  [entity code-generation](../Beef.CodeGen.Core/README.md) YAML configuration by inferring the database configuration for the specified tables into a temporary `temp.entity.beef-5.yaml` file. Additionally, an initial C# validator will also be generated for each table.

The developer is then responsible for the copy+paste of the required yaml into the `entity.beef-5.yaml` and `refdata.beef-5.yaml` file(s) and further amending as appropriate. After use, the developer should remove the `temp.entity.beef-5.yaml` file as it is otherwise not referenced by the code-generation. 

This by no means endorses the direct mapping between entity and database model as the developer is still encouraged to reshape the entity to take advantage of object-orientation and resulting JSON capabilities.

The following provides the help content for the `yaml` sub-command:

```
codegen yaml <Schema> <Table> [<Table>...]   Creates a temporary Beef entity YAML file for the specified table(s).
                                             - A table name with a prefix ! denotes that no CRUD operations are required.
                                             - A table name with a prefix * denotes that a 'GetByArgs' operation is required.
```

An example is as follows:

```
dotnet run codegen yaml Hr Gender *Employee !EmergencyContact TerminationReason
```

<br/>

## Samples

The [`My.Hr`](../../samples/My.Hr/My.Hr.Database) and [`MyEf.Hr`](../../samples/MyEf.Hr/MyEf.Hr.Database) samples demonstrate usage.
