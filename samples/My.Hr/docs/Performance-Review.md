# Step 6 - Performance Review

This will walk through the process of creating and testing the employee performance review capability.

<br/>

## Functional requirement

The employee performance review can occur at any time during an employee's employment. 

<br/>

## Data repository

A new `PerformanceReview` table and related Entity Framework model will be required to support.

<br/>

### Create Performance Review table

First step, as was previously the case, is to create the `PerformanceReview` table migration script. Execute the following to create the migration script.

```
dotnet run scriptnew -create Hr.PerformanceReview
```

Open the newly created migration script and replace its contents with the following.

``` sql
-- Migration Script

BEGIN TRANSACTION

CREATE TABLE [Hr].[PerformanceReview] (
  [PerformanceReviewId] UNIQUEIDENTIFIER NOT NULL DEFAULT (NEWSEQUENTIALID()) PRIMARY KEY,
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

To support the above a `Ref.PerformanceOutcome` tabke is also required.

Execute the following to create the migration script. No further changes will be needed.

```
dotnet run scriptnew -create Ref.PerformanceOutcome
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

For the performance review Entity Framework will be used exclusively to support the full CRUD capabilities. Append the following to the end of `My.Hr.Database.xml`.

``` xml
  <!-- PerformanceReview table and related referenace data. -->
  <Table Name="PerformanceReview" Schema="Hr" EfModel="true" />
  <Table Name="PerformanceOutcome" Schema="Ref" EfModel="true" />
```

<br/>

### Cascading-style delete

Without an explict cascading-style delete implemented using a TSQL constraint an explicit delete for the performance reviews is required when deleting an employee.

To accomplish, replace the `Delete` stored procedure configuration as follows.

``` xml
  <Table Name="Employee" Schema="Hr">
  ...
    <StoredProcedure Name="Delete" Type="Delete">
      <Execute Statement="DELETE FROM [Hr].[EmergencyContact] WHERE [EmployeeId] = @EmployeeId" />
      <Execute Statement="DELETE FROM [Hr].[PerformanceReview] WHERE [EmployeeId] = @EmployeeId" />
    </StoredProcedure>
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

The reference data API for the `PerformanceOutcome` needs to be added. Append the following to the end of `My.RefData.xml` within the `My.Hr.CodeGen` project.

``` xml
  <Entity Name="PerformanceOutcome" RefDataType="Guid" Collection="true" WebApiRoutePrefix="api/v1/ref/performanceOutcomes" AutoImplement="EntityFramework" EntityFrameworkEntity="EfModel.PerformanceOutcome" />
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

The `PerformanceReview` entity and operations configuration is required as follows, append to the end of `My.Hr.xml` (`My.Hr.CodeGen` project). Given that we are updating a single table row within the database, all of the operations will be able to be automatically implemented (generated) limiting the amount of code that needs to be added.

