# Change log

Represents the **NuGet** versions.

## v2.1.5
- *Enhancement:* An invocation with an `If-Match` will override the value where it implements `IEtag` as this should take precedence over the value inside of the value itself via `WebApiActionBase.Value`. Code-gen has also been updated to take advantage of this; next gen will introduce usage within `XxxApiController` classes.

## v2.1.4
- *Fixed:* `WebApiControllerHelper.SetETag` will not format where already formatted. 

## v2.1.3
- *Fixed:* InvokerBase was non functioning as a generic class; reimplemented. Other Invokers updated accordingly.

## v2.1.2
- *Fixed:* Inconsistent version numbers.

## v2.1.1
- *New:* Initial publish to GitHub.