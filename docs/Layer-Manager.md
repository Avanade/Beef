# Manager (Domain Logic)

The *Manager* is primary responsible for hosting the key business and/or workflow logic. This is also where the primary validation is performed to ensure the consistency of the request before any further processing is performed.

<br>

## Usage
 
This layer is generally code-generated and provides options to provide a fully custom implementation, or has extension opportunities to inject additional logic into the processing pipeline.

The processing pipeline generally consists of:
- `ExecutionContext.OperationType` set to specify the type of operation being performed (`Create`, `Read`, `Update` or `Delete`) so other functions down the call stack can infer operation intent.
- Entity `CleanUp` is the process of reviewing and updating the entity properties to make sure it is in a logical / consistent state.
- `PreValidate` extension opportunity.
- *Validation* is performed to ensure data consistency before processing.
- `OnBefore` extension opportunity.
- Scope a `Transaction` where required.
- [`{Entity}DataSvc`](./Layer-DataSvc.md) is invoked to orchestrate the data processing.
- `OnAfter` extension opportunity.
- Entity `CleanUp` of response before returning.

The [`Operation`](./Entity-Operation-element.md) element within the `entity.xml` configuration primarily drives the ouput. There is a generated class per [`Entity`](./Entity-Entity-element.md) named `{Entity}Manager`.

There is also a corresonding interface named `I{Entity}Manager` generated so the likes of test mocking etc. can be leveraged.