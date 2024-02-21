namespace Company.AppName.Api;

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
                .AddWebApi()
                .AddJsonMergePatch()
                .AddReferenceDataContentWebApi()
                .AddRequestCache()
                .AddValidationTextProvider()
                .AddValidators<AppNameSettings>()
                .AddMappers<AppNameSettings>()
                .AddSingleton<IIdentifierGenerator, IdentifierGenerator>();

#if (implement_database || implement_sqlserver)
        // Add the database services (scoped per request/connection).
        services.AddDatabase(sp => new AppNameDb(() => new SqlConnection(sp.GetRequiredService<AppNameSettings>().DatabaseConnectionString), sp.GetRequiredService<ILogger<AppNameDb>>()));

#endif
#if (implement_mysql)
        // Add the database services (scoped per request/connection).
        services.AddDatabase(sp => new AppNameDb(() => new MySqlConnection(sp.GetRequiredService<AppNameSettings>().DatabaseConnectionString), sp.GetRequiredService<ILogger<AppNameDb>>()));

#endif
#if (implement_postgres)
        // Add the database services (scoped per request/connection).
        services.AddDatabase(sp => new AppNameDb(() => new NpgsqlConnection(sp.GetRequiredService<AppNameSettings>().DatabaseConnectionString), sp.GetRequiredService<ILogger<AppNameDb>>()));

#endif
#if (implement_entityframework)
        // Add the entity framework services (scoped per request/connection).
        services.AddDbContext<AppNameEfDbContext>();
        services.AddEfDb<AppNameEfDb>();

#endif
#if (implement_cosmos)
        // Add the cosmos database.
        services.AddSingleton<ICosmos>(sp =>
        {
            var settings = sp.GetRequiredService<AppNameSettings>();
            var cco = new AzCosmos.CosmosClientOptions { SerializerOptions = new AzCosmos.CosmosSerializationOptions { PropertyNamingPolicy = AzCosmos.CosmosPropertyNamingPolicy.CamelCase, IgnoreNullValues = true } };
            return new CosmosDb(new AzCosmos.CosmosClient(settings.CosmosConnectionString, cco).GetDatabase(settings.CosmosDatabaseId), sp.GetRequiredService<CoreEx.Mapping.IMapper>());
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

#if (implement_subscriber)
        // Add event publishing services.
        services.AddEventDataFormatter()
                .AddCloudEventSerializer()
                .AddEventPublisher();

#if (implement_database || implement_sqlserver)
        // Add sql server transactional event outbox enqueue services.
        services.AddScoped<IEventSender, EventOutboxEnqueue>();

        // Add transactional event outbox dequeue dependencies.
        services.AddSingleton(sp => new AzServiceBus.ServiceBusClient(sp.GetRequiredService<AppNameSettings>().ServiceBusConnectionString))
                .AddSingleton<IServiceSynchronizer>(sp => new BlobLeaseSynchronizer(new AzBlobs.BlobContainerClient(sp.GetRequiredService<AppNameSettings>().StorageConnectionString, "event-synchronizer")));

        // Add transactional event outbox dequeue hosted service (_must_ be explicit with the IServiceBusSender).
        services.AddScoped<IServiceBusSender, ServiceBusSender>()
                .AddSqlServerEventOutboxHostedService(sp =>
                {
                    return new EventOutboxDequeue(sp.GetRequiredService<IDatabase>(), sp.GetRequiredService<IServiceBusSender>(), sp.GetRequiredService<ILogger<EventOutboxDequeue>>());
                });
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

        // Add additional services.
        services.AddControllers();
        services.AddHealthChecks();
        services.AddHttpClient();

        // Add the swagger capabilities.
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Company.AppName API", Version = "v1" });
            options.OperationFilter<AcceptsBodyOperationFilter>();  // Needed to support AcceptsBodyAttribute where body parameter not explicitly defined.
            options.OperationFilter<PagingOperationFilter>();       // Needed to support PagingAttribute where PagingArgs parameter not explicitly defined.
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

        // Add Swagger as an endpoint and to serve the swagger-ui to the pipeline.
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Company.AppName"));

        // Add execution context set up to the pipeline.
        app.UseExecutionContext();
        app.UseReferenceDataOrchestrator();

        // Add health checks.
        app.UseHealthChecks("/health");

        // Use controllers.
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}