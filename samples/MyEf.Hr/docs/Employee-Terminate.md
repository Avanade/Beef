# Step 5 - Employee Terminate

This will walk through the process of creating and testing the employee termination capability.

<br/>

## Functional requirement

The employee termination will only terminate an existing employee under certain conditions.
- An employee can not be terminated more than once.
- An employee can not be termintaed on a date prior to their start date.

<br/>

## Explicit command
 
This functionality could be added to the existing update; however, certains types of updates are better suited to using commands, in this case _terminate_. This explicitly separates this specific functionaly from the more general purpose update previously implemented.

This separation of logic (and potentially underlying data) can allow additional functionality, and/or security, to be applied where applicable at a later stage minimizing impact within the overall solution.

This is essentially an application of the [CQRS pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs).

> CQRS stands for Command and Query Responsibility Segregation, a pattern that separates read and update operations for a data store. Implementing CQRS in your application can maximize its performance, scalability, and security. 

<br/>

## Data repository

No additional data repository effort is required as the intent is to reuse what already exists; specifically the `Employee` table already exposes the termination related columns.

<br/>

## Code generation

The `Termination` operation needs to be added to the `Employee` entity configuration; add the following after the existing `GetByArgs` operation.

``` yaml
      # Terminate operation
      # - Text is specified to override the default for an Update.
      # - Type is Update as it follows a similar operation pattern.
      # - ValueType is overridden with the TerminationDetail to use this instead of the default Employee.
      # - Validator is overridden to use the TerminationDetailValidator.
      # - WebApiRoute is overridden to be terminate.
      # - WebApiMethod is overriden to use HttpPost (an Update otherwise defaults to an HttpPut).
      # - EventSubject is overridden so that the action component will be Terminated.
      # - AutoImplement is None as this will be specifically implemented by the developer.
      # - An additional Id parameter is passed; in this instance we do not use the Operation.PrimaryKey as we require the value to be passed down all the layers.
      { name: Terminate, text: 'Terminates an existing {{Employee}}', type: Update, valueType: TerminationDetail, validator: TerminationDetailValidator, webApiRoute: '{id}/terminate', webApiMethod: HttpPost, eventSubject: 'Hr.Employee:Terminated', autoImplement: None,
        parameters: [
          { name: Id, property: Id, isMandatory: true, text: '{{Employee}} identifier' }
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

This is an instance where there is some validation logic that has been added to this data component versus in a related validator. The primary reason for this is efficiency, as we need to do a `Get` to then `Update`, so to minimize chattiness and keep this logic together it is all implemented here.

Add the following code to the non-generated `EmployeeData.cs` (`MyEf.Hr.Business/Data`) that was created earlier. _Note_ that we are reusing the `Get` and the `Update` we implemented previously. Also, of note is the use of the `Result` type (railway-oriented programming) to enable, including the usage of the `Result.ValidationError` versus throwing a `ValidationException` (ultimately results in the same outcome).

``` csharp
    /// <summary>
    /// Terminates an existing employee by updating the termination-related columns.
    /// </summary>
    /// <remarks>Need to pre-query the data to, 1) check they exist, 2) check they are still employed, 3) not prior to starting, and, then 4) update.</remarks>
    private Task<Result<Employee>> TerminateOnImplementationAsync(TerminationDetail value, Guid id)
        => Result.GoAsync(GetAsync(id))
            .When(e => e is null, _ => Result.NotFoundError())
            .When(e => e.Termination is not null, _ => Result.ValidationError("An Employee can not be terminated more than once."))
            .When(e => value.Date < e.StartDate, _ => Result.ValidationError("An Employee can not be terminated prior to their start date."))
            .Then(e => e.Termination = value)
            .ThenAsAsync(e => UpdateAsync(e));
```

<br/>

## Validation

Although, some of the validation was added into the data access logic, the property-level validation should still occur as usual within a validator. It is considered best practice to ensure the integrity of the data prior to making more expensive data access calls where possible; i.e. fail-fast.

Within the `MyEf.Hr.Business/Validation` folder create `TerminationDetailValidator.cs` and implement as follows.

``` csharp
namespace MyEf.Hr.Business.Validation;

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
```

<br/>

## End-to-End testing

For the purposes of this sample un-comment the region `Terminate` within `EmployeeTest.cs`. Execute the tests and ensure they all pass as expected.

As extra homework, you should also consider implementing unit testing for the validator.

<br/>

## Conclusion

At this stage we now have added and tested the employee termination and search, in addition to the employee CRUD APIs. 

Next we will implement the employee [performance review](./Performance-Review.md) endpoints.