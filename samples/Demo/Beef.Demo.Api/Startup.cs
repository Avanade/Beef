using Beef.Demo.Common.Agents;
using AzCosmos = Microsoft.Azure.Cosmos;

namespace Beef.Demo.Api
{
    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;

            // Use JSON property names in validation.
            ValidationArgs.DefaultUseJsonNames = true;

            // Default the page size.
            // PagingArgs.DefaultTake = config.GetValue<int>("BeefDefaultPageSize");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add the core services.
            //services.AddBeefExecutionContext()
            //        .AddBeefSystemTime()
            //        .AddBeefRequestCache()
            //        .AddBeefCachePolicyManager(_config.GetSection("BeefCaching").Get<CachePolicyConfig>())
            //        .AddBeefWebApiServices()
            //        .AddBeefGrpcServiceServices()
            //        .AddBeefBusinessServices()
            //        .AddBeefTextProviderAsSingleton();
            services.AddSettings<DemoSettings>()
                    .AddExecutionContext()
                    .AddJsonSerializer()
                    .AddReferenceDataOrchestrator<IReferenceDataProvider>()
                    .AddWebApi()
                    .AddJsonMergePatch()
                    .AddReferenceDataContentWebApi()
                    .AddRequestCache()
                    .AddValidationTextProvider()
                    .AddValidators<PersonManager>();

            // Add the data sources as singletons for dependency injection requirements.
            //services.AddBeefDatabaseServices(() => new Database(WebApiStartup.GetConnectionString(_config, "BeefDemo")))
            //        .AddBeefEntityFrameworkServices<EfDbContext, EfDb>()
            //        .AddBeefCosmosDbServices<CosmosDb>(_config.GetSection("CosmosDb"))
            //        .AddSingleton<ITestOData>(_ => new TestOData(new Uri(WebApiStartup.GetConnectionString(_config, "TestOData"))))
            //        .AddSingleton<ITripOData>(_ => new TripOData(new Uri(WebApiStartup.GetConnectionString(_config, "TripOData"))));
            services.AddDatabase(sp => new Database(() => new SqlConnection(sp.GetRequiredService<DemoSettings>().DatabaseConnectionString), sp.GetRequiredService<ILogger<Database>>()))
                    .AddDbContext<EfDbContext>()
                    .AddEfDb<EfDb>();

            services.AddSingleton(sp =>
            {
                var settings = sp.GetRequiredService<DemoSettings>();
                var cco = new AzCosmos.CosmosClientOptions { SerializerOptions = new AzCosmos.CosmosSerializationOptions { PropertyNamingPolicy = AzCosmos.CosmosPropertyNamingPolicy.CamelCase, IgnoreNullValues = true } };
                return new DemoCosmosDb(new AzCosmos.CosmosClient(settings.CosmosConnectionString, cco).GetDatabase(settings.CosmosDatabaseId), sp.GetRequiredService<CoreEx.Mapping.IMapper>());
            });

            // Add the generated reference data services for dependency injection requirements.
            services.AddGeneratedReferenceDataManagerServices()
                    .AddGeneratedReferenceDataDataSvcServices()
                    .AddGeneratedReferenceDataDataServices();

            // Add the generated entity services for dependency injection requirements.
            services.AddGeneratedManagerServices()
                    //.AddGeneratedValidationServices()
                    .AddGeneratedDataSvcServices()
                    .AddGeneratedDataServices();

            // Add event publishing.
            //var ehcs = _config.GetValue<string>("EventHubConnectionString");
            //var sbcs = _config.GetValue<string>("ServiceBusConnectionString");
            //if (!string.IsNullOrEmpty(sbcs))
            //    services.AddBeefEventHubEventProducer(new EventHubProducerClient(ehcs));
            //else if (!string.IsNullOrEmpty(sbcs))
            //    services.AddBeefServiceBusSender(new ServiceBusClient(sbcs));
            //else
            //    services.AddBeefNullEventPublisher();
            services.AddEventDataFormatter(new CoreEx.Events.EventDataFormatter { SubjectAppendKey = true, SubjectCasing = CoreEx.Globalization.TextInfoCasing.None, ActionCasing = CoreEx.Globalization.TextInfoCasing.None });
            services.AddNullEventPublisher();

            // Add identifier generator services.
            //services.AddSingleton<IGuidIdentifierGenerator, GuidIdentifierGenerator>()
            //        .AddSingleton<IStringIdentifierGenerator, StringIdentifierGenerator>();
            services.AddSingleton<IIdentifierGenerator, IdentifierGenerator>();

            // Add event outbox services.
            //services.AddGeneratedDatabaseEventOutbox();
            //services.AddBeefDatabaseEventOutboxPublisherService();

            // Add custom services; in this instance to allow it to call itself for testing purposes.
            //services.AddHttpClient("demo", c => c.BaseAddress = new Uri(_config.GetValue<string>("DemoServiceAgentUrl")));
            //services.AddScoped<Common.Agents.IDemoWebApiAgentArgs>(sp => new Common.Agents.DemoWebApiAgentArgs(sp.GetService<System.Net.Http.IHttpClientFactory>().CreateClient("demo")));
            //services.AddScoped<Common.Agents.IPersonAgent, Common.Agents.PersonAgent>();
            services.AddHttpClient<IPersonAgent, PersonAgent>("person", (sp, c) => c.BaseAddress = new Uri("http://unittest"));

            // Add services; note Beef requires NewtonsoftJson.
            services.AddAutoMapper(CoreEx.Mapping.AutoMapperProfile.Assembly, typeof(ContactData).Assembly)
                    .AddAutoMapperWrapper();

            services.AddControllers().AddNewtonsoftJson();
            //services.AddGrpc();
            services.AddHealthChecks();
            services.AddHttpClient();

            // Set up services for calling http://api.zippopotam.us/.
            //services.AddHttpClient("zippo", c => c.BaseAddress = new Uri(_config.GetValue<string>("ZippoAgentUrl")));
            //services.AddScoped<IZippoAgentArgs>(sp => new ZippoAgentArgs(sp.GetService<System.Net.Http.IHttpClientFactory>().CreateClient("zippo")));
            //services.AddScoped<IZippoAgent, ZippoAgent>();
            services.AddHttpClient<ZippoAgent>("zippo", (sp, c) => c.BaseAddress = new Uri(sp.GetRequiredService<DemoSettings>().ZippoAgentUrl));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Beef (Business Entity Execution Framework) Demo API", Version = "v1" });

                var xmlName = $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
                var xmlFile = Path.Combine(AppContext.BaseDirectory, xmlName);
                if (File.Exists(xmlFile))
                    c.IncludeXmlComments(xmlFile);

                c.OperationFilter<AcceptsBodyOperationFilter>();  // Needed to support AcceptsBodyAttribue where body parameter not explicitly defined.
            });

            //services.AddSwaggerGenNewtonsoftSupport();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            // Override the exception handling.
            app.UseWebApiExceptionHandler();

            // Set up the health checks.
            app.UseHealthChecks("/health");

            // Enable middleware to serve generated Swagger as a JSON endpoint and serve the swagger-ui.
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BEEF Demo"));

            // Configure the ExecutionContext for the request.
            app.UseExecutionContext();

            // Use controllers.
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapGrpcService<Grpc.RobotService>();
            });
        }
    }
}