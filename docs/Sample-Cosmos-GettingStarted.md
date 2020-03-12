# Getting started

This tutorial will demonstrate how to get a .NET Solution using _Beef_ created on your machine connecting to a local Azure Cosmos DB Emulator. A pre-configured soluion will be created to enable, and demonstrate, the key end-to-end capabilities. 

<br/>

## Prerequisites

The following need to be installed locally for the sample to run:

- Download and install the **Azure Cosmos DB Emulator**: https://docs.microsoft.com/en-us/azure/cosmos-db/local-emulator

It is recommended that the following is installed to simplify the opening of a command line from Visual Studio:
- Download and install the **Open Command Line** extension for Visual Studio: http://vsixgallery.com/extension/f4ab1e64-5d35-4f06-bad9-bf414f4b3bbb/. The use of `Alt+Space` will open a command line (or PowerShell) in the directory of the open file.

<br/>

## Beef Template Solution

The [`Beef.Template.Solution`](../templates/Beef.Template.Solution/README.md) needs to be installed so that it can be used to easily create the required [solution structure](./Solution-Structure.md).

Install (or update) the latest template from the public [NuGet](https://www.nuget.org/packages/Beef.Template.Solution/) repository using the `dotnet new -i` command as follows:

```
dotnet new -i beef.template.solution --nuget-source https://api.nuget.org/v3/index.json
``` 

<br/>

## Create solution

To create the _Solution_ you must first be in the directory that you intend to create the artefacts within. The _beef_ template requires the `company` and `appname`; which is also the recommended directory name (it will also represent the .NET namespace). For this tutorial we will also choose the `Cosmos` data source.

```
mkdir Foo.Bar
cd Foo.Bar
dotnet new beef --company Foo --appname Bar --datasource Cosmos
```

The solution should now have been created; and the file system should look like the following:

```
└── Foo.Bar
  └── Foo.Bar.Api         # API end-point and operations
  └── Foo.Bar.Business    # Core business logic components
  └── Foo.Bar.CodeGen     # Entity and Reference Data code generation console
  └── Foo.Bar.Common      # Common / shared components
  └── Foo.Bar.Test        # Unit and intra-integration tests
  └── Foo.Bar.sln         # Solution file that references all above projects
```

<br/>

## Code-generation

The solution has been created with a sample `Person` entity defined and related [reference data](./Reference-Data.md) to demonstrate the code generation configuration. There are other `Person` related classes within the solutiom to demonstrate the corresponding non-generated interactions, as well as the [intra-integration testing](../tools/Beef.Test.NUnit/README.md).

The [code-generation](../tools/Beef.CodeGen.Core/README.md) will reference the following configuration within the `Foo.Bar.CodeGen` directory:
- `Foo.Bar.xml` - contains the entity(s) configuration.
- `Foo.RefData.xml` - contains the reference data configuration.

Generate the configured entities and reference data:

```
cd Foo.Bar.CodeGen
dotnet new all
```

This will build and run the `Foo.Bar.CodeGen` console; the outcome of the code generation will be logged to the console showing what was added or updated.

<br/>

## Testing

To verify that the generated APIs function as expected an example set of tests has been created to exercise the GET/PUT/POST/[PATCH](./Http-Patch.md)/DELETE operations:

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