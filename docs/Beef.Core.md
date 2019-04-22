# `Beef.Core`

This is the foundational assembly that provides the core capabilities of _Beef_. It is composed of the following key features:

Namespace | Description
-|-
~ | Core or common capabilities, such as `ExecutionContext`, `IBusinessException`, `Factory`, and `DataContextScope`.
`Business` | _Business_ tier components; specifically the invokers (see `BusinessInvokerBase`). 
`Caching` | In-memory cache capabilities with associated policies to periodically flush.
`CodeGen` | Core _Code Generator_ capabilities used by all tooling.
`Diagnostics` | Basic diagnostics such as the shared `Logger` and `PerformanceTimer`.
`Entities` | Provides the key capabilities to enable the rich _business entity_ functionality central to _Beef_.
`Events` | Provides basic infrastucture to support a basic _event-driven_ architecture, through `Event` and `EventData`. 
`Executors` | Execution, and corresponding trigger orchestration, to standardise the processing of long-running, batch-style, operations.
`FlatFile` | Provides a rich framework for reading and writing fixed, and delimited, flat files.
`Json` | Additional capabilities to process JSON, such as `JsonEntityMerge` and `JsonPropertyFilter`.
`Mapper` | Provides the base, and entity-to-entity, class and property mapping central to _Beef_.
`Net` | Additional `HTTP` capabilities.
`RefData` | Provides the key capabilities to enable the rich _business reference data_ functionality central to _Beef_.
`Reflection` | Additional reflection capabilities leveraged primarily by the _Beef_ framework.
`Strings` | Embedded string resources used by _Beef_.
`Validation` | Provides a rich, fluent-style, validation framework.
`WebApi` | Provides additional capabilities to standardize the consumption of Web APIs.