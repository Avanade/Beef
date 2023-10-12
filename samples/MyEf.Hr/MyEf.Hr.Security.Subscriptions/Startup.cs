[assembly: FunctionsStartup(typeof(MyEf.Hr.Security.Subscriptions.Startup))]

namespace MyEf.Hr.Security.Subscriptions;

public class Startup : FunctionsStartup
{
    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder) => builder.ConfigurationBuilder
        .AddJsonFile(Path.Combine(builder.GetContext().ApplicationRootPath ?? "", "appsettings.json"), optional: true)
        .AddEnvironmentVariables("Hr_");

    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services
            .AddSettings<SecuritySettings>()
            .AddExecutionContext()
            .AddJsonSerializer()
            .AddValidationTextProvider()
            .AddCloudEventSerializer()
            .AddEventSubscribers<Startup>()
            .AddEventSubscriberOrchestrator((_, o) =>
            {
                o.NotSubscribedHandling = ErrorHandling.CompleteAsSilent;
                o.AddSubscribers(EventSubscriberOrchestrator.GetSubscribers<Startup>());
            })
            .AddAzureServiceBusOrchestratedSubscriber((_, o) =>
            {
                o.EventDataDeserializationErrorHandling = ErrorHandling.Handle;
            })
            .AddTypedHttpClient<OktaHttpClient>("OktaApi");
    }
}