# Step 2 - Employee API

This will walk through the process of creating the required APIs, including the related business and data access logic.

The [`Beef.CodeGen.Core`](../../../tools/Beef.CodeGen.Core/README.md) provides the code-generation capabilities that will be leveraged. The underlying documentation describes these capabilities and the code-gen approach in greater detail.

_Note:_ Any time that command line execution is requested, this should be performed from the base `MyEf.Hr.CodeGen` folder.

<br/>

## Clean up existing

The following files were created when the solution was provisioned, these should be removed (deleted):
- `MyEf.Hr.Business/Data/PersonData.cs`
- `MyEf.Hr.Business/Validation/PersonArgsValidator.cs`
- `MyEf.Hr.Business/Validation/PersonValidator.cs`

<br/>

## Generate corresponding entity configuration

The previous database code-generation supports an additional `yaml` sub-command that will generate the basic entity YAML configuration by inferring the database configuration for the specified tables into a temporary `temp.entity.beef-5.yaml` file. Additionally, an initial C# validator will also be generated for each table.

The developer is then responsible for the copy+paste of the required yaml into the `entity.beef-5.yaml` and `refdata.beef-5.yaml` file(s) and further amending as appropriate. After use, the developer should remove the `temp.entity.beef-5.yaml` file as it is otherwise not referenced by the code-generation. 

This helps accelerate the configuration of the entity YAML configuration, and is particularly useful when there are a large number of tables to be configured.

_Note:_ This by no means endorses the direct mapping between entity and database model as the developer is still encouraged to reshape the entity to take advantage of object-orientation and resulting JSON capabilities.

The following provides the help content for the `yaml` sub-command:

```
codegen yaml <Schema> <Table> [<Table>...]   Creates a temporary Beef entity YAML file for the specified table(s).
                                             - A table name with a prefix ! denotes that no CRUD operations are required.
                                             - A table name with a prefix @ denotes that a 'GetByArgs' operation is required.
```

An example of the database command usage is as follows:

```
dotnet run codegen yaml Hr Gender *Employee !EmergencyContact TerminationReason
```

<br/>

## Reference Data configuration

The `refdata.beef-5.yaml` within `MyEf.Hr.CodeGen` provides the code-gen configuration for the [Reference Data](../../../docs/Reference-Data.md). For the purposes of this sample, this configuration is relatively straightforward.

Each reference data entity is defined, by specifying the name, the Web API route prefix (i.e. its endpoint), that it is to be automatically implemented using Entity Framework, and the name of the corresponding Entity Framework model (which was previously generated from the database; see `MyEf.Hr.Business/Data/EfModel/Generated` folder).

Replace the existing YAML with the following.

``` yaml
webApiRoutePrefix: ref
entities:
- { name: Gender, refDataType: Guid, collection: true, webApiRoutePrefix: genders, autoImplement: EntityFramework, entityFrameworkModel: EfModel.Gender }
- { name: TerminationReason, refDataType: Guid, collection: true, webApiRoutePrefix: terminationReasons, autoImplement: EntityFramework, entityFrameworkModel: EfModel.TerminationReason }
- { name: RelationshipType, refDataType: Guid, collection: true, webApiRoutePrefix: relationshipTypes, autoImplement: EntityFramework, entityFrameworkModel: EfModel.RelationshipType }
- { name: USState, refDataType: Guid, collection: true, webApiRoutePrefix: usStates, autoImplement: EntityFramework, entityFrameworkModel: EfModel.USState }
```

<br/>

## Reference Data code-gen

Once the reference data has been configured the code-generation can be performed. Use the following command line to generate. This will generate all of the required layers, from the API controller, through to the database access using Entity Framework. This is all that is required to operationalize the end-to-end reference data functionality. 

```
dotnet run refdata
```

<br/>

## Employee CRUD

To implement the core `Employee` CRUD operations, the following will need to be performed:
- Code-gen configuration
- Code-gen execution
- Data access logic (custom)
- Validation

<br/>

### Code-gen configuration

First up, the entities need to be defined (configured) within the `entity.beef-5.yaml` (`MyEf.Hr.CodeGen` project). The entities that will be created are as follows. Note that we will add some shape (e.g. address) to the data so it is easier (and more logical) to consume and understand, versus mapping directly to the database structure. 

