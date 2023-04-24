namespace MyEf.Hr.Security.Subscriptions;

public class OktaHttpClient : TypedHttpClientBase<OktaHttpClient>
{
    public OktaHttpClient(HttpClient client, SecuritySettings settings, IJsonSerializer? jsonSerializer = null, CoreEx.ExecutionContext? executionContext = null, ILogger<OktaHttpClient>? logger = null) 
        : base(client, jsonSerializer, executionContext, settings, logger)
    {
        Client.BaseAddress = new Uri(settings.OktaHttpClientBaseUri);
        DefaultOptions.WithRetry().EnsureSuccess().ThrowKnownException();
    }

    /// <summary>
    /// Gets the identifier for the email (see <see href="https://developer.okta.com/docs/reference/api/users/#list-users-with-search"/>).
    /// </summary>
    public async Task<OktaUser?> GetUser(string email)
    {
        var response = await GetAsync<List<OktaUser>>($"/api/v1/users?search=profile.email eq \"{email}\"").ConfigureAwait(false);
        return response.Value.SingleOrDefault();
    }

    /// <summary>
    /// Deactivates the specified user (<see href="https://developer.okta.com/docs/reference/api/users/#deactivate-user"/>)
    /// </summary>
    public Task DeactivateUser(string id) => PostAsync($"/api/v1/users/{id}/lifecycle/deactivate?sendEmail=true");

    /// <summary>
    /// The basic OKTA user properties (see <see href="https://developer.okta.com/docs/reference/api/users/#user-object"/>)
    /// </summary>
    public class OktaUser
    {
        public string? Id { get; set; }
        public string? Status { get; set; }
        public bool IsDeactivatable => new string[] { "STAGED", "PROVISIONED", "ACTIVE", "RECOVERY", "LOCKED_OUT", "PASSWORD_EXPIRED", "SUSPENDED" }.Contains(Status?.ToUpperInvariant());
    }
}