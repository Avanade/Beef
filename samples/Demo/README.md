# Beef.Demo

This sample represents an end-to-end demonstration of the usage of _Beef_; as well as serving as the primary test vehicle of the overall code-generation and supporing execution framework.

</br>

## Naming

There are two key attributes within the [code-generation](../../tools/Beef.CodeGen.Core/README.md) that drive the naming:
- **Company** - the company name - for this sample we use: _**Beef**_
- **AppName** - the application name - for this sample we use: _**Demo**_

This sample has been set up and named in accordance with the documented [solution structure](../../docs/Solution-Structure.md).

</br>

## Beef.Demo.CodeGen

This is the primary entity-based [code generation](../../tools/Beef.CodeGen.Core/README.md) console that is at the heart of _Beef_. 

A number of sample entities have been configured to demonstrate:
- [`Beef.Demo.xml`](./Beef.Demo.CodeGen/Beef.Demo.xml) - business entity configuration:
  - `Person` - demonstrates ADO.NET and EntityFramework CRUD database activities.
  - `Product` - demonstrates ODATA queries against an example ODATA [endpoint](http://services.odata.org/V4/OData/OData.svc/).
  - `CustomerGroup` - demonstrates ODATA CRUD activities against [Dynamics 365](https://dynamics.microsoft.com/finance-and-operations/overview/).
- [`Beef.RefData.xml`](./Beef.Demo.CodeGen/Beef.RefData.xml) - reference data configuration.

The [`Program.cs`](./Beef.Demo.CodeGen/Program.cs) demonstrates how to configure the company and application name.

To test try the following command line executions:

```
dotnet run entity        # Generates only the entity using Beef.Demo.Xml
dotnet run refdata       # Generates only the reference data using Beef.RefData.Xml
dotnet run all           # Generates all (both entity and reference data)
```

_Note:_ Database code generation can be driven from this project also; this is where the richer database and data manangement capabilities are not required. 

</br>

## Beef.Demo.Database

This is the optional database and data management console used where a SQL Server database is required and the documented [approach](../../tools/Beef.Database.Core/README.md) is required.

The following demonstrates usage:
 - [`Beef.Demo.Database.xml`](./Beef.Demo.Database/Beef.Demo.Database.xml)
 - [`Data/RefData.yaml`](./Beef.Demo.Database/Data/RefData.yaml)
 - [`Migrations/*`](./Beef.Demo.Database/Migrations) - demonstrates the scripts required to create the required schemas and tables.
 - [`Schema/*`](./Beef.Demo.Database/Schema) - demonstrates the `Demo` and `Ref` schemas and underlying stored procedures, etc. that were generated.

The [`Program.cs`](./Beef.Demo.Database/Program.cs) demonstrates how to configure the company, application name and default connection string.

To test try the following command line executions:

```
dotnet run all           # Database Create, Migrate, CodeGen, Schema and Data.
dotnet run drop          # Database Drop if it already exists.
dotnet run codegen       # Generates only the database Schema objects.
```

</br>

## Beef.Demo.Test

This demonstrates the intra-integration tests; try running the tests and ensure they all pass. 
