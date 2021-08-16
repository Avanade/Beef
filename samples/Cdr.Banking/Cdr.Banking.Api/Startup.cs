using Beef;
using Beef.AspNetCore.WebApi;
using Beef.Caching.Policy;
using Beef.Entities;
using Beef.Validation;
using Cdr.Banking.Business;
using Cdr.Banking.Business.Data;
using Cdr.Banking.Business.DataSvc;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Cosmos = Microsoft.Azure.Cosmos;

namespace Cdr.Banking.Api
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
            _config = config;

            // Use JSON property names in validation; and determine whether unhandled exception details are to be included in the response.
            ValidationArgs.DefaultUseJsonNames = true;

            // Add "page" and "page-size" to the supported paging query string parameters as defined by the CDR specification; and default the page size to 25 from config.
            WebApiQueryString.PagingArgsPageQueryStringNames.Add("page");
            WebApiQueryString.PagingArgsTakeQueryStringNames.Add("page-size");
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

            // Add the core beef services (including the customized ExecutionContext).
            services.AddBeefExecutionContext(_ => new Business.ExecutionContext())
                    .AddBeefTextProviderAsSingleton()
                    .AddBeefSystemTime()
                    .AddBeefRequestCache()
                    .AddBeefCachePolicyManager(_config.GetSection("BeefCaching").Get<CachePolicyConfig>())
                    .AddBeefWebApiServices()
                    .AddBeefBusinessServices();

            // Add the data sources as singletons for dependency injection requirements.
            var ccs = _config.GetSection("CosmosDb");
            services.AddSingleton<Beef.Data.Cosmos.ICosmosDb>(_ => new CosmosDb(new Cosmos.CosmosClient(ccs.GetValue<string>("EndPoint"), ccs.GetValue<string>("AuthKey")), ccs.GetValue<string>("Database")));

            // Add beef cache policy management.
            services.AddBeefCachePolicyManager(_config.GetSection("BeefCaching").Get<CachePolicyConfig>());

            // Add the generated reference data services for dependency injection requirements.
            services.AddGeneratedReferenceDataManagerServices()
                    .AddGeneratedReferenceDataDataSvcServices()
                    .AddGeneratedReferenceDataDataServices();

            // Add the generated services for dependency injection requirements.
            services.AddGeneratedManagerServices()
                    .AddGeneratedValidationServices()
                    .AddGeneratedDataSvcServices()
                    .AddGeneratedDataServices();

            // Add AutoMapper services via Assembly-based probing for Profiles.
            services.AddAutoMapper(Beef.Mapper.AutoMapperProfile.Assembly, typeof(AccountData).Assembly);

            // Add services; note Beef requires NewtonsoftJson.
            services.AddControllers().AddNewtonsoftJson();
            services.AddHealthChecks();
            services.AddHttpClient();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Cdr.Banking API", Version = "v1" });

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
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public void Configure(IApplicationBuilder app, ILogger<Startup> logger)
        {
            // Add exception handling to the pipeline.
            app.UseWebApiExceptionHandler(logger, _config.GetValue<bool>("BeefIncludeExceptionInInternalServerError"));

            // Add Swagger as a JSON endpoint and to serve the swagger-ui to the pipeline.
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cdr.Banking"));

            // Add health checks page to the pipeline.
            app.UseHealthChecks("/health");

            // Add execution context set up to the pipeline.
            app.UseExecutionContext((hc, ec) =>
            {
                // TODO: This would be replaced with appropriate OAuth integration, etc... - this is purely for illustrative purposes only.
                if (!hc.Request.Headers.TryGetValue("cdr-user", out var username) || username.Count != 1)
                    throw new Beef.AuthorizationException();

                var bec = (Business.ExecutionContext)ec;
                bec.Timestamp = SystemTime.Get(hc.RequestServices).UtcNow;

                switch (username[0])
                {
                    case "jessica":
                        bec.Accounts.AddRange(new string[] { "12345678", "34567890", "45678901" });
                        break;

                    case "jenny":
                        bec.Accounts.Add("23456789");
                        break;

                    case "jason":
                        break;

                    default:
                        throw new Beef.AuthorizationException();
                }

                return Task.CompletedTask;
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