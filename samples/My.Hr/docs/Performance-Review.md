# Step 6 - Employee Performance Review

This will walk through the process of creating and testing the employee performance review capability.

<br/>

## Functional requirement

In simple terms an employee performance review can occur at any time during an employee's employment. It captures the basic information, when, the reviewer, the outcome and related notes.

<br/>

## Data repository

A new `PerformanceReview` table and related Entity Framework model will be required to support.

<br/>

### Create Performance Review table

First step, as was previously the case, is to create the `PerformanceReview` table migration script. Execute the following to create the migration script for the `My.Hr.Database` project.

```
dotnet run scriptnew create Hr PerformanceReview
```

Open the newly created migration script and replace its contents with the following.

``` sql
-- Create table: Hr.PerformanceReview

BEGIN TRANSACTION

CREATE TABLE [Hr].[PerformanceReview] (
  [PerformanceReviewId] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,
  [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
  [Date] DATETIME2 NULL,
  [PerformanceOutcomeCode] NVARCHAR(50) NULL,
  [Reviewer] NVARCHAR(100) NULL,
  [Notes] NVARCHAR(4000) NULL,
  [RowVersion] TIMESTAMP NOT NULL,
  [CreatedBy] NVARCHAR(250) NULL,
  [CreatedDate] DATETIME2 NULL,
  [UpdatedBy] NVARCHAR(250) NULL,
  [UpdatedDate] DATETIME2 NULL
);
	
COMMIT TRANSACTION
```

<br/>

### Create Reference Data table

To support the above a `Hr.PerformanceOutcome` table is also required. Execute the following to create the migration script. No further changes will be needed.

```
dotnet run scriptnew createref Hr PerformanceOutcome
```

<br/>

### Reference Data data

Now that the Reference Data table exists it will need to be populated. Append the following YAML to the `RefData.yaml` file.

``` yaml
  - $PerformanceOutcome:
    - DN: Does not meet expectations
    - ME: Meets expectations
    - EE: Exceeds expectations
```

<br/>

### Entity Framework model

For the performance review Entity Framework will be used exclusively to support the full CRUD capabilities. Append the following to the end of `database.beef.yaml`.

``` yaml
  # PerformanceReview table and related referenace data.
- { name: PerformanceReview, efModel: true }
- { name: PerformanceOutcome, efModel: true }
```

<br/>

### Cascading-style delete

Without an explict cascading-style delete implemented using a TSQL constraint an explicit delete for the performance reviews is required when deleting an employee.

To accomplish, replace the `Delete` stored procedure configuration for the Employee table as follows.

``` yaml
      { name: Delete, type: Delete,
        execute: [
          { statement: 'DELETE FROM [Hr].[EmergencyContact] WHERE [EmployeeId] = @EmployeeId' },
          { statement: 'DELETE FROM [Hr].[PerformanceReview] WHERE [EmployeeId] = @EmployeeId' }
        ]
      }
``` 

<br/>

### Database management

Once the configuration has been completed then the database can be created/updated, the code-generation performed, and the corresponding reference data loaded into the corresponding tables.

At the command line execute the following command to perform. The log output will describe all actions that were performed.

```
dotnet run all
```

<br/>

## Reference Data API

The reference data API for the `PerformanceOutcome` needs to be added. Append the following to the end of `refdata.beef.yaml` within the `My.Hr.CodeGen` project.

``` yaml
- { name: PerformanceOutcome, refDataType: Guid, collection: true, webApiRoutePrefix: api/v1/ref/performanceOutcomes, autoImplement: EntityFramework, entityFrameworkModel: EfModel.PerformanceOutcome }
```

Once the reference data has been configured the code-generation can be performed. Use the following command line to generate.

```
dotnet run refdata
```

<br/>

## Performance Review API

To implement the core `PerformanceReview` operations the following will need to be performed:
- Code configuration
- Data access logic
- Validation

<br/>

### Code generation

