using System;
using System.IO;
using System.Reflection;
using Azure.Messaging.ServiceBus;
using Beef;
using Beef.AspNetCore.WebApi;
using Beef.Caching.Policy;
#if (implement_cosmos)
using Beef.Data.Cosmos;
#endif
#if (implement_database || implement_entityframework)
using Beef.Data.Database;
#endif
#if (implement_entityframework)
using Beef.Data.EntityFrameworkCore;
#endif
using Beef.Entities;
using Beef.Events.ServiceBus;
using Beef.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Company.AppName.Business;
using Company.AppName.Business.Data;
using Company.AppName.Business.DataSvc;

namespace Company.AppName.Api
{
    /// <summary>
    /// Represents the <b>startup</b> class.
    /// </summary>
    public class Startup
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="config">The <see cref="IConfiguration"/>.</param>
        public Startup(IConfiguration config)
        {
            _config = Check.NotNull(config, nameof(config));

            // Use JSON property names in validation and default the page size.
            ValidationArgs.DefaultUseJsonNames = true;
            PagingArgs.DefaultTake = config.GetValue<int>("BeefDefaultPageSize");
        }

        /// <summary>
        /// The configure services method called by the runtime; use this method to add services to the container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // Add the core beef services.
            services.AddBeefExecutionContext()
                    .AddBeefTextProviderAsSingleton()
                    .AddBeefSystemTime()
                    .AddBeefRequestCache()
                    .AddBeefCachePolicyManager(_config.GetSection("BeefCaching").Get<CachePolicyConfig>())
                    .AddBeefWebApiServices()
                    .AddBeefBusinessServices();

#if (implement_database || implement_entityframework)
            // Add the beef database services (scoped per request/connection).
            services.AddBeefDatabaseServices(() => new AppNameDb(WebApiStartup.GetConnectionString(_config, "Database")));

#endif
#if (implement_entityframework)
            // Add the beef entity framework services (scoped per request/connection).
            services.AddBeefEntityFrameworkServices<AppNameEfDbContext, AppNameEfDb>();

#endif
#if (implement_cosmos)
            // Add the beef cosmos services (singleton).
            services.AddBeefCosmosDbServices<AppNameCosmosDb>(_config.GetSection("CosmosDb"));

#endif
            // Add the generated reference data services.
            services.AddGeneratedReferenceDataManagerServices()
                    .AddGeneratedReferenceDataDataSvcServices()
                    .AddGeneratedReferenceDataDataServices();

            // Add the generated entity services.
            services.AddGeneratedManagerServices()
                    .AddGeneratedValidationServices()
                    .AddGeneratedDataSvcServices()
                    .AddGeneratedDataServices();

            // Add GUID identifier generator service.
            services.AddSingleton<IGuidIdentifierGenerator, GuidIdentifierGenerator>();

#if (implement_database || implement_entityframework)
            // Add transactional event outbox services.
            services.AddGeneratedDatabaseEventOutbox();
            services.AddBeefDatabaseEventOutboxPublisherService();

#endif
            // Add event publishing services.
            var sbcs = _config.GetValue<string>("ServiceBusConnectionString");
            if (!string.IsNullOrEmpty(sbcs))
                services.AddBeefServiceBusSender(new ServiceBusClient(sbcs));
            else
                services.AddBeefNullEventPublisher();

            // Add AutoMapper services via Assembly-based probing for Profiles.
            services.AddAutoMapper(Beef.Mapper.AutoMapperProfile.Assembly, typeof(PersonData).Assembly);

            // Add additional services; note Beef requires NewtonsoftJson.
            services.AddControllers().AddNewtonsoftJson();
            services.AddHealthChecks();
            services.AddHttpClient();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Company.AppName API", Version = "v1" });

                var xmlName = $"{Assembly.GetEntryAssembly()!.GetName().Name}.xml";
                var xmlFile = Path.Combine(AppContext.BaseDirectory, xmlName);
                if (File.Exists(xmlFile))
                    c.IncludeXmlComments(xmlFile);
            });

            services.AddSwaggerGenNewtonsoftSupport();
        }

        /// <summary>
        /// The configure method called by the runtime; use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="logger">The <see cref="ILogger{WebApiExceptionHandlerMiddleware}"/>.</param>
        public void Configure(IApplicationBuilder app, ILogger<WebApiExceptionHandlerMiddleware> logger)
        {
            // Add exception handling to the pipeline.
            app.UseWebApiExceptionHandler(logger, _config.GetValue<bool>("BeefIncludeExceptionInInternalServerError"));

            // Add Swagger as a JSON endpoint and to serve the swagger-ui to the pipeline.
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
}