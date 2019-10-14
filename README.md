<br/>

![Logo](./docs/images/LogoPill.png "Beef")

<br/>

## Introduction

The _Beef_ framework, and the underlying code generation, has been primarily created to support the **industralisation** of API development.

> A means to have software developers focus directly on the **accelerated** delivery of **business value**; with consistently **higher quality** outcomes at an overall **lower cost**.

The key industralisation goals are:
1. **Value** - focus on business value, not on boilerplate
2. **Acceleration** – improve velocity; reduce costs and time to market
3. **Simplicity** – increase effective usage and minimise learning
4. **Standardised** – increase knowledgeable resource pool
5. **Consistency** – improve overall quality and maintainability
6. **Flexibility** – enable innovation and evolution easily over time

<br/>

## Architecture

_Beef_ has been developed to encourage the standardisation and industrialisation of the tiering and layering within the microservices (APIs) of an Application Architecture.

<br/>

### API-enabled domain-based channel-agnostic architecture 

The conceptual architecture is as follows; with _Beef_ being targeted specifically at implementation of the API tier.

![Domains](./docs/images/ApiArchitecture.png "Domain-based")

The key concepts are as follows:

- **Channel-agnostic** - the APIs are based around the key entities and the operations that can be performed on them: 
  - APIs represent the key trust boundary; as such, they make no assumptions on the consumer. The APIs will always validate the request data, and house the application’s functional business and orchestration rules.
  - APIs should not be developed to service a specific user interface interaction; as the APIs are agnostic to the consumer. The consumer has the responsibility of coordinating across API calls.
