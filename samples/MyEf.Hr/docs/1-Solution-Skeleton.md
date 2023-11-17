# Step 1 - Solution Skeleton

The starting point of any solution built on top of Beef is to first create the Visual Studio solution skeleton, utilizing the solution [template](../../templates/Beef.Template.Solution/README.md).

*To simplify the ongoing copy and paste activities within this sample it is highly recommended that the `MyEf.Hr` naming convention below is used.*

Open a development terminal, navigate to an appropriate parent folder and execute the following four commands to create the solution structure.

```
dotnet new install beef.template.solution --nuget-source https://api.nuget.org/v3/index.json
mkdir MyEf.Hr
cd MyEf.Hr
dotnet new beef --company MyEf --appname Hr --datasource SqlServer
```

The following solution structure will have been generated. Execute `.\MyEf.Hr.sln` to open in Visual Studio.

```
└── MyEf.Hr               # Solution that references all underlying projects
  └── Testing
    └── MyEf.Hr.Test      # Unit and intra-integration tests
  └── Tools
    └── MyEf.Hr.CodeGen   # Entity and Reference Data code generation console
    └── MyEf.Hr.Database  # Database code generation console
  └── MyEf.Hr.Api         # API end-point and operations
  └── MyEf.Hr.Business    # Core business logic components
  └── MyEf.Hr.Common      # Common / shared components
```

## Important Notes
Code generation should **not** be performed before updating the corresponding YAML files as described in the next sections. Otherwise, extraneous files will be generated that will then need to be manually removed.

Also, any files that start with `Person` (being the demonstration entity) should be removed (deleted) from their respective projects as they are encountered. This then represents the baseline to build up the solution from.

## Appendix

For more detail on the solution template see the following docs:
* [Solution Template](../../../templates/Beef.Template.Solution/README.md)
* [Getting started guide](../../../docs/Sample-SqlServer-EF-GettingStarted.md).