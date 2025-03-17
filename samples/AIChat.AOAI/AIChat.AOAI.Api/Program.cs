using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables("AOAI_");

var startup = new Startup();
startup.ConfigureServices(builder.Services, builder.Environment);

// Add the semantic kernel
builder.Services.AddKernel()
    .AddAzureOpenAIChatCompletion(
        builder.Configuration["AzureOpenAI::ModelName"] ?? string.Empty,
        builder.Configuration["AzureOpenAI::Endpoint"] ?? string.Empty,
        builder.Configuration["AzureOpenAI::APIKey"] ?? string.Empty
    );

builder.Services.AddTransient((serviceProvider) => {
    return new Kernel(serviceProvider);
});

if (!builder.Environment.IsDevelopment())
    builder.Services.AddOpenTelemetry().UseAzureMonitor().WithTracing(b => b.AddSource("CoreEx.*", "AIChat.AOAI.*"));

var app = builder.Build();
startup.Configure(app);

app.Run();