- **Domain-based** – the APIs are based around, and encapsulate, the capabilities for a functional domain:
  - Outcome of a [Domain-Driven Design](https://en.wikipedia.org/wiki/Domain-driven_design); divides capapabilities into different [Bounded Contexts](https://www.martinfowler.com/bliki/BoundedContext.html).
  - Encourages micro vs monolithic services.

<br/>

### Microservices

An architectural pattern for creating domain-based APIs:

- Is a software architecture style in which complex applications are composed of small, independent processes communicating with each other using language-agnostic APIs.
- These services are small, highly decoupled and focus on doing a small task, facilitating a modular approach to system-building.
- Implementation independence:
  - **Loose coupling** – should have its own persistence repository; data is duplicated (synchronised), not shared; eventual consistency; no distributed transactions.
  - **Polyglot persistence / programming** – use the best persistence repository to support the storage requirements; use a mix of programming languages (fit-for-purpose). Note: _Beef_ provides a C# / .NET implementation approach as one option.
  - **Eventual consistency** - for the most part, eventual consistency is good enough; real-time distributed transactional integrity is rarely required (although generally desired). An asynchronous messaging system, such as Queues or a Service Bus, can be leveraged to orchestrate cross domain data (eventual) consistency.

> “Micro” doesn’t imply number of lines of code; but a bounded concept / business capability within your Domain. - [http://herdingcode.com](http://herdingcode.com/herding-code-210-ian-cooper-on-microservices-and-the-brighter-library/)

<br/>

### Tiering and layering

The architecture supports a domain-based channel-agnostic microservices approach. The API service endpoints represent a light-weight facade for the Business (domain logic) tier, that is ultimately responsible for the fulfillment of the request. 

The following represents the prescribed tiering and layering of the architecture:

![Layers](./docs/images/Layers.png "Tiering and Layering")

Given this architecture, the .NET Solution you create using _Beef_ should adhere to the prescribed [solution structure](./docs/Solution-Structure.md).

Each of the key layers / components above are further detailed (`Xxx` denotes the entity name): 
- [Entity (DTO)](./docs/Layer-Entity.md) - `Xxx`
- [Service agent](./docs/Layer-ServiceAgent.md) - `XxxAgent` and `XxxServiceAgent`
- [Service interface](./docs/Layer-ServiceInterface.md) - `XxxController`
- [Domain logic](./docs/Layer-Manager.md) - `XxxManager`
- [Service orchestration](./docs/Layer-DataSvc.md) - `XxxDataSvc`
- [Data access](./docs/Layer-Data.md) - `XxxData`

<br/>

### Event-driven

To support the goals of an [Event-driven Architecture](https://en.wikipedia.org/wiki/Event-driven_architecture) _Beef_ enables the key capabilities; the publishing and subscribing of events (messages) to and from an event-stream (or equivalent).

![Layers](./docs/images/EventDrivenArchitecture.png "Event-Driven Architecture")

- **Publish** - the publishing of events is integrated into the API processing pipeline; this is enabled within the [Service orchestration](./docs/Layer-DataSvc.md) layer to ensure consistency of approach. _Beef_ is largely agnostic to the underlying event/messaging infrastructure (event-stream) and must be implemented by the developer.

- **Subscribe** - a event subscriber is then implemented to listen to events from the underlying event/messaging infrastructure (event-stream) and perform the related action. The event subscriber is encouraged to re-use the underlying logic by hosting the _Beef_ capabilities to implement. The [Domain logic](./docs/Layer-Manager.md) layer can be re-leveraged to perform the underlying business logic on the receipt of an event (within the context of a subscribing domain).

The _Beef_ support for an event-driven architecture is enabled by the [`Beef.Events`](./src/Beef.Events) assembly.

<br/>

## Framework 

A comprehensive **framework** has been created to support the defined architecture, to encapsulate and standardise capabilities, to achieve the desired code-generation outcomes and improve the overall developer experience.

Standardised approach, ensures consistency of implementation:
- Reduction in development effort.
- Higher quality of output; reduced defects.
- Greater confidence in adherence to architectural vision; minimised deviation.
- Generation and alike enables the solution to evolve more quickly and effectively over time. 

A key accelerator for _Beef_ is achieved using a flexible [code generation](./tools/Beef.CodeGen.Core/README.md) approach.

An extensive framework of capabilities has also been developed to support this entity-based development. Specifically around entities and their collections, entity mapping, reference data, validation, standardised exceptions, standardised messaging, basic caching, logging, flat-file reader/writer, RESTful API support, ADO.NET database access, Entity Framework (EF) data access, OData access, Azure Service Bus, long running (execution and triggers) processes, etc.

The **key** capabilities for _Beef_ are enabled by the following run-time assemblies:

Assembly | Description | NuGet | Changes
-|-|-|-
[`Beef.Core`](./src/Beef.Core) | Core foundational framework. | [![NuGet version](https://badge.fury.io/nu/Beef.core.svg)](https://badge.fury.io/nu/Beef.core) | [Log](./src/Beef.Core/CHANGELOG.md)
[`Beef.AspNetCore.WebApi`](./src/Beef.AspNetCore.WebApi) | ASP.NET Core Web API framework. | [![NuGet version](https://badge.fury.io/nu/Beef.AspNetCore.WebApi.svg)](https://badge.fury.io/nu/Beef.AspNetCore.WebApi) | [Log](./src/Beef.AspNetCore.WebApi/CHANGELOG.md)
[`Beef.Data.Database`](./src/Beef.Data.Database) | ADO.NET database framework. | [![NuGet version](https://badge.fury.io/nu/Beef.Data.Database.svg)](https://badge.fury.io/nu/Beef.Data.Database) | [Log](./src/Beef.Data.Database/CHANGELOG.md)
[`Beef.Data.EntityFrameworkCore`](./src/Beef.Data.EntityFrameworkCore) | Entity Framework (EF) Core framework. | [![NuGet version](https://badge.fury.io/nu/Beef.Data.EntityFrameworkCore.svg)](https://badge.fury.io/nu/Beef.Data.EntityFrameworkCore) | [Log](./src/Beef.Data.EntityFrameworkCore/CHANGELOG.md)
[`Beef.Data.Cosmos`](./src/Beef.Data.Cosmos) | Cosmos DB execution framework. | [![NuGet version](https://badge.fury.io/nu/Beef.Data.Cosmos.svg)](https://badge.fury.io/nu/Beef.Data.Cosmos) | [Log](./src/Beef.Data.Cosmos/CHANGELOG.md)
[`Beef.Data.OData`](./src/Beef.Data.OData) | OData execution framework. | [![NuGet version](https://badge.fury.io/nu/Beef.Data.OData.svg)](https://badge.fury.io/nu/Beef.Data.OData) | [Log](./src/Beef.Data.OData/CHANGELOG.md)
[`Beef.Events`](./src/Beef.Events) | Supporting Event-driven framework. | [![NuGet version](https://badge.fury.io/nu/Beef.Events.svg)](https://badge.fury.io/nu/Beef.Events) | [Log](./src/Beef.Events/CHANGELOG.md)

The tooling / supporting capabilities for _Beef_ are enabled by the following assemblies:

Assembly | Description | NuGet | Changes
-|-|-|-
[`Beef.CodeGen.Core`](./tools/Beef.CodeGen.Core) | Code generation console tool. | [![NuGet version](https://badge.fury.io/nu/Beef.CodeGen.Core.svg)](https://badge.fury.io/nu/Beef.CodeGen.Core) | [Log](./tools/Beef.CodeGen.Core/CHANGELOG.md)
[`Beef.Database.Core`](./tools/Beef.Database.Core) | Database and data management console tool. | [![NuGet version](https://badge.fury.io/nu/Beef.Database.Core.svg)](https://badge.fury.io/nu/Beef.Database.Core) | [Log](./tools/Beef.Database.Core/CHANGELOG.md)
[`Beef.Test.NUnit`](./tools/Beef.Test.NUnit) | Unit and intra-domain integration testing framework. | [![NuGet version](https://badge.fury.io/nu/Beef.Test.NUnit.svg)](https://badge.fury.io/nu/Beef.Test.NUnit) | [Log](./tools/Beef.Test.NUnit/CHANGELOG.md)
[`Beef.Template.Solution`](./templates/Beef.Template.Solution) | Solution and projects template. | [![NuGet version](https://badge.fury.io/nu/Beef.Template.Solution.svg)](https://badge.fury.io/nu/Beef.Template.Solution) | [Log](./templates/Beef.Template.Solution/CHANGELOG.md)

The following samples are provided to guide usage:

Sample | Description
-|-
[`Demo`](./samples/Demo) | A sample as an end-to-end solution to demonstrate the tiering & layering, code-generation, database management and automated intra-domain integration testing. This is also used to further test the key end-to-end capabilities enabled by _Beef_.

<br/>

## License

_Beef_ is open source under the [MIT license](./LICENSE) and is free for commercial use.

<br/>

## Getting started

To start using _Beef_ you do not need to clone; you just need to create a solution with the underlying projects using the prescribed [solution structure](./docs/Solution-Structure.md), including referencing the appropriate NuGet packages. To accelerate this a .NET Core [template capability](./templates/Beef.Template.Solution/README.md) is provided to enable you to get up and running in minutes.

<br/>

## Contributing
One of the easiest ways to contribute is to participate in discussions on GitHub issues. You can also contribute by submitting pull requests (PR) with code changes.

<br/>

### Coding guidelines

The most general guideline is that we use all the VS default settings in terms of code formatting; if it doubt, follow the coding convention of the existing code base.
1. Use four spaces of indentation (no tabs).
2. Use `_camelCase` for private fields.
3. Avoid `this.` unless absolutely necessary.
4. Always specify member visibility, even if it's the default (i.e. `private string _foo;` not `string _foo;`).
5. Open-braces (`{`) go on a new line (an `if` with single-line statement does not need braces).
6. Use any language features available to you (expression-bodied members, throw expressions, tuples, etc.) as long as they make for readable, manageable code.
7. All methods and properties must include the [XML documentation comments](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/xml-documentation-comments). Private methods and properties only need to specifiy the [summary](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/summary) as a minimum.

For further guidance see ASP.NET Core [Engineering guidelines](https://github.com/aspnet/AspNetCore/wiki/Engineering-guidelines).

<br/>

### Tests

We use [`NUnit`](https://github.com/nunit/nunit) for all unit testing.

- Tests need to be provided for every bug/feature that is completed.
- Tests only need to be present for issues that need to be verified by QA (for example, not tasks).
- If there is a scenario that is far too hard to test there does not need to be a test for it.
- "Too hard" is determined by the team as a whole.

We understand there is more work to be performed in generating a higher level of code coverage; this technical debt is on the backlog.

<br/>

### Code reviews and checkins
To help ensure that only the highest quality code makes its way into the project, please submit all your code changes to GitHub as PRs. This includes runtime code changes, unit test updates, and updates to the end-to-end demo.

For example, sending a PR for just an update to a unit test might seem like a waste of time but the unit tests are just as important as the product code and as such, reviewing changes to them is also just as important. This also helps create visibility for your changes so that others can observe what is going on.

The advantages are numerous: improving code quality, more visibility on changes and their potential impact, avoiding duplication of effort, and creating general awareness of progress being made in various areas.
