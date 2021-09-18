using Azure.Messaging.EventHubs.Producer;
using Azure.Messaging.ServiceBus;
using Beef.AspNetCore.WebApi;
using Beef.Caching.Policy;
using Beef.Data.Cosmos;
using Beef.Data.Database;
using Beef.Data.EntityFrameworkCore;
using Beef.Demo.Business;
using Beef.Demo.Business.Data;
using Beef.Demo.Business.DataSvc;
using Beef.Entities;
using Beef.Events.EventHubs;
using Beef.Events.ServiceBus;
using Beef.Grpc;
using Beef.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;

namespace Beef.Demo.Api
{
    public class Startup
    {
        private readonly IConfiguration _config;
        private ILogger _logger;

        public Startup(IConfiguration config)
        {
            _config = config;

            // Use JSON property names in validation.
            ValidationArgs.DefaultUseJsonNames = true;

            // Default the page size.
            PagingArgs.DefaultTake = config.GetValue<int>("BeefDefaultPageSize");
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add the core beef services.
            services.AddBeefExecutionContext()
                    .AddBeefSystemTime()
                    .AddBeefRequestCache()
                    .AddBeefCachePolicyManager(_config.GetSection("BeefCaching").Get<CachePolicyConfig>())
                    .AddBeefWebApiServices()
                    .AddBeefGrpcServiceServices()
                    .AddBeefBusinessServices()
                    .AddBeefTextProviderAsSingleton();

            // Add the data sources as singletons for dependency injection requirements.
            services.AddBeefDatabaseServices(() => new Database(WebApiStartup.GetConnectionString(_config, "BeefDemo")))
                    .AddBeefEntityFrameworkServices<EfDbContext, EfDb>()
                    .AddBeefCosmosDbServices<CosmosDb>(_config.GetSection("CosmosDb"))
                    .AddSingleton<ITestOData>(_ => new TestOData(new Uri(WebApiStartup.GetConnectionString(_config, "TestOData"))))
                    .AddSingleton<ITripOData>(_ => new TripOData(new Uri(WebApiStartup.GetConnectionString(_config, "TripOData"))));

            // Add the generated reference data services for dependency injection requirements.
            services.AddGeneratedReferenceDataManagerServices()
                    .AddGeneratedReferenceDataDataSvcServices()
                    .AddGeneratedReferenceDataDataServices();

            // Add the generated entity services for dependency injection requirements.
            services.AddGeneratedManagerServices()
                    .AddGeneratedValidationServices()
                    .AddGeneratedDataSvcServices()
                    .AddGeneratedDataServices();

            // Add event publishing.
            var ehcs = _config.GetValue<string>("EventHubConnectionString");
            var sbcs = _config.GetValue<string>("ServiceBusConnectionString");
            if (!string.IsNullOrEmpty(sbcs))
                services.AddBeefEventHubEventProducer(new EventHubProducerClient(ehcs));
            else if (!string.IsNullOrEmpty(sbcs))
                services.AddBeefServiceBusSender(new ServiceBusClient(sbcs));
            else
                services.AddBeefNullEventPublisher();

            // Add identifier generator services.
            services.AddSingleton<IGuidIdentifierGenerator, GuidIdentifierGenerator>()
                    .AddSingleton<IStringIdentifierGenerator, StringIdentifierGenerator>();

            // Add event outbox services.
            services.AddGeneratedDatabaseEventOutbox();
            services.AddBeefDatabaseEventOutboxPublisherService();

            // Add custom services; in this instance to allow it to call itself for testing purposes.
            services.AddHttpClient("demo", c => c.BaseAddress = new Uri(_config.GetValue<string>("DemoServiceAgentUrl")));
            services.AddScoped<Common.Agents.IDemoWebApiAgentArgs>(sp => new Common.Agents.DemoWebApiAgentArgs(sp.GetService<System.Net.Http.IHttpClientFactory>().CreateClient("demo")));
            services.AddScoped<Common.Agents.IPersonAgent, Common.Agents.PersonAgent>();

            // Add services; note Beef requires NewtonsoftJson.
            services.AddAutoMapper(Mapper.AutoMapperProfile.Assembly, typeof(Beef.Demo.Common.Grpc.Transformers).Assembly, typeof(ContactData).Assembly);
            services.AddControllers().AddNewtonsoftJson();
            services.AddGrpc();
            services.AddHealthChecks();
            services.AddHttpClient();

            // Set up services for calling http://api.zippopotam.us/.
            services.AddHttpClient("zippo", c => c.BaseAddress = new Uri(_config.GetValue<string>("ZippoAgentUrl")));
            services.AddScoped<IZippoAgentArgs>(sp => new ZippoAgentArgs(sp.GetService<System.Net.Http.IHttpClientFactory>().CreateClient("zippo")));
            services.AddScoped<IZippoAgent, ZippoAgent>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Beef (Business Entity Execution Framework) Demo API", Version = "v1" });

                var xmlName = $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
                var xmlFile = Path.Combine(AppContext.BaseDirectory, xmlName);
                if (File.Exists(xmlFile))
                    c.IncludeXmlComments(xmlFile);
            });

            services.AddSwaggerGenNewtonsoftSupport();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IConfiguration config, ILogger<WebApiExceptionHandlerMiddleware> logger)
        {
            // Configure the logger.
            _logger = Check.NotNull(logger, nameof(logger));

            // Override the exception handling.
            app.UseWebApiExceptionHandler(_logger, config.GetValue<bool>("BeefIncludeExceptionInInternalServerError"));

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
                endpoints.MapGrpcService<Grpc.RobotService>();
            });
        }
    }
}