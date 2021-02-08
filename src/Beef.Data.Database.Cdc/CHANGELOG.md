# Change log

Represents the **NuGet** versions.

## v4.1.2
- *Fixed:* Given recent changes to the `IEventPublisher` related to `Publish` and `Send` separation there was an issue where although the instance is scoped, it is being reused when executing multiple batches. The `IEventPublisher.Reset` was added after the `Send` invocation to correct.

## v4.1.1
- *New:* Initial publish to GitHub.
