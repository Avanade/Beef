# Beef.Test.NUnit

Unit and intra-domain integration testing framework. This capability leverages [NUnit](https://github.com/nunit/nunit) for all testing.

## Intra-domain vs. inter-domain testing

**Intra-domain** essentially means within (isolated to) the domain itself; excluding any external domain-based dependencies. For example a _Billing_ domain, may be supported by a SQL Server Database for data persistence, and as such is a candidate for inclusion within the testing.

However, if within this _Billing_ domain, there is an _Invoice_ entity with a _CustomerId_ attribute where the corresponding _Customer_ resides in another domain (external domain-based dependency) which is called to validate existence, then this should be excluded from within the testing. In this example, the cross-domain invocation should be mocked-out as it is considered **Inter-domain**.

In summary, **Intra-** is about _tight-coupling_, and **Inter-** is about _loose-coupling_.

<br/>

## Test set up

Before a test executes, there may be a requirement to perform set up activities; such as a test data source, etc. for example. The [`TestSetUp`](./TestSetUp.cs) and [`TestSetUpAttribute`](./TestSetUpAttribute.cs) enable.

<br/>

### One-time set-up for the set-up fixture

Within the [`OneTimeSetUp`](https://github.com/nunit/docs/wiki/OneTimeSetUp-Attribute) for the [`SetUpFixture`](https://github.com/nunit/docs/wiki/SetUpFixture-Attribute) the `TestSetUp.RegisterSetUp` enables the configuration of the set up (including that which is re-invoked with the one-time set-up for the test fixture). Other set up logic including setting the defaults for the local _Reference Data_ (`TestSetUp.SetDefaultLocalReferenceData`) is initiated. An [example](../../samples/My.Hr/My.Hr.Test/FixtureSetup.cs) is as follows:

``` csharp
[SetUpFixture]
public class FixtureSetUp
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        TestSetUp.DefaultEnvironmentVariablePrefix = "Hr";
        TestSetUp.SetDefaultLocalReferenceData<IReferenceData, ReferenceDataAgentProvider, IReferenceDataAgent, ReferenceDataAgent>();
        TestSetUp.DefaultExpectNoEvents = true;
        var config = AgentTester.BuildConfiguration<Startup>();

        TestSetUp.RegisterSetUp(async (count, _) =>
        {
            return await DatabaseExecutor.RunAsync(new DatabaseExecutorArgs(
                count == 0 ? DatabaseExecutorCommand.ResetAndDatabase : DatabaseExecutorCommand.ResetAndData, config["ConnectionStrings:Database"],
                typeof(Database.Program).Assembly, Assembly.GetExecutingAssembly()) { UseBeefDbo = true } ).ConfigureAwait(false) == 0;
        });
    }
}
```

<br/>

### One-time set-up for the test class

Within the [`OneTimeSetUp`](https://github.com/nunit/docs/wiki/OneTimeSetUp-Attribute) for each [`TestFixture`](https://github.com/nunit/docs/wiki/TestFixture-Attribute) the `TestSetUp.Reset` should be invoked; this will (re)invoke the registered set up (`TestSetUp.RegisterSetUp`). Example as follows

``` csharp
[TestFixture, NonParallelizable]
public class PersonTest
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        TestSetUp.Reset();
    }
```

Or, alternatively, by using the [`TestSetUpAttribute`](./TestSetUpAttribute.cs) on a test method this will perform the same function.

<br/>

## Agent testing

As _beef_ is largely about accelerating API development this testing capability further enables and simplifies the testing of APIs directly. The philosophy of this testing is to exercise the APIs end-to-end, including over the wire transport and protocols, and tightly-coupled backend data sources where applicable (_intra-domain_).

The [`AgentTester`](./AgentTester.cs) provides a means to invoke an API and _assert_ (expect) a given response to be considered valid. The `AgentTester` invokes the API via its corresponding [Service Agent](../../docs/Layer-ServiceAgent.md). The advantage of this is that the HTTP request/response, HTTP headers, URL parameterisation, etc. are verified, as well as the underlying _intra-domain_ business logic and corresponding data services. 

The `AgentTester` has a `Test` method to simplify the construction enabling fluent-style method-chaining to _assert_ (expect) and execute a selected API operation; these _asserts_ are as follows:

Method | Description
-|-
`ExpectStatusCode` | Expect a response with the specified [`HttpStatusCode`](https://docs.microsoft.com/it-it/dotnet/api/system.net.httpstatuscode).
`ExpectErrorType` | Expect a response with the specified [`ErrorType`](../../src/Beef.Core/ErrorType.cs).
`ExpectMessages` | Expect a response with the specified messages.
`ExpectNullValue` | Expect `null` response value.
`ExpectValue` | Expect a response comparing the specified values (supports ignoring of specified properties).
`IgnoreChangeLog` | Ignores the [`IChangeLog`](../../src/Beef.Core/Entities/IChangeLog.cs) property.
`ExpectChangeLogCreated` | Expects the [`IChangeLog`](../../src/Beef.Core/Entities/IChangeLog.cs) property to be implemented for the response with generated values for the underlying `CreatedBy` and `CreatedDate` matching the specified values.
`ExpectChangeLogUpdated` | Expects the [`IChangeLog`](../../src/Beef.Core/Entities/IChangeLog.cs) property to be implemented for the response with generated values for the underlying `UpdatedBy` and `UpdatedDate` matching the specified values.
`IgnoreETag` | Ignores the [`IETag`](../../src/Beef.Core/Entities/IETag.cs) property.
`ExpectETag` | Expects the [`IETag`](../../src/Beef.Core/Entities/IETag.cs) to be implemented for the response with a generated value different to the previous value.
`ExpectUniqueKey` | Expects the [`IUniqueKey`](../../src/Beef.Core/Entities/IUniqueKey.cs) to be implemented for the response with a generated value.
`ExpectEvent` | Expects an event is published (in order specified). The expected event can use wildcards for `EventData.Subject` and optionally define         `EventData.Action`. An `EventData.Value` can be optionally specified including any corresponding members to igore for the comparison. Finally, the remaining `EventData` properties are not compared. Once an event is speficied then all expected events must be specified. 
`ExpectEventWithValue` | Same as `ExpectEvent` above defaulting the `EventData.Value` to the return value. 
`ExpectNoEvents` | Expects that *no* `Event` was published.

An example usage is as follows (see [`PersonTest`](../../samples/Demo/Beef.Demo.Test/PersonTest.cs) for more complete usage):

``` csharp
[Test, TestSetUp]
public void A140_Validation_ServiceAgentInvalid()
{
    AgentTester.Test<PersonAgent, Person>()
        .ExpectStatusCode(HttpStatusCode.BadRequest)
        .ExpectErrorType(ErrorType.ValidationError)
        .ExpectMessages(
            "First Name must not exceed 50 characters in length.",
            "Last Name must not exceed 50 characters in length.",
            "Gender is invalid.",
            "Eye Color is invalid.",
            "Birthday must be less than or equal to Today.")
        .Run(a => a.UpdateAsync(new Person() { FirstName = 'x'.ToLongString(), LastName = 'x'.ToLongString(), Birthday = DateTime.Now.AddDays(1), Gender = "X", EyeColor = "Y" }, 1.ToGuid()));
}
```

Another example usage is as follows (see [`RobotTest`](../../samples/Demo/Beef.Demo.Test/RobotTest.cs) for more complete usage):

``` csharp
AgentTester.Test<RobotAgent, Robot>()
    .ExpectStatusCode(HttpStatusCode.OK)
    .ExpectChangeLogUpdated()
    .ExpectETag(v.ETag)
    .ExpectUniqueKey()
    .ExpectEventWithValue("Demo.Robot.*", "Update")
    .ExpectValue((t) => v)
    .Run(a => a.UpdateAsync(v, 1.ToGuid()));
```

<br/>

## Mocking

As stated earlier, the likes of any cross domain (_inter-domain_) dependencies should be mocked out. Or any other layer within the _Beef_ to support a specific testing use case. To support this, both dependency injection (DI) and the [Moq](https://github.com/moq/moq4) framework is leveraged (or alternatively, any mocking framework can be used if required).

``` csharp
// Create the mock object (not use of the ReturnsWebApiAgentResultAsync helper in this scenario)
Mock<IPersonAgent> mock = new Mock<IPersonAgent>();
mock.Setup(x => x.GetAsync(1.ToGuid(), null)).ReturnsWebApiAgentResultAsync(new Person { LastName = "Mockulater" });

// Replace the existing scoped item with the mock object.
var svc = new Action<Microsoft.Extensions.DependencyInjection.IServiceCollection>(sc => sc.ReplaceScoped<IPersonAgent>(mock.Object));

// Use the alternative light-weight WebApplicationFactory (WAF) as the agent tester (optional).
using var agentTester = Beef.Test.NUnit.AgentTester.CreateWaf<Startup>(svc);
      
// Execute the test.      
agentTester.Test<PersonAgent, string>()
    .ExpectStatusCode(HttpStatusCode.OK)
    .ExpectValue(_ => "Mockulater")
    .Run(a => a.InvokeApiViaAgentAsync(1.ToGuid()));
```

</br>

## Validation testing

To simplify the testing of validations, and limit the need to have the backing data source (by mocking) to improve testing run time performance, the [`ValidatorTester`](./ValidationTester.cs) manages the testing of a validator outside of an API execution context with integrated mocking of services as required. The `ValidationTester` uses a similar _assert_ (expect) approach to enable with integrated service mocking.

The following is an example excerpt from [`EmployeeValidatorTest`](../../samples/My.Hr/My.Hr.Test/Validators/EmployeeValidatorTest.cs):

``` csharp
var eds = new Mock<IEmployeeDataSvc>();
eds.Setup(x => x.GetAsync(1.ToGuid())).ReturnsAsync(new Employee { Termination = new TerminationDetail { Date = DateTime.UtcNow } });

ValidationTester.Test()
    .OperationType(Beef.OperationType.Update)
    .AddScopedService(_referenceData)
    .AddScopedService(eds)
    .ExpectErrorType(Beef.ErrorType.ValidationError, "Once an Employee has been Terminated the data can no longer be updated.")
    .Run(() => EmployeeValidator.Default.Validate(e));
```

</br>

## Additional capabilities

The following additional capabilities have been added to further aid testing:

- [`ExpectException`](./ExpectException.cs) - Expects and asserts the specfied `Exception` type and its corresponding exception message.
- [`ExpectValidationException`](./ExpectValidationException.cs) - Expects and asserts a [`ValidationException`](../../src/Beef.Core/ValidationException.cs) and its corresponding messages.

The following extension methods have beed added to aid testing:
- [`int.ToGuid()`](./ExtensionMethods.cs) - Converts an `int` to a `Guid`. For example: `1.ToGuid()` will return `00000001-0000-0000-0000-000000000000`.
- [`char.ToLongString()`](./ExtensionMethods.cs) - Creates a long `string` by repeating the `char` for the specified count (defaults to 250). For example: `'x'.ToLongString()` will return `"xxxxx..."` (with 250 `x`'s).

<br/>

## User context

Within an API execution the user context should be defined to ensure that the likes of authentication and authorisation are performed for a request. This user context needs to be passed from the consumer (the test agent) to the service (API).

For _Beef_ the [`ExecutionContext`](../../src/Beef.Core/ExecutionContext.cs) houses the `Username` for the request; additional properties can be added as required. The `ExecutionContext` is used both within the consumer, as well as within the service processing. The same instance is not used (shared) between the two. The `ExecutionContext` is essentially internal to _Beef_ execution only.

User context is typically passed using the likes of [JWT](https://jwt.io/introduction/)s as an HTTP header on the request. The _Beef_ testing framework enables the opportunity for this to occur.

</br>

### Set user name

There are two opportunities to set the username for a test (specifically for the consumer):

- [`TestSetUpAttribute`](./TestSetUpAttribute.cs) - this has an overload in which the username is set for the test; behind the scenes this will set the `ExecutionContext` as the test starts.
- [`AgentTester`](./AgentTester.cs) - the `Test` method has an overload in which the username is overridden for the test; behind the scenes this will override the `ExecutionContext`.

Each of the above have overloads that take an `object userIdentifer` to support consts or enum values. The `AgentTester.UsernameConverter` function enables logic to be added to convert the identifier to a corresponding username. 

Where the username is not set it will default to the `AgentTester.DefaultUsername`. By default the value is `Anonymous`.

<br/>

### Sending the user context

There is nothing in _Beef_ that by default will send the user context for an API; this is the responsibility of the developer to implement as there is no standard approach.

To access each HTTP request before it is sent the `AgentTester.RegisterBeforeRequest` action should be set. This is passed the `HttpRequestMessage` which should be updated as required. The `ExecutionContext.Current.Username` should be used.

An example of sending the user as a header is as follows; this code should be replaced with a bearer token in the form of a JWT for example:

``` csharp
AgentTester.RegisterBeforeRequest(r => r.Headers.Add("cdr-user", Beef.ExecutionContext.Current.Username));
```
