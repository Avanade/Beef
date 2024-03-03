new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureAppConfiguration(builder => builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true))
    .ConfigureServices(services =>
    {
        services.ConfigureFunctionsApplicationInsights()
                .AddApplicationInsightsTelemetryWorkerService()
                .AddOpenTelemetry().WithTracing(b => b.AddSource("CoreEx.*", "Company.AppName.*").AddAzureMonitorTraceExporter());

        // See https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide?tabs=windows#start-up-and-configuration
        services.Configure<LoggerFilterOptions>(options =>
        {
            var rule = options.Rules.FirstOrDefault(rule => rule.ProviderName == typeof(Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider).FullName);
            if (rule is not null)
                options.Rules.Remove(rule);
        });
    })
    .ConfigureHostStartup<Company.AppName.Services.Startup>()
    .Build().Run();