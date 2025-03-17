using Microsoft.SemanticKernel;

namespace AIChat.AOAI.Api;

/// <summary>
/// Represents the <b>startup</b> class.
/// </summary>
public class Startup
{
    /// <summary>
    /// The configure services method called by the runtime; use this method to add services to the container.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    public void ConfigureServices(IServiceCollection services, IWebHostEnvironment env)
    {
        // Add the core services.
        services.AddSettings<AOAISettings>()
                .AddExecutionContext()
                .AddJsonSerializer()
                .AddReferenceDataOrchestrator()
                .AddReferenceDataContentWebApi()
                .AddWebApi()
                .AddJsonMergePatch()
                .AddRequestCache()
                .AddValidationTextProvider()
                .AddValidators<AOAISettings>()
                .AddMappers<AOAISettings>()
                .AddSingleton<IIdentifierGenerator, IdentifierGenerator>();

        // Add the cosmos database.
        services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<AOAISettings>();
            var cco = new AzCosmos.CosmosClientOptions { SerializerOptions = new AzCosmos.CosmosSerializationOptions { PropertyNamingPolicy = AzCosmos.CosmosPropertyNamingPolicy.CamelCase, IgnoreNullValues = true } };
            return new AzCosmos.CosmosClient(settings.CosmosConnectionString, cco);
        }).AddCosmosDb(sp =>
        {
            var settings = sp.GetRequiredService<AOAISettings>();
            return new AOAICosmosDb(sp.GetRequiredService<AzCosmos.CosmosClient>().GetDatabase(settings.CosmosDatabaseId), sp.GetRequiredService<CoreEx.Mapping.IMapper>());
        });

        // Add the generated reference data services.
        services.AddGeneratedReferenceDataManagerServices()
                .AddGeneratedReferenceDataDataSvcServices()
                .AddGeneratedReferenceDataDataServices();

        // Add the generated entity services.
        services.AddGeneratedManagerServices()
                .AddGeneratedDataSvcServices()
                .AddGeneratedDataServices();

        // Add the event publishing; this will need to be updated from the null publisher to the actual as appropriate.
        services.AddEventDataFormatter()
                .AddCloudEventSerializer()
                .AddNullEventPublisher();

        // Add controllers.
        services.AddControllers();

        // Add health checks.
        services.AddHealthChecks();

        // Add Azure monitor open telemetry.
        if (!env.IsDevelopment())
            services.AddOpenTelemetry().UseAzureMonitor().WithTracing(b => b.AddSource("CoreEx.*", "AIChat.AOAI.*", "Microsoft.EntityFrameworkCore.*", "EntityFrameworkCore.*"));

        // Add the swagger capabilities.
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "AIChat.AOAI API", Version = "v1" });
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
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "AIChat.AOAI");
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