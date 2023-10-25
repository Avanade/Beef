# Step 6 - Employee Performance Review

This will walk through the process of creating and testing the employee performance review capability.

<br/>

## Functional requirement

In simple terms an employee performance review can occur at any time during an employee's employment. It captures the basic information, when, the reviewer, the outcome and related notes.

<br/>

## Data repository

A new `PerformanceReview` table and related Entity Framework (EF) model will be required to support.

<br/>

### Create Performance Review table

First step, as was previously the case, is to create the `PerformanceReview` table migration script. Execute the following to create the migration script for the `MyEf.Hr.Database` project.

```
dotnet run script create Hr PerformanceReview
```

Open the newly created migration script and replace its contents with the following.

``` sql
-- Create table: [Hr].[PerformanceReview]

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

To support the above a `Hr.PerformanceOutcome` reference data table is also required. Execute the following to create the migration script. No further changes will be needed.

```
dotnet run script refdata Hr PerformanceOutcome
```

<br/>

### Reference Data data

Now that the Reference Data table exists it will need to be populated. Append the following YAML to the existing `RefData.yaml` file.

``` yaml
  - $PerformanceOutcome:
    - DN: Does not meet expectations
    - ME: Meets expectations
    - EE: Exceeds expectations
```

<br/>

### Entity Framework model

For the performance review, Entity Framework will be used exclusively to support the full CRUD capabilities. Append the following after the other reference data tables within the `database.beef-5.yaml`.

``` yaml
- name: PerformanceOutcome
```

Add the additional relationship to the `Employee` table by replacing existing configuration.

``` yaml
  # References the Employee and related tables to implement the EF Model and infer the underlying schema.
- name: Employee
  relationships: [
    # Relationships can be code-generated (basic functionality), or handcrafted in the .NET code using the standard EntityFramework capabilities.
    # - One-to-many to EmergencyContacts table foreign key using EmployeeId column. Cascade the delete. Auto include collection on get and track for updates.
    { name: EmergencyContact, propertyName: EmergencyContacts, foreignKeyColumns: [ EmployeeId ], onDelete: ClientCascade, autoInclude: true },
    # - One-to-many to PerformanceReview table foreign key using EmployeeId column. Cascade the delete. Do _not_ auto include collection on get and track for updates (default).
    { name: PerformanceReview, propertyName: PerformanceReviews, foreignKeyColumns: [ EmployeeId ], onDelete: ClientCascade }
  ]
```

Finally add the `PerformanceReview` table to the end of the file to create the related Entity Framework model.

```
- name: PerformanceReview
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

The reference data API for the `PerformanceOutcome` needs to be added. Append the following to the end of `refdata.beef.yaml` within the `MyEf.Hr.CodeGen` project.

``` yaml
- { name: PerformanceOutcome, entityFrameworkModel: EfModel.PerformanceOutcome }
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

The `PerformanceReview` entity and operations configuration is required as follows, append to the end of `entity.beef-5.yaml` (`MyEf.Hr.CodeGen` project). Given that we are updating a single table row within the database, all of the operations will be able to be automatically implemented (generated) limiting the amount of code that needs to be added.

``` yaml
  # Creating a PerformanceReview entity
  # - Collection and CollectionResult required by GetByEmployeeId operation.
  # - WebApiRoutePrefix is explicitly specified overridding the default.
  # - Behavior specifies that a Get, Update, Patch and Delete operation should be implemeted following the standard pattern.
  # - EntityFrameworkModel specifies the corresponding model and will be auto-implemented (defaults from configuration hierarchy).
  # - EmployeeId is made immutable within mapper by specifing DataOperationTypes as AnyExceptUpdate; i.e. the value will never map (override) on an update.
- { name: PerformanceReview, collection: true, collectionResult: true, webApiRoutePrefix: reviews, behavior: gupd, validator: PerformanceReviewValidator, entityFrameworkModel: EfModel.PerformanceReview,
    properties: [
      { name: Id, type: Guid, text: '{{PerformanceReview}} identifier', primaryKey: true, dataName: PerformanceReviewId, dataAutoGenerated: true },
      { name: EmployeeId, text: '{{Employee.Id}} (value is immutable)', type: Guid, dataOperationTypes: AnyExceptUpdate },
      { name: Date, type: DateTime },
      { name: Outcome, type: ^PerformanceOutcome, dataName: PerformanceOutcomeCode },
      { name: Reviewer },
      { name: Notes },
      { name: ETag },
      { name: ChangeLog, type: ChangeLog }
    ],
    operations: [
      # Operations (in addition to the Get, Update, Patch and Delete).
      # GetByEmployeeId - this requires the EmployeeId to be passed in via the URI which is filtered within the developer extension.
      # Create - this requires the EmployeeId to be passed in via the URI which will override the value in the entity within the Manager layer (as defined by LayerPassing value of ToManagerSet).
      # The WebApiRoute with a ! prefix informs code-gen to use as-is versus prepending with the WebApiRoutePrefix; otherwise, would incorrectly result in 'reviews/employees/{employeeId}'.
      { name: GetByEmployeeId, type: GetColl, paging: true, webApiRoute: '!employees/{employeeId}/reviews',
        parameters: [
          { name: EmployeeId, text: '{{Employee}} identifier', type: Guid, isMandatory: true }
        ]
      },
      { name: Create, type: Create, webApiRoute: '!employees/{employeeId}/reviews',
        parameters: [
          { name: EmployeeId, text: '{{Employee}} identifier', type: Guid, layerPassing: ToManagerSet, isMandatory: true }
        ]
      }
    ]
  }