``` xml
  <!-- Creating a PerformanceReview entity
       - Collection and CollectionResult required by GetByEmployeeId operation. 
       - WebApiRoutPrefix does not include entity as this will differ per operation (where more explicitly stated). 
       - Default is to AutoImplement using EntityFramework against the EfModel.PerformanceReview generated from the database.
       - EmployeeId is made immutable within mapper by specifing DataOperationTypes as AnyExceptUpdate; i.e. the value will never map (override) on an update. -->
  <Entity Name="PerformanceReview" Collection="true" CollectionResult="true" Validator="PerformanceReviewValidator" WebApiRoutePrefix="api/v1" AutoImplement="EntityFramework" EntityFrameworkEntity="EfModel.PerformanceReview">
    <Property Name="Id" Type="Guid" Text="{{Employee}} identifier" UniqueKey="true" DataName="PerformanceReviewId" DataAutoGenerated="true" />
    <Property Name="EmployeeId" Text="{{Employee.Id}} (value is immutable)" Type="Guid" DataOperationTypes="AnyExceptUpdate" />
    <Property Name="Date" Type="DateTime" />
    <Property Name="Outcome" Type="RefDataNamespace.PerformanceOutcome" DataName="PerformanceOutcomeCode" />
    <Property Name="Reviewer" Type="string" />
    <Property Name="Notes" Type="string" />
    <Property Name="ETag" ArgumentName="etag" Type="string" />
    <Property Name="ChangeLog" Type="ChangeLog" IsEntity="true" />
    
    <!-- Operations 
         Get - this is a simple Get by unique key (being the Id) which will be automatically implemented.
         GetByEmployeeId - this requires the EmployeeId to be passed in via the URI which is filtered within the developer extension.
         Create - this requires the EmployeeId to be passed in via the URI which will override the value in the entity within the Manager layer (as defined by LayerPassing value of ToManagerSet).
         Update/Patch/Delete - are all automatically implemented as they all simply follow the standard pattern. -->
    <Operation Name="Get" OperationType="Get" UniqueKey="true" WebApiRoute="reviews/{id}" />
    <Operation Name="GetByEmployeeId" OperationType="GetColl" PagingArgs="true" WebApiRoute="employees/{employeeId}/reviews">
      <Parameter Name="EmployeeId" Text="{{Employee.Id}}" Type="Guid" />
    </Operation>
    <Operation Name="Create" OperationType="Create" WebApiRoute="employees/{employeeId}/reviews">
      <Parameter Name="EmployeeId" Text="{{Employee.Id}}" Type="Guid" LayerPassing="ToManagerSet" />
    </Operation>
    <Operation Name="Update" OperationType="Update" UniqueKey="true" WebApiRoute="reviews/{id}" />
    <Operation Name="Patch" OperationType="Patch" UniqueKey="true" WebApiRoute="reviews/{id}" />
    <Operation Name="Delete" OperationType="Delete" UniqueKey="true" WebApiRoute="reviews/{id}" />
  </Entity>
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
using Beef.Entities;
using Beef.Validation;
using My.Hr.Common.Entities;
using System;

namespace My.Hr.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="PerformanceReview"/> validator.
    /// </summary>
    public class PerformanceReviewValidator : Validator<PerformanceReview, PerformanceReviewValidator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceReviewValidator"/> class.
        /// </summary>
        public PerformanceReviewValidator()
        {
            Property(x => x.EmployeeId).Mandatory();
            Property(x => x.Date).Mandatory().CompareValue(CompareOperator.LessThanEqual, _ => Cleaner.Clean(DateTime.Now), _ => "today");
            Property(x => x.Notes).String(4000);
            Property(x => x.Reviewer).Mandatory().String(256);
            Property(x => x.Outcome).Mandatory().IsValid();
        }

        /// <summary>
        /// Add further validation logic.
        /// </summary>
        protected override void OnValidate(ValidationContext<PerformanceReview> context)
        {
            if (!context.HasError(x => x.EmployeeId))
            {
                // Ensure that the EmployeeId has not be changed (change back) as it is immutable.
                if (ExecutionContext.Current.OperationType == OperationType.Update)
                {
                    var prm = (IPerformanceReviewManager)context.ServiceProvider.GetService(typeof(IPerformanceReviewManager));
                    var prv = prm.GetAsync(context.Value.Id).GetAwaiter().GetResult();
                    if (prv == null)
                        throw new NotFoundException();

                    if (context.Value.EmployeeId != prv.EmployeeId)
                    {
                        context.AddError(x => x.EmployeeId, ValidatorStrings.ImmutableFormat);
                        return;
                    }
                }

                // Check that the referenced Employee exists, and the review data is within the bounds of their employment.
                var em = (IEmployeeManager)context.ServiceProvider.GetService(typeof(IEmployeeManager));
                var ev = em.GetAsync(context.Value.EmployeeId).GetAwaiter().GetResult();
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

For the purposes of this sample, copy the contents of [`PerformanceReviewTest.cs`](../My.Hr.Test/PerformanceReviewTest.cs) (`My.Hr.Test` root folder) and [`PerformanceReviewValidatorTest.cs`](../My.Hr.Test/Validators)PerformanceReviewValidatorTest.cs) (`My.Hr.Test/Validators` folder).

Review and execute the tests and ensure they all pass as expected.

<br/>

## Conclusion

At this stage we now have added and tested the employee termination and search, in addition to the employee CRUD APIs. Next we will implement the employee review endpoints.