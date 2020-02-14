using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Beef.AspNetCore.WebApi;
using Beef.Caching.Policy;
using Beef.Demo.Business;
using Beef.Demo.Business.Data;
using Beef.Diagnostics;
using Beef.Entities;
using Beef.Validation;
using Beef.WebApi;
using Microsoft.AspNetCore.Builder;
using Cosmos = Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Beef.Events;
using Beef.Events.Publish;
using Microsoft.Azure.EventHubs;
using Microsoft.OpenApi.Models;

namespace Beef.Demo.Api
{
    public class Startup
    {
        private ILogger _logger;

        public Startup(IConfiguration config)
        {
            // Use JSON property names in validation.
            ValidationArgs.DefaultUseJsonNames = true;

            // Load the cache policies.
            CachePolicyManager.SetFromCachePolicyConfig(config.GetSection("BeefCaching").Get<CachePolicyConfig>());
            CachePolicyManager.StartFlushTimer(CachePolicyManager.TenMinutes, CachePolicyManager.FiveMinutes);

            // Register the ReferenceData provider.
            RefData.ReferenceDataManager.Register(new ReferenceDataProvider());

            // Register the database.
            Database.Register(() => new Database(WebApiStartup.GetConnectionString(config, "BeefDemo")));
            Beef.Data.Database.DatabaseInvoker.Default = new Beef.Data.Database.SqlRetryDatabaseInvoker();

            // Register the DocumentDb/CosmosDb client.
            CosmosDb.Register(() =>
            {
                var cs = config.GetSection("CosmosDb");
                return new CosmosDb(new Cosmos.CosmosClient(cs.GetValue<string>("EndPoint"), cs.GetValue<string>("AuthKey")), cs.GetValue<string>("Database"));
            });

            // Register the test OData service.
            TestOData.Register(() => new TestOData(WebApiStartup.GetConnectionString(config, "TestOData")));

            // Default the page size.
            PagingArgs.DefaultTake = config.GetValue<int>("BeefDefaultPageSize");

            // Configure the Service Agents from the configuration and register.
            var sac = config.GetSection("BeefServiceAgents").Get<WebApiServiceAgentConfig>();
            sac?.RegisterAll();

            // Set up the event publishing to event hubs.
            if (config.GetValue<bool>("EventHubPublishing"))
            {
                var ehc = EventHubClient.CreateFromConnectionString(config.GetValue<string>("EventHubConnectionString"));
                ehc.RetryPolicy = RetryPolicy.Default;
                var ehp = new EventHubPublisher(ehc);
                Event.Register((events) => ehp.Publish(events));
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add services; note Beef requires NewtonsoftJson.
            services.AddControllers().AddNewtonsoftJson();
            services.AddHealthChecks();
            services.AddHttpClient();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Beef (Business Entity Execution Framework) Demo API", Version = "v1" });

                var xmlName = $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
                var xmlFile = Path.Combine(AppContext.BaseDirectory, xmlName);
                if (File.Exists(xmlFile))
                    c.IncludeXmlComments(xmlFile);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IConfiguration config, ILoggerFactory loggerFactory, IHttpClientFactory clientFactory)
        {
            // Configure the logger.
            _logger = loggerFactory.CreateLogger("Logging");
            Logger.RegisterGlobal((largs) => WebApiStartup.BindLogger(_logger, largs));

            // Register the HttpClientCreate so it uses the factory.
            WebApiServiceAgentManager.RegisterHttpClientCreate((rd) =>
            {
                var hc = clientFactory.CreateClient(rd.BaseAddress.AbsoluteUri);
                hc.BaseAddress = rd.BaseAddress;
                return hc;
            });

            // Override the exception handling.
            WebApiExceptionHandlerMiddleware.IncludeUnhandledExceptionInResponse = config.GetValue<bool>("BeefIncludeExceptionInInternalServerError");
            app.UseWebApiExceptionHandler();

            // Set up the health checks.
            app.UseHealthChecks("/health");

            // Enable middleware to serve generated Swagger as a JSON endpoint and serve the swagger-ui.
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BEEF Demo"));

            // Configure the ExecutionContext for the request.
            app.UseExecutionContext((context, ec) =>
            {
                ec.Username = context.User.Identity.Name ?? WebApiExecutionContextMiddleware.DefaultUsername;
                ec.Timestamp = DateTime.Now;
            });

            // Use controllers.
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
