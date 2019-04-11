# Data (Data access)

The _Data_ is primary responsible for performing the persistence request (Create, Read, Update or Delete) handling the wire transport, protocols and payload to enable. 

There are _Beef_ capabilities to encapsulate the processing for the following in a largely consistent manner:
- Database - `Beef.Data.Database`
- EntityFramework - `Beef.Data.EntityFrameworkCore`
- OData - `Beef.Data.OData`

<br>

## Usage
 
This layer is generally code-generated and provides options to provide a fully custom implementation, or has extension opportunities to inject additional logic into the processing pipeline.

The processing pipeline generally consists of:
- Scope a `Transaction` where required.
- Set the operation arguments, e.g. stored procedure name, paging.
- `OnBefore` extension opportunity.
- `OnQuery` extension opportunity for query operations.
- Invoke operation (using _Beef_ capabilities) or custom implementation.
- `OnAfter` extension opportunity.
- Exception handling
