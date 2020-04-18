# Authentication

This article will describe how to integrate _authentication_ into a _Beef_ solution; in which _Beef_ in an of itself does not enable directly, but leverages the capabilities such as [Azure Active Directory B2C](https://azure.microsoft.com/en-us/services/active-directory-b2c/) to perform. Equally, it could be any _Identity platform_ of your choosing.

For the purposes of this artice _AAD B2C_ will be used. Review Microsoft's [documentation](https://docs.microsoft.com/en-us/azure/active-directory-b2c/) on how to set up and configure in [_Azure_](https://portal.azure.com/) as this will not be covered here.

<br/>

## Company.AppName.Api

The _authentication_ process primarily takes place within the API itself. This capability is added within the `Startup.cs` leveraging the standard _ASP.NET Core_ [authentication](https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication/) capabilities, as further described.

<br/>

### ConfigureServices

Within the `ConfigureServices` method a call to `AddAuthentication` is required to configure. For _AAD B2C_ `AddAzureADB2CBearer`  is used to load in the appropriate configuration ([`Microsoft.AspNetCore.Authentication.AzureADB2C.UI`](https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.AzureADB2C.UI) NuGet package is required). 

``` csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add authentication using Azure AD B2C.
    services.AddAuthentication(AzureADB2CDefaults.BearerAuthenticationScheme)
        .AddAzureADB2CBearer(options => _config.Bind("AzureAdB2C", options));

    ...
```

<br/>

The `Bind` method loads the configuration from the application settings; for _Beef_ this is the `webapisettings.json` file. The `"AzureAdB2C"` represents the node within the underlying JSON that contains the corresponding configuration:

``` json
{
  "AzureAdB2C": {
    "Domain": "Xxxx.onmicrosoft.com",                   // Azure AD B2C domain name
    "Instance": "https://Xxxx.b2clogin.com/tfp/",       // Instance name, the domain name Xxxx is duplicated here
    "ClientId": "12345678-097e-4786-b489-123dabeff688", // Application (client) identifier
    "SignUpSignInPolicyId": "B2C_1_SignUpSignIn"        // SignUpSignIn policy name
  }, ...
```

<br/>

#### Swagger

Where [Swagger UI](https://www.nuget.org/packages/Swashbuckle.AspNetCore.SwaggerUI/) is being used and support for entering in the bearer token is required, then `AddSecurityDefinition` and `OperationFilter` are required, as follows:

``` csharp
services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Xxxx API", Version = "v1" });

    var xmlName = $"{Assembly.GetEntryAssembly()!.GetName().Name}.xml";
    var xmlFile = Path.Combine(AppContext.BaseDirectory, xmlName);
    if (File.Exists(xmlFile))
        c.IncludeXmlComments(xmlFile);

    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
    });

    c.OperationFilter<SecurityRequirementsOperationFilter>();
});
```

<br/>

### Configure

The next step is to add the _authentication_ into the HTTP request pipeline. The sequencing of this is important, as follows: `UseRouting`, `UseAuthentication`, `UseAuthorization`, `UseExecutionContext` and `UseEndpoints`.

For _Beef_ the [`ExecutionContext`](../src/Beef.Core/ExecutionContext.cs) plays a key role for housing the user details, namely the `Username` (optionally `UserId`). For the likes of _authorization_ `SetRoles` can also be used. To enable additional capabilities a custom [`ExecutionContext`](../samples/Cdr.Banking/Cdr.Banking.Business/ExecutionContext.cs) can be created (inheriting base) similar to that demonstrated within the [Cdr.Banking](../samples/Cdr.Banking/README.md) sample.

The `UseExecutionContext` also represents an opportuntity to perform further authentication validation, such as verifying the issuer (`iss`) and audience (`aud`) claims for example. 

_Note:_ the `Username` is set to `emails#oid` claims as the `emails` value may not be unique and is mutable, whereas the `oid` is unique and immutable. This may also be appropriate in your scenario especially where the `Username` is used for the likes of auditing. 

``` csharp
public void Configure(IApplicationBuilder app, IHttpClientFactory clientFactory)
{
    ...

    // Use routing, authentication and authorization.
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    // Add execution context set up to the pipeline (must be after UseAuth* as needs claims from user).
    app.UseExecutionContext((ctx, ec) =>
    {
        if (ctx.User.Identity.IsAuthenticated)
        {
            if (Guid.TryParse(ctx.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value, out var oid))
                fec.UserId = oid;
            else
                throw new Beef.AuthenticationException("Token must have an 'oid' (object identifier GUID) claim.");

            fec.Username = $"{ctx.User.FindFirst("emails")?.Value ?? throw new Beef.AuthenticationException("Token must have an 'emails' claim.")}#{fec.UserId}";
        }
        else
            fec.Username = "Anonymous";
    });

    // Finally add the controllers.
    app.UseEndpoints(endpoints => endpoints.MapControllers());
``` 

</br>

_Disclaimer:_ the example above is for illustrative purposes only; it is the responsibility of the developer to fully implement the claims verification that is applicable to their specific use case. 

</br>

## Company.AppName.CodeGen

For the authentication to occur within an API invocation the [`AuthorizeAttribute`](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authorization.authorizeattribute) must be specified. The output of this _attribute_ is controlled by the [code generation](../tools/Beef.CodeGen.Core/README.md) configuration.

The following XML elements support the `WebApiAuthorize` attribute, with two options `Authorize` or `AllowAnonymous`. The value is inherited from its parent within the hierarchy where not explicitly defined (overridden):
 
1. [`CodeGeneration`](./Entity-CodeGeneration-element.md) 
2. [`Entity`](./Entity-CodeGeneration-element.md) 
3. [`Operation`](./Entity-CodeGeneration-element.md) 

_Note:_ Where no `WebApiAuthorize` attribute is specified and cannot be inferred via parental inheritence, it will default to `AllowAnonymous`.

<br/>

## Company.AppName.Test

To support the [intra-domain integration testing](../tools/Beef.Test.NUnit/README.md) the _bearer token_ must be passed from the test to the API otherwise all requests will fail with an authentication error.


### FixtureSetUp

The [`AgentTester`](../tools/Beef.Test.NUnit/AgentTester.cs) has a static `RegisterBeforeRequest` method that enables the [`HttpRequestMessage`](https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httprequestmessage) to be modified prior to the request being made.

Refer to the Microsoft [documentation](https://docs.microsoft.com/en-us/azure/active-directory-b2c/configure-ropc) for further AAD B2C configuration and troubleshooting.

The following code demonstrates the creation of the _bearer token_ by calling the _OAuth_ endpoint passing the username and password. The resulting _token_ is then added to the HTTP request header. Note that the username comes from the `ExecutionContext.Current.Username` which is set within each executing test (see next section).

``` csharp
[SetUpFixture]
public class FixtureSetUp
{
    private static readonly KeyedLock<string> _lock = new KeyedLock<string>();
    private static readonly Dictionary<string, string> _userTokens = new Dictionary<string, string>();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        TestSetUp.RegisterSetUp(async (count, _) =>
        {
            return await DatabaseExecutor.RunAsync(
                count == 0 ? DatabaseExecutorCommand.ResetAndDatabase : DatabaseExecutorCommand.ResetAndData, 
                AgentTester.Configuration["ConnectionStrings:Database"],
                typeof(DatabaseExecutor).Assembly, typeof(Database.Program).Assembly, Assembly.GetExecutingAssembly()).ConfigureAwait(false) == 0;
        });

        AgentTester.StartupTestServer<Startup>(environmentVariablesPrefix: "AppName_");
        AgentTester.DefaultExpectNoEvents = true;
        AgentTester.RegisterBeforeRequest(BeforeRequet);
    }

    private static void BeforeRequet(HttpRequestMessage r)
    {
        var username = ExecutionContext.Current.Username;
        if (username.Equals("Anonymous", System.StringComparison.OrdinalIgnoreCase))
            return;

        // Cache the token for a user to minimise web calls (perf improvement).
        _lock.Lock(username, () =>
        {
            if (!_userTokens.TryGetValue(username, out string? token))
            {
                var data = new NameValueCollection
                {
                    { "grant_type", "password" },
                    { "client_id", "12345678-097e-4786-b489-123dabeff688" }, // Application (client) identifier
                    { "scope", $"openid 12345678-097e-4786-b489-123dabeff688 offline_access" },
                    { "username", $"{username}@domain.com" },                // Appends domain to user (if applicable)
                    { "password", "password" }                               // Assumes all test users have same password
                };

                // The 'Xxxx' represents your AAD B2C domain
                using var webClient = new WebClient();
                var bytes = webClient.UploadValues("https://Xxxx.b2clogin.com/Xxxx.onmicrosoft.com/oauth2/v2.0/token?p=B2C_1_ROPC_Auth", "POST", data);
                var body = Encoding.UTF8.GetString(bytes);
                token = (string)JObject.Parse(body)["access_token"]!;
                _userTokens.Add(username, token);
            }

            r.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        });
    }
}
```

<br/>

### Tests

The username can be specified for each test, either using the [`TestSetUpAttribute`](../tools/Beef.Test.NUnit/TestSetUpAttribute.cs) or specifiying when invoking the [AgentTester.Create](../tools/Beef.Test.NUnit/AgentTester.cs). This in turn will set the `ExecutionContext.Current.Username`.

``` csharp
[Test, TestSetUp("username")]
public void A110_GetMe_NotFound()
{
    AgentTester.Create<ClaimantAgent, Claimant>()
        .ExpectStatusCode(HttpStatusCode.NotFound)
        .ExpectErrorType(Beef.ErrorType.NotFoundError)
        .Run((a) => a.Agent.GetMeAsync());

    AgentTester.Create<ClaimantAgent, Claimant>("username2")
        .ExpectStatusCode(HttpStatusCode.NotFound)
        .ExpectErrorType(Beef.ErrorType.NotFoundError)
        .Run((a) => a.Agent.GetMeAsync());
}
```