```

Once configured the code-generation can be performed. Use the following command line to generate.

```
dotnet run entity
```

<br/>

### Data access logic

The generated `PerformanceReviewData.cs` logic will need to be extended to support the filtering for the `GetByEmployeeId` operation. This logic must be implemented by the developer in a non-generated `partial` class. A new `PerformanceReviewData.cs` must be created within `MyEf.Hr.Business/Data`.

To implement the filtering the extension delegate named `_getByEmployeeIdOnQuery` is implemented. The following represents the implementation. 

``` csharp
namespace MyEf.Hr.Business.Data;

public partial class PerformanceReviewData
{
    partial void PerformanceReviewDataCtor()
    {
        _getByEmployeeIdOnQuery = (q, employeeId) => q.Where(x => x.EmployeeId == employeeId).OrderByDescending(x => x.Date);
    }
}
```

<br/>

### Validation

Within the `MyEf.Hr.Business/Validation` folder create `PerformanceReviewValidator.cs` and implement as follows.

``` csharp
namespace MyEf.Hr.Business.Validation;

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
        _performanceReviewManager = performanceReviewManager ?? throw new ArgumentNullException(nameof(performanceReviewManager));
        _employeeManager = employeeManager ?? throw new ArgumentNullException(nameof(employeeManager));

        Property(x => x.EmployeeId).Mandatory();
        Property(x => x.Date).Mandatory().CompareValue(CompareOperator.LessThanEqual, _ => CoreEx.ExecutionContext.Current.Timestamp, _ => "today");
        Property(x => x.Notes).String(4000);
        Property(x => x.Reviewer).Mandatory().String(256);
        Property(x => x.Outcome).Mandatory().IsValid();
    }

    /// <summary>
    /// Add further validation logic.
    /// </summary>
    protected override async Task<Result> OnValidateAsync(ValidationContext<PerformanceReview> context, CancellationToken cancellationToken)
    {
        // Exit where the EmployeeId is not valid.
        if (context.HasError(x => x.EmployeeId))
            return Result.Success;

        // Ensure that the EmployeeId, on Update, has not been changed as it is immutable.
        if (ExecutionContext.OperationType == OperationType.Update)
        {
            var prr = await Result.GoAsync(_performanceReviewManager.GetAsync(context.Value.Id))
                                  .When(v => v is null, _ => Result.NotFoundError()).ConfigureAwait(false);

            if (prr.IsFailure)
                return prr.AsResult();

            if (context.Value.EmployeeId != prr.Value.EmployeeId)
                return Result.Done(() => context.AddError(x => x.EmployeeId, ValidatorStrings.ImmutableFormat));
        }

        // Check that the referenced Employee exists, and the review data is within the bounds of their employment.
        return await Result.GoAsync(_employeeManager.GetAsync(context.Value.EmployeeId)).Then(e =>
        {
            if (e == null)
                context.AddError(x => x.EmployeeId, ValidatorStrings.ExistsFormat);
            else
            {
                if (!context.HasError(x => x.Date))
                {
                    if (context.Value.Date < e.StartDate)
                        context.AddError(x => x.Date, "{0} must not be prior to the Employee starting.");
                    else if (e.Termination != null && context.Value.Date > e.Termination.Date)
                        context.AddError(x => x.Date, "{0} must not be after the Employee has terminated.");
                }
            }
        }).AsResult().ConfigureAwait(false);
    }
}
```

<br/>

## End-to-End testing

For the purposes of this sample, copy the contents of [`PerformanceReviewTest.cs`](../MyEf.Hr.Test/Apis/PerformanceReviewTest.cs) (`MyEf.Hr.Test/Apis` folder) and [`PerformanceReviewValidatorTest.cs`](../MyEf.Hr.Test/Validators/PerformanceReviewValidatorTest.cs) (`MyEf.Hr.Test/Validators` folder).

For the end-to-end testing to function the performance review related data must first be populated into the database; append the following into the existing `Data.yaml` (`MyEf.Hr.Test/Data`).

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

Next we will consider the addition of [Event-driven architecture](./../README.md#Event-driven-architecture-implementation) capabilities.
