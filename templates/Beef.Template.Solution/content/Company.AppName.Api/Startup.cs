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
        if (services == null)
            throw new ArgumentNullException(nameof(services));

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
                .AddValidators<PersonValidator>()
                .AddSingleton<IIdentifierGenerator, IdentifierGenerator>();

#if (implement_database || implement_entityframework)
        // Add the database services (scoped per request/connection).
        services.AddDatabase(sp => new AppNameDb(() => new SqlConnection(sp.GetRequiredService<AppNameSettings>().DatabaseConnectionString), sp.GetRequiredService<ILogger<AppNameDb>>()));

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
            var settings = sp.GetRequiredService<BankingSettings>();
            var cco = new AzCosmos.CosmosClientOptions { SerializerOptions = new AzCosmos.CosmosSerializationOptions { PropertyNamingPolicy = AzCosmos.CosmosPropertyNamingPolicy.CamelCase, IgnoreNullValues = true } };
            return new CosmosDb(new AzCosmos.CosmosClient(settings.CosmosConnectionString, cco).GetDatabase(settings.CosmosDatabaseId), sp.GetRequiredService<CoreEx.Mapping.IMapper>());
        });

#endif
#if (implement_httpagent)
        // Add the HTTP agent services.
        services.AddHttpClient("Xxx", c => c.BaseAddress = new Uri(_config.GetValue<string>("XxxAgentUrl")));
        services.AddScoped<IXxxAgentArgs>(sp => new XxxAgentArgs(sp.GetService<IHttpClientFactory>().CreateClient("Xxx")));
        services.AddScoped<IXxxAgent, XxxAgent>();

#endif
        // Add the generated reference data services.
        services.AddGeneratedReferenceDataManagerServices()
                .AddGeneratedReferenceDataDataSvcServices()
                .AddGeneratedReferenceDataDataServices();

        // Add the generated entity services.
        services.AddGeneratedManagerServices()
                .AddGeneratedDataSvcServices()
                .AddGeneratedDataServices();

#if (implement_entityframework || implement_cosmos)
        // Add type-to-type mapping services using reflection.
        services.AddMappers<AppNameSettings>();

#endif
        // Add the event publishing; this will need to be updated from the logger publisher to the actual as appropriate.
        services.AddEventDataFormatter()
                .AddLoggerEventPublisher();

        // Add additional services.
        services.AddControllers();
        services.AddHealthChecks();
        services.AddHttpClient();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Company.AppName API", Version = "v1" });
            options.OperationFilter<CoreEx.WebApis.AcceptsBodyOperationFilter>();  // Needed to support AcceptsBodyAttribue where body parameter not explicitly defined.
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