The `PerformanceReview` entity and operations configuration is required as follows, append to the end of `entity.beef.yaml` (`My.Hr.CodeGen` project). Given that we are updating a single table row within the database, all of the operations will be able to be automatically implemented (generated) limiting the amount of code that needs to be added.

``` yaml
  # Creating a PerformanceReview entity
  # - Collection and CollectionResult required by GetByEmployeeId operation.
  # - WebApiRoutPrefix does not include entity as this will differ per operation (where more explicitly stated).
  # - Default is to AutoImplement using EntityFramework against the EfModel.PerformanceReview generated from the database.
  # - EmployeeId is made immutable within mapper by specifing DataOperationTypes as AnyExceptUpdate; i.e. the value will never map (override) on an update.
- { name: PerformanceReview, collection: true, collectionResult: true, validator: PerformanceReviewValidator, webApiRoutePrefix: api/v1, autoImplement: EntityFramework, entityFrameworkModel: EfModel.PerformanceReview,
    properties: [
      { name: Id, type: Guid, text: '{{Employee}} identifier', uniqueKey: true, dataName: PerformanceReviewId, dataAutoGenerated: true },
      { name: EmployeeId, text: '{{Employee.Id}} (value is immutable)', type: Guid, dataOperationTypes: AnyExceptUpdate },
      { name: Date, type: DateTime },
      { name: Outcome, type: ^PerformanceOutcome, dataName: PerformanceOutcomeCode },
      { name: Reviewer },
      { name: Notes },
      { name: ETag },
      { name: ChangeLog, type: ChangeLog, isEntity: true }
    ],
    operations: [
      # Operations
      # Get - this is a simple Get by unique key (being the Id) which will be automatically implemented.
      # GetByEmployeeId - this requires the EmployeeId to be passed in via the URI which is filtered within the developer extension.
      # Create - this requires the EmployeeId to be passed in via the URI which will override the value in the entity within the Manager layer (as defined by LayerPassing value of ToManagerSet).
      # Update/Patch/Delete - are all automatically implemented as they all simply follow the standard pattern.
      { name: Get, type: Get, uniqueKey: true, webApiRoute: 'reviews/{id}' },
      { name: GetByEmployeeId, type: GetColl, paging: true, webApiRoute: 'employees/{employeeId}/reviews',
        parameters: [
          { name: EmployeeId, text: '{{Employee.Id}}', type: Guid }
        ]
      },
      { name: Create, type: Create, webApiRoute: 'employees/{employeeId}/reviews',
        parameters: [
          { name: EmployeeId, text: '{{Employee.Id}}', type: Guid, layerPassing: ToManagerSet }
        ]
      },
      { name: Update, type: Update, uniqueKey: true, webApiRoute: 'reviews/{id}' },
      { name: Patch, type: Patch, uniqueKey: true, webApiRoute: 'reviews/{id}' },
      { name: Delete, type: Delete, uniqueKey: true, webApiRoute: 'reviews/{id}' }
    ]
  }
```

Once configured the code-generation can be performed. Use the following command line to generate.

```
dotnet run entity
```

<br/>

### Data access logic

The generated `PerformanceReviewData.cs` logic will need to be extended to support the filtering for the `GetByEmployeeId` operation. This logic must be implemented by the developer in a non-generated `partial` class. A new `PerformanceReviewData.cs` must be created within `My.Hr.Business/Data`.

To implement the filtering the extension delegate named `_getByEmployeeIdOnQuery` is implemented. The following represents the implementation. 

``` csharp
using System.Linq;

namespace My.Hr.Business.Data
{
    public partial class PerformanceReviewData
    {
        partial void PerformanceReviewDataCtor()
        {
            _getByEmployeeIdOnQuery = (q_, employeeId, _) => q_.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.Date);
        }
    }
}
```

<br/>

### Validation

Within the `My.Hr.Business/Validation` folder create `PerformanceReviewValidator.cs` and implement as follows. Of note, the `Context.ServiceProvider` is the means to get the required implementation for classes that are instantiated via dependency injection.

