# Beef.Test.NUnit

Under construction :-|

## User context

Within an API execution the user context should be defined to ensure that the likes of authentication and authorisation are performed for a request. This user context needs to be passed from the consumer (the test agent) to the service (API).

For _Beef_ the [`ExecutionContext`](../../src/Beef.Core/ExecutionContext.cs) houses the `Username` for the request; additional properties can be added as required. The `ExecutionContext` is used both within the consumer, as well as within the service processing. The same instance is not used (shared) between the two. The `ExecutionContext` is essentially internal to _Beef_ execution only.

User context is typically passed using the likes of [JWT](https://jwt.io/introduction/)s as an HTTP header on the request. The _Beef_ testing framework enables the opportunity for this to occur.

</br>

### Set user name

There are two opportunities to set the user name for a test (specifically for the consumer):

- [`TestSetUpAttribute`](./TestSetUpAttribute.cs) - this has an overload in which the user name is set for the test; behind the scenes this will set the `ExecutionContext` as the test starts.
- [`AgentTester`](./AgentTester.cs) - the `Create` method has an overload in which the user name is overridden for the test; behind the scenes this will override the `ExecutionContext`.

Where the user name is not set it will default to the `AgentTester.DefaultUsername`.

<br/>

### Sending the user context

There is nothin in _Beef_ that by default will send the user context; this is the responsibility of the developer to implement as there is no standard approach.

To access each HTTP request before it is sent the `AgentTester.RegisterBeforeRequest` action should be set. This is passed the `HttpRequestMessage` which should be updated as required. The user name is set from above should be accessed via `ExecutionContext.Username`.
