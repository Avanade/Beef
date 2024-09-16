﻿namespace Company.AppName.Api;

/// <summary>
/// Represents the <b>startup</b> class.
/// </summary>
public class Startup
{
    /// <summary>
    /// The configure services method called by the runtime; use this method to add services to the container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        // Add the core services.
        services.AddSettings<AppNameSettings>()
                .AddExecutionContext()
                .AddJsonSerializer()
                .AddReferenceDataOrchestrator()
                .AddReferenceDataContentWebApi()
                .AddWebApi()
                .AddJsonMergePatch()
                .AddRequestCache()
                .AddValidationTextProvider()
                .AddValidators<AppNameSettings>()
                .AddMappers<AppNameSettings>()
                .AddSingleton<IIdentifierGenerator, IdentifierGenerator>();

#if (implement_database || implement_sqlserver)
        // Add the database services (scoped per request/connection).
        services.AddDatabase(sp => new AppNameDb(() => new SqlConnection(sp.GetRequiredService<AppNameSettings>().DatabaseConnectionString), sp.GetRequiredService<ILogger<AppNameDb>>()), healthCheckName: "sql-server");

#endif
#if (implement_mysql)
        // Add the database services (scoped per request/connection).
        services.AddDatabase(sp => new AppNameDb(() => new MySqlConnection(sp.GetRequiredService<AppNameSettings>().DatabaseConnectionString), sp.GetRequiredService<ILogger<AppNameDb>>()), healthCheckName: "my-sql");

#endif
#if (implement_postgres)
        // Add the database services (scoped per request/connection).
        services.AddDatabase(sp => new AppNameDb(() => new NpgsqlConnection(sp.GetRequiredService<AppNameSettings>().DatabaseConnectionString), sp.GetRequiredService<ILogger<AppNameDb>>()), healthCheckName: "postgres");

#endif
#if (implement_entityframework)
        // Add the entity framework services (scoped per request/connection).
        services.AddDbContext<AppNameEfDbContext>();
        services.AddEfDb<AppNameEfDb>();

#endif
#if (implement_cosmos)
        // Add the cosmos database.
        services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<AppNameSettings>();
            var cco = new AzCosmos.CosmosClientOptions { SerializerOptions = new AzCosmos.CosmosSerializationOptions { PropertyNamingPolicy = AzCosmos.CosmosPropertyNamingPolicy.CamelCase, IgnoreNullValues = true } };
            return new AzCosmos.CosmosClient(settings.CosmosConnectionString, cco);
        }).AddCosmosDb(sp =>
        {
            var settings = sp.GetRequiredService<AppNameSettings>();
            return new AppNameCosmosDb(sp.GetRequiredService<AzCosmos.CosmosClient>().GetDatabase(settings.CosmosDatabaseId), sp.GetRequiredService<CoreEx.Mapping.IMapper>());
        });

#endif
#if (implement_httpagent)
        // Add the HTTP agent services.
        services.AddHttpClient<XxxAgent>("Xxx", (sp, c) => c.BaseAddress = new Uri(sp.GetRequiredService<AppNameSettings>().XxxAgentUrl));

#endif
        // Add the generated reference data services.
        services.AddGeneratedReferenceDataManagerServices()
                .AddGeneratedReferenceDataDataSvcServices()
                .AddGeneratedReferenceDataDataServices();

        // Add the generated entity services.
        services.AddGeneratedManagerServices()
                .AddGeneratedDataSvcServices()
                .AddGeneratedDataServices();

#if (implement_services)
        // Add event publishing services.
        services.AddEventDataFormatter()
                .AddCloudEventSerializer()
                .AddEventPublisher();

