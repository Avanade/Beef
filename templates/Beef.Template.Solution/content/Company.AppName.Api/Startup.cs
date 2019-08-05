using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using Beef.AspNetCore.WebApi;
using Beef.Caching.Policy;
using Beef.Diagnostics;
using Beef.Entities;
using Beef.Validation;
using Beef.WebApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using Company.AppName.Business;
using Company.AppName.Business.Data;

namespace Company.AppName.Api
{
    /// <summary>
    /// Represents the <b>startup</b> class.
    /// </summary>
    public class Startup
    {
        private ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        public Startup(IConfiguration config)
        {
            // Use JSON property names in validation.
            ValidationArgs.DefaultUseJsonNames = true;

            // Load the cache policies.
            CachePolicyManager.SetFromCachePolicyConfig(config.GetSection("BeefCaching").Get<CachePolicyConfig>());
            CachePolicyManager.StartFlushTimer(CachePolicyManager.TenMinutes, CachePolicyManager.FiveMinutes);

            // Register the ReferenceData provider.
            Beef.RefData.ReferenceDataManager.Register(new ReferenceDataProvider());

#if (implement_database || implement_entityframework)
            // Register the database.
            Database.Register(() => new Database(WebApiStartup.GetConnectionString(config, "Database")));

#endif
#if (implement_cosmos)
            // Register the DocumentDb/CosmosDb client.
            CosmosDb.Register(() =>
            {
                var cs = config.GetSection("CosmosDb");
                return new CosmosDb(new Cosmos.CosmosClient(cs.GetValue<string>("EndPoint"), cs.GetValue<string>("AuthKey")), cs.GetValue<string>("Database"));
            });

#endif
            // Default the page size.
            PagingArgs.DefaultTake = config.GetValue<int>("BeefDefaultPageSize");
        }

        /// <summary>
        /// The configure services method called by the runtime; use this method to add services to the container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "Company.AppName API", Version = "v1" });

                var xmlName = $"{Assembly.GetEntryAssembly().GetName().Name}.xml";
                var xmlFile = Path.Combine(AppContext.BaseDirectory, xmlName);
                if (File.Exists(xmlFile))
                    c.IncludeXmlComments(xmlFile);
            });

            services.AddHttpClient();
        }

        /// <summary>
        /// The configure method called by the runtime; use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="env">The <see cref="IHostingEnvironment"/>.</param>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        /// <param name="loggerFactory">The <see cref="ILoggerFactory"/>.</param>
        /// <param name="clientFactory">The <see cref="IHttpClientFactory"/>.</param>
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
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Company.AppName");
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