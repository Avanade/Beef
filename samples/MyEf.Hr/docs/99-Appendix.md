# Appendix

This appendix includes additional information that might be useful when developing and deploying the sample.

<br/>

## Azure Resource

If you want to deploy your app to `Azure App Service`, execute the following command.

However, you need change the variables accordingly.

```
az login

echo create resource group
az group create --name <resource group name> -- location <your location>

echo create app service
az appservice plan create -g <resource group name> -n <app plan name> --sku b1
az webapp create -g <resource group name> -p <app plan name> -n <app name>

echo create storage
az storage account create -g <resource group name> -l <your location> --sku Standard_LRS -n <storage account name>

echo get storage account connection string
az storage account show-connection-string -g <resource group name> -n <storage account name>

echo create service bus
az servicebus namespace create -g <resource group name> --name <service bus namespace> --location <your location>
az servicebus queue create -g <resource group name> --namespace-name <service bus namespace> --name <service bus queue name>

echo get servicebus primary connection string
az servicebus namespace authorization-rule keys list -g <resource group name> --namespace-name <service bus namespace> --name RootManageSharedAccessKey --query primaryConnectionString --output tsv
```

If you need to rebuild, execute the following to drop and start again.

```
az group delete --name <resource group name>
```

<br/>

## Create Azure Function project

The update project dependencies is invalid because `CoreEx.Azure` and Function project have different version of `Microsoft.Azure.Functions.Worker.Extensions.ServiceBus` .

Therefore, you need to delete the `Microsoft.Azure.Functions.Worker.Extensions.ServiceBus` before adding the `CoreEx.Azure` NuGet package as dependencies.

<br/>

## Debug Option

If you don't want to send telemetry to azure monitor using local debug or a service, you should include code similar to the following. Alternatively, remove environment variable `APPLICATIONINSIGHTS_CONNECTION_STRING`.

```csharp
        // Add Azure monitor open telemetry.
        if (env.IsProduction())
           services.AddOpenTelemetry().UseAzureMonitor().WithTracing(b => b.AddSource("CoreEx.*", "MyEf.Hr.*", "Microsoft.EntityFrameworkCore.*", "EntityFrameworkCore.*"));
```
