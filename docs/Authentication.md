# Authentication and Authorization

_Beef_ leverages and integrates seamlessly with the _ASP.NET Core_ out-of-the-box [authentication and authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity) capabilities.

The following sections describe the support enabled in each of the underlying [solution projects](./Solution-Structure.md).

<br/>

## Company.AppName.Api

The _authentication_ process primarily takes place within the API itself. This capability is generally added within the `Startup.cs` leveraging the standard _ASP.NET Core_ capabilities. The minimum requirement is the update of the [`ExecutionContext`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/ExecutionContext.cs) similar to the following within the `Configure` method.

For _CoreEx_ the [`ExecutionContext`](https://github.com/Avanade/CoreEx/blob/main/src/CoreEx/ExecutionContext.cs) plays a key role for housing the user details, namely the `Username` (optionally `UserId`). For the likes of _authorization_ `SetRoles` can also be used. To enable additional capabilities a custom [`ExecutionContext`](../samples/Cdr.Banking/Cdr.Banking.Business/ExecutionContext.cs) can be created (inheriting base) similar to that demonstrated within the [Cdr.Banking](../samples/Cdr.Banking/README.md) sample.

_Note:_ in the following example, the `Username` is set to `emails#oid` claims as the `emails` value may not be unique and is mutable, whereas the `oid` is unique and immutable. This may be appropriate in your scenario especially where the `Username` is used for the likes of auditing. 

``` csharp
public void Configure(IApplicationBuilder app)
{
    ...

    app.UseAuthentication();
    app.UseExecutionContext((ctx, ec) =>
    {
        //ec.UserName = ctx.User.Identity?.Name ?? "Anonymous";
        //return Task.CompletedTask;
        if (ctx.User.Identity is not null && ctx.User.Identity.IsAuthenticated)
        {
            ec.UserId = ctx.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            if (ec.UserId is null)
                throw new AuthenticationException("Token must have an 'oid' (object identifier) claim.");

            ec.UserName = $"{ctx.User.FindFirst("emails")?.Value ?? throw new AuthenticationException("Token must have an 'emails' laim.")}#{ec.UserId}";
        }
        else
            ec.UserName = "Anonymous";

        return Task.CompletedTask;
    });


    ...
``` 

_Disclaimer:_ the above example is purely for _illustrative purposes only_; it is the responsibility of the developer to implement the appropriate authentication, claims verification, etc. that is applicable to their specific use case. 

</br>

## Company.AppName.CodeGen

For the authentication to occur within an API invocation the [`AuthorizeAttribute`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.authorization.authorizeattribute) must be specified. The output of this _attribute_ is controlled by the [code generation](../tools/Beef.CodeGen.Core/README.md) configuration.

The following YAML elements support the `webApiAuthorize` attribute, generally specifying either of the two options, being `Authorize` or `AllowAnonymous` (the value is not validated so any supported value can be specified). The YAML value is inherited from its parent within the hierarchy where not explicitly defined (overridden).
 
1. [`CodeGeneration`](./Entity-CodeGeneration-config.md) 
2. [`Entity`](./Entity-CodeGeneration-config.md) 
3. [`Operation`](./Entity-CodeGeneration-config.md) 

_Note:_ Where no `webApiAuthorize` attribute is specified and cannot be inferred via parental inheritence, it will default to `AllowAnonymous`.

<br/>

## Company.AppName.Test

To support the [intra-domain integration testing](../tools/Beef.Test.NUnit/README.md) the _bearer token_ may need to be passed from the test to the API otherwise all requests will fail with an authentication error.

The [`UnitTextEx.TestSetUp`](https://github.com/Avanade/UnitTestEx/blob/main/src/UnitTestEx/TestSetUp.cs) contains two delegates that can be provided to update the _request_ to include credentials prior to send, these are `OnBeforeHttpRequestSendAsync` and `OnBeforeHttpRequestMessageSendAsync` depending on requirements. Where the test has been configured for a specified user (see `WithUser`), the username is passed as a parameter to the delegate. This should be configured as part of the one-time set up.

``` csharp
// Fixture set up.
TestSetUp.Default.OnBeforeHttpRequestMessageSendAsync = (req, userName, _) =>
{
    req.Headers.Add("cdr-user", userName);
    return Task.CompletedTask;
};

...

// Test logic with user name specified.
var v = Agent<AccountAgent, AccountCollectionResult>()
    .WithUser("jenny")
    .ExpectStatusCode(HttpStatusCode.OK)
    .Run(a => a.GetAccountsAsync(null)).Value;
```

Additionally, the authentication can be bypassed altogether, using the likes of the `ApiTester.BypassAuthorization`. This will result in the underlying services being configured with a [`BypassAuthorizationHandler`](https://github.com/Avanade/UnitTestEx/blob/main/src/UnitTestEx/AspNetCore/BypassAuthorizationHandler.cs).

``` csharp
ApiTester.BypassAuthorization();

// Invoke the agent.
Agent<FooAgent, int>()
    .ExpectStatusCode(HttpStatusCode.OK)
    .Run(a => a.BarAsync(1))
    .Assert(1234);
```