#if (implement_database || implement_sqlserver)
        // Add sql server transactional event outbox enqueue services; AfterSend used to trigger the hosted service to dequeue within 5 seconds.
        services.AddEventSender<EventOutboxEnqueue>((sp, es) => es.AfterSend += (sender, e) => EventOutboxHostedService.OneOffTrigger(sp, TimeSpan.FromSeconds(5)));

        // Add transactional event outbox dequeue dependencies.
        services.AddSingleton(sp => new AzServiceBus.ServiceBusClient(sp.GetRequiredService<AppNameSettings>().ServiceBusConnectionString))
                .AddSingleton(sp => new AzBlobs.BlobContainerClient(sp.GetRequiredService<AppNameSettings>().StorageConnectionString, "event-synchronizer"))
                .AddSingleton<IServiceSynchronizer, BlobLeaseSynchronizer>();

        // Add transactional event outbox dequeue hosted service (_must_ be explicit with the IServiceBusSender).
        services.AddScoped<IServiceBusSender, ServiceBusSender>()
                .AddSqlServerEventOutboxHostedService(sp => new EventOutboxDequeue(sp.GetRequiredService<IDatabase>(), sp.GetRequiredService<IServiceBusSender>(), sp.GetRequiredService<ILogger<EventOutboxDequeue>>()));
#else
        // Add service bus event sender.
        services.AddSingleton(sp => new AzServiceBus.ServiceBusClient(sp.GetRequiredService<AppNameSettings>().ServiceBusConnectionString))
                .AddScoped<IEventSender, ServiceBusSender>();
#endif
#else
        // Add the event publishing; this will need to be updated from the null publisher to the actual as appropriate.
        services.AddEventDataFormatter()
                .AddCloudEventSerializer()
                .AddNullEventPublisher();
#endif

        // Add controllers.
        services.AddControllers();

        // Add health checks.
#if (implement_services)
        services.AddHealthChecks()
#if (implement_database || implement_sqlserver)
                .AddAzureBlobStorage(sp => new AzBlobs.BlobServiceClient(sp.GetRequiredService<AppNameSettings>().StorageConnectionString), name: "azure-blob-storage")
                .AddEventPublisherHealthCheck("azure-service-bus", sp => new EventPublisher(sp.GetRequiredService<EventDataFormatter>(), sp.GetRequiredService<IEventSerializer>(), sp.GetRequiredService<IServiceBusSender>()));
#else
                .AddEventPublisherHealthCheck("azure-service-bus", sp => new EventPublisher(sp.GetRequiredService<EventDataFormatter>(), sp.GetRequiredService<IEventSerializer>(), sp.GetRequiredService<IEventSender>()));
#endif
#else
        services.AddHealthChecks();
#endif

        // Add Azure monitor open telemetry.
        services.AddOpenTelemetry().UseAzureMonitor().WithTracing(b => b.AddSource("CoreEx.*", "Company.AppName.*", "Microsoft.EntityFrameworkCore.*", "EntityFrameworkCore.*"));

        // Add the swagger capabilities.
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Company.AppName API", Version = "v1" });
            options.OperationFilter<AcceptsBodyOperationFilter>();  // Needed to support AcceptsBodyAttribute where body parameter not explicitly defined.
            options.OperationFilter<PagingOperationFilter>();       // Needed to support PagingAttribute where PagingArgs parameter not explicitly defined.
            options.OperationFilter<QueryOperationFilter>();        // Needed to support QueryAttribute where QueryArgs parameter not explicitly defined.
        });
    }

    /// <summary>
    /// The configure method called by the runtime; use this method to configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
    public void Configure(IApplicationBuilder app)
    {
        // Handle any unhandled exceptions.
        app.UseWebApiExceptionHandler();

        // Authenticate the user.
        //app.UseAuthentication();

        // Add Swagger as an endpoint and to serve the swagger-ui to the pipeline.
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = ""; // Default as the root/home page.
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Company.AppName");
        });

        // Add execution context set up to the pipeline.
        app.UseExecutionContext();
        app.UseReferenceDataOrchestrator();

        // Add health checks.
        app.UseHealthChecks("/health");
        app.UseHealthChecks("/health/detailed", new HealthCheckOptions { ResponseWriter = HealthReportStatusWriter.WriteJsonResults }); // Secure with permissions / or remove given data returned.

        // Use controllers.
        app.UseRouting();
        //app.UseAuthorization();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}