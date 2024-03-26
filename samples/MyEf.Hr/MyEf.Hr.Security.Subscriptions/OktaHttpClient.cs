namespace MyEf.Hr.Security.Subscriptions;

public class OktaHttpClient : TypedHttpClientBase<OktaHttpClient>
{
    public OktaHttpClient(HttpClient client, SecuritySettings settings, IJsonSerializer? jsonSerializer = null, CoreEx.ExecutionContext? executionContext = null) 
        : base(client, jsonSerializer, executionContext)
    {
        Client.BaseAddress = new Uri(settings.OktaHttpClientBaseUri);
        DefaultOptions.EnsureSuccess().ThrowKnownException();
    }

    /// <summary>
    /// Gets the identifier for the email (see <see href="https://developer.okta.com/docs/reference/api/users/#list-users-with-search"/>).
    /// </summary>
    public async Task<Result<OktaUser>> GetUserAsync(Guid id, string email) 
        => Result.GoFrom(await GetAsync<List<OktaUser>>($"/api/v1/users?search=profile.email eq \"{email}\"").ConfigureAwait(false))
            .ThenAs(coll => coll.Count switch 
            {
                0 => Result.NotFoundError($"Employee {id} with email {email} not found within OKTA."),
                1 => Result.Ok(coll[0]),
                _ => Result.NotFoundError($"Employee {id} with email {email} has multiple entries within OKTA.")
            });

    /// <summary>
    /// Deactivates the specified user (<see href="https://developer.okta.com/docs/reference/api/users/#deactivate-user"/>)
    /// </summary>
    public async Task<Result> DeactivateUserAsync(string id) => Result.GoFrom(await PostAsync($"/api/v1/users/{id}/lifecycle/deactivate?sendEmail=true").ConfigureAwait(false));

    /// <summary>
    /// The basic OKTA user properties (see <see href="https://developer.okta.com/docs/reference/api/users/#user-object"/>)
    /// </summary>
    public class OktaUser
    {
        private static readonly string[] _statuses = ["STAGED", "PROVISIONED", "ACTIVE", "RECOVERY", "LOCKED_OUT", "PASSWORD_EXPIRED", "SUSPENDED"];

        public string? Id { get; set; }
        public string? Status { get; set; }
        public bool IsDeactivatable => _statuses.Contains(Status, StringComparer.OrdinalIgnoreCase);
    }
}