# Upgrading to .NET Core 3.1

The _Beef_ solution has been upgraded to .NET Core 3.1. As this is a major upgrade to .NET the opportunity to introduce changes (some breaking) has also occured within _Beef_ itself.

<br/>

## Versioning

As a result of the upgrade all version numbers for the `Beef` projects (and corresponding NuGet packages) will now follow a **new** standard convention. Being the [.NET Major and Minor version](https://docs.microsoft.com/en-us/dotnet/core/versions/), plus the corresponding Beef version as it relates to the Major+Minor combination.

Therefore, the versioning scheme will be: `[DotNet_MajorVersion].[DotNet_MinorVersion].[Beef_Version]`. As such, the initial version number for all artefacts will be: `3.1.1`.

<br/>

## Major changes

The following major changes have occured:

- All projects now take a dependency on either [.NET Standard 2.1 or .NET Core 3.1](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version) as applicable, which results in the C# language version of `v8.0`. A number of new C# `v8.0` capabilities are being leveraged in the solution.
- Nullable reference types - all the _Beef_ projects with the exception of `Beef.Data.OData` (scheduled) have introduced [nullable reference types](https://devblogs.microsoft.com/dotnet/embracing-nullable-reference-types/) throughout.
    - All c# code-generation templates have been updated to be nullable reference type aware. The new `#nullable enable` and `#nullable restore` encompass the generated code so as to not enforce usage in the consuming code until the consumer is ready to introduce holistically; i.e. can coexist side-by-side.
- Async everywhere - where identified, long-running actions, will now function asynchronously. For the most part the pre-existing synchronous capability has been removed which may break existing consumers where leveraging. In this instance look for the corresponding asynchronous method and invoke accordingly; e.g. `Run` is now `RunAsync`.
    - The `Beef.Data.Database` assembly/package has had the most significant impact from this change; all existing database operations have been made asynchronous. 
- All dependent NuGet packages have been updated to their latest versions.

<br/>

## Change Log

Review the `CHANGELOG.md` for each project/assembly to review all the changes made.

<br/>

## Upgrading existing

The following walks through the high-level process of upgrading an existing .NET Core 2.1/2.2 solution leveraging _Beef_.

<br/>

### 1. All projects

First step is to update all projects:
- To the latest .NET version; and,
- Update all corresponding NuGet packages.

<br/>

#### 1.1 .NET version

Update the .NET version to either .NET Standard 2.1 or .NET Core 3.1 by updating the projects `csproj` file

_Remove_ either of the following:

``` xml
    <TargetFramework>netstandard2.0</TargetFramework> 
    <TargetFramework>netcoreapp2.2</TargetFramework>
```

And, _replace_ with the corresponding:

``` xml
    <TargetFramework>netstandard2.1</TargetFramework> 
    <TargetFramework>netcoreapp3.1</TargetFramework>
```

<br/>

#### 1.2 Latest NuGet packages

Update all packages using the `Manage NuGet Packages...` function for each project within Visual Studio 2019. Update all packages to the latest. 

<br/>

### 2. Company.AppName.CodeGen

The code-generation must be updated:
- To support asynchronous execution; and,
- Re-generate all artefacts from the latest templates - this will result in all generated code being updated to take advantage of the latest language and _Beef_ features. Note: that the entities will no longer output `Property_Xxx` constants; where these were previously leveraged update with the C# `nameof(Xxx)` syntax as this will perform the same function in a more C# language native manner.

<br/>

#### 2.1 Async execution

Make the following changes to `Program.cs`, change from the previous synchronous code:

``` csharp
static int Main(string[] args)
{
    return CodeGenConsoleWrapper.Create("Company", "AppName").Supports(entity: true, refData: true, dataModel: true).Run(args);
}
```

<br/>

To the new asynchronous code:

 ``` csharp
using System.Threading.Tasks;

...

static Task<int> Main(string[] args)
{
    return CodeGenConsoleWrapper.Create("Company", "AppName).Supports(entity: true, refData: true, dataModel: true).RunAsync(args);
}
```

<br/>

### 3. Company.AppName.Database

The code-generation must be updated to support asynchronous execution. Perform the same changes as described above.

<br/>

### 4. Company.AppName.Business

Where using database access, the existing custom partial classes will need to be updated for the removed `Property_xxx` consts and new asynchronous operations/methods.

An example of the before code:

``` csharp
private void GetByArgsOnQuery(DatabaseParameters p, PersonArgs args, IDatabaseArgs dbArgs)
{
    p.ParamWithWildcard(args?.FirstName, DbMapper.Default[Person.Property_FirstName])
     .ParamWithWildcard(args?.LastName, DbMapper.Default[Person.Property_LastName])
     .TableValuedParamWith(args?.Genders, "GenderIds", () => TableValuedParameter.Create(args.Genders.ToGuidIdList()));
}

...

private Task<PersonDetail> GetDetailOnImplementationAsync(Guid id)
{
    PersonDetail pd = null;

    Database.Default.StoredProcedure("[Demo].[spPersonGetDetail]")
        .Param(DbMapper.Default.GetParamName(PersonDetail.Property_Id), id)
        .SelectQueryMultiSet(
            new MultiSetSingleArgs<Person>(PersonData.DbMapper.Default, (r) => { pd = new PersonDetail(); pd.CopyFrom(r); }, isMandatory: false),
            new MultiSetCollArgs<WorkHistoryCollection, WorkHistory>(WorkHistoryData.DbMapper.Default, (r) => pd.History = r));

    return Task.FromResult(pd);
}
```

<br/>

An example of the corresponding after code:

``` csharp
private void GetByArgsOnQuery(DatabaseParameters p, PersonArgs args, IDatabaseArgs dbArgs)
{
    p.ParamWithWildcard(args?.FirstName, DbMapper.Default[nameof(Person.FirstName)])
     .ParamWithWildcard(args?.LastName, DbMapper.Default[nameof(Person.LastName)])
     .TableValuedParamWith(args?.Genders, "GenderIds", () => TableValuedParameter.Create(args.Genders.ToGuidIdList()));
}

...

private async Task<PersonDetail> GetDetailOnImplementationAsync(Guid id)
{
    PersonDetail pd = null;

    await Database.Default.StoredProcedure("[Demo].[spPersonGetDetail]")
        .Param(DbMapper.Default.GetParamName(nameof(PersonDetail.Id)), id)
        .SelectQueryMultiSetAsync(
            new MultiSetSingleArgs<Person>(PersonData.DbMapper.Default, (r) => { pd = new PersonDetail(); pd.CopyFrom(r); }, isMandatory: false),
            new MultiSetCollArgs<WorkHistoryCollection, WorkHistory>(WorkHistoryData.DbMapper.Default, (r) => pd.History = r));

    return pd;
} 
```

<br/>

### 5. Company.AppName.Api

There is [guidance](https://docs.microsoft.com/en-us/aspnet/core/migration/22-to-30?view=aspnetcore-3.1&tabs=visual-studio) from Microsoft on the process of migrating from ASP.NET Core 2.2 to ASP.NET Core 3.x. There are also [breaking changes](https://docs.microsoft.com/en-us/dotnet/core/compatibility/2.2-3.1) that occur when upgrading from ASP.NET Core 2.2 to ASP.NET Core 3.1 that may need to be considered.

The following relates to the basics of getting the `Beef` requirements of the solution upgraded.

<br/>

#### 5.1. Program.cs

There is a minor change required, perform a find and replace: `IHostingEnvironment` to `IWebHostEnvironment`. 

<br/>

#### 5.2. Startup.cs

Within the `ConfigureServices` method we need to switch from the new out-of-the-box JSON serializer to the `Newtonsoft.Json` version as _Beef_ currently has a hard dependency on this serializer. 

Remove the following:

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc();

    ...

public void Configure(IApplicationBuilder app, IHostingEnvironment env, ...

    ...

    // Use mvc.
    app.UseMvc();
```
<br/>

And replace with:

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add services; note Beef requires NewtonsoftJson.
    services.AddControllers().AddNewtonsoftJson();

    ...

public void Configure(IApplicationBuilder app, IHostingEnvironment env, ...
        {

    ...

    // Use controllers.
    app.UseRouting();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });
```

<br/>

### 6. Company.AppName.Test

Make the following changes to `FixtureSetup.cs`, change from the previous synchronous code:

``` csharp
[OneTimeSetUp]
public void OneTimeSetUp()
{
    TestSetUp.RegisterSetUp((count, data) =>
    {
        return DatabaseExecutor.Run(
            count == 0 ? DatabaseExecutorCommand.ResetAndDatabase : atabaseExecutorCommand.ResetAndData, 
            AgentTester.Configuration["ConnectionStrings:BeefDemo"],
            typeof(DatabaseExecutor).Assembly, typeof(Database.Program).Assembly, ssembly.GetExecutingAssembly()) == 0;
    });

    AgentTester.StartupTestServer<Startup>(environmentVariablesPrefix: "Beef_");
}
```

<br/>

To the new asynchronous code:

``` csharp
[OneTimeSetUp]
public void OneTimeSetUp()
{
    TestSetUp.RegisterSetUp(async (count, data) =>
    {
        return await DatabaseExecutor.RunAsync(
            count == 0 ? DatabaseExecutorCommand.ResetAndDatabase : atabaseExecutorCommand.ResetAndData, 
            AgentTester.Configuration["ConnectionStrings:BeefDemo"],
            typeof(DatabaseExecutor).Assembly, typeof(Database.Program).Assembly, ssembly.GetExecutingAssembly()).ConfigureAwait(false) == 0;
    });

    AgentTester.StartupTestServer<Startup>(environmentVariablesPrefix: "Beef_");
}
```