# DataSvc (Service Orchestration)

The _DataSvc_ is primary responsible for orchestrating the underlying data access; whilst often one-to-one there may be times that this class will be used to coordinate multiple data access components. It is responsible for the ensuring that the related `Entity` is fully constructed/updated/etc. as per the desired operation.

<br>

## Usage
 
This layer is generally code-generated and provides options to provide a fully custom implementation, or has extension opportunities to inject additional logic into the processing pipeline.

The processing pipeline generally consists of:
- Scope a `Transaction` where required.
- Get/set the `ExecutionContext` request _cache_ to improve in-process get request performance (reduce chattiness to backends whilst processing a single request).
- `{Entity}Data` is invoked to perform the data processing. An `I{Entity}Data` is constructed by a Factory to enable mocking for the likes of testing.
- `Event.PublishAsync` is performed for Create/Update/Delete operations to enable event-driven messaging in a consistent manner.

The [`Operation`](./Entity-Operation-element.md) element within the `entity.xml` configuration primarily drives the ouput. There is a generated class per [`Entity`](./Entity-Entity-element.md) named `{Entity}DatSvc`.
