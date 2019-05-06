# Reference Data

The _Beef_ framework and runtime enable a rich, first class, support for Reference Data given its key role within an application.

<br/>

## Types of data

At a high-level a typical applications deals with different types of data:

- **Reference Data** is data that is managed within an application primarily used to provide lists of valid values. These values provide contextual information that are generally used to trigger business processes, workflows and/or used for grouping / filtering.
This data has a low level of volatility, in that it remains largely static for significant periods of time. There are low volumes of this data within an application. It is a very good candidate for the likes of caching.
Reference Data is generally never deleted; instead it may become inactive. 
Example: Country, Gender, Payment Type, etc. 

- **Master data** is data that is captured and continuously maintained to reflect a current known understanding; there is no historical context other than that provided by an audit process providing a version history over time.
This data has a moderate level of volatility, in that changes generally occur infrequently. There are moderate volumes of this data within an application.
Master data can be deleted (or logically deleted) as required; typically the latter. 
Example: Customer, Vendor, Product, GL Account, etc. 

- **Transactional data** is data that is recorded to capture/manage an event or action, tied to specific business rules, at a point in time. 
The data will typically have a high level of volatility at inception decreasing significantly over time. Once the corresponding workflow has completed the data becomes immutable and serves the purpose of providing a historical context.
Transactional data is generally never deleted as it provides an auditable recording. There are high volumes of this type of data within an application.
Example: Purchase Order, Sales Invoice, GL Posting, etc. 

<br/>

## Anatomy of reference data

The [`ReferenceDataBase`](../src/Beef.Core/RefData/ReferenceDataBase.cs):

Property | Description
-|-
`Id` | The internal unique identifier as either an `int` or a `Guid`.
`Code` | The external unique identifier as a `string`. This is the value that would be used by external parties (applications) to consume.
`Text` | The textual `string` used for the likes of drop-downs etc. within an application.
`SortOrder` | Defines the sort order within the underlying reference data collection.
`IsActive` | Indicates whether the value is active or not.

Additional properties exist that can be leveraged:




Additional properties can be, and should be, extended where required. The Reference Data framework will then make these available within the application to enable simple usage by a developer.
