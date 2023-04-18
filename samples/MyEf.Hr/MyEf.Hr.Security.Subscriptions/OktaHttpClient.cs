namespace MyEf.Hr.Security.Subscriptions;

public class OktaHttpClient : TypedHttpClientCore<OktaHttpClient>
{
    public OktaHttpClient(HttpClient client, SecuritySettings settings, IJsonSerializer? jsonSerializer = null, CoreEx.ExecutionContext? executionContext = null, ILogger<TypedHttpClientCore<OktaHttpClient>>? logger = null) 
        : base(client, jsonSerializer, executionContext, settings, logger)
    {
        Client.BaseAddress = new Uri(settings.OktaHttpClientBaseUri);
        DefaultOptions.WithRetry().EnsureOK().EnsureSuccess().ThrowKnownException();
    }

    /// <summary>
    /// Gets the identifier for the email (see <see href="https://developer.okta.com/docs/reference/api/users/#list-users-with-search"/>).
    /// </summary>
    public async Task<string?> GetIdentifier(string email)
    {
        var response = await EnsureOK().GetAsync<List<OktaUser>>($"/api/v1/users?search=profile.email eq \"{email}\"").ConfigureAwait(false);
        var user = response.Value.SingleOrDefault();

        return user?.Status?.ToUpperInvariant() switch
        {
            "STAGED" or "PROVISIONED" or "ACTIVE" or "RECOVERY" or "LOCKED_OUT" or "PASSWORD_EXPIRED" or "SUSPENDED" => user?.Id,
            _ => null
        };
    }

    /// <summary>
    /// Deactivates the specified user (<see href="https://developer.okta.com/docs/reference/api/users/#deactivate-user"/>)
    /// </summary>
    public async Task DeactivateUser(string identifier)
    {
        var response = await EnsureOK().EnsureNoContent().PostAsync($"/api/v1/users/{identifier}/lifecycle/deactivate?sendEmail=true").ConfigureAwait(false);
        response.ThrowOnError();
    }

    /// <summary>
    /// The basic OKTA user properties (see <see href="https://developer.okta.com/docs/reference/api/users/#user-object"/>)
    /// </summary>
    private class OktaUser
    {
        public string? Id { get; set; }
        public string? Status { get; set; }
    }
}