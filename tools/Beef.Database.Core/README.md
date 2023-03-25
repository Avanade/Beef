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

## Samples

The [`My.Hr`](../../samples/My.Hr/My.Hr.Database) and [`MyEf.Hr`](../../samples/MyEf.Hr/MyEf.Hr.Database) samples demonstrate usage.