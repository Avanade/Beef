using AzCosmos = Microsoft.Azure.Cosmos;

namespace Cdr.Banking.Api
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

            // Add "page" and "page-size" to the supported paging query string parameters as defined by the CDR specification.
            HttpConsts.PagingArgsPageQueryStringNames.Add("page");
            HttpConsts.PagingArgsTakeQueryStringNames.Add("page-size");

            // Add the core services (including the customized ExecutionContext).
            services.AddSettings<BankingSettings>()
                    .AddExecutionContext(_ => new Business.ExecutionContext())
                    .AddJsonSerializer()
                    .AddReferenceDataOrchestrator()
                    .AddWebApi()
                    .AddReferenceDataContentWebApi()
                    .AddRequestCache()
                    .AddValidationTextProvider()
                    .AddValidators<AccountManager>();

            // Add the cosmos database.
            services.AddSingleton<ICosmos>(sp =>
            {
                var settings = sp.GetRequiredService<BankingSettings>();
                var cco = new AzCosmos.CosmosClientOptions { SerializerOptions = new AzCosmos.CosmosSerializationOptions { PropertyNamingPolicy = AzCosmos.CosmosPropertyNamingPolicy.CamelCase, IgnoreNullValues = true } };
                return new CosmosDb(new AzCosmos.CosmosClient(settings.CosmosConnectionString, cco).GetDatabase(settings.CosmosDatabaseId), sp.GetRequiredService<CoreEx.Mapping.IMapper>());
            });

            // Add the generated reference data services.
            services.AddGeneratedReferenceDataManagerServices()
                    .AddGeneratedReferenceDataDataSvcServices()
                    .AddGeneratedReferenceDataDataServices();

            // Add the generated services.
            services.AddGeneratedManagerServices()
                    .AddGeneratedDataSvcServices()
                    .AddGeneratedDataServices();

            // Add AutoMapper services via Assembly-based probing for Profiles.
            services.AddMappers<BankingSettings>();

            // Add additional services.
            services.AddControllers();
            services.AddHealthChecks();
            services.AddHttpClient();

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Cdr.Banking API", Version = "v1" });
                options.OperationFilter<AcceptsBodyOperationFilter>();  // Needed to support AcceptsBodyAttribute where body parameter not explicitly defined.
                options.OperationFilter<PagingOperationFilter>();       // Needed to support PagingAttribute where PagingArgs parameter not explicitly defined.
            });
        }

        /// <summary>
        /// The configure method called by the runtime; use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="logger">The <see cref="ILogger"/>.</param>
        public void Configure(IApplicationBuilder app, ILogger<Startup> logger)
        {
            // Handle any unhandled exceptions.
            app.UseWebApiExceptionHandler();

            // Add Swagger as a JSON endpoint and to serve the swagger-ui to the pipeline.
            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cdr.Banking"));

            // Add execution context set up to the pipeline.
            app.UseExecutionContext((hc, ec) =>
            {
                // TODO: This would be replaced with appropriate OAuth integration, etc... - this is purely for illustrative purposes only.
                if (!hc.Request.Headers.TryGetValue("cdr-user", out var username) || username.Count != 1)
                    throw new AuthenticationException();

                var bec = (Business.ExecutionContext)ec;
                bec.Timestamp = SystemTime.Get().UtcNow;

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
                        throw new AuthenticationException();
                }

                return Task.CompletedTask;
            });

            // Add health checks page to the pipeline.
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