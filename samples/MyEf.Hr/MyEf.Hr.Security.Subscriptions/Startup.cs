namespace MyEf.Hr.Security.Subscriptions;

/// <summary>
/// The <see cref="HostStartup"/> to enable testable dependency injection.
/// </summary>
public class Startup : HostStartup
{
    /// <inheritdoc/>
    public override void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder config)
    {
        config.AddEnvironmentVariables("MyEf_Hr_");
    }

    /// <inheritdoc/>
    public override void ConfigureServices(IServiceCollection services)
    {
        services
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
                o.EventDataDeserializationErrorHandling = ErrorHandling.HandleBySubscriber;
            })
            .AddTypedHttpClient<OktaHttpClient>("OktaApi");
    }
}