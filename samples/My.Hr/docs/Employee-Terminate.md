# Step 5 - Employee Terminate

This will walk through the process of creating and testing the employee termination capability.

<br/>

## Functional requirement

The employee termination will only terminate an existing employee under certain conditions.
- An employee can not be terminated more than once.
- An employee can not be termintaed on a date prior to their start date.

<br/>

## Data repository

No additional data repository effort is required as the intent is to reuse what already exists; specifically the `Update` stored procedure that already exposes the termination related columns.

<br/>

## Code generation

The `Termination` operation needs to be added to the `Employee` entity configuration; add the following after the existing `GetByArgs` operation.

``` yaml
      # Terminate operation
      # - Text is specified to override the default for an Update.
      # - OperationType is Update as it follows a similar pattern.
      # - ValueType is overridden with the TerminationDetail to use this instead of the default Employee.
      # - Validator is overridden to use the TerminationDetailValidator.
      # - WebApiMethod is overriden to use HttpPost (an Update otherwise defaults to an HttpPut).
      # - EventSubject is overridden so that the action component will be Terminated.
      # - AutoImplement is None as this will be implemented by the developer.
      # - An additional Id parameter is passed; in this instance we do not use the UniqueKey as we require the value to be passed down all the layers.
      { name: Terminate, text: 'Terminates an existing {{Employee}}', type: Update, valueType: TerminationDetail, validator: TerminationDetailValidator, webApiRoute: '{id}/terminate', webApiMethod: HttpPost, eventSubject: 'Hr.Employee:Terminated', autoImplement: None,
        parameters: [
          { name: Id, type: Guid, text: '{{Employee}} identifier' }
        ]
      }
```

Execute the code-generation using the command line.

```
dotnet run entity
```

</br>

## Data access logic

The existing `EmployeeData.cs` logic will need to be extended to support the new `Terminate`. 

This is an instance where there is some validation logic that has been added to this data component versus in the related validator. The primary reason for this is efficiency, as we need to do a `Get` to then `Update`, so to minimize chattiness and keep this logic together it is all implemented here.

Add the following code to the non-generated `EmployeeData.cs` (`My.Hr.Business/Data`) that was created earlier. _Note_ that we are reusing the `Get` and the `Update` we implemented previously.

``` csharp
        /// <summary>
        /// Terminates an existing employee by updating their termination columns.
        /// </summary>
        private async Task<Employee> TerminateOnImplementationAsync(TerminationDetail value, Guid id)
        {
            // Need to pre-query the data to, 1) check they exist, 2) check they are still employed, and 3) update.
            var curr = await GetOnImplementationAsync(id).ConfigureAwait(false);
            if (curr == null)
                throw new NotFoundException();

            if (curr.Termination != null)
                throw new ValidationException("An Employee can not be terminated more than once.");

            if (value.Date < curr.StartDate)
                throw new ValidationException("An Employee can not be terminated prior to their start date.");

            curr.Termination = value;
            return await UpdateOnImplementationAsync(curr).ConfigureAwait(false);
        }
```

<br/>

## Validation

Although, some of the validation was added into the data access logic, the property-level validation should still occur as usual within a validator. It is considered best practice to ensure the integrity of the data prior to making more expensive data access calls where possible; i.e. fail-fast.

Within the `My.Hr.Business/Validation` folder create `TerminationDetailValidator.cs` and implement as follows.

``` csharp
using Beef.Validation;
using My.Hr.Business.Entities;

namespace My.Hr.Business.Validation
{
    /// <summary>
    /// Represents a <see cref="TerminationDetail"/> validator.
    /// </summary>
    public class TerminationDetailValidator : Validator<TerminationDetail>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TerminationDetailValidator"/> class.
        /// </summary>
        public TerminationDetailValidator()
        {
            Property(x => x.Date).Mandatory();
            Property(x => x.Reason).Mandatory().IsValid();
        }
    }
}
```

<br/>

## End-to-End testing

For the purposes of this sample un-comment the region `Terminate` within `EmployeeTest.cs`. Execute the tests and ensure they all pass as expected.

As extra homework, you should also consider implementing unit testing for the validator.

<br/>

## Conclusion

At this stage we now have added and tested the employee termination and search, in addition to the employee CRUD APIs. 

Next we will implement the employee [performance review](./Performance-Review.md) endpoints.