- `EmployeeBase` - this represents the base Employee in that it contains the key properties and will be used as the base for searching as a means to minimise the properties that are available outside of the `Employee` CRUD itself.
- `Employee` - this represents the complete detailed Employee, which inherits from the `EmployeeBase`. All of the key operations for the Employee including the search will be configured/grouped as a logical set here.
- `TerminationDetail` - this represents an employee's termination (being date and reason). By having as a sub-type this enables additional related data to be more easily added under the single `Employee.Termination` property.
- `Address` - this represents the employees' address. By having as a sub-type it makes it easier and more explicit that there is a valid address via the `Employee.Address` property; in that we can validate the full address on the existence of the property itself (i.e. not `null`).
- `EmergencyContact` - this represents the collection of emergency contacts for an employee. 

Replace the existing `entity.beef-5.yaml` with the following. The comments included are intended to describe the usage and why certain attributes have been specified.

``` yaml
# Configuring the code-generation global settings
# - RefDataText generates a corresponding reference data text property for '$text=true' output.
# - WebApiRoutrePrefix will prefix/prepend all route Entity and Operation routes with specified value.
# - WebApiAutoLocation indicates to automatically set the HTTP response location for a create.
# - EventSubjectRoot specifies the root for the event subject.
# - EventSourceRoot specifies the root for the event source.
# - EventSourceKind will be a relative path URI.
# - EventActionFormat specifies past tense for the event action.
# - EventTransaction will ensure that the data update and event send will occur within a database transaction.
refDataText: true
webApiRoutePrefix: ''
webApiAutoLocation: true
eventSubjectRoot: MyEf
eventSourceRoot: MyEf/Hr
eventSourceKind: Relative
eventActionFormat: PastTense
eventTransaction: true
entities:
  # Creating an Employee base with only the subset of fields that we want returned from the GetByArgs.
  # - As we will be returning more than one we need the Collection and CollectionResult.
  # - Any Text with a handlebars '{{xxx}}' is a shortcut for .NET see comments; e.g. '<see cref="xxx"/>'.
  # - The identifier/primary key property(ies) must be specified.
  # - ExcludeData of RequiresMapper is a special condition to output a corresponding DataMapper.
  # - Use of DataName is to reference the name of the column where different to the property name iteself.
  # - Use of DataAutoGenerated indicates that the data source will automatically generate the value.
  # - A Type with a '^' prefix is shorthand for 'RefDataNamespace.*', this is how to reference a reference data entity (will default RefDataType to 'string' where not specified).
  # - A DateTimeTransform of DateOnly is used to indicate that the DateTime property should only be concerned with the Date component.
  # - AutoImplement of EntityFramework will ensure that the mapper is generated; used by the Employee.Get/Create/Update.
  # - The Termination property EntityFrameworkMapper is set to Ignore as this cannot be automatically generated; custom code will need to be developed to handle; used by the Employee.GetByArgs.
- { name: EmployeeBase, text: '{{Employee}} base', collection: true, collectionResult: true, excludeData: RequiresMapper, autoImplement: EntityFramework, entityFrameworkModel: EfModel.Employee,
    properties: [
      { name: Id, type: Guid, text: '{{Employee}} identifier', primaryKey: true, dataName: EmployeeId, dataAutoGenerated: true },
      { name: Email, text: 'Unique {{Employee}} Email' },
      { name: FirstName },
      { name: LastName },
      { name: Gender, type: ^Gender, dataName: GenderCode },
      { name: Birthday, type: DateTime, dateTimeTransform: DateOnly },
      { name: StartDate, type: DateTime, dateTimeTransform: DateOnly },
      { name: Termination, type: TerminationDetail, entityFrameworkMapper: Flatten },
      { name: PhoneNo }
    ]
  }

  # Creating an Employee inheriting from EmployeeBase (DataMapper will also inherit).
  # - The Id is re-specified, but marked as inherited, as is needed to assist with the operations that reference the PrimaryKey.
  # - The Validator is specified, which is then used by both the Create and Update operations.
  # - The AutoImplement specifies that operations should be auto-implemented using EntityFramework unless explicitly overridden.
  # - The WebApiRoutePrefix is defined, which is in turn extended by each operation.
  # - CRUD operations auto implemented via: get, create, update and patch option attributes (auto-set).
- { name: Employee, inherits: EmployeeBase, validator: EmployeeValidator, webApiRoutePrefix: employees, crud: true, autoImplement: EntityFramework, entityFrameworkModel: EfModel.Employee, entityFrameworkMapperBase: EmployeeBaseData,
    properties: [
      { name: Id, text: '{{Employee}} identifier', type: Guid, primaryKey: true, inherited: true, databaseIgnore: true, entityFrameworkMapper: Ignore },
      { name: Address, type: Address, dataConverter: 'ObjectToJsonConverter{T}', dataName: AddressJson, entityFrameworkMapper: Set },
      { name: EmergencyContacts, type: EmergencyContactCollection, entityFrameworkMapper: Map },
      { name: ETag },
      { name: ChangeLog, type: ChangeLog }
    ],
    operations: [
      # Delete operation
      # - Explictly defining so that we can tie further validation to the identifier check.
      # - Using the Property attribute to copy configuration from the Entity itself.
      # - Providing further validation by using the Common extension method to invoke the EmployeeValidator.CanDelete.
      { name: Delete, type: Delete, webApiRoute: '{id}', autoImplement: EntityFramework,
        parameters: [
          { name: Id, property: Id, isMandatory: true, validatorCode: Common(EmployeeValidator.CanDelete) }
        ]
      }
    ]
  }

  # Creating a TerminationDetail with Date and Reason.
  # - ExcludeAll is used so only the entity (not other layers are generated); with the exception of ExcludeData of RequiresMapper where it is a special case to output a DataMapper.
  # - EntityFrameworkModel ensures that the EF Mappings also get generated.
- { name: TerminationDetail, excludeData: RequiresMapper, autoImplement: EntityFramework, entityFrameworkModel: EfModel.Employee,
    properties: [
      { name: Date, type: DateTime, dateTimeTransform: DateOnly, dataName: TerminationDate },
      { name: Reason, type: ^TerminationReason, dataName: TerminationReasonCode }
    ]
  }

  # Creating an Address.
- { name: Address,
    properties: [
      { name: Street1 },
      { name: Street2 },
      { name: City },
      { name: State, type: ^USState },
      { name: PostCode }
    ]
  }

  # Creating a EmergencyContact and corresponding collection.
  # - ExcludeData of RequiresMapper is a special case to specifically output a DataMapper.
- { name: EmergencyContact, collection: true, excludeData: RequiresMapper, autoImplement: EntityFramework, entityFrameworkModel: EfModel.EmergencyContact,
    properties: [
      { name: Id, type: Guid, primaryKey: true, dataName: EmergencyContactId },
      { name: FirstName },
      { name: LastName },
      { name: PhoneNo },
      { name: Relationship, type: ^RelationshipType, dataName: RelationshipTypeCode }
    ]
  }
```

