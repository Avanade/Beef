Host.CreateDefaultBuilder()
    .ConfigureWebHostDefaults(b => b.UseStartup<Startup>())
    .ConfigureAppConfiguration(c => c.AddEnvironmentVariables("AppName_").AddCommandLine(args))
    .ConfigureServices(s =>
    {
#if (implement_entityframework)
        s.AddOpenTelemetry().UseAzureMonitor().WithTracing(b => b.AddEntityFrameworkCoreInstrumentation().AddSource("CoreEx.*", "Company.AppName.*"));
#else
        s.AddOpenTelemetry().UseAzureMonitor().WithTracing(b => b.AddSource("CoreEx.*", "Company.AppName.*"));
#endif
    })
    .Build()
    .Run();