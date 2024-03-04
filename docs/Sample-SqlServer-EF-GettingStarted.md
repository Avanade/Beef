# Getting started

This tutorial will demonstrate how to get a .NET Solution using _Beef_ created on your machine connecting to a local Microsoft SQL Server using [Entity Framework](https://docs.microsoft.com/en-us/ef/core/) for data access. A pre-configured soluion will be created to enable, and demonstrate, the key end-to-end capabilities. 

<br/>

## Prerequisites

The following need to be installed locally for the sample to run:

- Download and install the **Microsoft SQL Server Developer edition**: https://www.microsoft.com/en-us/sql-server/sql-server-downloads

It is recommended that the following is installed to simplify the opening of a command line from Visual Studio:
- Download and install the **Open Command Line** extension for Visual Studio: http://vsixgallery.com/extension/f4ab1e64-5d35-4f06-bad9-bf414f4b3bbb/. The use of `Alt+Space` will open a command line (or PowerShell) in the directory of the open file.

<br/>

## Beef Template Solution

The [`Beef.Template.Solution`](../templates/Beef.Template.Solution/README.md) needs to be installed so that it can be used to easily create the required [solution structure](./Solution-Structure.md).

Install (or update) the latest template from the public [NuGet](https://www.nuget.org/packages/Beef.Template.Solution/) repository using the [`dotnet new install`](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-new-install) or `dotnet new -i` ([deprecated](https://github.com/dotnet/docs/issues/32195)) command as follows (or alternatively specify the required version):

```
dotnet new install beef.template.solution --nuget-source https://api.nuget.org/v3/index.json
dotnet new install beef.template.solution::5.0.1.preview7 --nuget-source https://api.nuget.org/v3/index.json
``` 

<br/>

## Create solution

To create the _Solution_ you must first be in the directory that you intend to create the artefacts within. The _beef_ template requires the `company` and `appname`; which is also the recommended directory name (it will also represent the .NET namespace). For this tutorial we will also choose the `SqlServer` data source.

```
mkdir Foo.Bar
cd Foo.Bar
dotnet new beef --company Foo --appname Bar --datasource SqlServer
```

The solution should now have been created; and the file system should look like the following:

```
└── Foo.Bar
  └── Foo.Bar.Api         # API end-point and operations
  └── Foo.Bar.Business    # Core business logic components
  └── Foo.Bar.CodeGen     # Entity and Reference Data code generation console
  └── Foo.Bar.Database    # Database setup and configuration
  └── Foo.Bar.Common      # Common / shared components
  └── Foo.Bar.Test        # Unit and intra-integration tests
  └── Foo.Bar.sln         # Solution file that references all above projects
```

<br/>

## Code-generation

The solution has been created with a sample `Person` entity defined and related [reference data](./Reference-Data.md) to demonstrate the code generation configuration. There are other `Person` related classes within the solutiom to demonstrate the corresponding non-generated interactions, as well as the [intra-integration testing](../tools/Beef.Test.NUnit/README.md).

The [code-generation](../tools/Beef.CodeGen.Core/README.md) will reference the following configuration within the `Foo.Bar.CodeGen` directory:
- `entity.beef-5.yaml` - contains the entity(s) configuration.
- `refdata.beef-5.yaml` - contains the reference data configuration.

Generate the configured entities and reference data by performing the following:

```
cd Foo.Bar.CodeGen
dotnet run all
```

This will build and run the `Foo.Bar.CodeGen` console; the outcome of the code generation will be logged to the console showing what was added or updated.

<br/>

## Database generation and configuration

The solution has been created with the sample `Person` table defined and related reference data tables, migration scripts to create the database objects.

The [database generation](../tools/Beef.Database.Core/README.md) will reference the following configuration within the `Foo.Bar.Database` directory:
- `database.beef-5.yaml` - contains the table(s) and related C# model configuration.

Generate the configured tables and C# models by performing the following:

```
cd Foo.Bar.Database
dotnet run all
```

This will build and run the `Foo.Bar.Database` console; the outcome of the code generation and database setup/configuration will be logged to the console showing what was added or updated.

<br/>

## Testing

To verify that the generated APIs function as expected an example set of tests has been created to exercise the GET/PUT/POST/PATCH/DELETE operations:

```
cd ..\Foo.Bar.Test
dotnet test Foo.Bar.Test.csproj
``` 

<br/>

## Visual Studio

Open the solution within Visual Studio:

```
cd ..
Foo.Bar.sln
``` 

<br/>

## Subscribing Services

Within a microservices context, where event-based message streaming is used, the templated code from above instantiates the foundation. To further implement this pattern then a domain will likely also need to subscribe to events. An additional `dotnet new beef ... --services AzFunction` switch will also create the required Azure Function _Services_ project and related configuration; including an example unit test. The template will also further implement with an expectation that the event-based messaging infrastructure is Azure Service Bus.

```
dotnet new beef --company Foo --appname Bar --datasource SqlServer --services AzFunction
```

The solution should now have been created; and the file system should look like the following:

```
└── Foo.Bar
  └── Foo.Bar.Api             # API end-point and operations
  └── Foo.Bar.Business        # Core business logic components
  └── Foo.Bar.CodeGen         # Entity and Reference Data code generation console
  └── Foo.Bar.Database        # Database setup and configuration
  └── Foo.Bar.Common          # Common / shared components
  └── Foo.Bar.Test            # Unit and intra-integration tests for Business and Api
  └── Foo.Bar.Services        # Azure Function Services **NEW**
  └── Foo.Bar.Services.Test   # Unit and intra-integration tests for Services **NEW**
  └── Foo.Bar.sln             # Solution file that references all above projects
```