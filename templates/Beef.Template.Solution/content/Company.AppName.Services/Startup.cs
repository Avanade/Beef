namespace Company.AppName.Services;

/// <summary>
/// The <see cref="HostStartup"/> to enable runtime and testable dependency injection.
/// </summary>
public class Startup : HostStartup
{
    /// <inheritdoc/>
    public override void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder config)
    {
        config.AddEnvironmentVariables("AppName_");
    }

    /// <inheritdoc/>
    public override void ConfigureServices(IServiceCollection services)
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
                .AddValidators<AppNameSettings, Startup>()
                .AddMappers<AppNameSettings, Startup>()
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

#if (implement_services)
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

        // Add the event subscribing services.
        services.AddCloudEventSerializer()
                .AddEventSubscribers<Startup>()
                .AddEventSubscriberOrchestrator((_, o) =>
                {
                    o.NotSubscribedHandling = ErrorHandling.CompleteAsSilent;
                    o.AddSubscribers(EventSubscriberOrchestrator.GetSubscribers<Startup>());
                })
                .AddAzureServiceBusOrchestratedSubscriber((_, o) =>
                {
                    o.EventDataDeserializationErrorHandling = ErrorHandling.HandleBySubscriber;
                });
    }
}