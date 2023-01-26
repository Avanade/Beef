# .NET Solution Structure

The _Beef_ code-generation framework is somewhat opinionated around the naming of the .NET projects, their underlying .NET namespaces, and the names of the generated files themselves.

There are two key attributes within the [code-generation](../tools/Beef.CodeGen.Core/README.md) that drive the naming:
- **Company** - the company and product name.
- **AppName** - the application / domain name.

The project names and underlying namespaces must be named `Company.AppName.Yyy` where `Yyy` is the name of the layer / component. Files within typically follow a specific convention where `Xxx` is the key entity (or equivalent) name.

<br>

## Code-generation separation

Given the extensive usage of code generation, the generated artefacts are stored in a `Generated` folders to separate. These generated artefacts should **only** be created and updated by the tooling. Where a file is no longer required it will need to be deleted manually. 

Hand-crafted, custom, artefacts should reside in the appropriate parent folder to physically separate from the generated.

<br>

## Tiering and layering

The following represents the tiering and layering for the architecture; and therefore the required .NET solution structure.

![Layers](./images/Layers.png)

<br>

## Solution projects

The underlying Visual Studio solution project structure should generally be as follows:

```
└── <root>
  └── Testing
    └── Company.AppName.Test      # Unit and intra-integration tests
  └── Tools
    └── Company.AppName.CodeGen   # Entity and Reference Data code generation console
    └── Company.AppName.Database  # Database and data management console
  └── Company.AppName.Api         # API end-point and operations
  └── Company.AppName.Business    # Core business logic components
  └── Company.AppName.Common      # Common / shared components
```

There a three primary run-time projects that make up the core of a solution based on _Beef_; including their dependencies (&dagger; denotes optional as required):

Project | Description | Dependencies
-|-|-
`Company.AppName.Api` | API end-point and operations: **Service interface**. This one of many possible host for the business logic; in this instance providing the HTTP RESTful endpoints. | `Company.AppName.Business` <br/>`Company.AppName.Common`
`Company.AppName.Business` | Core business logic components: **Domain logic**, **Service orchestration** and **Data access**. This contains the internal business and data logic (intellectual property) and should not be shared. | `CoreEx` <br/> `CoreEx.Database.*`&dagger; <br> `CoreEx.EntityFrameworkCore`&dagger; <br/> `CoreEx.Validation`&dagger;
`Company.AppName.Common` | Common / shared components: **Entity** and **Service Agent**. These can be shared by any consumer as they only define the entity (contract) and an agent to consume the underlying API. | `CoreEx`

<br>

Additionally, there are up to two tooling projects, and a testing project:

Project | Description | Dependencies
-|-|-
`Company.AppName.CodeGen` | Entity and Reference Data code generation console tool. | `Beef.CodeGen.Core`
`Company.AppName.Database` | Database and data management console tool. | `Beef.Database.*` <br/> `Beef.CodeGen.Core`
`Company.AppName.Test` | Unit and intra-integration tests. | `UnitTestEx.*` <br/> `Company.AppName.Api` <br/> `Company.AppName.Database`

<br>

### Company.AppName.Common

This is a _.NET **Standard** Class Library_ project and contains the common reusuable components; specifically the `Entities` and `Agents`. This assembly could be shared externally as part of an SDK as it largely encapsulates the logic to consume the underlying APIs and the entities that form the contracts. 

