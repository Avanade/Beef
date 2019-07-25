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
using Microsoft.AspNetCore.Hosting;
using Cosmos = Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;

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
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Beef (Business Entity Execution Framework) Demo API", Version = "v1" });

                var xmlName = $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
                var xmlFile = Path.Combine(AppContext.BaseDirectory, xmlName);
                if (File.Exists(xmlFile))
                    c.IncludeXmlComments(xmlFile);
            });

            services.AddHttpClient();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IConfiguration config, ILoggerFactory loggerFactory, IHttpClientFactory clientFactory)
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

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "BEEF Demo");
            });

            // Override the exception handling.
            var includeExceptionInInternalServerError = config.GetValue<bool>("BeefIncludeExceptionInInternalServerError");
            app.UseExceptionHandler(c => WebApiStartup.ExceptionHandler(c, includeExceptionInInternalServerError));

            // Configure the ExecutionContext for the request.
            app.UseExecutionContext((context, ec) =>
            {
                ec.Username = context.User.Identity.Name ?? "Anonymous";
                ec.Timestamp = DateTime.Now;
            });

            app.UseMvc();
        }
    }
}
