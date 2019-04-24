# .NET Solution Structure

The _Beef_ code-generation framework is somewhat opinionated around the naming of the .NET projects, their underlying .NET namespaces, and the names of the generated files themselves.

There are two key attributes within the [code-generation](../tools/Beef.CodeGen.Core/README.md) that drive the naming:
- **Company** - the company name.
- **AppName** - the application / domain name.

The project names and underlying namespaces must be named `Company.AppName.Yyy` where `Yyy` is the name of the layer / component. Files within typically follow a specific convention where `Xxx` is the key entity (or equivalent) name.

<br>

## Code-generation separation

Given the extensive usage of code generation, the generated artefacts are stored in a `Generated` folders to separate. These generated artefacts should **only** be created and updated by the tooling. Where a file is no longer required it will need to be deleted manually. 

Hand-crafted, custom, artefacts should reside in the appropriate parent folder to physically separate from the generated.

<br>

## Tiering and layering

The following represents the tiering and layering for the architecture; and there for the required .NET solution structure.

![Layers](./images/Layers.png)

<br>

## Solution projects

There a three primary run-time projects that make up the core of a solution based on _Beef_; including their dependencies (* denotes optional as required):

Project | Description | Dependencies
-|-|-
`Company.AppName.Common` | Common / reusable components: **Entity** and **Service Agent**. These can be shared by any consumer as they only define the entity (contract) and an agent to consume the underlying API. | `Beef.Core`
`Company.AppName.Business` | Core business logic components: **Domain logic**, **Service orchestration** and **Data access**. This contains the internal business and data logic (intellectual property) and should not be shared. | `Company.AppName.Common` <br>	`Beef.Core` <br> `Beef.Data.Database`* <br> `Beef.Data.EntityFrameworkCode`* <br> `Beef.Data.OData`*
`Company.AppName.Api` | API end-point and operations: **Service interface**. This one of many possible host for the business logic; in this instance providing the HTTP RESTful endpoints. | `Company.AppName.Business` <br>`Beef.Core` <br> `Beef.AspNetCore.WebApi`

<br>

Additionally, there are two tooling projects, and a testing project:

Project | Description | Dependencies
-|-|-
`Company.AppName.CodeGen` | Entity and Reference Data code generation console tool. | `Beef.CodeGen.Core`
`Company.AppName.Database` | Database and data management console tool. | `Beef.Database.Core`
`Company.AppName.Test` | Unit and intra-integration tests. | `Beef.Test.NUnit` <br/> `Company.AppName.Api` <br/> `Company.AppName.Database`

<br>

### Company.AppName.Common

This is a _.NET **Standard** Class Library_ project and contains the common reusuable components; specifically the `Entities`, `Agents` and `ServiceAgents`. This assembly could be shared externally as part of an SDK as it largely encapsulates the logic to consume the underlying APIs and the entities that form the contracts. 

It is a generally a [_.NET Standard_](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) project (althought it does not have to be) so it can be consumed by any number of .NET platforms.

The underlying project folder structure will be as follows (the code generation will create on first execution where they do not already exist):

```
└── <root>                    # Shared / common code
  └── Entities                # Custom entities (Xxx.cs)
    └── Generated             # Generated entities (Xxx.cs)
  └── Agents                  # Custom agents (XxxAgent.cs)
    └── Generated             # Generated agents (XxxAgent.cs)
    └── Service Agents        # Custom service agents (XxxServiceAgent.cs)
      └── Generated           # Generated service agents (XxxServiceAgent.cs)
```

<br>

### Company.AppName.Business

This is a _.NET **Core** Class Library_ project and contains the primary domain business logic; being the [Domain logic](./Layer-Manager.md), [Service orchestration](./Layer-DataSvc.md) and Data access.

The underlying project folder structure will be as follows (the code generation will create on first execution where they do not already exist):

```
└── <root>                    # Custom manager (XxxManager.cs)
  └── Generated               # Generated manager (XxxManager.cs)
  └── Data                    # Custom data access (XxxData.cs)
    └── Generated             # Generated data access (XxxData.cs)
  └── DataSvc                 # Custom data service (XxxDataSvc.cs)
    └── Generared             # Generated data service (XxxDataSvc.cs)
  └── Validation              # Custom validation (XxxValidator.cs)
```

<br>

### Company.AppName.Api

This is an _ASP.NET **Core** Web Application_ project and contains the Web API logic; being the [Service interface](./Layer-ServiceInterface.md). When creating the project in _Microsoft Visual Studio_ make sure that the **API** option is selected.

The underlying project folder structure will be as follows (the code generation will create on first execution where they do not already exist):

```
└── <root>                    # Program.cs, Startup.cs, etc.
  └── Controllers             # Custom controllers (XxxController.cs)
    └── Generated             # Generated controllers (XxxController.cs)
```

The `Startup.cs` file will need additional start up activities performed to ensure the correct usage and execution of _Beef_; the primary activities are listed (see the `Beef.Demo.WebApi` project as an example):
- Set up of the `ExecutionContext`.
- Unhandled `Exception` management.
- Connections (for the likes of databases) etc. as needed by underlying data components. 

<br>

### Company.AppName.CodeGen

This is a _.NET **Core** Console Application_ project and contains the configuration for the **Entity-driven** [code generation](./Beef-CodeGen-Core.md) and the program execution stub to run.

The underlying project folder structure will be as follows:

```
└── <root>
  └── Company.AppName.xml     # Primary entity code-gen config
  └── Company.RefData.xml     # Optional reference data code-gen config
  └── Program.cs              # Standard code to execure code-gen
```

The `Program.cs` should contain this standardised C# code stub setting the `Company` and `AppName` to enable the correct substitution within the generated artefacts:

``` csharp
class Program
{
    static int Main(string[] args)
    {
        return CodeGenConsoleWrapper.Create("Company", "AppName").Supports(true, false, true).Run(args);
    }
}
```

<br>

### Company.AppName.Database

This is a _.NET **Core** Console Application_ project and contains the database schema, data, and table-driven [code generation](./Beef-CodeGen-Core.md) configuration. Also includes the program execution stub to run.

The underlying project folder structure will be as follows:

```
└── <root>
  └── Company.AppName.Database.xml    # Primary database/table code-gen config
  └── Program.cs                      # Standard code to execure code-gen
  └── Data                            # Data load definition files: *.yaml
  └── Migrations                      # One-time SQL migration scripts: *.sql
  └── Schema                          # Schema related scripts
    └── SSS                           # Folder structure per schema...
      └── Functions                   # Custom SQL functions
      └── Stored Procedures           # Custom SQL stored procedures
        └── Generated                 # Generated SQL stored procedures
      └── Types                       # Type related scripts
        └── User-Defined Table Types  # Custom SQL user defined table types
          └── Generated               # Generated SQL user defined table types
      └── Views                       # Custom SQL views
        └── Generated                 # Generated SQL views
```

The `Program.cs` should contain this basic C# code stub setting the `ConnectionString`, `Company` and	`AppName` to enable the correct substitution within the generated artefacts:

``` csharp
public class Program
{
    static int Main(string[] args)
    {
        return DatabaseConsoleWrapper.Create("Data Source=.;Initial Catalog=DBNAME;Integrated Security=True", "Company", "AppName").Run(args);
    }
}
```