<br/>

### Code-gen execution

Once the entities has been configured the code-generation can be performed. Use the following command line to generate. This will generate all of the required layers, from the API controller, through to the database access using Entity Framework (EF) where configured. 

```
dotnet run entity
```

<br/>

### Validation

The final component that must be implemented by the developer is the validation logic. _CoreEx_ provides a rich, integrated, [validation framework](https://github.com/Avanade/CoreEx/tree/main/src/CoreEx.Validation) to simplify and standardize the validation as much as possible. This is also intended to encourage a more thorough approach to validation as the API is considered the primary custodian of the underlying data integrity - as Deep Throat said to Mulder in the [X-Files](https://en.wikipedia.org/wiki/The_X-Files), "trust no one"!

To encourage reuse, _CoreEx_ has the concept of common validators which allow for standardised validations to be created that are then reusable. Within the `MyEf.Hr.Business/Validation` folder create `CommonValidators.cs` and implement as follows.

```
namespace MyEf.Hr.Business.Validation;

/// <summary>
/// Provides common validator capabilities.
/// </summary>
public static class CommonValidators
{
    /// <summary>
    /// Provides a common person's name validator, ensure max length is 100.
    /// </summary>
    public static CommonValidator<string?> PersonName { get; } = CommonValidator.Create<string?>(cv => cv.String(100));

    /// <summary>
    /// Provides a common address's street validator, ensure max length is 100.
    /// </summary>
    public static CommonValidator<string?> Street { get; } = CommonValidator.Create<string?>(cv => cv.String(100));

    /// <summary>
    /// Provides a common phone number validator, just length, but could be regex or other.
    /// </summary>
    public static CommonValidator<string?> PhoneNo { get; } = CommonValidator.Create<string?>(cv => cv.String(50));
}
```

The entity code-gen configuration references a `EmployeeValidator` that needs to be implemented. Within the `MyEf.Hr.Business/Validation` folder create an `EmployeeValidator.cs` and implement as follows.

``` csharp
namespace MyEf.Hr.Business.Validation;

/// <summary>
/// Represents a <see cref="Employee"/> validator.
/// </summary>
public class EmployeeValidator : Validator<Employee>
{
    private readonly IEmployeeDataSvc _employeeDataSvc;

    // Address validator implemented using fluent-style method-chaining.
    private static readonly Validator<Address> _addressValidator = Validator.Create<Address>()
        .HasProperty(x => x.Street1, p => p.Mandatory().Common(CommonValidators.Street))
        .HasProperty(x => x.Street2, p => p.Common(CommonValidators.Street))
        .HasProperty(x => x.City, p => p.Mandatory().String(50))
        .HasProperty(x => x.State, p => p.Mandatory().IsValid())
        .HasProperty(x => x.PostCode, p => p.Mandatory().String(new Regex(@"^\d{5}(?:[-\s]\d{4})?$")));

    // Emergency Contact validator implemented using fluent-style method-chaining.
    public static readonly Validator<EmergencyContact> _emergencyContactValidator = Validator.Create<EmergencyContact>()
        .HasProperty(x => x.FirstName, p => p.Mandatory().Common(CommonValidators.PersonName))
        .HasProperty(x => x.LastName, p => p.Mandatory().Common(CommonValidators.PersonName))
        .HasProperty(x => x.PhoneNo, p => p.Mandatory().Common(CommonValidators.PhoneNo))
        .HasProperty(x => x.Relationship, p => p.Mandatory().IsValid());

    /// <summary>
    /// Initializes a new instance of the <see cref="EmployeeValidator"/> class.
    /// </summary>
    /// <param name="employeeDataSvc">The <see cref="IEmployeeDataSvc"/>.</param>
    public EmployeeValidator(IEmployeeDataSvc employeeDataSvc)
    {
        _employeeDataSvc = employeeDataSvc ?? throw new ArgumentNullException(nameof(employeeDataSvc));

        Property(x => x.Email).Mandatory().Email();
        Property(x => x.FirstName).Mandatory().Common(CommonValidators.PersonName);
        Property(x => x.LastName).Mandatory().Common(CommonValidators.PersonName);
        Property(x => x.Gender).Mandatory().IsValid();
        Property(x => x.Birthday).Mandatory().CompareValue(CompareOperator.LessThanEqual, _ => CoreEx.ExecutionContext.Current.Timestamp.AddYears(-18), errorText: "Birthday is invalid as the Employee must be at least 18 years of age.");
        Property(x => x.StartDate).Mandatory().CompareValue(CompareOperator.GreaterThanEqual, new DateTime(1999, 01, 01, 0, 0, 0, DateTimeKind.Utc), "January 1, 1999");
        Property(x => x.PhoneNo).Mandatory().Common(CommonValidators.PhoneNo);
        Property(x => x.Address).Entity(_addressValidator);
        Property(x => x.EmergencyContacts).Collection(maxCount: 5, item: CollectionRuleItem.Create(_emergencyContactValidator).DuplicateCheck(ignoreWhereKeyIsInitial: true));
    }

    /// <summary>
    /// Add further validation logic non-property bound.
    /// </summary>
    protected override Task<Result> OnValidateAsync(ValidationContext<Employee> context, CancellationToken cancellationToken)
    {
        // Ensure that the termination data is always null on an update; unless already terminated then it can no longer be updated.
        switch (ExecutionContext.OperationType)
        {
            case OperationType.Create:
                context.Value.Termination = null;
                break;

            case OperationType.Update:
                return Result.GoAsync(_employeeDataSvc.GetAsync(context.Value.Id))
                    .When(existing => existing is null, _ => Result.NotFoundError())
                    .When(existing => existing!.Termination is not null, _ => Result.ValidationError("Once an Employee has been Terminated the data can no longer be updated."))
                    .ThenAs(_ => context.Value.Termination = null)
                    .AsResult();
        }

        return Result.SuccessTask;
    }

    /// <summary>
    /// Validator that will be referenced by the Delete operation to ensure that the employee can indeed be deleted.
    /// </summary>
    public static CommonValidator<Guid> CanDelete { get; } = CommonValidator.Create<Guid>(cv => cv.CustomAsync((context, _) =>
    {
        // Unable to use inheritance DI for a Common Validator so the ExecutionContext.GetService will get/create the instance in the same manner.
        return Result.GoAsync(CoreEx.ExecutionContext.GetRequiredService<IEmployeeDataSvc>().GetAsync(context.Value))
            .When(existing => existing is null, _ => Result.NotFoundError())
            .When(existing => existing!.StartDate <= CoreEx.ExecutionContext.Current.Timestamp, _ => Result.ValidationError("An employee cannot be deleted after they have started their employment."))
            .AsResult();
    }));
}
```

</br>

## Conclusion

At this stage we now have a compiling and working API including database access logic for the reference data and key employee CRUD activities. 

Next we need to perform end-to-end [intra-integration testing](./Employee-Test.md) to ensure it is functioning as expected.