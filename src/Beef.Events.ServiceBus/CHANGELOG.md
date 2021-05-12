# Change log

Represents the **NuGet** versions.

## v4.1.4
- *Enhancement:* Added Topic/Subscription support in addition to previous Queue support.
- *Enhancement:* Updated project to produce symbol packages for improved [debugging](https://devblogs.microsoft.com/dotnet/improving-debug-time-productivity-with-source-link/).

## v4.1.3
- *Fixed:* The `AddBeefEventServiceBusSender` extension method was missing the `removeKeyFromSubject` parameter from the auto-infer queue name overload.
- *Enhancement:* Added `ServiceBusReceiverHost.CreateServiceBusData` that will create using configuration in a similar manner as the `ServiceBusTriggerAttribute` resulting in the correct service bus namespace and queue name.

## v4.1.2
- *Enhancement:* Support new `IEventDataContentSerializer` and `IEventDataConverter`.
- *Enhancement:* Added `AzureServiceBusMessageConverter` and `MicrosoftServiceBusMessageConverter` for their respective, different, SDK versions.

## v4.1.1
- *New:* Initial publish to GitHub.
