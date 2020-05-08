# Beef.Grpc

[![NuGet version](https://badge.fury.io/nu/Beef.Grpc.svg)](https://badge.fury.io/nu/Beef.Grpc)

[gRPC](https://grpc.io/) support is enabled within _Beef_. The ins-and-outs of gRPC will not be covered here, and will assume the reader has a base understanding of gRPC, [`.proto`](https://grpc.io/docs/tutorials/basic/csharp/#defining-the-service) files, etc. _Beef_ leverages the [support](https://docs.microsoft.com/en-us/aspnet/core/grpc/) that Microsoft have added for .NET.

Given _Beef's_ approach to [tiering and layering](../../docs/Solution-Structure.md), along with the [code-generation](../../tools/Beef.CodeGen.Core/README.md), additional [service interface](../../docs/Layer-ServiceInterface.md) capabilities such as gRPC are easily enabled, and encourage reusability of the existing business and data access logic.

gRPC support is enabled for all operation types (`Get`, `Create`, `Update` and `Delete`), with the exception of `Patch` as this is a special-case that uses JSON merging versus the required binary structure.

<br/>

## Code-generation

gPRC support is enabled by the code-generation configuration. Additional attributes have been added that will _opt-in_ to the output of gRPC related artefacts.

There are two key attributes:
- `Grpc` - this is required for the [CodeGeneration](../../docs/Entity-CodeGeneration-Element.md), [Entity](../../docs/Entity-Entity-Element.md) and [Operation](../../docs/Entity-Operation-Element.md) elements to enable as support must be explicitly opted-into. 
- `GrpcFieldNo` - this is required for each [Property](../../docs/Entity-Property-Element.md) to both, a) opt-in, and b) define the unique and immutable field positioning number (required for non-breaking versioning).

The following is an [example](../../samples/Demo/Beef.Demo.CodeGen/Beef.Demo.xml) snippet of gRPC support being added for an entity, and the related arguments entity:

``` xml
<CodeGeneration RefDataNamespace="Beef.Demo.Common.Entities" Grpc="true" ... >
  <Entity Name="Robot" Grpc="true" ... >
    <Property Name="Id" Text="{{Robot}} identifier" Type="Guid" UniqueKey="true" GrpcFieldNo="1" />
    <Property Name="ModelNo" Text="Model number" Type="string" GrpcFieldNo="2" />
    <Property Name="SerialNo" Text="Unique serial number" Type="string" GrpcFieldNo="3" />
    <Property Name="EyeColor" Type="RefDataNamespace.EyeColor" RefDataType="string" GrpcFieldNo="4" />
    <Property Name="PowerSource" Type="RefDataNamespace.PowerSource" RefDataType="string" GrpcFieldNo="5" />
    <Property Name="ETag" ArgumentName="etag" Type="string" GrpcFieldNo="6" />
    <Property Name="ChangeLog" Type="ChangeLog" IsEntity="true" GrpcFieldNo="7" />

    <Operation Name="Get" OperationType="Get" UniqueKey="true" WebApiRoute="{id}" Grpc="true" />
    <Operation Name="Create" OperationType="Create" WebApiRoute="" Grpc="true" />
    <Operation Name="Update" OperationType="Update" UniqueKey="true" WebApiRoute="{id}" Grpc="true" />
    <Operation Name="Delete" OperationType="Delete" UniqueKey="true" WebApiRoute="{id}" Grpc="true" />

    <Operation Name="GetByArgs" OperationType="GetColl" PagingArgs="true" WebApiRoute="" Grpc="true">
      <Parameter Name="Args" Type="RobotArgs" Validator="RobotArgsValidator" />
    </Operation>
  </Entity>

  <Entity Name="RobotArgs" Text="{{Robot}} arguments" ExcludeAll="true" Grpc="true">
    <Property Name="ModelNo" JsonName="model-no" Text="Model number" Type="string" GrpcFieldNo="1" />
    <Property Name="SerialNo" JsonName="serial-no" Text="Unique serial number" Type="string" GrpcFieldNo="2" />
  </Entity>
</CodeGeneration>
```

<br/>

### .proto file

gRPC uses a contract-first approach to API development. Services and messages are defined in `*.proto` files. _Beef_ will generate a `Company.AppName.proto` file within the `Company.AppName.Common` project, in the `Grpc/Generated` folder.

See the example [`beef.demo.proto`](../../samples/Demo/Beef.Demo.Common/Grpc/Generated/beef.demo.proto).

<br/>

### Transformers

This is a capability added by _Beef_ to support the data transformation requirements between the gRPC and .NET types, and the .NET entities and gRPC formatted entities to be used by the gRPC service and Agent/ServiceAgent components. _Beef_ will generate a `Transformers.cs` file within the `Company.AppName.Common` project, in the `Grpc/Generated` folder. This leverages the _Beef_ mapping and converting capabilities.

See the example [`Transformers.cs`](../../samples/Demo/Beef.Demo.Common/Grpc/Generated/Transformers.cs).

<br/>

### Service

The gRPC Service is the functional equivalent to the [WebAPI Controller](../../docs/Layer-ServiceInterface.md), in that it provides the server-side capability. It will use the above _transformers_ to transform the request and response for gRPC. At its core it will invoke the [Domain logic](../../docs/Layer-Manager.md) to perform the work (the same logic is being reused). _Beef_ will generate a `XxxService.cs` file within the `Company.AppName.Api` project, in the `Grpc/Generated` folder.

See the example [`RobotService.cs`](../../samples/Demo/Beef.Demo.Api/Grpc/Generated/RobotService.cs).

<br/>

### Agent/ServiceAgent

The gRPC Agent/ServiceAgent is the functional equivalent to [WebAPI Agent/ServiceAgent](../../docs/Layer-ServiceAgent.md), in that it provides the client-side capability (i.e. it invokes the above Service). It will use the above _transformers_ to transform the request and response for gRPC. _Beef_ will generate a `XxxAgent.cs` file within the `Company.AppName.Common` project, in the `Grpc/Generated` folder; as well as a `XxxAgent.cs` file within the `Company.AppName.Common` project, in the `Grpc/ServiceAgents/Generated` folder.

See the example [RobotAgent](../../samples/Demo/Beef.Demo.Common/Grpc/Generated/RobotAgent.cs) and [RobotServiceAgent](../../samples/Demo/Beef.Demo.Common/Grpc/ServiceAgents/Generated/RobotServiceAgent.cs).

<br/>

## Run-time enablement

The following describes the changes required for run-time enablement.

<br/>

### Company.AppName.Common

The following NuGet packages will need to be added:
- `Beef.Grpc`
- `Grpc.Tools` - this provides the tooling and runtime for the Grpc server and client capabilities. This tooling will also generate the required gRPC server and client components within.

Check the `Company.AppName.Common.csproj`; it should look similar to the following:

``` xml
  <ItemGroup>
    <PackageReference Include="Grpc.Tools" Version="2.28.1">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <Protobuf Include=".\Grpc\Generated\beef.demo.proto" GrpcServices="Client,Server" />
  </ItemGroup> 
```

<br/>

### Company.AppName.Api

The following NuGet packages will need to be added:
- `Beef.Grpc`
- `Grpc.AspNetCore` - this provides the ASP.NET Core gRPC server hosting capabilities.

The following gRPC changes will need to be made to the [`Startup.cs`](../../samples/Demo/Beef.Demo.Api/Startup.cs):

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers().AddNewtonsoftJson();
    services.AddGrpc();
    services.AddHealthChecks();
    services.AddHttpClient();

    ...
}

public void Configure(IApplicationBuilder app, IConfiguration config, ILoggerFactory loggerFactory, IHttpClientFactory clientFactory)
{
    ...

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
        endpoints.MapGrpcService<Grpc.RobotService>();
    });
}
```

<br/>

## Testing

The _Beef_ unit and intra-domain integration testing framework ([Beef.Test.NUnit](../../tools/Beef.Test.NUnit/README.md)). The experience of using the `AgentTester` is largely the same to minimize differences in approach. There is a new `AgentTester.CreateGrpc` method that is used to create the `GrpcAgentTester`. All other `Expect*`, `Ignore*` and `Run*` methods are the same.

The following [snippet](../../samples/Demo/Beef.Demo.Test/RobotGrpcTest.cs) provides an example:

``` csharp
[Test, TestSetUp]
public void B120_Get_Found()
{
    AgentTester.CreateGrpc<RobotAgent, Robot>()
        .ExpectStatusCode(HttpStatusCode.OK)
        .IgnoreChangeLog()
        .IgnoreETag()
        .ExpectValue((t) => new Robot { Id = 1.ToGuid(), ModelNo = "T1000", SerialNo = "123456", PowerSource = "F" })
        .Run((a) => a.Agent.GetAsync(1.ToGuid()));
}
```