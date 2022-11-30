# Beef.Template.Solution

This is the capability to enable the initial creation of the _beef_ solution and all projects as defined by the [solution structure](../../docs/Solution-Structure.md). This leverages the .NET Core [templating](https://docs.microsoft.com/en-au/dotnet/core/tools/custom-templates) functionality.

<br/>

## Key attributes

There are two key attributes that drive the template; as well as the underlying [code-generation](../../tools/Beef.CodeGen.Core/README.md):
- **Company** - the company and product name (represents namespace and prefixing); e.g. `My.Company` or `Microsoft.Azure`, etc.
- **AppName** - the application / domain name (must be a single unique word); e.g. `App`, `Core`, `Sales`. There can be multiple `AppName`'s under a `Company` where supporting multiple business domains within a microservices-based architecture.

These are critical to the naming of the solution and its underlying projects. Once executed the following solution and underlying project structure will be created:

```
└── <root>
  └── Company.AppName.Api         # API end-point and operations
  └── Company.AppName.Business    # Core business logic components
  └── Company.AppName.CodeGen     # Entity and Reference Data code generation console
  └── Company.AppName.Common      # Common / shared components
  └── Company.AppName.Database    # Database and data management console
  └── Company.AppName.Test        # Unit and intra-integration tests
  └── Company.AppName.sln         # Solution file that references all above projects
```

<br/>

## Installation

Before the `Beef.Template.Solution` template can be used it must be installed from [NuGet](https://www.nuget.org/packages/Beef.Template.Solution/). The `dotnet new -i` command is used to perform this:

```
-- Use the latest published from NuGet...
dotnet new -i beef.template.solution --nuget-source https://api.nuget.org/v3/index.json

-- Or alternatively, point to a local folder...
dotnet new -i beef.template.solution --nuget-source C:\source\repos\Avanade\Beef\nuget-publish
```

<br/>

## Create Solution

To create the _Solution_ you must first be in the directory that you intend to create the artefacts within. The _beef_ template requires the `company` and `appname` attributes as discussed above; it is important that these are entered in your desired casing as they will be used as-is. 

Additionally, there is a futher optional `datasource` attribute to drive the desired output. This parameter supports the following values:
- `Database` (default),
- `EntityFramework`,
- `Cosmos`
- `HttpAgent`
- `None`

The `dotnet new` command is used to create, e.g.:

```
dotnet new beef --company My.Company --appname Sales

dotnet new beef --company My.Company --appname Sales --datasource Cosmos
```

 The following will be created:

```

└── <root>
  └── My.Company.Sales.Api         # API end-point and operations
  └── My.Company.Sales.Business    # Core business logic components
  └── My.Company.Sales.CodeGen     # Entity and Reference Data code generation console
  └── My.Company.Sales.Common      # Common / shared components
  └── My.Company.Sales.Database    # Database and data management console
  └── My.Company.Sales.Test        # Unit and intra-integration tests
  └── My.Company.Sales.sln         # Solution file that references all above projects
```

<br/>

## What is created?

The solution and projects created contain all the requisite .NET Classes and NuGet references to build a _beef_ solution.

_Note:_ the solution will **not** compile. There is a reference to `ReferenceDataProvider` within [`Startup.cs`](./content/Company.AppName.Api/Startup.cs) that does not initially exist (and needs to be created/generated).

To get the solution up and running quickly, an example `Person` entity and database table has been preconfigured for code generation. The next sub-sections describe content and how to perform the requisite generation. The entity-driven code-gen step will resolve the aforementioned compilation error.

<br/>

### Entity-driven code-gen

The `Company.AppName.CodeGen` project contains the following:
- [`Company.AppName.xml`](./content/Company.AppName.CodeGen/Company.AppName.xml) - a basic `Person` entity example is pre-configured; this can either be extended or replaced.
- [`Company.RefData.xml`](./content/Company.AppName.CodeGen/Company.RefData.xml) - a basic `Gender` reference data entity example is pre-configured; this can either be extended or replaced.

To perform the code generation, first navigate to the directory where the above files reside, then execute the following:

```
dotnet run all
``` 

For more information see: [code-generation](../../tools/Beef.CodeGen.Core/README.md).

<br/>

### Database table-driven code-gen

The `Company.AppName.Database` project contains the following:
- [`Company.AppName.Database.xml`](./content/Company.AppName.Database/Company.AppName.Database.xml) - the basic `Person` and `Gender` stored procedure examples are pre-configured; these can either be extended or replaced.
- [`RefData.xml`](./content/Company.AppName.Database/Data/RefData.yaml) - the example `Gender` data has been preconfigured; these can either be extended or replaced.
- [`Company.AppName.Database/Migrations`](./content/Company.AppName.Database/Migrations) - the example scripts for create the requite schema and table for `Person` and `Gender`; these can either be extended or replaced.

To perform the data generation, first navigate to the directory where the above files reside, then execute the following:

```
dotnet run all
``` 

For more information see: [data-generation](../../tools/Beef.Database.Core/README.md).

<br/>

## Samples

See the following for example end-to-end usage:
- [Cosmos sample](../../docs/Sample-Cosmos-GettingStarted.md)