var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables("Bookings_");

var startup = new Startup();
startup.ConfigureServices(builder.Services);

#if (implement_entityframework)
builder.Services.AddOpenTelemetry().UseAzureMonitor().WithTracing(b => b.AddEntityFrameworkCoreInstrumentation().AddSource("CoreEx.*", "Company.AppName.*"));
#else
builder.Services.AddOpenTelemetry().UseAzureMonitor().WithTracing(b => b.AddSource("CoreEx.*", "Company.AppName.*"));
#endif

var app = builder.Build();
startup.Configure(app);

app.Run();