FROM mcr.microsoft.com/dotnet/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# It's important to keep lines from here down to "COPY . ." identical in all Dockerfiles
# to take advantage of Docker's build cache, to speed up local container builds
COPY "samples/My.Hr/My.Hr.sln" "samples/My.Hr/My.Hr.sln"

COPY "samples/My.Hr/My.Hr.Api/My.Hr.Api.csproj" "samples/My.Hr/My.Hr.Api/My.Hr.Api.csproj"
COPY "samples/My.Hr/My.Hr.Business/My.Hr.Business.csproj" "samples/My.Hr/My.Hr.Business/My.Hr.Business.csproj"
COPY "samples/My.Hr/My.Hr.CodeGen/My.Hr.CodeGen.csproj" "samples/My.Hr/My.Hr.CodeGen/My.Hr.CodeGen.csproj"
COPY "samples/My.Hr/My.Hr.Common/My.Hr.Common.csproj" "samples/My.Hr/My.Hr.Common/My.Hr.Common.csproj"
COPY "samples/My.Hr/My.Hr.Database/My.Hr.Database.csproj" "samples/My.Hr/My.Hr.Database/My.Hr.Database.csproj"
COPY "samples/My.Hr/My.Hr.Test/My.Hr.Test.csproj" "samples/My.Hr/My.Hr.Test/My.Hr.Test.csproj"

COPY "src/Beef.Abstractions/Beef.Abstractions.csproj" "src/Beef.Abstractions/Beef.Abstractions.csproj"
COPY "src/Beef.AspNetCore.WebApi/Beef.AspNetCore.WebApi.csproj" "src/Beef.AspNetCore.WebApi/Beef.AspNetCore.WebApi.csproj"
COPY "src/Beef.Core/Beef.Core.csproj" "src/Beef.Core/Beef.Core.csproj"
COPY "src/Beef.Data.Cosmos/Beef.Data.Cosmos.csproj" "src/Beef.Data.Cosmos/Beef.Data.Cosmos.csproj"
COPY "src/Beef.Data.Database/Beef.Data.Database.csproj" "src/Beef.Data.Database/Beef.Data.Database.csproj"
COPY "src/Beef.Data.EntityFrameworkCore/Beef.Data.EntityFrameworkCore.csproj" "src/Beef.Data.EntityFrameworkCore/Beef.Data.EntityFrameworkCore.csproj"
COPY "src/Beef.Data.OData/Beef.Data.OData.csproj" "src/Beef.Data.OData/Beef.Data.OData.csproj"
COPY "src/Beef.Events/Beef.Events.csproj" "src/Beef.Events/Beef.Events.csproj"
COPY "src/Beef.Events.EventHubs/Beef.Events.EventHubs.csproj" "src/Beef.Events.EventHubs/Beef.Events.EventHubs.csproj"
COPY "src/Beef.Events.ServiceBus/Beef.Events.ServiceBus.csproj" "src/Beef.Events.ServiceBus/Beef.Events.ServiceBus.csproj"
COPY "src/Beef.Grpc/Beef.Grpc.csproj" "src/Beef.Grpc/Beef.Grpc.csproj"

COPY "tools/Beef.CodeGen.Core/Beef.CodeGen.Core.csproj" "tools/Beef.CodeGen.Core/Beef.CodeGen.Core.csproj"
COPY "tools/Beef.Database.Core/Beef.Database.Core.csproj" "tools/Beef.Database.Core/Beef.Database.Core.csproj"
COPY "tools/Beef.Test.NUnit/Beef.Test.NUnit.csproj" "tools/Beef.Test.NUnit/Beef.Test.NUnit.csproj"
COPY "tools/Beef.CodeGen.Core/Beef.CodeGen.Core.csproj" "tools/Beef.CodeGen.Core/Beef.CodeGen.Core.csproj"
COPY "tools/Beef.Database.Core/Beef.Database.Core.csproj" "tools/Beef.Database.Core/Beef.Database.Core.csproj"
COPY "tools/Beef.Test.NUnit/Beef.Test.NUnit.csproj" "tools/Beef.Test.NUnit/Beef.Test.NUnit.csproj"

RUN dotnet restore "samples/My.Hr/My.Hr.sln"

COPY . .
WORKDIR /src/samples/My.Hr/My.Hr.Api
RUN dotnet publish --no-restore -c Release -o /app

FROM build as unittest
WORKDIR /src/samples/My.Hr/My.Hr.Test

FROM build AS publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "My.Hr.Api.dll"]
