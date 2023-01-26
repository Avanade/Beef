using CoreEx.Database;
using CoreEx.Events;

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
            if (services == null)
                throw new ArgumentNullException(nameof(services));

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
            services.AddDatabase(sp => new HrDb(() => new SqlConnection(sp.GetRequiredService<HrSettings>().DatabaseConnectionString), sp.GetRequiredService<ILogger<HrDb>>()));

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
            services.AddNullEventPublisher();

            // Add transactional event outbox services.
            services.AddScoped<IEventSender>(sp =>
            {
                var eoe = new EventOutboxEnqueue(sp.GetRequiredService<IDatabase>(), sp.GetRequiredService<ILogger<EventOutboxEnqueue>>());
                //eoe.SetPrimaryEventSender(/* the primary sender instance; i.e. service bus */); // This is optional.
                return eoe;
            });

            // Add entity mapping services using assembly probing.
            services.AddMappers<HrSettings>();

            // Add additional services.
            services.AddControllers();
            services.AddHealthChecks();
            services.AddHttpClient();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "MyEf.Hr API", Version = "v1" });

                var xmlName = $"{Assembly.GetEntryAssembly()!.GetName().Name}.xml";
                var xmlFile = Path.Combine(AppContext.BaseDirectory, xmlName);
                if (File.Exists(xmlFile))
                    options.IncludeXmlComments(xmlFile);

                options.OperationFilter<CoreEx.WebApis.AcceptsBodyOperationFilter>();  // Needed to support AcceptsBodyAttribue where body parameter not explicitly defined.
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
}