It is a generally a [_.NET Standard_](https://docs.microsoft.com/en-us/dotnet/standard/net-standard) project (althought it does not have to be) so it can be consumed by any number of .NET platforms.

The underlying project folder structure will be as follows (the code generation will create on first execution where they do not already exist):

```
└── <root>                    # Shared / common code
  └── Entities                # Custom entities (Xxx.cs)
    └── Generated             # Generated entities (Xxx.cs)
  └── Agents                  # Custom agents (XxxAgent.cs)
    └── Generated             # Generated agents (XxxAgent.cs)
```

<br>

### Company.AppName.Business

This is a _.NET Class Library_ project and contains the primary domain business logic; being the [Domain logic](./Layer-Manager.md), [Service orchestration](./Layer-DataSvc.md) and Data access.

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

This is an _ASP.NET Web Application_ project and contains the Web API logic; being the [Service interface](./Layer-ServiceInterface.md). When creating the project manually in _Microsoft Visual Studio_ make sure that the **API** option is selected.

The underlying project folder structure will be as follows (the code generation will create on first execution where they do not already exist):

```
└── <root>                    # Program.cs, Startup.cs, etc.
  └── Controllers             # Custom controllers (XxxController.cs)
    └── Generated             # Generated controllers (XxxController.cs)
```

The `Startup.cs` file will register all required services to execute; and perform any other start-up activities. 

<br>

### Company.AppName.CodeGen

This is a _.NET Console Application_ project and contains the configuration for the **Entity-driven** [code generation](../tools/Beef.CodeGen.Core/README.md) and the program execution stub to run.

The underlying project folder structure will be as follows:

```
└── <root>
  └── entity.beef-5.yaml      # Primary entity code-gen config
  └── refdata.beef-5.yaml     # Optional reference data code-gen config
  └── datamodel.beef-5.yaml   # Optional data model code-gen config
  └── Program.cs              # Standard code to execure code-gen
```

The `Program.cs` should contain this standardized C# code stub setting the `Company` and `AppName` to enable the correct substitution within the generated artefacts:

``` csharp
public static class Program
{
    /// <summary>
    /// Main startup.
    /// </summary>
    /// <param name="args">The startup arguments.</param>
    /// <returns>The status code whereby zero indicates success.</returns>
    public static Task<int> Main(string[] args) => Beef.CodeGen.CodeGenConsole.Create("Compny", "AppName").Supports(entity: true, refData: true).RunAsync(args);
}
```

<br>

### Company.AppName.Database

This is a _.NET Console Application_ project and contains the database schema, data, and table-driven [code generation](../tools/Beef.CodeGen.Core/README.md) configuration. Also includes the program execution stub to run.

The underlying project folder structure will be as follows:

```
└── <root>
  └── database.beef-5.yaml            # Primary database/table code-gen config
  └── Program.cs                      # Standard code to execure code-gen
  └── Data                            # Data load definition files: *.yaml
  └── Migrations                      # One-time SQL migration scripts: *.sql
  └── Schema                          # Schema related scripts
    └── SSS                           # Folder (optional) structure per schema...
      └── Functions                   # Custom SQL functions
      └── Stored Procedures           # Custom SQL stored procedures
        └── Generated                 # Generated SQL stored procedures
      └── Types                       # Type related scripts
        └── User-Defined Table Types  # Custom SQL user defined table types
          └── Generated               # Generated SQL user defined table types
      └── Views                       # Custom SQL views
        └── Generated                 # Generated SQL views
```

The `Program.cs` should contain this basic C# code stub setting the `ConnectionString`, `Company` and `AppName` to enable the correct substitution within the generated artefacts. The `ConfigureMigrationArgs` is separated as it is also used by the intra-domain tests to ensure consistency of `MigrationArgs` set-up:

``` csharp
public class Program
{
    /// <summary>
    /// Main startup.
    /// </summary>
    /// <param name="args">The startup arguments.</param>
    /// <returns>The status code whereby zero indicates success.</returns>
    public static Task<int> Main(string[] args) => MySqlMigrationConsole
        .Create("Server=localhost; Port=3306; Database=paint_tracker; Uid=dbuser; Pwd=dbpassword;", "Company", "AppName")
        .Configure(c => ConfigureMigrationArgs(c.Args))
        .RunAsync(args);

    /// <summary>
    /// Configure the <see cref="MigrationArgs"/>.
    /// </summary>
    /// <param name="args">The <see cref="MigrationArgs"/>.</param>
    /// <returns>The <see cref="MigrationArgs"/>.</returns>
    public static MigrationArgs ConfigureMigrationArgs(MigrationArgs args) => args.AddAssembly<Program>();
}
```

<br/>

### Company.AppName.Test

This is a _.NET Unit Test (Class Library)_ project that by default leverages [NUnit](https://github.com/nunit/nunit) framework to enable. This also uses the [UnitTestEx](https://github.com/Avanade/unittestex) to enable (simplify) the testing of the intra-integration tests.