``` csharp
using Beef;
using Beef.Validation;
using My.Hr.Business.Entities;
using System.Threading.Tasks;

namespace My.Hr.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="PerformanceReview"/> validator.
    /// </summary>
    public class PerformanceReviewValidator : Validator<PerformanceReview>
    {
        private readonly IPerformanceReviewManager _performanceReviewManager;
        private readonly IEmployeeManager _employeeManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceReviewValidator"/> class.
        /// </summary>
        public PerformanceReviewValidator(IPerformanceReviewManager performanceReviewManager, IEmployeeManager employeeManager)
        {
            _performanceReviewManager = Check.NotNull(performanceReviewManager, nameof(performanceReviewManager));
            _employeeManager = Check.NotNull(employeeManager, nameof(employeeManager));

            Property(x => x.EmployeeId).Mandatory();
            Property(x => x.Date).Mandatory().CompareValue(CompareOperator.LessThanEqual, _ => ExecutionContext.Current.Timestamp, _ => "today");
            Property(x => x.Notes).String(4000);
            Property(x => x.Reviewer).Mandatory().String(256);
            Property(x => x.Outcome).Mandatory().IsValid();
        }

        /// <summary>
        /// Add further validation logic.
        /// </summary>
        protected override async Task OnValidateAsync(ValidationContext<PerformanceReview> context)
        {
            if (!context.HasError(x => x.EmployeeId))
            {
                // Ensure that the EmployeeId has not be changed (change back) as it is immutable.
                if (ExecutionContext.Current.OperationType == OperationType.Update)
                {
                    var prv = await _performanceReviewManager.GetAsync(context.Value.Id).ConfigureAwait(false);
                    if (prv == null)
                        throw new NotFoundException();

                    if (context.Value.EmployeeId != prv.EmployeeId)
                    {
                        context.AddError(x => x.EmployeeId, ValidatorStrings.ImmutableFormat);
                        return;
                    }
                }

                // Check that the referenced Employee exists, and the review data is within the bounds of their employment.
                var ev = await _employeeManager.GetAsync(context.Value.EmployeeId).ConfigureAwait(false);
                if (ev == null)
                    context.AddError(x => x.EmployeeId, ValidatorStrings.ExistsFormat);
                else
                {
                    if (!context.HasError(x => x.Date))
                    {
                        if (context.Value.Date < ev.StartDate)
                            context.AddError(x => x.Date, "{0} must not be prior to the Employee starting.");
                        else if (ev.Termination != null && context.Value.Date > ev.Termination.Date)
                            context.AddError(x => x.Date, "{0} must not be after the Employee has terminated.");
                    }
                }
            }
        }
    }
}
```

<br/>

## End-to-End testing

For the purposes of this sample, copy the contents of [`PerformanceReviewTest.cs`](../My.Hr.Test/Apis/PerformanceReviewTest.cs) (`My.Hr.Test/Apis` folder) and [`PerformanceReviewValidatorTest.cs`](../My.Hr.Test/Validators/PerformanceReviewValidatorTest.cs) (`My.Hr.Test/Validators` folder).

For the end-to-end testing to function performance review related data must first be populated into the database; append the following into the existing `Data.yaml` (`My.Hr.Test/Data`).

``` yaml
  - PerformanceReview:
    - { PerformanceReviewId: 1, EmployeeId: 2, Date: 2014-03-15, PerformanceOutcomeCode: DN, Reviewer: r.Browne@org.com, Notes: Work quality low. }
    - { PerformanceReviewId: 2, EmployeeId: 1, Date: 2016-11-12, PerformanceOutcomeCode: EE, Reviewer: r.Browne@org.com, Notes: They are awesome! }
    - { PerformanceReviewId: 3, EmployeeId: 2, Date: 2014-01-15, PerformanceOutcomeCode: DN, Reviewer: r.Browne@org.com, Notes: Work quality below standard. }
```

Review and execute the tests and ensure they all pass as expected.

<br/>

## Conclusion

At this stage we now have added and tested the performance review capabilities. All the desired functional requirements have now been implemented.

Next we will [wrap up](./../README.md#conclusion) the sample.