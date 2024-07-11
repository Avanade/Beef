using Azure.Monitor.OpenTelemetry.AspNetCore;
using CoreEx.AspNetCore.HealthChecks;
using CoreEx.Azure.ServiceBus;
using CoreEx.Azure.Storage;
using CoreEx.Database;
using CoreEx.Events;
using CoreEx.Hosting;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using OpenTelemetry.Instrumentation.EntityFrameworkCore;
using Az = Azure.Messaging.ServiceBus;

namespace MyEf.Hr.Api
{
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
            services.AddSettings<HrSettings>()
                    .AddExecutionContext()
                    .AddJsonSerializer()
                    .AddReferenceDataOrchestrator<IReferenceDataProvider>()
                    .AddWebApi()
                    .AddJsonMergePatch()
                    .AddReferenceDataContentWebApi()
                    .AddRequestCache()
                    .AddValidationTextProvider()
                    .AddValidators<EmployeeManager>();

            // Add the beef database services (scoped per request/connection).
            services.AddDatabase(sp => new HrDb(() => new SqlConnection(sp.GetRequiredService<HrSettings>().DatabaseConnectionString), sp.GetRequiredService<ILogger<HrDb>>()), healthCheckName: "sql-server");

            // Add the beef entity framework services (scoped per request/connection).
            services.AddDbContext<HrEfDbContext>();
            services.AddEfDb<HrEfDb>();

            // Add the generated reference data services.
            services.AddGeneratedReferenceDataManagerServices()
                    .AddGeneratedReferenceDataDataSvcServices()
                    .AddGeneratedReferenceDataDataServices();

            // Add the generated entity services.
            services.AddGeneratedManagerServices()
                    .AddGeneratedDataSvcServices()
                    .AddGeneratedDataServices();

            // Add event publishing services.
            services.AddEventDataFormatter();
            services.AddCloudEventSerializer();
            services.AddEventPublisher();

            // Add transactional event outbox enqueue services.
            services.AddScoped<IEventSender, EventOutboxEnqueue>();

            // Add transactional event outbox dequeue dependencies.
            services.AddSingleton(sp => new Az.ServiceBusClient(sp.GetRequiredService<HrSettings>().ServiceBusConnectionString))
                    .AddSingleton<IServiceSynchronizer>(sp => new BlobLeaseSynchronizer(new Azure.Storage.Blobs.BlobContainerClient(sp.GetRequiredService<HrSettings>().StorageConnectionString, "event-synchronizer")))
                    .AddScoped<IServiceBusSender, ServiceBusSender>();

            // Add transactional event outbox dequeue hosted service (_must_ be explicit with the IServiceBusSender as the IEventSender).
            services.AddSqlServerEventOutboxHostedService(sp =>
            {
                return new EventOutboxDequeue(sp.GetRequiredService<IDatabase>(), sp.GetRequiredService<IServiceBusSender>(), sp.GetRequiredService<ILogger<EventOutboxDequeue>>());
            });

            // Add entity mapping services using assembly probing.
            services.AddMappers<HrSettings>();

            // Add additional services.
            services.AddControllers();

            // Add health checks.
            services.AddHealthChecks()
                    .AddAzureBlobStorage(sp => new Azure.Storage.Blobs.BlobServiceClient(sp.GetRequiredService<HrSettings>().StorageConnectionString), name: "azure-blob-storage")
                    .AddEventPublisherHealthCheck("event-publisher", sp => new EventPublisher(sp.GetRequiredService<EventDataFormatter>(), sp.GetRequiredService<IEventSerializer>(), sp.GetRequiredService<IServiceBusSender>()));


            // Add Azure monitor open telemetry.
            services.AddOpenTelemetry().UseAzureMonitor().WithTracing(b => b.AddSource("CoreEx.*", "MyEf.Hr.*", "Microsoft.EntityFrameworkCore.*", "EntityFrameworkCore.*"));
            services.Configure<EntityFrameworkInstrumentationOptions>(options => options.SetDbStatementForText = true);

            // Add Swagger services.
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "MyEf.Hr API", Version = "v1" });

                var xmlName = $"{Assembly.GetEntryAssembly()!.GetName().Name}.xml";
                var xmlFile = Path.Combine(AppContext.BaseDirectory, xmlName);
                if (File.Exists(xmlFile))
                    options.IncludeXmlComments(xmlFile);

                xmlName = $"{Assembly.GetEntryAssembly()!.GetName().Name}.xml".Replace(".Api", ".Common");
                xmlFile = Path.Combine(AppContext.BaseDirectory, xmlName);
                if (File.Exists(xmlFile))
                    options.IncludeXmlComments(xmlFile);

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
            // Add exception handling to the pipeline.
            app.UseWebApiExceptionHandler();

            // Add Swagger as a JSON endpoint and to serve the swagger-ui to the pipeline.
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyEf.Hr"));

            // Add execution context set up to the pipeline.
            app.UseExecutionContext();
            app.UseReferenceDataOrchestrator();

            // Add health checks.
            app.UseHealthChecks("/health");
            app.UseHealthChecks("/health/detailed", new HealthCheckOptions { ResponseWriter = HealthReportStatusWriter.WriteJsonResults }); // Secure with permissions / or remove given data returned.

            // Use controllers.
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}